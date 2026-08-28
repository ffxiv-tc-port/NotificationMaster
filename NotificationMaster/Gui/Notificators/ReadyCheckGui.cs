namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawReadyCheckConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.readyCheck_Enable))
        {
            ReadyCheck.Setup(p.cfg.readyCheck_Enable, p);
        }
        if(p.cfg.readyCheck_Enable)
        {
            ImGui.Text("When a ready check is initiated, do the following if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.readyCheck_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.readyCheck_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.readyCheck_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.readyCheck_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.readyCheck_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC. Unlike the actions above, this also happens while the game is in the foreground. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ForegroundWarning(p.cfg.readyCheck_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.readyCheck_SoundSettings);
            DrawHttpMaster(p.cfg.readyCheck_HttpRequests, ref p.cfg.readyCheck_HttpRequestsEnable,
                "None available".Loc());
        }
    }
}
