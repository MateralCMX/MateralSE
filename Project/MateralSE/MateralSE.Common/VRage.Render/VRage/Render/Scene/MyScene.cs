namespace VRage.Render.Scene
{
    using System;
    using VRage.Render.Scene.Components;
    using VRageMath;

    public abstract class MyScene
    {
        public static long FrameCounter;
        public readonly IMyDebugDraw DebugDraw;
        public readonly IMyActorFactory ActorFactory;
        public readonly IMyBillboardsHelper BillboardsHelper;
        public readonly IMyComponentFactory ComponentFactory;
        public MyEnvironment Environment;
        public readonly MyActorUpdater Updater = new MyActorUpdater();
        public readonly MyDynamicAABBTreeD ManualCullTree;
        public readonly MyDynamicAABBTreeD MergeGroupsDBVH;
        public readonly MyDynamicAABBTreeD DynamicRenderablesDBVH;
        public readonly MyDynamicAABBTreeD DynamicRenderablesFarDBVH;

        protected MyScene(IMyDebugDraw debugDraw, IMyComponentFactory componentFactory, IMyActorFactory actorFactory, IMyBillboardsHelper billboardsHelper, MyDynamicAABBTreeD manualCullTree, MyDynamicAABBTreeD mergeGroupsDBVH, MyDynamicAABBTreeD dynamicRenderablesDBVH, MyDynamicAABBTreeD dynamicRenderablesFarDBVH)
        {
            this.DebugDraw = debugDraw;
            this.ActorFactory = actorFactory;
            this.ManualCullTree = manualCullTree;
            this.BillboardsHelper = billboardsHelper;
            this.MergeGroupsDBVH = mergeGroupsDBVH;
            this.ComponentFactory = componentFactory;
            this.DynamicRenderablesDBVH = dynamicRenderablesDBVH;
            this.DynamicRenderablesFarDBVH = dynamicRenderablesFarDBVH;
        }

        public abstract MyManualCullTreeData AllocateGroupData();
        public virtual void Clear()
        {
            this.DynamicRenderablesDBVH.Clear();
            this.DynamicRenderablesFarDBVH.Clear();
            this.MergeGroupsDBVH.Clear();
        }

        public abstract MyChildCullTreeData CompileCullData(MyChildCullTreeData data);
        public abstract void FreeGroupData(MyManualCullTreeData data);
        public abstract void SetActorParent(MyActor parent, MyActor child, Matrix? childToParent);

        public abstract float FadeOutTime { get; }
    }
}

