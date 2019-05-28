namespace Sandbox.Definitions
{
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_GpsCollectionDefinition), (Type) null)]
    public class MyGpsCollectionDefinition : MyDefinitionBase
    {
        public List<MyGpsCoordinate> Positions;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GpsCollectionDefinition definition = builder as MyObjectBuilder_GpsCollectionDefinition;
            this.Positions = new List<MyGpsCoordinate>();
            if ((definition.Positions != null) && (definition.Positions.Length != 0))
            {
                StringBuilder name = new StringBuilder();
                Vector3D zero = Vector3D.Zero;
                StringBuilder additionalData = new StringBuilder();
                string[] positions = definition.Positions;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (MyGpsCollection.ParseOneGPSExtended(positions[i], name, ref zero, additionalData))
                    {
                        MyGpsCoordinate item = new MyGpsCoordinate {
                            Name = name.ToString(),
                            Coords = zero
                        };
                        string str = additionalData.ToString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            char[] separator = new char[] { ':' };
                            string[] strArray2 = str.Split(separator);
                            for (int j = 0; j < (strArray2.Length / 2); j++)
                            {
                                string str2 = strArray2[2 * j];
                                string str3 = strArray2[(2 * j) + 1];
                                if (!string.IsNullOrWhiteSpace(str2) && !string.IsNullOrWhiteSpace(str3))
                                {
                                    if (item.Actions == null)
                                    {
                                        item.Actions = new List<MyGpsAction>();
                                    }
                                    MyGpsAction action = new MyGpsAction {
                                        BlockName = str2,
                                        ActionId = str3
                                    };
                                    item.Actions.Add(action);
                                }
                            }
                        }
                        this.Positions.Add(item);
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyGpsAction
        {
            public string BlockName;
            public string ActionId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyGpsCoordinate
        {
            public string Name;
            public Vector3D Coords;
            public List<MyGpsCollectionDefinition.MyGpsAction> Actions;
        }
    }
}

