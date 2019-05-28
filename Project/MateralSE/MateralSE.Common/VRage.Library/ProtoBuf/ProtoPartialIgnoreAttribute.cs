namespace ProtoBuf
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
    public class ProtoPartialIgnoreAttribute : ProtoIgnoreAttribute
    {
        private readonly string memberName;

        public ProtoPartialIgnoreAttribute(string memberName)
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

