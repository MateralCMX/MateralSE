namespace VRage.Game
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using VRage;
    using VRage.ObjectBuilders;
    using VRage.Serialization;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Character : MyObjectBuilder_EntityBase
    {
        public static Dictionary<string, SerializableVector3> CharacterModels;
        [ProtoMember(0x91)]
        public string CharacterModel;
        [ProtoMember(0x94), DefaultValue((string) null), Serialize(MyObjectFlags.DefaultZero)]
        public MyObjectBuilder_Inventory Inventory;
        [ProtoMember(0x98), XmlElement("HandWeapon", Type=typeof(MyAbstractXmlSerializer<MyObjectBuilder_EntityBase>)), Nullable, DynamicObjectBuilder(false)]
        public MyObjectBuilder_EntityBase HandWeapon;
        [ProtoMember(0x9d)]
        public MyObjectBuilder_Battery Battery;
        [ProtoMember(160)]
        public bool LightEnabled;
        [ProtoMember(0xa3), DefaultValue(true)]
        public bool DampenersEnabled = true;
        [ProtoMember(0xa6), DefaultValue((float) 1f)]
        public float CharacterGeneralDamageModifier = 1f;
        [ProtoMember(0xa9)]
        public long? UsingLadder;
        [ProtoMember(0xac)]
        public SerializableVector2 HeadAngle;
        [ProtoMember(0xaf)]
        public SerializableVector3 LinearVelocity;
        [ProtoMember(0xb2)]
        public float AutoenableJetpackDelay;
        [ProtoMember(0xb5)]
        public bool JetpackEnabled;
        [ProtoMember(0xb8), NoSerialize]
        public float? Health;
        [ProtoMember(0xbd), DefaultValue(false)]
        public bool AIMode;
        [ProtoMember(0xc0)]
        public SerializableVector3 ColorMaskHSV;
        [ProtoMember(0xc3)]
        public float LootingCounter;
        [ProtoMember(0xc6)]
        public string DisplayName;
        [ProtoMember(0xc9)]
        public bool IsInFirstPersonView = true;
        [ProtoMember(0xcc)]
        public bool EnableBroadcasting = true;
        [ProtoMember(0xcf)]
        public float OxygenLevel = 1f;
        [ProtoMember(210)]
        public float EnvironmentOxygenLevel = 1f;
        [ProtoMember(0xd5), Nullable]
        public List<StoredGas> StoredGases = new List<StoredGas>();
        [ProtoMember(0xd9)]
        public MyCharacterMovementEnum MovementState;
        [ProtoMember(0xdd), Nullable]
        public List<string> EnabledComponents = new List<string>();
        [ProtoMember(0xe1)]
        public ulong PlayerSteamId;
        [ProtoMember(0xe3)]
        public int PlayerSerialId;
        [ProtoMember(230)]
        public bool NeedsOxygenFromSuit;
        [ProtoMember(0xe9, IsRequired=false)]
        public long? OwningPlayerIdentityId;
        [ProtoMember(0xec, IsRequired=false)]
        public bool IsPersistenceCharacter;
        [ProtoMember(0xef)]
        public long RelativeDampeningEntity;

        static MyObjectBuilder_Character()
        {
            Dictionary<string, SerializableVector3> dictionary1 = new Dictionary<string, SerializableVector3>();
            dictionary1.Add("Soldier", new SerializableVector3(0f, 0f, 0.05f));
            dictionary1.Add("Astronaut", new SerializableVector3(0f, -1f, 0f));
            dictionary1.Add("Astronaut_Black", new SerializableVector3(0f, -0.96f, -0.5f));
            dictionary1.Add("Astronaut_Blue", new SerializableVector3(0.575f, 0.15f, 0.2f));
            dictionary1.Add("Astronaut_Green", new SerializableVector3(0.333f, -0.33f, -0.05f));
            dictionary1.Add("Astronaut_Red", new SerializableVector3(0f, 0f, 0.05f));
            dictionary1.Add("Astronaut_White", new SerializableVector3(0f, -0.8f, 0.6f));
            dictionary1.Add("Astronaut_Yellow", new SerializableVector3(0.122f, 0.05f, 0.46f));
            dictionary1.Add("Engineer_suit_no_helmet", new SerializableVector3(-100f, -100f, -100f));
            CharacterModels = dictionary1;
        }

        public bool ShouldSerializeHealth() => 
            false;

        public bool ShouldSerializeMovementState() => 
            (this.MovementState != MyCharacterMovementEnum.Standing);

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct StoredGas
        {
            [ProtoMember(0x7e)]
            public SerializableDefinitionId Id;
            [ProtoMember(0x81)]
            public float FillLevel;
        }
    }
}

