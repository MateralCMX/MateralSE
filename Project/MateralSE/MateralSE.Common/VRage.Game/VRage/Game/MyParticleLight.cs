namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;
    using VRageRender.Messages;

    public class MyParticleLight
    {
        private static readonly int Version;
        private string m_name;
        private MyParticleEffect m_effect;
        private uint m_renderObjectID = uint.MaxValue;
        private Vector3D m_position;
        private Vector4 m_color;
        private float m_range;
        private float m_intensity;
        private float m_falloff;
        private Vector3 m_localPositionVarRnd;
        private float m_colorVarRnd;
        private float m_lastVarianceTime;
        private float m_rangeVarRnd;
        private float m_intensityRnd;
        private IMyConstProperty[] m_properties = new IMyConstProperty[Enum.GetValues(typeof(MyLightPropertiesEnum)).Length];
        private uint m_parentID;

        private T AddProperty<T>(MyLightPropertiesEnum e, T property) where T: IMyConstProperty
        {
            this.m_properties[(int) e] = property;
            return property;
        }

        public void Close()
        {
            for (int i = 0; i < this.m_properties.Length; i++)
            {
                this.m_properties[i] = null;
            }
            this.m_effect = null;
            this.CloseLight();
        }

        private void CloseLight()
        {
            if (this.m_renderObjectID != uint.MaxValue)
            {
                MyRenderProxy.RemoveRenderObject(this.m_renderObjectID, MyRenderProxy.ObjectType.Light, false);
                this.m_renderObjectID = uint.MaxValue;
            }
        }

        public MyParticleLight CreateInstance(MyParticleEffect effect)
        {
            MyParticleLight light;
            MyParticlesManager.LightsPool.AllocateOrCreate(out light);
            light.Start(effect);
            light.Name = this.Name;
            for (int i = 0; i < this.m_properties.Length; i++)
            {
                light.m_properties[i] = this.m_properties[i].Duplicate();
            }
            return light;
        }

        public void DebugDraw()
        {
        }

        public void Deserialize(XmlReader reader)
        {
            this.m_name = reader.GetAttribute("name");
            Convert.ToInt32(reader.GetAttribute("version"), CultureInfo.InvariantCulture);
            reader.ReadStartElement();
            IMyConstProperty[] properties = this.m_properties;
            for (int i = 0; i < properties.Length; i++)
            {
                properties[i].Deserialize(reader);
            }
            reader.ReadEndElement();
        }

        public void DeserializeFromObjectBuilder(ParticleLight light)
        {
            this.m_name = light.Name;
            foreach (GenerationProperty property in light.Properties)
            {
                for (int i = 0; i < this.m_properties.Length; i++)
                {
                    if (this.m_properties[i].Name.Equals(property.Name))
                    {
                        this.m_properties[i].DeserializeFromObjectBuilder(property);
                    }
                }
            }
        }

        public void Done()
        {
            for (int i = 0; i < this.m_properties.Length; i++)
            {
                if (this.m_properties[i] is IMyAnimatedProperty)
                {
                    (this.m_properties[i] as IMyAnimatedProperty).ClearKeys();
                }
            }
            this.Close();
        }

        public MyParticleLight Duplicate(MyParticleEffect effect) => 
            this.CreateInstance(effect);

        public MyParticleEffect GetEffect() => 
            this.m_effect;

        public IEnumerable<IMyConstProperty> GetProperties() => 
            this.m_properties;

        public void Init()
        {
            this.AddProperty<MyAnimatedPropertyVector3>(MyLightPropertiesEnum.Position, new MyAnimatedPropertyVector3("Position"));
            this.AddProperty<MyAnimatedPropertyVector3>(MyLightPropertiesEnum.PositionVar, new MyAnimatedPropertyVector3("Position var"));
            this.AddProperty<MyAnimatedPropertyVector4>(MyLightPropertiesEnum.Color, new MyAnimatedPropertyVector4("Color"));
            this.AddProperty<MyAnimatedPropertyFloat>(MyLightPropertiesEnum.ColorVar, new MyAnimatedPropertyFloat("Color var"));
            this.AddProperty<MyAnimatedPropertyFloat>(MyLightPropertiesEnum.Range, new MyAnimatedPropertyFloat("Range"));
            this.AddProperty<MyAnimatedPropertyFloat>(MyLightPropertiesEnum.RangeVar, new MyAnimatedPropertyFloat("Range var"));
            this.AddProperty<MyAnimatedPropertyFloat>(MyLightPropertiesEnum.Intensity, new MyAnimatedPropertyFloat("Intensity"));
            this.AddProperty<MyAnimatedPropertyFloat>(MyLightPropertiesEnum.IntensityVar, new MyAnimatedPropertyFloat("Intensity var"));
            this.AddProperty<MyConstPropertyFloat>(MyLightPropertiesEnum.GravityDisplacement, new MyConstPropertyFloat("Gravity Displacement"));
            this.AddProperty<MyConstPropertyFloat>(MyLightPropertiesEnum.Falloff, new MyConstPropertyFloat("Falloff"));
            this.AddProperty<MyConstPropertyFloat>(MyLightPropertiesEnum.VarianceTimeout, new MyConstPropertyFloat("Variance Timeout"));
            this.AddProperty<MyConstPropertyBool>(MyLightPropertiesEnum.Enabled, new MyConstPropertyBool("Enabled"));
            this.InitDefault();
        }

        public void InitDefault()
        {
            this.Color.AddKey<Vector4>(0f, Vector4.One);
            this.Range.AddKey<float>(0f, 2.5f);
            this.Intensity.AddKey<float>(0f, 10f);
            this.Falloff.SetValue(1f);
            this.VarianceTimeout.SetValue(0.1f);
            this.Enabled.SetValue(true);
        }

        private void InitLight()
        {
            this.m_renderObjectID = MyRenderProxy.CreateRenderLight("ParticleLight");
        }

        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("ParticleLight");
            writer.WriteAttributeString("Name", this.Name);
            writer.WriteAttributeString("Version", Version.ToString(CultureInfo.InvariantCulture));
            writer.WriteStartElement("Properties");
            foreach (IMyConstProperty property in this.m_properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", property.Name);
                writer.WriteAttributeString("Type", property.BaseValueType);
                PropertyAnimationType @const = PropertyAnimationType.Const;
                if (property.Animated)
                {
                    @const = property.Is2D ? PropertyAnimationType.Animated2D : PropertyAnimationType.Animated;
                }
                writer.WriteAttributeString("AnimationType", @const.ToString());
                property.Serialize(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void Start(MyParticleEffect effect)
        {
            this.m_effect = effect;
            this.m_name = "ParticleLight";
            this.m_parentID = uint.MaxValue;
        }

        public unsafe void Update()
        {
            bool flag = false;
            if (!this.Enabled)
            {
                if (this.m_renderObjectID != uint.MaxValue)
                {
                    this.CloseLight();
                }
            }
            else
            {
                Vector3 vector;
                Vector4 vector2;
                float num2;
                float num3;
                if (this.m_renderObjectID == uint.MaxValue)
                {
                    this.InitLight();
                    flag = true;
                }
                float num = this.m_effect.GetElapsedTime() - this.m_lastVarianceTime;
                bool local1 = (num > this.VarianceTimeout) || (num < 0f);
                if (local1)
                {
                    this.m_lastVarianceTime = this.m_effect.GetElapsedTime();
                }
                this.Position.GetInterpolatedValue<Vector3>(this.m_effect.GetElapsedTime(), out vector);
                bool local2 = local1;
                if (local2)
                {
                    Vector3 vector3;
                    this.PositionVar.GetInterpolatedValue<Vector3>(this.m_effect.GetElapsedTime(), out vector3);
                    this.m_localPositionVarRnd = new Vector3(MyUtils.GetRandomFloat(-vector3.X, vector3.X), MyUtils.GetRandomFloat(-vector3.Y, vector3.Y), MyUtils.GetRandomFloat(-vector3.Z, vector3.Z));
                }
                vector += this.m_localPositionVarRnd;
                this.Color.GetInterpolatedValue<Vector4>(this.m_effect.GetElapsedTime(), out vector2);
                bool local3 = local2;
                if (local3)
                {
                    float num4;
                    this.ColorVar.GetInterpolatedValue<float>(this.m_effect.GetElapsedTime(), out num4);
                    this.m_colorVarRnd = MyUtils.GetRandomFloat(1f - num4, 1f + num4);
                }
                Vector4* vectorPtr1 = (Vector4*) ref vector2;
                vectorPtr1->X = MathHelper.Clamp((float) (vector2.X * this.m_colorVarRnd), (float) 0f, (float) 1f);
                Vector4* vectorPtr2 = (Vector4*) ref vector2;
                vectorPtr2->Y = MathHelper.Clamp((float) (vector2.Y * this.m_colorVarRnd), (float) 0f, (float) 1f);
                Vector4* vectorPtr3 = (Vector4*) ref vector2;
                vectorPtr3->Z = MathHelper.Clamp((float) (vector2.Z * this.m_colorVarRnd), (float) 0f, (float) 1f);
                this.Range.GetInterpolatedValue<float>(this.m_effect.GetElapsedTime(), out num2);
                bool local4 = local3;
                if (local4)
                {
                    float num5;
                    this.RangeVar.GetInterpolatedValue<float>(this.m_effect.GetElapsedTime(), out num5);
                    this.m_rangeVarRnd = MyUtils.GetRandomFloat(-num5, num5);
                }
                num2 += this.m_rangeVarRnd;
                this.Intensity.GetInterpolatedValue<float>(this.m_effect.GetElapsedTime(), out num3);
                if (local4)
                {
                    float num6;
                    this.IntensityVar.GetInterpolatedValue<float>(this.m_effect.GetElapsedTime(), out num6);
                    this.m_intensityRnd = MyUtils.GetRandomFloat(-num6, num6);
                }
                num3 += this.m_intensityRnd;
                if (this.m_effect.IsStopped)
                {
                    num3 = 0f;
                }
                Vector3D vectord = Vector3D.Transform(vector * this.m_effect.GetEmitterScale(), this.m_effect.WorldMatrix);
                if (this.m_effect.Gravity.LengthSquared() > 0.0001f)
                {
                    Vector3 gravity = this.m_effect.Gravity;
                    gravity.Normalize();
                    vectord += gravity * this.GravityDisplacement;
                }
                if (this.m_parentID != this.m_effect.ParentID)
                {
                    this.m_parentID = this.m_effect.ParentID;
                    Matrix? childToParent = null;
                    MyRenderProxy.SetParentCullObject(this.m_renderObjectID, this.m_parentID, childToParent);
                }
                bool flag2 = this.m_position != vectord;
                bool flag3 = !(this.m_range == num2);
                bool flag4 = this.m_falloff != this.Falloff;
                if (((((flag2 || (this.m_color != vector2)) | flag3) || !(this.m_intensity == num3)) | flag) | flag4)
                {
                    this.m_color = vector2;
                    this.m_intensity = num3;
                    this.m_range = num2;
                    this.m_position = vectord;
                    this.m_falloff = (float) this.Falloff;
                    MyLightLayout layout3 = new MyLightLayout {
                        Range = this.m_range * this.m_effect.GetEmitterScale(),
                        Color = new Vector3(this.m_color) * this.m_intensity,
                        Falloff = this.m_falloff,
                        GlossFactor = 1f,
                        DiffuseFactor = 3.14f
                    };
                    MySpotLightLayout layout4 = new MySpotLightLayout {
                        Up = Vector3.Up,
                        Direction = Vector3.Forward
                    };
                    UpdateRenderLightData data = new UpdateRenderLightData {
                        ID = this.m_renderObjectID,
                        Position = this.m_position,
                        PointLightOn = true,
                        PointLight = layout3,
                        PositionChanged = flag2,
                        SpotLight = layout4,
                        AabbChanged = flag3,
                        PointIntensity = this.m_intensity
                    };
                    MyRenderProxy.UpdateRenderLight(ref data);
                }
            }
        }

        public MyAnimatedPropertyVector3 Position
        {
            get => 
                ((MyAnimatedPropertyVector3) this.m_properties[0]);
            private set => 
                (this.m_properties[0] = value);
        }

        public MyAnimatedPropertyVector3 PositionVar
        {
            get => 
                ((MyAnimatedPropertyVector3) this.m_properties[1]);
            private set => 
                (this.m_properties[1] = value);
        }

        public MyAnimatedPropertyVector4 Color
        {
            get => 
                ((MyAnimatedPropertyVector4) this.m_properties[2]);
            private set => 
                (this.m_properties[2] = value);
        }

        public MyAnimatedPropertyFloat ColorVar
        {
            get => 
                ((MyAnimatedPropertyFloat) this.m_properties[3]);
            private set => 
                (this.m_properties[3] = value);
        }

        public MyAnimatedPropertyFloat Range
        {
            get => 
                ((MyAnimatedPropertyFloat) this.m_properties[4]);
            private set => 
                (this.m_properties[4] = value);
        }

        public MyAnimatedPropertyFloat RangeVar
        {
            get => 
                ((MyAnimatedPropertyFloat) this.m_properties[5]);
            private set => 
                (this.m_properties[5] = value);
        }

        public MyAnimatedPropertyFloat Intensity
        {
            get => 
                ((MyAnimatedPropertyFloat) this.m_properties[6]);
            private set => 
                (this.m_properties[6] = value);
        }

        public MyAnimatedPropertyFloat IntensityVar
        {
            get => 
                ((MyAnimatedPropertyFloat) this.m_properties[7]);
            private set => 
                (this.m_properties[7] = value);
        }

        public MyConstPropertyBool Enabled
        {
            get => 
                ((MyConstPropertyBool) this.m_properties[8]);
            private set => 
                (this.m_properties[8] = value);
        }

        public MyConstPropertyFloat GravityDisplacement
        {
            get => 
                ((MyConstPropertyFloat) this.m_properties[9]);
            private set => 
                (this.m_properties[9] = value);
        }

        public MyConstPropertyFloat Falloff
        {
            get => 
                ((MyConstPropertyFloat) this.m_properties[10]);
            private set => 
                (this.m_properties[10] = value);
        }

        public MyConstPropertyFloat VarianceTimeout
        {
            get => 
                ((MyConstPropertyFloat) this.m_properties[11]);
            private set => 
                (this.m_properties[11] = value);
        }

        public string Name
        {
            get => 
                this.m_name;
            set => 
                (this.m_name = value);
        }

        private enum MyLightPropertiesEnum
        {
            Position,
            PositionVar,
            Color,
            ColorVar,
            Range,
            RangeVar,
            Intensity,
            IntensityVar,
            Enabled,
            GravityDisplacement,
            Falloff,
            VarianceTimeout
        }
    }
}

