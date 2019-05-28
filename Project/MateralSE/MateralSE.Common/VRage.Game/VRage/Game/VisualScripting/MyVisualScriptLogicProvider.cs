namespace VRage.Game.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using VRage.Game.Components.Session;
    using VRage.Game.VisualScripting.Utils;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public static class MyVisualScriptLogicProvider
    {
        private const int m_cornflower = -10185235;
        private const int m_slateBlue = -9807155;
        [Display(Name="Mission", Description="When mission starts.")]
        public static SingleKeyMissionEvent MissionStarted;
        [Display(Name="Mission", Description="When mission finishes.")]
        public static SingleKeyMissionEvent MissionFinished;

        [VisualScriptingMiscData("Math", "Calculates abs function of the value.", -10510688), VisualScriptingMember(false, false)]
        public static float Abs(float value) => 
            Math.Abs(value);

        [VisualScriptingMiscData("Math", "Adds two numbers (integers).", -10510688), VisualScriptingMember(false, false)]
        public static int AddInt(int a, int b) => 
            (a + b);

        [VisualScriptingMiscData("Math", "Calculates ceiling function of the value.", -10510688), VisualScriptingMember(false, false)]
        public static int Ceil(float value) => 
            ((int) Math.Ceiling((double) value));

        [VisualScriptingMiscData("Math", "Clamps the value.", -10510688), VisualScriptingMember(false, false)]
        public static float Clamp(float value, float min, float max) => 
            MathHelper.Clamp(value, min, max);

        [VisualScriptingMiscData("Math", "Creates Vector3 (double) value.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D CreateVector3D(float x = 0f, float y = 0f, float z = 0f) => 
            new Vector3D((double) x, (double) y, (double) z);

        [VisualScriptingMiscData("Math", "Calculates direction vector from the speed.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D DirectionVector(Vector3D speed) => 
            (!(speed == Vector3D.Zero) ? Vector3D.Normalize(speed) : Vector3D.Forward);

        [VisualScriptingMiscData("Math", "Calculates distance of two vectors.", -10510688), VisualScriptingMember(false, false)]
        public static float DistanceVector3(Vector3 posA, Vector3 posB) => 
            Vector3.Distance(posA, posB);

        [VisualScriptingMiscData("Math", "Calculates distance of two vectors.", -10510688), VisualScriptingMember(false, false)]
        public static float DistanceVector3D(Vector3D posA, Vector3D posB) => 
            ((float) Vector3D.Distance(posA, posB));

        [VisualScriptingMiscData("String", "Converts float to string.", -10510688), VisualScriptingMember(false, false)]
        public static string FloatToString(float value) => 
            value.ToString();

        [VisualScriptingMiscData("Math", "Calculates floor function of the value.", -10510688), VisualScriptingMember(false, false)]
        public static int Floor(float value) => 
            ((int) Math.Floor((double) value));

        [VisualScriptingMiscData("Input", "Returns X-coordinate of mouse position.", -10510688), VisualScriptingMember(false, false)]
        public static float GetMouseX() => 
            MyInput.Static.GetMouseXForGamePlayF();

        [VisualScriptingMiscData("Input", "Returns Y-coordinate of mouse position.", -10510688), VisualScriptingMember(false, false)]
        public static float GetMouseY() => 
            MyInput.Static.GetMouseYForGamePlayF();

        [VisualScriptingMiscData("Math", "Gets X, Y, Z of the vector.", -10510688), VisualScriptingMember(false, false)]
        public static void GetVector3DComponents(Vector3D vector, out float x, out float y, out float z)
        {
            x = (float) vector.X;
            y = (float) vector.Y;
            z = (float) vector.Z;
        }

        public static void Init()
        {
            MyVisualScriptingProxy.WhitelistExtensions(typeof(MyVSCollectionExtensions));
            Type type1 = typeof(List<>);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("Insert"), true);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("RemoveAt"), true);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("Clear"), true);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("Add"), true);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("Remove"), true);
            MyVisualScriptingProxy.WhitelistMethod(type1.GetMethod("Contains"), false);
            Type[] types = new Type[] { typeof(int), typeof(int) };
            MyVisualScriptingProxy.WhitelistMethod(typeof(string).GetMethod("Substring", types), true);
        }

        [VisualScriptingMiscData("String", "Converts int to string.", -10510688), VisualScriptingMember(false, false)]
        public static string IntToString(int value) => 
            value.ToString();

        [VisualScriptingMiscData("Input", "Checks if input control is currently pressed.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGameControlPressed(string controlStringId) => 
            MyInput.Static.IsGameControlPressed(MyStringId.GetOrCompute(controlStringId));

        [VisualScriptingMiscData("Input", "Checks if input control is currently released.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGameControlReleased(string controlStringId) => 
            MyInput.Static.IsGameControlReleased(MyStringId.GetOrCompute(controlStringId));

        [VisualScriptingMiscData("Input", "Checks if input control is blacklisted.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsLocalInputBlacklisted(string controlStringId) => 
            MyInput.Static.IsControlBlocked(MyStringId.GetOrCompute(controlStringId));

        [VisualScriptingMiscData("Input", "Checks if input control has been pressed this frame.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsNewGameControlPressed(string controlStringId) => 
            MyInput.Static.IsNewGameControlPressed(MyStringId.GetOrCompute(controlStringId));

        [VisualScriptingMiscData("Input", "Checks if input control has been released this frame.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsNewGameControlReleased(string controlStringId) => 
            MyInput.Static.IsNewGameControlReleased(MyStringId.GetOrCompute(controlStringId));

        [VisualScriptingMiscData("Shared Storage", "Loads boolean from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static bool LoadBool(string key) => 
            ((MySessionComponentScriptSharedStorage.Instance != null) && MySessionComponentScriptSharedStorage.Instance.ReadBool(key));

        [VisualScriptingMiscData("Shared Storage", "Loads float from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static float LoadFloat(string key) => 
            ((MySessionComponentScriptSharedStorage.Instance == null) ? 0f : MySessionComponentScriptSharedStorage.Instance.ReadFloat(key));

        [VisualScriptingMiscData("Shared Storage", "Loads integer from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static int LoadInteger(string key) => 
            ((MySessionComponentScriptSharedStorage.Instance == null) ? 0 : MySessionComponentScriptSharedStorage.Instance.ReadInt(key));

        [VisualScriptingMiscData("Shared Storage", "Loads long integer from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static long LoadLong(string key) => 
            ((MySessionComponentScriptSharedStorage.Instance == null) ? 0L : MySessionComponentScriptSharedStorage.Instance.ReadLong(key));

        [VisualScriptingMiscData("Shared Storage", "Loads string from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static string LoadString(string key) => 
            MySessionComponentScriptSharedStorage.Instance?.ReadString(key);

        [VisualScriptingMiscData("Shared Storage", "Loads Vector3 (doubles) from the shared storage.", -9807155), VisualScriptingMember(false, false)]
        public static Vector3D LoadVector(string key) => 
            ((MySessionComponentScriptSharedStorage.Instance == null) ? Vector3D.Zero : MySessionComponentScriptSharedStorage.Instance.ReadVector3D(key));

        [VisualScriptingMiscData("String", "Converts long integer to string.", -10510688), VisualScriptingMember(false, false)]
        public static string LongToString(long value) => 
            value.ToString();

        [VisualScriptingMiscData("Math", "Calculates maximum of the values.", -10510688), VisualScriptingMember(false, false)]
        public static float Max(float value1, float value2) => 
            Math.Max(value1, value2);

        [VisualScriptingMiscData("Math", "Calculates minimum of the values.", -10510688), VisualScriptingMember(false, false)]
        public static float Min(float value1, float value2) => 
            Math.Min(value1, value2);

        [VisualScriptingMiscData("Math", "Calculates modulo of the number.", -10510688), VisualScriptingMember(false, false)]
        public static int Modulo(int number, int mod) => 
            (number % mod);

        [VisualScriptingMiscData("Math", "Generates random float.", -10510688), VisualScriptingMember(false, false)]
        public static float RandomFloat(float min, float max) => 
            MyUtils.GetRandomFloat(min, max);

        [VisualScriptingMiscData("Math", "Generates random int.", -10510688), VisualScriptingMember(false, false)]
        public static int RandomInt(int min, int max) => 
            MyUtils.GetRandomInt(min, max);

        [VisualScriptingMiscData("Math", "Rounds float value to int.", -10510688), VisualScriptingMember(false, false)]
        public static int Round(float value) => 
            ((int) Math.Round((double) value));

        [VisualScriptingMiscData("Input", "Enables/Disables input control blacklist state.", -10510688), VisualScriptingMember(true, false)]
        public static void SetLocalInputBlacklistState(string controlStringId, bool enabled = false)
        {
            MyInput.Static.SetControlBlock(MyStringId.GetOrCompute(controlStringId), !enabled);
        }

        [VisualScriptingMiscData("Shared Storage", "Stores boolean in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreBool(string key, bool value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("Shared Storage", "Stores float in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreFloat(string key, float value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("Shared Storage", "Stores integer in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreInteger(string key, int value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("Shared Storage", "Stores long integer in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreLong(string key, long value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("Shared Storage", "Stores string in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreString(string key, string value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("Shared Storage", "Stores Vector3 (doubles) in the shared storage. This value is accessible from all scripts.", -10185235), VisualScriptingMember(true, false)]
        public static void StoreVector(string key, Vector3D value)
        {
            if (MySessionComponentScriptSharedStorage.Instance != null)
            {
                MySessionComponentScriptSharedStorage.Instance.Write(key, value, false);
            }
        }

        [VisualScriptingMiscData("String", "Concatenates two strings.", -10510688), VisualScriptingMember(false, false)]
        public static string StringConcat(string a, string b) => 
            (a + b);

        [VisualScriptingMiscData("String", "Checks if value contains another string.", -10510688), VisualScriptingMember(false, false)]
        public static bool StringContains(string value, string contains) => 
            (!string.IsNullOrEmpty(value) ? value.Contains(contains) : false);

        [VisualScriptingMiscData("String", "Checks if string is null or empty.", -10510688), VisualScriptingMember(false, false)]
        public static bool StringIsNullOrEmpty(string value) => 
            ((value == null) || (value.Length == 0));

        [VisualScriptingMiscData("String", "Gets length of the specified string.", -10510688), VisualScriptingMember(false, false)]
        public static int StringLength(string value) => 
            ((value == null) ? 0 : value.Length);

        [VisualScriptingMiscData("String", "Replaces old value with the new value in the specified string.", -10510688), VisualScriptingMember(false, false)]
        public static string StringReplace(string value, string oldValue, string newValue) => 
            value?.Replace(oldValue, newValue);

        [VisualScriptingMiscData("String", "Checks if string starts with another string (Invariant Culture).", -10510688), VisualScriptingMember(false, false)]
        public static bool StringStartsWith(string value, string starting, bool ignoreCase = true) => 
            (!string.IsNullOrEmpty(value) ? value.StartsWith(starting, ignoreCase, CultureInfo.InvariantCulture) : false);

        [VisualScriptingMiscData("String", "Gets substring of the specified string.", -10510688), VisualScriptingMember(false, false)]
        public static string SubString(string value, int startIndex = 0, int length = 0) => 
            ((value == null) ? null : ((length <= 0) ? value.Substring(startIndex) : value.Substring(startIndex, length)));

        [VisualScriptingMiscData("String", "Converts Vector3 (doubles) to string.", -10510688), VisualScriptingMember(false, false)]
        public static string Vector3DToString(Vector3D value) => 
            value.ToString();

        [VisualScriptingMiscData("Math", "Calculates vector length.", -10510688), VisualScriptingMember(false, false)]
        public static float VectorLength(Vector3D speed) => 
            ((float) speed.Length());
    }
}

