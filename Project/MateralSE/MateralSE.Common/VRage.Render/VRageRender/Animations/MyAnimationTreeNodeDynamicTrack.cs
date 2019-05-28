namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public class MyAnimationTreeNodeDynamicTrack : MyAnimationTreeNodeTrack
    {
        public static Func<MyStringId, DynamicTrackData> OnAction;
        public MyStringId DefaultAnimation;

        public override void SetAction(MyStringId action)
        {
            DynamicTrackData data = new DynamicTrackData();
            if (OnAction != null)
            {
                data = OnAction(action);
            }
            if (data.Clip == null)
            {
                data = OnAction(this.DefaultAnimation);
            }
            if (data.Clip != null)
            {
                base.SetClip(data.Clip);
                base.Loop = data.Loop;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DynamicTrackData
        {
            public MyAnimationClip Clip;
            public bool Loop;
        }
    }
}

