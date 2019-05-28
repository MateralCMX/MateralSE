namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    internal class MySessionComponentThrower : MySessionComponentBase
    {
        public static bool USE_SPECTATOR_FOR_THROW;
        private bool m_isActive;
        private int m_startTime;

        public void Activate()
        {
            this.m_isActive = true;
        }

        private void CurrentToolbar_SelectedSlotChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            if (!(toolbar.SelectedItem is MyToolbarItemPrefabThrower))
            {
                this.Enabled = false;
            }
        }

        private void CurrentToolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            if (!(toolbar.GetItemAtIndex(toolbar.SlotToIndex(args.SlotNumber.Value)) is MyToolbarItemPrefabThrower))
            {
                this.Enabled = false;
            }
        }

        private void CurrentToolbar_Unselected(MyToolbar toolbar)
        {
            this.Enabled = false;
        }

        public void Deactivate()
        {
            this.m_isActive = false;
        }

        public override void HandleInput()
        {
            if ((this.m_isActive && (MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay)) && ((MyInput.Static.ENABLE_DEVELOPER_KEYS || !MySession.Static.SurvivalMode) || MySession.Static.IsUserAdmin(Sync.MyId)))
            {
                base.HandleInput();
                if (MyControllerHelper.IsControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false))
                {
                    this.m_startTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                }
                if (MyControllerHelper.IsControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_RELEASED, false))
                {
                    MyObjectBuilder_CubeGrid[] gridPrefab = MyPrefabManager.Static.GetGridPrefab(this.CurrentDefinition.PrefabToThrow);
                    Vector3D zero = Vector3D.Zero;
                    Vector3D forward = Vector3D.Zero;
                    if (USE_SPECTATOR_FOR_THROW)
                    {
                        zero = MySpectator.Static.Position;
                        forward = MySpectator.Static.Orientation.Forward;
                    }
                    else if ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity))
                    {
                        zero = MySector.MainCamera.Position;
                        forward = MySector.MainCamera.WorldMatrix.Forward;
                    }
                    else
                    {
                        if (MySession.Static.ControlledEntity == null)
                        {
                            return;
                        }
                        zero = MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false).Translation;
                        forward = MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false).Forward;
                    }
                    Vector3D vectord3 = zero + forward;
                    Vector3D vectord4 = (forward * MathHelper.Clamp(((((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_startTime)) / 1000f) / this.CurrentDefinition.PushTime) * this.CurrentDefinition.MaxSpeed, this.CurrentDefinition.MinSpeed, this.CurrentDefinition.MaxSpeed)) + MySession.Static.ControlledEntity.Entity.Physics.LinearVelocity;
                    float num2 = 0f;
                    if (this.CurrentDefinition.Mass != null)
                    {
                        num2 = MyDestructionHelper.MassToHavok(this.CurrentDefinition.Mass.Value);
                    }
                    gridPrefab[0].EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<MyObjectBuilder_CubeGrid, Vector3D, Vector3D, float, MyCueId>(s => new Action<MyObjectBuilder_CubeGrid, Vector3D, Vector3D, float, MyCueId>(MySessionComponentThrower.OnThrowMessageSuccess), gridPrefab[0], vectord3, vectord4, num2, this.CurrentDefinition.ThrowSound, targetEndpoint, position);
                    this.m_startTime = 0;
                }
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
            MyToolbarComponent.CurrentToolbar.SelectedSlotChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
            MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.Unselected += new Action<MyToolbar>(this.CurrentToolbar_Unselected);
        }

        [Event(null, 0xce), Reliable, Server, Broadcast]
        private static void OnThrowMessageSuccess(MyObjectBuilder_CubeGrid grid, Vector3D position, Vector3D linearVelocity, float mass, MyCueId throwSound)
        {
            Static.Throw(grid, position, linearVelocity, mass, throwSound);
        }

        public void Throw(MyObjectBuilder_CubeGrid grid, Vector3D position, Vector3D linearVelocity, float mass, MyCueId throwSound)
        {
            if (Sync.IsServer)
            {
                MyEntity entity = MyEntities.CreateFromObjectBuilder(grid, false);
                if (entity != null)
                {
                    entity.PositionComp.SetPosition(position, null, false, true);
                    entity.Physics.LinearVelocity = (Vector3) linearVelocity;
                    if (mass > 0f)
                    {
                        entity.Physics.RigidBody.Mass = mass;
                    }
                    MyEntities.Add(entity, true);
                    if (!throwSound.IsNull)
                    {
                        MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                        if (emitter != null)
                        {
                            emitter.SetPosition(new Vector3D?(position));
                            bool? nullable = null;
                            emitter.PlaySoundWithDistance(throwSound, false, false, false, true, false, false, nullable);
                        }
                    }
                }
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (MyToolbarComponent.CurrentToolbar != null)
            {
                MyToolbarComponent.CurrentToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
                MyToolbarComponent.CurrentToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
                MyToolbarComponent.CurrentToolbar.Unselected -= new Action<MyToolbar>(this.CurrentToolbar_Unselected);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
        }

        public static MySessionComponentThrower Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }

        public bool Enabled
        {
            get => 
                this.m_isActive;
            set => 
                (this.m_isActive = value);
        }

        public MyPrefabThrowerDefinition CurrentDefinition { get; set; }

        public override bool IsRequiredByGame =>
            false;

        public override Type[] Dependencies =>
            new Type[] { typeof(MyToolbarComponent) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentThrower.<>c <>9 = new MySessionComponentThrower.<>c();
            public static Func<IMyEventOwner, Action<MyObjectBuilder_CubeGrid, Vector3D, Vector3D, float, MyCueId>> <>9__19_0;

            internal Action<MyObjectBuilder_CubeGrid, Vector3D, Vector3D, float, MyCueId> <HandleInput>b__19_0(IMyEventOwner s) => 
                new Action<MyObjectBuilder_CubeGrid, Vector3D, Vector3D, float, MyCueId>(MySessionComponentThrower.OnThrowMessageSuccess);
        }
    }
}

