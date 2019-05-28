namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using VRageMath;

    public static class My2DClipmapHelpers
    {
        public static readonly Vector2D[] CoordsFromIndex = new Vector2D[] { Vector2D.Zero, Vector2D.UnitX, Vector2D.UnitY, Vector2D.One };
        public static readonly Color[] LodColors;

        static My2DClipmapHelpers()
        {
            Color[] colorArray1 = new Color[12];
            colorArray1[0] = Color.Red;
            colorArray1[1] = Color.Green;
            colorArray1[2] = Color.Blue;
            colorArray1[3] = Color.Yellow;
            colorArray1[4] = Color.Magenta;
            colorArray1[5] = Color.Cyan;
            colorArray1[6] = new Color(1f, 0.5f, 0f);
            colorArray1[7] = new Color(1f, 0f, 0.5f);
            colorArray1[8] = new Color(0.5f, 0f, 1f);
            colorArray1[9] = new Color(0.5f, 1f, 0f);
            colorArray1[10] = new Color(0f, 1f, 0.5f);
            colorArray1[11] = new Color(0f, 0.5f, 1f);
            LodColors = colorArray1;
        }
    }
}

