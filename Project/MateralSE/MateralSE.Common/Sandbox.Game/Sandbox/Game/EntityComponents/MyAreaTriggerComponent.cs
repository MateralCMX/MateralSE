namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRageMath;

    [MyComponentBuilder(typeof(MyObjectBuilder_AreaTrigger), true)]
    public class MyAreaTriggerComponent : MyTriggerComponent
    {
        private readonly HashSet<MyEntity> m_prevQuery;
        private readonly List<MyEntity> m_resultsToRemove;

        public MyAreaTriggerComponent() : this(string.Empty)
        {
        }

        public MyAreaTriggerComponent(string name) : base(MyTriggerComponent.TriggerType.Sphere, 20)
        {
            this.m_prevQuery = new HashSet<MyEntity>();
            this.m_resultsToRemove = new List<MyEntity>();
            this.Name = name;
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_AreaTrigger trigger = (MyObjectBuilder_AreaTrigger) builder;
            this.Name = trigger.Name;
        }

        public override bool IsSerialized() => 
            true;

        protected override bool QueryEvaluator(MyEntity entity) => 
            (!(entity is MyCharacter) ? (entity is MyCubeGrid) : true);

        public override MyObjectBuilder_ComponentBase Serialize(bool copy)
        {
            MyObjectBuilder_AreaTrigger trigger1 = base.Serialize(copy) as MyObjectBuilder_AreaTrigger;
            trigger1.Name = this.Name;
            return trigger1;
        }

        protected override void UpdateInternal()
        {
            base.UpdateInternal();
            foreach (MyEntity entity in this.m_prevQuery)
            {
                if (!base.QueryResult.Contains(entity))
                {
                    if (MyVisualScriptLogicProvider.AreaTrigger_EntityLeft != null)
                    {
                        MyVisualScriptLogicProvider.AreaTrigger_EntityLeft(this.Name, entity.EntityId, entity.Name);
                    }
                    if (MyVisualScriptLogicProvider.AreaTrigger_Left != null)
                    {
                        MyPlayer.PlayerId id;
                        MyCharacter character = entity as MyCharacter;
                        if (((character == null) || !character.IsBot) && MySession.Static.Players.ControlledEntities.TryGetValue(entity.EntityId, out id))
                        {
                            MyIdentity identity = MySession.Static.Players.TryGetPlayerIdentity(id);
                            MyVisualScriptLogicProvider.AreaTrigger_Left(this.Name, identity.IdentityId);
                        }
                    }
                    this.m_resultsToRemove.Add(entity);
                }
            }
            foreach (MyEntity entity2 in this.m_resultsToRemove)
            {
                this.m_prevQuery.Remove(entity2);
            }
            this.m_resultsToRemove.Clear();
            foreach (MyEntity entity3 in base.QueryResult)
            {
                if (this.m_prevQuery.Add(entity3))
                {
                    if (MyVisualScriptLogicProvider.AreaTrigger_EntityEntered != null)
                    {
                        MyVisualScriptLogicProvider.AreaTrigger_EntityEntered(this.Name, entity3.EntityId, entity3.Name);
                    }
                    if (MyVisualScriptLogicProvider.AreaTrigger_Entered != null)
                    {
                        MyPlayer.PlayerId id2;
                        MyCharacter character2 = entity3 as MyCharacter;
                        if (((character2 == null) || !character2.IsBot) && MySession.Static.Players.ControlledEntities.TryGetValue(entity3.EntityId, out id2))
                        {
                            MyIdentity identity2 = MySession.Static.Players.TryGetPlayerIdentity(id2);
                            MyVisualScriptLogicProvider.AreaTrigger_Entered(this.Name, identity2.IdentityId);
                        }
                    }
                }
            }
        }

        public string Name { get; set; }

        public double Radius
        {
            get => 
                base.m_boundingSphere.Radius;
            set => 
                (base.m_boundingSphere.Radius = value);
        }

        public Vector3D Center
        {
            get => 
                base.m_boundingSphere.Center;
            set
            {
                base.m_boundingSphere.Center = value;
                if (base.Entity != null)
                {
                    base.DefaultTranslation = base.m_boundingSphere.Center - base.Entity.PositionComp.GetPosition();
                }
            }
        }
    }
}

