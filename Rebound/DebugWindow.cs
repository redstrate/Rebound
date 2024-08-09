#if DEBUG
using System;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;

namespace Rebound;

public class DebugWindow(Plugin plugin)
    : Window("Rebound Debug",
             ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar),
      IDisposable
{
    private int frameIndex;
    private int maxTimesRan;
    private int timesRan;
    private float[] values = new float[144];

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Spacing();

        float fps;
        unsafe
        {
            fps = Framework.Instance()->FrameRate;
        }

        if ((int)fps > values.Length) values = new float[(int)fps];

        if (frameIndex > 0 && frameIndex < values.Length)
        {
            values[frameIndex] = plugin.ExecutePhysics ? 0.5f : 0.0f;
            if (plugin.ExecutePhysics) timesRan++;
        }

        maxTimesRan = int.Max(timesRan, maxTimesRan);

        frameIndex++;
        if (frameIndex > fps)
        {
            frameIndex = 0;
            timesRan = 0;
        }

        ImGui.PlotLines("##Physics Ticks", ref values[0], (int)fps);

        ImGui.Text($"Max Times Ran: {maxTimesRan}");

        ImGui.TextWrapped("This graph shows which ticks the game physics are running within the current second.");
        ImGui.TextWrapped(
            "High values are when it runs, and low values are when it's not (the diagonal lines are just a limitation of the widget I chose to use, they mean nothing.)");
        ImGui.Separator();
        ImGui.TextWrapped(
            "If your current FPS is below 60, it should be all HIGH values since the physics should always be running.");
        ImGui.TextWrapped(
            "If your current FPS is above 60, the space between ticks should be more or less evenly spaced. The higher your FPS, the more ticks you can fit into a second.");
        ImGui.TextWrapped(
            "'Max Times Ran' should ALWAYS be around 60, it may be higher due to how shoddily made this debug window is.");
        ImGui.Separator();
        ImGui.Checkbox("Enable Fix", ref plugin.EnableFix);
        if (ImGui.Button("Reset Times Ran")) maxTimesRan = 0;

        if (ImGui.Button("Show Bone Simulators")) plugin.BoneSimulatorWindow.IsOpen = true;
    }
}
#endif
