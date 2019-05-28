namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlTabPage))]
    public class MyGuiControlTabPage : MyGuiControlParent
    {
        private MyStringId m_textEnum;
        private StringBuilder m_text;
        private int m_pageKey;
        public float TextScale;

        public MyGuiControlTabPage() : this(0, nullable, nullable, nullable2, 1f)
        {
            Vector2? nullable = null;
            nullable = null;
        }

        public MyGuiControlTabPage(int pageKey, Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Vector4? color = new Vector4?(), float captionTextScale = 1f) : base(position, size, color, null)
        {
            base.Name = "TabPage";
            this.m_pageKey = pageKey;
            this.TextScale = captionTextScale;
            this.IsTabVisible = true;
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlTabPage objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_GuiControlTabPage;
            objectBuilder.PageKey = this.PageKey;
            objectBuilder.TextEnum = this.TextEnum.ToString();
            objectBuilder.TextScale = this.TextScale;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GuiControlTabPage page = builder as MyObjectBuilder_GuiControlTabPage;
            this.m_pageKey = page.PageKey;
            this.TextEnum = MyStringId.GetOrCompute(page.TextEnum);
            this.TextScale = page.TextScale;
        }

        public int PageKey =>
            this.m_pageKey;

        public MyStringId TextEnum
        {
            get => 
                this.m_textEnum;
            set
            {
                this.m_textEnum = value;
                this.m_text = MyTexts.Get(this.m_textEnum);
            }
        }

        public StringBuilder Text
        {
            get => 
                this.m_text;
            set => 
                (this.m_text = value);
        }

        public bool IsTabVisible { get; set; }
    }
}

