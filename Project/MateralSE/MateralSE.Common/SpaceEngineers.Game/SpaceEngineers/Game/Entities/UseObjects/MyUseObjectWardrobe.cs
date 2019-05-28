namespace SpaceEngineers.Game.Entities.UseObjects
{
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    [MyUseObject("wardrobe")]
    internal class MyUseObjectWardrobe : MyUseObjectBase
    {
        public readonly MyCubeBlock Block;
        public readonly Matrix LocalMatrix;

        public MyUseObjectWardrobe(IMyEntity owner, string dummyName, MyModelDummy dummyData, uint key) : base(owner, dummyData)
        {
            this.Block = owner as MyCubeBlock;
            this.LocalMatrix = dummyData.Matrix;
        }

        private void ActiveGameplayScreen_Closed(MyGuiScreenBase source)
        {
            MyMedicalRoom block = this.Block as MyMedicalRoom;
            if (block != null)
            {
                block.StopUsingWardrobe();
            }
            MySessionComponentContainerDropSystem component = MySession.Static.GetComponent<MySessionComponentContainerDropSystem>();
            if (component != null)
            {
                component.EnableWindowPopups = true;
            }
            if (MyGuiScreenGamePlay.ActiveGameplayScreen != null)
            {
                MyGuiScreenGamePlay.ActiveGameplayScreen.Closed -= new MyGuiScreenBase.ScreenHandler(this.ActiveGameplayScreen_Closed);
                MyGuiScreenGamePlay.ActiveGameplayScreen = null;
            }
        }

        public override MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription description = new MyActionDescription {
                Text = MyCommonTexts.NotificationHintPressToUseWardrobe
            };
            description.FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyControlsSpace.USE) + "]" };
            description.IsTextControlHint = true;
            description.JoystickFormatParams = new object[] { "[" + MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.USE).ToString() + "]" };
            return description;
        }

        public override bool HandleInput() => 
            false;

        public override void OnSelectionLost()
        {
        }

        public override void Use(UseActionEnum actionEnum, IMyEntity entity)
        {
            MyCharacter user = entity as MyCharacter;
            if (!this.Block.GetUserRelationToOwner(user.ControllerInfo.ControllingIdentityId).IsFriendly() && !MySession.Static.AdminSettings.HasFlag(AdminSettingsEnum.UseTerminals))
            {
                if (user.ControllerInfo.IsLocallyHumanControlled())
                {
                    MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                }
            }
            else if (actionEnum == UseActionEnum.Manipulate)
            {
                MyMedicalRoom block = this.Block as MyMedicalRoom;
                if ((block != null) && block.IsWorking)
                {
                    if (!block.SuitChangeAllowed)
                    {
                        MyHud.Notifications.Add(MyNotificationSingletons.AccessDenied);
                    }
                    else
                    {
                        MyHud.SelectedObjectHighlight.HighlightStyle = MyHudObjectHighlightStyle.None;
                        bool flag = (user.Definition.Name == "Default_Astronaut") || (user.Definition.Name == "Default_Astronaut_Female");
                        if (block.CustomWardrobesEnabled)
                        {
                            if (!(MyGameService.IsActive & flag))
                            {
                                MyGuiScreenWardrobe screen = new MyGuiScreenWardrobe(user, block.CustomWardrobeNames);
                                MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                                MyGuiSandbox.AddScreen(screen);
                            }
                            else
                            {
                                MySessionComponentContainerDropSystem component = MySession.Static.GetComponent<MySessionComponentContainerDropSystem>();
                                if (component != null)
                                {
                                    component.EnableWindowPopups = false;
                                }
                                MyGuiScreenLoadInventory screen = new MyGuiScreenLoadInventory(true, block.CustomWardrobeNames);
                                MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                                MyGuiSandbox.AddScreen(screen);
                                MyGuiScreenGamePlay.ActiveGameplayScreen.Closed += new MyGuiScreenBase.ScreenHandler(this.ActiveGameplayScreen_Closed);
                                block.UseWardrobe(user);
                            }
                        }
                        else if (MyGameService.IsActive & flag)
                        {
                            MySessionComponentContainerDropSystem component = MySession.Static.GetComponent<MySessionComponentContainerDropSystem>();
                            if (component != null)
                            {
                                component.EnableWindowPopups = false;
                            }
                            MyGuiScreenLoadInventory screen = new MyGuiScreenLoadInventory(true, null);
                            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                            MyGuiSandbox.AddScreen(screen);
                            MyGuiScreenGamePlay.ActiveGameplayScreen.Closed += new MyGuiScreenBase.ScreenHandler(this.ActiveGameplayScreen_Closed);
                            block.UseWardrobe(user);
                        }
                        else
                        {
                            MyGuiScreenWardrobe screen = new MyGuiScreenWardrobe(user, null);
                            MyGuiScreenGamePlay.ActiveGameplayScreen = screen;
                            MyGuiSandbox.AddScreen(screen);
                        }
                    }
                }
            }
        }

        public override float InteractiveDistance =>
            MyConstants.DEFAULT_INTERACTIVE_DISTANCE;

        public override MatrixD ActivationMatrix =>
            (this.LocalMatrix * this.Block.WorldMatrix);

        public override MatrixD WorldMatrix =>
            this.Block.WorldMatrix;

        public override uint RenderObjectID =>
            this.Block.Render.GetRenderObjectID();

        public override int InstanceID =>
            -1;

        public override bool ShowOverlay =>
            true;

        public override UseActionEnum SupportedActions =>
            (this.PrimaryAction | this.SecondaryAction);

        public override UseActionEnum PrimaryAction =>
            UseActionEnum.Manipulate;

        public override UseActionEnum SecondaryAction =>
            UseActionEnum.None;

        public override bool ContinuousUsage =>
            false;

        public override bool PlayIndicatorSound =>
            true;
    }
}

