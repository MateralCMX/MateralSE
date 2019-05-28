namespace VRage.Library.Utils
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;

    public static class MyImageHeaderUtils
    {
        private const uint DDS_MAGIC = 0x20534444;
        private const uint PNG_MAGIC = 0x474e5089;

        public static bool Read_DDS_HeaderData(string filePath, out DDS_HEADER header)
        {
            DDS_HEADER dds_header = new DDS_HEADER {
                dwReserved1 = new uint[11]
            };
            header = dds_header;
            if (!MyFileSystem.FileExists(filePath))
            {
                return false;
            }
            using (Stream stream = MyFileSystem.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (reader.ReadUInt32() == 0x20534444)
                    {
                        header.dwSize = reader.ReadUInt32();
                        header.dwFlags = reader.ReadUInt32();
                        header.dwHeight = reader.ReadUInt32();
                        header.dwWidth = reader.ReadUInt32();
                        header.dwPitchOrLinearSize = reader.ReadUInt32();
                        header.dwDepth = reader.ReadUInt32();
                        header.dwMipMapCount = reader.ReadUInt32();
                        int index = 0;
                        while (true)
                        {
                            if (index >= 11)
                            {
                                header.ddspf.dwSize = reader.ReadUInt32();
                                header.ddspf.dwFlags = reader.ReadUInt32();
                                header.ddspf.dwFourCC = reader.ReadUInt32();
                                header.ddspf.dwRGBBitCount = reader.ReadUInt32();
                                header.ddspf.dwRBitMask = reader.ReadUInt32();
                                header.ddspf.dwGBitMask = reader.ReadUInt32();
                                header.ddspf.dwBBitMask = reader.ReadUInt32();
                                header.ddspf.dwABitMask = reader.ReadUInt32();
                                header.dwCaps = reader.ReadUInt32();
                                header.dwCaps2 = reader.ReadUInt32();
                                header.dwCaps3 = reader.ReadUInt32();
                                header.dwCaps4 = reader.ReadUInt32();
                                header.dwReserved2 = reader.ReadUInt32();
                                break;
                            }
                            header.dwReserved1[index] = reader.ReadUInt32();
                            index++;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool Read_PNG_Dimensions(string filePath, out int width, out int height)
        {
            bool flag;
            width = 0;
            height = 0;
            if (!MyFileSystem.FileExists(filePath))
            {
                return false;
            }
            using (Stream stream = MyFileSystem.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    if (reader.ReadUInt32() != 0x474e5089)
                    {
                        flag = false;
                    }
                    else
                    {
                        reader.ReadBytes(12);
                        width = reader.ReadLittleEndianInt32();
                        height = reader.ReadLittleEndianInt32();
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private static int ReadLittleEndianInt32(this BinaryReader binaryReader)
        {
            byte[] buffer = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                buffer[3 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DDS_HEADER
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwHeight;
            public uint dwWidth;
            public uint dwPitchOrLinearSize;
            public uint dwDepth;
            public uint dwMipMapCount;
            public uint[] dwReserved1;
            public MyImageHeaderUtils.DDS_PIXELFORMAT ddspf;
            public uint dwCaps;
            public uint dwCaps2;
            public uint dwCaps3;
            public uint dwCaps4;
            public uint dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DDS_PIXELFORMAT
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwFourCC;
            public uint dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
        }
    }
}

