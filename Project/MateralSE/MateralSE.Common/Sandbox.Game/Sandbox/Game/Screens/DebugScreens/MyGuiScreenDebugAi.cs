namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Library.Utils;
    using VRageMath;
    using VRageRender.Utils;

    [MyDebugScreen("Game", "AI")]
    internal class MyGuiScreenDebugAi : MyGuiScreenDebugBase
    {
        private int m_ctr;

        public MyGuiScreenDebugAi() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugAi";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Debug screen AI", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? textColor = null;
            captionOffset = null;
            base.AddButton("Test Chatbot", x => this.TestChatbot(), null, textColor, captionOffset);
            base.AddLabel("Options:", Color.OrangeRed.ToVector4(), 1f, null, "Debug");
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Spawn barbarians near the player", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.BARBARIANS_SPAWN_NEAR_PLAYER)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Remove voxel navmesh cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.REMOVE_VOXEL_NAVMESH_CELLS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Debug draw bots", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_BOTS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    * Bot steering", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_BOT_STEERING)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    * Bot aiming", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_BOT_AIMING)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    * Bot navigation", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_BOT_NAVIGATION)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] += 0.01f;
            base.AddLabel("Navmesh debug draw:", Color.OrangeRed.ToVector4(), 1f, null, "Debug");
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw found path", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_FOUND_PATH)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw funnel path refining", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_FUNNEL)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr9 = (float*) ref base.m_currentPosition.Y;
            singlePtr9[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Processed voxel navmesh cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_PROCESSED_VOXEL_CELLS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr10 = (float*) ref base.m_currentPosition.Y;
            singlePtr10[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Prepared voxel navmesh cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_PREPARED_VOXEL_CELLS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr11 = (float*) ref base.m_currentPosition.Y;
            singlePtr11[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Cells on paths", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_CELLS_ON_PATHS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr12 = (float*) ref base.m_currentPosition.Y;
            singlePtr12[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Voxel navmesh connection helper", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_VOXEL_CONNECTION_HELPER)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr13 = (float*) ref base.m_currentPosition.Y;
            singlePtr13[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw navmesh links", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_LINKS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr14 = (float*) ref base.m_currentPosition.Y;
            singlePtr14[0] -= 0.005f;
            float* singlePtr15 = (float*) ref base.m_currentPosition.Y;
            singlePtr15[0] += 0.01f;
            base.AddLabel("Hierarchical pathfinding:", Color.OrangeRed.ToVector4(), 1f, null, "Debug");
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Navmesh cell borders", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_CELL_BORDERS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr16 = (float*) ref base.m_currentPosition.Y;
            singlePtr16[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("HPF (draw navmesh hierarchy)", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr17 = (float*) ref base.m_currentPosition.Y;
            singlePtr17[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    * (Lite version)", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_HIERARCHY_LITE)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr18 = (float*) ref base.m_currentPosition.Y;
            singlePtr18[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    + Explored HL cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_EXPLORED_HL_CELLS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr19 = (float*) ref base.m_currentPosition.Y;
            singlePtr19[0] -= 0.005f;
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("    + Fringe HL cells", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.DEBUG_DRAW_NAVMESH_FRINGE_HL_CELLS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            float* singlePtr20 = (float*) ref base.m_currentPosition.Y;
            singlePtr20[0] -= 0.005f;
            float* singlePtr21 = (float*) ref base.m_currentPosition.Y;
            singlePtr21[0] += 0.01f;
            base.AddLabel("Winged-edge mesh debug draw:", Color.OrangeRed.ToVector4(), 1f, null, "Debug");
            Vector2 currentPosition = base.m_currentPosition;
            textColor = null;
            this.AddCheckBox("    Lines", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.LINES) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.LINES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.LINES))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition = currentPosition + new Vector2(0.15f, 0f);
            textColor = null;
            this.AddCheckBox("    Lines Z-culled", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.LINES_DEPTH) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.LINES_DEPTH) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.LINES_DEPTH))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition.X = currentPosition.X;
            currentPosition = base.m_currentPosition;
            textColor = null;
            this.AddCheckBox("    Edges", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.EDGES) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.EDGES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.EDGES))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition = currentPosition + new Vector2(0.15f, 0f);
            textColor = null;
            this.AddCheckBox("    Faces", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.FACES) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.FACES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.FACES))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition.X = currentPosition.X;
            currentPosition = base.m_currentPosition;
            textColor = null;
            this.AddCheckBox("    Vertices", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.VERTICES) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.VERTICES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.VERTICES))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition = currentPosition + new Vector2(0.15f, 0f);
            textColor = null;
            this.AddCheckBox("    Vertices detailed", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.VERTICES_DETAILED) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.VERTICES_DETAILED) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.VERTICES_DETAILED))), true, null, textColor, new Vector2(-0.15f, 0f));
            base.m_currentPosition.X = currentPosition.X;
            currentPosition = base.m_currentPosition;
            textColor = null;
            this.AddCheckBox("    Normals", (Func<bool>) (() => ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.NORMALS) != MyWEMDebugDrawMode.NONE)), (Action<bool>) (b => (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.NORMALS) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.NORMALS))), true, null, textColor, new Vector2(-0.15f, 0f));
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Animals", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_ANIMALS)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddCheckBox("Spiders", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_FAUNA_COMPONENT)), Array.Empty<ParameterExpression>())), true, null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            this.AddCheckBox("Switch Survival/Creative", (Func<bool>) (() => MySession.Static.CreativeMode), (Action<bool>) (b => (MySession.Static.Settings.GameMode = b ? MyGameModeEnum.Creative : MyGameModeEnum.Survival)), true, null, textColor, captionOffset);
        }

        private void TestChatbot()
        {
            StreamReader reader = new StreamReader(@"c:\x\stats.log");
            StreamWriter outF = new StreamWriter(@"c:\x\stats_resp.csv", false);
            this.m_ctr = 0;
            int num = 0;
            MyTimeSpan span1 = new MyTimeSpan(Stopwatch.GetTimestamp());
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                MySession.Static.ChatBot.FilterMessage("? " + line, delegate (string x) {
                    string[] textArray1 = new string[] { "\"", line, "\", \"", x, "\"" };
                    outF.WriteLine(string.Concat(textArray1));
                    this.m_ctr--;
                });
                MySandboxGame.Static.ProcessInvoke();
                this.m_ctr++;
                num++;
            }
            while (this.m_ctr != 0)
            {
                MySandboxGame.Static.ProcessInvoke();
            }
            reader.Close();
            outF.Close();
            MyTimeSpan span2 = new MyTimeSpan(Stopwatch.GetTimestamp());
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugAi.<>c <>9 = new MyGuiScreenDebugAi.<>c();
            public static Func<bool> <>9__2_19;
            public static Action<bool> <>9__2_20;
            public static Func<bool> <>9__2_21;
            public static Action<bool> <>9__2_22;
            public static Func<bool> <>9__2_23;
            public static Action<bool> <>9__2_24;
            public static Func<bool> <>9__2_25;
            public static Action<bool> <>9__2_26;
            public static Func<bool> <>9__2_27;
            public static Action<bool> <>9__2_28;
            public static Func<bool> <>9__2_29;
            public static Action<bool> <>9__2_30;
            public static Func<bool> <>9__2_31;
            public static Action<bool> <>9__2_32;
            public static Func<bool> <>9__2_35;
            public static Action<bool> <>9__2_36;

            internal bool <RecreateControls>b__2_19() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.LINES) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_20(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.LINES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.LINES);
            }

            internal bool <RecreateControls>b__2_21() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.LINES_DEPTH) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_22(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.LINES_DEPTH) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.LINES_DEPTH);
            }

            internal bool <RecreateControls>b__2_23() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.EDGES) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_24(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.EDGES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.EDGES);
            }

            internal bool <RecreateControls>b__2_25() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.FACES) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_26(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.FACES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.FACES);
            }

            internal bool <RecreateControls>b__2_27() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.VERTICES) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_28(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.VERTICES) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.VERTICES);
            }

            internal bool <RecreateControls>b__2_29() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.VERTICES_DETAILED) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_30(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.VERTICES_DETAILED) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.VERTICES_DETAILED);
            }

            internal bool <RecreateControls>b__2_31() => 
                ((MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & MyWEMDebugDrawMode.NORMALS) != MyWEMDebugDrawMode.NONE);

            internal void <RecreateControls>b__2_32(bool b)
            {
                MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES = b ? (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES | MyWEMDebugDrawMode.NORMALS) : (MyDebugDrawSettings.DEBUG_DRAW_NAVMESHES & ~MyWEMDebugDrawMode.NORMALS);
            }

            internal bool <RecreateControls>b__2_35() => 
                MySession.Static.CreativeMode;

            internal void <RecreateControls>b__2_36(bool b)
            {
                MySession.Static.Settings.GameMode = b ? MyGameModeEnum.Creative : MyGameModeEnum.Survival;
            }
        }
    }
}

