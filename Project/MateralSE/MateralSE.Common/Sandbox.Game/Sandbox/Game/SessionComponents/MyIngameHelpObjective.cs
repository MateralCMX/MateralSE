namespace Sandbox.Game.SessionComponents
{
    using System;
    using VRage.Input;
    using VRage.Utils;

    internal abstract class MyIngameHelpObjective
    {
        public string Id;
        public string[] RequiredIds;
        public string FollowingId;
        public Func<bool> RequiredCondition;
        public MyStringId TitleEnum;
        public MyIngameHelpDetail[] Details;
        public float DelayToHide;
        public float DelayToAppear;

        protected MyIngameHelpObjective()
        {
        }

        public static object GetHighlightedControl(string text) => 
            ("[" + text + "]");

        public static object GetHighlightedControl(MyStringId controlId)
        {
            string controlButtonName = MyInput.Static.GetGameControl(controlId)?.GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
            string str2 = MyInput.Static.GetGameControl(controlId)?.GetControlButtonName(MyGuiInputDeviceEnum.Mouse);
            if (string.IsNullOrEmpty(controlButtonName))
            {
                return ("[" + str2 + "]");
            }
            if (string.IsNullOrEmpty(str2))
            {
                return ("[" + controlButtonName + "]");
            }
            string[] textArray1 = new string[] { "[", controlButtonName, "'/'", str2, "]" };
            return string.Concat(textArray1);
        }

        public virtual bool IsCritical() => 
            false;

        public virtual void OnActivated()
        {
        }
    }
}

