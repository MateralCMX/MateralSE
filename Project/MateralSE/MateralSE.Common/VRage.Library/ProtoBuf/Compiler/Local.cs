namespace ProtoBuf.Compiler
{
    using System;
    using System.Reflection.Emit;

    internal sealed class Local : IDisposable
    {
        public static readonly Local InputValue = new Local(null, null);
        private LocalBuilder value;
        private CompilerContext ctx;

        private Local(LocalBuilder value)
        {
            this.value = value;
        }

        internal Local(CompilerContext ctx, System.Type type)
        {
            this.ctx = ctx;
            if (ctx != null)
            {
                this.value = ctx.GetFromPool(type);
            }
        }

        public Local AsCopy() => 
            ((this.ctx != null) ? new Local(this.value) : this);

        public void Dispose()
        {
            if (this.ctx != null)
            {
                this.ctx.ReleaseToPool(this.value);
                this.value = null;
                this.ctx = null;
            }
        }

        public System.Type Type =>
            this.value?.LocalType;

        internal LocalBuilder Value
        {
            get
            {
                if (this.value == null)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.value;
            }
        }
    }
}

