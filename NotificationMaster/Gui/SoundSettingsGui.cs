namespace NotificationMaster;

internal partial class ConfigGui
{
    private void DrawSoundSettings(ref SoundSettings settings)
    {
        ImGui.Checkbox("Play sound".Loc(), ref settings.PlaySound);
        if(settings.PlaySound)
        {
            ImGui.Text("Path to file: ".Loc());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 100);
            ImGui.InputText("##PathToFile", ref settings.SoundPath, 1000);
            ImGui.SameLine();
            if(ImGui.Button("Browse...".Loc()))
            {
                p.fileSelector.SelectFile(settings);
            }
            if(ImGui.Button("Test".Loc()))
            {
                p.audioPlayer.Play(settings.SoundPath, false, settings.Volume, settings.Repeat);
            }
            ImGui.SameLine();
            if(ImGui.Button("Stop".Loc()))
            {
                p.audioPlayer.Stop();
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("Stop playing once game is focused".Loc(), ref settings.StopSoundOnceFocused))
            {
                if(!settings.StopSoundOnceFocused) settings.Repeat = false;
            }
            ImGui.SameLine();
            if(ImGui.Checkbox("Repeat".Loc(), ref settings.Repeat))
            {
                if(settings.Repeat) settings.StopSoundOnceFocused = true;
            }
            ImGui.SameLine();
            ImGui.Text("|  Volume: ".Loc());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.SliderFloat("##volume", ref settings.Volume, 0f, 1f);
        }
    }
}
