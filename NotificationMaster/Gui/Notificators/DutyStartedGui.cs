namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawDutyStartedConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.dutyStart_Enable))
        {
            DutyStarted.Setup(p.cfg.dutyStart_Enable, p);
        }
        if(p.cfg.dutyStart_Enable)
        {
            ImGui.Text("When the duty begins (starting countdown ends), do the following if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.dutyStart_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.dutyStart_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.dutyStart_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.dutyStart_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.dutyStart_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC. Unlike the actions above, this also happens while the game is in the foreground. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ImGui.Checkbox("Also notify when the duty recommences after a wipe".Loc(), ref p.cfg.dutyStart_NotifyRecommence);
            ForegroundWarning(p.cfg.dutyStart_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.dutyStart_SoundSettings);
            DrawHttpMaster(p.cfg.dutyStart_HttpRequests, ref p.cfg.dutyStart_HttpRequestsEnable,
                "$N - name of the duty".Loc());
        }
    }
}
