namespace Sandbox.Game.AI.Commands
{
    using Sandbox.Definitions;
    using System;

    public interface IMyAiCommand
    {
        void ActivateCommand();
        void InitCommand(MyAiCommandDefinition definition);
    }
}

