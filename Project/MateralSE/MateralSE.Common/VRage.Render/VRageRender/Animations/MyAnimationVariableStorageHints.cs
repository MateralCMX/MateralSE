namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public static class MyAnimationVariableStorageHints
    {
        public static readonly Dictionary<MyStringId, MyVariableNameHint> VariableNameHints = new Dictionary<MyStringId, MyVariableNameHint>(0x20);
        public static readonly MyStringId StrIdAnimationFinished = MyStringId.GetOrCompute("@animationfinished");
        public static readonly MyStringId StrIdRandom = MyStringId.GetOrCompute("@random");
        public static readonly MyStringId StrIdRandomStable = MyStringId.GetOrCompute("@randomstable");
        public static readonly MyStringId StrIdCrouch = MyStringId.GetOrCompute("crouch");
        public static readonly MyStringId StrIdDead = MyStringId.GetOrCompute("dead");
        public static readonly MyStringId StrIdFalling = MyStringId.GetOrCompute("falling");
        public static readonly MyStringId StrIdFirstPerson = MyStringId.GetOrCompute("firstperson");
        public static readonly MyStringId StrIdForcedFirstPerson = MyStringId.GetOrCompute("forcedFirstperson");
        public static readonly MyStringId StrIdFlying = MyStringId.GetOrCompute("flying");
        public static readonly MyStringId StrIdHoldingTool = MyStringId.GetOrCompute("holdingtool");
        public static readonly MyStringId StrIdIronsight = MyStringId.GetOrCompute("ironsight");
        public static readonly MyStringId StrIdJumping = MyStringId.GetOrCompute("jumping");
        public static readonly MyStringId StrIdLean = MyStringId.GetOrCompute("lean");
        public static readonly MyStringId StrIdShooting = MyStringId.GetOrCompute("shooting");
        public static readonly MyStringId StrIdSitting = MyStringId.GetOrCompute("sitting");
        public static readonly MyStringId StrIdSpeed = MyStringId.GetOrCompute("speed");
        public static readonly MyStringId StrIdSpeedAngle = MyStringId.GetOrCompute("speed_angle");
        public static readonly MyStringId StrIdSpeedX = MyStringId.GetOrCompute("speed_x");
        public static readonly MyStringId StrIdSpeedY = MyStringId.GetOrCompute("speed_y");
        public static readonly MyStringId StrIdSpeedZ = MyStringId.GetOrCompute("speed_z");
        public static readonly MyStringId StrIdTurningSpeed = MyStringId.GetOrCompute("turningspeed");
        public static readonly MyStringId StrIdLadder = MyStringId.GetOrCompute("ladder");
        public static readonly MyStringId StrIdHelmetOpen = MyStringId.GetOrCompute("helmetopen");
        public static readonly MyStringId StrIdActionJump = MyStringId.GetOrCompute("jump");
        public static readonly MyStringId StrIdActionAttack = MyStringId.GetOrCompute("attack");
        public static readonly MyStringId StrIdActionHurt = MyStringId.GetOrCompute("hurt");
        public static readonly MyStringId StrIdActionShout = MyStringId.GetOrCompute("shout");

        static MyAnimationVariableStorageHints()
        {
            VariableNameHints.Add(StrIdAnimationFinished, new MyVariableNameHint("@animationfinished", "Percentage of animation played [0 - 1]"));
            VariableNameHints.Add(StrIdRandom, new MyVariableNameHint("@random", "Random number, unique number generated each access [0 - 1]"));
            VariableNameHints.Add(StrIdRandomStable, new MyVariableNameHint("@randomstable", "Random number, unique number generated each frame [0 - 1]"));
            VariableNameHints.Add(StrIdCrouch, new MyVariableNameHint("crouch", "Character is crouched [0 or 1]"));
            VariableNameHints.Add(StrIdDead, new MyVariableNameHint("dead", "Character is dead [0 or 1]"));
            VariableNameHints.Add(StrIdFalling, new MyVariableNameHint("falling", "Character is falling [0 or 1]"));
            VariableNameHints.Add(StrIdFirstPerson, new MyVariableNameHint("firstperson", "Character camera is in first person mode [0 or 1]"));
            VariableNameHints.Add(StrIdForcedFirstPerson, new MyVariableNameHint("forcedfirstperson", "Character camera is in forced first person mode [0 or 1]"));
            VariableNameHints.Add(StrIdFlying, new MyVariableNameHint("flying", "Character is flying [0 or 1]"));
            VariableNameHints.Add(StrIdHoldingTool, new MyVariableNameHint("holdingtool", "Character is holding a tool [0 or 1]"));
            VariableNameHints.Add(StrIdIronsight, new MyVariableNameHint("ironsight", "Character is using ironsight to aim [0 or 1]"));
            VariableNameHints.Add(StrIdJumping, new MyVariableNameHint("jumping", "Character is jumping [0 or 1]"));
            VariableNameHints.Add(StrIdLean, new MyVariableNameHint("lean", "Character leaning angle [-90 - 90]"));
            VariableNameHints.Add(StrIdShooting, new MyVariableNameHint("shooting", "Character is shooting [0 or 1]"));
            VariableNameHints.Add(StrIdSitting, new MyVariableNameHint("sitting", "Character is sitting [0 or 1]"));
            VariableNameHints.Add(StrIdSpeed, new MyVariableNameHint("speed", "Character speed [0 or more]"));
            VariableNameHints.Add(StrIdSpeedAngle, new MyVariableNameHint("speed_angle", "Character movement angle [0 - 360, clockwise]"));
            VariableNameHints.Add(StrIdSpeedX, new MyVariableNameHint("speed_x", "Character x speed (left-right) [m/s]"));
            VariableNameHints.Add(StrIdSpeedY, new MyVariableNameHint("speed_y", "Character y speed (down-up) [m/s]"));
            VariableNameHints.Add(StrIdSpeedZ, new MyVariableNameHint("speed_z", "Character z speed (backward-forward) [m/s]"));
            VariableNameHints.Add(StrIdTurningSpeed, new MyVariableNameHint("turningspeed", "Character turning speed [clockwise, degrees]"));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyVariableNameHint
        {
            public string Name;
            public string Hint;
            public MyVariableNameHint(string name, string hint)
            {
                this.Name = name;
                this.Hint = hint;
            }
        }
    }
}

