namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Gui;
    using VRage.Generics;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenHudBase : MyGuiScreenBase
    {
        private static readonly MyStringId ID_SQUARE = MyStringId.GetOrCompute("Square");
        protected string m_atlas;
        protected MyAtlasTextureCoordinate[] m_atlasCoords;
        protected float m_textScale;
        protected StringBuilder m_hudIndicatorText;
        protected StringBuilder m_helperSB;
        protected MyObjectsPoolSimple<MyHudText> m_texts;

        public MyGuiScreenHudBase() : base(new Vector2?(Vector2.Zero), nullable, nullable2, false, null, 0f, 0f)
        {
            this.m_hudIndicatorText = new StringBuilder();
            this.m_helperSB = new StringBuilder();
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.CanHaveFocus = false;
            base.m_drawEvenWithoutFocus = true;
            base.m_closeOnEsc = false;
            this.m_texts = new MyObjectsPoolSimple<MyHudText>(0x7d0);
        }

        public MyHudText AllocateText() => 
            this.m_texts.Allocate();

        public static Vector2 ConvertHudToNormalizedGuiPosition(ref Vector2 hudPos)
        {
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            Vector2 vector = new Vector2((float) safeFullscreenRectangle.Width, (float) safeFullscreenRectangle.Height);
            Vector2 vector2 = new Vector2((float) safeFullscreenRectangle.X, (float) safeFullscreenRectangle.Y);
            Rectangle safeGuiRectangle = MyGuiManager.GetSafeGuiRectangle();
            Vector2 vector3 = new Vector2((float) safeGuiRectangle.Width, (float) safeGuiRectangle.Height);
            Vector2 vector4 = new Vector2((float) safeGuiRectangle.X, (float) safeGuiRectangle.Y);
            return ((((hudPos * vector) + vector2) - vector4) / vector3);
        }

        protected static Vector2 ConvertNormalizedGuiToHud(ref Vector2 normGuiPos)
        {
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            Vector2 vector = new Vector2((float) safeFullscreenRectangle.Width, (float) safeFullscreenRectangle.Height);
            Vector2 vector2 = new Vector2((float) safeFullscreenRectangle.X, (float) safeFullscreenRectangle.Y);
            Rectangle safeGuiRectangle = MyGuiManager.GetSafeGuiRectangle();
            Vector2 vector3 = new Vector2((float) safeGuiRectangle.Width, (float) safeGuiRectangle.Height);
            Vector2 vector4 = new Vector2((float) safeGuiRectangle.X, (float) safeGuiRectangle.Y);
            return ((((normGuiPos * vector3) + vector4) - vector2) / vector);
        }

        public override bool Draw()
        {
            if (MySandboxGame.Config.ShowCrosshair && !MyHud.CutsceneHud)
            {
                MyHud.Crosshair.Draw(this.m_atlas, this.m_atlasCoords);
            }
            return base.Draw();
        }

        private static void DrawSelectedObjectHighlight(MyHudSelectedObject selection, MyHudObjectHighlightStyleData? data)
        {
            if (selection.InteractiveObject.RenderObjectID != uint.MaxValue)
            {
                switch (selection.HighlightStyle)
                {
                    case MyHudObjectHighlightStyle.None:
                        return;

                    case MyHudObjectHighlightStyle.DummyHighlight:
                        DrawSelectedObjectHighlightDummy(selection, data.Value.AtlasTexture, data.Value.TextureCoord);
                        break;

                    case MyHudObjectHighlightStyle.OutlineHighlight:
                        if (((selection.SectionNames == null) || (selection.SectionNames.Length != 0)) || (selection.SubpartIndices != null))
                        {
                            DrawSelectedObjectHighlightOutline(selection, false);
                        }
                        else
                        {
                            DrawSelectedObjectHighlightDummy(selection, data.Value.AtlasTexture, data.Value.TextureCoord);
                        }
                        break;

                    case MyHudObjectHighlightStyle.EdgeHighlight:
                        DrawSelectedObjectHighlightOutline(selection, true);
                        break;

                    default:
                        throw new Exception("Unknown highlight style");
                }
                selection.Visible = true;
            }
        }

        public static unsafe void DrawSelectedObjectHighlightDummy(MyHudSelectedObject selection, string atlasTexture, MyAtlasTextureCoordinate textureCoord)
        {
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            Vector2 scale = new Vector2((float) safeFullscreenRectangle.Width, (float) safeFullscreenRectangle.Height);
            MatrixD worldMatrix = (selection.InteractiveObject.ActivationMatrix * MySector.MainCamera.ViewMatrix) * MySector.MainCamera.ProjectionMatrix;
            BoundingBoxD xd2 = new BoundingBoxD(-Vector3D.Half, Vector3D.Half).TransformSlow(ref worldMatrix);
            Vector2 pos = new Vector2((float) xd2.Min.X, (float) xd2.Min.Y);
            Vector2 vector3 = new Vector2((float) xd2.Max.X, (float) xd2.Max.Y);
            pos = (pos * 0.5f) + (0.5f * Vector2.One);
            vector3 = (vector3 * 0.5f) + (0.5f * Vector2.One);
            Vector2* vectorPtr1 = (Vector2*) ref pos;
            vectorPtr1->Y = 1f - pos.Y;
            Vector2* vectorPtr2 = (Vector2*) ref vector3;
            vectorPtr2->Y = 1f - vector3.Y;
            float textureScale = ((float) Math.Pow((double) Math.Abs((pos - vector3).X), 0.34999999403953552)) * 2.5f;
            if (selection.InteractiveObject.ShowOverlay)
            {
                BoundingBoxD localbox = new BoundingBoxD(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
                Color color = Color.Gold * 0.4f;
                MatrixD activationMatrix = selection.InteractiveObject.ActivationMatrix;
                MatrixD worldToLocal = MatrixD.Invert(selection.InteractiveObject.WorldMatrix);
                MyStringId? lineMaterial = null;
                MySimpleObjectDraw.DrawAttachedTransparentBox(ref activationMatrix, ref localbox, ref color, selection.InteractiveObject.RenderObjectID, ref worldToLocal, MySimpleObjectRasterizer.Solid, 0, 0.05f, new MyStringId?(ID_SQUARE), lineMaterial, true);
            }
            if (MyFakes.ENABLE_USE_OBJECT_CORNERS)
            {
                DrawSelectionCorner(atlasTexture, selection, textureCoord, scale, pos, -Vector2.UnitY, textureScale);
                DrawSelectionCorner(atlasTexture, selection, textureCoord, scale, new Vector2(pos.X, vector3.Y), Vector2.UnitX, textureScale);
                DrawSelectionCorner(atlasTexture, selection, textureCoord, scale, new Vector2(vector3.X, pos.Y), -Vector2.UnitX, textureScale);
                DrawSelectionCorner(atlasTexture, selection, textureCoord, scale, vector3, Vector2.UnitY, textureScale);
            }
        }

        private static void DrawSelectedObjectHighlightOutline(MyHudSelectedObject selection, bool edgeHighlight = false)
        {
            Color color = selection.Color;
            if (edgeHighlight)
            {
                color.A = 0;
            }
            float contourHighlightThickness = MySector.EnvironmentDefinition.ContourHighlightThickness;
            float highlightPulseInSeconds = MySector.EnvironmentDefinition.HighlightPulseInSeconds;
            if ((MySession.Static.GetComponent<MyHighlightSystem>() != null) && !MySession.Static.GetComponent<MyHighlightSystem>().IsReserved(selection.InteractiveObject.Owner.EntityId))
            {
                MyRenderProxy.UpdateModelHighlight(selection.InteractiveObject.RenderObjectID, selection.SectionNames, selection.SubpartIndices, new Color?(color), contourHighlightThickness, highlightPulseInSeconds, selection.InteractiveObject.InstanceID);
            }
        }

        public static void DrawSelectionCorner(string atlasTexture, MyHudSelectedObject selection, MyAtlasTextureCoordinate textureCoord, Vector2 scale, Vector2 pos, Vector2 rightVector, float textureScale)
        {
            if (MyVideoSettingsManager.IsTripleHead())
            {
                pos.X *= 3f;
            }
            MyRenderProxy.DrawSpriteAtlas(atlasTexture, pos, textureCoord.Offset, textureCoord.Size, rightVector, scale, selection.Color, (selection.HalfSize / MyGuiManager.GetHudSize()) * textureScale, null);
        }

        public void DrawTexts()
        {
            if (this.m_texts.GetAllocatedCount() > 0)
            {
                for (int i = 0; i < this.m_texts.GetAllocatedCount(); i++)
                {
                    MyHudText allocatedItem = this.m_texts.GetAllocatedItem(i);
                    if (allocatedItem.GetStringBuilder().Length != 0)
                    {
                        allocatedItem.Position /= MyGuiManager.GetHudSize();
                        Vector2 position = ConvertHudToNormalizedGuiPosition(ref allocatedItem.Position);
                        Vector2 textSize = MyGuiManager.MeasureString(allocatedItem.Font, allocatedItem.GetStringBuilder(), MyGuiSandbox.GetDefaultTextScaleWithLanguage()) * allocatedItem.Scale;
                        MyGuiTextShadows.DrawShadow(ref position, ref textSize, null, ((float) allocatedItem.Color.A) / 255f, allocatedItem.Alignement);
                        MyGuiManager.DrawString(allocatedItem.Font, allocatedItem.GetStringBuilder(), position, allocatedItem.Scale, new Color?(allocatedItem.Color), allocatedItem.Alignement, false, float.PositiveInfinity);
                    }
                }
                this.m_texts.ClearAllAllocated();
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenHudBase";

        public MyAtlasTextureCoordinate GetTextureCoord(MyHudTexturesEnum texture) => 
            this.m_atlasCoords[(int) texture];

        public static void HandleSelectedObjectHighlight(MyHudSelectedObject selection, MyHudObjectHighlightStyleData? data)
        {
            if (selection.PreviousObject.Instance != null)
            {
                RemoveObjectHighlightInternal(ref selection.PreviousObject, true);
            }
            switch (selection.State)
            {
                case MyHudSelectedObjectState.VisibleStateSet:
                    if (!selection.Visible || (((selection.CurrentObject.Style != MyHudObjectHighlightStyle.OutlineHighlight) && ((selection.CurrentObject.Style != MyHudObjectHighlightStyle.EdgeHighlight) && (selection.CurrentObject.Style != MyHudObjectHighlightStyle.DummyHighlight))) && (selection.VisibleRenderID == selection.CurrentObject.Instance.RenderObjectID)))
                    {
                        break;
                    }
                    DrawSelectedObjectHighlight(selection, data);
                    return;

                case MyHudSelectedObjectState.MarkedForVisible:
                    DrawSelectedObjectHighlight(selection, data);
                    return;

                case MyHudSelectedObjectState.MarkedForNotVisible:
                    RemoveObjectHighlight(selection);
                    break;

                default:
                    return;
            }
        }

        public override void LoadContent()
        {
            LoadTextureAtlas(out this.m_atlas, out this.m_atlasCoords);
            base.LoadContent();
        }

        public static void LoadTextureAtlas(out string atlasFile, out MyAtlasTextureCoordinate[] atlasCoords)
        {
            MyTextureAtlasUtils.LoadTextureAtlas(MyEnumsToStrings.HudTextures, @"Textures\HUD\", @"Textures\HUD\HudAtlas.tai", out atlasFile, out atlasCoords);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            MyHud.Crosshair.RecreateControls(constructor);
        }

        private static void RemoveObjectHighlight(MyHudSelectedObject selection)
        {
            RemoveObjectHighlightInternal(ref selection.CurrentObject, false);
            selection.Visible = false;
        }

        private static void RemoveObjectHighlightInternal(ref MyHudSelectedObjectStatus status, bool reset)
        {
            if ((((status.Style - 2) <= MyHudObjectHighlightStyle.DummyHighlight) && ((MySession.Static.GetComponent<MyHighlightSystem>() != null) && !MySession.Static.GetComponent<MyHighlightSystem>().IsReserved(status.Instance.Owner.EntityId))) && (status.Instance.RenderObjectID != uint.MaxValue))
            {
                Color? outlineColor = null;
                MyRenderProxy.UpdateModelHighlight(status.Instance.RenderObjectID, null, status.SubpartIndices, outlineColor, -1f, 0f, status.Instance.InstanceID);
            }
            if (reset)
            {
                status.Reset();
            }
        }

        public override void UnloadContent()
        {
            this.m_atlas = null;
            this.m_atlasCoords = null;
            base.UnloadContent();
        }

        public override bool Update(bool hasFocus)
        {
            if (MySandboxGame.Config.ShowCrosshair)
            {
                MyHud.Crosshair.Update();
            }
            return base.Update(hasFocus);
        }

        public string TextureAtlas =>
            this.m_atlas;
    }
}

