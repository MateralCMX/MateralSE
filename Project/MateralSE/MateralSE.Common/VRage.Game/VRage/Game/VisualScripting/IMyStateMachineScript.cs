namespace VRage.Game.VisualScripting
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMyStateMachineScript
    {
        [VisualScriptingMiscData("Self", "Completes the scripts by setting state to completed.", -10510688), VisualScriptingMember(true, true)]
        void Complete(string transitionName = "Completed");
        [VisualScriptingMember(true, false)]
        void Deserialize();
        [VisualScriptingMember(true, false)]
        void Dispose();
        [VisualScriptingMember(false, true)]
        long GetOwnerId();
        [VisualScriptingMember(true, false)]
        void Init();
        [VisualScriptingMember(true, false)]
        void Update();

        string TransitionTo { get; set; }

        long OwnerId { get; set; }
    }
}

