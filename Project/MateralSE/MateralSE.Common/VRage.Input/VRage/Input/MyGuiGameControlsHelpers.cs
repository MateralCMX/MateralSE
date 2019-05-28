namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using VRage.Utils;

    public static class MyGuiGameControlsHelpers
    {
        private static readonly Dictionary<MyStringId, MyGuiDescriptor> m_gameControlHelpers = new Dictionary<MyStringId, MyGuiDescriptor>(MyStringId.Comparer);

        static MyGuiGameControlsHelpers()
        {
            MyLog.Default.WriteLine("MyGuiGameControlsHelpers()");
        }

        public static void Add(MyStringId control, MyGuiDescriptor descriptor)
        {
            m_gameControlHelpers.Add(control, descriptor);
        }

        public static MyGuiDescriptor GetGameControlHelper(MyStringId controlHelper)
        {
            MyGuiDescriptor descriptor;
            return (!m_gameControlHelpers.TryGetValue(controlHelper, out descriptor) ? null : descriptor);
        }

        public static void Reset()
        {
            m_gameControlHelpers.Clear();
        }
    }
}

