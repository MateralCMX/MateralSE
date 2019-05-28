namespace Sandbox.Game.World
{
    using System;
    using VRage.Game;

    public abstract class MyWorldGeneratorOperationBase
    {
        public string FactionTag;

        protected MyWorldGeneratorOperationBase()
        {
        }

        public abstract void Apply();
        public virtual MyObjectBuilder_WorldGeneratorOperation GetObjectBuilder()
        {
            MyObjectBuilder_WorldGeneratorOperation operation1 = MyWorldGenerator.OperationFactory.CreateObjectBuilder(this);
            operation1.FactionTag = this.FactionTag;
            return operation1;
        }

        public virtual void Init(MyObjectBuilder_WorldGeneratorOperation builder)
        {
            this.FactionTag = builder.FactionTag;
        }
    }
}

