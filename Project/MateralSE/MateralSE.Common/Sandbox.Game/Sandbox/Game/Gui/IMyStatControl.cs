namespace Sandbox.Game.GUI
{
    using System;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    public interface IMyStatControl
    {
        void Draw(float transitionAlpha);

        float StatCurrent { set; }

        float StatMaxValue { set; }

        float StatMinValue { set; }

        string StatString { get; set; }

        uint FadeInTimeMs { get; set; }

        uint FadeOutTimeMs { get; set; }

        uint MaxOnScreenTimeMs { get; set; }

        uint SpentInStateTimeMs { get; set; }

        MyStatControlState State { get; set; }

        VisualStyleCategory Category { get; set; }

        MyStatControls Parent { get; }

        Vector2 Position { get; set; }

        Vector2 Size { get; set; }

        Vector4 ColorMask { get; set; }

        MyAlphaBlinkBehavior BlinkBehavior { get; }
    }
}

