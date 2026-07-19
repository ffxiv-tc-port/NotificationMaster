using Dalamud.Game.Text;

namespace NotificationMaster;

internal partial class ConfigGui
{
    private int CustomTypeToAdd = 0;
    internal void DrawChatMessageGui()
    {
        var id = 0;
        var toDelete = -1;
        if(ImGui.Checkbox("啟用", ref p.cfg.chatMessage_Enable))
        {
            ChatMessage.Setup(p.cfg.chatMessage_Enable, p);
        }
        if(p.cfg.chatMessage_Enable)
        {
            //ImGui.TextColored(ImGui.ColorConvertU32ToFloat4(0xff0000ff), "Triggers are paused while configuration is open.");
            ImGui.TextWrapped("當收到符合任一規則的聊天訊息時，若 FFXIV 在背景執行：");
            ImGui.Checkbox("顯示系統匣通知", ref p.cfg.chatMessage_ShowToastNotification);
            ImGui.Checkbox("閃爍工作列圖示", ref p.cfg.chatMessage_FlashTrayIcon);
            ImGui.Checkbox("將 FFXIV 帶到前景", ref p.cfg.chatMessage_AutoActivateWindow);
            ImGui.Checkbox("即使遊戲在前景也執行動作", ref p.cfg.chatMessage_AlwaysExecute);
            ForegroundWarning(p.cfg.chatMessage_AutoActivateWindow);
            DrawSoundSettings(ref p.cfg.chatMessage_SoundSettings);
            DrawHttpMaster(p.cfg.chatMessage_HttpRequests, ref p.cfg.chatMessage_HttpRequestsEnable,
                "$S - 發送者\n$M - 訊息內容\n$T - 聊天類型");
            ImGui.Separator();
            if(ImGui.CollapsingHeader("觸發規則"))
            {
                //ImGui.BeginChild("##trigs");
                if(ImGui.Button("新增"))
                {
                    p.cfg.chatMessage_Elements.Add(new ChatMessageElement());
                }
                ImGui.Columns(5);
                ImGui.SetColumnWidth(0, 150f);
                ImGui.SetColumnWidth(1, 150f);
                ImGui.SetColumnWidth(2, ImGuiEx.GetWindowContentRegionWidth() - 150 - 150 - 100 - 40);
                ImGui.SetColumnWidth(3, 100f);
                ImGui.SetColumnWidth(4, 40f);
                ImGui.Text("類型");
                ImGui.NextColumn();
                ImGui.Text("發送者");
                ImGui.NextColumn();
                ImGui.Text("訊息內容");
                ImGui.NextColumn();
                ImGui.Text("比對模式");
                ImGui.NextColumn();
                ImGui.Text("刪除");
                ImGui.NextColumn();
                ImGui.Columns(1);
                for(var i = 0; i < p.cfg.chatMessage_Elements.Count; i++)
                {
                    var elem = p.cfg.chatMessage_Elements[i];
                    ImGui.Columns(5);
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if(ImGui.BeginCombo("##fselect" + i, elem.ChatTypes.Count == 0 ? "任何" : elem.ChatTypes.Count == 1 ? ((XivChatType)elem.ChatTypes.First()).ToString() : $"{elem.ChatTypes.Count} 種類型"))
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
                        if(ImGui.Button("新增自訂類型"))
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
                    ImGui.Combo("##f4" + i, ref elem.CompareType, ChatMessageElement.CompareTypes, ChatMessageElement.CompareTypes.Length);
                    ImGui.NextColumn();
                    if(ImGui.Button("[X]##del" + i))
                    {
                        toDelete = i;
                    }
                    ImGui.NextColumn();
                    ImGui.Columns(1);
                    ImGui.Text("例外：");
                    ImGui.SameLine();
                    ImGui.Checkbox("不閃爍##" + i, ref elem.NoFlash);
                    ImGui.SameLine();
                    ImGui.Checkbox("不帶到前景##" + i, ref elem.NoForeground);
                    ImGui.SameLine();
                    ImGui.Checkbox("不顯示提示##" + i, ref elem.NoToast);
                    ImGui.SameLine();
                    ImGui.Checkbox("不發送 HTTP##" + i, ref elem.NoHTTP);
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
            if(ImGui.CollapsingHeader("訊息記錄"))
            {
                //ImGui.BeginChild("##nm_chatlog");
                ImGui.Checkbox("暫停記錄", ref p.chatMessage.pause);
                if(p.chatMessage != null)
                {
                    ImGui.Columns(3);
                    ImGui.Text("類型");
                    ImGui.NextColumn();
                    ImGui.Text("發送者");
                    ImGui.NextColumn();
                    ImGui.Text("訊息內容");
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
                    ImGui.Text("錯誤");
                }
                //ImGui.EndChild();
            }
        }
    }
}
