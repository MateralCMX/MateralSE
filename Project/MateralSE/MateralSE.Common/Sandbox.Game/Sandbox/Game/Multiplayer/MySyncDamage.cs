namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Network;
    using VRage.Utils;

    [StaticEventOwner]
    public class MySyncDamage
    {
        public static void DoDamageSynced(MyEntity entity, float damage, MyStringHash type, long attackerId)
        {
            IMyDestroyableObject obj2 = entity as IMyDestroyableObject;
            if (obj2 != null)
            {
                MyHitInfo? hitInfo = null;
                obj2.DoDamage(damage, type, false, hitInfo, attackerId);
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, float, MyStringHash, long>(s => new Action<long, float, MyStringHash, long>(MySyncDamage.OnDoDamage), entity.EntityId, damage, type, attackerId, targetEndpoint, position);
            }
        }

        [Event(null, 0x22), Reliable, Broadcast]
        private static void OnDoDamage(long destroyableId, float damage, MyStringHash type, long attackerId)
        {
            MyEntity entity;
            if (MyEntities.TryGetEntityById(destroyableId, out entity, false))
            {
                IMyDestroyableObject obj2 = entity as IMyDestroyableObject;
                if (obj2 != null)
                {
                    MyHitInfo? hitInfo = null;
                    obj2.DoDamage(damage, type, false, hitInfo, attackerId);
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySyncDamage.<>c <>9 = new MySyncDamage.<>c();
            public static Func<IMyEventOwner, Action<long, float, MyStringHash, long>> <>9__0_0;

            internal Action<long, float, MyStringHash, long> <DoDamageSynced>b__0_0(IMyEventOwner s) => 
                new Action<long, float, MyStringHash, long>(MySyncDamage.OnDoDamage);
        }
    }
}

