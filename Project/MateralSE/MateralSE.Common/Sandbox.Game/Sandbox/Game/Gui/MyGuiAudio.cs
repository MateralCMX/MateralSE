namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Audio;
    using VRage.Data.Audio;

    public class MyGuiAudio : IMyGuiAudio
    {
        public static bool HudWarnings;
        private static Dictionary<MyGuiSounds, MySoundPair> m_sounds = new Dictionary<MyGuiSounds, MySoundPair>(Enum.GetValues(typeof(MyGuiSounds)).Length);
        private static Dictionary<MyGuiSounds, int> m_lastTimePlaying = new Dictionary<MyGuiSounds, int>();

        static MyGuiAudio()
        {
            Static = new MyGuiAudio();
            foreach (MyGuiSounds sounds in Enum.GetValues(typeof(MyGuiSounds)))
            {
                m_sounds.Add(sounds, new MySoundPair(sounds.ToString(), false));
            }
        }

        private static bool CheckForSynchronizedSounds(MyGuiSounds sound)
        {
            MySoundData cue = MyAudio.Static.GetCue(m_sounds[sound].SoundId);
            if ((cue != null) && (cue.PreventSynchronization >= 0))
            {
                int num;
                int sessionTotalFrames = MyFpsManager.GetSessionTotalFrames();
                if (!m_lastTimePlaying.TryGetValue(sound, out num))
                {
                    m_lastTimePlaying.Add(sound, sessionTotalFrames);
                }
                else
                {
                    if (Math.Abs((int) (sessionTotalFrames - num)) <= cue.PreventSynchronization)
                    {
                        return false;
                    }
                    m_lastTimePlaying[sound] = sessionTotalFrames;
                }
            }
            return true;
        }

        internal static MyCueId GetCue(MyGuiSounds sound) => 
            m_sounds[sound].SoundId;

        private MyGuiSounds GetSound(GuiSounds sound)
        {
            switch (sound)
            {
                case GuiSounds.MouseClick:
                    return MyGuiSounds.HudMouseClick;

                case GuiSounds.MouseOver:
                    return MyGuiSounds.HudMouseOver;

                case GuiSounds.Item:
                    return MyGuiSounds.HudItem;
            }
            return MyGuiSounds.HudClick;
        }

        public void PlaySound(GuiSounds sound)
        {
            if (sound != GuiSounds.None)
            {
                PlaySound(this.GetSound(sound));
            }
        }

        public static IMySourceVoice PlaySound(MyGuiSounds sound)
        {
            if ((MyFakes.ENABLE_NEW_SOUNDS && ((MySession.Static != null) && (MySession.Static.Settings.RealisticSound && ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.OxygenComponent != null))))) && !MySession.Static.LocalCharacter.OxygenComponent.HelmetEnabled)
            {
                MySoundData cue = MyAudio.Static.GetCue(m_sounds[sound].SoundId);
                if ((cue != null) && cue.CanBeSilencedByVoid)
                {
                    MyCockpit parent = MySession.Static.LocalCharacter.Parent as MyCockpit;
                    if (((parent == null) || !parent.BlockDefinition.IsPressurized) && (MySession.Static.LocalCharacter.EnvironmentOxygenLevel <= 0f))
                    {
                        return null;
                    }
                }
            }
            return (!CheckForSynchronizedSounds(sound) ? null : MyAudio.Static.PlaySound(m_sounds[sound].SoundId, null, MySoundDimensions.D2, false, false));
        }

        public static IMyGuiAudio Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }
    }
}

