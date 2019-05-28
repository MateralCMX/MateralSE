namespace Sandbox.Game.Entities.Blocks
{
    using System;
    using System.Collections.Generic;

    public interface IMyMultiTextPanelComponentOwner : IMyTextPanelComponentOwner
    {
        void SelectPanel(List<MyGuiControlListbox.Item> selectedItems);

        MyMultiTextPanelComponent MultiTextPanel { get; }
    }
}

