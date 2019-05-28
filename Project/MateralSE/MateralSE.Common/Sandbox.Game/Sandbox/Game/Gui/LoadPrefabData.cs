namespace Sandbox.Game.Gui
{
    using ParallelTasks;
    using Sandbox.Game.GUI;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public class LoadPrefabData : WorkData
    {
        private MyObjectBuilder_Definitions m_prefab;
        private string m_path;
        private MyGuiBlueprintScreen_Reworked m_blueprintScreen;
        private ulong? m_id;
        private MyBlueprintItemInfo m_info;

        public LoadPrefabData(MyObjectBuilder_Definitions prefab, MyBlueprintItemInfo info, MyGuiBlueprintScreen_Reworked blueprintScreen)
        {
            this.m_prefab = prefab;
            this.m_blueprintScreen = blueprintScreen;
            this.m_info = info;
        }

        public LoadPrefabData(MyObjectBuilder_Definitions prefab, string path, MyGuiBlueprintScreen_Reworked blueprintScreen, ulong? id = new ulong?())
        {
            this.m_prefab = prefab;
            this.m_path = path;
            this.m_blueprintScreen = blueprintScreen;
            this.m_id = id;
        }

        public void CallLoadPrefab(WorkData workData)
        {
            this.m_prefab = MyBlueprintUtils.LoadPrefab(this.m_path);
            this.CallOnPrefabLoaded();
        }

        public void CallLoadPrefabFromCloud(WorkData workData)
        {
            this.m_prefab = MyBlueprintUtils.LoadPrefabFromCloud(this.m_info);
            this.CallOnPrefabLoaded();
        }

        public void CallLoadWorkshopPrefab(WorkData workData)
        {
            this.m_prefab = MyBlueprintUtils.LoadWorkshopPrefab(this.m_path, this.m_id, false);
            this.CallOnPrefabLoaded();
        }

        public void CallOnPrefabLoaded()
        {
            if ((this.m_blueprintScreen != null) && (this.m_blueprintScreen.State == MyGuiScreenState.OPENED))
            {
                this.m_blueprintScreen.OnPrefabLoaded(this.m_prefab);
            }
        }

        public MyObjectBuilder_Definitions Prefab =>
            this.m_prefab;
    }
}

