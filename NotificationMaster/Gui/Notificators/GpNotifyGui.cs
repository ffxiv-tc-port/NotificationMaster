using Dalamud.Game.ClientState.Objects.Enums;

namespace NotificationMaster;

internal unsafe partial class ConfigGui
{
    internal void DrawGpNotify()
    {
        var curPosEnable = ImGui.GetCursorPos();
        if(ImGui.Checkbox("Enable".Loc() + "##gpn", ref p.cfg.gp_Enable))
        {
            GpNotify.Setup(p.cfg.gp_Enable, p);
        }
        if(p.cfg.gp_Enable)
        {
            var curPosCont = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(500f, curPosEnable.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.Text("Debug info: ".Loc());
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Nodes around: ??".Loc(Svc.Objects.Count(x => x.ObjectKind == ObjectKind.GatheringPoint)));
            ImGui.SetCursorPosX(500f);
            ImGui.Text("Potion cooldown: ??".Loc(FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroupDetail(GpNotify.PotionCDGroup)->IsActive));
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(curPosCont);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Notify upon reaching this amount of GP".Loc(), ref p.cfg.gp_GPTreshold, 1f, 0, 10000);
            ImGui.Text("Use command /gp <number> to quickly change this amount".Loc());
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Potion capacity".Loc(), ref p.cfg.gp_PotionCapacity, 1f, 0, 1000);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("Tolerance".Loc(), ref p.cfg.gp_Tolerance, 1f, 0, 100);
            ImGui.Checkbox("Suppress notification if no potential gathering places are around".Loc(), ref p.cfg.gp_SuppressIfNoNodes);
            if(ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("If your GP is lower than targeted by not more than this amount, notification will not be sent upon regaining it.".Loc());
            }
            ImGui.Text("Notification options:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.gp_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.gp_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.gp_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.gp_AlwaysExecute);
            ForegroundWarning(p.cfg.gp_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.gp_SoundSettings);
            DrawHttpMaster(p.cfg.gp_HttpRequests, ref p.cfg.gp_HttpRequestsEnable,
                "$G - available GP".Loc());
        }
    }
}
