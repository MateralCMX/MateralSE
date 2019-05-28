namespace VRage.Render.Scene
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    public static class MyViewIds
    {
        public const int MAX_MAIN_VIEWS = 1;
        public const int MAX_SHADOW_CASCADES = 8;
        public const int MAX_SHADOW_PROJECTIONS = 4;
        public const int MAX_FORWARD_VIEWS = 6;
        public const int MAX_VIEW_COUNT = 0x13;
        public const int MAIN_VIEW_ID = 0;

        static MyViewIds()
        {
            ViewNames = new string[0x13];
            for (int i = 0; i < 0x13; i++)
            {
                string str = "Undefined";
                if (IsMainId(i))
                {
                    str = "GBuffer";
                }
                else if (IsShadowCascadeId(i))
                {
                    str = "CascadeDepth" + (i - GetShadowCascadeId(0));
                }
                else if (IsShadowProjectionId(i))
                {
                    str = "SingleDepth" + (i - GetShadowProjectionId(0));
                }
                else if (IsForwardId(i))
                {
                    str = "Forward" + (i - GetForwardId(0));
                }
                else
                {
                    MyRenderProxy.Error("Unknown view id", 0, false);
                }
                ViewNames[i] = str;
            }
        }

        public static int GetForwardId(int i) => 
            (((i + 1) + 8) + 4);

        public static int GetForwardIndex(int id) => 
            (((id - 1) - 8) - 4);

        public static int GetId(MyViewType viewType, int viewIndex)
        {
            switch (viewType)
            {
                case MyViewType.Main:
                    return GetMainId(viewIndex);

                case MyViewType.ShadowCascade:
                    return GetShadowCascadeId(viewIndex);

                case MyViewType.ShadowProjection:
                    return GetShadowProjectionId(viewIndex);

                case MyViewType.EnvironmentProbe:
                    return GetForwardId(viewIndex);
            }
            return -1;
        }

        public static int GetMainId(int i) => 
            i;

        public static int GetMainIndex(int id) => 
            id;

        public static int GetShadowCascadeId(int i) => 
            (i + 1);

        public static int GetShadowCascadeIndex(int id) => 
            (id - 1);

        public static int GetShadowProjectionId(int i) => 
            ((i + 1) + 8);

        public static int GetShadowProjectionIndex(int id) => 
            ((id - 1) - 8);

        public static int GetViewCount(MyViewType viewType)
        {
            switch (viewType)
            {
                case MyViewType.Main:
                    return 1;

                case MyViewType.ShadowCascade:
                    return 8;

                case MyViewType.ShadowProjection:
                    return 4;

                case MyViewType.EnvironmentProbe:
                    return 6;
            }
            return -1;
        }

        public static bool IsForwardId(int viewId) => 
            ((viewId >= GetForwardId(0)) && (viewId <= GetForwardId(5)));

        public static bool IsMainId(int viewId) => 
            (viewId == 0);

        public static bool IsShadowCascadeId(int viewId) => 
            ((viewId >= GetShadowCascadeId(0)) && (viewId <= GetShadowCascadeId(7)));

        public static bool IsShadowId(int viewId) => 
            ((viewId >= 1) && (viewId < GetForwardId(0)));

        public static bool IsShadowProjectionId(int viewId) => 
            ((viewId >= GetShadowProjectionId(0)) && (viewId <= GetShadowProjectionId(3)));

        public static string[] ViewNames
        {
            [CompilerGenerated]
            get => 
                <ViewNames>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<ViewNames>k__BackingField = value);
        }
    }
}

