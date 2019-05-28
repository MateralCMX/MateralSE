namespace Sandbox.Game.Lights
{
    using System;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Lights;
    using VRageRender.Messages;

    public class MyLight
    {
        private MyLightType m_lightType;
        private uint m_renderObjectID = uint.MaxValue;
        private bool m_propertiesDirty;
        private bool m_positionDirty;
        private bool m_parentDirty;
        private Vector3D m_position;
        private uint m_parentID = uint.MaxValue;
        private float m_glareMaxDistance;
        private MySubGlare[] m_subGlares;
        private float m_glareIntensity;
        private Vector2 m_glareSize;
        private float m_glareQuerySize;
        private float m_glareQueryFreqMinMs;
        private float m_glareQueryFreqRndMs;
        private float m_glareQueryShift;
        private MyGlareTypeEnum m_glareType;
        private bool m_glareOn;
        private VRageMath.Color m_color = VRageMath.Color.White;
        private float m_falloff;
        private float m_glossFactor;
        private float m_diffuseFactor;
        private float m_range;
        private float m_intensity;
        private bool m_lightOn;
        private float m_reflectorIntensity;
        private bool m_reflectorOn;
        private Vector3 m_reflectorDirection;
        private Vector3 m_reflectorUp;
        private float m_reflectorConeMaxAngleCos;
        private VRageMath.Color m_reflectorColor;
        private float m_reflectorRange;
        private float m_reflectorFalloff;
        private float m_reflectorGlossFactor;
        private float m_reflectorDiffuseFactor;
        private string m_reflectorTexture;
        private bool m_castShadows;
        private float m_pointLightOffset;
        private MatrixD m_matrix;
        private Vector3 m_colorLinear = Vector3.One;
        private Vector3 m_reflectorColorLinear = Vector3.One;
        private bool m_aabbDirty;
        private string m_debugName;

        public void Clear()
        {
            MyRenderProxy.RemoveRenderObject(this.RenderObjectID, MyRenderProxy.ObjectType.Light, false);
            this.m_renderObjectID = uint.MaxValue;
        }

        private static float ConeDegreesToConeMaxAngleCos(float value) => 
            ConeRadiansToConeMaxAngleCos(MathHelper.ToRadians(value));

        private static float ConeMaxAngleCosToDegrees(float reflectorConeMaxAngleCos) => 
            MathHelper.ToDegrees(ConeMaxAngleCosToRadians(reflectorConeMaxAngleCos));

        private static float ConeMaxAngleCosToRadians(float reflectorConeMaxAngleCos) => 
            (((float) Math.Acos((double) (1f - reflectorConeMaxAngleCos))) * 2f);

        private static float ConeRadiansToConeMaxAngleCos(float value) => 
            (1f - ((float) Math.Cos((double) (value / 2f))));

        public void MarkPositionDirty()
        {
            this.m_positionDirty = true;
        }

        public bool SpotlightNotTooLarge(float reflectorConeMaxAngleCos, float reflectorRange) => 
            ((reflectorConeMaxAngleCos <= MyLightsConstants.MAX_SPOTLIGHT_ANGLE_COS) && (reflectorRange <= 1200f));

        public void Start(string debugName)
        {
            this.m_debugName = debugName;
            this.m_positionDirty = true;
            this.m_propertiesDirty = true;
            this.m_aabbDirty = true;
            this.ReflectorOn = false;
            this.ReflectorRange = 1f;
            this.ReflectorUp = Vector3.Up;
            this.ReflectorDirection = Vector3.Forward;
            this.ReflectorGlossFactor = 1f;
            this.ReflectorDiffuseFactor = 3.14f;
            this.ReflectorFalloff = 2f;
            this.LightOn = true;
            this.Intensity = 1f;
            this.GlareOn = false;
            this.GlareQueryFreqMinMs = 150f;
            this.GlareQueryFreqRndMs = 100f;
            this.GlareMaxDistance = 100f;
            this.GlareSize = new Vector2(1f, 1f);
            this.GlareIntensity = 1f;
            this.PointLightOffset = 0f;
            this.CastShadows = true;
            this.Range = 0.5f;
            this.GlossFactor = 1f;
            this.DiffuseFactor = 3.14f;
            this.Falloff = 1f;
            this.ParentID = uint.MaxValue;
            this.GlareQueryShift = 0f;
            this.GlareQuerySize = 0f;
            this.GlareType = MyGlareTypeEnum.Normal;
            this.PointLightOffset = 0f;
            this.ReflectorIntensity = 0f;
            this.ReflectorTexture = null;
            this.m_renderObjectID = MyRenderProxy.CreateRenderLight(debugName);
        }

        public void Start(Vector4 color, float range, string debugName)
        {
            this.Start(debugName);
            this.Color = color;
            this.Range = range;
        }

        public void Start(Vector3D position, Vector4 color, float range, string debugName)
        {
            this.Start(color, range, debugName);
            this.Position = position;
        }

        public unsafe void UpdateLight()
        {
            if (this.m_positionDirty)
            {
                this.m_matrix = this.ReflectorOn ? MatrixD.CreateWorld(this.Position, this.ReflectorDirection, this.ReflectorUp) : MatrixD.CreateTranslation(this.Position);
            }
            if (this.m_parentDirty)
            {
                this.m_parentDirty = false;
                MyRenderProxy.SetParentCullObject(this.m_renderObjectID, this.ParentID, new Matrix?((Matrix) this.m_matrix));
            }
            if (this.m_propertiesDirty || this.m_positionDirty)
            {
                bool flag;
                MyFlareDesc* descPtr1;
                int num1;
                MyLightLayout layout3 = new MyLightLayout {
                    Range = this.Range,
                    Color = this.m_colorLinear * this.Intensity,
                    Falloff = this.m_falloff,
                    GlossFactor = this.GlossFactor,
                    DiffuseFactor = this.DiffuseFactor
                };
                MyLightLayout layout = layout3;
                MySpotLightLayout layout4 = new MySpotLightLayout();
                layout3 = new MyLightLayout {
                    Range = this.ReflectorRange,
                    Color = this.m_reflectorColorLinear * this.ReflectorIntensity,
                    Falloff = this.m_reflectorFalloff,
                    GlossFactor = this.ReflectorGlossFactor,
                    DiffuseFactor = this.ReflectorDiffuseFactor
                };
                layout4.Light = layout3;
                layout4.Up = this.ReflectorUp;
                layout4.Direction = this.ReflectorDirection;
                layout4.ApertureCos = (float) Math.Min(Math.Max((double) (1f - this.ReflectorConeMaxAngleCos), 0.01), 0.99000000953674316);
                MySpotLightLayout layout2 = layout4;
                MyFlareDesc desc2 = new MyFlareDesc();
                if ((!this.GlareOn || ((this.GlareIntensity <= 0.01f) || ((this.GlareSize.X <= 0.01f) || (this.GlareSize.Y <= 0.01f)))) || (this.SubGlares == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (this.SubGlares.Length != 0);
                }
                descPtr1->Enabled = (bool) num1;
                descPtr1 = (MyFlareDesc*) ref desc2;
                desc2.Type = this.GlareType;
                desc2.MaxDistance = this.GlareMaxDistance;
                desc2.QuerySize = this.GlareQuerySize;
                desc2.QueryShift = this.GlareQueryShift;
                desc2.QueryFreqMinMs = this.GlareQueryFreqMinMs;
                desc2.QueryFreqRndMs = this.GlareQueryFreqRndMs;
                desc2.Intensity = this.GlareIntensity;
                desc2.SizeMultiplier = this.GlareSize;
                desc2.Glares = this.SubGlares;
                UpdateRenderLightData data = new UpdateRenderLightData {
                    ID = this.RenderObjectID,
                    Position = this.Position,
                    CastShadows = this.CastShadows,
                    PointLightOn = this.LightOn,
                    PointLight = layout,
                    PointIntensity = this.Intensity,
                    PointOffset = this.PointLightOffset,
                    SpotLightOn = this.ReflectorOn,
                    SpotLight = layout2,
                    SpotIntensity = this.ReflectorIntensity,
                    ReflectorTexture = this.ReflectorTexture,
                    Glare = desc2,
                    PositionChanged = this.m_positionDirty,
                    AabbChanged = this.m_aabbDirty
                };
                MyRenderProxy.UpdateRenderLight(ref data);
                this.m_aabbDirty = flag = false;
                this.m_propertiesDirty = this.m_positionDirty = flag;
            }
        }

        public void UpdateReflectorRangeAndAngle(float reflectorConeMaxAngleCos, float reflectorRange)
        {
            this.m_reflectorRange = reflectorRange;
            this.m_reflectorConeMaxAngleCos = reflectorConeMaxAngleCos;
        }

        public Vector3D Position
        {
            get => 
                this.m_position;
            set
            {
                if (Vector3D.DistanceSquared(this.m_position, value) > 0.0001)
                {
                    this.m_position = value;
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public uint ParentID
        {
            get => 
                this.m_parentID;
            set
            {
                if (this.m_parentID != value)
                {
                    this.m_parentID = value;
                    this.m_parentDirty = true;
                }
            }
        }

        public float PointLightOffset
        {
            get => 
                this.m_pointLightOffset;
            set
            {
                if (this.m_pointLightOffset != value)
                {
                    this.m_pointLightOffset = value;
                    this.m_propertiesDirty = true;
                    this.m_aabbDirty = true;
                }
            }
        }

        public VRageMath.Color Color
        {
            get => 
                this.m_color;
            set
            {
                if (this.m_color != value)
                {
                    this.m_color = value;
                    this.m_colorLinear = this.m_color.ToVector3().ToLinearRGB();
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float Falloff
        {
            get => 
                this.m_falloff;
            set
            {
                if (this.m_falloff != value)
                {
                    this.m_falloff = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float GlossFactor
        {
            get => 
                this.m_glossFactor;
            set
            {
                if (this.m_glossFactor != value)
                {
                    this.m_glossFactor = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float DiffuseFactor
        {
            get => 
                this.m_diffuseFactor;
            set
            {
                if (this.m_diffuseFactor != value)
                {
                    this.m_diffuseFactor = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float Range
        {
            get => 
                this.m_range;
            set
            {
                if (this.m_range != value)
                {
                    if (value <= 0f)
                    {
                        value = 0.5f;
                    }
                    this.m_range = value;
                    this.m_aabbDirty = true;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float Intensity
        {
            get => 
                this.m_intensity;
            set
            {
                if (this.m_intensity != value)
                {
                    this.m_intensity = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public bool LightOn
        {
            get => 
                this.m_lightOn;
            set
            {
                if (this.m_lightOn != value)
                {
                    this.m_lightOn = value;
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public MyLightType LightType
        {
            get => 
                this.m_lightType;
            set => 
                (this.m_lightType = value);
        }

        public float ReflectorIntensity
        {
            get => 
                this.m_reflectorIntensity;
            set
            {
                if (this.m_reflectorIntensity != value)
                {
                    this.m_reflectorIntensity = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public bool ReflectorOn
        {
            get => 
                this.m_reflectorOn;
            set
            {
                if (this.m_reflectorOn != value)
                {
                    this.m_reflectorOn = value;
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public Vector3 ReflectorDirection
        {
            get => 
                this.m_reflectorDirection;
            set
            {
                if (Vector3.DistanceSquared(this.m_reflectorDirection, value) > 1E-05f)
                {
                    this.m_reflectorDirection = value;
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public Vector3 ReflectorUp
        {
            get => 
                this.m_reflectorUp;
            set
            {
                if (Vector3.DistanceSquared(this.m_reflectorUp, value) > 1E-05f)
                {
                    this.m_reflectorUp = MyUtils.Normalize(value);
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public float ReflectorConeMaxAngleCos
        {
            get => 
                this.m_reflectorConeMaxAngleCos;
            set
            {
                if (this.m_reflectorConeMaxAngleCos != value)
                {
                    this.m_reflectorConeMaxAngleCos = value;
                    this.m_propertiesDirty = true;
                    this.m_aabbDirty = true;
                }
            }
        }

        public VRageMath.Color ReflectorColor
        {
            get => 
                this.m_reflectorColor;
            set
            {
                if (this.m_reflectorColor != value)
                {
                    this.m_reflectorColor = value;
                    this.m_reflectorColorLinear = this.m_reflectorColor.ToVector3().ToLinearRGB();
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float ReflectorRange
        {
            get => 
                this.m_reflectorRange;
            set
            {
                if (this.m_reflectorRange != value)
                {
                    this.m_reflectorRange = value;
                    this.m_propertiesDirty = true;
                    this.m_aabbDirty = true;
                }
            }
        }

        public float ReflectorFalloff
        {
            get => 
                this.m_reflectorFalloff;
            set
            {
                if (this.m_reflectorFalloff != value)
                {
                    this.m_reflectorFalloff = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float ReflectorGlossFactor
        {
            get => 
                this.m_reflectorGlossFactor;
            set
            {
                if (this.m_reflectorGlossFactor != value)
                {
                    this.m_reflectorGlossFactor = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float ReflectorDiffuseFactor
        {
            get => 
                this.m_reflectorDiffuseFactor;
            set
            {
                if (this.m_reflectorDiffuseFactor != value)
                {
                    this.m_reflectorDiffuseFactor = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public string ReflectorTexture
        {
            get => 
                this.m_reflectorTexture;
            set
            {
                if (this.m_reflectorTexture != value)
                {
                    this.m_reflectorTexture = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public bool CastShadows
        {
            get => 
                this.m_castShadows;
            set
            {
                if (this.m_castShadows != value)
                {
                    this.m_castShadows = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public bool GlareOn
        {
            get => 
                this.m_glareOn;
            set
            {
                if (this.m_glareOn != value)
                {
                    this.m_glareOn = value;
                    this.m_propertiesDirty = true;
                    this.m_positionDirty = true;
                }
            }
        }

        public MyGlareTypeEnum GlareType
        {
            get => 
                this.m_glareType;
            set
            {
                if (this.m_glareType != value)
                {
                    this.m_glareType = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float GlareQuerySize
        {
            get => 
                this.m_glareQuerySize;
            set
            {
                if (this.m_glareQuerySize != value)
                {
                    this.m_glareQuerySize = value;
                    this.m_propertiesDirty = true;
                    this.m_aabbDirty = true;
                }
            }
        }

        public float GlareQueryShift
        {
            get => 
                this.m_glareQueryShift;
            set
            {
                if (this.m_glareQueryShift != value)
                {
                    this.m_glareQueryShift = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float GlareQueryFreqMinMs
        {
            get => 
                this.m_glareQueryFreqMinMs;
            set
            {
                if (this.m_glareQueryFreqMinMs != value)
                {
                    this.m_glareQueryFreqMinMs = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float GlareQueryFreqRndMs
        {
            get => 
                this.m_glareQueryFreqRndMs;
            set
            {
                if (this.m_glareQueryFreqRndMs != value)
                {
                    this.m_glareQueryFreqRndMs = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public MySubGlare[] SubGlares
        {
            get => 
                this.m_subGlares;
            set
            {
                this.m_subGlares = value;
                this.m_propertiesDirty = true;
            }
        }

        public float GlareIntensity
        {
            get => 
                this.m_glareIntensity;
            set
            {
                if (this.m_glareIntensity != value)
                {
                    this.m_glareIntensity = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public Vector2 GlareSize
        {
            get => 
                this.m_glareSize;
            set
            {
                if (this.m_glareSize != value)
                {
                    this.m_glareSize = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float GlareMaxDistance
        {
            get => 
                this.m_glareMaxDistance;
            set
            {
                if (this.m_glareMaxDistance != value)
                {
                    this.m_glareMaxDistance = value;
                    this.m_propertiesDirty = true;
                }
            }
        }

        public float ReflectorConeRadians
        {
            get => 
                ConeMaxAngleCosToRadians(this.ReflectorConeMaxAngleCos);
            set => 
                (this.ReflectorConeMaxAngleCos = ConeRadiansToConeMaxAngleCos(value));
        }

        public float ReflectorConeDegrees
        {
            get => 
                ConeMaxAngleCosToDegrees(this.ReflectorConeMaxAngleCos);
            set => 
                (this.ReflectorConeMaxAngleCos = ConeDegreesToConeMaxAngleCos(value));
        }

        public uint RenderObjectID =>
            this.m_renderObjectID;
    }
}

