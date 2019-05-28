namespace Sandbox.Game.World.Triggers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Triggers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library;
    using VRage.Utils;

    [TriggerType(typeof(MyObjectBuilder_TriggerBlockDestroyed))]
    internal class MyTriggerBlockDestroyed : MyTrigger, ICloneable
    {
        private Dictionary<MyTerminalBlock, BlockState> m_blocks;
        public string SingleMessage;
        private static List<MyTerminalBlock> m_blocksHelper = new List<MyTerminalBlock>();
        private StringBuilder m_progress;

        public MyTriggerBlockDestroyed()
        {
            this.m_blocks = new Dictionary<MyTerminalBlock, BlockState>();
            this.m_progress = new StringBuilder();
        }

        public MyTriggerBlockDestroyed(MyTriggerBlockDestroyed trg) : base(trg)
        {
            this.m_blocks = new Dictionary<MyTerminalBlock, BlockState>();
            this.m_progress = new StringBuilder();
            this.SingleMessage = trg.SingleMessage;
            this.m_blocks.Clear();
            foreach (KeyValuePair<MyTerminalBlock, BlockState> pair in trg.m_blocks)
            {
                this.m_blocks.Add(pair.Key, pair.Value);
            }
        }

        public override object Clone() => 
            new MyTriggerBlockDestroyed(this);

        public override void DisplayGUI()
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenTriggerBlockDestroyed(this));
        }

        public override void DisplayHints(MyPlayer player, VRage.Game.Entity.MyEntity me)
        {
            foreach (KeyValuePair<MyTerminalBlock, BlockState> pair in this.m_blocks)
            {
                if (((BlockState) pair.Value) == BlockState.MessageShown)
                {
                    continue;
                }
                if (pair.Key.SlimBlock.IsDestroyed)
                {
                    m_blocksHelper.Add(pair.Key);
                }
            }
            foreach (MyTerminalBlock block in m_blocksHelper)
            {
                if (this.SingleMessage != null)
                {
                    MyAPIGateway.Utilities.ShowNotification(string.Format(this.SingleMessage, block.CustomName), 0x4e20, "Blue");
                }
                this.m_blocks[block] = BlockState.MessageShown;
            }
            m_blocksHelper.Clear();
            base.DisplayHints(player, me);
        }

        public static MyStringId GetCaption() => 
            MySpaceTexts.GuiTriggerCaptionBlockDestroyed;

        public override MyObjectBuilder_Trigger GetObjectBuilder()
        {
            MyObjectBuilder_TriggerBlockDestroyed objectBuilder = (MyObjectBuilder_TriggerBlockDestroyed) base.GetObjectBuilder();
            objectBuilder.BlockIds = new List<long>();
            foreach (KeyValuePair<MyTerminalBlock, BlockState> pair in this.m_blocks)
            {
                if (!pair.Key.SlimBlock.IsDestroyed)
                {
                    objectBuilder.BlockIds.Add(pair.Key.EntityId);
                }
            }
            objectBuilder.SingleMessage = this.SingleMessage;
            return objectBuilder;
        }

        public override StringBuilder GetProgress()
        {
            this.m_progress.Clear().Append(MyTexts.Get(MySpaceTexts.ScenarioProgressDestroyBlocks));
            foreach (KeyValuePair<MyTerminalBlock, BlockState> pair in this.m_blocks)
            {
                if (((BlockState) pair.Value) == BlockState.Ok)
                {
                    this.m_progress.Append(MyEnvironment.NewLine).Append("   ").Append(pair.Key.CustomName);
                }
            }
            return this.m_progress;
        }

        public override void Init(MyObjectBuilder_Trigger builder)
        {
            base.Init(builder);
            MyObjectBuilder_TriggerBlockDestroyed destroyed = (MyObjectBuilder_TriggerBlockDestroyed) builder;
            using (List<long>.Enumerator enumerator = destroyed.BlockIds.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyTerminalBlock block;
                    if (!Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyTerminalBlock>(enumerator.Current, out block, false))
                    {
                        continue;
                    }
                    this.m_blocks.Add(block, BlockState.Ok);
                }
            }
            this.SingleMessage = destroyed.SingleMessage;
        }

        public override bool Update(MyPlayer player, VRage.Game.Entity.MyEntity me)
        {
            bool flag = false;
            foreach (KeyValuePair<MyTerminalBlock, BlockState> pair in this.m_blocks)
            {
                if (((BlockState) pair.Value) != BlockState.MessageShown)
                {
                    if (pair.Key.SlimBlock.IsDestroyed)
                    {
                        m_blocksHelper.Add(pair.Key);
                        continue;
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                base.m_IsTrue = true;
            }
            if (m_blocksHelper.Count > 0)
            {
                foreach (MyTerminalBlock block in m_blocksHelper)
                {
                    this.m_blocks[block] = BlockState.Destroyed;
                }
                m_blocksHelper.Clear();
            }
            return base.m_IsTrue;
        }

        public Dictionary<MyTerminalBlock, BlockState> Blocks
        {
            get => 
                this.m_blocks;
            private set => 
                (this.m_blocks = value);
        }

        public enum BlockState
        {
            Ok,
            Destroyed,
            MessageShown
        }
    }
}

