namespace VRage.Audio
{
    using System;

    public static class MyAudioConstants
    {
        public const float MUSIC_MASTER_VOLUME_MIN = 0f;
        public const float MUSIC_MASTER_VOLUME_DEFAULT = 0.5f;
        public const float MUSIC_MASTER_VOLUME_MAX = 1f;
        public const float GAME_MASTER_VOLUME_MIN = 0f;
        public const float GAME_MASTER_VOLUME_MAX = 1f;
        public const float VOICE_CHAT_VOLUME_MIN = 0f;
        public const float VOICE_CHAT_VOLUME_MAX = 1f;
        public const float REVERB_MAX = 100f;
        public const int MAX_SAME_CUES_PLAYED = 7;
        public const int PREALLOCATED_UNITED_SOUNDS_PER_PHYS_OBJECT = 100;
        public const bool LIMIT_MAX_SAME_CUES = false;
        public const int MAX_COLLISION_SOUNDS = 3;
        public const int MAX_COLLISION_SOUNDS_PER_SECOND = 5;
        public const float MIN_DECELERATION_FOR_COLLISION_SOUND = 0f;
        public const float MAX_DECELERATION = -1f;
        public const float DECELERATION_MIN_VOLUME = 0.95f;
        public const float DECELERATION_MAX_VOLUME = 1f;
        public const float OCCLUSION_INTERVAL = 200f;
        public const float MAIN_MENU_DECREASE_VOLUME_LEVEL = 0.5f;
        public const int FAREST_TIME_IN_PAST = -60000;
    }
}

