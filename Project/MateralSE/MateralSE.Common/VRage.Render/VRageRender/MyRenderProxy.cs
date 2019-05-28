namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unsharper;
    using VRage;
    using VRage.FileSystem;
    using VRage.Generics;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.ExternalApp;
    using VRageRender.Import;
    using VRageRender.Messages;
    using VRageRender.Voxels;

    [UnsharperStaticInitializersPriority(1)]
    public static class MyRenderProxy
    {
        private static readonly char[] m_invalidGeneratedTextureChars = new char[] { '(', ')', '<', '>', '|', '\\', '/', '\0', '\t', ' ', '.', ',' };
        public static MyStatsState DrawRenderStats = MyStatsState.NoDraw;
        public static List<Vector3D> PointsForVoxelPrecache = new List<Vector3D>();
        private static bool m_settingsDirty = true;
        private static IMyRender m_render;
        public const uint RENDER_ID_UNASSIGNED = uint.MaxValue;
        public static MyRenderSettings Settings = MyRenderSettings.Default;
        public static MyRenderDebugOverrides DebugOverrides = new MyRenderDebugOverrides();
        public static MyMessagePool MessagePool = new MyMessagePool();
        public static Action WaitForFlushDelegate = null;
        public static bool LimitMaxQueueSize = false;
        public static bool EnableAppEventsCall = true;
        public static bool EnableAutoReenablingAsserts = true;
        private static readonly bool[] m_trackObjectType = new bool[11];
        private static readonly Dictionary<uint, ObjectType> m_objectTypes = new Dictionary<uint, ObjectType>();
        private static readonly HashSet<uint> m_objectsToRemove = new HashSet<uint>();
        public static float CPULoad;
        public static float CPULoadSmooth;
        public static float CPUTimeSmooth;
        public static float GPULoad;
        public static float GPULoadSmooth;
        public static float GPUTimeSmooth;
        private static ObjectType[] TYPE_ENTITY = new ObjectType[1];
        private static ObjectType[] TYPE_ENTITY_AND_CULL;

        static MyRenderProxy()
        {
            ObjectType[] typeArray1 = new ObjectType[2];
            typeArray1[1] = ObjectType.ManualCull;
            TYPE_ENTITY_AND_CULL = typeArray1;
        }

        public static void AddBillboard(MyBillboard billboard)
        {
            if (DebugOverrides.BillboardsStatic)
            {
                BillboardsWrite.Add(billboard);
            }
        }

        public static void AddBillboards(List<MyBillboard> billboards)
        {
            if (DebugOverrides.BillboardsStatic)
            {
                BillboardsWrite.AddList<MyBillboard>(billboards);
            }
        }

        public static void AddBillboardViewProjection(int id, MyBillboardViewProjection billboardViewProjection)
        {
            MyBillboardViewProjection projection;
            if (!BillboardsViewProjectionWrite.TryGetValue(id, out projection))
            {
                BillboardsViewProjectionWrite.Add(id, billboardViewProjection);
            }
            else
            {
                BillboardsViewProjectionWrite[id] = billboardViewProjection;
            }
        }

        public static MyBillboard AddPersistentBillboard() => 
            m_render.SharedData.AddPersistentBillboard();

        public static void AddRuntimeModel(string name, MyRenderMessageAddRuntimeModel message)
        {
            message.Name = name;
            EnqueueMessage(message);
        }

        public static void AddToParticleTextureArray(HashSet<string> textures)
        {
            MyRenderMessageAddToParticleTextureArray message = MessagePool.Get<MyRenderMessageAddToParticleTextureArray>(MyRenderMessageEnum.AddToParticleTextureArray);
            message.Files = textures;
            EnqueueMessage(message);
        }

        public static void AfterRender()
        {
            if (m_render.SharedData != null)
            {
                m_render.SharedData.AfterRender();
            }
        }

        public static void AfterUpdate(MyTimeSpan? updateTimestamp)
        {
            if (m_render.SharedData != null)
            {
                m_render.SharedData.AfterUpdate(updateTimestamp);
            }
        }

        public static uint AllocateObjectId(ObjectType type, bool track = true) => 
            GetMessageId(type, track);

        public static void Ansel_DrawScene()
        {
            m_render.Ansel_DrawScene();
        }

        public static void ApplyActionOnPersistentBillboards(Action<MyBillboard> a)
        {
            m_render.SharedData.ApplyActionOnPersistentBillboards(a);
        }

        public static void ApplySettings(MyRenderDeviceSettings settings)
        {
            m_render.ApplySettings(settings);
        }

        [Conditional("DEBUG")]
        public static void AssertRenderThread()
        {
        }

        public static void BeforeRender(MyTimeSpan? currentDrawTime)
        {
            m_render.SharedData.BeforeRender(currentDrawTime);
        }

        public static void BeforeUpdate()
        {
            if (m_render.SharedData != null)
            {
                m_render.SharedData.BeforeUpdate();
            }
        }

        public static void ChangeMaterialTexture(uint id, Dictionary<string, MyTextureChange> textureChanges)
        {
            MyRenderMessageChangeMaterialTexture message = MessagePool.Get<MyRenderMessageChangeMaterialTexture>(MyRenderMessageEnum.ChangeMaterialTexture);
            Dictionary<string, MyTextureChange> changes = message.Changes;
            message.Changes = textureChanges;
            message.RenderObjectID = id;
            EnqueueMessage(message);
        }

        public static void ChangeMaterialTexture(uint id, string materialName, string colorMetalFileName = null, string normalGlossFileName = null, string extensionsFileName = null, string alphamaskFileName = null)
        {
            MyRenderMessageChangeMaterialTexture message = MessagePool.Get<MyRenderMessageChangeMaterialTexture>(MyRenderMessageEnum.ChangeMaterialTexture);
            if (message.Changes == null)
            {
                message.Changes = new Dictionary<string, MyTextureChange>();
            }
            MyTextureChange change = new MyTextureChange {
                ColorMetalFileName = colorMetalFileName,
                NormalGlossFileName = normalGlossFileName,
                ExtensionsFileName = extensionsFileName,
                AlphamaskFileName = alphamaskFileName
            };
            message.Changes.Add(materialName, change);
            message.RenderObjectID = id;
            EnqueueMessage(message);
        }

        [Obsolete]
        public static void ChangeModel(uint id, string model, float scale = 1f)
        {
            MyRenderMessageChangeModel message = MessagePool.Get<MyRenderMessageChangeModel>(MyRenderMessageEnum.ChangeModel);
            message.ID = id;
            message.Model = model;
            message.Scale = scale;
            EnqueueMessage(message);
        }

        [Conditional("DEBUG")]
        public static void CheckMessageId(uint GID, ObjectType[] allowedTypes = null)
        {
            ObjectType objectType = GetObjectType(GID);
            if ((objectType != ObjectType.Invalid) && (allowedTypes != null))
            {
                ObjectType[] typeArray = allowedTypes;
                for (int i = 0; (i < typeArray.Length) && (typeArray[i] != objectType); i++)
                {
                }
            }
        }

        private static void CheckRenderObjectIds()
        {
            Dictionary<uint, ObjectType> objectTypes = m_objectTypes;
            lock (objectTypes)
            {
            }
        }

        public static void CheckValidGeneratedTextureName(string name)
        {
            if (!IsValidGeneratedTextureName(name))
            {
                throw new Exception("Generated texture must not contain any of the following characters: '(', ')','<', '>', '|', '\\', '/', '\0', '\t', ' ', '.', ','");
            }
        }

        public static void ClearDecals()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageClearScreenDecals>(MyRenderMessageEnum.ClearDecals));
        }

        public static void ClearLargeMessages()
        {
            MessagePool.Clear(MyRenderMessageEnum.CreateRenderInstanceBuffer);
            MessagePool.Clear(MyRenderMessageEnum.UpdateRenderCubeInstanceBuffer);
        }

        public static void ClearLightShadowIgnore(uint id)
        {
            MyRenderMessageClearLightShadowIgnore message = MessagePool.Get<MyRenderMessageClearLightShadowIgnore>(MyRenderMessageEnum.ClearLightShadowIgnore);
            message.ID = id;
            EnqueueMessage(message);
        }

        public static void CloseVideo(uint id)
        {
            MyRenderMessageCloseVideo message = MessagePool.Get<MyRenderMessageCloseVideo>(MyRenderMessageEnum.CloseVideo);
            message.ID = id;
            EnqueueMessage(message);
        }

        public static void CollectGarbage()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageCollectGarbage>(MyRenderMessageEnum.CollectGarbage));
        }

        public static uint CreateDecal(uint[] parentIds, ref MyDecalTopoData data, MyDecalFlags flags, string sourceTarget, string material, int matIndex)
        {
            MyRenderMessageCreateScreenDecal message = MessagePool.Get<MyRenderMessageCreateScreenDecal>(MyRenderMessageEnum.CreateScreenDecal);
            message.ID = GetMessageId(ObjectType.ScreenDecal, false);
            message.ParentIDs = parentIds;
            message.TopoData = data;
            message.SourceTarget = sourceTarget;
            message.Flags = flags;
            message.Material = material;
            message.MaterialIndex = matIndex;
            EnqueueMessage(message);
            return message.ID;
        }

        public static MyRenderDeviceSettings CreateDevice(MyRenderThread renderThread, IntPtr windowHandle, MyRenderDeviceSettings? settingsToTry, out MyAdapterInfo[] adaptersList)
        {
            RenderThread = renderThread;
            return m_render.CreateDevice(windowHandle, settingsToTry, out adaptersList);
        }

        public static void CreateFont(int fontId, string fontPath, bool isDebugFont = false, string targetTexture = null, Color? colorMask = new Color?())
        {
            MyRenderMessageCreateFont message = MessagePool.Get<MyRenderMessageCreateFont>(MyRenderMessageEnum.CreateFont);
            message.FontId = fontId;
            message.FontPath = fontPath;
            message.IsDebugFont = isDebugFont;
            message.ColorMask = colorMask;
            EnqueueMessage(message);
        }

        public static void CreateGeneratedTexture(string textureName, int width, int height, MyGeneratedTextureType type = 0, int numMiplevels = 1)
        {
            CheckValidGeneratedTextureName(textureName);
            if (numMiplevels <= 0)
            {
                throw new ArgumentOutOfRangeException("numMiplevels");
            }
            MyRenderMessageCreateGeneratedTexture message = MessagePool.Get<MyRenderMessageCreateGeneratedTexture>(MyRenderMessageEnum.CreateGeneratedTexture);
            message.TextureName = textureName;
            message.Width = width;
            message.Height = height;
            message.Type = type;
            EnqueueMessage(message);
        }

        public static uint CreateGPUEmitter(string debugName)
        {
            uint messageId = GetMessageId(ObjectType.GPUEmitter, true);
            MyRenderMessageCreateGPUEmitter message = MessagePool.Get<MyRenderMessageCreateGPUEmitter>(MyRenderMessageEnum.CreateGPUEmitter);
            message.ID = messageId;
            message.DebugName = debugName;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateLineBasedObject(string colorMetalTexture, string normalGlossTexture, string extensionTexture, string debugName)
        {
            uint messageId = GetMessageId(ObjectType.Entity, true);
            MyRenderMessageCreateLineBasedObject message = MessagePool.Get<MyRenderMessageCreateLineBasedObject>(MyRenderMessageEnum.CreateLineBasedObject);
            message.ID = messageId;
            message.ColorMetalTexture = colorMetalTexture;
            message.NormalGlossTexture = normalGlossTexture;
            message.ExtensionTexture = extensionTexture;
            message.DebugName = debugName;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateManualCullObject(string debugName, MatrixD worldMatrix)
        {
            uint messageId = GetMessageId(ObjectType.ManualCull, true);
            MyRenderMessageCreateManualCullObject message = MessagePool.Get<MyRenderMessageCreateManualCullObject>(MyRenderMessageEnum.CreateManualCullObject);
            message.ID = messageId;
            message.DebugName = debugName;
            message.WorldMatrix = worldMatrix;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateRenderCharacter(string debugName, string lod0, MatrixD worldMatrix, Color? diffuseColor, Vector3? colorMaskHSV, RenderFlags flags, bool fadeIn)
        {
            uint messageId = GetMessageId(ObjectType.Entity, true);
            MyRenderMessageCreateRenderCharacter message = MessagePool.Get<MyRenderMessageCreateRenderCharacter>(MyRenderMessageEnum.CreateRenderCharacter);
            message.ID = messageId;
            message.DebugName = debugName;
            message.Model = lod0;
            message.WorldMatrix = worldMatrix;
            message.DiffuseColor = diffuseColor;
            message.ColorMaskHSV = colorMaskHSV;
            message.Flags = flags;
            message.FadeIn = fadeIn;
            EnqueueMessage(message);
            float? dithering = null;
            UpdateRenderEntity(messageId, diffuseColor, colorMaskHSV, dithering, false);
            return messageId;
        }

        public static uint CreateRenderEntity(string debugName, string model, MatrixD worldMatrix, MyMeshDrawTechnique technique, RenderFlags flags, CullingOptions cullingOptions, Color diffuseColor, Vector3 colorMaskHsv, float dithering = 0f, float maxViewDistance = 3.402823E+38f, byte depthBias = 0, float rescale = 1f, bool fadeIn = false)
        {
            uint messageId = GetMessageId(ObjectType.Entity, true);
            MyRenderMessageCreateRenderEntity message = MessagePool.Get<MyRenderMessageCreateRenderEntity>(MyRenderMessageEnum.CreateRenderEntity);
            message.ID = messageId;
            message.DebugName = debugName;
            message.Model = model;
            message.WorldMatrix = worldMatrix;
            message.Technique = technique;
            message.Flags = flags;
            message.CullingOptions = cullingOptions;
            message.MaxViewDistance = maxViewDistance;
            message.Rescale = rescale;
            message.DepthBias = depthBias;
            EnqueueMessage(message);
            UpdateRenderEntity(messageId, new Color?(diffuseColor), new Vector3?(colorMaskHsv), new float?(dithering), fadeIn);
            return messageId;
        }

        public static uint CreateRenderEntityAtmosphere(string debugName, string model, MatrixD worldMatrix, MyMeshDrawTechnique technique, RenderFlags flags, CullingOptions cullingOptions, float atmosphereRadius, float planetRadius, Vector3 atmosphereWavelengths, float dithering = 0f, float maxViewDistance = 3.402823E+38f, bool fadeIn = false)
        {
            uint messageId = GetMessageId(ObjectType.Atmosphere, true);
            MyRenderMessageCreateRenderEntityAtmosphere message = MessagePool.Get<MyRenderMessageCreateRenderEntityAtmosphere>(MyRenderMessageEnum.CreateRenderEntityAtmosphere);
            message.ID = messageId;
            message.DebugName = debugName;
            message.Model = model;
            message.WorldMatrix = worldMatrix;
            message.Technique = technique;
            message.Flags = flags;
            message.CullingOptions = cullingOptions;
            message.MaxViewDistance = maxViewDistance;
            message.AtmosphereRadius = atmosphereRadius;
            message.PlanetRadius = planetRadius;
            message.AtmosphereWavelengths = atmosphereWavelengths;
            message.FadeIn = fadeIn;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateRenderEntityCloudLayer(uint atmosphereId, string debugName, string model, List<string> textures, Vector3D centerPoint, double altitude, double minScaledAltitude, bool scalingEnabled, double fadeOutRelativeAltitudeStart, double fadeOutRelativeAltitudeEnd, float applyFogRelativeDistance, double maxPlanetHillRadius, Vector3D rotationAxis, float angularVelocity, float initialRotation, Vector4 color, bool fadeIn)
        {
            uint messageId = GetMessageId(ObjectType.Cloud, true);
            MyCloudLayerSettingsRender render = new MyCloudLayerSettingsRender {
                ID = messageId,
                AtmosphereID = atmosphereId,
                Model = model,
                Textures = textures,
                CenterPoint = centerPoint,
                Altitude = altitude,
                MinScaledAltitude = minScaledAltitude,
                ScalingEnabled = scalingEnabled,
                DebugName = debugName,
                RotationAxis = rotationAxis,
                AngularVelocity = angularVelocity,
                InitialRotation = initialRotation,
                MaxPlanetHillRadius = maxPlanetHillRadius,
                FadeOutRelativeAltitudeStart = fadeOutRelativeAltitudeStart,
                FadeOutRelativeAltitudeEnd = fadeOutRelativeAltitudeEnd,
                ApplyFogRelativeDistance = applyFogRelativeDistance,
                Color = color,
                FadeIn = fadeIn
            };
            MyRenderMessageCreateRenderEntityClouds message = MessagePool.Get<MyRenderMessageCreateRenderEntityClouds>(MyRenderMessageEnum.CreateRenderEntityClouds);
            message.Settings = render;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateRenderInstanceBuffer(string debugName, MyRenderInstanceBufferType type, uint parentId = 0xffffffff)
        {
            uint messageId = GetMessageId(ObjectType.InstanceBuffer, true);
            MyRenderMessageCreateRenderInstanceBuffer message = MessagePool.Get<MyRenderMessageCreateRenderInstanceBuffer>(MyRenderMessageEnum.CreateRenderInstanceBuffer);
            message.ID = messageId;
            message.ParentID = parentId;
            message.DebugName = debugName;
            message.Type = type;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateRenderLight(string debugName)
        {
            uint messageId = GetMessageId(ObjectType.Light, true);
            MyRenderMessageCreateRenderLight message = MessagePool.Get<MyRenderMessageCreateRenderLight>(MyRenderMessageEnum.CreateRenderLight);
            message.ID = messageId;
            message.DebugName = debugName;
            EnqueueMessage(message);
            return messageId;
        }

        public static uint CreateRenderVoxelDebris(string debugName, string model, MatrixD worldMatrix, float textureCoordOffset, float textureCoordScale, float textureColorMultiplier, byte voxelMaterialIndex, bool fadeIn)
        {
            uint messageId = GetMessageId(ObjectType.Entity, true);
            MyRenderMessageCreateRenderVoxelDebris message = MessagePool.Get<MyRenderMessageCreateRenderVoxelDebris>(MyRenderMessageEnum.CreateRenderVoxelDebris);
            message.ID = messageId;
            message.DebugName = debugName;
            message.Model = model;
            message.WorldMatrix = worldMatrix;
            message.TextureCoordOffset = textureCoordOffset;
            message.TextureCoordScale = textureCoordScale;
            message.TextureColorMultiplier = textureColorMultiplier;
            message.VoxelMaterialIndex = voxelMaterialIndex;
            message.FadeIn = fadeIn;
            EnqueueMessage(message);
            return messageId;
        }

        public static void CreateRenderVoxelMaterials(MyRenderVoxelMaterialData[] materials)
        {
            MyRenderMessageCreateRenderVoxelMaterials message = MessagePool.Get<MyRenderMessageCreateRenderVoxelMaterials>(MyRenderMessageEnum.CreateRenderVoxelMaterials);
            message.Materials = materials;
            EnqueueMessage(message);
        }

        public static uint CreateStaticGroup(string model, Vector3D translation, Matrix[] localMatrices)
        {
            uint messageId = GetMessageId(ObjectType.Entity, true);
            MyRenderMessageCreateStaticGroup message = MessagePool.Get<MyRenderMessageCreateStaticGroup>(MyRenderMessageEnum.CreateStaticGroup);
            message.ID = messageId;
            message.Model = model;
            message.Translation = translation;
            message.LocalMatrices = localMatrices;
            EnqueueMessage(message);
            return messageId;
        }

        public static void DebugClearPersistentMessages()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageDebugClearPersistentMessages>(MyRenderMessageEnum.DebugClearPersistentMessages));
        }

        public static void DebugCrashRenderThread()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageDebugCrashRenderThread>(MyRenderMessageEnum.DebugCrashRenderThread));
        }

        public static void DebugDraw6FaceConvex(Vector3D[] vertices, Color color, float alpha, bool depthRead, bool fill, bool persistent = false)
        {
            MyRenderMessageDebugDraw6FaceConvex message = MessagePool.Get<MyRenderMessageDebugDraw6FaceConvex>(MyRenderMessageEnum.DebugDraw6FaceConvex);
            message.Vertices = (Vector3D[]) vertices.Clone();
            message.Color = color;
            message.Alpha = alpha;
            message.DepthRead = depthRead;
            message.Fill = fill;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawAABB(BoundingBoxD aabb, Color color, float alpha = 1f, float scale = 1f, bool depthRead = true, bool shaded = false, bool persistent = false)
        {
            MyRenderMessageDebugDrawAABB message = MessagePool.Get<MyRenderMessageDebugDrawAABB>(MyRenderMessageEnum.DebugDrawAABB);
            message.AABB = aabb;
            message.Color = color;
            message.Alpha = alpha;
            message.Scale = scale;
            message.DepthRead = depthRead;
            message.Shaded = shaded;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawArrow3D(Vector3D pointFrom, Vector3D pointTo, Color colorFrom, Color? colorTo = new Color?(), bool depthRead = false, double tipScale = 0.1, string text = null, float textSize = 0.5f, bool persistent = false)
        {
            Color? nullable = colorTo;
            Color color = (nullable != null) ? nullable.GetValueOrDefault() : colorFrom;
            Vector3D v = pointTo - pointFrom;
            double num = v.Length();
            if (num > 9.9999997473787516E-05)
            {
                tipScale *= num;
                v /= num;
                Vector3D vectord2 = Vector3D.CalculatePerpendicularVector(v);
                v *= tipScale;
                Vector3D vectord3 = Vector3D.Cross(vectord2, v) * tipScale;
                vectord2 *= tipScale;
                DebugDrawLine3D(pointTo, (pointTo + vectord2) - v, color, color, depthRead, persistent);
                DebugDrawLine3D(pointTo, (pointTo - vectord2) - v, color, color, depthRead, persistent);
                DebugDrawLine3D(pointTo, (pointTo + vectord3) - v, color, color, depthRead, persistent);
                DebugDrawLine3D(pointTo, (pointTo - vectord3) - v, color, color, depthRead, persistent);
            }
            DebugDrawLine3D(pointFrom, pointTo, colorFrom, color, depthRead, persistent);
            if ((text != null) && (num > 9.9999997473787516E-05))
            {
                DebugDrawText3D(pointTo + v, text, color, textSize, depthRead, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, persistent);
            }
        }

        public static void DebugDrawArrow3DDir(Vector3D posFrom, Vector3D direction, Color color, Color? colorTo = new Color?(), bool depthRead = false, double tipScale = 0.1, string text = null, float textSize = 0.5f, bool persistent = false)
        {
            Color? nullable = colorTo;
            DebugDrawArrow3D(posFrom, posFrom + direction, color, new Color?((nullable != null) ? nullable.GetValueOrDefault() : color), depthRead, tipScale, text, textSize, persistent);
        }

        public static void DebugDrawAxis(MatrixD matrix, float axisLength, bool depthRead, bool skipScale = false, bool persistent = false)
        {
            MyRenderMessageDebugDrawAxis message = MessagePool.Get<MyRenderMessageDebugDrawAxis>(MyRenderMessageEnum.DebugDrawAxis);
            message.Matrix = matrix;
            message.AxisLength = axisLength;
            message.DepthRead = depthRead;
            message.SkipScale = skipScale;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static IMyDebugDrawBatchAabb DebugDrawBatchAABB(MatrixD worldMatrix, Color color, bool depthRead = true, bool shaded = true) => 
            (!shaded ? ((IMyDebugDrawBatchAabb) new MyDebugDrawBatchAabbLines(DebugDrawLine3DOpenBatch(depthRead, false), ref worldMatrix, color, depthRead)) : ((IMyDebugDrawBatchAabb) new MyDebugDrawBatchAabbShaded(PrepareDebugDrawTriangles(), ref worldMatrix, color, depthRead)));

        public static void DebugDrawCapsule(Vector3D p0, Vector3D p1, float radius, Color color, bool depthRead, bool shaded = false, bool persistent = false)
        {
            MyRenderMessageDebugDrawCapsule message = MessagePool.Get<MyRenderMessageDebugDrawCapsule>(MyRenderMessageEnum.DebugDrawCapsule);
            message.P0 = p0;
            message.P1 = p1;
            message.Radius = radius;
            message.Color = color;
            message.DepthRead = depthRead;
            message.Shaded = shaded;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawCone(Vector3D translation, Vector3D directionVec, Vector3D baseVec, Color color, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawCone message = MessagePool.Get<MyRenderMessageDebugDrawCone>(MyRenderMessageEnum.DebugDrawCone);
            message.Translation = translation;
            message.DirectionVector = directionVec;
            message.BaseVector = baseVec;
            message.DepthRead = depthRead;
            message.Color = color;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawCross(Vector3D center, Vector3D normal, Vector3D face, Color color, bool depthRead = false, bool persistent = false)
        {
            Vector3D vectord = Vector3D.Cross(face, Vector3.Normalize(normal));
            Vector3D pointTo = center - vectord;
            DebugDrawLine3D(center + face, center - face, color, color, depthRead, persistent);
            DebugDrawLine3D(center + vectord, pointTo, color, color, depthRead, persistent);
        }

        public static void DebugDrawCylinder(MatrixD matrix, Color color, float alpha, bool depthRead, bool smooth, bool persistent = false)
        {
            MyRenderMessageDebugDrawCylinder message = MessagePool.Get<MyRenderMessageDebugDrawCylinder>(MyRenderMessageEnum.DebugDrawCylinder);
            message.Matrix = matrix;
            message.Color = color;
            message.Alpha = alpha;
            message.DepthRead = depthRead;
            message.Smooth = smooth;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static unsafe void DebugDrawCylinder(MatrixD worldMatrix, Vector3D vertexA, Vector3D vertexB, float radius, Color color, float alpha, bool depthRead, bool smooth, bool persistent = false)
        {
            Vector3 vector = (Vector3) (vertexB - vertexA);
            float yScale = vector.Length();
            float xScale = 2f * radius;
            Matrix identity = Matrix.Identity;
            identity.Up = vector / yScale;
            Matrix* matrixPtr1 = (Matrix*) ref identity;
            matrixPtr1.Right = Vector3.CalculatePerpendicularVector(identity.Up);
            Matrix* matrixPtr2 = (Matrix*) ref identity;
            matrixPtr2.Forward = Vector3.Cross(identity.Up, identity.Right);
            identity = Matrix.CreateScale(xScale, yScale, xScale) * identity;
            identity.Translation = (Vector3) ((vertexA + vertexB) * 0.5);
            DebugDrawCylinder(identity * worldMatrix, color, alpha, depthRead, smooth, persistent);
        }

        public static unsafe void DebugDrawCylinder(Vector3D position, Quaternion orientation, float radius, float height, Color color, float alpha, bool depthRead, bool smooth, bool persistent = false)
        {
            MatrixD matrix = MatrixD.CreateFromQuaternion(orientation);
            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
            xdPtr1.Right *= 2f * radius;
            MatrixD* xdPtr2 = (MatrixD*) ref matrix;
            xdPtr2.Forward *= 2f * radius;
            MatrixD* xdPtr3 = (MatrixD*) ref matrix;
            xdPtr3.Up *= height;
            matrix.Translation = position;
            DebugDrawCylinder(matrix, color, alpha, depthRead, smooth, persistent);
        }

        public static unsafe void DebugDrawCylinder(Vector3D position, QuaternionD orientation, double radius, double height, Color color, float alpha, bool depthRead, bool smooth, bool persistent = false)
        {
            MatrixD matrix = MatrixD.CreateFromQuaternion(orientation);
            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
            xdPtr1.Right *= 2.0 * radius;
            MatrixD* xdPtr2 = (MatrixD*) ref matrix;
            xdPtr2.Forward *= 2.0 * radius;
            MatrixD* xdPtr3 = (MatrixD*) ref matrix;
            xdPtr3.Up *= height;
            matrix.Translation = position;
            DebugDrawCylinder(matrix, color, alpha, depthRead, smooth, persistent);
        }

        public static void DebugDrawFrustrum(BoundingFrustumD frustrum, Color color, float alpha, bool depthRead, bool smooth = false, bool persistent = false)
        {
            MyRenderMessageDebugDrawFrustrum message = MessagePool.Get<MyRenderMessageDebugDrawFrustrum>(MyRenderMessageEnum.DebugDrawFrustrum);
            message.Frustum = frustrum;
            message.Color = color;
            message.Alpha = alpha;
            message.DepthRead = depthRead;
            message.Smooth = smooth;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawLine2D(Vector2 pointFrom, Vector2 pointTo, Color colorFrom, Color colorTo, Matrix? projection = new Matrix?(), bool persistent = false)
        {
            MyRenderMessageDebugDrawLine2D message = MessagePool.Get<MyRenderMessageDebugDrawLine2D>(MyRenderMessageEnum.DebugDrawLine2D);
            message.PointFrom = pointFrom;
            message.PointTo = pointTo;
            message.ColorFrom = colorFrom;
            message.ColorTo = colorTo;
            message.Projection = projection;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawLine3D(Vector3D pointFrom, Vector3D pointTo, Color colorFrom, Color colorTo, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawLine3D message = MessagePool.Get<MyRenderMessageDebugDrawLine3D>(MyRenderMessageEnum.DebugDrawLine3D);
            message.PointFrom = pointFrom;
            message.PointTo = pointTo;
            message.ColorFrom = colorFrom;
            message.ColorTo = colorTo;
            message.DepthRead = depthRead;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static MyRenderMessageDebugDrawLine3DBatch DebugDrawLine3DOpenBatch(bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawLine3DBatch local1 = MessagePool.Get<MyRenderMessageDebugDrawLine3DBatch>(MyRenderMessageEnum.DebugDrawLine3DBatch);
            local1.DepthRead = depthRead;
            local1.Persistent = persistent;
            return local1;
        }

        public static void DebugDrawLine3DSubmitBatch(MyRenderMessageDebugDrawLine3DBatch message)
        {
            EnqueueMessage(message);
        }

        public static uint DebugDrawMesh(List<MyFormatPositionColor> vertices, MatrixD worldMatrix, bool depthRead, bool shaded)
        {
            MyRenderMessageDebugDrawMesh message = MessagePool.Get<MyRenderMessageDebugDrawMesh>(MyRenderMessageEnum.DebugDrawMesh);
            message.ID = GetMessageId(ObjectType.DebugDrawMesh, true);
            message.Vertices = vertices;
            message.WorldMatrix = worldMatrix;
            message.DepthRead = depthRead;
            message.Shaded = shaded;
            EnqueueMessage(message);
            return message.ID;
        }

        public static void DebugDrawModel(string model, MatrixD worldMatrix, Color color, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawModel message = MessagePool.Get<MyRenderMessageDebugDrawModel>(MyRenderMessageEnum.DebugDrawModel);
            message.Model = model;
            message.WorldMatrix = worldMatrix;
            message.Color = color;
            message.DepthRead = depthRead;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static unsafe void DebugDrawOBB(MyOrientedBoundingBoxD obb, Color color, float alpha, bool depthRead, bool smooth, bool persistent = false)
        {
            MatrixD matrix = MatrixD.CreateFromQuaternion(obb.Orientation);
            MatrixD* xdPtr1 = (MatrixD*) ref matrix;
            xdPtr1.Right *= obb.HalfExtent.X * 2.0;
            MatrixD* xdPtr2 = (MatrixD*) ref matrix;
            xdPtr2.Up *= obb.HalfExtent.Y * 2.0;
            MatrixD* xdPtr3 = (MatrixD*) ref matrix;
            xdPtr3.Forward *= obb.HalfExtent.Z * 2.0;
            matrix.Translation = obb.Center;
            DebugDrawOBB(matrix, color, alpha, depthRead, smooth, true, persistent);
        }

        public static void DebugDrawOBB(MatrixD matrix, Color color, float alpha, bool depthRead, bool smooth, bool cull = true, bool persistent = false)
        {
            MyRenderMessageDebugDrawOBB message = MessagePool.Get<MyRenderMessageDebugDrawOBB>(MyRenderMessageEnum.DebugDrawOBB);
            message.Matrix = matrix;
            message.Color = color;
            message.Alpha = alpha;
            message.DepthRead = depthRead;
            message.Smooth = smooth;
            message.Cull = cull;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawPlane(Vector3D position, Vector3 normal, Color color, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawPlane message = MessagePool.Get<MyRenderMessageDebugDrawPlane>(MyRenderMessageEnum.DebugDrawPlane);
            message.Position = position;
            message.Normal = normal;
            message.Color = color;
            message.DepthRead = depthRead;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawPoint(Vector3D position, Color color, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawPoint message = MessagePool.Get<MyRenderMessageDebugDrawPoint>(MyRenderMessageEnum.DebugDrawPoint);
            message.Position = position;
            message.Color = color;
            message.DepthRead = depthRead;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawSphere(Vector3D position, float radius, Color color, float alpha = 1f, bool depthRead = true, bool smooth = false, bool cull = true, bool persistent = false)
        {
            MyRenderMessageDebugDrawSphere message = MessagePool.Get<MyRenderMessageDebugDrawSphere>(MyRenderMessageEnum.DebugDrawSphere);
            message.Position = position;
            message.Radius = radius;
            message.Color = color;
            message.Alpha = alpha;
            message.DepthRead = depthRead;
            message.Smooth = smooth;
            message.Cull = cull;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawText2D(Vector2 screenCoord, string text, Color color, float scale, MyGuiDrawAlignEnum align = 0, bool persistent = false)
        {
            MyRenderMessageDebugDrawText2D message = MessagePool.Get<MyRenderMessageDebugDrawText2D>(MyRenderMessageEnum.DebugDrawText2D);
            message.Coord = screenCoord;
            message.Text = text;
            message.Color = color;
            message.Scale = scale;
            message.Align = align;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawText3D(Vector3D worldCoord, string text, Color color, float scale, bool depthRead, MyGuiDrawAlignEnum align = 0, int customViewProjection = -1, bool persistent = false)
        {
            MyRenderMessageDebugDrawText3D message = MessagePool.Get<MyRenderMessageDebugDrawText3D>(MyRenderMessageEnum.DebugDrawText3D);
            message.Coord = worldCoord;
            message.Text = text;
            message.Color = color;
            message.Scale = scale;
            message.DepthRead = depthRead;
            message.Align = align;
            message.CustomViewProjection = customViewProjection;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawTriangle(Vector3D vertex0, Vector3D vertex1, Vector3D vertex2, Color color, bool smooth, bool depthRead, bool persistent = false)
        {
            MyRenderMessageDebugDrawTriangle message = MessagePool.Get<MyRenderMessageDebugDrawTriangle>(MyRenderMessageEnum.DebugDrawTriangle);
            message.Vertex0 = vertex0;
            message.Vertex1 = vertex1;
            message.Vertex2 = vertex2;
            message.Color = color;
            message.DepthRead = depthRead;
            message.Smooth = smooth;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawTriangles(IDrawTrianglesMessage msgInterface, MatrixD? worldMatrix = new MatrixD?(), bool depthRead = true, bool shaded = true, bool overlayWireframe = false, bool persistent = false)
        {
            MatrixD? nullable = worldMatrix;
            MyRenderMessageDebugDrawTriangles triangles1 = (MyRenderMessageDebugDrawTriangles) msgInterface;
            MyRenderMessageDebugDrawTriangles triangles2 = (MyRenderMessageDebugDrawTriangles) msgInterface;
            triangles2.WorldMatrix = (nullable != null) ? nullable.GetValueOrDefault() : MatrixD.Identity;
            MyRenderMessageDebugDrawTriangles local1 = triangles2;
            local1.DepthRead = depthRead;
            local1.Shaded = shaded;
            local1.Edges = overlayWireframe || !shaded;
            MyRenderMessageDebugDrawTriangles message = local1;
            message.Persistent = persistent;
            EnqueueMessage(message);
        }

        public static void DebugDrawUpdateMesh(uint ID, List<MyFormatPositionColor> vertices, MatrixD worldMatrix, bool depthRead, bool shaded)
        {
            MyRenderMessageDebugDrawMesh message = MessagePool.Get<MyRenderMessageDebugDrawMesh>(MyRenderMessageEnum.DebugDrawMesh);
            message.ID = ID;
            message.Vertices = vertices;
            message.WorldMatrix = worldMatrix;
            message.DepthRead = depthRead;
            message.Shaded = shaded;
            EnqueueMessage(message);
        }

        public static void DisposeDevice()
        {
            if (m_render != null)
            {
                m_render.DisposeDevice();
            }
            RenderThread = null;
        }

        public static void Draw()
        {
            m_render.Draw(true);
        }

        public static void Draw3DScene()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageDrawScene>(MyRenderMessageEnum.DrawScene));
        }

        public static void DrawBegin()
        {
            m_render.DrawBegin();
        }

        public static void DrawEnd()
        {
            m_render.DrawEnd();
        }

        public static void DrawSprite(string texture, ref RectangleF destination, bool scaleDestination, ref Rectangle? sourceRectangle, Color color, float rotation, Vector2 rightVector, ref Vector2 origin, SpriteEffects effects, float depth, bool waitTillLoaded = true, string targetTexture = null)
        {
            MyRenderMessageDrawSprite message = MessagePool.Get<MyRenderMessageDrawSprite>(MyRenderMessageEnum.DrawSprite);
            message.Texture = texture;
            message.DestinationRectangle = destination;
            message.SourceRectangle = sourceRectangle;
            message.Color = color;
            message.Rotation = rotation;
            message.RightVector = rightVector;
            message.Depth = depth;
            message.Effects = effects;
            message.Origin = origin;
            message.ScaleDestination = scaleDestination;
            message.WaitTillLoaded = waitTillLoaded;
            message.TargetTexture = targetTexture;
            EnqueueMessage(message);
        }

        public static void DrawSprite(string texture, Vector2 normalizedCoord, Vector2 normalizedSize, Color color, MyGuiDrawAlignEnum drawAlign, float rotation, Vector2 rightVector, float scale, Vector2? originNormalized, float rotSpeed = 0f, bool waitTillLoaded = true, string targetTexture = null)
        {
            MyRenderMessageDrawSpriteNormalized message = MessagePool.Get<MyRenderMessageDrawSpriteNormalized>(MyRenderMessageEnum.DrawSpriteNormalized);
            message.Texture = texture;
            message.NormalizedCoord = normalizedCoord;
            message.NormalizedSize = normalizedSize;
            message.Color = color;
            message.DrawAlign = drawAlign;
            message.Rotation = rotation;
            message.RightVector = rightVector;
            message.Scale = scale;
            message.OriginNormalized = originNormalized;
            message.RotationSpeed = rotSpeed;
            message.WaitTillLoaded = waitTillLoaded;
            message.TargetTexture = targetTexture;
            EnqueueMessage(message);
        }

        public static void DrawSpriteAtlas(string texture, Vector2 position, Vector2 textureOffset, Vector2 textureSize, Vector2 rightVector, Vector2 scale, Color color, Vector2 halfSize, string targetTexture = null)
        {
            MyRenderMessageDrawSpriteAtlas message = MessagePool.Get<MyRenderMessageDrawSpriteAtlas>(MyRenderMessageEnum.DrawSpriteAtlas);
            message.Texture = texture;
            message.Position = position;
            message.TextureOffset = textureOffset;
            message.TextureSize = textureSize;
            message.RightVector = rightVector;
            message.Scale = scale;
            message.Color = color;
            message.HalfSize = halfSize;
            message.TargetTexture = targetTexture;
            EnqueueMessage(message);
        }

        public static void DrawString(int fontIndex, Vector2 screenCoord, Color colorMask, string text, float screenScale, float screenMaxWidth, string targetTexture = null)
        {
            MyRenderMessageDrawString message = MessagePool.Get<MyRenderMessageDrawString>(MyRenderMessageEnum.DrawString);
            message.Text = text;
            message.FontIndex = fontIndex;
            message.ScreenCoord = screenCoord;
            message.ColorMask = colorMask;
            message.ScreenScale = screenScale;
            message.ScreenMaxWidth = screenMaxWidth;
            message.TargetTexture = targetTexture;
            EnqueueMessage(message);
        }

        public static void DrawStringAligned(int fontIndex, Vector2 screenCoord, Color colorMask, string text, float screenScale, float screenMaxWidth, string targetTexture = null, int textureWidthinPx = 0x200, MyRenderTextAlignmentEnum align = 0)
        {
            MyRenderMessageDrawStringAligned message = MessagePool.Get<MyRenderMessageDrawStringAligned>(MyRenderMessageEnum.DrawStringAligned);
            message.Text = text;
            message.FontIndex = fontIndex;
            message.ScreenCoord = screenCoord;
            message.ColorMask = colorMask;
            message.ScreenScale = screenScale;
            message.ScreenMaxWidth = screenMaxWidth;
            message.TargetTexture = targetTexture;
            message.TextureWidthInPx = textureWidthinPx;
            message.Alignment = align;
            EnqueueMessage(message);
        }

        public static void DrawVideo(uint id, Rectangle rect, Color color, MyVideoRectangleFitMode fitMode)
        {
            MyRenderMessageDrawVideo message = MessagePool.Get<MyRenderMessageDrawVideo>(MyRenderMessageEnum.DrawVideo);
            message.ID = id;
            message.Rectangle = rect;
            message.Color = color;
            message.FitMode = fitMode;
            EnqueueMessage(message);
        }

        public static void EnableAtmosphere(bool enabled)
        {
            MyRenderMessageEnableAtmosphere message = MessagePool.Get<MyRenderMessageEnableAtmosphere>(MyRenderMessageEnum.EnableAtmosphere);
            message.Enabled = enabled;
            EnqueueMessage(message);
        }

        public static void EnqueueMainThreadCallback(Action callback)
        {
            MyRenderMessageMainThreadCallback message = MessagePool.Get<MyRenderMessageMainThreadCallback>(MyRenderMessageEnum.MainThreadCallback);
            message.Callback = callback;
            EnqueueOutputMessage(message);
        }

        private static void EnqueueMessage(MyRenderMessageBase message)
        {
            m_render.EnqueueMessage(message, LimitMaxQueueSize);
        }

        private static void EnqueueOutputMessage(MyRenderMessageBase message)
        {
            m_render.EnqueueOutputMessage(message);
        }

        public static void Error(string messageText, int skipStack = 0, bool shouldTerminate = false)
        {
            StackTrace trace = new StackTrace(1 + skipStack, true);
            MyRenderMessageError message = MessagePool.Get<MyRenderMessageError>(MyRenderMessageEnum.Error);
            message.Callstack = trace.ToString();
            message.Message = messageText;
            message.ShouldTerminate = shouldTerminate;
            EnqueueOutputMessage(message);
        }

        public static void ExportToObjComplete(bool success, string filename)
        {
            MyRenderMessageExportToObjComplete message = MessagePool.Get<MyRenderMessageExportToObjComplete>(MyRenderMessageEnum.ExportToObjComplete);
            message.Success = success;
            message.Filename = filename;
            EnqueueOutputMessage(message);
        }

        public static void GenerateShaderCache(bool clean, OnShaderCacheProgressDelegate onShaderCacheProgress)
        {
            m_render.GenerateShaderCache(clean, onShaderCacheProgress);
        }

        public static long GetAvailableTextureMemory() => 
            m_render.GetAvailableTextureMemory();

        public static string GetLastExecutedAnnotation() => 
            m_render.GetLastExecutedAnnotation();

        private static uint GetMessageId(ObjectType type, bool track = true)
        {
            Dictionary<uint, ObjectType> objectTypes = m_objectTypes;
            lock (objectTypes)
            {
                if (track)
                {
                    TrackNewMessageId(type);
                }
                uint globalMessageCounter = m_render.GlobalMessageCounter;
                m_render.GlobalMessageCounter = globalMessageCounter + 1;
                return globalMessageCounter;
            }
        }

        public static ObjectType GetObjectType(uint GID)
        {
            Dictionary<uint, ObjectType> objectTypes = m_objectTypes;
            lock (objectTypes)
            {
                ObjectType type;
                return (!m_objectTypes.TryGetValue(GID, out type) ? ObjectType.Invalid : type);
            }
        }

        public static MyRenderProfiler GetRenderProfiler() => 
            m_render.GetRenderProfiler();

        public static string GetStatistics() => 
            m_render.GetStatistics();

        public static VideoState GetVideoState(uint id) => 
            m_render.GetVideoState(id);

        public static void HandleFocusMessage(MyWindowFocusMessage msg)
        {
            m_render.HandleFocusMessage(msg);
        }

        public static void Initialize(IMyRender render)
        {
            for (int i = 0; i < 11; i++)
            {
                m_trackObjectType[i] = true;
            }
            m_trackObjectType[5] = false;
            m_render = render;
            UpdateDebugOverrides();
            ProfilerShort.SetProfiler(render.GetRenderProfiler());
        }

        public static bool IsValidGeneratedTextureName(string name) => 
            (!string.IsNullOrEmpty(name) ? (name.IndexOfAny(m_invalidGeneratedTextureChars) < 0) : false);

        public static bool IsVideoValid(uint id) => 
            m_render.IsVideoValid(id);

        public static void LoadContent(MyRenderQualityEnum quality)
        {
            m_render.LoadContent(quality);
        }

        public static uint PlayVideo(string videoFile, float volume)
        {
            uint messageId = GetMessageId(ObjectType.Video, true);
            MyRenderMessagePlayVideo message = MessagePool.Get<MyRenderMessagePlayVideo>(MyRenderMessageEnum.PlayVideo);
            message.ID = messageId;
            message.VideoFile = videoFile;
            message.Volume = volume;
            EnqueueMessage(message);
            return messageId;
        }

        public static void PreloadMaterials(string name)
        {
            MyRenderMessagePreloadMaterials message = MessagePool.Get<MyRenderMessagePreloadMaterials>(MyRenderMessageEnum.PreloadMaterials);
            message.Name = name;
            EnqueueMessage(message);
        }

        public static void PreloadModel(string name, float rescale = 1f, bool forceOldPipeline = false)
        {
            MyRenderMessagePreloadModel message = MessagePool.Get<MyRenderMessagePreloadModel>(MyRenderMessageEnum.PreloadModel);
            message.Name = name;
            message.Rescale = rescale;
            message.ForceOldPipeline = forceOldPipeline;
            EnqueueMessage(message);
        }

        public static void PreloadModels(List<string> models, bool forInstancedComponent)
        {
            MyRenderMessagePreloadModels message = MessagePool.Get<MyRenderMessagePreloadModels>(MyRenderMessageEnum.PreloadModels);
            message.Models = models;
            message.ForInstancedComponent = forInstancedComponent;
            EnqueueMessage(message);
        }

        public static void PreloadTextures(List<string> texturesToLoad, TextureType textureType)
        {
            for (int i = 0; i < texturesToLoad.Count; i++)
            {
                string str = texturesToLoad[i];
                if (str.Contains(MyFileSystem.ContentPath))
                {
                    char[] trimChars = new char[] { Path.DirectorySeparatorChar };
                    texturesToLoad[i] = str.Remove(0, MyFileSystem.ContentPath.Length).TrimStart(trimChars);
                }
            }
            MyRenderMessagePreloadTextures message = MessagePool.Get<MyRenderMessagePreloadTextures>(MyRenderMessageEnum.PreloadTextures);
            message.TextureType = textureType;
            message.Files = new List<string>(texturesToLoad);
            EnqueueMessage(message);
        }

        public static void PreloadVoxelMaterials(byte[] materials)
        {
            MyRenderMessagePreloadVoxelMaterials message = MessagePool.Get<MyRenderMessagePreloadVoxelMaterials>(MyRenderMessageEnum.PreloadVoxelMaterials);
            message.Materials = materials;
            EnqueueMessage(message);
        }

        public static MyRenderMessageAddRuntimeModel PrepareAddRuntimeModel()
        {
            MyRenderMessageAddRuntimeModel local1 = MessagePool.Get<MyRenderMessageAddRuntimeModel>(MyRenderMessageEnum.AddRuntimeModel);
            local1.ModelData.Clear();
            return local1;
        }

        public static MyRenderMessageDebugDrawTriangles PrepareDebugDrawTriangles()
        {
            MyRenderMessageDebugDrawTriangles local1 = MessagePool.Get<MyRenderMessageDebugDrawTriangles>(MyRenderMessageEnum.DebugDrawTriangles);
            local1.Color = Color.White;
            local1.Indices.Clear();
            local1.Vertices.Clear();
            return local1;
        }

        public static MyRenderMessageSetRenderEntityData PrepareSetRenderEntityData()
        {
            MyRenderMessageSetRenderEntityData local1 = MessagePool.Get<MyRenderMessageSetRenderEntityData>(MyRenderMessageEnum.SetRenderEntityData);
            local1.ModelData.Clear();
            return local1;
        }

        public static void Present()
        {
            m_render.Present();
        }

        public static void PrintAllFileTexturesIntoLog()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageDebugPrintAllFileTexturesIntoLog>(MyRenderMessageEnum.DebugPrintAllFileTexturesIntoLog));
        }

        public static void ProcessMessages()
        {
            m_render.Draw(false);
        }

        public static void RebuildCullingStructure()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageRebuildCullingStructure>(MyRenderMessageEnum.RebuildCullingStructure));
        }

        public static void RegisterDecals(Dictionary<string, List<MyDecalMaterialDesc>> descriptions)
        {
            MyRenderMessageRegisterScreenDecalsMaterials message = MessagePool.Get<MyRenderMessageRegisterScreenDecalsMaterials>(MyRenderMessageEnum.RegisterDecalsMaterials);
            message.MaterialDescriptions = descriptions;
            EnqueueMessage(message);
        }

        public static void ReloadContent(MyRenderQualityEnum quality)
        {
            m_render.ReloadContent(quality);
        }

        public static void ReloadEffects()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageReloadEffects>(MyRenderMessageEnum.ReloadEffects));
        }

        public static void ReloadModels()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageReloadModels>(MyRenderMessageEnum.ReloadModels));
        }

        public static void ReloadTextures()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageReloadTextures>(MyRenderMessageEnum.ReloadTextures));
        }

        public static void RemoveBillboardViewProjection(int id)
        {
            BillboardsViewProjectionWrite.Remove(id);
        }

        public static void RemoveDecal(uint decalId)
        {
            MyRenderMessageRemoveDecal message = MessagePool.Get<MyRenderMessageRemoveDecal>(MyRenderMessageEnum.RemoveDecal);
            message.ID = decalId;
            EnqueueMessage(message);
        }

        public static void RemoveGPUEmitter(uint GID, bool instant)
        {
            MyRenderMessageRemoveGPUEmitter message = MessagePool.Get<MyRenderMessageRemoveGPUEmitter>(MyRenderMessageEnum.RemoveGPUEmitter);
            message.GID = GID;
            message.Instant = instant;
            EnqueueMessage(message);
            RemoveMessageId(message.GID, ObjectType.GPUEmitter, false);
        }

        public static void RemoveMessageId(uint GID, ObjectType type, bool now = true)
        {
            if (m_trackObjectType[(int) type])
            {
                Dictionary<uint, ObjectType> objectTypes = m_objectTypes;
                lock (objectTypes)
                {
                    ObjectType objectType = GetObjectType(GID);
                    if ((objectType == ObjectType.Invalid) || (((ObjectType) m_objectTypes[GID]) != type))
                    {
                        object[] args = new object[] { type.ToString(), objectType.ToString(), Environment.StackTrace };
                        MyLog.Default.Error("Invalid object type Expected:{0} Real:{1} \n{2}", args);
                    }
                    else if (!now)
                    {
                        m_objectsToRemove.Add(GID);
                    }
                    else
                    {
                        m_objectTypes.Remove(GID);
                        m_objectsToRemove.Remove(GID);
                    }
                }
            }
        }

        public static void RemovePersistentBillboard(MyBillboard billboard)
        {
            m_render.SharedData.RemovePersistentBillboard(billboard);
        }

        public static void RemoveRenderComponent<TComponent>(uint id)
        {
            MyRenderMessageUpdateComponent message = MessagePool.Get<MyRenderMessageUpdateComponent>(MyRenderMessageEnum.UpdateRenderComponent);
            message.ID = id;
            message.Type = MyRenderMessageUpdateComponent.UpdateType.Delete;
            message.Initialize<DeleteComponentData>().SetComponent<TComponent>();
            EnqueueMessage(message);
        }

        public static void RemoveRenderObject(uint id, ObjectType objectType, bool fadeOut = false)
        {
            if (objectType == ObjectType.Invalid)
            {
                objectType = GetObjectType(id);
            }
            RemoveMessageId(id, objectType, false);
            MyRenderMessageRemoveRenderObject message = MessagePool.Get<MyRenderMessageRemoveRenderObject>(MyRenderMessageEnum.RemoveRenderObject);
            message.ID = id;
            message.FadeOut = fadeOut;
            EnqueueMessage(message);
        }

        public static void RenderColoredTextures(List<renderColoredTextureProperties> texturesToRender)
        {
            MyRenderMessageRenderColoredTexture message = MessagePool.Get<MyRenderMessageRenderColoredTexture>(MyRenderMessageEnum.RenderColoredTexture);
            message.texturesToRender = texturesToRender;
            EnqueueMessage(message);
        }

        public static string RendererInterfaceName() => 
            m_render.ToString();

        public static void RenderOffscreenTexture(string offscreenTexture, Vector2? aspectRatio = new Vector2?(), Color? backgroundColor = new Color?())
        {
            CheckValidGeneratedTextureName(offscreenTexture);
            MyRenderMessageRenderOffscreenTexture message = MessagePool.Get<MyRenderMessageRenderOffscreenTexture>(MyRenderMessageEnum.RenderOffscreenTexture);
            message.OffscreenTexture = offscreenTexture;
            message.BackgroundColor = backgroundColor;
            Vector2? nullable = aspectRatio;
            message.AspectRatio = (nullable != null) ? nullable.GetValueOrDefault() : Vector2.One;
            EnqueueMessage(message);
        }

        public static void RenderProfilerInput(RenderProfilerCommand command, int index, string value)
        {
            MyRenderMessageRenderProfiler message = MessagePool.Get<MyRenderMessageRenderProfiler>(MyRenderMessageEnum.RenderProfiler);
            message.Command = command;
            message.Index = index;
            message.Value = value;
            EnqueueMessage(message);
        }

        public static uint RenderVoxelCreate(string debugName, MatrixD worldMatrix, IMyLodController clipmap, RenderFlags flags = 0x10, float dithering = 0f)
        {
            if (clipmap == null)
            {
                throw new ArgumentNullException("clipmap");
            }
            MyRenderMessageVoxelCreate message = MessagePool.Get<MyRenderMessageVoxelCreate>(MyRenderMessageEnum.VoxelCreate);
            message.Id = GetMessageId(ObjectType.Entity, true);
            message.DebugName = debugName;
            message.WorldMatrix = worldMatrix;
            message.Clipmap = clipmap;
            message.Size = clipmap.Size;
            message.SpherizeRadius = clipmap.SpherizeRadius;
            message.SpherizePosition = clipmap.SpherizePosition;
            message.RenderFlags = flags;
            message.Dithering = dithering;
            EnqueueMessage(message);
            return message.Id;
        }

        public static void RequestVideoAdapters()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageVideoAdaptersRequest>(MyRenderMessageEnum.VideoAdaptersRequest));
        }

        public static bool ResetDevice() => 
            m_render.ResetDevice();

        public static void ResetEnvironmentProbes()
        {
            m_render.ResetEnvironmentProbes();
        }

        public static void ResetGeneratedTexture(string textureName, byte[] data)
        {
            MyRenderMessageResetGeneratedTexture message = MessagePool.Get<MyRenderMessageResetGeneratedTexture>(MyRenderMessageEnum.ResetGeneratedTexture);
            message.TextureName = textureName;
            message.Data = data;
            EnqueueMessage(message);
        }

        public static void ResetRandomness(int? seed = new int?())
        {
            MyRenderMessageResetRandomness message = MessagePool.Get<MyRenderMessageResetRandomness>(MyRenderMessageEnum.ResetRandomness);
            message.Seed = seed;
            EnqueueMessage(message);
        }

        public static void ScreenshotTaken(bool success, string filename, bool showNotification)
        {
            MyRenderMessageScreenshotTaken message = MessagePool.Get<MyRenderMessageScreenshotTaken>(MyRenderMessageEnum.ScreenshotTaken);
            message.Success = success;
            message.Filename = filename;
            message.ShowNotification = showNotification;
            EnqueueOutputMessage(message);
        }

        public static void SendClipmapsReady()
        {
            EnqueueOutputMessage(MessagePool.Get<MyRenderMessageClipmapsReady>(MyRenderMessageEnum.ClipmapsReady));
        }

        public static void SendCreatedDeviceSettings(MyRenderDeviceSettings settings)
        {
            MyRenderMessageCreatedDeviceSettings message = MessagePool.Get<MyRenderMessageCreatedDeviceSettings>(MyRenderMessageEnum.CreatedDeviceSettings);
            message.Settings = settings;
            EnqueueOutputMessage(message);
        }

        public static void SendTasksFinished()
        {
            EnqueueOutputMessage(MessagePool.Get<MyRenderMessageTasksFinished>(MyRenderMessageEnum.TasksFinished));
        }

        public static void SendVideoAdapters(MyAdapterInfo[] adapters)
        {
            MyRenderMessageVideoAdaptersResponse message = MessagePool.Get<MyRenderMessageVideoAdaptersResponse>(MyRenderMessageEnum.VideoAdaptersResponse);
            message.Adapters = adapters;
            EnqueueOutputMessage(message);
        }

        public static void SetCameraViewMatrix(MatrixD viewMatrix, Matrix projectionMatrix, Matrix projectionFarMatrix, float fov, float fovSkybox, float nearPlane, float farPlane, float farFarPlane, Vector3D cameraPosition, float projectionOffsetX = 0f, float projectionOffsetY = 0f, int lastMomentUpdateIndex = 1)
        {
            MyRenderMessageSetCameraViewMatrix message = MessagePool.Get<MyRenderMessageSetCameraViewMatrix>(MyRenderMessageEnum.SetCameraViewMatrix);
            message.ViewMatrix = viewMatrix;
            message.ProjectionMatrix = projectionMatrix;
            message.ProjectionFarMatrix = projectionFarMatrix;
            message.FOV = fov;
            message.FOVForSkybox = fovSkybox;
            message.NearPlane = nearPlane;
            message.FarPlane = farPlane;
            message.FarFarPlane = farFarPlane;
            message.CameraPosition = cameraPosition;
            message.LastMomentUpdateIndex = lastMomentUpdateIndex;
            message.ProjectionOffsetX = projectionOffsetX;
            message.ProjectionOffsetY = projectionOffsetY;
            EnqueueMessage(message);
        }

        public static void SetCharacterSkeleton(uint characterID, MySkeletonBoneDescription[] skeletonBones, int[] skeletonIndices)
        {
            MyRenderMessageSetCharacterSkeleton message = MessagePool.Get<MyRenderMessageSetCharacterSkeleton>(MyRenderMessageEnum.SetCharacterSkeleton);
            message.CharacterID = characterID;
            message.SkeletonBones = skeletonBones;
            message.SkeletonIndices = skeletonIndices;
            EnqueueMessage(message);
        }

        public static bool SetCharacterTransforms(uint characterID, Matrix[] boneTransforms, IReadOnlyList<MyBoneDecalUpdate> boneDecalUpdates)
        {
            MyRenderMessageSetCharacterTransforms message = MessagePool.Get<MyRenderMessageSetCharacterTransforms>(MyRenderMessageEnum.SetCharacterTransforms);
            message.CharacterID = characterID;
            if ((message.BoneAbsoluteTransforms == null) || (message.BoneAbsoluteTransforms.Length < boneTransforms.Length))
            {
                message.BoneAbsoluteTransforms = new Matrix[boneTransforms.Length];
            }
            Array.Copy(boneTransforms, message.BoneAbsoluteTransforms, boneTransforms.Length);
            message.BoneDecalUpdates.AddRange(boneDecalUpdates);
            EnqueueMessage(message);
            return false;
        }

        public static void SetDecalGlobals(MyDecalGlobals globals)
        {
            MyRenderMessageSetDecalGlobals message = MessagePool.Get<MyRenderMessageSetDecalGlobals>(MyRenderMessageEnum.SetDecalGlobals);
            message.Globals = globals;
            EnqueueMessage(message);
        }

        public static void SetFrameTimeStep(float timestepInSeconds = 0f)
        {
            MyRenderMessageSetFrameTimeStep message = MessagePool.Get<MyRenderMessageSetFrameTimeStep>(MyRenderMessageEnum.SetFrameTimeStep);
            message.TimeStepInSeconds = timestepInSeconds;
            EnqueueMessage(message);
        }

        public static void SetGlobalValues(string rootDirectory, string rootDirectoryEffects, string rootDirectoryDebug)
        {
            m_render.RootDirectory = rootDirectory;
            m_render.RootDirectoryEffects = rootDirectoryEffects;
            m_render.RootDirectoryDebug = rootDirectoryDebug;
        }

        public static void SetGravityProvider(Func<Vector3D, Vector3> calculateGravityInPoint)
        {
            MyRenderMessageSetGravityProvider message = MessagePool.Get<MyRenderMessageSetGravityProvider>(MyRenderMessageEnum.SetGravityProvider);
            message.CalculateGravityInPoint = calculateGravityInPoint;
            EnqueueMessage(message);
        }

        public static void SetInstanceBuffer(uint entityId, uint instanceBufferId, int instanceStart, int instanceCount, BoundingBox entityLocalAabb, MyInstanceData[] instanceData = null)
        {
            MyRenderMessageSetInstanceBuffer message = MessagePool.Get<MyRenderMessageSetInstanceBuffer>(MyRenderMessageEnum.SetInstanceBuffer);
            message.ID = entityId;
            message.InstanceBufferId = instanceBufferId;
            message.InstanceStart = instanceStart;
            message.InstanceCount = instanceCount;
            message.LocalAabb = entityLocalAabb;
            message.InstanceData = instanceData;
            EnqueueMessage(message);
        }

        public static void SetLightShadowIgnore(uint id, uint ignoreId)
        {
            MyRenderMessageSetLightShadowIgnore message = MessagePool.Get<MyRenderMessageSetLightShadowIgnore>(MyRenderMessageEnum.SetLightShadowIgnore);
            message.ID = id;
            message.ID2 = ignoreId;
            EnqueueMessage(message);
        }

        public static void SetParentCullObject(uint renderObject, uint parentCullObject, Matrix? childToParent = new Matrix?())
        {
            MyRenderMessageSetParentCullObject message = MessagePool.Get<MyRenderMessageSetParentCullObject>(MyRenderMessageEnum.SetParentCullObject);
            message.ID = renderObject;
            message.CullObjectID = parentCullObject;
            message.ChildToParent = childToParent;
            EnqueueMessage(message);
        }

        public static void SetRenderEntityData(uint renderObjectId, MyRenderMessageSetRenderEntityData message)
        {
            message.ID = renderObjectId;
            EnqueueMessage(message);
        }

        public static void SetSettingsDirty()
        {
            m_settingsDirty = true;
        }

        public static void SetTimings(MyTimeSpan cpuDraw, MyTimeSpan cpuWait)
        {
            m_render.SetTimings(cpuDraw, cpuWait);
            CPULoadSmooth = MathHelper.Smooth(CPULoad = (float) ((cpuDraw.Seconds / (cpuDraw.Seconds + cpuWait.Seconds)) * 100.0), CPULoadSmooth);
            CPUTimeSmooth = MathHelper.Smooth(((float) cpuDraw.Seconds) * 1000f, CPUTimeSmooth);
        }

        public static bool SettingsChanged(MyRenderDeviceSettings settings) => 
            m_render.SettingsChanged(settings);

        public static void SetVideoVolume(uint id, float volume)
        {
            MyRenderMessageSetVideoVolume message = MessagePool.Get<MyRenderMessageSetVideoVolume>(MyRenderMessageEnum.SetVideoVolume);
            message.ID = id;
            message.Volume = volume;
            EnqueueMessage(message);
        }

        public static void SetVisibilityUpdates(uint id, bool state)
        {
            MyRenderMessageSetVisibilityUpdates message = MessagePool.Get<MyRenderMessageSetVisibilityUpdates>(MyRenderMessageEnum.SetVisibilityUpdates);
            message.ID = id;
            message.State = state;
            EnqueueMessage(message);
        }

        public static void SpriteScissorPop(string targetTexture = null)
        {
            MyRenderMessageSpriteScissorPop message = MessagePool.Get<MyRenderMessageSpriteScissorPop>(MyRenderMessageEnum.SpriteScissorPop);
            EnqueueMessage(message);
            message.TargetTexture = targetTexture;
        }

        public static void SpriteScissorPush(Rectangle screenRectangle, string targetTexture = null)
        {
            MyRenderMessageSpriteScissorPush message = MessagePool.Get<MyRenderMessageSpriteScissorPush>(MyRenderMessageEnum.SpriteScissorPush);
            message.ScreenRectangle = screenRectangle;
            message.TargetTexture = targetTexture;
            EnqueueMessage(message);
        }

        public static void SwitchDeviceSettings(MyRenderDeviceSettings settings)
        {
            MyRenderMessageSwitchDeviceSettings message = MessagePool.Get<MyRenderMessageSwitchDeviceSettings>(MyRenderMessageEnum.SwitchDeviceSettings);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void SwitchPostprocessSettings(ref MyPostprocessSettings settings)
        {
            MyRenderMessageUpdatePostprocessSettings message = MessagePool.Get<MyRenderMessageUpdatePostprocessSettings>(MyRenderMessageEnum.UpdatePostprocessSettings);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void SwitchRenderSettings(MyRenderSettings settings)
        {
            if (m_render != null)
            {
                MyRenderMessageSwitchRenderSettings message = MessagePool.Get<MyRenderMessageSwitchRenderSettings>(MyRenderMessageEnum.SwitchRenderSettings);
                message.Settings = settings;
                m_settingsDirty = false;
                EnqueueMessage(message);
            }
        }

        public static void SwitchRenderSettings(MyRenderSettings1 settings)
        {
            Settings.User = settings;
            if (settings.GrassDensityFactor == 0f)
            {
                Settings.User.GrassDrawDistance = 0f;
            }
            SwitchRenderSettings(Settings);
        }

        public static void TakeScreenshot(Vector2 sizeMultiplier, string pathToSave, bool debug, bool ignoreSprites, bool showNotification)
        {
            if (debug && (pathToSave != null))
            {
                throw new ArgumentException("When taking debug screenshot, path to save must be null, becase debug takes a lot of screenshots");
            }
            MyRenderMessageTakeScreenshot message = MessagePool.Get<MyRenderMessageTakeScreenshot>(MyRenderMessageEnum.TakeScreenshot);
            message.IgnoreSprites = ignoreSprites;
            message.SizeMultiplier = sizeMultiplier;
            message.PathToSave = pathToSave;
            message.Debug = debug;
            message.ShowNotification = showNotification;
            EnqueueMessage(message);
        }

        public static MyRenderDeviceCooperativeLevel TestDeviceCooperativeLevel() => 
            m_render.TestDeviceCooperativeLevel();

        private static void TrackNewMessageId(ObjectType type)
        {
            if (m_trackObjectType[(int) type])
            {
                m_objectTypes.Add(m_render.GlobalMessageCounter, type);
            }
        }

        public static void UnloadContent()
        {
            m_render.UnloadContent();
            ClearLargeMessages();
        }

        public static void UnloadData()
        {
            ClearLargeMessages();
            EnqueueMessage(MessagePool.Get<MyRenderMessageUnloadData>(MyRenderMessageEnum.UnloadData));
        }

        public static void UnloadTexture(string textureName)
        {
            MyRenderMessageUnloadTexture message = MessagePool.Get<MyRenderMessageUnloadTexture>(MyRenderMessageEnum.UnloadTexture);
            message.Texture = textureName;
            EnqueueMessage(message);
        }

        public static void UpdateAtmosphereSettings(uint id, MyAtmosphereSettings settings)
        {
            MyRenderMessageUpdateAtmosphereSettings message = MessagePool.Get<MyRenderMessageUpdateAtmosphereSettings>(MyRenderMessageEnum.UpdateAtmosphereSettings);
            message.ID = id;
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void UpdateCloudLayerFogFlag(bool shouldDrawFog)
        {
            MyRenderMessageUpdateCloudLayerFogFlag message = MessagePool.Get<MyRenderMessageUpdateCloudLayerFogFlag>(MyRenderMessageEnum.UpdateCloudLayerFogFlag);
            message.ShouldDrawFog = shouldDrawFog;
            EnqueueMessage(message);
        }

        public static void UpdateColorEmissivity(uint id, int lod, string materialName, Color diffuseColor, float emissivity)
        {
            if (id != uint.MaxValue)
            {
                MyRenderMessageUpdateColorEmissivity message = MessagePool.Get<MyRenderMessageUpdateColorEmissivity>(MyRenderMessageEnum.UpdateColorEmissivity);
                message.ID = id;
                message.LOD = lod;
                message.MaterialName = materialName;
                message.DiffuseColor = diffuseColor;
                message.Emissivity = emissivity;
                EnqueueMessage(message);
            }
        }

        public static void UpdateDebugOverrides()
        {
            MyRenderMessageUpdateDebugOverrides message = MessagePool.Get<MyRenderMessageUpdateDebugOverrides>(MyRenderMessageEnum.UpdateDebugOverrides);
            message.Overrides = DebugOverrides.Clone();
            EnqueueMessage(message);
        }

        public static void UpdateDecals(List<MyDecalPositionUpdate> decals)
        {
            MyRenderMessageUpdateScreenDecal message = MessagePool.Get<MyRenderMessageUpdateScreenDecal>(MyRenderMessageEnum.UpdateScreenDecal);
            message.Decals.AddRange(decals);
            EnqueueMessage(message);
        }

        public static void UpdateEnvironmentMap()
        {
            EnqueueMessage(MessagePool.Get<MyRenderMessageUpdateEnvironmentMap>(MyRenderMessageEnum.UpdateEnvironmentMap));
        }

        public static void UpdateFogSettings(ref MyRenderFogSettings settings)
        {
            MyRenderMessageUpdateFogSettings message = MessagePool.Get<MyRenderMessageUpdateFogSettings>(MyRenderMessageEnum.UpdateFogSettings);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void UpdateGameplayFrame(int frame)
        {
            MyRenderMessageUpdateGameplayFrame message = MessagePool.Get<MyRenderMessageUpdateGameplayFrame>(MyRenderMessageEnum.UpdateGameplayFrame);
            message.GameplayFrame = frame;
            EnqueueMessage(message);
        }

        public static void UpdateGPUEmitters(ref List<MyGPUEmitter> emitters)
        {
            MyRenderMessageUpdateGPUEmitters message = MessagePool.Get<MyRenderMessageUpdateGPUEmitters>(MyRenderMessageEnum.UpdateGPUEmitters);
            MyUtils.Swap<List<MyGPUEmitter>>(ref emitters, ref message.Emitters);
            EnqueueMessage(message);
        }

        public static void UpdateGPUEmittersLite(ref List<MyGPUEmitterLite> emitters)
        {
            MyRenderMessageUpdateGPUEmittersLite message = MessagePool.Get<MyRenderMessageUpdateGPUEmittersLite>(MyRenderMessageEnum.UpdateGPUEmittersLite);
            MyUtils.Swap<List<MyGPUEmitterLite>>(ref emitters, ref message.Emitters);
            EnqueueMessage(message);
        }

        public static void UpdateGPUEmittersTransform(ref List<MyGPUEmitterTransformUpdate> emitters)
        {
            MyRenderMessageUpdateGPUEmittersTransform message = MessagePool.Get<MyRenderMessageUpdateGPUEmittersTransform>(MyRenderMessageEnum.UpdateGPUEmittersTransform);
            MyUtils.Swap<List<MyGPUEmitterTransformUpdate>>(ref emitters, ref message.Emitters);
            EnqueueMessage(message);
        }

        public static void UpdateHBAOSettings(ref MyHBAOData settings)
        {
            MyRenderMessageUpdateHBAO message = MessagePool.Get<MyRenderMessageUpdateHBAO>(MyRenderMessageEnum.UpdateHBAO);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void UpdateHighlightOverlappingModel(uint modelRenderId, bool enable = true)
        {
            MyRenderMessageUpdateOverlappingModelsForHighlight message = MessagePool.Get<MyRenderMessageUpdateOverlappingModelsForHighlight>(MyRenderMessageEnum.UpdateOverlappingModelsForHighlight);
            message.Enable = enable;
            message.OverlappingModelID = modelRenderId;
            EnqueueMessage(message);
        }

        public static void UpdateLineBasedObject(uint id, Vector3D worldPointA, Vector3D worldPointB)
        {
            MyRenderMessageUpdateLineBasedObject message = MessagePool.Get<MyRenderMessageUpdateLineBasedObject>(MyRenderMessageEnum.UpdateLineBasedObject);
            message.ID = id;
            message.WorldPointA = worldPointA;
            message.WorldPointB = worldPointB;
            EnqueueMessage(message);
        }

        public static void UpdateLodImmediately(uint id)
        {
            MyRenderMessageUpdateLodImmediately message = MessagePool.Get<MyRenderMessageUpdateLodImmediately>(MyRenderMessageEnum.UpdateLodImmediately);
            message.Id = id;
            EnqueueMessage(message);
        }

        public static void UpdateModelHighlight(uint id, string[] sectionNames, uint[] subpartIndices, Color? outlineColor, float thickness = -1f, float pulseTimeInSeconds = 0f, int instanceIndex = -1)
        {
            MyRenderMessageUpdateModelHighlight message = MessagePool.Get<MyRenderMessageUpdateModelHighlight>(MyRenderMessageEnum.UpdateModelHighlight);
            message.ID = id;
            message.SectionNames = sectionNames;
            message.SubpartIndices = subpartIndices;
            message.OutlineColor = outlineColor;
            message.Thickness = thickness;
            message.PulseTimeInSeconds = pulseTimeInSeconds;
            message.InstanceIndex = instanceIndex;
            EnqueueMessage(message);
        }

        public static void UpdateModelProperties(uint id, string materialName, RenderFlags addFlags, RenderFlags removeFlags, Color? diffuseColor, float? emissivity)
        {
            MyRenderMessageUpdateModelProperties message = MessagePool.Get<MyRenderMessageUpdateModelProperties>(MyRenderMessageEnum.UpdateModelProperties);
            message.ID = id;
            message.MaterialName = materialName;
            if ((addFlags == 0) && (removeFlags == 0))
            {
                message.FlagsChange = null;
            }
            else
            {
                RenderFlagsChange change = new RenderFlagsChange {
                    Add = addFlags,
                    Remove = removeFlags
                };
                message.FlagsChange = new RenderFlagsChange?(change);
            }
            message.DiffuseColor = diffuseColor;
            message.Emissivity = emissivity;
            EnqueueMessage(message);
        }

        public static void UpdateMouseCapture(bool capture)
        {
            MyRenderMessageSetMouseCapture message = MessagePool.Get<MyRenderMessageSetMouseCapture>(MyRenderMessageEnum.SetMouseCapture);
            message.Capture = capture;
            EnqueueMessage(message);
        }

        public static void UpdateNewLoddingSettings(MyNewLoddingSettings settings)
        {
            MyRenderMessageUpdateNewLoddingSettings message = MessagePool.Get<MyRenderMessageUpdateNewLoddingSettings>(MyRenderMessageEnum.UpdateNewLoddingSettings);
            message.Settings.CopyFrom(settings);
            EnqueueMessage(message);
        }

        public static void UpdatePlanetSettings(ref MyRenderPlanetSettings settings)
        {
            MyRenderMessageUpdatePlanetSettings message = MessagePool.Get<MyRenderMessageUpdatePlanetSettings>(MyRenderMessageEnum.UpdatePlanetSettings);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void UpdateRenderComponent<TData, TContext>(uint id, TContext context, Action<TData, TContext> activator) where TData: UpdateData
        {
            MyRenderMessageUpdateComponent message = MessagePool.Get<MyRenderMessageUpdateComponent>(MyRenderMessageEnum.UpdateRenderComponent);
            message.ID = id;
            message.Type = MyRenderMessageUpdateComponent.UpdateType.Update;
            activator(message.Initialize<TData>(), context);
            EnqueueMessage(message);
        }

        public static void UpdateRenderCubeInstanceBuffer(uint id, ref List<MyCubeInstanceData> instanceData, int capacity, ref List<MyCubeInstanceDecalData> decalData)
        {
            MyRenderMessageUpdateRenderCubeInstanceBuffer message = MessagePool.Get<MyRenderMessageUpdateRenderCubeInstanceBuffer>(MyRenderMessageEnum.UpdateRenderCubeInstanceBuffer);
            message.ID = id;
            message.Capacity = capacity;
            MyUtils.Swap<List<MyCubeInstanceDecalData>>(ref message.DecalsData, ref decalData);
            MyUtils.Swap<List<MyCubeInstanceData>>(ref message.InstanceData, ref instanceData);
            EnqueueMessage(message);
        }

        public static void UpdateRenderEntity(uint id, Color? diffuseColor, Vector3? colorMaskHsv, float? dithering = new float?(), bool fadeIn = false)
        {
            bool flag1 = dithering != null;
            MyRenderMessageUpdateRenderEntity message = MessagePool.Get<MyRenderMessageUpdateRenderEntity>(MyRenderMessageEnum.UpdateRenderEntity);
            message.ID = id;
            message.DiffuseColor = diffuseColor;
            message.ColorMaskHSV = colorMaskHsv;
            message.Dithering = dithering;
            message.FadeIn = fadeIn;
            EnqueueMessage(message);
        }

        public static void UpdateRenderEnvironment(ref MyEnvironmentData data, bool resetEyeAdaptation)
        {
            MyRenderMessageUpdateRenderEnvironment message = MessagePool.Get<MyRenderMessageUpdateRenderEnvironment>(MyRenderMessageEnum.UpdateRenderEnvironment);
            message.Data = data;
            message.ResetEyeAdaptation = resetEyeAdaptation;
            EnqueueMessage(message);
        }

        public static void UpdateRenderInstanceBufferRange(uint id, MyInstanceData[] instanceData, int offset = 0, bool trimEnd = false)
        {
            MyRenderMessageUpdateRenderInstanceBufferRange message = MessagePool.Get<MyRenderMessageUpdateRenderInstanceBufferRange>(MyRenderMessageEnum.UpdateRenderInstanceBufferRange);
            message.ID = id;
            message.InstanceData = instanceData;
            message.StartOffset = offset;
            message.Trim = trimEnd;
            EnqueueMessage(message);
        }

        public static void UpdateRenderLight(ref UpdateRenderLightData data)
        {
            MyRenderMessageUpdateRenderLight message = MessagePool.Get<MyRenderMessageUpdateRenderLight>(MyRenderMessageEnum.UpdateRenderLight);
            message.Data = data;
            EnqueueMessage(message);
        }

        public static void UpdateRenderObject(uint id, MatrixD? worldMatrix, BoundingBox? aabb = new BoundingBox?(), int lastMomentUpdateIndex = -1, Matrix? localMatrix = new Matrix?())
        {
            MyRenderMessageUpdateRenderObject message = MessagePool.Get<MyRenderMessageUpdateRenderObject>(MyRenderMessageEnum.UpdateRenderObject);
            message.ID = id;
            message.Data.WorldMatrix = worldMatrix;
            message.Data.LocalAABB = aabb;
            message.LastMomentUpdateIndex = lastMomentUpdateIndex;
            message.Data.LocalMatrix = localMatrix;
            EnqueueMessage(message);
        }

        public static void UpdateRenderObjectVisibility(uint id, bool visible, bool near)
        {
            MyRenderMessageUpdateRenderObjectVisibility message = MessagePool.Get<MyRenderMessageUpdateRenderObjectVisibility>(MyRenderMessageEnum.UpdateRenderObjectVisibility);
            message.ID = id;
            message.Visible = visible;
            message.NearFlag = near;
            EnqueueMessage(message);
        }

        public static void UpdateRenderVoxelMaterials(MyRenderVoxelMaterialData[] materials)
        {
            MyRenderMessageUpdateRenderVoxelMaterials message = MessagePool.Get<MyRenderMessageUpdateRenderVoxelMaterials>(MyRenderMessageEnum.UpdateRenderVoxelMaterials);
            message.Materials = materials;
            EnqueueMessage(message);
        }

        public static void UpdateShadowsSettings(MyShadowsSettings settings)
        {
            MyRenderMessageUpdateShadowSettings message = MessagePool.Get<MyRenderMessageUpdateShadowSettings>(MyRenderMessageEnum.UpdateShadowSettings);
            message.Settings.CopyFrom(settings);
            EnqueueMessage(message);
        }

        public static void UpdateSSAOSettings(ref MySSAOSettings settings)
        {
            MyRenderMessageUpdateSSAOSettings message = MessagePool.Get<MyRenderMessageUpdateSSAOSettings>(MyRenderMessageEnum.UpdateSSAOSettings);
            message.Settings = settings;
            EnqueueMessage(message);
        }

        public static void UpdateVideo(uint id)
        {
            MyRenderMessageUpdateVideo message = MessagePool.Get<MyRenderMessageUpdateVideo>(MyRenderMessageEnum.UpdateVideo);
            message.ID = id;
            EnqueueMessage(message);
        }

        public static MyRenderThread RenderThread
        {
            [CompilerGenerated]
            get => 
                <RenderThread>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<RenderThread>k__BackingField = value);
        }

        public static List<MyBillboard> BillboardsRead =>
            m_render.SharedData.Billboards.Read.Billboards;

        public static List<MyBillboard> BillboardsWrite =>
            m_render.SharedData.Billboards.Write.Billboards;

        public static Dictionary<int, MyBillboardViewProjection> BillboardsViewProjectionRead =>
            m_render.SharedData.Billboards.Read.Matrices;

        public static Dictionary<int, MyBillboardViewProjection> BillboardsViewProjectionWrite =>
            m_render.SharedData.Billboards.Write.Matrices;

        public static MyObjectsPool<MyBillboard> BillboardsPoolRead =>
            m_render.SharedData.Billboards.Read.Pool;

        public static MyObjectsPool<MyBillboard> BillboardsPoolWrite =>
            m_render.SharedData.Billboards.Write.Pool;

        public static MyObjectsPool<MyTriangleBillboard> TriangleBillboardsPoolRead =>
            m_render.SharedData.TriangleBillboards.Read.Pool;

        public static MyObjectsPool<MyTriangleBillboard> TriangleBillboardsPoolWrite =>
            m_render.SharedData.TriangleBillboards.Write.Pool;

        public static HashSet<uint> VisibleObjectsRead =>
            m_render.SharedData?.VisibleObjects.Read;

        public static HashSet<uint> VisibleObjectsWrite =>
            m_render.SharedData?.VisibleObjects.Write;

        public static MyTimeSpan CurrentDrawTime
        {
            get => 
                m_render.CurrentDrawTime;
            set => 
                (m_render.CurrentDrawTime = value);
        }

        public static MyViewport MainViewport =>
            m_render.MainViewport;

        public static Vector2I BackBufferResolution =>
            m_render.BackBufferResolution;

        public static MyLog Log =>
            m_render.Log;

        public static bool IsInstantiated =>
            (m_render != null);

        public static bool SettingsDirty =>
            m_settingsDirty;

        public static MyMessageQueue OutputQueue =>
            m_render.OutputQueue;

        public static FrameProcessStatusEnum FrameProcessStatus =>
            m_render.FrameProcessStatus;

        public static int PersistentBillboardsCount =>
            m_render.SharedData.PersistentBillboardsCount;

        public enum MyStatsState
        {
            NoDraw,
            Last,
            SimpleTimingStats,
            ComplexTimingStats,
            Draw,
            MoveNext
        }

        public enum ObjectType
        {
            Entity,
            InstanceBuffer,
            Light,
            Video,
            DebugDrawMesh,
            ScreenDecal,
            Cloud,
            Atmosphere,
            GPUEmitter,
            ManualCull,
            Invalid,
            Max
        }
    }
}

