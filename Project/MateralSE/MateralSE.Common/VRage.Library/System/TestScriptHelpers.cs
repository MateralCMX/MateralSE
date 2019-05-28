namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    public struct TestScriptHelpers
    {
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern uint MessageBox(IntPtr hWndle, string text, string caption, int buttons);
        public static void DoEvilThings()
        {
        }
    }
}

