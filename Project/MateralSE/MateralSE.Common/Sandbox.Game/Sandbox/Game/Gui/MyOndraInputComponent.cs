namespace Sandbox.Game.Gui
{
    using Havok;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyOndraInputComponent : MyDebugComponent
    {
        private bool m_gridDebugInfo;

        public override void Draw()
        {
            base.Draw();
        }

        public override string GetName() => 
            "Ondra";

        public override bool HandleInput()
        {
            bool flag = false;
            if (this.m_gridDebugInfo)
            {
                MyCubeGrid grid;
                Vector3I vectori;
                double num;
                LineD line = new LineD(MySector.MainCamera.Position, MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 1000f));
                if (MyCubeGrid.GetLineIntersection(ref line, out grid, out vectori, out num))
                {
                    MatrixD worldMatrix = grid.WorldMatrix;
                    MatrixD xd2 = Matrix.CreateTranslation((Vector3) (vectori * grid.GridSize)) * worldMatrix;
                    grid.GetCubeBlock(vectori);
                    Vector2 screenCoord = new Vector2();
                    MyRenderProxy.DebugDrawText2D(screenCoord, vectori.ToString(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawOBB(Matrix.CreateScale(new Vector3(grid.GridSize) + new Vector3(0.15f)) * xd2, Color.Red.ToVector3(), 0.2f, true, true, true, false);
                }
            }
            if (!MyInput.Static.IsAnyAltKeyPressed())
            {
                MyInput.Static.IsAnyShiftKeyPressed();
                MyInput.Static.IsAnyCtrlKeyPressed();
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad6))
                {
                    Matrix matrix = Matrix.Invert((Matrix) MySector.MainCamera.ViewMatrix);
                    MyObjectBuilder_Ore content = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Stone");
                    MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(1, content, 1f), matrix.Translation + (matrix.Forward * 1f), matrix.Forward, matrix.Up, null, null);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad7))
                {
                    using (IEnumerator<MyCubeGrid> enumerator = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            IEnumerator<MyMotorStator> enumerator2 = (from s in enumerator.Current.CubeBlocks
                                select s.FatBlock into s
                                where s != null
                                select s).OfType<MyMotorStator>().GetEnumerator();
                            try
                            {
                                while (enumerator2.MoveNext())
                                {
                                    MyMotorStator current = enumerator2.Current;
                                    if (current.Rotor != null)
                                    {
                                        Quaternion quaternion = Quaternion.CreateFromAxisAngle((Vector3) current.Rotor.WorldMatrix.Up, MathHelper.ToRadians((float) 45f));
                                        current.Rotor.CubeGrid.WorldMatrix = MatrixD.CreateFromQuaternion(quaternion) * current.Rotor.CubeGrid.WorldMatrix;
                                    }
                                }
                            }
                            finally
                            {
                                if (enumerator2 == null)
                                {
                                    continue;
                                }
                                enumerator2.Dispose();
                            }
                        }
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad8))
                {
                    Matrix matrix2 = Matrix.Invert((Matrix) MySector.MainCamera.ViewMatrix);
                    MyObjectBuilder_Ore ore2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Stone");
                    MyObjectBuilder_InventoryItem item1 = new MyObjectBuilder_InventoryItem();
                    item1.PhysicalContent = ore2;
                    item1.Amount = 0x3e8;
                    MyObjectBuilder_FloatingObject objectBuilder = new MyObjectBuilder_FloatingObject();
                    objectBuilder.Item = item1;
                    objectBuilder.PositionAndOrientation = new MyPositionAndOrientation(matrix2.Translation + (2f * matrix2.Forward), matrix2.Forward, matrix2.Up);
                    objectBuilder.PersistentFlags = MyPersistentEntityFlags2.InScene;
                    Sandbox.Game.Entities.MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false).Physics.LinearVelocity = Vector3.Normalize(matrix2.Forward) * 50f;
                }
                MyInput.Static.IsNewKeyPressed(MyKeys.Divide);
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply))
                {
                    MyDebugDrawSettings.ENABLE_DEBUG_DRAW = !MyDebugDrawSettings.ENABLE_DEBUG_DRAW;
                    MyDebugDrawSettings.DEBUG_DRAW_STRUCTURAL_INTEGRITY = true;
                    foreach (MyCubeGrid grid2 in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>())
                    {
                        if (grid2.IsStatic)
                        {
                            grid2.CreateStructuralIntegrity();
                        }
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad1))
                {
                    MyCubeGrid grid3 = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().FirstOrDefault<MyCubeGrid>();
                    if (grid3 != null)
                    {
                        grid3.Physics.RigidBody.MaxLinearVelocity = 1000f;
                        if (grid3.Physics.RigidBody2 != null)
                        {
                            grid3.Physics.RigidBody2.MaxLinearVelocity = 1000f;
                        }
                        grid3.Physics.LinearVelocity = new Vector3(1000f, 0f, 0f);
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Decimal))
                {
                    Vector3 initialLinearVelocity = new Vector3();
                    initialLinearVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab("respawnship", MySector.MainCamera.Position, MySector.MainCamera.ForwardVector, MySector.MainCamera.UpVector, initialLinearVelocity, initialLinearVelocity, null, null, SpawningOptions.None, 0L, false, null);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply) && MyInput.Static.IsAnyShiftKeyPressed())
                {
                    GC.Collect(2);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad5))
                {
                    Thread.Sleep(250);
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad9))
                {
                    VRage.Game.Entity.MyEntity entity = MySession.Static.ControlledEntity?.Entity;
                    if (entity != null)
                    {
                        entity.PositionComp.SetPosition(entity.PositionComp.GetPosition() + (entity.WorldMatrix.Forward * 5.0), null, false, true);
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad4))
                {
                    VRage.Game.Entity.MyEntity controlledEntity = MySession.Static.ControlledEntity as VRage.Game.Entity.MyEntity;
                    if ((controlledEntity != null) && controlledEntity.HasInventory)
                    {
                        MyFixedPoint amount = 0x4e20;
                        controlledEntity.GetInventory(0).AddItems(amount, MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Stone"));
                    }
                    flag = true;
                }
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.Delete))
                {
                    Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyFloatingObject>().Count<MyFloatingObject>();
                    foreach (MyFloatingObject local3 in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyFloatingObject>())
                    {
                        if (local3 == MySession.Static.ControlledEntity)
                        {
                            Vector3D? position = null;
                            MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
                        }
                        local3.Close();
                    }
                    flag = true;
                }
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.Decimal))
                {
                    foreach (VRage.Game.Entity.MyEntity entity3 in Sandbox.Game.Entities.MyEntities.GetEntities())
                    {
                        if (ReferenceEquals(entity3, MySession.Static.ControlledEntity))
                        {
                            continue;
                        }
                        if (((MySession.Static.ControlledEntity == null) || !ReferenceEquals(entity3, MySession.Static.ControlledEntity.Entity.Parent)) && !ReferenceEquals(entity3, MyCubeBuilder.Static.FindClosestGrid()))
                        {
                            entity3.Close();
                        }
                    }
                    flag = true;
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad9) || MyInput.Static.IsNewKeyPressed(MyKeys.NumPad5))
                {
                    MyPhysicsComponentBase physics = MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).Physics;
                    if (physics.RigidBody != null)
                    {
                        physics.RigidBody.ApplyLinearImpulse((Vector3) ((physics.Entity.WorldMatrix.Forward * physics.Mass) * 2.0));
                    }
                    flag = true;
                }
                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.OemComma))
                {
                    MyFloatingObject[] objArray = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyFloatingObject>().ToArray<MyFloatingObject>();
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        objArray[i].Close();
                    }
                }
            }
            return flag;
        }

        private void phantom_Enter(HkPhantomCallbackShape sender, HkRigidBody body)
        {
        }

        private void phantom_Leave(HkPhantomCallbackShape sender, HkRigidBody body)
        {
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyOndraInputComponent.<>c <>9 = new MyOndraInputComponent.<>c();
            public static Func<MySlimBlock, MyCubeBlock> <>9__5_0;
            public static Func<MyCubeBlock, bool> <>9__5_1;

            internal MyCubeBlock <HandleInput>b__5_0(MySlimBlock s) => 
                s.FatBlock;

            internal bool <HandleInput>b__5_1(MyCubeBlock s) => 
                (s != null);
        }
    }
}

