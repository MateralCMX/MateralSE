namespace Sandbox.Game.Entities.Character.Components
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyCharacterShapecastDetectorComponent : MyCharacterDetectorComponent
    {
        public const float DEFAULT_SHAPE_RADIUS = 0.1f;
        private List<MyPhysics.HitInfo> m_hits = new List<MyPhysics.HitInfo>();
        private Vector3D m_rayOrigin = Vector3D.Zero;
        private Vector3D m_rayDirection = Vector3D.Zero;

        public MyCharacterShapecastDetectorComponent()
        {
            this.ShapeRadius = 0.1f;
        }

        private int CompareHits(MyPhysics.HitInfo info1, MyPhysics.HitInfo info2)
        {
            System.Type type = info1.HkHitInfo.GetHitEntity().GetType();
            System.Type type2 = info2.HkHitInfo.GetHitEntity().GetType();
            if (type != type2)
            {
                System.Type type3 = typeof(MyVoxelMap);
                if (type == type3)
                {
                    return 1;
                }
                if (type2 == type3)
                {
                    return -1;
                }
                System.Type type4 = typeof(MyVoxelPhysics);
                if (type == type4)
                {
                    return 1;
                }
                if (type2 == type4)
                {
                    return -1;
                }
                System.Type type5 = typeof(MyCubeGrid);
                if (type == type5)
                {
                    return 1;
                }
                if (type2 == type5)
                {
                    return -1;
                }
            }
            Vector3D vectord = info1.Position - this.m_rayOrigin;
            Vector3D vectord2 = info2.Position - this.m_rayOrigin;
            int num3 = Vector3.Dot((Vector3) this.m_rayDirection, Vector3.Normalize(vectord2)).CompareTo(Vector3.Dot((Vector3) this.m_rayDirection, Vector3.Normalize(vectord)));
            if (num3 != 0)
            {
                return num3;
            }
            int num4 = vectord2.LengthSquared().CompareTo(vectord.LengthSquared());
            return ((num4 == 0) ? 0 : num4);
        }

        protected override void DoDetection(bool useHead)
        {
            this.DoDetection(useHead, false);
        }

        private void DoDetection(bool useHead, bool doModelIntersection)
        {
            bool flag;
            if (ReferenceEquals(base.Character, MySession.Static.ControlledEntity))
            {
                MyHud.SelectedObjectHighlight.RemoveHighlight();
            }
            MatrixD xd = base.Character.GetHeadMatrix(false, true, false, false, false);
            Vector3D translation = xd.Translation;
            Vector3D forward = xd.Forward;
            if (!useHead)
            {
                Vector3D planePoint = xd.Translation - (xd.Forward * 0.3);
                if (!ReferenceEquals(base.Character, MySession.Static.LocalCharacter))
                {
                    translation = planePoint;
                    forward = xd.Forward;
                }
                else
                {
                    translation = MySector.MainCamera.WorldMatrix.Translation;
                    forward = MySector.MainCamera.WorldMatrix.Forward;
                    translation = MyUtils.LinePlaneIntersection(planePoint, (Vector3) forward, translation, (Vector3) forward);
                }
            }
            Vector3D to = translation + (forward * 2.5);
            base.StartPosition = translation;
            MatrixD transform = MatrixD.CreateTranslation(translation);
            HkShape shape = (HkShape) new HkSphereShape(this.ShapeRadius);
            IMyEntity objA = null;
            base.ShapeKey = uint.MaxValue;
            base.HitPosition = Vector3D.Zero;
            base.HitNormal = Vector3.Zero;
            base.HitMaterial = MyStringHash.NullOrEmpty;
            base.HitTag = null;
            this.m_hits.Clear();
            Vector3 zero = (Vector3) Vector3D.Zero;
            try
            {
                bool flag2;
                bool flag3;
                int num;
                base.EnableDetectorsInArea(translation);
                MyPhysics.CastShapeReturnContactBodyDatas(to, shape, ref transform, 0, 0f, this.m_hits, true);
                this.m_rayOrigin = translation;
                this.m_rayDirection = forward;
                this.m_hits.Sort(new Comparison<MyPhysics.HitInfo>(this.CompareHits));
                if (this.m_hits.Count <= 0)
                {
                    goto TR_0012;
                }
                else
                {
                    flag2 = false;
                    flag3 = false;
                    num = 0;
                }
                goto TR_002F;
            TR_0017:
                num++;
            TR_002F:
                while (true)
                {
                    if (num >= this.m_hits.Count)
                    {
                        break;
                    }
                    HkRigidBody hkEntity = this.m_hits[num].HkHitInfo.Body;
                    IMyEntity hitEntity = this.m_hits[num].HkHitInfo.GetHitEntity();
                    if (!ReferenceEquals(hitEntity, base.Character))
                    {
                        int num1;
                        if (hitEntity is MyEntitySubpart)
                        {
                            hitEntity = hitEntity.Parent;
                        }
                        if (((hkEntity == null) || (hitEntity == null)) || ReferenceEquals(hitEntity, base.Character))
                        {
                            num1 = 0;
                        }
                        else
                        {
                            num1 = (int) !hkEntity.HasProperty(HkCharacterRigidBody.MANIPULATED_OBJECT);
                        }
                        flag2 = (bool) num1;
                        flag3 = (hitEntity != null) && (hitEntity.Physics != null);
                        if (ReferenceEquals(objA, null) & flag2)
                        {
                            objA = hitEntity;
                            base.ShapeKey = this.m_hits[num].HkHitInfo.GetShapeKey(0);
                        }
                        if (hitEntity is MyCubeGrid)
                        {
                            List<MyCube> list = (hitEntity as MyCubeGrid).RayCastBlocksAllOrdered(translation, to);
                            if ((list != null) && (list.Count > 0))
                            {
                                MySlimBlock cubeBlock = list[0].CubeBlock;
                                if (cubeBlock.FatBlock != null)
                                {
                                    flag3 = true;
                                    objA = cubeBlock.FatBlock;
                                    base.ShapeKey = 0;
                                }
                            }
                        }
                        if (!((base.HitMaterial.Equals(MyStringHash.NullOrEmpty) & flag2) & flag3))
                        {
                            if (hkEntity == null)
                            {
                                num++;
                                goto TR_0017;
                            }
                            else
                            {
                                zero = this.m_hits[num].GetFixedPosition();
                            }
                        }
                        else
                        {
                            base.HitBody = hkEntity;
                            base.HitNormal = this.m_hits[num].HkHitInfo.Normal;
                            base.HitPosition = this.m_hits[num].GetFixedPosition();
                            base.HitMaterial = hkEntity.GetBody().GetMaterialAt(base.HitPosition);
                            zero = (Vector3) base.HitPosition;
                        }
                        break;
                    }
                    goto TR_0017;
                }
            }
            finally
            {
                shape.RemoveReference();
            }
        TR_0012:
            flag = false;
            IMyUseObject interactive = objA as IMyUseObject;
            base.DetectedEntity = objA;
            if (objA != null)
            {
                MyUseObjectsComponentBase component = null;
                objA.Components.TryGet<MyUseObjectsComponentBase>(out component);
                if (component != null)
                {
                    interactive = component.GetInteractiveObject(base.ShapeKey);
                }
                if (doModelIntersection)
                {
                    LineD line = new LineD(translation, to);
                    MyCharacter character = objA as MyCharacter;
                    if (character == null)
                    {
                        MyIntersectionResultLineTriangleEx? nullable;
                        if (objA.GetIntersectionWithLine(ref line, out nullable, IntersectionFlags.ALL_TRIANGLES))
                        {
                            base.HitPosition = nullable.Value.IntersectionPointInWorldSpace;
                            base.HitNormal = nullable.Value.NormalInWorldSpace;
                        }
                    }
                    else if (character.GetIntersectionWithLine(ref line, ref base.CharHitInfo, IntersectionFlags.ALL_TRIANGLES))
                    {
                        base.HitPosition = base.CharHitInfo.Triangle.IntersectionPointInWorldSpace;
                        base.HitNormal = base.CharHitInfo.Triangle.NormalInWorldSpace;
                        base.HitTag = base.CharHitInfo;
                    }
                }
            }
            if (((interactive != null) && ((interactive.SupportedActions != UseActionEnum.None) && (Vector3D.Distance(translation, zero) < interactive.InteractiveDistance))) && ReferenceEquals(base.Character, MySession.Static.ControlledEntity))
            {
                HandleInteractiveObject(interactive);
                base.UseObject = interactive;
                flag = true;
            }
            if (!flag)
            {
                base.UseObject = null;
            }
            base.DisableDetectors();
        }

        public void DoDetectionModel()
        {
            this.DoDetection(!base.Character.TargetFromCamera, true);
        }

        public float ShapeRadius { get; set; }
    }
}

