namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_SoundBlock)), MyTerminalInterface(new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMySoundBlock), typeof(SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock) })]
    public class MySoundBlock : MyFunctionalBlock, SpaceEngineers.Game.ModAPI.IMySoundBlock, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock
    {
        private static StringBuilder m_helperSB = new StringBuilder();
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_soundRadius;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_volume;
        private readonly VRage.Sync.Sync<string, SyncDirection.BothWays> m_cueIdString;
        private readonly VRage.Sync.Sync<float, SyncDirection.BothWays> m_loopPeriod;
        private MyEntity3DSoundEmitter m_soundEmitter;
        private bool m_willStartSound;
        private bool m_isPlaying;
        private bool m_isLooping;
        private long m_soundStartTime;
        private string m_playingSoundName;
        private static MyTerminalControlButton<MySoundBlock> m_playButton;
        private static MyTerminalControlButton<MySoundBlock> m_stopButton;
        private static MyTerminalControlSlider<MySoundBlock> m_loopableTimeSlider;

        public MySoundBlock()
        {
            this.CreateTerminalControls();
            this.m_volume.ValueChanged += x => this.VolumeChanged();
            this.m_soundRadius.ValueChanged += x => this.RadiusChanged();
            this.m_cueIdString.ValueChanged += x => this.SelectionChanged();
        }

        protected override bool CheckIsWorking() => 
            (base.CheckIsWorking() && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId));

        protected override void Closing()
        {
            base.Closing();
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.StopSound(true, true);
                this.m_soundEmitter.StoppedPlaying -= new Action<MyEntity3DSoundEmitter>(this.m_soundEmitter_StoppedPlaying);
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySoundBlock>())
            {
                base.CreateTerminalControls();
                MyTerminalControlSlider<MySoundBlock> slider1 = new MyTerminalControlSlider<MySoundBlock>("VolumeSlider", MySpaceTexts.BlockPropertyTitle_SoundBlockVolume, MySpaceTexts.BlockPropertyDescription_SoundBlockVolume);
                slider1.SetLimits((float) 0f, (float) 100f);
                slider1.DefaultValue = 100f;
                slider1.Getter = x => x.Volume * 100f;
                MyTerminalControlSlider<MySoundBlock> local32 = slider1;
                MyTerminalControlSlider<MySoundBlock> local33 = slider1;
                local33.Setter = (x, v) => x.Volume = v * 0.01f;
                MyTerminalControlSlider<MySoundBlock> local30 = local33;
                MyTerminalControlSlider<MySoundBlock> local31 = local33;
                local31.Writer = (x, result) => result.AppendInt32(((int) (x.Volume * 100.0))).Append(" %");
                MyTerminalControlSlider<MySoundBlock> slider = local31;
                slider.EnableActions<MySoundBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySoundBlock>(slider);
                MyTerminalControlSlider<MySoundBlock> slider2 = new MyTerminalControlSlider<MySoundBlock>("RangeSlider", MySpaceTexts.BlockPropertyTitle_SoundBlockRange, MySpaceTexts.BlockPropertyDescription_SoundBlockRange);
                MyTerminalControlSlider<MySoundBlock> slider3 = new MyTerminalControlSlider<MySoundBlock>("RangeSlider", MySpaceTexts.BlockPropertyTitle_SoundBlockRange, MySpaceTexts.BlockPropertyDescription_SoundBlockRange);
                slider3.SetLimits(x => x.BlockDefinition.MinRange, x => x.BlockDefinition.MaxRange);
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local7 = (MyTerminalValueControl<MySoundBlock, float>.GetterDelegate) slider3;
                local7.DefaultValue = new float?((float) 50);
                local7.Getter = x => x.Range;
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local28 = local7;
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local29 = local7;
                local29.Setter = (x, v) => x.Range = v;
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local26 = local29;
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local27 = local29;
                local27.Writer = (x, result) => result.AppendInt32(((int) x.Range)).Append(" m");
                MyTerminalValueControl<MySoundBlock, float>.GetterDelegate local11 = local27;
                ((MyTerminalControlSlider<MySoundBlock>) local11).EnableActions<MySoundBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySoundBlock>((MyTerminalControl<MySoundBlock>) local11);
                m_playButton = new MyTerminalControlButton<MySoundBlock>("PlaySound", MySpaceTexts.BlockPropertyTitle_SoundBlockPlay, MySpaceTexts.Blank, x => x.RequestPlaySound());
                m_playButton.Enabled = x => x.IsSoundSelected;
                MyStringId? title = null;
                m_playButton.EnableAction<MySoundBlock>(null, title, null);
                MyTerminalControlFactory.AddControl<MySoundBlock>(m_playButton);
                m_stopButton = new MyTerminalControlButton<MySoundBlock>("StopSound", MySpaceTexts.BlockPropertyTitle_SoundBlockStop, MySpaceTexts.Blank, x => x.RequestStopSound());
                m_stopButton.Enabled = x => x.IsSoundSelected;
                title = null;
                m_stopButton.EnableAction<MySoundBlock>(null, title, null);
                MyTerminalControlFactory.AddControl<MySoundBlock>(m_stopButton);
                m_loopableTimeSlider = new MyTerminalControlSlider<MySoundBlock>("LoopableSlider", MySpaceTexts.BlockPropertyTitle_SoundBlockLoopTime, MySpaceTexts.Blank);
                m_loopableTimeSlider.DefaultValue = 1f;
                m_loopableTimeSlider.Getter = x => x.LoopPeriod;
                m_loopableTimeSlider.Setter = (x, f) => x.LoopPeriod = f;
                m_loopableTimeSlider.Writer = (x, result) => MyValueFormatter.AppendTimeInBestUnit(x.LoopPeriod, result);
                m_loopableTimeSlider.Enabled = x => x.IsSelectedSoundLoopable();
                m_loopableTimeSlider.Normalizer = (x, f) => x.NormalizeLoopPeriod(f);
                m_loopableTimeSlider.Denormalizer = (x, f) => x.DenormalizeLoopPeriod(f);
                m_loopableTimeSlider.EnableActions<MySoundBlock>(0.05f, null, null);
                MyTerminalControlFactory.AddControl<MySoundBlock>(m_loopableTimeSlider);
                MyTerminalControlListbox<MySoundBlock> listbox1 = new MyTerminalControlListbox<MySoundBlock>("SoundsList", MySpaceTexts.BlockPropertyTitle_SoundBlockSoundList, MySpaceTexts.Blank, false, 8);
                MyTerminalControlListbox<MySoundBlock> listbox2 = new MyTerminalControlListbox<MySoundBlock>("SoundsList", MySpaceTexts.BlockPropertyTitle_SoundBlockSoundList, MySpaceTexts.Blank, false, 8);
                listbox2.ListContent = (x, list1, list2) => x.FillListContent(list1, list2);
                MyTerminalControlListbox<MySoundBlock> local24 = listbox2;
                MyTerminalControlListbox<MySoundBlock> control = listbox2;
                control.ItemSelected = (x, y) => x.SelectSound(y, true);
                MyTerminalControlFactory.AddControl<MySoundBlock>(control);
            }
        }

        private float DenormalizeLoopPeriod(float value) => 
            ((value != 0f) ? MathHelper.InterpLog(value, 1f, this.BlockDefinition.MaxLoopPeriod) : 1f);

        private void FillListContent(ICollection<MyGuiControlListbox.Item> listBoxContent, ICollection<MyGuiControlListbox.Item> listBoxSelectedItems)
        {
            foreach (MySoundCategoryDefinition definition in MyDefinitionManager.Static.GetSoundCategoryDefinitions())
            {
                if (definition.Public)
                {
                    foreach (MySoundCategoryDefinition.SoundDescription description in definition.Sounds)
                    {
                        m_helperSB.Clear().Append(description.SoundText);
                        MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(m_helperSB, null, null, description.SoundId, null);
                        listBoxContent.Add(item);
                        if (description.SoundId.Equals((string) this.m_cueIdString))
                        {
                            listBoxSelectedItems.Add(item);
                        }
                    }
                }
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SoundBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_SoundBlock;
            objectBuilderCubeBlock.Volume = this.Volume;
            objectBuilderCubeBlock.Range = this.Range;
            objectBuilderCubeBlock.LoopPeriod = this.LoopPeriod;
            objectBuilderCubeBlock.IsLoopableSound = this.m_isLooping;
            objectBuilderCubeBlock.ElapsedSoundSeconds = (float) (((double) (Stopwatch.GetTimestamp() - this.m_soundStartTime)) / ((double) Stopwatch.Frequency));
            if (this.m_isPlaying && (this.m_soundEmitter.IsPlaying || (this.m_isLooping && (this.LoopPeriod > objectBuilderCubeBlock.ElapsedSoundSeconds))))
            {
                objectBuilderCubeBlock.IsPlaying = true;
                objectBuilderCubeBlock.CueName = this.m_playingSoundName;
            }
            else
            {
                objectBuilderCubeBlock.IsPlaying = false;
                objectBuilderCubeBlock.ElapsedSoundSeconds = 0f;
                objectBuilderCubeBlock.CueName = this.m_cueIdString.Value;
            }
            return objectBuilderCubeBlock;
        }

        private MyCueId GetSoundQueue(string nameOrId)
        {
            MyCueId id2;
            MyCueId cueId = MySoundPair.GetCueId(nameOrId);
            if (!cueId.IsNull)
            {
                return cueId;
            }
            else
            {
                using (List<MySoundCategoryDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetSoundCategoryDefinitions().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        List<MySoundCategoryDefinition.SoundDescription>.Enumerator enumerator2 = enumerator.Current.Sounds.GetEnumerator();
                        try
                        {
                            while (true)
                            {
                                if (!enumerator2.MoveNext())
                                {
                                    break;
                                }
                                MySoundCategoryDefinition.SoundDescription current = enumerator2.Current;
                                if (nameOrId == current.SoundText)
                                {
                                    return MySoundPair.GetCueId(current.SoundId);
                                }
                            }
                            continue;
                        }
                        finally
                        {
                            enumerator2.Dispose();
                            continue;
                        }
                        return id2;
                    }
                }
                return cueId;
            }
            return id2;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            MySoundBlockDefinition blockDefinition = this.BlockDefinition;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(blockDefinition.ResourceSinkGroup, 0.0002f, new Func<float>(this.UpdateRequiredPowerInput));
            component.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            base.ResourceSink = component;
            this.m_soundEmitter = new MyEntity3DSoundEmitter(this, false, 0f);
            this.m_soundEmitter.Force3D = true;
            this.m_soundEmitter.StoppedPlaying += new Action<MyEntity3DSoundEmitter>(this.m_soundEmitter_StoppedPlaying);
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_SoundBlock block = (MyObjectBuilder_SoundBlock) objectBuilder;
            this.m_isPlaying = block.IsPlaying;
            this.m_isLooping = block.IsLoopableSound;
            this.m_volume.SetLocalValue(MathHelper.Clamp(block.Volume, 0f, 1f));
            this.m_soundRadius.SetLocalValue(MathHelper.Clamp(block.Range, blockDefinition.MinRange, blockDefinition.MaxRange));
            this.m_loopPeriod.SetLocalValue(MathHelper.Clamp(block.LoopPeriod, 0f, blockDefinition.MaxLoopPeriod));
            if (block.IsPlaying)
            {
                this.m_willStartSound = true;
                this.m_playingSoundName = block.CueName;
                this.m_soundStartTime = Stopwatch.GetTimestamp() - (((long) block.ElapsedSoundSeconds) * Stopwatch.Frequency);
            }
            this.InitCue(block.CueName);
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
        }

        private void InitCue(string cueName)
        {
            if (string.IsNullOrEmpty(cueName))
            {
                this.m_cueIdString.SetLocalValue("");
            }
            else if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_cueIdString.SetLocalValue(cueName);
            }
            else
            {
                MySoundPair pair = new MySoundPair(cueName, true);
                MySoundCategoryDefinition.SoundDescription description = null;
                using (List<MySoundCategoryDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetSoundCategoryDefinitions().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        foreach (MySoundCategoryDefinition.SoundDescription description2 in enumerator.Current.Sounds)
                        {
                            MyCueId soundId = pair.SoundId;
                            if (soundId.ToString().EndsWith(description2.SoundId.ToString()))
                            {
                                description = description2;
                            }
                        }
                    }
                }
                if (description != null)
                {
                    this.m_cueIdString.SetLocalValue(description.SoundId);
                }
                else
                {
                    this.m_cueIdString.SetLocalValue("");
                }
            }
        }

        private bool IsSelectedSoundLoopable() => 
            IsSoundLoopable(this.GetSoundQueue(this.m_cueIdString.Value));

        private static bool IsSoundLoopable(MyCueId cueId)
        {
            MySoundData cue = MyAudio.Static.GetCue(cueId);
            return ((cue != null) && cue.Loopable);
        }

        private void m_soundEmitter_StoppedPlaying(MyEntity3DSoundEmitter obj)
        {
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && this.m_isPlaying) && !this.m_isLooping)
            {
                this.RequestStopSound();
            }
        }

        private float NormalizeLoopPeriod(float value) => 
            ((value > 1f) ? MathHelper.InterpLogInv(value, 1f, this.BlockDefinition.MaxLoopPeriod) : 0f);

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            base.ResourceSink.Update();
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            IMySourceVoice sound = this.m_soundEmitter.Sound;
            if (sound != null)
            {
                sound.Resume();
            }
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && this.m_willStartSound) && (base.CubeGrid.Physics != null))
            {
                MySoundPair cueId = new MySoundPair(this.m_playingSoundName, true);
                if (this.m_isLooping)
                {
                    this.PlayLoopableSound(cueId);
                }
                else
                {
                    this.PlaySingleSound(cueId);
                }
                this.m_willStartSound = false;
            }
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            IMySourceVoice sound = this.m_soundEmitter.Sound;
            if (sound != null)
            {
                sound.Pause();
            }
        }

        private void PlayLoopableSound(MySoundPair cueId)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_FRAME;
            bool? nullable = null;
            this.m_soundEmitter.PlaySound(cueId, true, false, false, false, false, nullable);
        }

        private void PlaySingleSound(MySoundPair cueId)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.StopSoundInternal(!this.IsLoopablePlaying);
            bool? nullable = null;
            this.m_soundEmitter.PlaySingleSound(cueId, true, false, false, nullable);
        }

        [Event(null, 0x13f), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
        public void PlaySound(bool isLoopable)
        {
            if (base.Enabled && base.IsWorking)
            {
                string str = this.m_cueIdString.Value;
                if (!string.IsNullOrEmpty(str))
                {
                    if (!Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        MySoundPair cueId = new MySoundPair(str, true);
                        this.StopSound();
                        if (isLoopable)
                        {
                            this.PlayLoopableSound(cueId);
                        }
                        else
                        {
                            this.PlaySingleSound(cueId);
                        }
                    }
                    this.m_isPlaying = true;
                    this.m_isLooping = isLoopable;
                    this.m_playingSoundName = str;
                    this.m_soundStartTime = Stopwatch.GetTimestamp();
                }
            }
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        private void RadiusChanged()
        {
            this.m_soundEmitter.CustomMaxDistance = new float?((float) this.m_soundRadius);
        }

        public void RequestPlaySound()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySoundBlock, bool>(this, x => new Action<bool>(x.PlaySound), this.IsSelectedSoundLoopable(), targetEndpoint);
        }

        public void RequestStopSound()
        {
            EndpointId targetEndpoint = new EndpointId();
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MySoundBlock>(this, x => new Action(x.StopSound), targetEndpoint);
            this.m_willStartSound = false;
        }

        private void SelectionChanged()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                m_loopableTimeSlider.UpdateVisual();
                m_playButton.UpdateVisual();
                m_stopButton.UpdateVisual();
            }
        }

        public void SelectSound(List<MyGuiControlListbox.Item> cuesId, bool sync)
        {
            this.SelectSound(cuesId[0].UserData.ToString(), sync);
        }

        public void SelectSound(string cueId, bool sync)
        {
            this.m_cueIdString.Value = cueId;
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.GetSounds(List<string> list)
        {
            list.Clear();
            using (List<MySoundCategoryDefinition>.Enumerator enumerator = MyDefinitionManager.Static.GetSoundCategoryDefinitions().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (MySoundCategoryDefinition.SoundDescription description in enumerator.Current.Sounds)
                    {
                        list.Add(description.SoundId);
                    }
                }
            }
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.Play()
        {
            this.RequestPlaySound();
        }

        void SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.Stop()
        {
            this.RequestStopSound();
        }

        [Event(null, 0x182), Reliable, Server, Broadcast]
        public void StopSound()
        {
            this.StopSoundInternal(true);
        }

        private void StopSoundInternal(bool force = false)
        {
            this.m_isPlaying = false;
            this.m_soundEmitter.StopSound(force, true);
            if (!base.HasDamageEffect)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
            base.DetailedInfo.Clear();
            base.RaisePropertiesChanged();
        }

        public override void UpdateBeforeSimulation()
        {
            if (base.IsWorking)
            {
                base.UpdateBeforeSimulation();
                if (this.IsLoopablePlaying)
                {
                    this.UpdateLoopableSoundEmitter();
                }
            }
        }

        private void UpdateLoopableSoundEmitter()
        {
            double num = ((double) (Stopwatch.GetTimestamp() - this.m_soundStartTime)) / ((double) Stopwatch.Frequency);
            if (num > ((double) this.m_loopPeriod))
            {
                this.StopSoundInternal(true);
            }
            else
            {
                base.DetailedInfo.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertiesText_LoopTimer));
                MyValueFormatter.AppendTimeInBestUnit(Math.Max(0f, (float) (((double) this.m_loopPeriod) - num)), base.DetailedInfo);
                base.RaisePropertiesChanged();
            }
        }

        private float UpdateRequiredPowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return 0.0002f;
        }

        public override void UpdateSoundEmitters()
        {
            if (base.IsWorking)
            {
                if ((!Sandbox.Engine.Platform.Game.IsDedicated && this.m_isPlaying) && !this.m_soundEmitter.IsPlaying)
                {
                    this.RequestPlaySound();
                }
                if (this.m_soundEmitter != null)
                {
                    this.m_soundEmitter.Update();
                }
            }
        }

        private void VolumeChanged()
        {
            this.m_soundEmitter.CustomVolume = new float?((float) this.m_volume);
        }

        private MySoundBlockDefinition BlockDefinition =>
            ((MySoundBlockDefinition) base.BlockDefinition);

        public float Range
        {
            get => 
                ((float) this.m_soundRadius);
            set
            {
                if (this.m_soundRadius != value)
                {
                    this.m_soundRadius.Value = value;
                }
            }
        }

        public float Volume
        {
            get => 
                ((float) this.m_volume);
            set
            {
                if (this.m_volume != value)
                {
                    this.m_volume.Value = value;
                }
            }
        }

        public float LoopPeriod
        {
            get => 
                ((float) this.m_loopPeriod);
            set => 
                (this.m_loopPeriod.Value = value);
        }

        public bool IsLoopablePlaying =>
            (this.m_isPlaying && this.m_isLooping);

        public bool IsLoopPeriodUnderThreshold =>
            (this.m_loopPeriod < this.BlockDefinition.LoopUpdateThreshold);

        public bool IsSoundSelected =>
            !string.IsNullOrEmpty((string) this.m_cueIdString);

        float SpaceEngineers.Game.ModAPI.IMySoundBlock.Volume
        {
            get => 
                this.Volume;
            set => 
                (this.Volume = value);
        }

        float SpaceEngineers.Game.ModAPI.IMySoundBlock.Range
        {
            get => 
                this.Range;
            set => 
                (this.Range = value);
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.Volume
        {
            get => 
                this.Volume;
            set => 
                (this.Volume = MathHelper.Clamp(value, 0f, 1f));
        }

        float SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.Range
        {
            get => 
                this.Range;
            set => 
                (this.Range = MathHelper.Clamp(value, this.BlockDefinition.MinRange, this.BlockDefinition.MaxRange));
        }

        bool SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.IsSoundSelected =>
            this.IsSoundSelected;

        float SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.LoopPeriod
        {
            get => 
                this.LoopPeriod;
            set => 
                (this.LoopPeriod = value);
        }

        string SpaceEngineers.Game.ModAPI.Ingame.IMySoundBlock.SelectedSound
        {
            get => 
                ((string) this.m_cueIdString);
            set => 
                this.SelectSound(this.GetSoundQueue(value).ToString(), true);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySoundBlock.<>c <>9 = new MySoundBlock.<>c();
            public static MyTerminalValueControl<MySoundBlock, float>.GetterDelegate <>9__32_0;
            public static MyTerminalValueControl<MySoundBlock, float>.SetterDelegate <>9__32_1;
            public static MyTerminalControl<MySoundBlock>.WriterDelegate <>9__32_2;
            public static MyTerminalValueControl<MySoundBlock, float>.GetterDelegate <>9__32_3;
            public static MyTerminalValueControl<MySoundBlock, float>.GetterDelegate <>9__32_4;
            public static MyTerminalValueControl<MySoundBlock, float>.GetterDelegate <>9__32_5;
            public static MyTerminalValueControl<MySoundBlock, float>.SetterDelegate <>9__32_6;
            public static MyTerminalControl<MySoundBlock>.WriterDelegate <>9__32_7;
            public static Action<MySoundBlock> <>9__32_8;
            public static Func<MySoundBlock, bool> <>9__32_9;
            public static Action<MySoundBlock> <>9__32_10;
            public static Func<MySoundBlock, bool> <>9__32_11;
            public static MyTerminalValueControl<MySoundBlock, float>.GetterDelegate <>9__32_12;
            public static MyTerminalValueControl<MySoundBlock, float>.SetterDelegate <>9__32_13;
            public static MyTerminalControl<MySoundBlock>.WriterDelegate <>9__32_14;
            public static Func<MySoundBlock, bool> <>9__32_15;
            public static MyTerminalControlSlider<MySoundBlock>.FloatFunc <>9__32_16;
            public static MyTerminalControlSlider<MySoundBlock>.FloatFunc <>9__32_17;
            public static MyTerminalControlListbox<MySoundBlock>.ListContentDelegate <>9__32_18;
            public static MyTerminalControlListbox<MySoundBlock>.SelectItemDelegate <>9__32_19;
            public static Func<MySoundBlock, Action<bool>> <>9__39_0;
            public static Func<MySoundBlock, Action> <>9__45_0;

            internal float <CreateTerminalControls>b__32_0(MySoundBlock x) => 
                (x.Volume * 100f);

            internal void <CreateTerminalControls>b__32_1(MySoundBlock x, float v)
            {
                x.Volume = v * 0.01f;
            }

            internal void <CreateTerminalControls>b__32_10(MySoundBlock x)
            {
                x.RequestStopSound();
            }

            internal bool <CreateTerminalControls>b__32_11(MySoundBlock x) => 
                x.IsSoundSelected;

            internal float <CreateTerminalControls>b__32_12(MySoundBlock x) => 
                x.LoopPeriod;

            internal void <CreateTerminalControls>b__32_13(MySoundBlock x, float f)
            {
                x.LoopPeriod = f;
            }

            internal void <CreateTerminalControls>b__32_14(MySoundBlock x, StringBuilder result)
            {
                MyValueFormatter.AppendTimeInBestUnit(x.LoopPeriod, result);
            }

            internal bool <CreateTerminalControls>b__32_15(MySoundBlock x) => 
                x.IsSelectedSoundLoopable();

            internal float <CreateTerminalControls>b__32_16(MySoundBlock x, float f) => 
                x.NormalizeLoopPeriod(f);

            internal float <CreateTerminalControls>b__32_17(MySoundBlock x, float f) => 
                x.DenormalizeLoopPeriod(f);

            internal void <CreateTerminalControls>b__32_18(MySoundBlock x, ICollection<MyGuiControlListbox.Item> list1, ICollection<MyGuiControlListbox.Item> list2)
            {
                x.FillListContent(list1, list2);
            }

            internal void <CreateTerminalControls>b__32_19(MySoundBlock x, List<MyGuiControlListbox.Item> y)
            {
                x.SelectSound(y, true);
            }

            internal void <CreateTerminalControls>b__32_2(MySoundBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) (x.Volume * 100.0))).Append(" %");
            }

            internal float <CreateTerminalControls>b__32_3(MySoundBlock x) => 
                x.BlockDefinition.MinRange;

            internal float <CreateTerminalControls>b__32_4(MySoundBlock x) => 
                x.BlockDefinition.MaxRange;

            internal float <CreateTerminalControls>b__32_5(MySoundBlock x) => 
                x.Range;

            internal void <CreateTerminalControls>b__32_6(MySoundBlock x, float v)
            {
                x.Range = v;
            }

            internal void <CreateTerminalControls>b__32_7(MySoundBlock x, StringBuilder result)
            {
                result.AppendInt32(((int) x.Range)).Append(" m");
            }

            internal void <CreateTerminalControls>b__32_8(MySoundBlock x)
            {
                x.RequestPlaySound();
            }

            internal bool <CreateTerminalControls>b__32_9(MySoundBlock x) => 
                x.IsSoundSelected;

            internal Action<bool> <RequestPlaySound>b__39_0(MySoundBlock x) => 
                new Action<bool>(x.PlaySound);

            internal Action <RequestStopSound>b__45_0(MySoundBlock x) => 
                new Action(x.StopSound);
        }
    }
}

