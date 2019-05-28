namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_GuiBlockCategoryDefinition), (Type) null)]
    public class MyGuiBlockCategoryDefinition : MyDefinitionBase
    {
        public string Name;
        public HashSet<string> ItemIds;
        public bool IsShipCategory;
        public bool IsBlockCategory = true;
        public bool SearchBlocks = true;
        public bool ShowAnimations;
        public bool ShowInCreative = true;
        public bool IsAnimationCategory;
        public bool IsToolCategory;
        public int ValidItems;
        public bool Public = true;

        public bool HasItem(string itemId)
        {
            using (HashSet<string>.Enumerator enumerator = this.ItemIds.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    string current = enumerator.Current;
                    if (itemId.EndsWith(current))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_GuiBlockCategoryDefinition definition = ob as MyObjectBuilder_GuiBlockCategoryDefinition;
            this.Name = definition.Name;
            this.ItemIds = new HashSet<string>(definition.ItemIds.ToList<string>());
            this.IsBlockCategory = definition.IsBlockCategory;
            this.IsShipCategory = definition.IsShipCategory;
            this.SearchBlocks = definition.SearchBlocks;
            this.ShowAnimations = definition.ShowAnimations;
            this.ShowInCreative = definition.ShowInCreative;
            this.Public = definition.Public;
            this.IsAnimationCategory = definition.IsAnimationCategory;
            this.IsToolCategory = definition.IsToolCategory;
        }

        private class SubtypeComparer : IComparer<MyGuiBlockCategoryDefinition>
        {
            public static MyGuiBlockCategoryDefinition.SubtypeComparer Static = new MyGuiBlockCategoryDefinition.SubtypeComparer();

            public int Compare(MyGuiBlockCategoryDefinition x, MyGuiBlockCategoryDefinition y) => 
                x.Id.SubtypeName.CompareTo(y.Id.SubtypeName);
        }
    }
}

