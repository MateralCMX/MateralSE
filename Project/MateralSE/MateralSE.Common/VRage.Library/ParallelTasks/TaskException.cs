namespace ParallelTasks
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Library;

    [Serializable]
    public class TaskException : Exception
    {
        public TaskException(Exception[] inner) : base("An exception(s) was thrown while executing a task.", null)
        {
            this.InnerExceptions = inner;
        }

        public override string ToString()
        {
            string str = base.ToString() + MyEnvironment.NewLine;
            for (int i = 0; i < this.InnerExceptions.Length; i++)
            {
                str = (str + $"Task exception, inner exception {i}:" + MyEnvironment.NewLine) + this.InnerExceptions[i].ToString();
            }
            return str;
        }

        public Exception[] InnerExceptions { get; private set; }
    }
}

