namespace Sandbox.Game.World
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Screens.Helpers;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.GameServices;
    using VRage.Input;
    using VRageMath;

    public class MyEntityRemoteController
    {
        private static readonly Random m_random = new Random();
        private readonly string[] m_animations = new string[] { "Wave", "Thumb-Up", "FacePalm", "Victory" };
        private readonly float m_animationTimer = 20f;
        private readonly float m_doubleClickPause = 0.2f;
        private MyEntity m_controlledEntity;
        private float m_currentAnimationTime;
        private bool m_canPlayAnimation;
        private int m_buttonClicks;
        private float m_lastClickTime;
        private float m_currentTime;
        private float m_rotationSpeed;
        private float m_rotationSpeedDecay = 0.95f;
        private Vector3 m_rotationDirection = Vector3.Zero;
        private GlobalAxis m_rotationLocks;
        private Vector3 m_rotationVector = Vector3.One;
        private Dictionary<string, MyGameInventoryItemSlot> m_toolsNames;

        public MyEntityRemoteController(MyEntity entity)
        {
            this.m_controlledEntity = entity;
            this.m_rotationLocks = GlobalAxis.None;
            this.m_rotationVector = Vector3.One;
            this.m_toolsNames = new Dictionary<string, MyGameInventoryItemSlot>();
            this.m_toolsNames.Add("AutomaticRifleItem", MyGameInventoryItemSlot.Rifle);
            this.m_toolsNames.Add("RapidFireAutomaticRifleItem", MyGameInventoryItemSlot.Rifle);
            this.m_toolsNames.Add("PreciseAutomaticRifleItem", MyGameInventoryItemSlot.Rifle);
            this.m_toolsNames.Add("UltimateAutomaticRifleItem", MyGameInventoryItemSlot.Rifle);
            this.m_toolsNames.Add("WelderItem", MyGameInventoryItemSlot.Welder);
            this.m_toolsNames.Add("Welder2Item", MyGameInventoryItemSlot.Welder);
            this.m_toolsNames.Add("Welder3Item", MyGameInventoryItemSlot.Welder);
            this.m_toolsNames.Add("Welder4Item", MyGameInventoryItemSlot.Welder);
            this.m_toolsNames.Add("AngleGrinderItem", MyGameInventoryItemSlot.Grinder);
            this.m_toolsNames.Add("AngleGrinder2Item", MyGameInventoryItemSlot.Grinder);
            this.m_toolsNames.Add("AngleGrinder3Item", MyGameInventoryItemSlot.Grinder);
            this.m_toolsNames.Add("AngleGrinder4Item", MyGameInventoryItemSlot.Grinder);
            this.m_toolsNames.Add("HandDrillItem", MyGameInventoryItemSlot.Drill);
            this.m_toolsNames.Add("HandDrill2Item", MyGameInventoryItemSlot.Drill);
            this.m_toolsNames.Add("HandDrill3Item", MyGameInventoryItemSlot.Drill);
            this.m_toolsNames.Add("HandDrill4Item", MyGameInventoryItemSlot.Drill);
        }

        public void ActivateCharacterToolbarItem(MyDefinitionId item)
        {
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                MyToolbar toolbar = controlledEntity.Toolbar;
                if (toolbar != null)
                {
                    if (item.TypeId.IsNull)
                    {
                        toolbar.Unselect(false);
                    }
                    else
                    {
                        MyDefinitionBase base2;
                        if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(item, out base2))
                        {
                            MyToolbarItemWeapon weapon = MyToolbarItemFactory.CreateToolbarItem(MyToolbarItemFactory.ObjectBuilderFromDefinition(base2)) as MyToolbarItemWeapon;
                            if (weapon != null)
                            {
                                controlledEntity.SwitchToWeapon(weapon);
                            }
                        }
                    }
                }
            }
        }

        public void ChangeCharacterColor(Color color)
        {
            this.ChangeCharacterColor(color.ColorToHSVDX11());
        }

        public void ChangeCharacterColor(Vector3 hsvColor)
        {
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                controlledEntity.ChangeModelAndColor(controlledEntity.ModelName, hsvColor, false, 0L);
                MyLocalCache.SaveInventoryConfig(controlledEntity);
            }
        }

        public List<MyPhysicalInventoryItem> GetInventoryTools()
        {
            List<MyPhysicalInventoryItem> list = new List<MyPhysicalInventoryItem>();
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                foreach (MyPhysicalInventoryItem item in controlledEntity.GetInventoryBase().GetItems())
                {
                    if (this.m_toolsNames.ContainsKey(item.Content.SubtypeName))
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public MyGameInventoryItemSlot GetToolSlot(string name) => 
            (!this.m_toolsNames.ContainsKey(name) ? MyGameInventoryItemSlot.None : this.m_toolsNames[name]);

        public void LockRotationAxis(GlobalAxis axis)
        {
            this.RotationLocks |= axis;
        }

        public void PlayCharacterAnimation(string animationName)
        {
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (((controlledEntity != null) && (MyDefinitionManager.Static.TryGetAnimationDefinition(animationName) != null)) && controlledEntity.UseNewAnimationSystem)
            {
                controlledEntity.TriggerCharacterAnimationEvent(animationName.ToLower(), true);
            }
        }

        public void PlayRandomCharacterAnimation()
        {
            if (this.m_canPlayAnimation)
            {
                int index = m_random.Next(0, this.m_animations.Length);
                this.PlayCharacterAnimation(this.m_animations[index]);
                this.m_canPlayAnimation = false;
            }
        }

        public void RotateEntity(Vector3 rotation)
        {
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                controlledEntity.MoveAndRotate(Vector3.Zero, new Vector2(0f, rotation.Y) * -3f, 0f);
            }
            else if ((this.m_controlledEntity != null) && this.m_controlledEntity.InScene)
            {
                MatrixD xd;
                MatrixD xd3;
                rotation = (rotation * 3.141593f) / 180f;
                MatrixD.CreateFromYawPitchRoll((double) (rotation.X * this.m_rotationVector.X), (double) (rotation.Y * this.m_rotationVector.Y), (double) (rotation.Z * this.m_rotationVector.Z), out xd);
                xd.Translation = Vector3D.Zero;
                MatrixD worldMatrix = this.m_controlledEntity.WorldMatrix;
                MatrixD.Multiply(ref xd, ref worldMatrix, out xd3);
                this.m_controlledEntity.WorldMatrix = xd3;
            }
        }

        public void SetRotationWithSpeed(Vector3 rotation, float speed)
        {
            this.m_rotationDirection = rotation;
            this.m_rotationSpeed = speed;
        }

        public void ToggleCharacterBackpack()
        {
            MyCharacter controlledEntity = this.m_controlledEntity as MyCharacter;
            if (controlledEntity != null)
            {
                controlledEntity.EnableBag(!controlledEntity.EnabledBag);
            }
        }

        public void ToggleCharacterHelmet()
        {
            IMyControllableEntity controlledEntity = this.m_controlledEntity as IMyControllableEntity;
            if (controlledEntity != null)
            {
                controlledEntity.SwitchHelmet();
            }
        }

        public void UnlockRotationAxis(GlobalAxis axis)
        {
            this.RotationLocks &= ~axis;
        }

        public void Update(bool isMouseOverAnyControl)
        {
            this.m_currentTime += 0.01666667f;
            this.m_currentAnimationTime += 0.01666667f;
            if (this.m_currentAnimationTime > this.m_animationTimer)
            {
                this.m_currentAnimationTime = 0f;
                this.m_canPlayAnimation = true;
            }
            if (MyInput.Static.IsMousePressed(MyMouseButtonsEnum.Left) && !isMouseOverAnyControl)
            {
                this.SetRotationWithSpeed(Vector3.One, MyInput.Static.GetCursorPositionDelta().X * 50f);
            }
            if (MyInput.Static.IsMousePressed(MyMouseButtonsEnum.Right) && !isMouseOverAnyControl)
            {
                this.PlayRandomCharacterAnimation();
            }
            if (MyInput.Static.IsNewMousePressed(MyMouseButtonsEnum.Left) && !isMouseOverAnyControl)
            {
                if ((this.m_lastClickTime + this.m_doubleClickPause) > this.m_currentTime)
                {
                    this.m_buttonClicks++;
                }
                this.m_lastClickTime = this.m_currentTime;
            }
            if (this.m_currentTime > (this.m_lastClickTime + this.m_doubleClickPause))
            {
                this.m_buttonClicks = 0;
                this.m_lastClickTime = this.m_currentTime;
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELMET) || (this.m_buttonClicks == 2))
            {
                this.ToggleCharacterHelmet();
                this.m_buttonClicks = 0;
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.D1))
            {
                this.PlayCharacterAnimation(this.m_animations[0]);
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.D2))
            {
                this.PlayCharacterAnimation(this.m_animations[1]);
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.D3))
            {
                this.PlayCharacterAnimation(this.m_animations[2]);
            }
            if (MyInput.Static.IsNewKeyPressed(MyKeys.D4))
            {
                this.PlayCharacterAnimation(this.m_animations[3]);
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.CROUCH))
            {
                MySession.Static.LocalCharacter.Crouch();
            }
            if (this.m_rotationSpeed != 0f)
            {
                if (this.m_rotationDirection != Vector3.Zero)
                {
                    this.RotateEntity((this.m_rotationDirection * this.m_rotationSpeed) * 0.01666667f);
                }
                this.m_rotationSpeed *= this.m_rotationSpeedDecay;
                if (Math.Abs(this.m_rotationSpeed) < 0.001f)
                {
                    this.m_rotationSpeed = 0f;
                }
            }
        }

        public GlobalAxis RotationLocks
        {
            get => 
                this.m_rotationLocks;
            private set
            {
                this.m_rotationLocks = value;
                this.m_rotationVector = ((((this.m_rotationLocks & GlobalAxis.X) == GlobalAxis.None) ? Vector3.Right : Vector3.Zero) + (((this.m_rotationLocks & GlobalAxis.Y) == GlobalAxis.None) ? Vector3.Up : Vector3.Zero)) + (((this.m_rotationLocks & GlobalAxis.Z) == GlobalAxis.None) ? Vector3.Backward : Vector3.Zero);
            }
        }
    }
}

