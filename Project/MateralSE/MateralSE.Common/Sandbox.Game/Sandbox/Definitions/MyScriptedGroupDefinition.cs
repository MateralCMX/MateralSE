namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ScriptedGroupDefinition), (Type) null)]
    public class MyScriptedGroupDefinition : MyDefinitionBase
    {
        public MyStringHash Category;
        public MyStringHash Script;
        private HashSet<MyDefinitionId> m_scriptedObjects;
        private List<MyDefinitionId> m_scriptedObjectsList;

        public void Add(MyModContext context, MyDefinitionId obj)
        {
            if (context == null)
            {
                MyLog.Default.WriteLine("Writing to scripted group definition without context");
            }
            else
            {
                this.m_scriptedObjects.Add(obj);
            }
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ScriptedGroupDefinition definition = builder as MyObjectBuilder_ScriptedGroupDefinition;
            this.Category = MyStringHash.GetOrCompute(definition.Category);
            this.Script = MyStringHash.GetOrCompute(definition.Script);
            this.m_scriptedObjects = new HashSet<MyDefinitionId>();
        }

        public HashSetReader<MyDefinitionId> SetReader =>
            new HashSetReader<MyDefinitionId>(this.m_scriptedObjects);

        public ListReader<MyDefinitionId> ListReader
        {
            get
            {
                if (this.m_scriptedObjectsList == null)
                {
                    this.m_scriptedObjectsList = new List<MyDefinitionId>(this.m_scriptedObjects);
                }
                return new ListReader<MyDefinitionId>(this.m_scriptedObjectsList);
            }
        }
    }
}

