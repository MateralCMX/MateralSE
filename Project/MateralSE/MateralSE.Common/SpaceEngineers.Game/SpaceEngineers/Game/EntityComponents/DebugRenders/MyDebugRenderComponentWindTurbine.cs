namespace SpaceEngineers.Game.EntityComponents.DebugRenders
{
    using Sandbox.Game.Components;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponentWindTurbine : MyDebugRenderComponent
    {
        public MyDebugRenderComponentWindTurbine(MyWindTurbine turbine) : base(turbine)
        {
        }

        public override void DebugDraw()
        {
            base.DebugDraw();
            float[] rayEffectivities = this.Entity.RayEffectivities;
            for (int i = 0; i < rayEffectivities.Length; i++)
            {
                Vector3D vectord;
                Vector3D vectord2;
                this.Entity.GetRaycaster(i, out vectord, out vectord2);
                Vector3D vectord3 = Vector3D.Lerp(vectord, vectord2, (double) this.Entity.BlockDefinition.MinRaycasterClearance);
                Vector3D pointTo = Vector3D.Lerp(vectord3, vectord2, (double) rayEffectivities[i]);
                MyRenderProxy.DebugDrawText3D(vectord2, rayEffectivities[i].ToString("F2"), Color.Green, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawLine3D(vectord, vectord3, Color.Black, Color.Black, false, false);
                MyRenderProxy.DebugDrawLine3D(vectord3, pointTo, Color.Green, Color.Green, false, false);
                MyRenderProxy.DebugDrawLine3D(pointTo, vectord2, Color.Red, Color.Red, false, false);
            }
        }

        public MyWindTurbine Entity =>
            ((MyWindTurbine) base.Entity);
    }
}

