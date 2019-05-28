namespace VRageRender
{
    using System;

    public static class MyRenderConstants
    {
        public const int RENDER_STEP_IN_MILLISECONDS = 0x10;
        public static readonly MyRenderQualityProfile[] m_renderQualityProfiles = new MyRenderQualityProfile[Enum.GetValues(typeof(MyRenderQualityEnum)).Length];

        static MyRenderConstants()
        {
            float[] numArray = new float[] { 66f, 200f, 550f, 1000f, 1700f, 3000f, 6000f, 15000f, 40000f, 100000f, 250000f };
            float[] numArray2 = new float[] { 60f, 180f, 500f, 900f, 1600f, 2800f, 5500f, 14000f, 35000f, 90000f, 220000f };
            float[] numArray3 = new float[] { 55f, 150f, 450f, 800f, 1500f, 2600f, 4000f, 13000f, 30000f, 80000f, 200000f };
            MyRenderQualityProfile profile = new MyRenderQualityProfile();
            profile.LodClipmapRanges = new float[][] { new float[] { 100f, 300f, 800f, 2000f, 4500f, 13500f, 30000f, 100000f }, numArray2 };
            profile.ExplosionDebrisCountMultiplier = 0.5f;
            m_renderQualityProfiles[1] = profile;
            profile = new MyRenderQualityProfile();
            profile.LodClipmapRanges = new float[][] { new float[] { 80f, 240f, 600f, 1600f, 4800f, 14000f, 35000f, 100000f }, numArray3 };
            profile.ExplosionDebrisCountMultiplier = 0f;
            m_renderQualityProfiles[0] = profile;
            profile = new MyRenderQualityProfile();
            profile.LodClipmapRanges = new float[][] { new float[] { 120f, 360f, 900f, 2000f, 4500f, 13500f, 30000f, 100000f }, numArray };
            profile.ExplosionDebrisCountMultiplier = 0.8f;
            m_renderQualityProfiles[2] = profile;
            profile = new MyRenderQualityProfile();
            profile.LodClipmapRanges = new float[][] { new float[] { 140f, 400f, 1000f, 2000f, 4500f, 13500f, 30000f, 100000f }, numArray };
            profile.ExplosionDebrisCountMultiplier = 3f;
            m_renderQualityProfiles[3] = profile;
        }

        public static MyRenderQualityProfile RenderQualityProfile =>
            m_renderQualityProfiles[(int) MyRenderProxy.Settings.User.VoxelQuality];
    }
}

