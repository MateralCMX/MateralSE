namespace Sandbox.Game.World
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.Utils;
    using VRageMath;

    public class MySessionSnapshot
    {
        private static FastResourceLock m_savingLock = new FastResourceLock();
        public string TargetDir;
        public string SavingDir;
        public MyObjectBuilder_Checkpoint CheckpointSnapshot;
        public MyObjectBuilder_Sector SectorSnapshot;
        public const int MAX_WINDOWS_PATH = 260;

        private void Backup()
        {
            if (MySession.Static.MaxBackupSaves <= 0)
            {
                if ((MySession.Static.MaxBackupSaves == 0) && Directory.Exists(Path.Combine(this.TargetDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER)))
                {
                    Directory.Delete(Path.Combine(this.TargetDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER), true);
                }
            }
            else
            {
                string str = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                string path = Path.Combine(this.TargetDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER, str);
                Directory.CreateDirectory(path);
                string[] files = Directory.GetFiles(this.TargetDir);
                int index = 0;
                while (true)
                {
                    if (index >= files.Length)
                    {
                        string[] directories = Directory.GetDirectories(Path.Combine(this.TargetDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER));
                        if (!IsSorted(directories))
                        {
                            Array.Sort<string>(directories);
                        }
                        if (directories.Length <= MySession.Static.MaxBackupSaves)
                        {
                            break;
                        }
                        int num2 = directories.Length - MySession.Static.MaxBackupSaves;
                        for (int i = 0; i < num2; i++)
                        {
                            Directory.Delete(directories[i], true);
                        }
                        return;
                    }
                    string str3 = files[index];
                    string destFileName = Path.Combine(path, Path.GetFileName(str3));
                    if ((destFileName.Length < 260) && (str3.Length < 260))
                    {
                        File.Copy(str3, destFileName, true);
                    }
                    index++;
                }
            }
        }

        private bool CheckAccessToFiles()
        {
            foreach (string str in Directory.GetFiles(this.TargetDir, "*", SearchOption.TopDirectoryOnly))
            {
                if ((str != MySession.Static.ThumbPath) && !MyFileSystem.CheckFileWriteAccess(str))
                {
                    MySandboxGame.Log.WriteLine($"Couldn't access file '{Path.GetFileName(str)}'.");
                    return false;
                }
            }
            return true;
        }

        public static bool IsSorted(string[] arr)
        {
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i - 1].CompareTo(arr[i]) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Save()
        {
            bool flag = true;
            using (m_savingLock.AcquireExclusiveUsing())
            {
                MySandboxGame.Log.WriteLine("Session snapshot save - START");
                using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
                {
                    Directory.CreateDirectory(this.TargetDir);
                    MySandboxGame.Log.WriteLine("Checking file access for files in target dir.");
                    if (this.CheckAccessToFiles())
                    {
                        string savingDir = this.SavingDir;
                        if (Directory.Exists(savingDir))
                        {
                            Directory.Delete(savingDir, true);
                        }
                        Directory.CreateDirectory(savingDir);
                        try
                        {
                            bool flag4;
                            ulong sizeInBytes = 0UL;
                            ulong num2 = 0UL;
                            ulong num3 = 0UL;
                            flag = MyLocalCache.SaveSector(this.SectorSnapshot, this.SavingDir, Vector3I.Zero, out sizeInBytes) && flag4;
                            if (flag)
                            {
                                foreach (KeyValuePair<string, byte[]> pair in this.VoxelSnapshots)
                                {
                                    bool flag1;
                                    ulong size = 0UL;
                                    flag = flag && flag1;
                                    if (flag)
                                    {
                                        flag1 = this.SaveVoxelSnapshot(pair.Key, pair.Value, true, out size);
                                        num3 += size;
                                    }
                                }
                                this.VoxelSnapshots.Clear();
                                this.VoxelStorageNameCache.Clear();
                                foreach (KeyValuePair<string, byte[]> pair2 in this.CompressedVoxelSnapshots)
                                {
                                    bool flag3;
                                    ulong size = 0UL;
                                    flag = flag && flag3;
                                    if (flag)
                                    {
                                        flag3 = this.SaveVoxelSnapshot(pair2.Key, pair2.Value, false, out size);
                                        num3 += size;
                                    }
                                }
                                this.CompressedVoxelSnapshots.Clear();
                            }
                            if (flag && Sync.IsServer)
                            {
                                flag = MyLocalCache.SaveLastSessionInfo(this.TargetDir, false, false, MySession.Static.Name, null, 0);
                            }
                            if (flag)
                            {
                                flag4 = MyLocalCache.SaveCheckpoint(this.CheckpointSnapshot, this.SavingDir, out num2);
                                this.SavedSizeInBytes = (sizeInBytes + num2) + num3;
                            }
                        }
                        catch (Exception exception)
                        {
                            MySandboxGame.Log.WriteLine("There was an error while saving snapshot.");
                            MySandboxGame.Log.WriteLine(exception);
                            flag = false;
                        }
                        if (!flag)
                        {
                            if (Directory.Exists(savingDir))
                            {
                                Directory.Delete(savingDir, true);
                            }
                        }
                        else
                        {
                            HashSet<string> set = new HashSet<string>();
                            string[] files = Directory.GetFiles(savingDir);
                            int index = 0;
                            while (true)
                            {
                                if (index >= files.Length)
                                {
                                    files = Directory.GetFiles(this.TargetDir);
                                    index = 0;
                                    while (true)
                                    {
                                        if (index >= files.Length)
                                        {
                                            Directory.Delete(savingDir);
                                            this.Backup();
                                            break;
                                        }
                                        string str4 = files[index];
                                        string item = Path.GetFileName(str4);
                                        if (!set.Contains(item) && (item != MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION))
                                        {
                                            File.Delete(str4);
                                        }
                                        index++;
                                    }
                                    break;
                                }
                                string path = files[index];
                                string fileName = Path.GetFileName(path);
                                string str3 = Path.Combine(this.TargetDir, fileName);
                                if (File.Exists(str3))
                                {
                                    File.Delete(str3);
                                }
                                File.Move(path, str3);
                                set.Add(fileName);
                                index++;
                            }
                        }
                    }
                    else
                    {
                        this.SavingSuccess = false;
                        return false;
                    }
                }
                MySandboxGame.Log.WriteLine("Session snapshot save - END");
            }
            this.SavingSuccess = flag;
            return flag;
        }

        public void SaveParallel(Action completionCallback = null)
        {
            Action action = () => this.Save();
            if (completionCallback != null)
            {
                Parallel.Start(action, completionCallback);
            }
            else
            {
                Parallel.Start(action);
            }
        }

        private bool SaveVoxelSnapshot(string storageName, byte[] snapshotData, bool compress, out ulong size)
        {
            string path = Path.Combine(this.SavingDir, storageName + ".vx2");
            try
            {
                if (compress)
                {
                    using (MemoryStream stream = new MemoryStream(0x4000))
                    {
                        using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Compress))
                        {
                            stream2.Write(snapshotData, 0, snapshotData.Length);
                        }
                        byte[] bytes = stream.ToArray();
                        File.WriteAllBytes(path, bytes);
                        size = (ulong) bytes.Length;
                        if (this.VoxelStorageNameCache != null)
                        {
                            IMyStorage storage = null;
                            if (this.VoxelStorageNameCache.TryGetValue(storageName, out storage) && !storage.Closed)
                            {
                                storage.SetCompressedDataCache(bytes);
                            }
                        }
                        goto TR_0002;
                    }
                }
                File.WriteAllBytes(path, snapshotData);
                size = (ulong) snapshotData.Length;
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine($"Failed to write voxel file '{path}'");
                MySandboxGame.Log.WriteLine(exception);
                size = 0L;
                return false;
            }
        TR_0002:
            return true;
        }

        public static void WaitForSaving()
        {
            int exclusiveWaiters = 0;
            while (true)
            {
                using (m_savingLock.AcquireExclusiveUsing())
                {
                    exclusiveWaiters = m_savingLock.ExclusiveWaiters;
                }
                if (exclusiveWaiters <= 0)
                {
                    return;
                }
            }
        }

        public Dictionary<string, byte[]> CompressedVoxelSnapshots { get; set; }

        public Dictionary<string, byte[]> VoxelSnapshots { get; set; }

        public Dictionary<string, IMyStorage> VoxelStorageNameCache { get; set; }

        public ulong SavedSizeInBytes { get; private set; }

        public bool SavingSuccess { get; private set; }
    }
}

