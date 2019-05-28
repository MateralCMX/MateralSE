namespace Sandbox.Game.GameSystems
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGridGasSystem
    {
        private static bool DEBUG_MODE = false;
        public const float OXYGEN_UNIFORMIZATION_TIME_MS = 1500f;
        private readonly Vector3I[] m_neighbours = new Vector3I[] { new Vector3I(1, 0, 0), new Vector3I(-1, 0, 0), new Vector3I(0, 1, 0), new Vector3I(0, -1, 0), new Vector3I(0, 0, 1), new Vector3I(0, 0, -1) };
        private readonly Vector3I[] m_neighboursForDelete = new Vector3I[] { new Vector3I(1, 0, 0), new Vector3I(-1, 0, 0), new Vector3I(0, 1, 0), new Vector3I(0, -1, 0), new Vector3I(0, 0, 1), new Vector3I(0, 0, -1), new Vector3I(0, 0, 0) };
        private readonly IMyCubeGrid m_cubeGrid;
        private static readonly MySoundPair m_airleakSound = new MySoundPair("EventAirVent", true);
        private bool m_isProcessingData;
        private MyOxygenCube m_cubeRoom;
        private MyConcurrentList<MyOxygenRoom> m_rooms;
        private int m_lastRoomIndex;
        private Queue<Vector3I> m_blockQueue = new Queue<Vector3I>();
        private Vector3I m_storedGridMin;
        private Vector3I m_storedGridMax;
        private Vector3I m_previousGridMin;
        private Vector3I m_previousGridMax;
        private OxygenRoom[] m_savedRooms;
        private List<IMySlimBlock> m_gasBlocks = new List<IMySlimBlock>();
        private List<IMySlimBlock> m_gasBlocksForUpdate = new List<IMySlimBlock>();
        private bool m_generatedDataPending;
        private bool m_gridExpanded;
        private bool m_gridShrinked;
        private List<IMySlimBlock> m_deletedBlocks = new List<IMySlimBlock>();
        private List<IMySlimBlock> m_deletedBlocksSwap = new List<IMySlimBlock>();
        private List<IMySlimBlock> m_addedBlocks = new List<IMySlimBlock>();
        private List<IMySlimBlock> m_addedBlocksSwap = new List<IMySlimBlock>();
        private Task m_backgroundTask;
        private int m_lastUpdateTime;
        private bool isClosing;
        private HashSet<Vector3I> m_visitedBlocks = new HashSet<Vector3I>();
        private HashSet<Vector3I> m_initializedBlocks = new HashSet<Vector3I>();
        private readonly float m_debugTextlineSize = 17f;
        private bool m_debugShowTopRoom;
        private bool m_debugShowRoomIndex = true;
        private bool m_debugShowPositions;
        private int m_debugRoomIndex;
        private bool m_debugShowBlockCount;
        private bool m_debugShowOxygenAmount;
        private bool m_debugToggleView;

        public MyGridGasSystem(IMyCubeGrid cubeGrid)
        {
            this.m_cubeGrid = cubeGrid;
            cubeGrid.OnBlockAdded += new Action<IMySlimBlock>(this.cubeGrid_OnBlockAdded);
            cubeGrid.OnBlockRemoved += new Action<IMySlimBlock>(this.cubeGrid_OnBlockRemoved);
            this.m_lastUpdateTime = this.GetTotalGamePlayTimeInMilliseconds();
        }

        private void AddBlock(IMySlimBlock block)
        {
            Vector3I min = block.Min;
            Vector3I start = block.Min;
            Vector3I max = block.Max;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref max);
            while (iterator.IsValid())
            {
                MyOxygenRoom oxygenRoomForCubeGridPosition = this.GetOxygenRoomForCubeGridPosition(ref min);
                if (oxygenRoomForCubeGridPosition != null)
                {
                    int num1;
                    oxygenRoomForCubeGridPosition.IsDirty = true;
                    bool flag = false;
                    Sandbox.ModAPI.IMyDoor fatBlock = block.FatBlock as Sandbox.ModAPI.IMyDoor;
                    if (fatBlock != null)
                    {
                        flag = true;
                        if (fatBlock is MyAirtightSlideDoor)
                        {
                            return;
                        }
                    }
                    MyCubeBlockDefinition blockDefinition = block.BlockDefinition as MyCubeBlockDefinition;
                    bool? nullable = this.IsAirtightFromDefinition(blockDefinition, block.BuildLevelRatio);
                    if (blockDefinition == null)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        bool? nullable2 = nullable;
                        bool flag2 = true;
                        num1 = (int) ((nullable2.GetValueOrDefault() == flag2) & (nullable2 != null));
                    }
                    if ((num1 | flag) != 0)
                    {
                        Vector3I item = min;
                        oxygenRoomForCubeGridPosition.BlockCount--;
                        oxygenRoomForCubeGridPosition.Blocks.Remove(item);
                        this.m_cubeRoom[item.X, item.Y, item.Z].RoomLink = null;
                    }
                }
                iterator.GetNext(out min);
            }
        }

        public static unsafe void AddDepressurizationEffects(MyCubeGrid grid, Vector3I from, Vector3I to)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (from != to))
            {
                MyParticleEffect effect;
                MatrixD xd;
                MatrixD* xdPtr1;
                Vector3D vectord = grid.GridIntegerToWorld(from);
                Vector3I vec = to - from;
                if (vec.IsAxisAligned())
                {
                    xd = CreateAxisAlignedMatrix(ref vec);
                    xdPtr1.Translation = (Vector3D) (from * ((grid.GridSizeEnum == MyCubeSize.Small) ? 0.5f : 2.5f));
                }
                else
                {
                    xdPtr1 = (MatrixD*) ref xd;
                    xd = MatrixD.Normalize(MatrixD.CreateFromDir(vectord - grid.GridIntegerToWorld(to)));
                    xd.Translation = vectord;
                    xd *= grid.PositionComp.WorldMatrixNormalizedInv;
                }
                Vector3D worldPosition = vectord;
                if (MyParticlesManager.TryCreateParticleEffect("OxyLeakLarge", ref xd, ref worldPosition, grid.Render.GetRenderObjectID(), out effect))
                {
                    MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                    if (emitter != null)
                    {
                        emitter.SetPosition(new Vector3D?(vectord));
                        bool? nullable = null;
                        emitter.PlaySound(m_airleakSound, false, false, false, false, false, nullable);
                        if (grid.Physics != null)
                        {
                            emitter.SetVelocity(new Vector3?(grid.Physics.LinearVelocity));
                        }
                    }
                    if (grid.GridSizeEnum == MyCubeSize.Small)
                    {
                        effect.UserScale = 0.2f;
                    }
                }
            }
        }

        private void CheckPositionForEmptyRoom(Vector3I position)
        {
            if (!this.m_initializedBlocks.Contains(position))
            {
                MyOxygenBlock block;
                if (!this.m_cubeRoom.TryGetValue(position, out block))
                {
                    block = new MyOxygenBlock();
                    this.m_cubeRoom.Add(position, block);
                }
                if ((block == null) || (block.Room == null))
                {
                    Vector3I pos = position;
                    IMySlimBlock cubeBlock = this.m_cubeGrid.GetCubeBlock(pos);
                    if (cubeBlock != null)
                    {
                        MyCubeBlockDefinition blockDefinition = cubeBlock.BlockDefinition as MyCubeBlockDefinition;
                        bool? nullable = this.IsAirtightFromDefinition(blockDefinition, cubeBlock.BuildLevelRatio);
                        if (blockDefinition != null)
                        {
                            bool? nullable2 = nullable;
                            bool flag = true;
                            if ((nullable2.GetValueOrDefault() == flag) & (nullable2 != null))
                            {
                                return;
                            }
                        }
                        Sandbox.ModAPI.IMyDoor fatBlock = cubeBlock.FatBlock as Sandbox.ModAPI.IMyDoor;
                        if (((fatBlock != null) && ((fatBlock.Status == DoorStatus.Closed) || (fatBlock.Status == DoorStatus.Closing))) && !(fatBlock is MyAirtightSlideDoor))
                        {
                            return;
                        }
                    }
                    HashSet<Vector3I> roomBlocks = this.GetRoomBlocks(pos, null);
                    if (roomBlocks.Count > 0)
                    {
                        this.CreateAirtightRoom(roomBlocks, 0f, position);
                        this.m_initializedBlocks.UnionWith(roomBlocks);
                    }
                }
            }
        }

        private void Clear()
        {
            this.m_rooms = null;
            this.m_cubeRoom = null;
            this.m_lastRoomIndex = 0;
            this.m_visitedBlocks.Clear();
            this.m_initializedBlocks.Clear();
        }

        private MyOxygenRoom CreateAirtightRoom(HashSet<Vector3I> roomBlocks, float oxygenAmount, Vector3I startingPosition)
        {
            this.m_lastRoomIndex++;
            MyOxygenRoom room = new MyOxygenRoom(this.m_lastRoomIndex) {
                IsAirtight = true,
                OxygenAmount = oxygenAmount,
                EnvironmentOxygen = MyOxygenProviderSystem.GetOxygenInPoint(this.m_cubeGrid.GridIntegerToWorld(startingPosition)),
                DepressurizationTime = this.GetTotalGamePlayTimeInMilliseconds(),
                BlockCount = roomBlocks.Count,
                Blocks = roomBlocks,
                StartingPosition = startingPosition
            };
            float num = room.OxygenLevel(this.m_cubeGrid.GridSize);
            if (room.EnvironmentOxygen > num)
            {
                room.OxygenAmount = room.MaxOxygen(this.m_cubeGrid.GridSize) * room.EnvironmentOxygen;
            }
            this.m_rooms.Add(room);
            MyOxygenRoomLink roomPointer = new MyOxygenRoomLink(room);
            foreach (Vector3I vectori in roomBlocks)
            {
                MyOxygenBlock block = new MyOxygenBlock(roomPointer);
                this.m_cubeRoom.Add(vectori, block);
            }
            return room;
        }

        public static unsafe MatrixD CreateAxisAlignedMatrix(ref Vector3I vec)
        {
            MatrixD zero = MatrixD.Zero;
            if (vec.X != 0)
            {
                if (vec.X > 0)
                {
                    MatrixD* xdPtr1 = (MatrixD*) ref zero;
                    xdPtr1->M31 = zero.M22 = 1.0;
                }
                else
                {
                    MatrixD* xdPtr2 = (MatrixD*) ref zero;
                    xdPtr2->M31 = zero.M22 = -1.0;
                }
                zero.M13 = 1.0;
            }
            else if (vec.Y != 0)
            {
                if (vec.Y > 0)
                {
                    MatrixD* xdPtr3 = (MatrixD*) ref zero;
                    xdPtr3->M32 = zero.M21 = 1.0;
                }
                else
                {
                    MatrixD* xdPtr4 = (MatrixD*) ref zero;
                    xdPtr4->M32 = zero.M21 = -1.0;
                }
                zero.M13 = 1.0;
            }
            else
            {
                if (vec.Z == 0)
                {
                    return MatrixD.Identity;
                }
                if (vec.Z > 0)
                {
                    MatrixD* xdPtr5 = (MatrixD*) ref zero;
                    xdPtr5->M33 = zero.M21 = 1.0;
                }
                else
                {
                    MatrixD* xdPtr6 = (MatrixD*) ref zero;
                    xdPtr6->M33 = zero.M21 = -1.0;
                }
                zero.M12 = 1.0;
            }
            return zero;
        }

        private void cubeGrid_OnBlockAdded(IMySlimBlock addedBlock)
        {
            if (addedBlock.FatBlock is Sandbox.ModAPI.IMyDoor)
            {
                ((Sandbox.ModAPI.IMyDoor) addedBlock.FatBlock).OnDoorStateChanged += new Action<Sandbox.ModAPI.IMyDoor, bool>(this.OnDoorStateChanged);
            }
            IMyGasBlock fatBlock = addedBlock.FatBlock as IMyGasBlock;
            if ((fatBlock != null) && fatBlock.CanPressurizeRoom)
            {
                this.m_gasBlocks.Add(addedBlock);
            }
            if (this.m_gasBlocks.Count != 0)
            {
                this.m_addedBlocks.Add(addedBlock);
                Vector3I vectori = !this.m_isProcessingData ? this.m_storedGridMin : this.m_previousGridMin;
                Vector3I vectori2 = !this.m_isProcessingData ? this.m_storedGridMax : this.m_previousGridMax;
                if ((Vector3I.Min(this.GridMin(), vectori) != vectori) || (Vector3I.Max(this.GridMax(), vectori2) != vectori2))
                {
                    this.m_gridExpanded = true;
                }
                if (this.m_rooms == null)
                {
                    this.m_generatedDataPending = true;
                }
                this.MarkCubeGridForUpdate();
            }
        }

        private void cubeGrid_OnBlockRemoved(IMySlimBlock deletedBlock)
        {
            Sandbox.ModAPI.IMyDoor fatBlock = deletedBlock.FatBlock as Sandbox.ModAPI.IMyDoor;
            if (fatBlock != null)
            {
                fatBlock.OnDoorStateChanged -= new Action<Sandbox.ModAPI.IMyDoor, bool>(this.OnDoorStateChanged);
            }
            IMyGasBlock block = deletedBlock.FatBlock as IMyGasBlock;
            if ((block != null) && block.CanPressurizeRoom)
            {
                this.m_gasBlocks.Remove(deletedBlock);
            }
            if ((this.m_gasBlocks.Count != 0) || (block != null))
            {
                this.m_deletedBlocks.Add(deletedBlock);
                this.MarkCubeGridForUpdate();
            }
        }

        public unsafe void DebugDraw()
        {
            if (!this.m_isProcessingData && (this.m_rooms != null))
            {
                Vector2 zero = Vector2.Zero;
                MyRenderProxy.DebugDrawText2D(zero, "CTRL+ (T Toggle Top Room) (R Toggle Room Index) (Y Toggle Positions) (U Toggle View) ([ Index Down) (] Index Up) (- Index Reset) (+ Index Last)", Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr1 = (float*) ref zero.Y;
                singlePtr1[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(zero, "Rooms Count: " + this.m_rooms.Count, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr2 = (float*) ref zero.Y;
                singlePtr2[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(zero, "Selected Room", Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr3 = (float*) ref zero.Y;
                singlePtr3[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(zero, "   Index: " + this.m_debugRoomIndex, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                if (MyInput.Static.IsLeftCtrlKeyPressed())
                {
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.T))
                    {
                        this.m_debugShowTopRoom = !this.m_debugShowTopRoom;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.R))
                    {
                        this.m_debugShowRoomIndex = !this.m_debugShowRoomIndex;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.Y))
                    {
                        this.m_debugShowPositions = !this.m_debugShowPositions;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.OemOpenBrackets))
                    {
                        this.m_debugRoomIndex = (this.m_debugRoomIndex == 0) ? 0 : (this.m_debugRoomIndex - 1);
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.OemCloseBrackets))
                    {
                        this.m_debugRoomIndex = (this.m_debugRoomIndex >= this.m_lastRoomIndex) ? this.m_lastRoomIndex : (this.m_debugRoomIndex + 1);
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.OemPlus))
                    {
                        this.m_debugRoomIndex = this.m_lastRoomIndex;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.OemMinus))
                    {
                        this.m_debugRoomIndex = 0;
                    }
                    if (MyInput.Static.IsNewKeyPressed(MyKeys.U))
                    {
                        this.m_debugToggleView = !this.m_debugToggleView;
                    }
                }
                if (!this.m_debugToggleView)
                {
                    this.DrawRooms(zero);
                }
                else
                {
                    Vector3I storedGridMin = this.m_storedGridMin;
                    Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref this.m_storedGridMin, ref this.m_storedGridMax);
                    while (iterator.IsValid())
                    {
                        MyOxygenBlock block;
                        if (this.m_cubeRoom.TryGetValue(storedGridMin, out block))
                        {
                            MyOxygenRoom room = block.Room;
                            if ((room != null) && ((room.Index != 0) || this.m_debugShowTopRoom))
                            {
                                this.DrawBlock(room, storedGridMin);
                            }
                        }
                        iterator.GetNext(out storedGridMin);
                    }
                }
            }
        }

        private void DrawBlock(MyOxygenRoom room, Vector3I blockPosition)
        {
            Vector3D position = this.m_cubeGrid.GridIntegerToWorld(blockPosition);
            MyRenderProxy.DebugDrawPoint(position, room.IsAirtight ? Color.Lerp(Color.Red, Color.Green, room.OxygenLevel(this.m_cubeGrid.GridSize)) : Color.Blue, false, false);
            if (this.m_debugShowRoomIndex)
            {
                MyRenderProxy.DebugDrawText3D(position, room.Index.ToString(), Color.LightGray, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (this.m_debugShowPositions)
            {
                MyRenderProxy.DebugDrawText3D(position, $"{blockPosition.X}, {blockPosition.Y}, {blockPosition.Z}", Color.LightGray, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        private unsafe void DrawRoomInfo(Vector2 textPosition, MyOxygenRoom room)
        {
            if (room.Index == this.m_debugRoomIndex)
            {
                string str = $"{room.BlockCount} : {room.Blocks.Count}";
                float* singlePtr1 = (float*) ref textPosition.Y;
                singlePtr1[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(textPosition, "   Block Count: " + str, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr2 = (float*) ref textPosition.Y;
                singlePtr2[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(textPosition, "   Oxygen Amount: " + room.OxygenAmount, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr3 = (float*) ref textPosition.Y;
                singlePtr3[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(textPosition, "   Min: " + this.m_storedGridMin, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr4 = (float*) ref textPosition.Y;
                singlePtr4[0] += this.m_debugTextlineSize;
                MyRenderProxy.DebugDrawText2D(textPosition, "   Max: " + this.m_storedGridMax, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
        }

        private void DrawRooms(Vector2 textPosition)
        {
            foreach (MyOxygenRoom room in this.m_rooms)
            {
                this.DrawRoomInfo(textPosition, room);
                foreach (Vector3I vectori in room.Blocks)
                {
                    if (((room.Index != 0) || this.m_debugShowTopRoom) && ((this.m_debugRoomIndex == 0) || (room.Index == this.m_debugRoomIndex)))
                    {
                        this.DrawBlock(room, vectori);
                    }
                }
            }
        }

        private void ExpandAirtightData()
        {
            Vector3I vectori = this.GridMin();
            Vector3I vectori2 = this.GridMax();
            Vector3I vectori3 = vectori2 - this.m_storedGridMax;
            if (((this.m_storedGridMin - vectori) != Vector3I.Zero) || (vectori3 != Vector3I.Zero))
            {
                Vector3 vector1 = (vectori2 - vectori) + Vector3I.One;
                this.m_rooms[0].IsDirty = true;
                this.m_storedGridMin = vectori;
                this.m_storedGridMax = vectori2;
            }
        }

        private void GenerateAirtightData()
        {
            if (this.m_rooms == null)
            {
                this.m_rooms = new MyConcurrentList<MyOxygenRoom>();
            }
            else
            {
                this.m_lastRoomIndex = 0;
                this.m_rooms.Clear();
            }
            this.m_initializedBlocks.Clear();
            this.GenerateTopRoom();
            this.GenerateGasBlockRooms();
            this.GenerateEmptyRooms();
            if (this.m_savedRooms != null)
            {
                OxygenRoom[] savedRooms = this.m_savedRooms;
                int index = 0;
                while (true)
                {
                    if (index >= savedRooms.Length)
                    {
                        this.m_savedRooms = null;
                        break;
                    }
                    OxygenRoom room = savedRooms[index];
                    if ((Vector3I.Min(room.StartingPosition, this.m_storedGridMin) == this.m_storedGridMin) && (Vector3I.Max(room.StartingPosition, this.m_storedGridMax) == this.m_storedGridMax))
                    {
                        MyOxygenBlock block = this.m_cubeRoom[room.StartingPosition.X, room.StartingPosition.Y, room.StartingPosition.Z];
                        if (((block != null) && (block.RoomLink != null)) && (block.RoomLink.Room != null))
                        {
                            block.RoomLink.Room.OxygenAmount = room.OxygenAmount;
                        }
                    }
                    index++;
                }
            }
            this.m_initializedBlocks.Clear();
            this.m_gridExpanded = false;
        }

        private void GenerateEmptyRooms()
        {
            Vector3I storedGridMin = this.m_storedGridMin;
            Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref this.m_storedGridMin, ref this.m_storedGridMax);
            while (iterator.IsValid())
            {
                this.CheckPositionForEmptyRoom(storedGridMin);
                iterator.GetNext(out storedGridMin);
            }
        }

        private void GenerateGasBlockRooms()
        {
            foreach (IMySlimBlock block in this.m_gasBlocksForUpdate)
            {
                Vector3I position = block.Position;
                MyOxygenBlock block2 = this.m_cubeRoom[position.X, position.Y, position.Z];
                if ((block2 == null) || (block2.Room == null))
                {
                    HashSet<Vector3I> roomBlocks = this.GetRoomBlocks(block.Position, null);
                    this.CreateAirtightRoom(roomBlocks, 0f, position);
                    this.m_initializedBlocks.UnionWith(roomBlocks);
                }
            }
        }

        private void GenerateTopRoom()
        {
            HashSet<Vector3I> roomBlocks = this.GetRoomBlocks(this.m_storedGridMin, null);
            MyOxygenRoom room = new MyOxygenRoom(0) {
                IsAirtight = false,
                EnvironmentOxygen = MyOxygenProviderSystem.GetOxygenInPoint(this.m_cubeGrid.GridIntegerToWorld(this.m_storedGridMin)),
                DepressurizationTime = this.GetTotalGamePlayTimeInMilliseconds(),
                BlockCount = roomBlocks.Count,
                Blocks = roomBlocks,
                StartingPosition = this.m_storedGridMin
            };
            this.m_rooms.Add(room);
            MyOxygenRoomLink roomPointer = new MyOxygenRoomLink(room);
            foreach (Vector3I vectori in roomBlocks)
            {
                MyOxygenBlock block = new MyOxygenBlock(roomPointer);
                this.m_cubeRoom.Add(vectori, block);
                this.m_initializedBlocks.Add(vectori);
            }
        }

        private MyOxygenRoom GetMaxBlockRoom(ref Vector3I current, MyOxygenRoom topRoom)
        {
            MyOxygenRoom oxygenRoomForCubeGridPosition = this.GetOxygenRoomForCubeGridPosition(ref current);
            for (int i = 0; i < this.m_neighbours.Length; i++)
            {
                Vector3I pos = (Vector3I) (current + this.m_neighbours[i]);
                if ((this.IsInBounds(current) && this.IsInBounds(pos)) && !this.IsAirtightBetweenPositions(current, pos))
                {
                    MyOxygenRoom objA = this.GetOxygenRoomForCubeGridPosition(ref pos);
                    if (objA != null)
                    {
                        if (oxygenRoomForCubeGridPosition == null)
                        {
                            oxygenRoomForCubeGridPosition = objA;
                        }
                        else if (ReferenceEquals(objA, topRoom))
                        {
                            oxygenRoomForCubeGridPosition = topRoom;
                        }
                        else if ((oxygenRoomForCubeGridPosition.BlockCount < objA.BlockCount) && !ReferenceEquals(oxygenRoomForCubeGridPosition, topRoom))
                        {
                            oxygenRoomForCubeGridPosition = objA;
                        }
                    }
                }
            }
            return oxygenRoomForCubeGridPosition;
        }

        internal OxygenRoom[] GetOxygenAmount()
        {
            if ((this.m_rooms == null) || (this.m_rooms.List == null))
            {
                return null;
            }
            int count = this.m_rooms.List.Count;
            MyOxygenRoom[] internalArray = this.m_rooms.List.GetInternalArray<MyOxygenRoom>();
            OxygenRoom[] roomArray2 = new OxygenRoom[count];
            for (int i = 0; i < count; i++)
            {
                MyOxygenRoom room = internalArray[i];
                if (room != null)
                {
                    roomArray2[i].OxygenAmount = room.OxygenAmount;
                    roomArray2[i].StartingPosition = room.StartingPosition;
                }
            }
            return roomArray2;
        }

        public MyOxygenBlock GetOxygenBlock(Vector3D worldPosition)
        {
            Vector3I pos = this.m_cubeGrid.WorldToGridInteger(worldPosition);
            if ((this.m_cubeRoom == null) || !this.IsInBounds(pos))
            {
                return new MyOxygenBlock();
            }
            return this.m_cubeRoom[pos.X, pos.Y, pos.Z];
        }

        public MyOxygenRoom GetOxygenRoomForCubeGridPosition(ref Vector3I gridPosition)
        {
            Vector3I pos = gridPosition;
            if (!this.IsInBounds(pos))
            {
                return null;
            }
            MyOxygenBlock block = this.m_cubeRoom[pos.X, pos.Y, pos.Z];
            return block?.Room;
        }

        private HashSet<Vector3I> GetRoomBlocks(Vector3I startPosition, MyOxygenRoom initRoom = null)
        {
            this.m_blockQueue.Clear();
            this.m_blockQueue.Enqueue(startPosition);
            this.m_visitedBlocks.Clear();
            this.m_visitedBlocks.Add(startPosition);
            HashSet<Vector3I> set = new HashSet<Vector3I>();
            Vector3I item = startPosition;
            set.Add(item);
            if (initRoom != null)
            {
                MyOxygenBlock block;
                if (!this.m_cubeRoom.TryGetValue(item, out block))
                {
                    block = new MyOxygenBlock();
                    this.m_cubeRoom.Add(item, block);
                }
                block.RoomLink = initRoom.Link;
            }
            while (this.m_blockQueue.Count > 0)
            {
                Vector3I startPos = this.m_blockQueue.Dequeue();
                for (int i = 0; i < this.m_neighbours.Length; i++)
                {
                    Vector3I vectori3 = (Vector3I) (startPos + this.m_neighbours[i]);
                    if (((Vector3I.Min(vectori3, this.m_storedGridMin) == this.m_storedGridMin) && ((Vector3I.Max(vectori3, this.m_storedGridMax) == this.m_storedGridMax) && !this.m_visitedBlocks.Contains(vectori3))) && !this.IsAirtightBetweenPositions(startPos, vectori3))
                    {
                        this.m_visitedBlocks.Add(vectori3);
                        this.m_blockQueue.Enqueue(vectori3);
                        Vector3I vectori4 = vectori3;
                        set.Add(vectori4);
                        if (initRoom != null)
                        {
                            MyOxygenBlock block2;
                            if (!this.m_cubeRoom.TryGetValue(vectori4, out block2))
                            {
                                block2 = new MyOxygenBlock();
                                this.m_cubeRoom.Add(vectori4, block2);
                            }
                            block2.RoomLink = initRoom.Link;
                        }
                    }
                }
            }
            return set;
        }

        public MyOxygenBlock GetSafeOxygenBlock(Vector3D position)
        {
            MyOxygenBlock oxygenBlock = this.GetOxygenBlock(position);
            if ((oxygenBlock == null) || (oxygenBlock.Room == null))
            {
                Vector3D vectord = Vector3D.Transform(position, this.m_cubeGrid.PositionComp.WorldMatrixNormalizedInv) / ((double) this.m_cubeGrid.GridSize);
                List<Vector3D> list = new List<Vector3D>(3);
                if ((vectord.X - Math.Floor(vectord.X)) > 0.5)
                {
                    list.Add(new Vector3D(-1.0, 0.0, 0.0));
                }
                else
                {
                    list.Add(new Vector3D(1.0, 0.0, 0.0));
                }
                if ((vectord.Y - Math.Floor(vectord.Y)) > 0.5)
                {
                    list.Add(new Vector3D(0.0, -1.0, 0.0));
                }
                else
                {
                    list.Add(new Vector3D(0.0, 1.0, 0.0));
                }
                if ((vectord.Z - Math.Floor(vectord.Z)) > 0.5)
                {
                    list.Add(new Vector3D(0.0, 0.0, -1.0));
                }
                else
                {
                    list.Add(new Vector3D(0.0, 0.0, 1.0));
                }
                using (List<Vector3D>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Vector3D current = enumerator.Current;
                        Vector3D worldPosition = Vector3D.Transform((vectord + current) * this.m_cubeGrid.GridSize, this.m_cubeGrid.PositionComp.WorldMatrix);
                        MyOxygenBlock block2 = this.GetOxygenBlock(worldPosition);
                        if ((block2 != null) && ((block2.Room != null) && block2.Room.IsAirtight))
                        {
                            return block2;
                        }
                    }
                }
            }
            return oxygenBlock;
        }

        private int GetTotalGamePlayTimeInMilliseconds() => 
            MySandboxGame.TotalGamePlayTimeInMilliseconds;

        private Vector3I GridMax() => 
            ((Vector3I) (this.m_cubeGrid.Max + Vector3I.One));

        private Vector3I GridMin() => 
            (this.m_cubeGrid.Min - Vector3I.One);

        internal void Init(OxygenRoom[] oxygenAmount)
        {
            this.m_savedRooms = oxygenAmount;
        }

        private bool IsAirtightBetweenPositions(Vector3I startPos, Vector3I endPos)
        {
            IMySlimBlock cubeBlock = this.m_cubeGrid.GetCubeBlock(startPos);
            IMySlimBlock objB = this.m_cubeGrid.GetCubeBlock(endPos);
            if (!ReferenceEquals(cubeBlock, objB))
            {
                if ((cubeBlock == null) || !this.IsAirtightBlock(cubeBlock, startPos, (Vector3) (endPos - startPos)))
                {
                    return ((objB != null) && this.IsAirtightBlock(objB, endPos, (Vector3) (startPos - endPos)));
                }
                return true;
            }
            if (cubeBlock == null)
            {
                return false;
            }
            MyCubeBlockDefinition blockDefinition = cubeBlock.BlockDefinition as MyCubeBlockDefinition;
            bool? nullable = this.IsAirtightFromDefinition(blockDefinition, cubeBlock.BuildLevelRatio);
            if (blockDefinition == null)
            {
                return false;
            }
            bool? nullable2 = nullable;
            bool flag = true;
            return ((nullable2.GetValueOrDefault() == flag) & (nullable2 != null));
        }

        private bool IsAirtightBlock(IMySlimBlock block, Vector3I pos, Vector3 normal)
        {
            Matrix matrix;
            MyCubeBlockDefinition blockDefinition = block.BlockDefinition as MyCubeBlockDefinition;
            if (blockDefinition == null)
            {
                return false;
            }
            bool? nullable = this.IsAirtightFromDefinition(blockDefinition, block.BuildLevelRatio);
            if (nullable != null)
            {
                return nullable.Value;
            }
            block.Orientation.GetMatrix(out matrix);
            matrix.TransposeRotationInPlace();
            Vector3I transformedNormal = Vector3I.Round(Vector3.Transform(normal, matrix));
            Vector3 zero = Vector3.Zero;
            if (block.FatBlock != null)
            {
                zero = (Vector3) (pos - block.FatBlock.Position);
            }
            Vector3 vector2 = Vector3.Transform(zero, matrix) + blockDefinition.Center;
            if (blockDefinition.IsCubePressurized[Vector3I.Round(vector2)][transformedNormal])
            {
                return true;
            }
            Sandbox.ModAPI.IMyDoor fatBlock = block.FatBlock as Sandbox.ModAPI.IMyDoor;
            if ((fatBlock == null) || ((fatBlock.Status != DoorStatus.Closed) && (fatBlock.Status != DoorStatus.Closing)))
            {
                return false;
            }
            return this.IsDoorAirtight(fatBlock, ref transformedNormal, blockDefinition);
        }

        private bool? IsAirtightFromDefinition(MyCubeBlockDefinition blockDefinition, float buildLevelRatio)
        {
            if ((blockDefinition.BuildProgressModels != null) && (blockDefinition.BuildProgressModels.Length != 0))
            {
                MyCubeBlockDefinition.BuildProgressModel model = blockDefinition.BuildProgressModels[blockDefinition.BuildProgressModels.Length - 1];
                if (buildLevelRatio < model.BuildRatioUpperBound)
                {
                    return false;
                }
            }
            return blockDefinition.IsAirTight;
        }

        private bool IsDoorAirtight(Sandbox.ModAPI.IMyDoor doorBlock, ref Vector3I transformedNormal, MyCubeBlockDefinition blockDefinition)
        {
            if (doorBlock is MyAdvancedDoor)
            {
                if (doorBlock.IsFullyClosed)
                {
                    foreach (MyCubeBlockDefinition.MountPoint point in blockDefinition.MountPoints)
                    {
                        if (transformedNormal == point.Normal)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                if (doorBlock is MyAirtightSlideDoor)
                {
                    return (doorBlock.IsFullyClosed && (transformedNormal == Vector3I.Forward));
                }
                if (doorBlock is MyAirtightDoorGeneric)
                {
                    return (doorBlock.IsFullyClosed && ((transformedNormal == Vector3I.Forward) || (transformedNormal == Vector3I.Backward)));
                }
                if (doorBlock.IsFullyClosed)
                {
                    foreach (MyCubeBlockDefinition.MountPoint point2 in blockDefinition.MountPoints)
                    {
                        if (transformedNormal == point2.Normal)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool IsInBounds(Vector3I pos) => 
            (!(this.m_storedGridMin != Vector3I.Min(pos, this.m_storedGridMin)) ? !(this.m_storedGridMax != Vector3I.Max(pos, this.m_storedGridMax)) : false);

        private void MarkCubeGridForUpdate()
        {
            MyCubeGrid cubeGrid = this.m_cubeGrid as MyCubeGrid;
            if (cubeGrid != null)
            {
                cubeGrid.MarkForUpdate();
            }
        }

        private void MergeRooms(MyOxygenRoom target, MyOxygenRoom withRoom, MyOxygenRoomLink link)
        {
            if ((target.Blocks != null) && (withRoom.Blocks != null))
            {
                target.Blocks.UnionWith(withRoom.Blocks);
                foreach (Vector3I vectori in withRoom.Blocks)
                {
                    this.m_cubeRoom[vectori.X, vectori.Y, vectori.Z].RoomLink = link;
                }
            }
        }

        public void OnAltitudeChanged()
        {
            if (this.m_rooms != null)
            {
                foreach (MyOxygenRoom room in this.m_rooms)
                {
                    room.EnvironmentOxygen = MyOxygenProviderSystem.GetOxygenInPoint(this.m_cubeGrid.GridIntegerToWorld(room.StartingPosition));
                }
                this.MarkCubeGridForUpdate();
            }
        }

        private void OnBackgroundTaskFinished()
        {
            this.m_isProcessingData = false;
        }

        public void OnCubeGridShrinked()
        {
            if (this.m_rooms == null)
            {
                this.m_generatedDataPending = true;
            }
            else
            {
                this.m_gridShrinked = true;
            }
            this.MarkCubeGridForUpdate();
        }

        private void OnDoorStateChanged(Sandbox.ModAPI.IMyDoor door, bool areOpen)
        {
            if (this.m_gasBlocks.Count != 0)
            {
                MySlimBlock slimBlock = door.SlimBlock as MySlimBlock;
                if (slimBlock != null)
                {
                    if (areOpen)
                    {
                        this.m_deletedBlocks.Add(slimBlock);
                    }
                    else
                    {
                        this.m_addedBlocks.Add(slimBlock);
                    }
                }
                this.MarkCubeGridForUpdate();
            }
        }

        public void OnGridClosing()
        {
            this.isClosing = true;
            if (this.m_isProcessingData)
            {
                try
                {
                    this.m_backgroundTask.WaitOrExecute(false);
                }
                catch (Exception exception)
                {
                    MySandboxGame.Log.WriteLineAndConsole("MyGridGasSystem.OnGridClosing: " + exception.Message + ", " + exception.StackTrace);
                }
            }
            this.m_cubeGrid.OnBlockAdded -= new Action<IMySlimBlock>(this.cubeGrid_OnBlockAdded);
            this.m_cubeGrid.OnBlockRemoved -= new Action<IMySlimBlock>(this.cubeGrid_OnBlockRemoved);
            MyCubeGrid cubeGrid = this.m_cubeGrid as MyCubeGrid;
            if (cubeGrid != null)
            {
                foreach (Sandbox.ModAPI.IMyDoor door in cubeGrid.GetFatBlocks())
                {
                    if (door != null)
                    {
                        door.OnDoorStateChanged -= new Action<Sandbox.ModAPI.IMyDoor, bool>(this.OnDoorStateChanged);
                    }
                }
            }
            this.Clear();
        }

        internal void OnSlimBlockBuildRatioLowered(IMySlimBlock block)
        {
            MyCubeBlockDefinition blockDefinition = block.BlockDefinition as MyCubeBlockDefinition;
            if ((blockDefinition != null) && ((blockDefinition.BuildProgressModels != null) && (blockDefinition.BuildProgressModels.Length != 0)))
            {
                int num = 0;
                int index = blockDefinition.BuildProgressModels.Length - 1;
                while (true)
                {
                    if (index < 0)
                    {
                        if (num == (blockDefinition.BuildProgressModels.Length - 1))
                        {
                            this.cubeGrid_OnBlockRemoved(block);
                        }
                        break;
                    }
                    if (blockDefinition.BuildProgressModels[index].BuildRatioUpperBound > block.BuildLevelRatio)
                    {
                        num = index;
                    }
                    index--;
                }
            }
        }

        internal void OnSlimBlockBuildRatioRaised(IMySlimBlock block)
        {
            MyCubeBlockDefinition blockDefinition = block.BlockDefinition as MyCubeBlockDefinition;
            if ((blockDefinition != null) && ((blockDefinition.BuildProgressModels != null) && (blockDefinition.BuildProgressModels.Length != 0)))
            {
                MyCubeBlockDefinition.BuildProgressModel model = blockDefinition.BuildProgressModels[blockDefinition.BuildProgressModels.Length - 1];
                if (block.BuildLevelRatio >= model.BuildRatioUpperBound)
                {
                    this.cubeGrid_OnBlockAdded(block);
                }
            }
        }

        private void RefreshDirtyRooms()
        {
            int count = this.m_rooms.Count;
            for (int i = 0; i < count; i++)
            {
                MyOxygenRoom room = this.m_rooms[i];
                if (room.Index != 0)
                {
                    this.RefreshRoomBlocks(room);
                }
            }
        }

        private void RefreshRoomBlocks(MyOxygenRoom room)
        {
            MyOxygenRoom room2;
            Vector3I startingPosition;
            Vector3I vectori2;
            HashSet<Vector3I> set;
            bool flag;
            int num;
            if (room == null)
            {
                return;
            }
            else
            {
                if (room.IsAirtight && !room.IsDirty)
                {
                    return;
                }
                room2 = this.m_rooms[0];
                startingPosition = room.StartingPosition;
                vectori2 = startingPosition;
                this.m_blockQueue.Clear();
                this.m_blockQueue.Enqueue(vectori2);
                set = new HashSet<Vector3I> {
                    vectori2
                };
                flag = true;
            }
            goto TR_0031;
        TR_0001:
            room.IsAirtight = flag;
            room.IsDirty = false;
            return;
        TR_0019:
            flag = false;
            goto TR_0031;
        TR_001A:
            num++;
        TR_002E:
            while (true)
            {
                if (num >= this.m_neighbours.Length)
                {
                    break;
                }
                Vector3I item = (Vector3I) (vectori2 + this.m_neighbours[num]);
                if (!set.Contains(item))
                {
                    if (Vector3I.Min(item, this.m_storedGridMin) != this.m_storedGridMin)
                    {
                        goto TR_0019;
                    }
                    else if (!(Vector3I.Max(item, this.m_storedGridMax) != this.m_storedGridMax))
                    {
                        bool flag2 = this.IsAirtightBetweenPositions(vectori2, item);
                        if (!flag2)
                        {
                            set.Add(item);
                            IMySlimBlock cubeBlock = this.m_cubeGrid.GetCubeBlock(item);
                            if (cubeBlock != null)
                            {
                                Sandbox.ModAPI.IMyDoor fatBlock = cubeBlock.FatBlock as Sandbox.ModAPI.IMyDoor;
                                if (fatBlock != null)
                                {
                                    if ((fatBlock.Status == DoorStatus.Open) || !flag2)
                                    {
                                        this.m_blockQueue.Enqueue(item);
                                    }
                                    goto TR_001A;
                                }
                            }
                            MyOxygenBlock block2 = this.m_cubeRoom[item.X, item.Y, item.Z];
                            if (((block2 != null) && (block2.Room != null)) || (cubeBlock == null))
                            {
                                this.m_blockQueue.Enqueue(item);
                            }
                        }
                    }
                    else
                    {
                        goto TR_0019;
                    }
                }
                goto TR_001A;
            }
        TR_0031:
            while (true)
            {
                if (this.m_blockQueue.Count > 0)
                {
                    vectori2 = this.m_blockQueue.Dequeue();
                    num = 0;
                }
                else
                {
                    if (!flag)
                    {
                        goto TR_0001;
                    }
                    else
                    {
                        MyOxygenRoomLink link = null;
                        if (ReferenceEquals(room2, room))
                        {
                            this.m_lastRoomIndex++;
                            room = new MyOxygenRoom(this.m_lastRoomIndex);
                            this.m_rooms.Add(room);
                            link = new MyOxygenRoomLink(room);
                            room.StartingPosition = startingPosition;
                            foreach (Vector3I vectori4 in set)
                            {
                                if (ReferenceEquals(this.m_cubeRoom[vectori4.X, vectori4.Y, vectori4.Z].Room, room2))
                                {
                                    this.m_cubeRoom[vectori4.X, vectori4.Y, vectori4.Z].RoomLink = link;
                                    room.BlockCount++;
                                    room2.BlockCount--;
                                    room2.Blocks.Remove(vectori4);
                                }
                            }
                        }
                        else
                        {
                            link = room.Link;
                            int blockCount = room.BlockCount;
                            room.BlockCount = set.Count;
                            foreach (Vector3I vectori5 in set)
                            {
                                MyOxygenBlock block3;
                                if (!this.m_cubeRoom.TryGetValue(vectori5, out block3))
                                {
                                    block3 = new MyOxygenBlock();
                                    this.m_cubeRoom.Add(vectori5, block3);
                                }
                                block3.RoomLink = link;
                            }
                            if (blockCount > set.Count)
                            {
                                HashSet<Vector3I> blocks = room.Blocks;
                                blocks.ExceptWith(set);
                                float oxygenAmount = (room.OxygenAmount / ((float) blockCount)) * blocks.Count;
                                this.CreateAirtightRoom(blocks, oxygenAmount, blocks.FirstElement<Vector3I>());
                                room.OxygenAmount -= oxygenAmount;
                            }
                        }
                    }
                    room.Blocks = set;
                    goto TR_0001;
                }
                break;
            }
            goto TR_002E;
        }

        private void RefreshRoomData()
        {
            if (this.m_cubeRoom != null)
            {
                if (this.m_gridExpanded)
                {
                    this.m_gridExpanded = false;
                    this.ExpandAirtightData();
                }
                foreach (IMySlimBlock block in this.m_addedBlocksSwap)
                {
                    this.AddBlock(block);
                }
                this.m_addedBlocksSwap.Clear();
                this.RefreshTopRoom();
                this.RefreshDirtyRooms();
                this.m_initializedBlocks.Clear();
                this.GenerateGasBlockRooms();
                this.GenerateEmptyRooms();
                this.m_initializedBlocks.Clear();
            }
        }

        private void RefreshTopRoom()
        {
            MyOxygenRoom initRoom = this.m_rooms[0];
            if (initRoom.IsDirty)
            {
                HashSet<Vector3I> roomBlocks = this.GetRoomBlocks(this.m_storedGridMin, initRoom);
                HashSet<Vector3I> blocks = initRoom.Blocks;
                blocks.ExceptWith(roomBlocks);
                if (blocks.Count != 0)
                {
                    this.CreateAirtightRoom(blocks, 0f, blocks.FirstElement<Vector3I>()).IsDirty = true;
                }
                initRoom.BlockCount = roomBlocks.Count;
                initRoom.Blocks = roomBlocks;
                initRoom.IsDirty = false;
                initRoom.StartingPosition = this.m_storedGridMin;
            }
        }

        private bool RemoveBlock(Vector3I deletedBlockPosition, out Vector3I depressFrom, out Vector3I depressTo)
        {
            bool flag = false;
            depressFrom = Vector3I.Zero;
            depressTo = Vector3I.Zero;
            Vector3I current = deletedBlockPosition;
            MyOxygenRoom topRoom = this.m_rooms[0];
            MyOxygenRoom maxBlockRoom = this.GetMaxBlockRoom(ref current, topRoom);
            if (maxBlockRoom != null)
            {
                for (int i = 0; i < this.m_neighboursForDelete.Length; i++)
                {
                    Vector3I pos = (Vector3I) (current + this.m_neighboursForDelete[i]);
                    if (this.IsInBounds(pos))
                    {
                        MyOxygenRoom oxygenRoomForCubeGridPosition = this.GetOxygenRoomForCubeGridPosition(ref pos);
                        if (((oxygenRoomForCubeGridPosition != null) && !ReferenceEquals(oxygenRoomForCubeGridPosition, maxBlockRoom)) && ((current == pos) || !this.IsAirtightBetweenPositions(current, pos)))
                        {
                            if (maxBlockRoom.IsAirtight && !oxygenRoomForCubeGridPosition.IsAirtight)
                            {
                                oxygenRoomForCubeGridPosition.BlockCount += maxBlockRoom.BlockCount;
                                oxygenRoomForCubeGridPosition.OxygenAmount += maxBlockRoom.OxygenAmount;
                                this.MergeRooms(oxygenRoomForCubeGridPosition, maxBlockRoom, oxygenRoomForCubeGridPosition.Link);
                                if ((maxBlockRoom.Blocks != null) && (oxygenRoomForCubeGridPosition.Blocks != null))
                                {
                                    oxygenRoomForCubeGridPosition.Blocks.UnionWith(maxBlockRoom.Blocks);
                                }
                                if ((maxBlockRoom.OxygenLevel(this.m_cubeGrid.GridSize) - oxygenRoomForCubeGridPosition.EnvironmentOxygen) > 0.2f)
                                {
                                    flag = true;
                                    depressFrom = current;
                                    depressTo = pos;
                                }
                                maxBlockRoom.IsAirtight = false;
                                maxBlockRoom.OxygenAmount = 0f;
                                maxBlockRoom.EnvironmentOxygen = Math.Max(maxBlockRoom.EnvironmentOxygen, oxygenRoomForCubeGridPosition.EnvironmentOxygen);
                                maxBlockRoom.DepressurizationTime = this.GetTotalGamePlayTimeInMilliseconds();
                                maxBlockRoom.Link.Room = oxygenRoomForCubeGridPosition;
                                if (!ReferenceEquals(oxygenRoomForCubeGridPosition, maxBlockRoom) && !ReferenceEquals(maxBlockRoom, topRoom))
                                {
                                    maxBlockRoom.BlockCount = 0;
                                    maxBlockRoom.Blocks = null;
                                    this.m_rooms.Remove(maxBlockRoom);
                                }
                                maxBlockRoom = oxygenRoomForCubeGridPosition;
                            }
                            else if (maxBlockRoom.IsAirtight || !oxygenRoomForCubeGridPosition.IsAirtight)
                            {
                                maxBlockRoom.BlockCount += oxygenRoomForCubeGridPosition.BlockCount;
                                maxBlockRoom.OxygenAmount += oxygenRoomForCubeGridPosition.OxygenAmount;
                                this.MergeRooms(maxBlockRoom, oxygenRoomForCubeGridPosition, maxBlockRoom.Link);
                                oxygenRoomForCubeGridPosition.Link.Room = maxBlockRoom;
                                if (!ReferenceEquals(oxygenRoomForCubeGridPosition, maxBlockRoom) && !ReferenceEquals(oxygenRoomForCubeGridPosition, topRoom))
                                {
                                    oxygenRoomForCubeGridPosition.BlockCount = 0;
                                    oxygenRoomForCubeGridPosition.Blocks = null;
                                    this.m_rooms.Remove(oxygenRoomForCubeGridPosition);
                                }
                            }
                            else
                            {
                                maxBlockRoom.BlockCount += oxygenRoomForCubeGridPosition.BlockCount;
                                maxBlockRoom.OxygenAmount += oxygenRoomForCubeGridPosition.OxygenAmount;
                                this.MergeRooms(maxBlockRoom, oxygenRoomForCubeGridPosition, maxBlockRoom.Link);
                                maxBlockRoom.EnvironmentOxygen = Math.Max(maxBlockRoom.EnvironmentOxygen, oxygenRoomForCubeGridPosition.EnvironmentOxygen);
                                if ((oxygenRoomForCubeGridPosition.OxygenLevel(this.m_cubeGrid.GridSize) - maxBlockRoom.EnvironmentOxygen) > 0.2f)
                                {
                                    flag = true;
                                    depressFrom = current;
                                    depressTo = pos;
                                }
                                oxygenRoomForCubeGridPosition.IsAirtight = false;
                                oxygenRoomForCubeGridPosition.OxygenAmount = 0f;
                                oxygenRoomForCubeGridPosition.EnvironmentOxygen = Math.Max(maxBlockRoom.EnvironmentOxygen, oxygenRoomForCubeGridPosition.EnvironmentOxygen);
                                oxygenRoomForCubeGridPosition.DepressurizationTime = this.GetTotalGamePlayTimeInMilliseconds();
                                oxygenRoomForCubeGridPosition.Link.Room = maxBlockRoom;
                                if (!ReferenceEquals(oxygenRoomForCubeGridPosition, maxBlockRoom) && !ReferenceEquals(oxygenRoomForCubeGridPosition, topRoom))
                                {
                                    oxygenRoomForCubeGridPosition.BlockCount = 0;
                                    oxygenRoomForCubeGridPosition.Blocks = null;
                                    this.m_rooms.Remove(oxygenRoomForCubeGridPosition);
                                }
                            }
                        }
                    }
                }
                Vector3I key = current;
                MyOxygenBlock block = this.m_cubeRoom[key.X, key.Y, key.Z];
                if (block == null)
                {
                    block = new MyOxygenBlock();
                    this.m_cubeRoom.Add(key, block);
                }
                if (block.Room == null)
                {
                    block.RoomLink = maxBlockRoom.Link;
                    maxBlockRoom.BlockCount++;
                    maxBlockRoom.Blocks.Add(key);
                }
            }
            return flag;
        }

        private void RemoveBlocks()
        {
            bool flag = false;
            Vector3I zero = Vector3I.Zero;
            Vector3I vectori2 = Vector3I.Zero;
            foreach (IMySlimBlock local1 in this.m_deletedBlocksSwap)
            {
                Vector3I min = local1.Min;
                Vector3I start = local1.Min;
                Vector3I max = local1.Max;
                Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref start, ref max);
                while (iterator.IsValid())
                {
                    Vector3I vectori6;
                    Vector3I vectori7;
                    if (this.RemoveBlock(min, out vectori6, out vectori7))
                    {
                        flag = true;
                        zero = vectori6;
                        vectori2 = vectori7;
                    }
                    iterator.GetNext(out min);
                }
            }
            if (flag)
            {
                IMyCubeGrid cubeGrid = this.m_cubeGrid;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long, Vector3I, Vector3I>(x => new Action<long, Vector3I, Vector3I>(MyCubeGrid.DepressurizeEffect), this.m_cubeGrid.EntityId, zero, vectori2, targetEndpoint, position);
            }
            this.m_deletedBlocksSwap.Clear();
        }

        private bool ShouldPressurize()
        {
            if (this.m_cubeGrid.Physics != null)
            {
                if (this.m_gasBlocks.Count > 0)
                {
                    return true;
                }
                if (this.m_rooms == null)
                {
                    return false;
                }
                for (int i = 0; i < this.m_rooms.Count; i++)
                {
                    MyOxygenRoom room = this.m_rooms[i];
                    if (room.IsAirtight && (room.OxygenAmount > 1f))
                    {
                        return true;
                    }
                    if (!room.IsAirtight && ((this.GetTotalGamePlayTimeInMilliseconds() - room.DepressurizationTime) < 1500f))
                    {
                        return true;
                    }
                }
                this.m_rooms = null;
                this.m_lastRoomIndex = 0;
                this.m_cubeRoom = null;
            }
            return false;
        }

        private void ShrinkData()
        {
            if (this.m_cubeRoom != null)
            {
                Vector3I vectori = this.GridMin();
                Vector3I vectori2 = this.GridMax();
                Vector3I vectori3 = this.m_storedGridMax - vectori2;
                if (((vectori - this.m_storedGridMin) != Vector3I.Zero) || (vectori3 != Vector3I.Zero))
                {
                    this.m_storedGridMin = vectori;
                    this.m_storedGridMax = vectori2;
                    MyOxygenRoom room = this.m_rooms[0];
                    HashSet<Vector3I> other = new HashSet<Vector3I>();
                    foreach (Vector3I vectori4 in room.Blocks)
                    {
                        if (!this.IsInBounds(vectori4))
                        {
                            other.Add(vectori4);
                        }
                    }
                    if (other.Count > 0)
                    {
                        room.Blocks.ExceptWith(other);
                        room.BlockCount = room.Blocks.Count;
                        room.StartingPosition = this.m_storedGridMin;
                    }
                }
            }
        }

        private void StartGenerateAirtightData()
        {
            this.m_isProcessingData = true;
            this.m_cubeRoom = new MyOxygenCube();
            this.m_previousGridMin = this.m_storedGridMin;
            this.m_previousGridMax = this.m_storedGridMax;
            this.m_storedGridMin = this.GridMin();
            this.m_storedGridMax = this.GridMax();
            this.m_addedBlocks.Clear();
            this.m_deletedBlocks.Clear();
            this.m_gasBlocksForUpdate.Clear();
            this.m_gasBlocksForUpdate.AddRange(this.m_gasBlocks);
            this.MarkCubeGridForUpdate();
            this.m_backgroundTask = Parallel.Start(new Action(this.GenerateAirtightData), new Action(this.OnBackgroundTaskFinished));
        }

        private void StartRefreshRoomData()
        {
            if (!this.m_isProcessingData)
            {
                if (this.m_cubeRoom == null)
                {
                    this.m_addedBlocks.Clear();
                    this.m_gridExpanded = false;
                }
                else
                {
                    if (this.m_gridExpanded)
                    {
                        this.m_previousGridMin = this.m_storedGridMin;
                        this.m_previousGridMax = this.m_storedGridMax;
                    }
                    List<IMySlimBlock> addedBlocksSwap = this.m_addedBlocksSwap;
                    this.m_addedBlocksSwap = this.m_addedBlocks;
                    this.m_addedBlocks = addedBlocksSwap;
                    this.m_gasBlocksForUpdate.Clear();
                    this.m_gasBlocksForUpdate.AddRange(this.m_gasBlocks);
                    this.m_isProcessingData = true;
                    this.m_backgroundTask = Parallel.Start(new Action(this.RefreshRoomData), new Action(this.OnBackgroundTaskFinished));
                }
            }
        }

        private void StartRemoveBlocks()
        {
            if (!this.m_isProcessingData)
            {
                if (this.m_gasBlocks.Count == 0)
                {
                    this.Clear();
                }
                if (this.m_rooms == null)
                {
                    this.m_deletedBlocks.Clear();
                }
                else
                {
                    this.m_isProcessingData = true;
                    List<IMySlimBlock> deletedBlocksSwap = this.m_deletedBlocksSwap;
                    this.m_deletedBlocksSwap = this.m_deletedBlocks;
                    this.m_deletedBlocks = deletedBlocksSwap;
                    this.m_backgroundTask = Parallel.Start(new Action(this.RemoveBlocks), new Action(this.OnBackgroundTaskFinished));
                }
            }
        }

        private void StartShrinkData()
        {
            if (!this.m_isProcessingData)
            {
                this.m_previousGridMin = this.m_storedGridMin;
                this.m_previousGridMax = this.m_storedGridMax;
                this.m_isProcessingData = true;
                this.m_gridShrinked = false;
                this.m_backgroundTask = Parallel.Start(new Action(this.ShrinkData), new Action(this.OnBackgroundTaskFinished));
            }
        }

        public void UpdateBeforeSimulation()
        {
            if (MyFakes.BACKGROUND_OXYGEN && !this.m_isProcessingData)
            {
                MySimpleProfiler.Begin("Gas System", MySimpleProfiler.ProfilingBlockType.BLOCK, "UpdateBeforeSimulation");
                if (this.m_generatedDataPending)
                {
                    if (MyFakes.BACKGROUND_OXYGEN && this.ShouldPressurize())
                    {
                        this.StartGenerateAirtightData();
                    }
                    this.m_generatedDataPending = false;
                }
                if (this.m_gridShrinked)
                {
                    this.StartShrinkData();
                }
                if (this.m_addedBlocks.Count > 0)
                {
                    this.StartRefreshRoomData();
                }
                if (this.m_deletedBlocks.Count > 0)
                {
                    this.StartRemoveBlocks();
                }
                MySimpleProfiler.End("UpdateBeforeSimulation");
            }
        }

        public void UpdateBeforeSimulation100()
        {
        }

        public bool NeedsPerFrameUpdate =>
            (!this.m_isProcessingData && (this.m_generatedDataPending || (this.m_gridShrinked || (this.m_gridExpanded || ((this.m_deletedBlocks.Count > 0) || (this.m_addedBlocks.Count > 0))))));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridGasSystem.<>c <>9 = new MyGridGasSystem.<>c();
            public static Func<IMyEventOwner, Action<long, Vector3I, Vector3I>> <>9__74_0;

            internal Action<long, Vector3I, Vector3I> <RemoveBlocks>b__74_0(IMyEventOwner x) => 
                new Action<long, Vector3I, Vector3I>(MyCubeGrid.DepressurizeEffect);
        }
    }
}

