namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRageMath;

    public abstract class MyAttachableTopBlockBase : MySyncedBlock, Sandbox.ModAPI.IMyAttachableTopBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyAttachableTopBlock
    {
        private long? m_parentId;
        private MyMechanicalConnectionBlockBase m_parentBlock;

        protected MyAttachableTopBlockBase()
        {
        }

        public virtual void Attach(MyMechanicalConnectionBlockBase parent)
        {
            this.m_parentBlock = parent;
        }

        public virtual void Detach(bool isWelding)
        {
            if (!isWelding)
            {
                this.m_parentBlock = null;
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_CubeBlock objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy);
            MyObjectBuilder_AttachableTopBlockBase base2 = objectBuilderCubeBlock as MyObjectBuilder_AttachableTopBlockBase;
            if (base2 != null)
            {
                base2.ParentEntityId = (this.m_parentBlock != null) ? this.m_parentBlock.EntityId : 0L;
                base2.YieldLastComponent = base.SlimBlock.YieldLastComponent;
            }
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.Init(builder, cubeGrid);
            MyObjectBuilder_AttachableTopBlockBase base2 = builder as MyObjectBuilder_AttachableTopBlockBase;
            if (base2 != null)
            {
                if (!base2.YieldLastComponent)
                {
                    base.SlimBlock.DisableLastComponentYield();
                }
                if (base2.ParentEntityId != 0)
                {
                    VRage.Game.Entity.MyEntity entity;
                    Sandbox.Game.Entities.MyEntities.TryGetEntityById(base2.ParentEntityId, out entity, false);
                    MyMechanicalConnectionBlockBase base3 = entity as MyMechanicalConnectionBlockBase;
                    if (base3 != null)
                    {
                        base3.MarkForReattach();
                    }
                }
            }
            this.LoadDummies();
        }

        private void LoadDummies()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("wheel"))
                {
                    Matrix matrix = Matrix.Normalize(pair.Value.Matrix) * base.PositionComp.LocalMatrix;
                    this.WheelDummy = matrix.Translation;
                    break;
                }
            }
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            if (this.Stator != null)
            {
                this.Stator.OnTopBlockCubeGridChanged(oldGrid);
            }
            base.OnCubeGridChanged(oldGrid);
        }

        public Vector3 WheelDummy { get; private set; }

        public MyMechanicalConnectionBlockBase Stator =>
            this.m_parentBlock;

        Sandbox.ModAPI.IMyMechanicalConnectionBlock Sandbox.ModAPI.IMyAttachableTopBlock.Base =>
            this.Stator;

        bool Sandbox.ModAPI.Ingame.IMyAttachableTopBlock.IsAttached =>
            (this.Stator != null);

        Sandbox.ModAPI.Ingame.IMyMechanicalConnectionBlock Sandbox.ModAPI.Ingame.IMyAttachableTopBlock.Base =>
            this.Stator;
    }
}

