namespace Sandbox.Game
{
    using System;
    using System.Text;

    public class MyCreditsPerson
    {
        public StringBuilder Name;

        public MyCreditsPerson(string name)
        {
            this.Name = new StringBuilder(name);
        }
    }
}

