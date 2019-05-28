namespace VRage.Compression
{
    using System;
    using System.Reflection;

    public class MyZipArchiveReflection
    {
        public static readonly BindingFlags StaticBind = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        public static readonly BindingFlags InstanceBind = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        public static readonly Assembly ZipAssembly = typeof(Package).Assembly;
        public static readonly Type ZipArchiveType = ZipAssembly.GetType("MS.Internal.IO.Zip.ZipArchive");
        public static readonly Type CompressionMethodType = ZipAssembly.GetType("MS.Internal.IO.Zip.CompressionMethodEnum");
        public static readonly Type DeflateOptionType = ZipAssembly.GetType("MS.Internal.IO.Zip.DeflateOptionEnum");
        public static readonly MethodInfo OpenOnFileMethod = ZipArchiveType.GetMethod("OpenOnFile", StaticBind);
        public static readonly MethodInfo OpenOnStreamMethod = ZipArchiveType.GetMethod("OpenOnStream", StaticBind);
        public static readonly MethodInfo GetFilesMethod = ZipArchiveType.GetMethod("GetFiles", InstanceBind);
        public static readonly MethodInfo GetFileMethod = ZipArchiveType.GetMethod("GetFile", InstanceBind);
        public static readonly MethodInfo FileExistsMethod = ZipArchiveType.GetMethod("FileExists", InstanceBind);
        public static readonly MethodInfo AddFileMethod = ZipArchiveType.GetMethod("AddFile", InstanceBind);
        public static readonly MethodInfo DeleteFileMethod = ZipArchiveType.GetMethod("DeleteFile", InstanceBind);
        public static readonly Func<string, FileMode, FileAccess, FileShare, bool, object> OpenOnFile = OpenOnFileMethod.StaticCall<Func<string, FileMode, FileAccess, FileShare, bool, object>>();
        public static readonly Func<Stream, FileMode, FileAccess, bool, object> OpenOnStream = OpenOnStreamMethod.StaticCall<Func<Stream, FileMode, FileAccess, bool, object>>();
        public static readonly Func<object, object> GetFiles = GetFilesMethod.InstanceCall<Func<object, object>>();
        public static readonly Func<object, string, object> GetFile = GetFileMethod.InstanceCall<Func<object, string, object>>();
        public static readonly Func<object, string, bool> FileExists = FileExistsMethod.InstanceCall<Func<object, string, bool>>();
        public static readonly Func<object, string, ushort, byte, object> AddFile = AddFileMethod.InstanceCall<Func<object, string, ushort, byte, object>>();
        public static readonly Action<object, string> DeleteFile = DeleteFileMethod.InstanceCall<Action<object, string>>();
    }
}

