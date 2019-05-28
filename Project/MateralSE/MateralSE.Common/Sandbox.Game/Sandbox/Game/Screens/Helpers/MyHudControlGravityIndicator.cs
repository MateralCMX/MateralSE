namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage.Game.Components;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyHudControlGravityIndicator
    {
        private readonly MyObjectBuilder_GuiTexture m_overlayTexture;
        private readonly MyObjectBuilder_GuiTexture m_fillTexture;
        private readonly MyObjectBuilder_GuiTexture m_velocityTexture;
        private Vector2 m_position;
        private Vector2 m_size;
        private Vector2 m_sizeOnScreen;
        private Vector2 m_screenPosition;
        private Vector2 m_origin;
        private Vector2 m_scale;
        private Vector2 m_screenSize;
        private Vector2 m_velocitySizeOnScreen;
        private MyGuiDrawAlignEnum m_originAlign;
        private ConditionBase m_visibleCondition;

        public MyHudControlGravityIndicator(MyObjectBuilder_GravityIndicatorVisualStyle definition)
        {
            MyObjectBuilder_GravityIndicatorVisualStyle style = definition;
            this.m_position = style.OffsetPx;
            this.m_size = style.SizePx;
            this.m_velocitySizeOnScreen = style.VelocitySizePx;
            this.m_fillTexture = MyGuiTextures.Static.GetTexture(style.FillTexture);
            this.m_overlayTexture = MyGuiTextures.Static.GetTexture(style.OverlayTexture);
            this.m_velocityTexture = MyGuiTextures.Static.GetTexture(style.VelocityTexture);
            this.m_originAlign = style.OriginAlign;
            this.m_visibleCondition = style.VisibleCondition;
            if (style.VisibleCondition != null)
            {
                this.InitStatConditions(style.VisibleCondition);
            }
            this.RecalculatePosition();
        }

        public void Draw(float alpha)
        {
            if ((this.m_visibleCondition == null) || this.m_visibleCondition.Eval())
            {
                if ((Math.Abs((float) (MySector.MainCamera.Viewport.Width - this.m_screenSize.X)) > 1E-05f) || (Math.Abs((float) (MySector.MainCamera.Viewport.Height - this.m_screenSize.Y)) > 1E-05f))
                {
                    this.RecalculatePosition();
                }
                IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
                if ((controlledEntity != null) && (controlledEntity.Entity != null))
                {
                    RectangleF ef;
                    Rectangle? nullable;
                    MatrixD worldMatrix = controlledEntity.Entity.PositionComp.WorldMatrix;
                    Vector3D forward = worldMatrix.Forward;
                    Vector3D right = worldMatrix.Right;
                    Vector3D up = worldMatrix.Up;
                    MyPhysicsComponentBase base2 = controlledEntity.Physics();
                    Vector3 v = (base2 != null) ? base2.LinearVelocity : Vector3.Zero;
                    Vector3 vector2 = MyGravityProviderSystem.CalculateTotalGravityInPoint(worldMatrix.Translation);
                    Color color = MyGuiControlBase.ApplyColorMaskModifiers((Vector4) Color.White, true, alpha);
                    if (vector2 != Vector3.Zero)
                    {
                        vector2.Normalize();
                        double num = (forward.Dot(vector2) + 1.0) / 2.0;
                        Vector2D vectord4 = new Vector2D(right.Dot(vector2), up.Dot(vector2));
                        double a = ((vectord4.LengthSquared() > 9.9999997473787516E-06) ? Math.Atan2(vectord4.Y, vectord4.X) : 0.0) + 3.1415926535897931;
                        int num3 = (int) (this.m_sizeOnScreen.Y * num);
                        ef = new RectangleF(this.m_screenPosition.X, (this.m_screenPosition.Y + this.m_sizeOnScreen.Y) - num3, this.m_sizeOnScreen.X, (float) num3);
                        int height = (int) (this.m_fillTexture.SizePx.Y * num);
                        nullable = new Rectangle(0, this.m_fillTexture.SizePx.Y - height, this.m_fillTexture.SizePx.X, height);
                        Vector2 rightVector = new Vector2((float) Math.Sin(a), (float) Math.Cos(a));
                        MyRenderProxy.DrawSprite(this.m_fillTexture.Path, ref ef, false, ref nullable, color, 0f, rightVector, ref this.m_origin, SpriteEffects.None, 0f, true, null);
                    }
                    if ((v != Vector3.Zero) && (this.m_velocityTexture != null))
                    {
                        Vector2 vector4 = new Vector2((float) right.Dot(v), -((float) up.Dot(v)));
                        float transitionAlpha = Math.Min(MyMath.Clamp((v.Length() / (MyGridPhysics.ShipMaxLinearVelocity() + 7f)) / 0.05f, 0f, 1f), alpha);
                        float num6 = vector4.Length();
                        float num7 = MyMath.Clamp(1f - ((float) Math.Exp((double) (-num6 * 0.01f))), 0f, 1f);
                        nullable = null;
                        ef = new RectangleF(((this.m_screenPosition + (this.m_sizeOnScreen * 0.5f)) - (this.m_velocitySizeOnScreen * 0.5f)) + ((vector4 * ((1f / num6) * num7)) * (this.m_sizeOnScreen / 2f)), this.m_velocitySizeOnScreen);
                        MyRenderProxy.DrawSprite(this.m_velocityTexture.Path, ref ef, false, ref nullable, MyGuiControlBase.ApplyColorMaskModifiers((Vector4) Color.White, true, transitionAlpha), 0f, Vector2.UnitX, ref this.m_origin, SpriteEffects.None, 0f, true, null);
                    }
                    ef = new RectangleF(this.m_screenPosition, this.m_sizeOnScreen);
                    nullable = null;
                    MyRenderProxy.DrawSprite(this.m_overlayTexture.Path, ref ef, false, ref nullable, color, 0f, Vector2.UnitX, ref this.m_origin, SpriteEffects.None, 0f, true, null);
                }
            }
        }

        private void InitStatConditions(ConditionBase conditionBase)
        {
            StatCondition condition = conditionBase as StatCondition;
            if (condition != null)
            {
                condition.SetStat(MyHud.Stats.GetStat(condition.StatId));
            }
            else
            {
                Condition condition2 = conditionBase as Condition;
                if (condition2 != null)
                {
                    foreach (ConditionBase base2 in condition2.Terms)
                    {
                        this.InitStatConditions(base2);
                    }
                }
            }
        }

        private void RecalculatePosition()
        {
            float? customUIScale = MyHud.HudDefinition.CustomUIScale;
            float num = (customUIScale != null) ? customUIScale.GetValueOrDefault() : MyGuiManager.GetSafeScreenScale();
            this.m_sizeOnScreen = this.m_size * num;
            this.m_velocitySizeOnScreen *= num;
            this.m_screenSize = new Vector2((float) MySandboxGame.ScreenSize.X, (float) MySandboxGame.ScreenSize.Y);
            this.m_screenPosition = this.m_position * num;
            this.m_screenPosition = MyUtils.AlignCoord(this.m_screenPosition, (Vector2) MySandboxGame.ScreenSize, this.m_originAlign);
            this.m_origin = this.m_screenPosition + (this.m_sizeOnScreen / 2f);
        }
    }
}

