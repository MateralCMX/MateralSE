namespace LitJson
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class Lexer
    {
        private static int[] fsm_return_table;
        private static StateHandler[] fsm_handler_table;
        private bool allow_comments = true;
        private bool allow_single_quoted_strings = true;
        private bool end_of_input = false;
        private FsmContext fsm_context;
        private int input_buffer = 0;
        private int input_char;
        private TextReader reader;
        private int state = 1;
        private StringBuilder string_buffer = new StringBuilder(0x80);
        private string string_value;
        private int token;
        private int unichar;

        static Lexer()
        {
            PopulateFsmTables();
        }

        public Lexer(TextReader reader)
        {
            this.reader = reader;
            this.fsm_context = new FsmContext();
            this.fsm_context.L = this;
        }

        private bool GetChar()
        {
            this.input_char = this.NextChar();
            if (this.input_char != -1)
            {
                return true;
            }
            this.end_of_input = true;
            return false;
        }

        private static int HexValue(int digit)
        {
            switch (digit)
            {
                case 0x41:
                    goto TR_0001;

                case 0x42:
                    goto TR_0002;

                case 0x43:
                    goto TR_0003;

                case 0x44:
                    goto TR_0004;

                case 0x45:
                    goto TR_0005;

                case 70:
                    break;

                default:
                    switch (digit)
                    {
                        case 0x61:
                            goto TR_0001;

                        case 0x62:
                            goto TR_0002;

                        case 0x63:
                            goto TR_0003;

                        case 100:
                            goto TR_0004;

                        case 0x65:
                            goto TR_0005;

                        case 0x66:
                            break;

                        default:
                            return (digit - 0x30);
                    }
                    break;
            }
            return 15;
        TR_0001:
            return 10;
        TR_0002:
            return 11;
        TR_0003:
            return 12;
        TR_0004:
            return 13;
        TR_0005:
            return 14;
        }

        private int NextChar()
        {
            if (this.input_buffer == 0)
            {
                return this.reader.Read();
            }
            int num = this.input_buffer;
            this.input_buffer = 0;
            return num;
        }

        public bool NextToken()
        {
            this.fsm_context.Return = false;
            while (true)
            {
                StateHandler handler = fsm_handler_table[this.state - 1];
                if (!handler(this.fsm_context))
                {
                    throw new JsonException(this.input_char);
                }
                if (this.end_of_input)
                {
                    return false;
                }
                if (this.fsm_context.Return)
                {
                    this.string_value = this.string_buffer.ToString();
                    this.string_buffer.Remove(0, this.string_buffer.Length);
                    this.token = fsm_return_table[this.state - 1];
                    if (this.token == 0x10006)
                    {
                        this.token = this.input_char;
                    }
                    this.state = this.fsm_context.NextState;
                    return true;
                }
                this.state = this.fsm_context.NextState;
            }
        }

        private static void PopulateFsmTables()
        {
            StateHandler[] handlerArray1 = new StateHandler[0x1c];
            handlerArray1[0] = new StateHandler(Lexer.State1);
            handlerArray1[1] = new StateHandler(Lexer.State2);
            handlerArray1[2] = new StateHandler(Lexer.State3);
            handlerArray1[3] = new StateHandler(Lexer.State4);
            handlerArray1[4] = new StateHandler(Lexer.State5);
            handlerArray1[5] = new StateHandler(Lexer.State6);
            handlerArray1[6] = new StateHandler(Lexer.State7);
            handlerArray1[7] = new StateHandler(Lexer.State8);
            handlerArray1[8] = new StateHandler(Lexer.State9);
            handlerArray1[9] = new StateHandler(Lexer.State10);
            handlerArray1[10] = new StateHandler(Lexer.State11);
            handlerArray1[11] = new StateHandler(Lexer.State12);
            handlerArray1[12] = new StateHandler(Lexer.State13);
            handlerArray1[13] = new StateHandler(Lexer.State14);
            handlerArray1[14] = new StateHandler(Lexer.State15);
            handlerArray1[15] = new StateHandler(Lexer.State16);
            handlerArray1[0x10] = new StateHandler(Lexer.State17);
            handlerArray1[0x11] = new StateHandler(Lexer.State18);
            handlerArray1[0x12] = new StateHandler(Lexer.State19);
            handlerArray1[0x13] = new StateHandler(Lexer.State20);
            handlerArray1[20] = new StateHandler(Lexer.State21);
            handlerArray1[0x15] = new StateHandler(Lexer.State22);
            handlerArray1[0x16] = new StateHandler(Lexer.State23);
            handlerArray1[0x17] = new StateHandler(Lexer.State24);
            handlerArray1[0x18] = new StateHandler(Lexer.State25);
            handlerArray1[0x19] = new StateHandler(Lexer.State26);
            handlerArray1[0x1a] = new StateHandler(Lexer.State27);
            handlerArray1[0x1b] = new StateHandler(Lexer.State28);
            fsm_handler_table = handlerArray1;
            fsm_return_table = new int[] { 
                0x10006, 0, 0x10001, 0x10001, 0, 0x10001, 0, 0x10001, 0, 0, 0x10002, 0, 0, 0, 0x10003, 0,
                0, 0x10004, 0x10005, 0x10006, 0, 0, 0x10005, 0x10006, 0, 0, 0, 0
            };
        }

        private static char ProcessEscChar(int esc_char)
        {
            if (esc_char > 0x5c)
            {
                if (esc_char <= 0x66)
                {
                    if (esc_char == 0x62)
                    {
                        return '\b';
                    }
                    if (esc_char == 0x66)
                    {
                        return '\f';
                    }
                }
                else
                {
                    if (esc_char == 110)
                    {
                        return '\n';
                    }
                    if (esc_char == 0x72)
                    {
                        return '\r';
                    }
                    if (esc_char == 0x74)
                    {
                        return '\t';
                    }
                }
            }
            else if (esc_char > 0x27)
            {
                if ((esc_char == 0x2f) || (esc_char == 0x5c))
                {
                    goto TR_0001;
                }
            }
            else if ((esc_char == 0x22) || (esc_char == 0x27))
            {
                goto TR_0001;
            }
            return '?';
        TR_0001:
            return Convert.ToChar(esc_char);
        }

        private static bool State1(FsmContext ctx)
        {
            while (true)
            {
                if (!ctx.L.GetChar())
                {
                    return true;
                }
                if ((ctx.L.input_char != 0x20) && ((ctx.L.input_char < 9) || (ctx.L.input_char > 13)))
                {
                    if ((ctx.L.input_char >= 0x31) && (ctx.L.input_char <= 0x39))
                    {
                        ctx.L.string_buffer.Append((char) ctx.L.input_char);
                        ctx.NextState = 3;
                        return true;
                    }
                    int num = ctx.L.input_char;
                    if (num > 0x5b)
                    {
                        if (num > 110)
                        {
                            if (num == 0x74)
                            {
                                ctx.NextState = 9;
                                return true;
                            }
                            if ((num != 0x7b) && (num != 0x7d))
                            {
                                goto TR_0002;
                            }
                        }
                        else if (num != 0x5d)
                        {
                            if (num == 0x66)
                            {
                                ctx.NextState = 12;
                                return true;
                            }
                            if (num == 110)
                            {
                                ctx.NextState = 0x10;
                                return true;
                            }
                            goto TR_0002;
                        }
                    }
                    else
                    {
                        if (num > 0x27)
                        {
                            switch (num)
                            {
                                case 0x2c:
                                    goto TR_0009;

                                case 0x2d:
                                    ctx.L.string_buffer.Append((char) ctx.L.input_char);
                                    ctx.NextState = 2;
                                    return true;

                                case 0x2e:
                                    break;

                                case 0x2f:
                                    if (!ctx.L.allow_comments)
                                    {
                                        return false;
                                    }
                                    ctx.NextState = 0x19;
                                    return true;

                                case 0x30:
                                    ctx.L.string_buffer.Append((char) ctx.L.input_char);
                                    ctx.NextState = 4;
                                    return true;

                                default:
                                    if ((num != 0x3a) && (num != 0x5b))
                                    {
                                        break;
                                    }
                                    goto TR_0009;
                            }
                        }
                        else
                        {
                            if (num == 0x22)
                            {
                                ctx.NextState = 0x13;
                                ctx.Return = true;
                                return true;
                            }
                            if (num == 0x27)
                            {
                                if (!ctx.L.allow_single_quoted_strings)
                                {
                                    return false;
                                }
                                ctx.L.input_char = 0x22;
                                ctx.NextState = 0x17;
                                ctx.Return = true;
                                return true;
                            }
                        }
                        goto TR_0002;
                    }
                    break;
                }
            }
            goto TR_0009;
        TR_0002:
            return false;
        TR_0009:
            ctx.NextState = 1;
            ctx.Return = true;
            return true;
        }

        private static bool State10(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x75)
            {
                return false;
            }
            ctx.NextState = 11;
            return true;
        }

        private static bool State11(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x65)
            {
                return false;
            }
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        }

        private static bool State12(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x61)
            {
                return false;
            }
            ctx.NextState = 13;
            return true;
        }

        private static bool State13(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x6c)
            {
                return false;
            }
            ctx.NextState = 14;
            return true;
        }

        private static bool State14(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x73)
            {
                return false;
            }
            ctx.NextState = 15;
            return true;
        }

        private static bool State15(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x65)
            {
                return false;
            }
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        }

        private static bool State16(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x75)
            {
                return false;
            }
            ctx.NextState = 0x11;
            return true;
        }

        private static bool State17(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x6c)
            {
                return false;
            }
            ctx.NextState = 0x12;
            return true;
        }

        private static bool State18(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x6c)
            {
                return false;
            }
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        }

        private static bool State19(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                int num = ctx.L.input_char;
                if (num == 0x22)
                {
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 20;
                    return true;
                }
                if (num == 0x5c)
                {
                    ctx.StateStack = 0x13;
                    ctx.NextState = 0x15;
                    return true;
                }
                ctx.L.string_buffer.Append((char) ctx.L.input_char);
            }
            return true;
        }

        private static bool State2(FsmContext ctx)
        {
            ctx.L.GetChar();
            if ((ctx.L.input_char >= 0x31) && (ctx.L.input_char <= 0x39))
            {
                ctx.L.string_buffer.Append((char) ctx.L.input_char);
                ctx.NextState = 3;
                return true;
            }
            if (ctx.L.input_char != 0x30)
            {
                return false;
            }
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 4;
            return true;
        }

        private static bool State20(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x22)
            {
                return false;
            }
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        }

        private static bool State21(FsmContext ctx)
        {
            ctx.L.GetChar();
            int num = ctx.L.input_char;
            if (num > 0x5c)
            {
                if (num > 0x66)
                {
                    if (num != 110)
                    {
                        switch (num)
                        {
                            case 0x72:
                            case 0x74:
                                break;

                            case 0x75:
                                ctx.NextState = 0x16;
                                return true;

                            default:
                                goto TR_0000;
                        }
                    }
                }
                else if ((num != 0x62) && (num != 0x66))
                {
                    goto TR_0000;
                }
                goto TR_0001;
            }
            else if (num > 0x27)
            {
                if ((num == 0x2f) || (num == 0x5c))
                {
                    goto TR_0001;
                }
            }
            else if ((num == 0x22) || (num == 0x27))
            {
                goto TR_0001;
            }
        TR_0000:
            return false;
        TR_0001:
            ctx.L.string_buffer.Append(ProcessEscChar(ctx.L.input_char));
            ctx.NextState = ctx.StateStack;
            return true;
        }

        private static bool State22(FsmContext ctx)
        {
            int num = 0;
            int num2 = 0x1000;
            ctx.L.unichar = 0;
            while (ctx.L.GetChar())
            {
                if ((((ctx.L.input_char < 0x30) || (ctx.L.input_char > 0x39)) && ((ctx.L.input_char < 0x41) || (ctx.L.input_char > 70))) && ((ctx.L.input_char < 0x61) || (ctx.L.input_char > 0x66)))
                {
                    return false;
                }
                ctx.L.unichar += HexValue(ctx.L.input_char) * num2;
                num2 /= 0x10;
                if ((num + 1) == 4)
                {
                    ctx.L.string_buffer.Append(Convert.ToChar(ctx.L.unichar));
                    ctx.NextState = ctx.StateStack;
                    return true;
                }
            }
            return true;
        }

        private static bool State23(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                int num = ctx.L.input_char;
                if (num == 0x27)
                {
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 0x18;
                    return true;
                }
                if (num == 0x5c)
                {
                    ctx.StateStack = 0x17;
                    ctx.NextState = 0x15;
                    return true;
                }
                ctx.L.string_buffer.Append((char) ctx.L.input_char);
            }
            return true;
        }

        private static bool State24(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x27)
            {
                return false;
            }
            ctx.L.input_char = 0x22;
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        }

        private static bool State25(FsmContext ctx)
        {
            ctx.L.GetChar();
            int num = ctx.L.input_char;
            if (num == 0x2a)
            {
                ctx.NextState = 0x1b;
                return true;
            }
            if (num != 0x2f)
            {
                return false;
            }
            ctx.NextState = 0x1a;
            return true;
        }

        private static bool State26(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == 10)
                {
                    ctx.NextState = 1;
                    return true;
                }
            }
            return true;
        }

        private static bool State27(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == 0x2a)
                {
                    ctx.NextState = 0x1c;
                    return true;
                }
            }
            return true;
        }

        private static bool State28(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char != 0x2a)
                {
                    if (ctx.L.input_char == 0x2f)
                    {
                        ctx.NextState = 1;
                        return true;
                    }
                    ctx.NextState = 0x1b;
                    return true;
                }
            }
            return true;
        }

        private static bool State3(FsmContext ctx)
        {
            while (true)
            {
                if (!ctx.L.GetChar())
                {
                    return true;
                }
                if ((ctx.L.input_char >= 0x30) && (ctx.L.input_char <= 0x39))
                {
                    ctx.L.string_buffer.Append((char) ctx.L.input_char);
                    continue;
                }
                if (ctx.L.input_char == 0x20)
                {
                    goto TR_0002;
                }
                else
                {
                    if ((ctx.L.input_char >= 9) && (ctx.L.input_char <= 13))
                    {
                        goto TR_0002;
                    }
                    int num = ctx.L.input_char;
                    if (num > 0x45)
                    {
                        if (num != 0x5d)
                        {
                            if (num == 0x65)
                            {
                                break;
                            }
                            if (num != 0x7d)
                            {
                                goto TR_0003;
                            }
                        }
                    }
                    else if (num != 0x2c)
                    {
                        if (num == 0x2e)
                        {
                            ctx.L.string_buffer.Append((char) ctx.L.input_char);
                            ctx.NextState = 5;
                            return true;
                        }
                        if (num == 0x45)
                        {
                            break;
                        }
                        goto TR_0003;
                    }
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }
                break;
            }
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 7;
            return true;
        TR_0002:
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        TR_0003:
            return false;
        }

        private static bool State4(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char == 0x20)
            {
                goto TR_0000;
            }
            else
            {
                if ((ctx.L.input_char >= 9) && (ctx.L.input_char <= 13))
                {
                    goto TR_0000;
                }
                int num = ctx.L.input_char;
                if (num > 0x45)
                {
                    if (num != 0x5d)
                    {
                        if (num == 0x65)
                        {
                            goto TR_0002;
                        }
                        else if (num != 0x7d)
                        {
                            goto TR_0001;
                        }
                    }
                }
                else if (num != 0x2c)
                {
                    if (num == 0x2e)
                    {
                        ctx.L.string_buffer.Append((char) ctx.L.input_char);
                        ctx.NextState = 5;
                        return true;
                    }
                    if (num != 0x45)
                    {
                        goto TR_0001;
                    }
                    goto TR_0002;
                }
                ctx.L.UngetChar();
                ctx.Return = true;
                ctx.NextState = 1;
                return true;
            }
            goto TR_0002;
        TR_0000:
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        TR_0001:
            return false;
        TR_0002:
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 7;
            return true;
        }

        private static bool State5(FsmContext ctx)
        {
            ctx.L.GetChar();
            if ((ctx.L.input_char < 0x30) || (ctx.L.input_char > 0x39))
            {
                return false;
            }
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 6;
            return true;
        }

        private static bool State6(FsmContext ctx)
        {
            while (true)
            {
                if (!ctx.L.GetChar())
                {
                    return true;
                }
                if ((ctx.L.input_char >= 0x30) && (ctx.L.input_char <= 0x39))
                {
                    ctx.L.string_buffer.Append((char) ctx.L.input_char);
                    continue;
                }
                if (ctx.L.input_char == 0x20)
                {
                    goto TR_0002;
                }
                else
                {
                    if ((ctx.L.input_char >= 9) && (ctx.L.input_char <= 13))
                    {
                        goto TR_0002;
                    }
                    int num = ctx.L.input_char;
                    if (num > 0x45)
                    {
                        if (num != 0x5d)
                        {
                            if (num == 0x65)
                            {
                                break;
                            }
                            if (num != 0x7d)
                            {
                                goto TR_0003;
                            }
                        }
                    }
                    else if (num != 0x2c)
                    {
                        if (num == 0x45)
                        {
                            break;
                        }
                        goto TR_0003;
                    }
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }
                break;
            }
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 7;
            return true;
        TR_0002:
            ctx.Return = true;
            ctx.NextState = 1;
            return true;
        TR_0003:
            return false;
        }

        private static bool State7(FsmContext ctx)
        {
            ctx.L.GetChar();
            if ((ctx.L.input_char >= 0x30) && (ctx.L.input_char <= 0x39))
            {
                ctx.L.string_buffer.Append((char) ctx.L.input_char);
                ctx.NextState = 8;
                return true;
            }
            int num = ctx.L.input_char;
            if ((num != 0x2b) && (num != 0x2d))
            {
                return false;
            }
            ctx.L.string_buffer.Append((char) ctx.L.input_char);
            ctx.NextState = 8;
            return true;
        }

        private static bool State8(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if ((ctx.L.input_char < 0x30) || (ctx.L.input_char > 0x39))
                {
                    if ((ctx.L.input_char == 0x20) || ((ctx.L.input_char >= 9) && (ctx.L.input_char <= 13)))
                    {
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;
                    }
                    int num = ctx.L.input_char;
                    if (((num != 0x2c) && (num != 0x5d)) && (num != 0x7d))
                    {
                        return false;
                    }
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }
                ctx.L.string_buffer.Append((char) ctx.L.input_char);
            }
            return true;
        }

        private static bool State9(FsmContext ctx)
        {
            ctx.L.GetChar();
            if (ctx.L.input_char != 0x72)
            {
                return false;
            }
            ctx.NextState = 10;
            return true;
        }

        private void UngetChar()
        {
            this.input_buffer = this.input_char;
        }

        public bool AllowComments
        {
            get => 
                this.allow_comments;
            set => 
                (this.allow_comments = value);
        }

        public bool AllowSingleQuotedStrings
        {
            get => 
                this.allow_single_quoted_strings;
            set => 
                (this.allow_single_quoted_strings = value);
        }

        public bool EndOfInput =>
            this.end_of_input;

        public int Token =>
            this.token;

        public string StringValue =>
            this.string_value;

        private delegate bool StateHandler(FsmContext ctx);
    }
}

