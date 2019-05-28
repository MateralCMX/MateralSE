namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x7d0)]
    public class MySessionComponentIngameHelp : MySessionComponentBase
    {
        public static float DEFAULT_INITIAL_DELAY = 5f;
        public static float DEFAULT_OBJECTIVE_DELAY = 5f;
        public static float TIMEOUT_DELAY = 120f;
        public static readonly HashSet<string> EssentialObjectiveIds = new HashSet<string>();
        private static List<ObjectiveDescription> m_objectiveDescriptions = new List<ObjectiveDescription>();
        private Dictionary<MyStringHash, MyIngameHelpObjective> m_availableObjectives = new Dictionary<MyStringHash, MyIngameHelpObjective>();
        private MyIngameHelpObjective m_currentObjective;
        private MyIngameHelpObjective m_nextObjective;
        private float m_currentDelayCounter = DEFAULT_INITIAL_DELAY;
        private float m_currentTimeoutCounter = TIMEOUT_DELAY;
        private float m_timeSinceLastObjective;
        private bool m_hintsEnabled = true;
        private MyCueId m_newObjectiveCue = MySoundPair.GetCueId("HudGPSNotification3");
        private MyCueId m_detailFinishedCue = MySoundPair.GetCueId("HudGPSNotification2");
        private MyCueId m_objectiveFinishedCue = MySoundPair.GetCueId("HudGPSNotification1");

        static MySessionComponentIngameHelp()
        {
            EssentialObjectiveIds.Add("IngameHelp_Movement");
            EssentialObjectiveIds.Add("IngameHelp_Camera");
            EssentialObjectiveIds.Add("IngameHelp_Intro");
            EssentialObjectiveIds.Add("IngameHelp_Jetpack");
            RegisterFromAssembly(Assembly.GetAssembly(typeof(MySessionComponentIngameHelp)));
        }

        private void CancelObjective()
        {
            this.m_currentObjective = null;
            this.m_timeSinceLastObjective = 0f;
            MyHud.Questlog.Visible = false;
        }

        private void FinishObjective()
        {
            MySandboxGame.Config.TutorialsFinished.Add(this.m_currentObjective.Id);
            MySandboxGame.Config.Save();
            this.m_availableObjectives.Remove(MyStringHash.GetOrCompute(this.m_currentObjective.Id));
            MyIngameHelpObjective objective = null;
            MyAudio.Static.PlaySound(this.m_objectiveFinishedCue, null, MySoundDimensions.D2, false, false);
            if (!string.IsNullOrEmpty(this.m_currentObjective.FollowingId) && this.m_availableObjectives.TryGetValue(MyStringHash.GetOrCompute(this.m_currentObjective.FollowingId), out objective))
            {
                this.m_nextObjective = objective;
            }
            this.m_currentObjective = null;
            this.m_timeSinceLastObjective = 0f;
        }

        internal static IReadOnlyList<MyIngameHelpObjective> GetFinishedObjectives()
        {
            List<MyIngameHelpObjective> list = new List<MyIngameHelpObjective>();
            using (List<string>.Enumerator enumerator = MySandboxGame.Config.TutorialsFinished.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ObjectiveDescription description = m_objectiveDescriptions.FirstOrDefault<ObjectiveDescription>(delegate (ObjectiveDescription x) {
                        string id;
                        return x.Id == id;
                    });
                    if (!string.IsNullOrEmpty(description.Id))
                    {
                        MyIngameHelpObjective item = (MyIngameHelpObjective) Activator.CreateInstance(description.Type);
                        item.Id = description.Id;
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        private void Init()
        {
            this.m_currentObjective = null;
            this.m_currentDelayCounter = DEFAULT_INITIAL_DELAY;
            this.m_availableObjectives.Clear();
            foreach (ObjectiveDescription description in m_objectiveDescriptions)
            {
                if (!MySandboxGame.Config.TutorialsFinished.Contains(description.Id))
                {
                    MyIngameHelpObjective objective = (MyIngameHelpObjective) Activator.CreateInstance(description.Type);
                    objective.Id = description.Id;
                    this.m_availableObjectives.Add(MyStringHash.GetOrCompute(objective.Id), objective);
                }
            }
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            this.m_hintsEnabled = MySandboxGame.Config.GoodBotHints;
            this.Init();
        }

        public static void RegisterFromAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                int index = 0;
                while (index < types.Length)
                {
                    Type type = types[index];
                    object[] customAttributes = type.GetCustomAttributes(typeof(IngameObjectiveAttribute), false);
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= customAttributes.Length)
                        {
                            index++;
                            break;
                        }
                        IngameObjectiveAttribute attribute = (IngameObjectiveAttribute) customAttributes[num2];
                        m_objectiveDescriptions.Add(new ObjectiveDescription(attribute.Id, type, attribute.Priority));
                        num2++;
                    }
                }
                m_objectiveDescriptions.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            }
        }

        public void Reset()
        {
            MySandboxGame.Config.TutorialsFinished.Clear();
            MySandboxGame.Config.Save();
            this.Init();
        }

        private void SetObjective(MyIngameHelpObjective objective)
        {
            MyAudio.Static.PlaySound(this.m_newObjectiveCue, null, MySoundDimensions.D2, false, false);
            MyHud.Questlog.CleanDetails();
            MyHud.Questlog.Visible = true;
            MyHud.Questlog.QuestTitle = MyTexts.GetString(objective.TitleEnum);
            foreach (MyIngameHelpDetail detail in objective.Details)
            {
                string str = (detail.Args == null) ? MyTexts.GetString(detail.TextEnum) : string.Format(MyTexts.GetString(detail.TextEnum), detail.Args);
                MyHud.Questlog.AddDetail(str, true, detail.FinishCondition != null);
            }
            this.m_currentDelayCounter = objective.DelayToHide;
            this.m_currentObjective = objective;
            this.m_currentTimeoutCounter = TIMEOUT_DELAY;
            this.m_currentObjective.OnActivated();
        }

        public bool TryCancelObjective()
        {
            this.m_currentDelayCounter = 0f;
            if (this.m_currentObjective == null)
            {
                return false;
            }
            this.CancelObjective();
            this.m_currentDelayCounter = 0f;
            return true;
        }

        private MyIngameHelpObjective TryToFindObjective(bool onlyCritical = false)
        {
            Dictionary<MyStringHash, MyIngameHelpObjective>.ValueCollection.Enumerator enumerator;
            MyIngameHelpObjective objective2;
            if (!onlyCritical)
            {
                if (this.m_nextObjective != null)
                {
                    this.m_nextObjective = null;
                    return this.m_nextObjective;
                }
                using (enumerator = this.m_availableObjectives.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyIngameHelpObjective current = enumerator.Current;
                        bool flag2 = true;
                        if (current.RequiredIds != null)
                        {
                            foreach (string str2 in current.RequiredIds)
                            {
                                if (this.m_availableObjectives.ContainsKey(MyStringHash.GetOrCompute(str2)))
                                {
                                    flag2 = false;
                                    break;
                                }
                            }
                        }
                        if ((current.DelayToAppear <= 0f) || (this.m_timeSinceLastObjective < current.DelayToAppear))
                        {
                            if (current.RequiredCondition != null)
                            {
                                flag2 &= current.RequiredCondition();
                            }
                            else if (current.DelayToAppear > 0f)
                            {
                                flag2 = false;
                            }
                        }
                        if (flag2)
                        {
                            return current;
                        }
                    }
                }
                return null;
            }
            else
            {
                using (enumerator = this.m_availableObjectives.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            MyIngameHelpObjective current = enumerator.Current;
                            bool flag = true;
                            if (current.RequiredIds != null)
                            {
                                foreach (string str in current.RequiredIds)
                                {
                                    if (this.m_availableObjectives.ContainsKey(MyStringHash.GetOrCompute(str)))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            if (!flag || !current.IsCritical())
                            {
                                continue;
                            }
                            objective2 = current;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    }
                    return objective2;
                }
            }
            return objective2;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_hintsEnabled != MySandboxGame.Config.GoodBotHints)
            {
                this.m_hintsEnabled = MySandboxGame.Config.GoodBotHints;
                if (!this.m_hintsEnabled)
                {
                    this.m_currentObjective = null;
                    MyHud.Questlog.CleanDetails();
                    MyHud.Questlog.Visible = false;
                    return;
                }
            }
            if ((((this.m_hintsEnabled && (MySession.Static != null)) && MySession.Static.Ready) && MySession.Static.Settings.EnableGoodBotHints) && ((!MyHud.Questlog.Visible || (this.m_currentObjective != null)) || (this.m_currentDelayCounter > 0f)))
            {
                if (MyGuiScreenGamePlay.ActiveGameplayScreen == null)
                {
                    if ((this.m_availableObjectives.Count > 0) && ((this.m_currentObjective == null) || !this.m_currentObjective.IsCritical()))
                    {
                        MyIngameHelpObjective objective = this.TryToFindObjective(true);
                        if (objective != null)
                        {
                            if (this.m_currentObjective != null)
                            {
                                this.CancelObjective();
                            }
                            this.SetObjective(objective);
                            return;
                        }
                    }
                    if ((this.m_currentObjective == null) && (this.m_currentDelayCounter > 0f))
                    {
                        this.m_currentDelayCounter -= 0.01666667f;
                        if (this.m_currentDelayCounter < 0f)
                        {
                            this.m_currentDelayCounter = 0f;
                            MyHud.Questlog.Visible = false;
                        }
                        return;
                    }
                }
                if (MyGuiScreenGamePlay.ActiveGameplayScreen == null)
                {
                    if ((this.m_currentObjective != null) && (this.m_currentTimeoutCounter > 0f))
                    {
                        this.m_currentTimeoutCounter -= 0.01666667f;
                        if (this.m_currentTimeoutCounter <= 0f)
                        {
                            this.m_currentTimeoutCounter = 0f;
                            this.m_currentDelayCounter = (float) TimeSpan.FromMinutes(5.0).TotalSeconds;
                            this.CancelObjective();
                            return;
                        }
                    }
                    if ((this.m_currentObjective == null) && (this.m_availableObjectives.Count > 0))
                    {
                        MyIngameHelpObjective objective = this.TryToFindObjective(false);
                        if (objective != null)
                        {
                            this.SetObjective(objective);
                        }
                    }
                }
                if ((this.m_currentObjective == null) || (MyScreenManager.FocusedControl != null))
                {
                    this.m_timeSinceLastObjective += 0.01666667f;
                }
                else
                {
                    bool flag = true;
                    int id = 0;
                    MyIngameHelpDetail[] details = this.m_currentObjective.Details;
                    int index = 0;
                    while (true)
                    {
                        if (index >= details.Length)
                        {
                            if (!flag)
                            {
                                break;
                            }
                            this.FinishObjective();
                            return;
                        }
                        MyIngameHelpDetail detail = details[index];
                        if (detail.FinishCondition != null)
                        {
                            bool flag2 = detail.FinishCondition();
                            if (flag2 && !MyHud.Questlog.IsCompleted(id))
                            {
                                MyGuiAudio.PlaySound(MyGuiSounds.HudObjectiveComplete);
                                MyHud.Questlog.SetCompleted(id, true);
                            }
                            flag &= flag2;
                        }
                        id++;
                        index++;
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentIngameHelp.<>c <>9 = new MySessionComponentIngameHelp.<>c();
            public static Comparison<MySessionComponentIngameHelp.ObjectiveDescription> <>9__6_0;

            internal int <RegisterFromAssembly>b__6_0(MySessionComponentIngameHelp.ObjectiveDescription x, MySessionComponentIngameHelp.ObjectiveDescription y) => 
                x.Priority.CompareTo(y.Priority);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ObjectiveDescription
        {
            public string Id;
            public System.Type Type;
            public int Priority;
            public ObjectiveDescription(string id, System.Type type, int priority)
            {
                this.Id = id;
                this.Type = type;
                this.Priority = priority;
            }

            public override string ToString() => 
                this.Id;
        }
    }
}

