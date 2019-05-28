namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRageMath;

    [PreloadRequired]
    public static class MyCubeGridDefinitions
    {
        public static readonly Dictionary<Vector3I, MyEdgeOrientationInfo> EdgeOrientations;
        private static TableEntry[] m_tileTable;
        private static MatrixI[] m_allPossible90rotations;
        private static MatrixI[][] m_uniqueTopologyRotationTable;

        static MyCubeGridDefinitions()
        {
            Dictionary<Vector3I, MyEdgeOrientationInfo> dictionary1 = new Dictionary<Vector3I, MyEdgeOrientationInfo>(new Vector3INormalEqualityComparer());
            dictionary1.Add(new Vector3I(0, 0, 1), new MyEdgeOrientationInfo(Matrix.Identity, MyCubeEdgeType.Horizontal));
            dictionary1.Add(new Vector3I(0, 0, -1), new MyEdgeOrientationInfo(Matrix.Identity, MyCubeEdgeType.Horizontal));
            dictionary1.Add(new Vector3I(1, 0, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(1.570796f), MyCubeEdgeType.Horizontal));
            dictionary1.Add(new Vector3I(-1, 0, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(1.570796f), MyCubeEdgeType.Horizontal));
            dictionary1.Add(new Vector3I(0, 1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationX(1.570796f), MyCubeEdgeType.Vertical));
            dictionary1.Add(new Vector3I(0, -1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationX(1.570796f), MyCubeEdgeType.Vertical));
            dictionary1.Add(new Vector3I(-1, 0, -1), new MyEdgeOrientationInfo(Matrix.CreateRotationZ(1.570796f), MyCubeEdgeType.Horizontal_Diagonal));
            dictionary1.Add(new Vector3I(1, 0, 1), new MyEdgeOrientationInfo(Matrix.CreateRotationZ(1.570796f), MyCubeEdgeType.Horizontal_Diagonal));
            dictionary1.Add(new Vector3I(-1, 0, 1), new MyEdgeOrientationInfo(Matrix.CreateRotationZ(-1.570796f), MyCubeEdgeType.Horizontal_Diagonal));
            dictionary1.Add(new Vector3I(1, 0, -1), new MyEdgeOrientationInfo(Matrix.CreateRotationZ(-1.570796f), MyCubeEdgeType.Horizontal_Diagonal));
            dictionary1.Add(new Vector3I(0, 1, -1), new MyEdgeOrientationInfo(Matrix.Identity, MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(0, -1, 1), new MyEdgeOrientationInfo(Matrix.Identity, MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(-1, -1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(-1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(0, -1, -1), new MyEdgeOrientationInfo(Matrix.CreateRotationX(1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(1, -1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(-1, 1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(1, 1, 0), new MyEdgeOrientationInfo(Matrix.CreateRotationY(-1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            dictionary1.Add(new Vector3I(0, 1, 1), new MyEdgeOrientationInfo(Matrix.CreateRotationX(1.570796f), MyCubeEdgeType.Vertical_Diagonal));
            EdgeOrientations = dictionary1;
            TableEntry entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.None
            };
            MyTileDefinition definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Right, Vector3.Up),
                Normal = Vector3.Up,
                FullQuad = true
            };
            MyTileDefinition[] definitionArray1 = new MyTileDefinition[6];
            definitionArray1[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray1[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Backward),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray1[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Left, Vector3.Down),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray1[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Backward, Vector3.Right),
                Normal = Vector3.Right,
                FullQuad = true
            };
            definitionArray1[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Left),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray1[5] = definition;
            entry.Tiles = definitionArray1;
            MyEdgeDefinition definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray2 = new MyEdgeDefinition[12];
            definitionArray2[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, 1, 1),
                Side0 = 0,
                Side1 = 5
            };
            definitionArray2[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 4
            };
            definitionArray2[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray2[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 1
            };
            definitionArray2[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 3,
                Side1 = 5
            };
            definitionArray2[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray2[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 2
            };
            definitionArray2[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 5
            };
            definitionArray2[8] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray2[9] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 2
            };
            definitionArray2[10] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 4,
                Side1 = 2
            };
            definitionArray2[11] = definition2;
            entry.Edges = definitionArray2;
            TableEntry[] entryArray1 = new TableEntry[0x13];
            entryArray1[0] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(0f, 1f, 1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray3 = new MyTileDefinition[5];
            definitionArray3[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray3[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray3[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray3[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray3[4] = definition;
            entry.Tiles = definitionArray3;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray4 = new MyEdgeDefinition[9];
            definitionArray4[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 0,
                Side1 = 1
            };
            definitionArray4[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray4[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray4[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray4[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray4[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray4[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray4[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 2,
                Side1 = 4
            };
            definitionArray4[8] = definition2;
            entry.Edges = definitionArray4;
            entryArray1[1] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(1f, -1f, 0f)
            };
            MyTileDefinition[] definitionArray5 = new MyTileDefinition[4];
            definitionArray5[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray5[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(-1f, 1f, 1f)),
                IsEmpty = true
            };
            definitionArray5[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Down,
                Up = new Vector3(1f, 0f, -1f)
            };
            definitionArray5[3] = definition;
            entry.Tiles = definitionArray5;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray6 = new MyEdgeDefinition[6];
            definitionArray6[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray6[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray6[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 1
            };
            definitionArray6[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray6[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray6[5] = definition2;
            entry.Edges = definitionArray6;
            entryArray1[2] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(1f, -1f, -1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray7 = new MyTileDefinition[7];
            definitionArray7[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Right,
                Up = new Vector3(0f, 1f, 1f)
            };
            definitionArray7[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f) * Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(-1f, 1f, 0f)
            };
            definitionArray7[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(-1.570796f) * Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Down,
                Up = new Vector3(-1f, 0f, 1f)
            };
            definitionArray7[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Up,
                FullQuad = true
            };
            definitionArray7[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray7[5] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(1.570796f),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray7[6] = definition;
            entry.Tiles = definitionArray7;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 2,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray8 = new MyEdgeDefinition[12];
            definitionArray8[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 2,
                Side1 = 5
            };
            definitionArray8[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 2,
                Side1 = 0
            };
            definitionArray8[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 1
            };
            definitionArray8[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 6,
                Side1 = 1
            };
            definitionArray8[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 1
            };
            definitionArray8[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray8[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 6,
                Side1 = 3
            };
            definitionArray8[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 5,
                Side1 = 3
            };
            definitionArray8[8] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 6
            };
            definitionArray8[9] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, 1, -1),
                Side0 = 5,
                Side1 = 4
            };
            definitionArray8[10] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 6
            };
            definitionArray8[11] = definition2;
            entry.Edges = definitionArray8;
            entryArray1[3] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Right),
                Normal = Vector3.Right,
                FullQuad = true
            };
            MyTileDefinition[] definitionArray9 = new MyTileDefinition[6];
            definitionArray9[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Right, Vector3.Up),
                Normal = Vector3.Up,
                FullQuad = true
            };
            definitionArray9[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray9[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Left),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray9[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Left, Vector3.Down),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray9[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Backward),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray9[5] = definition;
            entry.Tiles = definitionArray9;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray10 = new MyEdgeDefinition[12];
            definitionArray10[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, 1, 1),
                Side0 = 0,
                Side1 = 5
            };
            definitionArray10[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 4
            };
            definitionArray10[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray10[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 1
            };
            definitionArray10[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 3,
                Side1 = 5
            };
            definitionArray10[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray10[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 2
            };
            definitionArray10[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 5
            };
            definitionArray10[8] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray10[9] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 2
            };
            definitionArray10[10] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 4,
                Side1 = 2
            };
            definitionArray10[11] = definition2;
            entry.Edges = definitionArray10;
            entryArray1[4] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Right, Vector3.Up),
                Normal = Vector3.Up,
                FullQuad = true
            };
            MyTileDefinition[] definitionArray11 = new MyTileDefinition[6];
            definitionArray11[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray11[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Backward),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray11[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Left, Vector3.Down),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray11[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Backward, Vector3.Right),
                Normal = Vector3.Right,
                FullQuad = true
            };
            definitionArray11[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Left),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray11[5] = definition;
            entry.Tiles = definitionArray11;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray12 = new MyEdgeDefinition[12];
            definitionArray12[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, 1, 1),
                Side0 = 0,
                Side1 = 5
            };
            definitionArray12[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 4
            };
            definitionArray12[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray12[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 1
            };
            definitionArray12[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 3,
                Side1 = 5
            };
            definitionArray12[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray12[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 2
            };
            definitionArray12[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 5
            };
            definitionArray12[8] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray12[9] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 2
            };
            definitionArray12[10] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 4,
                Side1 = 2
            };
            definitionArray12[11] = definition2;
            entry.Edges = definitionArray12;
            entryArray1[5] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(0f, 1f, 1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray13 = new MyTileDefinition[5];
            definitionArray13[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                Up = new Vector3(0f, -1f, -1f),
                IsRounded = true
            };
            definitionArray13[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f),
                IsRounded = true
            };
            definitionArray13[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray13[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray13[4] = definition;
            entry.Tiles = definitionArray13;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray14 = new MyEdgeDefinition[7];
            definitionArray14[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray14[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray14[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray14[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray14[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray14[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 2,
                Side1 = 4
            };
            definitionArray14[6] = definition2;
            entry.Edges = definitionArray14;
            entryArray1[6] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(1f, -1f, 0f),
                IsRounded = true
            };
            MyTileDefinition[] definitionArray15 = new MyTileDefinition[4];
            definitionArray15[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f),
                IsRounded = true
            };
            definitionArray15[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(-1f, 1f, 1f)),
                IsEmpty = true
            };
            definitionArray15[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Down,
                Up = new Vector3(1f, 0f, -1f),
                IsRounded = true
            };
            definitionArray15[3] = definition;
            entry.Tiles = definitionArray15;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray16 = new MyEdgeDefinition[3];
            definitionArray16[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray16[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray16[2] = definition2;
            entry.Edges = definitionArray16;
            entryArray1[7] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(1f, -1f, -1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray17 = new MyTileDefinition[7];
            definitionArray17[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Right,
                Up = new Vector3(0f, 1f, 1f),
                IsRounded = true
            };
            definitionArray17[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f) * Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(-1f, 1f, 0f),
                IsRounded = true
            };
            definitionArray17[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(-1.570796f) * Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Down,
                Up = new Vector3(-1f, 0f, 1f),
                IsRounded = true
            };
            definitionArray17[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Up,
                FullQuad = true
            };
            definitionArray17[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray17[5] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(1.570796f),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray17[6] = definition;
            entry.Tiles = definitionArray17;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 2,
                Side1 = 3
            };
            MyEdgeDefinition[] definitionArray18 = new MyEdgeDefinition[9];
            definitionArray18[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 2,
                Side1 = 4
            };
            definitionArray18[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 3,
                Side1 = 1
            };
            definitionArray18[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 5,
                Side1 = 1
            };
            definitionArray18[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 0
            };
            definitionArray18[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 4,
                Side1 = 0
            };
            definitionArray18[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 4,
                Side1 = 5
            };
            definitionArray18[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, 1, -1),
                Side0 = 4,
                Side1 = 3
            };
            definitionArray18[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 3,
                Side1 = 5
            };
            definitionArray18[8] = definition2;
            entry.Edges = definitionArray18;
            entryArray1[8] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(0f, 1f, 1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray19 = new MyTileDefinition[5];
            definitionArray19[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray19[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray19[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray19[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray19[4] = definition;
            entry.Tiles = definitionArray19;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray20 = new MyEdgeDefinition[9];
            definitionArray20[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 0,
                Side1 = 1
            };
            definitionArray20[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray20[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray20[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray20[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray20[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray20[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray20[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 2,
                Side1 = 4
            };
            definitionArray20[8] = definition2;
            entry.Edges = definitionArray20;
            entryArray1[9] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(1f, -1f, 0f)
            };
            MyTileDefinition[] definitionArray21 = new MyTileDefinition[4];
            definitionArray21[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, -1f)
            };
            definitionArray21[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(-1f, 1f, 1f)),
                IsEmpty = true
            };
            definitionArray21[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Down,
                Up = new Vector3(1f, 0f, -1f)
            };
            definitionArray21[3] = definition;
            entry.Tiles = definitionArray21;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray22 = new MyEdgeDefinition[6];
            definitionArray22[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 0,
                Side1 = 3
            };
            definitionArray22[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 0,
                Side1 = 2
            };
            definitionArray22[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 1
            };
            definitionArray22[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray22[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray22[5] = definition2;
            entry.Edges = definitionArray22;
            entryArray1[10] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up),
                Normal = Vector3.Normalize(new Vector3(0f, 2f, 1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray23 = new MyTileDefinition[6];
            definitionArray23[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray23[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Backward),
                Normal = Vector3.Backward
            };
            definitionArray23[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Left, Vector3.Down),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray23[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Backward, Vector3.Right),
                Normal = Vector3.Right,
                Up = new Vector3(0f, -2f, -1f),
                DontOffsetTexture = true
            };
            definitionArray23[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Left),
                Normal = Vector3.Left,
                Up = new Vector3(0f, -2f, -1f),
                DontOffsetTexture = true
            };
            definitionArray23[5] = definition;
            entry.Tiles = definitionArray23;
            definition2 = new MyEdgeDefinition {
                Point0 = new Vector3(-1f, 1f, -1f),
                Point1 = new Vector3(1f, 1f, -1f),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray24 = new MyEdgeDefinition[7];
            definitionArray24[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 1
            };
            definitionArray24[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 3,
                Side1 = 5
            };
            definitionArray24[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray24[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 3,
                Side1 = 2
            };
            definitionArray24[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 5
            };
            definitionArray24[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray24[6] = definition2;
            entry.Edges = definitionArray24;
            entryArray1[11] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(0f, 2f, 1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray25 = new MyTileDefinition[5];
            definitionArray25[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                Up = new Vector3(0f, -2f, -1f),
                IsEmpty = true
            };
            definitionArray25[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -2f, -1f),
                IsEmpty = true
            };
            definitionArray25[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                FullQuad = true
            };
            definitionArray25[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward
            };
            definitionArray25[4] = definition;
            entry.Tiles = definitionArray25;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 0,
                Side1 = 3
            };
            MyEdgeDefinition[] definitionArray26 = new MyEdgeDefinition[4];
            definitionArray26[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray26[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray26[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 2,
                Side1 = 3
            };
            definitionArray26[3] = definition2;
            entry.Edges = definitionArray26;
            entryArray1[12] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(2f, 1f, 1f)),
                IsEmpty = true,
                DontOffsetTexture = true
            };
            MyTileDefinition[] definitionArray27 = new MyTileDefinition[5];
            definitionArray27[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                Up = new Vector3(0f, 1f, 1f),
                DontOffsetTexture = true
            };
            definitionArray27[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, 1f),
                IsEmpty = true,
                DontOffsetTexture = true
            };
            definitionArray27[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                Up = new Vector3(-2f, 0f, 1f),
                DontOffsetTexture = true
            };
            definitionArray27[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                Up = new Vector3(-2f, 1f, 0f),
                DontOffsetTexture = true
            };
            definitionArray27[4] = definition;
            entry.Tiles = definitionArray27;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 0,
                Side1 = 1
            };
            MyEdgeDefinition[] definitionArray28 = new MyEdgeDefinition[4];
            definitionArray28[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 4
            };
            definitionArray28[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            definitionArray28[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 1,
                Side1 = 4
            };
            definitionArray28[3] = definition2;
            entry.Edges = definitionArray28;
            entryArray1[13] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(1f, -2f, 0f),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray29 = new MyTileDefinition[4];
            definitionArray29[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Right,
                Up = new Vector3(0f, -2f, -1f),
                IsEmpty = true
            };
            definitionArray29[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(-1f, 2f, 1f)),
                IsEmpty = true
            };
            definitionArray29[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Down,
                Up = new Vector3(1f, 0f, -1f),
                IsEmpty = true
            };
            definitionArray29[3] = definition;
            entry.Tiles = definitionArray29;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 1,
                Side1 = 3
            };
            entry.Edges = new MyEdgeDefinition[] { definition2 };
            entryArray1[14] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(2f, -2f, -1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray31 = new MyTileDefinition[7];
            definitionArray31[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Right,
                Up = new Vector3(0f, -1f, 2f)
            };
            definitionArray31[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f) * Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(2f, 0f, -1f)
            };
            definitionArray31[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(-1.570796f) * Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Down,
                Up = new Vector3(1f, 0f, 2f)
            };
            definitionArray31[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Up,
                FullQuad = true
            };
            definitionArray31[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray31[5] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(1.570796f),
                Normal = Vector3.Backward,
                FullQuad = true
            };
            definitionArray31[6] = definition;
            entry.Tiles = definitionArray31;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 2,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray32 = new MyEdgeDefinition[9];
            definitionArray32[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 2,
                Side1 = 5
            };
            definitionArray32[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 1
            };
            definitionArray32[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, -1, 1),
                Side0 = 6,
                Side1 = 1
            };
            definitionArray32[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 6,
                Side1 = 3
            };
            definitionArray32[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 5,
                Side1 = 3
            };
            definitionArray32[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 6
            };
            definitionArray32[6] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, 1, -1),
                Side0 = 5,
                Side1 = 4
            };
            definitionArray32[7] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 6
            };
            definitionArray32[8] = definition2;
            entry.Edges = definitionArray32;
            entryArray1[15] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(2f, -2f, -1f)),
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray33 = new MyTileDefinition[7];
            definitionArray33[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Right,
                Up = new Vector3(0f, 1f, 1f)
            };
            definitionArray33[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(3.141593f) * Matrix.CreateRotationY(-1.570796f),
                Normal = Vector3.Forward,
                Up = new Vector3(0f, -2f, -1f)
            };
            definitionArray33[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(-1.570796f) * Matrix.CreateRotationX(3.141593f),
                Normal = Vector3.Down,
                Up = new Vector3(2f, 0f, -1f)
            };
            definitionArray33[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Up,
                FullQuad = true
            };
            definitionArray33[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(1.570796f),
                Normal = Vector3.Left,
                FullQuad = true
            };
            definitionArray33[5] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(1.570796f),
                Normal = Vector3.Backward
            };
            definitionArray33[6] = definition;
            entry.Tiles = definitionArray33;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 2,
                Side1 = 4
            };
            MyEdgeDefinition[] definitionArray34 = new MyEdgeDefinition[7];
            definitionArray34[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 2,
                Side1 = 5
            };
            definitionArray34[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 1
            };
            definitionArray34[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, -1),
                Side0 = 5,
                Side1 = 3
            };
            definitionArray34[3] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, -1, 1),
                Side0 = 5,
                Side1 = 6
            };
            definitionArray34[4] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(-1, 1, -1),
                Side0 = 5,
                Side1 = 4
            };
            definitionArray34[5] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, 1),
                Point1 = (Vector3) new Vector3I(1, 1, 1),
                Side0 = 4,
                Side1 = 6
            };
            definitionArray34[6] = definition2;
            entry.Edges = definitionArray34;
            entryArray1[0x10] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Horizontal
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Right),
                Normal = Vector3.Right,
                FullQuad = false
            };
            MyTileDefinition[] definitionArray35 = new MyTileDefinition[6];
            definitionArray35[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Backward),
                Normal = Vector3.Backward,
                FullQuad = false,
                IsEmpty = true
            };
            definitionArray35[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up),
                Normal = Vector3.Up,
                FullQuad = false
            };
            definitionArray35[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Left),
                Normal = Vector3.Left,
                FullQuad = false
            };
            definitionArray35[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Down, Vector3.Forward),
                Normal = Vector3.Forward,
                FullQuad = true
            };
            definitionArray35[4] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Down),
                Normal = Vector3.Down,
                FullQuad = false
            };
            definitionArray35[5] = definition;
            entry.Tiles = definitionArray35;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 4,
                Side1 = 5
            };
            MyEdgeDefinition[] definitionArray36 = new MyEdgeDefinition[4];
            definitionArray36[0] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(-1, 1, -1),
                Side0 = 4,
                Side1 = 3
            };
            definitionArray36[1] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 4,
                Side1 = 0
            };
            definitionArray36[2] = definition2;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, 1, -1),
                Point1 = (Vector3) new Vector3I(1, 1, -1),
                Side0 = 4,
                Side1 = 2
            };
            definitionArray36[3] = definition2;
            entry.Edges = definitionArray36;
            entryArray1[0x11] = entry;
            entry = new TableEntry {
                RotationOptions = MyRotationOptionsEnum.Both
            };
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(-1.570796f),
                Normal = Vector3.Forward,
                IsEmpty = true
            };
            MyTileDefinition[] definitionArray37 = new MyTileDefinition[5];
            definitionArray37[0] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Normalize(new Vector3(0f, 1f, 1f)),
                IsEmpty = true
            };
            definitionArray37[1] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.Identity,
                Normal = Vector3.Left,
                IsEmpty = true
            };
            definitionArray37[2] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationZ(3.141593f),
                Normal = Vector3.Down,
                IsEmpty = true
            };
            definitionArray37[3] = definition;
            definition = new MyTileDefinition {
                LocalMatrix = Matrix.CreateRotationX(-1.570796f) * Matrix.CreateRotationY(3.141593f),
                Normal = Vector3.Right,
                IsEmpty = true
            };
            definitionArray37[4] = definition;
            entry.Tiles = definitionArray37;
            definition2 = new MyEdgeDefinition {
                Point0 = (Vector3) new Vector3I(-1, -1, -1),
                Point1 = (Vector3) new Vector3I(1, -1, -1),
                Side0 = 3,
                Side1 = 0
            };
            entry.Edges = new MyEdgeDefinition[] { definition2 };
            entryArray1[0x12] = entry;
            m_tileTable = entryArray1;
            InitTopologyUniqueRotationsMatrices();
        }

        private static void FillRotationsForTopology(MyCubeTopology topology, int mainTile)
        {
            Vector3[] vectorArray = new Vector3[m_allPossible90rotations.Length];
            m_uniqueTopologyRotationTable[(int) topology] = new MatrixI[m_allPossible90rotations.Length];
            for (int i = 0; i < m_allPossible90rotations.Length; i++)
            {
                int index = -1;
                if (mainTile != -1)
                {
                    Vector3 vector;
                    Vector3.TransformNormal(ref m_tileTable[(int) topology].Tiles[mainTile].Normal, ref m_allPossible90rotations[i], out vector);
                    vectorArray[i] = vector;
                    for (int j = 0; j < i; j++)
                    {
                        if (Vector3.Dot(vectorArray[j], vector) > 0.98f)
                        {
                            index = j;
                            break;
                        }
                    }
                }
                m_uniqueTopologyRotationTable[(int) topology][i] = (index == -1) ? m_allPossible90rotations[i] : m_uniqueTopologyRotationTable[(int) topology][index];
            }
        }

        public static MyRotationOptionsEnum GetCubeRotationOptions(MyCubeBlockDefinition block) => 
            ((block.CubeDefinition != null) ? m_tileTable[(int) block.CubeDefinition.CubeTopology].RotationOptions : MyRotationOptionsEnum.Both);

        public static MyTileDefinition[] GetCubeTiles(MyCubeBlockDefinition block) => 
            ((block.CubeDefinition != null) ? m_tileTable[(int) block.CubeDefinition.CubeTopology].Tiles : null);

        public static void GetRotatedBlockSize(MyCubeBlockDefinition block, ref Matrix rotation, out Vector3I size)
        {
            Vector3I.TransformNormal(ref block.Size, ref rotation, out size);
        }

        public static TableEntry GetTopologyInfo(MyCubeTopology topology) => 
            m_tileTable[(int) topology];

        public static MyBlockOrientation GetTopologyUniqueOrientation(MyCubeTopology myCubeTopology, MyBlockOrientation orientation)
        {
            if (m_uniqueTopologyRotationTable[(int) myCubeTopology] != null)
            {
                for (int i = 0; i < m_allPossible90rotations.Length; i++)
                {
                    MatrixI xi = m_allPossible90rotations[i];
                    if ((xi.Forward == orientation.Forward) && (xi.Up == orientation.Up))
                    {
                        return m_uniqueTopologyRotationTable[(int) myCubeTopology][i].GetBlockOrientation();
                    }
                }
            }
            return MyBlockOrientation.Identity;
        }

        private static void InitTopologyUniqueRotationsMatrices()
        {
            MatrixI[] xiArray1 = new MatrixI[0x18];
            xiArray1[0] = new MatrixI(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);
            xiArray1[1] = new MatrixI(Base6Directions.Direction.Down, Base6Directions.Direction.Forward);
            xiArray1[2] = new MatrixI(Base6Directions.Direction.Backward, Base6Directions.Direction.Down);
            xiArray1[3] = new MatrixI(Base6Directions.Direction.Up, Base6Directions.Direction.Backward);
            xiArray1[4] = new MatrixI(Base6Directions.Direction.Forward, Base6Directions.Direction.Right);
            xiArray1[5] = new MatrixI(Base6Directions.Direction.Down, Base6Directions.Direction.Right);
            xiArray1[6] = new MatrixI(Base6Directions.Direction.Backward, Base6Directions.Direction.Right);
            xiArray1[7] = new MatrixI(Base6Directions.Direction.Up, Base6Directions.Direction.Right);
            xiArray1[8] = new MatrixI(Base6Directions.Direction.Forward, Base6Directions.Direction.Down);
            xiArray1[9] = new MatrixI(Base6Directions.Direction.Up, Base6Directions.Direction.Forward);
            xiArray1[10] = new MatrixI(Base6Directions.Direction.Backward, Base6Directions.Direction.Up);
            xiArray1[11] = new MatrixI(Base6Directions.Direction.Down, Base6Directions.Direction.Backward);
            xiArray1[12] = new MatrixI(Base6Directions.Direction.Forward, Base6Directions.Direction.Left);
            xiArray1[13] = new MatrixI(Base6Directions.Direction.Up, Base6Directions.Direction.Left);
            xiArray1[14] = new MatrixI(Base6Directions.Direction.Backward, Base6Directions.Direction.Left);
            xiArray1[15] = new MatrixI(Base6Directions.Direction.Down, Base6Directions.Direction.Left);
            xiArray1[0x10] = new MatrixI(Base6Directions.Direction.Left, Base6Directions.Direction.Up);
            xiArray1[0x11] = new MatrixI(Base6Directions.Direction.Left, Base6Directions.Direction.Backward);
            xiArray1[0x12] = new MatrixI(Base6Directions.Direction.Left, Base6Directions.Direction.Down);
            xiArray1[0x13] = new MatrixI(Base6Directions.Direction.Left, Base6Directions.Direction.Forward);
            xiArray1[20] = new MatrixI(Base6Directions.Direction.Right, Base6Directions.Direction.Down);
            xiArray1[0x15] = new MatrixI(Base6Directions.Direction.Right, Base6Directions.Direction.Backward);
            xiArray1[0x16] = new MatrixI(Base6Directions.Direction.Right, Base6Directions.Direction.Up);
            xiArray1[0x17] = new MatrixI(Base6Directions.Direction.Right, Base6Directions.Direction.Forward);
            m_allPossible90rotations = xiArray1;
            m_uniqueTopologyRotationTable = new MatrixI[Enum.GetValues(typeof(MyCubeTopology)).Length][];
            m_uniqueTopologyRotationTable[0] = null;
            FillRotationsForTopology(MyCubeTopology.Slope, 0);
            FillRotationsForTopology(MyCubeTopology.Corner, 2);
            FillRotationsForTopology(MyCubeTopology.InvCorner, 0);
            FillRotationsForTopology(MyCubeTopology.StandaloneBox, -1);
            FillRotationsForTopology(MyCubeTopology.RoundedSlope, -1);
            FillRotationsForTopology(MyCubeTopology.RoundSlope, 0);
            FillRotationsForTopology(MyCubeTopology.RoundCorner, 2);
            FillRotationsForTopology(MyCubeTopology.RoundInvCorner, -1);
            FillRotationsForTopology(MyCubeTopology.RotatedSlope, -1);
            FillRotationsForTopology(MyCubeTopology.RotatedCorner, -1);
            FillRotationsForTopology(MyCubeTopology.HalfBox, 1);
            FillRotationsForTopology(MyCubeTopology.Slope2Base, -1);
            FillRotationsForTopology(MyCubeTopology.Slope2Tip, -1);
            FillRotationsForTopology(MyCubeTopology.Corner2Base, -1);
            FillRotationsForTopology(MyCubeTopology.Corner2Tip, -1);
            FillRotationsForTopology(MyCubeTopology.InvCorner2Base, -1);
            FillRotationsForTopology(MyCubeTopology.InvCorner2Tip, -1);
            FillRotationsForTopology(MyCubeTopology.HalfSlopeBox, -1);
        }

        public static MatrixI[] AllPossible90rotations =>
            m_allPossible90rotations;

        public class TableEntry
        {
            public MyRotationOptionsEnum RotationOptions;
            public MyTileDefinition[] Tiles;
            public MyEdgeDefinition[] Edges;
        }
    }
}

