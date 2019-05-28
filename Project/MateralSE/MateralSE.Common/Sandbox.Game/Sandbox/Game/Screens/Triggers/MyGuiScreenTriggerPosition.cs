namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenTriggerPosition : MyGuiScreenTrigger
    {
        private MyGuiControlLabel m_labelInsX;
        protected MyGuiControlTextbox m_xCoord;
        private MyGuiControlLabel m_labelInsY;
        protected MyGuiControlTextbox m_yCoord;
        private MyGuiControlLabel m_labelInsZ;
        protected MyGuiControlTextbox m_zCoord;
        private MyGuiControlLabel m_labelRadius;
        protected MyGuiControlTextbox m_radius;
        protected MyGuiControlButton m_pasteButton;
        private const float WINSIZEX = 0.4f;
        private const float WINSIZEY = 0.37f;
        private const float spacingH = 0.01f;
        private string m_clipboardText;
        protected bool m_coordsChanged;
        protected Vector3D m_coords;
        private static readonly string m_ScanPattern = @"GPS:([^:]{0,32}):([\d\.-]*):([\d\.-]*):([\d\.-]*):";

        public MyGuiScreenTriggerPosition(MyTrigger trg) : base(trg, new Vector2(0.5f, 0.42f))
        {
            float x = MyGuiScreenTrigger.MIDDLE_PART_ORIGIN.X - 0.2f;
            float y = -0.185f + MyGuiScreenTrigger.MIDDLE_PART_ORIGIN.Y;
            Vector4? colorMask = null;
            this.m_labelInsX = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_X).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelInsX.Size.X + 0.01f;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox1.Position = new Vector2(x, y);
            textbox1.Size = new Vector2(0.11f - this.m_labelInsX.Size.X, 0.035f);
            textbox1.Name = "textX";
            this.m_xCoord = textbox1;
            this.m_xCoord.Enabled = false;
            x += this.m_xCoord.Size.X + 0.01f;
            colorMask = null;
            this.m_labelInsY = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.388f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Y).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelInsY.Size.X + 0.01f;
            MyGuiControlTextbox textbox2 = new MyGuiControlTextbox();
            textbox2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox2.Position = new Vector2(x, y);
            textbox2.Size = new Vector2(0.11f - this.m_labelInsY.Size.X, 0.035f);
            textbox2.Name = "textY";
            this.m_yCoord = textbox2;
            this.m_yCoord.Enabled = false;
            x += this.m_yCoord.Size.X + 0.01f;
            colorMask = null;
            this.m_labelInsZ = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.TerminalTab_GPS_Z).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelInsZ.Size.X + 0.01f;
            MyGuiControlTextbox textbox3 = new MyGuiControlTextbox();
            textbox3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox3.Position = new Vector2(x, y);
            textbox3.Size = new Vector2(0.11f - this.m_labelInsZ.Size.X, 0.035f);
            textbox3.Name = "textZ";
            this.m_zCoord = textbox3;
            this.m_zCoord.Enabled = false;
            x = MyGuiScreenTrigger.MIDDLE_PART_ORIGIN.X - 0.2f;
            y += this.m_zCoord.Size.Y + 0.01f;
            colorMask = null;
            this.m_labelRadius = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.GuiTriggerPositionRadius).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelRadius.Size.X + 0.01f;
            MyGuiControlTextbox textbox4 = new MyGuiControlTextbox();
            textbox4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox4.Position = new Vector2(x, y);
            textbox4.Size = new Vector2(0.11f - this.m_labelInsZ.Size.X, 0.035f);
            textbox4.Name = "radius";
            this.m_radius = textbox4;
            this.m_radius.TextChanged += new Action<MyGuiControlTextbox>(this.OnRadiusChanged);
            StringBuilder text = MyTexts.Get(MySpaceTexts.GuiTriggerPasteGps);
            Vector2? size = null;
            colorMask = null;
            int? buttonIndex = null;
            this.m_pasteButton = new MyGuiControlButton(new Vector2(x + ((this.m_radius.Size.X + 0.01f) + 0.05f), y), MyGuiControlButtonStyleEnum.Small, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnPasteButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_labelInsX);
            this.Controls.Add(this.m_xCoord);
            this.Controls.Add(this.m_labelInsY);
            this.Controls.Add(this.m_yCoord);
            this.Controls.Add(this.m_labelInsZ);
            this.Controls.Add(this.m_zCoord);
            this.Controls.Add(this.m_labelRadius);
            this.Controls.Add(this.m_radius);
            this.Controls.Add(this.m_pasteButton);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerPosition";

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            base.StrToDouble(this.m_radius.Text);
            base.OnOkButtonClick(sender);
        }

        private void OnPasteButtonClick(MyGuiControlButton sender)
        {
            Thread thread1 = new Thread(() => this.PasteFromClipboard());
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            thread1.Join();
            if (this.ScanText(this.m_clipboardText))
            {
                this.m_coordsChanged = true;
            }
        }

        public void OnRadiusChanged(MyGuiControlTextbox sender)
        {
            if (base.StrToDouble(sender.Text) != null)
            {
                sender.ColorMask = Vector4.One;
                base.m_okButton.Enabled = true;
            }
            else
            {
                sender.ColorMask = Color.Red.ToVector4();
                base.m_okButton.Enabled = false;
            }
        }

        private void PasteFromClipboard()
        {
            this.m_clipboardText = Clipboard.GetText();
        }

        private bool ScanText(string input)
        {
            using (IEnumerator enumerator = Regex.Matches(input, m_ScanPattern).GetEnumerator())
            {
                while (true)
                {
                    double num;
                    double num2;
                    double num3;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Match current = (Match) enumerator.Current;
                    string text1 = current.Groups[1].Value;
                    try
                    {
                        num = Math.Round(double.Parse(current.Groups[2].Value, CultureInfo.InvariantCulture), 2);
                        num2 = Math.Round(double.Parse(current.Groups[3].Value, CultureInfo.InvariantCulture), 2);
                        num3 = Math.Round(double.Parse(current.Groups[4].Value, CultureInfo.InvariantCulture), 2);
                    }
                    catch (SystemException)
                    {
                        continue;
                    }
                    this.m_xCoord.Text = num.ToString();
                    this.m_coords.X = num;
                    this.m_yCoord.Text = num2.ToString();
                    this.m_coords.Y = num2;
                    this.m_zCoord.Text = num3.ToString();
                    this.m_coords.Z = num3;
                    return true;
                }
            }
            return false;
        }
    }
}

