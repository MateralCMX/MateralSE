namespace VRage.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public class MyFileProviderAggregator : IFileProvider
    {
        private HashSet<IFileProvider> m_providers = new HashSet<IFileProvider>();

        public MyFileProviderAggregator(params IFileProvider[] providers)
        {
            foreach (IFileProvider provider in providers)
            {
                this.AddProvider(provider);
            }
        }

        public void AddProvider(IFileProvider provider)
        {
            this.m_providers.Add(provider);
        }

        public bool DirectoryExists(string path)
        {
            using (HashSet<IFileProvider>.Enumerator enumerator = this.m_providers.GetEnumerator())
            {
                while (true)
                {
                    bool flag;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IFileProvider current = enumerator.Current;
                    try
                    {
                        if (!current.DirectoryExists(path))
                        {
                            continue;
                        }
                        flag = true;
                    }
                    catch
                    {
                        continue;
                    }
                    return flag;
                }
            }
            return false;
        }

        public bool FileExists(string path)
        {
            using (HashSet<IFileProvider>.Enumerator enumerator = this.m_providers.GetEnumerator())
            {
                while (true)
                {
                    bool flag;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IFileProvider current = enumerator.Current;
                    try
                    {
                        if (!current.FileExists(path))
                        {
                            continue;
                        }
                        flag = true;
                    }
                    catch
                    {
                        continue;
                    }
                    return flag;
                }
            }
            return false;
        }

        public IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption)
        {
            using (HashSet<IFileProvider>.Enumerator enumerator = this.m_providers.GetEnumerator())
            {
                while (true)
                {
                    IEnumerable<string> enumerable2;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IFileProvider current = enumerator.Current;
                    try
                    {
                        IEnumerable<string> enumerable = current.GetFiles(path, filter, searchOption);
                        if (enumerable == null)
                        {
                            continue;
                        }
                        enumerable2 = enumerable;
                    }
                    catch
                    {
                        continue;
                    }
                    return enumerable2;
                }
            }
            return null;
        }

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            using (HashSet<IFileProvider>.Enumerator enumerator = this.m_providers.GetEnumerator())
            {
                while (true)
                {
                    Stream stream2;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    IFileProvider current = enumerator.Current;
                    try
                    {
                        Stream stream = current.Open(path, mode, access, share);
                        if (stream == null)
                        {
                            continue;
                        }
                        stream2 = stream;
                    }
                    catch
                    {
                        continue;
                    }
                    return stream2;
                }
            }
            return null;
        }

        public Stream OpenRead(string path) => 
            this.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        public Stream OpenWrite(string path, FileMode mode = 4) => 
            this.Open(path, mode, FileAccess.Write, FileShare.Read);

        public void RemoveProvider(IFileProvider provider)
        {
            this.m_providers.Remove(provider);
        }

        public HashSetReader<IFileProvider> Providers =>
            new HashSetReader<IFileProvider>(this.m_providers);
    }
}

