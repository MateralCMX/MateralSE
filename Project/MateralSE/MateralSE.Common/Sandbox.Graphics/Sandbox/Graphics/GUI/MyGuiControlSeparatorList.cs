namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlSeparatorList))]
    public class MyGuiControlSeparatorList : MyGuiControlBase
    {
        private List<Separator> m_separators;

        public MyGuiControlSeparatorList() : base(nullable, nullable, nullable2, null, null, false, false, false, MyGuiControlHighlightType.NEVER, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? nullable = null;
            nullable = null;
            this.m_separators = new List<Separator>();
        }

        public unsafe Separator AddHorizontal(Vector2 start, float length, float width = 0f, Vector4? color = new Vector4?())
        {
            Separator* separatorPtr1;
            Separator separator2 = new Separator {
                Start = start,
                Size = new Vector2(length, width)
            };
            Vector4? nullable = color;
            separatorPtr1->Color = (nullable != null) ? nullable.GetValueOrDefault() : MyGuiConstants.THEMED_GUI_LINE_COLOR.ToVector4();
            separatorPtr1 = (Separator*) ref separator2;
            separator2.Visible = true;
            Separator item = separator2;
            this.m_separators.Add(item);
            return item;
        }

        public unsafe Separator AddVertical(Vector2 start, float length, float width = 0f, Vector4? color = new Vector4?())
        {
            Separator* separatorPtr1;
            Separator separator2 = new Separator {
                Start = start,
                Size = new Vector2(width, length)
            };
            Vector4? nullable = color;
            separatorPtr1->Color = (nullable != null) ? nullable.GetValueOrDefault() : MyGuiConstants.THEMED_GUI_LINE_COLOR.ToVector4();
            separatorPtr1 = (Separator*) ref separator2;
            separator2.Visible = true;
            Separator item = separator2;
            this.m_separators.Add(item);
            return item;
        }

        public void Clear()
        {
            this.m_separators.Clear();
        }

        public override unsafe void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.GetPositionAbsoluteCenter();
            foreach (Separator separator in this.m_separators)
            {
                if (separator.Visible)
                {
                    Color color = ApplyColorMaskModifiers(base.ColorMask * separator.Color, base.Enabled, transitionAlpha);
                    Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(base.GetPositionAbsoluteCenter() + separator.Start, false);
                    Vector2 screenSizeFromNormalizedSize = MyGuiManager.GetScreenSizeFromNormalizedSize(separator.Size, false);
                    if (screenSizeFromNormalizedSize.X == 0f)
                    {
                        float* singlePtr1 = (float*) ref screenSizeFromNormalizedSize.X;
                        singlePtr1[0]++;
                    }
                    else if (screenSizeFromNormalizedSize.Y == 0f)
                    {
                        float* singlePtr2 = (float*) ref screenSizeFromNormalizedSize.Y;
                        singlePtr2[0]++;
                    }
                    MyGuiManager.DrawSpriteBatch(@"Textures\GUI\Blank.dds", (int) screenCoordinateFromNormalizedCoordinate.X, (int) screenCoordinateFromNormalizedCoordinate.Y, (int) screenSizeFromNormalizedSize.X, (int) screenSizeFromNormalizedSize.Y, color, true);
                }
            }
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlSeparatorList objectBuilder = (MyObjectBuilder_GuiControlSeparatorList) base.GetObjectBuilder();
            objectBuilder.Separators = new List<MyObjectBuilder_GuiControlSeparatorList.Separator>(this.m_separators.Count);
            foreach (Separator separator in this.m_separators)
            {
                MyObjectBuilder_GuiControlSeparatorList.Separator item = new MyObjectBuilder_GuiControlSeparatorList.Separator {
                    StartX = separator.Start.X,
                    StartY = separator.Start.Y,
                    SizeX = separator.Size.X,
                    SizeY = separator.Size.Y
                };
                objectBuilder.Separators.Add(item);
            }
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlSeparatorList list = (MyObjectBuilder_GuiControlSeparatorList) builder;
            this.m_separators.Clear();
            this.m_separators.Capacity = list.Separators.Count;
            foreach (MyObjectBuilder_GuiControlSeparatorList.Separator separator in list.Separators)
            {
                Separator item = new Separator {
                    Start = new Vector2(separator.StartX, separator.StartY),
                    Size = new Vector2(separator.SizeX, separator.SizeY)
                };
                this.m_separators.Add(item);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Separator
        {
            public Vector2 Start;
            public Vector2 Size;
            public Vector4 Color;
            public bool Visible;
        }
    }
}

