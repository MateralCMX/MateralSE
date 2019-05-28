namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Electricity;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.EntityComponents.GameLogic;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Network;

    [MyCubeBlockType(typeof(MyObjectBuilder_SurvivalKit))]
    public class MySurvivalKit : MyAssembler, IMyLifeSupportingBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, IMyRechargeSocketOwner, IMySpawnBlock
    {
        private readonly List<MyTextPanelComponent> m_panels = new List<MyTextPanelComponent>();
        private MyLifeSupportingComponent m_lifeSupportingComponent;

        public MySurvivalKit()
        {
            this.SpawnName = new StringBuilder();
            base.Render = new MyRenderComponentScreenAreas(this);
        }

        public override bool AllowSelfPulling() => 
            true;

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MySurvivalKit>())
            {
                base.CreateTerminalControls();
                MyTerminalControlTextbox<MySurvivalKit> textbox1 = new MyTerminalControlTextbox<MySurvivalKit>("SpawnName", MySpaceTexts.SurvivalKit_SpawnNameLabel, MySpaceTexts.SurvivalKit_SpawnNameToolTip);
                MyTerminalControlTextbox<MySurvivalKit> textbox2 = new MyTerminalControlTextbox<MySurvivalKit>("SpawnName", MySpaceTexts.SurvivalKit_SpawnNameLabel, MySpaceTexts.SurvivalKit_SpawnNameToolTip);
                textbox2.Getter = x => x.SpawnName;
                MyTerminalControlTextbox<MySurvivalKit> local4 = textbox2;
                MyTerminalControlTextbox<MySurvivalKit> local5 = textbox2;
                local5.Setter = (x, v) => x.SetSpawnName(v);
                MyTerminalControlTextbox<MySurvivalKit> control = local5;
                control.SupportsMultipleBlocks = false;
                MyTerminalControlFactory.AddControl<MySurvivalKit>(control);
            }
        }

        public override float GetEfficiencyMultiplierForBlueprint(MyBlueprintDefinitionBase targetBlueprint)
        {
            MyBlueprintDefinitionBase.Item[] prerequisites = targetBlueprint.Prerequisites;
            for (int i = 0; i < prerequisites.Length; i++)
            {
                if (prerequisites[i].Id.TypeId == typeof(MyObjectBuilder_Ore))
                {
                    return 1f;
                }
            }
            return base.GetEfficiencyMultiplierForBlueprint(targetBlueprint);
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_SurvivalKit objectBuilderCubeBlock = (MyObjectBuilder_SurvivalKit) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.SpawnName = this.SpawnName.ToString();
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.Init(objectBuilder, cubeGrid);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            MyObjectBuilder_SurvivalKit kit = objectBuilder as MyObjectBuilder_SurvivalKit;
            this.SpawnName.Clear();
            if (kit.SpawnName != null)
            {
                this.SpawnName.Append(kit.SpawnName);
            }
            MySoundPair progressSound = new MySoundPair(this.BlockDefinition.ProgressSound, true);
            this.m_lifeSupportingComponent = new MyLifeSupportingComponent(this, progressSound, "GenericHeal", 1f);
            base.Components.Add<MyLifeSupportingComponent>(this.m_lifeSupportingComponent);
            if (base.CubeGrid.CreatePhysics)
            {
                base.Components.Add<MyEntityRespawnComponentBase>(new MyRespawnComponent());
            }
            base.ResourceSink.IsPoweredChanged += new Action(this.PowerReceiver_IsPoweredChanged);
            List<ScreenArea> screenAreas = this.BlockDefinition.ScreenAreas;
            if ((screenAreas != null) && (screenAreas.Count > 0))
            {
                for (int i = 0; i < screenAreas.Count; i++)
                {
                    MyTextPanelComponent item = new MyTextPanelComponent(i, this, screenAreas[i].Name, screenAreas[i].DisplayName, screenAreas[i].TextureResolution, 1, 1, true);
                    this.m_panels.Add(item);
                    base.SyncType.Append(item);
                    item.Init(null, null, null);
                }
            }
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
            base.ResourceSink.Update();
            base.OnEnabledChanged();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void PowerReceiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        [Event(null, 0xa3), Reliable, Server(ValidationType.Ownership | ValidationType.Access), Broadcast]
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

        void IMyLifeSupportingBlock.BroadcastSupportRequest(MyCharacter user)
        {
            EndpointId targetEndpoint = new EndpointId();
            MyMultiplayer.RaiseEvent<MySurvivalKit, long>(this, x => new Action<long>(x.RequestSupport), user.EntityId, targetEndpoint);
        }

        void IMyLifeSupportingBlock.ShowTerminal(MyCharacter user)
        {
            MyGuiScreenTerminal.Show(MyTerminalPageEnum.ControlPanel, user, this);
        }

        private void SetSpawnName(StringBuilder text)
        {
            if (this.SpawnName.CompareUpdate(text))
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MySurvivalKit, string>(this, x => new Action<string>(x.SetSpawnTextEvent), text.ToString(), targetEndpoint);
            }
        }

        [Event(null, 0xce), Reliable, Server(ValidationType.Ownership | ValidationType.Access), BroadcastExcept]
        protected void SetSpawnTextEvent(string text)
        {
            this.SpawnName.CompareUpdate(text);
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            this.m_lifeSupportingComponent.Update10();
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.UpdateScreen();
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
            this.m_lifeSupportingComponent.UpdateSoundEmitters();
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public MySurvivalKitDefinition BlockDefinition =>
            ((MySurvivalKitDefinition) base.BlockDefinition);

        public override bool SupportsAdvancedFunctions =>
            false;

        MyRechargeSocket IMyRechargeSocketOwner.RechargeSocket =>
            this.m_lifeSupportingComponent.RechargeSocket;

        bool IMyLifeSupportingBlock.RefuelAllowed =>
            true;

        bool IMyLifeSupportingBlock.HealingAllowed =>
            true;

        MyLifeSupportingBlockType IMyLifeSupportingBlock.BlockType =>
            MyLifeSupportingBlockType.SurvivalKit;

        public override int GUIPriority =>
            600;

        public StringBuilder SpawnName { get; private set; }

        string IMySpawnBlock.SpawnName =>
            this.SpawnName.ToString();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySurvivalKit.<>c <>9 = new MySurvivalKit.<>c();
            public static MyTerminalControlTextbox<MySurvivalKit>.GetterDelegate <>9__7_0;
            public static MyTerminalControlTextbox<MySurvivalKit>.SetterDelegate <>9__7_1;
            public static Func<MySurvivalKit, Action<long>> <>9__23_0;
            public static Func<MySurvivalKit, Action<string>> <>9__34_0;

            internal StringBuilder <CreateTerminalControls>b__7_0(MySurvivalKit x) => 
                x.SpawnName;

            internal void <CreateTerminalControls>b__7_1(MySurvivalKit x, StringBuilder v)
            {
                x.SetSpawnName(v);
            }

            internal Action<long> <Sandbox.Game.GameSystems.IMyLifeSupportingBlock.BroadcastSupportRequest>b__23_0(MySurvivalKit x) => 
                new Action<long>(x.RequestSupport);

            internal Action<string> <SetSpawnName>b__34_0(MySurvivalKit x) => 
                new Action<string>(x.SetSpawnTextEvent);
        }
    }
}

