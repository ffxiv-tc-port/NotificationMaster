using System.IO;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace NotificationMaster;

public enum FishBiteType : byte
{
    Unknown = 0,
    Light = 36,
    Medium = 37,
    Heavy = 38,
    None = 255,
}

internal class FishBite : IDisposable
{
    private NotificationMaster p;
    private IntPtr _tugTypeAddress = IntPtr.Zero;
    private FishingState _lastFishingState = FishingState.NotFishing;

    // 一次性診斷旗標。實機要驗的是「State 這個偏移在台服讀不讀得到真東西」,
    // 那是狀態轉換時才有意義的事件,不是每幀都要印的東西。
    private bool _loggedFirstActiveState;
    private bool _loggedUnknownState;
    private bool _loggedFirstBiteTug;

    // Signature from AutoHook plugin
    private const string TugTypeSignature = "48 8D 35 ?? ?? ?? ?? 4C 8B CE";

    public FishBite(NotificationMaster plugin)
    {
        p = plugin;
        try
        {
            _tugTypeAddress = Svc.SigScanner.GetStaticAddressFromSig(TugTypeSignature);
            PluginLog.Debug($"FishBite: Found TugType address: {_tugTypeAddress:X}");
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Could not find TugType signature: {e.Message}\n{e.StackTrace ?? ""}");
        }

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    /// <summary>
    /// 讀取遊戲當前的釣魚狀態。取不到時回 <see cref="FishingState.NotFishing"/>(＝不通知)。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 📌 2026-08-19 更正:本函式原本硬編碼 <c>return None</c>,附註寫的理由是「API13 的
    /// FFXIVClientStructs 只有泛型 <c>EventHandler*</c>,沒有 <c>FishingEventHandler</c>
    /// 專屬 struct 與 State 欄位」。<b>那個前提對現行 pin 不成立</b>:
    /// <c>FFXIV/Client/Game/Event/FishingEventHandler.cs</c> 就在樹裡,帶
    /// <c>[FieldOffset(0x228)] public FishingState State</c>,而
    /// <c>EventHandlerModule+0x70</c> 的註解已標明那個指標就是它。
    /// 同艦隊的 AutoHook(<c>SeFunctions/BaitManager.cs</c>)與 GatherBuddyReborn
    /// (<c>SeFunctions/EventFramework.cs</c>)都走同一條路徑讀同一個偏移。
    /// </para>
    /// <para>
    /// 🔴 <b>不跨幀保存任何原生指標</b> —— 每次呼叫都重新取得 EventFramework 與事件處理器。
    /// <c>EventFramework.Instance()</c> 宣告為 <c>isPointer: true</c>,<b>會回 null</b>
    /// (登入前、區域切換中);<c>FishingEventHandler</c> 指標在釣魚系統初始化前也是 null。
    /// 兩個都必須判空 —— 少判一個就是 AccessViolation,而 AVE 是 corrupted-state exception,
    /// <c>try/catch</c> 攔不到。
    /// </para>
    /// <para>
    /// ⚠️ <b>只取低位元組</b>:CS 這份 <c>FishingState</c> 沒寫底層型別(＝<c>int</c>,4 bytes),
    /// 但 AutoHook 與 GatherBuddyReborn 兩邊實際在跑的宣告都是 <c>: byte</c>。
    /// 取低位元組在兩種解讀下得到的值相同,且不會被 0x229~0x22B 的內容汙染。
    /// </para>
    /// <para>
    /// ⚠️ <b>0x228 這個偏移是 CS 對國際服的宣告,對台服的正確性離線無法證明。</b>
    /// 因此讀到的值只要不在具名列舉值裡就當 <c>NotFishing</c> ——
    /// 偏移萬一不成立時的失敗形式是「不通知」,不是亂通知,更不是崩潰。
    /// </para>
    /// </remarks>
    private unsafe FishingState GetFishingState()
    {
        var eventFramework = EventFramework.Instance();
        if (eventFramework == null)
            return FishingState.NotFishing;

        // EventHandlerModule 是內嵌在 EventFramework 位移 0 的 struct;
        // 它的 FishingEventHandler 欄位在 pin 裡型別還是泛型 EventHandler*(CS 自己標了 TODO),
        // 所以這裡要轉成 FishingEventHandler* 才拿得到 State。
        var fishingHandler = (FishingEventHandler*)eventFramework->EventHandlerModule.FishingEventHandler;
        if (fishingHandler == null)
            return FishingState.NotFishing;

        var state = (FishingState)unchecked((byte)fishingHandler->State);

        if (!IsKnownFishingState(state))
        {
            if (!_loggedUnknownState)
            {
                _loggedUnknownState = true;
                PluginLog.Information(
                    $"FishBite: 讀到未知的釣魚狀態值 {(byte)state},不在具名列舉值內,當作 NotFishing 處理。"
                    + "若這行反覆出現,代表 FishingEventHandler.State(0x228)這個偏移對台服不成立。");
            }

            return FishingState.NotFishing;
        }

        if (state != FishingState.NotFishing && !_loggedFirstActiveState)
        {
            _loggedFirstActiveState = true;
            PluginLog.Information(
                $"FishBite: 首次讀到非 NotFishing 的釣魚狀態:{state}(原始值 {(byte)state})。"
                + "這代表 FishingEventHandler.State 在台服讀得到真實資料。");
        }

        return state;
    }

    /// <summary>
    /// 這個值是不是 <see cref="FishingState"/> 的具名值。
    /// </summary>
    /// <remarks>
    /// ⚠️ 這個列舉<b>有斷號</b>(7、10、11 都沒有具名值),所以不能用「小於等於最大值」當範圍檢查。
    /// 逐值列舉而不是 <c>Enum.IsDefined</c>:這是每幀路徑,switch 會編成比較鏈/跳表且零配置。
    /// </remarks>
    private static bool IsKnownFishingState(FishingState state) => state switch
    {
        FishingState.NotFishing or FishingState.PoleOut or FishingState.PullPoleIn
            or FishingState.Quit or FishingState.PoleReady or FishingState.Bite
            or FishingState.Reeling or FishingState.Waiting
            or FishingState.NormalFishing or FishingState.LureFishing => true,
        _ => false,
    };

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
    }

