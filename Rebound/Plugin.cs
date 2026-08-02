using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;

namespace Rebound;

public sealed class Plugin : IDalamudPlugin
{
    // TODO: Replace with BonePhysicsModule.GetForceOverrideSimulationTime() when that's available

    /// When true, checks if we're about ~10 FPS over 60 and enables UseOverrideSimulationTime on BonePhysicsModules.
    /// If you need to find the signature for this again, start in Client::Graphics::Scene::CharacterBase.UpdateRender.
    [Signature("38 1D ?? ?? ?? ?? F3 0F 10 3D", ScanType = ScanType.StaticAddress)]
    public unsafe bool* _useOverrideSimulationTime = null;

    public Plugin()
    {
        Hooking.InitializeFromAttributes(this);

        unsafe
        {
            *_useOverrideSimulationTime = true;
        }
    }

    [PluginService]
    internal static IGameInteropProvider Hooking { get; private set; } = null!;

    public void Dispose()
    {
        unsafe
        {
            *_useOverrideSimulationTime = false;
        }
    }
}
