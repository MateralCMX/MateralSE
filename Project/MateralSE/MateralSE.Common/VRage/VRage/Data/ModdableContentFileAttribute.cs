namespace VRage.Data
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple=false)]
    public class ModdableContentFileAttribute : Attribute
    {
        public string[] FileExtensions;

        public ModdableContentFileAttribute(string fileExtension)
        {
            this.FileExtensions = new string[] { fileExtension };
        }

        public ModdableContentFileAttribute(params string[] fileExtensions)
        {
            this.FileExtensions = fileExtensions;
        }
    }
}

