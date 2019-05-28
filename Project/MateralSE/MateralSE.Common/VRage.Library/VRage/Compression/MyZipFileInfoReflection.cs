namespace VRage.Compression
{
    using System;
    using System.Reflection;

    public class MyZipFileInfoReflection
    {
        public static readonly BindingFlags Bind = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        public static readonly Type ZipFileInfoType = MyZipArchiveReflection.ZipAssembly.GetType("MS.Internal.IO.Zip.ZipFileInfo");
        public static readonly PropertyInfo CompressionMethodProperty = ZipFileInfoType.GetProperty("CompressionMethod", Bind);
        public static readonly PropertyInfo DeflateOptionProperty = ZipFileInfoType.GetProperty("DeflateOption", Bind);
        public static readonly PropertyInfo FolderFlagProperty = ZipFileInfoType.GetProperty("FolderFlag", Bind);
        public static readonly PropertyInfo LastModFileDateTimeProperty = ZipFileInfoType.GetProperty("LastModFileDateTime", Bind);
        public static readonly PropertyInfo NameProperty = ZipFileInfoType.GetProperty("Name", Bind);
        public static readonly PropertyInfo VolumeLabelFlagProperty = ZipFileInfoType.GetProperty("VolumeLabelFlag", Bind);
        public static readonly MethodInfo GetStreamMethod = ZipFileInfoType.GetMethod("GetStream", Bind);
        public static readonly Func<object, ushort> CompressionMethod = CompressionMethodProperty.CreateGetter<object, ushort>();
        public static readonly Func<object, byte> DeflateOption = DeflateOptionProperty.CreateGetter<object, byte>();
        public static readonly Func<object, bool> FolderFlag = FolderFlagProperty.CreateGetter<object, bool>();
        public static readonly Func<object, DateTime> LastModFileDateTime = LastModFileDateTimeProperty.CreateGetter<object, DateTime>();
        public static readonly Func<object, string> Name = NameProperty.CreateGetter<object, string>();
        public static readonly Func<object, bool> VolumeLabelFlag = VolumeLabelFlagProperty.CreateGetter<object, bool>();
        public static readonly Func<object, FileMode, FileAccess, Stream> GetStream = GetStreamMethod.InstanceCall<Func<object, FileMode, FileAccess, Stream>>();
    }
}

