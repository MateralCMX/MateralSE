namespace Sandbox.Game
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyInventoryConstraint
    {
        public string Icon;
        public bool m_useDefaultIcon;
        public readonly string Description;
        private HashSet<MyDefinitionId> m_constrainedIds;
        private HashSet<MyObjectBuilderType> m_constrainedTypes;

        public MyInventoryConstraint(string description, string icon = null, bool whitelist = true)
        {
            this.Icon = icon;
            this.m_useDefaultIcon = ReferenceEquals(icon, null);
            this.Description = description;
            this.m_constrainedIds = new HashSet<MyDefinitionId>();
            this.m_constrainedTypes = new HashSet<MyObjectBuilderType>();
            this.IsWhitelist = whitelist;
        }

        public MyInventoryConstraint(MyStringId description, string icon = null, bool whitelist = true)
        {
            this.Icon = icon;
            this.m_useDefaultIcon = ReferenceEquals(icon, null);
            this.Description = MyTexts.GetString(description);
            this.m_constrainedIds = new HashSet<MyDefinitionId>();
            this.m_constrainedTypes = new HashSet<MyObjectBuilderType>();
            this.IsWhitelist = whitelist;
        }

        public MyInventoryConstraint Add(MyDefinitionId id)
        {
            this.m_constrainedIds.Add(id);
            this.UpdateIcon();
            return this;
        }

        public MyInventoryConstraint AddObjectBuilderType(MyObjectBuilderType type)
        {
            this.m_constrainedTypes.Add(type);
            this.UpdateIcon();
            return this;
        }

        public bool Check(MyDefinitionId checkedId) => 
            (!this.IsWhitelist ? (!this.m_constrainedTypes.Contains(checkedId.TypeId) ? !this.m_constrainedIds.Contains(checkedId) : false) : (!this.m_constrainedTypes.Contains(checkedId.TypeId) ? this.m_constrainedIds.Contains(checkedId) : true));

        public void Clear()
        {
            this.m_constrainedIds.Clear();
            this.m_constrainedTypes.Clear();
            this.UpdateIcon();
        }

        public MyInventoryConstraint Remove(MyDefinitionId id)
        {
            this.m_constrainedIds.Remove(id);
            this.UpdateIcon();
            return this;
        }

        public MyInventoryConstraint RemoveObjectBuilderType(MyObjectBuilderType type)
        {
            this.m_constrainedTypes.Remove(type);
            this.UpdateIcon();
            return this;
        }

        public void UpdateIcon()
        {
            if (this.m_useDefaultIcon)
            {
                if ((this.m_constrainedIds.Count != 0) || (this.m_constrainedTypes.Count != 1))
                {
                    if ((this.m_constrainedIds.Count != 1) || (this.m_constrainedTypes.Count != 0))
                    {
                        this.Icon = null;
                    }
                    else if (this.m_constrainedIds.First<MyDefinitionId>() == new MyDefinitionId(typeof(MyObjectBuilder_Ingot), "Uranium"))
                    {
                        this.Icon = MyGuiConstants.TEXTURE_ICON_FILTER_URANIUM;
                    }
                }
                else
                {
                    MyObjectBuilderType type = this.m_constrainedTypes.First<MyObjectBuilderType>();
                    if (type == typeof(MyObjectBuilder_Ore))
                    {
                        this.Icon = MyGuiConstants.TEXTURE_ICON_FILTER_ORE;
                    }
                    else if (type == typeof(MyObjectBuilder_Ingot))
                    {
                        this.Icon = MyGuiConstants.TEXTURE_ICON_FILTER_INGOT;
                    }
                    else if (type == typeof(MyObjectBuilder_Component))
                    {
                        this.Icon = MyGuiConstants.TEXTURE_ICON_FILTER_COMPONENT;
                    }
                }
            }
        }

        public bool IsWhitelist { get; set; }

        public IEnumerable<MyDefinitionId> ConstrainedIds =>
            this.m_constrainedIds.Skip<MyDefinitionId>(0);

        public IEnumerable<MyObjectBuilderType> ConstrainedTypes =>
            this.m_constrainedTypes.Skip<MyObjectBuilderType>(0);
    }
}

