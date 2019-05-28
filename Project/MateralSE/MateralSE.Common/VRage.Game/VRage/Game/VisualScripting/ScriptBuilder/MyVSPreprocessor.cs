namespace VRage.Game.VisualScripting.ScriptBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.FileSystem;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyVSPreprocessor
    {
        private readonly HashSet<string> m_filePaths = new HashSet<string>();
        private readonly HashSet<string> m_classNames = new HashSet<string>();

        public void AddFile(string filePath, string localModPath)
        {
            MyObjectBuilder_VSFiles files;
            if (filePath == null)
            {
                return;
            }
            else if (this.m_filePaths.Add(filePath))
            {
                files = null;
                MyContentPath path1 = new MyContentPath(filePath, localModPath);
                using (Stream stream = MyFileSystem.OpenRead(filePath))
                {
                    if (stream == null)
                    {
                        MyLog.Default.WriteLine("VisualScripting Preprocessor: " + filePath + " is Missing.");
                    }
                    if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(stream, out files))
                    {
                        this.m_filePaths.Remove(filePath);
                        return;
                    }
                }
                if (files.VisualScript != null)
                {
                    if (this.m_classNames.Add(files.VisualScript.Name))
                    {
                        foreach (string str in files.VisualScript.DependencyFilePaths)
                        {
                            this.AddFile(new MyContentPath(str, localModPath).GetExitingFilePath(), localModPath);
                        }
                    }
                    else
                    {
                        this.m_filePaths.Remove(filePath);
                    }
                }
            }
            else
            {
                return;
            }
            if (files.StateMachine != null)
            {
                MyObjectBuilder_ScriptSMNode[] nodes = files.StateMachine.Nodes;
                int index = 0;
                while (true)
                {
                    if (index >= nodes.Length)
                    {
                        this.m_filePaths.Remove(filePath);
                        break;
                    }
                    MyObjectBuilder_ScriptSMNode node = nodes[index];
                    if ((!(node is MyObjectBuilder_ScriptSMSpreadNode) && !(node is MyObjectBuilder_ScriptSMBarrierNode)) && !string.IsNullOrEmpty(node.ScriptFilePath))
                    {
                        this.AddFile(new MyContentPath(node.ScriptFilePath, localModPath).GetExitingFilePath(), localModPath);
                    }
                    index++;
                }
            }
            if (files.LevelScript != null)
            {
                if (this.m_classNames.Add(files.LevelScript.Name))
                {
                    foreach (string str2 in files.LevelScript.DependencyFilePaths)
                    {
                        this.AddFile(new MyContentPath(str2, localModPath).GetExitingFilePath(), localModPath);
                    }
                }
                else
                {
                    this.m_filePaths.Remove(filePath);
                }
            }
        }

        public void Clear()
        {
            this.m_filePaths.Clear();
            this.m_classNames.Clear();
        }

        public string[] FileSet
        {
            get
            {
                string[] strArray = new string[this.m_filePaths.Count];
                int index = 0;
                foreach (string str in this.m_filePaths)
                {
                    index++;
                    strArray[index] = str;
                }
                return strArray;
            }
        }
    }
}

