namespace VRage.Game.ModAPI.Ingame.Utilities
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyIniParseResult
    {
        private int m_lineNo;
        private readonly TextPtr m_ptr;
        public readonly string Error;
        public static bool operator ==(MyIniParseResult a, MyIniParseResult b) => 
            a.Equals(b);

        public static bool operator !=(MyIniParseResult a, MyIniParseResult b) => 
            !a.Equals(b);

        public bool Equals(MyIniParseResult other) => 
            ((this.LineNo == other.LineNo) && string.Equals(this.Error, other.Error));

        public override bool Equals(object obj) => 
            ((obj != null) ? ((obj is MyIniParseResult) && this.Equals((MyIniParseResult) obj)) : false);

        public override int GetHashCode() => 
            ((this.LineNo * 0x18d) ^ ((this.Error != null) ? this.Error.GetHashCode() : 0));

        internal MyIniParseResult(TextPtr ptr, string error)
        {
            this.m_lineNo = 0;
            this.m_ptr = ptr;
            this.Error = error;
        }

        public int LineNo
        {
            get
            {
                if (this.m_lineNo == 0)
                {
                    this.m_lineNo = this.m_ptr.FindLineNo();
                }
                return this.m_lineNo;
            }
        }
        public bool Success =>
            (this.IsDefined && ReferenceEquals(this.Error, null));
        public bool IsDefined =>
            this.m_ptr.IsEmpty;
        public override string ToString() => 
            (this.Success ? "Success" : $"Line {this.LineNo}: {this.Error}");
    }
}

