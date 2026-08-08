using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;

namespace Rebound;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin()
    {
        Hooking.InitializeFromAttributes(this);

        unsafe
        {
            *BonePhysicsModule.GetForceOverrideSimulationTime() = true;
        }
    }

    [PluginService]
    internal static IGameInteropProvider Hooking { get; private set; } = null!;

    public void Dispose()
    {
        unsafe
        {
            *BonePhysicsModule.GetForceOverrideSimulationTime() = false;
        }
    }
}
