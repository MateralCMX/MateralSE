namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Models;
    using VRageMath;
    using VRageRender.Models;

    public class MyHudSelectedObject
    {
        [ThreadStatic]
        private static List<string> m_tmpSectionNames = new List<string>();
        [ThreadStatic]
        private static List<uint> m_tmpSubpartIds = new List<uint>();
        private bool m_highlightAttributeDirty;
        private bool m_visible;
        private uint m_visibleRenderID = uint.MaxValue;
        private string m_highlightAttribute;
        internal MyHudSelectedObjectStatus CurrentObject;
        internal MyHudSelectedObjectStatus PreviousObject;
        private Vector2 m_halfSize = (Vector2.One * 0.02f);
        private VRageMath.Color m_color = MyHudConstants.HUD_COLOR_LIGHT;
        private MyHudObjectHighlightStyle m_style;

        private bool CheckForTransition()
        {
            if ((this.CurrentObject.Instance == null) || !this.m_visible)
            {
                return false;
            }
            if (this.PreviousObject.Instance == null)
            {
                this.DoTransition();
            }
            return true;
        }

        public void Clean()
        {
            this.CurrentObject = new MyHudSelectedObjectStatus();
            this.PreviousObject = new MyHudSelectedObjectStatus();
        }

        private void ComputeHighlightIndices()
        {
            if (this.m_highlightAttributeDirty)
            {
                if (this.m_highlightAttribute == null)
                {
                    this.m_highlightAttributeDirty = false;
                }
                else
                {
                    m_tmpSectionNames.Clear();
                    m_tmpSubpartIds.Clear();
                    char[] separator = new char[] { ";"[0] };
                    string[] strArray = this.m_highlightAttribute.Split(separator);
                    MyModel model = this.CurrentObject.Instance.Owner.Render.GetModel();
                    bool flag = true;
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string str = strArray[i];
                        if (str.StartsWith("subpart_"))
                        {
                            MyEntitySubpart subpart;
                            string name = str.Substring("subpart_".Length);
                            flag = this.CurrentObject.Instance.Owner.TryGetSubpart(name, out subpart);
                            if (!flag)
                            {
                                break;
                            }
                            uint renderObjectID = subpart.Render.GetRenderObjectID();
                            if (renderObjectID != uint.MaxValue)
                            {
                                m_tmpSubpartIds.Add(renderObjectID);
                            }
                        }
                        else if (!str.StartsWith("subblock_"))
                        {
                            MyMeshSection section;
                            flag = model.TryGetMeshSection(strArray[i], out section);
                            if (!flag)
                            {
                                break;
                            }
                            m_tmpSectionNames.Add(section.Name);
                        }
                        else
                        {
                            MySlimBlock block2;
                            MyCubeBlock owner = this.CurrentObject.Instance.Owner as MyCubeBlock;
                            if (owner == null)
                            {
                                break;
                            }
                            flag = owner.TryGetSubBlock(str.Substring("subblock_".Length), out block2);
                            if (!flag)
                            {
                                break;
                            }
                            uint renderObjectID = block2.FatBlock.Render.GetRenderObjectID();
                            if (renderObjectID != uint.MaxValue)
                            {
                                m_tmpSubpartIds.Add(renderObjectID);
                            }
                        }
                    }
                    if (!flag)
                    {
                        this.CurrentObject.SectionNames = new string[0];
                        this.CurrentObject.SubpartIndices = null;
                    }
                    else
                    {
                        this.CurrentObject.SectionNames = m_tmpSectionNames.ToArray();
                        if (m_tmpSubpartIds.Count != 0)
                        {
                            this.CurrentObject.SubpartIndices = m_tmpSubpartIds.ToArray();
                        }
                    }
                    this.m_highlightAttributeDirty = false;
                }
            }
        }

        private void DoTransition()
        {
            this.PreviousObject = this.CurrentObject;
            this.State = MyHudSelectedObjectState.MarkedForVisible;
        }

        internal void Highlight(IMyUseObject obj)
        {
            if (!this.SetObjectInternal(obj))
            {
                if (!this.m_visible)
                {
                    this.State = MyHudSelectedObjectState.MarkedForVisible;
                }
                else if (this.State == MyHudSelectedObjectState.MarkedForNotVisible)
                {
                    this.State = MyHudSelectedObjectState.VisibleStateSet;
                }
            }
        }

        internal void RemoveHighlight()
        {
            if (this.m_visible)
            {
                this.State = MyHudSelectedObjectState.MarkedForNotVisible;
            }
            else if (this.State == MyHudSelectedObjectState.MarkedForVisible)
            {
                this.State = MyHudSelectedObjectState.VisibleStateSet;
            }
        }

        internal void ResetCurrent()
        {
            this.CurrentObject.Reset();
            this.m_highlightAttributeDirty = true;
        }

        private bool SetObjectInternal(IMyUseObject obj)
        {
            if (ReferenceEquals(this.CurrentObject.Instance, obj))
            {
                return false;
            }
            this.ResetCurrent();
            this.CurrentObject.Instance = obj;
            return this.CheckForTransition();
        }

        internal MyHudSelectedObjectState State { get; private set; }

        public string HighlightAttribute
        {
            get => 
                this.m_highlightAttribute;
            internal set
            {
                if (this.m_highlightAttribute != value)
                {
                    this.CheckForTransition();
                    this.m_highlightAttribute = value;
                    this.CurrentObject.SectionNames = null;
                    this.CurrentObject.SubpartIndices = null;
                    if (value != null)
                    {
                        this.m_highlightAttributeDirty = true;
                    }
                }
            }
        }

        public MyHudObjectHighlightStyle HighlightStyle
        {
            get => 
                this.m_style;
            set
            {
                if (this.m_style != value)
                {
                    this.CheckForTransition();
                    this.m_style = value;
                }
            }
        }

        public Vector2 HalfSize
        {
            get => 
                this.m_halfSize;
            set
            {
                if (this.m_halfSize != value)
                {
                    this.CheckForTransition();
                    this.m_halfSize = value;
                }
            }
        }

        public VRageMath.Color Color
        {
            get => 
                this.m_color;
            set
            {
                if (this.m_color != value)
                {
                    this.CheckForTransition();
                    this.m_color = value;
                }
            }
        }

        public bool Visible
        {
            get => 
                this.m_visible;
            internal set
            {
                this.m_visibleRenderID = !value ? uint.MaxValue : this.CurrentObject.Instance.RenderObjectID;
                this.CurrentObject.Style = !value ? MyHudObjectHighlightStyle.None : this.m_style;
                this.m_visible = value;
                this.State = MyHudSelectedObjectState.VisibleStateSet;
            }
        }

        public uint VisibleRenderID =>
            this.m_visibleRenderID;

        public IMyUseObject InteractiveObject =>
            this.CurrentObject.Instance;

        internal uint[] SubpartIndices
        {
            get
            {
                this.ComputeHighlightIndices();
                return this.CurrentObject.SubpartIndices;
            }
        }

        internal string[] SectionNames
        {
            get
            {
                this.ComputeHighlightIndices();
                return this.CurrentObject.SectionNames;
            }
        }
    }
}

