namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Generics;
    using VRage.Utils;
    using VRageMath;

    internal class MyGridMassComputer : MySparseGrid<HkMassElement, MassCellData>
    {
        [ThreadStatic]
        private static HkInertiaTensorComputer s_inertiaComputer;
        [ThreadStatic]
        private static List<HkMassElement> s_tmpElements;
        private const float DefaultUpdateThreshold = 0.05f;
        private float m_updateThreshold;
        private HkMassProperties m_massProperties;

        public MyGridMassComputer(int cellSize, float updateThreshold = 0.05f) : base(cellSize)
        {
            this.m_updateThreshold = updateThreshold;
        }

        public HkMassProperties CombineMassProperties(List<HkMassElement> elements) => 
            InertiaComputer.CombineMassPropertiesInstance(elements);

        public HkMassProperties UpdateMass()
        {
            HkMassElement element = new HkMassElement {
                Tranform = Matrix.Identity
            };
            bool flag = false;
            foreach (Vector3I vectori in base.DirtyCells)
            {
                MySparseGrid<HkMassElement, MassCellData>.Cell cell;
                if (!base.TryGetCell(vectori, out cell))
                {
                    flag = true;
                    continue;
                }
                float num = 0f;
                foreach (KeyValuePair<Vector3I, HkMassElement> pair in cell.Items)
                {
                    TmpElements.Add(pair.Value);
                    num += pair.Value.Properties.Mass;
                }
                if (Math.Abs((float) (1f - (cell.CellData.LastMass / num))) > this.m_updateThreshold)
                {
                    element.Properties = InertiaComputer.CombineMassPropertiesInstance(TmpElements);
                    cell.CellData.MassElement = element;
                    cell.CellData.LastMass = num;
                    flag = true;
                }
                TmpElements.Clear();
            }
            base.UnmarkDirtyAll();
            if (flag)
            {
                foreach (KeyValuePair<Vector3I, MySparseGrid<HkMassElement, MassCellData>.Cell> pair2 in this)
                {
                    TmpElements.Add(pair2.Value.CellData.MassElement);
                }
                this.m_massProperties = (TmpElements.Count <= 0) ? new HkMassProperties() : InertiaComputer.CombineMassPropertiesInstance(TmpElements);
                TmpElements.Clear();
            }
            return this.m_massProperties;
        }

        private static HkInertiaTensorComputer InertiaComputer =>
            MyUtils.Init<HkInertiaTensorComputer>(ref s_inertiaComputer);

        private static List<HkMassElement> TmpElements =>
            MyUtils.Init<List<HkMassElement>>(ref s_tmpElements);
    }
}

