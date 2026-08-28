using Lumina.Excel.Sheets;

namespace NotificationMaster;

internal class DutyStarted : IDisposable
{
    private NotificationMaster p;

    public void Dispose()
    {
        Svc.DutyState.DutyStarted -= OnDutyStarted;
        Svc.DutyState.DutyRecommenced -= OnDutyRecommenced;
    }

    public DutyStarted(NotificationMaster plugin)
    {
        p = plugin;
        Svc.DutyState.DutyStarted += OnDutyStarted;
        Svc.DutyState.DutyRecommenced += OnDutyRecommenced;
    }

    private void OnDutyStarted(object sender, ushort territory)
    {
        Handle(territory, false);
    }

    private void OnDutyRecommenced(object sender, ushort territory)
    {
        if(!p.cfg.dutyStart_NotifyRecommence) return;
        Handle(territory, true);
    }

    private void Handle(ushort territory, bool recommenced)
    {
        PluginLog.Debug($"Duty {(recommenced ? "recommenced" : "started")}, territory={territory}");
        if(p.PauseUntil > Environment.TickCount64) return;
        // 🔴 刻意放在「遊戲是否在前景」的判斷之前：語音提醒是給正在玩的人聽的，
        // 跟著「只在背景時通知」的規則走的話，開著遊戲反而永遠不出聲，看起來像壞掉。
        // 其他通知動作的觸發條件完全沒有被動到。
        // 團滅後重開（recommenced）算同一場任務，不重複念。
        if(!recommenced && p.cfg.dutyStart_TataruPraise) TataruPraiseBridge.Praise(TataruPraiseBridge.CategoryDutyStart);
        if(!Utils.IsApplicationActivated || p.cfg.dutyStart_AlwaysExecute)
        {
            DoNotify(GetDutyName(territory), recommenced);
        }
    }

    private static string GetDutyName(ushort territory)
    {
        if(Svc.Data.GetExcelSheet<TerritoryType>().TryGetRow(territory, out var terr))
        {
            return terr.ContentFinderCondition.ValueNullable?.Name.ToString() ?? "";
        }
        return "";
    }

    private void DoNotify(string dutyName, bool recommenced)
    {
        var title = recommenced ? "Duty recommenced".Loc() : "Duty started".Loc();
        if(p.cfg.dutyStart_FlashTrayIcon)
        {
            Native.Impl.FlashWindow();
        }
        if(p.cfg.dutyStart_AutoActivateWindow) Native.Impl.Activate();
        if(p.cfg.dutyStart_ShowToastNotification)
        {
            if(dutyName == "")
            {
                TrayIconManager.ShowToast(title);
            }
            else
            {
                TrayIconManager.ShowToast(dutyName, title);
            }
        }
        if(p.cfg.dutyStart_HttpRequestsEnable)
        {
            p.httpMaster.DoRequests(p.cfg.dutyStart_HttpRequests,
                new string[][]
                {
                    new string[] {"$N", dutyName},
                }
            );
        }
        if(p.cfg.dutyStart_SoundSettings.PlaySound)
        {
            p.audioPlayer.Play(p.cfg.dutyStart_SoundSettings);
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.dutyStarted == null)
            {
                p.dutyStarted = new DutyStarted(p);
                PluginLog.Information("Enabling dutyStarted module");
            }
            else
            {
                PluginLog.Information("dutyStarted module already enabled");
            }
        }
        else
        {
            if(p.dutyStarted != null)
            {
                p.dutyStarted.Dispose();
                p.dutyStarted = null;
                PluginLog.Information("Disabling dutyStarted module");
            }
            else
            {
                PluginLog.Information("dutyStarted module already disabled");
            }
        }
    }
}
