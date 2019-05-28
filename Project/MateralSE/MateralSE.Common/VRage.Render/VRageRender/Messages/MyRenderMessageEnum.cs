﻿namespace VRageRender.Messages
{
    using System;

    public enum MyRenderMessageEnum
    {
        DrawSprite,
        DrawSpriteNormalized,
        DrawSpriteAtlas,
        UnloadTexture,
        RenderProfiler,
        CreateRenderEntity,
        CreateRenderEntityAtmosphere,
        CreateRenderEntityClouds,
        AddRuntimeModel,
        PreloadModel,
        PreloadMaterials,
        SetRenderEntityData,
        CreateRenderInstanceBuffer,
        UpdateRenderInstanceBufferRange,
        UpdateRenderCubeInstanceBuffer,
        SetInstanceBuffer,
        CreateStaticGroup,
        CreateManualCullObject,
        SetParentCullObject,
        SetCameraViewMatrix,
        DrawScene,
        UpdateRenderObject,
        UpdateRenderComponent,
        UpdateRenderObjectVisibility,
        UpdateRenderEntity,
        RemoveRenderObject,
        UpdateModelProperties,
        UpdateModelHighlight,
        UpdateOverlappingModelsForHighlight,
        UpdateColorEmissivity,
        ChangeModel,
        CreateGeneratedTexture,
        ResetGeneratedTexture,
        ChangeMaterialTexture,
        RenderOffscreenTexture,
        UpdateGameplayFrame,
        InvalidateClipmapRange,
        ClipmapsReady,
        VoxelCreate,
        CreateRenderVoxelMaterials,
        UpdateRenderVoxelMaterials,
        PreloadVoxelMaterials,
        CreateRenderVoxelDebris,
        RebuildCullingStructure,
        CreateGPUEmitter,
        UpdateGPUEmitters,
        UpdateGPUEmittersLite,
        UpdateGPUEmittersTransform,
        RemoveGPUEmitter,
        CreateRenderLight,
        UpdateRenderLight,
        SetLightShadowIgnore,
        ClearLightShadowIgnore,
        UpdateShadowSettings,
        UpdateNewLoddingSettings,
        UpdateNewPipelineSettings,
        UpdateMaterialsSettings,
        ReloadEffects,
        ReloadModels,
        ReloadTextures,
        UpdatePostprocessSettings,
        UpdateRenderEnvironment,
        UpdateSSAOSettings,
        UpdateHBAO,
        UpdateFogSettings,
        UpdateEnvironmentMap,
        UpdateAtmosphereSettings,
        EnableAtmosphere,
        UpdateCloudLayerFogFlag,
        PlayVideo,
        UpdateVideo,
        DrawVideo,
        CloseVideo,
        SetVideoVolume,
        CreateScreenDecal,
        UpdateScreenDecal,
        RemoveDecal,
        SetDecalGlobals,
        RegisterDecalsMaterials,
        ClearDecals,
        TakeScreenshot,
        ScreenshotTaken,
        ExportToObjComplete,
        Error,
        CreateRenderCharacter,
        SetCharacterSkeleton,
        SetCharacterTransforms,
        DebugDrawLine3D,
        DebugDrawLine2D,
        DebugDrawLine3DBatch,
        DebugDrawPoint,
        DebugDrawSphere,
        DebugDrawAABB,
        DebugDrawAxis,
        DebugDrawOBB,
        DebugDrawFrustrum,
        DebugDrawTriangle,
        DebugDrawCapsule,
        DebugDrawText2D,
        DebugDrawText3D,
        DebugDrawModel,
        DebugDrawTriangles,
        DebugCrashRenderThread,
        DebugDrawPlane,
        DebugDrawCylinder,
        DebugDrawCone,
        DebugDrawMesh,
        DebugDraw6FaceConvex,
        DebugWaitForPresent,
        DebugClearPersistentMessages,
        DebugPrintAllFileTexturesIntoLog,
        UpdateDebugOverrides,
        UnloadData,
        CreateFont,
        DrawString,
        DrawStringAligned,
        PreloadTextures,
        AddToParticleTextureArray,
        SetFrameTimeStep,
        ResetRandomness,
        CollectGarbage,
        SpriteScissorPush,
        SpriteScissorPop,
        RenderColoredTexture,
        CreateLineBasedObject,
        UpdateLineBasedObject,
        VideoAdaptersRequest,
        VideoAdaptersResponse,
        CreatedDeviceSettings,
        SwitchDeviceSettings,
        SwitchRenderSettings,
        SetMouseCapture,
        SetVisibilityUpdates,
        UpdatePlanetSettings,
        MainThreadCallback,
        UpdateLodImmediately,
        TasksFinished,
        PreloadModels,
        SetGravityProvider
    }
}

