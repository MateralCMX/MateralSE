namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class PropertyDecorator : ProtoDecoratorBase
    {
        private readonly PropertyInfo property;
        private readonly Type forType;
        private readonly bool readOptionsWriteValue;
        private readonly MethodInfo shadowSetter;

        public PropertyDecorator(TypeModel model, Type forType, PropertyInfo property, IProtoSerializer tail) : base(tail)
        {
            this.forType = forType;
            this.property = property;
            SanityCheck(model, property, tail, out this.readOptionsWriteValue, true, true);
            this.shadowSetter = GetShadowSetter(model, property);
        }

        internal static bool CanWrite(TypeModel model, MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            PropertyInfo property = member as PropertyInfo;
            return ((property == null) ? (member is FieldInfo) : (property.CanWrite || (GetShadowSetter(model, property) != null)));
        }

        protected override void EmitRead(CompilerContext ctx, Local valueFrom)
        {
            bool flag;
            SanityCheck(ctx.Model, this.property, base.Tail, out flag, ctx.NonPublic, ctx.AllowInternal(this.property));
            if (this.ExpectedType.IsValueType && (valueFrom == null))
            {
                throw new InvalidOperationException("Attempt to mutate struct on the head of the stack; changes would be lost");
            }
            ctx.LoadAddress(valueFrom, this.ExpectedType);
            if (flag && base.Tail.RequiresOldValue)
            {
                ctx.CopyValue();
            }
            if (base.Tail.RequiresOldValue)
            {
                ctx.LoadValue(this.property);
            }
            ctx.ReadNullCheckedTail(this.property.PropertyType, base.Tail, null);
            if (!flag)
            {
                if (base.Tail.ReturnsValue)
                {
                    ctx.DiscardValue();
                }
            }
            else
            {
                CodeLabel label = new CodeLabel();
                CodeLabel label2 = new CodeLabel();
                if (!this.property.PropertyType.IsValueType)
                {
                    ctx.CopyValue();
                    label = ctx.DefineLabel();
                    label2 = ctx.DefineLabel();
                    ctx.BranchIfFalse(label, true);
                }
                if (this.shadowSetter == null)
                {
                    ctx.StoreValue(this.property);
                }
                else
                {
                    ctx.EmitCall(this.shadowSetter);
                }
                if (!this.property.PropertyType.IsValueType)
                {
                    ctx.Branch(label2, true);
                    ctx.MarkLabel(label);
                    ctx.DiscardValue();
                    ctx.DiscardValue();
                    ctx.MarkLabel(label2);
                }
            }
        }

        protected override void EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            ctx.LoadAddress(valueFrom, this.ExpectedType);
            ctx.LoadValue(this.property);
            ctx.WriteNullCheckedTail(this.property.PropertyType, base.Tail, null);
        }

        private static MethodInfo GetShadowSetter(TypeModel model, PropertyInfo property)
        {
            Type[] types = new Type[] { property.PropertyType };
            MethodInfo info = Helpers.GetInstanceMethod(property.ReflectedType, "Set" + property.Name, types);
            if (((info == null) || !info.IsPublic) || (info.ReturnType != model.MapType(typeof(void))))
            {
                return null;
            }
            return info;
        }

        public override object Read(object value, ProtoReader source)
        {
            object obj2 = base.Tail.RequiresOldValue ? this.property.GetValue(value, null) : null;
            object obj3 = base.Tail.Read(obj2, source);
            if (this.readOptionsWriteValue && (obj3 != null))
            {
                if (this.shadowSetter == null)
                {
                    this.property.SetValue(value, obj3, null);
                }
                else
                {
                    object[] parameters = new object[] { obj3 };
                    this.shadowSetter.Invoke(value, parameters);
                }
            }
            return null;
        }

        private static void SanityCheck(TypeModel model, PropertyInfo property, IProtoSerializer tail, out bool writeValue, bool nonPublic, bool allowInternal)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            writeValue = tail.ReturnsValue && ((GetShadowSetter(model, property) != null) || (property.CanWrite && (Helpers.GetSetMethod(property, nonPublic, allowInternal) != null)));
            if (!property.CanRead || (Helpers.GetGetMethod(property, nonPublic, allowInternal) == null))
            {
                throw new InvalidOperationException($"Cannot serialize property without a get accessor: {property.DeclaringType}.{property.Name}");
            }
            if (!writeValue && (!tail.RequiresOldValue || Helpers.IsValueType(tail.ExpectedType)))
            {
                throw new InvalidOperationException("Cannot apply changes to property " + property.DeclaringType.FullName + "." + property.Name);
            }
        }

        public override void Write(object value, ProtoWriter dest)
        {
            object obj1 = this.property.GetValue(value, null);
            value = obj1;
            if (value != null)
            {
                base.Tail.Write(value, dest);
            }
        }

        public override Type ExpectedType =>
            this.forType;

        public override bool RequiresOldValue =>
            true;

        public override bool ReturnsValue =>
            false;
    }
}

