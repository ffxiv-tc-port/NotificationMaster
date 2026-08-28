using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System.Text.RegularExpressions;

namespace NotificationMaster;

internal class ChatMessage : IDisposable
{
    private NotificationMaster p;
    internal (ushort Type, string Sender, string Message)[] ChatLog = new (ushort, string, string)[100];
    internal bool pause = false;

    public void Dispose()
    {
        Svc.Chat.ChatMessage -= Chat_ChatMessage;
    }

    public ChatMessage(NotificationMaster plugin)
    {
        p = plugin;
        Svc.Chat.ChatMessage += Chat_ChatMessage;
    }

    private void Chat_ChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if(p.configGui.open)
        {
            if(!pause) ChatLog.AddShifting(((ushort)type, sender.ToString(), message.ToString()));
        }
        //else
        {
            if(p.PauseUntil > Environment.TickCount64) return;
            if(!Utils.IsApplicationActivated || p.cfg.chatMessage_AlwaysExecute)
            {
                var senderFullStr = sender.ToString();
                var messageFullStr = message.ToString();
                // 一則訊息可能同時命中多條規則，但只念一次。
                var praised = false;
                foreach(var e in p.cfg.chatMessage_Elements)
                {
                    if(
                        (e.ChatTypes.Count == 0 || e.ChatTypes.Contains((ushort)type))
                        && (e.MessageStr == ""
                            //|| (e.MessageStr == "" && messageFullStr == "")
                            || (e.CompareType == 0 && messageFullStr.Contains(e.MessageStr, StringComparison.Ordinal))
                            || (e.CompareType == 1 && messageFullStr.Contains(e.MessageStr, StringComparison.OrdinalIgnoreCase))
                            || (e.CompareType == 2 && Regex.IsMatch(messageFullStr, e.MessageStr))
                        )
                        && (e.SenderStr == ""
                            //|| (e.SenderStr == "" && senderFullStr == "")
                            || (e.CompareType == 0 && senderFullStr.Contains(e.SenderStr, StringComparison.Ordinal))
                            || (e.CompareType == 1 && senderFullStr.Contains(e.SenderStr, StringComparison.OrdinalIgnoreCase))
                            || (e.CompareType == 2 && Regex.IsMatch(senderFullStr, e.SenderStr))
                        )
                    )
                    {
                        // 沿用這條規則既有的比對條件（頻道／寄件者／訊息內容）才會走到這裡。
                        // ⚠️ 與其他模組不同，整個比對迴圈本來就包在「遊戲不在前景」的閘門裡，
                        // 所以語音同樣跟著那個閘門走。
                        if(p.cfg.chatMessage_TataruPraise && !praised)
                        {
                            praised = true;
                            TataruPraiseBridge.Praise(TataruPraiseBridge.CategoryChatMessage);
                        }
                        if(p.cfg.chatMessage_FlashTrayIcon && !e.NoFlash)
                        {
                            Native.Impl.FlashWindow();
                        }
                        if(p.cfg.chatMessage_AutoActivateWindow && !e.NoForeground) Native.Impl.Activate();
                        if(p.cfg.chatMessage_ShowToastNotification && !e.NoToast)
                        {
                            TrayIconManager.ShowToast(messageFullStr, senderFullStr == "" ? "Message".Loc() : "Message from ??".Loc(senderFullStr));
                        }
                        if(p.cfg.chatMessage_HttpRequestsEnable && !e.NoHTTP)
                        {
                            p.httpMaster.DoRequests(p.cfg.chatMessage_HttpRequests,
                                new string[][]
                                {
                                    new string[] {"$S", senderFullStr},
                                    new string[] {"$M", messageFullStr},
                                    new string[] {"$T", type.ToString()},
                                }
                            );
                        }
                        if(p.cfg.chatMessage_SoundSettings.PlaySound)
                        {
                            p.audioPlayer.Play(p.cfg.chatMessage_SoundSettings);
                        }
                    }
                }
            }
        }
    }

    internal static void Setup(bool enable, NotificationMaster p)
    {
        if(enable)
        {
            if(p.chatMessage == null)
            {
                p.chatMessage = new ChatMessage(p);
                PluginLog.Information("Enabling chat message module");
            }
            else
            {
                PluginLog.Information("chat message module already enabled");
            }
        }
        else
        {
            if(p.chatMessage != null)
            {
                p.chatMessage.Dispose();
                p.chatMessage = null;
                PluginLog.Information("Disabling chat message module");
            }
            else
            {
                PluginLog.Information("chat message module already disabled");
            }
        }
    }
}

[Serializable]
public class ChatMessageElement
{
    [NonSerialized] internal static readonly string[] CompareTypes = { "Exact", "Case insensitive", "Regexp" };

    public ushort ChatType = 0;
    public HashSet<ushort> ChatTypes = [];
    /// <summary>
    /// 0: Exact; 1: Case insensitive; 2: Regexp
    /// </summary>
    public int CompareType = 0;
    public string SenderStr = "";
    public string MessageStr = "";
    public bool NoFlash = false;
    public bool NoForeground = false;
    public bool NoToast = false;
    public bool NoHTTP = false;
}
