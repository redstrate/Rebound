using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;

namespace Rebound
{
    public sealed class Plugin : IDalamudPlugin
    {
        private readonly IFramework framework;
        private DalamudPluginInterface PluginInterface { get; init; }

        [PluginService]
        internal static IGameInteropProvider Hooking { get; private set; } = null!;

        // g_Client::System::Framework::Framework::InstancePointer2
        // Used as the dummy call when we don't update the bone for a frame
        [Signature("48 8B 05 ?? ?? ?? ?? F3 0F 10 B0 ?? ?? ?? ?? F3 41 0F 5D F2")]
        private readonly IntPtr frameworkPointer = IntPtr.Zero;

        /// The detour function signature
        private delegate IntPtr BoneSimulatorUpdate(IntPtr a1, IntPtr a2);

        // Client::Graphics::Physics::BoneSimulator::Update
        // This is called for each BoneSimulator, such as hair, ears, etc
        [Signature("48 8B C4 48 89 48 08 55 48 81 EC", DetourName = nameof(BoneUpdate))]
        private readonly Hook<BoneSimulatorUpdate>? boneSimulatorUpdateHook = null!;

        /// If the physics simulation should be ran 
        public bool ExecutePhysics;

        /// If the physics were ran for this slice
        public bool RanPhysics;

        /// The target FPS the physics should be run at
        private const double TargetFps = 60.0;

        /// The number of ticks for the length of the target FPS
        private static long SliceLength => (long)(1 / TargetFps * TimeSpan.TicksPerSecond);

        /// Timekeeping state
        private long startTick;

        public long EndTick => startTick + SliceLength;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] IFramework framework)
        {
            this.framework = framework;
            this.PluginInterface = pluginInterface;
            Hooking.InitializeFromAttributes(this);

            startTick = DateTime.Now.Ticks;

            boneSimulatorUpdateHook?.Enable();
            framework.Update += Update;
        }

        // Called every frame.
        public void Update(IFramework _)
        {
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
            {
                ExecutePhysics = false;
            }
            else
            {
                RanPhysics = true;
                ExecutePhysics = true;
            }
        }

        /// Our new bone simulator update function.
        /// Called for each BoneSimulator, so possibly multiple times every frame. Should be kept very simple for performance reasons.
        private IntPtr BoneUpdate(IntPtr a1, IntPtr a2)
        {
            // Update the physics if requested, otherwise don't do anything.
            return ExecutePhysics ? boneSimulatorUpdateHook!.Original(a1, a2) : frameworkPointer;
        }

        public void Dispose()
        {
            boneSimulatorUpdateHook?.Dispose();
            framework.Update -= Update;
        }
    }
}
