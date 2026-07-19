namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawCfPopConfig()
    {
        if(ImGui.Checkbox("啟用", ref p.cfg.cfPop_Enable))
        {
            CfPop.Setup(p.cfg.cfPop_Enable, p);
        }
        if(p.cfg.cfPop_Enable)
        {
            ImGui.Text("當任務彈出時，若 FFXIV 在背景執行，執行以下動作：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.cfPop_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.cfPop_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.cfPop_AutoActivateWindow);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.cfPop_AlwaysExecute);
            ForegroundWarning(p.cfg.cfPop_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.cfPop_SoundSettings);
            ImGui.Checkbox("若 30 秒後仍未接受邀請則重複通知", ref p.cfg.cfPop_NotifyIn30);
            if(p.cfg.cfPop_NotifyIn30)
            {
                ImGui.Indent();
                ImGui.Checkbox("僅在剩餘 15 秒時通知", ref p.cfg.cfPop_NotifyOnlyIn30);
                ImGui.Unindent();
            }
            DrawHttpMaster(p.cfg.cfPop_HttpRequests, ref p.cfg.cfPop_HttpRequestsEnable,
                "$N - 任務名稱\n$T - 剩餘接受時間");
        }
    }
}
