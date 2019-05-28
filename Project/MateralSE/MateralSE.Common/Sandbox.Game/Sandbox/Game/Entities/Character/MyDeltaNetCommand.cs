namespace Sandbox.Game.Entities.Character
{
    using System;
    using VRageMath;

    internal class MyDeltaNetCommand : IMyNetworkCommand
    {
        private MyCharacter m_character;
        private Vector3D m_delta;

        public MyDeltaNetCommand(MyCharacter character, ref Vector3D delta)
        {
            this.m_character = character;
            this.m_delta = delta;
        }

        public unsafe void Apply()
        {
            MatrixD worldMatrix = this.m_character.WorldMatrix;
            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
            xdPtr1.Translation += this.m_delta;
            this.m_character.PositionComp.SetWorldMatrix(worldMatrix, null, true, true, true, false, false, false);
        }

        public bool ExecuteBeforeMoveAndRotate =>
            true;
    }
}

