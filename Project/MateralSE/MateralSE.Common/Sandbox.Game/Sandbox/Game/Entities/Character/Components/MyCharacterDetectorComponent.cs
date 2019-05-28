namespace Sandbox.Game.Entities.Character.Components
{
    using Havok;
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Import;

    public abstract class MyCharacterDetectorComponent : MyCharacterComponent
    {
        private IMyEntity m_detectedEntity;
        private IMyUseObject m_interactiveObject;
        protected static List<VRage.Game.Entity.MyEntity> m_detectableEntities = new List<VRage.Game.Entity.MyEntity>();
        protected MyHudNotification m_useObjectNotification;
        protected MyHudNotification m_showTerminalNotification;
        protected MyHudNotification m_openInventoryNotification;
        protected bool m_usingContinuously;
        [CompilerGenerated]
        private static Action<IMyUseObject> OnInteractiveObjectChanged;
        [CompilerGenerated]
        private static Action<IMyUseObject> OnInteractiveObjectUsed;
        protected MyCharacterHitInfo CharHitInfo;

        public static  event Action<IMyUseObject> OnInteractiveObjectChanged
        {
            [CompilerGenerated] add
            {
                Action<IMyUseObject> onInteractiveObjectChanged = OnInteractiveObjectChanged;
                while (true)
                {
                    Action<IMyUseObject> a = onInteractiveObjectChanged;
                    Action<IMyUseObject> action3 = (Action<IMyUseObject>) Delegate.Combine(a, value);
                    onInteractiveObjectChanged = Interlocked.CompareExchange<Action<IMyUseObject>>(ref OnInteractiveObjectChanged, action3, a);
                    if (ReferenceEquals(onInteractiveObjectChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyUseObject> onInteractiveObjectChanged = OnInteractiveObjectChanged;
                while (true)
                {
                    Action<IMyUseObject> source = onInteractiveObjectChanged;
                    Action<IMyUseObject> action3 = (Action<IMyUseObject>) Delegate.Remove(source, value);
                    onInteractiveObjectChanged = Interlocked.CompareExchange<Action<IMyUseObject>>(ref OnInteractiveObjectChanged, action3, source);
                    if (ReferenceEquals(onInteractiveObjectChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<IMyUseObject> OnInteractiveObjectUsed
        {
            [CompilerGenerated] add
            {
                Action<IMyUseObject> onInteractiveObjectUsed = OnInteractiveObjectUsed;
                while (true)
                {
                    Action<IMyUseObject> a = onInteractiveObjectUsed;
                    Action<IMyUseObject> action3 = (Action<IMyUseObject>) Delegate.Combine(a, value);
                    onInteractiveObjectUsed = Interlocked.CompareExchange<Action<IMyUseObject>>(ref OnInteractiveObjectUsed, action3, a);
                    if (ReferenceEquals(onInteractiveObjectUsed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<IMyUseObject> onInteractiveObjectUsed = OnInteractiveObjectUsed;
                while (true)
                {
                    Action<IMyUseObject> source = onInteractiveObjectUsed;
                    Action<IMyUseObject> action3 = (Action<IMyUseObject>) Delegate.Remove(source, value);
                    onInteractiveObjectUsed = Interlocked.CompareExchange<Action<IMyUseObject>>(ref OnInteractiveObjectUsed, action3, source);
                    if (ReferenceEquals(onInteractiveObjectUsed, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyCharacterDetectorComponent()
        {
        }

        protected void DisableDetectors()
        {
            using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = m_detectableEntities.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyUseObjectsComponentBase base2;
                    if (!enumerator.Current.Components.TryGet<MyUseObjectsComponentBase>(out base2))
                    {
                        continue;
                    }
                    if (base2.DetectorPhysics != null)
                    {
                        base2.DetectorPhysics.Enabled = false;
                    }
                }
            }
            m_detectableEntities.Clear();
        }

        public void DoDetection()
        {
            if (base.Character != null)
            {
                this.DoDetection(!base.Character.TargetFromCamera);
            }
        }

        protected abstract void DoDetection(bool useHead);
        protected void EnableDetectorsInArea(Vector3D from)
        {
            this.GatherDetectorsInArea(from);
            for (int i = 0; i < m_detectableEntities.Count; i++)
            {
                MyUseObjectsComponentBase base2;
                VRage.Game.Entity.MyEntity entity = m_detectableEntities[i];
                MyCompoundCubeBlock block = entity as MyCompoundCubeBlock;
                if (block != null)
                {
                    foreach (MySlimBlock block2 in block.GetBlocks())
                    {
                        if (block2.FatBlock != null)
                        {
                            m_detectableEntities.Add(block2.FatBlock);
                        }
                    }
                }
                if (entity.Components.TryGet<MyUseObjectsComponentBase>(out base2) && (base2.DetectorPhysics != null))
                {
                    base2.PositionChanged(base2.Container.Get<MyPositionComponentBase>());
                    base2.DetectorPhysics.Enabled = true;
                }
            }
        }

        protected void GatherDetectorsInArea(Vector3D from)
        {
            BoundingSphereD sphere = new BoundingSphereD(from, (double) MyConstants.DEFAULT_INTERACTIVE_DISTANCE);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, m_detectableEntities, MyEntityQueryType.Both);
        }

        private void GetNotification(IMyUseObject useObject, UseActionEnum actionType, ref MyHudNotification notification)
        {
            if ((useObject.SupportedActions & actionType) != UseActionEnum.None)
            {
                MyActionDescription actionInfo = useObject.GetActionInfo(actionType);
                base.Character.RemoveNotification(ref notification);
                notification = new MyHudNotification(actionInfo.Text, 0, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, actionInfo.IsTextControlHint ? MyNotificationLevel.Control : MyNotificationLevel.Normal);
                if (!MyDebugDrawSettings.DEBUG_DRAW_JOYSTICK_CONTROL_HINTS && (!MyInput.Static.IsJoystickConnected() || !MyInput.Static.IsJoystickLastUsed))
                {
                    notification.SetTextFormatArguments(actionInfo.FormatParams);
                }
                else
                {
                    if (actionInfo.JoystickText != null)
                    {
                        notification.Text = actionInfo.JoystickText.Value;
                    }
                    if (actionInfo.JoystickFormatParams != null)
                    {
                        notification.SetTextFormatArguments(actionInfo.JoystickFormatParams);
                    }
                }
            }
        }

        protected static void HandleInteractiveObject(IMyUseObject interactive)
        {
            if (!MyFakes.ENABLE_USE_NEW_OBJECT_HIGHLIGHT)
            {
                MyHud.SelectedObjectHighlight.HighlightAttribute = null;
                MyHud.SelectedObjectHighlight.HighlightStyle = MyHudObjectHighlightStyle.DummyHighlight;
            }
            else
            {
                MyHud.SelectedObjectHighlight.Color = MySector.EnvironmentDefinition.ContourHighlightColor;
                if (((interactive.InstanceID != -1) || (interactive is MyFloatingObject)) || (interactive.Owner is MyInventoryBagEntity))
                {
                    MyHud.SelectedObjectHighlight.HighlightAttribute = null;
                    MyHud.SelectedObjectHighlight.HighlightStyle = MyHudObjectHighlightStyle.OutlineHighlight;
                }
                else
                {
                    MyCharacter character = interactive as MyCharacter;
                    if ((character != null) && character.IsDead)
                    {
                        MyHud.SelectedObjectHighlight.HighlightAttribute = null;
                        MyHud.SelectedObjectHighlight.HighlightStyle = MyHudObjectHighlightStyle.OutlineHighlight;
                    }
                    else
                    {
                        bool flag = false;
                        MyModelDummy dummy = interactive.Dummy;
                        if ((dummy != null) && (dummy.CustomData != null))
                        {
                            object obj2;
                            flag = dummy.CustomData.TryGetValue("highlight", out obj2);
                            string str = obj2 as string;
                            if (flag && (str != null))
                            {
                                MyHud.SelectedObjectHighlight.HighlightAttribute = str;
                                MyHud.SelectedObjectHighlight.HighlightStyle = !(interactive.Owner is MyTextPanel) ? MyHudObjectHighlightStyle.OutlineHighlight : MyHudObjectHighlightStyle.EdgeHighlight;
                            }
                            bool flag1 = dummy.CustomData.TryGetValue("highlighttype", out obj2);
                            string str2 = obj2 as string;
                            if (flag1 && (str2 != null))
                            {
                                MyHud.SelectedObjectHighlight.HighlightStyle = (str2 != "edge") ? MyHudObjectHighlightStyle.OutlineHighlight : MyHudObjectHighlightStyle.EdgeHighlight;
                            }
                        }
                        if (!flag)
                        {
                            MyHud.SelectedObjectHighlight.HighlightAttribute = null;
                            MyHud.SelectedObjectHighlight.HighlightStyle = MyHudObjectHighlightStyle.DummyHighlight;
                        }
                    }
                }
            }
            MyCubeBlock owner = interactive.Owner as MyCubeBlock;
            if ((owner != null) && (owner.GetPlayerRelationToOwner() == MyRelationsBetweenPlayerAndBlock.Enemies))
            {
                MyHud.SelectedObjectHighlight.Color = MySector.EnvironmentDefinition.ContourHighlightColorAccessDenied;
            }
            MyHud.SelectedObjectHighlight.Highlight(interactive);
        }

        private void InteractiveObjectChanged()
        {
            if (ReferenceEquals(MySession.Static.ControlledEntity, base.Character))
            {
                if (this.UseObject != null)
                {
                    this.GetNotification(this.UseObject, UseActionEnum.Manipulate, ref this.m_useObjectNotification);
                    this.GetNotification(this.UseObject, UseActionEnum.OpenTerminal, ref this.m_showTerminalNotification);
                    this.GetNotification(this.UseObject, UseActionEnum.OpenInventory, ref this.m_openInventoryNotification);
                    MyStringId id = (this.m_useObjectNotification != null) ? this.m_useObjectNotification.Text : MySpaceTexts.Blank;
                    MyStringId id2 = (this.m_showTerminalNotification != null) ? this.m_showTerminalNotification.Text : MySpaceTexts.Blank;
                    MyStringId id3 = (this.m_openInventoryNotification != null) ? this.m_openInventoryNotification.Text : MySpaceTexts.Blank;
                    if (id != MySpaceTexts.Blank)
                    {
                        MyHud.Notifications.Add(this.m_useObjectNotification);
                    }
                    if ((id2 != MySpaceTexts.Blank) && (id2 != id))
                    {
                        MyHud.Notifications.Add(this.m_showTerminalNotification);
                    }
                    if (((id3 != MySpaceTexts.Blank) && (id3 != id2)) && (id3 != id))
                    {
                        MyHud.Notifications.Add(this.m_openInventoryNotification);
                    }
                }
                if (OnInteractiveObjectChanged != null)
                {
                    OnInteractiveObjectChanged(this.UseObject);
                }
            }
        }

        private void InteractiveObjectRemoved()
        {
            if (base.Character != null)
            {
                base.Character.RemoveNotification(ref this.m_useObjectNotification);
                base.Character.RemoveNotification(ref this.m_showTerminalNotification);
                base.Character.RemoveNotification(ref this.m_openInventoryNotification);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            base.NeedsUpdateAfterSimulation10 = true;
        }

        public override void OnCharacterDead()
        {
            this.UseObject = null;
            base.OnCharacterDead();
        }

        protected virtual void OnDetectedEntityMarkForClose(IMyEntity obj)
        {
            this.DetectedEntity = null;
            if (this.UseObject != null)
            {
                this.UseObject = null;
                MyHud.SelectedObjectHighlight.RemoveHighlight();
            }
        }

        public override void OnRemovedFromScene()
        {
            this.UseObject = null;
            base.OnRemovedFromScene();
        }

        public void RaiseObjectUsed()
        {
            if (OnInteractiveObjectUsed != null)
            {
                OnInteractiveObjectUsed(this.UseObject);
            }
        }

        public override void UpdateAfterSimulation10()
        {
            if ((this.m_useObjectNotification != null) && !this.m_usingContinuously)
            {
                MyHud.Notifications.Add(this.m_useObjectNotification);
            }
            this.m_usingContinuously = false;
            if (!base.Character.IsSitting && !base.Character.IsDead)
            {
                MySandboxGame.Static.Invoke(new Action(this.DoDetection), "MyCharacterDetectorComponent::DoDetection");
            }
            else if (ReferenceEquals(MySession.Static.ControlledEntity, base.Character))
            {
                MyHud.SelectedObjectHighlight.RemoveHighlight();
            }
        }

        private void UseClose()
        {
            if (((base.Character != null) && (this.UseObject != null)) && this.UseObject.IsActionSupported(UseActionEnum.Close))
            {
                this.UseObject.Use(UseActionEnum.Close, base.Character);
            }
        }

        public void UseContinues()
        {
            MyHud.Notifications.Remove(this.m_useObjectNotification);
            this.m_usingContinuously = true;
        }

        public IMyUseObject UseObject
        {
            get => 
                this.m_interactiveObject;
            set
            {
                if (!ReferenceEquals(value, this.m_interactiveObject))
                {
                    if (this.m_interactiveObject != null)
                    {
                        this.UseClose();
                        this.m_interactiveObject.OnSelectionLost();
                        this.InteractiveObjectRemoved();
                    }
                    this.m_interactiveObject = value;
                    this.InteractiveObjectChanged();
                }
            }
        }

        public IMyEntity DetectedEntity
        {
            get => 
                this.m_detectedEntity;
            protected set
            {
                if (this.m_detectedEntity != null)
                {
                    this.m_detectedEntity.OnMarkForClose -= new Action<IMyEntity>(this.OnDetectedEntityMarkForClose);
                }
                this.m_detectedEntity = value;
                if (this.m_detectedEntity != null)
                {
                    this.m_detectedEntity.OnMarkForClose += new Action<IMyEntity>(this.OnDetectedEntityMarkForClose);
                }
            }
        }

        public Vector3D HitPosition { get; protected set; }

        public Vector3 HitNormal { get; protected set; }

        public uint ShapeKey { get; protected set; }

        public Vector3D StartPosition { get; protected set; }

        public MyStringHash HitMaterial { get; protected set; }

        public HkRigidBody HitBody { get; protected set; }

        public object HitTag { get; protected set; }
    }
}

