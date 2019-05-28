namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyPlanetEnvironmentMapping
    {
        public MyMaterialEnvironmentItem[] Items;
        public MyPlanetSurfaceRule Rule;
        private float[] CumulativeIntervals;
        public float TotalFrequency;

        public MyPlanetEnvironmentMapping(PlanetEnvironmentItemMapping map)
        {
            this.Rule = map.Rule;
            this.Items = new MyMaterialEnvironmentItem[map.Items.Length];
            if (this.Items.Length == 0)
            {
                this.CumulativeIntervals = null;
                this.TotalFrequency = 0f;
            }
            else
            {
                this.TotalFrequency = 0f;
                for (int i = 0; i < map.Items.Length; i++)
                {
                    MyObjectBuilderType type;
                    MyPlanetEnvironmentItemDef def = map.Items[i];
                    if ((def.TypeId == null) || !MyObjectBuilderType.TryParse(def.TypeId, out type))
                    {
                        MyLog.Default.WriteLine($"Object builder type {def.TypeId} does not exist.");
                        this.Items[i].Frequency = 0f;
                    }
                    else if ((!typeof(MyObjectBuilder_BotDefinition).IsAssignableFrom((Type) type) && !typeof(MyObjectBuilder_VoxelMapStorageDefinition).IsAssignableFrom((Type) type)) && !typeof(MyObjectBuilder_EnvironmentItems).IsAssignableFrom((Type) type))
                    {
                        MyLog.Default.WriteLine($"Object builder type {def.TypeId} is not supported for environment items.");
                        this.Items[i].Frequency = 0f;
                    }
                    else
                    {
                        MyMaterialEnvironmentItem item1 = new MyMaterialEnvironmentItem();
                        item1.Definition = new MyDefinitionId(type, def.SubtypeId);
                        item1.Frequency = map.Items[i].Density;
                        item1.IsDetail = map.Items[i].IsDetail;
                        item1.IsBot = typeof(MyObjectBuilder_BotDefinition).IsAssignableFrom((Type) type);
                        item1.IsVoxel = typeof(MyObjectBuilder_VoxelMapStorageDefinition).IsAssignableFrom((Type) type);
                        item1.IsEnvironemntItem = typeof(MyObjectBuilder_EnvironmentItems).IsAssignableFrom((Type) type);
                        item1.BaseColor = map.Items[i].BaseColor;
                        item1.ColorSpread = map.Items[i].ColorSpread;
                        item1.MaxRoll = (float) Math.Cos((double) MathHelper.ToDegrees(map.Items[i].MaxRoll));
                        item1.Offset = map.Items[i].Offset;
                        item1.GroupId = map.Items[i].GroupId;
                        item1.GroupIndex = map.Items[i].GroupIndex;
                        item1.ModifierId = map.Items[i].ModifierId;
                        item1.ModifierIndex = map.Items[i].ModifierIndex;
                        this.Items[i] = item1;
                    }
                }
                this.ComputeDistribution();
            }
        }

        public void ComputeDistribution()
        {
            if (!this.Valid)
            {
                this.TotalFrequency = 0f;
                this.CumulativeIntervals = null;
            }
            else
            {
                this.TotalFrequency = 0f;
                for (int i = 0; i < this.Items.Length; i++)
                {
                    this.TotalFrequency += this.Items[i].Frequency;
                }
                this.CumulativeIntervals = new float[this.Items.Length - 1];
                float num = 0f;
                for (int j = 0; j < this.CumulativeIntervals.Length; j++)
                {
                    this.CumulativeIntervals[j] = num + (this.Items[j].Frequency / this.TotalFrequency);
                    num = this.CumulativeIntervals[j];
                }
            }
        }

        public int GetItemRated(float rate) => 
            this.CumulativeIntervals.BinaryIntervalSearch<float>(rate);

        public bool Valid =>
            ((this.Items != null) && (this.Items.Length != 0));
    }
}

