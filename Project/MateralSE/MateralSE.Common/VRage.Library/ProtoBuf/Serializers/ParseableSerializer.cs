namespace ProtoBuf.Serializers
{
    using ProtoBuf;
    using ProtoBuf.Compiler;
    using ProtoBuf.Meta;
    using System;
    using System.Reflection;

    internal sealed class ParseableSerializer : IProtoSerializer
    {
        private readonly MethodInfo parse;

        private ParseableSerializer(MethodInfo parse)
        {
            this.parse = parse;
        }

        private static MethodInfo GetCustomToString(Type type) => 
            type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Helpers.EmptyTypes, null);

        void IProtoSerializer.EmitRead(CompilerContext ctx, Local valueFrom)
        {
            ctx.EmitBasicRead("ReadString", ctx.MapType(typeof(string)));
            ctx.EmitCall(this.parse);
        }

        void IProtoSerializer.EmitWrite(CompilerContext ctx, Local valueFrom)
        {
            Type expectedType = this.ExpectedType;
            if (expectedType.IsValueType)
            {
                using (Local local = ctx.GetLocalWithValue(expectedType, valueFrom))
                {
                    ctx.LoadAddress(local, expectedType);
                    ctx.EmitCall(GetCustomToString(expectedType));
                    goto TR_0000;
                }
            }
            ctx.EmitCall(ctx.MapType(typeof(object)).GetMethod("ToString"));
        TR_0000:
            ctx.EmitBasicWrite("WriteString", valueFrom);
        }

        public object Read(object value, ProtoReader source)
        {
            object[] parameters = new object[] { source.ReadString() };
            return this.parse.Invoke(null, parameters);
        }

        public static ParseableSerializer TryCreate(Type type, TypeModel model)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Type[] types = new Type[] { model.MapType(typeof(string)) };
            MethodInfo parse = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, types, null);
            if ((parse == null) || (parse.ReturnType != type))
            {
                return null;
            }
            if (Helpers.IsValueType(type))
            {
                MethodInfo customToString = GetCustomToString(type);
                if ((customToString == null) || (customToString.ReturnType != model.MapType(typeof(string))))
                {
                    return null;
                }
            }
            return new ParseableSerializer(parse);
        }

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteString(value.ToString(), dest);
        }

        public Type ExpectedType =>
            this.parse.DeclaringType;

        bool IProtoSerializer.RequiresOldValue =>
            false;

        bool IProtoSerializer.ReturnsValue =>
            true;
    }
}

