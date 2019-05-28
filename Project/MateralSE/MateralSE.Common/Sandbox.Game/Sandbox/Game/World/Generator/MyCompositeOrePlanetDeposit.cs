namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRageMath;

    internal class MyCompositeOrePlanetDeposit : MyCompositeShapeOreDeposit
    {
        private float m_minDepth;
        private const float DEPOSIT_MAX_SIZE = 1000f;
        private int m_numDeposits;
        private Dictionary<Vector3I, MyCompositeShapeOreDeposit> m_deposits;
        private Dictionary<string, List<MyVoxelMaterialDefinition>> m_materialsByOreType;

        public MyCompositeOrePlanetDeposit(MyCsgShapeBase baseShape, int seed, float minDepth, float maxDepth, MyOreProbability[] oreProbabilties, MyVoxelMaterialDefinition material) : base(baseShape, material)
        {
            this.m_deposits = new Dictionary<Vector3I, MyCompositeShapeOreDeposit>();
            this.m_materialsByOreType = new Dictionary<string, List<MyVoxelMaterialDefinition>>();
            this.m_minDepth = minDepth;
            double num2 = (12.566371917724609 * Math.Pow(1000.0, 3.0)) / 3.0;
            double num3 = ((12.566371917724609 * Math.Pow((double) minDepth, 3.0)) / 3.0) - ((12.566371917724609 * Math.Pow((double) maxDepth, 3.0)) / 3.0);
            this.m_numDeposits = (oreProbabilties.Length != 0) ? ((int) Math.Floor((double) ((num3 * 0.40000000596046448) / num2))) : 0;
            float single1 = minDepth / 1000f;
            MyRandom instance = MyRandom.Instance;
            this.FillMaterialCollections();
            Vector3D vectord = -new Vector3D(500.0);
            using (instance.PushSeed(seed))
            {
                for (int i = 0; i < this.m_numDeposits; i++)
                {
                    MyCompositeShapeOreDeposit deposit;
                    float num5 = instance.NextFloat(maxDepth, minDepth);
                    Vector3D vectord2 = MyProceduralWorldGenerator.GetRandomDirection(instance) * num5;
                    Vector3I key = Vector3I.Ceiling((base.Shape.Center() + vectord2) / 1000.0);
                    if (!this.m_deposits.TryGetValue(key, out deposit))
                    {
                        MyOreProbability ore = this.GetOre(instance.NextFloat(0f, 1f), oreProbabilties);
                        MyVoxelMaterialDefinition definition = this.m_materialsByOreType[ore.OreName][instance.Next() % this.m_materialsByOreType[ore.OreName].Count];
                        deposit = new MyCompositeShapeOreDeposit(new MyCsgSimpleSphere((key * 1000f) + vectord, instance.NextFloat(64f, 500f)), definition);
                        this.m_deposits[key] = deposit;
                    }
                }
            }
            this.m_materialsByOreType.Clear();
        }

        public override void DebugDraw(ref MatrixD translation, Color materialColor)
        {
            foreach (KeyValuePair<Vector3I, MyCompositeShapeOreDeposit> pair in this.m_deposits)
            {
                pair.Value.DebugDraw(ref translation, materialColor);
            }
        }

        private void FillMaterialCollections()
        {
            foreach (MyVoxelMaterialDefinition definition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (definition.MinedOre != "Organic")
                {
                    List<MyVoxelMaterialDefinition> list;
                    if (!this.m_materialsByOreType.TryGetValue(definition.MinedOre, out list))
                    {
                        list = new List<MyVoxelMaterialDefinition>();
                    }
                    list.Add(definition);
                    this.m_materialsByOreType[definition.MinedOre] = list;
                }
            }
        }

        public override MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize)
        {
            MyCompositeShapeOreDeposit deposit;
            Vector3I key = Vector3I.Ceiling(pos / 1000f);
            if (!this.m_deposits.TryGetValue(key, out deposit) || (deposit.Shape.SignedDistance(ref pos, lodSize, null, null) != -1f))
            {
                return null;
            }
            return deposit.GetMaterialForPosition(ref pos, lodSize);
        }

        private MyOreProbability GetOre(float probability, MyOreProbability[] probalities)
        {
            foreach (MyOreProbability probability2 in probalities)
            {
                if (probability2.CummulativeProbability >= probability)
                {
                    return probability2;
                }
            }
            return null;
        }

        public float MinDepth =>
            this.m_minDepth;
    }
}

