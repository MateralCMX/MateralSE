namespace Sandbox.Game
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Utils;

    public class MyDLCs
    {
        private static readonly Dictionary<uint, MyDLC> m_dlcs;
        private static readonly Dictionary<string, MyDLC> m_dlcsByName;

        static MyDLCs()
        {
            Dictionary<uint, MyDLC> dictionary1 = new Dictionary<uint, MyDLC>();
            dictionary1.Add(MyDLC.DeluxeEdition.AppId, MyDLC.DeluxeEdition);
            dictionary1.Add(MyDLC.DecorativeBlocks.AppId, MyDLC.DecorativeBlocks);
            m_dlcs = dictionary1;
            Dictionary<string, MyDLC> dictionary2 = new Dictionary<string, MyDLC>();
            dictionary2.Add(MyDLC.DeluxeEdition.Name, MyDLC.DeluxeEdition);
            dictionary2.Add(MyDLC.DecorativeBlocks.Name, MyDLC.DecorativeBlocks);
            m_dlcsByName = dictionary2;
        }

        public static string GetDLCIcon(uint id)
        {
            MyDLC ydlc;
            return (!TryGetDLC(id, out ydlc) ? null : ydlc.Icon);
        }

        public static string GetRequiredDLCTooltip(string name)
        {
            MyDLC ydlc;
            return (!TryGetDLC(name, out ydlc) ? null : GetRequiredDLCTooltip(ydlc.AppId));
        }

        public static string GetRequiredDLCTooltip(uint id)
        {
            MyDLC ydlc;
            return (!TryGetDLC(id, out ydlc) ? string.Format(MyTexts.GetString(MyCommonTexts.RequiresDlc), id) : string.Format(MyTexts.GetString(MyCommonTexts.RequiresDlc), MyTexts.GetString(ydlc.DisplayName)));
        }

        public static bool TryGetDLC(string name, out MyDLC dlc) => 
            m_dlcsByName.TryGetValue(name, out dlc);

        public static bool TryGetDLC(uint id, out MyDLC dlc) => 
            m_dlcs.TryGetValue(id, out dlc);

        public static DictionaryReader<uint, MyDLC> DLCs =>
            m_dlcs;

        public sealed class MyDLC
        {
            public static readonly MyDLCs.MyDLC DeluxeEdition = new MyDLCs.MyDLC(MyPerGameSettings.DeluxeEditionDlcId, "DeluxeEdition", MySpaceTexts.DisplayName_DLC_DeluxeEdition, MySpaceTexts.Description_DLC_DeluxeEdition, MyPerGameSettings.DeluxeEditionUrl, "", @"Textures\GUI\DLCs\Deluxe\DeluxeEdition.dds");
            public static readonly MyDLCs.MyDLC DecorativeBlocks = new MyDLCs.MyDLC(0x1004be, "DecorativeBlocks", MySpaceTexts.DisplayName_DLC_DecorativeBlocks, MySpaceTexts.Description_DLC_DecorativeBlocks, "https://store.steampowered.com/app/1049790", @"Textures\GUI\DLCs\Decorative\DecorativeBlocks.DDS", @"Textures\GUI\DLCs\Decorative\DecorativeDLC_Badge.DDS");

            private MyDLC(uint appId, string name, MyStringId displayName, MyStringId description, string url, string icon, string badge)
            {
                this.<AppId>k__BackingField = appId;
                this.<Name>k__BackingField = name;
                this.<DisplayName>k__BackingField = displayName;
                this.<Description>k__BackingField = description;
                this.<URL>k__BackingField = url;
                this.<Icon>k__BackingField = icon;
                this.<Badge>k__BackingField = badge;
            }

            public uint AppId =>
                this.<AppId>k__BackingField;

            public string Name =>
                this.<Name>k__BackingField;

            public MyStringId DisplayName =>
                this.<DisplayName>k__BackingField;

            public MyStringId Description =>
                this.<Description>k__BackingField;

            public string URL =>
                this.<URL>k__BackingField;

            public string Icon =>
                this.<Icon>k__BackingField;

            public string Badge =>
                this.<Badge>k__BackingField;
        }
    }
}

