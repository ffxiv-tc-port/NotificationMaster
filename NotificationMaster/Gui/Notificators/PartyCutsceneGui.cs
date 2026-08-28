namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawPartyCutsceneConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.partyCutscene_Enable))
        {
            PartyCutsceneEnded.Setup(p.cfg.partyCutscene_Enable, p);
        }
        if(p.cfg.partyCutscene_Enable)
        {
            ImGui.Text("When all party members have finished watching a cutscene inside a duty, do the following:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.partyCutscene_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.partyCutscene_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.partyCutscene_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.partyCutscene_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.partyCutscene_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC. Unlike the actions above, this also happens while the game is in the foreground. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ImGui.Checkbox("Show chat message".Loc(), ref p.cfg.partyCutscene_ChatMessage);
            ImGui.SetNextItemWidth(150f);
            ImGui.SliderInt("Minimum cutscene duration to trigger notification, seconds".Loc(), ref p.cfg.partyCutscene_MinSeconds, 1, 30);
            ForegroundWarning(p.cfg.partyCutscene_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.partyCutscene_SoundSettings);
            DrawHttpMaster(p.cfg.partyCutscene_HttpRequests, ref p.cfg.partyCutscene_HttpRequestsEnable,
                "$S - seconds waited".Loc());
        }
    }
}
