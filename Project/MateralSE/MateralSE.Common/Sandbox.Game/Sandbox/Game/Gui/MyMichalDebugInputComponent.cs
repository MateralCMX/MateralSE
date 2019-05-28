namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.AI.BehaviorTree;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.VoiceChat;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyMichalDebugInputComponent : MyDebugComponent
    {
        public bool CastLongRay;
        public int DebugPacketCount;
        public int CurrentQueuedBytes;
        public bool Reliable = true;
        public bool DebugDraw;
        public bool CustomGridCreation;
        public IMyBot SelectedBot;
        public int BotPointer;
        public int SelectedTreeIndex;
        public MyBehaviorTree SelectedTree;
        public int[] BotsIndexes = new int[0];
        private Dictionary<MyJoystickAxesEnum, float?> AxesCollection;
        private List<MyJoystickAxesEnum> Axes;
        public MatrixD HeadMatrix = MatrixD.Identity;
        private const int HeadMatrixFlag = 15;
        private int CurrentHeadMatrixFlag;
        public bool SPAWN_FLORA_ENTITY;
        public bool OnSelectDebugBot;
        public bool ENABLE_FLORA_SPAWNING;
        public MyFloraElementDefinition SELECTED_FLORA;
        private int SELECTED_FLORA_IDX;
        private string multiplayerStats = string.Empty;
        private Vector3D? m_lineStart;
        private Vector3D? m_lineEnd;
        private Vector3D? m_sphereCen;
        private float? m_rad;
        private List<MyAgentDefinition> m_agentDefinitions = new List<MyAgentDefinition>();
        private int? m_selectedDefinition;
        private string m_selectBotName;

        public MyMichalDebugInputComponent()
        {
            Static = this;
            this.Axes = new List<MyJoystickAxesEnum>();
            this.AxesCollection = new Dictionary<MyJoystickAxesEnum, float?>();
            foreach (MyJoystickAxesEnum enum2 in Enum.GetValues(typeof(MyJoystickAxesEnum)))
            {
                this.AxesCollection[enum2] = null;
                this.Axes.Add(enum2);
            }
            base.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Enable LMB spawning: " + this.ENABLE_FLORA_SPAWNING.ToString(), delegate {
                this.ENABLE_FLORA_SPAWNING = !this.ENABLE_FLORA_SPAWNING;
                return true;
            });
            base.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Select flora to spawn. Selected: " + this.SELECTED_FLORA, new Func<bool>(this.SelectNextFloraToSpawn));
            this.AddShortcut(MyKeys.NumPad0, true, false, false, false, () => "Debug draw", new Func<bool>(this.DebugDrawFunc));
            base.AddShortcut(MyKeys.NumPad9, true, false, false, false, new Func<string>(this.OnRecording), new Func<bool>(this.ToggleVoiceChat));
            if (MyPerGameSettings.Game == GameEnum.SE_GAME)
            {
                this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Remove grids with space balls", new Func<bool>(this.RemoveGridsWithSpaceBallsFunc));
                this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Throw 50 ores and 50 scrap metals", new Func<bool>(this.ThrowFloatingObjectsFunc));
            }
            if (MyPerGameSettings.EnableAi)
            {
                this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Next head matrix", new Func<bool>(this.NextHeadMatrix));
                this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Previous head matrix", new Func<bool>(this.PreviousHeadMatrix));
                base.AddShortcut(MyKeys.NumPad3, true, false, false, false, new Func<string>(this.OnSelectBotForDebugMsg), delegate {
                    this.OnSelectDebugBot = !this.OnSelectDebugBot;
                    return true;
                });
                this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Remove bot", delegate {
                    MyAIComponent.Static.DebugRemoveFirstBot();
                    return true;
                });
                this.AddShortcut(MyKeys.L, true, true, false, false, () => "Add animal bot", new Func<bool>(this.SpawnAnimalAroundPlayer));
                base.AddShortcut(MyKeys.OemSemicolon, true, true, false, false, () => "Spawn selected bot " + ((this.m_selectBotName != null) ? this.m_selectBotName : "NOT SELECTED"), new Func<bool>(this.SpawnBot));
                this.AddShortcut(MyKeys.OemMinus, true, true, false, false, () => "Previous bot definition", new Func<bool>(this.PreviousBot));
                this.AddShortcut(MyKeys.OemPlus, true, true, false, false, () => "Next bot definition", new Func<bool>(this.NextBot));
                this.AddShortcut(MyKeys.OemQuotes, true, true, false, false, () => "Reload bot definitions", new Func<bool>(this.ReloadDefinitions));
                this.AddShortcut(MyKeys.OemComma, true, true, false, false, () => "RemoveAllTimbers", new Func<bool>(this.RemoveAllTimbers));
                this.AddShortcut(MyKeys.N, true, true, false, false, () => "Cast long ray", new Func<bool>(this.ChangeAlgo));
            }
        }

        private bool ChangeAlgo()
        {
            this.CastLongRay = !this.CastLongRay;
            return true;
        }

        public bool DebugDrawFunc()
        {
            this.DebugDraw = !this.DebugDraw;
            return true;
        }

        public override void Draw()
        {
            base.Draw();
            if (this.DebugDraw)
            {
                if (MySession.Static.LocalCharacter != null)
                {
                    this.HeadMatrix = MySession.Static.LocalCharacter.GetHeadMatrix((this.CurrentHeadMatrixFlag & 1) == 1, (this.CurrentHeadMatrixFlag & 2) == 2, (this.CurrentHeadMatrixFlag & 4) == 4, (this.CurrentHeadMatrixFlag & 8) == 8, false);
                    MyRenderProxy.DebugDrawAxis(this.HeadMatrix, 1f, false, false, false);
                    string text = $"GetHeadMatrix({(this.CurrentHeadMatrixFlag & 1) == 1}, {(this.CurrentHeadMatrixFlag & 2) == 2}, {(this.CurrentHeadMatrixFlag & 4) == 4}, {(this.CurrentHeadMatrixFlag & 8) == 8})";
                    MyRenderProxy.DebugDrawText2D(new Vector2(600f, 20f), text, Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MatrixD worldMatrix = MySession.Static.LocalCharacter.WorldMatrix;
                    Vector3D forward = worldMatrix.Forward;
                    float single1 = MathHelper.ToRadians((float) 15f);
                    Math.Cos((double) single1);
                    Math.Sin((double) single1);
                    MatrixD matrix = MatrixD.CreateRotationY((double) single1);
                    MatrixD xd3 = MatrixD.Transpose(matrix);
                    Vector3D pointTo = worldMatrix.Translation + worldMatrix.Forward;
                    Vector3D vectord2 = worldMatrix.Translation + Vector3D.TransformNormal(worldMatrix.Forward, matrix);
                    Vector3D vectord3 = worldMatrix.Translation + Vector3D.TransformNormal(worldMatrix.Forward, xd3);
                    MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, pointTo, Color.Aqua, Color.Aqua, false, false);
                    MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, vectord2, Color.Red, Color.Red, false, false);
                    MyRenderProxy.DebugDrawLine3D(worldMatrix.Translation, vectord3, Color.Green, Color.Green, false, false);
                    if (MyToolbarComponent.CurrentToolbar != null)
                    {
                        Rectangle safeGuiRectangle = MyGuiManager.GetSafeGuiRectangle();
                        Vector2 vector1 = new Vector2((float) safeGuiRectangle.Right, safeGuiRectangle.Top + (safeGuiRectangle.Height * 0.5f));
                    }
                }
                if ((MyAIComponent.Static != null) && (MyAIComponent.Static.Bots != null))
                {
                    Vector2 vector4 = new Vector2(10f, 150f);
                    Dictionary<int, IMyBot>.KeyCollection keys = MyAIComponent.Static.Bots.BotsDictionary.Keys;
                    this.BotsIndexes = new int[keys.Count];
                    keys.CopyTo(this.BotsIndexes, 0);
                    foreach (VRage.Game.Entity.MyEntity entity in Sandbox.Game.Entities.MyEntities.GetEntities())
                    {
                        if (!(entity is MyCubeGrid))
                        {
                            continue;
                        }
                        MyCubeGrid grid = entity as MyCubeGrid;
                        if (grid.BlocksCount == 1)
                        {
                            MySlimBlock cubeBlock = grid.GetCubeBlock(new Vector3I(0, 0, 0));
                            if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                            {
                                MyRenderProxy.DebugDrawText3D(cubeBlock.FatBlock.PositionComp.GetPosition(), cubeBlock.BlockDefinition.Id.SubtypeName, Color.Aqua, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                                MyRenderProxy.DebugDrawPoint(cubeBlock.FatBlock.PositionComp.GetPosition(), Color.Aqua, false, false);
                            }
                        }
                    }
                }
                if ((this.m_lineStart != null) && (this.m_lineEnd != null))
                {
                    MyRenderProxy.DebugDrawLine3D(this.m_lineStart.Value, this.m_lineEnd.Value, Color.Red, Color.Green, true, false);
                }
                if ((this.m_sphereCen != null) && (this.m_rad != null))
                {
                    MyRenderProxy.DebugDrawSphere(this.m_sphereCen.Value, this.m_rad.Value, Color.Red, 1f, true, false, true, false);
                }
                Vector2 screenCoord = new Vector2(10f, 250f);
                Vector2 vector2 = new Vector2(0f, 10f);
                foreach (MyJoystickAxesEnum enum2 in this.Axes)
                {
                    if (this.AxesCollection[enum2] == null)
                    {
                        MyRenderProxy.DebugDrawText2D(screenCoord, enum2.ToString() + ": INVALID", Color.Aqua, 0.4f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    else
                    {
                        float? nullable = this.AxesCollection[enum2];
                        MyRenderProxy.DebugDrawText2D(screenCoord, enum2.ToString() + ": " + nullable.Value, Color.Aqua, 0.4f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    }
                    screenCoord += vector2;
                }
                MyRenderProxy.DebugDrawText2D(screenCoord, "Mouse coords: " + MyGuiManager.MouseCursorPosition.ToString(), Color.BlueViolet, 0.4f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                MyRenderProxy.DebugDrawText2D(new Vector2(0f, 450f), this.multiplayerStats, Color.Yellow, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            }
        }

        public override string GetName() => 
            "Michal";

        public override bool HandleInput()
        {
            foreach (MyJoystickAxesEnum enum2 in this.Axes)
            {
                if (MyInput.Static.IsJoystickAxisValid(enum2))
                {
                    this.AxesCollection[enum2] = new float?(MyInput.Static.GetJoystickAxisStateForGameplay(enum2));
                    continue;
                }
                this.AxesCollection[enum2] = null;
            }
            return base.HandleInput();
        }

        private bool NextBot()
        {
            if (this.m_agentDefinitions.Count != 0)
            {
                this.m_selectedDefinition = (this.m_selectedDefinition != null) ? new int?((this.m_selectedDefinition.Value + 1) % this.m_agentDefinitions.Count) : 0;
                this.m_selectBotName = this.m_agentDefinitions[this.m_selectedDefinition.Value].Id.SubtypeName;
            }
            return true;
        }

        private bool NextHeadMatrix()
        {
            this.CurrentHeadMatrixFlag++;
            if (this.CurrentHeadMatrixFlag > 15)
            {
                this.CurrentHeadMatrixFlag = 15;
            }
            if (MySession.Static.LocalCharacter != null)
            {
                this.HeadMatrix = MySession.Static.LocalCharacter.GetHeadMatrix((this.CurrentHeadMatrixFlag & 1) == 1, (this.CurrentHeadMatrixFlag & 2) == 2, (this.CurrentHeadMatrixFlag & 4) == 4, (this.CurrentHeadMatrixFlag & 8) == 8, false);
            }
            return true;
        }

        private string OnRecording() => 
            ((MyVoiceChatSessionComponent.Static == null) ? string.Format("VoIP unavailable", Array.Empty<object>()) : $"VoIP recording: {(MyVoiceChatSessionComponent.Static.IsRecording ? "TRUE" : "FALSE")}");

        private string OnSelectBotForDebugMsg() => 
            $"Auto select bot for debug: {(this.OnSelectDebugBot ? "TRUE" : "FALSE")}";

        private bool PreviousBot()
        {
            if (this.m_agentDefinitions.Count != 0)
            {
                if (this.m_selectedDefinition == null)
                {
                    this.m_selectedDefinition = new int?(this.m_agentDefinitions.Count - 1);
                }
                else
                {
                    this.m_selectedDefinition = new int?(this.m_selectedDefinition.Value - 1);
                    if (this.m_selectedDefinition.Value == -1)
                    {
                        this.m_selectedDefinition = new int?(this.m_agentDefinitions.Count - 1);
                    }
                }
                this.m_selectBotName = this.m_agentDefinitions[this.m_selectedDefinition.Value].Id.SubtypeName;
            }
            return true;
        }

        private bool PreviousHeadMatrix()
        {
            this.CurrentHeadMatrixFlag--;
            if (this.CurrentHeadMatrixFlag < 0)
            {
                this.CurrentHeadMatrixFlag = 0;
            }
            if (MySession.Static.LocalCharacter != null)
            {
                this.HeadMatrix = MySession.Static.LocalCharacter.GetHeadMatrix((this.CurrentHeadMatrixFlag & 1) == 1, (this.CurrentHeadMatrixFlag & 2) == 2, (this.CurrentHeadMatrixFlag & 4) == 4, (this.CurrentHeadMatrixFlag & 8) == 8, false);
            }
            return true;
        }

        private bool ReloadDefinitions()
        {
            this.m_selectedDefinition = null;
            this.m_selectBotName = null;
            this.m_agentDefinitions.Clear();
            foreach (MyBotDefinition definition in MyDefinitionManager.Static.GetBotDefinitions())
            {
                if (definition is MyAgentDefinition)
                {
                    this.m_agentDefinitions.Add(definition as MyAgentDefinition);
                }
            }
            return true;
        }

        private bool RemoveAllTimbers()
        {
            foreach (MyCubeBlock block in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
                if (block == null)
                {
                    continue;
                }
                if (block.BlockDefinition.Id.SubtypeName == "Timber1")
                {
                    block.Close();
                }
            }
            return true;
        }

        private bool RemoveGridsWithSpaceBallsFunc()
        {
            foreach (MyCubeGrid grid1 in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
            }
            return true;
        }

        private bool SelectNextFloraToSpawn()
        {
            ListReader<MyFloraElementDefinition> definitionsOfType = MyDefinitionManager.Static.GetDefinitionsOfType<MyFloraElementDefinition>();
            this.SELECTED_FLORA_IDX = (this.SELECTED_FLORA_IDX + 1) % definitionsOfType.Count;
            this.SELECTED_FLORA = definitionsOfType.ItemAt(this.SELECTED_FLORA_IDX);
            return true;
        }

        public void SetDebugDrawLine(Vector3D start, Vector3D end)
        {
            this.m_lineStart = new Vector3D?(start);
            this.m_lineEnd = new Vector3D?(end);
        }

        public void SetDebugSphere(Vector3D cen, float rad)
        {
            this.m_sphereCen = new Vector3D?(cen);
            this.m_rad = new float?(rad);
        }

        private bool SpawnAnimalAroundPlayer()
        {
            if (MySession.Static.LocalCharacter != null)
            {
                MySession.Static.LocalCharacter.PositionComp.GetPosition();
                MyBotDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_BotDefinition), "NormalDeer"));
                MyAIComponent.Static.SpawnNewBot(botDefinition as MyAgentDefinition);
            }
            return true;
        }

        private bool SpawnBot()
        {
            if ((MySession.Static.LocalCharacter != null) && (this.m_selectedDefinition != null))
            {
                MatrixD xd = MySession.Static.LocalCharacter.GetHeadMatrix(true, true, false, false, false);
                MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_BotDefinition), "BarbarianTest"));
                Vector3D translation = xd.Translation;
                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(translation, xd.Translation + (xd.Forward * 30.0), 0);
                if (nullable != null)
                {
                    MyAgentDefinition agentDefinition = this.m_agentDefinitions[this.m_selectedDefinition.Value];
                    MyAIComponent.Static.SpawnNewBot(agentDefinition, nullable.Value.Position, true);
                }
            }
            return true;
        }

        private unsafe bool ThrowFloatingObjectsFunc()
        {
            Matrix inv = Matrix.Invert((Matrix) MySector.MainCamera.ViewMatrix);
            MyObjectBuilder_Ore content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Stone");
            MyObjectBuilder_Ore scrapBuilder = MyFloatingObject.ScrapBuilder;
            for (int i = 1; i <= 0x19; i++)
            {
                Action<VRage.Game.Entity.MyEntity> <>9__0;
                Action<VRage.Game.Entity.MyEntity> completionCallback = <>9__0;
                if (<>9__0 == null)
                {
                    Action<VRage.Game.Entity.MyEntity> local1 = <>9__0;
                    completionCallback = <>9__0 = delegate (VRage.Game.Entity.MyEntity entity) {
                        entity.Physics.LinearVelocity = inv.Forward * 50f;
                    };
                }
                MyFloatingObjects.Spawn(new MyPhysicalInventoryItem((MyRandom.Instance.Next() % 200) + 1, content, 1f), inv.Translation + ((inv.Forward * i) * 1f), inv.Forward, inv.Up, null, completionCallback);
            }
            Vector3D translation = inv.Translation;
            double* numPtr1 = (double*) ref translation.X;
            numPtr1[0] += 10.0;
            for (int j = 1; j <= 0x19; j++)
            {
                Action<VRage.Game.Entity.MyEntity> <>9__1;
                Action<VRage.Game.Entity.MyEntity> completionCallback = <>9__1;
                if (<>9__1 == null)
                {
                    Action<VRage.Game.Entity.MyEntity> local2 = <>9__1;
                    completionCallback = <>9__1 = delegate (VRage.Game.Entity.MyEntity entity) {
                        entity.Physics.LinearVelocity = inv.Forward * 50f;
                    };
                }
                MyFloatingObjects.Spawn(new MyPhysicalInventoryItem((MyRandom.Instance.Next() % 200) + 1, scrapBuilder, 1f), translation + ((inv.Forward * j) * 1f), inv.Forward, inv.Up, null, completionCallback);
            }
            return true;
        }

        private bool ToggleVoiceChat()
        {
            if (MyVoiceChatSessionComponent.Static.IsRecording)
            {
                MyVoiceChatSessionComponent.Static.StopRecording();
            }
            else
            {
                MyVoiceChatSessionComponent.Static.StartRecording();
            }
            return true;
        }

        public override void Update10()
        {
            base.Update10();
            this.multiplayerStats = MyMultiplayer.GetMultiplayerStats();
        }

        public static MyMichalDebugInputComponent Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMichalDebugInputComponent.<>c <>9 = new MyMichalDebugInputComponent.<>c();
            public static Func<string> <>9__5_3;
            public static Func<string> <>9__5_4;
            public static Func<string> <>9__5_5;
            public static Func<string> <>9__5_6;
            public static Func<string> <>9__5_7;
            public static Func<string> <>9__5_9;
            public static Func<bool> <>9__5_10;
            public static Func<string> <>9__5_11;
            public static Func<string> <>9__5_13;
            public static Func<string> <>9__5_14;
            public static Func<string> <>9__5_15;
            public static Func<string> <>9__5_16;
            public static Func<string> <>9__5_17;

            internal bool <.ctor>b__5_10()
            {
                MyAIComponent.Static.DebugRemoveFirstBot();
                return true;
            }

            internal string <.ctor>b__5_11() => 
                "Add animal bot";

            internal string <.ctor>b__5_13() => 
                "Previous bot definition";

            internal string <.ctor>b__5_14() => 
                "Next bot definition";

            internal string <.ctor>b__5_15() => 
                "Reload bot definitions";

            internal string <.ctor>b__5_16() => 
                "RemoveAllTimbers";

            internal string <.ctor>b__5_17() => 
                "Cast long ray";

            internal string <.ctor>b__5_3() => 
                "Debug draw";

            internal string <.ctor>b__5_4() => 
                "Remove grids with space balls";

            internal string <.ctor>b__5_5() => 
                "Throw 50 ores and 50 scrap metals";

            internal string <.ctor>b__5_6() => 
                "Next head matrix";

            internal string <.ctor>b__5_7() => 
                "Previous head matrix";

            internal string <.ctor>b__5_9() => 
                "Remove bot";
        }
    }
}

