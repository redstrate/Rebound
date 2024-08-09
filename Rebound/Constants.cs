using System;

namespace Rebound;

public static class Constants
{
    /// Client::Graphics::Physics::BoneSimulator::Update
    /// This is called for each BoneSimulator, such as hair, ears, etc
    public const String BoneSimulatorUpdateSignature = "48 8B C4 48 89 48 08 55 53 56 41 57";

    /// The return call in Client::Graphics::Physics::BoneSimulator::Update
    public const String BoneSimulatorReturnSignature =
        "C3 CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53 55 57 41 54 41 56 48 83 EC 40 4C 89 AC 24 80 00 00 00";

    /// The target FPS the physics should be run at
    public const double TargetFps = 60.0;
}
