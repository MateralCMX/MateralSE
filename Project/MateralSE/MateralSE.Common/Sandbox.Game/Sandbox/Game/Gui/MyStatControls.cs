namespace Sandbox.Game.GUI
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyStatControls
    {
        private readonly List<StatBinding> m_bindings = new List<StatBinding>();
        private readonly Dictionary<VisualStyleCategory, float> m_alphaMultipliersByCategory = new Dictionary<VisualStyleCategory, float>();
        private MyObjectBuilder_StatControls m_objectBuilder;
        private float m_resolutionScaleRatio;
        private double m_lastDrawTimeMs;
        private IMyHudStat m_showStatesStat;
        private float m_uiScaleFactor;
        private Vector2 m_position;

        public MyStatControls(MyObjectBuilder_StatControls ob, float uiScale = 1f)
        {
            this.m_objectBuilder = ob;
            this.m_uiScaleFactor = uiScale;
            if (this.m_objectBuilder.StatStyles != null)
            {
                foreach (MyObjectBuilder_StatVisualStyle style in this.m_objectBuilder.StatStyles)
                {
                    if (!(style.StatId != MyStringHash.NullOrEmpty))
                    {
                        this.AddControl(null, style);
                    }
                    else
                    {
                        IMyHudStat stat = MyHud.Stats.GetStat(style.StatId);
                        if (stat != null)
                        {
                            this.AddControl(stat, style);
                        }
                    }
                }
            }
            if (ob.VisibleCondition != null)
            {
                InitConditions(ob.VisibleCondition);
            }
            this.m_showStatesStat = MyHud.Stats.GetStat(MyStringHash.GetOrCompute("hud_show_states"));
            this.m_lastDrawTimeMs = MySession.Static.ElapsedGameTime.TotalMilliseconds;
            foreach (object obj2 in typeof(VisualStyleCategory).GetEnumValues())
            {
                this.m_alphaMultipliersByCategory[(VisualStyleCategory) obj2] = 1f;
            }
        }

        private void AddControl(IMyHudStat stat, MyObjectBuilder_StatVisualStyle style)
        {
            IMyStatControl control = null;
            switch (style)
            {
                case (MyObjectBuilder_CircularProgressBarStatVisualStyle _):
                    control = this.InitCircularProgressBar((MyObjectBuilder_CircularProgressBarStatVisualStyle) style);
                    break;

                case (MyObjectBuilder_ProgressBarStatVisualStyle _):
                    control = this.InitProgressBar((MyObjectBuilder_ProgressBarStatVisualStyle) style);
                    break;

                case (MyObjectBuilder_TextStatVisualStyle _):
                    control = this.InitText((MyObjectBuilder_TextStatVisualStyle) style);
                    break;

                case (MyObjectBuilder_ImageStatVisualStyle _):
                    control = this.InitImage((MyObjectBuilder_ImageStatVisualStyle) style);
                    break;
            }
            if (control != null)
            {
                this.InitStatControl(control, stat, style);
                StatBinding item = new StatBinding();
                item.Control = control;
                item.Style = style;
                item.Stat = stat;
                this.m_bindings.Add(item);
            }
        }

        [Conditional("DEBUG")]
        private unsafe void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_HUD)
            {
                Vector2 pointTo = this.Position + new Vector2(50f, 0f);
                Vector2 vector2 = this.Position + new Vector2(0f, 50f);
                Matrix? projection = null;
                MyRenderProxy.DebugDrawLine2D(this.Position, pointTo, Color.Green, Color.Green, projection, false);
                projection = null;
                MyRenderProxy.DebugDrawLine2D(this.Position, vector2, Color.Green, Color.Green, projection, false);
                foreach (StatBinding binding in this.m_bindings)
                {
                    Vector2 position = binding.Control.Position;
                    Vector2 vector4 = binding.Control.Position;
                    float* singlePtr1 = (float*) ref vector4.X;
                    singlePtr1[0] += binding.Control.Size.X;
                    Vector2 vector5 = binding.Control.Position + binding.Control.Size;
                    Vector2 vector6 = binding.Control.Position;
                    float* singlePtr2 = (float*) ref vector6.Y;
                    singlePtr2[0] += binding.Control.Size.Y;
                    projection = null;
                    MyRenderProxy.DebugDrawLine2D(position, vector4, Color.Red, Color.Red, projection, false);
                    projection = null;
                    MyRenderProxy.DebugDrawLine2D(vector4, vector5, Color.Red, Color.Red, projection, false);
                    projection = null;
                    MyRenderProxy.DebugDrawLine2D(vector5, vector6, Color.Red, Color.Red, projection, false);
                    projection = null;
                    MyRenderProxy.DebugDrawLine2D(vector6, position, Color.Red, Color.Red, projection, false);
                }
            }
        }

        public void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if ((this.m_objectBuilder.VisibleCondition == null) || this.m_objectBuilder.VisibleCondition.Eval())
            {
                double num = MySession.Static.ElapsedGameTime.TotalMilliseconds - this.m_lastDrawTimeMs;
                this.m_lastDrawTimeMs = MySession.Static.ElapsedGameTime.TotalMilliseconds;
                using (List<StatBinding>.Enumerator enumerator = this.m_bindings.GetEnumerator())
                {
                    StatBinding current;
                    bool flag;
                    float num2;
                    goto TR_0028;
                TR_0003:
                    throw new ArgumentOutOfRangeException();
                TR_0006:
                    if ((current.Control.State & (MyStatControlState.Visible | MyStatControlState.FadingIn | MyStatControlState.FadingOut)) != 0)
                    {
                        num2 = Math.Min(transitionAlpha, num2);
                        current.Control.Draw(num2 * this.m_alphaMultipliersByCategory[current.Control.Category]);
                    }
                    current.LastVisibleConditionCheckResult = flag;
                    goto TR_0028;
                TR_0010:
                    if (flag)
                    {
                        if (current.Control.SpentInStateTimeMs <= current.Control.FadeInTimeMs)
                        {
                            num2 = ((float) current.Control.SpentInStateTimeMs) / ((float) current.Control.FadeInTimeMs);
                        }
                        else
                        {
                            current.Control.State = MyStatControlState.Visible;
                            current.Control.SpentInStateTimeMs = 0;
                        }
                    }
                    else
                    {
                        current.Control.State = MyStatControlState.FadingOut;
                        current.Control.SpentInStateTimeMs = current.Control.MaxOnScreenTimeMs - current.Control.SpentInStateTimeMs;
                        goto TR_0017;
                    }
                    goto TR_0006;
                TR_0017:
                    while (true)
                    {
                        if (!flag)
                        {
                            break;
                        }
                        if (current.LastVisibleConditionCheckResult)
                        {
                            break;
                        }
                        current.Control.State = MyStatControlState.FadingIn;
                        current.Control.SpentInStateTimeMs = current.Control.MaxOnScreenTimeMs - current.Control.SpentInStateTimeMs;
                        goto TR_0010;
                    }
                    if (current.Control.SpentInStateTimeMs <= current.Control.FadeOutTimeMs)
                    {
                        num2 = 1f - (((float) current.Control.SpentInStateTimeMs) / ((float) current.Control.FadeOutTimeMs));
                    }
                    else
                    {
                        current.Control.State = MyStatControlState.Invisible;
                        current.Control.SpentInStateTimeMs = 0;
                    }
                    goto TR_0006;
                TR_0028:
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            IMyStatControl control = current.Control;
                            if (current.Stat != null)
                            {
                                control.StatCurrent = current.Stat.CurrentValue;
                                control.StatString = current.Stat.GetValueString();
                                control.StatMaxValue = current.Stat.MaxValue;
                                control.StatMinValue = current.Stat.MinValue;
                            }
                            if (current.Style.BlinkCondition != null)
                            {
                                control.BlinkBehavior.Blink = current.Style.BlinkCondition.Eval();
                            }
                            flag = true;
                            if (current.Style.VisibleCondition != null)
                            {
                                flag = current.Style.VisibleCondition.Eval();
                            }
                            current.Control.SpentInStateTimeMs += (uint) num;
                            num2 = 1f;
                            MyStatControlState state = current.Control.State;
                            switch (state)
                            {
                                case MyStatControlState.FadingOut:
                                    goto TR_0017;

                                case MyStatControlState.FadingIn:
                                    break;

                                case (MyStatControlState.FadingIn | MyStatControlState.FadingOut):
                                    goto TR_0003;

                                case MyStatControlState.Visible:
                                    if ((!flag || ((current.Control.MaxOnScreenTimeMs > 0) && (current.Control.MaxOnScreenTimeMs < current.Control.SpentInStateTimeMs))) && (this.m_showStatesStat.CurrentValue <= 0.5f))
                                    {
                                        current.Control.State = MyStatControlState.FadingOut;
                                        current.Control.SpentInStateTimeMs = 0;
                                    }
                                    goto TR_0006;

                                default:
                                    if (state == MyStatControlState.Invisible)
                                    {
                                        if (flag && (!current.LastVisibleConditionCheckResult || (this.m_showStatesStat.CurrentValue >= 0.5f)))
                                        {
                                            current.Control.State = MyStatControlState.FadingIn;
                                            current.Control.SpentInStateTimeMs = 0;
                                            num2 = 0f;
                                        }
                                    }
                                    else
                                    {
                                        goto TR_0003;
                                    }
                                    goto TR_0006;
                            }
                            goto TR_0010;
                        }
                        else
                        {
                            return;
                        }
                        break;
                    }
                    goto TR_0017;
                }
            }
        }

        private IMyStatControl InitCircularProgressBar(MyObjectBuilder_CircularProgressBarStatVisualStyle style)
        {
            MyObjectBuilder_GuiTexture texture = null;
            Vector2? nullable1;
            if (!MyGuiTextures.Static.TryGetTexture(style.SegmentTexture, out texture))
            {
                return null;
            }
            MyObjectBuilder_GuiTexture texture2 = null;
            if (style.BackgroudTexture != null)
            {
                MyGuiTextures.Static.TryGetTexture(style.BackgroudTexture.Value, out texture2);
            }
            MyStatControlCircularProgressBar bar1 = new MyStatControlCircularProgressBar(this, texture, texture2);
            bar1.Position = this.Position + (style.OffsetPx * this.m_uiScaleFactor);
            bar1.Size = style.SizePx * this.m_uiScaleFactor;
            Vector2? segmentOrigin = style.SegmentOrigin;
            float uiScaleFactor = this.m_uiScaleFactor;
            MyStatControlCircularProgressBar bar2 = bar1;
            if (segmentOrigin != null)
            {
                nullable1 = new Vector2?(segmentOrigin.GetValueOrDefault() * uiScaleFactor);
            }
            else
            {
                nullable1 = null;
            }
            Vector2? nullable = nullable1;
            bar1.SegmentOrigin = (nullable != null) ? nullable.GetValueOrDefault() : ((style.SegmentSizePx * this.m_uiScaleFactor) / 2f);
            MyStatControlCircularProgressBar local1 = bar1;
            local1.SegmentSize = style.SegmentSizePx * this.m_uiScaleFactor;
            MyStatControlCircularProgressBar bar = local1;
            if (style.AngleOffset != null)
            {
                bar.TextureRotationOffset = style.AngleOffset.Value;
            }
            if (style.SpacingAngle != null)
            {
                bar.TextureRotationAngle = style.SpacingAngle.Value;
            }
            if (style.Animate != null)
            {
                bar.Animate = style.Animate.Value;
            }
            if (style.AnimatedSegmentColorMask != null)
            {
                bar.AnimatedSegmentColorMask = style.AnimatedSegmentColorMask.Value;
            }
            if (style.FullSegmentColorMask != null)
            {
                bar.FullSegmentColorMask = style.FullSegmentColorMask.Value;
            }
            if (style.EmptySegmentColorMask != null)
            {
                bar.EmptySegmentColorMask = style.EmptySegmentColorMask.Value;
            }
            if (style.AnimationDelayMs != null)
            {
                bar.AnimationDelay = style.AnimationDelayMs.Value;
            }
            if (style.AnimationSegmentDelayMs != null)
            {
                bar.SegmentAnimationMs = style.AnimationSegmentDelayMs.Value;
            }
            if (style.NumberOfSegments != null)
            {
                bar.NumberOfSegments = style.NumberOfSegments.Value;
            }
            if (style.ShowEmptySegments != null)
            {
                bar.ShowEmptySegments = style.ShowEmptySegments.Value;
            }
            return bar;
        }

        private static void InitConditions(ConditionBase conditionBase)
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
                    ConditionBase[] terms = condition2.Terms;
                    for (int i = 0; i < terms.Length; i++)
                    {
                        InitConditions(terms[i]);
                    }
                }
            }
        }

        private IMyStatControl InitImage(MyObjectBuilder_ImageStatVisualStyle style)
        {
            MyObjectBuilder_GuiTexture texture;
            if (!MyGuiTextures.Static.TryGetTexture(style.Texture, out texture))
            {
                return null;
            }
            MyStatControlImage image1 = new MyStatControlImage(this);
            image1.Size = style.SizePx * this.m_uiScaleFactor;
            image1.Position = this.Position + (style.OffsetPx * this.m_uiScaleFactor);
            MyStatControlImage image = image1;
            image.Texture = texture;
            if (style.ColorMask != null)
            {
                image.ColorMask = style.ColorMask.Value;
            }
            return image;
        }

        private IMyStatControl InitProgressBar(MyObjectBuilder_ProgressBarStatVisualStyle style)
        {
            MyObjectBuilder_GuiTexture texture2;
            MyObjectBuilder_GuiTexture texture3;
            if (style.NineTiledStyle != null)
            {
                MyObjectBuilder_CompositeTexture texture;
                if (!MyGuiTextures.Static.TryGetCompositeTexture(style.NineTiledStyle.Value.Texture, out texture))
                {
                    return null;
                }
                MyStatControlProgressBar bar1 = new MyStatControlProgressBar(this, texture);
                bar1.Position = this.Position + (style.OffsetPx * this.m_uiScaleFactor);
                bar1.Size = style.SizePx * this.m_uiScaleFactor;
                MyStatControlProgressBar bar = bar1;
                if (style.NineTiledStyle.Value.ColorMask != null)
                {
                    bar.ColorMask = style.NineTiledStyle.Value.ColorMask.Value;
                }
                if (style.Inverted != null)
                {
                    bar.Inverted = style.Inverted.Value;
                }
                return bar;
            }
            if (style.SimpleStyle == null)
            {
                return null;
            }
            if (!MyGuiTextures.Static.TryGetTexture(style.SimpleStyle.Value.BackgroundTexture, out texture2) || !MyGuiTextures.Static.TryGetTexture(style.SimpleStyle.Value.ProgressTexture, out texture3))
            {
                return null;
            }
            MyStatControlProgressBar bar3 = new MyStatControlProgressBar(this, texture2, texture3, style.SimpleStyle.Value.ProgressTextureOffsetPx, style.SimpleStyle.Value.BackgroundColorMask, style.SimpleStyle.Value.ProgressColorMask);
            bar3.Position = this.Position + (style.OffsetPx * this.m_uiScaleFactor);
            bar3.Size = style.SizePx * this.m_uiScaleFactor;
            MyStatControlProgressBar bar2 = bar3;
            if (style.Inverted != null)
            {
                bar2.Inverted = style.Inverted.Value;
            }
            return bar2;
        }

        private void InitStatControl(IMyStatControl control, IMyHudStat stat, MyObjectBuilder_StatVisualStyle style)
        {
            if (stat != null)
            {
                control.StatMaxValue = stat.MaxValue;
                control.StatMinValue = stat.MinValue;
                control.StatCurrent = stat.CurrentValue;
                control.StatString = stat.GetValueString();
            }
            if (style.Blink != null)
            {
                control.BlinkBehavior.Blink = style.Blink.Blink;
                control.BlinkBehavior.IntervalMs = style.Blink.IntervalMs;
                control.BlinkBehavior.MinAlpha = style.Blink.MinAlpha;
                control.BlinkBehavior.MaxAlpha = style.Blink.MaxAlpha;
                if (style.Blink.ColorMask != null)
                {
                    control.BlinkBehavior.ColorMask = style.Blink.ColorMask;
                }
            }
            if (style.FadeInTimeMs != null)
            {
                control.FadeInTimeMs = style.FadeInTimeMs.Value;
            }
            if (style.FadeOutTimeMs != null)
            {
                control.FadeOutTimeMs = style.FadeOutTimeMs.Value;
            }
            if (style.MaxOnScreenTimeMs != null)
            {
                control.MaxOnScreenTimeMs = style.MaxOnScreenTimeMs.Value;
            }
            if (style.BlinkCondition != null)
            {
                InitConditions(style.BlinkCondition);
            }
            if (style.VisibleCondition != null)
            {
                InitConditions(style.VisibleCondition);
            }
            if (style.Category != null)
            {
                control.Category = style.Category.Value;
            }
            else
            {
                style.Category = 0;
            }
        }

        private IMyStatControl InitText(MyObjectBuilder_TextStatVisualStyle style)
        {
            MyStatControlText text1 = new MyStatControlText(this, style.Text);
            text1.Position = this.Position + (style.OffsetPx * this.m_uiScaleFactor);
            text1.Size = style.SizePx * this.m_uiScaleFactor;
            text1.Font = style.Font;
            text1.Scale = style.Scale * this.m_uiScaleFactor;
            MyStatControlText text = text1;
            if (style.ColorMask != null)
            {
                text.TextColorMask = style.ColorMask.Value;
            }
            if (style.TextAlign != null)
            {
                text.TextAlign = style.TextAlign.Value;
            }
            return text;
        }

        protected void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            Vector2 vector = newPosition - oldPosition;
            using (List<StatBinding>.Enumerator enumerator = this.m_bindings.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Control.Position += vector;
                }
            }
        }

        public void RegisterAlphaMultiplier(VisualStyleCategory category, float multiplier)
        {
            this.m_alphaMultipliersByCategory[category] = multiplier;
        }

        public float ChildrenScaleFactor =>
            this.m_uiScaleFactor;

        public Vector2 Position
        {
            get => 
                this.m_position;
            set
            {
                this.OnPositionChanged(this.m_position, value);
                this.m_position = value;
            }
        }

        private class StatBinding
        {
            public IMyStatControl Control;
            public IMyHudStat Stat;
            public MyObjectBuilder_StatVisualStyle Style;
            public bool LastVisibleConditionCheckResult;
        }
    }
}

