namespace Sandbox.Game.Components
{
    using Havok;
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Lights;
    using Sandbox.Game.Utils;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Ansel;
    using VRage.Game;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Lights;

    internal class MyRenderComponentCharacter : MyRenderComponentSkinnedEntity
    {
        private static readonly MyStringId ID_REFLECTOR_CONE = MyStringId.GetOrCompute("ReflectorConeCharacter");
        private static readonly MyStringId ID_REFLECTOR_GLARE = MyStringId.GetOrCompute("ReflectorGlareAlphaBlended");
        private static readonly MyStringHash ID_CHARACTER = MyStringHash.GetOrCompute("Character");
        private static readonly int MAX_DISCONNECT_ICON_DISTANCE = 50;
        private int m_lastWalkParticleCheckTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
        private int m_walkParticleSpawnCounterMs = 0x3e8;
        private const int m_walkParticleGravityDelay = 0x2710;
        private const int m_walkParticleJetpackOffDelay = 0x7d0;
        private const int m_walkParticleDefaultDelay = 0x3e8;
        private uint m_cullRenderId = uint.MaxValue;
        private List<MyJetpackThrust> m_jetpackThrusts = new List<MyJetpackThrust>(8);
        private MyLight m_light;
        private MyLight m_flareLeft;
        private MyLight m_flareRight;
        private Vector3D m_leftGlarePosition;
        private Vector3D m_rightGlarePosition;
        private int m_leftLightIndex = -1;
        private int m_rightLightIndex = -1;
        private float m_oldReflectorAngle = -1f;
        private Vector3 m_lightLocalPosition;
        private const float HIT_INDICATOR_LENGTH = 0.8f;
        private float m_currentHitIndicatorCounter;
        public static float JETPACK_LIGHT_INTENSITY_BASE = 9f;
        public static float JETPACK_LIGHT_INTENSITY_LENGTH = 200f;
        public static float JETPACK_LIGHT_RANGE_RADIUS = 1.2f;
        public static float JETPACK_LIGHT_RANGE_LENGTH = 0.3f;
        public static float JETPACK_GLARE_INTENSITY_BASE = 0.06f;
        public static float JETPACK_GLARE_INTENSITY_LENGTH = 0f;
        public static float JETPACK_GLARE_SIZE_RADIUS = 2.49f;
        public static float JETPACK_GLARE_SIZE_LENGTH = 0.4f;
        public static float JETPACK_THRUST_INTENSITY_BASE = 0.6f;
        public static float JETPACK_THRUST_INTENSITY = 10f;
        public static float JETPACK_THRUST_THICKNESS = 0.5f;
        public static float JETPACK_THRUST_LENGTH = 0.6f;
        public static float JETPACK_THRUST_OFFSET = -0.22f;
        private readonly MyFlareDefinition m_flareJetpack;
        private readonly MyFlareDefinition m_flareHeadlamp;

