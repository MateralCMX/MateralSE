namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.UseObject;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Models;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    [MyCubeBlockType(typeof(MyObjectBuilder_Cockpit)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyCockpit), typeof(Sandbox.ModAPI.Ingame.IMyCockpit) })]
    public class MyCockpit : MyShipController, IMyCameraController, IMyUsableEntity, Sandbox.ModAPI.IMyCockpit, Sandbox.ModAPI.IMyShipController, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyShipController, VRage.Game.ModAPI.Interfaces.IMyControllableEntity, Sandbox.ModAPI.Ingame.IMyCockpit, Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider, Sandbox.ModAPI.IMyTextSurfaceProvider, IMyConveyorEndpointBlock, IMyGasBlock, IMyMultiTextPanelComponentOwner, IMyTextPanelComponentOwner
    {
        private readonly float DEFAULT_FPS_CAMERA_X_ANGLE = -10f;
        public static float MAX_SHAKE_DAMAGE = 500f;
        public const double MAX_DRAW_DISTANCE = 200.0;
        private bool m_isLargeCockpit;
        private Vector3 m_playerHeadSpring;
        private Vector3 m_playerHeadShakeDir;
        protected float MinHeadLocalXAngle = -60f;
        protected float MaxHeadLocalXAngle = 70f;
        protected float MinHeadLocalYAngle = -90f;
        protected float MaxHeadLocalYAngle = 90f;
        private MatrixD m_cameraDummy = MatrixD.Identity;
        protected MatrixD m_characterDummy = MatrixD.Identity;
        protected MyCharacter m_pilot;
        private MyCharacter m_savedPilot;
        private long m_serverSidePilotId;
        private Matrix? m_pilotRelativeWorld;
        private MyAutopilotBase m_aiPilot;
        protected MyDefinitionId? m_pilotGunDefinition;
        private bool m_updateSink;
        private float m_headLocalXAngle;
        private float m_headLocalYAngle;
        private long m_lastGasInputUpdateTick;
        private string m_cockpitInteriorModel;
        private string m_cockpitGlassModel;
        private bool m_defferAttach;
        private bool m_playIdleSound;
        private float m_currentCameraShakePower;
        private bool? m_lastNearFlag;
        private int m_forcedFpsTimeoutMs;
        private const int m_forcedFpsTimeoutDefaultMs = 500;
        private float MIN_SHAKE_ACC = 1f;
        private float MAX_SHAKE_ACC = 10f;
        private float MAX_SHAKE = 0.5f;
        protected Action<VRage.Game.Entity.MyEntity> m_pilotClosedHandler;
        private bool? m_pilotJetpackEnabledBackup;
        public float GlassDirt = 1f;
        private MyMultiTextPanelComponent m_multiPanel;
        private MyGuiScreenTextPanel m_textBox;
        private bool m_isInFirstPersonView = true;
        private bool m_wasCameraForced;
        private MyMultilineConveyorEndpoint m_conveyorEndpoint;
        private readonly VRage.Sync.Sync<float, SyncDirection.FromServer> m_oxygenFillLevel;
        private bool m_retryAttachPilot;
        private bool m_pilotFirstPerson;
        private bool m_isTextPanelOpen;
        private readonly Vector3I[] m_neighbourPositions;
        private static readonly MyDefinitionId[] m_forgetTheseWeapons = new MyDefinitionId[] { new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer)) };

        public MyCockpit()
        {
            Vector3I[] vectoriArray1 = new Vector3I[0x1a];
            vectoriArray1[0] = new Vector3I(1, 0, 0);
            vectoriArray1[1] = new Vector3I(-1, 0, 0);
            vectoriArray1[2] = new Vector3I(0, 0, -1);
            vectoriArray1[3] = new Vector3I(0, 0, 1);
            vectoriArray1[4] = new Vector3I(0, 1, 0);
            vectoriArray1[5] = new Vector3I(0, -1, 0);
            vectoriArray1[6] = new Vector3I(1, 1, 0);
            vectoriArray1[7] = new Vector3I(-1, 1, 0);
            vectoriArray1[8] = new Vector3I(1, -1, 0);
            vectoriArray1[9] = new Vector3I(-1, -1, 0);
            vectoriArray1[10] = new Vector3I(1, 1, -1);
            vectoriArray1[11] = new Vector3I(-1, 1, -1);
            vectoriArray1[12] = new Vector3I(1, -1, -1);
            vectoriArray1[13] = new Vector3I(-1, -1, -1);
            vectoriArray1[14] = new Vector3I(1, 0, -1);
            vectoriArray1[15] = new Vector3I(-1, 0, -1);
            vectoriArray1[0x10] = new Vector3I(0, 1, -1);
            vectoriArray1[0x11] = new Vector3I(0, -1, -1);
            vectoriArray1[0x12] = new Vector3I(1, 1, 1);
            vectoriArray1[0x13] = new Vector3I(-1, 1, 1);
            vectoriArray1[20] = new Vector3I(1, -1, 1);
            vectoriArray1[0x15] = new Vector3I(-1, -1, 1);
            vectoriArray1[0x16] = new Vector3I(1, 0, 1);
            vectoriArray1[0x17] = new Vector3I(-1, 0, 1);
            vectoriArray1[0x18] = new Vector3I(0, 1, 1);
            vectoriArray1[0x19] = new Vector3I(0, -1, 1);
            this.m_neighbourPositions = vectoriArray1;
            this.m_pilotClosedHandler = new Action<VRage.Game.Entity.MyEntity>(this.m_pilot_OnMarkForClose);
            base.ResourceSink = new MyResourceSinkComponent(2);
            base.Render = new MyRenderComponentCockpit(this);
            base.m_soundEmitter.EmitterMethods[1].Add(new Func<bool>(this.ShouldPlay2D));
        }

        public void AddShake(float shakePower)
        {
            this.m_currentCameraShakePower += shakePower;
            this.m_currentCameraShakePower = Math.Min(this.m_currentCameraShakePower, MySector.MainCamera.CameraShake.MaxShake);
        }

        public bool AllowSelfPulling() => 
            false;

        public void AttachAutopilot(MyAutopilotBase newAutopilot, bool updateSync = true)
        {
            this.RemoveAutopilot(false);
            this.m_aiPilot = newAutopilot;
            this.m_aiPilot.OnAttachedToShipController(this);
            if (updateSync && Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, MyObjectBuilder_AutopilotBase>(this, x => new Action<MyObjectBuilder_AutopilotBase>(x.SetAutopilot_Client), newAutopilot.GetObjectBuilder(), targetEndpoint);
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
        }

        public void AttachPilot(MyCharacter pilot, bool storeOriginalPilotWorld = true, bool calledFromInit = false, bool merged = false)
        {
            if (Sandbox.Game.Entities.MyEntities.IsInsideWorld(pilot.PositionComp.GetPosition()) && Sandbox.Game.Entities.MyEntities.IsInsideWorld(base.PositionComp.GetPosition()))
            {
                MyAnalyticsHelper.ReportActivityStart(pilot, "cockpit", "cockpit", string.Empty, string.Empty, true);
                long playerIdentityId = pilot.GetPlayerIdentityId();
                this.m_pilot = pilot;
                this.m_pilot.OnMarkForClose += this.m_pilotClosedHandler;
                this.m_pilot.IsUsing = this;
                this.m_pilot.ResetHeadRotation();
                bool flag1 = !merged;
                if (flag1)
                {
                    if (storeOriginalPilotWorld)
                    {
                        this.m_pilotRelativeWorld = new Matrix?((Matrix) MatrixD.Multiply(pilot.WorldMatrix, base.PositionComp.WorldMatrixNormalizedInv));
                    }
                    else if (!calledFromInit)
                    {
                        this.m_pilotRelativeWorld = null;
                    }
                }
                if (pilot.InScene)
                {
                    Sandbox.Game.Entities.MyEntities.Remove(pilot);
                }
                this.m_pilot.Physics.Enabled = false;
                this.m_pilot.PositionComp.SetWorldMatrix(base.WorldMatrix, this, false, true, true, false, false, true);
                this.m_pilot.Physics.Clear();
                if (!base.Hierarchy.Children.Any<MyHierarchyComponentBase>(x => ReferenceEquals(x.Entity, this.m_pilot)))
                {
                    base.Hierarchy.AddChild(this.m_pilot, true, true);
                }
                base.NeedsWorldMatrix = true;
                bool local1 = flag1;
                if (local1)
                {
                    if (!(this.m_pilot.CurrentWeapon is VRage.Game.Entity.MyEntity) || m_forgetTheseWeapons.Contains<MyDefinitionId>(this.m_pilot.CurrentWeapon.DefinitionId))
                    {
                        this.m_pilotGunDefinition = null;
                    }
                    else
                    {
                        this.m_pilotGunDefinition = new MyDefinitionId?(this.m_pilot.CurrentWeapon.DefinitionId);
                    }
                    this.m_pilotFirstPerson = pilot.IsInFirstPersonView;
                }
                this.PlacePilotInSeat(pilot);
                this.m_pilot.SuitBattery.ResourceSink.TemporaryConnectedEntity = this;
                base.m_rechargeSocket.PlugIn(this.m_pilot.SuitBattery.ResourceSink);
                if (pilot.ControllerInfo.Controller != null)
                {
                    Sync.Players.SetPlayerToCockpit(pilot.ControllerInfo.Controller.Player, this);
                }
                if (!calledFromInit)
                {
                    this.GiveControlToPilot();
                    uint? inventoryItemId = null;
                    this.m_pilot.SwitchToWeapon(null, inventoryItemId, false);
                }
                if (Sync.IsServer)
                {
                    this.m_serverSidePilotId = this.m_pilot.EntityId;
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, long, Matrix?>(this, x => new Action<long, Matrix?>(this.NotifyClientPilotChanged), this.m_serverSidePilotId, this.m_pilotRelativeWorld, targetEndpoint);
                }
                if (local1)
                {
                    MyCharacterJetpackComponent jetpackComp = this.m_pilot.JetpackComp;
                    if ((jetpackComp != null) && !calledFromInit)
                    {
                        this.m_pilotJetpackEnabledBackup = new bool?(jetpackComp.TurnedOn);
                    }
                }
                if (this.m_pilot.JetpackComp != null)
                {
                    this.m_pilot.JetpackComp.TurnOnJetpack(false, false, false);
                }
                base.m_lastPilot = pilot;
                if ((!ReferenceEquals(base.GetInCockpitSound, MySoundPair.Empty) && !calledFromInit) && !merged)
                {
                    base.PlayUseSound(true);
                }
                this.m_playIdleSound = true;
                if (ReferenceEquals(pilot, MySession.Static.LocalCharacter) && !MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning)
                {
                    Vector3D? nullable2;
                    if (calledFromInit && ((MySession.Static.CameraController == null) || ReferenceEquals(MySession.Static.CameraController, this)))
                    {
                        nullable2 = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this, nullable2);
                    }
                    else if (ReferenceEquals(MySession.Static.CameraController, pilot) && ReferenceEquals(pilot, MySession.Static.LocalCharacter))
                    {
                        nullable2 = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this, nullable2);
                    }
                }
                if ((MyVisualScriptLogicProvider.PlayerEnteredCockpit != null) && (playerIdentityId != -1L))
                {
                    MyVisualScriptLogicProvider.PlayerEnteredCockpit(base.Name, playerIdentityId, base.CubeGrid.Name);
                }
                if (ReferenceEquals(this.m_pilot, MySession.Static.LocalCharacter))
                {
                    MyLocalCache.LoadInventoryConfig(pilot, false);
                }
                base.CubeGrid.GridSystems.RadioSystem.Register(this.m_pilot.RadioBroadcaster);
                base.CubeGrid.GridSystems.RadioSystem.Register(this.m_pilot.RadioReceiver);
                MyIdentity identity = pilot.GetIdentity();
                if (identity != null)
                {
                    identity.FactionChanged += new Action<MyFaction, MyFaction>(this.OnCharacterFactionChanged);
                }
            }
        }

        [Event(null, 0x87f), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        public void AttachPilotEvent(UseActionEnum actionEnum, long characterID)
        {
            VRage.Game.Entity.MyEntity entity2;
            IMyUsableEntity entity = this;
            bool flag1 = Sandbox.Game.Entities.MyEntities.TryGetEntityById<VRage.Game.Entity.MyEntity>(characterID, out entity2, false);
            Sandbox.Game.Entities.IMyControllableEntity user = entity2 as Sandbox.Game.Entities.IMyControllableEntity;
            MyCharacter pilot = user as MyCharacter;
            if ((flag1 && (entity != null)) && (entity.CanUse(actionEnum, user) == UseActionResult.OK))
            {
                if (this.m_pilot != null)
                {
                    this.RemovePilot();
                }
                this.AttachPilot(pilot, true, false, false);
            }
        }

        public void AttachPilotEventFailed(UseActionResult actionResult)
        {
            if (actionResult == UseActionResult.UsedBySomeoneElse)
            {
                MyHud.Notifications.Add(new MyHudNotification(MyCommonTexts.AlreadyUsedBySomebodyElse, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            }
            else if (actionResult == UseActionResult.MissingDLC)
            {
                MySession.Static.CheckDLCAndNotify(this.BlockDefinition);
            }
            else if (actionResult == UseActionResult.AccessDenied)
            {
                MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
            }
            else if (actionResult == UseActionResult.Unpowered)
            {
                MyHud.Notifications.Add(new MyHudNotification(MySpaceTexts.BlockIsNotPowered, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal));
            }
            else if (actionResult == UseActionResult.CockpitDamaged)
            {
                MyHudNotification notification = new MyHudNotification(MySpaceTexts.Notification_ControllableBlockIsDamaged, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                string[] arguments = new string[] { base.DefinitionDisplayNameText };
                notification.SetTextFormatArguments(arguments);
                MyHud.Notifications.Add(notification);
            }
        }

        public override string CalculateCurrentModel(out Matrix orientation)
        {
            base.Orientation.GetMatrix(out orientation);
            return (!base.Render.NearFlag ? this.BlockDefinition.Model : (string.IsNullOrEmpty(this.m_cockpitInteriorModel) ? this.BlockDefinition.Model : this.m_cockpitInteriorModel));
        }

        private float CalculateRequiredPowerInput()
        {
            if ((!base.IsFunctional || !this.BlockDefinition.EnableShipControl) || (base.CubeGrid.GridSystems.ResourceDistributor.ResourceState == MyResourceStateEnum.NoPower))
            {
                return 0f;
            }
            return 0.003f;
        }

        protected override bool CanBeMainCockpit() => 
            this.BlockDefinition.EnableShipControl;

        public virtual UseActionResult CanUse(UseActionEnum actionEnum, Sandbox.Game.Entities.IMyControllableEntity user)
        {
            if (this.m_pilot != null)
            {
                MyPlayer player;
                MyPlayer.PlayerId? savedPlayer = this.m_pilot.SavedPlayer;
                if ((savedPlayer == null) || MySession.Static.Players.TryGetPlayerById(savedPlayer.Value, out player))
                {
                    return UseActionResult.UsedBySomeoneElse;
                }
            }
            if (base.MarkedForClose)
            {
                return UseActionResult.Closed;
            }
            if (!base.IsFunctional)
            {
                return UseActionResult.CockpitDamaged;
            }
            long controllingIdentityId = user.ControllerInfo.ControllingIdentityId;
            if (controllingIdentityId == 0)
            {
                return UseActionResult.AccessDenied;
            }
            switch (base.HasPlayerAccessReason(controllingIdentityId))
            {
                case MyTerminalBlock.AccessRightsResult.Enemies:
                case MyTerminalBlock.AccessRightsResult.Other:
                    return UseActionResult.AccessDenied;

                case MyTerminalBlock.AccessRightsResult.MissingDLC:
                    return UseActionResult.MissingDLC;
            }
            return UseActionResult.OK;
        }

        private void ChangeGasFillLevel(float newFillLevel)
        {
            if (Sync.IsServer && (this.OxygenFillLevel != newFillLevel))
            {
                this.OxygenFillLevel = newFillLevel;
                this.CheckEmissiveState(false);
            }
        }

        public override void CheckEmissiveState(bool force = false)
        {
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPowered);

        private void CheckPilotRelation()
        {
            if (((this.m_pilot != null) && Sync.IsServer) && (base.ControllerInfo.Controller != null))
            {
                MyRelationsBetweenPlayerAndBlock userRelationToOwner = base.GetUserRelationToOwner(base.ControllerInfo.ControllingIdentityId);
                if ((userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Enemies) || (userRelationToOwner == MyRelationsBetweenPlayerAndBlock.Neutral))
                {
                    base.RaiseControlledEntityUsed();
                }
            }
        }

        public void ClearSavedpilot()
        {
            this.m_serverSidePilotId = 0L;
            this.m_savedPilot = null;
        }

        private void CloseWindow(bool isPublic)
        {
            MyGuiScreenGamePlay.ActiveGameplayScreen = MyGuiScreenGamePlay.TmpGameplayScreenHolder;
            MyGuiScreenGamePlay.TmpGameplayScreenHolder = null;
            foreach (MySlimBlock block in base.CubeGrid.CubeBlocks)
            {
                if ((block.FatBlock != null) && (block.FatBlock.EntityId == base.EntityId))
                {
                    this.SendChangeDescriptionMessage(this.m_textBox.Description.Text, isPublic);
                    this.SendChangeOpenMessage(false, false, 0UL, false);
                    break;
                }
            }
        }

        protected override void Closing()
        {
            base.Closing();
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.SetRender(null);
            }
        }

        protected override void ComponentStack_IsFunctionalChanged()
        {
            base.ComponentStack_IsFunctionalChanged();
            if (!base.IsFunctional)
            {
                if (this.m_pilot != null)
                {
                    this.RemovePilot();
                }
                this.ChangeGasFillLevel(0f);
                base.ResourceSink.Update();
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private float ComputeRequiredGas() => 
            (base.IsWorking ? Math.Min((float) ((this.OxygenAmountMissing * 60f) / 100f), (float) (base.ResourceSink.MaxRequiredInputByType(MyCharacterOxygenComponent.OxygenId) * 0.1f)) : 0f);

        protected override void CreateTerminalControls()
        {
            base.CreateTerminalControls();
            if (!MyTerminalControlFactory.AreControlsCreated<MyCockpit>())
            {
                MyMultiTextPanelComponent.CreateTerminalControls<MyCockpit>();
            }
        }

        private void CreateTextBox(bool isEditable, StringBuilder description, bool isPublic)
        {
            bool editable = isEditable;
            this.m_textBox = new MyGuiScreenTextPanel(this.DisplayNameText, "", this.PanelComponent.DisplayName, description.ToString(), new Action<VRage.Game.ModAPI.ResultEnum>(this.OnClosedPanelTextBox), null, null, editable, null);
        }

        protected Vector3D? FindFreeNeighbourPosition()
        {
            int num = 0x200;
            int num2 = 1;
            while (num > 0)
            {
                Vector3I[] neighbourPositions = this.m_neighbourPositions;
                int index = 0;
                while (true)
                {
                    Vector3D vectord;
                    if (index >= neighbourPositions.Length)
                    {
                        num2++;
                        num--;
                        break;
                    }
                    Vector3I neighbourOffsetI = neighbourPositions[index] * num2;
                    if (this.IsNeighbourPositionFree(neighbourOffsetI, out vectord))
                    {
                        return new Vector3D?(vectord);
                    }
                    index++;
                }
            }
            return null;
        }

        public override MatrixD GetHeadMatrix(bool includeY, bool includeX = true, bool forceHeadAnim = false, bool forceHeadBone = false)
        {
            MatrixD worldMatrix = base.PositionComp.WorldMatrix;
            float headLocalXAngle = this.m_headLocalXAngle;
            float headLocalYAngle = this.m_headLocalYAngle;
            if (!includeX)
            {
                headLocalXAngle = this.DEFAULT_FPS_CAMERA_X_ANGLE;
            }
            MatrixD xd2 = MatrixD.CreateFromAxisAngle(Vector3D.Right, (double) MathHelper.ToRadians(headLocalXAngle));
            if (includeY)
            {
                xd2 *= Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(headLocalYAngle));
            }
            worldMatrix = (xd2 * this.m_cameraDummy) * worldMatrix;
            Vector3D translation = worldMatrix.Translation;
            if (base.m_headLocalPosition != Vector3.Zero)
            {
                translation = Vector3D.Transform(base.m_headLocalPosition + this.m_playerHeadSpring, base.PositionComp.WorldMatrix);
            }
            else if (this.Pilot != null)
            {
                translation = this.Pilot.GetHeadMatrix(includeY, includeX, true, true, true).Translation;
            }
            worldMatrix.Translation = translation;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_MISC)
            {
                MyRenderProxy.DebugDrawSphere(worldMatrix.Translation, 0.05f, Color.Yellow, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, "Cockpit camera", Color.Yellow, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            MatrixD xd3 = worldMatrix;
            xd3.Translation = translation;
            return xd3;
        }

        public override List<MyHudEntityParams> GetHudParams(bool allowBlink)
        {
            List<MyHudEntityParams> hudParams = base.GetHudParams(allowBlink);
            long num = (MySession.Static.LocalHumanPlayer == null) ? 0L : MySession.Static.LocalHumanPlayer.Identity.IdentityId;
            if (base.ShowOnHUD || base.IsBeingHacked)
            {
                hudParams[0].Text.AppendLine();
            }
            else
            {
                hudParams[0].Text.Clear();
            }
            if (((base.ControllerInfo.ControllingIdentityId != num) && (this.Pilot != null)) && (this.Pilot != null))
            {
                hudParams[0].Text.Append(this.Pilot.UpdateCustomNameWithFaction());
            }
            if (!base.ShowOnHUD)
            {
                base.m_hudParams.Clear();
            }
            return hudParams;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            bool? pilotJetpackEnabledBackup;
            SerializableDefinitionId? nullable4;
            MyObjectBuilder_Cockpit objectBuilderCubeBlock = (MyObjectBuilder_Cockpit) base.GetObjectBuilderCubeBlock(copy);
            if ((this.m_pilot != null) && this.m_pilot.Save)
            {
                objectBuilderCubeBlock.Pilot = (MyObjectBuilder_Character) this.m_pilot.GetObjectBuilder(copy);
            }
            else if ((this.m_savedPilot == null) || !this.m_savedPilot.Save)
            {
                objectBuilderCubeBlock.Pilot = null;
            }
            else
            {
                objectBuilderCubeBlock.Pilot = (MyObjectBuilder_Character) this.m_savedPilot.GetObjectBuilder(copy);
            }
            if (objectBuilderCubeBlock.Pilot != null)
            {
                pilotJetpackEnabledBackup = this.m_pilotJetpackEnabledBackup;
            }
            else
            {
                pilotJetpackEnabledBackup = null;
            }
            objectBuilderCubeBlock.PilotJetpackEnabled = pilotJetpackEnabledBackup;
            objectBuilderCubeBlock.Autopilot = this.m_aiPilot?.GetObjectBuilder();
            MyDefinitionId? pilotGunDefinition = this.m_pilotGunDefinition;
            if (pilotGunDefinition != null)
            {
                nullable4 = new SerializableDefinitionId?(pilotGunDefinition.GetValueOrDefault());
            }
            else
            {
                nullable4 = null;
            }
            objectBuilderCubeBlock.PilotGunDefinition = nullable4;
            objectBuilderCubeBlock.PilotRelativeWorld = (this.m_pilotRelativeWorld == null) ? null : ((MyPositionAndOrientation?) new MyPositionAndOrientation(this.m_pilotRelativeWorld.Value));
            objectBuilderCubeBlock.IsInFirstPersonView = this.IsInFirstPersonView;
            objectBuilderCubeBlock.OxygenLevel = this.OxygenFillLevel;
            objectBuilderCubeBlock.TextPanels = this.m_multiPanel?.Serialize();
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation() => 
            null;

        public override MatrixD GetViewMatrix()
        {
            MatrixD xd2;
            bool flag = !this.ForceFirstPersonCamera;
            if (!this.IsInFirstPersonView & flag)
            {
                return MyThirdPersonSpectator.Static.GetViewMatrix();
            }
            MatrixD.Invert(ref this.GetHeadMatrix(this.IsInFirstPersonView || this.ForceFirstPersonCamera, this.IsInFirstPersonView || this.ForceFirstPersonCamera, false, false), out xd2);
            return xd2;
        }

        public void GiveControlToPilot()
        {
            MyCharacter entity = this.m_pilot ?? this.m_savedPilot;
            if ((entity.ControllerInfo != null) && (entity.ControllerInfo.Controller != null))
            {
                entity.SwitchControl(this);
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyDefinitionId? nullable1;
            Matrix? nullable4;
            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(objectBuilder.GetId());
            this.m_isLargeCockpit = cubeBlockDefinition.CubeSize == MyCubeSize.Large;
            this.m_cockpitInteriorModel = this.BlockDefinition.InteriorModel;
            this.m_cockpitGlassModel = this.BlockDefinition.GlassModel;
            base.NeedsWorldMatrix = true;
            base.InvalidateOnMove = true;
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyCockpit_IsWorkingChanged);
            if (this.m_cockpitInteriorModel == null)
            {
                base.NeedsWorldMatrix = false;
                base.InvalidateOnMove = false;
                base.Render = new MyRenderComponentCockpit(this);
            }
            base.Init(objectBuilder, cubeGrid);
            this.PostBaseInit();
            MyObjectBuilder_Cockpit cockpit = (MyObjectBuilder_Cockpit) objectBuilder;
            if (cockpit.Pilot != null)
            {
                VRage.Game.Entity.MyEntity entity;
                this.m_pilotJetpackEnabledBackup = cockpit.PilotJetpackEnabled;
                MyCharacter character = null;
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById(cockpit.Pilot.EntityId, out entity, false))
                {
                    character = (MyCharacter) Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(cockpit.Pilot, base.Render.FadeIn);
                }
                else
                {
                    character = (MyCharacter) entity;
                    if ((character.IsUsing is MyShipController) && !ReferenceEquals(character.IsUsing, this))
                    {
                        character = null;
                    }
                }
                if (character != null)
                {
                    this.m_savedPilot = character;
                    this.m_defferAttach = true;
                    base.m_singleWeaponMode = cockpit.UseSingleWeaponMode;
                    base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    if (Sync.IsServer)
                    {
                        base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
                    }
                }
                this.IsInFirstPersonView = cockpit.IsInFirstPersonView;
            }
            if (cockpit.Autopilot != null)
            {
                MyAutopilotBase autopilot = MyAutopilotFactory.CreateAutopilot(cockpit.Autopilot);
                autopilot.Init(cockpit.Autopilot);
                Action<VRage.Game.Entity.MyEntity> delayedAttachAutopilot = null;
                delayedAttachAutopilot = delegate (VRage.Game.Entity.MyEntity x) {
                    this.AttachAutopilot(autopilot, false);
                    this.AddedToScene -= delayedAttachAutopilot;
                };
                base.AddedToScene += delayedAttachAutopilot;
            }
            SerializableDefinitionId? pilotGunDefinition = cockpit.PilotGunDefinition;
            if (pilotGunDefinition != null)
            {
                nullable1 = new MyDefinitionId?(pilotGunDefinition.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            this.m_pilotGunDefinition = nullable1;
            if (cockpit.PilotRelativeWorld != null)
            {
                nullable4 = new Matrix?((Matrix) cockpit.PilotRelativeWorld.Value.GetMatrix());
            }
            else
            {
                nullable4 = null;
            }
            this.m_pilotRelativeWorld = nullable4;
            if (((this.m_pilotGunDefinition != null) && (this.m_pilotGunDefinition.Value.TypeId == typeof(MyObjectBuilder_AutomaticRifle))) && string.IsNullOrEmpty(this.m_pilotGunDefinition.Value.SubtypeName))
            {
                this.m_pilotGunDefinition = new MyDefinitionId(typeof(MyObjectBuilder_AutomaticRifle), "RifleGun");
            }
            if (!string.IsNullOrEmpty(this.m_cockpitInteriorModel))
            {
                if (MyModels.GetModelOnlyDummies(this.m_cockpitInteriorModel).Dummies.ContainsKey("head"))
                {
                    base.m_headLocalPosition = MyModels.GetModelOnlyDummies(this.m_cockpitInteriorModel).Dummies["head"].Matrix.Translation;
                }
            }
            else if (MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies.ContainsKey("head"))
            {
                base.m_headLocalPosition = MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies["head"].Matrix.Translation;
            }
            base.AddDebugRenderComponent(new MyDebugRenderComponentCockpit(this));
            this.InitializeConveyorEndpoint();
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_conveyorEndpoint));
            this.m_oxygenFillLevel.SetLocalValue(cockpit.OxygenLevel);
            MyResourceSinkInfo item = new MyResourceSinkInfo {
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                MaxRequiredInput = 0f,
                RequiredInputFunc = new Func<float>(this.CalculateRequiredPowerInput)
            };
            List<MyResourceSinkInfo> list1 = new List<MyResourceSinkInfo>();
            list1.Add(item);
            item = new MyResourceSinkInfo {
                ResourceTypeId = MyCharacterOxygenComponent.OxygenId,
                MaxRequiredInput = this.BlockDefinition.OxygenCapacity,
                RequiredInputFunc = new Func<float>(this.ComputeRequiredGas)
            };
            list1.Add(item);
            List<MyResourceSinkInfo> sinkData = list1;
            base.ResourceSink.Init(MyStringHash.GetOrCompute("Utility"), sinkData);
            base.ResourceSink.CurrentInputChanged += new MyCurrentResourceInputChangedDelegate(this.Sink_CurrentInputChanged);
            this.m_lastGasInputUpdateTick = MySession.Static.ElapsedGameTime.Ticks;
            if ((this.GetInventory(0) == null) && this.BlockDefinition.HasInventory)
            {
                Vector3 size = Vector3.One * 1f;
                MyInventory component = new MyInventory(size.Volume, size, MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(component);
            }
            if ((this.BlockDefinition.ScreenAreas != null) && (this.BlockDefinition.ScreenAreas.Count > 0))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
                this.m_multiPanel = new MyMultiTextPanelComponent(this, this.BlockDefinition.ScreenAreas, cockpit.TextPanels);
                this.m_multiPanel.Init(new Action<int, int[]>(this.SendAddImagesToSelectionRequest), new Action<int, int[]>(this.SendRemoveSelectedImageRequest));
            }
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_conveyorEndpoint = new MyMultilineConveyorEndpoint(this);
        }

        protected override bool IsCameraController() => 
            true;

        public override bool IsLargeShip() => 
            this.m_isLargeCockpit;

        public bool IsNeighbourPositionFree(Vector3I neighbourOffsetI, out Vector3D translation)
        {
            Vector3D vectord = (Vector3D) ((((((0.5f * base.PositionComp.LocalAABB.Size.X) * neighbourOffsetI.X) * base.PositionComp.WorldMatrix.Right) + (((0.5f * base.PositionComp.LocalAABB.Size.Y) * neighbourOffsetI.Y) * base.PositionComp.WorldMatrix.Up)) - (((0.5f * base.PositionComp.LocalAABB.Size.Z) * neighbourOffsetI.Z) * base.PositionComp.WorldMatrix.Forward)) + ((((0.9f * neighbourOffsetI.X) * base.PositionComp.WorldMatrix.Right) + ((0.9f * neighbourOffsetI.Y) * base.PositionComp.WorldMatrix.Up)) - ((0.9f * neighbourOffsetI.Z) * base.PositionComp.WorldMatrix.Forward)));
            MatrixD worldMatrix = MatrixD.CreateWorld(base.PositionComp.WorldMatrix.Translation + vectord, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up);
            translation = worldMatrix.Translation;
            return this.m_pilot.CanPlaceCharacter(ref worldMatrix, true, true, null);
        }

        private void m_pilot_OnMarkForClose(VRage.Game.Entity.MyEntity obj)
        {
            if (this.m_pilot != null)
            {
                base.Hierarchy.RemoveChild(this.m_pilot, false);
                base.m_rechargeSocket.Unplug();
                this.m_pilot.SuitBattery.ResourceSink.TemporaryConnectedEntity = null;
                this.m_pilot = null;
            }
        }

        private void MyCockpit_IsWorkingChanged(MyCubeBlock obj)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        [Event(null, 0x8d2), Reliable, Broadcast]
        private void NotifyClientPilotChanged(long pilotEntityId, Matrix? pilotRelativeWorld)
        {
            this.m_serverSidePilotId = pilotEntityId;
            this.m_pilotRelativeWorld = pilotRelativeWorld;
            if (pilotEntityId != 0)
            {
                this.TryAttachPilot(pilotEntityId);
            }
            else if (this.m_pilot != null)
            {
                this.RemovePilot();
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if ((this.m_savedPilot != null) || ((this.m_multiPanel != null) && (this.m_multiPanel.SurfaceCount > 0)))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.AddToScene(new int?((this.BlockDefinition.InteriorModel != null) ? 1 : 0));
            }
        }

        public void OnAssumeControl(IMyCameraController previousCameraController)
        {
            MyHud.SetHudDefinition(this.BlockDefinition.HUD);
            this.UpdateCameraAfterChange(true);
        }

        [Event(null, 0x245), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void OnChangeDescription(string description, bool isPublic)
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear().Append(description);
            this.PanelComponent.Text = builder.ToString();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void OnChangeOpen(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.IsTextPanelOpen = isOpen;
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && (user == Sync.MyId)) & isOpen)
            {
                this.OpenWindow(editable, false, isPublic);
            }
        }

        [Event(null, 0x1dd), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        private void OnChangeOpenRequest(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            if (!((Sync.IsServer && this.IsTextPanelOpen) & isOpen))
            {
                this.OnChangeOpen(isOpen, editable, user, isPublic);
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess), isOpen, editable, user, isPublic, targetEndpoint);
            }
        }

        [Event(null, 0x1e8), Reliable, Broadcast]
        private void OnChangeOpenSuccess(bool isOpen, bool editable, ulong user, bool isPublic)
        {
            this.OnChangeOpen(isOpen, editable, user, isPublic);
        }

        private void OnCharacterFactionChanged(MyFaction oldFaction, MyFaction newFaction)
        {
            this.CheckPilotRelation();
        }

        public void OnClosedPanelMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.m_textBox.Description.Text.Remove(0x186a0, this.m_textBox.Description.Text.Length - 0x186a0);
                this.CloseWindow(true);
            }
            else
            {
                this.CreateTextBox(true, this.m_textBox.Description.Text, true);
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        public void OnClosedPanelTextBox(VRage.Game.ModAPI.ResultEnum result)
        {
            if (this.m_textBox != null)
            {
                if (this.m_textBox.Description.Text.Length <= 0x186a0)
                {
                    this.CloseWindow(true);
                }
                else
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextTooLongText), null, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnClosedPanelMessageBox), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
        }

        protected override void OnControlAcquired_UpdateCamera()
        {
            base.CubeGrid.RaiseGridChanged();
            base.OnControlAcquired_UpdateCamera();
            this.m_currentCameraShakePower = 0f;
        }

        protected override void OnControlledEntity_Used()
        {
            MyCharacter pilot = this.m_pilot;
            this.RemovePilot();
            base.OnControlledEntity_Used();
        }

        protected override void OnControlReleased(MyEntityController controller)
        {
            if ((this.m_pilot == null) || ((this.m_pilot != null) && !MySessionComponentReplay.Static.HasEntityReplayData(base.CubeGrid.EntityId)))
            {
                base.OnControlReleased(controller);
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        protected override void OnControlReleased_UpdateCamera()
        {
            base.OnControlReleased_UpdateCamera();
            this.m_currentCameraShakePower = 0f;
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), true);
            base.OnDestroy();
        }

        protected virtual void OnInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            base.UpdateIsWorking();
            if (resourceTypeId != MyCharacterOxygenComponent.OxygenId)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
            else
            {
                float num = ((float) (MySession.Static.ElapsedPlayTime.Ticks - this.m_lastGasInputUpdateTick)) / 1E+07f;
                this.m_lastGasInputUpdateTick = MySession.Static.ElapsedPlayTime.Ticks;
                float num2 = oldInput * num;
                this.ChangeGasFillLevel(this.OxygenFillLevel + num2);
                this.m_updateSink = true;
            }
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.Reset();
            }
            if (base.ResourceSink != null)
            {
                this.UpdateScreen();
            }
            if (this.CheckIsWorking())
            {
                (base.Render as MyRenderComponentCockpit).UpdateModelProperties();
            }
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            this.CheckPilotRelation();
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            if (!this.m_defferAttach && (this.m_savedPilot != null))
            {
                this.AttachPilot(this.m_savedPilot, false, false, true);
                this.m_savedPilot = null;
            }
        }

        public void OnReleaseControl(IMyCameraController newCameraController)
        {
            this.UpdateNearFlag();
            if (base.m_enableFirstPerson)
            {
                this.UpdateCockpitModel();
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        public override void OnRemovedFromScene(object source)
        {
            if (!Sandbox.Game.Entities.MyEntities.CloseAllowed)
            {
                this.m_savedPilot = this.m_pilot;
                this.RemovePilot();
            }
            base.OnRemovedFromScene(source);
        }

        [Event(null, 0x1b5), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnRemoveSelectedImageRequest(int panelIndex, int[] selection)
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.RemoveItems(panelIndex, selection);
            }
        }

        [Event(null, 0x58b), Reliable, Server(ValidationType.IgnoreDLC)]
        public void OnRequestRemovePilot()
        {
            if (MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value))
            {
                this.RemovePilot();
            }
        }

        [Event(null, 0x1c3), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        private void OnSelectImageRequest(int panelIndex, int[] selection)
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.SelectItems(panelIndex, selection);
            }
        }

        public override void OnUnregisteredFromGridSystems()
        {
            base.OnUnregisteredFromGridSystems();
            if (this.m_pilot != null)
            {
                MyCharacter pilot = this.m_pilot;
                if (!Sandbox.Game.Entities.MyEntities.CloseAllowed)
                {
                    this.RemovePilot();
                    pilot.DoDamage(1000f, MyDamageType.Destruction, false, 0L);
                }
                else if (ReferenceEquals(MySession.Static.CameraController, this))
                {
                    Vector3D? position = null;
                    MySession.Static.SetCameraController(MySession.Static.GetCameraControllerEnum(), this.m_pilot, position);
                }
            }
        }

        public void OpenWindow(bool isEditable, bool sync, bool isPublic)
        {
            if (sync)
            {
                this.SendChangeOpenMessage(true, isEditable, Sync.MyId, isPublic);
            }
            else
            {
                this.CreateTextBox(isEditable, new StringBuilder(this.PanelComponent.Text), isPublic);
                MyGuiScreenGamePlay.TmpGameplayScreenHolder = MyGuiScreenGamePlay.ActiveGameplayScreen;
                MyGuiScreenGamePlay.ActiveGameplayScreen = this.m_textBox;
                MyScreenManager.AddScreen(this.m_textBox);
            }
        }

        protected virtual void PlacePilotInSeat(MyCharacter pilot)
        {
            bool playerIsPilot = (MySession.Static.LocalHumanPlayer != null) && ReferenceEquals(MySession.Static.LocalHumanPlayer.Identity.Character, pilot);
            this.m_pilot.Sit(base.m_enableFirstPerson, playerIsPilot, false, this.BlockDefinition.CharacterAnimation);
            pilot.PositionComp.SetWorldMatrix(this.m_characterDummy * base.WorldMatrix, this, false, true, true, false, false, false);
            base.CubeGrid.RegisterOccupiedBlock(this);
            base.CubeGrid.SetInventoryMassDirty();
        }

        protected virtual void PostBaseInit()
        {
            this.TryGetDummies();
        }

        private void RefillFromBottlesOnGrid()
        {
            List<IMyConveyorEndpoint> reachableVertices = new List<IMyConveyorEndpoint>();
            MyGridConveyorSystem.FindReachable(this.ConveyorEndpoint, reachableVertices, vertex => (vertex.CubeBlock != null) && (base.FriendlyWithBlock(vertex.CubeBlock) && vertex.CubeBlock.HasInventory), null, null);
            bool flag = false;
            using (List<IMyConveyorEndpoint>.Enumerator enumerator = reachableVertices.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyCubeBlock cubeBlock = enumerator.Current.CubeBlock;
                    int inventoryCount = cubeBlock.InventoryCount;
                    for (int i = 0; i < inventoryCount; i++)
                    {
                        MyInventory inventory = cubeBlock.GetInventory(i);
                        using (List<MyPhysicalInventoryItem>.Enumerator enumerator2 = inventory.GetItems().GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyObjectBuilder_GasContainerObject content = enumerator2.Current.Content as MyObjectBuilder_GasContainerObject;
                                if ((content != null) && (content.GasLevel != 0f))
                                {
                                    MyOxygenContainerDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(content) as MyOxygenContainerDefinition;
                                    if (physicalItemDefinition.StoredGasId == MyCharacterOxygenComponent.OxygenId)
                                    {
                                        float num3 = content.GasLevel * physicalItemDefinition.Capacity;
                                        float num4 = Math.Min(num3, this.OxygenAmountMissing);
                                        if (num4 != 0f)
                                        {
                                            content.GasLevel = (num3 - num4) / physicalItemDefinition.Capacity;
                                            if (content.GasLevel < 0f)
                                            {
                                                content.GasLevel = 0f;
                                            }
                                            float gasLevel = content.GasLevel;
                                            inventory.UpdateGasAmount();
                                            flag = true;
                                            this.OxygenAmount += num4;
                                            if (this.OxygenFillLevel >= 1f)
                                            {
                                                this.ChangeGasFillLevel(1f);
                                                base.ResourceSink.Update();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (flag)
            {
                MyHud.Notifications.Add(new MyHudNotification(MySpaceTexts.NotificationBottleRefill, 0x9c4, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Important));
            }
        }

        public void RemoveAutopilot(bool updateSync = true)
        {
            if (this.m_aiPilot != null)
            {
                this.m_aiPilot.OnRemovedFromCockpit();
                this.m_aiPilot = null;
                if (updateSync && Sync.IsServer)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, MyObjectBuilder_AutopilotBase>(this, x => new Action<MyObjectBuilder_AutopilotBase>(x.SetAutopilot_Client), null, targetEndpoint);
                }
            }
            if ((!Sync.IsServer && ((base.ControllerInfo.Controller == null) || !base.ControllerInfo.IsLocallyControlled())) && (this.m_multiPanel == null))
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        protected override void RemoveLocal()
        {
            if (MyCubeBuilder.Static.IsActivated)
            {
                MySession.Static.GameFocusManager.Clear();
            }
            base.RemoveLocal();
            this.RemovePilot();
        }

        public void RemoveOriginalPilotPosition()
        {
            this.m_pilotRelativeWorld = null;
        }

        public bool RemovePilot()
        {
            EndpointId id;
            if (this.m_pilot == null)
            {
                return true;
            }
            if (Sync.IsServer && !base.CubeGrid.IsBlockTrasferInProgress)
            {
                this.m_serverSidePilotId = 0L;
                id = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, long, Matrix?>(this, x => new Action<long, Matrix?>(this.NotifyClientPilotChanged), this.m_serverSidePilotId, this.m_pilotRelativeWorld, id);
            }
            MyAnalyticsHelper.ReportActivityEnd(this.m_pilot, "Cockpit");
            if (this.m_pilot.Physics == null)
            {
                this.m_pilot = null;
                return true;
            }
            this.StopLoopSound();
            this.m_pilot.OnMarkForClose -= this.m_pilotClosedHandler;
            if (MyVisualScriptLogicProvider.PlayerLeftCockpit != null)
            {
                MyVisualScriptLogicProvider.PlayerLeftCockpit(base.Name, this.m_pilot.GetPlayerIdentityId(), base.CubeGrid.Name);
            }
            base.Hierarchy.RemoveChild(this.m_pilot, false);
            base.NeedsWorldMatrix = this.m_cockpitInteriorModel != null;
            base.InvalidateOnMove = base.NeedsWorldMatrix;
            if (this.m_pilot.IsDead)
            {
                if (base.ControllerInfo.Controller != null)
                {
                    this.SwitchControl(this.m_pilot);
                }
                Sandbox.Game.Entities.MyEntities.Add(this.m_pilot, true);
                this.m_pilot.WorldMatrix = base.WorldMatrix;
                this.m_pilotGunDefinition = null;
                base.m_rechargeSocket.Unplug();
                this.m_pilot.SuitBattery.ResourceSink.TemporaryConnectedEntity = null;
                if (ReferenceEquals(this.m_pilot, MySession.Static.LocalCharacter))
                {
                    MyLocalCache.LoadInventoryConfig(this.m_pilot, false);
                }
                this.m_pilot = null;
                return true;
            }
            bool flag = false;
            MatrixD worldMatrix = MatrixD.Identity;
            if (this.m_pilotRelativeWorld != null)
            {
                Vector3D to = Vector3D.Transform((Vector3) (base.Position * base.CubeGrid.GridSize), base.CubeGrid.WorldMatrix);
                worldMatrix = MatrixD.Multiply(this.m_pilotRelativeWorld.Value, base.WorldMatrix);
                MyPhysics.HitInfo? nullable2 = MyPhysics.CastRay(worldMatrix.Translation, to, 15);
                if (nullable2 == null)
                {
                    if (this.m_pilot.CanPlaceCharacter(ref worldMatrix, false, false, null))
                    {
                        flag = true;
                    }
                }
                else
                {
                    VRage.ModAPI.IMyEntity hitEntity = nullable2.Value.HkHitInfo.GetHitEntity();
                    if (base.CubeGrid.Equals(hitEntity) && this.m_pilot.CanPlaceCharacter(ref worldMatrix, false, false, null))
                    {
                        flag = true;
                    }
                }
            }
            Vector3D? nullable = null;
            if (!flag)
            {
                nullable = this.FindFreeNeighbourPosition();
                if (nullable == null)
                {
                    nullable = new Vector3D?(base.PositionComp.GetPosition());
                }
            }
            this.RemovePilotFromSeat(this.m_pilot);
            base.EndShootAll();
            base.CubeGrid.GridSystems.RadioSystem.Unregister(this.m_pilot.RadioBroadcaster);
            base.CubeGrid.GridSystems.RadioSystem.Unregister(this.m_pilot.RadioReceiver);
            MyIdentity identity = this.m_pilot.GetIdentity();
            if (identity != null)
            {
                identity.FactionChanged -= new Action<MyFaction, MyFaction>(this.OnCharacterFactionChanged);
            }
            if (base.CubeGrid.IsBlockTrasferInProgress)
            {
                MyCharacter pilot = this.m_pilot;
                this.m_pilot = null;
                if (base.ControllerInfo.Controller != null)
                {
                    this.SwitchControl(pilot);
                }
            }
            else if (flag || (nullable != null))
            {
                Vector3D? nullable3;
                MatrixD xd1;
                base.Hierarchy.RemoveChild(this.m_pilot, false);
                if (!flag)
                {
                    xd1 = MatrixD.CreateWorld(nullable.Value - base.WorldMatrix.Up, base.WorldMatrix.Forward, base.WorldMatrix.Up);
                }
                else
                {
                    xd1 = worldMatrix;
                }
                MatrixD xd2 = xd1;
                if (!Sandbox.Game.Entities.MyEntities.CloseAllowed)
                {
                    this.m_pilot.PositionComp.SetWorldMatrix(xd2, this, false, true, true, false, false, false);
                }
                Sandbox.Game.Entities.MyEntities.Add(this.m_pilot, true);
                this.m_pilot.Physics.Enabled = true;
                base.m_rechargeSocket.Unplug();
                this.m_pilot.SuitBattery.ResourceSink.TemporaryConnectedEntity = null;
                this.m_pilot.Stand();
                if ((this.m_pilotJetpackEnabledBackup != null) && (this.m_pilot.JetpackComp != null))
                {
                    this.m_pilot.JetpackComp.TurnOnJetpack(this.m_pilotJetpackEnabledBackup.Value, false, false);
                }
                if ((base.Parent != null) && (base.Parent.Physics != null))
                {
                    VRage.Game.Entity.MyEntity entityById = Sandbox.Game.Entities.MyEntities.GetEntityById(this.m_pilot.ClosestParentId, false);
                    if ((entityById == null) || Sync.IsServer)
                    {
                        this.m_pilot.Physics.LinearVelocity = base.Parent.Physics.LinearVelocity;
                    }
                    else
                    {
                        this.m_pilot.Physics.LinearVelocity = entityById.Physics.LinearVelocity - base.Parent.Physics.LinearVelocity;
                    }
                    if (base.Parent.Physics.LinearVelocity.LengthSquared() > 100f)
                    {
                        MyCharacterJetpackComponent jetpackComp = this.m_pilot.JetpackComp;
                        if (jetpackComp != null)
                        {
                            jetpackComp.EnableDampeners(true);
                            jetpackComp.TurnOnJetpack(true, false, false);
                            this.m_pilot.RelativeDampeningEntity = base.CubeGrid;
                            if (Sync.IsServer)
                            {
                                id = new EndpointId();
                                nullable3 = null;
                                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient), this.m_pilot.EntityId, this.m_pilot.RelativeDampeningEntity.EntityId, id, nullable3);
                            }
                        }
                    }
                }
                MyCharacter pilot = this.m_pilot;
                this.m_pilot = null;
                if (base.ControllerInfo.Controller != null)
                {
                    if (base.ControllerInfo.Controller.Player.IsLocalPlayer && (pilot != null))
                    {
                        pilot.RadioReceiver.Clear();
                    }
                    this.SwitchControl(pilot);
                }
                if (this.m_pilotGunDefinition != null)
                {
                    pilot.SwitchToWeapon(this.m_pilotGunDefinition, false);
                }
                else
                {
                    MyDefinitionId? weaponDefinition = null;
                    pilot.SwitchToWeapon(weaponDefinition, false);
                }
                if (ReferenceEquals(pilot, MySession.Static.LocalCharacter))
                {
                    MyLocalCache.LoadInventoryConfig(pilot, false);
                }
                if (ReferenceEquals(MySession.Static.CameraController, this) && ReferenceEquals(pilot, MySession.Static.LocalCharacter))
                {
                    bool isInFirstPersonView = this.IsInFirstPersonView;
                    nullable3 = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, pilot, nullable3);
                }
                pilot.IsInFirstPersonView = this.m_pilotFirstPerson;
                this.CheckEmissiveState(false);
                return true;
            }
            this.CheckEmissiveState(false);
            return false;
        }

        protected virtual void RemovePilotFromSeat(MyCharacter pilot)
        {
            base.CubeGrid.UnregisterOccupiedBlock(this);
            base.CubeGrid.SetInventoryMassDirty();
        }

        public void RequestRemovePilot()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit>(this, x => new Action(this.OnRequestRemovePilot), targetEndpoint);
        }

        public void RequestUse(UseActionEnum actionEnum, MyCharacter user)
        {
            if (!user.IsDead)
            {
                UseActionResult oK = UseActionResult.OK;
                Sandbox.Game.Entities.IMyControllableEntity entity = user;
                if ((oK = this.CanUse(actionEnum, entity)) != UseActionResult.OK)
                {
                    this.AttachPilotEventFailed(oK);
                }
                else
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, UseActionEnum, long>(this, x => new Action<UseActionEnum, long>(x.AttachPilotEvent), actionEnum, user.EntityId, targetEndpoint);
                }
            }
        }

        public void Rotate(Vector2 rotationIndicator, float roll)
        {
            float num = MyInput.Static.GetMouseSensitivity() * 0.13f;
            if (rotationIndicator.X != 0f)
            {
                this.m_headLocalXAngle = MathHelper.Clamp(this.m_headLocalXAngle - (rotationIndicator.X * num), this.MinHeadLocalXAngle, this.MaxHeadLocalXAngle);
            }
            if (rotationIndicator.Y != 0f)
            {
                bool isInFirstPersonView = this.IsInFirstPersonView;
                this.m_headLocalYAngle = !(!(this.MinHeadLocalYAngle == 0f) & isInFirstPersonView) ? (this.m_headLocalYAngle - (rotationIndicator.Y * num)) : MathHelper.Clamp(this.m_headLocalYAngle - (rotationIndicator.Y * num), this.MinHeadLocalYAngle, this.MaxHeadLocalYAngle);
            }
            if (!this.IsInFirstPersonView)
            {
                MyThirdPersonSpectator.Static.Rotate(rotationIndicator, roll);
            }
            rotationIndicator = Vector2.Zero;
        }

        public void RotateStopped()
        {
            base.MoveAndRotateStopped();
        }

        void IMyMultiTextPanelComponentOwner.SelectPanel(List<MyGuiControlListbox.Item> panelItems)
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.SelectPanel((int) panelItems[0].UserData);
            }
            base.RaisePropertiesChanged();
        }

        bool IMyGasBlock.IsWorking() => 
            (base.IsWorking && this.BlockDefinition.IsPressurized);

        void Sandbox.ModAPI.IMyCockpit.AttachPilot(IMyCharacter pilot)
        {
            if (!pilot.IsDead)
            {
                UseActionEnum manipulate = UseActionEnum.Manipulate;
                if (this.CanUse(manipulate, pilot as Sandbox.Game.Entities.IMyControllableEntity) == UseActionResult.OK)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, UseActionEnum, long>(this, x => new Action<UseActionEnum, long>(x.AttachPilotEvent), manipulate, pilot.EntityId, targetEndpoint);
                }
            }
        }

        void Sandbox.ModAPI.IMyCockpit.RemovePilot()
        {
            this.RemoveLocal();
        }

        Sandbox.ModAPI.Ingame.IMyTextSurface Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.GetSurface(int index) => 
            this.m_multiPanel?.GetSurface(index);

        private void SendAddImagesToSelectionRequest(int panelIndex, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, int, int[]>(this, x => new Action<int, int[]>(x.OnSelectImageRequest), panelIndex, selection, targetEndpoint);
        }

        private void SendChangeDescriptionMessage(StringBuilder description, bool isPublic)
        {
            if (base.CubeGrid.IsPreview || !base.CubeGrid.SyncFlag)
            {
                this.PanelComponent.Text = description.ToString();
            }
            else if (description.CompareTo(this.PanelComponent.Text) != 0)
            {
                EndpointId targetEndpoint = new EndpointId();
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, string, bool>(this, x => new Action<string, bool>(x.OnChangeDescription), description.ToString(), isPublic, targetEndpoint);
            }
        }

        private void SendChangeOpenMessage(bool isOpen, bool editable = false, ulong user = 0UL, bool isPublic = false)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, bool, bool, ulong, bool>(this, x => new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest), isOpen, editable, user, isPublic, targetEndpoint);
        }

        private void SendRemoveSelectedImageRequest(int panelIndex, int[] selection)
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCockpit, int, int[]>(this, x => new Action<int, int[]>(x.OnRemoveSelectedImageRequest), panelIndex, selection, targetEndpoint);
        }

        [Event(null, 0x8c5), Reliable, Broadcast]
        private void SetAutopilot_Client([Serialize(MyObjectFlags.Dynamic | MyObjectFlags.DefaultZero, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_AutopilotBase autopilot)
        {
            if (autopilot == null)
            {
                this.RemoveAutopilot(false);
            }
            else
            {
                this.AttachAutopilot(MyAutopilotFactory.CreateAutopilot(autopilot), false);
            }
        }

        private bool ShouldPlay2D() => 
            ((MySession.Static.LocalCharacter != null) && ReferenceEquals(this.Pilot, MySession.Static.LocalCharacter));

        protected override bool ShouldSit() => 
            (this.m_isLargeCockpit || base.ShouldSit());

        public override void ShowInventory()
        {
            MyGuiScreenTerminal.Show(MyTerminalPageEnum.Inventory, this.m_pilot, this);
        }

        public override void ShowTerminal()
        {
            if (base.CubeGrid.InScene)
            {
                MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, this.m_pilot, this);
            }
        }

        private void Sink_CurrentInputChanged(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink)
        {
            this.OnInputChanged(resourceTypeId, oldInput, sink);
        }

        protected override void StartLoopSound()
        {
            this.m_playIdleSound = true;
            if (((base.m_soundEmitter != null) && base.hasPower) && !base.m_baseIdleSound.SoundId.IsNull)
            {
                bool? nullable = null;
                base.m_soundEmitter.PlaySound(base.m_baseIdleSound, true, false, false, false, false, nullable);
            }
        }

        protected override void StopLoopSound()
        {
            this.m_playIdleSound = false;
            if ((base.m_soundEmitter != null) && base.m_soundEmitter.IsPlaying)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
        }

        private void TryAttachPilot(long pilotId)
        {
            this.m_retryAttachPilot = false;
            if (((this.m_pilot == null) && ((this.m_savedPilot == null) || (this.m_savedPilot.EntityId != pilotId))) || ((this.m_pilot != null) && (this.m_pilot.EntityId != pilotId)))
            {
                VRage.Game.Entity.MyEntity entity;
                this.m_savedPilot = null;
                this.RemovePilot();
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<VRage.Game.Entity.MyEntity>(pilotId, out entity, false))
                {
                    this.m_retryAttachPilot = true;
                }
                else
                {
                    MyCharacter pilot = entity as MyCharacter;
                    if ((pilot != null) && Sandbox.Game.Entities.MyEntities.IsInsideWorld(pilot.PositionComp.GetPosition()))
                    {
                        this.AttachPilot(pilot, this.m_pilotRelativeWorld != null, false, false);
                    }
                }
            }
        }

        private void TryGetDummies()
        {
            if (base.Model != null)
            {
                MyModelDummy dummy;
                MyModelDummy dummy2;
                base.Model.Dummies.TryGetValue("camera", out dummy);
                if (dummy != null)
                {
                    this.m_cameraDummy = MatrixD.Normalize(dummy.Matrix);
                }
                base.Model.Dummies.TryGetValue("character", out dummy2);
                if (dummy2 != null)
                {
                    this.m_characterDummy = MatrixD.Normalize(dummy2.Matrix);
                }
            }
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.UpdateAfterSimulation(base.hasPower && this.CheckIsWorking());
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (ReferenceEquals(this.Pilot, MySession.Static.LocalCharacter) && base.CubeGrid.IsRespawnGrid)
            {
                MyIngameHelpPod1.StartingInPod = true;
            }
        }

        public override unsafe void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if ((base.m_soundEmitter != null) && (base.m_soundEmitter.VolumeMultiplier < 1f))
            {
                base.m_soundEmitter.VolumeMultiplier = Math.Min((float) 1f, (float) (base.m_soundEmitter.VolumeMultiplier + 0.005f));
            }
            if (this.m_forcedFpsTimeoutMs > 0)
            {
                this.m_forcedFpsTimeoutMs -= 0x10;
            }
            if (((this.m_pilot != null) && base.ControllerInfo.IsLocallyHumanControlled()) && (base.CubeGrid.Physics != null))
            {
                float num = base.CubeGrid.Physics.LinearAcceleration.Length();
                float num2 = base.CubeGrid.Physics.LinearVelocity.Length();
                if ((num > 0f) && (num2 > 0f))
                {
                    float num3 = MathHelper.Clamp((float) (((Vector3.Dot(Vector3.Normalize(base.CubeGrid.Physics.LinearVelocity), Vector3.Normalize(base.CubeGrid.Physics.LinearAcceleration)) * num) - this.MIN_SHAKE_ACC) / (this.MAX_SHAKE_ACC - this.MIN_SHAKE_ACC)), (float) 0f, (float) 1f);
                    this.AddShake(this.MAX_SHAKE * num3);
                }
            }
            bool flag = !this.IsInFirstPersonView && this.ForceFirstPersonCamera;
            if (this.m_wasCameraForced != flag)
            {
                this.UpdateCameraAfterChange(false);
            }
            this.m_wasCameraForced = flag;
            if (MyDebugDrawSettings.DEBUG_DRAW_COCKPIT && (this.m_pilotRelativeWorld != null))
            {
                MatrixD matrix = MatrixD.Multiply(this.m_pilotRelativeWorld.Value, base.WorldMatrix);
                if (((base.m_lastPilot != null) && (base.m_lastPilot.Physics != null)) && (base.m_lastPilot.Physics.CharacterProxy != null))
                {
                    int shapeIndex = 0;
                    Quaternion rotation = Quaternion.CreateFromRotationMatrix(matrix);
                    Vector3D vectord2 = Vector3D.TransformNormal(base.m_lastPilot.Physics.Center, matrix);
                    Vector3D translation = matrix.Translation + vectord2;
                    MatrixD* xdPtr1 = (MatrixD*) ref matrix;
                    xdPtr1.Translation += vectord2;
                    HkShape collisionShape = base.m_lastPilot.Physics.CharacterProxy.GetCollisionShape();
                    MyPhysicsDebugDraw.DrawCollisionShape(collisionShape, matrix, 1f, ref shapeIndex, "Pilot", false);
                    List<HkBodyCollision> results = new List<HkBodyCollision>();
                    MyPhysics.GetPenetrationsShape(collisionShape, ref translation, ref rotation, results, 0x12);
                    using (List<HkBodyCollision>.Enumerator enumerator = results.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            VRage.ModAPI.IMyEntity collisionEntity = enumerator.Current.GetCollisionEntity();
                            if ((collisionEntity != null) && ((collisionEntity.Physics != null) && !collisionEntity.Physics.IsPhantom))
                            {
                                Color? colorTo = null;
                                MyRenderProxy.DebugDrawArrow3D(matrix.Translation, collisionEntity.PositionComp.GetPosition(), Color.Lime, colorTo, false, 0.1, collisionEntity.DisplayName, 0.6f, false);
                            }
                        }
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (base.m_soundEmitter != null)
            {
                if ((base.hasPower && (this.m_playIdleSound && (!base.m_soundEmitter.IsPlaying || (!base.m_soundEmitter.SoundPair.Equals(base.m_baseIdleSound) && !base.m_soundEmitter.SoundPair.Equals(base.GetInCockpitSound))))) && !base.m_baseIdleSound.Equals(MySoundPair.Empty))
                {
                    base.m_soundEmitter.VolumeMultiplier = 0f;
                    bool? nullable = null;
                    base.m_soundEmitter.PlaySound(base.m_baseIdleSound, true, false, false, false, false, nullable);
                }
                else if (((!base.hasPower || !base.IsWorking) && base.m_soundEmitter.IsPlaying) && base.m_soundEmitter.SoundPair.Equals(base.m_baseIdleSound))
                {
                    base.m_soundEmitter.StopSound(true, true);
                }
            }
            if (((base.GridResourceDistributor != null) && (base.GridGyroSystem != null)) && (base.EntityThrustComponent != null))
            {
                bool autopilotEnabled = false;
                MyEntityThrustComponent component = base.CubeGrid.Components.Get<MyEntityThrustComponent>();
                if (component != null)
                {
                    autopilotEnabled = component.AutopilotEnabled;
                }
                bool flag2 = base.CubeGrid.GridSystems.ControlSystem.IsControlled | autopilotEnabled;
                if (Sync.IsServer)
                {
                    if (!flag2 && (this.m_aiPilot != null))
                    {
                        this.m_aiPilot.Update();
                    }
                    else if ((flag2 && (this.m_aiPilot != null)) && this.m_aiPilot.RemoveOnPlayerControl)
                    {
                        this.RemoveAutopilot(true);
                    }
                }
                if ((this.m_pilot != null) && base.ControllerInfo.IsLocallyHumanControlled())
                {
                    this.m_pilot.RadioReceiver.UpdateHud(false);
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((Sync.IsServer && ((this.m_pilot != null) && (this.OxygenFillLevel < 0.2f))) && (base.CubeGrid.GridSizeEnum == MyCubeSize.Small))
            {
                this.RefillFromBottlesOnGrid();
            }
            base.ResourceSink.Update();
            if (Sync.IsServer)
            {
                float num = ((float) (MySession.Static.ElapsedPlayTime.Ticks - this.m_lastGasInputUpdateTick)) / 1E+07f;
                this.m_lastGasInputUpdateTick = MySession.Static.ElapsedPlayTime.Ticks;
                float num2 = base.ResourceSink.CurrentInputByType(MyCharacterOxygenComponent.OxygenId) * num;
                this.ChangeGasFillLevel(this.OxygenFillLevel + num2);
                if (this.BlockDefinition.IsPressurized)
                {
                    float oxygenInPoint = MyOxygenProviderSystem.GetOxygenInPoint(base.CubeGrid.GridIntegerToWorld(base.Position));
                    if (this.OxygenFillLevel < oxygenInPoint)
                    {
                        this.ChangeGasFillLevel(oxygenInPoint);
                    }
                }
            }
            if (this.m_retryAttachPilot)
            {
                if (this.m_serverSidePilotId != 0)
                {
                    this.TryAttachPilot(this.m_serverSidePilotId);
                }
                else
                {
                    this.m_retryAttachPilot = false;
                }
            }
        }

        protected override void UpdateCameraAfterChange(bool resetHeadLocalAngle = true)
        {
            base.UpdateCameraAfterChange(resetHeadLocalAngle);
            if (resetHeadLocalAngle)
            {
                this.m_headLocalXAngle = 0f;
                this.m_headLocalYAngle = 0f;
            }
            this.UpdateNearFlag();
            if (base.m_enableFirstPerson)
            {
                this.UpdateCockpitModel();
            }
            else if ((MySession.Static.IsCameraControlledObject() && (MySession.Static.Settings.Enable3rdPersonView && (this.Pilot != null))) && this.Pilot.ControllerInfo.IsLocallyControlled())
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.ThirdPersonSpectator, null, position);
            }
        }

        public virtual void UpdateCockpitModel()
        {
            if (this.m_cockpitInteriorModel != null)
            {
                MyRenderComponentCockpit render = base.Render as MyRenderComponentCockpit;
                if ((render != null) && (((render.RenderObjectIDs.Length >= 2) && (render.ExteriorRenderId != uint.MaxValue)) && (render.InteriorRenderId != uint.MaxValue)))
                {
                    if (ReferenceEquals(MySession.Static.CameraController, this) && (this.IsInFirstPersonView || this.ForceFirstPersonCamera))
                    {
                        MyRenderProxy.UpdateRenderObjectVisibility(render.ExteriorRenderId, false, false);
                        MyRenderProxy.UpdateRenderObjectVisibility(render.InteriorRenderId, render.Visible, false);
                    }
                    else
                    {
                        MyRenderProxy.UpdateRenderObjectVisibility(render.ExteriorRenderId, render.Visible, false);
                        MyRenderProxy.UpdateRenderObjectVisibility(render.InteriorRenderId, false, false);
                    }
                }
            }
        }

        private void UpdateNearFlag()
        {
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (this.m_updateSink)
            {
                base.ResourceSink.Update();
                this.m_updateSink = false;
            }
            if (((this.m_savedPilot != null) && (!base.MarkedForClose && (!base.Closed && !this.m_savedPilot.MarkedForClose))) && !this.m_savedPilot.Closed)
            {
                if ((this.m_savedPilot.NeedsUpdate & MyEntityUpdateEnum.BEFORE_NEXT_FRAME) != MyEntityUpdateEnum.NONE)
                {
                    this.m_savedPilot.UpdateOnceBeforeFrame();
                    this.m_savedPilot.NeedsUpdate &= ~MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                    MySession.Static.Players.UpdatePlayerControllers(base.EntityId);
                    MySession.Static.Players.UpdatePlayerControllers(this.m_savedPilot.EntityId);
                }
                this.AttachPilot(this.m_savedPilot, false, true, false);
            }
            this.m_savedPilot = null;
            this.m_defferAttach = false;
            this.UpdateScreen();
        }

        public void UpdateScreen()
        {
            if (this.m_multiPanel != null)
            {
                this.m_multiPanel.UpdateScreen(base.hasPower && this.CheckIsWorking());
            }
        }

        protected override void UpdateSoundState()
        {
            base.UpdateSoundState();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.TryGetDummies();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        void IMyCameraController.ControlCamera(MyCamera currentCamera)
        {
            if (!base.m_enableFirstPerson)
            {
                this.IsInFirstPersonView = false;
            }
            if (base.Closed && (MySession.Static.LocalCharacter != null))
            {
                Vector3D? position = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.LocalCharacter, position);
            }
            currentCamera.SetViewMatrix(this.GetViewMatrix());
            currentCamera.CameraSpring.Enabled = true;
            currentCamera.CameraSpring.SetCurrentCameraControllerVelocity((base.CubeGrid.Physics != null) ? base.CubeGrid.Physics.LinearVelocity : Vector3.Zero);
            if (this.m_currentCameraShakePower > 0f)
            {
                currentCamera.CameraShake.AddShake(this.m_currentCameraShakePower);
                this.m_currentCameraShakePower = 0f;
            }
            if (((this.Pilot != null) && this.Pilot.InScene) && ReferenceEquals(this.Pilot, MySession.Static.LocalCharacter))
            {
                this.Pilot.EnableHead(!this.IsInFirstPersonView && !this.ForceFirstPersonCamera);
            }
        }

        bool IMyCameraController.HandlePickUp() => 
            false;

        bool IMyCameraController.HandleUse() => 
            false;

        void IMyCameraController.OnAssumeControl(IMyCameraController previousCameraController)
        {
            this.OnAssumeControl(previousCameraController);
            this.m_currentCameraShakePower = 0f;
        }

        void IMyCameraController.OnReleaseControl(IMyCameraController newCameraController)
        {
            this.OnReleaseControl(newCameraController);
            if ((this.Pilot != null) && this.Pilot.InScene)
            {
                this.Pilot.EnableHead(true);
            }
        }

        void IMyCameraController.Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            this.Rotate(rotationIndicator, rollIndicator);
        }

        void IMyCameraController.RotateStopped()
        {
            this.RotateStopped();
        }

        public MyAutopilotBase AiPilot =>
            this.m_aiPilot;

        public bool PilotJetpackEnabledBackup =>
            ((this.m_pilotJetpackEnabledBackup == null) ? false : this.m_pilotJetpackEnabledBackup.Value);

        public virtual bool IsInFirstPersonView
        {
            get => 
                this.m_isInFirstPersonView;
            set
            {
                bool isInFirstPersonView = this.m_isInFirstPersonView;
                this.m_isInFirstPersonView = value;
                if ((MySession.Static != null) && !MySession.Static.Enable3RdPersonView)
                {
                    this.m_isInFirstPersonView = true;
                }
                if ((this.m_isInFirstPersonView != isInFirstPersonView) && !this.ForceFirstPersonCamera)
                {
                    this.UpdateCameraAfterChange(true);
                }
            }
        }

        public override bool ForceFirstPersonCamera
        {
            get => 
                ((base.ForceFirstPersonCamera || MyThirdPersonSpectator.Static.IsCameraForced()) && (this.m_forcedFpsTimeoutMs <= 0));
            set
            {
                if (value && !base.ForceFirstPersonCamera)
                {
                    this.m_forcedFpsTimeoutMs = 500;
                }
                base.ForceFirstPersonCamera = value;
            }
        }

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_conveyorEndpoint;

        bool Sandbox.ModAPI.Ingame.IMyCockpit.IsMainCockpit
        {
            get => 
                base.IsMainCockpit;
            set
            {
                if (base.IsMainCockpitFree() && this.CanBeMainCockpit())
                {
                    base.IsMainCockpit = value;
                }
            }
        }

        public float OxygenFillLevel
        {
            get => 
                ((float) this.m_oxygenFillLevel);
            private set => 
                (this.m_oxygenFillLevel.Value = MathHelper.Clamp(value, 0f, 1f));
        }

        float Sandbox.ModAPI.Ingame.IMyCockpit.OxygenFilledRatio =>
            this.OxygenFillLevel;

        public float OxygenAmount
        {
            get => 
                (this.OxygenFillLevel * this.BlockDefinition.OxygenCapacity);
            set
            {
                if (this.BlockDefinition.OxygenCapacity != 0f)
                {
                    this.ChangeGasFillLevel(MathHelper.Clamp((float) (value / this.BlockDefinition.OxygenCapacity), (float) 0f, (float) 1f));
                }
                base.ResourceSink.Update();
            }
        }

        public bool CanPressurizeRoom =>
            false;

        float Sandbox.ModAPI.Ingame.IMyCockpit.OxygenCapacity =>
            this.BlockDefinition.OxygenCapacity;

        public float OxygenAmountMissing =>
            ((1f - this.OxygenFillLevel) * this.BlockDefinition.OxygenCapacity);

        MyMultiTextPanelComponent IMyMultiTextPanelComponentOwner.MultiTextPanel =>
            this.m_multiPanel;

        public MyTextPanelComponent PanelComponent =>
            this.m_multiPanel?.PanelComponent;

        public bool IsTextPanelOpen
        {
            get => 
                this.m_isTextPanelOpen;
            set
            {
                if (this.m_isTextPanelOpen != value)
                {
                    this.m_isTextPanelOpen = value;
                    base.RaisePropertiesChanged();
                }
            }
        }

        public override float HeadLocalXAngle
        {
            get => 
                this.m_headLocalXAngle;
            set => 
                (this.m_headLocalXAngle = value);
        }

        public override float HeadLocalYAngle
        {
            get => 
                this.m_headLocalYAngle;
            set => 
                (this.m_headLocalYAngle = value);
        }

        public Vector3I[] NeighbourPositions =>
            this.m_neighbourPositions;

        public MyCockpitDefinition BlockDefinition =>
            (base.BlockDefinition as MyCockpitDefinition);

        public override MyCharacter Pilot
        {
            get
            {
                if ((this.m_pilot != null) || (this.m_savedPilot == null))
                {
                    return this.m_pilot;
                }
                return this.m_savedPilot;
            }
        }

        public VRage.Game.Entity.MyEntity IsBeingUsedBy =>
            this.m_pilot;

        bool IMyCameraController.IsInFirstPersonView
        {
            get => 
                this.IsInFirstPersonView;
            set => 
                (this.IsInFirstPersonView = value);
        }

        bool IMyCameraController.ForceFirstPersonCamera
        {
            get => 
                (this.ForceFirstPersonCamera || !MySession.Static.Settings.Enable3rdPersonView);
            set => 
                (this.ForceFirstPersonCamera = value);
        }

        bool IMyCameraController.AllowCubeBuilding =>
            false;

        float Sandbox.ModAPI.IMyCockpit.OxygenFilledRatio
        {
            get => 
                this.OxygenFillLevel;
            set => 
                (this.OxygenAmount = value * this.BlockDefinition.OxygenCapacity);
        }

        int Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider.SurfaceCount =>
            ((this.m_multiPanel != null) ? this.m_multiPanel.SurfaceCount : 0);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCockpit.<>c <>9 = new MyCockpit.<>c();
            public static Func<MyCockpit, Action<int, int[]>> <>9__87_0;
            public static Func<MyCockpit, Action<int, int[]>> <>9__89_0;
            public static Func<MyCockpit, Action<bool, bool, ulong, bool>> <>9__92_0;
            public static Func<MyCockpit, Action<bool, bool, ulong, bool>> <>9__93_0;
            public static Func<MyCockpit, Action<string, bool>> <>9__100_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__142_1;
            public static Func<MyCockpit, Action<MyObjectBuilder_AutopilotBase>> <>9__146_0;
            public static Func<MyCockpit, Action<MyObjectBuilder_AutopilotBase>> <>9__147_0;
            public static Func<MyCockpit, Action<UseActionEnum, long>> <>9__205_0;
            public static Func<MyCockpit, Action<UseActionEnum, long>> <>9__219_0;

            internal Action<MyObjectBuilder_AutopilotBase> <AttachAutopilot>b__146_0(MyCockpit x) => 
                new Action<MyObjectBuilder_AutopilotBase>(x.SetAutopilot_Client);

            internal Action<bool, bool, ulong, bool> <OnChangeOpenRequest>b__93_0(MyCockpit x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenSuccess);

            internal Action<MyObjectBuilder_AutopilotBase> <RemoveAutopilot>b__147_0(MyCockpit x) => 
                new Action<MyObjectBuilder_AutopilotBase>(x.SetAutopilot_Client);

            internal Action<long, long> <RemovePilot>b__142_1(IMyEventOwner s) => 
                new Action<long, long>(MyPlayerCollection.SetDampeningEntityClient);

            internal Action<UseActionEnum, long> <RequestUse>b__205_0(MyCockpit x) => 
                new Action<UseActionEnum, long>(x.AttachPilotEvent);

            internal Action<UseActionEnum, long> <Sandbox.ModAPI.IMyCockpit.AttachPilot>b__219_0(MyCockpit x) => 
                new Action<UseActionEnum, long>(x.AttachPilotEvent);

            internal Action<int, int[]> <SendAddImagesToSelectionRequest>b__89_0(MyCockpit x) => 
                new Action<int, int[]>(x.OnSelectImageRequest);

            internal Action<string, bool> <SendChangeDescriptionMessage>b__100_0(MyCockpit x) => 
                new Action<string, bool>(x.OnChangeDescription);

            internal Action<bool, bool, ulong, bool> <SendChangeOpenMessage>b__92_0(MyCockpit x) => 
                new Action<bool, bool, ulong, bool>(x.OnChangeOpenRequest);

            internal Action<int, int[]> <SendRemoveSelectedImageRequest>b__87_0(MyCockpit x) => 
                new Action<int, int[]>(x.OnRemoveSelectedImageRequest);
        }
    }
}

