namespace Sandbox.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using VRage.Game.Models;
    using VRageMath;

    internal class MyModelObj
    {
        public List<Vector3> Vertexes = new List<Vector3>();
        public List<Vector3> Normals = new List<Vector3>();
        public List<MyTriangleVertexIndices> Triangles = new List<MyTriangleVertexIndices>();

        public MyModelObj(string filename)
        {
            foreach (string[] strArray in this.GetLineTokens(filename))
            {
                this.ParseObjLine(strArray);
            }
        }

        [IteratorStateMachine(typeof(<GetLineTokens>d__4))]
        private IEnumerable<string[]> GetLineTokens(string filename)
        {
            throw new Exception();
        }

        private void ParseObjLine(string[] lineTokens)
        {
            string str = lineTokens[0].ToLower();
            if (str == "v")
            {
                this.Vertexes.Add(ParseVector3(lineTokens));
            }
            else if (str == "vn")
            {
                this.Normals.Add(ParseVector3(lineTokens));
            }
            else if (str == "f")
            {
                int[] numArray = new int[3];
                for (int i = 1; i <= 3; i++)
                {
                    char[] separator = new char[] { '/' };
                    string[] strArray = lineTokens[i].Split(separator);
                    if (strArray.Length != 0)
                    {
                        numArray[i - 1] = int.Parse(strArray[0], CultureInfo.InvariantCulture);
                    }
                }
                this.Triangles.Add(new MyTriangleVertexIndices(numArray[0] - 1, numArray[1] - 1, numArray[2] - 1));
            }
        }

        private static Vector3 ParseVector3(string[] lineTokens) => 
            new Vector3(float.Parse(lineTokens[1], CultureInfo.InvariantCulture), float.Parse(lineTokens[2], CultureInfo.InvariantCulture), float.Parse(lineTokens[3], CultureInfo.InvariantCulture));

        [CompilerGenerated]
        private sealed class <GetLineTokens>d__4 : IEnumerable<string[]>, IEnumerable, IEnumerator<string[]>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string[] <>2__current;
            private int <>l__initialThreadId;
            private string filename;
            public string <>3__filename;
            private StreamReader <reader>5__2;
            private int <lineNumber>5__3;

            [DebuggerHidden]
            public <GetLineTokens>d__4(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<reader>5__2 != null)
                {
                    this.<reader>5__2.Dispose();
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
                        this.<reader>5__2 = new StreamReader(this.filename);
                        this.<>1__state = -3;
                        this.<lineNumber>5__3 = 1;
                        goto TR_0009;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                        goto TR_000B;
                    }
                    else
                    {
                        flag = false;
                    }
                    return flag;
                TR_0009:
                    if (!this.<reader>5__2.EndOfStream)
                    {
                        string[] strArray = Regex.Split(this.<reader>5__2.ReadLine().Trim(), @"\s+");
                        if (strArray.Length == 0)
                        {
                            goto TR_000B;
                        }
                        else if ((strArray[0] != string.Empty) && !strArray[0].StartsWith("#"))
                        {
                            this.<>2__current = strArray;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        else
                        {
                            goto TR_000B;
                        }
                    }
                    else
                    {
                        this.<>m__Finally1();
                        this.<reader>5__2 = null;
                        flag = false;
                    }
                    return flag;
                TR_000B:
                    while (true)
                    {
                        int num2 = this.<lineNumber>5__3;
                        this.<lineNumber>5__3 = num2 + 1;
                        break;
                    }
                    goto TR_0009;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator()
            {
                MyModelObj.<GetLineTokens>d__4 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MyModelObj.<GetLineTokens>d__4(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.filename = this.<>3__filename;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String[]>.GetEnumerator();

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

            string[] IEnumerator<string[]>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

    internal class GetLineTokens
    {
    }
}
}

