namespace LitJson
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class JsonWriter
    {
        private static NumberFormatInfo number_format = NumberFormatInfo.InvariantInfo;
        private WriterContext context;
        private Stack<WriterContext> ctx_stack;
        private bool has_reached_end;
        private char[] hex_seq;
        private int indentation;
        private int indent_value;
        private StringBuilder inst_string_builder;
        private bool pretty_print;
        private bool validate;
        private System.IO.TextWriter writer;

        public JsonWriter()
        {
            this.inst_string_builder = new StringBuilder();
            this.writer = new StringWriter(this.inst_string_builder);
            this.Init();
        }

        public JsonWriter(System.IO.TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.writer = writer;
            this.Init();
        }

        public JsonWriter(StringBuilder sb) : this(new StringWriter(sb))
        {
        }

        private void DoValidation(Condition cond)
        {
            if (!this.context.ExpectingValue)
            {
                this.context.Count++;
            }
            if (this.validate)
            {
                if (this.has_reached_end)
                {
                    throw new JsonException("A complete JSON symbol has already been written");
                }
                switch (cond)
                {
                    case Condition.InArray:
                        if (this.context.InArray)
                        {
                            break;
                        }
                        throw new JsonException("Can't close an array here");

                    case Condition.InObject:
                        if (this.context.InObject && !this.context.ExpectingValue)
                        {
                            break;
                        }
                        throw new JsonException("Can't close an object here");

                    case Condition.NotAProperty:
                        if (!this.context.InObject || this.context.ExpectingValue)
                        {
                            break;
                        }
                        throw new JsonException("Expected a property");

                    case Condition.Property:
                        if (this.context.InObject && !this.context.ExpectingValue)
                        {
                            break;
                        }
                        throw new JsonException("Can't add a property here");

                    case Condition.InValue:
                        if (this.context.InArray || (this.context.InObject && this.context.ExpectingValue))
                        {
                            break;
                        }
                        throw new JsonException("Can't add a value here");

                    default:
                        return;
                }
            }
        }

        private void Indent()
        {
            if (this.pretty_print)
            {
                this.indentation += this.indent_value;
            }
        }

        private void Init()
        {
            this.has_reached_end = false;
            this.hex_seq = new char[4];
            this.indentation = 0;
            this.indent_value = 4;
            this.pretty_print = false;
            this.validate = true;
            this.ctx_stack = new Stack<WriterContext>();
            this.context = new WriterContext();
            this.ctx_stack.Push(this.context);
        }

        private static void IntToHex(int n, char[] hex)
        {
            for (int i = 0; i < 4; i++)
            {
                int num = n % 0x10;
                hex[3 - i] = (num >= 10) ? ((char) (0x41 + (num - 10))) : ((char) (0x30 + num));
                n = n >> 4;
            }
        }

        private void Put(string str)
        {
            if (this.pretty_print && !this.context.ExpectingValue)
            {
                for (int i = 0; i < this.indentation; i++)
                {
                    this.writer.Write(' ');
                }
            }
            this.writer.Write(str);
        }

        private void PutNewline()
        {
            this.PutNewline(true);
        }

        private void PutNewline(bool add_comma)
        {
            if ((add_comma && !this.context.ExpectingValue) && (this.context.Count > 1))
            {
                this.writer.Write(',');
            }
            if (this.pretty_print && !this.context.ExpectingValue)
            {
                this.writer.Write('\n');
            }
        }

        private void PutString(string str)
        {
            this.Put(string.Empty);
            this.writer.Write('"');
            int length = str.Length;
            int num2 = 0;
            goto TR_0011;
        TR_0001:
            num2++;
        TR_0011:
            while (true)
            {
                if (num2 >= length)
                {
                    this.writer.Write('"');
                    return;
                }
                char ch = str[num2];
                switch (ch)
                {
                    case '\b':
                        this.writer.Write(@"\b");
                        goto TR_0001;

                    case '\t':
                        this.writer.Write(@"\t");
                        goto TR_0001;

                    case '\n':
                        this.writer.Write(@"\n");
                        goto TR_0001;

                    case '\v':
                        break;

                    case '\f':
                        this.writer.Write(@"\f");
                        goto TR_0001;

                    case '\r':
                        this.writer.Write(@"\r");
                        goto TR_0001;

                    default:
                        if ((ch != '"') && (ch != '\\'))
                        {
                            break;
                        }
                        this.writer.Write('\\');
                        this.writer.Write(str[num2]);
                        goto TR_0001;
                }
                if ((str[num2] >= ' ') && (str[num2] <= '~'))
                {
                    this.writer.Write(str[num2]);
                }
                else
                {
                    IntToHex(str[num2], this.hex_seq);
                    this.writer.Write(@"\u");
                    this.writer.Write(this.hex_seq);
                }
                break;
            }
            goto TR_0001;
        }

        public void Reset()
        {
            this.has_reached_end = false;
            this.ctx_stack.Clear();
            this.context = new WriterContext();
            this.ctx_stack.Push(this.context);
            if (this.inst_string_builder != null)
            {
                this.inst_string_builder.Remove(0, this.inst_string_builder.Length);
            }
        }

        public override string ToString() => 
            ((this.inst_string_builder != null) ? this.inst_string_builder.ToString() : string.Empty);

        private void Unindent()
        {
            if (this.pretty_print)
            {
                this.indentation -= this.indent_value;
            }
        }

        public void Write(bool boolean)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            this.Put(boolean ? "true" : "false");
            this.context.ExpectingValue = false;
        }

        public void Write(decimal number)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            this.Put(Convert.ToString(number, number_format));
            this.context.ExpectingValue = false;
        }

        public void Write(double number)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            string str = Convert.ToString(number, number_format);
            this.Put(str);
            if ((str.IndexOf('.') == -1) && (str.IndexOf('E') == -1))
            {
                this.writer.Write(".0");
            }
            this.context.ExpectingValue = false;
        }

        public void Write(int number)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            this.Put(Convert.ToString(number, number_format));
            this.context.ExpectingValue = false;
        }

        public void Write(long number)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            this.Put(Convert.ToString(number, number_format));
            this.context.ExpectingValue = false;
        }

        public void Write(string str)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            if (str == null)
            {
                this.Put("null");
            }
            else
            {
                this.PutString(str);
            }
            this.context.ExpectingValue = false;
        }

        public void Write(ulong number)
        {
            this.DoValidation(Condition.InValue);
            this.PutNewline();
            this.Put(Convert.ToString(number, number_format));
            this.context.ExpectingValue = false;
        }

        public void WriteArrayEnd()
        {
            this.DoValidation(Condition.InArray);
            this.PutNewline(false);
            this.ctx_stack.Pop();
            if (this.ctx_stack.Count == 1)
            {
                this.has_reached_end = true;
            }
            else
            {
                this.context = this.ctx_stack.Peek();
                this.context.ExpectingValue = false;
            }
            this.Unindent();
            this.Put("]");
        }

        public void WriteArrayStart()
        {
            this.DoValidation(Condition.NotAProperty);
            this.PutNewline();
            this.Put("[");
            this.context = new WriterContext();
            this.context.InArray = true;
            this.ctx_stack.Push(this.context);
            this.Indent();
        }

        public void WriteObjectEnd()
        {
            this.DoValidation(Condition.InObject);
            this.PutNewline(false);
            this.ctx_stack.Pop();
            if (this.ctx_stack.Count == 1)
            {
                this.has_reached_end = true;
            }
            else
            {
                this.context = this.ctx_stack.Peek();
                this.context.ExpectingValue = false;
            }
            this.Unindent();
            this.Put("}");
        }

        public void WriteObjectStart()
        {
            this.DoValidation(Condition.NotAProperty);
            this.PutNewline();
            this.Put("{");
            this.context = new WriterContext();
            this.context.InObject = true;
            this.ctx_stack.Push(this.context);
            this.Indent();
        }

        public void WritePropertyName(string property_name)
        {
            this.DoValidation(Condition.Property);
            this.PutNewline();
            this.PutString(property_name);
            if (!this.pretty_print)
            {
                this.writer.Write(':');
            }
            else
            {
                if (property_name.Length > this.context.Padding)
                {
                    this.context.Padding = property_name.Length;
                }
                int num = this.context.Padding - property_name.Length;
                while (true)
                {
                    if (num < 0)
                    {
                        this.writer.Write(": ");
                        break;
                    }
                    this.writer.Write(' ');
                    num--;
                }
            }
            this.context.ExpectingValue = true;
        }

        public int IndentValue
        {
            get => 
                this.indent_value;
            set
            {
                this.indentation = (this.indentation / this.indent_value) * value;
                this.indent_value = value;
            }
        }

        public bool PrettyPrint
        {
            get => 
                this.pretty_print;
            set => 
                (this.pretty_print = value);
        }

        public System.IO.TextWriter TextWriter =>
            this.writer;

        public bool Validate
        {
            get => 
                this.validate;
            set => 
                (this.validate = value);
        }
    }
}

