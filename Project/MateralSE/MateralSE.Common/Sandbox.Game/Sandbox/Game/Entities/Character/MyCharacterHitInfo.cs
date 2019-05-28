namespace Sandbox.Game.Entities.Character
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Models;
    using VRageMath;

    public class MyCharacterHitInfo
    {
        public MyCharacterHitInfo()
        {
            this.CapsuleIndex = -1;
        }

        public void Reset()
        {
            this.CapsuleIndex = -1;
            this.BoneIndex = -1;
            CapsuleD ed = new CapsuleD();
            this.Capsule = ed;
            Vector3 vector = new Vector3();
            this.HitNormalBindingPose = vector;
            vector = new Vector3();
            this.HitPositionBindingPose = vector;
            Matrix matrix = new Matrix();
            this.BindingTransformation = matrix;
            MyIntersectionResultLineTriangleEx ex = new MyIntersectionResultLineTriangleEx();
            this.Triangle = ex;
            this.HitHead = false;
        }

        public int CapsuleIndex { get; set; }

        public int BoneIndex { get; set; }

        public CapsuleD Capsule { get; set; }

        public Vector3 HitNormalBindingPose { get; set; }

        public Vector3 HitPositionBindingPose { get; set; }

        public Matrix BindingTransformation { get; set; }

        public MyIntersectionResultLineTriangleEx Triangle { get; set; }

        public bool HitHead { get; set; }
    }
}

