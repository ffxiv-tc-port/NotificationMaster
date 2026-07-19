namespace NotificationMaster;

internal partial class ConfigGui
{
    private string mobToDelete = null;
    private bool mobAllowDeleting = false;
    private string mobsToAdd = "";
    private (string filter, bool onlyWorld, bool onlySelected) terrSearchOptions = ("", true, false);
    private int tCounter = 0;
    internal void DrawMobPulledConfig()
    {
        if(ImGui.Checkbox("啟用", ref p.cfg.mobPulled_Enable))
        {
            MobPulled.Setup(p.cfg.mobPulled_Enable, p);
        }
        if(p.cfg.mobPulled_Enable)
        {
            tCounter = 0;
            if(mobToDelete != null)
            {
                p.cfg.mobPulled_Names.Remove(mobToDelete);
                mobToDelete = null;
                p.mobPulled.RebuildMobNames();
                p.mobPulled.ClearIgnoredMobs();
            }
            ImGui.TextColored(ImGuiColors.DalamudOrange, "請注意，外掛必須要能「看見」該怪物才能偵測到其被拉起。\n" +
                "若你所在區域極度擁擠，A級討伐怪可能會消失不見。\n" +
                "但 S/SS 級討伐怪應該永遠可見。此問題的解決方案將於日後推出。");
            ImGui.Text($"當清單中的怪物在指定區域被拉起時{(p.cfg.mobPulled_AlwaysExecute ? "" : "，若 FFXIV 在背景執行")}，執行以下動作：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.mobPulled_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.mobPulled_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.mobPulled_AutoActivateWindow);
            ForegroundWarning(p.cfg.mobPulled_AutoActivateWindow);
            ImGui.Checkbox("在聊天欄印出警告", ref p.cfg.mobPulled_ChatMessage);
            ImGui.Checkbox("顯示遊戲內提示訊息", ref p.cfg.mobPulled_Toast);
            DrawSoundSettings(ref p.cfg.mobPulled_SoundSettings);
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudOrange);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.mobPulled_AlwaysExecute);
            ImGui.PopStyleColor();
            DrawHttpMaster(p.cfg.mobPulled_HttpRequests, ref p.cfg.mobPulled_HttpRequestsEnable,
                "$M - 怪物名稱");
            if(ImGui.CollapsingHeader($"監控中的怪物清單（目前共 {p.cfg.mobPulled_Names.Count} 筆）###MPListmobs"))
            {
                ImGui.Checkbox("允許刪除項目", ref mobAllowDeleting);
                if(p.cfg.mobPulled_Names.Count > 0)
                {
                    ImGui.SameLine();
                    if(ImGui.Button("將怪物名稱匯出到剪貼簿"))
                    {
                        ImGui.SetClipboardText(string.Join("\n", p.cfg.mobPulled_Names));
                    }
                }
                foreach(var s in p.cfg.mobPulled_Names)
                {
                    if(mobAllowDeleting)
                    {
                        if(ImGui.SmallButton($"刪除##{s.GetHashCode()}"))
                        {
                            mobToDelete = s;
                        }
                        ImGui.SameLine();
                    }
                    ImGui.Text(s);
                }
                ImGui.TextColored(ImGuiColors.DalamudOrange, "新增怪物（一行一個；區分大小寫；多餘空白將被移除；重複項目將被刪除）");
                ImGui.InputTextMultiline("##addMobs", ref mobsToAdd, 100000,
                    new Vector2(ImGui.GetContentRegionAvail().X, Math.Min((mobsToAdd.Split('\n').Length + 1) * ImGui.CalcTextSize("AAAAAAAA").Y, 300f)));
                if(ImGui.Button($"新增怪物"))
                {
                    foreach(var mob in mobsToAdd.Split("\n"))
                    {
                        var trimmed = mob.Trim();
                        if(trimmed.Length > 0)
                        {
                            p.cfg.mobPulled_Names.Add(trimmed);
                        }
                    }
                    mobsToAdd = "";
                    p.mobPulled.RebuildMobNames();
                    p.mobPulled.ClearIgnoredMobs();
                }
            }

            if(ImGui.CollapsingHeader($"啟用此模組的區域清單，目前共 {p.cfg.mobPulled_Territories.Count} 筆###MPListOfTerr"))
            {
                ImGui.SetNextItemWidth(200f);
                ImGui.InputTextWithHint("##terrSearch", "篩選...", ref terrSearchOptions.filter, 100);
                ImGui.SameLine();
                ImGui.Checkbox("僅限世界地圖區域", ref terrSearchOptions.onlyWorld);
                ImGui.SameLine();
                ImGui.Checkbox("僅顯示已選取", ref terrSearchOptions.onlySelected);
                if(p.mobPulled.territories.TryGetValue(Svc.ClientState.TerritoryType, out var v))
                {
                    MPPrintZone(Svc.ClientState.TerritoryType, v);
                }
                foreach(var k in p.mobPulled.territories)
                {
                    MPPrintZone(k.Key, k.Value);
                }
            }
        }
    }

    private void MPPrintZone(uint territoryType, (string name, bool isWorld) v)
    {
        tCounter++;
        var cname = $"{territoryType} | {v.name}{(v.isWorld ? "（世界地圖區域）" : "")}";
        if(terrSearchOptions.filter.Length > 0 && !cname.Contains(terrSearchOptions.filter, StringComparison.OrdinalIgnoreCase)) return;
        if(terrSearchOptions.onlyWorld && !v.isWorld) return;
        if(terrSearchOptions.onlySelected && !p.cfg.mobPulled_Territories.Contains(territoryType)) return;
        if(v.isWorld) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
        var chk = p.cfg.mobPulled_Territories.Contains(territoryType);
        if(ImGui.Checkbox(cname + "##" + tCounter, ref chk))
        {
            if(chk)
            {
                p.cfg.mobPulled_Territories.Add(territoryType);
            }
            else
            {
                p.cfg.mobPulled_Territories.Remove(territoryType);
            }
            if(Svc.ClientState.IsLoggedIn) p.mobPulled.TerritoryChanged(Svc.ClientState.TerritoryType);
        }
        if(v.isWorld) ImGui.PopStyleColor();
    }
}