    private unsafe void OnFrameworkUpdate(object _)
    {
        if (_tugTypeAddress == IntPtr.Zero)
            return;

        var currentFishingState = GetFishingState();

        // Log state changes
        if (currentFishingState != _lastFishingState)
        {
            PluginLog.Debug($"FishBite: State changed from {_lastFishingState} to {currentFishingState}");
        }

        // Only trigger when state transitions TO FishingState.Bite
        if (currentFishingState == FishingState.Bite && _lastFishingState != FishingState.Bite)
        {
            var currentBite = *(FishBiteType*)_tugTypeAddress;

            if (!_loggedFirstBiteTug)
            {
                _loggedFirstBiteTug = true;
                PluginLog.Information(
                    $"FishBite: 首次偵測到咬餌,tug 原始值 = {(byte)currentBite} ({currentBite})。"
                    + "預期是 36/37/38(輕/中/重);若不是,代表 tug 靜態位址對台服不成立。");
            }

            if (currentBite != FishBiteType.None && currentBite != FishBiteType.Unknown)
            {
                OnBite(currentBite);
            }
        }

        _lastFishingState = currentFishingState;
    }

    private void OnBite(FishBiteType bite)
    {
        if (p.PauseUntil > Environment.TickCount64) return;
        if (Utils.IsApplicationActivated && !p.cfg.fishBite_AlwaysExecute) return;

        // Debug: show raw bite value
        PluginLog.Debug($"FishBite: OnBite called with value {(byte)bite} ({bite})");

        var biteName = bite switch
        {
            FishBiteType.Light => "light",
            FishBiteType.Medium => "medium",
            FishBiteType.Heavy => "heavy",
            _ => $"unknown({(byte)bite})"
        };

        var isEnabled = bite switch
        {
            FishBiteType.Light => p.cfg.fishBite_LightEnabled,
            FishBiteType.Medium => p.cfg.fishBite_MediumEnabled,
            FishBiteType.Heavy => p.cfg.fishBite_HeavyEnabled,
            _ => false
        };

        if (!isEnabled) return;

        PluginLog.Debug($"FishBite: {biteName} bite detected");

        if (p.cfg.fishBite_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }

        if (p.cfg.fishBite_AutoActivateWindow)
        {
            Native.Impl.Activate();
        }

        if (p.cfg.fishBite_ShowToastNotification)
        {
            var biteTitle = bite switch
            {
                FishBiteType.Light => "Light bite!".Loc(),
                FishBiteType.Medium => "Medium bite!".Loc(),
                FishBiteType.Heavy => "Heavy bite!".Loc(),
                _ => $"{char.ToUpper(biteName[0]) + biteName[1..]} bite!"
            };
            TrayIconManager.ShowToast(biteTitle, "Fish hooked".Loc());
        }

        if (p.cfg.fishBite_ChatMessage)
        {
            var biteWord = bite switch
            {
                FishBiteType.Light => "light".Loc(),
                FishBiteType.Medium => "medium".Loc(),
                FishBiteType.Heavy => "heavy".Loc(),
                _ => biteName
            };
            Svc.Chat.Print("[FishNotify] You hook a fish with a ?? bite.".Loc(biteWord));
        }

        var soundSettings = bite switch
        {
            FishBiteType.Light => p.cfg.fishBite_LightSoundSettings,
            FishBiteType.Medium => p.cfg.fishBite_MediumSoundSettings,
            FishBiteType.Heavy => p.cfg.fishBite_HeavySoundSettings,
            _ => null
        };

        PluginLog.Debug($"FishBite: soundSettings for {biteName}: PlaySound={soundSettings?.PlaySound}, Path={soundSettings?.SoundPath}");

        if (soundSettings?.PlaySound == true)
        {
            PluginLog.Debug($"FishBite: Playing sound for {biteName}");
            p.audioPlayer.Play(soundSettings);
        }

        if (p.cfg.fishBite_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.fishBite_HttpRequests,
                new string[][]
                {
                    new string[] { "$B", biteName }
                }
            );
        }
    }

    internal static void ResetToDefaults(NotificationMaster p)
    {
        try
        {
            var configDir = Svc.PluginInterface.ConfigDirectory.FullName;
            var soundsDir = Path.Combine(configDir, "Sounds");
            Directory.CreateDirectory(soundsDir);

            var infoPath = Path.Combine(soundsDir, "Info.wav");
            var alertPath = Path.Combine(soundsDir, "Alert.wav");
            var alarmPath = Path.Combine(soundsDir, "Alarm.wav");

            // Force re-extract sounds
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new Dictionary<string, string>
            {
                { "NotificationMaster.Sounds.Info.wav", "Info.wav" },
                { "NotificationMaster.Sounds.Alert.wav", "Alert.wav" },
                { "NotificationMaster.Sounds.Alarm.wav", "Alarm.wav" }
            };

            foreach (var (resourceName, fileName) in resourceNames)
            {
                var filePath = Path.Combine(soundsDir, fileName);
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var fileStream = File.Create(filePath);
                    stream.CopyTo(fileStream);
                }
            }

            // Reset light bite settings
            p.cfg.fishBite_LightEnabled = true;
            p.cfg.fishBite_LightSoundSettings.SoundPath = infoPath;
            p.cfg.fishBite_LightSoundSettings.PlaySound = true;
            p.cfg.fishBite_LightSoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_LightSoundSettings.Volume = 1.0f;
            p.cfg.fishBite_LightSoundSettings.Repeat = false;

            // Reset medium bite settings
            p.cfg.fishBite_MediumEnabled = true;
            p.cfg.fishBite_MediumSoundSettings.SoundPath = alertPath;
            p.cfg.fishBite_MediumSoundSettings.PlaySound = true;
            p.cfg.fishBite_MediumSoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_MediumSoundSettings.Volume = 1.0f;
            p.cfg.fishBite_MediumSoundSettings.Repeat = false;

            // Reset heavy bite settings
            p.cfg.fishBite_HeavyEnabled = true;
            p.cfg.fishBite_HeavySoundSettings.SoundPath = alarmPath;
            p.cfg.fishBite_HeavySoundSettings.PlaySound = true;
            p.cfg.fishBite_HeavySoundSettings.StopSoundOnceFocused = false;
            p.cfg.fishBite_HeavySoundSettings.Volume = 1.0f;
            p.cfg.fishBite_HeavySoundSettings.Repeat = false;

            // Reset other settings
            p.cfg.fishBite_ChatMessage = true;
            p.cfg.fishBite_AlwaysExecute = true;
            p.cfg.fishBite_FlashTrayIcon = true;
            p.cfg.fishBite_ShowToastNotification = false;
            p.cfg.fishBite_AutoActivateWindow = false;

            p.cfg.Save();
            Notify.Success("Fish Notify settings reset to defaults".Loc());
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Failed to reset to defaults: {e.Message}\n{e.StackTrace ?? ""}");
            Notify.Error("Failed to reset settings".Loc());
        }
    }

    internal static void ExtractDefaultSounds(NotificationMaster p)
    {
        try
        {
            var configDir = Svc.PluginInterface.ConfigDirectory.FullName;
            var soundsDir = Path.Combine(configDir, "Sounds");
            Directory.CreateDirectory(soundsDir);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = new Dictionary<string, string>
            {
                { "NotificationMaster.Sounds.Info.wav", "Info.wav" },
                { "NotificationMaster.Sounds.Alert.wav", "Alert.wav" },
                { "NotificationMaster.Sounds.Alarm.wav", "Alarm.wav" }
            };

            foreach (var (resourceName, fileName) in resourceNames)
            {
                var filePath = Path.Combine(soundsDir, fileName);
                if (!File.Exists(filePath))
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var fileStream = File.Create(filePath);
                        stream.CopyTo(fileStream);
                        PluginLog.Debug($"FishBite: Extracted {fileName} to {filePath}");
                    }
                }
            }

            // Set default sound paths if not already set
            var infoPath = Path.Combine(soundsDir, "Info.wav");
            var alertPath = Path.Combine(soundsDir, "Alert.wav");
            var alarmPath = Path.Combine(soundsDir, "Alarm.wav");

            if (string.IsNullOrEmpty(p.cfg.fishBite_LightSoundSettings.SoundPath) && File.Exists(infoPath))
            {
                p.cfg.fishBite_LightSoundSettings.SoundPath = infoPath;
                p.cfg.fishBite_LightSoundSettings.PlaySound = true;
                p.cfg.fishBite_LightSoundSettings.StopSoundOnceFocused = false;
            }

            if (string.IsNullOrEmpty(p.cfg.fishBite_MediumSoundSettings.SoundPath) && File.Exists(alertPath))
            {
                p.cfg.fishBite_MediumSoundSettings.SoundPath = alertPath;
                p.cfg.fishBite_MediumSoundSettings.PlaySound = true;
                p.cfg.fishBite_MediumSoundSettings.StopSoundOnceFocused = false;
            }

            if (string.IsNullOrEmpty(p.cfg.fishBite_HeavySoundSettings.SoundPath) && File.Exists(alarmPath))
            {
                p.cfg.fishBite_HeavySoundSettings.SoundPath = alarmPath;
                p.cfg.fishBite_HeavySoundSettings.PlaySound = true;
                p.cfg.fishBite_HeavySoundSettings.StopSoundOnceFocused = false;
            }

            p.cfg.Save();
        }
        catch (Exception e)
        {
            PluginLog.Error($"FishBite: Failed to extract default sounds: {e.Message}\n{e.StackTrace ?? ""}");
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if (enable)
        {
            if (p.fishBite == null)
            {
                ExtractDefaultSounds(p);
                p.fishBite = new FishBite(p);
                PluginLog.Information("Enabling fishBite module");
            }
            else
            {
                PluginLog.Information("fishBite module already enabled");
            }
        }
        else
        {
            if (p.fishBite != null)
            {
                p.fishBite.Dispose();
                p.fishBite = null;
                PluginLog.Information("Disabling fishBite module");
            }
            else
            {
                PluginLog.Information("fishBite module already disabled");
            }
        }
    }
}
