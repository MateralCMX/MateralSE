namespace Sandbox.Game.Entities.Character
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;

    [Obsolete]
    public static class MyInverseKinematics
    {
        public static void CosineLaw(float A, float B, float C, out double alpha, out double beta)
        {
            double d = MathHelper.Clamp((double) (-(((B * B) - (A * A)) - (C * C)) / ((2f * A) * C)), -1.0, 1.0);
            alpha = Math.Acos(d);
            double num2 = MathHelper.Clamp((double) (-(((C * C) - (A * A)) - (B * B)) / ((2f * A) * B)), -1.0, 1.0);
            beta = Math.Acos(num2);
        }

        public static double GetAngle(Vector3 a, Vector3 b) => 
            Math.Acos((double) MathHelper.Clamp(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b)), -1f, 1f));

        public static double GetAngleSigned(Vector3 a, Vector3 b, Vector3 normal)
        {
            double num = Math.Acos((double) MathHelper.Clamp(Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b)), -1f, 1f));
            if (Vector3.Dot(normal, Vector3.Cross(a, b)) < 0f)
            {
                num = -num;
            }
            return num;
        }

        public static CastHit? GetClosestFootSupportPosition(MyEntity characterEntity, MyEntity characterTool, Vector3 from, Vector3 up, Vector3 footDimension, Matrix WorldMatrix, float castDownLimit, float castUpLimit, uint raycastFilterLayer = 0)
        {
            bool flag = false;
            CastHit hit = new CastHit();
            MatrixD matrix = WorldMatrix;
            Vector3 zero = Vector3.Zero;
            matrix.Translation = Vector3.Zero;
            zero = (Vector3) Vector3.Transform(zero, matrix);
            matrix.Translation = (from + (up * castUpLimit)) + zero;
            Vector3 vector1 = new Vector3(0f, footDimension.Y / 2f, 0f);
            Vector3 vector4 = new Vector3(0f, footDimension.Y / 2f, -footDimension.Z);
            Vector3 worldCoord = from + (up * castUpLimit);
            Vector3 pointTo = from - (up * castDownLimit);
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_RAYCASTLINE)
            {
                MyRenderProxy.DebugDrawText3D(worldCoord + zero, "Cast line", Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                MyRenderProxy.DebugDrawLine3D(worldCoord + zero, pointTo + zero, Color.White, Color.White, false, false);
            }
            if (MyFakes.ENABLE_FOOT_IK_USE_HAVOK_RAYCAST)
            {
                MyPhysics.HitInfo info;
                if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_RAYCASTLINE)
                {
                    MyRenderProxy.DebugDrawText3D(worldCoord, "Raycast line", Color.Green, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    MyRenderProxy.DebugDrawLine3D(worldCoord, pointTo, Color.Green, Color.Green, false, false);
                }
                if (MyPhysics.CastRay(worldCoord, pointTo, out info, raycastFilterLayer, true))
                {
                    flag = true;
                    if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_RAYCASTHITS)
                    {
                        MyRenderProxy.DebugDrawSphere(info.Position, 0.02f, Color.Green, 1f, false, false, true, false);
                        MyRenderProxy.DebugDrawText3D(info.Position, "RayCast hit", Color.Green, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                    if (Vector3.Dot((Vector3) info.Position, up) > Vector3.Dot(hit.Position, up))
                    {
                        hit.Position = (Vector3) info.Position;
                        hit.Normal = info.HkHitInfo.Normal;
                    }
                }
            }
            if (flag)
            {
                return new CastHit?(hit);
            }
            return null;
        }

        public static void RotateBone(MyCharacterBone bone, Vector3 planeNormal, double angle)
        {
            Matrix matrix = Matrix.CreateFromAxisAngle(planeNormal, (float) angle);
            Matrix matrix2 = (bone.Parent != null) ? bone.Parent.AbsoluteTransform : Matrix.Identity;
            Matrix matrix3 = Matrix.Multiply(bone.AbsoluteTransform * matrix, Matrix.Invert(bone.BindTransform * matrix2));
            bone.Rotation = Quaternion.CreateFromRotationMatrix(matrix3);
            bone.ComputeAbsoluteTransform(true);
        }

        public static bool SolveCCDIk(ref Vector3 desiredEnd, List<MyCharacterBone> bones, float stopDistance, int maxTries, float gain, ref Matrix finalTransform, MyCharacterBone finalBone = null, bool allowFinalBoneTranslation = true)
        {
            MyCharacterBone bone = bones.Last<MyCharacterBone>();
            int num3 = 0;
            Vector3D zero = Vector3.Zero;
            while (true)
            {
                foreach (MyCharacterBone bone2 in bones.Reverse<MyCharacterBone>())
                {
                    bone.ComputeAbsoluteTransform(true);
                    Matrix absoluteTransform = bone2.AbsoluteTransform;
                    Vector3D translation = absoluteTransform.Translation;
                    Matrix matrix2 = bone.AbsoluteTransform;
                    zero = matrix2.Translation;
                    if (Vector3D.DistanceSquared(zero, desiredEnd) > stopDistance)
                    {
                        Vector3D vectord4 = zero - translation;
                        Vector3D v = desiredEnd - translation;
                        vectord4.Normalize();
                        v.Normalize();
                        double d = vectord4.Dot(v);
                        if (d < 1.0)
                        {
                            Vector3D vectord5 = vectord4.Cross(v);
                            vectord5.Normalize();
                            double num2 = Math.Acos(d);
                            Matrix matrix3 = Matrix.CreateFromAxisAngle((Vector3) vectord5, ((float) num2) * gain);
                            Matrix identity = Matrix.Identity;
                            if (bone2.Parent != null)
                            {
                                identity = bone2.Parent.AbsoluteTransform;
                            }
                            identity = Matrix.Normalize(identity);
                            bone2.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Multiply(Matrix.Normalize(absoluteTransform).GetOrientation() * matrix3, Matrix.Invert(bone2.BindTransform * identity)));
                            bone2.ComputeAbsoluteTransform(true);
                        }
                    }
                }
                num3++;
                if ((num3 >= maxTries) || (Vector3D.DistanceSquared(zero, desiredEnd) <= stopDistance))
                {
                    if ((finalBone != null) && finalTransform.IsValid())
                    {
                        MatrixD xd = !allowFinalBoneTranslation ? (finalTransform.GetOrientation() * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform)) : (finalTransform * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform));
                        finalBone.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Normalize((Matrix) xd.GetOrientation()));
                        if (allowFinalBoneTranslation)
                        {
                            finalBone.Translation = (Vector3) xd.Translation;
                        }
                        finalBone.ComputeAbsoluteTransform(true);
                    }
                    return (Vector3D.DistanceSquared(zero, desiredEnd) <= stopDistance);
                }
            }
        }

        public static bool SolveTwoJointsIk(ref Vector3 desiredEnd, MyCharacterBone firstBone, MyCharacterBone secondBone, MyCharacterBone endBone, ref Matrix finalTransform, Matrix WorldMatrix, MyCharacterBone finalBone = null, bool allowFinalBoneTranslation = true)
        {
            Matrix absoluteTransform = firstBone.AbsoluteTransform;
            Matrix matrix2 = secondBone.AbsoluteTransform;
            Matrix matrix3 = endBone.AbsoluteTransform;
            Vector3 translation = absoluteTransform.Translation;
            Vector3 vector2 = matrix3.Translation - translation;
            Vector3 vector3 = desiredEnd - translation;
            Vector3 vector4 = matrix2.Translation - translation;
            Vector3 position = vector2 - vector4;
            float num = vector4.Length();
            float num2 = position.Length();
            float num3 = vector3.Length();
            float num4 = vector2.Length();
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_IKSOLVERS)
            {
                MyRenderProxy.DebugDrawSphere(Vector3.Transform(desiredEnd, WorldMatrix), 0.01f, Color.Red, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation, WorldMatrix), Vector3.Transform(translation + vector2, WorldMatrix), Color.Yellow, Color.Yellow, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation, WorldMatrix), Vector3.Transform(translation + vector3, WorldMatrix), Color.Red, Color.Red, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation, WorldMatrix), Vector3.Transform(translation + vector4, WorldMatrix), Color.Green, Color.Green, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation + vector4, WorldMatrix), Vector3.Transform((translation + vector4) + position, WorldMatrix), Color.Blue, Color.Blue, false, false);
            }
            bool flag = (num + num2) > num3;
            double num5 = 0.0;
            double num6 = 0.0;
            if (flag)
            {
                num5 = Math.Acos(MathHelper.Clamp((double) (-(((num2 * num2) - (num * num)) - (num3 * num3)) / ((2f * num) * num3)), -1.0, 1.0));
                num6 = 3.1415926535897931 - Math.Acos(MathHelper.Clamp((double) (-(((num3 * num3) - (num * num)) - (num2 * num2)) / ((2f * num) * num2)), -1.0, 1.0));
            }
            Vector3 axis = Vector3.Cross(vector4, vector2);
            axis.Normalize();
            float angle = (float) (num5 - Math.Acos(MathHelper.Clamp((double) (-(((num2 * num2) - (num * num)) - (num4 * num4)) / ((2f * num) * num4)), -1.0, 1.0)));
            Matrix matrix = Matrix.CreateFromAxisAngle(-axis, angle);
            vector2.Normalize();
            vector3.Normalize();
            Vector3 vector7 = Vector3.Cross(vector2, vector3);
            vector7.Normalize();
            matrix = Matrix.CreateFromAxisAngle(vector7, (float) Math.Acos(MathHelper.Clamp((double) vector2.Dot(vector3), -1.0, 1.0))) * matrix;
            Matrix matrix5 = Matrix.CreateFromAxisAngle(axis, (float) (num6 - (3.1415926535897931 - Math.Acos(MathHelper.Clamp((double) (-(((num4 * num4) - (num * num)) - (num2 * num2)) / ((2f * num) * num2)), -1.0, 1.0))))) * matrix;
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_IK_IKSOLVERS)
            {
                Vector3 vector8 = Vector3.Transform(vector4, matrix);
                Vector3 vector9 = Vector3.Transform(position, matrix5);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation, WorldMatrix), Vector3.Transform(translation + vector8, WorldMatrix), Color.Purple, Color.Purple, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3.Transform(translation + vector8, WorldMatrix), Vector3.Transform((translation + vector8) + vector9, WorldMatrix), Color.White, Color.White, false, false);
            }
            Matrix matrix6 = firstBone.Parent.AbsoluteTransform;
            Matrix matrix7 = Matrix.Multiply(absoluteTransform * matrix, Matrix.Invert(firstBone.BindTransform * matrix6));
            firstBone.Rotation = Quaternion.CreateFromRotationMatrix(matrix7);
            firstBone.ComputeAbsoluteTransform(true);
            Matrix matrix8 = secondBone.Parent.AbsoluteTransform;
            Matrix matrix9 = Matrix.Multiply(matrix2 * matrix5, Matrix.Invert(secondBone.BindTransform * matrix8));
            secondBone.Rotation = Quaternion.CreateFromRotationMatrix(matrix9);
            secondBone.ComputeAbsoluteTransform(true);
            if (((finalBone != null) && finalTransform.IsValid()) & flag)
            {
                MatrixD xd = !allowFinalBoneTranslation ? (finalTransform.GetOrientation() * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform)) : (finalTransform * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform));
                finalBone.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Normalize((Matrix) xd.GetOrientation()));
                if (allowFinalBoneTranslation)
                {
                    finalBone.Translation = (Vector3) xd.Translation;
                }
                finalBone.ComputeAbsoluteTransform(true);
            }
            return flag;
        }

        public static bool SolveTwoJointsIk(ref Vector3 desiredEnd, MyCharacterBone firstBone, MyCharacterBone secondBone, MyCharacterBone endBone, ref Matrix finalTransform, Matrix WorldMatrix, Vector3 normal, bool preferPositiveAngle = true, MyCharacterBone finalBone = null, bool allowFinalBoneTranslation = true, bool minimizeRotation = true)
        {
            throw new NotImplementedException();
        }

        public static bool SolveTwoJointsIkCCD(MyCharacterBone[] characterBones, int firstBoneIndex, int secondBoneIndex, int endBoneIndex, ref Matrix finalTransform, ref MatrixD worldMatrix, MyCharacterBone finalBone = null, bool allowFinalBoneTranslation = true)
        {
            Matrix bindTransform;
            if (finalBone == null)
            {
                return false;
            }
            Vector3 translation = finalTransform.Translation;
            int num = 0;
            int num2 = 50;
            float num3 = 2.5E-05f;
            MyCharacterBone bone1 = characterBones[firstBoneIndex];
            MyCharacterBone bone3 = characterBones[secondBoneIndex];
            MyCharacterBone bone = characterBones[endBoneIndex];
            int[] numArray = new int[] { endBoneIndex };
            numArray[2] = firstBoneIndex;
            numArray[1] = secondBoneIndex;
            Vector3 zero = Vector3.Zero;
            for (int i = 0; i < 3; i++)
            {
                MyCharacterBone bone4 = characterBones[numArray[i]];
                bindTransform = bone4.BindTransform;
                Vector3 vector5 = bindTransform.Translation;
                Quaternion rotation = Quaternion.CreateFromRotationMatrix(bone4.BindTransform);
                bone4.SetCompleteTransform(ref vector5, ref rotation);
                bone4.ComputeAbsoluteTransform(true);
            }
            bone.ComputeAbsoluteTransform(true);
            zero = bone.AbsoluteTransform.Translation;
            float num4 = 1f / ((float) Vector3D.DistanceSquared(zero, translation));
            while (true)
            {
                int index = 0;
                while (true)
                {
                    if (index >= 3)
                    {
                        num++;
                        if ((num < num2) && (Vector3D.DistanceSquared(zero, translation) > num3))
                        {
                            break;
                        }
                        if (finalTransform.IsValid())
                        {
                            MatrixD xd = !allowFinalBoneTranslation ? (finalTransform.GetOrientation() * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform)) : (finalTransform * MatrixD.Invert(finalBone.BindTransform * finalBone.Parent.AbsoluteTransform));
                            finalBone.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Normalize((Matrix) xd.GetOrientation()));
                            if (allowFinalBoneTranslation)
                            {
                                finalBone.Translation = (Vector3) xd.Translation;
                            }
                            finalBone.ComputeAbsoluteTransform(true);
                        }
                        return true;
                    }
                    MyCharacterBone bone2 = characterBones[numArray[index]];
                    bone.ComputeAbsoluteTransform(true);
                    Matrix absoluteTransform = bone2.AbsoluteTransform;
                    Vector3 vector2 = absoluteTransform.Translation;
                    zero = bone.AbsoluteTransform.Translation;
                    double num7 = Vector3D.DistanceSquared(zero, translation);
                    if (num7 > num3)
                    {
                        Vector3 vector4 = zero - vector2;
                        Vector3 v = translation - vector2;
                        double num8 = vector4.LengthSquared();
                        double num9 = v.LengthSquared();
                        double num10 = vector4.Dot(v);
                        if ((num10 < 0.0) || ((num10 * num10) < ((num8 * num9) * 0.99998998641967773)))
                        {
                            Matrix matrix3;
                            Vector3 toVector = Vector3.Lerp(vector4, v, 1f / ((num4 * ((float) num7)) + 1f));
                            Matrix.CreateRotationFromTwoVectors(ref vector4, ref toVector, out matrix3);
                            bindTransform = Matrix.Normalize(absoluteTransform);
                            Matrix identity = Matrix.Identity;
                            if (bone2.Parent != null)
                            {
                                identity = bone2.Parent.AbsoluteTransform;
                            }
                            identity = Matrix.Normalize(identity);
                            bone2.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Multiply(bindTransform.GetOrientation() * matrix3, Matrix.Invert(bone2.BindTransform * identity)));
                            bone2.ComputeAbsoluteTransform(true);
                        }
                    }
                    index++;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CastHit
        {
            public Vector3 Position;
            public Vector3 Normal;
        }
    }
}

