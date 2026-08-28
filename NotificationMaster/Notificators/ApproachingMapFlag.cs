using ECommons.CSExtensions;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace NotificationMaster;

internal unsafe class ApproachingMapFlag
{
    private NotificationMaster p;

    /// <summary>
    /// 🔴 <c>AgentMap.Instance()</c> 由 <c>[Agent(AgentId.Map)]</c> 產生,內部鏈是
    /// AgentModule → UIModule → Framework:任何一層回 null 整條就回 null(登入前、切場景、
    /// 登出後都是常態),而最底層的 <c>[StaticAddress]</c>／<c>[MemberFunction]</c> 在特徵碼
    /// 失配時改為擲 <c>InvalidOperationException</c>——**兩種失效模式都存在,缺一等於假防護**。
    /// 裸解參考 null 原生指標是 AccessViolationException,在 .NET Core 屬 corrupted-state
    /// exception,<c>try/catch</c> 完全攔不到 ⇒ 只能事前判空。
    /// 下面四個屬性跑在 <c>Framework.Update</c>(每幀),所以判空後靜默回退成
    /// 「旗標未設定／territory 不符」,不寫 log(fail-closed:不通知比崩潰好)。
    /// </summary>
    private static AgentMap* AgentMapOrNull()
    {
        try
        {
            return AgentMap.Instance();
        }
        catch
        {
            return null;
        }
    }

    internal float flagX { get { var agent = AgentMapOrNull(); return agent == null ? 0f : agent->FlagMapMarker().XFloat; } }
    internal float flagY { get { var agent = AgentMapOrNull(); return agent == null ? 0f : agent->FlagMapMarker().YFloat; } }
    internal uint flagTerritory { get { var agent = AgentMapOrNull(); return agent == null ? uint.MaxValue : agent->FlagMapMarker().TerritoryId; } }
    internal bool isFlagSet { get { var agent = AgentMapOrNull(); return agent != null && agent->IsFlagMarkerSet(); } }

    public void Dispose()
    {
        Svc.Framework.Update -= ApproachingMapFlagWatcher;
    }

    public ApproachingMapFlag(NotificationMaster plugin)
    {
        p = plugin;
        try
        {
            Svc.Framework.Update += ApproachingMapFlagWatcher;
        }
        catch(Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? ""}");
            Setup(false, plugin);
        }
    }

    private bool IsEnabled = false;
    private bool HasTriggered = false;
    private bool DirectionX;
    private bool DirectionY;
    private void ApproachingMapFlagWatcher(object _)
    {
        if(p.PauseUntil > Environment.TickCount64 || (Utils.IsApplicationActivated && !p.cfg.mapFlag_AlwaysExecute) || Svc.Objects.LocalPlayer == null ||
            Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51] ||
             isFlagSet == false || flagTerritory != Svc.ClientState.TerritoryType)
        {
            IsEnabled = false;
            HasTriggered = false;
        }
        else
        {
            if(!IsEnabled)
            {
                UpdateDirections();
            }
            if(Vector2.Distance(new Vector2(flagX, flagY),
                new Vector2(Svc.Objects.LocalPlayer.Position.X,
                Svc.Objects.LocalPlayer.Position.Z)) <= p.cfg.mapFlag_TriggerDistance)
            {
                if(IsEnabled && !HasTriggered)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Distance reached, notification fired");
                    DoNotify("You have reached your destination!".Loc());
                }
                HasTriggered = true;
            }
            else
            {
                HasTriggered = false;
            }
            if((!DirectionX && flagX > Svc.Objects.LocalPlayer.Position.X + p.cfg.mapFlag_CrossDelta)
                || (DirectionX && flagX < Svc.Objects.LocalPlayer.Position.X - p.cfg.mapFlag_CrossDelta))
            {
                if(IsEnabled && !HasTriggered && p.cfg.mapFlag_TriggerOnCross)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Crossed X line, notification fired");
                    DoNotify("You have crossed your destination border (X)!".Loc());
                }
                UpdateDirections();
            }
            if((!DirectionY && flagY > Svc.Objects.LocalPlayer.Position.Z + p.cfg.mapFlag_CrossDelta)
                || (DirectionY && flagY < Svc.Objects.LocalPlayer.Position.Z - p.cfg.mapFlag_CrossDelta))
            {
                if(IsEnabled && !HasTriggered && p.cfg.mapFlag_TriggerOnCross)
                {
                    PluginLog.Debug($"{ImGui.GetFrameCount()} Crossed Y line, notification fired");
                    DoNotify("You have crossed your destination border (Y)!".Loc());
                }
                UpdateDirections();
            }
            IsEnabled = true;
        }
    }

    private void DoNotify(string s)
    {
        // 到達距離／越過 X 軸／越過 Y 軸三個觸發點都經過這裡；一次性判定（HasTriggered、
        // UpdateDirections）與 TriggerOnCross 開關由呼叫點既有的條件負責，這裡不再自己節流。
        // ⚠️ 與其他模組不同，這個模組的整個偵測狀態機平常只在「遊戲不在前景」時才跑
        // （見 ApproachingMapFlagWatcher 開頭的條件），所以語音同樣只在背景時出聲；
        // 勾了「即使前景也執行」（mapFlag_AlwaysExecute）後前景也會跑，語音也跟著出聲。
        if(p.cfg.mapFlag_TataruPraise) TataruPraiseBridge.Praise(TataruPraiseBridge.CategoryMapFlag);
        if(p.cfg.mapFlag_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }
        if(p.cfg.mapFlag_AutoActivateWindow) Native.Impl.Activate();
        if(p.cfg.mapFlag_ShowToastNotification)
        {
            TrayIconManager.ShowToast(s, "");
        }
        if(p.cfg.mapFlag_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.mapFlag_HttpRequests,
                new string[][]
                {
                }
            );
        }
        if(p.cfg.mapFlag_SoundSettings.PlaySound)
        {
            p.audioPlayer.Play(p.cfg.mapFlag_SoundSettings);
        }
    }

    private void UpdateDirections()
    {
        DirectionX = flagX > Svc.Objects.LocalPlayer.Position.X;
        DirectionY = flagY > Svc.Objects.LocalPlayer.Position.Z;
        //Svc.Chat.Print($"Directions: {DirectionX}, {DirectionY}");
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.mapFlag == null)
            {
                p.mapFlag = new ApproachingMapFlag(p);
                PluginLog.Information("Enabling mapFlag module");
            }
            else
            {
                PluginLog.Information("mapFlag module already enabled");
            }
        }
        else
        {
            if(p.mapFlag != null)
            {
                p.mapFlag.Dispose();
                p.mapFlag = null;
                PluginLog.Information("Disabling mapFlag module");
            }
            else
            {
                PluginLog.Information("mapFlag module already disabled");
            }
        }
    }
}
