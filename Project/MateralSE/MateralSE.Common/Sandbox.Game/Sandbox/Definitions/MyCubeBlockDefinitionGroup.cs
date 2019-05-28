namespace Sandbox.Definitions
{
    using System;
    using System.Reflection;
    using VRage.Game;

    public class MyCubeBlockDefinitionGroup
    {
        private static int m_sizeCount = Enum.GetValues(typeof(MyCubeSize)).Length;
        private readonly MyCubeBlockDefinition[] m_definitions = new MyCubeBlockDefinition[m_sizeCount];

        internal MyCubeBlockDefinitionGroup()
        {
        }

        public bool Contains(MyCubeBlockDefinition defCnt)
        {
            MyCubeBlockDefinition[] definitions = this.m_definitions;
            int index = 0;
            while (index < definitions.Length)
            {
                MyCubeBlockDefinition objA = definitions[index];
                if (ReferenceEquals(objA, defCnt))
                {
                    return true;
                }
                MyDefinitionId[] blockStages = objA.BlockStages;
                int num2 = 0;
                while (true)
                {
                    if (num2 >= blockStages.Length)
                    {
                        index++;
                        break;
                    }
                    MyDefinitionId id = blockStages[num2];
                    if (defCnt.Id == id)
                    {
                        return true;
                    }
                    num2++;
                }
            }
            return false;
        }

        public MyCubeBlockDefinition this[MyCubeSize size]
        {
            get => 
                this.m_definitions[(int) size];
            set => 
                (this.m_definitions[(int) size] = value);
        }

        public int SizeCount =>
            m_sizeCount;

        public MyCubeBlockDefinition Large =>
            this[MyCubeSize.Large];

        public MyCubeBlockDefinition Small =>
            this[MyCubeSize.Small];

        public MyCubeBlockDefinition Any
        {
            get
            {
                foreach (MyCubeBlockDefinition definition in this.m_definitions)
                {
                    if (definition != null)
                    {
                        return definition;
                    }
                }
                return null;
            }
        }

        public MyCubeBlockDefinition AnyPublic
        {
            get
            {
                foreach (MyCubeBlockDefinition definition in this.m_definitions)
                {
                    if ((definition != null) && definition.Public)
                    {
                        return definition;
                    }
                }
                return null;
            }
        }
    }
}

