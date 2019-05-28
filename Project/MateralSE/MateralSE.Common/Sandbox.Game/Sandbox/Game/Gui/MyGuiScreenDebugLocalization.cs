namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRageMath;

    [MyDebugScreen("Game", "Localization")]
    internal class MyGuiScreenDebugLocalization : MyGuiScreenDebugBase
    {
        private MyGuiControlListbox m_quotesListbox;
        private MyGuiControlMultilineText m_quotesDisplay;

        public MyGuiScreenDebugLocalization() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugLocalization";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Localization", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f * base.m_scale;
            base.AddLabel("Loading Screen Texts", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.m_quotesListbox = base.AddListBox(0.185f, null);
            this.m_quotesListbox.MultiSelect = false;
            this.m_quotesListbox.VisibleRowsCount = 5;
            foreach (MyLoadingScreenText text in MyLoadingScreenText.Texts)
            {
                StringBuilder userData = new StringBuilder();
                MyLoadingScreenQuote quote = text as MyLoadingScreenQuote;
                if (quote == null)
                {
                    userData.AppendLine(text.ToString());
                }
                else
                {
                    userData.Append(MyTexts.Get(text.Text));
                    userData.AppendLine();
                    userData.AppendLine().Append("- ").AppendStringBuilder(MyTexts.Get(quote.Author)).Append(" -");
                }
                MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(text.Text.String), userData.ToString(), null, userData, null);
                this.m_quotesListbox.Items.Add(item);
            }
            captionOffset = null;
            this.m_quotesDisplay = base.AddMultilineText(new Vector2(this.m_quotesListbox.Size.X, 0.2f), captionOffset, 1f, false);
            this.m_quotesDisplay.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            this.m_quotesListbox.ItemsSelected += delegate (MyGuiControlListbox e) {
                this.m_quotesDisplay.Clear();
                if (e.SelectedItem != null)
                {
                    this.m_quotesDisplay.AppendText((StringBuilder) e.SelectedItem.UserData);
                }
            };
        }
    }
}

