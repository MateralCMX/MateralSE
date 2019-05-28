namespace Sandbox.Game.AI
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI.Actions;
    using Sandbox.Game.AI.Logic;
    using System;
    using VRage.Game;

    public interface IMyBot
    {
        void Cleanup();
        void DebugDraw();
        MyObjectBuilder_Bot GetObjectBuilder();
        void Init(MyObjectBuilder_Bot botBuilder);
        void InitActions(Sandbox.Game.AI.ActionCollection actionCollection);
        void InitLogic(MyBotLogic logic);
        void Reset();
        void ReturnToLastMemory();
        void Update();

        bool IsValidForUpdate { get; }

        bool CreatedByPlayer { get; }

        string BehaviorSubtypeName { get; }

        Sandbox.Game.AI.ActionCollection ActionCollection { get; }

        MyBotMemory BotMemory { get; }

        MyBotMemory LastBotMemory { get; set; }

        MyBotDefinition BotDefinition { get; }

        MyBotActionsBase BotActions { get; set; }

        MyBotLogic BotLogic { get; }
    }
}

