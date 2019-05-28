namespace Sandbox.Game.Weapons
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.ModAPI.Weapons;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    public abstract class MyBlockPlacerBase : MyEngineerToolBase, IMyBlockPlacerBase, VRage.ModAPI.IMyEntity, VRage.Game.ModAPI.Ingame.IMyEntity, IMyEngineerToolBase, IMyHandheldGunObject<MyToolBase>, IMyGunObject<MyToolBase>
    {
        public static MyHudNotificationBase MissingComponentNotification = new MyHudNotification(MyCommonTexts.NotificationMissingComponentToPlaceBlockFormat, 0x9c4, "Red", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 1, MyNotificationLevel.Normal);
        protected int m_lastKeyPress;
        protected bool m_firstShot;
        protected bool m_closeAfterBuild;
        private MyHandItemDefinition m_definition;

        protected MyBlockPlacerBase(MyHandItemDefinition definition) : base(500)
        {
            this.m_definition = definition;
        }

        protected override void AddHudInfo()
        {
        }

        public override void BeginFailReaction(MyShootActionEnum action, MyGunStatusEnum status)
        {
        }

        public bool CanDoubleClickToStick(MyShootActionEnum action) => 
            false;

        public override bool CanShoot(MyShootActionEnum action, long shooter, out MyGunStatusEnum status)
        {
            bool flag = base.CanShoot(action, shooter, out status);
            if (((status == MyGunStatusEnum.Cooldown) && (action == MyShootActionEnum.PrimaryAction)) && this.m_firstShot)
            {
                status = MyGunStatusEnum.OK;
                flag = true;
            }
            return flag;
        }

        protected override void DrawHud()
        {
        }

        public override void EndShoot(MyShootActionEnum action)
        {
            base.EndShoot(action);
            this.m_firstShot = true;
            if (base.CharacterInventory != null)
            {
                MyCharacter owner = base.CharacterInventory.Owner as MyCharacter;
                if (((owner != null) && this.m_closeAfterBuild) && ((owner.ControllerInfo == null) || !owner.ControllerInfo.IsRemotelyControlled()))
                {
                    owner.SwitchToWeapon((MyToolbarItemWeapon) null);
                }
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder, this.m_definition.PhysicalItemId);
            float? scale = null;
            this.Init(null, null, null, scale, null);
            base.Render.CastShadows = true;
            base.Render.NeedsResolveCastShadow = false;
            base.HasSecondaryEffect = false;
            base.HasPrimaryEffect = false;
            this.m_firstShot = true;
            if (base.PhysicalObject != null)
            {
                base.PhysicalObject.GunEntity = (MyObjectBuilder_EntityBase) objectBuilder.Clone();
            }
        }

        public override void OnControlAcquired(MyCharacter owner)
        {
            base.OnControlAcquired(owner);
            if (base.Owner != null)
            {
                if (owner.UseNewAnimationSystem)
                {
                    base.Owner.TriggerCharacterAnimationEvent("building", false);
                }
                else
                {
                    base.Owner.PlayCharacterAnimation("Building_pose", MyBlendOption.Immediate, MyFrameOption.Loop, 0.2f, 1f, false, null, false);
                }
            }
        }

        public override void OnControlReleased()
        {
            if ((base.Owner != null) && base.Owner.ControllerInfo.IsLocallyHumanControlled())
            {
                this.BlockBuilder.Deactivate();
                MySession.Static.GameFocusManager.Clear();
            }
            base.OnControlReleased();
        }

        public static void OnMissingComponents(MyCubeBlockDefinition definition)
        {
            MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
            (MyHud.Notifications.Get(MyNotificationSingletons.MissingComponent) as MyHudMissingComponentNotification).SetBlockDefinition(definition);
            MyHud.Notifications.Add(MyNotificationSingletons.MissingComponent);
        }

        protected override void RemoveHudInfo()
        {
        }

        public override void Shoot(MyShootActionEnum action, Vector3 direction, Vector3D? overrideWeaponPos, string gunAction)
        {
            if (!MySession.Static.CreativeMode)
            {
                this.m_closeAfterBuild = false;
                Vector3D? nullable = null;
                base.Shoot(action, direction, nullable, gunAction);
                base.ShakeAmount = 0f;
                if ((action == MyShootActionEnum.PrimaryAction) && this.m_firstShot)
                {
                    this.m_firstShot = false;
                    this.m_lastKeyPress = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                    MyCubeBlockDefinition currentBlockDefinition = MyCubeBuilder.Static.CubeBuilderState.CurrentBlockDefinition;
                    if (currentBlockDefinition != null)
                    {
                        if (!base.Owner.ControllerInfo.IsLocallyControlled())
                        {
                            MyCockpit isUsing = base.Owner.IsUsing as MyCockpit;
                            if ((isUsing == null) || !isUsing.ControllerInfo.IsLocallyControlled())
                            {
                                return;
                            }
                        }
                        if (MyCubeBuilder.Static.CanStartConstruction(base.Owner))
                        {
                            MyCubeBuilder.Static.AddConstruction(base.Owner);
                        }
                        else if (!MySession.Static.CreativeToolsEnabled(Sync.MyId))
                        {
                            OnMissingComponents(currentBlockDefinition);
                        }
                    }
                }
            }
        }

        public bool ShouldEndShootOnPause(MyShootActionEnum action) => 
            true;

        public bool SupressShootAnimation() => 
            false;

        protected abstract MyBlockBuilderBase BlockBuilder { get; }
    }
}

