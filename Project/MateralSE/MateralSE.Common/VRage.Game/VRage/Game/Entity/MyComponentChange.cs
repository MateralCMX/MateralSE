namespace VRage.Game.Entity
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyComponentChange
    {
        private const int OPERATION_REMOVAL = 0;
        private const int OPERATION_ADDITION = 1;
        private const int OPERATION_CHANGE = 2;
        private byte m_operation;
        private MyDefinitionId m_toRemove;
        private MyDefinitionId m_toAdd;
        public int Amount;
        public bool IsRemoval() => 
            (this.m_operation == 0);

        public bool IsAddition() => 
            (this.m_operation == 1);

        public bool IsChange() => 
            (this.m_operation == 2);

        public MyDefinitionId ToRemove
        {
            get => 
                this.m_toRemove;
            set => 
                (this.m_toRemove = value);
        }
        public MyDefinitionId ToAdd
        {
            get => 
                this.m_toAdd;
            set => 
                (this.m_toAdd = value);
        }
        public static MyComponentChange CreateRemoval(MyDefinitionId toRemove, int amount) => 
            new MyComponentChange { 
                ToRemove = toRemove,
                Amount = amount,
                m_operation = 0
            };

        public static MyComponentChange CreateAddition(MyDefinitionId toAdd, int amount) => 
            new MyComponentChange { 
                ToAdd = toAdd,
                Amount = amount,
                m_operation = 1
            };

        public static MyComponentChange CreateChange(MyDefinitionId toRemove, MyDefinitionId toAdd, int amount) => 
            new MyComponentChange { 
                ToRemove = toRemove,
                ToAdd = toAdd,
                Amount = amount,
                m_operation = 2
            };
    }
}

