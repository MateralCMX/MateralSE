namespace VRageRender.Messages
{
    using System;
    using System.Runtime.InteropServices;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGPUEmitterData
    {
        public Vector4 Color0;
        public Vector4 Color1;
        public Vector4 Color2;
        public Vector4 Color3;
        public float ColorKey1;
        public float ColorKey2;
        public float AmbientFactor;
        public float Scale;
        public float Intensity0;
        public float Intensity1;
        public float Intensity2;
        public float Intensity3;
        public float IntensityKey1;
        public float IntensityKey2;
        public float AccelerationKey1;
        public float AccelerationKey2;
        public Vector3 AccelerationVector;
        public float RadiusVar;
        public Vector3 EmitterSize;
        public float EmitterSizeMin;
        public Vector3 Direction;
        public float Velocity;
        public float VelocityVar;
        public float DirectionInnerCone;
        public float DirectionConeVar;
        public float RotationVelocityVar;
        public float Acceleration0;
        public float Acceleration1;
        public float Acceleration2;
        public float Acceleration3;
        public Vector3 Gravity;
        public float Bounciness;
        public float ParticleSize0;
        public float ParticleSize1;
        public float ParticleSize2;
        public float ParticleSize3;
        public float ParticleSizeKeys1;
        public float ParticleSizeKeys2;
        public int NumParticlesToEmitThisFrame;
        public float ParticleLifeSpan;
        public float SoftParticleDistanceScale;
        public float StreakMultiplier;
        public GPUEmitterFlags Flags;
        public uint TextureIndex1;
        public uint TextureIndex2;
        public float AnimationFrameTime;
        public float HueVar;
        public float OITWeightFactor;
        public Matrix3x3 Rotation;
        public Vector3 Position;
        public Vector3 PositionDelta;
        public float MotionInheritance;
        public Vector3 Angle;
        public float ParticleLifeSpanVar;
        public Vector3 AngleVar;
        public float RotationVelocity;
        public float ParticleThickness0;
        public float ParticleThickness1;
        public float ParticleThickness2;
        public float ParticleThickness3;
        public float ParticleThicknessKeys1;
        public float ParticleThicknessKeys2;
        public float EmissivityKeys1;
        public float EmissivityKeys2;
        public float Emissivity0;
        public float Emissivity1;
        public float Emissivity2;
        public float Emissivity3;
        public float RotationVelocityCollisionMultiplier;
        public uint CollisionCountToKill;
        public float DistanceScalingFactor;
        public float ShadowAlphaMultiplier;
        public void InitDefaults()
        {
            Vector4 vector;
            float num;
            this.Color3 = vector = Vector4.One;
            this.Color2 = vector = vector;
            this.Color0 = this.Color1 = vector;
            this.ParticleSize3 = num = 1f;
            this.ParticleSize2 = num = num;
            this.ParticleSize0 = this.ParticleSize1 = num;
            this.ColorKey1 = this.ColorKey2 = 1f;
            this.Direction = Vector3.Forward;
            this.Velocity = 1f;
            this.ParticleLifeSpan = 1f;
            this.SoftParticleDistanceScale = 1f;
            this.RotationVelocity = 0f;
            this.StreakMultiplier = 4f;
            this.AnimationFrameTime = 1f;
            this.OITWeightFactor = 1f;
            this.Bounciness = 0.5f;
            this.DirectionInnerCone = 0f;
            this.DirectionConeVar = 0f;
            this.Rotation = Matrix3x3.Identity;
            this.PositionDelta = Vector3.Zero;
        }
    }
}

