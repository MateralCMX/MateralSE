namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ControllerSchemaDefinition), (Type) null)]
    public class MyControllerSchemaDefinition : MyDefinitionBase
    {
        public List<int> CompatibleDevices;
        public Dictionary<string, List<ControlGroup>> Schemas;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ControllerSchemaDefinition definition = builder as MyObjectBuilder_ControllerSchemaDefinition;
            if (definition.CompatibleDeviceIds != null)
            {
                this.CompatibleDevices = new List<int>(definition.CompatibleDeviceIds.Count);
                byte[] arr = new byte[4];
                foreach (string str in definition.CompatibleDeviceIds)
                {
                    if (str.Length < 8)
                    {
                        continue;
                    }
                    if (this.TryGetByteArray(str, 8, out arr))
                    {
                        int item = BitConverter.ToInt32(arr, 0);
                        this.CompatibleDevices.Add(item);
                    }
                }
            }
            if (definition.Schemas != null)
            {
                this.Schemas = new Dictionary<string, List<ControlGroup>>(definition.Schemas.Count);
                foreach (MyObjectBuilder_ControllerSchemaDefinition.Schema schema in definition.Schemas)
                {
                    if (schema.ControlGroups != null)
                    {
                        List<ControlGroup> list = new List<ControlGroup>(schema.ControlGroups.Count);
                        this.Schemas[schema.SchemaName] = list;
                        foreach (MyObjectBuilder_ControllerSchemaDefinition.ControlGroup group in schema.ControlGroups)
                        {
                            ControlGroup group2 = new ControlGroup {
                                Type = group.Type,
                                Name = group.Name
                            };
                            if (group.ControlDefs != null)
                            {
                                group2.ControlBinding = new Dictionary<string, MyControllerSchemaEnum>(group.ControlDefs.Count);
                                foreach (MyObjectBuilder_ControllerSchemaDefinition.ControlDef def in group.ControlDefs)
                                {
                                    group2.ControlBinding[def.Type] = def.Control;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool TryGetByteArray(string str, int count, out byte[] arr)
        {
            arr = null;
            if ((count % 2) == 1)
            {
                return false;
            }
            if (str.Length < count)
            {
                return false;
            }
            arr = new byte[count / 2];
            StringBuilder builder = new StringBuilder();
            int num = 0;
            for (int i = 0; num < count; i++)
            {
                builder.Clear().Append(str[num]).Append(str[num + 1]);
                if (!byte.TryParse(builder.ToString(), NumberStyles.HexNumber, (IFormatProvider) null, out arr[i]))
                {
                    return false;
                }
                num += 2;
            }
            return true;
        }

        public class ControlGroup
        {
            public string Type;
            public string Name;
            public Dictionary<string, MyControllerSchemaEnum> ControlBinding;
        }
    }
}

