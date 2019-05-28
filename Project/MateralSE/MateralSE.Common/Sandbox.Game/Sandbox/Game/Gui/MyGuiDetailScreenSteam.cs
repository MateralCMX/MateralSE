namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Network;
    using VRageMath;

    internal class MyGuiDetailScreenSteam : MyGuiDetailScreenBase
    {
        private ulong? m_publishedItemId;
        private MyGuiControlCombobox m_sendToCombo;

        public MyGuiDetailScreenSteam(Action<MyGuiControlListbox.Item> callBack, MyGuiControlListbox.Item selectedItem, MyGuiBlueprintScreen parent, string thumbnailTexture, float textScale) : base(false, parent, thumbnailTexture, selectedItem, textScale)
        {
            base.callBack = callBack;
            this.m_publishedItemId = (selectedItem.UserData as MyBlueprintItemInfo).PublishedItemId;
            string path = Path.Combine(MyBlueprintUtils.BLUEPRINT_FOLDER_WORKSHOP, this.m_publishedItemId.ToString() + MyBlueprintUtils.BLUEPRINT_WORKSHOP_EXTENSION);
            if (!File.Exists(path))
            {
                base.m_killScreen = true;
            }
            else
            {
                base.m_loadedPrefab = MyBlueprintUtils.LoadWorkshopPrefab(path, this.m_publishedItemId, true);
                if (base.m_loadedPrefab == null)
                {
                    base.m_killScreen = true;
                }
                else
                {
                    string displayName = base.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName;
                    if (displayName.Length > 40)
                    {
                        string str3 = displayName.Substring(0, 40);
                        base.m_loadedPrefab.ShipBlueprints[0].CubeGrids[0].DisplayName = str3;
                    }
                    this.RecreateControls(true);
                }
            }
        }

        protected override void CreateButtons()
        {
            Vector2 vector = new Vector2(0.215f, -0.197f) + base.m_offset;
            Vector2 vector2 = new Vector2(0.13f, 0.045f);
            float usableWidth = 0.26f;
            float textScale = base.m_textScale;
            MyBlueprintUtils.CreateButton(this, usableWidth, MyTexts.Get(MySpaceTexts.DetailScreen_Button_OpenInWorkshop), new Action<MyGuiControlButton>(this.OnOpenInWorkshop), true, new MyStringId?(MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop), textScale).Position = vector;
            usableWidth = 0.14f;
            MyGuiControlLabel control = base.MakeLabel(MyTexts.GetString(MySpaceTexts.DetailScreen_Button_SendToPlayer), vector + (new Vector2(-1f, 1.1f) * vector2), base.m_textScale);
            this.Controls.Add(control);
            VRageMath.Vector4? textColor = null;
            this.m_sendToCombo = base.AddCombo(null, textColor, new Vector2(0.14f, 0.1f), 10);
            this.m_sendToCombo.Position = vector + (new Vector2(-0.082f, 1f) * vector2);
            this.m_sendToCombo.SetToolTip(MyCommonTexts.Blueprints_PlayersTooltip);
            foreach (MyNetworkClient client in Sync.Clients.GetClients())
            {
                if (client.SteamUserId != Sync.MyId)
                {
                    int? sortOrder = null;
                    this.m_sendToCombo.AddItem(Convert.ToInt64(client.SteamUserId), new StringBuilder(client.DisplayName), sortOrder, null);
                }
            }
            this.m_sendToCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnSendToPlayer);
        }

        public override string GetFriendlyName() => 
            "MyDetailScreen";

        private void OnOpenInWorkshop(MyGuiControlButton button)
        {
            if (this.m_publishedItemId != null)
            {
                MyGuiSandbox.OpenUrlWithFallback($"http://steamcommunity.com/sharedfiles/filedetails/?id={this.m_publishedItemId}", "Steam Workshop", false);
            }
            else
            {
                StringBuilder messageCaption = new StringBuilder("Invalid workshop id");
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder(""), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        private void OnSendToPlayer()
        {
            ulong selectedKey = (ulong) this.m_sendToCombo.GetSelectedKey();
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong, string, ulong, string>(x => new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen.ShareBlueprintRequest), this.m_publishedItemId.Value, base.m_blueprintName, selectedKey, MySession.Static.LocalHumanPlayer.DisplayName, targetEndpoint, position);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiDetailScreenSteam.<>c <>9 = new MyGuiDetailScreenSteam.<>c();
            public static Func<IMyEventOwner, Action<ulong, string, ulong, string>> <>9__5_0;

            internal Action<ulong, string, ulong, string> <OnSendToPlayer>b__5_0(IMyEventOwner x) => 
                new Action<ulong, string, ulong, string>(MyGuiBlueprintScreen.ShareBlueprintRequest);
        }
    }
}

