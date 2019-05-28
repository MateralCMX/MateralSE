namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems.Electricity;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Network;
    using VRageMath;

    [MyDebugScreen("Game", "Gameplay"), StaticEventOwner]
    internal class MyGuiScreenDebugGameplay : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugGameplay() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugGameplay";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Gameplay", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f * base.m_scale;
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Debris enabled", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_DEBRIS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Drill rocks enabled", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_DRILL_ROCKS)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            color = null;
            this.AddSlider("Battery Depletion Multiplier", 0f, 100f, () => MyBattery.BATTERY_DEPLETION_MULTIPLIER, new Action<float>(MyGuiScreenDebugGameplay.SetDepletionMultiplierLocal), color).DefaultValue = 1f;
            color = null;
            this.AddSlider("Reactor Fuel Consumption Multiplier", 0f, 100f, () => MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER, new Action<float>(MyGuiScreenDebugGameplay.SetFuelConsumptionMultiplierLocal), color).DefaultValue = 1f;
        }

        [Event(null, 0x3a), Reliable, Server]
        private static void SetDepletionMultiplier(float multiplier)
        {
            MyBattery.BATTERY_DEPLETION_MULTIPLIER = multiplier;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<float>(s => new Action<float>(MyGuiScreenDebugGameplay.SetDepletionMultiplierSuccess), MyBattery.BATTERY_DEPLETION_MULTIPLIER, targetEndpoint, position);
        }

        private static void SetDepletionMultiplierLocal(float multiplier)
        {
            MyBattery.BATTERY_DEPLETION_MULTIPLIER = multiplier;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<float>(s => new Action<float>(MyGuiScreenDebugGameplay.SetDepletionMultiplier), MyBattery.BATTERY_DEPLETION_MULTIPLIER, targetEndpoint, position);
        }

        [Event(null, 0x42), Reliable, ServerInvoked, Broadcast]
        private static void SetDepletionMultiplierSuccess(float multiplier)
        {
            MyBattery.BATTERY_DEPLETION_MULTIPLIER = multiplier;
        }

        [Event(null, 0x4f), Reliable, Server]
        private static void SetFuelConsumptionMultiplier(float multiplier)
        {
            MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER = multiplier;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<float>(s => new Action<float>(MyGuiScreenDebugGameplay.SetFuelConsumptionMultiplierSuccess), MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER, targetEndpoint, position);
        }

        private static void SetFuelConsumptionMultiplierLocal(float multiplier)
        {
            MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER = multiplier;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<float>(s => new Action<float>(MyGuiScreenDebugGameplay.SetFuelConsumptionMultiplier), MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER, targetEndpoint, position);
        }

        [Event(null, 0x57), Reliable, ServerInvoked, Broadcast]
        private static void SetFuelConsumptionMultiplierSuccess(float multiplier)
        {
            MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER = multiplier;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugGameplay.<>c <>9 = new MyGuiScreenDebugGameplay.<>c();
            public static Func<float> <>9__1_2;
            public static Func<float> <>9__1_3;
            public static Func<IMyEventOwner, Action<float>> <>9__3_0;
            public static Func<IMyEventOwner, Action<float>> <>9__4_0;
            public static Func<IMyEventOwner, Action<float>> <>9__6_0;
            public static Func<IMyEventOwner, Action<float>> <>9__7_0;

            internal float <RecreateControls>b__1_2() => 
                MyBattery.BATTERY_DEPLETION_MULTIPLIER;

            internal float <RecreateControls>b__1_3() => 
                MyFueledPowerProducer.FUEL_CONSUMPTION_MULTIPLIER;

            internal Action<float> <SetDepletionMultiplier>b__4_0(IMyEventOwner s) => 
                new Action<float>(MyGuiScreenDebugGameplay.SetDepletionMultiplierSuccess);

            internal Action<float> <SetDepletionMultiplierLocal>b__3_0(IMyEventOwner s) => 
                new Action<float>(MyGuiScreenDebugGameplay.SetDepletionMultiplier);

            internal Action<float> <SetFuelConsumptionMultiplier>b__7_0(IMyEventOwner s) => 
                new Action<float>(MyGuiScreenDebugGameplay.SetFuelConsumptionMultiplierSuccess);

            internal Action<float> <SetFuelConsumptionMultiplierLocal>b__6_0(IMyEventOwner s) => 
                new Action<float>(MyGuiScreenDebugGameplay.SetFuelConsumptionMultiplier);
        }
    }
}

