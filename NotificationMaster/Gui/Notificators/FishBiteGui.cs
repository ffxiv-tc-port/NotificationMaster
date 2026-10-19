namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawFishBiteConfig()
    {
        if (ImGui.Checkbox("Enable".Loc(), ref p.cfg.fishBite_Enable))
        {
            FishBite.Setup(p.cfg.fishBite_Enable, p);
        }
        if (p.cfg.fishBite_Enable)
        {
            ImGui.Text("When a fish bites, do the following:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.fishBite_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.fishBite_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.fishBite_AutoActivateWindow);
            ImGui.Checkbox("Show chat message".Loc(), ref p.cfg.fishBite_ChatMessage);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.fishBite_AlwaysExecute);
            ForegroundWarning(p.cfg.fishBite_AutoActivateWindow);

            ImGui.Separator();
            ImGui.Text("Bite type settings:".Loc());

            ImGui.PushID("fishbite_light");
            if (ImGui.CollapsingHeader("Light bite (!)".Loc(), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled".Loc() + "##light", ref p.cfg.fishBite_LightEnabled);
                if (p.cfg.fishBite_LightEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_LightSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_medium");
            if (ImGui.CollapsingHeader("Medium bite (!!)".Loc(), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled".Loc() + "##medium", ref p.cfg.fishBite_MediumEnabled);
                if (p.cfg.fishBite_MediumEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_MediumSoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.PushID("fishbite_heavy");
            if (ImGui.CollapsingHeader("Heavy bite (!!!)".Loc(), ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enabled".Loc() + "##heavy", ref p.cfg.fishBite_HeavyEnabled);
                if (p.cfg.fishBite_HeavyEnabled)
                {
                    DrawSoundSettings(ref p.cfg.fishBite_HeavySoundSettings);
                }
                ImGui.Unindent();
            }
            ImGui.PopID();

            ImGui.Separator();
            DrawHttpMaster(p.cfg.fishBite_HttpRequests, ref p.cfg.fishBite_HttpRequestsEnable,
                "$B - bite type (light/medium/heavy)".Loc());

            ImGui.Separator();
            if (ImGui.Button("Reset to Defaults".Loc()))
            {
                FishBite.ResetToDefaults(p);
            }
            ImGui.SameLine();
            ImGui.TextDisabled("(Restores default sounds and settings)".Loc());
        }
    }
}
