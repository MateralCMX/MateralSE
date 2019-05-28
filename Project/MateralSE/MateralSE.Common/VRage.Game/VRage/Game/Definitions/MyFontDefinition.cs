namespace VRage.Game.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    [MyDefinitionType(typeof(MyObjectBuilder_FontDefinition), (Type) null)]
    public class MyFontDefinition : MyDefinitionBase
    {
        private MyObjectBuilder_FontDefinition m_ob;
        private List<MyObjectBuilder_FontData> m_currentResources;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.m_ob = builder as MyObjectBuilder_FontDefinition;
            this.m_currentResources = this.m_ob.Resources;
            SortBySize(this.m_currentResources);
        }

        private static void SortBySize(List<MyObjectBuilder_FontData> resources)
        {
            if (resources != null)
            {
                resources.Sort((dataX, dataY) => dataX.Size.CompareTo(dataY.Size));
            }
        }

        public void UseLanguage(string language)
        {
            MyObjectBuilder_FontDefinition.LanguageResources resources = this.m_ob.LanguageSpecificDefinitions.FirstOrDefault<MyObjectBuilder_FontDefinition.LanguageResources>(x => x.Language == language);
            if (resources == null)
            {
                this.m_currentResources = this.m_ob.Resources;
            }
            else
            {
                this.m_currentResources = resources.Resources;
                SortBySize(this.m_currentResources);
            }
        }

        public bool IsValid =>
            (this.m_ob != null);

        public string CompatibilityPath =>
            this.m_ob.Path;

        public Color? ColorMask =>
            this.m_ob.ColorMask;

        public bool Default =>
            this.m_ob.Default;

        public IEnumerable<MyObjectBuilder_FontData> Resources =>
            this.m_currentResources;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyFontDefinition.<>c <>9 = new MyFontDefinition.<>c();
            public static Comparison<MyObjectBuilder_FontData> <>9__14_0;

            internal int <SortBySize>b__14_0(MyObjectBuilder_FontData dataX, MyObjectBuilder_FontData dataY) => 
                dataX.Size.CompareTo(dataY.Size);
        }
    }
}

