using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Text.ReadOnly;
using AtkValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace NotificationMaster;

/// <summary>
/// 準備確認：別人發起時的是／否提示框（<c>SelectYesno</c>）一跳出就通知；
/// 狀態一覽視窗（<c>ReadyCheck</c>）只在沒看到提示框時照舊通知（自己發起、或比對失效）。
/// </summary>
/// <remarks>
/// 兩個回呼都在框架執行緒上跑。只讀不按；提示框文字在回呼裡當場抄成字串，不留原生指標。
/// </remarks>
internal class ReadyCheck : IDisposable
{
    private const string PromptAddon = "SelectYesno";
    private const string StatusAddon = "ReadyCheck";

    private NotificationMaster p;
    private readonly string receivedText;
    private readonly ReadyCheckSession session = new();
    private bool nearMissLogged;

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, StatusAddon, OnReadyCheckSetup);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, PromptAddon, OnPromptSetup);
    }

    public ReadyCheck(NotificationMaster plugin)
    {
        p = plugin;
        receivedText = LoadSheetText(ReadyCheckPrompt.ReceivedPromptRow);
        PluginLog.Information(receivedText.Length > 0
            ? $"準備確認：提示框比對文字＝「{receivedText}」（Addon#{ReadyCheckPrompt.ReceivedPromptRow}）。"
            : $"準備確認：讀不到 Addon#{ReadyCheckPrompt.ReceivedPromptRow}，只會在狀態一覽視窗打開時通知。");
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, StatusAddon, OnReadyCheckSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, PromptAddon, OnPromptSetup);
    }

    private void OnPromptSetup(AddonEvent type, AddonArgs args)
    {
        if(receivedText.Length == 0) return;
        var screen = ReadPromptText(args.Addon.Address);
        if(screen == null) return;
        if(!ReadyCheckPrompt.IsReceivedPrompt(screen, receivedText))
        {
            LogNearMissOnce(screen);
            return;
        }

        var active = Utils.IsApplicationActivated;
        if(session.OnPrompt(Environment.TickCount64, active, out var elapsedMs) == ReadyCheckVerdict.SkipReopened)
        {
            PluginLog.Information($"準備確認：提示框 {elapsedMs / 1000.0:F1} 秒內再次打開"
                + "（期間沒有打開狀態一覽視窗、遊戲在前景），視為從通知重開同一次準備確認，不再通知。");
            return;
        }

        Notify("提示框跳出", active);
    }

    private void OnReadyCheckSetup(AddonEvent type, AddonArgs args)
    {
        PluginLog.Debug("Ready check window opened");
        var active = Utils.IsApplicationActivated;
        if(session.OnStatusWindow(Environment.TickCount64, out var elapsedMs) == ReadyCheckVerdict.SkipSameCheck)
        {
            PluginLog.Information($"準備確認：狀態一覽視窗打開，{elapsedMs / 1000.0:F1} 秒前已處理同一次準備確認，不再通知。");
            return;
        }

        Notify("狀態一覽視窗打開（之前沒偵測到別人發起的提示框：自己發起或比對不到）", active);
    }

    private void Notify(string source, bool active)
    {
        if(p.PauseUntil > Environment.TickCount64)
        {
            PluginLog.Information($"準備確認：{source}，通知暫停中，這次不通知。");
            return;
        }

        // 見 TataruPraiseBridge：語音提醒不跟「只在背景時通知」的規則走。
        if(p.cfg.readyCheck_TataruPraise) TataruPraiseBridge.Praise(TataruPraiseBridge.CategoryReadyCheck);
        var run = !active || p.cfg.readyCheck_AlwaysExecute;
        if(run) DoNotify();
        PluginLog.Information($"準備確認：{source}，"
            + (run ? "已執行通知動作" : "遊戲在前景且沒勾「遊戲在前景也執行」，不執行通知動作")
            + (p.cfg.readyCheck_TataruPraise ? "；已送塔塔露語音請求。" : "。"));
    }

    private void DoNotify()
    {
        if(p.cfg.readyCheck_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }
        if(p.cfg.readyCheck_AutoActivateWindow) Native.Impl.Activate();
        if(p.cfg.readyCheck_ShowToastNotification)
        {
            TrayIconManager.ShowToast("Ready check initiated.".Loc(), "Ready check".Loc());
        }
        if(p.cfg.readyCheck_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.readyCheck_HttpRequests,
                new string[][] { }
            );
        }
        if(p.cfg.readyCheck_SoundSettings.PlaySound)
        {
            p.audioPlayer.Play(p.cfg.readyCheck_SoundSettings);
        }
    }

    /// <summary>讀不到（表沒有這一列、或 Lumina 擲例外）回空字串＝提示框永遠不命中，只剩狀態一覽視窗那條路。</summary>
    private static string LoadSheetText(uint rowId)
    {
        try
        {
            var row = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Addon>().GetRowOrDefault(rowId);
            if(row == null) return string.Empty;
            return ReadyCheckPrompt.ComparableText(row.Value.Text.Data.ToArray());
        }
        catch(Exception ex)
        {
            PluginLog.Information($"準備確認：讀 Addon#{rowId} 失敗：{ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>提示框第一個 AtkValue 的文字；不是字串或讀不到回 null。只在 PostSetup 回呼裡用。</summary>
    private static unsafe string ReadPromptText(nint address)
    {
        var addon = (AtkUnitBase*)address;
        if(addon == null || addon->AtkValues == null || addon->AtkValuesCount == 0) return null;

        var first = addon->AtkValues[0];
        var baseType = first.Type & AtkValueType.TypeMask;
        if(baseType != AtkValueType.String && baseType != AtkValueType.String8) return null;
        if(first.String.Value == null) return null;

        var raw = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(first.String.Value);
        return ReadyCheckPrompt.ComparableText(raw.ToArray());
    }

    /// <summary>開頭像、整句卻對不上：多半是改版改了字。只提醒一次，免得洗版。</summary>
    private void LogNearMissOnce(string screen)
    {
        if(nearMissLogged || receivedText.Length < 4) return;
        if(!screen.StartsWith(receivedText[..4], StringComparison.Ordinal)) return;

        nearMissLogged = true;
        PluginLog.Information($"準備確認：提示框「{screen}」開頭像、整句跟 Addon#{ReadyCheckPrompt.ReceivedPromptRow}「{receivedText}」不同，沒有在提示框通知。");
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.readyCheck == null)
            {
                p.readyCheck = new ReadyCheck(p);
                PluginLog.Information("Enabling readyCheck module");
            }
            else
            {
                PluginLog.Information("readyCheck module already enabled");
            }
        }
        else
        {
            if(p.readyCheck != null)
            {
                p.readyCheck.Dispose();
                p.readyCheck = null;
                PluginLog.Information("Disabling readyCheck module");
            }
            else
            {
                PluginLog.Information("readyCheck module already disabled");
            }
        }
    }
}

