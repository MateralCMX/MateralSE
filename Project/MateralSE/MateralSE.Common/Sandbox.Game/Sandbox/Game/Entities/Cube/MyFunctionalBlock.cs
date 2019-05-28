namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.World;
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
    using VRage.Sync;
    using VRage.Utils;

    [MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyFunctionalBlock), typeof(Sandbox.ModAPI.Ingame.IMyFunctionalBlock) })]
    public class MyFunctionalBlock : MyTerminalBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity
    {
        protected MySoundPair m_baseIdleSound = new MySoundPair();
        protected MySoundPair m_actionSound = new MySoundPair();
        public MyEntity3DSoundEmitter m_soundEmitter;
        private readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> m_enabled;
        [CompilerGenerated]
        private Action<MyTerminalBlock> EnabledChanged;

        public event Action<MyTerminalBlock> EnabledChanged
        {
            [CompilerGenerated] add
            {
                Action<MyTerminalBlock> enabledChanged = this.EnabledChanged;
                while (true)
                {
                    Action<MyTerminalBlock> a = enabledChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Combine(a, value);
                    enabledChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.EnabledChanged, action3, a);
                    if (ReferenceEquals(enabledChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyTerminalBlock> enabledChanged = this.EnabledChanged;
                while (true)
                {
                    Action<MyTerminalBlock> source = enabledChanged;
                    Action<MyTerminalBlock> action3 = (Action<MyTerminalBlock>) Delegate.Remove(source, value);
                    enabledChanged = Interlocked.CompareExchange<Action<MyTerminalBlock>>(ref this.EnabledChanged, action3, source);
                    if (ReferenceEquals(enabledChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<Sandbox.ModAPI.IMyTerminalBlock> Sandbox.ModAPI.IMyFunctionalBlock.EnabledChanged
        {
            add
            {
                this.EnabledChanged += this.GetDelegate(value);
            }
            remove
            {
                this.EnabledChanged -= this.GetDelegate(value);
            }
        }

        public MyFunctionalBlock()
        {
            this.CreateTerminalControls();
            this.m_enabled.ValueChanged += x => this.EnabledSyncChanged();
        }

        protected override bool CheckIsWorking() => 
            (this.Enabled && base.CheckIsWorking());

        protected override void Closing()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
            base.Closing();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyFunctionalBlock>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyFunctionalBlock> onOff = new MyTerminalControlOnOffSwitch<MyFunctionalBlock>("OnOff", MySpaceTexts.BlockAction_Toggle, tooltip, on, on) {
                    Getter = x => x.Enabled,
                    Setter = (x, v) => x.Enabled = v
                };
                onOff.EnableToggleAction<MyFunctionalBlock>();
                onOff.EnableOnOffActions<MyFunctionalBlock>();
                MyTerminalControlFactory.AddControl<MyFunctionalBlock>(0, onOff);
                MyTerminalControlFactory.AddControl<MyFunctionalBlock>(1, new MyTerminalControlSeparator<MyFunctionalBlock>());
            }
        }

        private void CubeBlock_IsWorkingChanged(MyCubeBlock obj)
        {
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
            else
            {
                this.OnStopWorking();
            }
        }

        private void EnabledSyncChanged()
        {
            base.UpdateIsWorking();
            this.OnEnabledChanged();
        }

        public virtual int GetBlockSpecificState() => 
            -1;

        private Action<MyTerminalBlock> GetDelegate(Action<Sandbox.ModAPI.IMyTerminalBlock> value) => 
            ((Action<MyTerminalBlock>) Delegate.CreateDelegate(typeof(Action<MyTerminalBlock>), value.Target, value.Method));

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_FunctionalBlock objectBuilderCubeBlock = (MyObjectBuilder_FunctionalBlock) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Enabled = this.Enabled;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_FunctionalBlock block = (MyObjectBuilder_FunctionalBlock) objectBuilder;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, true, 1f);
            this.m_enabled.SetLocalValue(block.Enabled);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.CubeBlock_IsWorkingChanged);
            this.m_baseIdleSound = base.BlockDefinition.PrimarySound;
            this.m_actionSound = base.BlockDefinition.ActionSound;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual void OnEnabledChanged()
        {
            if (base.IsWorking)
            {
                this.OnStartWorking();
            }
            else
            {
                this.OnStopWorking();
            }
            Action<MyTerminalBlock> enabledChanged = this.EnabledChanged;
            if (enabledChanged != null)
            {
                enabledChanged(this);
            }
            base.RaisePropertiesChanged();
        }

        public override void OnRemovedFromScene(object source)
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
            base.OnRemovedFromScene(source);
        }

        protected virtual void OnStartWorking()
        {
            if ((base.InScene && ((base.CubeGrid.Physics != null) && ((this.m_soundEmitter != null) && (this.m_baseIdleSound != null)))) && !ReferenceEquals(this.m_baseIdleSound, MySoundPair.Empty))
            {
                bool? nullable = null;
                this.m_soundEmitter.PlaySound(this.m_baseIdleSound, true, false, false, false, false, nullable);
            }
        }

        protected virtual void OnStopWorking()
        {
            if ((this.m_soundEmitter != null) && ((base.BlockDefinition.DamagedSound == null) || (this.m_soundEmitter.SoundId != base.BlockDefinition.DamagedSound.SoundId)))
            {
                this.m_soundEmitter.StopSound(false, true);
            }
        }

        void Sandbox.ModAPI.Ingame.IMyFunctionalBlock.RequestEnable(bool enable)
        {
            this.Enabled = enable;
        }

        public override void SetDamageEffect(bool show)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                base.SetDamageEffect(show);
                if ((this.m_soundEmitter != null) && (base.BlockDefinition.DamagedSound != null))
                {
                    if (show)
                    {
                        bool? nullable = null;
                        this.m_soundEmitter.PlaySound(base.BlockDefinition.DamagedSound, true, false, false, false, false, nullable);
                    }
                    else if (this.m_soundEmitter.SoundId == base.BlockDefinition.DamagedSound.SoundId)
                    {
                        this.m_soundEmitter.StopSound(false, true);
                    }
                }
            }
        }

        public override void StopDamageEffect(bool stopSound = true)
        {
            base.StopDamageEffect(stopSound);
            if ((stopSound && ((this.m_soundEmitter != null) && (base.BlockDefinition.DamagedSound != null))) && ((this.m_soundEmitter.SoundId == base.BlockDefinition.DamagedSound.Arcade) || (this.m_soundEmitter.SoundId != base.BlockDefinition.DamagedSound.Realistic)))
            {
                this.m_soundEmitter.StopSound(true, true);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if ((this.m_soundEmitter != null) && base.SilenceInChange)
            {
                base.SilenceInChange = this.m_soundEmitter.FastUpdate(base.IsSilenced);
                if ((!base.SilenceInChange && !base.UsedUpdateEveryFrame) && !base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.UpdateSoundOcclusion();
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((this.m_soundEmitter != null) && (MySector.MainCamera != null))
            {
                this.UpdateSoundEmitters();
            }
        }

        public virtual void UpdateSoundEmitters()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Update();
            }
        }

        internal MyEntity3DSoundEmitter SoundEmitter =>
            this.m_soundEmitter;

        public bool Enabled
        {
            get => 
                ((bool) this.m_enabled);
            set => 
                (this.m_enabled.Value = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFunctionalBlock.<>c <>9 = new MyFunctionalBlock.<>c();
            public static MyTerminalValueControl<MyFunctionalBlock, bool>.GetterDelegate <>9__15_0;
            public static MyTerminalValueControl<MyFunctionalBlock, bool>.SetterDelegate <>9__15_1;

            internal bool <CreateTerminalControls>b__15_0(MyFunctionalBlock x) => 
                x.Enabled;

            internal void <CreateTerminalControls>b__15_1(MyFunctionalBlock x, bool v)
            {
                x.Enabled = v;
            }
        }
    }
}

