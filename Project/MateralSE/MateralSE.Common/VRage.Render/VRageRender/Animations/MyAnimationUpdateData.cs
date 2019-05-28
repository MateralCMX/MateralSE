namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyAnimationUpdateData
    {
        public double DeltaTimeInSeconds;
        public MyAnimationController Controller;
        public MyCharacterBone[] CharacterBones;
        public bool[] LayerBoneMask;
        public List<MyAnimationClip.BoneState> BonesResult;
        public int[] VisitedTreeNodesPath;
        public int VisitedTreeNodesCounter;
        public void AddVisitedTreeNodesPathPoint(int nextPoint)
        {
            if ((this.VisitedTreeNodesPath != null) && (this.VisitedTreeNodesCounter < this.VisitedTreeNodesPath.Length))
            {
                this.VisitedTreeNodesPath[this.VisitedTreeNodesCounter] = nextPoint;
                this.VisitedTreeNodesCounter++;
            }
        }
    }
}

