namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.ModAPI;

    [MyComponentBuilder(typeof(MyObjectBuilder_ContainerDropComponent), true)]
    public class MyContainerDropComponent : MyEntityComponentBase
    {
        private MyEntity3DSoundEmitter m_soundEmitter;
        private string m_playingSound;
        private bool m_playSound;

        public MyContainerDropComponent()
        {
        }

        public MyContainerDropComponent(bool competetive, string gpsName, long owner, string sound)
        {
            this.Competetive = competetive;
            this.GPSName = gpsName;
            this.Owner = owner;
            this.m_playingSound = sound;
            this.m_playSound = !string.IsNullOrEmpty(this.m_playingSound);
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase baseBuilder)
        {
            MyObjectBuilder_ContainerDropComponent component = baseBuilder as MyObjectBuilder_ContainerDropComponent;
            this.Competetive = component.Competetive;
            this.GPSName = component.GPSName;
            this.Owner = component.Owner;
            this.m_playingSound = component.PlayingSound;
            this.m_playSound = !string.IsNullOrEmpty(this.m_playingSound);
        }

        public override bool IsSerialized() => 
            true;

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (this.m_playSound)
            {
                this.m_playSound = false;
                this.PlaySound(this.m_playingSound);
            }
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            if (this.m_playSound)
            {
                this.m_playSound = false;
                this.PlaySound(this.m_playingSound);
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.StopSound();
            this.m_soundEmitter = null;
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            this.StopSound();
            this.m_soundEmitter = null;
            if (Sync.IsServer)
            {
                MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().ContainerDestroyed(this);
            }
        }

        public void PlaySound(string soundName)
        {
            if (Game.IsDedicated)
            {
                this.m_playingSound = soundName;
            }
            else
            {
                MySoundPair soundId = new MySoundPair(soundName, true);
                if (!soundId.Arcade.IsNull || !soundId.Realistic.IsNull)
                {
                    if (this.m_soundEmitter == null)
                    {
                        this.m_soundEmitter = new MyEntity3DSoundEmitter((MyEntity) base.Entity, true, 1f);
                        MyCubeBlock entity = base.Entity as MyCubeBlock;
                        if (entity != null)
                        {
                            entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                        }
                    }
                    bool? nullable = null;
                    this.m_soundEmitter.PlaySound(soundId, true, false, false, false, false, nullable);
                    this.m_playingSound = soundName;
                }
            }
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_ContainerDropComponent component1 = MyComponentFactory.CreateObjectBuilder(this) as MyObjectBuilder_ContainerDropComponent;
            component1.Competetive = this.Competetive;
            component1.GPSName = this.GPSName;
            component1.Owner = this.Owner;
            component1.PlayingSound = this.m_playingSound;
            return component1;
        }

        public void StopSound()
        {
            if ((this.m_soundEmitter != null) && this.m_soundEmitter.IsPlaying)
            {
                this.m_soundEmitter.StopSound(true, true);
            }
        }

        public void UpdateSound()
        {
            if (this.m_soundEmitter != null)
            {
                this.m_soundEmitter.Update();
            }
        }

        public bool Competetive { get; private set; }

        public string GPSName { get; private set; }

        public long Owner { get; private set; }

        public long GridEntityId { get; set; }

        public override string ComponentTypeDebugString =>
            "ContainerDropComponent";
    }
}

