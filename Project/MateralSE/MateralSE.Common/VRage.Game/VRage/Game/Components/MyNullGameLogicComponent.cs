﻿namespace VRage.Game.Components
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.ObjectBuilders;

    public class MyNullGameLogicComponent : MyGameLogicComponent
    {
        public override void Close()
        {
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) => 
            null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
        }

        public override void MarkForClose()
        {
        }

        public override void UpdateAfterSimulation()
        {
        }

        public override void UpdateAfterSimulation10()
        {
        }

        public override void UpdateAfterSimulation100()
        {
        }

        public override void UpdateBeforeSimulation()
        {
        }

        public override void UpdateBeforeSimulation10()
        {
        }

        public override void UpdateBeforeSimulation100()
        {
        }

        public override void UpdateOnceBeforeFrame()
        {
        }
    }
}

