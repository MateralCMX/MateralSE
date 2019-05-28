namespace VRage.Profiler
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyProfilerBlockKey : IEquatable<MyProfilerBlockKey>
    {
        public string File;
        public string Member;
        public string Name;
        public int Line;
        public int ParentId;
        public int HashCode;
        public MyProfilerBlockKey(string file, string member, string name, int line, int parentId)
        {
            this.File = file;
            this.Member = member;
            this.Name = name;
            this.Line = line;
            this.ParentId = parentId;
            this.HashCode = file.GetHashCode();
            this.HashCode = (0x18d * this.HashCode) ^ member.GetHashCode();
            this.HashCode = (0x18d * this.HashCode) ^ (name ?? string.Empty).GetHashCode();
            this.HashCode = (0x18d * this.HashCode) ^ parentId.GetHashCode();
        }

        public bool IsSameLocation(MyProfilerBlockKey obj) => 
            ((this.Name == obj.Name) && (this.Member == obj.Member));

        public bool IsSimilarLocation(MyProfilerBlockKey obj)
        {
            int num;
            int index = this.File.IndexOf("Sources");
            if (obj.File.IndexOf("Sources") == -1)
            {
                num = 0;
            }
            if (index == -1)
            {
                index = 0;
            }
            return (this.IsSameLocation(obj) && ((this.File.Substring(index) == obj.File.Substring(num)) && (Math.Abs((int) (this.Line - obj.Line)) < 40)));
        }

        public bool Equals(MyProfilerBlockKey obj) => 
            ((this.ParentId == obj.ParentId) && ((this.File == obj.File) && ((this.Line == obj.Line) && this.IsSameLocation(obj))));

        public override int GetHashCode() => 
            this.HashCode;
    }
}

