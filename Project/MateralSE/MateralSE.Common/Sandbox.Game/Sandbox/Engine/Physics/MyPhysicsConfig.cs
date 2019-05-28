namespace Sandbox.Engine.Physics
{
    using System;

    public static class MyPhysicsConfig
    {
        public const float CollisionEpsilon = 0.2f;
        public const float Epsilon = 1E-06f;
        public const float TriangleEpsilon = 0.02f;
        public const float AllowedPenetration = 0.01f;
        public const float MaxVelMag = 0.5f;
        public const float AABBExtension = 3f;
        public const float AabbMultiplier = 1.3f;
        public const float DefaultEnergySleepThreshold = 0.02f;
        public const float DefaultMaxLinearVelocity = 1000f;
        public const float DefaultMaxAngularVelocity = 20f;
        public const int DefaultIterationCount = 20;
        public const int MaxContactPoints = 3;
        public const int MaxCollidingElements = 0x100;
        public static float WheelSoftnessRatio = 1f;
        public static float WheelSoftnessVelocity = 0.01666667f;
        public static float MaxPistonHeadDisplacement = 0.2f;
        public static bool EnableGridSpeedDebugDraw = false;
        public static bool EnablePistonImpulseChecking = true;
        public static bool EnablePistonImpulseDebugDraw = false;
        public static float MaxPistonConstraintForceAxis = 15000f;
        public static float MaxPistonConstraintForceNonAxis = 30000f;
        public static int WheelSlipCountdown = 5;
        public static float WheelImpulseBlending = 0.3f;
        public static float WheelSlipCutAwayRatio = 0.7f;
        public static float WheelSurfaceMaterialSteerRatio = 0.5f;
        public static float WheelAxleFriction = 500f;
        public static bool OverrideWheelAxleFriction = false;
        public static float ArtificialBrakingMultiplier = 0.5f;
        public static float ArtificialBrakingCoMStabilization = 0.5f;
    }
}

