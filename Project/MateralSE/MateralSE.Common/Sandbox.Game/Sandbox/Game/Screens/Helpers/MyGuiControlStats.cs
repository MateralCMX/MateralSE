namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlStats : MyGuiControlBase
    {
        private MyCharacterStatComponent m_statComponent;
        private Dictionary<MyStringHash, MyGuiControlStat> m_statControls;
        private List<MyEntityStat> m_sortedStats;

        public MyGuiControlStats() : base(nullable, nullable, nullable2, null, new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_STATS_BG.Texture), true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_sortedStats = new List<MyEntityStat>();
            Vector2? nullable = null;
            nullable = null;
        }

        public void ClearPotentialStatChange(MyDefinitionId consumableId)
        {
            MyConsumableItemDefinition definition = MyDefinitionManager.Static.GetDefinition(consumableId) as MyConsumableItemDefinition;
            if (definition != null)
            {
                foreach (MyConsumableItemDefinition.StatValue value2 in definition.Stats)
                {
                    this.SetPotentialStatChange(value2.Name, 0f);
                }
            }
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if ((MyFakes.ENABLE_STATS_GUI && (!MySession.Static.CreativeMode && (this.m_statControls != null))) && (this.m_statControls.Count != 0))
            {
                base.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        private void RecreateControls()
        {
            base.Elements.Clear();
            List<MyEntityStat> sortedStats = this.m_sortedStats;
            if (sortedStats.Count != 0)
            {
                base.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                if (base.Position == Vector2.Zero)
                {
                    Vector2 hudPos = new Vector2(0.025f, 0.016f);
                    base.Position = MyGuiScreenHudBase.ConvertHudToNormalizedGuiPosition(ref hudPos);
                }
                base.ColorMask = new Vector4(base.ColorMask.X, base.ColorMask.Y, base.ColorMask.Z, 0.75f);
                float num = 0f;
                foreach (MyEntityStat stat in sortedStats)
                {
                    num += stat.StatDefinition.GuiDef.HeightMultiplier;
                }
                float num2 = 0.005f;
                Vector2 paddingSizeGui = MyGuiConstants.TEXTURE_HUD_STATS_BG.PaddingSizeGui;
                float num3 = 0.025f - (2f * paddingSizeGui.Y);
                float num4 = num3 / 4f;
                base.Size = new Vector2(0.191f, ((4f * num2) + (num3 * num)) + ((sortedStats.Count - 1) * num4));
                this.m_statControls = new Dictionary<MyStringHash, MyGuiControlStat>();
                float x = base.Size.X - (2f * paddingSizeGui.X);
                float y = (-base.Size.Y / 2f) + num2;
                foreach (MyEntityStat stat2 in sortedStats)
                {
                    MyGuiControlStat stat3 = new MyGuiControlStat(stat2, new Vector2(0f, y) + paddingSizeGui, new Vector2(x, stat2.StatDefinition.GuiDef.HeightMultiplier * num3), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
                    this.m_statControls.Add(stat2.StatId, stat3);
                    base.Elements.Add(stat3);
                    stat3.RecreateControls();
                    y += stat3.Size.Y + num4;
                }
            }
        }

        public void SetPotentialStatChange(MyDefinitionId consumableId)
        {
            MyConsumableItemDefinition definition = MyDefinitionManager.Static.GetDefinition(consumableId) as MyConsumableItemDefinition;
            if (definition != null)
            {
                foreach (MyConsumableItemDefinition.StatValue value2 in definition.Stats)
                {
                    this.SetPotentialStatChange(value2.Name, value2.Value * value2.Time);
                }
            }
        }

        private void SetPotentialStatChange(string id, float value)
        {
            MyGuiControlStat stat;
            MyStringHash key = MyStringHash.Get(id);
            if (this.m_statControls.TryGetValue(key, out stat))
            {
                stat.PotentialChange = value;
            }
        }

        public override void Update()
        {
            base.Update();
            MyCharacterStatComponent objA = null;
            if (MySession.Static.LocalCharacter != null)
            {
                objA = MySession.Static.LocalCharacter.StatComp;
            }
            if (((objA != null) && !ReferenceEquals(objA, this.m_statComponent)) && (objA.Stats.Count > 0))
            {
                this.m_statComponent = objA;
                this.m_sortedStats.Clear();
                if (this.m_statComponent != null)
                {
                    foreach (MyEntityStat stat in this.m_statComponent.Stats)
                    {
                        this.m_sortedStats.Add(stat);
                    }
                    this.m_sortedStats.Sort((leftStat, rightStat) => rightStat.StatDefinition.GuiDef.Priority - leftStat.StatDefinition.GuiDef.Priority);
                }
                this.RecreateControls();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlStats.<>c <>9 = new MyGuiControlStats.<>c();
            public static Comparison<MyEntityStat> <>9__5_0;

            internal int <Update>b__5_0(MyEntityStat leftStat, MyEntityStat rightStat) => 
                (rightStat.StatDefinition.GuiDef.Priority - leftStat.StatDefinition.GuiDef.Priority);
        }

        public class MyGuiControlStat : MyGuiControlBase
        {
            private MyEntityStat m_stat;
            private MyGuiControlLabel m_statNameLabel;
            private MyGuiControlPanel m_progressBarBorder;
            private MyGuiControlPanel m_progressBarDivider;
            private MyGuiControlProgressBar m_progressBar;
            private MyGuiControlPanel m_effectArrow;
            private MyGuiControlLabel m_statValueLabel;
            private Color m_criticalValueColorFrom;
            private Color m_criticalValueColorTo;
            private static MyGuiCompositeTexture m_arrowUp = new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_STAT_EFFECT_ARROW_UP.Texture);
            private static MyGuiCompositeTexture m_arrowDown = new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_STAT_EFFECT_ARROW_DOWN.Texture);
            private float m_lastTotalValue;
            private float m_potentialChange;
            private float m_flashingProgress;
            private int m_lastFlashTime;
            private bool m_recalculatePotential;

            public MyGuiControlStat(MyEntityStat stat, Vector2 position, Vector2 size, MyGuiDrawAlignEnum originAlign = 4) : base(new Vector2?(position), new Vector2?(size), nullable, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
            {
                this.m_lastFlashTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_stat = stat;
                Vector3I criticalColorFrom = this.m_stat.StatDefinition.GuiDef.CriticalColorFrom;
                this.m_criticalValueColorFrom = new Color(criticalColorFrom.X, criticalColorFrom.Y, criticalColorFrom.Z);
                criticalColorFrom = this.m_stat.StatDefinition.GuiDef.CriticalColorTo;
                this.m_criticalValueColorTo = new Color(criticalColorFrom.X, criticalColorFrom.Y, criticalColorFrom.Z);
                if (this.m_stat != null)
                {
                    this.m_stat.OnStatChanged += new MyEntityStat.StatChangedDelegate(this.UpdateStatControl);
                }
            }

            public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
            {
                if (this.m_recalculatePotential)
                {
                    this.RecalculatePotentialBar();
                }
                base.Draw(transitionAlpha, backgroundTransitionAlpha);
            }

            public override void OnRemoving()
            {
                if (this.m_stat != null)
                {
                    this.m_stat.OnStatChanged -= new MyEntityStat.StatChangedDelegate(this.UpdateStatControl);
                }
                base.OnRemoving();
            }

            private void RecalculatePotentialBar()
            {
                if (this.m_progressBar.PotentialBar.Visible)
                {
                    this.RecalculateStatRegenLeft();
                    float num = 1.01f / ((float) MyGuiManager.GetFullscreenRectangle().Height);
                    float num2 = 1.01f / ((float) MyGuiManager.GetFullscreenRectangle().Height);
                    this.m_progressBar.PotentialBar.Size = new Vector2((this.m_progressBar.Size.X * MathHelper.Clamp((float) (((this.m_stat.StatRegenLeft + this.m_stat.Value) + this.m_potentialChange) / this.m_stat.MaxValue), (float) 0f, (float) 1f)) - num, this.m_progressBar.Size.Y - (2f * num2));
                }
            }

            private void RecalculateStatRegenLeft()
            {
                if (Sync.IsServer)
                {
                    this.m_stat.CalculateRegenLeftForLongestEffect();
                }
            }

            public void RecreateControls()
            {
                base.Elements.Clear();
                float num2 = ((float) Math.Pow(1.2999999523162842, (double) (this.m_stat.StatDefinition.GuiDef.HeightMultiplier - 1f))) / this.m_stat.StatDefinition.GuiDef.HeightMultiplier;
                float textScale = 0.48f * ((float) Math.Pow(1.2000000476837158, (double) (this.m_stat.StatDefinition.GuiDef.HeightMultiplier - 1f)));
                float x = 0.0875f;
                Vector2 vector = (new Vector2(base.Size.Y * 1.5f, base.Size.Y) * 0.5f) * num2;
                float num5 = 0.16f;
                float y = -0.1f;
                float num7 = -0.5f + num5;
                float num8 = num7 + 0.025f;
                float num9 = (num8 + (x / base.Size.X)) + 0.05f;
                float num10 = (num9 + vector.X) + 0.035f;
                MyEntityStatDefinition.GuiDefinition guiDef = this.m_stat.StatDefinition.GuiDef;
                string text = this.m_stat.StatId.ToString();
                Vector4? colorMask = null;
                this.m_statNameLabel = new MyGuiControlLabel(new Vector2?(base.Size * new Vector2(num7, y)), new Vector2(num5 * base.Size.X, 1f), text, colorMask, textScale, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                base.Elements.Add(this.m_statNameLabel);
                Vector3I vectori = this.m_stat.StatDefinition.GuiDef.Color;
                Color color = new Color(vectori.X, vectori.Y, vectori.Z);
                MyGuiCompositeTexture backgroundTexture = new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_STAT_BAR_BG.Texture);
                this.m_progressBar = new MyGuiControlProgressBar(new Vector2?(base.Size * new Vector2(num8, 0f)), new Vector2(x, base.Size.Y), new Color?(color), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, backgroundTexture, true, true, true, 0.01f);
                if (this.m_stat != null)
                {
                    this.m_progressBar.Value = this.m_stat.CurrentRatio;
                }
                this.m_progressBar.ForegroundBar.BorderColor = (Vector4) Color.Black;
                this.m_progressBar.ForegroundBar.BorderEnabled = true;
                this.m_progressBar.ForegroundBar.BorderSize = 1;
                this.m_progressBar.PotentialBar.Position = this.m_progressBar.ForegroundBar.Position;
                this.m_recalculatePotential = true;
                base.Elements.Add(this.m_progressBar);
                colorMask = null;
                this.m_progressBarDivider = new MyGuiControlPanel(new Vector2?(base.Size * new Vector2(num8, 0f)), new Vector2(x * guiDef.CriticalRatio, base.Size.Y), colorMask, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_progressBarDivider.Visible = guiDef.DisplayCriticalDivider;
                this.m_progressBarDivider.BorderColor = (Vector4) Color.Black;
                this.m_progressBarDivider.BorderSize = 1;
                this.m_progressBarDivider.BorderEnabled = true;
                base.Elements.Add(this.m_progressBarDivider);
                colorMask = null;
                this.m_progressBarBorder = new MyGuiControlPanel(new Vector2?(base.Size * new Vector2(num8, 0f)), new Vector2(x, base.Size.Y), colorMask, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_progressBarBorder.Visible = false;
                this.m_progressBarBorder.BorderColor = (Vector4) Color.Black;
                this.m_progressBarBorder.BorderSize = 2;
                this.m_progressBarBorder.BorderEnabled = true;
                base.Elements.Add(this.m_progressBarBorder);
                colorMask = null;
                this.m_effectArrow = new MyGuiControlPanel(new Vector2?(base.Size * new Vector2(num9, 0f)), new Vector2?(vector), colorMask, MyGuiConstants.TEXTURE_HUD_STAT_EFFECT_ARROW_UP.Texture, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                base.Elements.Add(this.m_effectArrow);
                StringBuilder sb = new StringBuilder();
                sb.AppendDecimal((float) ((int) this.m_stat.Value), 0);
                sb.Append("/");
                sb.AppendDecimal(this.m_stat.MaxValue, 0);
                Vector2? size = null;
                colorMask = null;
                this.m_statValueLabel = new MyGuiControlLabel(new Vector2?(base.Size * new Vector2(num10, y)), size, sb.ToString(), colorMask, textScale, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                base.Elements.Add(this.m_statValueLabel);
            }

            public override void Update()
            {
                base.Update();
                if (this.m_stat != null)
                {
                    float potentialChange = this.m_potentialChange;
                    foreach (KeyValuePair<int, MyEntityStatRegenEffect> pair in this.m_stat.GetEffects())
                    {
                        if (pair.Value.Duration >= 0f)
                        {
                            potentialChange += pair.Value.Amount;
                        }
                    }
                    if (potentialChange < 0f)
                    {
                        this.m_effectArrow.Visible = true;
                        this.m_effectArrow.BackgroundTexture = m_arrowDown;
                        this.m_progressBar.PotentialBar.Visible = false;
                    }
                    else if (potentialChange <= 0f)
                    {
                        this.m_effectArrow.Visible = false;
                        this.m_progressBar.PotentialBar.Visible = false;
                    }
                    else
                    {
                        this.m_effectArrow.Visible = true;
                        this.m_effectArrow.BackgroundTexture = m_arrowUp;
                        if ((this.m_stat.MaxValue != 0f) && (!this.m_progressBar.PotentialBar.Visible || (this.m_lastTotalValue != potentialChange)))
                        {
                            this.m_progressBar.PotentialBar.Visible = true;
                            this.m_recalculatePotential = true;
                        }
                    }
                    this.m_lastTotalValue = potentialChange;
                    if (this.m_stat.CurrentRatio > this.m_stat.StatDefinition.GuiDef.CriticalRatio)
                    {
                        this.m_progressBarBorder.Visible = false;
                    }
                    else
                    {
                        this.m_flashingProgress = (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastFlashTime) * 0.001f;
                        this.m_progressBarBorder.Visible = true;
                        this.m_progressBarBorder.BorderColor = Vector4.Lerp((Vector4) this.m_criticalValueColorFrom, (Vector4) this.m_criticalValueColorTo, this.m_flashingProgress);
                        if (this.m_flashingProgress >= 1f)
                        {
                            this.m_lastFlashTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                        }
                    }
                }
            }

            private void UpdateStatControl(float newValue, float oldValue, object statChangeData)
            {
                this.m_progressBar.Value = this.m_stat.CurrentRatio;
                if (this.m_statValueLabel != null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendDecimal((float) ((int) this.m_stat.Value), 0);
                    sb.Append("/");
                    sb.AppendDecimal(this.m_stat.MaxValue, 0);
                    this.m_statValueLabel.Text = sb.ToString();
                }
                this.m_recalculatePotential = true;
            }

            public float PotentialChange
            {
                get => 
                    this.m_potentialChange;
                set
                {
                    this.m_potentialChange = value;
                    this.m_progressBar.PotentialBar.Visible = !(value == 0f);
                    this.m_recalculatePotential = !(value == 0f);
                }
            }
        }
    }
}

