namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyDebugScreen("Game", "Audio")]
    internal class MyGuiScreenDebugAudio : MyGuiScreenDebugBase
    {
        private const string ALL_CATEGORIES = "_ALL_CATEGORIES_";
        private MyGuiControlCombobox m_categoriesCombo;
        private MyGuiControlCombobox m_cuesCombo;
        private static string m_currentCategorySelectedItem;
        private static int m_currentCueSelectedItem;
        private bool m_canUpdateValues;
        private IMySourceVoice m_sound;
        private MySoundData m_currentCue;
        private MyGuiControlSlider m_cueVolumeSlider;
        private MyGuiControlCombobox m_cueVolumeCurveCombo;
        private MyGuiControlSlider m_cueMaxDistanceSlider;
        private MyGuiControlSlider m_cueVolumeVariationSlider;
        private MyGuiControlSlider m_cuePitchVariationSlider;
        private MyGuiControlCheckbox m_soloCheckbox;
        private MyGuiControlButton m_applyVolumeToCategory;
        private MyGuiControlButton m_applyMaxDistanceToCategory;
        private MyGuiControlCombobox m_effects;
        private List<MyGuiControlCombobox> m_cues;
        private List<MyCueId> m_cueCache;

        public MyGuiScreenDebugAudio() : base(nullable, false)
        {
            this.m_canUpdateValues = true;
            this.m_cues = new List<MyGuiControlCombobox>();
            this.m_cueCache = new List<MyCueId>();
            this.RecreateControls(true);
        }

        private void categoriesCombo_OnSelect()
        {
            m_currentCategorySelectedItem = this.m_categoriesCombo.GetSelectedValue().ToString();
            this.m_applyVolumeToCategory.Enabled = m_currentCategorySelectedItem != "_ALL_CATEGORIES_";
            this.m_applyMaxDistanceToCategory.Enabled = m_currentCategorySelectedItem != "_ALL_CATEGORIES_";
            this.UpdateCuesCombo(this.m_cuesCombo);
            foreach (MyGuiControlCombobox combobox in this.m_cues)
            {
                this.UpdateCuesCombo(combobox);
            }
        }

        private void cuesCombo_OnSelect()
        {
            m_currentCueSelectedItem = (int) this.m_cuesCombo.GetSelectedKey();
            MyCueId cue = new MyCueId(MyStringHash.TryGet(this.m_cuesCombo.GetSelectedValue().ToString()));
            this.m_currentCue = MyAudio.Static.GetCue(cue);
            this.UpdateCueValues();
        }

        private void CueVolumeChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_currentCue.Volume = slider.Value;
            }
        }

        private void CueVolumeCurveChanged(MyGuiControlCombobox combobox)
        {
            if (this.m_canUpdateValues)
            {
                this.m_currentCue.VolumeCurve = (MyCurveType) ((int) combobox.GetSelectedKey());
            }
        }

        private void effects_ItemSelected()
        {
            MyAudioEffectDefinition definition;
            foreach (MyGuiControlCombobox combobox in this.m_cues)
            {
                this.Controls.Remove(combobox);
            }
            this.m_cues.Clear();
            MyStringHash subtypeId = MyStringHash.TryGet(this.m_effects.GetSelectedValue().ToString());
            if (MyDefinitionManager.Static.TryGetDefinition<MyAudioEffectDefinition>(new MyDefinitionId(typeof(MyObjectBuilder_AudioEffectDefinition), subtypeId), out definition))
            {
                for (int i = 0; i < (definition.Effect.SoundsEffects.Count - 1); i++)
                {
                    Vector4? textColor = null;
                    Vector2? size = null;
                    MyGuiControlCombobox box = base.AddCombo(null, textColor, size, 10);
                    this.UpdateCuesCombo(box);
                    this.m_cues.Add(box);
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugAudio";

        private void MaxDistanceChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_currentCue.MaxDistance = slider.Value;
            }
        }

        private void OnApplyMaxDistanceToCategorySelected(MyGuiControlButton button)
        {
            this.m_canUpdateValues = false;
            foreach (MySoundData data in MyAudio.Static.CueDefinitions)
            {
                if (m_currentCategorySelectedItem == data.Category.ToString())
                {
                    data.MaxDistance = this.m_cueMaxDistanceSlider.Value;
                }
            }
            this.m_canUpdateValues = true;
        }

        private void OnApplyVolumeToCategorySelected(MyGuiControlButton button)
        {
            this.m_canUpdateValues = false;
            foreach (MySoundData data in MyAudio.Static.CueDefinitions)
            {
                if (m_currentCategorySelectedItem == data.Category.ToString())
                {
                    data.Volume = this.m_cueVolumeSlider.Value;
                }
            }
            this.m_canUpdateValues = true;
        }

        private void OnPlaySelected(MyGuiControlButton button)
        {
            if ((this.m_sound != null) && this.m_sound.IsPlaying)
            {
                this.m_sound.Stop(true);
            }
            MyCueId cueId = new MyCueId(MyStringHash.TryGet(this.m_cuesCombo.GetSelectedValue().ToString()));
            this.m_sound = MyAudio.Static.PlaySound(cueId, null, MySoundDimensions.D2, false, false);
            MyStringHash hash = MyStringHash.TryGet(this.m_effects.GetSelectedValue().ToString());
            if (hash != MyStringHash.NullOrEmpty)
            {
                foreach (MyGuiControlCombobox combobox in this.m_cues)
                {
                    MyCueId item = new MyCueId(MyStringHash.TryGet(combobox.GetSelectedValue().ToString()));
                    this.m_cueCache.Add(item);
                }
                float? duration = null;
                IMyAudioEffect effect = MyAudio.Static.ApplyEffect(this.m_sound, hash, this.m_cueCache.ToArray(), duration, false);
                this.m_sound = effect.OutputSound;
                this.m_cueCache.Clear();
            }
        }

        private void OnReload(MyGuiControlButton button)
        {
            MyAudio.Static.UnloadData();
            MyDefinitionManager.Static.PreloadDefinitions();
            MyAudio.Static.ReloadData(MyAudioExtensions.GetSoundDataFromDefinitions(), MyAudioExtensions.GetEffectData());
        }

        private void OnSave(MyGuiControlButton button)
        {
            MyObjectBuilder_Definitions objectBuilder = new MyObjectBuilder_Definitions();
            DictionaryValuesReader<MyDefinitionId, MyAudioDefinition> soundDefinitions = MyDefinitionManager.Static.GetSoundDefinitions();
            objectBuilder.Sounds = new MyObjectBuilder_AudioDefinition[soundDefinitions.Count];
            int index = 0;
            foreach (MyAudioDefinition definition in soundDefinitions)
            {
                index++;
                objectBuilder.Sounds[index] = (MyObjectBuilder_AudioDefinition) definition.GetObjectBuilder();
            }
            MyObjectBuilderSerializer.SerializeXML(Path.Combine(MyFileSystem.ContentPath, @"Data\Audio.sbc"), false, objectBuilder, null);
        }

        private void OnStopSelected(MyGuiControlButton button)
        {
            if ((this.m_sound != null) && this.m_sound.IsPlaying)
            {
                this.m_sound.Stop(true);
            }
        }

        private void PitchVariationChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_currentCue.PitchVariation = slider.Value;
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Audio FX", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            if (!(MyAudio.Static is MyNullAudio))
            {
                Vector4? textColor = null;
                captionOffset = null;
                this.m_categoriesCombo = base.AddCombo(null, textColor, captionOffset, 10);
                int? sortOrder = null;
                this.m_categoriesCombo.AddItem(0L, new StringBuilder("_ALL_CATEGORIES_"), sortOrder, null);
                int num = 1;
                foreach (MyStringId id in MyAudio.Static.GetCategories())
                {
                    num++;
                    sortOrder = null;
                    this.m_categoriesCombo.AddItem((long) num, new StringBuilder(id.ToString()), sortOrder, null);
                }
                this.m_categoriesCombo.SortItemsByValueText();
                this.m_categoriesCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.categoriesCombo_OnSelect);
                textColor = null;
                captionOffset = null;
                this.m_cuesCombo = base.AddCombo(null, textColor, captionOffset, 10);
                this.m_cuesCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.cuesCombo_OnSelect);
                textColor = null;
                this.m_cueVolumeSlider = base.AddSlider("Volume", (float) 1f, (float) 0f, (float) 1f, textColor);
                this.m_cueVolumeSlider.ValueChanged = new Action<MyGuiControlSlider>(this.CueVolumeChanged);
                textColor = null;
                captionOffset = null;
                this.m_applyVolumeToCategory = base.AddButton(new StringBuilder("Apply to category"), new Action<MyGuiControlButton>(this.OnApplyVolumeToCategorySelected), null, textColor, captionOffset, true, true);
                this.m_applyVolumeToCategory.Enabled = false;
                textColor = null;
                captionOffset = null;
                this.m_cueVolumeCurveCombo = base.AddCombo(null, textColor, captionOffset, 10);
                foreach (object obj2 in Enum.GetValues(typeof(MyCurveType)))
                {
                    sortOrder = null;
                    this.m_cueVolumeCurveCombo.AddItem((long) ((int) obj2), new StringBuilder(obj2.ToString()), sortOrder, null);
                }
                textColor = null;
                captionOffset = null;
                this.m_effects = base.AddCombo(null, textColor, captionOffset, 10);
                sortOrder = null;
                this.m_effects.AddItem(0L, new StringBuilder(""), sortOrder, null);
                num = 1;
                foreach (MyAudioEffectDefinition definition in MyDefinitionManager.Static.GetAudioEffectDefinitions())
                {
                    num++;
                    sortOrder = null;
                    this.m_effects.AddItem((long) num, new StringBuilder(definition.Id.SubtypeName), sortOrder, null);
                }
                this.m_effects.SelectItemByIndex(0);
                this.m_effects.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.effects_ItemSelected);
                textColor = null;
                this.m_cueMaxDistanceSlider = base.AddSlider("Max distance", (float) 0f, (float) 0f, (float) 2000f, textColor);
                this.m_cueMaxDistanceSlider.ValueChanged = new Action<MyGuiControlSlider>(this.MaxDistanceChanged);
                textColor = null;
                captionOffset = null;
                this.m_applyMaxDistanceToCategory = base.AddButton(new StringBuilder("Apply to category"), new Action<MyGuiControlButton>(this.OnApplyMaxDistanceToCategorySelected), null, textColor, captionOffset, true, true);
                this.m_applyMaxDistanceToCategory.Enabled = false;
                textColor = null;
                this.m_cueVolumeVariationSlider = base.AddSlider("Volume variation", (float) 0f, (float) 0f, (float) 10f, textColor);
                this.m_cueVolumeVariationSlider.ValueChanged = new Action<MyGuiControlSlider>(this.VolumeVariationChanged);
                textColor = null;
                this.m_cuePitchVariationSlider = base.AddSlider("Pitch variation", (float) 0f, (float) 0f, (float) 500f, textColor);
                this.m_cuePitchVariationSlider.ValueChanged = new Action<MyGuiControlSlider>(this.PitchVariationChanged);
                textColor = null;
                captionOffset = null;
                this.m_soloCheckbox = base.AddCheckBox("Solo", false, (Action<MyGuiControlCheckbox>) null, true, null, textColor, captionOffset);
                this.m_soloCheckbox.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.SoloChanged);
                textColor = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Play selected"), new Action<MyGuiControlButton>(this.OnPlaySelected), null, textColor, captionOffset, true, true).CueEnum = GuiSounds.None;
                textColor = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Stop selected"), new Action<MyGuiControlButton>(this.OnStopSelected), null, textColor, captionOffset, true, true);
                textColor = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Save"), new Action<MyGuiControlButton>(this.OnSave), null, textColor, captionOffset, true, true);
                textColor = null;
                captionOffset = null;
                base.AddButton(new StringBuilder("Reload"), new Action<MyGuiControlButton>(this.OnReload), null, textColor, captionOffset, true, true);
                if (this.m_categoriesCombo.GetItemsCount() > 0)
                {
                    this.m_categoriesCombo.SelectItemByIndex(0);
                }
            }
        }

        private void SoloChanged(MyGuiControlCheckbox checkbox)
        {
            if (this.m_canUpdateValues)
            {
                if (checkbox.IsChecked)
                {
                    MyAudio.Static.SoloCue = this.m_currentCue;
                }
                else
                {
                    MyAudio.Static.SoloCue = null;
                }
            }
        }

        private void UpdateCuesCombo(MyGuiControlCombobox box)
        {
            box.ClearItems();
            long key = 0L;
            foreach (MySoundData data in MyAudio.Static.CueDefinitions)
            {
                if ((m_currentCategorySelectedItem == "_ALL_CATEGORIES_") || (m_currentCategorySelectedItem == data.Category.ToString()))
                {
                    int? sortOrder = null;
                    box.AddItem(key, new StringBuilder(data.SubtypeId.ToString()), sortOrder, null);
                    key += 1L;
                }
            }
            box.SortItemsByValueText();
            if (box.GetItemsCount() > 0)
            {
                box.SelectItemByIndex(0);
            }
        }

        private void UpdateCueValues()
        {
            this.m_canUpdateValues = false;
            this.m_cueVolumeSlider.Value = this.m_currentCue.Volume;
            this.m_cueVolumeCurveCombo.SelectItemByKey((long) this.m_currentCue.VolumeCurve, true);
            this.m_cueMaxDistanceSlider.Value = this.m_currentCue.MaxDistance;
            this.m_cueVolumeVariationSlider.Value = this.m_currentCue.VolumeVariation;
            this.m_cuePitchVariationSlider.Value = this.m_currentCue.PitchVariation;
            this.m_soloCheckbox.IsChecked = ReferenceEquals(this.m_currentCue, MyAudio.Static.SoloCue);
            this.m_canUpdateValues = true;
        }

        private void VolumeVariationChanged(MyGuiControlSlider slider)
        {
            if (this.m_canUpdateValues)
            {
                this.m_currentCue.VolumeVariation = slider.Value;
            }
        }
    }
}

