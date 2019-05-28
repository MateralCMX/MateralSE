namespace Sandbox.Game.Replication.ClientStates
{
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Library.Collections;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyCharacterClientState
    {
        public bool Valid;
        public float HeadX;
        public float HeadY;
        public MyCharacterMovementEnum MovementState;
        public bool Jetpack;
        public bool Dampeners;
        public bool TargetFromCamera;
        public Vector3 MoveIndicator;
        public Quaternion Rotation;
        public MyCharacterMovementFlags MovementFlags;
        public HkCharacterStateType CharacterState;
        public Vector3 SupportNormal;
        public float MovementSpeed;
        public Vector3 MovementDirection;
        public bool IsOnLadder;
        public MyCharacterClientState(BitStream stream)
        {
            this.HeadX = stream.ReadHalf();
            if (!this.HeadX.IsValid())
            {
                this.HeadX = 0f;
            }
            this.HeadY = stream.ReadHalf();
            this.MovementState = (MyCharacterMovementEnum) stream.ReadUInt16(0x10);
            this.MovementFlags = (MyCharacterMovementFlags) ((byte) stream.ReadUInt16(0x10));
            this.Jetpack = stream.ReadBool();
            this.Dampeners = stream.ReadBool();
            this.TargetFromCamera = stream.ReadBool();
            this.MoveIndicator = stream.ReadNormalizedSignedVector3(8);
            this.Rotation = stream.ReadQuaternion();
            this.CharacterState = (HkCharacterStateType) stream.ReadByte(8);
            this.SupportNormal = stream.ReadVector3();
            this.MovementSpeed = stream.ReadFloat();
            this.MovementDirection = stream.ReadVector3();
            this.IsOnLadder = stream.ReadBool();
            this.Valid = true;
        }

        public void Serialize(BitStream stream)
        {
            stream.WriteHalf(this.HeadX);
            stream.WriteHalf(this.HeadY);
            stream.WriteUInt16((ushort) this.MovementState, 0x10);
            stream.WriteUInt16((ushort) this.MovementFlags, 0x10);
            stream.WriteBool(this.Jetpack);
            stream.WriteBool(this.Dampeners);
            stream.WriteBool(this.TargetFromCamera);
            stream.WriteNormalizedSignedVector3(this.MoveIndicator, 8);
            stream.WriteQuaternion(this.Rotation);
            stream.WriteByte((byte) this.CharacterState, 8);
            stream.Write(this.SupportNormal);
            stream.WriteFloat(this.MovementSpeed);
            stream.Write(this.MovementDirection);
            stream.WriteBool(this.IsOnLadder);
        }
    }
}

