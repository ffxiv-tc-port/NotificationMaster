using NotificationMaster.Notificators;

namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawPartyFinderConfig()
    {
        if(ImGui.Checkbox("啟用", ref p.cfg.partyFinder_Enable))
        {
            PartyFinder.Setup(p.cfg.partyFinder_Enable, p);
        }
        if(p.cfg.partyFinder_Enable)
        {
            ImGui.Checkbox("僅在隊伍滿員時", ref p.cfg.partyFinder_OnlyWhenFilled);
            ImGui.Checkbox("隊伍被下架時通知", ref p.cfg.partyFinder_Delisted);

            if(p.cfg.partyFinder_OnlyWhenFilled)
            {
                if(p.cfg.partyFinder_Delisted)
                {
                    ImGui.Text("當隊伍滿員或被下架時");
                }
                else
                {
                    ImGui.Text("當隊伍滿員時");
                }
            }
            else
            {
                if(p.cfg.partyFinder_Delisted)
                {
                    ImGui.Text("當有人加入或離開隊伍，或隊伍被下架時");
                }
                else
                {
                    ImGui.Text("當有人加入或離開隊伍時");
                }
            }

            ImGui.Text("若 FFXIV 在背景執行，執行以下動作： ");

            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.partyFinder_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.partyFinder_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.partyFinder_AutoActivateWindow);
            ForegroundWarning(p.cfg.partyFinder_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.partyFinder_SoundSettings);
        }
    }
}
