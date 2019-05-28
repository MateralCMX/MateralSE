namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Game.GUI;
    using System;
    using VRage.Audio;
    using VRage.Game.Components;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.ObjectBuilders.ComponentSystem;

    [MyComponentType(typeof(MyCharacterPickupComponent)), MyComponentBuilder(typeof(MyObjectBuilder_CharacterPickupComponent), true)]
    public class MyCharacterPickupComponent : MyCharacterComponent
    {
        public virtual void PickUp()
        {
            MyCharacterDetectorComponent component = base.Character.Components.Get<MyCharacterDetectorComponent>();
            if (((component != null) && (component.UseObject != null)) && component.UseObject.IsActionSupported(UseActionEnum.PickUp))
            {
                if (component.UseObject.PlayIndicatorSound)
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUse);
                    base.Character.SoundComp.StopStateSound(true);
                }
                component.UseObject.Use(UseActionEnum.PickUp, base.Character);
            }
        }

        public virtual void PickUpContinues()
        {
            MyCharacterDetectorComponent component = base.Character.Components.Get<MyCharacterDetectorComponent>();
            if (((component != null) && ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.PickUp))) && component.UseObject.ContinuousUsage)
            {
                component.UseObject.Use(UseActionEnum.PickUp, base.Character);
            }
        }

        public virtual void PickUpFinished()
        {
            MyCharacterDetectorComponent component = base.Character.Components.Get<MyCharacterDetectorComponent>();
            if ((component.UseObject != null) && component.UseObject.IsActionSupported(UseActionEnum.UseFinished))
            {
                component.UseObject.Use(UseActionEnum.UseFinished, base.Character);
            }
        }
    }
}

