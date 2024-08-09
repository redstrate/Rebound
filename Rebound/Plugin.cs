using System;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;

namespace Rebound;

public sealed class Plugin : IDalamudPlugin
{
    /// The target FPS the physics should be run at
    private const double TargetFps = 60.0;
    
    [Signature(Constants.BoneSimulatorUpdateSignature, DetourName = nameof(BoneUpdate))]
    private readonly Hook<BoneSimulatorUpdate>? boneSimulatorUpdateHook = null!;

    // g_Client::System::Framework::Framework::InstancePointer2
    // Used as the dummy call when we don't update the bone for a frame
    // In DT, this seems to be a simple null pointer now
    private readonly IntPtr frameworkPointer = IntPtr.Zero;

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

        startTick = DateTime.Now.Ticks;

        boneSimulatorUpdateHook?.Enable();
        Framework.Update += Update;

#if DEBUG
        DebugWindow = new DebugWindow(this);
        WindowSystem.AddWindow(DebugWindow);
        DebugWindow.IsOpen = true;
        BoneSimulatorWindow = new BoneSimulatorWindow();
        WindowSystem.AddWindow(BoneSimulatorWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
#endif
    }

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static IGameInteropProvider Hooking { get; private set; } = null!;

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    /// The number of ticks for the length of the target FPS
    private static long SliceLength => (long)(1 / TargetFps * TimeSpan.TicksPerSecond);

    public long EndTick => startTick + SliceLength;

    public void Dispose()
    {
        boneSimulatorUpdateHook?.Dispose();
        Framework.Update -= Update;

#if DEBUG
        WindowSystem.RemoveAllWindows();
        DebugWindow.Dispose();
#endif
    }

    // Called every frame.
    public void Update(IFramework _)
    {
#if DEBUG
        if (!EnableFix)
        {
            ExecutePhysics = true;
            return;
        }
#endif

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
    }

    /// Our new bone simulator update function.
    /// Called for each BoneSimulator, so possibly multiple times every frame. Should be kept very simple for performance reasons.
    private unsafe IntPtr BoneUpdate(BoneSimulator* a1, IntPtr a2)
    {
        // Avoid updating the hair bangs, they tend to show the worst of the clipping.
        if (a1->Group != BoneSimulator.PhysicsGroup.HairA)
            return ExecutePhysics ? boneSimulatorUpdateHook!.Original(a1, a2) : frameworkPointer;

        return boneSimulatorUpdateHook!.Original(a1, a2);
    }

#if DEBUG
    private void DrawUI()
    {
        WindowSystem.Draw();
    }
#endif

    /// The detour function signature
    private unsafe delegate IntPtr BoneSimulatorUpdate(BoneSimulator* a1, IntPtr a2);

#if DEBUG
    public readonly WindowSystem WindowSystem = new("Rebound");
    private DebugWindow DebugWindow { get; init; }
    public BoneSimulatorWindow BoneSimulatorWindow { get; init; }
    [PluginService]
    public static IClientState ClientState { get; private set; } = null!;
#endif
}
