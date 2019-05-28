namespace Sandbox.Game
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyDecals : IMyDecalHandler
    {
        private const string DEFAULT = "Default";
        [ThreadStatic]
        private static MyCubeGrid.MyCubeGridHitInfo m_gridHitInfo;
        private static readonly MyDecals m_handler = new MyDecals();

        private MyDecals()
        {
        }

        public static void HandleAddDecal(IMyEntity entity, MyHitInfo hitInfo, MyStringHash material = new MyStringHash(), MyStringHash source = new MyStringHash(), object customdata = null, float damage = -1f)
        {
            IMyDecalProxy proxy = entity as IMyDecalProxy;
            if (proxy != null)
            {
                proxy.AddDecals(ref hitInfo, source, customdata, m_handler, material);
            }
            else
            {
                MyCubeGrid cubeGrid = entity as MyCubeGrid;
                MyCubeBlock block = entity as MyCubeBlock;
                MySlimBlock slimBlock = null;
                if (block != null)
                {
                    cubeGrid = block.CubeGrid;
                    slimBlock = block.SlimBlock;
                }
                else if (cubeGrid != null)
                {
                    slimBlock = cubeGrid.GetTargetedBlock(hitInfo.Position - (0.001f * hitInfo.Normal));
                }
                if (cubeGrid != null)
                {
                    if ((slimBlock != null) && !slimBlock.BlockDefinition.PlaceDecals)
                    {
                        return;
                    }
                    MyCubeGrid.MyCubeGridHitInfo info = customdata as MyCubeGrid.MyCubeGridHitInfo;
                    if (info != null)
                    {
                        MyCube cube;
                        if (!cubeGrid.TryGetCube(info.Position, out cube))
                        {
                            return;
                        }
                        slimBlock = cube.CubeBlock;
                    }
                    else
                    {
                        if (slimBlock == null)
                        {
                            return;
                        }
                        if (m_gridHitInfo == null)
                        {
                            m_gridHitInfo = new MyCubeGrid.MyCubeGridHitInfo();
                        }
                        m_gridHitInfo.Position = slimBlock.Position;
                        customdata = m_gridHitInfo;
                    }
                    MyCompoundCubeBlock block3 = (slimBlock != null) ? (slimBlock.FatBlock as MyCompoundCubeBlock) : null;
                    proxy = (block3 != null) ? ((IMyDecalProxy) block3) : ((IMyDecalProxy) slimBlock);
                }
                if (proxy != null)
                {
                    proxy.AddDecals(ref hitInfo, source, customdata, m_handler, material);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveDecal(uint decalId)
        {
            MyRenderProxy.RemoveDecal(decalId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateDecals(List<MyDecalPositionUpdate> decals)
        {
            MyRenderProxy.UpdateDecals(decals);
        }

        unsafe void IMyDecalHandler.AddDecal(ref MyDecalRenderInfo data, List<uint> ids)
        {
            if (data.RenderObjectIds != null)
            {
                IReadOnlyList<MyDecalMaterial> list;
                MyDecalBindingInfo info;
                Matrix matrix;
                Vector3 position;
                bool flag = MyDecalMaterials.TryGetDecalMaterial(data.Source.String, data.Material.String, out list);
                if (!flag)
                {
                    if (MyFakes.ENABLE_USE_DEFAULT_DAMAGE_DECAL)
                    {
                        flag = MyDecalMaterials.TryGetDecalMaterial("Default", "Default", out list);
                    }
                    if (!flag)
                    {
                        return;
                    }
                }
                if (data.Binding != null)
                {
                    info = data.Binding.Value;
                }
                else
                {
                    info = new MyDecalBindingInfo {
                        Position = (Vector3) data.Position,
                        Normal = data.Normal,
                        Transformation = Matrix.Identity
                    };
                }
                int matIndex = (int) Math.Round((double) (MyRandom.Instance.NextFloat() * (list.Count - 1)));
                MyDecalMaterial material = list[matIndex];
                float rotation = material.Rotation;
                if (float.IsPositiveInfinity(material.Rotation))
                {
                    rotation = MyRandom.Instance.NextFloat() * 6.283185f;
                }
                Vector3 vectorPart = Vector3.CalculatePerpendicularVector(info.Normal);
                if (rotation != 0f)
                {
                    Quaternion quaternion = Quaternion.CreateFromAxisAngle(info.Normal, rotation);
                    Vector3* vectorPtr1 = (Vector3*) ref vectorPart;
                    vectorPtr1 = (Vector3*) new Vector3((new Quaternion(vectorPart, 0f) * quaternion).ToVector4());
                }
                vectorPart = Vector3.Normalize(vectorPart);
                float minSize = material.MinSize;
                if (material.MaxSize > material.MinSize)
                {
                    minSize += MyRandom.Instance.NextFloat() * (material.MaxSize - material.MinSize);
                }
                float depth = material.Depth;
                Vector3 scales = new Vector3(minSize, minSize, depth);
                MyDecalTopoData data2 = new MyDecalTopoData();
                if (data.Flags.HasFlag(MyDecalFlags.World))
                {
                    matrix = Matrix.CreateWorld(Vector3.Zero, info.Normal, vectorPart);
                    position = (Vector3) data.Position;
                }
                else
                {
                    matrix = Matrix.CreateWorld(info.Position - ((info.Normal * depth) * 0.45f), info.Normal, vectorPart);
                    position = Vector3.Invalid;
                }
                data2.MatrixBinding = Matrix.CreateScale(scales) * matrix;
                data2.WorldPosition = position;
                MyDecalTopoData* dataPtr1 = (MyDecalTopoData*) ref data2;
                dataPtr1->MatrixCurrent = info.Transformation * data2.MatrixBinding;
                data2.BoneIndices = data.BoneIndices;
                data2.BoneWeights = data.BoneWeights;
                MyDecalFlags flags = material.Transparent ? MyDecalFlags.Transparent : MyDecalFlags.None;
                string stringId = MyDecalMaterials.GetStringId(data.Source, data.Material);
                uint item = MyRenderProxy.CreateDecal((uint[]) data.RenderObjectIds.Clone(), ref data2, data.Flags | flags, stringId, material.StringId, matIndex);
                if (ids != null)
                {
                    ids.Add(item);
                }
            }
        }
    }
}