        public MyRenderComponentCharacter()
        {
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), "Jetpack");
            MyFlareDefinition definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            this.m_flareJetpack = definition ?? new MyFlareDefinition();
            id = new MyDefinitionId(typeof(MyObjectBuilder_FlareDefinition), "Headlamp");
            definition = MyDefinitionManager.Static.GetDefinition(id) as MyFlareDefinition;
            this.m_flareHeadlamp = definition ?? new MyFlareDefinition();
        }

        public override void AddRenderObjects()
        {
            base.AddRenderObjects();
            this.m_cullRenderId = MyRenderProxy.CreateManualCullObject(base.Entity.DisplayName + " ManualCullObject", base.Entity.WorldMatrix);
            base.SetParent(0, this.m_cullRenderId, new Matrix?(Matrix.Identity));
            BoundingBox localAABB = base.Entity.LocalAABB;
            localAABB.Scale(new Vector3(1.5f, 2f, 1.5f));
            MatrixD? worldMatrix = null;
            Matrix? localMatrix = null;
            MyRenderProxy.UpdateRenderObject(base.GetRenderObjectID(), worldMatrix, new BoundingBox?(localAABB), -1, localMatrix);
        }

        public void CleanLights()
        {
            if (this.m_light != null)
            {
                MyLights.RemoveLight(this.m_light);
                this.m_light = null;
            }
            if (this.m_flareLeft != null)
            {
                MyLights.RemoveLight(this.m_flareLeft);
                this.m_flareLeft = null;
            }
            if (this.m_flareRight != null)
            {
                MyLights.RemoveLight(this.m_flareRight);
                this.m_flareRight = null;
            }
            using (List<MyJetpackThrust>.Enumerator enumerator = this.m_jetpackThrusts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyLights.RemoveLight(enumerator.Current.Light);
                }
            }
            this.m_jetpackThrusts.Clear();
        }

        private MyLight CreateFlare(string debugName)
        {
            MyLight light = MyLights.AddLight();
            if (light != null)
            {
                light.Start(base.Entity.DisplayName + " Reflector " + debugName + " Flare");
                light.ReflectorOn = false;
                light.LightOn = false;
                light.Color = MyCharacter.POINT_COLOR;
                light.GlareOn = true;
                light.GlareIntensity = this.m_flareHeadlamp.Intensity;
                light.GlareSize = this.m_flareHeadlamp.Size;
                light.SubGlares = this.m_flareHeadlamp.SubGlares;
                light.GlareQuerySize = 0.05f;
                light.GlareMaxDistance = 40f;
                light.GlareType = MyGlareTypeEnum.Directional;
            }
            return light;
        }

        public void Damage()
        {
            this.m_currentHitIndicatorCounter = 0.8f;
        }

        public override void Draw()
        {
            base.Draw();
            if (this.m_light != null)
            {
                bool flag = true;
                MyCharacter skinnedEntity = base.m_skinnedEntity as MyCharacter;
                float num = Vector3.DistanceSquared((Vector3) skinnedEntity.PositionComp.GetPosition(), (Vector3) MySector.MainCamera.Position);
                if (num < 1600f)
                {
                    Vector3 reflectorDirection = this.m_light.ReflectorDirection;
                    float length = 2.56f;
                    float thickness = 0.48f;
                    Vector3 vector2 = new Vector3((Vector4) this.m_light.ReflectorColor);
                    Vector3D vectord = this.m_light.Position + (reflectorDirection * 0.28f);
                    float num4 = Vector3.Dot(Vector3.Normalize(MySector.MainCamera.Position - vectord), reflectorDirection);
                    float currentLightPower = skinnedEntity.CurrentLightPower;
                    float num6 = ((1f - ((float) Math.Pow((double) (1f - (1f - Math.Abs(num4))), 30.0))) * 0.5f) * currentLightPower;
                    if (((!ReferenceEquals(skinnedEntity, MySession.Static.LocalCharacter) || (!MySession.Static.CameraController.ForceFirstPersonCamera && !MySession.Static.CameraController.IsInFirstPersonView)) && ((currentLightPower > 0f) && (this.m_leftLightIndex != -1))) && (this.m_rightLightIndex != -1))
                    {
                        float num8 = 1296f;
                        float num9 = 1f - MathHelper.Clamp((float) ((num - num8) / (1600f - num8)), (float) 0f, (float) 1f);
                        if (((length > 0f) && (thickness > 0f)) && (num9 > 0f))
                        {
                            MyTransparentGeometry.AddLineBillboard(ID_REFLECTOR_CONE, (new Vector4(vector2, 1f) * num6) * num9, this.m_leftGlarePosition - (reflectorDirection * 0.05f), reflectorDirection, length, thickness, MyBillboard.BlendTypeEnum.AdditiveBottom, -1, 1f, null);
                            MyTransparentGeometry.AddLineBillboard(ID_REFLECTOR_CONE, (new Vector4(vector2, 1f) * num6) * num9, this.m_rightGlarePosition - (reflectorDirection * 0.05f), reflectorDirection, length, thickness, MyBillboard.BlendTypeEnum.AdditiveBottom, -1, 1f, null);
                        }
                        if (num4 > 0f)
                        {
                            flag = false;
                            if (this.m_flareLeft != null)
                            {
                                this.m_flareLeft.GlareOn = true;
                                this.m_flareLeft.Position = this.m_leftGlarePosition;
                                this.m_flareLeft.ReflectorDirection = reflectorDirection;
                                this.m_flareLeft.UpdateLight();
                            }
                            if (this.m_flareRight != null)
                            {
                                this.m_flareRight.GlareOn = true;
                                this.m_flareRight.Position = this.m_rightGlarePosition;
                                this.m_flareRight.ReflectorDirection = reflectorDirection;
                                this.m_flareRight.UpdateLight();
                            }
                        }
                    }
                    if ((ReferenceEquals(MySession.Static.ControlledEntity, skinnedEntity) || ((MySession.Static.ControlledEntity is MyCockpit) && ReferenceEquals(((MyCockpit) MySession.Static.ControlledEntity).Pilot, skinnedEntity))) || ((MySession.Static.ControlledEntity is MyLargeTurretBase) && ReferenceEquals(((MyLargeTurretBase) MySession.Static.ControlledEntity).Pilot, skinnedEntity)))
                    {
                        if (skinnedEntity.IsDead && (skinnedEntity.CurrentRespawnCounter > 0f))
                        {
                            this.DrawBlood(1f);
                        }
                        if (!skinnedEntity.IsDead && (this.m_currentHitIndicatorCounter > 0f))
                        {
                            this.m_currentHitIndicatorCounter -= 0.01666667f;
                            if (this.m_currentHitIndicatorCounter < 0f)
                            {
                                this.m_currentHitIndicatorCounter = 0f;
                            }
                            float alpha = this.m_currentHitIndicatorCounter / 0.8f;
                            this.DrawBlood(alpha);
                        }
                        if (skinnedEntity.StatComp != null)
                        {
                            float healthRatio = skinnedEntity.StatComp.HealthRatio;
                            if ((healthRatio <= MyCharacterStatComponent.HEALTH_RATIO_CRITICAL) && !skinnedEntity.IsDead)
                            {
                                float alpha = (MathHelper.Clamp((float) (MyCharacterStatComponent.HEALTH_RATIO_CRITICAL - healthRatio), (float) 0f, (float) 1f) / MyCharacterStatComponent.HEALTH_RATIO_CRITICAL) + 0.3f;
                                this.DrawBlood(alpha);
                            }
                        }
                    }
                }
                if ((flag && (this.m_flareRight != null)) && this.m_flareLeft.GlareOn)
                {
                    this.m_flareLeft.GlareOn = false;
                    this.m_flareLeft.UpdateLight();
                    this.m_flareRight.GlareOn = false;
                    this.m_flareRight.UpdateLight();
                }
                this.DrawJetpackThrusts(skinnedEntity.UpdateCalled());
                this.DrawDisconnectedIndicator();
            }
        }

        private void DrawBlood(float alpha)
        {
            RectangleF destination = new RectangleF(0f, 0f, (float) MyGuiManager.GetFullscreenRectangle().Width, (float) MyGuiManager.GetFullscreenRectangle().Height);
            Rectangle? sourceRectangle = null;
            MyRenderProxy.DrawSprite(@"Textures\Gui\Blood.dds", ref destination, false, ref sourceRectangle, new Color(new Vector4(1f, 1f, 1f, alpha)), 0f, new Vector2(1f, 0f), ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
        }

        private unsafe void DrawDisconnectedIndicator()
        {
            MyPlayer player;
            MyPlayer.PlayerId? savedPlayer = (base.Entity as MyCharacter).SavedPlayer;
            if (((savedPlayer != null) && (savedPlayer.Value.SerialId == 0)) && !MySession.Static.Players.TryGetPlayerById(savedPlayer.Value, out player))
            {
                Vector3D vectord = (base.Entity.PositionComp.GetPosition() + (base.Entity.PositionComp.LocalAABB.Height * base.Entity.PositionComp.WorldMatrix.Up)) + (base.Entity.PositionComp.WorldMatrix.Up * 0.20000000298023224);
                double num = Vector3D.Distance(MySector.MainCamera.Position, vectord);
                if (num <= MAX_DISCONNECT_ICON_DISTANCE)
                {
                    Color white = Color.White;
                    Color* colorPtr1 = (Color*) ref white;
                    colorPtr1.A = (byte) (white.A * ((float) Math.Min(1.0, Math.Max((double) 0.0, (double) ((MAX_DISCONNECT_ICON_DISTANCE - num) / 10.0)))));
                    MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
                    MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_DISCONNECTED_PLAYER;
                    Vector3D vectord2 = Vector3D.Transform(vectord, MySector.MainCamera.ViewMatrix * MySector.MainCamera.ProjectionMatrix);
                    if (vectord2.Z < 1.0)
                    {
                        Vector2 hudPos = new Vector2((float) vectord2.X, (float) vectord2.Y);
                        hudPos = (hudPos * 0.5f) + (0.5f * Vector2.One);
                        Vector2* vectorPtr1 = (Vector2*) ref hudPos;
                        vectorPtr1->Y = 1f - hudPos.Y;
                        Vector2 normalizedCoord = MyGuiScreenHudBase.ConvertHudToNormalizedGuiPosition(ref hudPos);
                        MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, (texture.SizeGui * 0.5f) * (1f - (((float) num) / ((float) MAX_DISCONNECT_ICON_DISTANCE))), white, drawAlign, false, true);
                    }
                }
            }
        }

        private void DrawJetpackThrusts(bool updateCalled)
        {
            MyCharacter skinnedEntity = base.m_skinnedEntity as MyCharacter;
            if ((skinnedEntity != null) && (skinnedEntity.GetCurrentMovementState() != MyCharacterMovementEnum.Died))
            {
                MyCharacterJetpackComponent jetpackComp = skinnedEntity.JetpackComp;
                if ((jetpackComp != null) && jetpackComp.CanDrawThrusts)
                {
                    MyEntityThrustComponent component2 = base.Container.Get<MyEntityThrustComponent>();
                    if (component2 != null)
                    {
                        MatrixD worldToLocal = MatrixD.Invert(base.Container.Entity.PositionComp.WorldMatrix);
                        foreach (MyJetpackThrust thrust in this.m_jetpackThrusts)
                        {
                            Vector3D zero = Vector3D.Zero;
                            if ((!jetpackComp.TurnedOn || !jetpackComp.IsPowered) || ((skinnedEntity.IsInFirstPersonView && ReferenceEquals(skinnedEntity, MySession.Static.LocalCharacter)) && !MyAnsel.IsAnselSessionRunning))
                            {
                                if (updateCalled || (skinnedEntity.IsUsing != null))
                                {
                                    thrust.ThrustRadius = 0f;
                                }
                            }
                            else
                            {
                                MatrixD matrix = thrust.ThrustMatrix * base.Container.Entity.PositionComp.WorldMatrix;
                                Vector3D vectord2 = Vector3D.TransformNormal(thrust.Forward, matrix);
                                zero = matrix.Translation + (vectord2 * thrust.Offset);
                                float num = 0.05f;
                                if (updateCalled)
                                {
                                    thrust.ThrustRadius = MyUtils.GetRandomFloat(0.9f, 1.1f) * num;
                                }
                                float num2 = MathHelper.Clamp((float) (Vector3.Dot((Vector3) vectord2, (Vector3) (-Vector3.Transform(component2.FinalThrust, base.Entity.WorldMatrix.GetOrientation()) / ((double) skinnedEntity.BaseMass))) * 0.09f), (float) 0.1f, (float) 1f);
                                Vector4 color = Vector4.Zero;
                                Vector4 vector2 = Vector4.Zero;
                                if ((num2 > 0f) && (thrust.ThrustRadius > 0f))
                                {
                                    float num4 = (1f - ((float) Math.Pow((double) (1f - (1f - Math.Abs(Vector3.Dot((Vector3) MyUtils.Normalize(MySector.MainCamera.Position - zero), (Vector3) vectord2)))), 30.0))) * 0.5f;
                                    if (updateCalled)
                                    {
                                        thrust.ThrustLength = ((num2 * 12f) * MyUtils.GetRandomFloat(1.6f, 2f)) * num;
                                        thrust.ThrustThickness = MyUtils.GetRandomFloat(thrust.ThrustRadius * 1.9f, thrust.ThrustRadius);
                                    }
                                    color = thrust.Light.Color.ToVector4() * new Vector4(1f, 1f, 1f, 0.4f);
                                    MyTransparentGeometry.AddLineBillboard(thrust.ThrustLengthMaterial, color, zero + (vectord2 * JETPACK_THRUST_OFFSET), base.GetRenderObjectID(), ref worldToLocal, (Vector3) vectord2, thrust.ThrustLength * JETPACK_THRUST_LENGTH, thrust.ThrustThickness * JETPACK_THRUST_THICKNESS, MyBillboard.BlendTypeEnum.Standard, -1, num4 * (JETPACK_THRUST_INTENSITY_BASE + (num2 * JETPACK_THRUST_INTENSITY)), null);
                                }
                                if (thrust.ThrustRadius > 0f)
                                {
                                    vector2 = thrust.Light.Color.ToVector4() * new Vector4(1f, 1f, 1f, 0.4f);
                                    MyTransparentGeometry.AddPointBillboard(thrust.ThrustPointMaterial, vector2, zero, base.GetRenderObjectID(), ref worldToLocal, thrust.ThrustRadius * JETPACK_THRUST_THICKNESS, 0f, -1, MyBillboard.BlendTypeEnum.Standard, JETPACK_THRUST_INTENSITY_BASE + (num2 * JETPACK_THRUST_INTENSITY), null);
                                }
                            }
                            if (thrust.Light != null)
                            {
                                if (thrust.ThrustRadius <= 0f)
                                {
                                    thrust.Light.GlareOn = false;
                                    thrust.Light.LightOn = false;
                                    thrust.Light.UpdateLight();
                                }
                                else
                                {
                                    thrust.Light.LightOn = true;
                                    thrust.Light.Intensity = JETPACK_LIGHT_INTENSITY_BASE + (thrust.ThrustLength * JETPACK_LIGHT_INTENSITY_LENGTH);
                                    thrust.Light.Range = (thrust.ThrustRadius * JETPACK_LIGHT_RANGE_RADIUS) + (thrust.ThrustLength * JETPACK_LIGHT_RANGE_LENGTH);
                                    thrust.Light.Position = Vector3D.Transform(zero, base.Container.Entity.PositionComp.WorldMatrixNormalizedInv);
                                    thrust.Light.ParentID = this.m_cullRenderId;
                                    thrust.Light.GlareOn = true;
                                    thrust.Light.GlareIntensity = (JETPACK_GLARE_INTENSITY_BASE + (thrust.ThrustLength * JETPACK_GLARE_INTENSITY_LENGTH)) * this.m_flareJetpack.Intensity;
                                    thrust.Light.GlareType = MyGlareTypeEnum.Normal;
                                    thrust.Light.GlareSize = (this.m_flareJetpack.Size * ((thrust.ThrustRadius * JETPACK_GLARE_SIZE_RADIUS) + (thrust.ThrustLength * JETPACK_GLARE_SIZE_LENGTH))) * thrust.ThrustGlareSize;
                                    thrust.Light.SubGlares = this.m_flareJetpack.SubGlares;
                                    thrust.Light.GlareQuerySize = 0.1f;
                                    thrust.Light.UpdateLight();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitJetpackThrust(int bone, Vector3 forward, float offset, ref MyObjectBuilder_ThrustDefinition thrustProperties)
        {
            MyJetpackThrust thrust1 = new MyJetpackThrust();
            thrust1.Bone = bone;
            thrust1.Forward = forward;
            thrust1.Offset = offset;
            thrust1.ThrustPointMaterial = MyStringId.GetOrCompute(thrustProperties.FlamePointMaterial);
            thrust1.ThrustLengthMaterial = MyStringId.GetOrCompute(thrustProperties.FlameLengthMaterial);
            thrust1.ThrustGlareSize = 1f;
            MyJetpackThrust item = thrust1;
            item.Light = MyLights.AddLight();
            if (item.Light != null)
            {
                item.Light.ReflectorDirection = (Vector3) base.Container.Entity.PositionComp.WorldMatrix.Forward;
                item.Light.ReflectorUp = (Vector3) base.Container.Entity.PositionComp.WorldMatrix.Up;
                item.Light.ReflectorRange = 1f;
                item.Light.Color = thrustProperties.FlameIdleColor;
                item.Light.Start(base.Entity.DisplayName + " Jetpack " + this.m_jetpackThrusts.Count);
                item.Light.Falloff = 2f;
                this.m_jetpackThrusts.Add(item);
            }
        }

        public void InitJetpackThrusts(MyCharacterDefinition definition)
        {
            this.m_jetpackThrusts.Clear();
            if (definition.Jetpack != null)
            {
                foreach (MyJetpackThrustDefinition definition2 in definition.Jetpack.Thrusts)
                {
                    int num;
                    if (base.m_skinnedEntity.AnimationController.FindBone(definition2.ThrustBone, out num) != null)
                    {
                        this.InitJetpackThrust(num, Vector3.Forward, definition2.SideFlameOffset, ref definition.Jetpack.ThrustProperties);
                        this.InitJetpackThrust(num, Vector3.Left, definition2.SideFlameOffset, ref definition.Jetpack.ThrustProperties);
                        this.InitJetpackThrust(num, Vector3.Right, definition2.SideFlameOffset, ref definition.Jetpack.ThrustProperties);
                        this.InitJetpackThrust(num, Vector3.Backward, definition2.SideFlameOffset, ref definition.Jetpack.ThrustProperties);
                        this.InitJetpackThrust(num, Vector3.Up, definition2.FrontFlameOffset, ref definition.Jetpack.ThrustProperties);
                    }
                }
            }
        }

        public void InitLight(MyCharacterDefinition definition)
        {
            this.m_light = MyLights.AddLight();
            if (this.m_light != null)
            {
                this.m_light.Start(base.Entity.DisplayName + " Reflector");
                this.m_light.ReflectorOn = true;
                this.m_light.ReflectorTexture = @"Textures\Lights\dual_reflector_2.tif";
                this.UpdateLightBasics();
                this.m_flareLeft = this.CreateFlare("left");
                this.m_flareRight = this.CreateFlare("right");
                base.m_skinnedEntity.AnimationController.FindBone(definition.LeftLightBone, out this.m_leftLightIndex);
                base.m_skinnedEntity.AnimationController.FindBone(definition.RightLightBone, out this.m_rightLightIndex);
            }
        }

        public override void InvalidateRenderObjects()
        {
            if (this.m_cullRenderId != uint.MaxValue)
            {
                MatrixD worldMatrix = base.Container.Entity.PositionComp.WorldMatrix;
                BoundingBox? aabb = null;
                Matrix? localMatrix = null;
                MyRenderProxy.UpdateRenderObject(this.m_cullRenderId, new MatrixD?(worldMatrix), aabb, base.LastMomentUpdateIndex, localMatrix);
            }
        }

        public override void RemoveRenderObjects()
        {
            base.RemoveRenderObjects();
            if (this.m_cullRenderId != uint.MaxValue)
            {
                MyRenderProxy.RemoveRenderObject(this.m_cullRenderId, MyRenderProxy.ObjectType.ManualCull, false);
            }
            this.m_cullRenderId = uint.MaxValue;
        }

        internal void TrySpawnWalkingParticles()
        {
            if (MyFakes.ENABLE_WALKING_PARTICLES)
            {
                int lastWalkParticleCheckTime = this.m_lastWalkParticleCheckTime;
                this.m_lastWalkParticleCheckTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_walkParticleSpawnCounterMs -= this.m_lastWalkParticleCheckTime - lastWalkParticleCheckTime;
                if (this.m_walkParticleSpawnCounterMs <= 0)
                {
                    if (MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(base.Entity.PositionComp.WorldMatrix.Translation) <= 0f)
                    {
                        this.m_walkParticleSpawnCounterMs = 0x2710;
                    }
                    else
                    {
                        MyCharacter entity = base.Entity as MyCharacter;
                        if (entity.JetpackRunning)
                        {
                            this.m_walkParticleSpawnCounterMs = 0x7d0;
                        }
                        else
                        {
                            MyCharacterMovementEnum currentMovementState = entity.GetCurrentMovementState();
                            if ((currentMovementState.GetDirection() == 0) || (currentMovementState == MyCharacterMovementEnum.Falling))
                            {
                                this.m_walkParticleSpawnCounterMs = 0x3e8;
                            }
                            else
                            {
                                Vector3D up = base.Entity.PositionComp.WorldMatrix.Up;
                                Vector3D from = base.Entity.PositionComp.WorldMatrix.Translation + (0.2 * up);
                                MyPhysics.HitInfo? nullable = MyPhysics.CastRay(from, (base.Entity.PositionComp.WorldMatrix.Translation + (0.2 * up)) - (0.5 * up), 0x1c);
                                if (nullable != null)
                                {
                                    MyVoxelPhysicsBody physics = nullable.Value.HkHitInfo.GetHitEntity().Physics as MyVoxelPhysicsBody;
                                    if (physics != null)
                                    {
                                        MyStringId walk;
                                        ushort speed = currentMovementState.GetSpeed();
                                        if (speed == 0)
                                        {
                                            walk = MyMaterialPropertiesHelper.CollisionType.Walk;
                                            this.m_walkParticleSpawnCounterMs = 500;
                                        }
                                        else if (speed == 0x400)
                                        {
                                            walk = MyMaterialPropertiesHelper.CollisionType.Run;
                                            this.m_walkParticleSpawnCounterMs = 0x113;
                                        }
                                        else if (speed != 0x800)
                                        {
                                            walk = MyMaterialPropertiesHelper.CollisionType.Walk;
                                            this.m_walkParticleSpawnCounterMs = 0x3e8;
                                        }
                                        else
                                        {
                                            walk = MyMaterialPropertiesHelper.CollisionType.Sprint;
                                            this.m_walkParticleSpawnCounterMs = 250;
                                        }
                                        Vector3D position = nullable.Value.Position;
                                        MyVoxelMaterialDefinition materialAt = physics.m_voxelMap.GetMaterialAt(ref position);
                                        if (materialAt != null)
                                        {
                                            MyMaterialPropertiesHelper.Static.TryCreateCollisionEffect(walk, position, (Vector3) up, ID_CHARACTER, materialAt.MaterialTypeNameHash, null);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void TrySpawnWalkingParticles(ref HkContactPointEvent value)
        {
            if (MyFakes.ENABLE_WALKING_PARTICLES)
            {
                int lastWalkParticleCheckTime = this.m_lastWalkParticleCheckTime;
                this.m_lastWalkParticleCheckTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.m_walkParticleSpawnCounterMs -= this.m_lastWalkParticleCheckTime - lastWalkParticleCheckTime;
                if (this.m_walkParticleSpawnCounterMs <= 0)
                {
                    if (MyGravityProviderSystem.CalculateHighestNaturalGravityMultiplierInPoint(base.Entity.PositionComp.WorldMatrix.Translation) <= 0f)
                    {
                        this.m_walkParticleSpawnCounterMs = 0x2710;
                    }
                    else
                    {
                        MyCharacter entity = base.Entity as MyCharacter;
                        if (entity.JetpackRunning)
                        {
                            this.m_walkParticleSpawnCounterMs = 0x7d0;
                        }
                        else
                        {
                            MyCharacterMovementEnum currentMovementState = entity.GetCurrentMovementState();
                            if ((currentMovementState.GetDirection() == 0) || (currentMovementState == MyCharacterMovementEnum.Falling))
                            {
                                this.m_walkParticleSpawnCounterMs = 0x3e8;
                            }
                            else
                            {
                                MyVoxelPhysicsBody physics = value.GetOtherEntity(entity).Physics as MyVoxelPhysicsBody;
                                if (physics != null)
                                {
                                    MyStringId walk;
                                    ushort speed = currentMovementState.GetSpeed();
                                    if (speed == 0)
                                    {
                                        walk = MyMaterialPropertiesHelper.CollisionType.Walk;
                                        this.m_walkParticleSpawnCounterMs = 500;
                                    }
                                    else if (speed == 0x400)
                                    {
                                        walk = MyMaterialPropertiesHelper.CollisionType.Run;
                                        this.m_walkParticleSpawnCounterMs = 0x113;
                                    }
                                    else if (speed != 0x800)
                                    {
                                        walk = MyMaterialPropertiesHelper.CollisionType.Walk;
                                        this.m_walkParticleSpawnCounterMs = 0x3e8;
                                    }
                                    else
                                    {
                                        walk = MyMaterialPropertiesHelper.CollisionType.Sprint;
                                        this.m_walkParticleSpawnCounterMs = 250;
                                    }
                                    Vector3D worldPosition = physics.ClusterToWorld(value.ContactPoint.Position);
                                    MyVoxelMaterialDefinition materialAt = physics.m_voxelMap.GetMaterialAt(ref worldPosition);
                                    if (materialAt != null)
                                    {
                                        MyMaterialPropertiesHelper.Static.TryCreateCollisionEffect(walk, worldPosition, value.ContactPoint.Normal, ID_CHARACTER, materialAt.MaterialTypeNameHash, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UpdateLight(float lightPower, bool updateRenderObject, bool updateBasicLight)
        {
            if ((this.m_light != null) && (base.RenderObjectIDs[0] != uint.MaxValue))
            {
                bool flag = lightPower > 0f;
                if (this.m_light.ReflectorOn != flag)
                {
                    this.m_light.ReflectorOn = flag;
                    this.m_light.LightOn = flag;
                    MyRenderProxy.UpdateColorEmissivity(base.RenderObjectIDs[0], 0, "Headlight", Color.White, flag ? 1f : 0f);
                }
                if (updateBasicLight)
                {
                    this.UpdateLightBasics();
                }
                if (updateRenderObject | updateBasicLight)
                {
                    this.UpdateLightPosition();
                    this.UpdateLightProperties(lightPower);
                }
            }
        }

        private void UpdateLightBasics()
        {
            this.m_light.ReflectorColor = MyCharacter.REFLECTOR_COLOR;
            this.m_light.ReflectorConeMaxAngleCos = 0.373f;
            this.m_light.ReflectorRange = 35f;
            this.m_light.ReflectorFalloff = MyCharacter.REFLECTOR_FALLOFF;
            this.m_light.ReflectorGlossFactor = MyCharacter.REFLECTOR_GLOSS_FACTOR;
            this.m_light.ReflectorDiffuseFactor = MyCharacter.REFLECTOR_DIFFUSE_FACTOR;
            this.m_light.Color = MyCharacter.POINT_COLOR;
            this.m_light.Range = MyCharacter.POINT_LIGHT_RANGE;
            this.m_light.Falloff = MyCharacter.POINT_FALLOFF;
            this.m_light.GlossFactor = MyCharacter.POINT_GLOSS_FACTOR;
            this.m_light.DiffuseFactor = MyCharacter.POINT_DIFFUSE_FACTOR;
        }

        public void UpdateLightPosition()
        {
            if (this.m_light != null)
            {
                MyCharacter skinnedEntity = base.m_skinnedEntity as MyCharacter;
                this.m_lightLocalPosition = skinnedEntity.Definition.LightOffset;
                MatrixD matrix = skinnedEntity.GetHeadMatrix(false, true, false, true, false);
                this.m_light.ReflectorDirection = (Vector3) matrix.Forward;
                this.m_light.ReflectorUp = (Vector3) matrix.Right;
                this.m_light.Position = Vector3D.Transform(this.m_lightLocalPosition, matrix);
                this.m_light.UpdateLight();
                Matrix[] boneAbsoluteTransforms = skinnedEntity.BoneAbsoluteTransforms;
                if (this.m_leftLightIndex != -1)
                {
                    MatrixD xd2 = MatrixD.Normalize(boneAbsoluteTransforms[this.m_leftLightIndex]) * base.m_skinnedEntity.PositionComp.WorldMatrix;
                    this.m_leftGlarePosition = xd2.Translation;
                }
                if (this.m_rightLightIndex != -1)
                {
                    MatrixD xd3 = MatrixD.Normalize(boneAbsoluteTransforms[this.m_rightLightIndex]) * base.m_skinnedEntity.PositionComp.WorldMatrix;
                    this.m_rightGlarePosition = xd3.Translation;
                }
            }
        }

        public void UpdateLightProperties(float currentLightPower)
        {
            if (this.m_light != null)
            {
                this.m_light.ReflectorIntensity = MyCharacter.REFLECTOR_INTENSITY * currentLightPower;
                this.m_light.Intensity = MyCharacter.POINT_LIGHT_INTENSITY * currentLightPower;
                this.m_light.UpdateLight();
                float num = this.m_flareHeadlamp.Intensity * currentLightPower;
                this.m_flareLeft.GlareIntensity = num;
                this.m_flareRight.GlareIntensity = num;
            }
        }

        public void UpdateShadowIgnoredObjects()
        {
            if (this.m_light != null)
            {
                MyRenderProxy.ClearLightShadowIgnore(this.m_light.RenderObjectID);
                MyRenderProxy.SetLightShadowIgnore(this.m_light.RenderObjectID, base.RenderObjectIDs[0]);
            }
        }

        public void UpdateShadowIgnoredObjects(IMyEntity Parent)
        {
            if (this.m_light != null)
            {
                foreach (uint num2 in Parent.Render.RenderObjectIDs)
                {
                    MyRenderProxy.SetLightShadowIgnore(this.m_light.RenderObjectID, num2);
                }
            }
        }

        public void UpdateThrustMatrices(Matrix[] boneMatrices)
        {
            foreach (MyJetpackThrust thrust in this.m_jetpackThrusts)
            {
                thrust.ThrustMatrix = Matrix.Normalize(boneMatrices[thrust.Bone]);
            }
        }

        public void UpdateWalkParticles()
        {
            this.TrySpawnWalkingParticles();
        }

        public class MyJetpackThrust
        {
            public int Bone;
            public Vector3 Forward;
            public float Offset;
            public MyLight Light;
            public float ThrustLength;
            public float ThrustRadius;
            public float ThrustThickness;
            public Matrix ThrustMatrix;
            public MyStringId ThrustPointMaterial;
            public MyStringId ThrustLengthMaterial;
            public float ThrustGlareSize;
        }
    }
}

