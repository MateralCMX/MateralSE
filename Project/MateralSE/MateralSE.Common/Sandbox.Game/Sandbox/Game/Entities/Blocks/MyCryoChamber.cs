namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MyCubeBlockType(typeof(MyObjectBuilder_CryoChamber)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyCryoChamber), typeof(Sandbox.ModAPI.Ingame.IMyCryoChamber) })]
    public class MyCryoChamber : MyCockpit, Sandbox.ModAPI.IMyCryoChamber, Sandbox.ModAPI.IMyCockpit, Sandbox.ModAPI.IMyShipController, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyShipController, VRage.Game.ModAPI.Interfaces.IMyControllableEntity, Sandbox.ModAPI.Ingame.IMyCockpit, Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider, IMyCameraController, Sandbox.ModAPI.IMyTextSurfaceProvider, Sandbox.ModAPI.Ingame.IMyCryoChamber
    {
        private string m_overlayTextureName = @"Textures\GUI\Screens\cryopod_interior.dds";
        private MyPlayer.PlayerId? m_currentPlayerId;
        private readonly VRage.Sync.Sync<MyPlayer.PlayerId?, SyncDirection.FromServer> m_attachedPlayerId;
        private bool m_retryAttachPilot;
        private bool m_pilotLights;
        private bool m_pilotJetpack;
        private bool m_pilotCameraInFP = true;

        public MyCryoChamber()
        {
            base.ControllerInfo.ControlAcquired += new Action<MyEntityController>(this.OnCryoChamberControlAcquired);
            this.m_attachedPlayerId.ValueChanged += x => this.AttachedPlayerChanged();
            base.MinHeadLocalXAngle = -50f;
            base.MaxHeadLocalXAngle = 60f;
            base.MinHeadLocalYAngle = -30f;
            base.MaxHeadLocalYAngle = 30f;
        }

        [CompilerGenerated, DebuggerHidden]
        private void <>n__0(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            base.OnInputChanged(resourceTypeId, oldInput, sink);
        }

        private void AttachedPlayerChanged()
        {
            if (this.m_attachedPlayerId.Value != null)
            {
                MyPlayer.PlayerId id = new MyPlayer.PlayerId(this.m_attachedPlayerId.Value.Value.SteamId, this.m_attachedPlayerId.Value.Value.SerialId);
                MyPlayer playerById = Sync.Players.GetPlayerById(id);
                if (playerById == null)
                {
                    this.m_retryAttachPilot = true;
                }
                else if (this.Pilot == null)
                {
                    MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
                    MyPlayer player2 = playerById;
                }
                else
                {
                    if (ReferenceEquals(playerById, MySession.Static.LocalHumanPlayer))
                    {
                        this.OnPlayerLoaded();
                        if (!ReferenceEquals(MySession.Static.CameraController, this))
                        {
                            Vector3D? position = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this, position);
                        }
                    }
                    playerById.Controller.TakeControl(this);
                    playerById.Identity.ChangeCharacter(this.Pilot);
                }
            }
        }

        private float CalculateRequiredPowerInput() => 
            (base.IsFunctional ? this.BlockDefinition.IdlePowerConsumption : 0f);

        public void CameraAttachedToChanged(IMyCameraController oldController, IMyCameraController newController)
        {
            if (ReferenceEquals(oldController, this))
            {
                MyRenderProxy.UpdateRenderObjectVisibility(base.Render.RenderObjectIDs[0], true, false);
            }
        }

        protected override bool CanHaveHorizon() => 
            false;

        public override UseActionResult CanUse(UseActionEnum actionEnum, Sandbox.Game.Entities.IMyControllableEntity user) => 
            (base.IsFunctional ? (base.IsWorking ? ((base.m_pilot == null) ? base.CanUse(actionEnum, user) : UseActionResult.UsedBySomeoneElse) : UseActionResult.Unpowered) : UseActionResult.CockpitDamaged);

        public override void CheckEmissiveState(bool force = false)
        {
            if (base.IsWorking)
            {
                this.SetEmissiveStateWorking();
            }
            else if (base.IsFunctional)
            {
                this.SetEmissiveStateDisabled();
            }
            else
            {
                this.SetEmissiveStateDamaged();
            }
        }

        protected override void ComponentStack_IsFunctionalChanged()
        {
            MyCharacter pilot = base.m_pilot;
            MyEntityController controller = base.ControllerInfo.Controller;
            base.ComponentStack_IsFunctionalChanged();
            if ((!base.IsFunctional && (pilot != null)) && (controller == null))
            {
                if (MySession.Static.CreativeMode)
                {
                    pilot.Close();
                }
                else
                {
                    pilot.DoDamage(1000f, MyDamageType.Destruction, false, 0L);
                }
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CryoChamber objectBuilderCubeBlock = (MyObjectBuilder_CryoChamber) base.GetObjectBuilderCubeBlock(copy);
            if (this.m_currentPlayerId != null)
            {
                objectBuilderCubeBlock.SteamId = new ulong?(this.m_currentPlayerId.Value.SteamId);
                objectBuilderCubeBlock.SerialId = new int?(this.m_currentPlayerId.Value.SerialId);
            }
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.m_characterDummy = Matrix.Identity;
            base.Init(objectBuilder, cubeGrid);
            if (base.ResourceSink != null)
            {
                base.ResourceSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, this.BlockDefinition.IdlePowerConsumption);
                base.ResourceSink.SetRequiredInputFuncByType(MyResourceDistributorComponent.ElectricityId, new Func<float>(this.CalculateRequiredPowerInput));
                base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            }
            else
            {
                MyResourceSinkComponent component = new MyResourceSinkComponent(1);
                component.Init(MyStringHash.GetOrCompute(this.BlockDefinition.ResourceSinkGroup), this.BlockDefinition.IdlePowerConsumption, new Func<float>(this.CalculateRequiredPowerInput));
                component.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
                base.ResourceSink = component;
            }
            MyObjectBuilder_CryoChamber chamber = objectBuilder as MyObjectBuilder_CryoChamber;
            if ((chamber.SteamId == null) || (chamber.SerialId == null))
            {
                this.m_currentPlayerId = null;
            }
            else
            {
                this.m_currentPlayerId = new MyPlayer.PlayerId(chamber.SteamId.Value, chamber.SerialId.Value);
            }
            string overlayTexture = this.BlockDefinition.OverlayTexture;
            if (!string.IsNullOrEmpty(overlayTexture))
            {
                this.m_overlayTextureName = overlayTexture;
            }
            base.HorizonIndicatorEnabled = false;
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        private bool IsLocalCharacterInside() => 
            ((MySession.Static.LocalCharacter != null) && ReferenceEquals(MySession.Static.LocalCharacter, this.Pilot));

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        protected override void OnControlAcquired_UpdateCamera()
        {
            base.OnControlAcquired_UpdateCamera();
        }

        protected void OnCryoChamberControlAcquired(MyEntityController controller)
        {
            this.m_currentPlayerId = new MyPlayer.PlayerId?(controller.Player.Id);
        }

        protected override void OnInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!this.Closed)
                {
                    this.<>n__0(resourceTypeId, oldInput, sink);
                }
            }, "MyCryoChamber::OnInputChanged");
        }

        internal void OnPlayerLoaded()
        {
        }

        public override void OnUnregisteredFromGridSystems()
        {
            MyCharacter pilot = base.m_pilot;
            MyEntityController controller = base.ControllerInfo.Controller;
            base.OnUnregisteredFromGridSystems();
            if (((pilot != null) && (controller == null)) && MySession.Static.CreativeMode)
            {
                pilot.Close();
            }
            base.m_soundEmitter.StopSound(true, true);
        }

        protected override void PlacePilotInSeat(MyCharacter pilot)
        {
            this.m_pilotLights = pilot.LightEnabled;
            if (Sync.IsServer)
            {
                pilot.EnableLights(false);
            }
            this.m_pilotCameraInFP = pilot.IsInFirstPersonView;
            MyCharacterJetpackComponent jetpackComp = pilot.JetpackComp;
            if (jetpackComp != null)
            {
                this.m_pilotJetpack = jetpackComp.TurnedOn;
                if (Sync.IsServer)
                {
                    jetpackComp.TurnOnJetpack(false, false, false);
                }
            }
            pilot.Sit(true, ReferenceEquals(MySession.Static.LocalCharacter, pilot), false, this.BlockDefinition.CharacterAnimation);
            pilot.SuitBattery.ResourceSource.Enabled = true;
            pilot.PositionComp.SetWorldMatrix(base.m_characterDummy * base.WorldMatrix, this, false, true, true, false, false, false);
            this.CheckEmissiveState(false);
        }

        private void PowerDistributor_PowerStateChaged(MyResourceStateEnum newState)
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    base.UpdateIsWorking();
                }
            }, "MyCryoChamber::UpdateIsWorking");
        }

        private void Receiver_IsPoweredChanged()
        {
            MySandboxGame.Static.Invoke(delegate {
                if (!base.Closed)
                {
                    base.UpdateIsWorking();
                }
            }, "MyCryoChamber::UpdateIsWorking");
        }

        protected override void RemovePilotFromSeat(MyCharacter pilot)
        {
            if (ReferenceEquals(pilot, MySession.Static.LocalCharacter))
            {
                MyHudCameraOverlay.Enabled = false;
                base.Render.Visible = true;
            }
            this.m_currentPlayerId = null;
            if (Sync.IsServer)
            {
                this.m_attachedPlayerId.Value = null;
                if (this.m_pilotLights)
                {
                    pilot.EnableLights(true);
                }
                if (this.m_pilotJetpack && (pilot.JetpackComp != null))
                {
                    pilot.JetpackComp.TurnOnJetpack(true, false, false);
                }
            }
            pilot.IsInFirstPersonView = this.m_pilotCameraInFP;
            this.m_pilotLights = false;
            this.m_pilotJetpack = false;
            this.m_pilotCameraInFP = true;
            this.CheckEmissiveState(false);
        }

        public override bool SetEmissiveStateWorking()
        {
            if (!base.IsWorking)
            {
                return false;
            }
            if (this.Pilot != null)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Alternative, base.Render.RenderObjectIDs[0], null);
            }
            if ((base.OxygenFillLevel > 0f) || MySession.Static.CreativeMode)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null);
            }
            return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Warning, base.Render.RenderObjectIDs[0], null);
        }

        private void SetOverlay()
        {
            if (this.IsLocalCharacterInside())
            {
                MyHudCameraOverlay.TextureName = this.m_overlayTextureName;
                MyHudCameraOverlay.Enabled = true;
                base.Render.Visible = false;
            }
        }

        public bool TryToControlPilot(MyPlayer player)
        {
            if (this.Pilot == null)
            {
                return false;
            }
            MyPlayer.PlayerId id = player.Id;
            MyPlayer.PlayerId? currentPlayerId = this.m_currentPlayerId;
            if ((currentPlayerId != null) ? (id != currentPlayerId.GetValueOrDefault()) : true)
            {
                return false;
            }
            currentPlayerId = this.m_attachedPlayerId.Value;
            MyPlayer.PlayerId? nullable2 = this.m_currentPlayerId;
            if (((currentPlayerId != null) == (nullable2 != null)) ? ((currentPlayerId != null) ? (currentPlayerId.GetValueOrDefault() == nullable2.GetValueOrDefault()) : true) : false)
            {
                this.AttachedPlayerChanged();
            }
            else
            {
                this.m_attachedPlayerId.Value = this.m_currentPlayerId;
            }
            return true;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            base.ResourceSink.Update();
            if (this.m_retryAttachPilot)
            {
                this.m_retryAttachPilot = false;
                this.AttachedPlayerChanged();
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (MyFakes.ENABLE_OXYGEN_SOUNDS)
            {
                this.UpdateSound((this.Pilot != null) && ReferenceEquals(this.Pilot, MySession.Static.LocalCharacter));
            }
        }

        public override void UpdateCockpitModel()
        {
            base.UpdateCockpitModel();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            base.UpdateIsWorking();
            this.CheckEmissiveState(false);
            if (Sync.IsServer)
            {
                MyPlayer.PlayerId? nullable = this.m_attachedPlayerId.Value;
                MyPlayer.PlayerId? currentPlayerId = this.m_currentPlayerId;
                if (((nullable != null) == (currentPlayerId != null)) ? ((nullable != null) ? (nullable.GetValueOrDefault() != currentPlayerId.GetValueOrDefault()) : false) : true)
                {
                    this.m_attachedPlayerId.Value = this.m_currentPlayerId;
                }
            }
        }

        private void UpdateSound(bool isUsed)
        {
            if (base.IsWorking)
            {
                bool? nullable;
                if (!isUsed)
                {
                    if ((base.m_soundEmitter.SoundId != this.BlockDefinition.OutsideSound.Arcade) && (base.m_soundEmitter.SoundId != this.BlockDefinition.OutsideSound.Realistic))
                    {
                        base.m_soundEmitter.Force2D = false;
                        base.m_soundEmitter.Force3D = true;
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.OutsideSound, true, false, false, false, false, nullable);
                    }
                }
                else if ((base.m_soundEmitter.SoundId != this.BlockDefinition.InsideSound.Arcade) && (base.m_soundEmitter.SoundId != this.BlockDefinition.InsideSound.Realistic))
                {
                    base.m_soundEmitter.Force2D = true;
                    base.m_soundEmitter.Force3D = false;
                    if ((base.m_soundEmitter.SoundId == this.BlockDefinition.OutsideSound.Arcade) || (base.m_soundEmitter.SoundId != this.BlockDefinition.OutsideSound.Realistic))
                    {
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.InsideSound, true, false, false, false, false, nullable);
                    }
                    else
                    {
                        nullable = null;
                        base.m_soundEmitter.PlaySound(this.BlockDefinition.InsideSound, true, true, false, false, false, nullable);
                    }
                }
            }
        }

        public override bool IsInFirstPersonView
        {
            get => 
                true;
            set
            {
            }
        }

        private MyCryoChamberDefinition BlockDefinition =>
            ((MyCryoChamberDefinition) base.BlockDefinition);

        public override MyToolbarType ToolbarType =>
            MyToolbarType.None;
    }
}

