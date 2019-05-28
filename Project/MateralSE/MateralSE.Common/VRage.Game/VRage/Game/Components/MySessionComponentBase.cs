namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Game.Components.Interfaces;
    using VRage.Game.Components.Session;
    using VRage.Game.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public abstract class MySessionComponentBase : IMyUserInputComponent
    {
        public readonly string DebugName;
        public readonly int Priority;
        public readonly Type ComponentType;
        public IMySession Session;
        private bool m_initialized;

        public MySessionComponentBase()
        {
            Type element = base.GetType();
            MySessionComponentDescriptor descriptor = (MySessionComponentDescriptor) Attribute.GetCustomAttribute(element, typeof(MySessionComponentDescriptor), false);
            this.DebugName = element.Name;
            this.Priority = descriptor.Priority;
            this.UpdateOrder = descriptor.UpdateOrder;
            this.ObjectBuilderType = descriptor.ObjectBuilderType;
            this.ComponentType = descriptor.ComponentType;
            if (this.ObjectBuilderType != MyObjectBuilderType.Invalid)
            {
                MySessionComponentMapping.Map(base.GetType(), this.ObjectBuilderType);
            }
            if (this.ComponentType == null)
            {
                this.ComponentType = base.GetType();
            }
            else if ((this.ComponentType == base.GetType()) || this.ComponentType.IsSubclassOf(base.GetType()))
            {
                object[] args = new object[] { base.GetType(), this.ComponentType };
                MyLog.Default.Error("Component {0} tries to register itself as a component it does not inherit from ({1}). Ignoring...", args);
                this.ComponentType = base.GetType();
            }
        }

        public void AfterLoadData()
        {
            this.Loaded = true;
        }

        public virtual void BeforeStart()
        {
        }

        public virtual void Draw()
        {
        }

        public virtual MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            SerializableDefinitionId? nullable1;
            if (!(this.ObjectBuilderType != MyObjectBuilderType.Invalid))
            {
                return null;
            }
            MyDefinitionId? definition = this.Definition;
            MyObjectBuilder_SessionComponent component1 = Activator.CreateInstance((Type) this.ObjectBuilderType) as MyObjectBuilder_SessionComponent;
            if (definition != null)
            {
                nullable1 = new SerializableDefinitionId?(definition.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            (Activator.CreateInstance((Type) this.ObjectBuilderType) as MyObjectBuilder_SessionComponent).Definition = nullable1;
            return (Activator.CreateInstance((Type) this.ObjectBuilderType) as MyObjectBuilder_SessionComponent);
        }

        public virtual void HandleInput()
        {
        }

        public virtual void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            this.m_initialized = true;
            if ((sessionComponent != null) && (sessionComponent.Definition != null))
            {
                MyDefinitionId? nullable1;
                SerializableDefinitionId? definition = sessionComponent.Definition;
                if (definition != null)
                {
                    nullable1 = new MyDefinitionId?(definition.GetValueOrDefault());
                }
                else
                {
                    nullable1 = null;
                }
                this.Definition = nullable1;
            }
            if (this.Definition != null)
            {
                MySessionComponentDefinition definition = MyDefinitionManagerBase.Static.GetDefinition<MySessionComponentDefinition>(this.Definition.Value);
                if (definition == null)
                {
                    object[] args = new object[] { this.Definition, base.GetType().Name };
                    MyLog.Default.Warning("Missing definition {0} : for session component {1}", args);
                }
                else
                {
                    this.InitFromDefinition(definition);
                }
            }
        }

        public virtual void InitFromDefinition(MySessionComponentDefinition definition)
        {
        }

        public virtual void LoadData()
        {
        }

        public virtual void SaveData()
        {
        }

        public void SetUpdateOrder(MyUpdateOrder order)
        {
            this.Session.SetComponentUpdateOrder(this, order);
            this.UpdateOrder = order;
        }

        public virtual void Simulate()
        {
        }

        public override string ToString() => 
            this.DebugName;

        protected virtual void UnloadData()
        {
        }

        public void UnloadDataConditional()
        {
            if (this.Loaded)
            {
                this.UnloadData();
                this.Loaded = false;
            }
        }

        public virtual void UpdateAfterSimulation()
        {
        }

        public virtual void UpdateBeforeSimulation()
        {
        }

        public virtual bool UpdatedBeforeInit() => 
            false;

        public virtual void UpdatingStopped()
        {
        }

        public MyUpdateOrder UpdateOrder { get; private set; }

        public MyObjectBuilderType ObjectBuilderType { get; private set; }

        public IMyModContext ModContext { get; set; }

        public bool Loaded { get; private set; }

        public bool Initialized =>
            this.m_initialized;

        public bool UpdateOnPause { get; set; }

        public MyDefinitionId? Definition { get; set; }

        public virtual Type[] Dependencies =>
            Type.EmptyTypes;

        public virtual bool IsRequiredByGame =>
            false;
    }
}

