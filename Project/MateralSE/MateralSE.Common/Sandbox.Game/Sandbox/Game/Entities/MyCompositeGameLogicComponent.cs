namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.EntityComponents.Interfaces;
    using VRage.ObjectBuilders;

    public class MyCompositeGameLogicComponent : MyGameLogicComponent, IMyGameLogicComponent
    {
        private ICollection<MyGameLogicComponent> m_logicComponents;

        private MyCompositeGameLogicComponent(ICollection<MyGameLogicComponent> logicComponents)
        {
            this.m_logicComponents = logicComponents;
        }

        public override void Close()
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
        }

        public static MyGameLogicComponent Create(ICollection<MyGameLogicComponent> logicComponents, MyEntity entity)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.SetContainer(entity.Components);
                }
            }
            int count = logicComponents.Count;
            return ((count == 0) ? null : ((count == 1) ? logicComponents.First<MyGameLogicComponent>() : new MyCompositeGameLogicComponent(logicComponents)));
        }

        public override T GetAs<T>() where T: MyComponentBase
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyGameLogicComponent current = enumerator.Current;
                    if (current is T)
                    {
                        return (current as T);
                    }
                }
            }
            return default(T);
        }

        public MyGameLogicComponent GetAs(string typeName) => 
            this.m_logicComponents.FirstOrDefault<MyGameLogicComponent>(c => (c.GetType().FullName == typeName));

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyObjectBuilder_EntityBase objectBuilder = enumerator.Current.GetObjectBuilder(copy);
                    if (objectBuilder != null)
                    {
                        return objectBuilder;
                    }
                }
            }
            return null;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Init(objectBuilder);
                }
            }
        }

        public override void MarkForClose()
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.MarkForClose();
                }
            }
        }

        void IMyGameLogicComponent.Close()
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Close();
                }
            }
        }

        void IMyGameLogicComponent.RegisterForUpdate()
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.RegisterForUpdate();
                }
            }
        }

        void IMyGameLogicComponent.UnregisterForUpdate()
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UnregisterForUpdate();
                }
            }
        }

        void IMyGameLogicComponent.UpdateAfterSimulation(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateAfterSimulation(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateAfterSimulation10(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateAfterSimulation10(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateAfterSimulation100(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateAfterSimulation100(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateBeforeSimulation(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation10(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateBeforeSimulation10(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateBeforeSimulation100(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateBeforeSimulation100(entityUpdate);
                }
            }
        }

        void IMyGameLogicComponent.UpdateOnceBeforeFrame(bool entityUpdate)
        {
            using (IEnumerator<MyGameLogicComponent> enumerator = this.m_logicComponents.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateOnceBeforeFrame(entityUpdate);
                }
            }
        }
    }
}

