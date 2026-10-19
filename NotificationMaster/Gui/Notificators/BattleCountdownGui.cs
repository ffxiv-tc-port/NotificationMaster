namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawBattleCountdownConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.countdown_Enable))
        {
            BattleCountdown.Setup(p.cfg.countdown_Enable, p);
        }
        if(p.cfg.countdown_Enable)
        {
            ImGui.Text("When a party member starts a battle countdown, do the following if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.countdown_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.countdown_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.countdown_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.countdown_AlwaysExecute);
            ForegroundWarning(p.cfg.countdown_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.countdown_SoundSettings);
            DrawHttpMaster(p.cfg.countdown_HttpRequests, ref p.cfg.countdown_HttpRequestsEnable,
                "$T - seconds until battle starts\n$M - full countdown message".Loc());
        }
    }
}
