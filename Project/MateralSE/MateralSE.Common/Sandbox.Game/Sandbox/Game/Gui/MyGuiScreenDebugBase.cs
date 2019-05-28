namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenDebugBase : MyGuiScreenBase
    {
        private static VRageMath.Vector4 m_defaultColor = Color.Yellow.ToVector4();
        private static VRageMath.Vector4 m_defaultTextColor = new VRageMath.Vector4(1f, 1f, 0f, 1f);
        protected Vector2 m_currentPosition;
        protected float m_scale;
        protected float m_buttonXOffset;
        protected float m_sliderDebugScale;
        private float m_maxWidth;
        protected float Spacing;

        protected MyGuiScreenDebugBase(VRageMath.Vector4? backgroundColor = new VRageMath.Vector4?(), bool isTopMostScreen = false) : this(new Vector2(MyGuiManager.GetMaxMouseCoord().X - 0.16f, 0.5f), new Vector2(0.32f, 1f), new VRageMath.Vector4?(valueOrDefault), isTopMostScreen)
        {
            VRageMath.Vector4 valueOrDefault;
            VRageMath.Vector4? nullable = backgroundColor;
            if (nullable == null)
            {
                valueOrDefault = (VRageMath.Vector4) (0.85f * Color.Black.ToVector4());
            }
            else
            {
                valueOrDefault = nullable.GetValueOrDefault();
            }
            base.m_closeOnEsc = true;
            base.m_drawEvenWithoutFocus = true;
            base.m_isTopMostScreen = false;
            base.CanHaveFocus = false;
            base.m_isTopScreen = true;
        }

        protected MyGuiScreenDebugBase(Vector2 position, Vector2? size, VRageMath.Vector4? backgroundColor, bool isTopMostScreen) : base(new Vector2?(position), backgroundColor, size, isTopMostScreen, null, 0f, 0f)
        {
            this.m_scale = 1f;
            this.m_sliderDebugScale = 1f;
            base.CanBeHidden = false;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = false;
            base.m_canShareInput = true;
            base.m_isTopScreen = true;
        }

        public MyGuiControlButton AddButton(string text, Action<MyGuiControlButton> onClick, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? textColor = new VRageMath.Vector4?(), Vector2? size = new Vector2?()) => 
            this.AddButton(new StringBuilder(text), onClick, controlGroup, textColor, size, true, true);

        public unsafe MyGuiControlButton AddButton(StringBuilder text, Action<MyGuiControlButton> onClick, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? textColor = new VRageMath.Vector4?(), Vector2? size = new Vector2?(), bool increaseSpacing = true, bool addToControls = true)
        {
            Vector2? nullable = null;
            int? buttonIndex = null;
            MyGuiControlButton control = new MyGuiControlButton(new Vector2(this.m_buttonXOffset, this.m_currentPosition.Y), MyGuiControlButtonStyleEnum.Debug, nullable, new VRageMath.Vector4?(m_defaultColor), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, text, (0.8f * MyGuiConstants.DEBUG_BUTTON_TEXT_SCALE) * this.m_scale, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP
            };
            if (addToControls)
            {
                this.Controls.Add(control);
            }
            if (increaseSpacing)
            {
                float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
                singlePtr1[0] += (control.Size.Y + 0.01f) + this.Spacing;
            }
            if (controlGroup != null)
            {
                controlGroup.Add(control);
            }
            return control;
        }

        protected MyGuiControlCheckbox AddCheckBox(string text, bool enabled)
        {
            VRageMath.Vector4? color = null;
            Vector2? checkBoxOffset = null;
            MyGuiControlCheckbox checkbox1 = this.AddCheckBox(text, true, null, color, checkBoxOffset);
            checkbox1.IsChecked = enabled;
            return checkbox1;
        }

        protected MyGuiControlCheckbox AddCheckBox(string text, MyDebugComponent component, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?())
        {
            MyGuiControlCheckbox checkbox1 = this.AddCheckBox(text, true, controlGroup, color, checkBoxOffset);
            checkbox1.IsChecked = component.Enabled;
            checkbox1.IsCheckedChanged = delegate (MyGuiControlCheckbox sender) {
                component.Enabled = sender.IsChecked;
            };
            return checkbox1;
        }

        private unsafe MyGuiControlCheckbox AddCheckBox(string text, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?())
        {
            Vector2? size = null;
            VRageMath.Vector4? nullable3 = color;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.m_currentPosition), size, text, new VRageMath.Vector4?((nullable3 != null) ? nullable3.GetValueOrDefault() : m_defaultTextColor), (0.8f * MyGuiConstants.DEBUG_LABEL_TEXT_SCALE) * this.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            float num = control.GetTextSize().X + 0.02f;
            this.m_maxWidth = Math.Max(this.m_maxWidth, num);
            control.Enabled = enabled;
            this.Controls.Add(control);
            Vector2? nullable = base.GetSize();
            size = null;
            nullable3 = color;
            MyGuiControlCheckbox checkbox = new MyGuiControlCheckbox(size, new VRageMath.Vector4?((nullable3 != null) ? nullable3.GetValueOrDefault() : m_defaultColor), null, false, MyGuiControlCheckboxStyleEnum.Debug, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            size = checkBoxOffset;
            checkbox.Position = (this.m_currentPosition + new Vector2(nullable.Value.X - checkbox.Size.X, 0f)) + ((size != null) ? size.GetValueOrDefault() : Vector2.Zero);
            checkbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            checkbox.Enabled = enabled;
            this.Controls.Add(checkbox);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += Math.Max(checkbox.Size.Y, control.Size.Y) + this.Spacing;
            if (controlGroup != null)
            {
                controlGroup.Add(control);
                controlGroup.Add(checkbox);
            }
            return checkbox;
        }

        protected MyGuiControlCheckbox AddCheckBox(string text, bool checkedState, Action<MyGuiControlCheckbox> checkBoxChange, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?())
        {
            MyGuiControlCheckbox checkbox = this.AddCheckBox(text, enabled, controlGroup, color, checkBoxOffset);
            checkbox.IsChecked = checkedState;
            if (checkBoxChange != null)
            {
                checkbox.IsCheckedChanged = delegate (MyGuiControlCheckbox sender) {
                    checkBoxChange(sender);
                    this.ValueChanged(sender);
                };
            }
            return checkbox;
        }

        protected MyGuiControlCheckbox AddCheckBox(string text, Func<bool> getter, Action<bool> setter, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?())
        {
            MyGuiControlCheckbox checkbox = this.AddCheckBox(text, enabled, controlGroup, color, checkBoxOffset);
            if (getter != null)
            {
                checkbox.IsChecked = getter();
            }
            if (setter != null)
            {
                checkbox.IsCheckedChanged = delegate (MyGuiControlCheckbox sender) {
                    setter(sender.IsChecked);
                    this.ValueChanged(sender);
                };
            }
            return checkbox;
        }

        protected MyGuiControlCheckbox AddCheckBox(string text, object instance, MemberInfo memberInfo, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?())
        {
            MyGuiControlCheckbox checkbox = this.AddCheckBox(text, enabled, controlGroup, color, checkBoxOffset);
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo info = (PropertyInfo) memberInfo;
                checkbox.IsChecked = (bool) info.GetValue(instance, new object[0]);
                checkbox.UserData = new Tuple<object, PropertyInfo>(instance, info);
                checkbox.IsCheckedChanged = delegate (MyGuiControlCheckbox sender) {
                    Tuple<object, PropertyInfo> userData = sender.UserData as Tuple<object, PropertyInfo>;
                    userData.Item2.SetValue(userData.Item1, sender.IsChecked, new object[0]);
                    this.ValueChanged(sender);
                };
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo info2 = (FieldInfo) memberInfo;
                checkbox.IsChecked = (bool) info2.GetValue(instance);
                checkbox.UserData = new Tuple<object, FieldInfo>(instance, info2);
                checkbox.IsCheckedChanged = delegate (MyGuiControlCheckbox sender) {
                    Tuple<object, FieldInfo> userData = sender.UserData as Tuple<object, FieldInfo>;
                    userData.Item2.SetValue(userData.Item1, sender.IsChecked);
                    this.ValueChanged(sender);
                };
            }
            return checkbox;
        }

        protected MyGuiControlCheckbox AddCheckBox(MyStringId textEnum, bool checkedState, Action<MyGuiControlCheckbox> checkBoxChange, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?()) => 
            this.AddCheckBox(MyTexts.GetString(textEnum), checkedState, checkBoxChange, enabled, controlGroup, color, checkBoxOffset);

        protected MyGuiControlCheckbox AddCheckBox(MyStringId textEnum, Func<bool> getter, Action<bool> setter, bool enabled = true, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?(), Vector2? checkBoxOffset = new Vector2?()) => 
            this.AddCheckBox(MyTexts.GetString(textEnum), getter, setter, enabled, controlGroup, color, checkBoxOffset);

        private unsafe MyGuiControlColor AddColor(string text)
        {
            MyGuiControlColor control = new MyGuiControlColor(text, this.m_scale, this.m_currentPosition, Color.White, Color.White, MyCommonTexts.DialogAmount_AddAmountCaption, false, "Debug") {
                ColorMask = Color.Yellow.ToVector4(),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            this.Controls.Add(control);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += control.Size.Y;
            return control;
        }

        protected MyGuiControlColor AddColor(string text, Func<Color> getter, Action<Color> setter) => 
            this.AddColor(text, getter(), delegate (MyGuiControlColor c) {
                setter(c.GetColor());
            });

        protected MyGuiControlColor AddColor(string text, object instance, MemberInfo memberInfo)
        {
            MyGuiControlColor color = this.AddColor(text);
            switch (memberInfo)
            {
                case (PropertyInfo _):
                {
                    PropertyInfo info = (PropertyInfo) memberInfo;
                    object obj2 = info.GetValue(instance, new object[0]);
                    if (obj2 is Color)
                    {
                        color.SetColor((Color) obj2);
                    }
                    else if (obj2 is Vector3)
                    {
                        color.SetColor((Vector3) obj2);
                    }
                    else if (obj2 is VRageMath.Vector4)
                    {
                        color.SetColor((VRageMath.Vector4) obj2);
                    }
                    color.UserData = new Tuple<object, PropertyInfo>(instance, info);
                    color.OnChange += delegate (MyGuiControlColor sender) {
                        Tuple<object, PropertyInfo> userData = sender.UserData as Tuple<object, PropertyInfo>;
                        if (userData.Item2.MemberType.GetType() == typeof(Color))
                        {
                            userData.Item2.SetValue(userData.Item1, sender.GetColor(), new object[0]);
                            this.ValueChanged(sender);
                        }
                        else if (userData.Item2.MemberType.GetType() == typeof(Vector3))
                        {
                            userData.Item2.SetValue(userData.Item1, sender.GetColor().ToVector3(), new object[0]);
                            this.ValueChanged(sender);
                        }
                        else if (userData.Item2.MemberType.GetType() == typeof(VRageMath.Vector4))
                        {
                            userData.Item2.SetValue(userData.Item1, sender.GetColor().ToVector4(), new object[0]);
                            this.ValueChanged(sender);
                        }
                    };
                    break;
                }
                default:
                    if (memberInfo is FieldInfo)
                    {
                        FieldInfo info2 = (FieldInfo) memberInfo;
                        object obj3 = info2.GetValue(instance);
                        if (obj3 is Color)
                        {
                            color.SetColor((Color) obj3);
                        }
                        else if (obj3 is Vector3)
                        {
                            color.SetColor((Vector3) obj3);
                        }
                        else if (obj3 is VRageMath.Vector4)
                        {
                            color.SetColor((VRageMath.Vector4) obj3);
                        }
                        color.UserData = new Tuple<object, FieldInfo>(instance, info2);
                        color.OnChange += delegate (MyGuiControlColor sender) {
                            Tuple<object, FieldInfo> userData = sender.UserData as Tuple<object, FieldInfo>;
                            if (userData.Item2.FieldType == typeof(Color))
                            {
                                userData.Item2.SetValue(userData.Item1, sender.GetColor());
                                this.ValueChanged(sender);
                            }
                            else if (userData.Item2.FieldType == typeof(Vector3))
                            {
                                userData.Item2.SetValue(userData.Item1, sender.GetColor().ToVector3());
                                this.ValueChanged(sender);
                            }
                            else if (userData.Item2.FieldType == typeof(VRageMath.Vector4))
                            {
                                userData.Item2.SetValue(userData.Item1, sender.GetColor().ToVector4());
                                this.ValueChanged(sender);
                            }
                        };
                    }
                    break;
            }
            return color;
        }

        protected MyGuiControlColor AddColor(string text, Color value, Action<MyGuiControlColor> setter)
        {
            MyGuiControlColor colorControl = this.AddColor(text);
            colorControl.SetColor(value);
            colorControl.OnChange += delegate (MyGuiControlColor sender) {
                setter(colorControl);
            };
            return colorControl;
        }

        protected unsafe MyGuiControlCombobox AddCombo(List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? textColor = new VRageMath.Vector4?(), Vector2? size = new Vector2?(), int openAreaItemsCount = 10)
        {
            VRageMath.Vector4? backgroundColor = null;
            Vector2? textOffset = null;
            VRageMath.Vector4? nullable = textColor;
            textOffset = null;
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox(new Vector2?(this.m_currentPosition), size, backgroundColor, textOffset, openAreaItemsCount, textOffset, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable);
            combobox1.VisualStyle = MyGuiControlComboboxStyleEnum.Debug;
            combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlCombobox control = combobox1;
            this.Controls.Add(control);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += (control.Size.Y + 0.01f) + this.Spacing;
            if (controlGroup != null)
            {
                controlGroup.Add(control);
            }
            return control;
        }

        protected MyGuiControlCombobox AddCombo<TEnum>(TEnum selectedItem, Action<TEnum> valueChanged, bool enabled = true, int openAreaItemsCount = 10, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?()) where TEnum: struct, IComparable, IFormattable, IConvertible
        {
            Vector2? size = null;
            MyGuiControlCombobox combobox = this.AddCombo(controlGroup, color, size, openAreaItemsCount);
            foreach (TEnum local in MyEnum<TEnum>.Values)
            {
                int? sortOrder = null;
                combobox.AddItem((long) ((int) local), new StringBuilder(MyTexts.TrySubstitute(local.ToString())), sortOrder, null);
            }
            combobox.SelectItemByKey((long) ((int) selectedItem), true);
            combobox.ItemSelected += delegate {
                valueChanged(MyEnum<TEnum>.SetValue((ulong) combobox.GetSelectedKey()));
            };
            return combobox;
        }

        protected MyGuiControlCombobox AddCombo<TEnum>(object instance, MemberInfo memberInfo, bool enabled = true, int openAreaItemsCount = 10, List<MyGuiControlBase> controlGroup = null, VRageMath.Vector4? color = new VRageMath.Vector4?()) where TEnum: struct, IComparable, IFormattable, IConvertible
        {
            Vector2? size = null;
            MyGuiControlCombobox combobox = this.AddCombo(controlGroup, color, size, openAreaItemsCount);
            foreach (TEnum local in MyEnum<TEnum>.Values)
            {
                int? sortOrder = null;
                combobox.AddItem((long) ((int) local), new StringBuilder(local.ToString()), sortOrder, null);
            }
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo property = memberInfo as PropertyInfo;
                combobox.SelectItemByKey((long) ((int) property.GetValue(instance, new object[0])), true);
                combobox.ItemSelected += delegate {
                    property.SetValue(instance, System.Enum.ToObject(typeof(TEnum), combobox.GetSelectedKey()), new object[0]);
                };
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo field = memberInfo as FieldInfo;
                combobox.SelectItemByKey((long) ((int) field.GetValue(instance)), true);
                combobox.ItemSelected += delegate {
                    field.SetValue(instance, System.Enum.ToObject(typeof(TEnum), combobox.GetSelectedKey()));
                };
            }
            return combobox;
        }

        protected unsafe MyGuiControlLabel AddLabel(string text, VRageMath.Vector4 color, float scale, List<MyGuiControlBase> controlGroup = null, string font = "Debug")
        {
            Vector2? size = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.m_currentPosition), size, text, new VRageMath.Vector4?(color), ((0.8f * MyGuiConstants.DEBUG_LABEL_TEXT_SCALE) * scale) * this.m_scale, font, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            float num = control.GetTextSize().X + 0.02f;
            this.m_maxWidth = Math.Max(this.m_maxWidth, num);
            this.Controls.Add(control);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += control.Size.Y + this.Spacing;
            if (controlGroup != null)
            {
                controlGroup.Add(control);
            }
            return control;
        }

        protected unsafe MyGuiControlListbox AddListBox(float verticalSize, List<MyGuiControlBase> controlGroup = null)
        {
            MyGuiControlListbox listbox1 = new MyGuiControlListbox(new Vector2?(this.m_currentPosition), MyGuiControlListboxStyleEnum.Default);
            listbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiControlListbox control = listbox1;
            control.Size = new Vector2(control.Size.X, verticalSize);
            this.Controls.Add(control);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += (control.Size.Y + 0.01f) + this.Spacing;
            if (controlGroup != null)
            {
                controlGroup.Add(control);
            }
            return control;
        }

        protected MyGuiControlMultilineText AddMultilineText(Vector2? size = new Vector2?(), Vector2? offset = new Vector2?(), float textScale = 1f, bool selectable = false)
        {
            Vector2 valueOrDefault;
            Vector2? nullable = size;
            if (nullable != null)
            {
                valueOrDefault = nullable.GetValueOrDefault();
            }
            else
            {
                Vector2? nullable2 = base.Size;
                valueOrDefault = (nullable2 != null) ? nullable2.GetValueOrDefault() : new Vector2(0.5f, 0.5f);
            }
            Vector2 vector = valueOrDefault;
            nullable = offset;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText control = new MyGuiControlMultilineText(new Vector2?((this.m_currentPosition + (vector / 2f)) + ((nullable != null) ? nullable.GetValueOrDefault() : Vector2.Zero)), new Vector2?(vector), new VRageMath.Vector4?(m_defaultColor), "Debug", this.m_scale * textScale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, selectable, false, null, textPadding);
            this.Controls.Add(control);
            return control;
        }

        protected void AddShareFocusHint()
        {
            Vector2? size = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2(0.01f, (-base.m_size.Value.Y / 2f) + 0.07f), size, "(press ALT to share focus)", new VRageMath.Vector4?(Color.Yellow.ToVector4()), 0.56f, "Debug", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
            this.Controls.Add(control);
        }

        private unsafe MyGuiControlSlider AddSlider(string text, float valueMin, float valueMax, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            float width = 460f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float? defaultValue = null;
            VRageMath.Vector4? nullable2 = null;
            MyGuiControlSlider control = new MyGuiControlSlider(new Vector2?(this.m_currentPosition), valueMin, valueMax, width, defaultValue, nullable2, new StringBuilder(" {0}").ToString(), 3, 0.75f * this.m_scale, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, true) {
                DebugScale = this.m_sliderDebugScale
            };
            nullable2 = color;
            control.ColorMask = (nullable2 != null) ? nullable2.GetValueOrDefault() : m_defaultColor;
            this.Controls.Add(control);
            Vector2? size = null;
            nullable2 = color;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(this.m_currentPosition + new Vector2(0.015f, 0f)), size, text, new VRageMath.Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : m_defaultTextColor), 0.64f * this.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            float num = label.GetTextSize().X + 0.02f;
            this.m_maxWidth = Math.Max(this.m_maxWidth, num);
            this.Controls.Add(label);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += control.Size.Y + this.Spacing;
            return control;
        }

        protected MyGuiControlSliderBase AddSlider(string text, MyGuiSliderProperties properties, Func<float> getter, Action<float> setter, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            MyGuiControlSliderBase base1 = this.AddSliderBase(text, properties, color);
            base1.Value = getter();
            base1.UserData = setter;
            base1.ValueChanged = delegate (MyGuiControlSliderBase sender) {
                ((Action<float>) sender.UserData)(sender.Value);
                this.ValueChanged(sender);
            };
            return base1;
        }

        protected MyGuiControlSlider AddSlider(string text, float value, float valueMin, float valueMax, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            MyGuiControlSlider slider1 = this.AddSlider(text, valueMin, valueMax, color);
            slider1.Value = value;
            return slider1;
        }

        protected MyGuiControlSlider AddSlider(string text, float valueMin, float valueMax, Func<float> getter, Action<float> setter, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            MyGuiControlSlider slider1 = this.AddSlider(text, valueMin, valueMax, color);
            slider1.Value = getter();
            slider1.UserData = setter;
            slider1.ValueChanged = delegate (MyGuiControlSlider sender) {
                ((Action<float>) sender.UserData)(sender.Value);
                this.ValueChanged(sender);
            };
            return slider1;
        }

        protected MyGuiControlSlider AddSlider(string text, float valueMin, float valueMax, object instance, MemberInfo memberInfo, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            MyGuiControlSlider slider = this.AddSlider(text, valueMin, valueMax, color);
            if (memberInfo is PropertyInfo)
            {
                PropertyInfo info = (PropertyInfo) memberInfo;
                slider.Value = (float) info.GetValue(instance, new object[0]);
                slider.UserData = new Tuple<object, PropertyInfo>(instance, info);
                slider.ValueChanged = delegate (MyGuiControlSlider sender) {
                    Tuple<object, PropertyInfo> userData = sender.UserData as Tuple<object, PropertyInfo>;
                    userData.Item2.SetValue(userData.Item1, sender.Value, new object[0]);
                    this.ValueChanged(sender);
                };
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo info2 = (FieldInfo) memberInfo;
                slider.Value = (float) info2.GetValue(instance);
                slider.UserData = new Tuple<object, FieldInfo>(instance, info2);
                slider.ValueChanged = delegate (MyGuiControlSlider sender) {
                    Tuple<object, FieldInfo> userData = sender.UserData as Tuple<object, FieldInfo>;
                    userData.Item2.SetValue(userData.Item1, sender.Value);
                    this.ValueChanged(sender);
                };
            }
            return slider;
        }

        protected MyGuiControlSlider AddSlider(string text, float value, float valueMin, float valueMax, Action<MyGuiControlSlider> valueChange, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            MyGuiControlSlider slider1 = this.AddSlider(text, valueMin, valueMax, color);
            slider1.Value = value;
            slider1.ValueChanged = valueChange;
            slider1.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(slider1.ValueChanged, new Action<MyGuiControlSlider>(this.ValueChanged));
            return slider1;
        }

        private unsafe MyGuiControlSliderBase AddSliderBase(string text, MyGuiSliderProperties props, VRageMath.Vector4? color = new VRageMath.Vector4?())
        {
            float? defaultRatio = null;
            VRageMath.Vector4? nullable2 = null;
            MyGuiControlSliderBase control = new MyGuiControlSliderBase(new Vector2?(this.m_currentPosition), 460f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, props, defaultRatio, nullable2, 0.75f * this.m_scale, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true) {
                DebugScale = this.m_sliderDebugScale
            };
            nullable2 = color;
            control.ColorMask = (nullable2 != null) ? nullable2.GetValueOrDefault() : m_defaultColor;
            this.Controls.Add(control);
            Vector2? size = null;
            nullable2 = color;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(this.m_currentPosition + new Vector2(0.015f, 0f)), size, text, new VRageMath.Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : m_defaultTextColor), 0.64f * this.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            float num = label.GetTextSize().X + 0.02f;
            this.m_maxWidth = Math.Max(this.m_maxWidth, num);
            this.Controls.Add(label);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += control.Size.Y + this.Spacing;
            return control;
        }

        protected unsafe MyGuiControlLabel AddSubcaption(string text, VRageMath.Vector4? captionTextColor = new VRageMath.Vector4?(), Vector2? captionOffset = new Vector2?(), float captionScale = 0.8f)
        {
            float num = (base.m_size == null) ? 0f : (base.m_size.Value.X / 2f);
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += MyGuiConstants.SCREEN_CAPTION_DELTA_Y;
            float* singlePtr2 = (float*) ref this.m_currentPosition.X;
            singlePtr2[0] += num;
            Vector2? size = null;
            VRageMath.Vector4? nullable2 = captionTextColor;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(this.m_currentPosition + ((captionOffset != null) ? captionOffset.Value : Vector2.Zero)), size, text, new VRageMath.Vector4?((nullable2 != null) ? nullable2.GetValueOrDefault() : m_defaultColor), captionScale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            base.Elements.Add(control);
            float* singlePtr3 = (float*) ref this.m_currentPosition.Y;
            singlePtr3[0] += MyGuiConstants.SCREEN_CAPTION_DELTA_Y + this.Spacing;
            float* singlePtr4 = (float*) ref this.m_currentPosition.X;
            singlePtr4[0] -= num;
            return control;
        }

        protected MyGuiControlLabel AddSubcaption(MyStringId textEnum, VRageMath.Vector4? captionTextColor = new VRageMath.Vector4?(), Vector2? captionOffset = new Vector2?(), float captionScale = 0.8f) => 
            this.AddSubcaption(MyTexts.GetString(textEnum), captionTextColor, captionOffset, captionScale);

        protected unsafe MyGuiControlTextbox AddTextbox(string value, Action<MyGuiControlTextbox> onTextChanged, VRageMath.Vector4? color = new VRageMath.Vector4?(), float scale = 1f, MyGuiControlTextboxType type = 0, List<MyGuiControlBase> controlGroup = null, string font = "Debug", MyGuiDrawAlignEnum originAlign = 6, bool addToControls = true)
        {
            MyGuiControlTextbox control = new MyGuiControlTextbox(new Vector2?(this.m_currentPosition), value, 6, color, scale, type, MyGuiControlTextboxStyleEnum.Default) {
                OriginAlign = originAlign
            };
            if (onTextChanged != null)
            {
                control.EnterPressed += onTextChanged;
            }
            if (addToControls)
            {
                this.Controls.Add(control);
            }
            float* singlePtr1 = (float*) ref this.m_currentPosition.Y;
            singlePtr1[0] += (control.Size.Y + 0.01f) + this.Spacing;
            if (controlGroup != null)
            {
                controlGroup.Add(control);
            }
            return control;
        }

        public override bool Draw() => 
            (MyGuiSandbox.IsDebugScreenEnabled() ? base.Draw() : false);

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugBase";

        protected virtual void ValueChanged(MyGuiControlBase sender)
        {
        }
    }
}

