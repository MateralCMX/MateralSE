namespace VRage.Game.VisualScripting.ScriptBuilder
{
    using Microsoft.CodeAnalysis;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Collections;

    public class MyDependencyCollector
    {
        private HashSet<MetadataReference> m_references;

        public MyDependencyCollector()
        {
            this.m_references = new HashSet<MetadataReference>();
        }

        public MyDependencyCollector(IEnumerable<Assembly> assemblies) : this()
        {
            foreach (Assembly assembly in assemblies)
            {
                this.CollectReferences(assembly);
            }
        }

        public void CollectReferences(Assembly assembly)
        {
            if (assembly != null)
            {
                MetadataReferenceProperties properties;
                AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
                for (int i = 0; i < referencedAssemblies.Length; i++)
                {
                    Assembly assembly2 = Assembly.Load(referencedAssemblies[i]);
                    properties = new MetadataReferenceProperties();
                    this.m_references.Add(MetadataReference.CreateFromFile(assembly2.Location, properties, null));
                }
                properties = new MetadataReferenceProperties();
                this.m_references.Add(MetadataReference.CreateFromFile(assembly.Location, properties, null));
            }
        }

        public void RegisterAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                MetadataReferenceProperties properties = new MetadataReferenceProperties();
                this.m_references.Add(MetadataReference.CreateFromFile(assembly.Location, properties, null));
            }
        }

        public HashSetReader<MetadataReference> References =>
            new HashSetReader<MetadataReference>(this.m_references);
    }
}

