namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender;

    public class MyHudScreenEffects
    {
        private float m_blackScreenCurrent = 1f;
        private float m_blackScreenStart;
        private float m_blackScreenTimeIncrement;
        private float m_blackScreenTimeTimer;
        private float m_blackScreenTarget = 1f;
        private bool m_blackScreenDataSaved;
        private Color m_blackScreenDataSavedLightColor = Color.Black;
        private Color m_blackScreenDataSavedDarkColor = Color.Black;
        private float m_blackScreenDataSavedStrength;
        public bool BlackScreenMinimalizeHUD = true;
        public Color BlackScreenColor = Color.Black;

        public void FadeScreen(float targetAlpha, float time = 0f)
        {
            float single1 = MathHelper.Clamp(targetAlpha, 0f, 1f);
            targetAlpha = single1;
            if (time <= 0f)
            {
                this.m_blackScreenTarget = targetAlpha;
                this.m_blackScreenCurrent = targetAlpha;
            }
            else
            {
                this.m_blackScreenTarget = targetAlpha;
                this.m_blackScreenStart = this.m_blackScreenCurrent;
                this.m_blackScreenTimeTimer = 0f;
                this.m_blackScreenTimeIncrement = 0.01666667f / time;
            }
            if ((targetAlpha < 1f) && !this.m_blackScreenDataSaved)
            {
                this.m_blackScreenDataSaved = true;
                this.m_blackScreenDataSavedLightColor = MyPostprocessSettingsWrapper.Settings.Data.LightColor;
                this.m_blackScreenDataSavedDarkColor = MyPostprocessSettingsWrapper.Settings.Data.DarkColor;
                this.m_blackScreenDataSavedStrength = MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength;
            }
        }

        public void SwitchFadeScreen(float time = 0f)
        {
            this.FadeScreen(1f - this.m_blackScreenTarget, time);
        }

        public void Update()
        {
            this.UpdateBlackScreen();
        }

        private void UpdateBlackScreen()
        {
            if ((this.m_blackScreenTimeTimer < 1f) && (this.m_blackScreenCurrent != this.m_blackScreenTarget))
            {
                this.m_blackScreenTimeTimer += this.m_blackScreenTimeIncrement;
                if (this.m_blackScreenTimeTimer > 1f)
                {
                    this.m_blackScreenTimeTimer = 1f;
                }
                this.m_blackScreenCurrent = MathHelper.Lerp(this.m_blackScreenStart, this.m_blackScreenTarget, this.m_blackScreenTimeTimer);
            }
            if (this.m_blackScreenCurrent < 1f)
            {
                if (this.BlackScreenMinimalizeHUD)
                {
                    MyHud.CutsceneHud = true;
                }
                MyPostprocessSettingsWrapper.Settings.Data.LightColor = (Vector3) this.BlackScreenColor;
                MyPostprocessSettingsWrapper.Settings.Data.DarkColor = (Vector3) this.BlackScreenColor;
                MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength = 1f - this.m_blackScreenCurrent;
                MyPostprocessSettingsWrapper.MarkDirty();
            }
            else if (this.m_blackScreenDataSaved)
            {
                this.m_blackScreenDataSaved = false;
                MyHud.CutsceneHud = MySession.Static.GetComponent<MySessionComponentCutscenes>().IsCutsceneRunning;
                MyPostprocessSettingsWrapper.Settings.Data.LightColor = (Vector3) this.m_blackScreenDataSavedLightColor;
                MyPostprocessSettingsWrapper.Settings.Data.DarkColor = (Vector3) this.m_blackScreenDataSavedDarkColor;
                MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength = this.m_blackScreenDataSavedStrength;
                MyPostprocessSettingsWrapper.MarkDirty();
            }
        }

        public float BlackScreenCurrent =>
            this.m_blackScreenCurrent;
    }
}

