namespace Sandbox.Game.Weapons
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_UserControllableGun)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyUserControllableGun), typeof(Sandbox.ModAPI.Ingame.IMyUserControllableGun) })]
    public abstract class MyUserControllableGun : MyFunctionalBlock, Sandbox.ModAPI.IMyUserControllableGun, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUserControllableGun
    {
        protected VRage.Sync.Sync<bool, SyncDirection.FromServer> m_isShooting;
        protected static readonly MyStringId ID_RED_DOT = MyStringId.GetOrCompute("RedDot");
        protected readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_forceShoot;
        protected bool m_shootingBegun;
        [CompilerGenerated]
        private Action<int> ReloadStarted;

        public event Action<int> ReloadStarted
        {
            [CompilerGenerated] add
            {
                Action<int> reloadStarted = this.ReloadStarted;
                while (true)
                {
                    Action<int> a = reloadStarted;
                    Action<int> action3 = (Action<int>) Delegate.Combine(a, value);
                    reloadStarted = Interlocked.CompareExchange<Action<int>>(ref this.ReloadStarted, action3, a);
                    if (ReferenceEquals(reloadStarted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<int> reloadStarted = this.ReloadStarted;
                while (true)
                {
                    Action<int> source = reloadStarted;
                    Action<int> action3 = (Action<int>) Delegate.Remove(source, value);
                    reloadStarted = Interlocked.CompareExchange<Action<int>>(ref this.ReloadStarted, action3, source);
                    if (ReferenceEquals(reloadStarted, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyUserControllableGun()
        {
            this.CreateTerminalControls();
            this.m_isShooting.ValueChanged += x => this.ShootingChanged();
            base.NeedsWorldMatrix = true;
        }

        public virtual void BeginShoot(MyShootActionEnum action)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            this.m_shootingBegun = true;
            this.Shoot();
            this.RememberIdle();
            this.TakeControlFromTerminal();
            if (MyVisualScriptLogicProvider.WeaponBlockActivated != null)
            {
                MyVisualScriptLogicProvider.WeaponBlockActivated(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name, base.BlockDefinition.Id.TypeId.ToString(), base.BlockDefinition.Id.SubtypeId.ToString());
            }
        }

        public abstract bool CanOperate();
        public virtual bool CanShoot(out MyGunStatusEnum status)
        {
            status = MyGunStatusEnum.OK;
            return true;
        }

        public abstract bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status);
        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyUserControllableGun>())
            {
                base.CreateTerminalControls();
                if (MyFakes.ENABLE_WEAPON_TERMINAL_CONTROL)
                {
                    MyStringId? title = null;
                    MyTerminalControlButton<MyUserControllableGun> button = new MyTerminalControlButton<MyUserControllableGun>("ShootOnce", MySpaceTexts.Terminal_ShootOnce, MySpaceTexts.Blank, b => b.OnShootOncePressed());
                    button.EnableAction<MyUserControllableGun>(null, title, null);
                    MyTerminalControlFactory.AddControl<MyUserControllableGun>(button);
                    MyStringId tooltip = new MyStringId();
                    title = null;
                    title = null;
                    MyTerminalControlOnOffSwitch<MyUserControllableGun> switch1 = new MyTerminalControlOnOffSwitch<MyUserControllableGun>("Shoot", MySpaceTexts.Terminal_Shoot, tooltip, title, title);
                    MyTerminalControlOnOffSwitch<MyUserControllableGun> switch2 = new MyTerminalControlOnOffSwitch<MyUserControllableGun>("Shoot", MySpaceTexts.Terminal_Shoot, tooltip, title, title);
                    switch2.Getter = x => (bool) x.m_forceShoot;
                    MyTerminalControlOnOffSwitch<MyUserControllableGun> local5 = switch2;
                    MyTerminalControlOnOffSwitch<MyUserControllableGun> local6 = switch2;
                    local6.Setter = (x, v) => x.OnShootPressed(v);
                    MyTerminalControlOnOffSwitch<MyUserControllableGun> onOff = local6;
                    onOff.EnableToggleAction<MyUserControllableGun>();
                    onOff.EnableOnOffActions<MyUserControllableGun>();
                    MyTerminalControlFactory.AddControl<MyUserControllableGun>(onOff);
                    MyTerminalControlFactory.AddControl<MyUserControllableGun>(new MyTerminalControlSeparator<MyUserControllableGun>());
                }
            }
        }

        public virtual void EndShoot(MyShootActionEnum action)
        {
            this.m_shootingBegun = false;
            this.RestoreIdle();
            base.Render.NeedsDrawFromParent = false;
            this.StopShootFromTerminal();
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_UserControllableGun objectBuilderCubeBlock = (MyObjectBuilder_UserControllableGun) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.IsShooting = (bool) this.m_isShooting;
            objectBuilderCubeBlock.IsShootingFromTerminal = (bool) this.m_forceShoot;
            return objectBuilderCubeBlock;
        }

        public virtual Vector3D GetWeaponMuzzleWorldPosition() => 
            base.WorldMatrix.Translation;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_UserControllableGun gun = objectBuilder as MyObjectBuilder_UserControllableGun;
            this.m_forceShoot.SetLocalValue(gun.IsShootingFromTerminal);
            this.m_isShooting.SetLocalValue(gun.IsShooting);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.m_forceShoot.ValueChanged += new Action<SyncBase>(this.OnForceShootChanged);
        }

        public virtual bool IsStationary() => 
            false;

        public override void OnDestroy()
        {
            MyInventory inventory = this.GetInventory(0);
            if (inventory != null)
            {
                base.ReleaseInventory(inventory, false);
            }
            base.OnDestroy();
        }

        private void OnForceShootChanged(SyncBase obj)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public void OnReloadStarted(int reloadTime)
        {
            Action<int> reloadStarted = this.ReloadStarted;
            if (reloadStarted != null)
            {
                reloadStarted(reloadTime);
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            MyInventory inventory = this.GetInventory(0);
            if (inventory != null)
            {
                base.ReleaseInventory(inventory, false);
            }
            base.OnRemovedByCubeBuilder();
        }

        private void OnShootOncePressed()
        {
            this.SyncRotationAndOrientation();
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyUserControllableGun>(this, x => new Action(x.ShootOncePressedEvent), targetEndpoint);
        }

        private void OnShootPressed(bool isShooting)
        {
            this.m_forceShoot.Value = isShooting;
            if (!isShooting)
            {
                this.EndShoot(MyShootActionEnum.PrimaryAction);
            }
            else
            {
                this.BeginShoot(MyShootActionEnum.PrimaryAction);
                this.SyncRotationAndOrientation();
            }
        }

        protected virtual void RememberIdle()
        {
        }

        protected virtual void RestoreIdle()
        {
        }

        protected virtual void RotateModels()
        {
        }

        public void SetShooting(bool shooting)
        {
            this.OnShootPressed(shooting);
        }

        private void Shoot()
        {
            MyGunStatusEnum enum2;
            if ((this.CanShoot(MyShootActionEnum.PrimaryAction, base.OwnerId, out enum2) && this.CanShoot(out enum2)) && this.CanOperate())
            {
                this.ShootFromTerminal((Vector3) base.WorldMatrix.Forward);
            }
        }

        public virtual void ShootFromTerminal(Vector3 direction)
        {
            if (MyVisualScriptLogicProvider.WeaponBlockActivated != null)
            {
                MyVisualScriptLogicProvider.WeaponBlockActivated(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name, base.BlockDefinition.Id.TypeId.ToString(), base.BlockDefinition.Id.SubtypeId.ToString());
            }
        }

        protected void ShootingChanged()
        {
            if (this.m_isShooting != null)
            {
                this.BeginShoot(MyShootActionEnum.PrimaryAction);
            }
            else
            {
                this.EndShoot(MyShootActionEnum.PrimaryAction);
            }
        }

        [Event(null, 110), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void ShootOncePressedEvent()
        {
            this.Shoot();
        }

        public abstract void StopShootFromTerminal();
        public virtual void SyncRotationAndOrientation()
        {
        }

        public virtual void TakeControlFromTerminal()
        {
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (base.IsWorking && ((this.m_isShooting != null) || (this.m_forceShoot != null)))
            {
                this.TakeControlFromTerminal();
                this.Shoot();
                this.RotateModels();
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            base.Render.NeedsDrawFromParent = (this.m_isShooting != null) || ((bool) this.m_forceShoot);
            if ((this.m_isShooting != null) || (this.m_forceShoot != null))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyUserControllableGun.IsShooting =>
            ((bool) this.m_isShooting);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyUserControllableGun.<>c <>9 = new MyUserControllableGun.<>c();
            public static Action<MyUserControllableGun> <>9__8_0;
            public static MyTerminalValueControl<MyUserControllableGun, bool>.GetterDelegate <>9__8_1;
            public static MyTerminalValueControl<MyUserControllableGun, bool>.SetterDelegate <>9__8_2;
            public static Func<MyUserControllableGun, Action> <>9__14_0;

            internal void <CreateTerminalControls>b__8_0(MyUserControllableGun b)
            {
                b.OnShootOncePressed();
            }

            internal bool <CreateTerminalControls>b__8_1(MyUserControllableGun x) => 
                ((bool) x.m_forceShoot);

            internal void <CreateTerminalControls>b__8_2(MyUserControllableGun x, bool v)
            {
                x.OnShootPressed(v);
            }

            internal Action <OnShootOncePressed>b__14_0(MyUserControllableGun x) => 
                new Action(x.ShootOncePressedEvent);
        }
    }
}

