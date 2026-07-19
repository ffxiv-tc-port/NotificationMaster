namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawLoginErrorConfig()
    {
        if(ImGui.Checkbox("啟用", ref p.cfg.loginError_Enable))
        {
            LoginError.Setup(p.cfg.loginError_Enable, p);
        }
        if(p.cfg.loginError_Enable)
        {
            //ImGui.TextColored(ImGuiColors.DalamudOrange, "Please note that this function is in testing. ");
            ImGui.Text($"當發生伺服器連線錯誤時{(p.cfg.loginError_AlwaysExecute ? "" : "，若 FFXIV 在背景執行")}，執行以下動作：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.loginError_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.loginError_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.loginError_AutoActivateWindow);
            ForegroundWarning(p.cfg.loginError_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.loginError_SoundSettings);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.loginError_AlwaysExecute);
            DrawHttpMaster(p.cfg.loginError_HttpRequests, ref p.cfg.loginError_HttpRequestsEnable,
                "無");
        }
    }
}
