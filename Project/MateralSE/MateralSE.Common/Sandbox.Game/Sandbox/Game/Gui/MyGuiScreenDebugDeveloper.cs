namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Plugins;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenDebugDeveloper : MyGuiScreenDebugBase
    {
        private static MyGuiScreenBase s_activeScreen;
        private static List<MyGuiControlCheckbox> s_groupList = new List<MyGuiControlCheckbox>();
        private static List<MyGuiControlCheckbox> s_inputList = new List<MyGuiControlCheckbox>();
        private static MyDevelopGroup s_debugDrawGroup = new MyDevelopGroup("Debug draw");
        private static MyDevelopGroup s_performanceGroup = new MyDevelopGroup("Performance");
        private static List<MyDevelopGroup> s_mainGroups;
        private static MyDevelopGroup s_activeMainGroup;
        private static MyDevelopGroup s_debugInputGroup;
        private static MyDevelopGroup s_activeDevelopGroup;
        private static SortedDictionary<string, MyDevelopGroup> s_developGroups;
        private static Dictionary<string, SortedDictionary<string, MyDevelopGroupTypes>> s_developScreenTypes;
        private static bool m_profilerEnabled;

        static MyGuiScreenDebugDeveloper()
        {
            List<MyDevelopGroup> list1 = new List<MyDevelopGroup>();
            list1.Add(s_debugDrawGroup);
            list1.Add(s_performanceGroup);
            s_mainGroups = list1;
            s_activeMainGroup = s_debugDrawGroup;
            s_debugInputGroup = new MyDevelopGroup("Debug Input");
            s_developGroups = new SortedDictionary<string, MyDevelopGroup>(new DevelopGroupComparer());
            s_developScreenTypes = new Dictionary<string, SortedDictionary<string, MyDevelopGroupTypes>>();
            m_profilerEnabled = false;
            RegisterScreensFromAssembly(Assembly.GetExecutingAssembly());
            RegisterScreensFromAssembly(MyPlugins.GameAssembly);
            RegisterScreensFromAssembly(MyPlugins.SandboxAssembly);
            RegisterScreensFromAssembly(MyPlugins.UserAssemblies);
            s_developGroups.Add(s_debugInputGroup.Name, s_debugInputGroup);
            SortedDictionary<string, MyDevelopGroup>.ValueCollection.Enumerator enumerator = s_developGroups.Values.GetEnumerator();
            enumerator.MoveNext();
            s_activeDevelopGroup = enumerator.Current;
        }

        public MyGuiScreenDebugDeveloper() : base(new Vector2(0.5f, 0.5f), new Vector2(0.35f, 1f), new VRageMath.Vector4?((VRageMath.Vector4) (0.35f * Color.Yellow.ToVector4())), true)
        {
            base.m_backgroundColor = null;
            base.EnabledBackgroundFade = true;
            base.m_backgroundFadeColor = new Color(1f, 1f, 1f, 0.2f);
            this.RecreateControls(true);
        }

        private void AddGroupBox(string text, System.Type screenType, List<MyGuiControlBase> controlGroup)
        {
            VRageMath.Vector4? color = null;
            Vector2? checkBoxOffset = null;
            MyGuiControlCheckbox item = base.AddCheckBox(text, true, (Action<MyGuiControlCheckbox>) null, true, controlGroup, color, checkBoxOffset);
            item.IsChecked = (s_activeScreen != null) && (s_activeScreen.GetType() == screenType);
            item.UserData = screenType;
            s_groupList.Add(item);
            item.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(item.IsCheckedChanged, delegate (MyGuiControlCheckbox sender) {
                System.Type userData = sender.UserData as System.Type;
                if (!sender.IsChecked)
                {
                    if ((s_activeScreen != null) && (s_activeScreen.GetType() == userData))
                    {
                        s_activeScreen.CloseScreen();
                    }
                }
                else
                {
                    foreach (MyGuiControlCheckbox checkbox in s_groupList)
                    {
                        if (!ReferenceEquals(checkbox, sender))
                        {
                            checkbox.IsChecked = false;
                        }
                    }
                    MyGuiScreenBase base1 = (MyGuiScreenBase) Activator.CreateInstance(userData);
                    MyGuiScreenBase base2 = (MyGuiScreenBase) Activator.CreateInstance(userData);
                    base2.Closed += delegate (MyGuiScreenBase source) {
                        if (ReferenceEquals(source, s_activeScreen))
                        {
                            s_activeScreen = null;
                        }
                    };
                    MyGuiScreenBase screen = base2;
                    MyGuiSandbox.AddScreen(screen);
                    s_activeScreen = screen;
                }
            });
        }

        protected void AddGroupInput(string text, MyDebugComponent component, List<MyGuiControlBase> controlGroup = null)
        {
            VRageMath.Vector4? color = null;
            Vector2? checkBoxOffset = null;
            MyGuiControlCheckbox item = base.AddCheckBox(text, component, controlGroup, color, checkBoxOffset);
            s_inputList.Add(item);
        }

        private unsafe void CreateDebugDrawControls()
        {
            VRageMath.Vector4? color = null;
            Vector2? checkBoxOffset = null;
            base.AddCheckBox("Debug draw", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Property(null, (MethodInfo) methodof(MyDebugDrawSettings.get_ENABLE_DEBUG_DRAW)), Array.Empty<ParameterExpression>())), true, s_debugDrawGroup.ControlList, color, checkBoxOffset);
            color = null;
            checkBoxOffset = null;
            base.AddCheckBox("Draw physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS)), Array.Empty<ParameterExpression>())), true, s_debugDrawGroup.ControlList, color, checkBoxOffset);
            color = null;
            checkBoxOffset = null;
            base.AddCheckBox("Audio debug draw", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_AUDIO)), Array.Empty<ParameterExpression>())), true, s_debugDrawGroup.ControlList, color, checkBoxOffset);
            color = null;
            checkBoxOffset = null;
            this.AddButton(new StringBuilder("Clear persistent"), v => MyRenderProxy.DebugClearPersistentMessages(), s_debugDrawGroup.ControlList, color, checkBoxOffset, true, true);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        private unsafe void CreatePerformanceControls()
        {
            VRageMath.Vector4? color = null;
            Vector2? checkBoxOffset = null;
            this.AddCheckBox("Profiler", (Func<bool>) (() => EnableProfiler), (Action<bool>) (v => (EnableProfiler = v)), true, s_performanceGroup.ControlList, color, checkBoxOffset);
            color = null;
            checkBoxOffset = null;
            base.AddCheckBox("Particles", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyParticlesManager.Enabled)), Array.Empty<ParameterExpression>())), true, s_performanceGroup.ControlList, color, checkBoxOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        private void EnableGroup(MyDevelopGroup group, bool enable)
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = group.ControlList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = enable;
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugDeveloper";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.F12))
            {
                this.CloseScreen();
            }
        }

        private void OnClickGroup(MyGuiControlButton sender)
        {
            this.EnableGroup(s_activeDevelopGroup, false);
            foreach (MyDevelopGroup group in s_developGroups.Values)
            {
                if (ReferenceEquals(group.GroupControl, sender))
                {
                    s_activeDevelopGroup = group;
                    break;
                }
            }
            this.EnableGroup(s_activeDevelopGroup, true);
        }

        private void OnClickMainGroup(MyGuiControlButton sender)
        {
            this.EnableGroup(s_activeMainGroup, false);
            foreach (MyDevelopGroup group in s_mainGroups)
            {
                if (ReferenceEquals(group.GroupControl, sender))
                {
                    s_activeMainGroup = group;
                    break;
                }
            }
            this.EnableGroup(s_activeMainGroup, true);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector2? nullable;
            int? nullable2;
            base.RecreateControls(constructor);
            foreach (MyDevelopGroup group in s_developGroups.Values)
            {
                if (group.ControlList.Count > 0)
                {
                    this.EnableGroup(group, false);
                    group.ControlList.Clear();
                }
            }
            foreach (MyDevelopGroup group2 in s_mainGroups)
            {
                if (group2.ControlList.Count > 0)
                {
                    this.EnableGroup(group2, false);
                    group2.ControlList.Clear();
                }
            }
            float y = -0.02f;
            base.AddCaption("Developer screen", new VRageMath.Vector4?(Color.Yellow.ToVector4()), new Vector2(0f, y), 0.8f);
            base.m_scale = 0.9f;
            base.m_closeOnEsc = true;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.03f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += y;
            float num2 = 0f;
            Vector2 vector = new Vector2(0.09f, 0.03f);
            foreach (MyDevelopGroup group3 in s_mainGroups)
            {
                Vector2 vector2 = new Vector2((-0.03f + base.m_currentPosition.X) + num2, base.m_currentPosition.Y);
                nullable = null;
                nullable2 = null;
                group3.GroupControl = new MyGuiControlButton(new Vector2?(vector2), MyGuiControlButtonStyleEnum.Debug, nullable, new VRageMath.Vector4(1f, 1f, 0.5f, 1f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, new StringBuilder(group3.Name), ((MyGuiConstants.DEBUG_BUTTON_TEXT_SCALE * MyGuiConstants.DEBUG_LABEL_TEXT_SCALE) * base.m_scale) * 1.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnClickMainGroup), GuiSounds.MouseClick, 1f, nullable2, false);
                num2 += group3.GroupControl.Size.X * 1.1f;
                this.Controls.Add(group3.GroupControl);
            }
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += vector.Y * 1.1f;
            float num3 = base.m_currentPosition.Y;
            float num4 = num3;
            this.CreateDebugDrawControls();
            num4 = MathHelper.Max(num4, base.m_currentPosition.Y);
            base.m_currentPosition.Y = num3;
            this.CreatePerformanceControls();
            base.m_currentPosition.Y = MathHelper.Max(num4, base.m_currentPosition.Y);
            foreach (MyDevelopGroup group4 in s_mainGroups)
            {
                this.EnableGroup(group4, false);
            }
            this.EnableGroup(s_activeMainGroup, true);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.02f;
            num2 = 0f;
            foreach (MyDevelopGroup group5 in s_developGroups.Values)
            {
                Vector2 vector3 = new Vector2((-0.03f + base.m_currentPosition.X) + num2, base.m_currentPosition.Y);
                nullable = null;
                nullable2 = null;
                group5.GroupControl = new MyGuiControlButton(new Vector2?(vector3), MyGuiControlButtonStyleEnum.Debug, nullable, new VRageMath.Vector4(1f, 1f, 0.5f, 1f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, new StringBuilder(group5.Name), ((0.8f * MyGuiConstants.DEBUG_BUTTON_TEXT_SCALE) * base.m_scale) * 1.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnClickGroup), GuiSounds.MouseClick, 1f, nullable2, false);
                num2 += group5.GroupControl.Size.X * 1.1f;
                this.Controls.Add(group5.GroupControl);
            }
            num2 = -num2 / 2f;
            foreach (MyDevelopGroup group6 in s_developGroups.Values)
            {
                group6.GroupControl.PositionX = num2;
                num2 += group6.GroupControl.Size.X * 1.1f;
            }
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += vector.Y * 1.1f;
            float num5 = base.m_currentPosition.Y;
            bool flag = MySandboxGame.Config.GraphicsRenderer.ToString() == MySandboxGame.DirectX11RendererKey.ToString();
            foreach (KeyValuePair<string, SortedDictionary<string, MyDevelopGroupTypes>> pair in s_developScreenTypes)
            {
                MyDevelopGroup group7 = s_developGroups[pair.Key];
                foreach (KeyValuePair<string, MyDevelopGroupTypes> pair2 in pair.Value)
                {
                    if (pair2.Value.DirectXSupport < MyDirectXSupport.ALL)
                    {
                        continue;
                    }
                    if (flag)
                    {
                        this.AddGroupBox(pair2.Key, pair2.Value.Name, group7.ControlList);
                    }
                }
                base.m_currentPosition.Y = num5;
            }
            if (MyGuiSandbox.Gui is MyDX9Gui)
            {
                for (int i = 0; i < (MyGuiSandbox.Gui as MyDX9Gui).UserDebugInputComponents.Count; i++)
                {
                    this.AddGroupInput($"{(MyGuiSandbox.Gui as MyDX9Gui).UserDebugInputComponents[i].GetName()} (Ctrl + numPad{i})", (MyGuiSandbox.Gui as MyDX9Gui).UserDebugInputComponents[i], s_debugInputGroup.ControlList);
                }
            }
            base.m_currentPosition.Y = num5;
            foreach (MyDevelopGroup group8 in s_developGroups.Values)
            {
                this.EnableGroup(group8, false);
            }
            this.EnableGroup(s_activeDevelopGroup, true);
        }

        private static void RegisterScreensFromAssembly(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    RegisterScreensFromAssembly(assemblyArray[i]);
                }
            }
        }

        private static void RegisterScreensFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                System.Type type = typeof(MyGuiScreenBase);
                foreach (System.Type type2 in assembly.GetTypes())
                {
                    if (type.IsAssignableFrom(type2))
                    {
                        object[] customAttributes = type2.GetCustomAttributes(typeof(MyDebugScreenAttribute), false);
                        if (customAttributes.Length != 0)
                        {
                            SortedDictionary<string, MyDevelopGroupTypes> dictionary;
                            MyDebugScreenAttribute attribute = (MyDebugScreenAttribute) customAttributes[0];
                            if (!s_developScreenTypes.TryGetValue(attribute.Group, out dictionary))
                            {
                                dictionary = new SortedDictionary<string, MyDevelopGroupTypes>();
                                s_developScreenTypes.Add(attribute.Group, dictionary);
                                s_developGroups.Add(attribute.Group, new MyDevelopGroup(attribute.Group));
                            }
                            MyDevelopGroupTypes types = new MyDevelopGroupTypes(type2, attribute.DirectXSupport);
                            dictionary.Add(attribute.Name, types);
                        }
                    }
                }
            }
        }

        private static bool EnableProfiler
        {
            get => 
                VRage.Profiler.MyRenderProfiler.ProfilerVisible;
            set
            {
                if (VRage.Profiler.MyRenderProfiler.ProfilerVisible != value)
                {
                    MyRenderProxy.RenderProfilerInput(RenderProfilerCommand.Enable, 0, null);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugDeveloper.<>c <>9 = new MyGuiScreenDebugDeveloper.<>c();
            public static Action<MyGuiControlButton> <>9__23_3;
            public static Func<bool> <>9__24_0;
            public static Action<bool> <>9__24_1;
            public static MyGuiScreenBase.ScreenHandler <>9__26_1;
            public static Action<MyGuiControlCheckbox> <>9__26_0;

            internal void <AddGroupBox>b__26_0(MyGuiControlCheckbox sender)
            {
                System.Type userData = sender.UserData as System.Type;
                if (!sender.IsChecked)
                {
                    if ((MyGuiScreenDebugDeveloper.s_activeScreen != null) && (MyGuiScreenDebugDeveloper.s_activeScreen.GetType() == userData))
                    {
                        MyGuiScreenDebugDeveloper.s_activeScreen.CloseScreen();
                    }
                }
                else
                {
                    foreach (MyGuiControlCheckbox checkbox in MyGuiScreenDebugDeveloper.s_groupList)
                    {
                        if (!ReferenceEquals(checkbox, sender))
                        {
                            checkbox.IsChecked = false;
                        }
                    }
                    MyGuiScreenBase base1 = (MyGuiScreenBase) Activator.CreateInstance(userData);
                    MyGuiScreenBase base2 = (MyGuiScreenBase) Activator.CreateInstance(userData);
                    base2.Closed += delegate (MyGuiScreenBase source) {
                        if (ReferenceEquals(source, MyGuiScreenDebugDeveloper.s_activeScreen))
                        {
                            MyGuiScreenDebugDeveloper.s_activeScreen = null;
                        }
                    };
                    MyGuiScreenBase screen = base2;
                    MyGuiSandbox.AddScreen(screen);
                    MyGuiScreenDebugDeveloper.s_activeScreen = screen;
                }
            }

            internal void <AddGroupBox>b__26_1(MyGuiScreenBase source)
            {
                if (ReferenceEquals(source, MyGuiScreenDebugDeveloper.s_activeScreen))
                {
                    MyGuiScreenDebugDeveloper.s_activeScreen = null;
                }
            }

            internal void <CreateDebugDrawControls>b__23_3(MyGuiControlButton v)
            {
                MyRenderProxy.DebugClearPersistentMessages();
            }

            internal bool <CreatePerformanceControls>b__24_0() => 
                MyGuiScreenDebugDeveloper.EnableProfiler;

            internal void <CreatePerformanceControls>b__24_1(bool v)
            {
                MyGuiScreenDebugDeveloper.EnableProfiler = v;
            }
        }

        private class DevelopGroupComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if ((x != "Game") || (y != "Game"))
                {
                    if (x == "Game")
                    {
                        return -1;
                    }
                    if (y == "Game")
                    {
                        return 1;
                    }
                    if ((x != "Render") || (y != "Render"))
                    {
                        return ((x != "Render") ? ((y != "Render") ? x.CompareTo(y) : 1) : -1);
                    }
                }
                return 0;
            }
        }

        private class MyDevelopGroup
        {
            public string Name;
            public MyGuiControlBase GroupControl;
            public List<MyGuiControlBase> ControlList;

            public MyDevelopGroup(string name)
            {
                this.Name = name;
                this.ControlList = new List<MyGuiControlBase>();
            }
        }

        private class MyDevelopGroupTypes
        {
            public System.Type Name;
            public MyDirectXSupport DirectXSupport;

            public MyDevelopGroupTypes(System.Type name, MyDirectXSupport directXSupport)
            {
                this.Name = name;
                this.DirectXSupport = directXSupport;
            }
        }
    }
}

