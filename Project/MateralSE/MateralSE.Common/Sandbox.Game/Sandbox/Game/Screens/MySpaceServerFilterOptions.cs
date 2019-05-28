namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.GameServices;

    public class MySpaceServerFilterOptions : MyServerFilterOptions
    {
        public const byte SPACE_BOOL_OFFSET = 0x80;

        public MySpaceServerFilterOptions()
        {
        }

        public MySpaceServerFilterOptions(MyObjectBuilder_ServerFilterOptions ob) : base(ob)
        {
        }

        protected override Dictionary<byte, IMyFilterOption> CreateFilters()
        {
            Dictionary<byte, IMyFilterOption> dictionary = new Dictionary<byte, IMyFilterOption>();
            foreach (byte num in Enum.GetValues(typeof(MySpaceNumericOptionEnum)))
            {
                dictionary.Add(num, new MyFilterRange());
            }
            foreach (byte num2 in Enum.GetValues(typeof(MySpaceBoolOptionEnum)))
            {
                bool? nullable = null;
                dictionary.Add(num2, new MyFilterBool(nullable));
            }
            return dictionary;
        }

        public override bool FilterLobby(IMyLobby lobby)
        {
            if (!this.GetFilter(MySpaceNumericOptionEnum.InventoryMultipier).IsMatch(MyMultiplayerLobby.GetLobbyFloat("inventoryMultiplier", lobby, 1f)))
            {
                return false;
            }
            MyFilterRange filter = this.GetFilter(MySpaceNumericOptionEnum.ProductionMultipliers);
            return (filter.IsMatch(MyMultiplayerLobby.GetLobbyFloat("refineryMultiplier", lobby, 1f)) && filter.IsMatch(MyMultiplayerLobby.GetLobbyFloat("assemblerMultiplier", lobby, 1f)));
        }

        public override bool FilterServer(MyCachedServerItem server)
        {
            MyObjectBuilder_SessionSettings settings = server.Settings;
            if (settings == null)
            {
                return false;
            }
            if (!MySandboxGame.Config.ExperimentalMode && settings.IsSettingsExperimental())
            {
                return false;
            }
            if (!this.GetFilter(MySpaceNumericOptionEnum.InventoryMultipier).IsMatch(settings.InventorySizeMultiplier))
            {
                return false;
            }
            if (!this.GetFilter(MySpaceNumericOptionEnum.EnvionmentHostility).IsMatch((float) settings.EnvironmentHostility))
            {
                return false;
            }
            MyFilterRange filter = this.GetFilter(MySpaceNumericOptionEnum.ProductionMultipliers);
            return (filter.IsMatch(settings.AssemblerEfficiencyMultiplier) && (filter.IsMatch(settings.AssemblerSpeedMultiplier) && (filter.IsMatch(settings.RefinerySpeedMultiplier) && (this.GetFilter(MySpaceBoolOptionEnum.Spectator).IsMatch(settings.EnableSpectator) ? (this.GetFilter(MySpaceBoolOptionEnum.CopyPaste).IsMatch(settings.EnableCopyPaste) ? (this.GetFilter(MySpaceBoolOptionEnum.ThrusterDamage).IsMatch(settings.ThrusterDamage) ? (this.GetFilter(MySpaceBoolOptionEnum.PermanentDeath).IsMatch(settings.PermanentDeath) ? (this.GetFilter(MySpaceBoolOptionEnum.Weapons).IsMatch(settings.WeaponsEnabled) ? (this.GetFilter(MySpaceBoolOptionEnum.CargoShips).IsMatch(settings.CargoShipsEnabled) ? (this.GetFilter(MySpaceBoolOptionEnum.BlockDestruction).IsMatch(settings.DestructibleBlocks) ? (this.GetFilter(MySpaceBoolOptionEnum.Scripts).IsMatch(settings.EnableIngameScripts) ? (this.GetFilter(MySpaceBoolOptionEnum.Oxygen).IsMatch(settings.EnableOxygen) ? (this.GetFilter(MySpaceBoolOptionEnum.ThirdPerson).IsMatch(settings.Enable3rdPersonView) ? (this.GetFilter(MySpaceBoolOptionEnum.Encounters).IsMatch(settings.EnableEncounters) ? (this.GetFilter(MySpaceBoolOptionEnum.Airtightness).IsMatch(settings.EnableOxygenPressurization) ? (this.GetFilter(MySpaceBoolOptionEnum.UnsupportedStations).IsMatch(settings.StationVoxelSupport) ? (this.GetFilter(MySpaceBoolOptionEnum.VoxelDestruction).IsMatch(settings.EnableVoxelDestruction) ? (this.GetFilter(MySpaceBoolOptionEnum.Drones).IsMatch(settings.EnableDrones) ? (this.GetFilter(MySpaceBoolOptionEnum.Wolves).IsMatch(settings.EnableWolfs) ? (this.GetFilter(MySpaceBoolOptionEnum.Spiders).IsMatch(settings.EnableSpiders) ? (this.GetFilter(MySpaceBoolOptionEnum.RespawnShips).IsMatch(settings.EnableRespawnShips) ? ((server.Rules != null) && this.GetFilter(MySpaceBoolOptionEnum.ExternalServerManagement).IsMatch(server.Rules.ContainsKey("SM"))) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false) : false))));
        }

        public MyFilterBool GetFilter(MySpaceBoolOptionEnum key) => 
            ((MyFilterBool) base.Filters[(byte) key]);

        public MyFilterRange GetFilter(MySpaceNumericOptionEnum key) => 
            ((MyFilterRange) base.Filters[(byte) key]);
    }
}

