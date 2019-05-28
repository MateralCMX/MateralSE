namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Generics;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyAnimationVariableStorage : IMyVariableStorage<float>
    {
        private readonly Dictionary<MyStringId, float> m_storage = new Dictionary<MyStringId, float>(MyStringId.Comparer);
        private readonly MyRandom m_random = new MyRandom();
        private readonly FastResourceLock m_lock = new FastResourceLock();

        public void Clear()
        {
            this.m_storage.Clear();
        }

        public bool GetValue(MyStringId key, out float value)
        {
            if (key == MyAnimationVariableStorageHints.StrIdRandom)
            {
                value = this.m_random.NextFloat();
                return true;
            }
            using (this.m_lock.AcquireSharedUsing())
            {
                return this.m_storage.TryGetValue(key, out value);
            }
        }

        public void SetValue(MyStringId key, float newValue)
        {
            using (this.m_lock.AcquireExclusiveUsing())
            {
                this.m_storage[key] = newValue;
            }
        }

        public DictionaryReader<MyStringId, float> AllVariables =>
            this.m_storage;
    }
}

