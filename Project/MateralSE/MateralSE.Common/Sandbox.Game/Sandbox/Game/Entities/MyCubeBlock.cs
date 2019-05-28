namespace Sandbox.Game.Entities
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.ParticleEffects;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    public class MyCubeBlock : VRage.Game.Entity.MyEntity, IMyComponentOwner<MyIDModule>, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.IMyUpgradableBlock, Sandbox.ModAPI.Ingame.IMyUpgradableBlock
    {
        protected static readonly string DUMMY_SUBBLOCK_ID = "subblock_";
        private static List<MyCubeBlockDefinition.MountPoint> m_tmpMountPoints = new List<MyCubeBlockDefinition.MountPoint>();
        private static List<MyCubeBlockDefinition.MountPoint> m_tmpBlockMountPoints = new List<MyCubeBlockDefinition.MountPoint>();
        private static List<MyCubeBlockDefinition.MountPoint> m_tmpOtherBlockMountPoints = new List<MyCubeBlockDefinition.MountPoint>();
        protected static EmissiveNames m_emissiveNames = new EmissiveNames(true);
        public Dictionary<long, AttachedUpgradeModule> CurrentAttachedUpgradeModules;
        private MyResourceSinkComponent m_sinkComp;
        public bool IsBeingRemoved;
        protected List<MyCubeBlockEffect> m_activeEffects;
        private bool? m_setDamagedEffectDelayed = false;
        private bool m_checkConnectionAllowed;
        [CompilerGenerated]
        private Action<MyCubeBlock> CheckConnectionChanged;
        private int m_numberInGrid;
        public MySlimBlock SlimBlock;
        public bool IsSilenced;
        public bool SilenceInChange;
        public bool UsedUpdateEveryFrame;
        [CompilerGenerated]
        private Action<MyCubeBlock> IsWorkingChanged;
        [CompilerGenerated]
        private Func<bool> CanContinueBuildCheck;
        private MyIDModule m_IDModule;
        protected Dictionary<string, MySlimBlock> SubBlocks;
        private List<MyObjectBuilder_CubeBlock.MySubBlockId> m_loadedSubBlocks;
        private static MethodDataIsConnectedTo m_methodDataIsConnectedTo = new MethodDataIsConnectedTo();
        protected bool m_forceBlockDestructible;
        private MyParticleEffect m_damageEffect;
        private bool m_wasUpdatedEachFrame;
        private MyUpgradableBlockComponent m_upgradeComponent;
        private Dictionary<string, float> m_upgradeValues;
        [CompilerGenerated]
        private Action OnUpgradeValuesChanged;
        private bool m_inventorymassDirty;
        private MyStringHash m_skinSubtypeId;

        public event Func<bool> CanContinueBuildCheck
        {
            [CompilerGenerated] add
            {
                Func<bool> canContinueBuildCheck = this.CanContinueBuildCheck;
                while (true)
                {
                    Func<bool> a = canContinueBuildCheck;
                    Func<bool> func3 = (Func<bool>) Delegate.Combine(a, value);
                    canContinueBuildCheck = Interlocked.CompareExchange<Func<bool>>(ref this.CanContinueBuildCheck, func3, a);
                    if (ReferenceEquals(canContinueBuildCheck, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Func<bool> canContinueBuildCheck = this.CanContinueBuildCheck;
                while (true)
                {
                    Func<bool> source = canContinueBuildCheck;
                    Func<bool> func3 = (Func<bool>) Delegate.Remove(source, value);
                    canContinueBuildCheck = Interlocked.CompareExchange<Func<bool>>(ref this.CanContinueBuildCheck, func3, source);
                    if (ReferenceEquals(canContinueBuildCheck, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> CheckConnectionChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> checkConnectionChanged = this.CheckConnectionChanged;
                while (true)
                {
                    Action<MyCubeBlock> a = checkConnectionChanged;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    checkConnectionChanged = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.CheckConnectionChanged, action3, a);
                    if (ReferenceEquals(checkConnectionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> checkConnectionChanged = this.CheckConnectionChanged;
                while (true)
                {
                    Action<MyCubeBlock> source = checkConnectionChanged;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    checkConnectionChanged = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.CheckConnectionChanged, action3, source);
                    if (ReferenceEquals(checkConnectionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyCubeBlock> IsWorkingChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCubeBlock> isWorkingChanged = this.IsWorkingChanged;
                while (true)
                {
                    Action<MyCubeBlock> a = isWorkingChanged;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Combine(a, value);
                    isWorkingChanged = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.IsWorkingChanged, action3, a);
                    if (ReferenceEquals(isWorkingChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeBlock> isWorkingChanged = this.IsWorkingChanged;
                while (true)
                {
                    Action<MyCubeBlock> source = isWorkingChanged;
                    Action<MyCubeBlock> action3 = (Action<MyCubeBlock>) Delegate.Remove(source, value);
                    isWorkingChanged = Interlocked.CompareExchange<Action<MyCubeBlock>>(ref this.IsWorkingChanged, action3, source);
                    if (ReferenceEquals(isWorkingChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action OnUpgradeValuesChanged
        {
            [CompilerGenerated] add
            {
                Action onUpgradeValuesChanged = this.OnUpgradeValuesChanged;
                while (true)
                {
                    Action a = onUpgradeValuesChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onUpgradeValuesChanged = Interlocked.CompareExchange<Action>(ref this.OnUpgradeValuesChanged, action3, a);
                    if (ReferenceEquals(onUpgradeValuesChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onUpgradeValuesChanged = this.OnUpgradeValuesChanged;
                while (true)
                {
                    Action source = onUpgradeValuesChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onUpgradeValuesChanged = Interlocked.CompareExchange<Action>(ref this.OnUpgradeValuesChanged, action3, source);
                    if (ReferenceEquals(onUpgradeValuesChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<VRage.Game.ModAPI.IMyCubeBlock> VRage.Game.ModAPI.IMyCubeBlock.IsWorkingChanged
        {
            add
            {
                this.IsWorkingChanged += this.GetDelegate(value);
            }
            remove
            {
                this.IsWorkingChanged -= this.GetDelegate(value);
            }
        }

        public MyCubeBlock()
        {
            base.Render.ShadowBoxLod = true;
            base.NeedsWorldMatrix = false;
            base.InvalidateOnMove = false;
        }

        protected void AddSubBlock(string dummyName, MySlimBlock subblock)
        {
            MySlimBlock block;
            if (this.SubBlocks == null)
            {
                this.SubBlocks = new Dictionary<string, MySlimBlock>();
            }
            if (this.SubBlocks.TryGetValue(dummyName, out block))
            {
                if (ReferenceEquals(subblock, block))
                {
                    return;
                }
                this.RemoveSubBlock(dummyName, false);
            }
            this.SubBlocks.Add(dummyName, subblock);
            subblock.FatBlock.SubBlockName = dummyName;
            subblock.FatBlock.OwnerBlock = this.SlimBlock;
            subblock.FatBlock.OnClosing += new Action<VRage.Game.Entity.MyEntity>(this.SubBlock_OnClosing);
        }

        public void AddUpgradeValue(string name, float defaultValue)
        {
            float num;
            if (!this.UpgradeValues.TryGetValue(name, out num))
            {
                this.UpgradeValues.Add(name, defaultValue);
            }
            else if (num != defaultValue)
            {
                MyLog.Default.WriteLine("ERROR while adding upgraded block " + this.DisplayNameText.ToString() + ". Duplicate with different default value found!");
            }
        }

        protected bool AllSubBlocksInitialized() => 
            (((this.BlockDefinition.SubBlockDefinitions != null) && (this.BlockDefinition.SubBlockDefinitions.Count != 0)) ? ((this.SubBlocks != null) && ((this.SubBlocks.Count != 0) && ((this.SubBlocks.Count == this.BlockDefinition.SubBlockDefinitions.Count) || ((this.m_loadedSubBlocks == null) || (this.m_loadedSubBlocks.Count == 0))))) : false);

        public void CalcLocalMatrix(out Matrix localMatrix, out string currModel)
        {
            Matrix matrix;
            this.GetLocalMatrix(out localMatrix);
            currModel = this.SlimBlock.CalculateCurrentModel(out matrix);
            matrix.Translation = localMatrix.Translation;
            localMatrix = matrix;
        }

        public virtual string CalculateCurrentModel(out Matrix orientation)
        {
            this.Orientation.GetMatrix(out orientation);
            return this.BlockDefinition.Model;
        }

        public bool CanContinueBuild()
        {
            if (this.CanContinueBuildCheck == null)
            {
                return true;
            }
            bool flag = true;
            Delegate[] invocationList = this.CanContinueBuildCheck.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                Func<bool> func = invocationList[i] as Func<bool>;
                flag &= func();
            }
            return flag;
        }

        public void ChangeBlockOwnerRequest(long playerId, MyOwnershipShareModeEnum shareMode)
        {
            this.CubeGrid.ChangeOwnerRequest(this.CubeGrid, this, playerId, shareMode);
        }

        public void ChangeOwner(long owner, MyOwnershipShareModeEnum shareMode)
        {
            MyEntityOwnershipComponent component = base.Components.Get<MyEntityOwnershipComponent>();
            if (component == null)
            {
                if ((this.IDModule != null) && ((owner != this.m_IDModule.Owner) || (shareMode != this.m_IDModule.ShareMode)))
                {
                    long oldOwner = this.m_IDModule.Owner;
                    this.m_IDModule.Owner = owner;
                    this.m_IDModule.ShareMode = shareMode;
                    if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                    {
                        this.CubeGrid.ChangeOwner(this, oldOwner, owner);
                    }
                    this.OnOwnershipChanged();
                }
            }
            else if ((owner != component.OwnerId) || (shareMode != component.ShareMode))
            {
                long ownerId = component.OwnerId;
                component.OwnerId = owner;
                component.ShareMode = shareMode;
                if (MyFakes.ENABLE_TERMINAL_PROPERTIES)
                {
                    this.CubeGrid.ChangeOwner(this, ownerId, owner);
                }
                this.OnOwnershipChanged();
            }
        }

        public virtual void CheckEmissiveState(bool force = false)
        {
            if (this.IsWorking)
            {
                this.SetEmissiveStateWorking();
            }
            else if (this.IsFunctional)
            {
                this.SetEmissiveStateDisabled();
            }
            else
            {
                this.SetEmissiveStateDamaged();
            }
        }

        protected virtual bool CheckIsWorking() => 
            this.IsFunctional;

        protected override void Closing()
        {
            if (this.UseObjectsComponent.DetectorPhysics != null)
            {
                this.UseObjectsComponent.ClearPhysics();
            }
            if (MyFakes.ENABLE_SUBBLOCKS && (this.SubBlocks != null))
            {
                foreach (KeyValuePair<string, MySlimBlock> pair in this.SubBlocks)
                {
                    MySlimBlock block = pair.Value;
                    if (block.FatBlock != null)
                    {
                        block.FatBlock.OwnerBlock = null;
                        block.FatBlock.SubBlockName = null;
                        block.FatBlock.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.SubBlock_OnClosing);
                    }
                }
            }
            this.SetDamageEffect(false);
            base.Closing();
        }

        public void CommitUpgradeValues()
        {
            Action onUpgradeValuesChanged = this.OnUpgradeValuesChanged;
            if (onUpgradeValuesChanged != null)
            {
                onUpgradeValuesChanged();
            }
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            this.UpdateIsWorking();
            this.CubeGrid.UpdateOwnership(this.OwnerId, this.IsFunctional);
            if (this.UsesEmissivePreset)
            {
                this.CheckEmissiveState(false);
            }
            if (MyVisualScriptLogicProvider.BlockFunctionalityChanged != null)
            {
                MyVisualScriptLogicProvider.BlockFunctionalityChanged(base.EntityId, this.CubeGrid.EntityId, base.Name, this.CubeGrid.Name, this.SlimBlock.BlockDefinition.Id.TypeId.ToString(), this.SlimBlock.BlockDefinition.Id.SubtypeName, this.IsFunctional);
            }
        }

        public virtual bool ConnectionAllowed(ref Vector3I otherBlockPos, ref Vector3I faceNormal, MyCubeBlockDefinition def)
        {
            MySlimBlock cubeBlock;
            Vector3I position;
            if (!MyFakes.ENABLE_FRACTURE_COMPONENT || !base.Components.Has<MyFractureComponentBase>())
            {
                return true;
            }
            MyFractureComponentCubeBlock fractureComponent = this.GetFractureComponent();
            if (fractureComponent == null)
            {
                goto TR_0000;
            }
            else if (fractureComponent.MountPoints != null)
            {
                m_tmpBlockMountPoints.Clear();
                MyCubeGrid.TransformMountPoints(m_tmpBlockMountPoints, this.BlockDefinition, fractureComponent.MountPoints.GetInternalArray<MyCubeBlockDefinition.MountPoint>(), ref this.SlimBlock.Orientation);
                cubeBlock = this.CubeGrid.GetCubeBlock(otherBlockPos);
                if (cubeBlock == null)
                {
                    return true;
                }
                position = this.Position;
                m_tmpMountPoints.Clear();
                if (cubeBlock.FatBlock is MyCompoundCubeBlock)
                {
                    foreach (MySlimBlock block3 in (cubeBlock.FatBlock as MyCompoundCubeBlock).GetBlocks())
                    {
                        MyFractureComponentCubeBlock block4 = block3.GetFractureComponent();
                        MyCubeBlockDefinition.MountPoint[] mountPoints = null;
                        mountPoints = (block4 == null) ? block3.BlockDefinition.GetBuildProgressModelMountPoints(block3.BuildLevelRatio) : block4.MountPoints.GetInternalArray<MyCubeBlockDefinition.MountPoint>();
                        m_tmpOtherBlockMountPoints.Clear();
                        MyCubeGrid.TransformMountPoints(m_tmpOtherBlockMountPoints, block3.BlockDefinition, mountPoints, ref block3.Orientation);
                        m_tmpMountPoints.AddRange(m_tmpOtherBlockMountPoints);
                    }
                }
                else
                {
                    MyCubeBlockDefinition.MountPoint[] mountPoints = null;
                    MyFractureComponentCubeBlock block5 = cubeBlock.GetFractureComponent();
                    mountPoints = (block5 == null) ? def.GetBuildProgressModelMountPoints(cubeBlock.BuildLevelRatio) : block5.MountPoints.GetInternalArray<MyCubeBlockDefinition.MountPoint>();
                    MyCubeGrid.TransformMountPoints(m_tmpMountPoints, def, mountPoints, ref cubeBlock.Orientation);
                }
            }
            else
            {
                goto TR_0000;
            }
            m_tmpMountPoints.Clear();
            m_tmpBlockMountPoints.Clear();
            m_tmpOtherBlockMountPoints.Clear();
            return MyCubeGrid.CheckMountPointsForSide(m_tmpBlockMountPoints, ref this.SlimBlock.Orientation, ref position, this.BlockDefinition.Id, ref faceNormal, m_tmpMountPoints, ref cubeBlock.Orientation, ref otherBlockPos, def.Id);
        TR_0000:
            return true;
        }

        public virtual bool ConnectionAllowed(ref Vector3I otherBlockMinPos, ref Vector3I otherBlockMaxPos, ref Vector3I faceNormal, MyCubeBlockDefinition def)
        {
            Vector3I otherBlockPos = otherBlockMinPos;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref otherBlockMinPos, ref otherBlockMaxPos);
            while (iterator.IsValid())
            {
                if (this.ConnectionAllowed(ref otherBlockPos, ref faceNormal, def))
                {
                    return true;
                }
                iterator.GetNext(out otherBlockPos);
            }
            return false;
        }

        public virtual void ContactPointCallback(ref MyGridContactInfo value)
        {
        }

        public virtual void CreateRenderer(MyPersistentEntityFlags2 persistentFlags, Vector3 colorMaskHsv, object modelStorage)
        {
            this.m_skinSubtypeId = MyStringHash.NullOrEmpty;
            base.Render = new MyRenderComponentCubeBlock();
            base.Render.ColorMaskHsv = colorMaskHsv;
            base.Render.ShadowBoxLod = true;
            base.Render.EnableColorMaskHsv = true;
            base.Render.SkipIfTooSmall = false;
            MyRenderComponentBase render = base.Render;
            render.PersistentFlags |= persistentFlags | MyPersistentEntityFlags2.CastShadows;
            base.Render.ModelStorage = modelStorage;
            base.Render.FadeIn = this.CubeGrid.Render.FadeIn;
            this.UpdateSkin();
        }

        private void damageEffect_OnDelete(object sender, EventArgs e)
        {
            if (sender == this.m_damageEffect)
            {
                this.SetDamageEffect(false);
            }
        }

        public bool FriendlyWithBlock(MyCubeBlock block) => 
            ((this.GetUserRelationToOwner(block.OwnerId) != MyRelationsBetweenPlayerAndBlock.Enemies) ? (block.GetUserRelationToOwner(this.OwnerId) != MyRelationsBetweenPlayerAndBlock.Enemies) : false);

        public static Vector3 GetBlockGridOffset(MyCubeBlockDefinition blockDefinition)
        {
            float cubeSize = MyDefinitionManager.Static.GetCubeSize(blockDefinition.CubeSize);
            Vector3 zero = Vector3.Zero;
            if ((blockDefinition.Size.X % 2) == 0)
            {
                zero.X = cubeSize / 2f;
            }
            if ((blockDefinition.Size.Y % 2) == 0)
            {
                zero.Y = cubeSize / 2f;
            }
            if ((blockDefinition.Size.Z % 2) == 0)
            {
                zero.Z = cubeSize / 2f;
            }
            return zero;
        }

        public MyUpgradableBlockComponent GetComponent()
        {
            if (this.m_upgradeComponent == null)
            {
                this.m_upgradeComponent = new MyUpgradableBlockComponent(this);
            }
            return this.m_upgradeComponent;
        }

        private MatrixD GetDamageLocalMatrix()
        {
            MatrixD xd = MatrixD.CreateTranslation((Vector3) (0.85f * base.PositionComp.LocalVolume.Center));
            return ((base.PositionComp != null) ? (xd * base.PositionComp.LocalMatrix) : xd);
        }

        private MatrixD GetDamageWorldMatrix() => 
            (MatrixD.CreateTranslation((Vector3) (0.85f * base.PositionComp.LocalVolume.Center)) * base.WorldMatrix);

        protected virtual string GetDefaultEmissiveParts(byte index) => 
            ((index == 0) ? "Emissive" : ((index == 1) ? "Display" : null));

        private Action<MyCubeBlock> GetDelegate(Action<VRage.Game.ModAPI.IMyCubeBlock> value) => 
            ((Action<MyCubeBlock>) Delegate.CreateDelegate(typeof(Action<MyCubeBlock>), value.Target, value.Method));

        public MyFractureComponentCubeBlock GetFractureComponent()
        {
            MyFractureComponentCubeBlock block = null;
            if (MyFakes.ENABLE_FRACTURE_COMPONENT)
            {
                block = base.Components.Get<MyFractureComponentBase>() as MyFractureComponentCubeBlock;
            }
            return block;
        }

        public virtual BoundingBox GetGeometryLocalBox() => 
            ((base.Model == null) ? new BoundingBox(new Vector3(-this.CubeGrid.GridSize / 2f), new Vector3(this.CubeGrid.GridSize / 2f)) : base.Model.BoundingBox);

        public IMyUseObject GetInteractiveObject(uint shapeKey) => 
            (this.IsFunctional ? this.UseObjectsComponent.GetInteractiveObject(shapeKey) : null);

        public override bool GetIntersectionWithLine(ref LineD line, out MyIntersectionResultLineTriangleEx? t, IntersectionFlags flags = 3)
        {
            Matrix matrix;
            Vector3 vector;
            t = 0;
            if (base.ModelCollision == null)
            {
                return false;
            }
            if (this.BlockDefinition == null)
            {
                return false;
            }
            this.Orientation.GetMatrix(out matrix);
            Vector3.TransformNormal(ref this.BlockDefinition.ModelOffset, ref matrix, out vector);
            matrix.Translation = (this.Position * this.CubeGrid.GridSize) + vector;
            MatrixD customInvMatrix = MatrixD.Invert(base.WorldMatrix);
            t = base.ModelCollision.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, ref customInvMatrix, flags);
            if ((t == 0) && (base.Subparts != null))
            {
                foreach (KeyValuePair<string, MyEntitySubpart> pair in base.Subparts)
                {
                    if (pair.Value == null)
                    {
                        continue;
                    }
                    if (pair.Value.ModelCollision != null)
                    {
                        customInvMatrix = MatrixD.Invert(pair.Value.WorldMatrix);
                        t = pair.Value.ModelCollision.GetTrianglePruningStructure().GetIntersectionWithLine(this, ref line, ref customInvMatrix, flags);
                        if (t != 0)
                        {
                            break;
                        }
                    }
                }
            }
            return (t != 0);
        }

        public void GetLocalMatrix(out Matrix localMatrix)
        {
            this.SlimBlock.GetLocalMatrix(out localMatrix);
        }

        public virtual float GetMass()
        {
            Matrix matrix;
            return ((MyDestructionData.Static == null) ? this.BlockDefinition.Mass : MyDestructionData.Static.GetBlockMass(this.SlimBlock.CalculateCurrentModel(out matrix), this.BlockDefinition));
        }

        public sealed override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            base.GetObjectBuilder(copy);

        public virtual MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CubeBlock block = MyCubeBlockFactory.CreateObjectBuilder(this);
            block.ColorMaskHSV = base.Render.ColorMaskHsv;
            block.SkinSubtypeId = this.m_skinSubtypeId;
            block.EntityId = base.EntityId;
            block.Min = this.Min;
            block.Owner = 0L;
            block.ShareMode = MyOwnershipShareModeEnum.None;
            block.Name = base.Name;
            if (this.m_IDModule != null)
            {
                block.Owner = this.m_IDModule.Owner;
                block.ShareMode = this.m_IDModule.ShareMode;
            }
            if ((MyFakes.ENABLE_SUBBLOCKS && (this.SubBlocks != null)) && (this.SubBlocks.Count != 0))
            {
                block.SubBlocks = new MyObjectBuilder_CubeBlock.MySubBlockId[this.SubBlocks.Count];
                int index = 0;
                foreach (KeyValuePair<string, MySlimBlock> pair in this.SubBlocks)
                {
                    block.SubBlocks[index].SubGridId = pair.Value.CubeGrid.EntityId;
                    block.SubBlocks[index].SubGridName = pair.Key;
                    block.SubBlocks[index].SubBlockPosition = pair.Value.Min;
                    index++;
                }
            }
            block.ComponentContainer = base.Components.Serialize(copy);
            if (copy)
            {
                block.Name = null;
            }
            return block;
        }

        public string GetOwnerFactionTag()
        {
            if (this.IDModule == null)
            {
                return "";
            }
            if (this.IDModule.Owner == 0)
            {
                return "";
            }
            IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(this.IDModule.Owner);
            return ((faction != null) ? faction.Tag : "");
        }

        public MyRelationsBetweenPlayerAndBlock GetPlayerRelationToOwner() => 
            (MyFakes.SHOW_FACTIONS_GUI ? ((this.IDModule != null) ? ((MySession.Static.LocalHumanPlayer == null) ? MyRelationsBetweenPlayerAndBlock.Neutral : this.IDModule.GetUserRelationToOwner(MySession.Static.LocalHumanPlayer.Identity.IdentityId)) : MyRelationsBetweenPlayerAndBlock.NoOwnership) : MyRelationsBetweenPlayerAndBlock.NoOwnership);

        public static bool GetSubBlockDataFromDummy(MyCubeBlockDefinition ownerBlockDefinition, string dummyName, MyModelDummy dummy, bool useOffset, out MyCubeBlockDefinition subBlockDefinition, out MatrixD subBlockMatrix, out Vector3 dummyPosition)
        {
            MyDefinitionId id;
            subBlockDefinition = null;
            subBlockMatrix = MatrixD.Identity;
            dummyPosition = Vector3.Zero;
            if (!dummyName.ToLower().StartsWith(DUMMY_SUBBLOCK_ID))
            {
                return false;
            }
            if (ownerBlockDefinition.SubBlockDefinitions == null)
            {
                return false;
            }
            string key = dummyName.Substring(DUMMY_SUBBLOCK_ID.Length);
            if (!ownerBlockDefinition.SubBlockDefinitions.TryGetValue(key, out id))
            {
                return false;
            }
            MyDefinitionManager.Static.TryGetCubeBlockDefinition(id, out subBlockDefinition);
            if (subBlockDefinition == null)
            {
                return false;
            }
            subBlockMatrix = MatrixD.Normalize(dummy.Matrix);
            Vector3I intVector = Base6Directions.GetIntVector(Base6Directions.GetClosestDirection((Vector3) subBlockMatrix.Forward));
            if (Math.Abs((double) (1.0 - Vector3D.Dot(subBlockMatrix.Forward, (Vector3D) intVector))) <= 1E-08)
            {
                subBlockMatrix.Forward = (Vector3D) intVector;
            }
            Vector3I vectori2 = Base6Directions.GetIntVector(Base6Directions.GetClosestDirection((Vector3) subBlockMatrix.Right));
            if (Math.Abs((double) (1.0 - Vector3D.Dot(subBlockMatrix.Right, (Vector3D) vectori2))) <= 1E-08)
            {
                subBlockMatrix.Right = (Vector3D) vectori2;
            }
            Vector3I vectori3 = Base6Directions.GetIntVector(Base6Directions.GetClosestDirection((Vector3) subBlockMatrix.Up));
            if (Math.Abs((double) (1.0 - Vector3D.Dot(subBlockMatrix.Up, (Vector3D) vectori3))) <= 1E-08)
            {
                subBlockMatrix.Up = (Vector3D) vectori3;
            }
            dummyPosition = (Vector3) subBlockMatrix.Translation;
            if (useOffset)
            {
                Vector3 blockGridOffset = GetBlockGridOffset(subBlockDefinition);
                subBlockMatrix.Translation -= Vector3D.TransformNormal(blockGridOffset, subBlockMatrix);
            }
            return true;
        }

        public DictionaryReader<string, MySlimBlock> GetSubBlocks() => 
            new DictionaryReader<string, MySlimBlock>(this.SubBlocks);

        public MyRelationsBetweenPlayerAndBlock GetUserRelationToOwner(long identityId) => 
            (MyFakes.SHOW_FACTIONS_GUI ? ((this.IDModule != null) ? this.IDModule.GetUserRelationToOwner(identityId) : MyRelationsBetweenPlayerAndBlock.NoOwnership) : MyRelationsBetweenPlayerAndBlock.NoOwnership);

        public void Init()
        {
            Matrix matrix;
            string str;
            base.PositionComp.LocalAABB = new BoundingBox(new Vector3(-this.SlimBlock.CubeGrid.GridSize / 2f), new Vector3(this.SlimBlock.CubeGrid.GridSize / 2f));
            base.Components.Add<MyUseObjectsComponentBase>(new MyUseObjectsComponent());
            if (this.BlockDefinition.CubeDefinition != null)
            {
                this.SlimBlock.Orientation = MyCubeGridDefinitions.GetTopologyUniqueOrientation(this.BlockDefinition.CubeDefinition.CubeTopology, this.Orientation);
            }
            this.CalcLocalMatrix(out matrix, out str);
            if (!string.IsNullOrEmpty(str))
            {
                float? scale = null;
                this.Init(null, str, null, scale, null);
                this.OnModelChange();
            }
            base.Render.EnableColorMaskHsv = true;
            base.Render.FadeIn = this.CubeGrid.Render.FadeIn;
            base.Render.SkipIfTooSmall = false;
            this.CheckConnectionAllowed = false;
            base.PositionComp.SetLocalMatrix(ref matrix, this.CubeGrid, true);
            base.Save = false;
            if (this.CubeGrid.CreatePhysics)
            {
                this.UseObjectsComponent.LoadDetectorsFromModel();
            }
            this.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            if ((base.Subparts != null) && (base.Subparts.Count > 0))
            {
                bool flag = false;
                using (Dictionary<string, MyEntitySubpart>.ValueCollection.Enumerator enumerator = base.Subparts.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!(enumerator.Current.Render is MyParentedSubpartRenderComponent))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    base.NeedsWorldMatrix = true;
                }
            }
        }

        public virtual void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            if (builder.EntityId == 0)
            {
                base.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            }
            else if (builder.EntityId != 0)
            {
                base.EntityId = builder.EntityId;
            }
            base.Name = builder.Name;
            this.NumberInGrid = cubeGrid.BlockCounter.GetNextNumber(builder.GetId());
            base.Render.ColorMaskHsv = (Vector3) builder.ColorMaskHSV;
            this.UpdateSkin();
            base.Render.FadeIn = cubeGrid.Render.FadeIn;
            if (MyFakes.ENABLE_SUBBLOCKS && ((this.BlockDefinition.SubBlockDefinitions != null) && (this.BlockDefinition.SubBlockDefinitions.Count > 0)))
            {
                if ((builder.SubBlocks == null) || (builder.SubBlocks.Length == 0))
                {
                    if (Sync.IsServer)
                    {
                        this.m_loadedSubBlocks = new List<MyObjectBuilder_CubeBlock.MySubBlockId>();
                        this.SpawnSubBlocks();
                        base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                    }
                }
                else
                {
                    this.m_loadedSubBlocks = new List<MyObjectBuilder_CubeBlock.MySubBlockId>();
                    MyObjectBuilder_CubeBlock.MySubBlockId[] subBlocks = builder.SubBlocks;
                    int index = 0;
                    while (true)
                    {
                        if (index >= subBlocks.Length)
                        {
                            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
                            break;
                        }
                        MyObjectBuilder_CubeBlock.MySubBlockId item = subBlocks[index];
                        this.m_loadedSubBlocks.Add(item);
                        index++;
                    }
                }
            }
            this.UsesEmissivePreset = (this.BlockDefinition.EmissiveColorPreset != MyStringHash.NullOrEmpty) ? MyEmissiveColorPresets.ContainsPreset(this.BlockDefinition.EmissiveColorPreset) : false;
            base.Components.InitComponents(builder.TypeId, builder.SubtypeId, builder.ComponentContainer);
            base.Init(null);
            MyRenderComponentBase render = base.Render;
            render.PersistentFlags |= MyPersistentEntityFlags2.CastShadows;
            this.Init();
            base.AddDebugRenderComponent(new MyDebugRenderComponentCubeBlock(this));
            this.InitOwnership(builder);
        }

        public void Init(MyObjectBuilder_CubeBlock builder, VRage.Game.ModAPI.IMyCubeGrid cubeGrid)
        {
            if (cubeGrid is MyCubeGrid)
            {
                this.Init(builder, cubeGrid as MyCubeGrid);
            }
        }

        public override void InitComponents()
        {
            if (base.Render == null)
            {
                base.Render = new MyRenderComponentCubeBlock();
            }
            if (base.PositionComp == null)
            {
                base.PositionComp = new MyBlockPosComponent();
            }
            base.InitComponents();
        }

        private void InitOwnership(MyObjectBuilder_CubeBlock builder)
        {
            MyEntityOwnershipComponent component = base.Components.Get<MyEntityOwnershipComponent>();
            bool flag = this.BlockDefinition.ContainsComputer();
            if (this.UseObjectsComponent != null)
            {
                int num1;
                if (!flag)
                {
                    num1 = (int) (this.UseObjectsComponent.GetDetectors("ownership").Count > 0);
                }
                else
                {
                    num1 = 1;
                }
                flag = (bool) num1;
            }
            if (flag)
            {
                this.m_IDModule = new MyIDModule();
                if (MySession.Static.Settings.ResetOwnership && Sync.IsServer)
                {
                    this.m_IDModule.Owner = 0L;
                    this.m_IDModule.ShareMode = MyOwnershipShareModeEnum.None;
                }
                else
                {
                    if (builder.ShareMode == ~MyOwnershipShareModeEnum.None)
                    {
                        builder.ShareMode = MyOwnershipShareModeEnum.None;
                    }
                    MyEntityIdentifier.ID_OBJECT_TYPE idObjectType = MyEntityIdentifier.GetIdObjectType(builder.Owner);
                    if (((builder.Owner != 0) && ((idObjectType != MyEntityIdentifier.ID_OBJECT_TYPE.NPC) && (idObjectType != MyEntityIdentifier.ID_OBJECT_TYPE.SPAWN_GROUP))) && !Sync.Players.HasIdentity(builder.Owner))
                    {
                        builder.Owner = 0L;
                    }
                    this.m_IDModule.Owner = builder.Owner;
                    this.m_IDModule.ShareMode = builder.ShareMode;
                }
            }
            if ((component != null) && (builder.Owner != 0))
            {
                component.OwnerId = builder.Owner;
                component.ShareMode = MyOwnershipShareModeEnum.None;
            }
        }

        private void InitSubBlocks()
        {
            if (MyFakes.ENABLE_SUBBLOCKS && (this.m_loadedSubBlocks != null))
            {
                bool flag = this.AllSubBlocksInitialized();
                bool spawned = ((this.m_loadedSubBlocks.Count == 0) && Sync.IsServer) & flag;
                if (!flag)
                {
                    for (int i = this.m_loadedSubBlocks.Count - 1; i >= 0; i--)
                    {
                        VRage.Game.Entity.MyEntity entity;
                        MyObjectBuilder_CubeBlock.MySubBlockId id = this.m_loadedSubBlocks[i];
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(id.SubGridId, out entity, false))
                        {
                            MyCubeGrid grid = entity as MyCubeGrid;
                            if (grid != null)
                            {
                                MySlimBlock cubeBlock = grid.GetCubeBlock((Vector3I) id.SubBlockPosition);
                                if (cubeBlock != null)
                                {
                                    this.AddSubBlock(id.SubGridName, cubeBlock);
                                }
                            }
                            this.m_loadedSubBlocks.RemoveAt(i);
                        }
                    }
                }
                if (this.AllSubBlocksInitialized())
                {
                    this.m_loadedSubBlocks = null;
                    if (spawned || !flag)
                    {
                        this.SubBlocksInitialized(spawned);
                    }
                }
            }
        }

        private void Inventory_ContentsChanged(MyInventoryBase obj)
        {
            this.m_inventorymassDirty = true;
        }

        internal virtual void OnAddedNeighbours()
        {
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            this.UpdateIsWorking();
            if (this.UsesEmissivePreset)
            {
                this.CheckEmissiveState(false);
            }
            MyCubeBlockDefinition.BuildProgressModel[] buildProgressModels = this.BlockDefinition.BuildProgressModels;
            for (int i = 0; i < buildProgressModels.Length; i++)
            {
                MyRenderProxy.PreloadModel(buildProgressModels[i].File, 1f, false);
            }
            if ((MyFakes.SHOW_DAMAGE_EFFECTS && ((this.CubeGrid.Physics != null) && ((this.SlimBlock != null) && !this.BlockDefinition.RatioEnoughForDamageEffect(this.SlimBlock.BuildIntegrity / this.SlimBlock.MaxIntegrity)))) && this.BlockDefinition.RatioEnoughForDamageEffect(this.SlimBlock.Integrity / this.SlimBlock.MaxIntegrity))
            {
                this.SetDamageEffect(true);
            }
        }

        public virtual void OnBuildSuccess(long builtBy, bool instantBuild)
        {
        }

        protected virtual void OnConstraintAdded(GridLinkTypeEnum type, VRage.ModAPI.IMyEntity attachedEntity)
        {
            MyCubeGrid childNode = attachedEntity as MyCubeGrid;
            if ((childNode != null) && !MyCubeGridGroups.Static.GetGroups(type).LinkExists(base.EntityId, this.CubeGrid, childNode))
            {
                MyCubeGridGroups.Static.CreateLink(type, base.EntityId, this.CubeGrid, childNode);
            }
        }

        protected virtual void OnConstraintRemoved(GridLinkTypeEnum type, VRage.ModAPI.IMyEntity detachedEntity)
        {
            MyCubeGrid child = detachedEntity as MyCubeGrid;
            if (child != null)
            {
                MyCubeGridGroups.Static.BreakLink(type, base.EntityId, this.CubeGrid, child);
            }
        }

        public virtual void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            if (MyFakes.ENABLE_FRACTURE_COMPONENT && base.Components.Has<MyFractureComponentBase>())
            {
                MyFractureComponentCubeBlock fractureComponent = this.GetFractureComponent();
                if (fractureComponent != null)
                {
                    fractureComponent.OnCubeGridChanged();
                }
            }
        }

        public virtual void OnDestroy()
        {
            this.SetDamageEffect(false);
        }

        internal virtual void OnIntegrityChanged(float buildIntegrity, float integrity, bool setOwnership, long owner, MyOwnershipShareModeEnum sharing = 1)
        {
            if (this.BlockDefinition.ContainsComputer())
            {
                MyEntityOwnershipComponent component = base.Components.Get<MyEntityOwnershipComponent>();
                if (setOwnership)
                {
                    if ((this.m_IDModule.Owner == 0) && Sync.IsServer)
                    {
                        this.CubeGrid.ChangeOwnerRequest(this.CubeGrid, this, owner, sharing);
                    }
                    if (((component != null) && (component.OwnerId == 0)) && Sync.IsServer)
                    {
                        this.CubeGrid.ChangeOwnerRequest(this.CubeGrid, this, owner, sharing);
                    }
                }
                else
                {
                    if ((this.m_IDModule.Owner != 0) && Sync.IsServer)
                    {
                        sharing = MyOwnershipShareModeEnum.None;
                        this.CubeGrid.ChangeOwnerRequest(this.CubeGrid, this, 0L, sharing);
                    }
                    if (((component != null) && (component.OwnerId != 0)) && Sync.IsServer)
                    {
                        sharing = MyOwnershipShareModeEnum.None;
                        this.CubeGrid.ChangeOwnerRequest(this.CubeGrid, this, 0L, sharing);
                    }
                }
            }
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            this.CubeGrid.RegisterInventory(this);
            if ((inventory != null) && MyPerGameSettings.InventoryMass)
            {
                inventory.ContentsChanged += new Action<MyInventoryBase>(this.Inventory_ContentsChanged);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            this.CubeGrid.UnregisterInventory(this);
            if ((inventory != null) && MyPerGameSettings.InventoryMass)
            {
                inventory.ContentsChanged -= new Action<MyInventoryBase>(this.Inventory_ContentsChanged);
            }
        }

        public virtual void OnModelChange()
        {
            if (this.UsesEmissivePreset)
            {
                this.CheckEmissiveState(true);
            }
        }

        protected virtual void OnOwnershipChanged()
        {
        }

        public virtual void OnRegisteredToGridSystems()
        {
            if (this.m_upgradeComponent != null)
            {
                this.m_upgradeComponent.Refresh(this);
            }
        }

        public virtual void OnRemovedByCubeBuilder()
        {
            base.SetFadeOut(false);
            if (MyFakes.ENABLE_SUBBLOCKS && (this.SubBlocks != null))
            {
                foreach (KeyValuePair<string, MySlimBlock> pair in this.SubBlocks)
                {
                    MySlimBlock block = pair.Value;
                    block.CubeGrid.RemoveBlock(block, true);
                }
            }
            this.SetDamageEffect(false);
        }

        public override void OnRemovedFromScene(object source)
        {
            this.StopDamageEffect(true);
            base.OnRemovedFromScene(source);
        }

        internal virtual void OnRemovedNeighbours()
        {
        }

        protected virtual void OnSubBlockClosing(MySlimBlock subBlock)
        {
            subBlock.FatBlock.OnClosing -= new Action<VRage.Game.Entity.MyEntity>(this.SubBlock_OnClosing);
            if (this.SubBlocks != null)
            {
                this.SubBlocks.Remove(subBlock.FatBlock.SubBlockName);
            }
        }

        internal virtual void OnTransformed(ref MatrixI transform)
        {
        }

        public virtual void OnUnregisteredFromGridSystems()
        {
        }

        public override void RefreshModels(string modelPath, string modelCollisionPath)
        {
            MyModel modelOnlyData = MyModels.GetModelOnlyData(modelPath);
            if (modelOnlyData != null)
            {
                modelOnlyData.Rescale(this.CubeGrid.GridScale);
            }
            if (modelCollisionPath != null)
            {
                modelOnlyData = MyModels.GetModelOnlyData(modelCollisionPath);
                if (modelOnlyData != null)
                {
                    modelOnlyData.Rescale(this.CubeGrid.GridScale);
                }
            }
            base.RefreshModels(modelPath, modelCollisionPath);
        }

        public unsafe void ReleaseInventory(MyInventory inventory, bool damageContent = false)
        {
            if ((inventory != null) && Sync.IsServer)
            {
                MyEntityInventorySpawnComponent component = null;
                if (base.Components.TryGet<MyEntityInventorySpawnComponent>(out component))
                {
                    component.SpawnInventoryContainer(true);
                    MyInventory inventory2 = new MyInventory(inventory.MaxVolume, inventory.MaxMass, Vector3.One, inventory.GetFlags());
                    base.Components.Add<MyInventoryBase>(inventory2);
                }
                else
                {
                    foreach (MyPhysicalInventoryItem item in inventory.GetItems())
                    {
                        MyPhysicalInventoryItem inventoryItem = item;
                        if (damageContent && (item.Content.TypeId == typeof(MyObjectBuilder_Component)))
                        {
                            MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref inventoryItem.Amount;
                            pointPtr1[0] *= (MyFixedPoint) MyDefinitionManager.Static.GetComponentDefinition(item.Content.GetId()).DropProbability;
                            MyPhysicalInventoryItem* itemPtr1 = (MyPhysicalInventoryItem*) ref inventoryItem;
                            itemPtr1->Amount = MyFixedPoint.Floor(inventoryItem.Amount);
                            if (inventoryItem.Amount == 0)
                            {
                                continue;
                            }
                        }
                        MyFloatingObjects.EnqueueInventoryItemSpawn(inventoryItem, base.PositionComp.WorldAABB, (this.CubeGrid.Physics != null) ? this.CubeGrid.Physics.GetVelocityAtPoint(base.PositionComp.GetPosition()) : Vector3.Zero);
                    }
                    inventory.Clear(true);
                }
            }
        }

        public int RemoveEffect(string effectName, int exception = -1)
        {
            if (((this.BlockDefinition == null) || (this.BlockDefinition.Effects == null)) || (this.m_activeEffects == null))
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < this.BlockDefinition.Effects.Length; i++)
            {
                if (effectName.Equals(this.BlockDefinition.Effects[i].Name))
                {
                    for (int j = 0; j < this.m_activeEffects.Count; j++)
                    {
                        if ((this.m_activeEffects[j].EffectId == i) && (i != exception))
                        {
                            this.m_activeEffects[j].Stop();
                            this.m_activeEffects.RemoveAt(j);
                            num++;
                        }
                    }
                }
            }
            if ((this.m_activeEffects.Count == 0) && !this.m_wasUpdatedEachFrame)
            {
                base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
            }
            return num;
        }

        protected bool RemoveSubBlock(string subBlockName, bool removeFromGrid = true)
        {
            MySlimBlock block;
            if ((this.SubBlocks != null) && this.SubBlocks.TryGetValue(subBlockName, out block))
            {
                if (removeFromGrid)
                {
                    block.CubeGrid.RemoveBlock(block, true);
                }
                if (this.SubBlocks.Remove(subBlockName))
                {
                    if (block.FatBlock != null)
                    {
                        block.FatBlock.OwnerBlock = null;
                        block.FatBlock.SubBlockName = null;
                    }
                    return true;
                }
            }
            return false;
        }

        void Sandbox.ModAPI.Ingame.IMyUpgradableBlock.GetUpgrades(out Dictionary<string, float> upgrades)
        {
            upgrades = new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> pair in this.UpgradeValues)
            {
                upgrades.Add(pair.Key, pair.Value);
            }
        }

        public virtual void SetDamageEffect(bool show)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                int num1;
                if (MyPerGameSettings.UseNewDamageEffects & show)
                {
                    this.SetEffect("Damage", this.SlimBlock.Integrity / this.SlimBlock.MaxIntegrity, false, false, true);
                }
                if ((this.m_activeEffects == null) || !MyPerGameSettings.UseNewDamageEffects)
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (this.m_activeEffects.Count > 0);
                }
                bool flag = (bool) num1;
                if (MyPerGameSettings.UseNewDamageEffects && !show)
                {
                    this.RemoveEffect("Damage", -1);
                }
                if (MyFakes.SHOW_DAMAGE_EFFECTS && (!string.IsNullOrEmpty(this.BlockDefinition.DamageEffectName) || (this.BlockDefinition.DamageEffectID != null)))
                {
                    if (!show && (this.m_damageEffect != null))
                    {
                        this.m_damageEffect.Stop(false);
                        this.m_damageEffect.StopLights();
                        if (this.CubeGrid.Physics != null)
                        {
                            this.m_damageEffect.Velocity = this.CubeGrid.Physics.LinearVelocity;
                        }
                        this.m_damageEffect = null;
                    }
                    if ((show && ((this.m_damageEffect == null) && !flag)) && MySandboxGame.Static.EnableDamageEffects)
                    {
                        string damageEffectName = this.BlockDefinition.DamageEffectName;
                        if (string.IsNullOrEmpty(damageEffectName) && (this.BlockDefinition.DamageEffectID != null))
                        {
                            MyParticleEffect effect;
                            MyParticlesLibrary.GetParticleEffectsById().TryGetValue(this.BlockDefinition.DamageEffectID.Value, out effect);
                            if (effect != null)
                            {
                                damageEffectName = effect.Name;
                            }
                        }
                        MatrixD damageLocalMatrix = this.GetDamageLocalMatrix();
                        Vector3D translation = base.PositionComp.WorldMatrix.Translation;
                        if (MyParticlesManager.TryCreateParticleEffect(damageEffectName, ref damageLocalMatrix, ref translation, base.Render.ParentIDs[0], out this.m_damageEffect))
                        {
                            this.m_damageEffect.UserScale = base.Model.BoundingBox.Perimeter * 0.018f;
                            this.m_damageEffect.OnDelete += new EventHandler(this.damageEffect_OnDelete);
                        }
                    }
                }
            }
        }

        public virtual void SetDamageEffectDelayed(bool show)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            this.m_setDamagedEffectDelayed = true;
        }

        public bool SetEffect(string effectName, bool stopPrevious = false) => 
            this.SetEffect(effectName, 0f, stopPrevious, true, false);

        public bool SetEffect(string effectName, float parameter, bool stopPrevious = false, bool ignoreParameter = false, bool removeSameNameEffects = false)
        {
            if ((this.BlockDefinition == null) || (this.BlockDefinition.Effects == null))
            {
                return false;
            }
            int exception = -1;
            int index = 0;
            while (true)
            {
                if (index < this.BlockDefinition.Effects.Length)
                {
                    if (!effectName.Equals(this.BlockDefinition.Effects[index].Name) || (!ignoreParameter && ((parameter < this.BlockDefinition.Effects[index].ParameterMin) || (parameter > this.BlockDefinition.Effects[index].ParameterMax))))
                    {
                        index++;
                        continue;
                    }
                    exception = index;
                }
                if (exception == -1)
                {
                    return false;
                }
                if (this.m_activeEffects == null)
                {
                    this.m_activeEffects = new List<MyCubeBlockEffect>();
                }
                index = 0;
                while (true)
                {
                    if (index < this.m_activeEffects.Count)
                    {
                        if (this.m_activeEffects[index].EffectId != exception)
                        {
                            index++;
                            continue;
                        }
                        if (!stopPrevious)
                        {
                            return false;
                        }
                        this.m_activeEffects[index].Stop();
                        this.m_activeEffects.RemoveAt(index);
                    }
                    if (removeSameNameEffects)
                    {
                        this.RemoveEffect(effectName, exception);
                    }
                    if (this.m_activeEffects.Count == 0)
                    {
                        this.m_wasUpdatedEachFrame = (base.NeedsUpdate & MyEntityUpdateEnum.EACH_FRAME) != MyEntityUpdateEnum.NONE;
                        base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    }
                    this.m_activeEffects.Add(new MyCubeBlockEffect(exception, this.BlockDefinition.Effects[exception], this));
                    return true;
                }
            }
        }

        public bool SetEmissiveState(MyStringHash state, uint renderObjectId, string namedPart = null)
        {
            MyEmissiveColorStateResult result;
            if ((renderObjectId == uint.MaxValue) || !MyEmissiveColorPresets.LoadPresetState(this.BlockDefinition.EmissiveColorPreset, state, out result))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(namedPart))
            {
                UpdateNamedEmissiveParts(renderObjectId, namedPart, result.EmissiveColor, result.Emissivity);
            }
            else
            {
                byte index = 0;
                while (true)
                {
                    string defaultEmissiveParts = this.GetDefaultEmissiveParts(index);
                    if (string.IsNullOrEmpty(defaultEmissiveParts))
                    {
                        break;
                    }
                    UpdateNamedEmissiveParts(renderObjectId, defaultEmissiveParts, result.EmissiveColor, result.Emissivity);
                    index = (byte) (index + 1);
                }
            }
            return true;
        }

        public virtual bool SetEmissiveStateDamaged() => 
            ((base.Render != null) && ((base.Render.RenderObjectIDs.Length != 0) && this.SetEmissiveState(m_emissiveNames.Damaged, base.Render.RenderObjectIDs[0], null)));

        public virtual bool SetEmissiveStateDisabled() => 
            ((base.Render != null) && ((base.Render.RenderObjectIDs.Length != 0) && this.SetEmissiveState(m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], null)));

        public virtual bool SetEmissiveStateWorking() => 
            ((base.Render != null) && ((base.Render.RenderObjectIDs.Length != 0) && this.SetEmissiveState(m_emissiveNames.Working, base.Render.RenderObjectIDs[0], null)));

        private void SpawnSubBlocks()
        {
            if (MyFakes.ENABLE_SUBBLOCKS && this.CubeGrid.CreatePhysics)
            {
                foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies)
                {
                    MyCubeBlockDefinition definition;
                    MatrixD xd;
                    Vector3 vector;
                    Matrix matrix;
                    if (!GetSubBlockDataFromDummy(this.BlockDefinition, pair.Key, pair.Value, true, out definition, out xd, out vector))
                    {
                        continue;
                    }
                    string dummyName = pair.Key.Substring(DUMMY_SUBBLOCK_ID.Length);
                    this.GetLocalMatrix(out matrix);
                    Matrix worldMatrix = (Matrix) ((xd * matrix) * this.CubeGrid.WorldMatrix);
                    MySlimBlock subblock = null;
                    MyCubeGrid grid = MyCubeBuilder.SpawnDynamicGrid(definition, null, worldMatrix, new Vector3(0f, -1f, 0f), 0L, MyCubeBuilder.SpawnFlags.AddToScene | MyCubeBuilder.SpawnFlags.CreatePhysics | MyCubeBuilder.SpawnFlags.EnableSmallTolargeConnections, 0L, null);
                    if (grid != null)
                    {
                        subblock = grid.GetCubeBlock(Vector3I.Zero);
                        if ((subblock != null) && (subblock.FatBlock != null))
                        {
                            this.AddSubBlock(dummyName, subblock);
                        }
                    }
                }
            }
        }

        public virtual void StopDamageEffect(bool stopSound = true)
        {
            if (MyPerGameSettings.UseNewDamageEffects)
            {
                this.RemoveEffect("Damage", -1);
            }
            if ((MyFakes.SHOW_DAMAGE_EFFECTS && (!string.IsNullOrEmpty(this.BlockDefinition.DamageEffectName) || (this.BlockDefinition.DamageEffectID != null))) && (this.m_damageEffect != null))
            {
                this.m_damageEffect.StopEmitting(10f);
                this.m_damageEffect.StopLights();
                if (this.CubeGrid.Physics != null)
                {
                    this.m_damageEffect.Velocity = this.CubeGrid.Physics.LinearVelocity;
                }
                this.m_damageEffect = null;
            }
        }

        private void SubBlock_OnClosing(VRage.Game.Entity.MyEntity obj)
        {
            MyCubeBlock subblock = obj as MyCubeBlock;
            if (subblock != null)
            {
                KeyValuePair<string, MySlimBlock> pair = this.SubBlocks.FirstOrDefault<KeyValuePair<string, MySlimBlock>>(p => p.Value == subblock.SlimBlock);
                if (pair.Value != null)
                {
                    this.OnSubBlockClosing(pair.Value);
                }
            }
        }

        protected virtual void SubBlocksInitialized(bool spawned)
        {
        }

        public bool TryGetSubBlock(string name, out MySlimBlock block)
        {
            if (this.SubBlocks != null)
            {
                return this.SubBlocks.TryGetValue(name, out block);
            }
            block = null;
            return false;
        }

        public override void UpdateAfterSimulation100()
        {
            if (this.m_inventorymassDirty)
            {
                this.CubeGrid.SetInventoryMassDirty();
                this.m_inventorymassDirty = false;
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if ((this.m_activeEffects != null) && MyPerGameSettings.UseNewDamageEffects)
            {
                for (int i = 0; i < this.m_activeEffects.Count; i++)
                {
                    if (!this.m_activeEffects[i].CanBeDeleted)
                    {
                        this.m_activeEffects[i].Update();
                    }
                    else
                    {
                        this.m_activeEffects[i].Stop();
                        this.m_activeEffects.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (MyFakes.ENABLE_SUBBLOCKS && (this.m_loadedSubBlocks != null))
            {
                this.InitSubBlocks();
            }
        }

        public static void UpdateEmissiveParts(uint renderObjectId, float emissivity, Color emissivePartColor, Color displayPartColor)
        {
            if (renderObjectId != uint.MaxValue)
            {
                UpdateNamedEmissiveParts(renderObjectId, "Emissive", emissivePartColor, emissivity);
                UpdateNamedEmissiveParts(renderObjectId, "Display", displayPartColor, emissivity);
            }
        }

        public void UpdateIsWorking()
        {
            bool flag = this.CheckIsWorking();
            bool flag2 = flag != this.IsWorking;
            this.IsWorking = flag;
            if (flag2 && (this.IsWorkingChanged != null))
            {
                this.IsWorkingChanged(this);
            }
            if (this.UsesEmissivePreset & flag2)
            {
                this.CheckEmissiveState(false);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (MyFakes.ENABLE_SUBBLOCKS && (this.m_loadedSubBlocks != null))
            {
                this.InitSubBlocks();
            }
            if (this.m_setDamagedEffectDelayed != null)
            {
                this.SetDamageEffect(this.m_setDamagedEffectDelayed.Value);
                this.m_setDamagedEffectDelayed = null;
            }
        }

        private void UpdateSkin()
        {
            if (this.m_skinSubtypeId != this.SlimBlock.SkinSubtypeId)
            {
                this.m_skinSubtypeId = this.SlimBlock.SkinSubtypeId;
                Dictionary<string, MyTextureChange> assetModifierDefinitionForRender = null;
                if (this.m_skinSubtypeId != MyStringHash.NullOrEmpty)
                {
                    assetModifierDefinitionForRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(this.m_skinSubtypeId);
                }
                base.Render.TextureChanges = assetModifierDefinitionForRender;
            }
        }

        public virtual void UpdateVisual()
        {
            Matrix matrix;
            this.UpdateSkin();
            string model = this.SlimBlock.CalculateCurrentModel(out matrix);
            bool flag = (base.Model != null) && (base.Model.AssetName != model);
            if ((flag || (base.Render.ColorMaskHsv != this.SlimBlock.ColorMaskHSV)) || (base.Render.Transparency != this.SlimBlock.Dithering))
            {
                base.Render.ColorMaskHsv = this.SlimBlock.ColorMaskHSV;
                this.m_skinSubtypeId = this.SlimBlock.SkinSubtypeId;
                Dictionary<string, MyTextureChange> assetModifierDefinitionForRender = null;
                if (this.m_skinSubtypeId != MyStringHash.NullOrEmpty)
                {
                    assetModifierDefinitionForRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(this.m_skinSubtypeId);
                }
                base.Render.TextureChanges = assetModifierDefinitionForRender;
                base.Render.Transparency = this.SlimBlock.Dithering;
                MatrixD worldMatrix = matrix * this.CubeGrid.WorldMatrix;
                worldMatrix.Translation = base.WorldMatrix.Translation;
                base.PositionComp.SetWorldMatrix(worldMatrix, null, true, true, true, false, false, false);
                this.RefreshModels(model, null);
                base.Render.RemoveRenderObjects();
                base.Render.AddRenderObjects();
                if (this.CubeGrid.CreatePhysics & flag)
                {
                    this.UseObjectsComponent.LoadDetectorsFromModel();
                }
                this.OnModelChange();
            }
        }

        internal virtual void UpdateWorldMatrix()
        {
            Matrix matrix;
            this.GetLocalMatrix(out matrix);
            base.PositionComp.SetWorldMatrix(matrix, null, true, true, true, false, false, false);
        }

        bool IMyComponentOwner<MyIDModule>.GetComponent(out MyIDModule component)
        {
            component = this.m_IDModule;
            return ((this.m_IDModule != null) && this.IsFunctional);
        }

        void VRage.Game.ModAPI.IMyCubeBlock.CalcLocalMatrix(out Matrix localMatrix, out string currModel)
        {
            this.CalcLocalMatrix(out localMatrix, out currModel);
        }

        string VRage.Game.ModAPI.IMyCubeBlock.CalculateCurrentModel(out Matrix orientation) => 
            this.CalculateCurrentModel(out orientation);

        bool VRage.Game.ModAPI.IMyCubeBlock.DebugDraw()
        {
            base.DebugDraw();
            return true;
        }

        MyObjectBuilder_CubeBlock VRage.Game.ModAPI.IMyCubeBlock.GetObjectBuilderCubeBlock(bool copy) => 
            this.GetObjectBuilderCubeBlock(copy);

        MyRelationsBetweenPlayerAndBlock VRage.Game.ModAPI.IMyCubeBlock.GetPlayerRelationToOwner() => 
            this.GetPlayerRelationToOwner();

        MyRelationsBetweenPlayerAndBlock VRage.Game.ModAPI.IMyCubeBlock.GetUserRelationToOwner(long playerId) => 
            this.GetUserRelationToOwner(playerId);

        void VRage.Game.ModAPI.IMyCubeBlock.Init()
        {
            this.Init();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.Init(MyObjectBuilder_CubeBlock builder, VRage.Game.ModAPI.IMyCubeGrid cubeGrid)
        {
            this.Init(builder, cubeGrid);
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnBuildSuccess(long builtBy)
        {
            this.OnBuildSuccess(builtBy, false);
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnBuildSuccess(long builtBy, bool instantBuild)
        {
            this.OnBuildSuccess(builtBy, instantBuild);
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnDestroy()
        {
            this.OnDestroy();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnModelChange()
        {
            this.OnModelChange();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnRegisteredToGridSystems()
        {
            this.OnRegisteredToGridSystems();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnRemovedByCubeBuilder()
        {
            this.OnRemovedByCubeBuilder();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.OnUnregisteredFromGridSystems()
        {
            this.OnUnregisteredFromGridSystems();
        }

        string VRage.Game.ModAPI.IMyCubeBlock.RaycastDetectors(Vector3D worldFrom, Vector3D worldTo) => 
            base.Components.Get<MyUseObjectsComponentBase>().RaycastDetectors(worldFrom, worldTo);

        void VRage.Game.ModAPI.IMyCubeBlock.ReloadDetectors(bool refreshNetworks)
        {
            base.Components.Get<MyUseObjectsComponentBase>().LoadDetectorsFromModel();
        }

        int VRage.Game.ModAPI.IMyCubeBlock.RemoveEffect(string effectName, int exception) => 
            this.RemoveEffect(effectName, exception);

        void VRage.Game.ModAPI.IMyCubeBlock.SetDamageEffect(bool start)
        {
            this.SetDamageEffect(start);
        }

        bool VRage.Game.ModAPI.IMyCubeBlock.SetEffect(string effectName, bool stopPrevious) => 
            this.SetEffect(effectName, stopPrevious);

        bool VRage.Game.ModAPI.IMyCubeBlock.SetEffect(string effectName, float parameter, bool stopPrevious, bool ignoreParameter, bool removeSameNameEffects) => 
            this.SetEffect(effectName, parameter, stopPrevious, ignoreParameter, removeSameNameEffects);

        void VRage.Game.ModAPI.IMyCubeBlock.UpdateIsWorking()
        {
            this.UpdateIsWorking();
        }

        void VRage.Game.ModAPI.IMyCubeBlock.UpdateVisual()
        {
            this.UpdateVisual();
        }

        protected virtual void WorldPositionChanged(object source)
        {
        }

        public bool IsBeingHacked
        {
            get
            {
                MyTerminalBlock block = this as MyTerminalBlock;
                return ((block != null) && block.IsBeingHacked);
            }
        }

        public bool UsesEmissivePreset { get; private set; }

        public MyPhysicsBody Physics
        {
            get => 
                (base.Physics as MyPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public long OwnerId =>
            ((this.IDModule == null) ? 0L : this.IDModule.Owner);

        public long BuiltBy =>
            ((this.SlimBlock == null) ? 0L : this.SlimBlock.BuiltBy);

        public MyResourceSinkComponent ResourceSink
        {
            get => 
                this.m_sinkComp;
            protected set
            {
                if (base.ContainsDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerReciever)))
                {
                    base.RemoveDebugRenderComponent(typeof(MyDebugRenderComponentDrawPowerReciever));
                }
                if (base.Components.Contains(typeof(MyResourceSinkComponent)))
                {
                    base.Components.Remove<MyResourceSinkComponent>();
                }
                base.Components.Add<MyResourceSinkComponent>(value);
                base.AddDebugRenderComponent(new MyDebugRenderComponentDrawPowerReciever(value, this));
                this.m_sinkComp = value;
            }
        }

        public MyCubeBlockDefinition BlockDefinition =>
            this.SlimBlock.BlockDefinition;

        public Vector3I Min =>
            this.SlimBlock.Min;

        public Vector3I Max =>
            this.SlimBlock.Max;

        public MyBlockOrientation Orientation =>
            this.SlimBlock.Orientation;

        public Vector3I Position =>
            this.SlimBlock.Position;

        public MyCubeGrid CubeGrid =>
            this.SlimBlock.CubeGrid;

        public MyUseObjectsComponentBase UseObjectsComponent =>
            base.Components.Get<MyUseObjectsComponentBase>();

        public bool CheckConnectionAllowed
        {
            get => 
                this.m_checkConnectionAllowed;
            set
            {
                this.m_checkConnectionAllowed = value;
                Action<MyCubeBlock> checkConnectionChanged = this.CheckConnectionChanged;
                if (checkConnectionChanged != null)
                {
                    checkConnectionChanged(this);
                }
            }
        }

        public int NumberInGrid
        {
            get => 
                this.m_numberInGrid;
            set
            {
                this.m_numberInGrid = value;
                if (this.m_numberInGrid > 1)
                {
                    this.DisplayNameText = this.BlockDefinition.DisplayNameText + " " + this.m_numberInGrid;
                }
                else
                {
                    this.DisplayNameText = this.BlockDefinition.DisplayNameText;
                }
            }
        }

        public bool IsFunctional =>
            this.SlimBlock.ComponentStack.IsFunctional;

        public bool IsBuilt =>
            this.SlimBlock.ComponentStack.IsBuilt;

        public virtual float DisassembleRatio =>
            this.BlockDefinition.DisassembleRatio;

        public bool IsWorking { get; private set; }

        public MyIDModule IDModule =>
            this.m_IDModule;

        public bool IsSubBlock =>
            (this.SubBlockName != null);

        public string SubBlockName { get; internal set; }

        public MySlimBlock OwnerBlock { get; internal set; }

        public string DefinitionDisplayNameText =>
            this.BlockDefinition.DisplayNameText;

        public bool ForceBlockDestructible =>
            (MyFakes.ENABLE_VR_FORCE_BLOCK_DESTRUCTIBLE && this.m_forceBlockDestructible);

        protected bool HasDamageEffect =>
            (this.m_damageEffect != null);

        public Dictionary<string, float> UpgradeValues
        {
            get
            {
                if (this.m_upgradeValues == null)
                {
                    this.m_upgradeValues = new Dictionary<string, float>();
                }
                return this.m_upgradeValues;
            }
        }

        SerializableDefinitionId VRage.Game.ModAPI.Ingame.IMyCubeBlock.BlockDefinition =>
            ((SerializableDefinitionId) this.BlockDefinition.Id);

        VRage.Game.ModAPI.IMyCubeGrid VRage.Game.ModAPI.IMyCubeBlock.CubeGrid =>
            this.CubeGrid;

        VRage.Game.ModAPI.Ingame.IMyCubeGrid VRage.Game.ModAPI.Ingame.IMyCubeBlock.CubeGrid =>
            this.CubeGrid;

        bool VRage.Game.ModAPI.IMyCubeBlock.CheckConnectionAllowed
        {
            get => 
                this.CheckConnectionAllowed;
            set => 
                (this.CheckConnectionAllowed = value);
        }

        float VRage.Game.ModAPI.Ingame.IMyCubeBlock.Mass =>
            this.GetMass();

        VRage.Game.ModAPI.IMySlimBlock VRage.Game.ModAPI.IMyCubeBlock.SlimBlock =>
            this.SlimBlock;

        uint Sandbox.ModAPI.Ingame.IMyUpgradableBlock.UpgradeCount =>
            ((uint) this.UpgradeValues.Count);

        MyResourceSinkComponentBase VRage.Game.ModAPI.IMyCubeBlock.ResourceSink
        {
            get => 
                this.ResourceSink;
            set => 
                (this.ResourceSink = (MyResourceSinkComponent) value);
        }

        public class AttachedUpgradeModule
        {
            public Sandbox.ModAPI.IMyUpgradeModule Block;
            public int SlotCount;
            public bool Compatible;

            public AttachedUpgradeModule(Sandbox.ModAPI.IMyUpgradeModule block)
            {
                this.SlotCount = 1;
                this.Compatible = true;
                this.Block = block;
            }

            public AttachedUpgradeModule(Sandbox.ModAPI.IMyUpgradeModule block, int slotCount, bool compatible)
            {
                this.SlotCount = 1;
                this.Compatible = true;
                this.Block = block;
                this.SlotCount = slotCount;
                this.Compatible = compatible;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EmissiveNames
        {
            public MyStringHash Working;
            public MyStringHash Disabled;
            public MyStringHash Warning;
            public MyStringHash Damaged;
            public MyStringHash Alternative;
            public MyStringHash Locked;
            public MyStringHash Autolock;
            public MyStringHash Constraint;
            public EmissiveNames(bool ignore)
            {
                this.Working = MyStringHash.GetOrCompute("Working");
                this.Disabled = MyStringHash.GetOrCompute("Disabled");
                this.Damaged = MyStringHash.GetOrCompute("Damaged");
                this.Alternative = MyStringHash.GetOrCompute("Alternative");
                this.Locked = MyStringHash.GetOrCompute("Locked");
                this.Autolock = MyStringHash.GetOrCompute("Autolock");
                this.Warning = MyStringHash.GetOrCompute("Warning");
                this.Constraint = MyStringHash.GetOrCompute("Constraint");
            }
        }

        private class MethodDataIsConnectedTo
        {
            public List<MyCubeBlockDefinition.MountPoint> MyMountPoints = new List<MyCubeBlockDefinition.MountPoint>();
            public List<MyCubeBlockDefinition.MountPoint> OtherMountPoints = new List<MyCubeBlockDefinition.MountPoint>();

            public void Clear()
            {
                this.MyMountPoints.Clear();
                this.OtherMountPoints.Clear();
            }
        }

        public class MyBlockPosComponent : MyPositionComponent
        {
            protected override void OnWorldPositionChanged(object source, bool updateChildren, bool forceUpdateAllChildren)
            {
                base.OnWorldPositionChanged(source, updateChildren, forceUpdateAllChildren);
                (base.Container.Entity as MyCubeBlock).WorldPositionChanged(source);
            }
        }
    }
}

