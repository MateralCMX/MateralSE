namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Definitions.GUI;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;

    public class MyGuiSkinManager
    {
        private static MyGuiSkinManager m_instance;
        private MyGuiSkinDefinition m_currentSkin;
        private Dictionary<int, MyGuiSkinDefinition> m_availableSkins;

        public void Init()
        {
            this.m_availableSkins = new Dictionary<int, MyGuiSkinDefinition>();
            IEnumerable<MyGuiSkinDefinition> definitions = MyDefinitionManager.Static.GetDefinitions<MyGuiSkinDefinition>();
            if (definitions != null)
            {
                foreach (MyGuiSkinDefinition definition in definitions)
                {
                    MyStringId orCompute = MyStringId.GetOrCompute(definition.Id.SubtypeName);
                    this.m_availableSkins[orCompute.Id] = definition;
                }
                this.m_availableSkins.TryGetValue(MyStringId.GetOrCompute(MySandboxGame.Config.Skin).Id, out this.m_currentSkin);
            }
        }

        public void SelectSkin(int skinId)
        {
            if (this.m_availableSkins.TryGetValue(skinId, out this.m_currentSkin))
            {
                MySandboxGame.Config.Skin = this.m_currentSkin.Id.SubtypeName;
            }
        }

        public static MyGuiSkinManager Static
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyGuiSkinManager();
                }
                return m_instance;
            }
        }

        public MyGuiSkinDefinition CurrentSkin =>
            this.m_currentSkin;

        public Dictionary<int, MyGuiSkinDefinition> AvailableSkins =>
            this.m_availableSkins;

        public int CurrentSkinId
        {
            get
            {
                if (this.CurrentSkin == null)
                {
                    return 0;
                }
                return MyStringId.GetOrCompute(this.CurrentSkin.Id.SubtypeName).Id;
            }
        }

        public int SkinCount =>
            this.m_availableSkins.Count;
    }
}

