namespace VRageRender.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.Security;

    public static class ModelAutoRebuild
    {
        private static MyModelImporter m_importer = new MyModelImporter();

        public static Md5.Hash GetFileHash(string fileName) => 
            Md5.ComputeHash(File.ReadAllBytes(fileName));

        public static bool IsModelActual(string modelFile, string FBXFile, string HKTFile, string XMLFile)
        {
            m_importer.ImportData(modelFile, null);
            Dictionary<string, object> tagData = m_importer.GetTagData();
            if (File.Exists(FBXFile))
            {
                if (tagData.GetValueOrDefault<string, object>("FBXHash") == null)
                {
                    return false;
                }
                Md5.Hash hash = GetFileHash(FBXFile);
                Md5.Hash hash2 = (Md5.Hash) tagData.GetValueOrDefault<string, object>("FBXHash");
                if (((hash.A != hash2.A) || ((hash.B != hash2.B) || (hash.C != hash2.C))) || (hash.D != hash2.D))
                {
                    return false;
                }
            }
            if (File.Exists(HKTFile))
            {
                if (tagData.GetValueOrDefault<string, object>("HKTHash") == null)
                {
                    return false;
                }
                Md5.Hash hash3 = GetFileHash(HKTFile);
                Md5.Hash hash4 = (Md5.Hash) tagData.GetValueOrDefault<string, object>("HKTHash");
                if (((hash3.A != hash4.A) || ((hash3.B != hash4.B) || (hash3.C != hash4.C))) || (hash3.D != hash4.D))
                {
                    return false;
                }
            }
            if (!File.Exists(XMLFile))
            {
                return true;
            }
            if ((tagData.GetValueOrDefault<string, object>("XMLHash") == null) || !File.Exists(XMLFile))
            {
                return false;
            }
            Md5.Hash fileHash = GetFileHash(XMLFile);
            Md5.Hash valueOrDefault = (Md5.Hash) tagData.GetValueOrDefault<string, object>("XMLHash");
            return ((fileHash.A == valueOrDefault.A) && ((fileHash.B == valueOrDefault.B) && ((fileHash.C == valueOrDefault.C) && (fileHash.D == valueOrDefault.D))));
        }
    }
}

