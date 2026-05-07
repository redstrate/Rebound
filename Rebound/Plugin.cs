using System;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Physics.BonePhysicsUpdater;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace Rebound;

public sealed class Plugin : IDalamudPlugin
{
    private readonly Hook<BonePhysicsUpdater.Delegates.BoneSimulatorTask>? boneSimulatorUpdateHook = null!;

    [Signature("F3 0F 10 89 ?? ?? ?? ?? 4C 8B C2", DetourName = nameof(TaskHook))]
    private readonly Hook<TaskHookDelegate>? taskHook = null!;

#if DEBUG
    /// If the fix should be enabled, it's only toggleable here for debug purposes
    public bool EnableFix = true;
#endif

    /// If the physics simulation should be run
    public bool ExecutePhysics;

    /// If the physics were ran for this slice
    public bool RanPhysics;

    /// Timekeeping state
    private long startTick;

    public Plugin()
    {
        Hooking.InitializeFromAttributes(this);
        unsafe
        {
            boneSimulatorUpdateHook = Plugin.Hooking.HookFromAddress<BonePhysicsUpdater.Delegates.BoneSimulatorTask>((nint)BonePhysicsUpdater.MemberFunctionPointers.BoneSimulatorTask, BoneUpdate);
        }

        startTick = DateTime.Now.Ticks;

        boneSimulatorUpdateHook?.Enable();
        taskHook?.Enable();

#if DEBUG
        DebugWindow = new DebugWindow(this);
        WindowSystem.AddWindow(DebugWindow);
        DebugWindow.IsOpen = true;
        BoneSimulatorWindow = new BoneSimulatorWindow();
        WindowSystem.AddWindow(BoneSimulatorWindow);

        PluginInterface.UiBuilder.Draw += DrawUi;
#endif
    }

    [PluginService]
    internal static IGameInteropProvider Hooking { get; private set; } = null!;

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    /// The target FPS the physics should be run at
    public const double TargetFps = 60.0;

    /// The number of ticks for the length of the target FPS
    private static long SliceLength => (long)(1 / TargetFps * TimeSpan.TicksPerSecond);

    public long EndTick => startTick + SliceLength;

    public void Dispose()
    {
        boneSimulatorUpdateHook?.Dispose();
        taskHook?.Dispose();

#if DEBUG
        WindowSystem.RemoveAllWindows();
        DebugWindow.Dispose();
#endif
    }

    /// Our new bone simulator update function.
    /// Called for each BoneSimulator, so possibly multiple times every frame. Should be kept very simple for performance reasons.
    private unsafe void BoneUpdate(BonePhysicsUpdater* self, UpdateBoneSimulatorJobData* data)
    {
        if (!ExecutePhysics)
        {
            return;
        }

        boneSimulatorUpdateHook!.Original(self, data);
    }

    private unsafe void TaskHook(void* a1, void* a2)
    {
#if DEBUG
        if (!EnableFix)
        {
            taskHook!.Original(a1, a2);
            return;
        }
#endif

        // Don't apply our "fix" to cutscenes, the delay in updates causes animations to look buggy.
        if (PluginInterface.UiBuilder.CutsceneActive)
        {
            taskHook!.Original(a1, a2);
            return;
        }

        ExecutePhysics = false;

        // Disable physics while we're in the "off" or idle ticks.
        // If the current FPS is lower than the target FPS, this should never run and the physics should always be running.
        var currentTick = DateTime.Now.Ticks;
        while (currentTick > EndTick)
        {
            startTick = EndTick + 1;
            RanPhysics = false;
        }

        if (RanPhysics)
            ExecutePhysics = false;
        else
        {
            RanPhysics = true;
            ExecutePhysics = true;
        }

        taskHook!.Original(a1, a2);
    }

    /// The detour function signature
    private unsafe delegate void TaskHookDelegate(void* a1, void* a2);

#if DEBUG
    private void DrawUi() => WindowSystem.Draw();
#endif

#if DEBUG
    public readonly WindowSystem WindowSystem = new("Rebound");
    private DebugWindow DebugWindow { get; init; }
    public BoneSimulatorWindow BoneSimulatorWindow { get; init; }
    [PluginService]
    public static IClientState ClientState { get; private set; } = null!;
#endif
}
