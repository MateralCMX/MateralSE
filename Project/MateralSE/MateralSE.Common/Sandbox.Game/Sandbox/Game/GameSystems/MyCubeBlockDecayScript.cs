namespace Sandbox.Game.GameSystems
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Systems;

    [MyScriptedSystem("DecayBlocks")]
    public class MyCubeBlockDecayScript : MyGroupScriptBase
    {
        private HashSet<MyStringHash> m_tmpSubtypes = new HashSet<MyStringHash>(MyStringHash.Comparer);

        public override void ProcessObjects(ListReader<MyDefinitionId> objects)
        {
            MyConcurrentHashSet<MyEntity> entities = MyEntities.GetEntities();
            this.m_tmpSubtypes.Clear();
            foreach (MyDefinitionId id in objects)
            {
                this.m_tmpSubtypes.Add(id.SubtypeId);
            }
            foreach (MyFloatingObject obj2 in entities)
            {
                if (obj2 == null)
                {
                    continue;
                }
                MyDefinitionId objectId = obj2.Item.Content.GetObjectId();
                if (this.m_tmpSubtypes.Contains(objectId.SubtypeId))
                {
                    MyFloatingObjects.RemoveFloatingObject(obj2, true);
                }
            }
        }
    }
}

