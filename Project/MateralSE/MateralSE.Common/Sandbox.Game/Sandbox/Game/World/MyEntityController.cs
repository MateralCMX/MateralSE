namespace Sandbox.Game.World
{
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Interfaces;

    public class MyEntityController : IMyEntityController
    {
        private Action<MyEntity> m_controlledEntityClosing;
        [CompilerGenerated]
        private Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> ControlledEntityChanged;

        public event Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> ControlledEntityChanged
        {
            [CompilerGenerated] add
            {
                Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> controlledEntityChanged = this.ControlledEntityChanged;
                while (true)
                {
                    Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> a = controlledEntityChanged;
                    Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> action3 = (Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>) Delegate.Combine(a, value);
                    controlledEntityChanged = Interlocked.CompareExchange<Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>>(ref this.ControlledEntityChanged, action3, a);
                    if (ReferenceEquals(controlledEntityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> controlledEntityChanged = this.ControlledEntityChanged;
                while (true)
                {
                    Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> source = controlledEntityChanged;
                    Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> action3 = (Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>) Delegate.Remove(source, value);
                    controlledEntityChanged = Interlocked.CompareExchange<Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>>(ref this.ControlledEntityChanged, action3, source);
                    if (ReferenceEquals(controlledEntityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<VRage.Game.ModAPI.Interfaces.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity> IMyEntityController.ControlledEntityChanged
        {
            add
            {
                this.ControlledEntityChanged += this.GetDelegate(value);
            }
            remove
            {
                this.ControlledEntityChanged -= this.GetDelegate(value);
            }
        }

        public MyEntityController(MyPlayer parent)
        {
            this.Player = parent;
            this.m_controlledEntityClosing = new Action<MyEntity>(this.ControlledEntity_OnClosing);
        }

        private void ControlledEntity_OnClosing(MyEntity entity)
        {
            if (this.ControlledEntity != null)
            {
                this.TakeControl(null);
            }
        }

        private Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> GetDelegate(Action<VRage.Game.ModAPI.Interfaces.IMyControllableEntity, VRage.Game.ModAPI.Interfaces.IMyControllableEntity> value) => 
            ((Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>) Delegate.CreateDelegate(typeof(Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity>), value.Target, value.Method));

        private void RaiseControlledEntityChanged(Sandbox.Game.Entities.IMyControllableEntity old, Sandbox.Game.Entities.IMyControllableEntity entity)
        {
            Action<Sandbox.Game.Entities.IMyControllableEntity, Sandbox.Game.Entities.IMyControllableEntity> controlledEntityChanged = this.ControlledEntityChanged;
            if (controlledEntityChanged != null)
            {
                controlledEntityChanged(old, entity);
            }
        }

        public void SaveCamera()
        {
            if ((this.ControlledEntity != null) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                bool isLocalCharacter = (this.ControlledEntity is MyCharacter) && ReferenceEquals(MySession.Static.LocalCharacter, this.ControlledEntity);
                if (!(this.ControlledEntity is MyCharacter) || ReferenceEquals(MySession.Static.LocalCharacter, this.ControlledEntity))
                {
                    MyEntityCameraSettings cameraEntitySettings = this.ControlledEntity.GetCameraEntitySettings();
                    float headLocalXAngle = this.ControlledEntity.HeadLocalXAngle;
                    float headLocalYAngle = this.ControlledEntity.HeadLocalYAngle;
                    bool isFirstPerson = (cameraEntitySettings != null) ? cameraEntitySettings.IsFirstPerson : (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator);
                    MySession.Static.Cameras.SaveEntityCameraSettings(this.Player.Id, this.ControlledEntity.Entity.EntityId, isFirstPerson, MyThirdPersonSpectator.Static.GetViewerDistance(), isLocalCharacter, headLocalXAngle, headLocalYAngle, true);
                }
            }
        }

        public void SetCamera()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && (this.ControlledEntity.Entity is IMyCameraController))
            {
                MySession.Static.SetEntityCameraPosition(this.Player.Id, this.ControlledEntity.Entity);
            }
        }

        public void TakeControl(Sandbox.Game.Entities.IMyControllableEntity entity)
        {
            if (!ReferenceEquals(this.ControlledEntity, entity) && ((entity == null) || (entity.ControllerInfo.Controller == null)))
            {
                Sandbox.Game.Entities.IMyControllableEntity controlledEntity = this.ControlledEntity;
                this.SaveCamera();
                if (this.ControlledEntity != null)
                {
                    this.ControlledEntity.Entity.OnClosing -= this.m_controlledEntityClosing;
                    this.ControlledEntity.ControllerInfo.Controller = null;
                }
                this.ControlledEntity = entity;
                if (entity != null)
                {
                    this.ControlledEntity.Entity.OnClosing += this.m_controlledEntityClosing;
                    this.ControlledEntity.ControllerInfo.Controller = this;
                    this.SetCamera();
                }
                if (!ReferenceEquals(controlledEntity, entity))
                {
                    this.RaiseControlledEntityChanged(controlledEntity, entity);
                }
            }
        }

        void IMyEntityController.TakeControl(VRage.Game.ModAPI.Interfaces.IMyControllableEntity entity)
        {
            if (entity is Sandbox.Game.Entities.IMyControllableEntity)
            {
                this.TakeControl(entity as Sandbox.Game.Entities.IMyControllableEntity);
            }
        }

        public Sandbox.Game.Entities.IMyControllableEntity ControlledEntity { get; protected set; }

        public MyPlayer Player { get; private set; }

        VRage.Game.ModAPI.Interfaces.IMyControllableEntity IMyEntityController.ControlledEntity =>
            this.ControlledEntity;
    }
}

