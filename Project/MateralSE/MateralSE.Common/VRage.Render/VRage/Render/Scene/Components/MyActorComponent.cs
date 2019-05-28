namespace VRage.Render.Scene.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Render.Scene;
    using VRageMath;

    public class MyActorComponent
    {
        private bool m_allowsParallelUpdate;
        private bool m_needsPerFrameUpdate;
        private bool m_needsPerFrameActorUpdate;

        public virtual void Assign(MyActor owner)
        {
            this.Owner = owner;
        }

        public virtual void Construct()
        {
            this.Owner = null;
            this.AllowsParallelUpdate = false;
        }

        public virtual void Destruct()
        {
        }

        public virtual MyChildCullTreeData GetCullTreeData() => 
            null;

        public virtual void OnParentRemoved()
        {
        }

        public virtual void OnParentSet()
        {
        }

        public virtual void OnRemove(MyActor owner)
        {
            this.Destruct();
            this.NeedsPerFrameUpdate = false;
            this.NeedsPerFrameActorUpdate = false;
            this.Owner = null;
            owner.Scene.ComponentFactory.Deallocate(this);
        }

        public virtual void OnUpdateBeforeDraw()
        {
        }

        public virtual void OnVisibilityChange()
        {
        }

        public virtual bool StartFadeOut() => 
            true;

        public MyActor Owner { get; private set; }

        public virtual Color DebugColor =>
            Color.Magenta;

        public float FrameTime =>
            this.Owner.Scene.Environment.LastFrameDelta;

        public bool AllowsParallelUpdate
        {
            get => 
                this.m_allowsParallelUpdate;
            protected set
            {
                bool needsPerFrameUpdate = this.NeedsPerFrameUpdate;
                this.NeedsPerFrameUpdate = false;
                this.m_allowsParallelUpdate = value;
                this.NeedsPerFrameUpdate = needsPerFrameUpdate;
            }
        }

        public bool NeedsPerFrameUpdate
        {
            get => 
                this.m_needsPerFrameUpdate;
            protected set
            {
                if (this.m_needsPerFrameUpdate != value)
                {
                    this.m_needsPerFrameUpdate = value;
                    if (this.AllowsParallelUpdate)
                    {
                        if (value)
                        {
                            this.Owner.Scene.Updater.AddForParallelUpdate(this);
                        }
                        else
                        {
                            this.Owner.Scene.Updater.RemoveFromParallelUpdate(this);
                        }
                    }
                    else if (value)
                    {
                        this.Owner.Scene.Updater.AddToAlwaysUpdate(this);
                    }
                    else
                    {
                        this.Owner.Scene.Updater.RemoveFromAlwaysUpdate(this);
                    }
                }
            }
        }

        public bool NeedsPerFrameActorUpdate
        {
            get => 
                this.m_needsPerFrameActorUpdate;
            set
            {
                if (this.m_needsPerFrameActorUpdate != value)
                {
                    this.m_needsPerFrameActorUpdate = value;
                    this.Owner.AlwaysUpdate = value;
                }
            }
        }

        public bool IsVisible =>
            this.Owner.IsVisible;
    }
}

