namespace Sandbox.Definitions
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_SpawnGroupDefinition), (Type) null)]
    public class MySpawnGroupDefinition : MyDefinitionBase
    {
        public float Frequency;
        private float m_spawnRadius;
        private bool m_initialized;
        public bool IsPirate;
        public bool IsEncounter;
        public bool IsCargoShip;
        public bool ReactorsOn;
        public List<SpawnGroupPrefab> Prefabs = new List<SpawnGroupPrefab>();
        public List<SpawnGroupVoxel> Voxels = new List<SpawnGroupVoxel>();

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_SpawnGroupDefinition objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_SpawnGroupDefinition;
            objectBuilder.Frequency = this.Frequency;
            objectBuilder.Prefabs = new MyObjectBuilder_SpawnGroupDefinition.SpawnGroupPrefab[this.Prefabs.Count];
            int index = 0;
            foreach (SpawnGroupPrefab prefab in this.Prefabs)
            {
                objectBuilder.Prefabs[index] = new MyObjectBuilder_SpawnGroupDefinition.SpawnGroupPrefab();
                objectBuilder.Prefabs[index].BeaconText = prefab.BeaconText;
                objectBuilder.Prefabs[index].SubtypeId = prefab.SubtypeId;
                objectBuilder.Prefabs[index].Position = prefab.Position;
                objectBuilder.Prefabs[index].Speed = prefab.Speed;
                objectBuilder.Prefabs[index].ResetOwnership = prefab.ResetOwnership;
                objectBuilder.Prefabs[index].PlaceToGridOrigin = prefab.PlaceToGridOrigin;
                objectBuilder.Prefabs[index].Behaviour = prefab.Behaviour;
                objectBuilder.Prefabs[index].BehaviourActivationDistance = prefab.BehaviourActivationDistance;
                index++;
            }
            objectBuilder.Voxels = new MyObjectBuilder_SpawnGroupDefinition.SpawnGroupVoxel[this.Voxels.Count];
            index = 0;
            foreach (SpawnGroupVoxel voxel in this.Voxels)
            {
                objectBuilder.Voxels[index] = new MyObjectBuilder_SpawnGroupDefinition.SpawnGroupVoxel();
                objectBuilder.Voxels[index].Offset = voxel.Offset;
                objectBuilder.Voxels[index].CenterOffset = voxel.CenterOffset;
                objectBuilder.Voxels[index].StorageName = voxel.StorageName;
                index++;
            }
            objectBuilder.IsCargoShip = this.IsCargoShip;
            objectBuilder.IsEncounter = this.IsEncounter;
            objectBuilder.IsPirate = this.IsPirate;
            objectBuilder.ReactorsOn = this.ReactorsOn;
            return objectBuilder;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase baseBuilder)
        {
            base.Init(baseBuilder);
            MyObjectBuilder_SpawnGroupDefinition definition = baseBuilder as MyObjectBuilder_SpawnGroupDefinition;
            this.Frequency = definition.Frequency;
            if (this.Frequency == 0f)
            {
                MySandboxGame.Log.WriteLine("Spawn group initialization: spawn group has zero frequency");
            }
            else
            {
                this.SpawnRadius = 0f;
                BoundingSphere sphere = new BoundingSphere(Vector3.Zero, float.MinValue);
                this.Prefabs.Clear();
                foreach (MyObjectBuilder_SpawnGroupDefinition.SpawnGroupPrefab prefab in definition.Prefabs)
                {
                    SpawnGroupPrefab item = new SpawnGroupPrefab {
                        Position = prefab.Position,
                        SubtypeId = prefab.SubtypeId,
                        BeaconText = prefab.BeaconText,
                        Speed = prefab.Speed,
                        ResetOwnership = prefab.ResetOwnership,
                        PlaceToGridOrigin = prefab.PlaceToGridOrigin,
                        Behaviour = prefab.Behaviour,
                        BehaviourActivationDistance = prefab.BehaviourActivationDistance
                    };
                    if (MyDefinitionManager.Static.GetPrefabDefinition(item.SubtypeId) == null)
                    {
                        MySandboxGame.Log.WriteLine("Spawn group initialization: Could not get prefab " + item.SubtypeId);
                        return;
                    }
                    this.Prefabs.Add(item);
                }
                this.Voxels.Clear();
                if (definition.Voxels != null)
                {
                    foreach (MyObjectBuilder_SpawnGroupDefinition.SpawnGroupVoxel voxel in definition.Voxels)
                    {
                        SpawnGroupVoxel item = new SpawnGroupVoxel {
                            Offset = voxel.Offset,
                            StorageName = voxel.StorageName,
                            CenterOffset = voxel.CenterOffset
                        };
                        this.Voxels.Add(item);
                    }
                }
                this.SpawnRadius = sphere.Radius + 5f;
                this.IsEncounter = definition.IsEncounter;
                this.IsCargoShip = definition.IsCargoShip;
                this.IsPirate = definition.IsPirate;
                this.ReactorsOn = definition.ReactorsOn;
            }
        }

        public unsafe void ReloadPrefabs()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, float.MinValue);
            float num = 0f;
            using (List<SpawnGroupPrefab>.Enumerator enumerator = this.Prefabs.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    SpawnGroupPrefab current = enumerator.Current;
                    MyPrefabDefinition prefabDefinition = MyDefinitionManager.Static.GetPrefabDefinition(current.SubtypeId);
                    if (prefabDefinition != null)
                    {
                        BoundingSphere boundingSphere = prefabDefinition.BoundingSphere;
                        Vector3* vectorPtr1 = (Vector3*) ref boundingSphere.Center;
                        vectorPtr1[0] += current.Position;
                        sphere.Include(boundingSphere);
                        if (prefabDefinition.CubeGrids == null)
                        {
                            continue;
                        }
                        foreach (MyObjectBuilder_CubeGrid grid in prefabDefinition.CubeGrids)
                        {
                            float cubeSize = MyDefinitionManager.Static.GetCubeSize(grid.GridSizeEnum);
                            num = Math.Max(num, 2f * cubeSize);
                        }
                        continue;
                    }
                    MySandboxGame.Log.WriteLine("Spawn group initialization: Could not get prefab " + current.SubtypeId);
                    return;
                }
            }
            this.SpawnRadius = sphere.Radius + num;
            this.m_initialized = true;
        }

        public float SpawnRadius
        {
            get
            {
                if (!this.m_initialized)
                {
                    this.ReloadPrefabs();
                }
                return this.m_spawnRadius;
            }
            private set => 
                (this.m_spawnRadius = value);
        }

        public bool IsValid =>
            ((this.Frequency != 0f) && ((this.m_spawnRadius != 0f) && (this.Prefabs.Count != 0)));

        [StructLayout(LayoutKind.Sequential)]
        public struct SpawnGroupPrefab
        {
            public Vector3 Position;
            public string SubtypeId;
            public string BeaconText;
            public float Speed;
            public bool ResetOwnership;
            public bool PlaceToGridOrigin;
            public string Behaviour;
            public float BehaviourActivationDistance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpawnGroupVoxel
        {
            public Vector3 Offset;
            public bool CenterOffset;
            public string StorageName;
        }
    }
}

