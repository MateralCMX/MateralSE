namespace VRageRender.Animations
{
    using System;
    using VRage.Generics;
    using VRage.Utils;

    public class MyAnimationStateMachineTransition : MyStateMachineTransition
    {
        public double TransitionTimeInSec;
        public MyAnimationTransitionSyncType Sync = MyAnimationTransitionSyncType.NoSynchonization;
        public MyAnimationTransitionCurve Curve;

        public override bool Evaluate() => 
            ((base.Conditions.Count <= 0) ? (base.Name == MyStringId.NullOrEmpty) : base.Evaluate());
    }
}

