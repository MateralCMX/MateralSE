namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Network;
    using VRageMath;

    [MyDebugScreen("VRage", "Network Character")]
    internal class MyGuiScreenDebugNetworkCharacter : MyGuiScreenDebugBase
    {
        private MyGuiControlSlider m_maxJetpackGridDistanceSlider;
        private MyGuiControlSlider m_maxDisconnectDistanceSlider;
        private MyGuiControlSlider m_minJetpackGridSpeedSlider;
        private MyGuiControlSlider m_minJetpackDisconnectGridSpeedSlider;
        private MyGuiControlSlider m_minJetpackInsideGridSpeedSlider;
        private MyGuiControlSlider m_minJetpackDisconnectInsideGridSpeedSlider;
        private MyGuiControlSlider m_maxJetpackGridAccelerationSlider;
        private MyGuiControlSlider m_maxJetpackDisconnectGridAccelerationSlider;

        public MyGuiScreenDebugNetworkCharacter() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2? captionOffset = null;
            base.AddCaption("Network Character", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            this.AddSlider("Animation fallback threshold [m]", MyCharacterPhysicsStateGroup.EXCESSIVE_CORRECTION_THRESHOLD, 0f, 100f, (Action<MyGuiControlSlider>) (slider => (MyCharacterPhysicsStateGroup.EXCESSIVE_CORRECTION_THRESHOLD = slider.Value)), color);
            base.AddLabel("Support", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.AddSlider("Change delay [ms]", (float) MyCharacterPhysicsStateGroup.ParentChangeTimeOut.Milliseconds, 0f, 5000f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<double>(x => new Action<double>(MyMultiplayerBase.OnCharacterParentChangeTimeOut), (double) slider.Value, targetEndpoint, position);
            }, color);
            base.AddLabel("Jetpack Connect", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.m_maxJetpackGridDistanceSlider = base.AddSlider("Max distance [m]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance, 0f, 1000f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridDistance), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance = slider.Value;
                this.m_maxDisconnectDistanceSlider.Value = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance, slider.Value);
            }, color);
            color = null;
            this.m_maxJetpackGridAccelerationSlider = base.AddSlider("Max acceleration [m/s^2]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration, 0f, 1000f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridAcceleration), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration = slider.Value;
                this.m_maxJetpackDisconnectGridAccelerationSlider.Value = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration, slider.Value);
            }, color);
            color = null;
            this.m_minJetpackGridSpeedSlider = base.AddSlider("Min speed [m/s]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed, 0f, 100f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackGridSpeed), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed = slider.Value;
                this.m_minJetpackDisconnectGridSpeedSlider.Value = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed, slider.Value);
            }, color);
            color = null;
            this.m_minJetpackInsideGridSpeedSlider = base.AddSlider("Min inside speed [m/s]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed, 0f, 100f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackInsideGridSpeed), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed = slider.Value;
                this.m_minJetpackDisconnectInsideGridSpeedSlider.Value = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed, slider.Value);
            }, color);
            base.AddLabel("Jetpack Disconnect", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.m_maxDisconnectDistanceSlider = base.AddSlider("Max distance [m]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance, 0f, 1000f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridDisconnectDistance), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDisconnectDistance = slider.Value;
                this.m_maxJetpackGridDistanceSlider.Value = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentDistance, slider.Value);
            }, color);
            color = null;
            this.m_maxJetpackDisconnectGridAccelerationSlider = base.AddSlider("Max acceleration [m/s^2]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration, 0f, 1000f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackDisconnectGridAcceleration), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxDisconnectParentAcceleration = slider.Value;
                this.m_maxJetpackGridAccelerationSlider.Value = Math.Min(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MaxParentAcceleration, slider.Value);
            }, color);
            color = null;
            this.m_minJetpackDisconnectGridSpeedSlider = base.AddSlider("Min speed [m/s]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed, 0f, 100f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackDisconnectGridSpeed), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectParentSpeed = slider.Value;
                this.m_minJetpackGridSpeedSlider.Value = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinParentSpeed, slider.Value);
            }, color);
            color = null;
            this.m_minJetpackDisconnectInsideGridSpeedSlider = base.AddSlider("Min inside speed [m/s]", MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed, 0f, 100f, delegate (MyGuiControlSlider slider) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<float>(x => new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackDisconnectInsideGridSpeed), slider.Value, targetEndpoint, position);
                MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinDisconnectInsideParentSpeed = slider.Value;
                this.m_minJetpackInsideGridSpeedSlider.Value = Math.Max(MyCharacterPhysicsStateGroup.JetpackParentingSetup.MinInsideParentSpeed, slider.Value);
            }, color);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugNetworkCharacter.<>c <>9 = new MyGuiScreenDebugNetworkCharacter.<>c();
            public static Action<MyGuiControlSlider> <>9__9_0;
            public static Func<IMyEventOwner, Action<double>> <>9__9_10;
            public static Action<MyGuiControlSlider> <>9__9_1;
            public static Func<IMyEventOwner, Action<float>> <>9__9_11;
            public static Func<IMyEventOwner, Action<float>> <>9__9_12;
            public static Func<IMyEventOwner, Action<float>> <>9__9_13;
            public static Func<IMyEventOwner, Action<float>> <>9__9_14;
            public static Func<IMyEventOwner, Action<float>> <>9__9_15;
            public static Func<IMyEventOwner, Action<float>> <>9__9_16;
            public static Func<IMyEventOwner, Action<float>> <>9__9_17;
            public static Func<IMyEventOwner, Action<float>> <>9__9_18;

            internal void <RecreateControls>b__9_0(MyGuiControlSlider slider)
            {
                MyCharacterPhysicsStateGroup.EXCESSIVE_CORRECTION_THRESHOLD = slider.Value;
            }

            internal void <RecreateControls>b__9_1(MyGuiControlSlider slider)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<double>(x => new Action<double>(MyMultiplayerBase.OnCharacterParentChangeTimeOut), (double) slider.Value, targetEndpoint, position);
            }

            internal Action<double> <RecreateControls>b__9_10(IMyEventOwner x) => 
                new Action<double>(MyMultiplayerBase.OnCharacterParentChangeTimeOut);

            internal Action<float> <RecreateControls>b__9_11(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridDistance);

            internal Action<float> <RecreateControls>b__9_12(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridAcceleration);

            internal Action<float> <RecreateControls>b__9_13(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackGridSpeed);

            internal Action<float> <RecreateControls>b__9_14(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackInsideGridSpeed);

            internal Action<float> <RecreateControls>b__9_15(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackGridDisconnectDistance);

            internal Action<float> <RecreateControls>b__9_16(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMaxJetpackDisconnectGridAcceleration);

            internal Action<float> <RecreateControls>b__9_17(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackDisconnectGridSpeed);

            internal Action<float> <RecreateControls>b__9_18(IMyEventOwner x) => 
                new Action<float>(MyMultiplayerBase.OnCharacterMinJetpackDisconnectInsideGridSpeed);
        }
    }
}

