namespace ProtoBuf
{
    using System;
    using System.Runtime.Serialization;

    public sealed class SerializationContext
    {
        private bool frozen;
        private object context;
        private static readonly SerializationContext @default = new SerializationContext();
        private StreamingContextStates state = StreamingContextStates.Persistence;

        static SerializationContext()
        {
            @default.Freeze();
        }

        internal void Freeze()
        {
            this.frozen = true;
        }

        public static implicit operator StreamingContext(SerializationContext ctx) => 
            ((ctx != null) ? new StreamingContext(ctx.state, ctx.context) : new StreamingContext(StreamingContextStates.Persistence));

        public static implicit operator SerializationContext(StreamingContext ctx) => 
            new SerializationContext { 
                Context = ctx.Context,
                State = ctx.State
            };

        private void ThrowIfFrozen()
        {
            if (this.frozen)
            {
                throw new InvalidOperationException("The serialization-context cannot be changed once it is in use");
            }
        }

        public object Context
        {
            get => 
                this.context;
            set
            {
                if (this.context != value)
                {
                    this.ThrowIfFrozen();
                    this.context = value;
                }
            }
        }

        internal static SerializationContext Default =>
            @default;

        public StreamingContextStates State
        {
            get => 
                this.state;
            set
            {
                if (this.state != value)
                {
                    this.ThrowIfFrozen();
                    this.state = value;
                }
            }
        }
    }
}

