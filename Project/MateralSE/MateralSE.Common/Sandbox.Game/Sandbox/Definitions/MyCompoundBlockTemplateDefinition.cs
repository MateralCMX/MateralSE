namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_CompoundBlockTemplateDefinition), (Type) null)]
    public class MyCompoundBlockTemplateDefinition : MyDefinitionBase
    {
        public MyCompoundBlockBinding[] Bindings;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_CompoundBlockTemplateDefinition definition = builder as MyObjectBuilder_CompoundBlockTemplateDefinition;
            if (definition.Bindings == null)
            {
                this.Bindings = null;
            }
            else
            {
                this.Bindings = new MyCompoundBlockBinding[definition.Bindings.Length];
                for (int i = 0; i < definition.Bindings.Length; i++)
                {
                    MyCompoundBlockBinding binding = new MyCompoundBlockBinding {
                        BuildType = MyStringId.GetOrCompute(definition.Bindings[i].BuildType?.ToLower()),
                        Multiple = definition.Bindings[i].Multiple
                    };
                    if ((definition.Bindings[i].RotationBinds != null) && (definition.Bindings[i].RotationBinds.Length != 0))
                    {
                        binding.RotationBinds = new MyCompoundBlockRotationBinding[definition.Bindings[i].RotationBinds.Length];
                        for (int j = 0; j < definition.Bindings[i].RotationBinds.Length; j++)
                        {
                            if ((definition.Bindings[i].RotationBinds[j].Rotations != null) && (definition.Bindings[i].RotationBinds[j].Rotations.Length != 0))
                            {
                                binding.RotationBinds[j] = new MyCompoundBlockRotationBinding();
                                binding.RotationBinds[j].BuildTypeReference = MyStringId.GetOrCompute(definition.Bindings[i].RotationBinds[j].BuildTypeReference?.ToLower());
                                binding.RotationBinds[j].Rotations = new MyBlockOrientation[definition.Bindings[i].RotationBinds[j].Rotations.Length];
                                for (int k = 0; k < definition.Bindings[i].RotationBinds[j].Rotations.Length; k++)
                                {
                                    binding.RotationBinds[j].Rotations[k] = (MyBlockOrientation) definition.Bindings[i].RotationBinds[j].Rotations[k];
                                }
                            }
                        }
                    }
                    this.Bindings[i] = binding;
                }
            }
        }

        public class MyCompoundBlockBinding
        {
            public MyStringId BuildType;
            public bool Multiple;
            public MyCompoundBlockTemplateDefinition.MyCompoundBlockRotationBinding[] RotationBinds;
        }

        public class MyCompoundBlockRotationBinding
        {
            public MyStringId BuildTypeReference;
            public MyBlockOrientation[] Rotations;
        }
    }
}

