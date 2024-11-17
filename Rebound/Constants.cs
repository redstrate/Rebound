using System;

namespace Rebound;

public static class Constants
{
    /// Client::Graphics::Physics::BoneSimulator::Update
    /// This is called for each BoneSimulator, such as hair, ears, etc
    public const String BoneSimulatorUpdateSignature = "40 55 53 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 44 0F 29 94 24";

    /// The return call in Client::Graphics::Physics::BoneSimulator::Update
    public const String BoneSimulatorReturnSignature = "48 8B 8D ?? ?? ?? ?? 48 33 CC E8 ?? ?? ?? ?? 4C 8D 9C 24 ?? ?? ?? ?? 45 0F 28 53";

    /// The target FPS the physics should be run at
    public const double TargetFps = 60.0;
}
