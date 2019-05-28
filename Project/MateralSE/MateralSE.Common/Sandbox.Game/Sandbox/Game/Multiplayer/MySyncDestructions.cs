namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner, PreloadRequired]
    public static class MySyncDestructions
    {
        public static void AddDestructionEffect(string effectName, Vector3D position, Vector3 direction, float scale)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<string, Vector3D, Vector3, float>(s => new Action<string, Vector3D, Vector3, float>(MySyncDestructions.OnAddDestructionEffectMessage), effectName, position, direction, scale, targetEndpoint, nullable);
        }

        private static void AddFractureComponent(MyObjectBuilder_FractureComponentBase obFractureComponent, VRage.Game.Entity.MyEntity entity)
        {
            MyFractureComponentBase component = MyComponentFactory.CreateInstanceByTypeId(obFractureComponent.TypeId) as MyFractureComponentBase;
            if (component != null)
            {
                try
                {
                    if (!entity.Components.Has<MyFractureComponentBase>())
                    {
                        entity.Components.Add<MyFractureComponentBase>(component);
                        component.Deserialize(obFractureComponent);
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine("Cannot add received fracture component: " + exception.Message);
                    if (entity.Components.Has<MyFractureComponentBase>())
                    {
                        MyCubeBlock block = entity as MyCubeBlock;
                        if ((block == null) || (block.SlimBlock == null))
                        {
                            entity.Components.Remove<MyFractureComponentBase>();
                        }
                        else
                        {
                            block.SlimBlock.RemoveFractureComponent();
                        }
                    }
                    StringBuilder builder = new StringBuilder();
                    foreach (MyObjectBuilder_FractureComponentBase.FracturedShape shape in obFractureComponent.Shapes)
                    {
                        builder.Append(shape.Name).Append(" ");
                    }
                    MyLog.Default.WriteLine("Received fracture component not added, no shape found. Shapes: " + builder.ToString());
                }
            }
        }

        public static void CreateFractureComponent(long gridId, Vector3I position, ushort compoundBlockId, MyObjectBuilder_FractureComponentBase component)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3I, ushort, MyObjectBuilder_FractureComponentBase>(s => new Action<long, Vector3I, ushort, MyObjectBuilder_FractureComponentBase>(MySyncDestructions.OnCreateFractureComponentMessage), gridId, position, compoundBlockId, component, targetEndpoint, nullable);
        }

        public static void CreateFracturedBlock(MyObjectBuilder_FracturedBlock fracturedBlock, long gridId, Vector3I position)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3I, MyObjectBuilder_FracturedBlock>(s => new Action<long, Vector3I, MyObjectBuilder_FracturedBlock>(MySyncDestructions.OnCreateFracturedBlockMessage), gridId, position, fracturedBlock, targetEndpoint, nullable);
        }

        public static void CreateFracturePiece(MyObjectBuilder_FracturedPiece fracturePiece)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyObjectBuilder_FracturedPiece>(s => new Action<MyObjectBuilder_FracturedPiece>(MySyncDestructions.OnCreateFracturePieceMessage), fracturePiece, targetEndpoint, position);
        }

        [Conditional("DEBUG")]
        public static void FPManagerDbgMessage(long createdId, long removedId)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, long>(s => new Action<long, long>(MySyncDestructions.OnFPManagerDbgMessage), createdId, removedId, targetEndpoint, position);
        }

        [Event(null, 0x2b), Server, Broadcast]
        private static void OnAddDestructionEffectMessage(string effectName, Vector3D position, Vector3 direction, float scale)
        {
            MyGridPhysics.CreateDestructionEffect(effectName, position, direction, scale);
        }

        [Event(null, 0x93), Reliable, Broadcast]
        private static void OnCreateFractureComponentMessage(long gridId, Vector3I position, ushort compoundBlockId, [Serialize(MyObjectFlags.Dynamic, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_FractureComponentBase component)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(gridId, out entity, false))
            {
                MySlimBlock cubeBlock = (entity as MyCubeGrid).GetCubeBlock(position);
                if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                {
                    MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock == null)
                    {
                        AddFractureComponent(component, cubeBlock.FatBlock);
                    }
                    else
                    {
                        MySlimBlock block = fatBlock.GetBlock(compoundBlockId);
                        if (block != null)
                        {
                            AddFractureComponent(component, block.FatBlock);
                        }
                    }
                }
            }
        }

        [Event(null, 0x7d), Reliable, Broadcast]
        private static void OnCreateFracturedBlockMessage(long gridId, Vector3I position, [Serialize(MyObjectFlags.Dynamic, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_FracturedBlock fracturedBlock)
        {
            MyCubeGrid grid;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeGrid>(gridId, out grid, false))
            {
                grid.EnableGenerators(false, true);
                grid.CreateFracturedBlock(fracturedBlock, position);
                grid.EnableGenerators(true, true);
            }
        }

        [Event(null, 0x37), Reliable, Broadcast]
        private static void OnCreateFracturePieceMessage([Serialize(MyObjectFlags.Dynamic, DynamicSerializerType=typeof(MyObjectBuilderDynamicSerializer))] MyObjectBuilder_FracturedPiece fracturePiece)
        {
            MyFracturedPiece pieceFromPool = MyFracturedPiecesManager.Static.GetPieceFromPool(fracturePiece.EntityId, true);
            try
            {
                pieceFromPool.Init(fracturePiece);
                Sandbox.Game.Entities.MyEntities.Add(pieceFromPool, true);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Cannot add fracture piece: " + exception.Message);
                if (pieceFromPool != null)
                {
                    MyFracturedPiecesManager.Static.RemoveFracturePiece(pieceFromPool, 0f, true, false);
                    StringBuilder builder = new StringBuilder();
                    foreach (MyObjectBuilder_FracturedPiece.Shape shape in fracturePiece.Shapes)
                    {
                        builder.Append(shape.Name).Append(" ");
                    }
                    MyLog.Default.WriteLine("Received fracture piece not added, no shape found. Shapes: " + builder.ToString());
                }
            }
        }

        [Event(null, 0x70), Reliable, Server]
        private static void OnFPManagerDbgMessage(long createdId, long removedId)
        {
            MyFracturedPiecesManager.Static.DbgCheck(createdId, removedId);
        }

        [Event(null, 0x128), Reliable, Server]
        private static void OnRemoveFracturedPiecesMessage(ulong userId, Vector3D center, float radius)
        {
            if (MySession.Static.IsUserAdmin(userId))
            {
                MyFracturedPiecesManager.Static.RemoveFracturesInSphere(center, radius);
            }
        }

        [Event(null, 90), Reliable, Broadcast]
        private static void OnRemoveFracturePieceMessage(long entityId, float blendTime)
        {
            MyFracturedPiece piece;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyFracturedPiece>(entityId, out piece, false))
            {
                MyFracturedPiecesManager.Static.RemoveFracturePiece(piece, blendTime, true, false);
            }
        }

        [Event(null, 0xec), Reliable, Broadcast]
        private static void OnRemoveShapeFromFractureComponentMessage(long gridId, Vector3I position, ushort compoundBlockId, string[] shapeNames)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(gridId, out entity, false))
            {
                MySlimBlock cubeBlock = (entity as MyCubeGrid).GetCubeBlock(position);
                if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                {
                    MyCompoundCubeBlock fatBlock = cubeBlock.FatBlock as MyCompoundCubeBlock;
                    if (fatBlock == null)
                    {
                        RemoveFractureComponentChildShapes(cubeBlock, shapeNames);
                    }
                    else
                    {
                        MySlimBlock block = fatBlock.GetBlock(compoundBlockId);
                        if (block != null)
                        {
                            RemoveFractureComponentChildShapes(block, shapeNames);
                        }
                    }
                }
            }
        }

        private static void RemoveFractureComponentChildShapes(MySlimBlock block, string[] shapeNames)
        {
            MyFractureComponentCubeBlock fractureComponent = block.GetFractureComponent();
            if (fractureComponent != null)
            {
                fractureComponent.RemoveChildShapes(shapeNames);
            }
            else
            {
                MyLog.Default.WriteLine("Cannot remove child shapes from fracture component, fracture component not found in block, BlockDefinition: " + block.BlockDefinition.Id.ToString() + ", Shapes: " + string.Join(", ", shapeNames));
            }
        }

        public static void RemoveFracturedPiecesRequest(ulong userId, Vector3D center, float radius)
        {
            if (Sync.IsServer)
            {
                MyFracturedPiecesManager.Static.RemoveFracturesInSphere(center, radius);
            }
            else
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, Vector3D, float>(s => new Action<ulong, Vector3D, float>(MySyncDestructions.OnRemoveFracturedPiecesMessage), userId, center, radius, targetEndpoint, position);
            }
        }

        public static void RemoveFracturePiece(long entityId, float blendTime)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, float>(s => new Action<long, float>(MySyncDestructions.OnRemoveFracturePieceMessage), entityId, blendTime, targetEndpoint, position);
        }

        public static void RemoveShapeFromFractureComponent(long gridId, Vector3I position, ushort compoundBlockId, string shapeName)
        {
            string[] textArray1 = new string[] { shapeName };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3I, ushort, string[]>(s => new Action<long, Vector3I, ushort, string[]>(MySyncDestructions.OnRemoveShapeFromFractureComponentMessage), gridId, position, compoundBlockId, textArray1, targetEndpoint, nullable);
        }

        public static void RemoveShapesFromFractureComponent(long gridId, Vector3I position, ushort compoundBlockId, List<string> shapeNames)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? nullable = null;
            MyMultiplayer.RaiseStaticEvent<long, Vector3I, ushort, string[]>(s => new Action<long, Vector3I, ushort, string[]>(MySyncDestructions.OnRemoveShapeFromFractureComponentMessage), gridId, position, compoundBlockId, shapeNames.ToArray(), targetEndpoint, nullable);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySyncDestructions.<>c <>9 = new MySyncDestructions.<>c();
            public static Func<IMyEventOwner, Action<string, Vector3D, Vector3, float>> <>9__0_0;
            public static Func<IMyEventOwner, Action<MyObjectBuilder_FracturedPiece>> <>9__2_0;
            public static Func<IMyEventOwner, Action<long, float>> <>9__4_0;
            public static Func<IMyEventOwner, Action<long, long>> <>9__6_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, MyObjectBuilder_FracturedBlock>> <>9__8_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, ushort, MyObjectBuilder_FractureComponentBase>> <>9__10_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, ushort, string[]>> <>9__13_0;
            public static Func<IMyEventOwner, Action<long, Vector3I, ushort, string[]>> <>9__14_0;
            public static Func<IMyEventOwner, Action<ulong, Vector3D, float>> <>9__17_0;

            internal Action<string, Vector3D, Vector3, float> <AddDestructionEffect>b__0_0(IMyEventOwner s) => 
                new Action<string, Vector3D, Vector3, float>(MySyncDestructions.OnAddDestructionEffectMessage);

            internal Action<long, Vector3I, ushort, MyObjectBuilder_FractureComponentBase> <CreateFractureComponent>b__10_0(IMyEventOwner s) => 
                new Action<long, Vector3I, ushort, MyObjectBuilder_FractureComponentBase>(MySyncDestructions.OnCreateFractureComponentMessage);

            internal Action<long, Vector3I, MyObjectBuilder_FracturedBlock> <CreateFracturedBlock>b__8_0(IMyEventOwner s) => 
                new Action<long, Vector3I, MyObjectBuilder_FracturedBlock>(MySyncDestructions.OnCreateFracturedBlockMessage);

            internal Action<MyObjectBuilder_FracturedPiece> <CreateFracturePiece>b__2_0(IMyEventOwner s) => 
                new Action<MyObjectBuilder_FracturedPiece>(MySyncDestructions.OnCreateFracturePieceMessage);

            internal Action<long, long> <FPManagerDbgMessage>b__6_0(IMyEventOwner s) => 
                new Action<long, long>(MySyncDestructions.OnFPManagerDbgMessage);

            internal Action<ulong, Vector3D, float> <RemoveFracturedPiecesRequest>b__17_0(IMyEventOwner s) => 
                new Action<ulong, Vector3D, float>(MySyncDestructions.OnRemoveFracturedPiecesMessage);

            internal Action<long, float> <RemoveFracturePiece>b__4_0(IMyEventOwner s) => 
                new Action<long, float>(MySyncDestructions.OnRemoveFracturePieceMessage);

            internal Action<long, Vector3I, ushort, string[]> <RemoveShapeFromFractureComponent>b__13_0(IMyEventOwner s) => 
                new Action<long, Vector3I, ushort, string[]>(MySyncDestructions.OnRemoveShapeFromFractureComponentMessage);

            internal Action<long, Vector3I, ushort, string[]> <RemoveShapesFromFractureComponent>b__14_0(IMyEventOwner s) => 
                new Action<long, Vector3I, ushort, string[]>(MySyncDestructions.OnRemoveShapeFromFractureComponentMessage);
        }
    }
}

