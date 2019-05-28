namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyFloatingObjectClipboard
    {
        private List<MyObjectBuilder_FloatingObject> m_copiedFloatingObjects = new List<MyObjectBuilder_FloatingObject>();
        private List<Vector3D> m_copiedFloatingObjectOffsets = new List<Vector3D>();
        private List<MyFloatingObject> m_previewFloatingObjects = new List<MyFloatingObject>();
        private Vector3D m_pastePosition;
        private Vector3D m_pastePositionPrevious;
        private bool m_calculateVelocity = true;
        private Vector3 m_objectVelocity = Vector3.Zero;
        private float m_pasteOrientationAngle;
        private Vector3 m_pasteDirUp = new Vector3(1f, 0f, 0f);
        private Vector3 m_pasteDirForward = new Vector3(0f, 1f, 0f);
        private float m_dragDistance;
        private Vector3D m_dragPointToPositionLocal;
        private bool m_canBePlaced;
        private List<MyPhysics.HitInfo> m_raycastCollisionResults = new List<MyPhysics.HitInfo>();
        private float m_closestHitDistSq = float.MaxValue;
        private Vector3D m_hitPos = new Vector3(0f, 0f, 0f);
        private Vector3 m_hitNormal = new Vector3(1f, 0f, 0f);
        private bool m_visible = true;
        private bool m_enableStationRotation;

        public MyFloatingObjectClipboard(bool calculateVelocity = true)
        {
            this.m_calculateVelocity = calculateVelocity;
        }

        private void Activate()
        {
            this.ChangeClipboardPreview(true);
            this.IsActive = true;
        }

        private void AngleMinus(float angle)
        {
            this.m_pasteOrientationAngle -= angle;
            if (this.m_pasteOrientationAngle < 0f)
            {
                this.m_pasteOrientationAngle += 6.283185f;
            }
        }

        private void AnglePlus(float angle)
        {
            this.m_pasteOrientationAngle += angle;
            if (this.m_pasteOrientationAngle >= 6.283185f)
            {
                this.m_pasteOrientationAngle -= 6.283185f;
            }
        }

        private void ApplyOrientationAngle()
        {
            this.m_pasteDirForward = Vector3.Normalize(this.m_pasteDirForward);
            this.m_pasteDirUp = Vector3.Normalize(this.m_pasteDirUp);
            Vector3 vector = Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
            float num = (float) Math.Cos((double) this.m_pasteOrientationAngle);
            this.m_pasteDirForward = (this.m_pasteDirForward * num) - (vector * ((float) Math.Sin((double) this.m_pasteOrientationAngle)));
            this.m_pasteOrientationAngle = 0f;
        }

        public void CalculateRotationHints(MyBlockBuilderRotationHints hints, bool isRotating)
        {
        }

        private void ChangeClipboardPreview(bool visible)
        {
            if ((this.m_copiedFloatingObjects.Count == 0) || !visible)
            {
                foreach (MyFloatingObject local1 in this.m_previewFloatingObjects)
                {
                    Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(local1, false, color, 0.01f, inflateAmount, lineMaterial);
                    local1.Close();
                }
                this.m_previewFloatingObjects.Clear();
                this.m_visible = false;
            }
            else
            {
                Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection((IEnumerable<MyObjectBuilder_EntityBase>) this.m_copiedFloatingObjects);
                using (List<MyObjectBuilder_FloatingObject>.Enumerator enumerator2 = this.m_copiedFloatingObjects.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        MyFloatingObject floatingObject = Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilder(enumerator2.Current, false) as MyFloatingObject;
                        if (floatingObject == null)
                        {
                            this.ChangeClipboardPreview(false);
                            break;
                        }
                        this.MakeTransparent(floatingObject);
                        this.IsActive = visible;
                        this.m_visible = visible;
                        Sandbox.Game.Entities.MyEntities.Add(floatingObject, true);
                        MyFloatingObjects.UnregisterFloatingObject(floatingObject);
                        floatingObject.Save = false;
                        this.DisablePhysicsRecursively(floatingObject);
                        this.m_previewFloatingObjects.Add(floatingObject);
                    }
                }
            }
        }

        private bool CheckPastedFloatingObjects()
        {
            using (List<MyObjectBuilder_FloatingObject>.Enumerator enumerator = this.m_copiedFloatingObjects.GetEnumerator())
            {
                while (true)
                {
                    MyPhysicalItemDefinition definition;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyDefinitionId id = enumerator.Current.Item.PhysicalContent.GetId();
                    if (!MyDefinitionManager.Static.TryGetPhysicalItemDefinition(id, out definition))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void ClearClipboard()
        {
            if (this.IsActive)
            {
                this.Deactivate();
            }
            this.m_copiedFloatingObjects.Clear();
            this.m_copiedFloatingObjectOffsets.Clear();
        }

        public void CopyfloatingObject(MyFloatingObject floatingObject)
        {
            if (floatingObject != null)
            {
                this.m_copiedFloatingObjects.Clear();
                this.m_copiedFloatingObjectOffsets.Clear();
                this.CopyFloatingObjectInternal(floatingObject);
                this.Activate();
            }
        }

        private void CopyFloatingObjectInternal(MyFloatingObject toCopy)
        {
            this.m_copiedFloatingObjects.Add((MyObjectBuilder_FloatingObject) toCopy.GetObjectBuilder(true));
            if (this.m_copiedFloatingObjects.Count == 1)
            {
                MatrixD pasteMatrix = GetPasteMatrix();
                Vector3D translation = toCopy.WorldMatrix.Translation;
                this.m_dragPointToPositionLocal = Vector3D.TransformNormal(toCopy.PositionComp.GetPosition() - translation, toCopy.PositionComp.WorldMatrixNormalizedInv);
                this.m_dragDistance = (float) (translation - pasteMatrix.Translation).Length();
                this.m_pasteDirUp = (Vector3) toCopy.WorldMatrix.Up;
                this.m_pasteDirForward = (Vector3) toCopy.WorldMatrix.Forward;
                this.m_pasteOrientationAngle = 0f;
            }
            this.m_copiedFloatingObjectOffsets.Add(toCopy.WorldMatrix.Translation - this.m_copiedFloatingObjects[0].PositionAndOrientation.Value.Position);
        }

        public void CutFloatingObject(MyFloatingObject floatingObject)
        {
            if (floatingObject != null)
            {
                this.CopyfloatingObject(floatingObject);
                this.DeleteFloatingObject(floatingObject);
            }
        }

        public void Deactivate()
        {
            this.ChangeClipboardPreview(false);
            this.IsActive = false;
        }

        public void DeleteFloatingObject(MyFloatingObject floatingObject)
        {
            if (floatingObject != null)
            {
                MyFloatingObjects.RemoveFloatingObject(floatingObject, true);
                this.Deactivate();
            }
        }

        private void DisablePhysicsRecursively(VRage.Game.Entity.MyEntity entity)
        {
            if ((entity.Physics != null) && entity.Physics.Enabled)
            {
                entity.Physics.Enabled = false;
            }
            MyFloatingObject obj2 = entity as MyFloatingObject;
            if (obj2 != null)
            {
                obj2.NeedsUpdate = MyEntityUpdateEnum.NONE;
            }
            foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
            {
                this.DisablePhysicsRecursively(base2.Container.Entity as VRage.Game.Entity.MyEntity);
            }
        }

        private Matrix GetFirstGridOrientationMatrix() => 
            (Matrix.CreateWorld(Vector3.Zero, this.m_pasteDirForward, this.m_pasteDirUp) * Matrix.CreateFromAxisAngle(this.m_pasteDirUp, this.m_pasteOrientationAngle));

        private static MatrixD GetPasteMatrix()
        {
            if ((MySession.Static.ControlledEntity == null) || ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return MySector.MainCamera.WorldMatrix;
            }
            return MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
        }

        public bool HasCopiedFloatingObjects() => 
            (this.m_copiedFloatingObjects.Count > 0);

        public void Hide()
        {
            this.ChangeClipboardPreview(false);
        }

        public void HideWhenColliding(List<Vector3D> collisionTestPoints)
        {
            if (this.m_previewFloatingObjects.Count != 0)
            {
                bool flag = true;
                foreach (Vector3D vectord in collisionTestPoints)
                {
                    foreach (MyFloatingObject obj2 in this.m_previewFloatingObjects)
                    {
                        Vector3D point = Vector3.Transform((Vector3) vectord, obj2.PositionComp.WorldMatrixNormalizedInv);
                        BoundingBox localAABB = obj2.PositionComp.LocalAABB;
                        if (localAABB.Contains(point) == ContainmentType.Contains)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
                using (List<MyFloatingObject>.Enumerator enumerator2 = this.m_previewFloatingObjects.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        enumerator2.Current.Render.Visible = flag;
                    }
                }
            }
        }

        private void MakeTransparent(MyFloatingObject floatingObject)
        {
            floatingObject.Render.Transparency = 0.25f;
        }

        public void MoveEntityCloser()
        {
            this.m_dragDistance /= 1.1f;
        }

        public void MoveEntityFurther()
        {
            this.m_dragDistance *= 1.1f;
        }

        public bool PasteFloatingObject(MyInventory buildInventory = null)
        {
            if (this.m_copiedFloatingObjects.Count == 0)
            {
                return false;
            }
            if ((this.m_copiedFloatingObjects.Count > 0) && !this.IsActive)
            {
                if (this.CheckPastedFloatingObjects())
                {
                    this.Activate();
                }
                else
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.CopyPasteBlockNotAvailable);
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                }
                return true;
            }
            if (!this.m_canBePlaced)
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                return false;
            }
            MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceItem);
            Sandbox.Game.Entities.MyEntities.RemapObjectBuilderCollection((IEnumerable<MyObjectBuilder_EntityBase>) this.m_copiedFloatingObjects);
            bool flag = false;
            int num = 0;
            foreach (MyObjectBuilder_FloatingObject local1 in this.m_copiedFloatingObjects)
            {
                local1.PersistentFlags = MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
                local1.PositionAndOrientation = new MyPositionAndOrientation(this.m_previewFloatingObjects[num].WorldMatrix);
                num++;
                MyFloatingObjects.RequestSpawnCreative(local1);
                flag = true;
            }
            this.Deactivate();
            return flag;
        }

        private void RightMinus(float angle)
        {
            this.RightPlus(-angle);
        }

        private void RightPlus(float angle)
        {
            this.ApplyOrientationAngle();
            Vector3 vector = Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
            float num = (float) Math.Cos((double) angle);
            this.m_pasteDirUp = (this.m_pasteDirUp * num) + (vector * ((float) Math.Sin((double) angle)));
        }

        public void RotateAroundAxis(int axisIndex, int sign, bool newlyPressed, float angleDelta)
        {
            switch (axisIndex)
            {
                case 0:
                    if (sign < 0)
                    {
                        this.UpMinus(angleDelta);
                        return;
                    }
                    this.UpPlus(angleDelta);
                    return;

                case 1:
                    if (sign < 0)
                    {
                        this.AngleMinus(angleDelta);
                        return;
                    }
                    this.AnglePlus(angleDelta);
                    return;

                case 2:
                    if (sign < 0)
                    {
                        this.RightPlus(angleDelta);
                        return;
                    }
                    this.RightMinus(angleDelta);
                    return;
            }
        }

        public void SetFloatingObjectFromBuilder(MyObjectBuilder_FloatingObject floatingObject, Vector3D dragPointDelta, float dragVectorLength)
        {
            if (this.IsActive)
            {
                this.Deactivate();
            }
            this.m_copiedFloatingObjects.Clear();
            this.m_copiedFloatingObjectOffsets.Clear();
            GetPasteMatrix();
            this.m_dragPointToPositionLocal = dragPointDelta;
            this.m_dragDistance = dragVectorLength;
            MyPositionAndOrientation? positionAndOrientation = floatingObject.PositionAndOrientation;
            MyPositionAndOrientation orientation = (positionAndOrientation != null) ? positionAndOrientation.GetValueOrDefault() : MyPositionAndOrientation.Default;
            this.m_pasteDirUp = (Vector3) orientation.Up;
            this.m_pasteDirForward = (Vector3) orientation.Forward;
            this.SetFloatingObjectFromBuilderInternal(floatingObject, Vector3D.Zero);
            this.Activate();
        }

        private void SetFloatingObjectFromBuilderInternal(MyObjectBuilder_FloatingObject floatingObject, Vector3D offset)
        {
            this.m_copiedFloatingObjects.Add(floatingObject);
            this.m_copiedFloatingObjectOffsets.Add(offset);
        }

        public void Show()
        {
            if (this.IsActive && (this.m_previewFloatingObjects.Count == 0))
            {
                this.ChangeClipboardPreview(true);
            }
        }

        private bool TestPlacement()
        {
            for (int i = 0; i < this.m_previewFloatingObjects.Count; i++)
            {
                if (!Sandbox.Game.Entities.MyEntities.IsInsideWorld(this.m_previewFloatingObjects[i].PositionComp.GetPosition()))
                {
                    return false;
                }
            }
            return true;
        }

        public void Update()
        {
            if (this.IsActive && this.m_visible)
            {
                this.UpdateHitEntity();
                this.UpdatePastePosition();
                this.UpdateFloatingObjectTransformations();
                if (this.m_calculateVelocity)
                {
                    this.m_objectVelocity = (Vector3) ((this.m_pastePosition - this.m_pastePositionPrevious) / 0.01666666753590107);
                }
                this.m_canBePlaced = this.TestPlacement();
                this.UpdatePreviewBBox();
                if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "FW: " + this.m_pasteDirForward.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 20f), "UP: " + this.m_pasteDirUp.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 40f), "AN: " + this.m_pasteOrientationAngle.ToString(), Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
            }
        }

        private void UpdateFloatingObjectTransformations()
        {
            MatrixD firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
            MatrixD matrix = MatrixD.Invert(this.m_copiedFloatingObjects[0].PositionAndOrientation.Value.GetMatrix()).GetOrientation() * firstGridOrientationMatrix;
            for (int i = 0; i < this.m_previewFloatingObjects.Count; i++)
            {
                MatrixD rotationMatrix = this.m_copiedFloatingObjects[i].PositionAndOrientation.Value.GetMatrix();
                Vector3D normal = rotationMatrix.Translation - this.m_copiedFloatingObjects[0].PositionAndOrientation.Value.Position;
                this.m_copiedFloatingObjectOffsets[i] = Vector3D.TransformNormal(normal, matrix);
                Vector3D vectord2 = this.m_pastePosition + this.m_copiedFloatingObjectOffsets[i];
                rotationMatrix *= matrix;
                rotationMatrix.Translation = Vector3D.Zero;
                rotationMatrix = MatrixD.Orthogonalize(rotationMatrix);
                rotationMatrix.Translation = vectord2;
                this.m_previewFloatingObjects[i].PositionComp.SetWorldMatrix(rotationMatrix, null, false, true, true, false, false, false);
            }
        }

        private void UpdateHitEntity()
        {
            MatrixD pasteMatrix = GetPasteMatrix();
            MyPhysics.CastRay(pasteMatrix.Translation, pasteMatrix.Translation + (pasteMatrix.Forward * this.m_dragDistance), this.m_raycastCollisionResults, 0);
            this.m_closestHitDistSq = float.MaxValue;
            this.m_hitPos = new Vector3D(0.0, 0.0, 0.0);
            this.m_hitNormal = new Vector3(1f, 0f, 0f);
            foreach (MyPhysics.HitInfo info in this.m_raycastCollisionResults)
            {
                MyPhysicsBody userObject = (MyPhysicsBody) info.HkHitInfo.Body.UserObject;
                if (userObject != null)
                {
                    IMyEntity entity = userObject.Entity;
                    if (!(entity is MyVoxelMap))
                    {
                        if (!(entity is MyCubeGrid))
                        {
                            continue;
                        }
                        if (entity.EntityId == this.m_previewFloatingObjects[0].EntityId)
                        {
                            continue;
                        }
                    }
                    float num = (float) (info.Position - pasteMatrix.Translation).LengthSquared();
                    if (num < this.m_closestHitDistSq)
                    {
                        this.m_closestHitDistSq = num;
                        this.m_hitPos = info.Position;
                        this.m_hitNormal = info.HkHitInfo.Normal;
                    }
                }
            }
            this.m_raycastCollisionResults.Clear();
        }

        private void UpdatePastePosition()
        {
            this.m_pastePositionPrevious = this.m_pastePosition;
            MatrixD pasteMatrix = GetPasteMatrix();
            Vector3D vectord = pasteMatrix.Forward * this.m_dragDistance;
            this.m_pastePosition = pasteMatrix.Translation + vectord;
            MatrixD firstGridOrientationMatrix = this.GetFirstGridOrientationMatrix();
            this.m_pastePosition += Vector3D.TransformNormal(this.m_dragPointToPositionLocal, firstGridOrientationMatrix);
            if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
            {
                MyRenderProxy.DebugDrawSphere(pasteMatrix.Translation + vectord, 0.15f, Color.Pink, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawSphere(this.m_pastePosition, 0.15f, Color.Pink.ToVector3(), 1f, false, false, true, false);
            }
        }

        private void UpdatePreviewBBox()
        {
            if (this.m_previewFloatingObjects != null)
            {
                List<MyFloatingObject>.Enumerator enumerator;
                MyStringId? nullable3;
                if (!this.m_visible)
                {
                    using (enumerator = this.m_previewFloatingObjects.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Vector4? color = null;
                            Vector3? inflateAmount = null;
                            nullable3 = null;
                            Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(enumerator.Current, false, color, 0.01f, inflateAmount, nullable3);
                        }
                    }
                }
                else
                {
                    Vector4 vector = new Vector4(Color.Red.ToVector3() * 0.8f, 1f);
                    if (this.m_canBePlaced)
                    {
                        vector = Color.Gray.ToVector4();
                    }
                    Vector3 vector2 = new Vector3(0.1f);
                    using (enumerator = this.m_previewFloatingObjects.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            nullable3 = null;
                            Sandbox.Game.Entities.MyEntities.EnableEntityBoundingBoxDraw(enumerator.Current, true, new Vector4?(vector), 0.01f, new Vector3?(vector2), nullable3);
                        }
                    }
                }
            }
        }

        private void UpMinus(float angle)
        {
            this.UpPlus(-angle);
        }

        private void UpPlus(float angle)
        {
            this.ApplyOrientationAngle();
            Vector3.Cross(this.m_pasteDirForward, this.m_pasteDirUp);
            float num = (float) Math.Cos((double) angle);
            float num2 = (float) Math.Sin((double) angle);
            Vector3 vector = (this.m_pasteDirUp * num) - (this.m_pasteDirForward * num2);
            this.m_pasteDirForward = (this.m_pasteDirUp * num2) + (this.m_pasteDirForward * num);
            this.m_pasteDirUp = vector;
        }

        public bool IsActive { get; private set; }

        public List<MyFloatingObject> PreviewFloatingObjects =>
            this.m_previewFloatingObjects;

        public bool EnableStationRotation
        {
            get => 
                (this.m_enableStationRotation && MyFakes.ENABLE_STATION_ROTATION);
            set => 
                (this.m_enableStationRotation = value);
        }

        public string CopiedGridsName =>
            (!this.HasCopiedFloatingObjects() ? null : this.m_copiedFloatingObjects[0].Name);
    }
}

