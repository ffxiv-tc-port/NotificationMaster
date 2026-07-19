using Dalamud.Interface.Utility;
using ECommons.Funding;

namespace NotificationMaster;

internal partial class ConfigGui : IDisposable
{
    internal bool open = false;
    internal NotificationMaster p;
    internal ConfigGui(NotificationMaster p)
    {
        this.p = p;
        Svc.PluginInterface.UiBuilder.Draw += Draw;
        PatreonBanner.IsOfficialPlugin = () => true;
    }

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= Draw;
    }

    internal void Draw()
    {
        if(p.PauseUntil > Environment.TickCount64)
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            var sb = new StringBuilder("NotificationMaster 已暫停");
            if(p.PauseUntil != long.MaxValue)
            {
                var ts = TimeSpan.FromMilliseconds(p.PauseUntil - Environment.TickCount64);
                sb.Append($"，剩餘 {(ts.Days * 60 + ts.Hours):D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            }
            var text = sb.ToString();
            var dims = ImGui.CalcTextSize(text);
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(ImGuiHelpers.MainViewport.Size.X / 2 - dims.X / 2, 10f));
            ImGui.Begin("NotificationMasterPauseWarning", ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoInputs);
            ImGui.TextColored(ImGuiColors.DalamudOrange, text);
            ImGui.End();
        }
        if(open)
        {

            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(650f, 200f));
            if(ImGui.Begin("NotificationMaster 設定", ref open))
            {
                if(p.fileSelector.IsSelecting())
                {
                    ImGui.Text("等待選擇檔案...");
                }
                else
                {
                    PatreonBanner.DrawRight();
                    ImGui.BeginTabBar("##NMtabs");
                    DrawTab("GP 恢復", DrawGpNotify, p.cfg.gp_Enable);
                    DrawTab("過場動畫結束", DrawCutsceneConfig, p.cfg.cutscene_Enable);
                    DrawTab("聊天訊息", DrawChatMessageGui, p.cfg.chatMessage_Enable);
                    DrawTab("任務彈出", DrawCfPopConfig, p.cfg.cfPop_Enable);
                    DrawTab("連線錯誤", DrawLoginErrorConfig, p.cfg.loginError_Enable);
                    DrawTab("接近地圖標記", DrawMapFlagConfig, p.cfg.mapFlag_Enable);
                    DrawTab("怪物被拉起", DrawMobPulledConfig, p.cfg.mobPulled_Enable);
                    DrawTab("戰友招募", DrawPartyFinderConfig, p.cfg.partyFinder_Enable);
                    DrawTab("釣魚上鉤通知", DrawFishBiteConfig, p.cfg.fishBite_Enable);
                    PatreonBanner.RightTransparentTab();
                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
            if(!open)
            {
                p.cfg.Save();
                Notify.Success("設定已儲存");
            }
            ImGui.PopStyleVar();
        }
    }

    private void DrawTab(string name, Action function, bool enabled)
    {
        var colored = false;
        if(enabled)
        {
            colored = true;
            ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff00);
        }
        if(ImGui.BeginTabItem($"{name}"))
        {
            if(colored) ImGui.PopStyleColor();
            ImGui.BeginChild($"##{name}-child");
            function();
            ImGui.EndChild();
            ImGui.EndTabItem();
        }
        else
        {
            if(colored) ImGui.PopStyleColor();
        }
    }

    private void ForegroundWarning(bool display)
    {
        if(display)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "很遺憾，將 FFXIV 帶到前景的功能並不十分可靠。\n如果對你無效，很抱歉我們無能為力。");
        }
    }
}
