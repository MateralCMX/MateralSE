namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Debris;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_ConveyorPacket), true)]
    public class MyConveyorPacket : MyEntity
    {
        public MyPhysicalInventoryItem Item;
        public int LinePosition;
        private float m_segmentLength;
        private Base6Directions.Direction m_segmentDirection;

        public void Init(MyObjectBuilder_ConveyorPacket builder, MyEntity parent)
        {
            this.Item = new MyPhysicalInventoryItem(builder.Item);
            this.LinePosition = builder.LinePosition;
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(this.Item.Content);
            MyObjectBuilder_Ore content = this.Item.Content as MyObjectBuilder_Ore;
            string model = physicalItemDefinition.Model;
            float num = 1f;
            if (content != null)
            {
                using (Dictionary<string, MyVoxelMaterialDefinition>.ValueCollection.Enumerator enumerator = MyDefinitionManager.Static.GetVoxelMaterialDefinitions().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.MinedOre == content.SubtypeName)
                        {
                            model = MyDebris.GetRandomDebrisVoxel();
                            num = (float) Math.Pow((double) ((((float) this.Item.Amount) * physicalItemDefinition.Volume) / MyDebris.VoxelDebrisModelVolume), 0.33300000429153442);
                            break;
                        }
                    }
                }
            }
            if (num < 0.05f)
            {
                num = 0.05f;
            }
            else if (num > 1f)
            {
                num = 1f;
            }
            MyEntityIdentifier.AllocationSuspended = false;
            float? scale = null;
            this.Init(null, model, parent, scale, null);
            MyEntityIdentifier.AllocationSuspended = MyEntityIdentifier.AllocationSuspended;
            base.PositionComp.Scale = new float?(num);
            base.Save = false;
        }

        public unsafe void MoveRelative(float linePositionFraction)
        {
            base.PrepareForDraw();
            Matrix localMatrix = base.PositionComp.LocalMatrix;
            Matrix* matrixPtr1 = (Matrix*) ref localMatrix;
            matrixPtr1.Translation += ((base.PositionComp.LocalMatrix.GetDirectionVector(this.m_segmentDirection) * this.m_segmentLength) * linePositionFraction) / base.PositionComp.Scale.Value;
            base.PositionComp.LocalMatrix = localMatrix;
        }

        public void SetLocalPosition(Vector3I sectionStart, int sectionStartPosition, float cubeSize, Base6Directions.Direction forward, Base6Directions.Direction offset)
        {
            int num = this.LinePosition - sectionStartPosition;
            Matrix localMatrix = base.PositionComp.LocalMatrix;
            Vector3 vector = (base.PositionComp.LocalMatrix.GetDirectionVector(forward) * num) + (base.PositionComp.LocalMatrix.GetDirectionVector(offset) * 0.1f);
            localMatrix.Translation = (sectionStart + (vector / base.PositionComp.Scale.Value)) * cubeSize;
            base.PositionComp.LocalMatrix = localMatrix;
            this.m_segmentDirection = forward;
        }

        public void SetSegmentLength(float length)
        {
            this.m_segmentLength = length;
        }
    }
}

