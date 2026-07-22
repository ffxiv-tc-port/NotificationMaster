namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawCutsceneConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.cutscene_Enable))
        {
            CutsceneEnded.Setup(p.cfg.cutscene_Enable, p);
        }
        if(p.cfg.cutscene_Enable)
        {
            ImGui.Text("When cutscene ends do the following if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.cutscene_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.cutscene_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.cutscene_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.cutscene_AlwaysExecute);
            ForegroundWarning(p.cfg.cutscene_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.cutscene_SoundSettings);
            ImGui.Text("Zone locking:".Loc());
            ImGui.Checkbox("Only trigger in MSQ roulette dungeons".Loc(), ref p.cfg.cutscene_OnlyMSQ);
            DrawHttpMaster(p.cfg.cutscene_HttpRequests, ref p.cfg.cutscene_HttpRequestsEnable,
                "None available".Loc());
        }
    }
}
