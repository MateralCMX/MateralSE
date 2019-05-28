namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;

    public static class AssemblyExtensions
    {
        public static ProcessorArchitecture GetArchitecture(this Assembly assembly) => 
            assembly.GetPeKind().ToProcessorArchitecture();

        public static PortableExecutableKinds GetPeKind(this Assembly assembly)
        {
            PortableExecutableKinds kinds;
            ImageFileMachine machine;
            assembly.ManifestModule.GetPEKind(out kinds, out machine);
            return kinds;
        }

        public static ProcessorArchitecture ToProcessorArchitecture(this PortableExecutableKinds peKind)
        {
            PortableExecutableKinds kinds = peKind & ~PortableExecutableKinds.ILOnly;
            return ((kinds == PortableExecutableKinds.Required32Bit) ? ProcessorArchitecture.X86 : ((kinds == PortableExecutableKinds.PE32Plus) ? ProcessorArchitecture.Amd64 : ((kinds == PortableExecutableKinds.Unmanaged32Bit) ? ProcessorArchitecture.X86 : (((peKind & PortableExecutableKinds.ILOnly) != PortableExecutableKinds.NotAPortableExecutableImage) ? ProcessorArchitecture.MSIL : ProcessorArchitecture.None))));
        }

        public static ProcessorArchitecture TryGetArchitecture(this Assembly assembly)
        {
            try
            {
                return assembly.GetArchitecture();
            }
            catch
            {
                return ProcessorArchitecture.None;
            }
        }

        public static ProcessorArchitecture TryGetArchitecture(string assemblyName)
        {
            try
            {
                return AssemblyName.GetAssemblyName(assemblyName).ProcessorArchitecture;
            }
            catch
            {
                return ProcessorArchitecture.None;
            }
        }
    }
}

