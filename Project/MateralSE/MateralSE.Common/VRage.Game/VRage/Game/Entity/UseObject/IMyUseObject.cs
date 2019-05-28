namespace VRage.Game.Entity.UseObject
{
    using System;
    using VRage.ModAPI;
    using VRageMath;
    using VRageRender.Import;

    public interface IMyUseObject
    {
        MyActionDescription GetActionInfo(UseActionEnum actionEnum);
        bool HandleInput();
        void OnSelectionLost();
        void SetInstanceID(int id);
        void SetRenderID(uint id);
        void Use(UseActionEnum actionEnum, IMyEntity user);

        IMyEntity Owner { get; }

        MyModelDummy Dummy { get; }

        float InteractiveDistance { get; }

        MatrixD ActivationMatrix { get; }

        MatrixD WorldMatrix { get; }

        uint RenderObjectID { get; }

        int InstanceID { get; }

        bool ShowOverlay { get; }

        UseActionEnum SupportedActions { get; }

        UseActionEnum PrimaryAction { get; }

        UseActionEnum SecondaryAction { get; }

        bool ContinuousUsage { get; }

        bool PlayIndicatorSound { get; }
    }
}

