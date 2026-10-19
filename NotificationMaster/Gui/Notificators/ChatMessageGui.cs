using Dalamud.Game.Text;

namespace NotificationMaster;

internal partial class ConfigGui
{
    private int CustomTypeToAdd = 0;
    private static string[] compareTypesLoc = null;
    internal void DrawChatMessageGui()
    {
        var id = 0;
        var toDelete = -1;
        if(ImGui.Checkbox("Enable".Loc(), ref p.cfg.chatMessage_Enable))
        {
            ChatMessage.Setup(p.cfg.chatMessage_Enable, p);
        }
        if(p.cfg.chatMessage_Enable)
        {
            //ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff0000ff), "Triggers are paused while configuration is open.");
            ImGui.TextWrapped("When chat message matching any rule received, if FFXIV is running in background:".Loc());
            ImGui.Checkbox("Show tray notification".Loc(), ref p.cfg.chatMessage_ShowToastNotification);
            ImGui.Checkbox("Flash taskbar icon".Loc(), ref p.cfg.chatMessage_FlashTrayIcon);
            ImGui.Checkbox("Bring FFXIV to foreground".Loc(), ref p.cfg.chatMessage_AutoActivateWindow);
            ImGui.Checkbox("Execute actions even if game is active".Loc(), ref p.cfg.chatMessage_AlwaysExecute);
            ImGui.Checkbox("Ask Tataru to remind you when this triggers (requires TataruPraise)".Loc(), ref p.cfg.chatMessage_TataruPraise);
            if(ImGui.IsItemHovered()) ImGui.SetTooltip("Plays a TataruPraise voice line through IPC, under the same conditions as the actions above. Silently skipped if TataruPraise is not installed or is turned off.".Loc());
            ForegroundWarning(p.cfg.chatMessage_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.chatMessage_SoundSettings);
            DrawHttpMaster(p.cfg.chatMessage_HttpRequests, ref p.cfg.chatMessage_HttpRequestsEnable,
                "$S - sender\n$M - message\n$T - chat type".Loc());
            ImGui.Separator();
            if(ImGui.CollapsingHeader("Triggers".Loc()))
            {
                //ImGui.BeginChild("##trigs");
                if(ImGui.Button("Add".Loc()))
                {
                    p.cfg.chatMessage_Elements.Add(new ChatMessageElement());
                }
                ImGui.Columns(5);
                ImGui.SetColumnWidth(0, 150f);
                ImGui.SetColumnWidth(1, 150f);
                ImGui.SetColumnWidth(2, ImGuiEx.GetWindowContentRegionWidth() - 150 - 150 - 100 - 40);
                ImGui.SetColumnWidth(3, 100f);
                ImGui.SetColumnWidth(4, 40f);
                ImGui.Text("Type".Loc());
                ImGui.NextColumn();
                ImGui.Text("Sender".Loc());
                ImGui.NextColumn();
                ImGui.Text("Message".Loc());
                ImGui.NextColumn();
                ImGui.Text("Search mode".Loc());
                ImGui.NextColumn();
                ImGui.Text("Del".Loc());
                ImGui.NextColumn();
                ImGui.Columns(1);
                for(var i = 0; i < p.cfg.chatMessage_Elements.Count; i++)
                {
                    var elem = p.cfg.chatMessage_Elements[i];
                    ImGui.Columns(5);
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if(ImGui.BeginCombo("##fselect" + i, elem.ChatTypes.Count == 0 ? "Any".Loc() : elem.ChatTypes.Count == 1 ? ((XivChatType)elem.ChatTypes.First()).ToString() : "?? types".Loc(elem.ChatTypes.Count)))
                    {
                        var customElements = new HashSet<ushort>(elem.ChatTypes);
                        customElements.RemoveWhere(p => Enum.GetValues<XivChatType>().ToHashSet().Contains((XivChatType)p));
                        var elemenets = Enum.GetValues<XivChatType>().Select(e => (ushort)e).ToHashSet().Union(customElements);
                        foreach(var e in elemenets)
                        {
                            var selected = elem.ChatTypes.Contains(e);
                            ImGui.Checkbox(((XivChatType)e).ToString(), ref selected);
                            if(selected)
                            {
                                elem.ChatTypes.Add(e);
                            }
                            else
                            {
                                elem.ChatTypes.Remove(e);
                            }
                        }
                        ImGui.SetNextItemWidth(50f);
                        ImGui.InputInt("##typecustom" + i, ref CustomTypeToAdd, 0, 0);
                        ImGui.SameLine();
                        if(ImGui.Button("Add custom type".Loc()))
                        {
                            elem.ChatTypes.Add((ushort)CustomTypeToAdd);
                            CustomTypeToAdd = 0;
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.InputText("##f2" + i, ref elem.SenderStr, 1000);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.InputText("##f3" + i, ref elem.MessageStr, 1000);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    compareTypesLoc ??= ChatMessageElement.CompareTypes.Select(x => x.Loc()).ToArray();
                    ImGui.Combo("##f4" + i, ref elem.CompareType, compareTypesLoc, compareTypesLoc.Length);
                    ImGui.NextColumn();
                    if(ImGui.Button("[X]##del" + i))
                    {
                        toDelete = i;
                    }
                    ImGui.NextColumn();
                    ImGui.Columns(1);
                    ImGui.Text("Exceptions:".Loc());
                    ImGui.SameLine();
                    ImGui.Checkbox("No flashing".Loc() + "##" + i, ref elem.NoFlash);
                    ImGui.SameLine();
                    ImGui.Checkbox("No bring to foreground".Loc() + "##" + i, ref elem.NoForeground);
                    ImGui.SameLine();
                    ImGui.Checkbox("No toast".Loc() + "##" + i, ref elem.NoToast);
                    ImGui.SameLine();
                    ImGui.Checkbox("No HTTP".Loc() + "##" + i, ref elem.NoHTTP);
                    ImGui.Separator();
                }
                //ImGui.EndChild();
            }
            if(toDelete != -1)
            {
                try
                {
                    p.cfg.chatMessage_Elements.RemoveAt(toDelete);
                }
                catch(Exception e)
                {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace.NotNull());
                }
                toDelete = -1;
            }
            if(ImGui.CollapsingHeader("Message log".Loc()))
            {
                //ImGui.BeginChild("##nm_chatlog");
                ImGui.Checkbox("Pause log".Loc(), ref p.chatMessage.pause);
                if(p.chatMessage != null)
                {
                    ImGui.Columns(3);
                    ImGui.Text("Type".Loc());
                    ImGui.NextColumn();
                    ImGui.Text("Sender".Loc());
                    ImGui.NextColumn();
                    ImGui.Text("Message".Loc());
                    ImGui.NextColumn();
                    ImGui.Columns(1);
                    foreach(var e in p.chatMessage.ChatLog)
                    {
                        if(e.Type != 0)
                        {
                            var cursor = ImGui.GetCursorPos();
                            ImGui.Columns(3);
                            ImGui.TextWrapped($"{e.Type}/{(XivChatType)e.Type}");
                            ImGui.NextColumn();
                            ImGui.TextWrapped(e.Sender.NotNull());
                            ImGui.NextColumn();
                            ImGui.TextWrapped(e.Message.NotNull());
                            ImGui.NextColumn();
                            ImGui.Columns(1);
                            var cursor2 = ImGui.GetCursorPos();
                            ImGui.SetCursorPos(cursor);
                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                            if(ImGui.Selectable("##chm" + id++))
                            {
                                p.cfg.chatMessage_Elements.Add(new ChatMessageElement()
                                {
                                    ChatTypes = [e.Type],
                                    MessageStr = e.Message.Split('\n')[0],
                                    SenderStr = e.Sender.Split('\n')[0]
                                });
                            }
                            ImGui.SetCursorPos(cursor2);
                            ImGui.Separator();
                        }
                    }
                }
                else
                {
                    ImGui.Text("Error".Loc());
                }
                //ImGui.EndChild();
            }
        }
    }
}
