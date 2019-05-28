namespace Sandbox.Game.Entities
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Graphics;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_AirtightSlideDoor)), MyTerminalInterface(new Type[] { typeof(Sandbox.ModAPI.IMyAirtightSlideDoor), typeof(Sandbox.ModAPI.Ingame.IMyAirtightSlideDoor) })]
    public class MyAirtightSlideDoor : MyAirtightDoorGeneric, Sandbox.ModAPI.IMyAirtightSlideDoor, Sandbox.ModAPI.IMyAirtightDoorBase, Sandbox.ModAPI.IMyDoor, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDoor, Sandbox.ModAPI.Ingame.IMyAirtightDoorBase, Sandbox.ModAPI.Ingame.IMyAirtightSlideDoor
    {
        private bool hadEnoughPower;

        protected override void FillSubparts()
        {
            MyEntitySubpart subpart;
            base.m_subparts.Clear();
            if (base.Subparts.TryGetValue("DoorLeft", out subpart))
            {
                base.m_subparts.Add(subpart);
            }
            if (base.Subparts.TryGetValue("DoorRight", out subpart))
            {
                base.m_subparts.Add(subpart);
            }
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            MyAirtightDoorGeneric.m_emissiveTextureNames = new string[] { "Emissive" };
            base.Init(builder, cubeGrid);
        }

        public override bool SetEmissiveStateDamaged() => 
            false;

        public override bool SetEmissiveStateDisabled() => 
            false;

        public override bool SetEmissiveStateWorking() => 
            false;

        protected override unsafe void UpdateDoorPosition()
        {
            if (base.m_subparts.Count != 0)
            {
                float num = (float) Math.Sqrt(1.1375000476837158);
                float z = base.m_currOpening * 1.75f;
                float num3 = base.m_currOpening * 1.570796f;
                if (z < num)
                {
                    num3 = (float) Math.Asin((double) (z / 1.2f));
                }
                else
                {
                    float num5 = (1.75f - z) / (1.75f - num);
                    num3 = 1.570796f - ((num5 * num5) * ((float) (1.570796012878418 - Math.Asin((double) (num / 1.2f)))));
                }
                z--;
                MyGridPhysics bodyA = base.CubeGrid.Physics;
                bool flag = !Sync.IsServer;
                int num4 = 0;
                bool flag2 = true;
                foreach (MyEntitySubpart subpart in base.m_subparts)
                {
                    if (subpart != null)
                    {
                        Matrix matrix;
                        Matrix* matrixPtr1;
                        Matrix.CreateRotationY(flag2 ? num3 : -num3, out matrix);
                        matrixPtr1.Translation = new Vector3(flag2 ? -1.2f : 1.2f, 0f, z);
                        matrixPtr1 = (Matrix*) ref matrix;
                        Matrix renderLocal = matrix * base.PositionComp.LocalMatrix;
                        MyPhysicsComponentBase physics = subpart.Physics;
                        if (flag && (physics != null))
                        {
                            Matrix* matrixPtr2;
                            Matrix identity = Matrix.Identity;
                            matrixPtr2.Translation = new Vector3(flag2 ? -0.55f : 0.55f, 0f, 0f);
                            matrixPtr2 = (Matrix*) ref identity;
                            Matrix* matrixPtr3 = (Matrix*) ref identity;
                            Matrix.Multiply(ref (Matrix) ref matrixPtr3, ref matrix, out identity);
                            subpart.PositionComp.SetLocalMatrix(ref identity, null, true);
                        }
                        subpart.PositionComp.SetLocalMatrix(ref matrix, physics, true, ref renderLocal, true);
                        if (((bodyA != null) && (physics != null)) && (base.m_subpartConstraintsData.Count > num4))
                        {
                            bodyA.RigidBody.Activate();
                            physics.RigidBody.Activate();
                            matrix = Matrix.Invert(matrix);
                            base.m_subpartConstraintsData[num4].SetInBodySpace(base.PositionComp.LocalMatrix, matrix, bodyA, (MyPhysicsBody) physics);
                        }
                    }
                    flag2 = !flag2;
                    num4++;
                }
            }
        }

        protected override void UpdateEmissivity(bool force)
        {
            MyEmissiveColorStateResult result;
            Color red = Color.Red;
            float emissivity = 1f;
            if (base.IsWorking)
            {
                red = Color.Green;
                if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Working, out result))
                {
                    red = result.EmissiveColor;
                }
            }
            else if (!base.IsFunctional)
            {
                if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Damaged, out result))
                {
                    red = result.EmissiveColor;
                }
                emissivity = 0f;
            }
            else
            {
                if (MyEmissiveColorPresets.LoadPresetState(base.BlockDefinition.EmissiveColorPreset, MyCubeBlock.m_emissiveNames.Disabled, out result))
                {
                    red = result.EmissiveColor;
                }
                if (!base.IsEnoughPower())
                {
                    emissivity = 0f;
                }
            }
            base.SetEmissive(red, emissivity, force);
        }
    }
}

