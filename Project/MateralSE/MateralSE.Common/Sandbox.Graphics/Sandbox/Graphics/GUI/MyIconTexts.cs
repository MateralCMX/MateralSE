namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyIconTexts : Dictionary<MyGuiDrawAlignEnum, MyColoredText>
    {
        public void Draw(Vector2 iconPosition, Vector2 iconSize, float backgroundAlphaFade, float colorMultiplicator = 1f)
        {
            this.Draw(iconPosition, iconSize, backgroundAlphaFade, false, colorMultiplicator);
        }

        public void Draw(Vector2 iconPosition, Vector2 iconSize, float backgroundAlphaFade, bool isHighlight, float colorMultiplicator = 1f)
        {
            foreach (KeyValuePair<MyGuiDrawAlignEnum, MyColoredText> pair in this)
            {
                Vector2 normalizedPosition = this.GetPosition(iconPosition, iconSize, pair.Key);
                pair.Value.Draw(normalizedPosition, pair.Key, backgroundAlphaFade, isHighlight, colorMultiplicator);
            }
        }

        private Vector2 GetPosition(Vector2 iconPosition, Vector2 iconSize, MyGuiDrawAlignEnum drawAlign)
        {
            Vector2 vector;
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    vector = iconPosition;
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    vector = iconPosition + new Vector2(0f, iconSize.Y / 2f);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    vector = iconPosition + new Vector2(0f, iconSize.Y);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    vector = iconPosition + new Vector2(iconSize.X / 2f, 0f);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    vector = iconPosition + new Vector2(iconSize.X / 2f, iconSize.Y / 2f);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    vector = iconPosition + new Vector2(iconSize.X / 2f, iconSize.Y);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    vector = iconPosition + new Vector2(iconSize.X, 0f);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    vector = iconPosition + new Vector2(iconSize.X, iconSize.Y / 2f);
                    break;

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    vector = iconPosition + new Vector2(iconSize.X, iconSize.Y);
                    break;

                default:
                    throw new Exception();
            }
            return vector;
        }
    }
}

