namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyVoxelPathfindingLog : IMyPathfindingLog
    {
        private string m_navmeshName;
        private List<Operation> m_operations = new List<Operation>();
        private int m_ctr;
        private MyLog m_log;

        public MyVoxelPathfindingLog(string filename)
        {
            string path = Path.Combine(MyFileSystem.UserDataPath, filename);
            if (MyFakes.REPLAY_NAVMESH_GENERATION)
            {
                StreamReader reader = new StreamReader(path);
                string input = null;
                string pattern = @"NMOP: Voxel NavMesh: (\S+) (ADD|REM) \[X:(\d+), Y:(\d+), Z:(\d+)\]";
                string str4 = @"VOXOP: (\S*) \[X:(\d+), Y:(\d+), Z:(\d+)\] \[X:(\d+), Y:(\d+), Z:(\d+)\] (\S+) (\S+)";
                while (true)
                {
                    input = reader.ReadLine();
                    if (input == null)
                    {
                        reader.Close();
                        break;
                    }
                    char[] separator = new char[] { '[' };
                    input.Split(separator);
                    MatchCollection matchs = Regex.Matches(input, pattern);
                    if (matchs.Count == 1)
                    {
                        string str5 = matchs[0].Groups[1].Value;
                        if (this.m_navmeshName == null)
                        {
                            this.m_navmeshName = str5;
                        }
                        bool addition = matchs[0].Groups[2].Value == "ADD";
                        Vector3I cellCoord = new Vector3I(int.Parse(matchs[0].Groups[3].Value), int.Parse(matchs[0].Groups[4].Value), int.Parse(matchs[0].Groups[5].Value));
                        this.m_operations.Add(new NavMeshOp(this.m_navmeshName, addition, cellCoord));
                        continue;
                    }
                    matchs = Regex.Matches(input, str4);
                    if (matchs.Count == 1)
                    {
                        string voxelName = matchs[0].Groups[1].Value;
                        Vector3I voxelRangeMin = new Vector3I(int.Parse(matchs[0].Groups[2].Value), int.Parse(matchs[0].Groups[3].Value), int.Parse(matchs[0].Groups[4].Value));
                        Vector3I voxelRangeMax = new Vector3I(int.Parse(matchs[0].Groups[5].Value), int.Parse(matchs[0].Groups[6].Value), int.Parse(matchs[0].Groups[7].Value));
                        this.m_operations.Add(new VoxelWriteOp(voxelName, matchs[0].Groups[9].Value, (MyStorageDataTypeFlags) Enum.Parse(typeof(MyStorageDataTypeFlags), matchs[0].Groups[8].Value), voxelRangeMin, voxelRangeMax));
                    }
                }
            }
            if (MyFakes.LOG_NAVMESH_GENERATION)
            {
                this.m_log = new MyLog(false);
                this.m_log.Init(path, MyFinalBuildConstants.APP_VERSION_STRING);
            }
        }

        public void Close()
        {
            if (this.m_log != null)
            {
                this.m_log.Close();
            }
        }

        public void DebugDraw()
        {
            if (MyFakes.REPLAY_NAVMESH_GENERATION)
            {
                MyRenderProxy.DebugDrawText2D(new Vector2(500f, 10f), $"Next operation: {this.m_ctr}/{this.m_operations.Count}", Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
        }

        public void LogCellAddition(MyVoxelNavigationMesh navMesh, Vector3I cell)
        {
            this.m_log.WriteLine("NMOP: " + navMesh.ToString() + " ADD " + cell.ToString());
        }

        public void LogCellRemoval(MyVoxelNavigationMesh navMesh, Vector3I cell)
        {
            this.m_log.WriteLine("NMOP: " + navMesh.ToString() + " REM " + cell.ToString());
        }

        public void LogStorageWrite(MyVoxelBase map, MyStorageData source, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax)
        {
            string str = source.ToBase64();
            this.m_log.WriteLine($"VOXOP: {map.StorageName} {voxelRangeMin} {voxelRangeMax} {dataToWrite} {str}");
        }

        public void PerformOneOperation(bool triggerPressed)
        {
            if ((triggerPressed || (this.m_ctr <= 0x7fffffff)) && (this.m_ctr < this.m_operations.Count))
            {
                this.m_operations[this.m_ctr].Perform();
                this.m_ctr++;
            }
        }

        public int Counter =>
            this.m_ctr;

        private class NavMeshOp : MyVoxelPathfindingLog.Operation
        {
            private string m_navmeshName;
            private bool m_isAddition;
            private Vector3I m_cellCoord;

            public NavMeshOp(string navmeshName, bool addition, Vector3I cellCoord)
            {
                this.m_navmeshName = navmeshName;
                this.m_isAddition = addition;
                this.m_cellCoord = cellCoord;
            }

            public override void Perform()
            {
                char[] separator = new char[] { '-' };
                MyVoxelBase map = MySession.Static.VoxelMaps.TryGetVoxelMapByNameStart(this.m_navmeshName.Split(separator)[0]);
                if ((map != null) && (MyCestmirPathfindingShorts.Pathfinding.VoxelPathfinding.GetVoxelMapNavmesh(map) != null))
                {
                    bool isAddition = this.m_isAddition;
                }
            }
        }

        private abstract class Operation
        {
            protected Operation()
            {
            }

            public abstract void Perform();
        }

        private class VoxelWriteOp : MyVoxelPathfindingLog.Operation
        {
            private string m_voxelName;
            private string m_data;
            private MyStorageDataTypeFlags m_dataType;
            private Vector3I m_voxelMin;
            private Vector3I m_voxelMax;

            public VoxelWriteOp(string voxelName, string data, MyStorageDataTypeFlags dataToWrite, Vector3I voxelRangeMin, Vector3I voxelRangeMax)
            {
                this.m_voxelName = voxelName;
                this.m_data = data;
                this.m_dataType = dataToWrite;
                this.m_voxelMin = voxelRangeMin;
                this.m_voxelMax = voxelRangeMax;
            }

            public override void Perform()
            {
                MyVoxelBase base2 = MySession.Static.VoxelMaps.TryGetVoxelMapByNameStart(this.m_voxelName);
                if (base2 != null)
                {
                    base2.Storage.WriteRange(MyStorageData.FromBase64(this.m_data), this.m_dataType, this.m_voxelMin, this.m_voxelMax, true, false);
                }
            }
        }
    }
}

