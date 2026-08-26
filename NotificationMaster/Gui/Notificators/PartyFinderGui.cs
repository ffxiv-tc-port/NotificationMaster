using NotificationMaster.Notificators;

namespace NotificationMaster;

internal partial class ConfigGui
{
    internal void DrawPartyFinderConfig()
    {
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.partyFinder_Enable))
        {
            PartyFinder.Setup(p.cfg.partyFinder_Enable, p);
        }
        if(p.cfg.partyFinder_Enable)
        {
            ImGui.Checkbox("Only when the party fills up".Loc(), ref p.cfg.partyFinder_OnlyWhenFilled);
            ImGui.Checkbox("Notify if the party is delisted".Loc(), ref p.cfg.partyFinder_Delisted);

            if(p.cfg.partyFinder_OnlyWhenFilled)
            {
                if(p.cfg.partyFinder_Delisted)
                {
                    ImGui.Text("When the party fills or is delisted".Loc());
                }
                else
                {
                    ImGui.Text("When the party fills".Loc());
                }
            }
            else
            {
                if(p.cfg.partyFinder_Delisted)
                {
                    ImGui.Text("When someone joins or leaves the party, or the party is delisted".Loc());
                }
                else
                {
                    ImGui.Text("When someone joins or leaves the party".Loc());
                }
            }

            ImGui.Text("do the following if FFXIV is running in background: ".Loc());

            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.partyFinder_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.partyFinder_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.partyFinder_AutoActivateWindow);
            ForegroundWarning(p.cfg.partyFinder_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.partyFinder_SoundSettings);
        }
    }
}
