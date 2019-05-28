namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.ObjectBuilders.Definitions;

    public class MyDroneAIData
    {
        public string Name;
        public float Height;
        public float Depth;
        public float Width;
        public bool AvoidCollisions;
        public float SpeedLimit;
        public bool RotateToPlayer;
        public float PlayerYAxisOffset;
        public int WaypointDelayMsMin;
        public int WaypointDelayMsMax;
        public float WaypointThresholdDistance;
        public float PlayerTargetDistance;
        public float MaxManeuverDistance;
        public float MaxManeuverDistanceSq;
        public int WaypointMaxTime;
        public int LostTimeMs;
        public float MinStrafeDistance;
        public float MinStrafeDistanceSq;
        public bool UseStaticWeaponry;
        public float StaticWeaponryUsage;
        public float StaticWeaponryUsageSq;
        public bool UseTools;
        public float ToolsUsage;
        public float ToolsUsageSq;
        public bool UseKamikazeBehavior;
        public bool CanBeDisabled;
        public float KamikazeBehaviorDistance;
        public string AlternativeBehavior;
        public bool UsePlanetHover;
        public float PlanetHoverMin;
        public float PlanetHoverMax;
        public float RotationLimitSq;
        public bool UsesWeaponBehaviors;
        public float WeaponBehaviorNotFoundDelay;
        public List<MyWeaponBehavior> WeaponBehaviors;
        public string SoundLoop;

        public MyDroneAIData()
        {
            this.Name = "";
            this.Height = 10f;
            this.Depth = 5f;
            this.Width = 10f;
            this.AvoidCollisions = true;
            this.SpeedLimit = 25f;
            this.RotateToPlayer = true;
            this.PlayerYAxisOffset = 0.9f;
            this.WaypointDelayMsMin = 0x3e8;
            this.WaypointDelayMsMax = 0xbb8;
            this.WaypointThresholdDistance = 0.5f;
            this.PlayerTargetDistance = 200f;
            this.MaxManeuverDistance = 250f;
            this.MaxManeuverDistanceSq = 62500f;
            this.WaypointMaxTime = 0x3a98;
            this.LostTimeMs = 0x4e20;
            this.MinStrafeDistance = 2f;
            this.MinStrafeDistanceSq = 4f;
            this.UseStaticWeaponry = true;
            this.StaticWeaponryUsage = 300f;
            this.StaticWeaponryUsageSq = 90000f;
            this.UseTools = true;
            this.ToolsUsage = 5f;
            this.ToolsUsageSq = 25f;
            this.UseKamikazeBehavior = true;
            this.CanBeDisabled = true;
            this.KamikazeBehaviorDistance = 75f;
            this.AlternativeBehavior = "";
            this.PlanetHoverMin = 2f;
            this.PlanetHoverMax = 25f;
            this.WeaponBehaviorNotFoundDelay = 3f;
            this.SoundLoop = "";
            this.PostProcess();
        }

        public MyDroneAIData(MyObjectBuilder_DroneBehaviorDefinition definition)
        {
            int useStaticWeaponry;
            this.Name = "";
            this.Height = 10f;
            this.Depth = 5f;
            this.Width = 10f;
            this.AvoidCollisions = true;
            this.SpeedLimit = 25f;
            this.RotateToPlayer = true;
            this.PlayerYAxisOffset = 0.9f;
            this.WaypointDelayMsMin = 0x3e8;
            this.WaypointDelayMsMax = 0xbb8;
            this.WaypointThresholdDistance = 0.5f;
            this.PlayerTargetDistance = 200f;
            this.MaxManeuverDistance = 250f;
            this.MaxManeuverDistanceSq = 62500f;
            this.WaypointMaxTime = 0x3a98;
            this.LostTimeMs = 0x4e20;
            this.MinStrafeDistance = 2f;
            this.MinStrafeDistanceSq = 4f;
            this.UseStaticWeaponry = true;
            this.StaticWeaponryUsage = 300f;
            this.StaticWeaponryUsageSq = 90000f;
            this.UseTools = true;
            this.ToolsUsage = 5f;
            this.ToolsUsageSq = 25f;
            this.UseKamikazeBehavior = true;
            this.CanBeDisabled = true;
            this.KamikazeBehaviorDistance = 75f;
            this.AlternativeBehavior = "";
            this.PlanetHoverMin = 2f;
            this.PlanetHoverMax = 25f;
            this.WeaponBehaviorNotFoundDelay = 3f;
            this.SoundLoop = "";
            this.Name = definition.Id.SubtypeId;
            this.Height = definition.StrafeHeight;
            this.Depth = definition.StrafeDepth;
            this.Width = definition.StrafeWidth;
            this.AvoidCollisions = definition.AvoidCollisions;
            this.SpeedLimit = definition.SpeedLimit;
            this.RotateToPlayer = definition.RotateToPlayer;
            this.PlayerYAxisOffset = definition.PlayerYAxisOffset;
            this.WaypointDelayMsMin = definition.WaypointDelayMsMin;
            this.WaypointDelayMsMax = definition.WaypointDelayMsMax;
            this.WaypointThresholdDistance = definition.WaypointThresholdDistance;
            this.PlayerTargetDistance = definition.TargetDistance;
            this.MaxManeuverDistance = definition.MaxManeuverDistance;
            this.WaypointMaxTime = definition.WaypointMaxTime;
            this.LostTimeMs = definition.LostTimeMs;
            this.MinStrafeDistance = definition.MinStrafeDistance;
            this.UseStaticWeaponry = definition.UseStaticWeaponry;
            this.StaticWeaponryUsage = definition.StaticWeaponryUsage;
            this.UseKamikazeBehavior = definition.UseRammingBehavior;
            this.KamikazeBehaviorDistance = definition.RammingBehaviorDistance;
            this.AlternativeBehavior = definition.AlternativeBehavior;
            this.UseTools = definition.UseTools;
            this.ToolsUsage = definition.ToolsUsage;
            this.UsePlanetHover = definition.UsePlanetHover;
            this.PlanetHoverMin = definition.PlanetHoverMin;
            this.PlanetHoverMax = definition.PlanetHoverMax;
            if (!definition.UsesWeaponBehaviors || (definition.WeaponBehaviors.Count <= 0))
            {
                useStaticWeaponry = 0;
            }
            else
            {
                useStaticWeaponry = (int) this.UseStaticWeaponry;
            }
            this.UsesWeaponBehaviors = (bool) useStaticWeaponry;
            this.WeaponBehaviorNotFoundDelay = definition.WeaponBehaviorNotFoundDelay;
            this.WeaponBehaviors = definition.WeaponBehaviors;
            this.SoundLoop = definition.SoundLoop;
            this.PostProcess();
        }

        private void PostProcess()
        {
            this.MaxManeuverDistanceSq = this.MaxManeuverDistance * this.MaxManeuverDistance;
            this.MinStrafeDistanceSq = this.MinStrafeDistance * this.MinStrafeDistance;
            this.ToolsUsageSq = this.ToolsUsage * this.ToolsUsage;
            this.StaticWeaponryUsageSq = this.StaticWeaponryUsage * this.StaticWeaponryUsage;
            this.RotationLimitSq = Math.Max(this.ToolsUsageSq, Math.Max(this.StaticWeaponryUsageSq, this.MaxManeuverDistanceSq));
            if (this.WeaponBehaviors != null)
            {
                foreach (MyWeaponBehavior behavior in this.WeaponBehaviors)
                {
                    int num = 0;
                    while (true)
                    {
                        if (num >= behavior.Requirements.Count)
                        {
                            foreach (MyWeaponRule rule in behavior.WeaponRules)
                            {
                                if (string.IsNullOrEmpty(rule.Weapon))
                                {
                                    continue;
                                }
                                if (!rule.Weapon.Contains("MyObjectBuilder_"))
                                {
                                    rule.Weapon = "MyObjectBuilder_" + rule.Weapon;
                                }
                            }
                            break;
                        }
                        if (!behavior.Requirements[num].Contains("MyObjectBuilder_"))
                        {
                            behavior.Requirements[num] = "MyObjectBuilder_" + behavior.Requirements[num];
                        }
                        num++;
                    }
                }
            }
        }
    }
}

