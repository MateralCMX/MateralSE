namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyModelDummy : IMyModelDummy
    {
        public const string SUBBLOCK_PREFIX = "subblock_";
        public const string SUBPART_PREFIX = "subpart_";
        public const string ATTRIBUTE_FILE = "file";
        public const string ATTRIBUTE_HIGHLIGHT = "highlight";
        public const string ATTRIBUTE_HIGHLIGHT_TYPE = "highlighttype";
        public const string ATTRIBUTE_HIGHLIGHT_SEPARATOR = ";";
        public string Name;
        public Dictionary<string, object> CustomData;
        public VRageMath.Matrix Matrix;

        string IMyModelDummy.Name =>
            this.Name;

        VRageMath.Matrix IMyModelDummy.Matrix =>
            this.Matrix;
    }
}

