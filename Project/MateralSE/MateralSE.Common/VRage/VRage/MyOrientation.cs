namespace VRage
{
    using ProtoBuf;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct MyOrientation
    {
        [ProtoMember(14), XmlAttribute]
        public float Yaw;
        [ProtoMember(0x11), XmlAttribute]
        public float Pitch;
        [ProtoMember(20), XmlAttribute]
        public float Roll;
        public MyOrientation(float yaw, float pitch, float roll)
        {
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.Roll = roll;
        }

        public Quaternion ToQuaternion() => 
            Quaternion.CreateFromYawPitchRoll(this.Yaw, this.Pitch, this.Roll);

        public override bool Equals(object obj)
        {
            if (!(obj is MyOrientation))
            {
                return false;
            }
            MyOrientation orientation = (MyOrientation) obj;
            return (this == orientation);
        }

        public override int GetHashCode() => 
            ((((((int) (this.Yaw * 997f)) * 0x18d) ^ ((int) (this.Pitch * 997f))) * 0x18d) ^ ((int) (this.Roll * 997f)));

        public static bool operator ==(MyOrientation value1, MyOrientation value2) => 
            ((value1.Yaw == value2.Yaw) && ((value1.Pitch == value2.Pitch) && (value1.Roll == value2.Roll)));

        public static bool operator !=(MyOrientation value1, MyOrientation value2) => 
            ((value1.Yaw != value2.Yaw) || ((value1.Pitch != value2.Pitch) || (value1.Roll != value2.Roll)));
    }
}

