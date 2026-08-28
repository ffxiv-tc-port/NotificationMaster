namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawCfPopConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.cfPop_Enable))
        {
            CfPop.Setup(p.cfg.cfPop_Enable, p);
        }
        if(p.cfg.cfPop_Enable)
        {
            ImGui.Text("When duty pops, do the following if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.cfPop_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.cfPop_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.cfPop_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.cfPop_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.cfPop_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC. Unlike the actions above, this also happens while the game is in the foreground. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ForegroundWarning(p.cfg.cfPop_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.cfPop_SoundSettings);
            ImGui.Checkbox("Repeat in 30 seconds if invitation still not accepted".Loc(), ref p.cfg.cfPop_NotifyIn30);
            if(p.cfg.cfPop_NotifyIn30)
            {
                ImGui.Indent();
                ImGui.Checkbox("Only notify when 15 seconds are left".Loc(), ref p.cfg.cfPop_NotifyOnlyIn30);
                ImGui.Unindent();
            }
            DrawHttpMaster(p.cfg.cfPop_HttpRequests, ref p.cfg.cfPop_HttpRequestsEnable,
                "$N - name of the duty\n$T - time left to accept the duty".Loc());
        }
    }
}
