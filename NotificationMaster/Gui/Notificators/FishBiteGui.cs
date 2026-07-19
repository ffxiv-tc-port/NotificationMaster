namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawFishBiteConfig()
    {
        if (ImGui.Checkbox("啟用", ref p.cfg.fishBite_Enable))
        {
            FishBite.Setup(p.cfg.fishBite_Enable, p);
        }
        if (p.cfg.fishBite_Enable)
        {
            ImGui.Text("當魚上鉤時，執行以下動作：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.fishBite_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.fishBite_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.fishBite_AutoActivateWindow);
            ImGui.Checkbox("顯示聊天訊息", ref p.cfg.fishBite_ChatMessage);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.fishBite_AlwaysExecute);
            ForegroundWarning(p.cfg.fishBite_AutoActivateWindow);

            ImGui.Separator();
            ImGui.Text("上鉤類型設定：");

            ImGui.PushID("fishbite_light");
            if (ImGui.CollapsingHeader("輕微上鉤 (!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("啟用##light", ref p.cfg.fishBite_LightEnabled);
                if (p.cfg.fishBite_LightEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_LightSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_medium");
            if (ImGui.CollapsingHeader("中等上鉤 (!!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("啟用##medium", ref p.cfg.fishBite_MediumEnabled);
                if (p.cfg.fishBite_MediumEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_MediumSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_heavy");
            if (ImGui.CollapsingHeader("強烈上鉤 (!!!)", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("啟用##heavy", ref p.cfg.fishBite_HeavyEnabled);
                if (p.cfg.fishBite_HeavyEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_HeavySoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.Separator();
            DrawHttpMaster(p.cfg.fishBite_HttpRequests, ref p.cfg.fishBite_HttpRequestsEnable,
                "$B - 上鉤類型（輕微/中等/強烈）");

            ImGui.Separator();
            if (ImGui.Button("重設為預設值"))
            {
                FishBite.ResetToDefaults(p);
            }
            ImGui.SameLine();
            ImGui.TextDisabled("（恢復預設音效與設定）");
        }
    }
}
