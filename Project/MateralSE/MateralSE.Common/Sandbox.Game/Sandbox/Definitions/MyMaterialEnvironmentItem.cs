namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRageMath;

    public class MyMaterialEnvironmentItem
    {
        public MyDefinitionId Definition;
        public string GroupId;
        public int GroupIndex;
        public string ModifierId;
        public int ModifierIndex;
        public float Frequency;
        private bool m_detail;
        public bool IsBot;
        public bool IsVoxel;
        public bool IsEnvironemntItem;
        public Vector3 BaseColor;
        public Vector2 ColorSpread;
        public float Offset;
        public float MaxRoll;

        public bool IsDetail
        {
            get => 
                (this.m_detail || (this.IsBot || this.IsVoxel));
            set => 
                (this.m_detail = value);
        }
    }
}

