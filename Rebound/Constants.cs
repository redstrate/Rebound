using System;

namespace Rebound;

public static class Constants
{
    /// Client::Graphics::Physics::BoneSimulator::Update
    /// This is called for each BoneSimulator, such as hair, ears, etc
    public const String BoneSimulatorUpdateSignature = "40 55 53 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 44 0F 29 94 24";

    /// The target FPS the physics should be run at
    public const double TargetFps = 60.0;
}
