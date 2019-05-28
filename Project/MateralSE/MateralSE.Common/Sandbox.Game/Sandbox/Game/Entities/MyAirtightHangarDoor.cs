namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_AirtightHangarDoor)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyAirtightHangarDoor), typeof(Sandbox.ModAPI.Ingame.IMyAirtightHangarDoor) })]
    public class MyAirtightHangarDoor : MyAirtightDoorGeneric, Sandbox.ModAPI.IMyAirtightHangarDoor, Sandbox.ModAPI.IMyAirtightDoorBase, Sandbox.ModAPI.IMyDoor, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDoor, Sandbox.ModAPI.Ingame.IMyAirtightDoorBase, Sandbox.ModAPI.Ingame.IMyAirtightHangarDoor
    {
        protected override void FillSubparts()
        {
            MyEntitySubpart subpart;
            base.m_subparts.Clear();
            for (int i = 1; base.Subparts.TryGetValue("HangarDoor_door" + i, out subpart); i++)
            {
                base.m_subparts.Add(subpart);
            }
        }

        protected override unsafe void UpdateDoorPosition()
        {
            if (base.CubeGrid.Physics != null)
            {
                bool flag = !Sync.IsServer;
                if ((base.m_subpartConstraints.Count != 0) || flag)
                {
                    float num = ((base.m_currOpening - 1f) * base.m_subparts.Count) * base.m_subpartMovementDistance;
                    float num2 = 0f;
                    int num3 = 0;
                    foreach (MyEntitySubpart subpart in base.m_subparts)
                    {
                        num2 -= base.m_subpartMovementDistance;
                        if ((subpart != null) && (subpart.Physics != null))
                        {
                            Matrix matrix;
                            Matrix* matrixPtr1;
                            float y = (num < num2) ? num2 : num;
                            Vector3 position = new Vector3(0f, y, 0f);
                            Matrix.CreateTranslation(ref position, out matrix);
                            Matrix renderLocal = matrix * base.PositionComp.LocalMatrix;
                            subpart.PositionComp.SetLocalMatrix(ref (Matrix) ref matrixPtr1, flag ? null : subpart.Physics, true, ref renderLocal, true);
                            if (base.m_subpartConstraintsData.Count > 0)
                            {
                                if (base.CubeGrid.Physics != null)
                                {
                                    base.CubeGrid.Physics.RigidBody.Activate();
                                }
                                subpart.Physics.RigidBody.Activate();
                                position = new Vector3(0f, -y, 0f);
                                matrixPtr1 = (Matrix*) ref matrix;
                                Matrix.CreateTranslation(ref position, out matrix);
                                base.m_subpartConstraintsData[num3].SetInBodySpace(base.PositionComp.LocalMatrix, matrix, base.CubeGrid.Physics, (MyPhysicsBody) subpart.Physics);
                            }
                            num3++;
                        }
                    }
                }
            }
        }
    }
}

