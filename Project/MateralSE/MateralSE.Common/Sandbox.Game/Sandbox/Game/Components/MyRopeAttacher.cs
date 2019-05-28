namespace Sandbox.Game.Components
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.Entity;

    public class MyRopeAttacher
    {
        private long m_hookIdFrom;
        private readonly Action<MyEntity> m_selectedHook_OnClosing;
        private MyRopeDefinition m_ropeDefinition;

        public MyRopeAttacher(MyRopeDefinition ropeDefinition)
        {
            this.m_ropeDefinition = ropeDefinition;
            this.m_selectedHook_OnClosing = entity => this.m_hookIdFrom = 0L;
        }

        public void Clear()
        {
            MyEntity entity;
            if ((this.m_hookIdFrom != 0) && MyEntities.TryGetEntityById(this.m_hookIdFrom, out entity, false))
            {
                MyEntities.GetEntityById(this.m_hookIdFrom, false).OnClosing -= this.m_selectedHook_OnClosing;
                this.m_hookIdFrom = 0L;
            }
        }

        public void OnUse(long hookIdTarget)
        {
            bool flag = false;
            MyEntity entity = null;
            if (MyEntities.TryGetEntityById(hookIdTarget, out entity, false))
            {
                if (hookIdTarget != this.m_hookIdFrom)
                {
                    if (this.m_hookIdFrom == 0)
                    {
                        this.m_hookIdFrom = hookIdTarget;
                        entity.OnClosing += this.m_selectedHook_OnClosing;
                        flag = true;
                    }
                    else if (MyRopeComponent.HasRope(this.m_hookIdFrom))
                    {
                        this.m_hookIdFrom = hookIdTarget;
                        flag = true;
                    }
                    else
                    {
                        if (MyRopeComponent.CanConnectHooks(this.m_hookIdFrom, hookIdTarget, this.m_ropeDefinition))
                        {
                            MyRopeComponent.AddRopeRequest(this.m_hookIdFrom, hookIdTarget, this.m_ropeDefinition.Id);
                            flag = true;
                        }
                        this.Clear();
                    }
                }
                MySoundPair soundId = flag ? this.m_ropeDefinition.AttachSound : null;
                if (soundId != null)
                {
                    MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                    if (emitter != null)
                    {
                        emitter.SetPosition(new Vector3D?(entity.PositionComp.GetPosition()));
                        bool? nullable = null;
                        emitter.PlaySound(soundId, false, false, false, false, false, nullable);
                    }
                }
            }
        }
    }
}

