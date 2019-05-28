namespace Sandbox.Game.Gui
{
    using System;
    using VRageMath;

    public static class MyHudConstants
    {
        public const int MAX_HUD_TEXTS_COUNT = 0x7d0;
        public static readonly Color HUD_COLOR = new Color(180, 180, 180, 180);
        public static readonly Color HUD_COLOR_LIGHT = new Color(0xff, 0xff, 0xff, 0xff);
        public const float HUD_DIRECTION_INDICATOR_SIZE = 0.006667f;
        public const float HUD_TEXTS_OFFSET = 0.0125f;
        public const float HUD_UNPOWERED_OFFSET = 0.01f;
        public const float DIRECTION_INDICATOR_MAX_SCREEN_DISTANCE = 0.425f;
        public const float DIRECTION_INDICATOR_MAX_SCREEN_TARGETING_DISTANCE = 0.25f;
        public static readonly Vector2 DIRECTION_INDICATOR_SCREEN_CENTER = new Vector2(0.5f, 0.5f);
        public static readonly Color MARKER_COLOR_BLUE = Color.CornflowerBlue;
        public static readonly Color MARKER_COLOR_RED = new Color(1f, 0f, 0f, 1f);
        public static readonly Color MARKER_COLOR_GRAY = new Color(0.8f, 0.8f, 0.8f, 1f);
        public static readonly Color MARKER_COLOR_WHITE = new Color(1f, 1f, 1f, 1f);
        public static readonly Color MARKER_COLOR_GREEN = new Color(0f, 0.7f, 0f, 1f);
        public static readonly Color GPS_COLOR = HUD_COLOR;
        public static readonly Vector4 FRIEND_CUBE_COLOR = (new Vector4(0f, 0.7f, 0f, 1f) * 0.2f);
        public static readonly Vector4 ENEMY_CUBE_COLOR = (new Vector4(0.8f, 0.3f, 0.3f, 1f) * 0.2f);
        public static readonly Vector4 NEUTRAL_CUBE_COLOR = (new Vector4(0.8f, 0.8f, 0.8f, 1f) * 0.2f);
        public const float PLAYER_MARKER_MULTIPLIER = 0.3f;
        public const float RADAR_BOUNDING_BOX_SIZE = 3000f;
        public const float RADAR_BOUNDING_BOX_SIZE_HALF = 1500f;
        public const float DIRECTION_TO_SUN_LINE_LENGTH = 100f;
        public const float DIRECTION_TO_SUN_LINE_LENGTH_HALF = 142.8571f;
        public const float DIRECTION_TO_SUN_LINE_THICKNESS = 0.7f;
        public const float NAVIGATION_MESH_LINE_THICKNESS = 3f;
        public const float NAVIGATION_MESH_DISTANCE = 100f;
        public const int NAVIGATION_MESH_LINES_COUNT_HALF = 10;
        public static readonly Color HUD_RADAR_BACKGROUND_COLOR = new Color(HUD_COLOR.ToVector4().X * 0.1f, HUD_COLOR.ToVector4().Y * 0.1f, HUD_COLOR.ToVector4().Z * 0.1f, 0.9f);
        public static readonly Color HUD_RADAR_BACKGROUND_COLOR2D = Color.White;
        public static readonly Vector3 HUD_RADAR_PHYS_OBJECT_POINT_DELTA = new Vector3(0f, 0f, 1f);
        public const int MIN_RADAR_TYPE_SWITCH_TIME_MILLISECONDS = 500;
        public static Color HUD_STATUS_BACKGROUND_COLOR = new Color(0.482f, 0.635f, 0.643f, 0.35f);
        public static Color HUD_STATUS_DEFAULT_COLOR = new Color(1f, 1f, 1f, 1f);
        public static Color HUD_STATUS_BAR_COLOR_GREEN_STATUS = new Color(0x7c, 0xae, 0x7d, 0xcd);
        public static Color HUD_STATUS_BAR_COLOR_YELLOW_STATUS = new Color(0xda, 0xd5, 0x7d, 0xcd);
        public static Color HUD_STATUS_BAR_COLOR_ORANGE_STATUS = new Color(0xec, 0xa3, 0x61, 0xcd);
        public static Color HUD_STATUS_BAR_COLOR_RED_STATUS = new Color(0xbb, 0, 0, 0xcd);
        public const float HUD_STATUS_BAR_COLOR_GRADIENT_OFFSET = 2f;
        public static Vector2 HUD_STATUS_POSITION = new Vector2(0.02f, -0.02f);
        public static Vector2 HUD_MISSIONS_POSITION = new Vector2(-0.02f, 0.02f);
        public static Vector2 HUD_STATUS_ICON_SIZE = (new Vector2(0.012f, 0.01f) * 0.85f);
        public static Vector2 HUD_STATUS_SPEED_ICON_SIZE = new Vector2(0.0145f, 0.0145f);
        public static Vector2 HUD_STATUS_BAR_SIZE = new Vector2(0.0095f, 0.006f);
        public const float HUD_STATUS_ICON_DISTANCE = 0.0075f;
        public const float HUD_STATUS_BAR_DISTANCE = 0.001f;
        public const int HUD_STATUS_BAR_MAX_PIECES_COUNT = 5;
        public const int PREFAB_PREVIEW_SIZE = 0x80;
        public static readonly Vector2 LOW_FUEL_WARNING_POSITION = new Vector2(0.5f, 0.65f);
        public const float HUD_MAX_DISTANCE_ENEMIES = 2500f;
        public const float HUD_MAX_DISTANCE_NORMAL = 800f;
        public const float HUD_MAX_DISTANCE_ALPHA = 0.2f;
        public const float HUD_MIN_DISTANCE_ALPHA = 1f;
        public const float HUD_MAX_DISTANCE_TO_ALPHA_CORRECT_NORMAL = 800f;
        public const float HUD_MAX_DISTANCE_TO_ALPHA_CORRECT_ENEMIES = 2500f;
        public const float HUD_MIN_DISTANCE_TO_ALPHA_CORRECT = 50f;
    }
}

