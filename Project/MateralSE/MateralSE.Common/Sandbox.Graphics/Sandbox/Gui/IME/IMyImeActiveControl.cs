namespace Sandbox.Gui.IME
{
    using System;
    using VRageMath;

    public interface IMyImeActiveControl
    {
        void DeactivateIme();
        Vector2 GetCarriagePosition(int shiftX);
        Vector2 GetCornerPosition();
        int GetMaxLength();
        int GetSelectionLength();
        int GetTextLength();
        void InsertChar(bool conpositionEnd, char character);
        void InsertCharMultiple(bool conpositionEnd, string chars);
        void KeypressBackspace(bool conpositionEnd);
        void KeypressBackspaceMultiple(bool conpositionEnd, int count);
        void KeypressDelete(bool conpositionEnd);
        void KeypressEnter(bool conpositionEnd);
        void KeypressRedo();
        void KeypressUndo();

        bool IsImeActive { get; set; }
    }
}

