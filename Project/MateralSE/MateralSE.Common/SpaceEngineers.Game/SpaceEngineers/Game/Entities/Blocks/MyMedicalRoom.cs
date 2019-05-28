namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_MedicalRoom)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyMedicalRoom), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyMedicalRoom) })]
    public class MyMedicalRoom : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMyMedicalRoom, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMyMedicalRoom, IMyLifeSupportingBlock, IMyRechargeSocketOwner, IMyGasBlock, IMyConveyorEndpointBlock, IMySpawnBlock
    {
        private static readonly string[] m_emissiveTextureNames = new string[] { "Emissive2", "Emissive3" };
        private bool m_healingAllowed;
        private bool m_refuelAllowed;
        private bool m_suitChangeAllowed;
        private bool m_customWardrobesEnabled;
        private bool m_forceSuitChangeOnRespawn;
        private bool m_spawnWithoutOxygenEnabled;
        private HashSet<string> m_customWardrobeNames = new HashSet<string>();
        private string m_respawnSuitName;
        private MySoundPair m_idleSound;
        private MySoundPair m_progressSound;
        private MyCharacter m_wardrobeUser;
        private MatrixD m_wardrobeUserSpectatorMatrix;
        private byte m_wardrobeUserAwayCounter;
        private MyLight m_light;
        private readonly MyEntity3DSoundEmitter m_idleSoundEmitter;
        private MyMedicalRoomDefinition m_medicalRoomDefinition;
        private MyLifeSupportingComponent m_lifeSupportingComponent;
        protected bool m_takeSpawneeOwnership;
        protected bool m_setFactionToSpawnee;
        private MyResourceSinkComponent m_sinkComponent;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private long m_wardrobeUserId;
        private List<MyTextPanelComponent> m_panels = new List<MyTextPanelComponent>();

        public MyMedicalRoom()
        {
            this.CreateTerminalControls();
            this.SpawnName = new StringBuilder();
            this.m_idleSoundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            base.Render = new MyRenderComponentScreenAreas(this);
        }

        public bool AllowSelfPulling() => 
            false;

        protected override bool CheckIsWorking() => 
            (this.SinkComp.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            this.StopIdleSound();
            MyLights.RemoveLight(this.m_light);
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            this.SinkComp.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.UpdateVisual();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyMedicalRoom>())
            {
                base.CreateTerminalControls();
                MyTerminalControlTextbox<MyMedicalRoom> textbox1 = new MyTerminalControlTextbox<MyMedicalRoom>("SpawnName", MySpaceTexts.MedicalRoom_SpawnNameLabel, MySpaceTexts.MedicalRoom_SpawnNameToolTip);
                MyTerminalControlTextbox<MyMedicalRoom> textbox2 = new MyTerminalControlTextbox<MyMedicalRoom>("SpawnName", MySpaceTexts.MedicalRoom_SpawnNameLabel, MySpaceTexts.MedicalRoom_SpawnNameToolTip);
                textbox2.Getter = x => x.SpawnName;
                MyTerminalControlTextbox<MyMedicalRoom> local18 = textbox2;
                MyTerminalControlTextbox<MyMedicalRoom> local19 = textbox2;
                local19.Setter = (x, v) => x.SetSpawnName(v);
                MyTerminalControlTextbox<MyMedicalRoom> control = local19;
                control.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MyMedicalRoom>(control);
                MyTerminalControlLabel<MyMedicalRoom> label = new MyTerminalControlLabel<MyMedicalRoom>(MySpaceTexts.TerminalScenarioSettingsLabel);
                MyStringId? on = null;
                on = null;
                MyTerminalControlCheckbox<MyMedicalRoom> checkbox3 = new MyTerminalControlCheckbox<MyMedicalRoom>("TakeOwnership", MySpaceTexts.MedicalRoom_ownershipAssignmentLabel, MySpaceTexts.MedicalRoom_ownershipAssignmentTooltip, on, on);
                MyTerminalControlCheckbox<MyMedicalRoom> checkbox4 = new MyTerminalControlCheckbox<MyMedicalRoom>("TakeOwnership", MySpaceTexts.MedicalRoom_ownershipAssignmentLabel, MySpaceTexts.MedicalRoom_ownershipAssignmentTooltip, on, on);
                checkbox4.Getter = x => x.m_takeSpawneeOwnership;
                MyTerminalControlCheckbox<MyMedicalRoom> local16 = checkbox4;
                MyTerminalControlCheckbox<MyMedicalRoom> local17 = checkbox4;
                local17.Setter = (x, val) => x.m_takeSpawneeOwnership = val;
                MyTerminalControlCheckbox<MyMedicalRoom> local14 = local17;
                MyTerminalControlCheckbox<MyMedicalRoom> local15 = local17;
                local15.Enabled = x => MySession.Static.Settings.ScenarioEditMode;
                MyTerminalControlFactory.AddControl<MyMedicalRoom>(label);
                MyTerminalControlFactory.AddControl<MyMedicalRoom>(local15);
                on = null;
                on = null;
                MyTerminalControlCheckbox<MyMedicalRoom> checkbox1 = new MyTerminalControlCheckbox<MyMedicalRoom>("SetFaction", MySpaceTexts.MedicalRoom_factionAssignmentLabel, MySpaceTexts.MedicalRoom_factionAssignmentTooltip, on, on);
                MyTerminalControlCheckbox<MyMedicalRoom> checkbox2 = new MyTerminalControlCheckbox<MyMedicalRoom>("SetFaction", MySpaceTexts.MedicalRoom_factionAssignmentLabel, MySpaceTexts.MedicalRoom_factionAssignmentTooltip, on, on);
                checkbox2.Getter = x => x.m_setFactionToSpawnee;
                MyTerminalControlCheckbox<MyMedicalRoom> local12 = checkbox2;
                MyTerminalControlCheckbox<MyMedicalRoom> local13 = checkbox2;
                local13.Setter = (x, val) => x.m_setFactionToSpawnee = val;
                MyTerminalControlCheckbox<MyMedicalRoom> local10 = local13;
                MyTerminalControlCheckbox<MyMedicalRoom> local11 = local13;
                local11.Enabled = x => MySession.Static.Settings.ScenarioEditMode;
                MyTerminalControlFactory.AddControl<MyMedicalRoom>(local11);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_MedicalRoom objectBuilderCubeBlock = (MyObjectBuilder_MedicalRoom) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.SpawnName = this.SpawnName.ToString();
            objectBuilderCubeBlock.SteamUserId = this.SteamUserId;
            objectBuilderCubeBlock.IdleSound = this.m_idleSound.ToString();
            objectBuilderCubeBlock.ProgressSound = this.m_progressSound.ToString();
            objectBuilderCubeBlock.TakeOwnership = this.m_takeSpawneeOwnership;
            objectBuilderCubeBlock.SetFaction = this.m_setFactionToSpawnee;
            if (this.m_wardrobeUser != null)
            {
                objectBuilderCubeBlock.WardrobeUserId = this.m_wardrobeUser.EntityId;
            }
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyStringHash orCompute;
            this.m_medicalRoomDefinition = base.BlockDefinition as MyMedicalRoomDefinition;
            if (this.m_medicalRoomDefinition != null)
            {
                this.m_idleSound = new MySoundPair(this.m_medicalRoomDefinition.IdleSound, true);
                this.m_progressSound = new MySoundPair(this.m_medicalRoomDefinition.ProgressSound, true);
                orCompute = MyStringHash.GetOrCompute(this.m_medicalRoomDefinition.ResourceSinkGroup);
            }
            else
            {
                this.m_idleSound = new MySoundPair("BlockMedical", true);
                this.m_progressSound = new MySoundPair("BlockMedicalProgress", true);
                orCompute = MyStringHash.GetOrCompute("Utility");
            }
            this.SinkComp = new MyResourceSinkComponent(1);
            this.SinkComp.Init(orCompute, 0.002f, delegate {
                if (!base.Enabled || !base.IsFunctional)
                {
                    return 0f;
                }
                return this.SinkComp.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            this.SinkComp.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.Init(objectBuilder, cubeGrid);
            this.m_lifeSupportingComponent = new MyLifeSupportingComponent(this, this.m_progressSound, "MedRoomHeal", 5f);
            base.Components.Add<MyLifeSupportingComponent>(this.m_lifeSupportingComponent);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            MyObjectBuilder_MedicalRoom room = objectBuilder as MyObjectBuilder_MedicalRoom;
            this.SpawnName.Clear();
            if (room.SpawnName != null)
            {
                this.SpawnName.Append(room.SpawnName);
            }
            this.SteamUserId = room.SteamUserId;
            if (this.SteamUserId != 0)
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(this.SteamUserId));
                if (playerById != null)
                {
                    base.IDModule.Owner = playerById.Identity.IdentityId;
                    base.IDModule.ShareMode = MyOwnershipShareModeEnum.Faction;
                }
            }
            this.SteamUserId = 0L;
            this.m_takeSpawneeOwnership = room.TakeOwnership;
            this.m_setFactionToSpawnee = room.SetFaction;
            this.m_wardrobeUserId = room.WardrobeUserId;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            this.InitializeConveyorEndpoint();
            this.SinkComp.Update();
            base.Components.Remove<MyRespawnComponent>();
            if (base.CubeGrid.CreatePhysics)
            {
                base.Components.Add<MyEntityRespawnComponentBase>(new MyRespawnComponent());
            }
            this.m_healingAllowed = this.m_medicalRoomDefinition.HealingAllowed;
            this.m_refuelAllowed = this.m_medicalRoomDefinition.RefuelAllowed;
            this.m_suitChangeAllowed = this.m_medicalRoomDefinition.SuitChangeAllowed;
            this.m_customWardrobesEnabled = this.m_medicalRoomDefinition.CustomWardrobesEnabled;
            this.m_forceSuitChangeOnRespawn = this.m_medicalRoomDefinition.ForceSuitChangeOnRespawn;
            this.m_customWardrobeNames = this.m_medicalRoomDefinition.CustomWardrobeNames;
            this.m_respawnSuitName = this.m_medicalRoomDefinition.RespawnSuitName;
            this.m_spawnWithoutOxygenEnabled = this.m_medicalRoomDefinition.SpawnWithoutOxygenEnabled;
            this.RespawnAllowed = this.m_medicalRoomDefinition.RespawnAllowed;
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.m_light.Start((Vector4) Color.White, 2f, "Med bay light");
                this.m_light.Falloff = 1.3f;
                this.m_light.LightOn = false;
                this.m_light.UpdateLight();
            }
            if ((this.m_medicalRoomDefinition.ScreenAreas != null) && (this.m_medicalRoomDefinition.ScreenAreas.Count > 0))
            {
                this.m_panels = new List<MyTextPanelComponent>();
                for (int i = 0; i < this.m_medicalRoomDefinition.ScreenAreas.Count; i++)
                {
                    MyTextPanelComponent item = new MyTextPanelComponent(i, this, this.m_medicalRoomDefinition.ScreenAreas[i].Name, this.m_medicalRoomDefinition.ScreenAreas[i].DisplayName, this.m_medicalRoomDefinition.ScreenAreas[i].TextureResolution, 1, 1, true);
                    this.m_panels.Add(item);
                    base.SyncType.Append(item);
                    item.Init(null, null, null);
                }
            }
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if (this.m_panels.Count > 0)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            foreach (MyTextPanelComponent component in this.m_panels)
            {
                component.SetRender((MyRenderComponentScreenAreas) base.Render);
                ((MyRenderComponentScreenAreas) base.Render).AddScreenArea(base.Render.RenderObjectIDs, component.Name);
            }
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            this.SinkComp.Update();
        }

        protected override void OnStartWorking()
        {
            this.StartIdleSound();
            this.UpdateEmissivity();
        }

        protected override void OnStopWorking()
        {
            this.StopIdleSound();
            this.UpdateEmissivity();
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        [Event(null, 690), Reliable, Server(ValidationType.Access), Broadcast]
        private void RequestSupport(long userId)
        {
            if (base.GetUserRelationToOwner(MySession.Static.Players.TryGetIdentityId(MyEventContext.Current.Sender.Value, 0)).IsFriendly() || MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                MyCharacter character;
                Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCharacter>(userId, out character, false);
                if (character != null)
                {
                    this.m_lifeSupportingComponent.ProvideSupport(character);
                }
            }
        }

        bool IMyGasBlock.IsWorking() => 
            base.IsWorking;

        void IMyLifeSupportingBlock.BroadcastSupportRequest(MyCharacter user)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMedicalRoom, long>(this, x => new Action<long>(x.RequestSupport), user.EntityId, targetEndpoint);
        }

        void IMyLifeSupportingBlock.ShowTerminal(MyCharacter user)
        {
            MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this);
        }

        private void SetEmissive(Color color, int index, float emissivity = 1f)
        {
            if ((base.Render.RenderObjectIDs[0] != uint.MaxValue) && (m_emissiveTextureNames.Length > index))
            {
                UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], m_emissiveTextureNames[index], color, emissivity);
            }
        }

        private void SetSpawnName(StringBuilder text)
        {
            if (this.SpawnName.CompareUpdate(text))
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMedicalRoom, string>(this, x => new Action<string>(x.SetSpawnTextEvent), text.ToString(), targetEndpoint);
            }
        }

        [Event(null, 170), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        protected void SetSpawnTextEvent(string text)
        {
            this.SpawnName.CompareUpdate(text);
        }

        private unsafe void SetSpectatorCamera()
        {
            MatrixD worldMatrix = base.WorldMatrix;
            float num = 70f / MySector.MainCamera.FieldOfViewDegrees;
            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
            xdPtr1.Translation += ((base.WorldMatrix.Right * (-1.1499999761581421 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.X)) + (base.WorldMatrix.Up * (0.699999988079071 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.Y))) + ((base.WorldMatrix.Forward * (1.5 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.Z)) * num);
            worldMatrix.Left = base.WorldMatrix.Right;
            worldMatrix.Forward = base.WorldMatrix.Backward;
            if (this.m_light != null)
            {
                Vector3D vectord = ((base.WorldMatrix.Translation + (base.WorldMatrix.Right * (-0.5 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.X))) + (base.WorldMatrix.Up * (1.0 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.Y))) + ((base.WorldMatrix.Forward * (0.60000002384185791 + this.m_medicalRoomDefinition.WardrobeCharacterOffset.Z)) * num);
                this.m_light.Position = vectord;
                this.m_light.UpdateLight();
            }
            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, new Vector3D?(worldMatrix.Translation));
            MySpectatorCameraController.Static.SetTarget(worldMatrix.Translation + worldMatrix.Forward, new Vector3D?(worldMatrix.Up));
            if (!MySpectatorCameraController.Static.IsLightOn)
            {
                MySpectatorCameraController.Static.SwitchLight();
            }
        }

        private void StartIdleSound()
        {
            bool? nullable = null;
            this.m_idleSoundEmitter.PlaySound(this.m_idleSound, true, false, false, false, false, nullable);
        }

        private void StopIdleSound()
        {
            this.m_idleSoundEmitter.StopSound(false, true);
        }

        public void StopUsingWardrobe()
        {
            if (this.m_wardrobeUser != null)
            {
                this.m_wardrobeUser.UpdateRotationsOverride = true;
                this.m_wardrobeUserAwayCounter = 0;
                if (MyGuiScreenGamePlay.ActiveGameplayScreen is MyGuiScreenLoadInventory)
                {
                    MyGuiScreenGamePlay.ActiveGameplayScreen.CloseScreen();
                }
                MySpectatorCameraController.Static.SetViewMatrix(this.m_wardrobeUserSpectatorMatrix);
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this.m_wardrobeUser, position);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMedicalRoom>(this, x => new Action(x.StopUsingWardrobeSync), targetEndpoint);
                if (!base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
            }
        }

        [Event(null, 0x278), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private unsafe void StopUsingWardrobeSync()
        {
            if (this.m_wardrobeUser != null)
            {
                MatrixD spawnPosition;
                MyRespawnComponent component = base.Components.Get<MyEntityRespawnComponentBase>() as MyRespawnComponent;
                if (component != null)
                {
                    spawnPosition = component.GetSpawnPosition();
                }
                else
                {
                    spawnPosition = base.WorldMatrix;
                    MatrixD* xdPtr1 = (MatrixD*) ref spawnPosition;
                    xdPtr1.Translation += spawnPosition.Forward * base.BlockDefinition.Size.AbsMax();
                }
                if (Sync.IsServer)
                {
                    this.m_wardrobeUser.PositionComp.SetWorldMatrix(spawnPosition, null, false, true, true, false, false, false);
                }
                else if (ReferenceEquals(this.m_wardrobeUser, MySession.Static.LocalCharacter))
                {
                    this.m_wardrobeUser.ForceDisablePrediction = false;
                    this.m_wardrobeUser.UpdateCharacterPhysics(false);
                    this.m_wardrobeUser.PositionComp.SetWorldMatrix(spawnPosition, null, false, true, true, false, false, false);
                }
            }
            this.m_wardrobeUser = null;
            if (this.m_light != null)
            {
                this.m_light.LightOn = false;
                this.m_light.UpdateLight();
            }
            this.UpdateEmissivity();
        }

        public void TrySetFaction(MyPlayer player)
        {
            if ((MySession.Static.IsScenario && (this.m_setFactionToSpawnee && Sync.IsServer)) && (base.OwnerId != 0))
            {
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(base.OwnerId);
                if (faction != null)
                {
                    MyFactionCollection.SendJoinRequest(faction.FactionId, player.Identity.IdentityId);
                    if (!faction.AutoAcceptMember)
                    {
                        MyFactionCollection.AcceptJoin(faction.FactionId, player.Identity.IdentityId);
                    }
                }
            }
        }

        public void TryTakeSpawneeOwnership(MyPlayer player)
        {
            if ((MySession.Static.IsScenario && (this.m_takeSpawneeOwnership && Sync.IsServer)) && (base.OwnerId == 0))
            {
                base.ChangeBlockOwnerRequest(player.Identity.IdentityId, MyOwnershipShareModeEnum.None);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if ((this.m_wardrobeUser != null) && ReferenceEquals(this.m_wardrobeUser, MySession.Static.LocalCharacter))
            {
                this.SetSpectatorCamera();
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (this.m_wardrobeUser == null)
            {
                if (this.m_wardrobeUserId != 0)
                {
                    MyCharacter character;
                    Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCharacter>(this.m_wardrobeUserId, out character, false);
                    if (character != null)
                    {
                        this.m_wardrobeUser = character;
                        this.m_wardrobeUserId = 0L;
                    }
                }
            }
            else if (!ReferenceEquals(this.m_wardrobeUser, MySession.Static.LocalCharacter))
            {
                if (Sync.IsServer && (this.m_wardrobeUser.ControllerInfo.Controller == null))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMedicalRoom>(this, x => new Action(x.StopUsingWardrobeSync), targetEndpoint);
                }
            }
            else
            {
                double num = Math.Abs((double) (Vector3D.Distance(this.m_wardrobeUser.PositionComp.GetPosition(), base.PositionComp.GetPosition()) - this.m_medicalRoomDefinition.WardrobeCharacterOffsetLength));
                if (!this.m_wardrobeUser.IsDead && (num <= 0.5))
                {
                    this.m_wardrobeUserAwayCounter = 0;
                }
                else
                {
                    this.m_wardrobeUserAwayCounter = (byte) (this.m_wardrobeUserAwayCounter + 1);
                    if (this.m_wardrobeUserAwayCounter > 6)
                    {
                        this.StopUsingWardrobe();
                    }
                }
                if (!base.IsFunctional || !base.IsWorking)
                {
                    this.StopUsingWardrobe();
                    this.m_wardrobeUser = null;
                }
            }
            this.m_lifeSupportingComponent.Update10();
        }

        private void UpdateEmissivity()
        {
            if (!base.IsFunctional)
            {
                this.SetEmissive(Color.Black, 0, 0f);
                this.SetEmissive(Color.Black, 1, 0f);
            }
            else if (!base.IsWorking)
            {
                this.SetEmissive(Color.Red, 0, 1f);
                this.SetEmissive(Color.Red, 1, 1f);
            }
            else
            {
                this.SetEmissive(Color.Green, 0, 1f);
                if (this.m_wardrobeUser != null)
                {
                    this.SetEmissive(Color.Cyan, 0, 1f);
                    this.SetEmissive(Color.White, 1, 1f);
                }
                else
                {
                    this.SetEmissive(Color.Green, 0, 1f);
                    this.SetEmissive(Color.White, 1, 0f);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateScreen();
            this.UpdateEmissivity();
        }

        public void UpdateScreen()
        {
            if (!this.CheckIsWorking())
            {
                for (int i = 0; i < this.m_panels.Count; i++)
                {
                    ((MyRenderComponentScreenAreas) base.Render).ChangeTexture(i, this.m_panels[i].GetPathForID("Offline"));
                }
            }
            else
            {
                for (int i = 0; i < this.m_panels.Count; i++)
                {
                    ((MyRenderComponentScreenAreas) base.Render).ChangeTexture(i, null);
                }
            }
        }

        public override void UpdateSoundEmitters()
        {
            base.UpdateSoundEmitters();
            if (this.m_idleSoundEmitter != null)
            {
                this.m_idleSoundEmitter.Update();
            }
            this.m_lifeSupportingComponent.UpdateSoundEmitters();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public void UseWardrobe(MyCharacter user)
        {
            this.m_wardrobeUserSpectatorMatrix = MySpectatorCameraController.Static.GetViewMatrix();
            user.UpdateRotationsOverride = true;
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyMedicalRoom, long>(this, x => new Action<long>(x.UseWardrobeSync), user.EntityId, targetEndpoint);
        }

        [Event(null, 0x20c), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private unsafe void UseWardrobeSync(long userId)
        {
            MyCharacter character;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCharacter>(userId, out character, false);
            if (character != null)
            {
                this.m_wardrobeUser = character;
                MatrixD worldMatrix = base.WorldMatrix;
                if (!base.Model.Dummies.ContainsKey("detector_wardrobe"))
                {
                    MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
                    xdPtr2.Translation += ((base.WorldMatrix.Right * this.m_medicalRoomDefinition.WardrobeCharacterOffset.X) + (base.WorldMatrix.Up * this.m_medicalRoomDefinition.WardrobeCharacterOffset.Y)) + (base.WorldMatrix.Forward * this.m_medicalRoomDefinition.WardrobeCharacterOffset.Z);
                }
                else
                {
                    worldMatrix = MatrixD.Multiply(MatrixD.Normalize(base.Model.Dummies["detector_wardrobe"].Matrix), base.WorldMatrix);
                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                    xdPtr1.Translation -= base.WorldMatrix.Up * 0.98;
                }
                if (Sync.IsServer)
                {
                    character.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                }
                if (ReferenceEquals(character, MySession.Static.LocalCharacter))
                {
                    if (character.JetpackRunning)
                    {
                        character.SwitchJetpack();
                    }
                    character.ForceDisablePrediction = true;
                    character.UpdateCharacterPhysics(false);
                    character.PositionComp.SetPosition(worldMatrix.Translation, null, false, true);
                    character.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                }
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                if (this.m_light != null)
                {
                    this.m_light.LightOn = true;
                    this.m_light.UpdateLight();
                }
                this.UpdateEmissivity();
            }
        }

        public bool SetFactionToSpawnee =>
            this.m_setFactionToSpawnee;

        public MyResourceSinkComponent SinkComp
        {
            get => 
                this.m_sinkComponent;
            set
            {
                if (base.Components.Contains(typeof(MyResourceSinkComponent)))
                {
                    base.Components.Remove<MyResourceSinkComponent>();
                }
                base.Components.Add<MyResourceSinkComponent>(value);
                this.m_sinkComponent = value;
            }
        }

        private ulong SteamUserId { get; set; }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        public bool CanPressurizeRoom =>
            false;

        public bool HealingAllowed
        {
            get => 
                this.m_healingAllowed;
            set => 
                (this.m_healingAllowed = value);
        }

        public bool RefuelAllowed
        {
            get => 
                this.m_refuelAllowed;
            set => 
                (this.m_refuelAllowed = value);
        }

        public bool RespawnAllowed
        {
            get => 
                (base.Components.Get<MyEntityRespawnComponentBase>() != null);
            set
            {
                if (!value)
                {
                    base.Components.Remove<MyEntityRespawnComponentBase>();
                }
                else if (base.Components.Get<MyEntityRespawnComponentBase>() == null)
                {
                    base.Components.Add<MyEntityRespawnComponentBase>(new MyRespawnComponent());
                }
            }
        }

        public StringBuilder SpawnName { get; private set; }

        string IMySpawnBlock.SpawnName =>
            this.SpawnName.ToString();

        public bool SuitChangeAllowed
        {
            get => 
                (this.m_suitChangeAllowed && ReferenceEquals(this.m_wardrobeUser, null));
            set => 
                (this.m_suitChangeAllowed = value);
        }

        public bool CustomWardrobesEnabled
        {
            get => 
                this.m_customWardrobesEnabled;
            set => 
                (this.m_customWardrobesEnabled = value);
        }

        public HashSet<string> CustomWardrobeNames
        {
            get => 
                this.m_customWardrobeNames;
            set => 
                (this.m_customWardrobeNames = value);
        }

        public bool ForceSuitChangeOnRespawn
        {
            get => 
                this.m_forceSuitChangeOnRespawn;
            set
            {
                if (value && (this.m_respawnSuitName != null))
                {
                    this.m_forceSuitChangeOnRespawn = value;
                }
            }
        }

        public string RespawnSuitName
        {
            get => 
                this.m_respawnSuitName;
            set => 
                (this.m_respawnSuitName = value);
        }

        public bool SpawnWithoutOxygenEnabled
        {
            get => 
                this.m_spawnWithoutOxygenEnabled;
            set => 
                (this.m_spawnWithoutOxygenEnabled = value);
        }

        public bool IsGridPreview =>
            ((base.CubeGrid != null) && (base.CubeGrid.IsPreview || (base.CubeGrid.Projector != null)));

        MyLifeSupportingBlockType IMyLifeSupportingBlock.BlockType =>
            MyLifeSupportingBlockType.MedicalRoom;

        MyRechargeSocket IMyRechargeSocketOwner.RechargeSocket =>
            this.m_lifeSupportingComponent.RechargeSocket;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMedicalRoom.<>c <>9 = new MyMedicalRoom.<>c();
            public static Func<MyMedicalRoom, Action<string>> <>9__54_0;
            public static MyTerminalControlTextbox<MyMedicalRoom>.GetterDelegate <>9__77_0;
            public static MyTerminalControlTextbox<MyMedicalRoom>.SetterDelegate <>9__77_1;
            public static MyTerminalValueControl<MyMedicalRoom, bool>.GetterDelegate <>9__77_2;
            public static MyTerminalValueControl<MyMedicalRoom, bool>.SetterDelegate <>9__77_3;
            public static Func<MyMedicalRoom, bool> <>9__77_4;
            public static MyTerminalValueControl<MyMedicalRoom, bool>.GetterDelegate <>9__77_5;
            public static MyTerminalValueControl<MyMedicalRoom, bool>.SetterDelegate <>9__77_6;
            public static Func<MyMedicalRoom, bool> <>9__77_7;
            public static Func<MyMedicalRoom, Action> <>9__88_0;
            public static Func<MyMedicalRoom, Action<long>> <>9__89_0;
            public static Func<MyMedicalRoom, Action> <>9__94_0;
            public static Func<MyMedicalRoom, Action<long>> <>9__97_0;

            internal StringBuilder <CreateTerminalControls>b__77_0(MyMedicalRoom x) => 
                x.SpawnName;

            internal void <CreateTerminalControls>b__77_1(MyMedicalRoom x, StringBuilder v)
            {
                x.SetSpawnName(v);
            }

            internal bool <CreateTerminalControls>b__77_2(MyMedicalRoom x) => 
                x.m_takeSpawneeOwnership;

            internal void <CreateTerminalControls>b__77_3(MyMedicalRoom x, bool val)
            {
                x.m_takeSpawneeOwnership = val;
            }

            internal bool <CreateTerminalControls>b__77_4(MyMedicalRoom x) => 
                MySession.Static.Settings.ScenarioEditMode;

            internal bool <CreateTerminalControls>b__77_5(MyMedicalRoom x) => 
                x.m_setFactionToSpawnee;

            internal void <CreateTerminalControls>b__77_6(MyMedicalRoom x, bool val)
            {
                x.m_setFactionToSpawnee = val;
            }

            internal bool <CreateTerminalControls>b__77_7(MyMedicalRoom x) => 
                MySession.Static.Settings.ScenarioEditMode;

            internal Action<long> <Sandbox.Game.GameSystems.IMyLifeSupportingBlock.BroadcastSupportRequest>b__97_0(MyMedicalRoom x) => 
                new Action<long>(x.RequestSupport);

            internal Action<string> <SetSpawnName>b__54_0(MyMedicalRoom x) => 
                new Action<string>(x.SetSpawnTextEvent);

            internal Action <StopUsingWardrobe>b__94_0(MyMedicalRoom x) => 
                new Action(x.StopUsingWardrobeSync);

            internal Action <UpdateBeforeSimulation10>b__88_0(MyMedicalRoom x) => 
                new Action(x.StopUsingWardrobeSync);

            internal Action<long> <UseWardrobe>b__89_0(MyMedicalRoom x) => 
                new Action<long>(x.UseWardrobeSync);
        }
    }
}

