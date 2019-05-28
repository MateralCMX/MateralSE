namespace SpaceEngineers.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_UpgradeModule)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyUpgradeModule), typeof(Sandbox.ModAPI.Ingame.IMyUpgradeModule) })]
    public class MyUpgradeModule : MyFunctionalBlock, Sandbox.ModAPI.IMyUpgradeModule, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyUpgradeModule
    {
        private ConveyorLinePosition[] m_connectionPositions;
        private Dictionary<ConveyorLinePosition, MyCubeBlock> m_connectedBlocks;
        private MyUpgradeModuleInfo[] m_upgrades;
        private int m_connectedBlockCount;
        private SortedDictionary<string, MyModelDummy> m_dummies;
        private bool m_needsRefresh;
        private MyResourceStateEnum m_oldResourceState = MyResourceStateEnum.NoPower;

        private void AddEffectToBlock(MyCubeBlock block)
        {
            foreach (MyUpgradeModuleInfo info in this.m_upgrades)
            {
                float num2;
                if (block.UpgradeValues.TryGetValue(info.UpgradeType, out num2))
                {
                    double num3 = num2;
                    num3 = (info.ModifierType != MyUpgradeModifierType.Additive) ? (num3 * info.Modifier) : (num3 + info.Modifier);
                    block.UpgradeValues[info.UpgradeType] = (float) num3;
                }
            }
            block.CommitUpgradeValues();
        }

        private bool CanAffectBlock(MyCubeBlock block)
        {
            foreach (MyUpgradeModuleInfo info in this.m_upgrades)
            {
                if (block.UpgradeValues.ContainsKey(info.UpgradeType))
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearConnectedBlocks()
        {
            foreach (MyCubeBlock block in this.m_connectedBlocks.Values)
            {
                if ((block != null) && base.IsWorking)
                {
                    this.RemoveEffectFromBlock(block);
                }
                if ((block != null) && (block.CurrentAttachedUpgradeModules != null))
                {
                    block.CurrentAttachedUpgradeModules.Remove(base.EntityId);
                }
            }
            this.m_connectedBlocks.Clear();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.UpdateIsWorking();
            this.UpdateEmissivity();
        }

        private void CubeGrid_OnBlockAdded(MySlimBlock obj)
        {
            if (!ReferenceEquals(obj, base.SlimBlock))
            {
                this.m_needsRefresh = true;
            }
        }

        private void CubeGrid_OnBlockRemoved(MySlimBlock obj)
        {
            if (!ReferenceEquals(obj, base.SlimBlock))
            {
                this.m_needsRefresh = true;
            }
        }

        protected int GetBlockConnectionCount(MyCubeBlock cubeBlock)
        {
            int num = 0;
            using (Dictionary<ConveyorLinePosition, MyCubeBlock>.ValueCollection.Enumerator enumerator = this.m_connectedBlocks.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != cubeBlock)
                    {
                        continue;
                    }
                    num++;
                }
            }
            return num;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            this.m_connectedBlocks = new Dictionary<ConveyorLinePosition, MyCubeBlock>();
            this.m_dummies = new SortedDictionary<string, MyModelDummy>(MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies);
            this.InitDummies();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyUpgradeModule_IsWorkingChanged);
            this.m_upgrades = this.BlockDefinition.Upgrades;
            base.UpdateIsWorking();
        }

        private void InitDummies()
        {
            this.m_connectedBlocks.Clear();
            this.m_connectionPositions = MyMultilineConveyorEndpoint.GetLinePositions(this, this.m_dummies, "detector_upgrade");
            for (int i = 0; i < this.m_connectionPositions.Length; i++)
            {
                this.m_connectionPositions[i] = MyMultilineConveyorEndpoint.PositionToGridCoords(this.m_connectionPositions[i], this);
                this.m_connectedBlocks.Add(this.m_connectionPositions[i], null);
            }
        }

        private void MyUpgradeModule_IsWorkingChanged(MyCubeBlock obj)
        {
            this.RefreshEffects();
            this.UpdateEmissivity();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            this.UpdateEmissivity();
        }

        public override void OnRegisteredToGridSystems()
        {
            base.OnRegisteredToGridSystems();
            this.InitDummies();
            this.m_needsRefresh = true;
            this.UpdateEmissivity();
            base.CubeGrid.OnBlockAdded += new Action<MySlimBlock>(this.CubeGrid_OnBlockAdded);
            base.CubeGrid.OnBlockRemoved += new Action<MySlimBlock>(this.CubeGrid_OnBlockRemoved);
        }

        public override void OnUnregisteredFromGridSystems()
        {
            base.OnUnregisteredFromGridSystems();
            base.CubeGrid.OnBlockAdded -= new Action<MySlimBlock>(this.CubeGrid_OnBlockAdded);
            base.CubeGrid.OnBlockRemoved -= new Action<MySlimBlock>(this.CubeGrid_OnBlockRemoved);
            base.SlimBlock.ComponentStack.IsFunctionalChanged -= new Action(this.ComponentStack_IsFunctionalChanged);
            this.ClearConnectedBlocks();
        }

        private void RefreshConnections()
        {
            foreach (ConveyorLinePosition position in this.m_connectionPositions)
            {
                ConveyorLinePosition connectingPosition = position.GetConnectingPosition();
                MySlimBlock cubeBlock = base.CubeGrid.GetCubeBlock(connectingPosition.LocalGridPosition);
                if ((cubeBlock == null) || (cubeBlock.FatBlock == null))
                {
                    MyCubeBlock block4 = null;
                    this.m_connectedBlocks.TryGetValue(position, out block4);
                    if (block4 != null)
                    {
                        if ((block4 != null) && (block4.CurrentAttachedUpgradeModules != null))
                        {
                            block4.CurrentAttachedUpgradeModules.Remove(base.EntityId);
                        }
                        if (base.IsWorking)
                        {
                            this.RemoveEffectFromBlock(block4);
                        }
                        this.m_connectedBlocks[position] = null;
                    }
                }
                else
                {
                    MyCubeBlock fatBlock = cubeBlock.FatBlock;
                    MyCubeBlock block3 = null;
                    this.m_connectedBlocks.TryGetValue(position, out block3);
                    if ((fatBlock != null) && !fatBlock.GetComponent().ConnectionPositions.Contains(connectingPosition))
                    {
                        fatBlock = null;
                    }
                    if (!ReferenceEquals(fatBlock, block3))
                    {
                        if ((block3 != null) && (block3.CurrentAttachedUpgradeModules != null))
                        {
                            block3.CurrentAttachedUpgradeModules.Remove(base.EntityId);
                        }
                        if (fatBlock != null)
                        {
                            if (fatBlock.CurrentAttachedUpgradeModules == null)
                            {
                                fatBlock.CurrentAttachedUpgradeModules = new Dictionary<long, MyCubeBlock.AttachedUpgradeModule>();
                            }
                            if (!fatBlock.CurrentAttachedUpgradeModules.ContainsKey(base.EntityId))
                            {
                                fatBlock.CurrentAttachedUpgradeModules.Add(base.EntityId, new MyCubeBlock.AttachedUpgradeModule(this, 1, this.CanAffectBlock(fatBlock)));
                            }
                            else
                            {
                                MyCubeBlock.AttachedUpgradeModule local1 = fatBlock.CurrentAttachedUpgradeModules[base.EntityId];
                                local1.SlotCount++;
                            }
                        }
                        if (base.IsWorking)
                        {
                            if (block3 != null)
                            {
                                this.RemoveEffectFromBlock(block3);
                            }
                            if (fatBlock != null)
                            {
                                this.AddEffectToBlock(fatBlock);
                            }
                        }
                        this.m_connectedBlocks[position] = fatBlock;
                    }
                }
            }
            this.UpdateEmissivity();
        }

        private void RefreshEffects()
        {
            foreach (MyCubeBlock block in this.m_connectedBlocks.Values)
            {
                if (block != null)
                {
                    if (base.IsWorking)
                    {
                        this.AddEffectToBlock(block);
                        continue;
                    }
                    this.RemoveEffectFromBlock(block);
                }
            }
        }

        private void RemoveEffectFromBlock(MyCubeBlock block)
        {
            foreach (MyUpgradeModuleInfo info in this.m_upgrades)
            {
                float num2;
                if (block.UpgradeValues.TryGetValue(info.UpgradeType, out num2))
                {
                    double num3 = num2;
                    if (info.ModifierType == MyUpgradeModifierType.Additive)
                    {
                        num3 -= info.Modifier;
                        if (num3 < 0.0)
                        {
                            num3 = 0.0;
                        }
                    }
                    else
                    {
                        num3 /= (double) info.Modifier;
                        if (num3 < 1.0)
                        {
                            double num4 = num3 + 1E-07;
                            num3 = 1.0;
                        }
                    }
                    block.UpgradeValues[info.UpgradeType] = (float) num3;
                }
            }
            block.CommitUpgradeValues();
        }

        void Sandbox.ModAPI.Ingame.IMyUpgradeModule.GetUpgradeList(out List<MyUpgradeModuleInfo> upgradelist)
        {
            upgradelist = new List<MyUpgradeModuleInfo>();
            foreach (MyUpgradeModuleInfo info in this.m_upgrades)
            {
                upgradelist.Add(info);
            }
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (this.m_needsRefresh)
            {
                this.RefreshConnections();
                this.m_needsRefresh = false;
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                if (base.CubeGrid.GridSystems.ResourceDistributor.ResourceState != this.m_oldResourceState)
                {
                    this.m_oldResourceState = base.CubeGrid.GridSystems.ResourceDistributor.ResourceState;
                    this.UpdateEmissivity();
                }
                this.m_oldResourceState = base.CubeGrid.GridSystems.ResourceDistributor.ResourceState;
                if (base.m_soundEmitter != null)
                {
                    bool flag = false;
                    foreach (MyCubeBlock block in this.m_connectedBlocks.Values)
                    {
                        int isWorking;
                        if (((block == null) || (block.ResourceSink == null)) || !block.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                        {
                            isWorking = 0;
                        }
                        else
                        {
                            isWorking = (int) block.IsWorking;
                        }
                        flag |= isWorking;
                        if (flag)
                        {
                            break;
                        }
                    }
                    flag &= base.IsWorking;
                    if ((flag && (this.m_connectedBlockCount > 0)) && (!base.m_soundEmitter.IsPlaying || !ReferenceEquals(base.m_soundEmitter.SoundPair, base.m_baseIdleSound)))
                    {
                        bool? nullable = null;
                        base.m_soundEmitter.PlaySound(base.m_baseIdleSound, true, false, false, false, false, nullable);
                    }
                    else if (((!flag || (this.m_connectedBlockCount == 0)) && base.m_soundEmitter.IsPlaying) && ReferenceEquals(base.m_soundEmitter.SoundPair, base.m_baseIdleSound))
                    {
                        base.m_soundEmitter.StopSound(false, true);
                    }
                }
            }
        }

        private void UpdateEmissivity()
        {
            this.m_connectedBlockCount = 0;
            if (this.m_connectedBlocks != null)
            {
                for (int i = 0; i < this.m_connectionPositions.Length; i++)
                {
                    MyEmissiveColorStateResult result;
                    string emissiveName = "Emissive" + i.ToString();
                    Color green = Color.Green;
                    float emissivity = 1f;
                    MyCubeBlock block = null;
                    this.m_connectedBlocks.TryGetValue(this.m_connectionPositions[i], out block);
                    if (block != null)
                    {
                        this.m_connectedBlockCount++;
                    }
                    if (base.IsWorking && (this.m_oldResourceState != MyResourceStateEnum.NoPower))
                    {
                        if (block != null)
                        {
                            if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                            {
                                green = result.EmissiveColor;
                            }
                        }
                        else
                        {
                            green = Color.Yellow;
                            if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Warning, out result))
                            {
                                green = result.EmissiveColor;
                            }
                        }
                    }
                    else if (base.IsFunctional)
                    {
                        green = Color.Red;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                        {
                            green = result.EmissiveColor;
                        }
                    }
                    else
                    {
                        green = Color.Black;
                        emissivity = 0f;
                        if (MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                        {
                            green = result.EmissiveColor;
                        }
                    }
                    if (base.Render.RenderObjectIDs[0] != uint.MaxValue)
                    {
                        UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], emissiveName, green, emissivity);
                    }
                }
            }
        }

        private MyUpgradeModuleDefinition BlockDefinition =>
            ((MyUpgradeModuleDefinition) base.BlockDefinition);

        uint Sandbox.ModAPI.Ingame.IMyUpgradeModule.UpgradeCount =>
            ((uint) this.m_upgrades.Length);

        uint Sandbox.ModAPI.Ingame.IMyUpgradeModule.Connections
        {
            get
            {
                uint num = 0;
                MyCubeBlock objA = null;
                foreach (MyCubeBlock block2 in this.m_connectedBlocks.Values)
                {
                    if (ReferenceEquals(objA, block2))
                    {
                        continue;
                    }
                    if (block2 != null)
                    {
                        num++;
                        objA = block2;
                    }
                }
                return num;
            }
        }
    }
}