/// <summary>準備確認提示框的文字比對（純函式）。</summary>
internal static class ReadyCheckPrompt
{
    /// <summary>別人發起時跳的提示框文字（<c>Addon</c> 表）。</summary>
    internal const uint ReceivedPromptRow = 8133;

    /// <summary>SeString 位元組 → 只留文字、拿掉所有換行、修頭尾空白。</summary>
    /// <remarks>表裡的換行是 NewLine 巨集、畫面上可能是字面 LF；畫面與表走同一支才比得上。</remarks>
    internal static string ComparableText(byte[] seString)
        => new ReadOnlySeStringSpan(seString).ExtractText().ReplaceLineEndings(string.Empty).Trim();

    /// <summary>逐字相同才算；表文字是空字串（讀不到）一律不中。</summary>
    internal static bool IsReceivedPrompt(string screen, string received)
        => received.Length > 0 && string.Equals(screen, received, StringComparison.Ordinal);
}

/// <summary><see cref="ReadyCheckSession"/> 的判定。預設值＝通知（判不出來時寧可多通知一次）。</summary>
internal enum ReadyCheckVerdict
{
    Notify = 0,
    SkipReopened = 1,
    SkipSameCheck = 2,
}

/// <summary>同一次準備確認只通知一次。</summary>
/// <remarks>
/// 只在框架執行緒上用（兩個 addon 回呼），不加鎖。時間是 <see cref="Environment.TickCount64"/> 毫秒。
/// </remarks>
internal sealed class ReadyCheckSession
{
    /// <summary>狀態一覽視窗算「同一次」的時間窗，從那一次開始處理時算。</summary>
    internal const long SameCheckMs = 60_000;

    /// <summary>提示框重開算「同一次」的時間窗；要短於一次沒人回答的準備確認會持續的時間。</summary>
    internal const long ReopenWindowMs = 20_000;

    private const long Never = long.MinValue;

    private long promptAtMs = Never;
    private long handledAtMs = Never;
    private bool statusSincePrompt;

    /// <summary>別人發起的提示框打開。<paramref name="elapsedMs"/>＝距上一次算新的提示框，沒有回 -1。</summary>
    internal ReadyCheckVerdict OnPrompt(long nowMs, bool gameActive, out long elapsedMs)
    {
        elapsedMs = promptAtMs == Never ? -1 : nowMs - promptAtMs;

        // 從通知重開只能在遊戲裡點，遊戲必在前景；回答了就會打開狀態一覽視窗，之後不可能再重開。
        if(gameActive && !statusSincePrompt && elapsedMs >= 0 && elapsedMs < ReopenWindowMs)
            return ReadyCheckVerdict.SkipReopened;

        promptAtMs = nowMs;
        handledAtMs = nowMs;
        statusSincePrompt = false;
        return ReadyCheckVerdict.Notify;
    }

    /// <summary>狀態一覽視窗打開。<paramref name="elapsedMs"/>＝距上一次開始處理，沒有回 -1。</summary>
    internal ReadyCheckVerdict OnStatusWindow(long nowMs, out long elapsedMs)
    {
        elapsedMs = handledAtMs == Never ? -1 : nowMs - handledAtMs;
        statusSincePrompt = true;
        if(elapsedMs >= 0 && elapsedMs < SameCheckMs) return ReadyCheckVerdict.SkipSameCheck;

        handledAtMs = nowMs;
        return ReadyCheckVerdict.Notify;
    }
}
