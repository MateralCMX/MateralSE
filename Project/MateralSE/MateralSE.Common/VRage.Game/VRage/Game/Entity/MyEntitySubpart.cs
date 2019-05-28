namespace VRage.Game.Entity
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender.Import;

    public class MyEntitySubpart : MyEntity
    {
        public MyEntitySubpart()
        {
            base.Save = false;
        }

        public static bool GetSubpartFromDummy(string modelPath, string dummyName, MyModelDummy dummy, ref Data outData)
        {
            if (!dummyName.Contains("subpart_"))
            {
                return false;
            }
            Data data = new Data {
                Name = dummyName.Substring("subpart_".Length),
                File = Path.Combine(Path.GetDirectoryName(modelPath), (string) dummy.CustomData["file"]) + ".mwm",
                InitialTransform = Matrix.Normalize(dummy.Matrix)
            };
            outData = data;
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Data
        {
            public string Name;
            public string File;
            public Matrix InitialTransform;
        }
    }
}

