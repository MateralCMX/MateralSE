namespace Sandbox.Game.Weapons
{
    using System;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;

    public class MyToolBase : MyDeviceBase
    {
        protected Vector3 m_positionMuzzleLocal;
        protected Vector3D m_positionMuzzleWorld;

        public MyToolBase() : this(Vector3.Zero, MatrixD.Identity)
        {
        }

        public MyToolBase(Vector3 localMuzzlePosition, MatrixD matrix)
        {
            this.m_positionMuzzleLocal = localMuzzlePosition;
            this.OnWorldPositionChanged(matrix);
        }

        public override bool CanSwitchAmmoMagazine() => 
            false;

        public override Vector3D GetMuzzleLocalPosition() => 
            this.m_positionMuzzleLocal;

        public override Vector3D GetMuzzleWorldPosition() => 
            this.m_positionMuzzleWorld;

        public MyObjectBuilder_ToolBase GetObjectBuilder()
        {
            MyObjectBuilder_ToolBase local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolBase>();
            local1.InventoryItemId = base.InventoryItemId;
            return local1;
        }

        public void Init(MyObjectBuilder_ToolBase objectBuilder)
        {
            base.Init(objectBuilder);
        }

        public void OnWorldPositionChanged(MatrixD matrix)
        {
            this.m_positionMuzzleWorld = Vector3D.Transform(this.m_positionMuzzleLocal, matrix);
        }

        public override bool SwitchAmmoMagazineToNextAvailable() => 
            false;

        public override bool SwitchToNextAmmoMagazine() => 
            false;
    }
}

