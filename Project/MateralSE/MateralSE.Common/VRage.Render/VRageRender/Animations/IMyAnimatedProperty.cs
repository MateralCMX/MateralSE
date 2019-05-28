namespace VRageRender.Animations
{
    using System;
    using System.Runtime.InteropServices;

    public interface IMyAnimatedProperty : IMyConstProperty
    {
        int AddKey(float time, object val);
        void ClearKeys();
        void GetInterpolatedValue(float time, out object value);
        void GetKey(int index, out float time, out object value);
        void GetKey(int index, out int id, out float time, out object value);
        void GetKeyByID(int id, out float time, out object value);
        int GetKeysCount();
        void RemoveKey(int index);
        void RemoveKey(float time);
        void RemoveKeyByID(int id);
        void SetKey(int index, float time);
        void SetKey(int index, float time, object value);
        void SetKeyByID(int id, float time);
        void SetKeyByID(int id, float time, object value);
    }
}

