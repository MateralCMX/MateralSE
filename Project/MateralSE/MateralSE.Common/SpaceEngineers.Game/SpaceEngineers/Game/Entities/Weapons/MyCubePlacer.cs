namespace SpaceEngineers.Game.Entities.Weapons
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_CubePlacer), true)]
    public class MyCubePlacer : MyBlockPlacerBase
    {
        private static MyDefinitionId m_handItemDefId = new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer));

        public MyCubePlacer() : base(MyDefinitionManager.Static.TryGetHandItemDefinition(ref m_handItemDefId))
        {
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            int creativeMode;
            if (!MySession.Static.CreativeToolsEnabled(Sync.MyId) || !MySession.Static.HasCreativeRights)
            {
                creativeMode = (int) MySession.Static.CreativeMode;
            }
            else
            {
                creativeMode = 1;
            }
            if (creativeMode == 0)
            {
                base.Shoot(action, direction, overrideWeaponPos, gunAction);
                if (((action == MyShootActionEnum.PrimaryAction) && (!base.m_firstShot && ((MySandboxGame.TotalGamePlayTimeInMilliseconds - base.m_lastKeyPress) >= 500))) && (base.GetTargetBlock() != null))
                {
                    MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_Welder));
                    if (base.Owner.CanSwitchToWeapon(new MyDefinitionId?(id)))
                    {
                        base.Owner.SetupAutoswitch(new MyDefinitionId(typeof(MyObjectBuilder_Welder)), new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer)));
                    }
                }
            }
        }

        protected override MyBlockBuilderBase BlockBuilder =>
            MyCubeBuilder.Static;

        public override bool IsSkinnable =>
            false;
    }
}

