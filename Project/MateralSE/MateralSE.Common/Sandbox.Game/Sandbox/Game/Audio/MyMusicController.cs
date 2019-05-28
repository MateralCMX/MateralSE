namespace Sandbox.Game.Audio
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    internal class MyMusicController
    {
        private const int METEOR_SHOWER_MUSIC_FREQUENCY = 0xa8c0;
        private const int METEOR_SHOWER_CROSSFADE_LENGTH = 0x7d0;
        private const int DEFAULT_NO_MUSIC_TIME_MIN = 2;
        private const int DEFAULT_NO_MUSIC_TIME_MAX = 8;
        private const int FAST_NO_MUSIC_TIME_MIN = 1;
        private const int FAST_NO_MUSIC_TIME_MAX = 4;
        private const int BUILDING_NEED = 0x1b58;
        private const int BUILDING_COOLDOWN = 0xafc8;
        private const int BUILDING_CROSSFADE_LENGTH = 0x7d0;
        private const int FIGHTING_NEED = 100;
        private const int FIGHTING_COOLDOWN_LIGHT = 15;
        private const int FIGHTING_COOLDOWN_HEAVY = 20;
        private const int FIGHTING_CROSSFADE_LENGTH = 0x7d0;
        private static List<MusicOption> m_defaultSpaceCategories;
        private static List<MusicOption> m_defaultPlanetCategory;
        private static MyStringHash m_hashCrossfade;
        private static MyStringHash m_hashFadeIn;
        private static MyStringHash m_hashFadeOut;
        private static MyStringId m_stringIdDanger;
        private static MyStringId m_stringIdBuilding;
        private static MyStringId m_stringIdLightFight;
        private static MyStringId m_stringIdHeavyFight;
        private static MyCueId m_cueEmpty;
        public bool Active;
        public bool CanChangeCategoryGlobal = true;
        private bool CanChangeCategoryLocal = true;
        private Dictionary<MyStringId, List<MyCueId>> m_musicCuesAll;
        private Dictionary<MyStringId, List<MyCueId>> m_musicCuesRemaining;
        private List<MusicOption> m_actualMusicOptions = new List<MusicOption>();
        private MyPlanet m_lastVisitedPlanet;
        private MySoundData m_lastMusicData;
        private int m_frameCounter;
        private float m_noMusicTimer;
        private MyRandom m_random = new MyRandom();
        private IMySourceVoice m_musicSourceVoice;
        private int m_lastMeteorShower = -2147483648;
        private MusicCategory m_currentMusicCategory;
        private int m_meteorShower;
        private int m_building;
        private int m_buildingCooldown;
        private int m_fightLight;
        private int m_fightLightCooldown;
        private int m_fightHeavy;
        private int m_fightHeavyCooldown;

        static MyMusicController()
        {
            List<MusicOption> list1 = new List<MusicOption>();
            list1.Add(new MusicOption("Space", 0.7f));
            list1.Add(new MusicOption("Calm", 0.25f));
            list1.Add(new MusicOption("Mystery", 0.05f));
            m_defaultSpaceCategories = list1;
            List<MusicOption> list2 = new List<MusicOption>();
            list2.Add(new MusicOption("Planet", 0.8f));
            list2.Add(new MusicOption("Calm", 0.1f));
            list2.Add(new MusicOption("Danger", 0.1f));
            m_defaultPlanetCategory = list2;
            m_hashCrossfade = MyStringHash.GetOrCompute("CrossFade");
            m_hashFadeIn = MyStringHash.GetOrCompute("FadeIn");
            m_hashFadeOut = MyStringHash.GetOrCompute("FadeOut");
            m_stringIdDanger = MyStringId.GetOrCompute("Danger");
            m_stringIdBuilding = MyStringId.GetOrCompute("Building");
            m_stringIdLightFight = MyStringId.GetOrCompute("LightFight");
            m_stringIdHeavyFight = MyStringId.GetOrCompute("HeavyFight");
            m_cueEmpty = new MyCueId();
        }

        public MyMusicController(Dictionary<MyStringId, List<MyCueId>> musicCues = null)
        {
            this.CategoryPlaying = MyStringId.NullOrEmpty;
            this.CategoryLast = MyStringId.NullOrEmpty;
            this.Active = false;
            this.m_musicCuesAll = (musicCues != null) ? musicCues : new Dictionary<MyStringId, List<MyCueId>>(MyStringId.Comparer);
            this.m_musicCuesRemaining = new Dictionary<MyStringId, List<MyCueId>>(MyStringId.Comparer);
        }

        public void AddMusicCue(MyStringId category, MyCueId cueId)
        {
            if (!this.m_musicCuesAll.ContainsKey(category))
            {
                this.m_musicCuesAll.Add(category, new List<MyCueId>());
            }
            this.m_musicCuesAll[category].Add(cueId);
        }

        public void Building(int amount)
        {
            this.m_building = Math.Min(0x1b58, this.m_building + amount);
            this.m_buildingCooldown = Math.Min(0xafc8, this.m_buildingCooldown + (amount * 5));
            if (this.CanChangeCategory && (this.m_building >= 0x1b58))
            {
                this.m_noMusicTimer = this.m_random.Next(1, 4);
                if (this.m_currentMusicCategory < MusicCategory.building)
                {
                    this.PlayBuildingMusic();
                }
            }
        }

        private void CalculateNextCue()
        {
            if (MySession.Static == null)
            {
                return;
            }
            else if (MySession.Static.LocalCharacter != null)
            {
                this.m_noMusicTimer = this.m_random.Next(2, 8);
                Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                MySphericalNaturalGravityComponent component = (closestPlanet != null) ? (closestPlanet.Components.Get<MyGravityProviderComponent>() as MySphericalNaturalGravityComponent) : null;
                if (((closestPlanet != null) && (component != null)) && (Vector3D.Distance(position, closestPlanet.PositionComp.GetPosition()) <= (component.GravityLimit * 0.65f)))
                {
                    if (!ReferenceEquals(closestPlanet, this.m_lastVisitedPlanet))
                    {
                        this.m_lastVisitedPlanet = closestPlanet;
                        if ((closestPlanet.Generator.MusicCategories != null) && (closestPlanet.Generator.MusicCategories.Count > 0))
                        {
                            this.m_actualMusicOptions.Clear();
                            foreach (MyMusicCategory category in closestPlanet.Generator.MusicCategories)
                            {
                                this.m_actualMusicOptions.Add(new MusicOption(category.Category, category.Frequency));
                            }
                        }
                        else
                        {
                            this.m_actualMusicOptions = m_defaultPlanetCategory;
                        }
                    }
                }
                else
                {
                    this.m_lastVisitedPlanet = null;
                    this.m_actualMusicOptions = m_defaultSpaceCategories;
                }
            }
            else
            {
                return;
            }
            float num = 0f;
            foreach (MusicOption option in this.m_actualMusicOptions)
            {
                num += Math.Max(option.Frequency, 0f);
            }
            float num2 = ((float) this.m_random.NextDouble()) * num;
            MyStringId category = this.m_actualMusicOptions[0].Category;
            int num3 = 0;
            while (true)
            {
                if (num3 < this.m_actualMusicOptions.Count)
                {
                    if (num2 > this.m_actualMusicOptions[num3].Frequency)
                    {
                        num2 -= this.m_actualMusicOptions[num3].Frequency;
                        num3++;
                        continue;
                    }
                    category = this.m_actualMusicOptions[num3].Category;
                }
                this.CueIdPlaying = this.SelectCueFromCategory(category);
                this.CategoryPlaying = category;
                if (this.CueIdPlaying != m_cueEmpty)
                {
                    this.PlayMusic(this.CueIdPlaying, MyStringHash.NullOrEmpty, 0x7d0, null, true);
                    this.m_currentMusicCategory = MusicCategory.location;
                }
                return;
            }
        }

        public void ClearMusicCues()
        {
            this.m_musicCuesAll.Clear();
            this.m_musicCuesRemaining.Clear();
        }

        public void Fighting(bool heavy, int amount)
        {
            this.m_fightLight = Math.Min(this.m_fightLight + amount, 100);
            this.m_fightLightCooldown = 15;
            if (heavy)
            {
                this.m_fightHeavy = Math.Min(this.m_fightHeavy + amount, 100);
                this.m_fightHeavyCooldown = 20;
            }
            if (this.CanChangeCategory)
            {
                if ((this.m_fightHeavy >= 100) && (this.m_currentMusicCategory < MusicCategory.heavyFight))
                {
                    this.PlayFightingMusic(false);
                }
                else if ((this.m_fightLight >= 100) && (this.m_currentMusicCategory < MusicCategory.lightFight))
                {
                    this.PlayFightingMusic(true);
                }
            }
        }

        public void IncreaseCategory(MyStringId category, int amount)
        {
            if (category == m_stringIdLightFight)
            {
                this.Fighting(false, amount);
            }
            else if (category == m_stringIdHeavyFight)
            {
                this.Fighting(true, amount);
            }
            else if (category == m_stringIdBuilding)
            {
                this.Building(amount);
            }
            else if (category == m_stringIdDanger)
            {
                this.MeteorShowerIncoming();
            }
        }

        public void MeteorShowerIncoming()
        {
            int sessionTotalFrames = MyFpsManager.GetSessionTotalFrames();
            if (this.CanChangeCategory && (Math.Abs((int) (this.m_lastMeteorShower - sessionTotalFrames)) >= 0xa8c0))
            {
                this.m_meteorShower = 10;
                this.m_lastMeteorShower = sessionTotalFrames;
                this.m_noMusicTimer = this.m_random.Next(1, 4);
                if (this.m_currentMusicCategory < MusicCategory.danger)
                {
                    this.PlayDangerMusic();
                }
            }
        }

        public void MusicStopped()
        {
            if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
            {
                this.CategoryLast = this.CategoryPlaying;
                this.CategoryPlaying = MyStringId.NullOrEmpty;
                this.CanChangeCategoryLocal = true;
            }
        }

        private void PlayBuildingMusic()
        {
            this.CategoryPlaying = m_stringIdBuilding;
            this.m_currentMusicCategory = MusicCategory.building;
            if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
            {
                this.PlayMusic(this.SelectCueFromCategory(this.CategoryPlaying), m_hashFadeIn, 0x3e8, new MyCueId[0], true);
            }
            else
            {
                MyCueId[] cueIds = new MyCueId[] { this.SelectCueFromCategory(m_stringIdBuilding) };
                this.PlayMusic(this.CueIdPlaying, m_hashCrossfade, 0x7d0, cueIds, false);
            }
            this.m_noMusicTimer = this.m_random.Next(2, 8);
        }

        private void PlayDangerMusic()
        {
            this.CategoryPlaying = m_stringIdDanger;
            this.m_currentMusicCategory = MusicCategory.danger;
            if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
            {
                this.PlayMusic(this.SelectCueFromCategory(this.CategoryPlaying), m_hashFadeIn, 0x3e8, new MyCueId[0], true);
            }
            else
            {
                MyCueId[] cueIds = new MyCueId[] { this.SelectCueFromCategory(m_stringIdDanger) };
                this.PlayMusic(this.CueIdPlaying, m_hashCrossfade, 0x7d0, cueIds, false);
            }
            this.m_noMusicTimer = this.m_random.Next(2, 8);
        }

        private void PlayFightingMusic(bool light)
        {
            this.CategoryPlaying = light ? m_stringIdLightFight : m_stringIdHeavyFight;
            this.m_currentMusicCategory = light ? MusicCategory.lightFight : MusicCategory.heavyFight;
            if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
            {
                this.PlayMusic(this.SelectCueFromCategory(this.CategoryPlaying), m_hashFadeIn, 0x3e8, new MyCueId[0], true);
            }
            else
            {
                MyCueId[] cueIds = new MyCueId[] { this.SelectCueFromCategory(this.CategoryPlaying) };
                this.PlayMusic(this.CueIdPlaying, m_hashCrossfade, 0x7d0, cueIds, false);
            }
            this.m_noMusicTimer = this.m_random.Next(1, 4);
        }

        private void PlayMusic(MyCueId cue, MyStringHash effect, int effectDuration = 0x7d0, MyCueId[] cueIds = null, bool play = true)
        {
            if (MyAudio.Static != null)
            {
                if (play)
                {
                    this.m_musicSourceVoice = MyAudio.Static.PlayMusicCue(cue, true);
                }
                if (this.m_musicSourceVoice != null)
                {
                    if (effect != MyStringHash.NullOrEmpty)
                    {
                        this.m_musicSourceVoice = MyAudio.Static.ApplyEffect(this.m_musicSourceVoice, effect, cueIds, new float?((float) effectDuration), true).OutputSound;
                    }
                    if (this.m_musicSourceVoice != null)
                    {
                        this.m_musicSourceVoice.StoppedPlaying = (Action) Delegate.Combine(this.m_musicSourceVoice.StoppedPlaying, new Action(this.MusicStopped));
                    }
                }
                this.m_lastMusicData = MyAudio.Static.GetCue(cue);
            }
        }

        public void PlaySpecificMusicCategory(MyStringId category, bool playAtLeastOnce)
        {
            if (category.Id != 0)
            {
                this.CategoryPlaying = category;
                if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
                {
                    this.PlayMusic(this.SelectCueFromCategory(this.CategoryPlaying), m_hashFadeIn, 0x3e8, new MyCueId[0], true);
                }
                else
                {
                    MyCueId[] cueIds = new MyCueId[] { this.SelectCueFromCategory(this.CategoryPlaying) };
                    this.PlayMusic(this.CueIdPlaying, m_hashCrossfade, 0x7d0, cueIds, false);
                }
                this.m_noMusicTimer = this.m_random.Next(2, 8);
                this.CanChangeCategoryLocal = !playAtLeastOnce;
                this.m_currentMusicCategory = MusicCategory.custom;
            }
        }

        public void PlaySpecificMusicTrack(MyCueId cue, bool playAtLeastOnce)
        {
            if (!cue.IsNull)
            {
                if ((this.m_musicSourceVoice == null) || !this.m_musicSourceVoice.IsPlaying)
                {
                    this.PlayMusic(cue, m_hashFadeIn, 0x3e8, new MyCueId[0], true);
                }
                else
                {
                    MyCueId[] cueIds = new MyCueId[] { cue };
                    this.PlayMusic(this.CueIdPlaying, m_hashCrossfade, 0x7d0, cueIds, false);
                }
                this.m_noMusicTimer = this.m_random.Next(2, 8);
                this.CanChangeCategoryLocal = !playAtLeastOnce;
                this.m_currentMusicCategory = MusicCategory.location;
            }
        }

        private MyCueId SelectCueFromCategory(MyStringId category)
        {
            if (!this.m_musicCuesRemaining.ContainsKey(category))
            {
                this.m_musicCuesRemaining.Add(category, new List<MyCueId>());
            }
            if (this.m_musicCuesRemaining[category].Count == 0)
            {
                if (!this.m_musicCuesAll.ContainsKey(category))
                {
                    goto TR_0000;
                }
                else if ((this.m_musicCuesAll[category] != null) && (this.m_musicCuesAll[category].Count != 0))
                {
                    foreach (MyCueId id in this.m_musicCuesAll[category])
                    {
                        this.m_musicCuesRemaining[category].Add(id);
                    }
                    int? count = null;
                    this.m_musicCuesRemaining[category].ShuffleList<MyCueId>(0, count);
                }
                else
                {
                    goto TR_0000;
                }
            }
            this.m_musicCuesRemaining[category].RemoveAt(0);
            return this.m_musicCuesRemaining[category][0];
        TR_0000:
            return m_cueEmpty;
        }

        public void SetMusicCues(Dictionary<MyStringId, List<MyCueId>> musicCues)
        {
            this.ClearMusicCues();
            this.m_musicCuesAll = musicCues;
        }

        public void SetSpecificMusicCategory(MyStringId category)
        {
            if (category.Id != 0)
            {
                this.CategoryPlaying = category;
                this.m_currentMusicCategory = MusicCategory.custom;
            }
        }

        public void Unload()
        {
            if (this.m_musicSourceVoice != null)
            {
                this.m_musicSourceVoice.Stop(false);
                this.m_musicSourceVoice = null;
            }
            this.Active = false;
            this.ClearMusicCues();
        }

        public void Update()
        {
            if ((this.m_frameCounter % 60) == 0)
            {
                this.Update_1s();
            }
            if (this.MusicIsPlaying)
            {
                if (MyAudio.Static.Mute)
                {
                    MyAudio.Static.Mute = false;
                }
                this.m_musicSourceVoice.SetVolume((this.m_lastMusicData != null) ? (MyAudio.Static.VolumeMusic * this.m_lastMusicData.Volume) : MyAudio.Static.VolumeMusic);
            }
            else if (this.m_noMusicTimer > 0f)
            {
                this.m_noMusicTimer -= 0.01666667f;
            }
            else
            {
                if (this.CanChangeCategory)
                {
                    this.m_currentMusicCategory = (this.m_fightHeavy < 100) ? ((this.m_fightLight < 100) ? ((this.m_meteorShower <= 0) ? ((this.m_building < 0x1b58) ? MusicCategory.location : MusicCategory.building) : MusicCategory.danger) : MusicCategory.lightFight) : MusicCategory.heavyFight;
                }
                switch (this.m_currentMusicCategory)
                {
                    case MusicCategory.building:
                        this.PlayBuildingMusic();
                        break;

                    case MusicCategory.danger:
                        this.PlayDangerMusic();
                        break;

                    case MusicCategory.lightFight:
                        this.PlayFightingMusic(true);
                        break;

                    case MusicCategory.heavyFight:
                        this.PlayFightingMusic(false);
                        break;

                    case MusicCategory.custom:
                        this.PlaySpecificMusicCategory(this.CategoryLast, false);
                        break;

                    default:
                        this.CalculateNextCue();
                        break;
                }
            }
            this.m_frameCounter++;
        }

        private void Update_1s()
        {
            if (this.m_meteorShower > 0)
            {
                this.m_meteorShower--;
            }
            if (this.m_buildingCooldown > 0)
            {
                this.m_buildingCooldown = Math.Max(0, this.m_buildingCooldown - 0x3e8);
            }
            else if (this.m_building > 0)
            {
                this.m_building = Math.Max(0, this.m_building - 0x3e8);
            }
            if (this.m_fightHeavyCooldown > 0)
            {
                this.m_fightHeavyCooldown = Math.Max(0, this.m_fightHeavyCooldown - 1);
            }
            else if (this.m_fightHeavy > 0)
            {
                this.m_fightHeavy = Math.Max(0, this.m_fightHeavy - 10);
            }
            if (this.m_fightLightCooldown > 0)
            {
                this.m_fightLightCooldown = Math.Max(0, this.m_fightLightCooldown - 1);
            }
            else if (this.m_fightLight > 0)
            {
                this.m_fightLight = Math.Max(0, this.m_fightLight - 10);
            }
        }

        public static MyMusicController Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            set => 
                (<Static>k__BackingField = value);
        }

        public MyStringId CategoryPlaying { get; private set; }

        public MyStringId CategoryLast { get; private set; }

        public MyCueId CueIdPlaying { get; private set; }

        public float NextMusicTrackIn =>
            this.m_noMusicTimer;

        public bool CanChangeCategory =>
            (this.CanChangeCategoryGlobal && this.CanChangeCategoryLocal);

        public bool MusicIsPlaying =>
            ((this.m_musicSourceVoice != null) && this.m_musicSourceVoice.IsPlaying);

        private enum MusicCategory
        {
            location,
            building,
            danger,
            lightFight,
            heavyFight,
            custom
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MusicOption
        {
            public MyStringId Category;
            public float Frequency;
            public MusicOption(string category, float frequency)
            {
                this.Category = MyStringId.GetOrCompute(category);
                this.Frequency = frequency;
            }
        }
    }
}

