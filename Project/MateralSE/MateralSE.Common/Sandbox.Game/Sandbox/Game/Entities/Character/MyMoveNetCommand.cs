namespace Sandbox.Game.Entities.Character
{
    using System;
    using VRageMath;

    internal class MyMoveNetCommand : IMyNetworkCommand
    {
        private MyCharacter m_character;
        private Vector3 m_move;
        private Quaternion m_rotation;

        public MyMoveNetCommand(MyCharacter character, ref Vector3 move, ref Quaternion rotation)
        {
            this.m_character = character;
            this.m_move = move;
            this.m_rotation = rotation;
        }

        public void Apply()
        {
            this.m_character.ApplyRotation(this.m_rotation);
            this.m_character.MoveAndRotate(this.m_move, Vector2.Zero, 0f);
            this.m_character.MoveAndRotateInternal(this.m_move, Vector2.Zero, 0f, Vector3.Zero);
        }

        public bool ExecuteBeforeMoveAndRotate =>
            false;
    }
}

