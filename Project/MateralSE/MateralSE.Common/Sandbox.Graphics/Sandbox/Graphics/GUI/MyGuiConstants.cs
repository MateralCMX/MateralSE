namespace Sandbox.Graphics.GUI
{
    using System;
    using VRageMath;

    public static class MyGuiConstants
    {
        public static readonly Vector2 GUI_OPTIMAL_SIZE = new Vector2(1600f, 1200f);
        public const float DOUBLE_CLICK_DELAY = 500f;
        public const float CLICK_RELEASE_DELAY = 500f;
        public const float DEFAULT_TEXT_SCALE = 0.8f;
        public const float HUD_TEXT_SCALE = 0.8f;
        public const float HUD_LINE_SPACING = 0.025f;
        public static readonly Vector4 LABEL_TEXT_COLOR = Vector4.One;
        public static readonly Vector2 DEFAULT_LISTBOX_ITEM_SIZE = ((new Vector2(648f, 390f) - new Vector2(228f, 348f)) / GUI_OPTIMAL_SIZE);
        public static Vector4 DISABLED_CONTROL_COLOR_MASK_MULTIPLIER = new Vector4(0.7f);
        public static Color THEMED_GUI_LINE_COLOR = new Color(0x4d, 0x63, 0x71);
        public static Color THEMED_GUI_LINE_BORDER = new Color(0);
        public static Color THEMED_GUI_BACKGROUND_COLOR = new Color(0x49, 0x56, 0x5e);
        public static readonly Color GUI_NEWS_BACKGROUND_COLOR = new Color(0x23, 0x42, 0x55);
        public static readonly MyGuiSizedTexture TEXTURE_ICON_FAKE;
        public static readonly string TEXTURE_ICON_FILTER_URANIUM;
        public static readonly string TEXTURE_ICON_FILTER_ORE;
        public static readonly string TEXTURE_ICON_FILTER_INGOT;
        public static readonly string TEXTURE_ICON_FILTER_MISSILE;
        public static readonly string TEXTURE_ICON_FILTER_AMMO_25MM;
        public static readonly string TEXTURE_ICON_FILTER_AMMO_5_54MM;
        public static readonly string TEXTURE_ICON_FILTER_COMPONENT;
        public static readonly string TEXTURE_ICON_LARGE_BLOCK;
        public static readonly string TEXTURE_ICON_SMALL_BLOCK;
        public static readonly string TEXTURE_ICON_CLOSE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_DEFAULT_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_DEFAULT_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_RED_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_RED_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SKINS_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SKINS_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_CLOSE_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_CLOSE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_CLOSE_BCG_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_CLOSE_BCG_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_INFO_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_INFO_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_KEEN_LOGO;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_STRIPE_LEFT_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_STRIPE_LEFT_NORMAL_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_WELCOMESCREEN_SIGNATURE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_CHARACTER;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_CHARACTER_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_GRID;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_GRID_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_ALL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_ALL_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_ENERGY;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_ENERGY_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_STORAGE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_STORAGE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_SHIP;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_SHIP_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_SYSTEM;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_FILTER_SYSTEM_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_NULL;
        public static readonly MyGuiCompositeTexture TEXTURE_HIGHLIGHT_DARK;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_INCREASE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_DECREASE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_INCREASE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_DECREASE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ARROW_LEFT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ARROW_LEFT_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ARROW_RIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ARROW_RIGHT_HIGHLIGHT;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ARROW_SINGLE;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ARROW_DOUBLE;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_LIKE_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_LIKE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_BUG_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_BUG_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_HELP_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_HELP_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ENVELOPE_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_ENVELOPE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_SWITCHONOFF_LEFT_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SWITCHONOFF_LEFT_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_SWITCHONOFF_RIGHT_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SWITCHONOFF_RIGHT_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_INVENTORY_TRASH_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_INVENTORY_TRASH_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_INVENTORY_SWITCH_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_INVENTORY_SWITCH_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_CRAFTING_SWITCH_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_CRAFTING_SWITCH_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_TEXTBOX;
        public static readonly MyGuiCompositeTexture TEXTURE_TEXTBOX_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLABLE_LIST_TOOLS_BLOCKS;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLABLE_LIST;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLABLE_LIST_BORDER;
        public static readonly MyGuiCompositeTexture TEXTURE_WBORDER_LIST;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLABLE_WBORDER_LIST;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_DARK;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_BUTTON_BORDER;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_BUTTON_HIGHLIGHTED_BORDER;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_DARK_BORDER;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_LOAD_BORDER;
        public static readonly MyGuiCompositeTexture TEXTURE_NEWS_BACKGROUND;
        public static readonly MyGuiCompositeTexture TEXTURE_NEWS_BACKGROUND_BlueLine;
        public static readonly MyGuiCompositeTexture TEXTURE_NEWS_PAGING_BACKGROUND;
        public static readonly MyGuiCompositeTexture TEXTURE_RECTANGLE_NEUTRAL;
        public static readonly MyGuiCompositeTexture TEXTURE_COMBOBOX_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_COMBOBOX_HIGHLIGHT;
        public static readonly MyGuiHighlightTexture TEXTURE_GRID_ITEM;
        public static readonly MyGuiHighlightTexture TEXTURE_GRID_ITEM_SMALL;
        public static readonly MyGuiHighlightTexture TEXTURE_GRID_ITEM_TINY;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_LARGE_BLOCK;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_SMALL_BLOCK;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_TOOL;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_COMPONENT;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_DISASSEMBLY;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_REPEAT;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_REPEAT_INACTIVE;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_SLAVE;
        public static readonly MyGuiHighlightTexture TEXTURE_BUTTON_ICON_SLAVE_INACTIVE;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_WHITE_FLAG;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_SENT_WHITE_FLAG;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_SENT_JOIN_REQUEST;
        public static readonly MyGuiPaddedTexture TEXTURE_MESSAGEBOX_BACKGROUND_ERROR;
        public static readonly MyGuiPaddedTexture TEXTURE_MESSAGEBOX_BACKGROUND_INFO;
        public static readonly MyGuiPaddedTexture TEXTURE_QUESTLOG_BACKGROUND_INFO;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_BACKGROUND;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_BACKGROUND_RED;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_TOOLS_BACKGROUND_BLOCKS;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_TOOLS_BACKGROUND_CONTROLS;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_TOOLS_BACKGROUND_WEAPONS;
        public static readonly MyGuiPaddedTexture TEXTURE_SCREEN_STATS_BACKGROUND;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_MODS_LOCAL;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_BLUEPRINTS_LOCAL;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_BLUEPRINTS_CLOUD;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_STAR;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_LOCK;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_EXPERIMENTAL;
        public static readonly MyGuiHighlightTexture TEXTURE_BLUEPRINTS_ARROW;
        public static readonly MyGuiHighlightTexture TEXTURE_ICON_MODS_WORKSHOP;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_NORMAL_INDETERMINATE;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_INDETERMINATE;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_GREEN_CHECKED;
        public static readonly MyGuiCompositeTexture TEXTURE_CHECKBOX_BLANK;
        public static MyGuiHighlightTexture TEXTURE_SLIDER_THUMB_DEFAULT;
        public static MyGuiHighlightTexture TEXTURE_HUE_SLIDER_THUMB_DEFAULT;
        public static readonly MyGuiPaddedTexture TEXTURE_HUD_BG_MEDIUM_DEFAULT;
        public static readonly MyGuiPaddedTexture TEXTURE_HUD_BG_LARGE_DEFAULT;
        public static readonly MyGuiPaddedTexture TEXTURE_HUD_BG_MEDIUM_RED;
        public static readonly MyGuiPaddedTexture TEXTURE_HUD_BG_MEDIUM_RED2;
        public static readonly MyGuiPaddedTexture TEXTURE_HUD_BG_PERFORMANCE;
        public static readonly MyGuiPaddedTexture TEXTURE_VOICE_CHAT;
        public static readonly MyGuiPaddedTexture TEXTURE_DISCONNECTED_PLAYER;
        public static readonly MyGuiCompositeTexture TEXTURE_SLIDER_RAIL;
        public static readonly MyGuiCompositeTexture TEXTURE_SLIDER_RAIL_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_HUE_SLIDER_RAIL;
        public static readonly MyGuiCompositeTexture TEXTURE_HUE_SLIDER_RAIL_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_V_THUMB;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_V_THUMB_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_V_BACKGROUND;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_H_THUMB;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_H_THUMB_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_SCROLLBAR_H_BACKGROUND;
        public static readonly MyGuiCompositeTexture TEXTURE_TOOLBAR_TAB;
        public static readonly MyGuiCompositeTexture TEXTURE_TOOLBAR_TAB_HIGHLIGHT;
        public const string TEXTURE_BACKGROUND_FADE = @"Textures\Gui\Screens\screen_background_fade.dds";
        public const string BUTTON_LOCKED = @"Textures\GUI\LockedButton.dds";
        public const string BLANK_TEXTURE = @"Textures\GUI\Blank.dds";
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_ROUND_ALL;
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_ROUND_ALL_SMALL;
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_ROUND_TOP;
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_SLOPE_LEFTBOTTOM;
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_SLOPE_LEFTBOTTOM_30;
        public static readonly MyGuiCompositeTexture TEXTURE_COMPOSITE_BLOCKINFO_PROGRESSBAR;
        public static MyGuiPaddedTexture TEXTURE_HUD_GRAVITY_GLOBE;
        public static MyGuiPaddedTexture TEXTURE_HUD_GRAVITY_LINE;
        public static MyGuiPaddedTexture TEXTURE_HUD_GRAVITY_HORIZON;
        public static readonly MyGuiCompositeTexture TEXTURE_GUI_BLANK;
        public static MyGuiPaddedTexture TEXTURE_HUD_STATS_BG;
        public static MyGuiPaddedTexture TEXTURE_HUD_STAT_EFFECT_ARROW_UP;
        public static MyGuiPaddedTexture TEXTURE_HUD_STAT_EFFECT_ARROW_DOWN;
        public static MyGuiPaddedTexture TEXTURE_HUD_STAT_BAR_BG;
        public static readonly MyGuiCompositeTexture TEXTURE_HUD_GRID_LARGE;
        public static readonly MyGuiCompositeTexture TEXTURE_HUD_GRID_LARGE_FIT;
        public static readonly MyGuiCompositeTexture TEXTURE_HUD_GRID_SMALL;
        public static readonly MyGuiCompositeTexture TEXTURE_HUD_GRID_SMALL_FIT;
        public const string CURSOR_ARROW = @"Textures\GUI\MouseCursor.dds";
        public const string CURSOR_HAND = @"Textures\GUI\MouseCursorHand.dds";
        public const string PROGRESS_BAR = @"Textures\GUI\ProgressBar.dds";
        public const string LOADING_TEXTURE = @"Textures\GUI\screens\screen_loading_wheel.dds";
        public const string LOADING_TEXTURE_LOADING_SCREEN = @"Textures\GUI\screens\screen_loading_wheel_loading_screen.dds";
        public const float MOUSE_CURSOR_SPEED_MULTIPLIER = 1.3f;
        public const int VIDEO_OPTIONS_CONFIRMATION_TIMEOUT_IN_MILISECONDS = 0xea60;
        public static readonly Vector2 SHADOW_OFFSET;
        public static readonly Vector4 CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER;
        public static readonly Vector2 CONTROLS_DELTA;
        public static readonly Vector4 ROTATING_WHEEL_COLOR;
        public const float ROTATING_WHEEL_DEFAULT_SCALE = 0.36f;
        public static readonly int SHOW_CONTROL_TOOLTIP_DELAY;
        public static readonly float TOOLTIP_DISTANCE_FROM_BORDER;
        public static readonly Vector4 DEFAULT_CONTROL_BACKGROUND_COLOR;
        public static readonly Vector4 DEFAULT_CONTROL_NONACTIVE_COLOR;
        public static Color DISABLED_BUTTON_COLOR;
        public static Vector4 DISABLED_BUTTON_COLOR_VECTOR;
        public static Vector4 DISABLED_BUTTON_TEXT_COLOR;
        public static float LOCKBUTTON_SIZE_MODIFICATION;
        public const float APP_VERSION_TEXT_SCALE = 0.95f;
        public const float APP_VERSION_TEXT_SCALE_MainMenu = 0.6f;
        public const float APP_VERSION_TEXT_ALPHA_MainMenu = 0.6f;
        public static readonly Vector4 SCREEN_BACKGROUND_FADE_BLANK_DARK;
        public static readonly Vector4 SCREEN_BACKGROUND_FADE_BLANK_DARK_PROGRESS_SCREEN;
        public static readonly float SCREEN_CAPTION_DELTA_Y;
        public static readonly Vector4 SCREEN_BACKGROUND_COLOR;
        public const float REFERENCE_SCREEN_HEIGHT = 1080f;
        public const float SAFE_ASPECT_RATIO = 1.333333f;
        public const float LOADING_PLEASE_WAIT_SCALE = 1.1f;
        public static readonly Vector2 LOADING_PLEASE_WAIT_POSITION;
        public static readonly Vector4 LOADING_PLEASE_WAIT_COLOR;
        public const int TEXTBOX_MOVEMENT_DELAY = 100;
        public const int TEXTBOX_CHANGE_DELAY = 500;
        public const int TEXTBOX_INITIAL_THROTTLE_DELAY = 500;
        public const int TEXTBOX_REPEAT_THROTTLE_DELAY = 50;
        public const string TEXTBOX_FALLBACK_CHARACTER = "#";
        public static readonly Vector2 TEXTBOX_TEXT_OFFSET;
        public static readonly Vector2 TEXTBOX_MEDIUM_SIZE;
        public static readonly Vector4 MOUSE_CURSOR_COLOR;
        public const float MOUSE_CURSOR_SCALE = 1f;
        public const float MOUSE_ROTATION_INDICATOR_MULTIPLIER = 0.075f;
        public const float ROTATION_INDICATOR_MULTIPLIER = 0.15f;
        public static readonly Vector4 BUTTON_BACKGROUND_COLOR;
        public static readonly Vector2 MENU_BUTTONS_POSITION_DELTA;
        public static readonly Vector4 BACK_BUTTON_BACKGROUND_COLOR;
        public static readonly Vector4 BACK_BUTTON_TEXT_COLOR;
        public static readonly Vector2 BACK_BUTTON_SIZE;
        public static readonly Vector2 OK_BUTTON_SIZE;
        public static readonly Vector2 GENERIC_BUTTON_SPACING;
        public const float MAIN_MENU_BUTTON_TEXT_SCALE = 0.8f;
        public static Vector4 TREEVIEW_SELECTED_ITEM_COLOR;
        public static Vector4 TREEVIEW_DISABLED_ITEM_COLOR;
        public static readonly Vector4 TREEVIEW_TEXT_COLOR;
        public static readonly Vector4 TREEVIEW_VERTICAL_LINE_COLOR;
        public static readonly Vector2 TREEVIEW_VSCROLLBAR_SIZE;
        public static readonly Vector2 TREEVIEW_HSCROLLBAR_SIZE;
        public static readonly Vector2 COMBOBOX_MEDIUM_SIZE;
        public static readonly Vector2 COMBOBOX_MEDIUM_ELEMENT_SIZE;
        public static readonly Vector2 COMBOBOX_VSCROLLBAR_SIZE;
        public static readonly Vector2 COMBOBOX_HSCROLLBAR_SIZE;
        public static readonly Vector4 LISTBOX_BACKGROUND_COLOR;
        public static readonly Vector2 LISTBOX_ICON_SIZE;
        public static readonly Vector2 LISTBOX_ICON_OFFSET;
        public static readonly float LISTBOX_WIDTH;
        public static readonly Vector2 DRAG_AND_DROP_TEXT_OFFSET;
        public static readonly Vector4 DRAG_AND_DROP_TEXT_COLOR;
        public static readonly Vector2 DRAG_AND_DROP_SMALL_SIZE;
        public static readonly Vector4 DRAG_AND_DROP_BACKGROUND_COLOR;
        public const float DRAG_AND_DROP_ICON_SIZE_X = 0.07395f;
        public const float DRAG_AND_DROP_ICON_SIZE_Y = 0.0986f;
        public static readonly float SLIDER_INSIDE_OFFSET_X;
        public static readonly int REPEAT_PRESS_DELAY;
        public static readonly Vector2 MESSAGE_BOX_BUTTON_SIZE_SMALL;
        public static Vector2 TOOL_TIP_RELATIVE_DEFAULT_POSITION;
        public const float TOOL_TIP_TEXT_SCALE = 0.7f;
        public const int TRANSITION_OPENING_TIME = 200;
        public const int TRANSITION_CLOSING_TIME = 200;
        public const float TRANSITION_ALPHA_MIN = 0f;
        public const float TRANSITION_ALPHA_MAX = 1f;
        public static readonly Vector2I LOADING_BACKGROUND_TEXTURE_REAL_SIZE;
        public const int LOADING_THREAD_DRAW_SLEEP_IN_MILISECONDS = 10;
        public const float COLORED_TEXT_DEFAULT_TEXT_SCALE = 0.75f;
        public static readonly Color COLORED_TEXT_DEFAULT_COLOR;
        public static readonly Color COLORED_TEXT_DEFAULT_HIGHLIGHT_COLOR;
        public static readonly Vector2 MULTILINE_LABEL_BORDER;
        public static readonly float DEBUG_LABEL_TEXT_SCALE;
        public static readonly float DEBUG_BUTTON_TEXT_SCALE;
        public static readonly float DEBUG_STATISTICS_TEXT_SCALE;
        public static readonly float DEBUG_STATISTICS_ROW_DISTANCE;
        public const float FONT_SCALE = 0.7783784f;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_SMALL_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_SMALL_NORMAL;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_48_HIGHLIGHT;
        public static readonly MyGuiCompositeTexture TEXTURE_BUTTON_SQUARE_48_NORMAL;
        public const string CB_FREE_MODE_ICON = @"Textures\GUI\CubeBuilder\FreeModIcon.png";
        public const string CB_LCS_GRID_ICON = @"Textures\GUI\CubeBuilder\OnGridIcon.png";
        public const string CB_LARGE_GRID_MODE = @"Textures\GUI\CubeBuilder\GridModeLargeHighl.png";
        public const string CB_SMALL_GRID_MODE = @"Textures\GUI\CubeBuilder\GridModeSmallHighl.png";
        public const string BS_ANTENNA_ON = @"Textures\GUI\Icons\BroadcastStatus\AntennaOn.png";
        public const string BS_ANTENNA_OFF = @"Textures\GUI\Icons\BroadcastStatus\AntennaOff.png";
        public const string BS_KEY_ON = @"Textures\GUI\Icons\BroadcastStatus\KeyOn.png";
        public const string BS_KEY_OFF = @"Textures\GUI\Icons\BroadcastStatus\KeyOff.png";
        public const string BS_REMOTE_ON = @"Textures\GUI\Icons\BroadcastStatus\RemoteOn.png";
        public const string BS_REMOTE_OFF = @"Textures\GUI\Icons\BroadcastStatus\RemoteOff.png";

        static MyGuiConstants()
        {
            MyGuiSizedTexture texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Icons\Fake.dds",
                SizePx = new Vector2(81f, 81f)
            };
            TEXTURE_ICON_FAKE = texture;
            TEXTURE_ICON_FILTER_URANIUM = @"Textures\GUI\Icons\filter_uranium.dds";
            TEXTURE_ICON_FILTER_ORE = @"Textures\GUI\Icons\filter_ore.dds";
            TEXTURE_ICON_FILTER_INGOT = @"Textures\GUI\Icons\filter_ingot.dds";
            TEXTURE_ICON_FILTER_MISSILE = @"Textures\GUI\Icons\FilterMissile.dds";
            TEXTURE_ICON_FILTER_AMMO_25MM = @"Textures\GUI\Icons\FilterAmmo25mm.dds";
            TEXTURE_ICON_FILTER_AMMO_5_54MM = @"Textures\GUI\Icons\FilterAmmo5.54mm.dds";
            TEXTURE_ICON_FILTER_COMPONENT = @"Textures\GUI\Icons\FilterComponent.dds";
            TEXTURE_ICON_LARGE_BLOCK = @"Textures\GUI\CubeBuilder\GridModeLargeHighl.PNG";
            TEXTURE_ICON_SMALL_BLOCK = @"Textures\GUI\CubeBuilder\GridModeSmallHighl.PNG";
            TEXTURE_ICON_CLOSE = @"Textures\GUI\Controls\button_close_symbol.dds";
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(281f, 62f),
                Texture = @"Textures\GUI\Controls\button_default.dds"
            };
            MyGuiCompositeTexture texture1 = new MyGuiCompositeTexture(null);
            texture1.LeftTop = texture;
            TEXTURE_BUTTON_DEFAULT_NORMAL = texture1;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(281f, 62f),
                Texture = @"Textures\GUI\Controls\button_default_highlight.dds"
            };
            MyGuiCompositeTexture texture4 = new MyGuiCompositeTexture(null);
            texture4.LeftTop = texture;
            TEXTURE_BUTTON_DEFAULT_HIGHLIGHT = texture4;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(281f, 62f),
                Texture = @"Textures\GUI\Controls\button_red.dds"
            };
            MyGuiCompositeTexture texture5 = new MyGuiCompositeTexture(null);
            texture5.LeftTop = texture;
            TEXTURE_BUTTON_RED_NORMAL = texture5;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(281f, 62f),
                Texture = @"Textures\GUI\Controls\button_red_highlight.dds"
            };
            MyGuiCompositeTexture texture6 = new MyGuiCompositeTexture(null);
            texture6.LeftTop = texture;
            TEXTURE_BUTTON_RED_HIGHLIGHT = texture6;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(211f, 51f),
                Texture = @"Textures\GUI\Controls\button_skins_default.dds"
            };
            MyGuiCompositeTexture texture7 = new MyGuiCompositeTexture(null);
            texture7.LeftTop = texture;
            TEXTURE_BUTTON_SKINS_NORMAL = texture7;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(211f, 51f),
                Texture = @"Textures\GUI\Controls\button_skins_default_highlight.dds"
            };
            MyGuiCompositeTexture texture8 = new MyGuiCompositeTexture(null);
            texture8.LeftTop = texture;
            TEXTURE_BUTTON_SKINS_HIGHLIGHT = texture8;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_close_symbol.dds"
            };
            MyGuiCompositeTexture texture9 = new MyGuiCompositeTexture(null);
            texture9.LeftTop = texture;
            TEXTURE_BUTTON_CLOSE_NORMAL = texture9;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_close_symbol_highlight.dds"
            };
            MyGuiCompositeTexture texture10 = new MyGuiCompositeTexture(null);
            texture10.LeftTop = texture;
            TEXTURE_BUTTON_CLOSE_HIGHLIGHT = texture10;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_close_symbol_bcg.dds"
            };
            MyGuiCompositeTexture texture11 = new MyGuiCompositeTexture(null);
            texture11.LeftTop = texture;
            TEXTURE_BUTTON_CLOSE_BCG_NORMAL = texture11;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_close_symbol_bcg_highlight.dds"
            };
            MyGuiCompositeTexture texture12 = new MyGuiCompositeTexture(null);
            texture12.LeftTop = texture;
            TEXTURE_BUTTON_CLOSE_BCG_HIGHLIGHT = texture12;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_info_symbol.dds"
            };
            MyGuiCompositeTexture texture13 = new MyGuiCompositeTexture(null);
            texture13.LeftTop = texture;
            TEXTURE_BUTTON_INFO_NORMAL = texture13;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(34f, 31f),
                Texture = @"Textures\GUI\Controls\button_info_symbol_highlight.dds"
            };
            MyGuiCompositeTexture texture14 = new MyGuiCompositeTexture(null);
            texture14.LeftTop = texture;
            TEXTURE_BUTTON_INFO_HIGHLIGHT = texture14;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(198f, 99f),
                Texture = @"Textures\Gui\KeenLogo.dds"
            };
            MyGuiCompositeTexture texture15 = new MyGuiCompositeTexture(null);
            texture15.LeftTop = texture;
            TEXTURE_KEEN_LOGO = texture15;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(286f, 62f),
                Texture = @"Textures\GUI\Controls\button_stripe_left.dds"
            };
            MyGuiCompositeTexture texture16 = new MyGuiCompositeTexture(null);
            texture16.LeftTop = texture;
            TEXTURE_BUTTON_STRIPE_LEFT_NORMAL = texture16;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(286f, 62f),
                Texture = @"Textures\GUI\Controls\button_stripe_left_highlight.dds"
            };
            MyGuiCompositeTexture texture17 = new MyGuiCompositeTexture(null);
            texture17.LeftTop = texture;
            TEXTURE_BUTTON_STRIPE_LEFT_NORMAL_HIGHLIGHT = texture17;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(255f, 52f),
                Texture = @"Textures\Gui\Signature.dds"
            };
            MyGuiCompositeTexture texture18 = new MyGuiCompositeTexture(null);
            texture18.LeftTop = texture;
            TEXTURE_WELCOMESCREEN_SIGNATURE = texture18;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_character.dds"
            };
            MyGuiCompositeTexture texture19 = new MyGuiCompositeTexture(null);
            texture19.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_CHARACTER = texture19;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_character_highlight.dds"
            };
            MyGuiCompositeTexture texture20 = new MyGuiCompositeTexture(null);
            texture20.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_CHARACTER_HIGHLIGHT = texture20;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_grid.dds"
            };
            MyGuiCompositeTexture texture21 = new MyGuiCompositeTexture(null);
            texture21.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_GRID = texture21;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_grid_highlight.dds"
            };
            MyGuiCompositeTexture texture22 = new MyGuiCompositeTexture(null);
            texture22.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_GRID_HIGHLIGHT = texture22;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_all.dds"
            };
            MyGuiCompositeTexture texture23 = new MyGuiCompositeTexture(null);
            texture23.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_ALL = texture23;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_all_highlight.dds"
            };
            MyGuiCompositeTexture texture24 = new MyGuiCompositeTexture(null);
            texture24.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_ALL_HIGHLIGHT = texture24;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_energy.dds"
            };
            MyGuiCompositeTexture texture25 = new MyGuiCompositeTexture(null);
            texture25.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_ENERGY = texture25;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_energy_highlight.dds"
            };
            MyGuiCompositeTexture texture26 = new MyGuiCompositeTexture(null);
            texture26.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_ENERGY_HIGHLIGHT = texture26;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_storage.dds"
            };
            MyGuiCompositeTexture texture27 = new MyGuiCompositeTexture(null);
            texture27.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_STORAGE = texture27;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_storage_highlight.dds"
            };
            MyGuiCompositeTexture texture28 = new MyGuiCompositeTexture(null);
            texture28.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_STORAGE_HIGHLIGHT = texture28;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_ship.png"
            };
            MyGuiCompositeTexture texture29 = new MyGuiCompositeTexture(null);
            texture29.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_SHIP = texture29;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_ship_highlight.png"
            };
            MyGuiCompositeTexture texture30 = new MyGuiCompositeTexture(null);
            texture30.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_SHIP_HIGHLIGHT = texture30;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_system.dds"
            };
            MyGuiCompositeTexture texture31 = new MyGuiCompositeTexture(null);
            texture31.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_SYSTEM = texture31;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(69f, 69f),
                Texture = @"Textures\GUI\Controls\button_filter_system_highlight.dds"
            };
            MyGuiCompositeTexture texture32 = new MyGuiCompositeTexture(null);
            texture32.LeftTop = texture;
            TEXTURE_BUTTON_FILTER_SYSTEM_HIGHLIGHT = texture32;
            TEXTURE_NULL = new MyGuiCompositeTexture(null);
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.Zero,
                Texture = @"Textures\GUI\Controls\item_highlight_dark.dds"
            };
            MyGuiCompositeTexture texture33 = new MyGuiCompositeTexture(null);
            texture33.Center = texture;
            TEXTURE_HIGHLIGHT_DARK = texture33;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(60f, 60f),
                Texture = @"Textures\GUI\Controls\button_increase.dds"
            };
            MyGuiCompositeTexture texture34 = new MyGuiCompositeTexture(null);
            texture34.LeftTop = texture;
            TEXTURE_BUTTON_INCREASE = texture34;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(60f, 60f),
                Texture = @"Textures\GUI\Controls\button_decrease.dds"
            };
            MyGuiCompositeTexture texture35 = new MyGuiCompositeTexture(null);
            texture35.LeftTop = texture;
            TEXTURE_BUTTON_DECREASE = texture35;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(60f, 60f),
                Texture = @"Textures\GUI\Controls\button_increase_highlight.dds"
            };
            MyGuiCompositeTexture texture36 = new MyGuiCompositeTexture(null);
            texture36.LeftTop = texture;
            TEXTURE_BUTTON_INCREASE_HIGHLIGHT = texture36;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(60f, 60f),
                Texture = @"Textures\GUI\Controls\button_decrease_highlight.dds"
            };
            MyGuiCompositeTexture texture37 = new MyGuiCompositeTexture(null);
            texture37.LeftTop = texture;
            TEXTURE_BUTTON_DECREASE_HIGHLIGHT = texture37;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(30f, 29f),
                Texture = @"Textures\GUI\Controls\button_arrow_left.dds"
            };
            MyGuiCompositeTexture texture38 = new MyGuiCompositeTexture(null);
            texture38.LeftTop = texture;
            TEXTURE_BUTTON_ARROW_LEFT = texture38;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(30f, 29f),
                Texture = @"Textures\GUI\Controls\button_arrow_left_highlight.dds"
            };
            MyGuiCompositeTexture texture39 = new MyGuiCompositeTexture(null);
            texture39.LeftTop = texture;
            TEXTURE_BUTTON_ARROW_LEFT_HIGHLIGHT = texture39;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(30f, 29f),
                Texture = @"Textures\GUI\Controls\button_arrow_right.dds"
            };
            MyGuiCompositeTexture texture40 = new MyGuiCompositeTexture(null);
            texture40.LeftTop = texture;
            TEXTURE_BUTTON_ARROW_RIGHT = texture40;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(30f, 29f),
                Texture = @"Textures\GUI\Controls\button_arrow_right_highlight.dds"
            };
            MyGuiCompositeTexture texture41 = new MyGuiCompositeTexture(null);
            texture41.LeftTop = texture;
            TEXTURE_BUTTON_ARROW_RIGHT_HIGHLIGHT = texture41;
            MyGuiHighlightTexture texture2 = new MyGuiHighlightTexture {
                SizePx = new Vector2(64f, 64f),
                Normal = @"Textures\GUI\Icons\buttons\ArrowSingle.dds",
                Highlight = @"Textures\GUI\Icons\buttons\ArrowSingleHighlight.dds"
            };
            TEXTURE_BUTTON_ARROW_SINGLE = texture2;
            texture2 = new MyGuiHighlightTexture {
                SizePx = new Vector2(64f, 64f),
                Normal = @"Textures\GUI\Icons\buttons\ArrowDouble.dds",
                Highlight = @"Textures\GUI\Icons\buttons\ArrowDoubleHighlight.dds"
            };
            TEXTURE_BUTTON_ARROW_DOUBLE = texture2;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\Like.dds"
            };
            MyGuiCompositeTexture texture42 = new MyGuiCompositeTexture(null);
            texture42.Center = texture;
            TEXTURE_BUTTON_LIKE_NORMAL = texture42;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(128f, 128f),
                Texture = @"Textures\GUI\Icons\LikeHighlight.dds"
            };
            MyGuiCompositeTexture texture43 = new MyGuiCompositeTexture(null);
            texture43.Center = texture;
            TEXTURE_BUTTON_LIKE_HIGHLIGHT = texture43;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(16f, 64f),
                Texture = @"Textures\GUI\Icons\Bug.dds"
            };
            MyGuiCompositeTexture texture44 = new MyGuiCompositeTexture(null);
            texture44.Center = texture;
            TEXTURE_BUTTON_BUG_NORMAL = texture44;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(128f, 128f),
                Texture = @"Textures\GUI\Icons\BugHighlight.dds"
            };
            MyGuiCompositeTexture texture45 = new MyGuiCompositeTexture(null);
            texture45.Center = texture;
            TEXTURE_BUTTON_BUG_HIGHLIGHT = texture45;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\Help.dds"
            };
            MyGuiCompositeTexture texture46 = new MyGuiCompositeTexture(null);
            texture46.Center = texture;
            TEXTURE_BUTTON_HELP_NORMAL = texture46;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\HelpHighlight.dds"
            };
            MyGuiCompositeTexture texture47 = new MyGuiCompositeTexture(null);
            texture47.Center = texture;
            TEXTURE_BUTTON_HELP_HIGHLIGHT = texture47;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\Envelope.dds"
            };
            MyGuiCompositeTexture texture48 = new MyGuiCompositeTexture(null);
            texture48.Center = texture;
            TEXTURE_BUTTON_ENVELOPE_NORMAL = texture48;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\EnvelopeHighlight.dds"
            };
            MyGuiCompositeTexture texture49 = new MyGuiCompositeTexture(null);
            texture49.Center = texture;
            TEXTURE_BUTTON_ENVELOPE_HIGHLIGHT = texture49;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButtonHighlight.dds"
            };
            MyGuiCompositeTexture texture50 = new MyGuiCompositeTexture(null);
            texture50.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_HIGHLIGHT = texture50;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 64f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButton.dds"
            };
            MyGuiCompositeTexture texture51 = new MyGuiCompositeTexture(null);
            texture51.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_NORMAL = texture51;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(90f, 65f),
                Texture = @"Textures\GUI\Controls\switch_on_off_left_highlight.dds"
            };
            MyGuiCompositeTexture texture52 = new MyGuiCompositeTexture(null);
            texture52.LeftTop = texture;
            TEXTURE_SWITCHONOFF_LEFT_HIGHLIGHT = texture52;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(90f, 65f),
                Texture = @"Textures\GUI\Controls\switch_on_off_left.dds"
            };
            MyGuiCompositeTexture texture53 = new MyGuiCompositeTexture(null);
            texture53.LeftTop = texture;
            TEXTURE_SWITCHONOFF_LEFT_NORMAL = texture53;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(90f, 65f),
                Texture = @"Textures\GUI\Controls\switch_on_off_right_highlight.dds"
            };
            MyGuiCompositeTexture texture54 = new MyGuiCompositeTexture(null);
            texture54.LeftTop = texture;
            TEXTURE_SWITCHONOFF_RIGHT_HIGHLIGHT = texture54;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(90f, 65f),
                Texture = @"Textures\GUI\Controls\switch_on_off_right.dds"
            };
            MyGuiCompositeTexture texture55 = new MyGuiCompositeTexture(null);
            texture55.LeftTop = texture;
            TEXTURE_SWITCHONOFF_RIGHT_NORMAL = texture55;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(145f, 67f),
                Texture = @"Textures\GUI\Controls\screen_inventory_trash.dds"
            };
            MyGuiCompositeTexture texture56 = new MyGuiCompositeTexture(null);
            texture56.LeftTop = texture;
            TEXTURE_INVENTORY_TRASH_NORMAL = texture56;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(145f, 67f),
                Texture = @"Textures\GUI\Controls\screen_inventory_trash_highlight.dds"
            };
            MyGuiCompositeTexture texture57 = new MyGuiCompositeTexture(null);
            texture57.LeftTop = texture;
            TEXTURE_INVENTORY_TRASH_HIGHLIGHT = texture57;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(71f, 164f),
                Texture = @"Textures\GUI\Controls\screen_inventory_bag.dds"
            };
            MyGuiCompositeTexture texture58 = new MyGuiCompositeTexture(null);
            texture58.LeftTop = texture;
            TEXTURE_INVENTORY_SWITCH_NORMAL = texture58;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(71f, 164f),
                Texture = @"Textures\GUI\Controls\screen_inventory_bag_highlight.dds"
            };
            MyGuiCompositeTexture texture59 = new MyGuiCompositeTexture(null);
            texture59.LeftTop = texture;
            TEXTURE_INVENTORY_SWITCH_HIGHLIGHT = texture59;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(71f, 164f),
                Texture = @"Textures\GUI\Controls\screen_inventory_hammer.dds"
            };
            MyGuiCompositeTexture texture60 = new MyGuiCompositeTexture(null);
            texture60.LeftTop = texture;
            TEXTURE_CRAFTING_SWITCH_NORMAL = texture60;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(71f, 164f),
                Texture = @"Textures\GUI\Controls\screen_inventory_hammer_highlight.dds"
            };
            MyGuiCompositeTexture texture61 = new MyGuiCompositeTexture(null);
            texture61.LeftTop = texture;
            TEXTURE_CRAFTING_SWITCH_HIGHLIGHT = texture61;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_left.dds",
                SizePx = new Vector2(8f, 48f)
            };
            MyGuiCompositeTexture texture62 = new MyGuiCompositeTexture(null);
            texture62.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_center.dds",
                SizePx = new Vector2(4f, 48f)
            };
            texture62.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_right.dds",
                SizePx = new Vector2(8f, 48f)
            };
            texture62.RightTop = texture;
            TEXTURE_TEXTBOX = texture62;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_left_highlight.dds",
                SizePx = new Vector2(8f, 48f)
            };
            MyGuiCompositeTexture texture63 = new MyGuiCompositeTexture(null);
            texture63.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_center_highlight.dds",
                SizePx = new Vector2(4f, 48f)
            };
            texture63.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\textbox_right_highlight.dds",
                SizePx = new Vector2(8f, 48f)
            };
            texture63.RightTop = texture;
            TEXTURE_TEXTBOX_HIGHLIGHT = texture63;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(512f, 1030f),
                Texture = @"Textures\GUI\Screens\TabGScreen.dds"
            };
            MyGuiCompositeTexture texture64 = new MyGuiCompositeTexture(null);
            texture64.Center = texture;
            TEXTURE_SCROLLABLE_LIST_TOOLS_BLOCKS = texture64;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_left_top.dds"
            };
            MyGuiCompositeTexture texture65 = new MyGuiCompositeTexture(null);
            texture65.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_left_center.dds"
            };
            texture65.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_left_bottom.dds"
            };
            texture65.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_center_top.dds"
            };
            texture65.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_center.dds"
            };
            texture65.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_center_bottom.dds"
            };
            texture65.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_right_top.dds"
            };
            texture65.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_right_center.dds"
            };
            texture65.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_right_bottom.dds"
            };
            texture65.RightBottom = texture;
            TEXTURE_SCROLLABLE_LIST = texture65;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_left_top.dds"
            };
            MyGuiCompositeTexture texture66 = new MyGuiCompositeTexture(null);
            texture66.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_left_center.dds"
            };
            texture66.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_left_bottom.dds"
            };
            texture66.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_center_top.dds"
            };
            texture66.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_center.dds"
            };
            texture66.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_center_bottom.dds"
            };
            texture66.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_right_top.dds"
            };
            texture66.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_right_center.dds"
            };
            texture66.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_border_right_bottom.dds"
            };
            texture66.RightBottom = texture;
            TEXTURE_SCROLLABLE_LIST_BORDER = texture66;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_top.dds"
            };
            MyGuiCompositeTexture texture67 = new MyGuiCompositeTexture(null);
            texture67.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_center.dds"
            };
            texture67.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_bottom.dds"
            };
            texture67.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center_top.dds"
            };
            texture67.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center.dds"
            };
            texture67.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center_bottom.dds"
            };
            texture67.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_right_top.dds"
            };
            texture67.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\list_WBorder_right_center.png"
            };
            texture67.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_right_bottom.dds"
            };
            texture67.RightBottom = texture;
            TEXTURE_WBORDER_LIST = texture67;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_top.dds"
            };
            MyGuiCompositeTexture texture68 = new MyGuiCompositeTexture(null);
            texture68.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_center.dds"
            };
            texture68.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_left_bottom.dds"
            };
            texture68.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center_top.dds"
            };
            texture68.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center.dds"
            };
            texture68.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_center_bottom.dds"
            };
            texture68.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_right_top.dds"
            };
            texture68.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_right_center.dds"
            };
            texture68.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(50f, 4f),
                Texture = @"Textures\GUI\Controls\scrollable_list_WBorder_right_bottom.dds"
            };
            texture68.RightBottom = texture;
            TEXTURE_SCROLLABLE_WBORDER_LIST = texture68;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_left_top.dds"
            };
            MyGuiCompositeTexture texture69 = new MyGuiCompositeTexture(null);
            texture69.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_left_center.dds"
            };
            texture69.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_left_bottom.dds"
            };
            texture69.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_center_top.dds"
            };
            texture69.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_center.dds"
            };
            texture69.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_center_bottom.dds"
            };
            texture69.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_right_top.dds"
            };
            texture69.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_right_center.dds"
            };
            texture69.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_right_bottom.dds"
            };
            texture69.RightBottom = texture;
            TEXTURE_RECTANGLE_DARK = texture69;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_left_top.dds"
            };
            MyGuiCompositeTexture texture70 = new MyGuiCompositeTexture(null);
            texture70.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_left_center.dds"
            };
            texture70.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_left_bottom.dds"
            };
            texture70.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_center_top.dds"
            };
            texture70.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_center.dds"
            };
            texture70.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_center_bottom.dds"
            };
            texture70.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_right_top.dds"
            };
            texture70.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_right_center.dds"
            };
            texture70.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_right_bottom.dds"
            };
            texture70.RightBottom = texture;
            TEXTURE_RECTANGLE_BUTTON_BORDER = texture70;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_left_top.dds"
            };
            MyGuiCompositeTexture texture71 = new MyGuiCompositeTexture(null);
            texture71.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_left_center.dds"
            };
            texture71.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_left_bottom.dds"
            };
            texture71.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_center_top.dds"
            };
            texture71.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_center.dds"
            };
            texture71.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_center_bottom.dds"
            };
            texture71.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_right_top.dds"
            };
            texture71.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_right_center.dds"
            };
            texture71.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_button_highlighted_right_bottom.dds"
            };
            texture71.RightBottom = texture;
            TEXTURE_RECTANGLE_BUTTON_HIGHLIGHTED_BORDER = texture71;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_left_top.dds"
            };
            MyGuiCompositeTexture texture72 = new MyGuiCompositeTexture(null);
            texture72.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_left_center.dds"
            };
            texture72.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_left_bottom.dds"
            };
            texture72.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_center_top.dds"
            };
            texture72.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_center.dds"
            };
            texture72.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_center_bottom.dds"
            };
            texture72.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_right_top.dds"
            };
            texture72.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_right_center.dds"
            };
            texture72.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_dark_border_right_bottom.dds"
            };
            texture72.RightBottom = texture;
            TEXTURE_RECTANGLE_DARK_BORDER = texture72;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_left_top.dds"
            };
            MyGuiCompositeTexture texture73 = new MyGuiCompositeTexture(null);
            texture73.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_left_center.dds"
            };
            texture73.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_left_bottom.dds"
            };
            texture73.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_center_top.dds"
            };
            texture73.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_center.dds"
            };
            texture73.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_center_bottom.dds"
            };
            texture73.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_right_top.dds"
            };
            texture73.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_right_center.dds"
            };
            texture73.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_load_border_right_bottom.dds"
            };
            texture73.RightBottom = texture;
            TEXTURE_RECTANGLE_LOAD_BORDER = texture73;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(16f, 16f),
                Texture = @"Textures\GUI\Controls\news_background_left_top.dds"
            };
            MyGuiCompositeTexture texture74 = new MyGuiCompositeTexture(null);
            texture74.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(16f, 24f),
                Texture = @"Textures\GUI\Controls\news_background_left_center.dds"
            };
            texture74.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(16f, 16f),
                Texture = @"Textures\GUI\Controls\news_background_left_bottom.dds"
            };
            texture74.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(15f, 16f),
                Texture = @"Textures\GUI\Controls\news_background_center_top.dds"
            };
            texture74.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(15f, 24f),
                Texture = @"Textures\GUI\Controls\news_background_center.dds"
            };
            texture74.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(15f, 16f),
                Texture = @"Textures\GUI\Controls\news_background_center_bottom.dds"
            };
            texture74.CenterBottom = texture;
            TEXTURE_NEWS_BACKGROUND = texture74;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(7f, 14f),
                Texture = @"Textures\GUI\Controls\news_background_right_top.dds"
            };
            MyGuiCompositeTexture texture75 = new MyGuiCompositeTexture(null);
            texture75.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(7f, 24f),
                Texture = @"Textures\GUI\Controls\news_background_right_center.dds"
            };
            texture75.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(7f, 16f),
                Texture = @"Textures\GUI\Controls\news_background_right_bottom.dds"
            };
            texture75.RightBottom = texture;
            TEXTURE_NEWS_BACKGROUND_BlueLine = texture75;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(14f, 14f),
                Texture = @"Textures\GUI\Controls\news_background_left_top.dds"
            };
            MyGuiCompositeTexture texture76 = new MyGuiCompositeTexture(null);
            texture76.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(14f, 24f),
                Texture = @"Textures\GUI\Controls\news_background_left_center.dds"
            };
            texture76.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(15f, 14f),
                Texture = @"Textures\GUI\Controls\news_background_center_top.dds"
            };
            texture76.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(15f, 24f),
                Texture = @"Textures\GUI\Controls\news_background_center.dds"
            };
            texture76.Center = texture;
            TEXTURE_NEWS_PAGING_BACKGROUND = texture76;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_left_top.dds"
            };
            MyGuiCompositeTexture texture77 = new MyGuiCompositeTexture(null);
            texture77.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_left_center.dds"
            };
            texture77.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_left_bottom.dds"
            };
            texture77.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_center_top.dds"
            };
            texture77.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_center.dds"
            };
            texture77.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_center_bottom.dds"
            };
            texture77.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_right_top.dds"
            };
            texture77.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_right_center.dds"
            };
            texture77.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 4f),
                Texture = @"Textures\GUI\Controls\rectangle_neutral_right_bottom.dds"
            };
            texture77.RightBottom = texture;
            TEXTURE_RECTANGLE_NEUTRAL = texture77;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(20f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_left.dds"
            };
            MyGuiCompositeTexture texture78 = new MyGuiCompositeTexture(null);
            texture78.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_center.dds"
            };
            texture78.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(51f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_right.dds"
            };
            texture78.RightTop = texture;
            TEXTURE_COMBOBOX_NORMAL = texture78;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(20f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_highlight_left.dds"
            };
            MyGuiCompositeTexture texture79 = new MyGuiCompositeTexture(null);
            texture79.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(4f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_highlight_center.dds"
            };
            texture79.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(51f, 48f),
                Texture = @"Textures\GUI\Controls\combobox_default_highlight_right.dds"
            };
            texture79.RightTop = texture;
            TEXTURE_COMBOBOX_HIGHLIGHT = texture79;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Controls\grid_item.dds",
                Highlight = @"Textures\GUI\Controls\grid_item_highlight.dds",
                SizePx = new Vector2(82f, 82f)
            };
            TEXTURE_GRID_ITEM = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Controls\grid_item.dds",
                Highlight = @"Textures\GUI\Controls\grid_item_highlight.dds",
                SizePx = new Vector2(78f, 78f)
            };
            TEXTURE_GRID_ITEM_SMALL = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Controls\grid_item.dds",
                Highlight = @"Textures\GUI\Controls\grid_item_highlight.dds",
                SizePx = new Vector2(50f, 50f)
            };
            TEXTURE_GRID_ITEM_TINY = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\large_block.dds",
                Highlight = @"Textures\GUI\Icons\buttons\large_block_highlight.dds",
                SizePx = new Vector2(41f, 41f)
            };
            TEXTURE_BUTTON_ICON_LARGE_BLOCK = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\small_block.dds",
                Highlight = @"Textures\GUI\Icons\buttons\small_block_highlight.dds",
                SizePx = new Vector2(43f, 43f)
            };
            TEXTURE_BUTTON_ICON_SMALL_BLOCK = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\tool.dds",
                Highlight = @"Textures\GUI\Icons\buttons\tool_highlight.dds",
                SizePx = new Vector2(41f, 41f)
            };
            TEXTURE_BUTTON_ICON_TOOL = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\component.dds",
                Highlight = @"Textures\GUI\Icons\buttons\component_highlight.dds",
                SizePx = new Vector2(37f, 45f)
            };
            TEXTURE_BUTTON_ICON_COMPONENT = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\disassembly.dds",
                Highlight = @"Textures\GUI\Icons\buttons\disassembly_highlight.dds",
                SizePx = new Vector2(32f, 32f)
            };
            TEXTURE_BUTTON_ICON_DISASSEMBLY = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\repeat.dds",
                Highlight = @"Textures\GUI\Icons\buttons\repeat_highlight.dds",
                SizePx = new Vector2(54f, 34f)
            };
            TEXTURE_BUTTON_ICON_REPEAT = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\repeat_Inactive.dds",
                Highlight = @"Textures\GUI\Icons\buttons\repeat_Inactive_Highlight.dds",
                SizePx = new Vector2(54f, 34f)
            };
            TEXTURE_BUTTON_ICON_REPEAT_INACTIVE = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\coopmode.dds",
                Highlight = @"Textures\GUI\Icons\buttons\coopmode_highlight.dds",
                SizePx = new Vector2(54f, 34f)
            };
            TEXTURE_BUTTON_ICON_SLAVE = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\coopmode_Inactive.dds",
                Highlight = @"Textures\GUI\Icons\buttons\coopmode_Inactive_HighLight.dds",
                SizePx = new Vector2(54f, 34f)
            };
            TEXTURE_BUTTON_ICON_SLAVE_INACTIVE = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\WhiteFlag.dds",
                Highlight = @"Textures\GUI\WhiteFlag.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_WHITE_FLAG = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\RequestSent.dds",
                Highlight = @"Textures\GUI\RequestSent.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_SENT_WHITE_FLAG = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\PlayerRequest.dds",
                Highlight = @"Textures\GUI\PlayerRequest.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_SENT_JOIN_REQUEST = texture2;
            MyGuiPaddedTexture texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\message_background_red.dds",
                SizePx = new Vector2(1343f, 321f),
                PaddingSizePx = new Vector2(20f, 25f)
            };
            TEXTURE_MESSAGEBOX_BACKGROUND_ERROR = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\message_background_blue.dds",
                SizePx = new Vector2(1343f, 321f),
                PaddingSizePx = new Vector2(20f, 25f)
            };
            TEXTURE_MESSAGEBOX_BACKGROUND_INFO = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\message_background_questlog_blue.dds",
                SizePx = new Vector2(1343f, 321f),
                PaddingSizePx = new Vector2(20f, 25f)
            };
            TEXTURE_QUESTLOG_BACKGROUND_INFO = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_background.dds",
                SizePx = new Vector2(1024f, 1024f),
                PaddingSizePx = new Vector2(24f, 24f)
            };
            TEXTURE_SCREEN_BACKGROUND = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_background_red.dds",
                SizePx = new Vector2(1024f, 1024f),
                PaddingSizePx = new Vector2(24f, 24f)
            };
            TEXTURE_SCREEN_BACKGROUND_RED = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\CenterGScreen.dds",
                SizePx = new Vector2(913f, 820f),
                PaddingSizePx = new Vector2(12f, 10f)
            };
            TEXTURE_SCREEN_TOOLS_BACKGROUND_BLOCKS = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_tools_background_controls.dds",
                SizePx = new Vector2(397f, 529f),
                PaddingSizePx = new Vector2(24f, 24f)
            };
            TEXTURE_SCREEN_TOOLS_BACKGROUND_CONTROLS = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_tools_background_weapons.dds",
                SizePx = new Vector2(868f, 110f),
                PaddingSizePx = new Vector2(12f, 9f)
            };
            TEXTURE_SCREEN_TOOLS_BACKGROUND_WEAPONS = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_stats_background.dss",
                SizePx = new Vector2(256f, 128f),
                PaddingSizePx = new Vector2(12f, 12f)
            };
            TEXTURE_SCREEN_STATS_BACKGROUND = texture3;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\ModFolderIcon.dds",
                Highlight = @"Textures\GUI\Icons\buttons\ModFolderIcon.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_MODS_LOCAL = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\BluePrintFolderIcon.dds",
                Highlight = @"Textures\GUI\Icons\buttons\BluePrintFolderIcon.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_BLUEPRINTS_LOCAL = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\BluePrintCloud.png",
                Highlight = @"Textures\GUI\Icons\buttons\BluePrintCloud.png",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_BLUEPRINTS_CLOUD = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\star.png",
                Highlight = @"Textures\GUI\Icons\star.png",
                SizePx = new Vector2(24f, 24f)
            };
            TEXTURE_ICON_STAR = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\Lock.png",
                Highlight = @"Textures\GUI\Icons\Lock.png",
                SizePx = new Vector2(24f, 24f)
            };
            TEXTURE_ICON_LOCK = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\warning.png",
                Highlight = @"Textures\GUI\Icons\warning.png",
                SizePx = new Vector2(24f, 24f)
            };
            TEXTURE_ICON_EXPERIMENTAL = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\ArrowFolderIcon.dds",
                Highlight = @"Textures\GUI\Icons\buttons\ArrowFolderIcon.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_BLUEPRINTS_ARROW = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Icons\buttons\ModSteamIcon.dds",
                Highlight = @"Textures\GUI\Icons\buttons\ModSteamIcon.dds",
                SizePx = new Vector2(53f, 40f)
            };
            TEXTURE_ICON_MODS_WORKSHOP = texture2;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_checked.dds"
            };
            MyGuiCompositeTexture texture80 = new MyGuiCompositeTexture(null);
            texture80.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_NORMAL_CHECKED = texture80;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_unchecked.dds"
            };
            MyGuiCompositeTexture texture81 = new MyGuiCompositeTexture(null);
            texture81.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_NORMAL_UNCHECKED = texture81;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_indeterminate.dds"
            };
            MyGuiCompositeTexture texture82 = new MyGuiCompositeTexture(null);
            texture82.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_NORMAL_INDETERMINATE = texture82;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_checked_highlight.dds"
            };
            MyGuiCompositeTexture texture83 = new MyGuiCompositeTexture(null);
            texture83.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_CHECKED = texture83;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_unchecked_highlight.dds"
            };
            MyGuiCompositeTexture texture84 = new MyGuiCompositeTexture(null);
            texture84.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_UNCHECKED = texture84;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(57f, 54f),
                Texture = @"Textures\GUI\Controls\checkbox_indeterminate_highlight.dds"
            };
            MyGuiCompositeTexture texture85 = new MyGuiCompositeTexture(null);
            texture85.LeftTop = texture;
            TEXTURE_CHECKBOX_DEFAULT_HIGHLIGHT_INDETERMINATE = texture85;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(40f, 38f),
                Texture = @"Textures\GUI\Controls\checkbox_green_checked.png"
            };
            MyGuiCompositeTexture texture86 = new MyGuiCompositeTexture(null);
            texture86.LeftTop = texture;
            TEXTURE_CHECKBOX_GREEN_CHECKED = texture86;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(0f, 0f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            MyGuiCompositeTexture texture87 = new MyGuiCompositeTexture(null);
            texture87.LeftTop = texture;
            TEXTURE_CHECKBOX_BLANK = texture87;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Controls\slider_thumb.dds",
                Highlight = @"Textures\GUI\Controls\slider_thumb_highlight.dds",
                SizePx = new Vector2(32f, 32f)
            };
            TEXTURE_SLIDER_THUMB_DEFAULT = texture2;
            texture2 = new MyGuiHighlightTexture {
                Normal = @"Textures\GUI\Controls\hue_slider_thumb.dds",
                Highlight = @"Textures\GUI\Controls\hue_slider_thumb_highlight.dds",
                SizePx = new Vector2(32f, 32f)
            };
            TEXTURE_HUE_SLIDER_THUMB_DEFAULT = texture2;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\hud_bg_medium_default.dds",
                SizePx = new Vector2(213f, 183f),
                PaddingSizePx = new Vector2(9f, 15f)
            };
            TEXTURE_HUD_BG_MEDIUM_DEFAULT = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\hud_bg_large_default.dds",
                SizePx = new Vector2(300f, 366f),
                PaddingSizePx = new Vector2(9f, 15f)
            };
            TEXTURE_HUD_BG_LARGE_DEFAULT = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\hud_bg_medium_red.dds",
                SizePx = new Vector2(213f, 181f),
                PaddingSizePx = new Vector2(9f, 15f)
            };
            TEXTURE_HUD_BG_MEDIUM_RED = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\hud_bg_medium_red2.dds",
                SizePx = new Vector2(3f, 127f),
                PaddingSizePx = new Vector2(0f, 15f)
            };
            TEXTURE_HUD_BG_MEDIUM_RED2 = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\hud_bg_performance.dds",
                SizePx = new Vector2(441f, 124f),
                PaddingSizePx = new Vector2(0f, 0f)
            };
            TEXTURE_HUD_BG_PERFORMANCE = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Icons\VoiceIcon.dds",
                SizePx = new Vector2(128f, 128f),
                PaddingSizePx = new Vector2(5f, 5f)
            };
            TEXTURE_VOICE_CHAT = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Icons\DisconnectedPlayerIcon.png",
                SizePx = new Vector2(128f, 128f),
                PaddingSizePx = new Vector2(5f, 5f)
            };
            TEXTURE_DISCONNECTED_PLAYER = texture3;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_left.dds",
                SizePx = new Vector2(23f, 55f)
            };
            MyGuiCompositeTexture texture88 = new MyGuiCompositeTexture(null);
            texture88.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_center.dds",
                SizePx = new Vector2(4f, 55f)
            };
            texture88.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_right.dds",
                SizePx = new Vector2(23f, 55f)
            };
            texture88.RightTop = texture;
            TEXTURE_SLIDER_RAIL = texture88;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_left_highlight.dds",
                SizePx = new Vector2(23f, 55f)
            };
            MyGuiCompositeTexture texture89 = new MyGuiCompositeTexture(null);
            texture89.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_center_highlight.dds",
                SizePx = new Vector2(4f, 55f)
            };
            texture89.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\slider_rail_right_highlight.dds",
                SizePx = new Vector2(23f, 55f)
            };
            texture89.RightTop = texture;
            TEXTURE_SLIDER_RAIL_HIGHLIGHT = texture89;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_left.dds",
                SizePx = new Vector2(23f, 55f)
            };
            MyGuiCompositeTexture texture90 = new MyGuiCompositeTexture(null);
            texture90.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_center.dds",
                SizePx = new Vector2(4f, 55f)
            };
            texture90.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_right.dds",
                SizePx = new Vector2(23f, 55f)
            };
            texture90.RightTop = texture;
            TEXTURE_HUE_SLIDER_RAIL = texture90;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_left_highlight.dds",
                SizePx = new Vector2(23f, 55f)
            };
            MyGuiCompositeTexture texture91 = new MyGuiCompositeTexture(null);
            texture91.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_center_highlight.dds",
                SizePx = new Vector2(4f, 55f)
            };
            texture91.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\hue_slider_rail_right_highlight.dds",
                SizePx = new Vector2(23f, 55f)
            };
            texture91.RightTop = texture;
            TEXTURE_HUE_SLIDER_RAIL_HIGHLIGHT = texture91;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_top.dds",
                SizePx = new Vector2(46f, 46f)
            };
            MyGuiCompositeTexture texture92 = new MyGuiCompositeTexture(null);
            texture92.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_center.dds",
                SizePx = new Vector2(46f, 4f)
            };
            texture92.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_bottom.dds",
                SizePx = new Vector2(46f, 23f)
            };
            texture92.LeftBottom = texture;
            TEXTURE_SCROLLBAR_V_THUMB = texture92;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_top_highlight.dds",
                SizePx = new Vector2(46f, 46f)
            };
            MyGuiCompositeTexture texture93 = new MyGuiCompositeTexture(null);
            texture93.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_center_highlight.dds",
                SizePx = new Vector2(46f, 4f)
            };
            texture93.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_v_thumb_bottom_highlight.dds",
                SizePx = new Vector2(46f, 23f)
            };
            texture93.LeftBottom = texture;
            TEXTURE_SCROLLBAR_V_THUMB_HIGHLIGHT = texture93;
            TEXTURE_SCROLLBAR_V_BACKGROUND = new MyGuiCompositeTexture(null);
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_left.dds",
                SizePx = new Vector2(39f, 46f)
            };
            MyGuiCompositeTexture texture94 = new MyGuiCompositeTexture(null);
            texture94.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_center.dds",
                SizePx = new Vector2(4f, 46f)
            };
            texture94.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_right.dds",
                SizePx = new Vector2(26f, 46f)
            };
            texture94.RightTop = texture;
            TEXTURE_SCROLLBAR_H_THUMB = texture94;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_left_highlight.dds",
                SizePx = new Vector2(39f, 46f)
            };
            MyGuiCompositeTexture texture95 = new MyGuiCompositeTexture(null);
            texture95.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_center_highlight.dds",
                SizePx = new Vector2(4f, 46f)
            };
            texture95.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\scrollbar_h_thumb_right_highlight.dds",
                SizePx = new Vector2(26f, 46f)
            };
            texture95.RightTop = texture;
            TEXTURE_SCROLLBAR_H_THUMB_HIGHLIGHT = texture95;
            TEXTURE_SCROLLBAR_H_BACKGROUND = new MyGuiCompositeTexture(null);
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\ToolBarTab.dds"
            };
            MyGuiCompositeTexture texture96 = new MyGuiCompositeTexture(null);
            texture96.Center = texture;
            TEXTURE_TOOLBAR_TAB = texture96;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Controls\ToolBarTabHighlight.dds"
            };
            MyGuiCompositeTexture texture97 = new MyGuiCompositeTexture(null);
            texture97.Center = texture;
            TEXTURE_TOOLBAR_TAB_HIGHLIGHT = texture97;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_left_top.dds"
            };
            MyGuiCompositeTexture texture98 = new MyGuiCompositeTexture(null);
            texture98.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_right_top.dds"
            };
            texture98.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture98.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_left_bottom.dds"
            };
            texture98.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_right_bottom.dds"
            };
            texture98.RightBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture98.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture98.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture98.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture98.RightCenter = texture;
            TEXTURE_COMPOSITE_ROUND_ALL = texture98;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Composite\white_left_top.dds"
            };
            MyGuiCompositeTexture texture99 = new MyGuiCompositeTexture(null);
            texture99.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Composite\white_right_top.dds"
            };
            texture99.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture99.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Composite\white_left_bottom.dds"
            };
            texture99.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Composite\white_right_bottom.dds"
            };
            texture99.RightBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture99.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture99.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture99.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(3f, 3f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture99.RightCenter = texture;
            TEXTURE_COMPOSITE_ROUND_ALL_SMALL = texture99;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_left_top.dds"
            };
            MyGuiCompositeTexture texture100 = new MyGuiCompositeTexture(null);
            texture100.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Composite\white_right_top.dds"
            };
            texture100.RightTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture100.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture100.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture100.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(8f, 8f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture100.RightCenter = texture;
            TEXTURE_COMPOSITE_ROUND_TOP = texture100;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 48f),
                Texture = @"Textures\GUI\Composite\white_left_bottom_slope.dds"
            };
            MyGuiCompositeTexture texture101 = new MyGuiCompositeTexture(null);
            texture101.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 1f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture101.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture101.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(1f, 48f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture101.CenterBottom = texture;
            TEXTURE_COMPOSITE_SLOPE_LEFTBOTTOM = texture101;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 30f),
                Texture = @"Textures\GUI\Composite\white_left_bottom_slope_30.dds"
            };
            MyGuiCompositeTexture texture102 = new MyGuiCompositeTexture(null);
            texture102.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 1f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture102.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture102.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(1f, 30f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture102.CenterBottom = texture;
            TEXTURE_COMPOSITE_SLOPE_LEFTBOTTOM_30 = texture102;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 40f),
                Texture = @"Textures\GUI\Composite\white_left_bottom_blockinfo.dds"
            };
            MyGuiCompositeTexture texture103 = new MyGuiCompositeTexture(null);
            texture103.LeftBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 1f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.LeftCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(64f, 1f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.LeftTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(1f, 40f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.CenterBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.Center = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.CenterTop = texture;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(1f, 40f),
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.RightBottom = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.RightCenter = texture;
            texture = new MyGuiSizedTexture {
                SizePx = Vector2.One,
                Texture = @"Textures\GUI\Blank.dds"
            };
            texture103.RightTop = texture;
            TEXTURE_COMPOSITE_BLOCKINFO_PROGRESSBAR = texture103;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\GravityHudGlobe.dds",
                SizePx = new Vector2(138f, 138f),
                PaddingSizePx = new Vector2(0f, 0f)
            };
            TEXTURE_HUD_GRAVITY_GLOBE = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\GravityHudLine.dds",
                SizePx = new Vector2(228f, 2f),
                PaddingSizePx = new Vector2(0f, 0f)
            };
            TEXTURE_HUD_GRAVITY_LINE = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\GravityHudHorizon.dds",
                SizePx = new Vector2(512f, 512f),
                PaddingSizePx = new Vector2(0f, 0f)
            };
            TEXTURE_HUD_GRAVITY_HORIZON = texture3;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Blank.dds"
            };
            MyGuiCompositeTexture texture104 = new MyGuiCompositeTexture(null);
            texture104.Center = texture;
            TEXTURE_GUI_BLANK = texture104;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_stats_background.dds",
                SizePx = new Vector2(256f, 128f),
                PaddingSizePx = new Vector2(6f, 6f)
            };
            TEXTURE_HUD_STATS_BG = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Icons\ArrowUpBrown.dds"
            };
            TEXTURE_HUD_STAT_EFFECT_ARROW_UP = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Icons\ArrowDownRed.dds"
            };
            TEXTURE_HUD_STAT_EFFECT_ARROW_DOWN = texture3;
            texture3 = new MyGuiPaddedTexture {
                Texture = @"Textures\GUI\Screens\screen_stats_bar_background.dds",
                SizePx = new Vector2(72f, 13f),
                PaddingSizePx = new Vector2(1f, 1f)
            };
            TEXTURE_HUD_STAT_BAR_BG = texture3;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Icons\HUD 2017\GridSizeLarge.png"
            };
            MyGuiCompositeTexture texture105 = new MyGuiCompositeTexture(null);
            texture105.Center = texture;
            TEXTURE_HUD_GRID_LARGE = texture105;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Icons\HUD 2017\GridSizeLargeFit.png"
            };
            MyGuiCompositeTexture texture106 = new MyGuiCompositeTexture(null);
            texture106.Center = texture;
            TEXTURE_HUD_GRID_LARGE_FIT = texture106;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Icons\HUD 2017\GridSizeSmall.png"
            };
            MyGuiCompositeTexture texture107 = new MyGuiCompositeTexture(null);
            texture107.Center = texture;
            TEXTURE_HUD_GRID_SMALL = texture107;
            texture = new MyGuiSizedTexture {
                Texture = @"Textures\GUI\Icons\HUD 2017\GridSizeSmallFit.png"
            };
            MyGuiCompositeTexture texture108 = new MyGuiCompositeTexture(null);
            texture108.Center = texture;
            TEXTURE_HUD_GRID_SMALL_FIT = texture108;
            SHADOW_OFFSET = new Vector2(0f, 0f);
            CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER = new Vector4(1.2f, 1.2f, 1.2f, 1f);
            CONTROLS_DELTA = new Vector2(0f, 0.0525f);
            ROTATING_WHEEL_COLOR = Vector4.One;
            SHOW_CONTROL_TOOLTIP_DELAY = 20;
            TOOLTIP_DISTANCE_FROM_BORDER = 0.003f;
            DEFAULT_CONTROL_BACKGROUND_COLOR = new Vector4(1f, 1f, 1f, 1f);
            DEFAULT_CONTROL_NONACTIVE_COLOR = new Vector4(0.9f, 0.9f, 0.9f, 0.95f);
            DISABLED_BUTTON_COLOR = new Color(0x57, 0x7f, 0x93, 210);
            DISABLED_BUTTON_COLOR_VECTOR = new Vector4(0.52f, 0.6f, 0.63f, 0.9f);
            DISABLED_BUTTON_TEXT_COLOR = new Vector4(0.4f, 0.47f, 0.5f, 0.8f);
            LOCKBUTTON_SIZE_MODIFICATION = 0.85f;
            SCREEN_BACKGROUND_FADE_BLANK_DARK = new Vector4(0.03f, 0.04f, 0.05f, 0.7f);
            SCREEN_BACKGROUND_FADE_BLANK_DARK_PROGRESS_SCREEN = new Vector4(0.03f, 0.04f, 0.05f, 0.4f);
            SCREEN_CAPTION_DELTA_Y = 0.05f;
            SCREEN_BACKGROUND_COLOR = Vector4.One;
            LOADING_PLEASE_WAIT_POSITION = new Vector2(0.5f, 0.95f);
            LOADING_PLEASE_WAIT_COLOR = Vector4.One;
            TEXTBOX_TEXT_OFFSET = new Vector2(0.0075f, 0.007f);
            TEXTBOX_MEDIUM_SIZE = new Vector2(0.2525f, 0.055f);
            MOUSE_CURSOR_COLOR = Vector4.One;
            BUTTON_BACKGROUND_COLOR = DEFAULT_CONTROL_BACKGROUND_COLOR;
            MENU_BUTTONS_POSITION_DELTA = new Vector2(0f, 0.06f);
            BACK_BUTTON_BACKGROUND_COLOR = BUTTON_BACKGROUND_COLOR;
            BACK_BUTTON_TEXT_COLOR = DEFAULT_CONTROL_NONACTIVE_COLOR;
            BACK_BUTTON_SIZE = new Vector2(0.1625f, 0.05833333f);
            OK_BUTTON_SIZE = new Vector2(0.177f, 0.0765f);
            GENERIC_BUTTON_SPACING = new Vector2(0.002f, 0.002f);
            TREEVIEW_SELECTED_ITEM_COLOR = new Vector4(0.03f, 0.02f, 0.03f, 0.4f);
            TREEVIEW_DISABLED_ITEM_COLOR = new Vector4(1f, 0.3f, 0.3f, 1f);
            TREEVIEW_TEXT_COLOR = DEFAULT_CONTROL_NONACTIVE_COLOR;
            TREEVIEW_VERTICAL_LINE_COLOR = new Vector4(0.6196079f, 0.8156863f, 1f, 1f);
            TREEVIEW_VSCROLLBAR_SIZE = new Vector2(60f, 636f) / 3088f;
            TREEVIEW_HSCROLLBAR_SIZE = new Vector2(477f, 80f) / 3088f;
            COMBOBOX_MEDIUM_SIZE = new Vector2(0.3f, 0.03f);
            COMBOBOX_MEDIUM_ELEMENT_SIZE = new Vector2(0.3f, 0.03f);
            COMBOBOX_VSCROLLBAR_SIZE = new Vector2(0.02f, 0.08059585f);
            COMBOBOX_HSCROLLBAR_SIZE = new Vector2(0.08059585f, 0.02f);
            LISTBOX_BACKGROUND_COLOR = DEFAULT_CONTROL_BACKGROUND_COLOR;
            LISTBOX_ICON_SIZE = new Vector2(0.0205f, 0.02733f);
            LISTBOX_ICON_OFFSET = LISTBOX_ICON_SIZE / 8f;
            LISTBOX_WIDTH = 0.198f;
            DRAG_AND_DROP_TEXT_OFFSET = new Vector2(0.01f, 0f);
            DRAG_AND_DROP_TEXT_COLOR = DEFAULT_CONTROL_NONACTIVE_COLOR;
            DRAG_AND_DROP_SMALL_SIZE = new Vector2(0.07395f, 0.0986f);
            DRAG_AND_DROP_BACKGROUND_COLOR = new Vector4(1f, 1f, 1f, 1f);
            SLIDER_INSIDE_OFFSET_X = 0.017f;
            REPEAT_PRESS_DELAY = 100;
            MESSAGE_BOX_BUTTON_SIZE_SMALL = new Vector2(0.11875f, 0.05416667f);
            TOOL_TIP_RELATIVE_DEFAULT_POSITION = new Vector2(0.025f, 0.03f);
            LOADING_BACKGROUND_TEXTURE_REAL_SIZE = new Vector2I(0x780, 0x438);
            COLORED_TEXT_DEFAULT_COLOR = new Color(DEFAULT_CONTROL_NONACTIVE_COLOR);
            COLORED_TEXT_DEFAULT_HIGHLIGHT_COLOR = new Color(CONTROL_MOUSE_OVER_BACKGROUND_COLOR_MULTIPLIER * DEFAULT_CONTROL_NONACTIVE_COLOR);
            MULTILINE_LABEL_BORDER = new Vector2(0.01f, 0.005f);
            DEBUG_LABEL_TEXT_SCALE = 1f;
            DEBUG_BUTTON_TEXT_SCALE = 0.8f;
            DEBUG_STATISTICS_TEXT_SCALE = 0.75f;
            DEBUG_STATISTICS_ROW_DISTANCE = 0.02f;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(45f, 45f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButtonHighlight.dds"
            };
            MyGuiCompositeTexture texture109 = new MyGuiCompositeTexture(null);
            texture109.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_SMALL_HIGHLIGHT = texture109;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(45f, 45f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButton.dds"
            };
            MyGuiCompositeTexture texture110 = new MyGuiCompositeTexture(null);
            texture110.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_SMALL_NORMAL = texture110;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 48f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButtonHighlight.dds"
            };
            MyGuiCompositeTexture texture111 = new MyGuiCompositeTexture(null);
            texture111.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_48_HIGHLIGHT = texture111;
            texture = new MyGuiSizedTexture {
                SizePx = new Vector2(48f, 48f),
                Texture = @"Textures\GUI\Icons\buttons\SquareButton.dds"
            };
            MyGuiCompositeTexture texture112 = new MyGuiCompositeTexture(null);
            texture112.LeftTop = texture;
            TEXTURE_BUTTON_SQUARE_48_NORMAL = texture112;
        }
    }
}

