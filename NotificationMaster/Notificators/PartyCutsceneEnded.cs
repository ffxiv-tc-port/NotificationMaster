using System.Diagnostics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Throttlers;

namespace NotificationMaster;

internal class PartyCutsceneEnded : IDisposable
{
    /// <summary>OnlineStatus sheet row 15 = viewing cutscene (過場動畫中).</summary>
    private const uint OnlineStatusViewingCutscene = 15;

    private NotificationMaster p;
    private readonly Stopwatch stopwatch = new();
    private bool dutyCompleted = false;

    public void Dispose()
    {
        Svc.Framework.Update -= Tick;
        Svc.DutyState.DutyCompleted -= OnDutyCompleted;
        Svc.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    public PartyCutsceneEnded(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Framework.Update += Tick;
        Svc.DutyState.DutyCompleted += OnDutyCompleted;
        Svc.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    private void OnDutyCompleted(object sender, ushort territory)
    {
        // people watching the post-duty cutscene are not worth waiting for, stop monitoring
        dutyCompleted = true;
        stopwatch.Reset();
    }

    private void OnTerritoryChanged(ushort territory)
    {
        dutyCompleted = false;
        stopwatch.Reset();
    }

    private void Tick(object _)
    {
        if(!EzThrottler.Throttle("NotificationMaster.PartyCutscene", 200)) return;
        if(p.PauseUntil > Environment.TickCount64)
        {
            stopwatch.Reset();
            return;
        }
        var localPlayer = Svc.Objects.LocalPlayer;
        if(localPlayer == null
            || dutyCompleted
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || !Svc.Condition[ConditionFlag.BoundByDuty]
            || Svc.ClientState.IsPvP
            || Svc.Party.Length < 2)
        {
            stopwatch.Reset();
            return;
        }
        if(IsAnyPartyMemberWatchingCutscene(localPlayer.EntityId))
        {
            if(!stopwatch.IsRunning)
            {
                PluginLog.Debug("Party member started watching a cutscene");
                stopwatch.Restart();
            }
        }
        else if(stopwatch.IsRunning)
        {
            var elapsed = stopwatch.Elapsed;
            stopwatch.Reset();
            PluginLog.Debug($"Party members finished the cutscene after {elapsed.TotalSeconds:F0} seconds");
            // ignore short transition states to avoid false positives
            if(elapsed >= TimeSpan.FromSeconds(Math.Max(1, p.cfg.partyCutscene_MinSeconds)))
            {
                DoNotify((int)elapsed.TotalSeconds);
            }
        }
    }

    private static bool IsAnyPartyMemberWatchingCutscene(uint localEntityId)
    {
        foreach(var member in Svc.Party)
        {
            if(member.EntityId == 0 || member.EntityId == localEntityId) continue;
            var obj = member.GameObject;
            if(obj == null) continue;
            if(obj is ICharacter chr && chr.OnlineStatus.RowId == OnlineStatusViewingCutscene) return true;
            // before the duty starts, players watching the opening cutscene may be hidden instead of flagged
            if(!Svc.DutyState.IsDutyStarted && !obj.IsTargetable) return true;
        }
        return false;
    }

    private void DoNotify(int seconds)
    {
        if(p.cfg.partyCutscene_AlwaysExecute || !Utils.IsApplicationActivated)
        {
            var message = "Party members finished the cutscene. You waited ?? seconds.".Loc(seconds);
            if(p.cfg.partyCutscene_FlashTrayIcon && !Utils.IsApplicationActivated)
            {
                Native.Impl.FlashWindow();
            }
            if(p.cfg.partyCutscene_AutoActivateWindow && !Utils.IsApplicationActivated) Native.Impl.Activate();
            if(p.cfg.partyCutscene_ShowToastNotification)
            {
                TrayIconManager.ShowToast(message);
            }
            if(p.cfg.partyCutscene_ChatMessage)
            {
                Svc.Chat.Print(
                    new SeStringBuilder()
                    .AddUiForeground(16)
                    .AddText(message)
                    .AddUiForegroundOff()
                    .Build());
            }
            if(p.cfg.partyCutscene_HttpRequestsEnable)
            {
                p.httpMaster.DoRequests(p.cfg.partyCutscene_HttpRequests,
                    new string[][]
                    {
                        new string[] {"$S", seconds.ToString()},
                    }
                );
            }
            if(p.cfg.partyCutscene_SoundSettings.PlaySound)
            {
                p.audioPlayer.Play(p.cfg.partyCutscene_SoundSettings);
            }
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.partyCutsceneEnded == null)
            {
                p.partyCutsceneEnded = new PartyCutsceneEnded(p);
                PluginLog.Information("Enabling partyCutsceneEnded module");
            }
            else
            {
                PluginLog.Information("partyCutsceneEnded module already enabled");
            }
        }
        else
        {
            if(p.partyCutsceneEnded != null)
            {
                p.partyCutsceneEnded.Dispose();
                p.partyCutsceneEnded = null;
                PluginLog.Information("Disabling partyCutsceneEnded module");
            }
            else
            {
                PluginLog.Information("partyCutsceneEnded module already disabled");
            }
        }
    }
}
