namespace VRage.Game.Entity
{
    using System;

    public class MyEntitiesInterface
    {
        public static Action<MyEntity> RegisterUpdate;
        public static Action<MyEntity, bool> UnregisterUpdate;
        public static Action<MyEntity> RegisterDraw;
        public static Action<MyEntity> UnregisterDraw;
        public static Action<MyEntity, bool> SetEntityName;
        public static Func<bool> IsUpdateInProgress;
        public static Func<bool> IsCloseAllowed;
        public static Action<MyEntity> RemoveName;
        public static Action<MyEntity> RemoveFromClosedEntities;
        public static Func<MyEntity, bool> Remove;
        public static Action<MyEntity> RaiseEntityRemove;
        public static Action<MyEntity> Close;
    }
}

