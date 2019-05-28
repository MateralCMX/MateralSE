namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Game.Weapons;
    using System;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentAutomaticRifle : MyRenderComponent
    {
        private static readonly MyStringId ID_MUZZLE_FLASH_SIDE = MyStringId.GetOrCompute("MuzzleFlashMachineGunSide");
        private static readonly MyStringId ID_MUZZLE_FLASH_FRONT = MyStringId.GetOrCompute("MuzzleFlashMachineGunFront");
        private MyAutomaticRifleGun m_rifleGun;

        public override void Draw()
        {
            int num = MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_rifleGun.LastTimeShoot;
            MyGunBase gunBase = this.m_rifleGun.GunBase;
            if (gunBase.UseDefaultMuzzleFlash && (num <= gunBase.MuzzleFlashLifeSpan))
            {
                GenerateMuzzleFlash(gunBase.GetMuzzleWorldPosition(), (Vector3) gunBase.GetMuzzleWorldMatrix().Forward, 0.1f, 0.3f);
            }
        }

        public static void GenerateMuzzleFlash(Vector3D position, Vector3 dir, float radius, float length)
        {
            GenerateMuzzleFlash(position, dir, uint.MaxValue, ref MatrixD.Zero, radius, length);
        }

        public static void GenerateMuzzleFlash(Vector3D position, Vector3 dir, uint renderObjectID, ref MatrixD worldToLocal, float radius, float length)
        {
            float angle = MyParticlesManager.Paused ? 0f : MyUtils.GetRandomFloat(0f, 1.570796f);
            float x = 10f;
            Vector4 color = new Vector4(x, x, x, 1f);
            MyTransparentGeometry.AddLineBillboard(ID_MUZZLE_FLASH_SIDE, color, position, renderObjectID, ref worldToLocal, dir, length, 0.15f, MyBillboard.BlendTypeEnum.AdditiveBottom, -1, 1f, null);
            MyTransparentGeometry.AddPointBillboard(ID_MUZZLE_FLASH_FRONT, color, position, renderObjectID, ref worldToLocal, radius, angle, -1, MyBillboard.BlendTypeEnum.AdditiveBottom, 1f, null);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_rifleGun = base.Container.Entity as MyAutomaticRifleGun;
        }
    }
}

