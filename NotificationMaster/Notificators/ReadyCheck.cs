using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.Throttlers;

namespace NotificationMaster;

internal class ReadyCheck : IDisposable
{
    private NotificationMaster p;

    public void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "ReadyCheck", OnReadyCheckSetup);
    }

    public ReadyCheck(NotificationMaster plugin)
    {
        p = plugin;
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ReadyCheck", OnReadyCheckSetup);
    }

    private void OnReadyCheckSetup(AddonEvent type, AddonArgs args)
    {
        PluginLog.Debug("Ready check window opened");
        if(p.PauseUntil > Environment.TickCount64) return;
        // the same addon may be closed and reopened within one ready check; do not notify twice
        if(!EzThrottler.Throttle("NotificationMaster.ReadyCheck", 10000)) return;
        // 見 TataruPraiseBridge：語音提醒不跟「只在背景時通知」的規則走。
        if(p.cfg.readyCheck_TataruPraise) TataruPraiseBridge.Praise(TataruPraiseBridge.CategoryReadyCheck);
        if(!Utils.IsApplicationActivated || p.cfg.readyCheck_AlwaysExecute)
        {
            DoNotify();
        }
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
