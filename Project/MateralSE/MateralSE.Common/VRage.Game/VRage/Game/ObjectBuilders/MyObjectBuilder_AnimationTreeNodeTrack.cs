namespace VRage.Game.ObjectBuilders
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.ObjectBuilders;

    [ProtoContract, MyObjectBuilderDefinition((Type) null, null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_AnimationTreeNodeTrack : MyObjectBuilder_AnimationTreeNode
    {
        [ProtoMember(0x81)]
        public string PathToModel;
        [ProtoMember(0x87)]
        public string AnimationName;
        [ProtoMember(0x8d)]
        public bool Loop = true;
        [ProtoMember(0x93)]
        public double Speed = 1.0;
        [ProtoMember(0x99)]
        public bool Interpolate = true;
        [ProtoMember(0x9f)]
        public string SynchronizeWithLayer;

        protected internal override MyObjectBuilder_AnimationTreeNode DeepCopyWithMask(HashSet<MyObjectBuilder_AnimationTreeNode> selectedNodes, MyObjectBuilder_AnimationTreeNode parentNode, List<MyObjectBuilder_AnimationTreeNode> orphans)
        {
            MyObjectBuilder_AnimationTreeNodeTrack track1 = new MyObjectBuilder_AnimationTreeNodeTrack();
            track1.PathToModel = this.PathToModel;
            track1.AnimationName = this.AnimationName;
            track1.Loop = this.Loop;
            track1.Speed = this.Speed;
            track1.Interpolate = this.Interpolate;
            track1.SynchronizeWithLayer = this.SynchronizeWithLayer;
            track1.EdPos = base.EdPos;
            MyObjectBuilder_AnimationTreeNodeTrack item = track1;
            if (!((selectedNodes == null) || selectedNodes.Contains(this)))
            {
                return null;
            }
            if (parentNode == null)
            {
                orphans.Add(item);
            }
            return item;
        }

        public override MyObjectBuilder_AnimationTreeNode[] GetChildren() => 
            null;
    }
}

