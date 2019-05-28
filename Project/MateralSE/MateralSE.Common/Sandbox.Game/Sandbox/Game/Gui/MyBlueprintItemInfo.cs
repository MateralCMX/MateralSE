namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.GameServices;

    public class MyBlueprintItemInfo
    {
        public MyBlueprintItemInfo(MyBlueprintTypeEnum type, ulong? id = new ulong?())
        {
            this.Type = type;
            this.PublishedItemId = id;
            this.Data = new AdditionalBlueprintData();
        }

        public override bool Equals(object obj)
        {
            MyBlueprintItemInfo info = obj as MyBlueprintItemInfo;
            return (((info != null) && this.BlueprintName.Equals(info.BlueprintName)) && (this.Type.Equals(info.Type) && (this.Data.Name.Equals(info.Data.Name) && (this.Data.Description.Equals(info.Data.Description) && this.Data.CloudImagePath.Equals(info.Data.CloudImagePath)))));
        }

        public void SetAdditionalBlueprintInformation(string name = null, string description = null, uint[] dlcs = null)
        {
            this.Data.Name = name ?? string.Empty;
            this.Data.Description = description ?? string.Empty;
            this.Data.CloudImagePath = string.Empty;
            this.Data.DLCs = dlcs;
        }

        public MyBlueprintTypeEnum Type { get; set; }

        public ulong? PublishedItemId { get; set; }

        public MyWorkshopItem Item { get; set; }

        public DateTime? TimeCreated { get; set; }

        public DateTime? TimeUpdated { get; set; }

        public string BlueprintName { get; set; }

        public string CloudPathXML { get; set; }

        public string CloudPathPB { get; set; }

        public bool IsDirectory { get; set; }

        public MyCloudFileInfo CloudInfo { get; set; }

        public AdditionalBlueprintData Data { get; set; }
    }
}

