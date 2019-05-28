namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using VRage.Audio;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_AtmosphereDetectorComponent), true)]
    public class MyAtmosphereDetectorComponent : MyEntityComponentBase
    {
        private MyCharacter m_character;
        private bool m_localPlayer = true;
        private bool m_inAtmosphere;
        private AtmosphereStatus m_atmosphereStatus;

        public void InitComponent(bool onlyLocalPlayer, MyCharacter character)
        {
            this.m_localPlayer = onlyLocalPlayer;
            this.m_character = character;
        }

        public void UpdateAtmosphereStatus()
        {
            if ((this.m_character != null) && (!this.m_localPlayer || ((MySession.Static != null) && ReferenceEquals(this.m_character, MySession.Static.LocalCharacter))))
            {
                AtmosphereStatus atmosphereStatus = this.m_atmosphereStatus;
                Vector3D position = this.m_character.PositionComp.GetPosition();
                if (MyGravityProviderSystem.CalculateNaturalGravityInPoint(position).LengthSquared() <= 0f)
                {
                    this.m_atmosphereStatus = AtmosphereStatus.Space;
                }
                else
                {
                    MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                    if (((closestPlanet == null) || !closestPlanet.HasAtmosphere) || (closestPlanet.GetAirDensity(position) <= 0.5f))
                    {
                        this.m_atmosphereStatus = AtmosphereStatus.Space;
                    }
                    else
                    {
                        this.m_atmosphereStatus = AtmosphereStatus.Atmosphere;
                    }
                }
                if (this.m_atmosphereStatus == AtmosphereStatus.Space)
                {
                    float num = 0f;
                    if (this.m_character.OxygenComponent != null)
                    {
                        num = !this.m_localPlayer ? this.m_character.EnvironmentOxygenLevel : (!(MySession.Static.ControlledEntity is MyCharacter) ? ((float) this.m_character.OxygenLevelAtCharacterLocation) : this.m_character.EnvironmentOxygenLevel);
                    }
                    if (num > 0.1f)
                    {
                        this.m_atmosphereStatus = AtmosphereStatus.ShipOrStation;
                    }
                }
                if ((MyFakes.ENABLE_REALISTIC_LIMITER && (MyFakes.ENABLE_NEW_SOUNDS && ((atmosphereStatus != this.m_atmosphereStatus) && (MySession.Static != null)))) && MySession.Static.Settings.RealisticSound)
                {
                    MyAudio.Static.EnableMasterLimiter(!this.InAtmosphere && !this.InShipOrStation);
                }
            }
        }

        public bool InAtmosphere =>
            (this.m_atmosphereStatus == AtmosphereStatus.Atmosphere);

        public bool InShipOrStation =>
            (this.m_atmosphereStatus == AtmosphereStatus.ShipOrStation);

        public bool InVoid =>
            (this.m_atmosphereStatus == AtmosphereStatus.Space);

        public override string ComponentTypeDebugString =>
            "AtmosphereDetector";

        private enum AtmosphereStatus
        {
            NotSet,
            Space,
            ShipOrStation,
            Atmosphere
        }
    }
}

