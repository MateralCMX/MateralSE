namespace Sandbox.Game.Screens.Helpers.InputRecording
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using VRageMath;

    [Serializable, Obfuscation(Feature="cw symbol renaming", Exclude=true)]
    public class MyInputRecording
    {
        public string Name;
        public string Description;
        public List<MyInputSnapshot> SnapshotSequence = new List<MyInputSnapshot>();
        public MyInputRecordingSession Session;
        public int OriginalWidth;
        public int OriginalHeight;
        public bool UseReplayInstead;
        private int m_currentSnapshotNumber = 0;
        private int m_startScreenWidth;
        private int m_startScreenHeight;

        public void AddSnapshot(MyInputSnapshot snapshot)
        {
            this.SnapshotSequence.Add(snapshot);
        }

        public static MyInputRecording FromFile(string filename)
        {
            using (StreamReader reader = new StreamReader(filename))
            {
                return (MyInputRecording) new XmlSerializer(typeof(MyInputRecording)).Deserialize(reader);
            }
        }

        public MyInputSnapshot GetCurrentSnapshot() => 
            this.SnapshotSequence[this.m_currentSnapshotNumber];

        public Vector2 GetMouseNormalizationFactor() => 
            new Vector2(((float) this.m_startScreenWidth) / ((float) this.OriginalWidth), ((float) this.m_startScreenHeight) / ((float) this.OriginalHeight));

        public MyInputSnapshot GetNextSnapshot()
        {
            int currentSnapshotNumber = this.m_currentSnapshotNumber;
            this.m_currentSnapshotNumber = currentSnapshotNumber + 1;
            return this.SnapshotSequence[currentSnapshotNumber];
        }

        public int GetStartingScreenHeight() => 
            this.m_startScreenHeight;

        public int GetStartingScreenWidth() => 
            this.m_startScreenWidth;

        public bool IsDone() => 
            (this.m_currentSnapshotNumber == this.SnapshotSequence.Count);

        public void RemoveRest()
        {
            this.m_currentSnapshotNumber--;
            this.SnapshotSequence.RemoveRange(this.m_currentSnapshotNumber, this.SnapshotSequence.Count - this.m_currentSnapshotNumber);
        }

        public void Save()
        {
            Directory.CreateDirectory(this.Name);
            XmlSerializer serializer = new XmlSerializer(typeof(MyInputRecording));
            using (TextWriter writer = new StreamWriter(Path.Combine(this.Name, "input.xml"), false))
            {
                serializer.Serialize(writer, this);
            }
        }

        public void SetStartingScreenDimensions(int width, int height)
        {
            this.m_startScreenWidth = width;
            this.m_startScreenHeight = height;
        }
    }
}

