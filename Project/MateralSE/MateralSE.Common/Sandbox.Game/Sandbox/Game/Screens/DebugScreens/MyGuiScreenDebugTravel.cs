namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRageMath;

    [MyDebugScreen("Game", "Travel")]
    internal class MyGuiScreenDebugTravel : MyGuiScreenDebugBase
    {
        private static Dictionary<string, Vector3> s_travelPoints;

        static MyGuiScreenDebugTravel()
        {
            Dictionary<string, Vector3> dictionary1 = new Dictionary<string, Vector3>();
            dictionary1.Add("Mercury", new Vector3(-39f, 0f, 46f));
            dictionary1.Add("Venus", new Vector3(-2f, 0f, 108f));
            dictionary1.Add("Earth", new Vector3(101f, 0f, -111f));
            dictionary1.Add("Moon", new Vector3(101f, 0f, -111f) + new Vector3(-0.015f, 0f, -0.2f));
            dictionary1.Add("Mars", new Vector3(-182f, 0f, 114f));
            dictionary1.Add("Jupiter", new Vector3(-778f, 0f, 155.6f));
            dictionary1.Add("Saturn", new Vector3(1120f, 0f, -840f));
            dictionary1.Add("Uranus", new Vector3(-2700f, 0f, -1500f));
            dictionary1.Add("Zero", new Vector3(0f, 0f, 0f));
            dictionary1.Add("Billion", new Vector3(1000f));
            dictionary1.Add("BillionFlat0", new Vector3(999f, 1000f, 1000f));
            dictionary1.Add("BillionFlat1", new Vector3(1001f, 1000f, 1000f));
            s_travelPoints = dictionary1;
        }

        public MyGuiScreenDebugTravel() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugDrawSettings";

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector4? nullable2;
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Travel", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            using (Dictionary<string, Vector3>.Enumerator enumerator = s_travelPoints.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, Vector3> travelPair;
                    nullable2 = null;
                    captionOffset = null;
                    base.AddButton(new StringBuilder(travelPair.Key), button => this.TravelTo(travelPair.Value), null, nullable2, captionOffset, true, true);
                }
            }
            nullable2 = null;
            captionOffset = null;
            base.AddCheckBox("Testing jumpdrives", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.TESTING_JUMPDRIVE)), Array.Empty<ParameterExpression>())), true, null, nullable2, captionOffset);
        }

        private void TravelTo(Vector3 positionInMilions)
        {
            MyMultiplayer.TeleportControlledEntity(positionInMilions * 1000000.0);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugTravel.<>c <>9 = new MyGuiScreenDebugTravel.<>c();
        }
    }
}

