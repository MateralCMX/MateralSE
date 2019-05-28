namespace Sandbox.Game
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGUISettings
    {
        public bool EnableToolbarConfigScreen;
        public bool EnableTerminalScreen;
        public bool MultipleSpinningWheels;
        public Type HUDScreen;
        public Type ToolbarConfigScreen;
        public Type ToolbarControl;
        public Type OptionsScreen;
        public Type CustomWorldScreen;
        public Type ScenarioScreen;
        public Type EditWorldSettingsScreen;
        public Type HelpScreen;
        public Type VoxelMapEditingScreen;
        public Type GameplayOptionsScreen;
        public Type ScenarioLobbyClientScreen;
        public Type InventoryScreen;
        public Type AdminMenuScreen;
        public Type FactionScreen;
        public Type CreateFactionScreen;
        public Type PlayersScreen;
        public Type MainMenu;
        public Type PerformanceWarningScreen;
        public string[] MainMenuBackgroundVideos;
        public Vector2I LoadingScreenIndexRange;
    }
}

