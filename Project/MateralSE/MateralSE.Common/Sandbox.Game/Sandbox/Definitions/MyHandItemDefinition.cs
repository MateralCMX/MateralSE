namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_HandItemDefinition), (Type) null)]
    public class MyHandItemDefinition : MyDefinitionBase
    {
        public Matrix LeftHand;
        public Matrix RightHand;
        public Matrix ItemLocation;
        public Matrix ItemLocation3rd;
        public Matrix ItemWalkingLocation;
        public Matrix ItemWalkingLocation3rd;
        public Matrix ItemShootLocation;
        public Matrix ItemShootLocation3rd;
        public Matrix ItemIronsightLocation;
        public float BlendTime;
        public float XAmplitudeOffset;
        public float YAmplitudeOffset;
        public float ZAmplitudeOffset;
        public float XAmplitudeScale;
        public float YAmplitudeScale;
        public float ZAmplitudeScale;
        public float RunMultiplier;
        public float AmplitudeMultiplier3rd = 1f;
        public bool SimulateLeftHand = true;
        public bool SimulateRightHand = true;
        public bool SimulateLeftHandFps = true;
        public bool SimulateRightHandFps = true;
        public string FingersAnimation;
        public float ShootBlend;
        public Vector3 MuzzlePosition;
        public Vector3 ShootScatter;
        public float ScatterSpeed;
        public MyDefinitionId PhysicalItemId;
        public Vector4 LightColor;
        public float LightFalloff;
        public float LightRadius;
        public float LightGlareSize;
        public float LightGlareIntensity;
        public float LightIntensityLower;
        public float LightIntensityUpper;
        public float ShakeAmountTarget;
        public float ShakeAmountNoTarget;
        public MyItemPositioningEnum ItemPositioning;
        public MyItemPositioningEnum ItemPositioning3rd;
        public MyItemPositioningEnum ItemPositioningWalk;
        public MyItemPositioningEnum ItemPositioningWalk3rd;
        public MyItemPositioningEnum ItemPositioningShoot;
        public MyItemPositioningEnum ItemPositioningShoot3rd;
        public MyItemPositioningEnum ItemPositioningIronsight;
        public MyItemPositioningEnum ItemPositioningIronsight3rd;
        public List<ToolSound> ToolSounds;
        public MyStringHash ToolMaterial;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            bool? nullable;
            bool? nullable1;
            bool? nullable2;
            MyObjectBuilder_HandItemDefinition objectBuilder = (MyObjectBuilder_HandItemDefinition) base.GetObjectBuilder();
            objectBuilder.Id = (SerializableDefinitionId) base.Id;
            objectBuilder.LeftHandOrientation = Quaternion.CreateFromRotationMatrix(this.LeftHand);
            objectBuilder.LeftHandPosition = this.LeftHand.Translation;
            objectBuilder.RightHandOrientation = Quaternion.CreateFromRotationMatrix(this.RightHand);
            objectBuilder.RightHandPosition = this.RightHand.Translation;
            objectBuilder.ItemOrientation = Quaternion.CreateFromRotationMatrix(this.ItemLocation);
            objectBuilder.ItemPosition = this.ItemLocation.Translation;
            objectBuilder.ItemWalkingOrientation = Quaternion.CreateFromRotationMatrix(this.ItemWalkingLocation);
            objectBuilder.ItemWalkingPosition = this.ItemWalkingLocation.Translation;
            objectBuilder.BlendTime = this.BlendTime;
            objectBuilder.XAmplitudeOffset = this.XAmplitudeOffset;
            objectBuilder.YAmplitudeOffset = this.YAmplitudeOffset;
            objectBuilder.ZAmplitudeOffset = this.ZAmplitudeOffset;
            objectBuilder.XAmplitudeScale = this.XAmplitudeScale;
            objectBuilder.YAmplitudeScale = this.YAmplitudeScale;
            objectBuilder.ZAmplitudeScale = this.ZAmplitudeScale;
            objectBuilder.RunMultiplier = this.RunMultiplier;
            objectBuilder.ItemWalkingOrientation3rd = Quaternion.CreateFromRotationMatrix(this.ItemWalkingLocation3rd);
            objectBuilder.ItemWalkingPosition3rd = this.ItemWalkingLocation3rd.Translation;
            objectBuilder.ItemOrientation3rd = Quaternion.CreateFromRotationMatrix(this.ItemLocation3rd);
            objectBuilder.ItemPosition3rd = this.ItemLocation3rd.Translation;
            objectBuilder.AmplitudeMultiplier3rd = this.AmplitudeMultiplier3rd;
            objectBuilder.SimulateLeftHand = this.SimulateLeftHand;
            objectBuilder.SimulateRightHand = this.SimulateRightHand;
            MyObjectBuilder_HandItemDefinition definition2 = objectBuilder;
            if (this.SimulateLeftHandFps != this.SimulateLeftHand)
            {
                nullable1 = new bool?(this.SimulateLeftHandFps);
            }
            else
            {
                nullable = null;
                nullable1 = nullable;
            }
            definition2.SimulateLeftHandFps = nullable1;
            if (this.SimulateRightHandFps != this.SimulateRightHand)
            {
                nullable2 = new bool?(this.SimulateRightHandFps);
            }
            else
            {
                nullable = null;
                nullable2 = nullable;
            }
            definition2.SimulateRightHandFps = nullable2;
            MyObjectBuilder_HandItemDefinition local1 = definition2;
            local1.FingersAnimation = this.FingersAnimation;
            local1.ItemShootOrientation = Quaternion.CreateFromRotationMatrix(this.ItemShootLocation);
            local1.ItemShootPosition = this.ItemShootLocation.Translation;
            local1.ItemShootOrientation3rd = Quaternion.CreateFromRotationMatrix(this.ItemShootLocation3rd);
            local1.ItemShootPosition3rd = this.ItemShootLocation3rd.Translation;
            local1.ShootBlend = this.ShootBlend;
            local1.ItemIronsightOrientation = Quaternion.CreateFromRotationMatrix(this.ItemIronsightLocation);
            local1.ItemIronsightPosition = this.ItemIronsightLocation.Translation;
            local1.MuzzlePosition = this.MuzzlePosition;
            local1.ShootScatter = this.ShootScatter;
            local1.ScatterSpeed = this.ScatterSpeed;
            local1.PhysicalItemId = (SerializableDefinitionId) this.PhysicalItemId;
            local1.LightColor = this.LightColor;
            local1.LightFalloff = this.LightFalloff;
            local1.LightRadius = this.LightRadius;
            local1.LightGlareSize = this.LightGlareSize;
            local1.LightGlareIntensity = this.LightGlareIntensity;
            local1.LightIntensityLower = this.LightIntensityLower;
            local1.LightIntensityUpper = this.LightIntensityUpper;
            local1.ShakeAmountTarget = this.ShakeAmountTarget;
            local1.ShakeAmountNoTarget = this.ShakeAmountNoTarget;
            local1.ToolSounds = this.ToolSounds;
            local1.ToolMaterial = this.ToolMaterial.ToString();
            local1.ItemPositioning = this.ItemPositioning;
            local1.ItemPositioning3rd = this.ItemPositioning3rd;
            local1.ItemPositioningWalk = this.ItemPositioningWalk;
            local1.ItemPositioningWalk3rd = this.ItemPositioningWalk3rd;
            local1.ItemPositioningShoot = this.ItemPositioningShoot;
            local1.ItemPositioningShoot3rd = this.ItemPositioningShoot3rd;
            local1.ItemPositioningIronsight = this.ItemPositioningIronsight;
            local1.ItemPositioningIronsight3rd = this.ItemPositioningIronsight3rd;
            return local1;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_HandItemDefinition definition = builder as MyObjectBuilder_HandItemDefinition;
            base.Id = builder.Id;
            this.LeftHand = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.LeftHandOrientation));
            this.LeftHand.Translation = definition.LeftHandPosition;
            this.RightHand = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.RightHandOrientation));
            this.RightHand.Translation = definition.RightHandPosition;
            this.ItemLocation = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemOrientation));
            this.ItemLocation.Translation = definition.ItemPosition;
            this.ItemWalkingLocation = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemWalkingOrientation));
            this.ItemWalkingLocation.Translation = definition.ItemWalkingPosition;
            this.BlendTime = definition.BlendTime;
            this.XAmplitudeOffset = definition.XAmplitudeOffset;
            this.YAmplitudeOffset = definition.YAmplitudeOffset;
            this.ZAmplitudeOffset = definition.ZAmplitudeOffset;
            this.XAmplitudeScale = definition.XAmplitudeScale;
            this.YAmplitudeScale = definition.YAmplitudeScale;
            this.ZAmplitudeScale = definition.ZAmplitudeScale;
            this.RunMultiplier = definition.RunMultiplier;
            this.ItemLocation3rd = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemOrientation3rd));
            this.ItemLocation3rd.Translation = definition.ItemPosition3rd;
            this.ItemWalkingLocation3rd = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemWalkingOrientation3rd));
            this.ItemWalkingLocation3rd.Translation = definition.ItemWalkingPosition3rd;
            this.AmplitudeMultiplier3rd = definition.AmplitudeMultiplier3rd;
            this.SimulateLeftHand = definition.SimulateLeftHand;
            this.SimulateRightHand = definition.SimulateRightHand;
            bool? simulateLeftHandFps = definition.SimulateLeftHandFps;
            this.SimulateLeftHandFps = (simulateLeftHandFps != null) ? simulateLeftHandFps.GetValueOrDefault() : this.SimulateLeftHand;
            simulateLeftHandFps = definition.SimulateRightHandFps;
            this.SimulateRightHandFps = (simulateLeftHandFps != null) ? simulateLeftHandFps.GetValueOrDefault() : this.SimulateRightHand;
            this.FingersAnimation = MyDefinitionManager.Static.GetAnimationDefinitionCompatibility(definition.FingersAnimation);
            this.ItemShootLocation = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemShootOrientation));
            this.ItemShootLocation.Translation = definition.ItemShootPosition;
            this.ItemShootLocation3rd = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemShootOrientation3rd));
            this.ItemShootLocation3rd.Translation = definition.ItemShootPosition3rd;
            this.ShootBlend = definition.ShootBlend;
            this.ItemIronsightLocation = Matrix.CreateFromQuaternion(Quaternion.Normalize(definition.ItemIronsightOrientation));
            this.ItemIronsightLocation.Translation = definition.ItemIronsightPosition;
            this.MuzzlePosition = definition.MuzzlePosition;
            this.ShootScatter = definition.ShootScatter;
            this.ScatterSpeed = definition.ScatterSpeed;
            this.PhysicalItemId = definition.PhysicalItemId;
            this.LightColor = definition.LightColor;
            this.LightFalloff = definition.LightFalloff;
            this.LightRadius = definition.LightRadius;
            this.LightGlareSize = definition.LightGlareSize;
            this.LightGlareIntensity = definition.LightGlareIntensity;
            this.LightIntensityLower = definition.LightIntensityLower;
            this.LightIntensityUpper = definition.LightIntensityUpper;
            this.ShakeAmountTarget = definition.ShakeAmountTarget;
            this.ShakeAmountNoTarget = definition.ShakeAmountNoTarget;
            this.ToolSounds = definition.ToolSounds;
            this.ToolMaterial = MyStringHash.GetOrCompute(definition.ToolMaterial);
            this.ItemPositioning = definition.ItemPositioning;
            this.ItemPositioning3rd = definition.ItemPositioning3rd;
            this.ItemPositioningWalk = definition.ItemPositioningWalk;
            this.ItemPositioningWalk3rd = definition.ItemPositioningWalk3rd;
            this.ItemPositioningShoot = definition.ItemPositioningShoot;
            this.ItemPositioningShoot3rd = definition.ItemPositioningShoot3rd;
            this.ItemPositioningIronsight = definition.ItemPositioningIronsight;
            this.ItemPositioningIronsight3rd = definition.ItemPositioningIronsight3rd;
        }
    }
}

