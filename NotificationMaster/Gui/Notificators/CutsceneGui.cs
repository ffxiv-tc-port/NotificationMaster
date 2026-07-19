namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawCutsceneConfig()
    {
        if(ImGui.Checkbox("啟用", ref p.cfg.cutscene_Enable))
        {
            CutsceneEnded.Setup(p.cfg.cutscene_Enable, p);
        }
        if(p.cfg.cutscene_Enable)
        {
            ImGui.Text("當過場動畫結束且 FFXIV 在背景執行時，執行以下動作：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.cutscene_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.cutscene_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.cutscene_AutoActivateWindow);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.cutscene_AlwaysExecute);
            ForegroundWarning(p.cfg.cutscene_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.cutscene_SoundSettings);
            ImGui.Text("區域限制：");
            ImGui.Checkbox("僅於主線任務輪盤地下城中觸發", ref p.cfg.cutscene_OnlyMSQ);
            DrawHttpMaster(p.cfg.cutscene_HttpRequests, ref p.cfg.cutscene_HttpRequestsEnable,
                "無可用變數");
        }
    }
}
