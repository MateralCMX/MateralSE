namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity.UseObject;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyCharacterRaycastDetectorComponent : MyCharacterDetectorComponent
    {
        private readonly List<MyUseObjectsComponentBase> m_hitUseComponents = new List<MyUseObjectsComponentBase>();

        protected override void DoDetection(bool useHead)
        {
            Vector3D vectord2;
            Vector3D forward;
            if (ReferenceEquals(base.Character, MySession.Static.ControlledEntity))
            {
                MyHud.SelectedObjectHighlight.RemoveHighlight();
            }
            MatrixD xd = base.Character.GetHeadMatrix(false, true, false, false, false);
            Vector3D planePoint = xd.Translation - (xd.Forward * 0.3);
            if (useHead)
            {
                forward = xd.Forward;
                vectord2 = planePoint;
            }
            else
            {
                MatrixD worldMatrix = MySector.MainCamera.WorldMatrix;
                forward = worldMatrix.Forward;
                vectord2 = MyUtils.LinePlaneIntersection(planePoint, (Vector3) forward, worldMatrix.Translation, (Vector3) forward);
            }
            Vector3D to = vectord2 + (forward * MyConstants.DEFAULT_INTERACTIVE_DISTANCE);
            base.StartPosition = vectord2;
            LineD line = new LineD(vectord2, to);
            MyIntersectionResultLineTriangleEx? nullable = Sandbox.Game.Entities.MyEntities.GetIntersectionWithLine(ref line, base.Character, null, false, false, true, IntersectionFlags.ALL_TRIANGLES, 0f, true);
            bool flag = false;
            if (nullable != null)
            {
                IMyEntity currentEntity = nullable.Value.Entity;
                Vector3D intersectionPointInWorldSpace = nullable.Value.IntersectionPointInWorldSpace;
                if ((currentEntity is MyCubeGrid) && (nullable.Value.UserObject != null))
                {
                    MySlimBlock cubeBlock = (nullable.Value.UserObject as MyCube).CubeBlock;
                    if ((cubeBlock != null) && (cubeBlock.FatBlock != null))
                    {
                        currentEntity = cubeBlock.FatBlock;
                    }
                }
                this.m_hitUseComponents.Clear();
                IMyUseObject interactive = currentEntity as IMyUseObject;
                this.GetUseComponentsFromParentStructure(currentEntity, this.m_hitUseComponents);
                if ((interactive != null) || (this.m_hitUseComponents.Count > 0))
                {
                    if (this.m_hitUseComponents.Count <= 0)
                    {
                        base.HitMaterial = currentEntity.Physics.GetMaterialAt(base.HitPosition);
                        base.HitBody = currentEntity.Physics.RigidBody;
                    }
                    else
                    {
                        float maxValue = float.MaxValue;
                        double num2 = Vector3D.Distance(vectord2, intersectionPointInWorldSpace);
                        MyUseObjectsComponentBase base2 = null;
                        foreach (MyUseObjectsComponentBase base3 in this.m_hitUseComponents)
                        {
                            float num3;
                            IMyUseObject obj3 = base3.RaycastDetectors(vectord2, to, out num3);
                            num3 *= MyConstants.DEFAULT_INTERACTIVE_DISTANCE;
                            if ((Math.Abs(num3) < Math.Abs(maxValue)) && (num3 < num2))
                            {
                                maxValue = num3;
                                base2 = base3;
                                currentEntity = base3.Entity;
                                interactive = obj3;
                            }
                        }
                        if (base2 != null)
                        {
                            base.HitMaterial = base2.DetectorPhysics.GetMaterialAt(base.HitPosition);
                            this.HitBody = nullable.Value.Entity.Physics?.RigidBody;
                            base.HitPosition = intersectionPointInWorldSpace;
                            base.DetectedEntity = currentEntity;
                        }
                    }
                    if (interactive != null)
                    {
                        base.HitPosition = intersectionPointInWorldSpace;
                        base.DetectedEntity = currentEntity;
                        if ((ReferenceEquals(base.Character, MySession.Static.ControlledEntity) && (interactive.SupportedActions != UseActionEnum.None)) && !base.Character.IsOnLadder)
                        {
                            HandleInteractiveObject(interactive);
                            base.UseObject = interactive;
                            flag = true;
                        }
                        if (base.Character.IsOnLadder)
                        {
                            base.UseObject = null;
                        }
                    }
                }
            }
            if (!flag)
            {
                base.UseObject = null;
            }
        }

        private void GetUseComponentsFromParentStructure(IMyEntity currentEntity, List<MyUseObjectsComponentBase> useComponents)
        {
            MyUseObjectsComponentBase item = currentEntity.Components.Get<MyUseObjectsComponentBase>();
            if (item != null)
            {
                useComponents.Add(item);
            }
            if (currentEntity.Parent != null)
            {
                this.GetUseComponentsFromParentStructure(currentEntity.Parent, useComponents);
            }
        }
    }
}

