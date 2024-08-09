#if DEBUG
using System;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using ImGuiNET;

namespace Rebound;

public class BoneSimulatorWindow()
    : Window("Bone Simulators",
             ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoTitleBar),
      IDisposable
{
    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Bone Simulators:");

        ImGui.BeginChild("bonesim", new Vector2(-1, -1), true);

        unsafe
        {
            var player = Plugin.ClientState.LocalPlayer;
            if (player != null)
            {
                var model = (CharacterBase*)((GameObject*)player.Address)->DrawObject;
                var bonePhysicsModule = model->BonePhysicsModule;
                if (bonePhysicsModule != null)
                {
                    FillBoneSimulators(bonePhysicsModule->BoneSimulators.BoneSimulator_1);
                    FillBoneSimulators(bonePhysicsModule->BoneSimulators.BoneSimulator_2);
                    FillBoneSimulators(bonePhysicsModule->BoneSimulators.BoneSimulator_3);
                    FillBoneSimulators(bonePhysicsModule->BoneSimulators.BoneSimulator_4);
                    FillBoneSimulators(bonePhysicsModule->BoneSimulators.BoneSimulator_5);
                }
                else
                    ImGui.Text("Player found, but no BonePhysicsModule.");
            }
            else
                ImGui.Text("Player not found.");
        }

        ImGui.EndChild();
    }

    private void FillBoneSimulators(StdVector<Pointer<BoneSimulator>> boneSimulator1)
    {
        for (long i = 0; i < boneSimulator1.LongCount; i++)
            unsafe
            {
                ImGui.Text($"Bone Simulator {i}");
                DrawBoneSimulator(boneSimulator1[i].Value);
                ImGui.Separator();
            }
    }

    private void CtrlVec3(string label, ref Vector3 value)
    {
        var newVec = new System.Numerics.Vector3(value.X, value.Y, value.Z);
        if (ImGui.DragFloat3(label, ref newVec)) value = new Vector3(newVec.X, newVec.Y, newVec.Z);
    }

    private void CtrlFloat(string label, ref float value)
    {
        ImGui.DragFloat(label, ref value);
    }

    private unsafe void DrawBoneSimulator(BoneSimulator* boneSimulator)
    {
        ImGui.PushID((IntPtr)boneSimulator);
        ImGui.TextWrapped($"Physics Group: {boneSimulator->Group}");
        CtrlVec3("Character Position", ref boneSimulator->CharacterPosition);
        CtrlVec3("Gravity", ref boneSimulator->Gravity);
        CtrlVec3("Wind", ref boneSimulator->Wind);
        CtrlFloat("Spring", ref boneSimulator->Spring);
    }
}
#endif
