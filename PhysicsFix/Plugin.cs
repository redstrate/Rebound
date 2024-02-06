using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Physics Fix";

        private DalamudPluginInterface PluginInterface { get; init; }
        
        [PluginService] internal static IGameInteropProvider Hooking { get; private set; } = null!;
        
        // g_Client::System::Framework::Framework_InstancePointer2
        [Signature("48 8B 05 ?? ?? ?? ?? F3 0F 10 B0 ?? ?? ?? ?? F3 41 0F 5D F2")]
        private readonly IntPtr _frameworkPointer = IntPtr.Zero;

        // Client::Graphics::Physics::BoneSimulator_Update
        [Signature("48 8B C4 48 89 48 08 55 48 81 EC", DetourName = nameof(PhysicsSkip))]
        private readonly Hook<PhysicsSkipDelegate>? _physicsSkipHook = null!;
        
        private delegate IntPtr PhysicsSkipDelegate(IntPtr a1, IntPtr a2);
        
        [PluginService] public static IPluginLog Logger { get; private set; }
        
        [PluginService] public static IChatGui Chat { get; private set; }

        private bool _executePhysics = false;
        
        // Called for each BoneSimulator, so possibly multiple times every frame. Should be kept very simple for performance reasons.
        private IntPtr PhysicsSkip(IntPtr a1, IntPtr a2)
        {
            if (_executePhysics)
            {
                return _physicsSkipHook!.Original(a1, a2);
            }

            return _frameworkPointer;
        }

        private IFramework _framework;

        private int _counter = 0;
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IFramework framework)
        {
            _framework = framework;
            this.PluginInterface = pluginInterface;
            Hooking.InitializeFromAttributes(this);

            _physicsSkipHook?.Enable();
            
            Logger.Info("Plugin skip hook!");
            
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(45);
            stringBuilder.AddText($"[HighFPSPhysics] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddText("Plugin skip hook installed! " + (_physicsSkipHook == null));

            Chat.Print(stringBuilder.BuiltString);

            framework.Update += Framework_Update;
        }

        // Called every frame.
        public void Framework_Update(IFramework framework)
        {
            // Our current FPS. UpdateDelta is the last time Framework_Update was called in milliseconds (usually.)
            var fps = 1000.0 / framework.UpdateDelta.Milliseconds;
            
            // We want to fix the physics to 30 FPS
            var targetFps = 1000.0 / 30.0;
            
            // We want to tell whether or not we stepped 
            
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(45);
            stringBuilder.AddText($"[HighFPSPhysics] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddText("counter: " + _counter);
            
            _counter++;

            _executePhysics = _counter % 2 == 0;
            
            //Chat.Print(stringBuilder.BuiltString);
        }

        public void Dispose()
        {
            _physicsSkipHook?.Dispose();
            _framework.Update -= Framework_Update;
        }
    }
}
