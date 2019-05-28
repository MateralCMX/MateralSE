namespace VRageMath
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public static class ColorExtensions
    {
        public static VRageMath.Color Alpha(this VRageMath.Color c, float a) => 
            new VRageMath.Color(c, a);

        public static Vector3 ColorToHSV(this VRageMath.Color rgb)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B);
            int num = Math.Max(color.R, Math.Max(color.G, color.B));
            int num2 = Math.Min(color.R, Math.Min(color.G, color.B));
            return new Vector3(color.GetHue() / 360f, (num == 0) ? 0f : (1f - ((1f * num2) / ((float) num))), ((float) num) / 255f);
        }

        public static Vector3 ColorToHSVDX11(this VRageMath.Color rgb)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B);
            int num = Math.Max(color.R, Math.Max(color.G, color.B));
            int num2 = Math.Min(color.R, Math.Min(color.G, color.B));
            return new Vector3(color.GetHue() / 360f, (num == 0) ? -1f : (1f - ((2f * num2) / ((float) num))), -1f + ((2f * num) / 255f));
        }

        public static VRageMath.Color HexToColor(string hex)
        {
            if ((hex.Length > 0) && !hex.StartsWith("#"))
            {
                string text1 = "#" + hex;
                hex = text1;
            }
            System.Drawing.Color color = ColorTranslator.FromHtml(hex);
            return new VRageMath.Color(color.R, color.G, color.B);
        }

        public static Vector4 HexToVector4(string hex)
        {
            if ((hex.Length > 0) && !hex.StartsWith("#"))
            {
                string text1 = "#" + hex;
                hex = text1;
            }
            System.Drawing.Color color = ColorTranslator.FromHtml(hex);
            return (new Vector4((float) color.R, (float) color.G, (float) color.B, 255f) / 255f);
        }

        public static VRageMath.Color HSVtoColor(this Vector3 HSV) => 
            new VRageMath.Color((((Hue(HSV.X) - 1f) * HSV.Y) + 1f) * HSV.Z);

        private static Vector3 Hue(float H)
        {
            float num = 2f - Math.Abs((float) ((H * 6f) - 2f));
            float num2 = 2f - Math.Abs((float) ((H * 6f) - 4f));
            return new Vector3(MathHelper.Clamp((float) (Math.Abs((float) ((H * 6f) - 3f)) - 1f), (float) 0f, (float) 1f), MathHelper.Clamp(num, 0f, 1f), MathHelper.Clamp(num2, 0f, 1f));
        }

        public static float HueDistance(this VRageMath.Color color, float hue)
        {
            float num = Math.Abs((float) (color.ColorToHSV().X - hue));
            return Math.Min(num, 1f - num);
        }

        public static float HueDistance(this VRageMath.Color color, VRageMath.Color otherColor) => 
            color.HueDistance(otherColor.ColorToHSV().X);

        public static uint PackHSVToUint(this Vector3 HSV)
        {
            int num2 = ((int) Math.Round((double) ((HSV.Z * 100f) + 100f))) << 0x18;
            return (uint) ((((int) Math.Round((double) (HSV.X * 360f))) | (((int) Math.Round((double) ((HSV.Y * 100f) + 100f))) << 0x10)) | num2);
        }

        public static Vector4 PremultiplyColor(this Vector4 c) => 
            new Vector4(c.X * c.W, c.Y * c.W, c.Z * c.W, c.W);

        public static VRageMath.Color Shade(this VRageMath.Color c, float r) => 
            new VRageMath.Color((int) (c.R * r), (int) (c.G * r), (int) (c.B * r), c.A);

        public static Vector3 TemperatureToRGB(float temperature)
        {
            Vector3 vector = new Vector3();
            temperature /= 100f;
            if (temperature <= 66f)
            {
                vector.X = 1f;
                vector.Y = (float) MathHelper.Saturate((double) ((0.390081579 * Math.Log((double) temperature)) - 0.631841444));
            }
            else
            {
                float num = temperature - 60f;
                vector.X = (float) MathHelper.Saturate((double) (1.292936186 * Math.Pow((double) num, -0.1332047592)));
                vector.Y = (float) MathHelper.Saturate((double) (1.129890861 * Math.Pow((double) num, -0.0755148492)));
            }
            vector.Z = (temperature < 66f) ? ((temperature > 19f) ? ((float) MathHelper.Saturate((double) ((0.543206789 * Math.Log((double) (temperature - 10f))) - 1.196254089))) : 0f) : 1f;
            return vector;
        }

        public static VRageMath.Color Tint(this VRageMath.Color c, float r) => 
            new VRageMath.Color((int) (c.R + ((0xff - c.R) * r)), (int) (c.G + ((0xff - c.G) * r)), (int) (c.B + ((0xff - c.B) * r)), c.A);

        public static Vector3 ToGray(this Vector3 c) => 
            new Vector3(((0.2126f * c.X) + (0.7152f * c.Y)) + (0.0722f * c.Z));

        public static Vector4 ToGray(this Vector4 c) => 
            new Vector4(((0.2126f * c.X) + (0.7152f * c.Y)) + (0.0722f * c.Z), ((0.2126f * c.X) + (0.7152f * c.Y)) + (0.0722f * c.Z), ((0.2126f * c.X) + (0.7152f * c.Y)) + (0.0722f * c.Z), c.W);

        public static Vector3 ToHsv(this Vector3 rgb)
        {
            Vector4 vector = new Vector4(0f, -0.3333333f, 0.6666667f, -1f);
            Vector4 vector2 = (rgb.Z > rgb.Y) ? new Vector4(rgb.X, rgb.Y, vector.W, vector.Z) : new Vector4(rgb.Y, rgb.Z, vector.X, vector.Y);
            Vector4 vector3 = (vector2.X > rgb.X) ? new Vector4(vector2.X, vector2.Y, vector2.W, rgb.X) : new Vector4(rgb.X, vector2.Y, vector2.Z, vector2.X);
            float num = vector3.X - Math.Min(vector3.W, vector3.Y);
            float num2 = 1E-10f;
            return new Vector3(Math.Abs((double) (vector3.Z + (((double) (vector3.W - vector3.Y)) / ((6.0 * num) + num2)))), (double) (num / (vector3.X + num2)), (double) vector3.X);
        }

        public static Vector3 ToLinearRGB(this Vector3 c) => 
            new Vector3(ToLinearRGBComponent(c.X), ToLinearRGBComponent(c.Y), ToLinearRGBComponent(c.Z));

        public static Vector4 ToLinearRGB(this Vector4 c) => 
            new Vector4(ToLinearRGBComponent(c.X), ToLinearRGBComponent(c.Y), ToLinearRGBComponent(c.Z), c.W);

        public static float ToLinearRGBComponent(float c) => 
            ((c <= 0.04045f) ? (c / 12.92f) : ((float) Math.Pow((double) ((c + 0.055f) / 1.055f), 2.4000000953674316)));

        public static Vector3 ToSRGB(this Vector3 c) => 
            new Vector3(ToSRGBComponent(c.X), ToSRGBComponent(c.Y), ToSRGBComponent(c.Z));

        public static Vector4 ToSRGB(this Vector4 c) => 
            new Vector4(ToSRGBComponent(c.X), ToSRGBComponent(c.Y), ToSRGBComponent(c.Z), c.W);

        public static float ToSRGBComponent(float c) => 
            ((c <= 0.0031308f) ? (c * 12.92f) : ((((float) Math.Pow((double) c, 0.4166666567325592)) * 1.055f) - 0.055f));

        public static Vector4 UnmultiplyColor(this Vector4 c) => 
            ((c.W != 0f) ? new Vector4(c.X / c.W, c.Y / c.W, c.Z / c.W, c.W) : Vector4.Zero);

        public static Vector3 UnpackHSVFromUint(uint packed)
        {
            byte num = (byte) (packed >> 0x10);
            byte num2 = (byte) (packed >> 0x18);
            return new Vector3(((float) ((ushort) packed)) / 360f, ((float) (num - 100)) / 100f, ((float) (num2 - 100)) / 100f);
        }
    }
}

