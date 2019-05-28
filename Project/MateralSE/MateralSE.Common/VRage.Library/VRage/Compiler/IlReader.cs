namespace VRage.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using VRage;

    public class IlReader
    {
        private BinaryReader stream;
        private OpCode[] singleByteOpCode;
        private OpCode[] doubleByteOpCode;
        private byte[] instructions;
        private IList<LocalVariableInfo> locals;
        private ParameterInfo[] parameters;
        private Type[] typeArgs;
        private Type[] methodArgs;
        private MethodBase currentMethod;
        private List<IlInstruction> ilInstructions;

        public IlReader()
        {
            this.CreateOpCodes();
        }

        private void CreateOpCodes()
        {
            this.singleByteOpCode = new OpCode[0xe1];
            this.doubleByteOpCode = new OpCode[0x1f];
            FieldInfo[] opCodeFields = this.GetOpCodeFields();
            for (int i = 0; i < opCodeFields.Length; i++)
            {
                OpCode code = (OpCode) opCodeFields[i].GetValue(null);
                if (code.OpCodeType != OpCodeType.Nternal)
                {
                    if (code.Size == 1)
                    {
                        this.singleByteOpCode[code.Value] = code;
                    }
                    else
                    {
                        this.doubleByteOpCode[code.Value & 0xff] = code;
                    }
                }
            }
        }

        private FieldInfo[] GetOpCodeFields() => 
            typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);

        private object GetVariable(OpCode code, int index)
        {
            if (code.Name.Contains("loc"))
            {
                return this.locals[index];
            }
            if (!this.currentMethod.IsStatic)
            {
                index--;
            }
            return this.parameters[index];
        }

        public List<IlInstruction> ReadInstructions(MethodBase method)
        {
            this.ilInstructions = new List<IlInstruction>();
            this.currentMethod = method;
            MethodBody methodBody = method.GetMethodBody();
            this.parameters = method.GetParameters();
            if (methodBody != null)
            {
                this.locals = methodBody.LocalVariables;
                this.instructions = method.GetMethodBody().GetILAsByteArray();
                ByteStream input = new ByteStream(this.instructions, this.instructions.Length);
                this.stream = new BinaryReader(input);
                if (!typeof(ConstructorInfo).IsAssignableFrom(method.GetType()))
                {
                    this.methodArgs = method.GetGenericArguments();
                }
                if (method.DeclaringType != null)
                {
                    this.typeArgs = method.DeclaringType.GetGenericArguments();
                }
                IlInstruction item = null;
                while (this.stream.BaseStream.Position < this.stream.BaseStream.Length)
                {
                    item = new IlInstruction();
                    bool isDoubleByte = false;
                    OpCode code = this.ReadOpCode(ref isDoubleByte);
                    item.OpCode = code;
                    item.Offset = this.stream.BaseStream.Position - 1L;
                    if (isDoubleByte)
                    {
                        item.Offset -= 1L;
                    }
                    item.Operand = this.ReadOperand(code, method.Module, ref item.LocalVariableIndex);
                    this.ilInstructions.Add(item);
                }
            }
            return this.ilInstructions;
        }

        private OpCode ReadOpCode(ref bool isDoubleByte)
        {
            isDoubleByte = false;
            byte index = this.stream.ReadByte();
            if (index != 0xfe)
            {
                return this.singleByteOpCode[index];
            }
            isDoubleByte = true;
            return this.doubleByteOpCode[this.stream.ReadByte()];
        }

        private object ReadOperand(OpCode code, Module module, ref long localVariableIndex)
        {
            object variable = null;
            switch (code.OperandType)
            {
                case OperandType.InlineBrTarget:
                    variable = this.stream.ReadInt32() + this.stream.BaseStream.Position;
                    break;

                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    variable = module.ResolveMember(this.stream.ReadInt32(), this.typeArgs, this.methodArgs);
                    break;

                case OperandType.InlineI:
                    variable = this.stream.ReadInt32();
                    break;

                case OperandType.InlineI8:
                    variable = this.stream.ReadInt64();
                    break;

                case OperandType.InlineNone:
                    break;

                case OperandType.InlineR:
                    variable = this.stream.ReadDouble();
                    break;

                case OperandType.InlineSig:
                    variable = module.ResolveSignature(this.stream.ReadInt32());
                    break;

                case OperandType.InlineString:
                    variable = module.ResolveString(this.stream.ReadInt32());
                    break;

                case OperandType.InlineSwitch:
                {
                    int num = this.stream.ReadInt32();
                    int[] numArray = new int[num];
                    int[] numArray2 = new int[num];
                    int index = 0;
                    while (true)
                    {
                        if (index >= num)
                        {
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= num)
                                {
                                    variable = numArray;
                                    break;
                                }
                                numArray[num3] = ((int) this.stream.BaseStream.Position) + numArray2[num3];
                                num3++;
                            }
                            break;
                        }
                        numArray2[index] = this.stream.ReadInt32();
                        index++;
                    }
                    break;
                }
                case OperandType.InlineVar:
                {
                    int index = this.stream.ReadUInt16();
                    variable = this.GetVariable(code, index);
                    localVariableIndex = index;
                    break;
                }
                case OperandType.ShortInlineBrTarget:
                    if ((code.FlowControl == FlowControl.Branch) || (code.FlowControl == FlowControl.Cond_Branch))
                    {
                        variable = this.stream.ReadSByte() + this.stream.BaseStream.Position;
                    }
                    else
                    {
                        variable = this.stream.ReadSByte();
                    }
                    break;

                case OperandType.ShortInlineI:
                    variable = !(code == OpCodes.Ldc_I4_S) ? ((object) this.stream.ReadByte()) : ((object) ((sbyte) this.stream.ReadByte()));
                    break;

                case OperandType.ShortInlineR:
                    variable = this.stream.ReadSingle();
                    break;

                case OperandType.ShortInlineVar:
                {
                    int index = this.stream.ReadByte();
                    variable = this.GetVariable(code, index);
                    localVariableIndex = index;
                    break;
                }
                default:
                    throw new NotSupportedException();
            }
            return variable;
        }

        public IList<LocalVariableInfo> Locals =>
            this.locals;

        public class IlInstruction
        {
            public System.Reflection.Emit.OpCode OpCode;
            public object Operand;
            public long Offset;
            public long LocalVariableIndex;

            public string FormatOperand()
            {
                OperandType operandType = this.OpCode.OperandType;
                switch (operandType)
                {
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                        goto TR_0004;

                    case OperandType.InlineI:
                    case OperandType.InlineI8:
                        break;

                    case OperandType.InlineNone:
                        return string.Empty;

                    default:
                        if ((operandType - OperandType.InlineTok) > OperandType.InlineField)
                        {
                            break;
                        }
                        goto TR_0004;
                }
                return this.Operand.ToString();
            TR_0004:
                if (this.Operand is MethodInfo)
                {
                    MethodInfo info = (MethodInfo) this.Operand;
                    string str = info.ToString().Substring(info.ReturnType.Name.ToString().Length + 1);
                    return $"{info.ReturnType} {info.DeclaringType}::{str}";
                }
                if (!(this.Operand is ConstructorInfo))
                {
                    return this.Operand.ToString();
                }
                ConstructorInfo operand = (ConstructorInfo) this.Operand;
                string str2 = operand.ToString().Substring("Void".Length + 1);
                return $"{operand.DeclaringType}::{str2}";
            }

            public override string ToString() => 
                (this.OpCode + " " + this.FormatOperand());
        }
    }
}

