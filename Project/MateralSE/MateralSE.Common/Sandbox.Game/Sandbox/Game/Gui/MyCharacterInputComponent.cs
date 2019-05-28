namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Models;
    using VRage.Game.SessionComponents;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;

    internal class MyCharacterInputComponent : MyDebugComponent
    {
        private bool m_toggleMovementState;
        private bool m_toggleShowSkeleton;
        private const int m_maxLastAnimationActions = 20;
        private List<string> m_lastAnimationActions = new List<string>(20);
        private Dictionary<MyCharacterBone, int> m_boneRefToIndex;
        private string m_animationControllerName;

        public MyCharacterInputComponent()
        {
            this.AddShortcut(MyKeys.U, true, false, false, false, () => "Spawn new character", delegate {
                SpawnCharacter(null);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, false, false, false, false, () => "Kill everyone around you", delegate {
                this.KillEveryoneAround();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Use next ship", delegate {
                UseNextShip();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Toggle skeleton view", delegate {
                this.ToggleSkeletonView();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Reload animation tracks", delegate {
                this.ReloadAnimations();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Toggle character movement status", delegate {
                this.ShowMovementState();
                return true;
            });
        }

        public override void Draw()
        {
            base.Draw();
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                MyAnimationInverseKinematics.DebugTransform = MySession.Static.LocalCharacter.WorldMatrix;
            }
            if (this.m_toggleMovementState)
            {
                Vector2 screenCoord = new Vector2(10f, 200f);
                foreach (MyCharacter character in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCharacter>())
                {
                    MyRenderProxy.DebugDrawText2D(screenCoord, character.GetCurrentMovementState().ToString(), Color.Green, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false);
                    screenCoord += new Vector2(0f, 20f);
                }
            }
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                object[] arguments = new object[] { MySession.Static.LocalCharacter.RotationSpeed };
                base.Text("Character look speed: {0}", arguments);
            }
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                object[] arguments = new object[] { MySession.Static.LocalCharacter.CurrentMovementState };
                base.Text("Character state: {0}", arguments);
                object[] objArray3 = new object[] { MySession.Static.LocalCharacter.CharacterGroundState };
                base.Text("Character ground state: {0}", objArray3);
            }
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                object[] arguments = new object[] { MySession.Static.LocalCharacter.HeadMovementXOffset, MySession.Static.LocalCharacter.HeadMovementYOffset };
                base.Text("Character head offset: {0} {1}", arguments);
            }
            if ((MySession.Static != null) && (MySession.Static.LocalCharacter != null))
            {
                MyAnimationControllerComponent animationController = MySession.Static.LocalCharacter.AnimationController;
                StringBuilder builder = new StringBuilder(0x400);
                if (((animationController != null) && (animationController.Controller != null)) && (animationController.Controller.GetLayerByIndex(0) != null))
                {
                    builder.Clear();
                    int[] visitedTreeNodesPath = animationController.Controller.GetLayerByIndex(0).VisitedTreeNodesPath;
                    int index = 0;
                    while (true)
                    {
                        if (index < visitedTreeNodesPath.Length)
                        {
                            int num2 = visitedTreeNodesPath[index];
                            if (num2 != 0)
                            {
                                builder.Append(num2);
                                builder.Append(",");
                                index++;
                                continue;
                            }
                        }
                        base.Text(builder.ToString(), Array.Empty<object>());
                        break;
                    }
                }
                if ((animationController != null) && (animationController.Variables != null))
                {
                    foreach (KeyValuePair<MyStringId, float> pair in animationController.Variables.AllVariables)
                    {
                        builder.Clear();
                        builder.Append(pair.Key);
                        builder.Append(" = ");
                        builder.Append(pair.Value);
                        base.Text(builder.ToString(), Array.Empty<object>());
                    }
                }
                if (animationController != null)
                {
                    if (animationController.LastFrameActions != null)
                    {
                        foreach (MyStringId id in animationController.LastFrameActions)
                        {
                            this.m_lastAnimationActions.Add(id.ToString());
                        }
                        if (this.m_lastAnimationActions.Count > 20)
                        {
                            this.m_lastAnimationActions.RemoveRange(0, this.m_lastAnimationActions.Count - 20);
                        }
                    }
                    base.Text(Color.Red, "--- RECENTLY TRIGGERED ACTIONS ---", Array.Empty<object>());
                    foreach (string str in this.m_lastAnimationActions)
                    {
                        base.Text(Color.Yellow, str, Array.Empty<object>());
                    }
                }
                if ((animationController != null) && (animationController.Controller != null))
                {
                    int layerCount = animationController.Controller.GetLayerCount();
                    for (int i = 0; i < layerCount; i++)
                    {
                        MyAnimationStateMachine layerByIndex = animationController.Controller.GetLayerByIndex(i);
                        if ((layerByIndex != null) && (layerByIndex.CurrentNode != null))
                        {
                            StringBuilder builder2 = new StringBuilder();
                            foreach (MyAnimationStateMachine.MyStateTransitionBlending blending in layerByIndex.StateTransitionBlending)
                            {
                                builder2.AppendFormat(" + {0}(+{1:0.0})", blending.SourceState.Name, blending.TimeLeftInSeconds);
                            }
                            string text = $"{layerByIndex.Name} ... {layerByIndex.CurrentNode.Name}{builder2}";
                            MyRenderProxy.DebugDrawText2D(new Vector2(250f, (float) (150 + (i * 10))), text, Color.Lime, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        }
                    }
                }
            }
            if (this.m_toggleShowSkeleton)
            {
                this.DrawSkeleton();
            }
            MyRenderProxy.DebugDrawText2D(new Vector2(300f, 10f), "Debugging AC " + this.m_animationControllerName, Color.Yellow, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false);
            if (((MySession.Static != null) && ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.Definition != null))) && (MySession.Static.LocalCharacter.Definition.AnimationController == null))
            {
                DictionaryReader<string, MyAnimationPlayerBlendPair> allAnimationPlayers = MySession.Static.LocalCharacter.GetAllAnimationPlayers();
                float y = 40f;
                foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair2 in allAnimationPlayers)
                {
                    string[] textArray1 = new string[6];
                    string[] textArray2 = new string[6];
                    textArray2[0] = (pair2.Key != "") ? pair2.Key : "Body";
                    string[] local1 = textArray2;
                    local1[1] = ": ";
                    local1[2] = pair2.Value.ActualPlayer.AnimationNameDebug;
                    local1[3] = " (";
                    local1[4] = pair2.Value.ActualPlayer.AnimationMwmPathDebug;
                    local1[5] = ")";
                    MyRenderProxy.DebugDrawText2D(new Vector2(400f, y), string.Concat(local1), Color.Lime, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false);
                    y += 30f;
                }
            }
        }

        private void DrawBoneHierarchy(MyCharacter character, ref MatrixD parentTransform, MyCharacterBone[] characterBones, List<MyAnimationClip.BoneState> rawBones, int boneIndex)
        {
            MatrixD xd = (rawBones != null) ? (Matrix.CreateTranslation(rawBones[boneIndex].Translation) * parentTransform) : MatrixD.Identity;
            xd = (rawBones != null) ? (Matrix.CreateFromQuaternion(rawBones[boneIndex].Rotation) * xd) : xd;
            if (rawBones != null)
            {
                MyRenderProxy.DebugDrawLine3D(xd.Translation, parentTransform.Translation, Color.Green, Color.Green, false, false);
            }
            bool flag = false;
            for (int i = 0; characterBones[boneIndex].GetChildBone(i) != null; i++)
            {
                MyCharacterBone childBone = characterBones[boneIndex].GetChildBone(i);
                this.DrawBoneHierarchy(character, ref xd, characterBones, rawBones, this.m_boneRefToIndex[childBone]);
                flag = true;
            }
            if (!flag && (rawBones != null))
            {
                MyRenderProxy.DebugDrawLine3D(xd.Translation, xd.Translation + (xd.Left * 0.05000000074505806), Color.Green, Color.Cyan, false, false);
            }
            MyRenderProxy.DebugDrawText3D(Vector3D.Transform(characterBones[boneIndex].AbsoluteTransform.Translation, character.PositionComp.WorldMatrix), characterBones[boneIndex].Name, Color.Lime, 0.4f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            if (characterBones[boneIndex].Parent != null)
            {
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(characterBones[boneIndex].AbsoluteTransform.Translation, character.PositionComp.WorldMatrix), Vector3D.Transform(characterBones[boneIndex].Parent.AbsoluteTransform.Translation, character.PositionComp.WorldMatrix), Color.Purple, Color.Purple, false, false);
            }
            if (!flag)
            {
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(characterBones[boneIndex].AbsoluteTransform.Translation, character.PositionComp.WorldMatrix), Vector3D.Transform(characterBones[boneIndex].AbsoluteTransform.Translation + (characterBones[boneIndex].AbsoluteTransform.Left * 0.05f), character.PositionComp.WorldMatrix), Color.Purple, Color.Red, false, false);
            }
        }

        private void DrawSkeleton()
        {
            if (this.m_boneRefToIndex == null)
            {
                this.m_boneRefToIndex = new Dictionary<MyCharacterBone, int>(0x100);
            }
            if (MySessionComponentAnimationSystem.Static != null)
            {
                foreach (MyAnimationControllerComponent component in MySessionComponentAnimationSystem.Static.RegisteredAnimationComponents)
                {
                    MyCharacter character = (component != null) ? (component.Entity as MyCharacter) : null;
                    if (character == null)
                    {
                        break;
                    }
                    List<MyAnimationClip.BoneState> lastRawBoneResult = character.AnimationController.LastRawBoneResult;
                    MyCharacterBone[] characterBones = character.AnimationController.CharacterBones;
                    this.m_boneRefToIndex.Clear();
                    int num = 0;
                    while (true)
                    {
                        if (num >= characterBones.Length)
                        {
                            for (int i = 0; i < characterBones.Length; i++)
                            {
                                if (characterBones[i].Parent == null)
                                {
                                    MatrixD worldMatrix = character.PositionComp.WorldMatrix;
                                    this.DrawBoneHierarchy(character, ref worldMatrix, characterBones, lastRawBoneResult, i);
                                }
                            }
                            break;
                        }
                        this.m_boneRefToIndex.Add(character.AnimationController.CharacterBones[num], num);
                        num++;
                    }
                }
            }
        }

        public override string GetName() => 
            "Character";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? base.HandleInput() : false);

        private void KillEveryoneAround()
        {
            if (((MySession.Static.LocalCharacter != null) && (Sync.IsServer && MySession.Static.HasCreativeRights)) && MySession.Static.IsAdminMenuEnabled)
            {
                Vector3D position = MySession.Static.LocalCharacter.PositionComp.GetPosition();
                Vector3D vectord2 = new Vector3D(25.0, 25.0, 25.0);
                BoundingBoxD box = new BoundingBoxD(position - vectord2, position + vectord2);
                List<VRage.Game.Entity.MyEntity> result = new List<VRage.Game.Entity.MyEntity>();
                MyGamePruningStructure.GetAllEntitiesInBox(ref box, result, MyEntityQueryType.Both);
                foreach (VRage.Game.Entity.MyEntity entity in result)
                {
                    MyCharacter character = entity as MyCharacter;
                    if ((character != null) && !ReferenceEquals(entity, MySession.Static.LocalCharacter))
                    {
                        character.DoDamage(1000000f, MyDamageType.Debug, true, 0L);
                    }
                }
                MyRenderProxy.DebugDrawAABB(box, Color.Red, 0.5f, 1f, true, true, false);
            }
        }

        private void ReloadAnimations()
        {
            if (MySession.Static.LocalCharacter != null)
            {
                foreach (KeyValuePair<string, MyAnimationPlayerBlendPair> pair in MySession.Static.LocalCharacter.GetAllAnimationPlayers())
                {
                    MySession.Static.LocalCharacter.PlayerStop(pair.Key, 0f);
                }
            }
            foreach (MyAnimationDefinition local1 in MyDefinitionManager.Static.GetAnimationDefinitions())
            {
                MyModel model = MyModels.GetModel(local1.AnimationModel);
                if (model != null)
                {
                    model.UnloadData();
                }
                MyModel model2 = MyModels.GetModel(local1.AnimationModelFPS);
                if (model2 != null)
                {
                    model2.UnloadData();
                }
            }
            MySessionComponentAnimationSystem.Static.ReloadMwmTracks();
        }

        private void ShowMovementState()
        {
            this.m_toggleMovementState = !this.m_toggleMovementState;
        }

        public static MyCharacter SpawnCharacter(string model = null)
        {
            MyCharacter character = MySession.Static.LocalHumanPlayer?.Identity.Character;
            Vector3? colorMask = null;
            string characterName = (MySession.Static.LocalHumanPlayer == null) ? "" : MySession.Static.LocalHumanPlayer.Identity.DisplayName;
            string str2 = (MySession.Static.LocalHumanPlayer == null) ? MyCharacter.DefaultModel : MySession.Static.LocalHumanPlayer.Identity.Model;
            long identityId = (MySession.Static.LocalHumanPlayer == null) ? 0L : MySession.Static.LocalHumanPlayer.Identity.IdentityId;
            if (character != null)
            {
                colorMask = new Vector3?(character.ColorMask);
            }
            return MyCharacter.CreateCharacter(MatrixD.CreateTranslation((MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 6f)) + (MySector.MainCamera.LeftVector * 3f)), Vector3.Zero, characterName, model ?? str2, colorMask, null, false, false, null, true, identityId, true);
        }

        private void ToggleSkeletonView()
        {
            this.m_toggleShowSkeleton = !this.m_toggleShowSkeleton;
        }

        private static void UseCockpit(MyCockpit cockpit)
        {
            if (MySession.Static.LocalHumanPlayer != null)
            {
                if (MySession.Static.ControlledEntity is MyCockpit)
                {
                    MySession.Static.ControlledEntity.Use();
                }
                cockpit.RequestUse(UseActionEnum.Manipulate, MySession.Static.LocalHumanPlayer.Identity.Character);
                cockpit.RemoveOriginalPilotPosition();
            }
        }

        public static void UseNextShip()
        {
            MyCockpit cockpit = null;
            object obj2 = null;
            using (IEnumerator<MyCubeGrid> enumerator = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IEnumerator<MyCockpit> enumerator2 = (from s in enumerator.Current.GetBlocks()
                        select s.FatBlock as MyCockpit into s
                        where s != null
                        select s).GetEnumerator();
                    try
                    {
                        while (true)
                        {
                            if (!enumerator2.MoveNext())
                            {
                                break;
                            }
                            MyCockpit current = enumerator2.Current;
                            if ((cockpit == null) && (current.Pilot == null))
                            {
                                cockpit = current;
                            }
                            if (obj2 != MySession.Static.ControlledEntity)
                            {
                                obj2 = current;
                            }
                            else if (current.Pilot == null)
                            {
                                UseCockpit(current);
                                return;
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (enumerator2 == null)
                        {
                            continue;
                        }
                        enumerator2.Dispose();
                        continue;
                    }
                    return;
                }
            }
            if (cockpit != null)
            {
                UseCockpit(cockpit);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCharacterInputComponent.<>c <>9 = new MyCharacterInputComponent.<>c();
            public static Func<string> <>9__5_0;
            public static Func<bool> <>9__5_1;
            public static Func<string> <>9__5_2;
            public static Func<string> <>9__5_4;
            public static Func<bool> <>9__5_5;
            public static Func<string> <>9__5_6;
            public static Func<string> <>9__5_8;
            public static Func<string> <>9__5_10;
            public static Func<MySlimBlock, MyCockpit> <>9__11_0;
            public static Func<MyCockpit, bool> <>9__11_1;

            internal string <.ctor>b__5_0() => 
                "Spawn new character";

            internal bool <.ctor>b__5_1()
            {
                MyCharacterInputComponent.SpawnCharacter(null);
                return true;
            }

            internal string <.ctor>b__5_10() => 
                "Toggle character movement status";

            internal string <.ctor>b__5_2() => 
                "Kill everyone around you";

            internal string <.ctor>b__5_4() => 
                "Use next ship";

            internal bool <.ctor>b__5_5()
            {
                MyCharacterInputComponent.UseNextShip();
                return true;
            }

            internal string <.ctor>b__5_6() => 
                "Toggle skeleton view";

            internal string <.ctor>b__5_8() => 
                "Reload animation tracks";

            internal MyCockpit <UseNextShip>b__11_0(MySlimBlock s) => 
                (s.FatBlock as MyCockpit);

            internal bool <UseNextShip>b__11_1(MyCockpit s) => 
                (s != null);
        }
    }
}

