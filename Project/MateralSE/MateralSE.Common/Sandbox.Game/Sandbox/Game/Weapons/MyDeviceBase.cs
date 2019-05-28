namespace Sandbox.Game.Weapons
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    public abstract class MyDeviceBase
    {
        protected MyDeviceBase()
        {
        }

        public abstract bool CanSwitchAmmoMagazine();
        public static string GetGunNotificationName(MyDefinitionId gunId) => 
            MyDefinitionManager.Static.GetDefinition(gunId).DisplayNameText;

        public abstract Vector3D GetMuzzleLocalPosition();
        public abstract Vector3D GetMuzzleWorldPosition();
        public void Init(MyObjectBuilder_DeviceBase objectBuilder)
        {
            this.InventoryItemId = objectBuilder.InventoryItemId;
        }

        public abstract bool SwitchAmmoMagazineToNextAvailable();
        public abstract bool SwitchToNextAmmoMagazine();

        public uint? InventoryItemId { get; set; }
    }
}

