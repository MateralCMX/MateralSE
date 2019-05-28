namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders;
    using VRageMath;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0x22b, typeof(MyObjectBuilder_SectorWeatherComponent), (Type) null)]
    public class MySectorWeatherComponent : MySessionComponentBase
    {
        public const float ExtremeFreeze = 0f;
        public const float Freeze = 0.25f;
        public const float Cozy = 0.5f;
        public const float Hot = 0.75f;
        public const float ExtremeHot = 1f;
        private float m_speed;
        private Vector3 m_sunRotationAxis;
        private Vector3 m_baseSunDirection;
        private bool m_enabled;

        public override void BeforeStart()
        {
            if (this.Enabled)
            {
                if (((Math.Abs(this.m_baseSunDirection.X) + Math.Abs(this.m_baseSunDirection.Y)) + Math.Abs(this.m_baseSunDirection.Z)) >= 0.001)
                {
                    this.m_sunRotationAxis = MySector.SunProperties.SunRotationAxis;
                }
                else
                {
                    this.m_baseSunDirection = MySector.SunProperties.BaseSunDirectionNormalized;
                    this.m_sunRotationAxis = MySector.SunProperties.SunRotationAxis;
                    if (MySession.Static.ElapsedGameTime.Ticks != 0)
                    {
                        float angle = -6.283186f * ((float) (MySession.Static.ElapsedGameTime.TotalSeconds / ((double) this.m_speed)));
                        Vector3 vector = Vector3.Transform(this.m_baseSunDirection, Matrix.CreateFromAxisAngle(this.m_sunRotationAxis, angle));
                        vector.Normalize();
                        this.m_baseSunDirection = vector;
                    }
                }
                MySector.SunProperties.SunDirectionNormalized = this.CalculateSunDirection();
            }
        }

        private Vector3 CalculateSunDirection()
        {
            float angle = 6.283186f * ((float) (MySession.Static.ElapsedGameTime.TotalSeconds / ((double) this.m_speed)));
            Vector3 vector = Vector3.Transform(this.m_baseSunDirection, Matrix.CreateFromAxisAngle(this.m_sunRotationAxis, angle));
            vector.Normalize();
            return vector;
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SectorWeatherComponent objectBuilder = (MyObjectBuilder_SectorWeatherComponent) base.GetObjectBuilder();
            objectBuilder.BaseSunDirection = this.m_baseSunDirection;
            return objectBuilder;
        }

        public static float GetTemperatureInPoint(Vector3D worldPoint)
        {
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(worldPoint);
            if (closestPlanet == null)
            {
                return 0f;
            }
            float oxygenInPoint = MyOxygenProviderSystem.GetOxygenInPoint(worldPoint);
            if (oxygenInPoint < 0.01f)
            {
                return 0f;
            }
            oxygenInPoint = MathHelper.Saturate((float) (oxygenInPoint / 0.6f));
            float num2 = ((float) Vector3D.Distance(closestPlanet.PositionComp.GetPosition(), worldPoint)) / closestPlanet.AverageRadius;
            float num4 = MathHelper.Lerp((float) 0.5f, (float) 0.25f, (float) (1f - ((float) Math.Pow((double) (1f - ((Vector3.Dot(-MySector.SunProperties.SunDirectionNormalized, Vector3.Normalize(worldPoint - closestPlanet.PositionComp.GetPosition())) + 1f) / 2f)), 0.5))));
            float num5 = 0f;
            if (num2 < 1f)
            {
                num5 = MathHelper.Lerp(1f, num4, MathHelper.Saturate((float) (num2 / 0.8f)));
            }
            else
            {
                num5 = MathHelper.Lerp(0f, num4, oxygenInPoint);
            }
            return num5;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_SectorWeatherComponent component = (MyObjectBuilder_SectorWeatherComponent) sessionComponent;
            this.m_speed = 60f * MySession.Static.Settings.SunRotationIntervalMinutes;
            if (!component.BaseSunDirection.IsZero)
            {
                this.m_baseSunDirection = (Vector3) component.BaseSunDirection;
            }
            this.Enabled = MySession.Static.Settings.EnableSunRotation;
        }

        public static bool IsOnDarkSide(Vector3D point)
        {
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(point);
            return ((closestPlanet != null) ? IsThereNight(closestPlanet, ref point) : false);
        }

        public static bool IsThereNight(MyPlanet planet, ref Vector3D position)
        {
            Vector3D vectord = position - planet.PositionComp.GetPosition();
            if (((float) vectord.Length()) > (planet.MaximumRadius * 1.1f))
            {
                return false;
            }
            Vector3 vector = Vector3.Normalize(vectord);
            return (Vector3.Dot(MySector.DirectionToSunNormalized, vector) < -0.1f);
        }

        public static MyTemperatureLevel TemperatureToLevel(float temperature) => 
            ((temperature >= 0.125f) ? ((temperature >= 0.375f) ? ((temperature >= 0.625f) ? ((temperature >= 0.875f) ? MyTemperatureLevel.ExtremeHot : MyTemperatureLevel.Hot) : MyTemperatureLevel.Cozy) : MyTemperatureLevel.Freeze) : MyTemperatureLevel.ExtremeFreeze);

        public override void UpdateBeforeSimulation()
        {
            if (this.Enabled)
            {
                Vector3 vector = this.CalculateSunDirection();
                MySector.SunProperties.SunDirectionNormalized = vector;
            }
        }

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set => 
                (this.m_enabled = value);
        }

        public float RotationInterval
        {
            get => 
                this.m_speed;
            set
            {
                this.m_speed = value;
                this.Enabled = !(this.m_speed == 0f);
            }
        }
    }
}

