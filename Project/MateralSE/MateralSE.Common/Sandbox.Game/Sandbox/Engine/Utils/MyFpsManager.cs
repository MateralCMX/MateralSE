namespace Sandbox.Engine.Utils
{
    using Sandbox.Game.Debugging;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public static class MyFpsManager
    {
        private static long m_lastTime = 0L;
        private static uint m_fpsCounter = 0;
        private static uint m_sessionTotalFrames = 0;
        private static uint m_maxSessionFPS = 0;
        private static uint m_minSessionFPS = 0x7fffffff;
        private static uint m_lastFpsDrawn = 0;
        private static long m_lastFrameTime = 0L;
        private static long m_lastFrameMin = 0x7fffffffffffffffL;
        private static long m_lastFrameMax = -9223372036854775808L;
        private static byte m_firstFrames = 0;
        private static readonly MyMovingAverage m_frameTimeAvg = new MyMovingAverage(60, 0x3e8);

        public static int GetFps() => 
            ((int) m_lastFpsDrawn);

        public static int GetMaxSessionFPS() => 
            ((int) m_maxSessionFPS);

        public static int GetMinSessionFPS() => 
            ((int) m_minSessionFPS);

        public static int GetSessionTotalFrames() => 
            ((int) m_sessionTotalFrames);

        public static void PrepareMinMax()
        {
            if (m_firstFrames <= 20)
            {
                m_minSessionFPS = m_lastFpsDrawn;
                m_maxSessionFPS = m_lastFpsDrawn;
            }
        }

        public static void Reset()
        {
            m_maxSessionFPS = 0;
            m_minSessionFPS = 0x7fffffff;
            m_fpsCounter = 0;
            m_sessionTotalFrames = 0;
            m_lastTime = MyPerformanceCounter.ElapsedTicks;
            m_firstFrames = 0;
        }

        public static void Update()
        {
            m_fpsCounter++;
            m_sessionTotalFrames++;
            if (MySession.Static == null)
            {
                m_sessionTotalFrames = 0;
                m_maxSessionFPS = 0;
                m_minSessionFPS = 0x7fffffff;
            }
            long ticks = MyPerformanceCounter.ElapsedTicks - m_lastFrameTime;
            FrameTime = (float) MyPerformanceCounter.TicksToMs(ticks);
            m_lastFrameTime = MyPerformanceCounter.ElapsedTicks;
            m_frameTimeAvg.Enqueue(FrameTime);
            if (ticks > m_lastFrameMax)
            {
                m_lastFrameMax = ticks;
            }
            if (ticks < m_lastFrameMin)
            {
                m_lastFrameMin = ticks;
            }
            if (((float) MyPerformanceCounter.TicksToMs(MyPerformanceCounter.ElapsedTicks - m_lastTime)) >= 1000f)
            {
                FrameTimeMin = (float) MyPerformanceCounter.TicksToMs(m_lastFrameMin);
                FrameTimeMax = (float) MyPerformanceCounter.TicksToMs(m_lastFrameMax);
                m_lastFrameMin = 0x7fffffffffffffffL;
                m_lastFrameMax = -9223372036854775808L;
                if ((MySession.Static != null) && (m_firstFrames > 20))
                {
                    m_minSessionFPS = Math.Min(m_minSessionFPS, m_fpsCounter);
                    m_maxSessionFPS = Math.Max(m_maxSessionFPS, m_fpsCounter);
                }
                if (m_firstFrames <= 20)
                {
                    m_firstFrames = (byte) (m_firstFrames + 1);
                }
                m_lastTime = MyPerformanceCounter.ElapsedTicks;
                m_lastFpsDrawn = m_fpsCounter;
                m_fpsCounter = 0;
            }
        }

        public static float FrameTime
        {
            [CompilerGenerated]
            get => 
                <FrameTime>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<FrameTime>k__BackingField = value);
        }

        public static float FrameTimeAvg =>
            m_frameTimeAvg.Avg;

        public static float FrameTimeMin
        {
            [CompilerGenerated]
            get => 
                <FrameTimeMin>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<FrameTimeMin>k__BackingField = value);
        }

        public static float FrameTimeMax
        {
            [CompilerGenerated]
            get => 
                <FrameTimeMax>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<FrameTimeMax>k__BackingField = value);
        }
    }
}

