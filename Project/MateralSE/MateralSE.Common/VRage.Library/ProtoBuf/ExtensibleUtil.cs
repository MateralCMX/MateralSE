namespace ProtoBuf
{
    using ProtoBuf.Meta;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal static class ExtensibleUtil
    {
        internal static void AppendExtendValue(TypeModel model, IExtensible instance, int tag, DataFormat format, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            IExtension extensionObject = instance.GetExtensionObject(true);
            if (extensionObject == null)
            {
                throw new InvalidOperationException("No extension object available; appended data would be lost.");
            }
            bool commit = false;
            Stream dest = extensionObject.BeginAppend();
            try
            {
                using (ProtoWriter writer = new ProtoWriter(dest, model, null))
                {
                    model.TrySerializeAuxiliaryType(writer, null, format, tag, value, false);
                    writer.Close();
                }
                commit = true;
            }
            finally
            {
                extensionObject.EndAppend(dest, commit);
            }
        }

        public static void AppendExtendValueTyped<TSource, TValue>(TypeModel model, TSource instance, int tag, DataFormat format, TValue value) where TSource: class, IExtensible
        {
            AppendExtendValue(model, instance, tag, format, value);
        }

        [IteratorStateMachine(typeof(<GetExtendedValues>d__0))]
        internal static IEnumerable<TValue> GetExtendedValues<TValue>(IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            <GetExtendedValues>d__0<TValue> d__1 = new <GetExtendedValues>d__0<TValue>(-2);
            d__1.<>3__instance = instance;
            d__1.<>3__tag = tag;
            d__1.<>3__format = format;
            d__1.<>3__singleton = singleton;
            d__1.<>3__allowDefinedTag = allowDefinedTag;
            return d__1;
        }

        [IteratorStateMachine(typeof(<GetExtendedValues>d__1))]
        internal static IEnumerable GetExtendedValues(TypeModel model, Type type, IExtensible instance, int tag, DataFormat format, bool singleton, bool allowDefinedTag)
        {
            <GetExtendedValues>d__1 d__1 = new <GetExtendedValues>d__1(-2);
            d__1.<>3__model = model;
            d__1.<>3__type = type;
            d__1.<>3__instance = instance;
            d__1.<>3__tag = tag;
            d__1.<>3__format = format;
            d__1.<>3__singleton = singleton;
            return d__1;
        }

        [CompilerGenerated]
        private sealed class <GetExtendedValues>d__0<TValue> : IEnumerable<TValue>, IEnumerable, IEnumerator<TValue>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TValue <>2__current;
            private int <>l__initialThreadId;
            private IExtensible instance;
            public IExtensible <>3__instance;
            private int tag;
            public int <>3__tag;
            private DataFormat format;
            public DataFormat <>3__format;
            private bool singleton;
            public bool <>3__singleton;
            private bool allowDefinedTag;
            public bool <>3__allowDefinedTag;
            private IEnumerator <>7__wrap1;

            [DebuggerHidden]
            public <GetExtendedValues>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                IDisposable disposable = this.<>7__wrap1 as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = ExtensibleUtil.GetExtendedValues(RuntimeTypeModel.Default, typeof(TValue), this.instance, this.tag, this.format, this.singleton, this.allowDefinedTag).GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        flag = false;
                    }
                    else
                    {
                        TValue current = (TValue) this.<>7__wrap1.Current;
                        this.<>2__current = current;
                        this.<>1__state = 1;
                        flag = true;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ExtensibleUtil.<GetExtendedValues>d__0<TValue> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new ExtensibleUtil.<GetExtendedValues>d__0<TValue>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (ExtensibleUtil.<GetExtendedValues>d__0<TValue>) this;
                }
                d__.instance = this.<>3__instance;
                d__.tag = this.<>3__tag;
                d__.format = this.<>3__format;
                d__.singleton = this.<>3__singleton;
                d__.allowDefinedTag = this.<>3__allowDefinedTag;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TValue>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            TValue IEnumerator<TValue>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <GetExtendedValues>d__1 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private object <>2__current;
            private int <>l__initialThreadId;
            private IExtensible instance;
            public IExtensible <>3__instance;
            private int tag;
            public int <>3__tag;
            private TypeModel model;
            public TypeModel <>3__model;
            private bool singleton;
            public bool <>3__singleton;
            private DataFormat format;
            public DataFormat <>3__format;
            private Type type;
            public Type <>3__type;
            private IExtension <extn>5__2;
            private Stream <stream>5__3;
            private ProtoReader <reader>5__4;

            [DebuggerHidden]
            public <GetExtendedValues>d__1(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                this.<extn>5__2.EndQuery(this.<stream>5__3);
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                if (this.<reader>5__4 != null)
                {
                    this.<reader>5__4.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    object obj2;
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            if (this.instance == null)
                            {
                                throw new ArgumentNullException("instance");
                            }
                            if (this.tag <= 0)
                            {
                                throw new ArgumentOutOfRangeException("tag");
                            }
                            this.<extn>5__2 = this.instance.GetExtensionObject(false);
                            if (this.<extn>5__2 != null)
                            {
                                this.<stream>5__3 = this.<extn>5__2.BeginQuery();
                                obj2 = null;
                                this.<>1__state = -3;
                                SerializationContext context = new SerializationContext();
                                this.<reader>5__4 = new ProtoReader(this.<stream>5__3, this.model, context);
                                this.<>1__state = -4;
                            }
                            else
                            {
                                return false;
                            }
                            break;

                        case 1:
                            this.<>1__state = -4;
                            obj2 = null;
                            break;

                        case 2:
                            this.<>1__state = -3;
                            goto TR_0007;

                        default:
                            return false;
                    }
                    while (true)
                    {
                        bool flag;
                        if (this.model.TryDeserializeAuxiliaryType(this.<reader>5__4, this.format, this.tag, this.type, ref obj2, true, false, false, false) && (obj2 != null))
                        {
                            if (this.singleton)
                            {
                                continue;
                            }
                            this.<>2__current = obj2;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        else
                        {
                            this.<>m__Finally2();
                            this.<reader>5__4 = null;
                            if (!this.singleton || (obj2 == null))
                            {
                                break;
                            }
                            this.<>2__current = obj2;
                            this.<>1__state = 2;
                            flag = true;
                        }
                        return flag;
                    }
                TR_0007:
                    this.<>m__Finally1();
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                ExtensibleUtil.<GetExtendedValues>d__1 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new ExtensibleUtil.<GetExtendedValues>d__1(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.model = this.<>3__model;
                d__.type = this.<>3__type;
                d__.instance = this.<>3__instance;
                d__.tag = this.<>3__tag;
                d__.format = this.<>3__format;
                d__.singleton = this.<>3__singleton;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if (((num - -4) <= 1) || ((num - 1) <= 1))
                {
                    try
                    {
                        if ((num == -4) || (num == 1))
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally2();
                            }
                        }
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            object IEnumerator<object>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

