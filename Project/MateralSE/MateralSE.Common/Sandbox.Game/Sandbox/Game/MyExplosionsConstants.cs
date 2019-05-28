namespace Sandbox.Game
{
    using System;
    using VRageMath;

    public static class MyExplosionsConstants
    {
        public const float EXPLOSION_RADIUS_MAX = 100f;
        public const int MAX_EXPLOSIONS_COUNT = 0x400;
        public const int MIN_OBJECT_SIZE_TO_CAUSE_EXPLOSION_AND_CREATE_DEBRIS = 5;
        public const float OFFSET_LINE_FOR_DIRT_DECAL = 0.5f;
        public const float EXPLOSION_STRENGTH_IMPULSE = 100f;
        public const float EXPLOSION_STRENGTH_ANGULAR_IMPULSE = 50000f;
        public const float EXPLOSION_STRENGTH_ANGULAR_IMPULSE_PLAYER_MULTIPLICATOR = 0.25f;
        public const float EXPLOSION_RADIUS_MULTIPLIER = 1.25f;
        public const float EXPLOSION_RADIUS_MULTIPLIER_FOR_IMPULSE = 2f;
        public const float EXPLOSION_RADIUS_MULTPLIER_FOR_DIRT_GLASS_DECALS = 3f;
        public const float EXPLOSION_RANDOM_RADIUS_MAX = 25f;
        public const float EXPLOSION_RANDOM_RADIUS_MIN = 20f;
        public const int EXPLOSION_LIFESPAN = 700;
        public static readonly Vector4 EXPLOSION_LIGHT_COLOR = new Vector4(0.972549f, 0.7019608f, 0.04705882f, 1f);
        public const float CLOSE_EXPLOSION_DISTANCE = 15f;
        public const int FRAMES_PER_SPARK = 30;
        public const float DAMAGE_SPARKS = 0.4f;
        public const float EXPLOSION_FORCE_RADIUS_MULTIPLIER = 0.33f;
        public const int EXPLOSION_EFFECT_SIZE_FOR_HUGE_EXPLOSION = 300;
        public static float CAMERA_SHAKE_TIME_MS = 300f;
    }
}

