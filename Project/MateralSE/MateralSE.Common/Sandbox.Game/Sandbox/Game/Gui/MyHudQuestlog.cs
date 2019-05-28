namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Game.SessionComponents;
    using VRageMath;

    public class MyHudQuestlog
    {
        private bool m_isVisible;
        private string m_questTitle;
        private List<MultilineData> m_content = new List<MultilineData>();
        [CompilerGenerated]
        private Action ValueChanged;
        public readonly Vector2 QuestlogSize = new Vector2(0.4f, 0.22f);
        public bool HighlightChanges = true;

        public event Action ValueChanged
        {
            [CompilerGenerated] add
            {
                Action valueChanged = this.ValueChanged;
                while (true)
                {
                    Action a = valueChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    valueChanged = Interlocked.CompareExchange<Action>(ref this.ValueChanged, action3, a);
                    if (ReferenceEquals(valueChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action valueChanged = this.ValueChanged;
                while (true)
                {
                    Action source = valueChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    valueChanged = Interlocked.CompareExchange<Action>(ref this.ValueChanged, action3, source);
                    if (ReferenceEquals(valueChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public void AddDetail(string value, bool useTyping = true, bool isObjective = false)
        {
            MultilineData item = new MultilineData {
                Data = value,
                IsObjective = isObjective
            };
            if (!useTyping)
            {
                item.CharactersDisplayed = -1;
            }
            this.m_content.Add(item);
            this.RaiseValueChanged();
        }

        public void CleanDetails()
        {
            this.m_content.Clear();
            this.RaiseValueChanged();
        }

        public MyObjectBuilder_Questlog GetObjectBuilder()
        {
            MyObjectBuilder_Questlog questlog1 = new MyObjectBuilder_Questlog();
            questlog1.Title = this.QuestTitle;
            questlog1.Visible = this.Visible;
            questlog1.LineData.Capacity = this.m_content.Count;
            questlog1.LineData.AddList<MultilineData>(this.m_content);
            return questlog1;
        }

        public MultilineData[] GetQuestGetails() => 
            this.m_content.ToArray();

        public void Init()
        {
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            if (component != null)
            {
                MyObjectBuilder_Questlog questlogData = component.QuestlogData;
                if (questlogData != null)
                {
                    this.m_content.Clear();
                    this.m_content.AddList<MultilineData>(questlogData.LineData);
                    this.QuestTitle = questlogData.Title;
                    this.Visible = (questlogData.LineData.Count > 0) && questlogData.Visible;
                }
            }
        }

        public bool IsCompleted(int id) => 
            ((id < this.m_content.Count) && ((id >= 0) && this.m_content[id].Completed));

        public void ModifyDetail(int id, string value, bool useTyping = true)
        {
            if ((id < this.m_content.Count) && (id >= 0))
            {
                MultilineData data = this.m_content[id];
                data.Data = value;
                this.m_content[id] = data;
                if (!useTyping)
                {
                    this.m_content[id].CharactersDisplayed = -1;
                }
                this.RaiseValueChanged();
            }
        }

        private void RaiseValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged();
            }
        }

        public void Save()
        {
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            if (component != null)
            {
                component.QuestlogData = this.GetObjectBuilder();
            }
        }

        public bool SetAllCompleted(bool completed = true)
        {
            using (List<MultilineData>.Enumerator enumerator = this.m_content.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Completed = completed;
                }
            }
            this.RaiseValueChanged();
            return true;
        }

        public bool SetCompleted(int id, bool completed = true)
        {
            if ((id >= this.m_content.Count) || (id < 0))
            {
                return false;
            }
            if (this.m_content[id].Completed == completed)
            {
                return false;
            }
            this.m_content[id].Completed = completed;
            this.RaiseValueChanged();
            return true;
        }

        public string QuestTitle
        {
            get => 
                this.m_questTitle;
            set
            {
                this.m_questTitle = value;
                this.RaiseValueChanged();
            }
        }

        public bool Visible
        {
            get => 
                this.m_isVisible;
            set => 
                (this.m_isVisible = value);
        }
    }
}

