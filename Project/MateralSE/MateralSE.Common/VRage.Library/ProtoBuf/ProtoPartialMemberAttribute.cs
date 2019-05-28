namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public class ProtoPartialMemberAttribute : ProtoMemberAttribute
    {
        private readonly string memberName;

        public ProtoPartialMemberAttribute(int tag, string memberName) : base(0)
        {
            if (Helpers.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException("memberName");
            }
            this.memberName = memberName;
        }

        public string MemberName =>
            this.memberName;
    }
}

