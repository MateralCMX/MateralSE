namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Voxels;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyVoxelClipboard
    {
        private List<MyObjectBuilder_EntityBase> m_copiedVoxelMaps = new List<MyObjectBuilder_EntityBase>();
        private List<IMyStorage> m_copiedStorages = new List<IMyStorage>();
        private List<Vector3> m_copiedVoxelMapOffsets = new List<Vector3>();
        private List<MyVoxelBase> m_previewVoxelMaps = new List<MyVoxelBase>();
        private Vector3D m_pastePosition;
        private float m_dragDistance;
        private Vector3 m_dragPointToPositionLocal;
        private bool m_canBePlaced;
        private MyEntity m_blockingEntity;
        private bool m_visible = true;
        private bool m_shouldMarkForClose = true;
        private bool m_planetMode;
        private static readonly MyStringId ID_GIZMO_DRAW_LINE = MyStringId.GetOrCompute("GizmoDrawLine");
        private List<MyEntity> m_tmpResultList = new List<MyEntity>();

        private void Activate()
        {
            this.ChangeClipboardPreview(true);
            this.IsActive = true;
        }

        private void ChangeClipboardPreview(bool visible)
        {
            if ((this.m_copiedVoxelMaps.Count == 0) || !visible)
            {
                foreach (MyVoxelBase base2 in this.m_previewVoxelMaps)
                {
                    Vector4? color = null;
                    Vector3? inflateAmount = null;
                    MyStringId? lineMaterial = null;
                    MyEntities.EnableEntityBoundingBoxDraw(base2, false, color, 0.01f, inflateAmount, lineMaterial);
                    if (this.m_shouldMarkForClose)
                    {
                        base2.Close();
                    }
                }
                this.m_previewVoxelMaps.Clear();
                this.m_visible = false;
            }
            else
            {
                MyEntities.RemapObjectBuilderCollection(this.m_copiedVoxelMaps);
                for (int i = 0; i < this.m_copiedVoxelMaps.Count; i++)
                {
                    MyObjectBuilder_EntityBase builder = this.m_copiedVoxelMaps[i];
                    IMyStorage storage = this.m_copiedStorages[i];
                    MyVoxelBase entity = null;
                    if (builder is MyObjectBuilder_VoxelMap)
                    {
                        entity = new MyVoxelMap();
                    }
                    if (builder is MyObjectBuilder_Planet)
                    {
                        this.m_planetMode = true;
                        this.IsActive = visible;
                        this.m_visible = visible;
                    }
                    else
                    {
                        MyPositionAndOrientation local1 = builder.PositionAndOrientation.Value;
                        entity.Init(builder, storage);
                        entity.BeforePaste();
                        this.DisablePhysicsRecursively(entity);
                        this.MakeTransparent(entity);
                        MyEntities.Add(entity, true);
                        entity.PositionLeftBottomCorner = this.m_pastePosition - (entity.Storage.Size * 0.5f);
                        entity.PositionComp.SetPosition(this.m_pastePosition, null, false, true);
                        entity.Save = false;
                        this.m_previewVoxelMaps.Add(entity);
                        this.IsActive = visible;
                        this.m_visible = visible;
                        this.m_shouldMarkForClose = true;
                    }
                }
            }
        }

        public void ClearClipboard()
        {
            if (this.IsActive)
            {
                this.Deactivate();
            }
            this.m_copiedVoxelMapOffsets.Clear();
            this.m_copiedVoxelMaps.Clear();
        }

        public void CopyVoxelMap(MyVoxelBase voxelMap)
        {
            if (voxelMap != null)
            {
                this.m_copiedVoxelMaps.Clear();
                this.m_copiedVoxelMapOffsets.Clear();
                this.CopyVoxelMapInternal(voxelMap);
                this.Activate();
            }
        }

        private void CopyVoxelMapInternal(MyVoxelBase toCopy)
        {
            this.m_copiedVoxelMaps.Add(toCopy.GetObjectBuilder(true));
            if (this.m_copiedVoxelMaps.Count == 1)
            {
                MatrixD pasteMatrix = GetPasteMatrix();
                Vector3D translation = toCopy.WorldMatrix.Translation;
                this.m_dragPointToPositionLocal = (Vector3) Vector3D.TransformNormal(toCopy.PositionComp.GetPosition() - translation, toCopy.PositionComp.WorldMatrixNormalizedInv);
                this.m_dragDistance = (float) (translation - pasteMatrix.Translation).Length();
            }
            this.m_copiedVoxelMapOffsets.Add((Vector3) (toCopy.WorldMatrix.Translation - this.m_copiedVoxelMaps[0].PositionAndOrientation.Value.Position));
        }

        public void CutVoxelMap(MyVoxelBase voxelMap)
        {
            if (voxelMap != null)
            {
                this.CopyVoxelMap(voxelMap);
                MyEntities.SendCloseRequest(voxelMap);
                this.Deactivate();
            }
        }

        public void Deactivate()
        {
            this.ChangeClipboardPreview(false);
            this.IsActive = false;
            this.m_planetMode = false;
        }

        private void DisablePhysicsRecursively(MyEntity entity)
        {
            if ((entity.Physics != null) && entity.Physics.Enabled)
            {
                entity.Physics.Enabled = false;
            }
            foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
            {
                this.DisablePhysicsRecursively(base2.Container.Entity as MyEntity);
            }
        }

        private void EnablePhysicsRecursively(MyEntity entity)
        {
            if ((entity.Physics != null) && !entity.Physics.Enabled)
            {
                entity.Physics.Enabled = true;
            }
            foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
            {
                this.EnablePhysicsRecursively(base2.Container.Entity as MyEntity);
            }
        }

        private static MatrixD GetPasteMatrix()
        {
            if ((MySession.Static.ControlledEntity == null) || ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return MySector.MainCamera.WorldMatrix;
            }
            return MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
        }

        public void Hide()
        {
            this.ChangeClipboardPreview(false);
            this.m_planetMode = false;
        }

        private void MakeTransparent(MyVoxelBase voxelMap)
        {
            voxelMap.Render.Transparency = 0.25f;
        }

        private void MakeVisible(MyVoxelBase voxelMap)
        {
            voxelMap.Render.Transparency = 0f;
            voxelMap.Render.UpdateTransparency();
        }

        public void MoveEntityCloser()
        {
            this.m_dragDistance /= 1.1f;
        }

        public void MoveEntityFurther()
        {
            this.m_dragDistance *= 1.1f;
        }

        public bool PasteVoxelMap(MyInventory buildInventory = null)
        {
            if (this.m_planetMode)
            {
                if (!this.m_canBePlaced)
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.CopyPasteAsteoridObstructed);
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                    return false;
                }
                MyEntities.RemapObjectBuilderCollection(this.m_copiedVoxelMaps);
                for (int i = 0; i < this.m_copiedVoxelMaps.Count; i++)
                {
                    MyGuiScreenDebugSpawnMenu.SpawnPlanet(this.m_pastePosition - this.m_copiedVoxelMapOffsets[i]);
                }
                this.Deactivate();
                return true;
            }
            if (this.m_copiedVoxelMaps.Count == 0)
            {
                return false;
            }
            if ((this.m_copiedVoxelMaps.Count > 0) && !this.IsActive)
            {
                this.Activate();
                return true;
            }
            if (!this.m_canBePlaced)
            {
                MyHud.Notifications.Add(MyNotificationSingletons.CopyPasteAsteoridObstructed);
                MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                return false;
            }
            MyGuiAudio.PlaySound(MyGuiSounds.HudPlaceBlock);
            MyGuiScreenDebugSpawnMenu.RecreateAsteroidBeforePaste((float) this.m_previewVoxelMaps[0].PositionComp.GetPosition().Length());
            MyEntities.RemapObjectBuilderCollection(this.m_copiedVoxelMaps);
            foreach (MyVoxelBase base2 in this.m_previewVoxelMaps)
            {
                if (!Sync.IsServer)
                {
                    this.m_shouldMarkForClose = true;
                    MyGuiScreenDebugSpawnMenu.SpawnAsteroid(base2.PositionComp.GetPosition());
                }
                else
                {
                    base2.CreatedByUser = true;
                    base2.AsteroidName = MyGuiScreenDebugSpawnMenu.GetAsteroidName();
                    this.EnablePhysicsRecursively(base2);
                    base2.Save = true;
                    this.MakeVisible(base2);
                    this.m_shouldMarkForClose = false;
                    MyEntities.RaiseEntityCreated(base2);
                }
                base2.AfterPaste();
            }
            this.Deactivate();
            return true;
        }

        public void SetVoxelMapFromBuilder(MyObjectBuilder_EntityBase voxelMap, IMyStorage storage, Vector3 dragPointDelta, float dragVectorLength)
        {
            if (this.IsActive)
            {
                this.Deactivate();
            }
            this.m_copiedVoxelMaps.Clear();
            this.m_copiedVoxelMapOffsets.Clear();
            this.m_copiedStorages.Clear();
            GetPasteMatrix();
            this.m_dragPointToPositionLocal = dragPointDelta;
            this.m_dragDistance = dragVectorLength;
            Vector3 zero = Vector3.Zero;
            if (voxelMap is MyObjectBuilder_Planet)
            {
                zero = storage.Size / 2f;
            }
            this.SetVoxelMapFromBuilderInternal(voxelMap, storage, zero);
            this.Activate();
        }

        private void SetVoxelMapFromBuilderInternal(MyObjectBuilder_EntityBase voxelMap, IMyStorage storage, Vector3 offset)
        {
            this.m_copiedVoxelMaps.Add(voxelMap);
            this.m_copiedStorages.Add(storage);
            this.m_copiedVoxelMapOffsets.Add(offset);
        }

        public void Show()
        {
            if (this.IsActive && (this.m_previewVoxelMaps.Count == 0))
            {
                this.ChangeClipboardPreview(true);
            }
        }

        private bool TestPlacement()
        {
            bool flag;
            if (!MyEntities.IsInsideWorld(this.m_pastePosition))
            {
                return false;
            }
            if (MySession.Static.ControlledEntity == null)
            {
                goto TR_0001;
            }
            else
            {
                if (((MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Entity) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.ThirdPersonSpectator)) || (MySession.Static.GetCameraControllerEnum() == MyCameraControllerEnum.Spectator))
                {
                    int num = 0;
                    while (true)
                    {
                        ClearToken<MyEntity> token;
                        if (num < this.m_previewVoxelMaps.Count)
                        {
                            if (!MyEntities.IsInsideWorld(this.m_previewVoxelMaps[num].PositionComp.GetPosition()))
                            {
                                return false;
                            }
                            BoundingBoxD worldAABB = this.m_previewVoxelMaps[num].PositionComp.WorldAABB;
                            using (token = this.m_tmpResultList.GetClearToken<MyEntity>())
                            {
                                MyGamePruningStructure.GetTopMostEntitiesInBox(ref worldAABB, this.m_tmpResultList, MyEntityQueryType.Both);
                                if (!this.TestPlacement(this.m_tmpResultList))
                                {
                                    return false;
                                }
                            }
                            num++;
                            continue;
                        }
                        else if (this.m_planetMode)
                        {
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= this.m_copiedVoxelMaps.Count)
                                {
                                    break;
                                }
                                MyObjectBuilder_Planet planet = this.m_copiedVoxelMaps[num2] as MyObjectBuilder_Planet;
                                if (planet != null)
                                {
                                    using (token = this.m_tmpResultList.GetClearToken<MyEntity>())
                                    {
                                        BoundingSphereD sphere = new BoundingSphereD(this.m_pastePosition, (double) (planet.Radius * 1.1f));
                                        MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, this.m_tmpResultList, MyEntityQueryType.Both);
                                        if (!this.TestPlacement(this.m_tmpResultList))
                                        {
                                            return false;
                                        }
                                    }
                                }
                                num2++;
                            }
                        }
                        break;
                    }
                }
                goto TR_0001;
            }
            return flag;
        TR_0001:
            return true;
        }

        private bool TestPlacement(List<MyEntity> entities)
        {
            using (List<MyEntity>.Enumerator enumerator = entities.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyEntity current = enumerator.Current;
                    if (!(current is MyVoxelBase) && (!(current is MyCubeGrid) || !(current as MyCubeGrid).IsStatic))
                    {
                        entities.Clear();
                        return false;
                    }
                }
            }
            return true;
        }

        public void Update()
        {
            if (this.IsActive && this.m_visible)
            {
                this.UpdatePastePosition();
                this.UpdateVoxelMapTransformations();
                this.m_canBePlaced = this.TestPlacement();
            }
        }

        private void UpdatePastePosition()
        {
            MatrixD pasteMatrix = GetPasteMatrix();
            Vector3D vectord = pasteMatrix.Forward * this.m_dragDistance;
            this.m_pastePosition = pasteMatrix.Translation + vectord;
            if (MyDebugDrawSettings.DEBUG_DRAW_COPY_PASTE)
            {
                MyRenderProxy.DebugDrawSphere(this.m_pastePosition, 0.15f, Color.Pink.ToVector3(), 1f, false, false, true, false);
            }
        }

        private void UpdateVoxelMapTransformations()
        {
            Color color = this.m_canBePlaced ? Color.Green : Color.Red;
            if (this.m_planetMode)
            {
                for (int i = 0; i < this.m_copiedVoxelMaps.Count; i++)
                {
                    MyObjectBuilder_Planet planet = this.m_copiedVoxelMaps[i] as MyObjectBuilder_Planet;
                    if (planet != null)
                    {
                        MyRenderProxy.DebugDrawSphere(this.m_pastePosition, planet.Radius * 1.1f, color, 1f, true, true, true, false);
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.m_previewVoxelMaps.Count; i++)
                {
                    this.m_previewVoxelMaps[i].PositionLeftBottomCorner = (this.m_pastePosition + this.m_copiedVoxelMapOffsets[i]) - (this.m_previewVoxelMaps[i].Storage.Size * 0.5f);
                    this.m_previewVoxelMaps[i].PositionComp.SetPosition(this.m_pastePosition + this.m_copiedVoxelMapOffsets[i], null, false, true);
                    MatrixD worldMatrix = this.m_previewVoxelMaps[i].PositionComp.WorldMatrix;
                    BoundingBoxD localbox = new BoundingBoxD((Vector3D) (-this.m_previewVoxelMaps[i].Storage.Size * 0.5f), (Vector3D) (this.m_previewVoxelMaps[i].Storage.Size * 0.5f));
                    MyStringId? faceMaterial = null;
                    MySimpleObjectDraw.DrawTransparentBox(ref worldMatrix, ref localbox, ref color, MySimpleObjectRasterizer.Wireframe, 1, 0.04f, faceMaterial, new MyStringId?(ID_GIZMO_DRAW_LINE), false, -1, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                }
            }
        }

        public bool IsActive { get; private set; }
    }
}

