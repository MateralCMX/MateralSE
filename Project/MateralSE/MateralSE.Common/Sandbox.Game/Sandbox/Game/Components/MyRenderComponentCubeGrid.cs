namespace Sandbox.Game.Components
{
    using Sandbox;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VRage.Game;
    using VRage.Plugins;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyRenderComponentCubeGrid : MyRenderComponent
    {
        private static readonly MyStringId ID_RED_DOT_IGNORE_DEPTH = MyStringId.GetOrCompute("RedDotIgnoreDepth");
        private static readonly MyStringId ID_WEAPON_LASER_IGNORE_DEPTH = MyStringId.GetOrCompute("WeaponLaserIgnoreDepth");
        private static readonly List<MyPhysics.HitInfo> m_tmpHitList = new List<MyPhysics.HitInfo>();
        private MyCubeGrid m_grid;
        private bool m_deferRenderRelease;
        private bool m_shouldReleaseRenderObjects;
        private MyCubeGridRenderData m_renderData;
        private MyParticleEffect m_atmosphericEffect;
        private const float m_atmosphericEffectMinSpeed = 75f;
        private const float m_atmosphericEffectMinFade = 0.85f;
        private const int m_atmosphericEffectVoxelContactDelay = 0x1388;
        private int m_lastVoxelContactTime;
        private float m_lastWorkingIntersectDistance;
        private static List<Vector3> m_tmpCornerList = new List<Vector3>();
        private List<IMyBlockAdditionalModelGenerator> m_additionalModelGenerators = new List<IMyBlockAdditionalModelGenerator>();

        public MyRenderComponentCubeGrid()
        {
            this.m_renderData = new MyCubeGridRenderData(this);
        }

        public override void AddRenderObjects()
        {
            MyCubeGrid entity = base.Container.Entity as MyCubeGrid;
            if ((base.m_renderObjectIDs[0] == uint.MaxValue) && entity.IsDirty())
            {
                entity.UpdateInstanceData();
            }
        }

        public void CloseModelGenerators()
        {
            using (List<IMyBlockAdditionalModelGenerator>.Enumerator enumerator = this.AdditionalModelGenerators.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
            this.AdditionalModelGenerators.Clear();
        }

        public void CreateAdditionalModelGenerators(MyCubeSize gridSizeEnum)
        {
            Assembly[] first = new Assembly[] { Assembly.GetExecutingAssembly(), MyPlugins.GameAssembly, MyPlugins.SandboxAssembly };
            if (MyPlugins.UserAssemblies != null)
            {
                first = first.Union<Assembly>(MyPlugins.UserAssemblies).ToArray<Assembly>();
            }
            foreach (Assembly assembly in first)
            {
                if (assembly != null)
                {
                    Type lookupType = typeof(IMyBlockAdditionalModelGenerator);
                    using (IEnumerator<Type> enumerator = (from t in assembly.GetTypes()
                        where lookupType.IsAssignableFrom(t) && (t.IsClass && !t.IsAbstract)
                        select t).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            IMyBlockAdditionalModelGenerator item = Activator.CreateInstance(enumerator.Current) as IMyBlockAdditionalModelGenerator;
                            if (item.Initialize(this.m_grid, gridSizeEnum))
                            {
                                this.AdditionalModelGenerators.Add(item);
                                continue;
                            }
                            item.Close();
                        }
                    }
                }
            }
        }

        public override unsafe void Draw()
        {
            List<MyPhysics.HitInfo>.Enumerator enumerator2;
            base.Draw();
            foreach (MyCubeBlock block in this.m_grid.BlocksForDraw)
            {
                if (MyRenderProxy.VisibleObjectsRead.Contains(block.Render.RenderObjectIDs[0]))
                {
                    block.Render.Draw();
                }
            }
            if ((MyCubeGrid.ShowCenterOfMass && (!this.IsStatic && (base.Container.Entity.Physics != null))) && base.Container.Entity.Physics.HasRigidBody)
            {
                MatrixD worldMatrix = base.Container.Entity.Physics.GetWorldMatrix();
                Vector3D centerOfMassWorld = base.Container.Entity.Physics.CenterOfMassWorld;
                Vector3D position = MySector.MainCamera.Position;
                float num = Vector3.Distance((Vector3) position, (Vector3) centerOfMassWorld);
                bool flag = false;
                if (num < 30f)
                {
                    flag = true;
                }
                else if (num < 200f)
                {
                    flag = true;
                    MyPhysics.CastRay(position, centerOfMassWorld, m_tmpHitList, 0x10);
                    using (enumerator2 = m_tmpHitList.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (!ReferenceEquals(enumerator2.Current.HkHitInfo.GetHitEntity(), this))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    m_tmpHitList.Clear();
                }
                if (flag)
                {
                    float num2 = MathHelper.Lerp((float) 1f, (float) 9f, (float) (num / 200f));
                    MyStringId id = ID_WEAPON_LASER_IGNORE_DEPTH;
                    Vector4 color = Color.Yellow.ToVector4();
                    float thickness = 0.02f * num2;
                    MySimpleObjectDraw.DrawLine(centerOfMassWorld - ((worldMatrix.Up * 0.5) * num2), centerOfMassWorld + ((worldMatrix.Up * 0.5) * num2), new MyStringId?(id), ref color, thickness, MyBillboard.BlendTypeEnum.AdditiveTop);
                    MySimpleObjectDraw.DrawLine(centerOfMassWorld - ((worldMatrix.Forward * 0.5) * num2), centerOfMassWorld + ((worldMatrix.Forward * 0.5) * num2), new MyStringId?(id), ref color, thickness, MyBillboard.BlendTypeEnum.AdditiveTop);
                    MySimpleObjectDraw.DrawLine(centerOfMassWorld - ((worldMatrix.Right * 0.5) * num2), centerOfMassWorld + ((worldMatrix.Right * 0.5) * num2), new MyStringId?(id), ref color, thickness, MyBillboard.BlendTypeEnum.AdditiveTop);
                    MyTransparentGeometry.AddBillboardOriented(ID_RED_DOT_IGNORE_DEPTH, Color.White.ToVector4(), centerOfMassWorld, MySector.MainCamera.LeftVector, MySector.MainCamera.UpVector, 0.1f * num2, MyBillboard.BlendTypeEnum.AdditiveTop, -1, 0f);
                }
            }
            if (MyCubeGrid.ShowGridPivot)
            {
                MatrixD worldMatrix = base.Container.Entity.WorldMatrix;
                Vector3D translation = worldMatrix.Translation;
                Vector3D position = MySector.MainCamera.Position;
                float num4 = Vector3.Distance((Vector3) position, (Vector3) translation);
                bool flag2 = false;
                if (num4 < 30f)
                {
                    flag2 = true;
                }
                else if (num4 < 200f)
                {
                    flag2 = true;
                    MyPhysics.CastRay(position, translation, m_tmpHitList, 0x10);
                    using (enumerator2 = m_tmpHitList.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (!ReferenceEquals(enumerator2.Current.HkHitInfo.GetHitEntity(), this))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    m_tmpHitList.Clear();
                }
                if (flag2)
                {
                    float num5 = MathHelper.Lerp((float) 1f, (float) 9f, (float) (num4 / 200f));
                    MyStringId id2 = ID_WEAPON_LASER_IGNORE_DEPTH;
                    float thickness = 0.02f * num5;
                    Vector4 color = Color.Green.ToVector4();
                    MySimpleObjectDraw.DrawLine(translation, translation + ((worldMatrix.Up * 0.5) * num5), new MyStringId?(id2), ref color, thickness, MyBillboard.BlendTypeEnum.Standard);
                    color = Color.Blue.ToVector4();
                    MySimpleObjectDraw.DrawLine(translation, translation + ((worldMatrix.Forward * 0.5) * num5), new MyStringId?(id2), ref color, thickness, MyBillboard.BlendTypeEnum.Standard);
                    color = Color.Red.ToVector4();
                    MySimpleObjectDraw.DrawLine(translation, translation + ((worldMatrix.Right * 0.5) * num5), new MyStringId?(id2), ref color, thickness, MyBillboard.BlendTypeEnum.Standard);
                    MyTransparentGeometry.AddBillboardOriented(ID_RED_DOT_IGNORE_DEPTH, Color.White.ToVector4(), translation, MySector.MainCamera.LeftVector, MySector.MainCamera.UpVector, 0.1f * num5, MyBillboard.BlendTypeEnum.Standard, -1, 0f);
                    MyRenderProxy.DebugDrawAxis(worldMatrix, 0.5f, false, false, false);
                }
            }
            if (!MyCubeGrid.ShowStructuralIntegrity)
            {
                if ((this.m_grid.StructuralIntegrity != null) && this.m_grid.StructuralIntegrity.EnabledOnlyForDraw)
                {
                    this.m_grid.CloseStructuralIntegrity();
                }
            }
            else if (this.m_grid.StructuralIntegrity != null)
            {
                this.m_grid.StructuralIntegrity.Draw();
            }
            else if (MyFakes.ENABLE_STRUCTURAL_INTEGRITY)
            {
                this.m_grid.CreateStructuralIntegrity();
                if (this.m_grid.StructuralIntegrity != null)
                {
                    this.m_grid.StructuralIntegrity.EnabledOnlyForDraw = true;
                }
            }
            if (MyFakes.ENABLE_ATMOSPHERIC_ENTRYEFFECT)
            {
                this.DrawAtmosphericEntryEffect();
            }
            if (this.m_grid.MarkedAsTrash)
            {
                BoundingBoxD localAABB = this.m_grid.PositionComp.LocalAABB;
                Vector3D* vectordPtr1 = (Vector3D*) ref localAABB.Max;
                vectordPtr1[0] += 0.2f;
                Vector3D* vectordPtr2 = (Vector3D*) ref localAABB.Min;
                vectordPtr2[0] -= 0.20000000298023224;
                MatrixD worldMatrix = this.m_grid.PositionComp.WorldMatrix;
                Color red = Color.Red;
                red.A = (byte) (((100.0 * (Math.Sin((double) (((float) this.m_grid.TrashHighlightCounter) / 10f)) + 1.0)) / 2.0) + 100.0);
                red.R = (byte) (((200.0 * (Math.Sin((double) (((float) this.m_grid.TrashHighlightCounter) / 10f)) + 1.0)) / 2.0) + 50.0);
                Color* colorPtr1 = (Color*) ref red;
                MyStringId? faceMaterial = null;
                faceMaterial = null;
                MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localAABB, ref (Color) ref colorPtr1, ref red, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.008f, faceMaterial, faceMaterial, false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
            }
        }

        private unsafe void DrawAtmosphericEntryEffect()
        {
            int num1;
            bool flag = (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastVoxelContactTime) < 0x1388;
            bool flag2 = !(MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.m_grid.PositionComp.GetPosition()).Length() != 0f);
            bool flag1 = ReferenceEquals(this.m_grid.Physics, null);
            if (!flag1)
            {
                num1 = (int) (this.m_grid.Physics.LinearVelocity.Length() < 75f);
            }
            else
            {
                num1 = 0;
            }
            bool flag3 = (bool) num1;
            if (((flag1 | flag) | flag2) | flag3)
            {
                if (this.m_atmosphericEffect != null)
                {
                    MyParticlesManager.RemoveParticleEffect(this.m_atmosphericEffect, false);
                    this.m_atmosphericEffect = null;
                }
            }
            else
            {
                Vector3 linearVelocity = this.m_grid.Physics.LinearVelocity;
                Vector3 v = Vector3.Normalize(linearVelocity);
                float num2 = linearVelocity.Length();
                BoundingBox worldAABB = (BoundingBox) this.m_grid.PositionComp.WorldAABB;
                Vector3 center = worldAABB.Center;
                Vector3 position = new Vector3();
                foreach (Vector3 vector8 in worldAABB.GetCorners())
                {
                    Vector3 vector9 = vector8 - center;
                    if (vector9.Dot(v) > 0.01f)
                    {
                        m_tmpCornerList.Add(vector8);
                        position += vector8;
                        if (m_tmpCornerList.Count == 4)
                        {
                            break;
                        }
                    }
                }
                if (m_tmpCornerList.Count > 0)
                {
                    position /= (float) m_tmpCornerList.Count;
                }
                Plane plane = new Plane(position, -v);
                m_tmpCornerList.Clear();
                Vector3D centerOfMassWorld = this.m_grid.Physics.CenterOfMassWorld;
                float? nullable2 = new Ray((Vector3) centerOfMassWorld, v).Intersects(plane);
                this.m_lastWorkingIntersectDistance = (nullable2 != null) ? nullable2.GetValueOrDefault() : this.m_lastWorkingIntersectDistance;
                Matrix identity = Matrix.Identity;
                identity.Translation = (Vector3) (centerOfMassWorld + ((0.875f * v) * this.m_lastWorkingIntersectDistance));
                identity.Forward = v;
                Vector3 direction = Vector3.Transform(v, Quaternion.CreateFromAxisAngle((Vector3) this.m_grid.PositionComp.WorldMatrix.Left, 1.570796f));
                identity.Up = Vector3.Normalize(Vector3.Reject((Vector3) this.m_grid.PositionComp.WorldMatrix.Left, direction));
                Matrix* matrixPtr1 = (Matrix*) ref identity;
                matrixPtr1.Left = identity.Up.Cross(identity.Forward);
                float num3 = MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(this.m_grid.PositionComp.GetPosition());
                if ((this.m_atmosphericEffect != null) || MyParticlesManager.TryCreateParticleEffect("Dummy", identity, out this.m_atmosphericEffect))
                {
                    this.m_atmosphericEffect.UserScale = worldAABB.ProjectedArea(v) / ((float) Math.Pow(38.0 * this.m_grid.GridSize, 2.0));
                    this.m_atmosphericEffect.UserAxisScale = Vector3.Normalize(new Vector3(1f, 1f, 1f + ((1.5f * (this.m_grid.Physics.LinearVelocity.Length() - 75f)) / (MyGridPhysics.ShipMaxLinearVelocity() - 75f))));
                    this.m_atmosphericEffect.UserColorMultiplier = new Vector4(MathHelper.Clamp((float) (((num2 - 75f) / 37.5f) * ((float) Math.Pow((double) num3, 1.5))), (float) 0f, (float) 0.85f));
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_grid = base.Container.Entity as MyCubeGrid;
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (this.m_atmosphericEffect != null)
            {
                MyParticlesManager.RemoveParticleEffect(this.m_atmosphericEffect, false);
                this.m_atmosphericEffect = null;
            }
        }

        public void RebuildDirtyCells()
        {
            this.m_renderData.RebuildDirtyCells(this.GetRenderFlags());
        }

        public override void RemoveRenderObjects()
        {
            if (this.m_deferRenderRelease)
            {
                this.m_shouldReleaseRenderObjects = true;
            }
            else
            {
                this.m_shouldReleaseRenderObjects = false;
                this.m_renderData.OnRemovedFromRender();
                for (int i = 0; i < base.m_renderObjectIDs.Length; i++)
                {
                    if (base.m_renderObjectIDs[i] != uint.MaxValue)
                    {
                        this.ReleaseRenderObjectID(i);
                    }
                }
            }
        }

        public void ResetLastVoxelContactTimer()
        {
            this.m_lastVoxelContactTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        }

        public void UpdateRenderObjectMatrices(Matrix matrix)
        {
            for (int i = 0; i < base.m_renderObjectIDs.Length; i++)
            {
                if (base.m_renderObjectIDs[i] != uint.MaxValue)
                {
                    BoundingBox? aabb = null;
                    Matrix? localMatrix = null;
                    MyRenderProxy.UpdateRenderObject(base.RenderObjectIDs[i], new MatrixD?(matrix), aabb, base.LastMomentUpdateIndex, localMatrix);
                }
            }
        }

        protected override void UpdateRenderObjectVisibility(bool visible)
        {
            base.UpdateRenderObjectVisibility(visible);
        }

        public MyCubeGrid CubeGrid =>
            this.m_grid;

        public bool DeferRenderRelease
        {
            get => 
                this.m_deferRenderRelease;
            set
            {
                this.m_deferRenderRelease = value;
                if (!value && this.m_shouldReleaseRenderObjects)
                {
                    this.RemoveRenderObjects();
                }
            }
        }

        public MyCubeGridRenderData RenderData =>
            this.m_renderData;

        public List<IMyBlockAdditionalModelGenerator> AdditionalModelGenerators =>
            this.m_additionalModelGenerators;

        public MyCubeSize GridSizeEnum =>
            this.m_grid.GridSizeEnum;

        public float GridSize =>
            this.m_grid.GridSize;

        public bool IsStatic =>
            this.m_grid.IsStatic;
    }
}

