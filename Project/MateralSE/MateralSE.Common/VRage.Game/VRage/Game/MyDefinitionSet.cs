namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game.Definitions;
    using VRage.Utils;

    public class MyDefinitionSet
    {
        public MyModContext Context;
        public readonly Dictionary<Type, Dictionary<MyStringHash, MyDefinitionBase>> Definitions = new Dictionary<Type, Dictionary<MyStringHash, MyDefinitionBase>>();

        public void AddDefinition(MyDefinitionBase def)
        {
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            if (!this.Definitions.TryGetValue((Type) def.Id.TypeId, out dictionary))
            {
                dictionary = new Dictionary<MyStringHash, MyDefinitionBase>();
                this.Definitions[(Type) def.Id.TypeId] = dictionary;
            }
            dictionary[def.Id.SubtypeId] = def;
        }

        public bool AddOrRelaceDefinition(MyDefinitionBase def)
        {
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            if (!this.Definitions.TryGetValue((Type) def.Id.TypeId, out dictionary))
            {
                dictionary = new Dictionary<MyStringHash, MyDefinitionBase>();
                this.Definitions[(Type) def.Id.TypeId] = dictionary;
            }
            dictionary[def.Id.SubtypeId] = def;
            return dictionary.ContainsKey(def.Id.SubtypeId);
        }

        public void Clear()
        {
            foreach (KeyValuePair<Type, Dictionary<MyStringHash, MyDefinitionBase>> pair in this.Definitions)
            {
                pair.Value.Clear();
            }
        }

        public bool ContainsDefinition(MyDefinitionId id)
        {
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            return (this.Definitions.TryGetValue((Type) id.TypeId, out dictionary) && dictionary.ContainsKey(id.SubtypeId));
        }

        public T GetDefinition<T>(MyDefinitionId id) where T: MyDefinitionBase
        {
            MyDefinitionBase base2 = null;
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            if (this.Definitions.TryGetValue((Type) id.TypeId, out dictionary))
            {
                dictionary.TryGetValue(id.SubtypeId, out base2);
            }
            return (base2 as T);
        }

        public T GetDefinition<T>(MyStringHash subtypeId) where T: MyDefinitionBase
        {
            MyDefinitionBase base2 = null;
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            Type objectBuilderType = MyDefinitionManagerBase.GetObjectBuilderType(typeof(T));
            if (this.Definitions.TryGetValue(objectBuilderType, out dictionary))
            {
                dictionary.TryGetValue(subtypeId, out base2);
            }
            return (T) base2;
        }

        public IEnumerable<T> GetDefinitionsOfType<T>() where T: MyDefinitionBase
        {
            Dictionary<MyStringHash, MyDefinitionBase> dictionary = null;
            return (!this.Definitions.TryGetValue(MyDefinitionManagerBase.GetObjectBuilderType(typeof(T)), out dictionary) ? null : dictionary.Values.Cast<T>());
        }

        public IEnumerable<T> GetDefinitionsOfTypeAndSubtypes<T>() where T: MyDefinitionBase
        {
            HashSet<Type> subtypes = MyDefinitionManagerBase.Static.GetSubtypes<T>();
            Dictionary<MyStringHash, MyDefinitionBase> dictionary = null;
            return ((subtypes != null) ? (from x in subtypes select this.Definitions.GetOrEmpty<Type, MyStringHash, MyDefinitionBase>(MyDefinitionManagerBase.GetObjectBuilderType(x)).Cast<T>()) : (!this.Definitions.TryGetValue(MyDefinitionManagerBase.GetObjectBuilderType(typeof(T)), out dictionary) ? null : dictionary.Values.Cast<T>()));
        }

        public virtual void OverrideBy(MyDefinitionSet definitionSet)
        {
            MyDefinitionPostprocessor.Bundle bundle3 = new MyDefinitionPostprocessor.Bundle {
                Set = this,
                Context = this.Context
            };
            MyDefinitionPostprocessor.Bundle currentDefinitions = bundle3;
            bundle3 = new MyDefinitionPostprocessor.Bundle {
                Set = definitionSet,
                Context = definitionSet.Context
            };
            MyDefinitionPostprocessor.Bundle overrideBySet = bundle3;
            foreach (KeyValuePair<Type, Dictionary<MyStringHash, MyDefinitionBase>> pair in definitionSet.Definitions)
            {
                Dictionary<MyStringHash, MyDefinitionBase> dictionary;
                if (!this.Definitions.TryGetValue(pair.Key, out dictionary))
                {
                    dictionary = new Dictionary<MyStringHash, MyDefinitionBase>();
                    this.Definitions[pair.Key] = dictionary;
                }
                MyDefinitionPostprocessor postProcessor = MyDefinitionManagerBase.GetPostProcessor(pair.Key);
                if (postProcessor == null)
                {
                    postProcessor = MyDefinitionManagerBase.GetPostProcessor(MyDefinitionManagerBase.GetObjectBuilderType(pair.Value.First<KeyValuePair<MyStringHash, MyDefinitionBase>>().Value.GetType()));
                }
                currentDefinitions.Definitions = dictionary;
                overrideBySet.Definitions = pair.Value;
                postProcessor.OverrideBy(ref currentDefinitions, ref overrideBySet);
            }
        }

        public void RemoveDefinition(ref MyDefinitionId defId)
        {
            Dictionary<MyStringHash, MyDefinitionBase> dictionary;
            if (this.Definitions.TryGetValue((Type) defId.TypeId, out dictionary))
            {
                dictionary.Remove(defId.SubtypeId);
            }
        }
    }
}

