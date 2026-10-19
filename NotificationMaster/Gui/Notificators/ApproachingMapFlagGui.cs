namespace NotificationMaster;

internal unsafe partial class ConfigGui
{
    internal void DrawMapFlagConfig()
    {
        /*ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        ImGui.TextWrapped("Warning: this feature is EXPERIMENTAL!");
        ImGui.PopStyleColor();*/
        var curPosEnable = ImGui.GetCursorPos();
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.mapFlag_Enable))
        {
            ApproachingMapFlag.Setup(p.cfg.mapFlag_Enable, p);
        }
        if(p.cfg.mapFlag_Enable)
        {
            var curPosCont = ImGui.GetCursorPos();
            var distance = 0f;
            ImGui.SetCursorPos(new Vector2(500f, curPosEnable.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.Text("Debug info: ".Loc());
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Flag state: ??".Loc(p.mapFlag.isFlagSet));
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Flag territory: ??".Loc(p.mapFlag.flagTerritory));
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Flag X: ??".Loc(p.mapFlag.flagX));
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Flag Y: ??".Loc(p.mapFlag.flagY));
            if(Svc.Objects.LocalPlayer != null)
            {
                ImGui.SetCursorPosX(500f);
                ImGui.Text("Player X: ??".Loc(Svc.Objects.LocalPlayer.Position.X));
                ImGui.SetCursorPosX(500f);
                ImGui.Text("Player Y: ??".Loc(Svc.Objects.LocalPlayer.Position.Z));
                ImGui.SetCursorPosX(500f);
                ImGui.Text("Territory: ??".Loc(Svc.ClientState.TerritoryType));
                ImGui.SetCursorPosX(500f);
                distance = Vector2.Distance(new Vector2(p.mapFlag.flagX, p.mapFlag.flagY),
                    new Vector2(Svc.Objects.LocalPlayer.Position.X, Svc.Objects.LocalPlayer.Position.Z));
                ImGui.Text("Distance: ??".Loc(distance));
            }
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(curPosCont);
            ImGui.Text("When getting close to map flag if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.mapFlag_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.mapFlag_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.mapFlag_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.mapFlag_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.mapFlag_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC, under the same conditions as the actions above. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ForegroundWarning(p.cfg.mapFlag_AutoActivateWindow);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Distance to marker".Loc(), ref p.cfg.mapFlag_TriggerDistance);
            ImGui.Text("Note: this is in-game coordinates distance, not map coordinates distance.".Loc());
            if(p.mapFlag.isFlagSet && Svc.ClientState.TerritoryType == p.mapFlag.flagTerritory)
            {
                ImGui.Text("You are currently ?? yalms away from currently set marker.".Loc($"{distance:0}"));
            }
            else
            {
                ImGui.Text("Set flag on your map to see your current distance to it".Loc());
            }
            ImGui.Checkbox("Also trigger on crossing X/Y flag axis before reaching set distance".Loc(), ref p.cfg.mapFlag_TriggerOnCross);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Axis cross tolerance".Loc(), ref p.cfg.mapFlag_CrossDelta);
            DrawSoundSettings(ref p.cfg.mapFlag_SoundSettings);
            DrawHttpMaster(p.cfg.mapFlag_HttpRequests, ref p.cfg.mapFlag_HttpRequestsEnable,
                "None available".Loc());
        }
    }
}
