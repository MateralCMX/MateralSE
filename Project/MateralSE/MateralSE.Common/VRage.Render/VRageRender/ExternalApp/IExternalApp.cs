namespace VRageRender.ExternalApp
{
    using System;

    public interface IExternalApp
    {
        void Draw();
        void Update();
        void UpdateMainThread();
    }
}

