namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRageMath;

    internal abstract class MyRichLabelPart
    {
        protected MyRichLabelPart()
        {
        }

        public virtual void AppendTextTo(StringBuilder builder)
        {
        }

        public abstract bool Draw(Vector2 position, float alphamask, ref int charactersLeft);
        public abstract bool HandleInput(Vector2 position);

        public virtual Vector2 Size { get; protected set; }
    }
}

