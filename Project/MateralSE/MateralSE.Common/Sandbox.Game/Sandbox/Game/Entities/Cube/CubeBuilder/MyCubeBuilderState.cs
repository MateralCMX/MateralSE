namespace Sandbox.Game.Entities.Cube.CubeBuilder
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    public class MyCubeBuilderState
    {
        public Dictionary<MyDefinitionId, Quaternion> RotationsByDefinitionHash = new Dictionary<MyDefinitionId, Quaternion>(MyDefinitionId.Comparer);
        public Dictionary<MyDefinitionId, int> StageIndexByDefinitionHash = new Dictionary<MyDefinitionId, int>(MyDefinitionId.Comparer);
        public List<MyCubeBlockDefinition> CurrentBlockDefinitionStages = new List<MyCubeBlockDefinition>();
        private MyCubeBlockDefinitionWithVariants m_definitionWithVariants;
        private MyCubeSize m_cubeSizeMode;

        public void ChooseComplementBlock()
        {
            MyCubeBlockDefinitionWithVariants definitionWithVariants = this.m_definitionWithVariants;
            if (definitionWithVariants != null)
            {
                MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(definitionWithVariants.Base.BlockPairName);
                if (definitionWithVariants.Base.CubeSize != MyCubeSize.Small)
                {
                    if (((definitionWithVariants.Base.CubeSize == MyCubeSize.Large) && (definitionGroup.Small != null)) && (definitionGroup.Small.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS))
                    {
                        this.CurrentBlockDefinition = definitionGroup.Small;
                    }
                }
                else if ((definitionGroup.Large != null) && (definitionGroup.Large.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS))
                {
                    this.CurrentBlockDefinition = definitionGroup.Large;
                }
            }
        }

        public bool HasComplementBlock()
        {
            if (this.m_definitionWithVariants == null)
            {
                return false;
            }
            MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.m_definitionWithVariants.Base.BlockPairName);
            return ((this.m_definitionWithVariants.Base.CubeSize != MyCubeSize.Small) ? ((this.m_definitionWithVariants.Base.CubeSize == MyCubeSize.Large) && ((definitionGroup.Small != null) && (definitionGroup.Small.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS))) : ((definitionGroup.Large != null) && (definitionGroup.Large.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS)));
        }

        public void SetCubeSize(MyCubeSize newCubeSize)
        {
            this.m_cubeSizeMode = newCubeSize;
            this.UpdateComplementBlock();
        }

        public void UpdateBlockDefinitionStages(MyDefinitionId? id)
        {
            if ((id != null) && (this.CurrentBlockDefinition != null))
            {
                int num;
                MyDefinitionId key = id.Value;
                if (this.CurrentBlockDefinitionStages.Count > 1)
                {
                    key = this.CurrentBlockDefinitionStages[0].Id;
                }
                if ((this.CurrentBlockDefinitionStages.Count > 1) && ((this.StageIndexByDefinitionHash.TryGetValue(key, out num) && (num >= 0)) && (num < this.CurrentBlockDefinitionStages.Count)))
                {
                    this.CurrentBlockDefinition = this.CurrentBlockDefinitionStages[num];
                }
            }
        }

        internal void UpdateComplementBlock()
        {
            if ((this.CurrentBlockDefinition != null) && (this.StartBlockDefinition != null))
            {
                MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.StartBlockDefinition.BlockPairName);
                this.CurrentBlockDefinition = (this.m_cubeSizeMode == MyCubeSize.Large) ? definitionGroup.Large : definitionGroup.Small;
            }
        }

        public void UpdateCubeBlockDefinition(MyDefinitionId? id, MatrixD localMatrixAdd)
        {
            if (id != null)
            {
                if (this.CurrentBlockDefinition != null)
                {
                    MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.CurrentBlockDefinition.BlockPairName);
                    if (this.CurrentBlockDefinitionStages.Count > 1)
                    {
                        definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(this.CurrentBlockDefinitionStages[0].BlockPairName);
                        if (definitionGroup.Small != null)
                        {
                            this.StageIndexByDefinitionHash[definitionGroup.Small.Id] = this.CurrentBlockDefinitionStages.IndexOf(this.CurrentBlockDefinition);
                        }
                        if (definitionGroup.Large != null)
                        {
                            this.StageIndexByDefinitionHash[definitionGroup.Large.Id] = this.CurrentBlockDefinitionStages.IndexOf(this.CurrentBlockDefinition);
                        }
                    }
                    Quaternion quaternion = Quaternion.CreateFromRotationMatrix(localMatrixAdd);
                    if (definitionGroup.Small != null)
                    {
                        this.RotationsByDefinitionHash[definitionGroup.Small.Id] = quaternion;
                    }
                    if (definitionGroup.Large != null)
                    {
                        this.RotationsByDefinitionHash[definitionGroup.Large.Id] = quaternion;
                    }
                }
                MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(id.Value);
                if (cubeBlockDefinition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS)
                {
                    this.CurrentBlockDefinition = cubeBlockDefinition;
                }
                else
                {
                    this.CurrentBlockDefinition = (cubeBlockDefinition.CubeSize == MyCubeSize.Large) ? MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName).Small : MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName).Large;
                }
                this.StartBlockDefinition = this.CurrentBlockDefinition;
            }
        }

        public MyCubeBlockDefinition CurrentBlockDefinition
        {
            get => 
                ((MyCubeBlockDefinition) this.m_definitionWithVariants);
            set
            {
                if (value == null)
                {
                    this.m_definitionWithVariants = null;
                    this.CurrentBlockDefinitionStages.Clear();
                }
                else
                {
                    this.m_definitionWithVariants = new MyCubeBlockDefinitionWithVariants(value, -1);
                    if (MyFakes.ENABLE_BLOCK_STAGES && !this.CurrentBlockDefinitionStages.Contains(value))
                    {
                        this.CurrentBlockDefinitionStages.Clear();
                        if (value.BlockStages != null)
                        {
                            this.CurrentBlockDefinitionStages.Add(value);
                            foreach (MyDefinitionId id in value.BlockStages)
                            {
                                MyCubeBlockDefinition definition;
                                MyDefinitionManager.Static.TryGetCubeBlockDefinition(id, out definition);
                                if (definition != null)
                                {
                                    this.CurrentBlockDefinitionStages.Add(definition);
                                }
                            }
                        }
                    }
                }
            }
        }

        public MyCubeBlockDefinition StartBlockDefinition { get; private set; }

        public MyCubeSize CubeSizeMode =>
            this.m_cubeSizeMode;
    }
}

