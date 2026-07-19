namespace NotificationMaster;

internal unsafe partial class ConfigGui
{
    internal void DrawMapFlagConfig()
    {
        /*ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        ImGui.TextWrapped("Warning: this feature is EXPERIMENTAL!");
        ImGui.PopStyleColor();*/
        var curPosEnable = ImGui.GetCursorPos();
        if(ImGui.Checkbox("啟用", ref p.cfg.mapFlag_Enable))
        {
            ApproachingMapFlag.Setup(p.cfg.mapFlag_Enable, p);
        }
        if(p.cfg.mapFlag_Enable)
        {
            var curPosCont = ImGui.GetCursorPos();
            var distance = 0f;
            ImGui.SetCursorPos(new Vector2(500f, curPosEnable.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
            ImGui.Text("除錯資訊： ");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"標記狀態： {p.mapFlag.isFlagSet}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"標記所在區域： {p.mapFlag.flagTerritory}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"標記 X： {p.mapFlag.flagX}");
            ImGui.SetCursorPosX(500f);
            ImGui.Text($"標記 Y： {p.mapFlag.flagY}");
            if(Svc.ClientState.LocalPlayer != null)
            {
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"玩家 X： {Svc.ClientState.LocalPlayer.Position.X}");
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"玩家 Y： {Svc.ClientState.LocalPlayer.Position.Z}");
                ImGui.SetCursorPosX(500f);
                ImGui.Text($"所在區域： {Svc.ClientState.TerritoryType}");
                ImGui.SetCursorPosX(500f);
                distance = Vector2.Distance(new Vector2(p.mapFlag.flagX, p.mapFlag.flagY),
                    new Vector2(Svc.ClientState.LocalPlayer.Position.X, Svc.ClientState.LocalPlayer.Position.Z));
                ImGui.Text($"距離： {distance}");
            }
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(curPosCont);
            ImGui.Text("當接近地圖標記時，若 FFXIV 在背景執行：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.mapFlag_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.mapFlag_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.mapFlag_AutoActivateWindow);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.mapFlag_AlwaysExecute);
            ForegroundWarning(p.cfg.mapFlag_AutoActivateWindow);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("與標記的距離", ref p.cfg.mapFlag_TriggerDistance);
            ImGui.Text("注意：此為遊戲內座標距離，並非地圖座標距離。");
            if(p.mapFlag.isFlagSet && Svc.ClientState.TerritoryType == p.mapFlag.flagTerritory)
            {
                ImGui.Text($"你目前距離已設定的標記 {distance:0} 雅魯。");
            }
            else
            {
                ImGui.Text("請在地圖上設定標記以查看目前距離");
            }
            ImGui.Checkbox("在到達設定距離前，若跨越標記的 X/Y 軸也觸發", ref p.cfg.mapFlag_TriggerOnCross);
            ImGui.SetNextItemWidth(100f);
            ImGui.DragInt("跨軸容許誤差", ref p.cfg.mapFlag_CrossDelta);
            DrawSoundSettings(ref p.cfg.mapFlag_SoundSettings);
            DrawHttpMaster(p.cfg.mapFlag_HttpRequests, ref p.cfg.mapFlag_HttpRequestsEnable,
                "無可用變數");
        }
    }
}
