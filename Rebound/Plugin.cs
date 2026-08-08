using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics.Physics;

namespace Rebound;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin()
    {
        unsafe
        {
            *BonePhysicsModule.GetForceOverrideSimulationTime() = true;
        }
    }

    public void Dispose()
    {
        unsafe
        {
            *BonePhysicsModule.GetForceOverrideSimulationTime() = false;
        }
    }
}
