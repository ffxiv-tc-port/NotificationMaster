namespace NotificationMaster;

internal partial class ConfigGui
{
    private void DrawSoundSettings(ref SoundSettings settings)
    {
        ImGui.Checkbox("播放音效", ref settings.PlaySound);
        if(settings.PlaySound)
        {
            ImGui.Text("檔案路徑： ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100);
            ImGui.InputText("##PathToFile", ref settings.SoundPath, 1000);
            ImGui.SameLine();
            if(ImGui.Button("瀏覽..."))
            {
                p.fileSelector.SelectFile(settings);
            }
            if(ImGui.Button("測試"))
            {
                p.audioPlayer.Play(settings.SoundPath, false, settings.Volume, settings.Repeat);
            }
            ImGui.SameLine();
            if(ImGui.Button("停止"))
            {
                p.audioPlayer.Stop();
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("遊戲取得焦點後停止播放", ref settings.StopSoundOnceFocused))
            {
                if(!settings.StopSoundOnceFocused) settings.Repeat = false;
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("重複播放", ref settings.Repeat))
            {
                if(settings.Repeat) settings.StopSoundOnceFocused = true;
            }
            ImGui.SameLine();
            ImGui.Text("｜ 音量： ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.SliderFloat("##volume", ref settings.Volume, 0f, 1f);
        }
    }
}
