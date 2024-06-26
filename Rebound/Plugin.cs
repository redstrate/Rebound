﻿using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;

namespace Rebound
{
    public sealed class Plugin : IDalamudPlugin
    {
        [PluginService]
        internal static IFramework Framework { get; private set; } = null!;

        [PluginService]
        internal static IGameInteropProvider Hooking { get; private set; } = null!;
        
        [PluginService]
        internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        
        #if DEBUG
        public readonly WindowSystem WindowSystem = new("Rebound");
        private DebugWindow DebugWindow { get; init; }
        #endif
        
        // g_Client::System::Framework::Framework::InstancePointer2
        // Used as the dummy call when we don't update the bone for a frame
        // In DT, this seems to be a simple null pointer now
        private readonly IntPtr frameworkPointer = IntPtr.Zero;

        /// The detour function signature
        private delegate IntPtr BoneSimulatorUpdate(IntPtr a1, IntPtr a2);

        // Client::Graphics::Physics::BoneSimulator::Update
        // This is called for each BoneSimulator, such as hair, ears, etc
        [Signature("48 8B C4 48 89 48 08 55 53 5641", DetourName = nameof(BoneUpdate))]
        private readonly Hook<BoneSimulatorUpdate>? boneSimulatorUpdateHook = null!;

        #if DEBUG
        /// If the fix should be enabled, it's only toggleable here for debug purposes
        public bool EnableFix = true;
        #endif
        
        /// If the physics simulation should be run 
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

            PluginInterface.UiBuilder.Draw += DrawUI;
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
            Framework.Update -= Update;
            
            #if DEBUG
            WindowSystem.RemoveAllWindows();
            DebugWindow.Dispose();
            #endif
        }
        
        #if DEBUG
        private void DrawUI() => WindowSystem.Draw();
        #endif
    }
}
