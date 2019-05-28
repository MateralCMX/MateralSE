namespace Sandbox.Game
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class MyCreditsDepartment
    {
        public StringBuilder Name;
        public List<MyCreditsPerson> Persons;
        public string LogoTexture;
        public Vector2? LogoNormalizedSize;
        public float? LogoScale;
        public float LogoOffsetPre = 0.07f;
        public float LogoOffsetPost = 0.07f;

        public MyCreditsDepartment(string name)
        {
            this.Name = new StringBuilder(name);
            this.Persons = new List<MyCreditsPerson>();
        }
    }
}

