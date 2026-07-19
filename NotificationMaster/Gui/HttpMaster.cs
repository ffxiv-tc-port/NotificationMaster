namespace NotificationMaster;

internal partial class ConfigGui
{
    private string[] HttpTypes = { "GET", "POST", "JSON POST" };

    private void DrawHttpMaster(List<HttpRequestElement> l, ref bool enable, string placeholders = "")
    {
        ImGui.Checkbox("##PerformRequests", ref enable);
        ImGui.SameLine();
        if(ImGui.CollapsingHeader("執行以下 HTTP 請求："))
        {
            ImGui.TextUnformatted("你可以使用以下佔位符：\n" + placeholders);
            if(ImGui.Button("-  新增  -"))
            {
                l.Add(new HttpRequestElement());
            }
            var i = 0;
            var toDelete = -1;
            foreach(var e in l)
            {
                i++;
                if(ImGui.Button("刪除##" + i) && ImGui.GetIO().KeyCtrl)
                {
                    toDelete = i - 1;
                }
                if(ImGui.IsItemHovered()) ImGui.SetTooltip("按住 CTRL 並點擊以刪除");
                ImGui.SameLine();
                if(ImGui.Button("測試##" + i))
                {
                    p.httpMaster.DoRequests(l,
                        [
                            ["$N", "The Aurum Vale"],
                            ["$T", "45"]
                        ]
                    );
                }
                ImGui.SameLine();
                ImGui.Text("網址：");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100f);
                ImGui.Combo("##type" + i, ref e.Type, HttpTypes, HttpTypes.Length);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("##url" + i, ref e.URI, 100000);
                ImGui.Text("內容：");
                ImGui.InputTextMultiline("##MultilineContent" + i, ref e.Content, 1000000, new Vector2(ImGui.GetContentRegionAvail().X, Math.Min((e.Content.Split('\n').Length + 1) * ImGui.CalcTextSize("AAAAAAAA").Y, 300f)));
                ImGui.Separator();
            }
            try
            {
                if(toDelete >= 0)
                {
                    l.RemoveAt(toDelete);
                    toDelete = -1;
                }
            }
            catch(Exception e)
            {
                PluginLog.Error($"Error: {e.Message}\n{e.StackTrace ?? ""}");
            }
        }
    }
}
