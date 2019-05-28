namespace VRage.Utils
{
    using System;
    using System.Reflection;

    public static class MyMergeHelper
    {
        private static bool IsPrimitive(Type type) => 
            (type.IsPrimitive || ((type == typeof(string)) || (type == typeof(Type))));

        public static void Merge<T>(T self, T source, T other) where T: class
        {
            if (self == null)
            {
                MyLog.Default.WriteLine("self cannot be null!!! type: " + typeof(T));
            }
            if (source == null)
            {
                MyLog.Default.WriteLine("Source cannot be null!!! type: " + typeof(T));
            }
            if (other == null)
            {
                MyLog.Default.WriteLine("Other cannot be null!!! type: " + typeof(T));
            }
            object obj2 = self;
            object obj3 = source;
            object obj4 = other;
            MergeInternal(typeof(T), ref obj2, ref obj3, ref obj4);
        }

        public static void Merge<T>(ref T self, ref T source, ref T other) where T: struct
        {
            object obj2 = (T) self;
            object obj3 = (T) source;
            object obj4 = (T) other;
            MergeInternal(typeof(T), ref obj2, ref obj3, ref obj4);
            self = (T) obj2;
        }

        private static void MergeInternal(Type type, ref object self, ref object source, ref object other)
        {
            FieldInfo info;
            object obj2;
            if (type == null)
            {
                object[] objArray1 = new object[] { "type cannot be null!!! self: ", self, " source: ", source, " other: ", other };
                MyLog.Default.WriteLine(string.Concat(objArray1));
            }
            if (self == null)
            {
                self = Activator.CreateInstance(type);
            }
            if (source == null)
            {
                source = Activator.CreateInstance(type);
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            int index = 0;
            goto TR_000F;
        TR_0001:
            index++;
        TR_000F:
            while (true)
            {
                if (index >= fields.Length)
                {
                    return;
                }
                info = fields[index];
                obj2 = info.GetValue(source);
                object obj3 = info.GetValue(other);
                if (obj2 == obj3)
                {
                    goto TR_0001;
                }
                else if (obj2 != null)
                {
                    bool flag = false;
                    if ((!IsPrimitive(info.FieldType) || (flag = obj2.Equals(obj3))) && ((obj2 == null) || (obj3 != null)))
                    {
                        if (!flag)
                        {
                            object obj4 = info.GetValue(self);
                            MergeInternal(info.FieldType, ref obj4, ref obj2, ref obj3);
                            info.SetValue(self, obj4);
                        }
                        goto TR_0001;
                    }
                }
                else
                {
                    MyLog.Default.WriteLine("ERROR: Error detected related to the following resource: " + obj3 + " Please check your definition files and reload");
                    object[] objArray2 = new object[] { "More info MergeInternal: field: ", info, " source: ", source, " , other: ", other, " , valueOther: ", obj3 };
                    MyLog.Default.WriteLine(string.Concat(objArray2));
                    goto TR_0001;
                }
                break;
            }
            info.SetValue(self, obj2);
            goto TR_0001;
        }
    }
}

