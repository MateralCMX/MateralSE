namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Animations;

    public class MyRagdollMapper
    {
        public const float RAGDOLL_DEACTIVATION_TIME = 10f;
        private MyRagdollAnimWeightBlendingHelper m_animationBlendingHelper = new MyRagdollAnimWeightBlendingHelper();
        private Dictionary<int, List<int>> m_rigidBodiesToBonesIndices = new Dictionary<int, List<int>>();
        private MyCharacter m_character;
        private MyCharacterBone[] m_bones;
        private Matrix[] m_ragdollRigidBodiesAbsoluteTransforms;
        public Matrix[] BodiesRigTransfoms;
        public Matrix[] BonesRigTransforms;
        public Matrix[] BodiesRigTransfomsInverted;
        public Matrix[] BonesRigTransformsInverted;
        private Matrix[] m_bodyToBoneRigTransforms;
        private Matrix[] m_boneToBodyRigTransforms;
        private Dictionary<string, int> m_rigidBodies;
        public bool PositionChanged;
        private bool m_inicialized;
        private List<int> m_keyframedBodies;
        private List<int> m_dynamicBodies;
        private Dictionary<string, MyCharacterDefinition.RagdollBoneSet> m_ragdollBonesMappings;
        private MatrixD m_lastSyncedWorldMatrix = MatrixD.Identity;
        public float DeactivationCounter = 10f;
        private bool m_changed;

        public MyRagdollMapper(MyCharacter character, MyCharacterBone[] bones)
        {
            this.m_character = character;
            this.m_bones = bones;
            this.m_rigidBodies = new Dictionary<string, int>();
            this.m_keyframedBodies = new List<int>();
            this.m_dynamicBodies = new List<int>();
            this.IsActive = false;
            this.m_inicialized = false;
            this.IsPartiallySimulated = false;
        }

        public void Activate()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.Activate");
            }
            if (this.Ragdoll == null)
            {
                this.IsActive = false;
            }
            else
            {
                this.IsActive = true;
                this.m_character.Physics.Ragdoll.AddedToWorld -= new Action<HkRagdoll>(this.OnRagdollAdded);
                this.m_character.Physics.Ragdoll.AddedToWorld += new Action<HkRagdoll>(this.OnRagdollAdded);
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.Activate - END");
                }
            }
        }

        public void ActivatePartialSimulation(List<int> dynamicRigidBodies = null)
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.ActivatePartialSimulation");
            }
            if ((this.m_inicialized && (this.Ragdoll != null)) && !this.IsPartiallySimulated)
            {
                if (dynamicRigidBodies != null)
                {
                    this.m_dynamicBodies.Clear();
                    this.m_dynamicBodies.AddList<int>(dynamicRigidBodies);
                    this.m_keyframedBodies.Clear();
                    this.m_keyframedBodies.AddRange(this.m_rigidBodies.Values.Except<int>(dynamicRigidBodies));
                }
                this.m_animationBlendingHelper.ResetWeights();
                this.SetBodiesSimulationMode();
                if (this.Ragdoll.InWorld)
                {
                    this.Ragdoll.EnableConstraints();
                    this.Ragdoll.Activate();
                }
                this.IsActive = true;
                this.IsPartiallySimulated = true;
                this.UpdateRagdollPose();
                this.SetVelocities(false, false);
                this.m_character.Physics.Ragdoll.AddedToWorld -= new Action<HkRagdoll>(this.OnRagdollAdded);
                this.m_character.Physics.Ragdoll.AddedToWorld += new Action<HkRagdoll>(this.OnRagdollAdded);
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.ActivatePartialSimulation - END");
                }
            }
        }

        private void AddRigidBodyToBonesMap(int rigidBodyIndex, List<int> bonesIndices, string rigidBodyName)
        {
            foreach (int local1 in bonesIndices)
            {
            }
            this.m_rigidBodiesToBonesIndices.Add(rigidBodyIndex, bonesIndices);
            this.m_rigidBodies.Add(rigidBodyName, rigidBodyIndex);
        }

        public int BodyIndex(string bodyName)
        {
            int num;
            return (!this.m_rigidBodies.TryGetValue(bodyName, out num) ? 0 : num);
        }

        private void CalculateRagdollTransformsFromBones()
        {
            if ((this.Ragdoll != null) && (this.m_inicialized && this.IsActive))
            {
                foreach (int num in this.m_rigidBodiesToBonesIndices.Keys)
                {
                    HkRigidBody local1 = this.Ragdoll.RigidBodies[num];
                    List<int> source = this.m_rigidBodiesToBonesIndices[num];
                    this.m_ragdollRigidBodiesAbsoluteTransforms[num] = this.m_bones[source.First<int>()].AbsoluteTransform;
                }
            }
        }

        public void Deactivate()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.Deactivate");
            }
            if (this.IsPartiallySimulated)
            {
                this.DeactivatePartialSimulation();
            }
            this.IsActive = false;
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.Deactivate -END");
            }
        }

        public void DeactivatePartialSimulation()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.DeactivatePartialSimulation");
            }
            if (this.IsPartiallySimulated && (this.Ragdoll != null))
            {
                if (this.Ragdoll.InWorld)
                {
                    this.Ragdoll.DisableConstraints();
                    this.Ragdoll.Deactivate();
                }
                this.m_keyframedBodies.Clear();
                this.m_dynamicBodies.Clear();
                this.m_dynamicBodies.AddRange(this.m_rigidBodies.Values);
                this.SetBodiesSimulationMode();
                this.Ragdoll.ResetToRigPose();
                this.IsPartiallySimulated = false;
                this.IsActive = false;
                this.m_character.Physics.Ragdoll.AddedToWorld -= new Action<HkRagdoll>(this.OnRagdollAdded);
                this.m_animationBlendingHelper.ResetWeights();
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.DeactivatePartialSimulation - END");
                }
            }
        }

        public void DebugDraw(MatrixD worldMatrix)
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_ORIGINAL_RIG)
                {
                    foreach (int num in this.m_rigidBodiesToBonesIndices.Keys)
                    {
                        Matrix matrix = this.BodiesRigTransfoms[num] * worldMatrix;
                        MyRenderProxy.DebugDrawSphere(matrix.Translation, 0.03f, Color.White, 0.1f, false, false, true, false);
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_ORIGINAL_RIG)
                {
                    foreach (int num2 in this.m_rigidBodiesToBonesIndices.Keys)
                    {
                        (this.m_bodyToBoneRigTransforms[num2] * this.BodiesRigTransfoms[num2]) * worldMatrix;
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_DESIRED)
                {
                    foreach (int num3 in this.m_rigidBodiesToBonesIndices.Keys)
                    {
                        Matrix matrix2 = (this.m_bodyToBoneRigTransforms[num3] * this.Ragdoll.GetRigidBodyLocalTransform(num3)) * worldMatrix;
                        MyRenderProxy.DebugDrawSphere(matrix2.Translation, 0.035f, Color.Blue, 0.8f, false, false, true, false);
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_COMPUTED_BONES)
                {
                    MyCharacterBone[] bones = this.m_bones;
                    for (int i = 0; i < bones.Length; i++)
                    {
                        Matrix matrix3 = bones[i].AbsoluteTransform * worldMatrix;
                        MyRenderProxy.DebugDrawSphere(matrix3.Translation, 0.03f, Color.Red, 0.8f, false, false, true, false);
                    }
                }
                if (MyDebugDrawSettings.DEBUG_DRAW_CHARACTER_RAGDOLL_POSE)
                {
                    foreach (int num5 in this.m_rigidBodiesToBonesIndices.Keys)
                    {
                        Color color = new Color((num5 & 1) * 0xff, (num5 & 2) * 0xff, (num5 & 4) * 0xff);
                        MatrixD xd = this.Ragdoll.GetRigidBodyLocalTransform(num5) * worldMatrix;
                        DrawShape(this.Ragdoll.RigidBodies[num5].GetShape(), xd, color, 0.6f, true);
                        MyRenderProxy.DebugDrawAxis(xd, 0.3f, false, false, false);
                        MyRenderProxy.DebugDrawSphere(xd.Translation, 0.03f, Color.Green, 0.8f, false, false, true, false);
                    }
                }
            }
        }

        public static void DrawShape(HkShape shape, MatrixD worldMatrix, Color color, float alpha, bool shaded = true)
        {
            color.A = (byte) (alpha * 255f);
            if (shape.ShapeType != HkShapeType.Capsule)
            {
                MyRenderProxy.DebugDrawSphere(worldMatrix.Translation, 0.05f, color, 1f, false, false, true, false);
            }
            else
            {
                HkCapsuleShape shape2 = (HkCapsuleShape) shape;
                Vector3 vector = (Vector3) Vector3.Transform(shape2.VertexB, worldMatrix);
                MyRenderProxy.DebugDrawCapsule(Vector3.Transform(shape2.VertexA, worldMatrix), vector, shape2.Radius, color, false, false, false);
            }
        }

        public List<int> GetBodiesBindedToBones(List<string> bones)
        {
            List<int> list = new List<int>();
            foreach (string str in bones)
            {
                foreach (KeyValuePair<string, MyCharacterDefinition.RagdollBoneSet> pair in this.m_ragdollBonesMappings)
                {
                    if (!pair.Value.Bones.Contains<string>(str))
                    {
                        continue;
                    }
                    if (!list.Contains(this.m_rigidBodies[pair.Key]))
                    {
                        list.Add(this.m_rigidBodies[pair.Key]);
                    }
                }
            }
            return list;
        }

        public HkRigidBody GetBodyBindedToBone(MyCharacterBone myCharacterBone)
        {
            if (this.Ragdoll != null)
            {
                if (myCharacterBone == null)
                {
                    return null;
                }
                using (Dictionary<string, MyCharacterDefinition.RagdollBoneSet>.Enumerator enumerator = this.m_ragdollBonesMappings.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<string, MyCharacterDefinition.RagdollBoneSet> current = enumerator.Current;
                        if (current.Value.Bones.Contains<string>(myCharacterBone.Name))
                        {
                            return this.Ragdoll.RigidBodies[this.m_rigidBodies[current.Key]];
                        }
                    }
                }
            }
            return null;
        }

        public bool Init(Dictionary<string, MyCharacterDefinition.RagdollBoneSet> ragdollBonesMappings)
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.Init");
            }
            this.m_ragdollBonesMappings = ragdollBonesMappings;
            using (Dictionary<string, MyCharacterDefinition.RagdollBoneSet>.Enumerator enumerator = ragdollBonesMappings.GetEnumerator())
            {
                bool flag;
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        KeyValuePair<string, MyCharacterDefinition.RagdollBoneSet> current = enumerator.Current;
                        try
                        {
                            string key = current.Key;
                            List<int> bonesIndices = new List<int>();
                            int index = this.Ragdoll.FindRigidBodyIndex(key);
                            string[] bones = current.Value.Bones;
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= bones.Length)
                                {
                                    if (this.Ragdoll.RigidBodies.IsValidIndex<HkRigidBody>(index))
                                    {
                                        this.AddRigidBodyToBonesMap(index, bonesIndices, key);
                                        break;
                                    }
                                    flag = false;
                                }
                                else
                                {
                                    string bone = bones[num2];
                                    int num3 = Array.FindIndex<MyCharacterBone>(this.m_bones, x => x.Name == bone);
                                    if (this.m_bones.IsValidIndex<MyCharacterBone>(num3))
                                    {
                                        bonesIndices.Add(num3);
                                        num2++;
                                        continue;
                                    }
                                    flag = false;
                                }
                                return flag;
                            }
                            continue;
                        }
                        catch (Exception)
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        goto TR_0002;
                    }
                    break;
                }
                return flag;
            }
        TR_0002:
            this.InitRigTransforms();
            this.m_inicialized = true;
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.Init FINISHED");
            }
            return true;
        }

        private void InitRigTransforms()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.InitRigTransforms");
            }
            this.m_ragdollRigidBodiesAbsoluteTransforms = new Matrix[this.Ragdoll.RigidBodies.Count];
            this.m_bodyToBoneRigTransforms = new Matrix[this.Ragdoll.RigidBodies.Count];
            this.m_boneToBodyRigTransforms = new Matrix[this.Ragdoll.RigidBodies.Count];
            this.BodiesRigTransfoms = new Matrix[this.Ragdoll.RigidBodies.Count];
            this.BodiesRigTransfomsInverted = new Matrix[this.Ragdoll.RigidBodies.Count];
            foreach (int num in this.m_rigidBodiesToBonesIndices.Keys)
            {
                Matrix absoluteRigTransform = this.m_bones[this.m_rigidBodiesToBonesIndices[num].First<int>()].GetAbsoluteRigTransform();
                Matrix matrix = this.Ragdoll.RigTransforms[num];
                Matrix matrix3 = absoluteRigTransform * Matrix.Invert(matrix);
                Matrix matrix4 = matrix * Matrix.Invert(absoluteRigTransform);
                this.m_bodyToBoneRigTransforms[num] = matrix3;
                this.m_boneToBodyRigTransforms[num] = matrix4;
                this.BodiesRigTransfoms[num] = matrix;
                this.BodiesRigTransfomsInverted[num] = Matrix.Invert(matrix);
            }
            this.BonesRigTransforms = new Matrix[this.m_bones.Length];
            this.BonesRigTransformsInverted = new Matrix[this.m_bones.Length];
            for (int i = 0; i < this.BonesRigTransforms.Length; i++)
            {
                this.BonesRigTransforms[i] = this.m_bones[i].GetAbsoluteRigTransform();
                this.BonesRigTransformsInverted[i] = Matrix.Invert(this.m_bones[i].GetAbsoluteRigTransform());
            }
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.InitRigTransforms - END");
            }
        }

        private void OnRagdollAdded(HkRagdoll ragdoll)
        {
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
            if (this.IsPartiallySimulated)
            {
                this.SetBodiesSimulationMode();
            }
        }

        private void SetBodiesSimulationMode()
        {
            foreach (int num in this.m_dynamicBodies)
            {
                this.Ragdoll.SetToDynamic(num);
                this.Ragdoll.SwitchRigidBodyToLayer(num, 0x1f);
            }
            foreach (int num2 in this.m_keyframedBodies)
            {
                this.Ragdoll.SetToKeyframed(num2);
                this.Ragdoll.SwitchRigidBodyToLayer(num2, 0x1f);
            }
        }

        private void SetBoneTo(RagdollBone ragdollBone, float weight, float dynamicChildrenWeight, float keyframedChildrenWeight, bool translationEnabled)
        {
            if ((this.Ragdoll != null) && (this.m_inicialized && this.IsActive))
            {
                int index = this.m_rigidBodiesToBonesIndices[ragdollBone.m_rigidBodyIndex][0];
                MyCharacterBone bone = this.m_bones[index];
                Matrix matrix = (bone.Parent != null) ? bone.Parent.AbsoluteTransform : Matrix.Identity;
                Matrix matrix3 = (this.m_bodyToBoneRigTransforms[ragdollBone.m_rigidBodyIndex] * this.Ragdoll.GetRigidBodyLocalTransform(ragdollBone.m_rigidBodyIndex)) * Matrix.Invert(bone.BindTransform * matrix);
                if (!this.m_animationBlendingHelper.Initialized)
                {
                    this.m_animationBlendingHelper.Init(this.m_bones, this.m_character.AnimationController.Controller);
                }
                this.m_animationBlendingHelper.BlendWeight(ref weight, bone, this.m_character.AnimationController.Variables);
                weight *= MyFakes.RAGDOLL_ANIMATION_WEIGHTING;
                float single1 = MathHelper.Clamp(weight, 0f, 1f);
                weight = single1;
                if (matrix3.IsValid() && (matrix3 != Matrix.Zero))
                {
                    if (weight == 1f)
                    {
                        bone.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Normalize(matrix3.GetOrientation()));
                        if (translationEnabled)
                        {
                            bone.Translation = matrix3.Translation;
                        }
                    }
                    else
                    {
                        bone.Rotation = Quaternion.Slerp(bone.Rotation, Quaternion.CreateFromRotationMatrix(Matrix.Normalize(matrix3.GetOrientation())), weight);
                        if (translationEnabled)
                        {
                            bone.Translation = Vector3.Lerp(bone.Translation, matrix3.Translation, weight);
                        }
                    }
                }
                bone.ComputeAbsoluteTransform(true);
                foreach (RagdollBone bone2 in ragdollBone.m_children)
                {
                    float num2 = dynamicChildrenWeight;
                    if (this.m_keyframedBodies.Contains(bone2.m_rigidBodyIndex))
                    {
                        num2 = keyframedChildrenWeight;
                    }
                    if (this.IsPartiallySimulated)
                    {
                        this.SetBoneTo(bone2, num2, dynamicChildrenWeight, keyframedChildrenWeight, false);
                    }
                    else
                    {
                        this.SetBoneTo(bone2, num2, dynamicChildrenWeight, keyframedChildrenWeight, !this.Ragdoll.IsRigidBodyPalmOrFoot(bone2.m_rigidBodyIndex) && MyFakes.ENABLE_RAGDOLL_BONES_TRANSLATION);
                    }
                }
            }
        }

        public void SetLimitedVelocities()
        {
            List<HkRigidBody> rigidBodies = this.Ragdoll.RigidBodies;
            if (rigidBodies[0] != null)
            {
                float num;
                float num2;
                HkRigidBody rigidBody = this.m_character.Physics.RigidBody;
                if (rigidBody != null)
                {
                    num = rigidBody.MaxLinearVelocity + 5f;
                    num2 = rigidBody.MaxAngularVelocity + 1f;
                }
                else
                {
                    num = Math.Max((float) 10f, (float) (rigidBodies[0].LinearVelocity.Length() + 5f));
                    num2 = Math.Max((float) 12.56637f, (float) (rigidBodies[0].AngularVelocity.Length() + 1f));
                }
                foreach (int num3 in this.m_dynamicBodies)
                {
                    if (this.IsPartiallySimulated)
                    {
                        rigidBodies[num3].MaxLinearVelocity = num;
                        rigidBodies[num3].MaxAngularVelocity = num2;
                        rigidBodies[num3].LinearDamping = 0.2f;
                        rigidBodies[num3].AngularDamping = 0.2f;
                        continue;
                    }
                    rigidBodies[num3].MaxLinearVelocity = this.Ragdoll.MaxLinearVelocity;
                    rigidBodies[num3].MaxAngularVelocity = this.Ragdoll.MaxAngularVelocity;
                    rigidBodies[num3].LinearDamping = 0.5f;
                    rigidBodies[num3].AngularDamping = 0.5f;
                }
            }
        }

        public void SetRagdollToDynamic()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.SetRagdollToDynamic");
            }
            if (this.Ragdoll != null)
            {
                this.Ragdoll.SetToDynamic();
                this.m_keyframedBodies.Clear();
                this.m_dynamicBodies.Clear();
                this.m_dynamicBodies.AddRange(this.m_rigidBodies.Values);
                this.IsPartiallySimulated = false;
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.SetRagdollToDynamic - END");
                }
            }
        }

        public void SetRagdollToKeyframed()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.SetRagdollToKeyframed");
            }
            if (this.Ragdoll != null)
            {
                this.Ragdoll.SetToKeyframed();
                this.m_dynamicBodies.Clear();
                this.m_keyframedBodies.Clear();
                this.m_keyframedBodies.AddRange(this.m_rigidBodies.Values);
                this.IsPartiallySimulated = false;
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.SetRagdollToKeyframed - END");
                }
            }
        }

        public void SetVelocities(bool onlyKeyframed = false, bool onlyIfChanged = false)
        {
            if ((this.m_inicialized && this.IsActive) && ((this.m_character != null) && (this.m_character.Physics != null)))
            {
                MyPhysicsBody physics = this.m_character.Physics;
                bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
                if (this.m_changed || !onlyIfChanged)
                {
                    physics.SetRagdollVelocities(onlyKeyframed ? this.m_keyframedBodies : null, null);
                }
            }
        }

        public void SyncRigidBodiesTransforms(MatrixD worldTransform)
        {
            bool flag = this.m_lastSyncedWorldMatrix != worldTransform;
            foreach (int num in this.m_rigidBodiesToBonesIndices.Keys)
            {
                HkRigidBody local1 = this.Ragdoll.RigidBodies[num];
                Matrix rigidBodyLocalTransform = this.Ragdoll.GetRigidBodyLocalTransform(num);
                flag = (this.m_ragdollRigidBodiesAbsoluteTransforms[num] != rigidBodyLocalTransform) | flag;
                this.m_ragdollRigidBodiesAbsoluteTransforms[num] = rigidBodyLocalTransform;
            }
            if (flag && MyFakes.ENABLE_RAGDOLL_CLIENT_SYNC)
            {
                this.m_character.SendRagdollTransforms((Matrix) worldTransform, this.m_ragdollRigidBodiesAbsoluteTransforms);
                this.m_lastSyncedWorldMatrix = worldTransform;
            }
        }

        public void UpdateCharacterPose(float dynamicBodiesWeight = 1f, float keyframedBodiesWeight = 1f)
        {
            if (this.m_inicialized && this.IsActive)
            {
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateCharacterPose");
                }
                float weight = dynamicBodiesWeight;
                if (this.m_keyframedBodies.Contains(this.Ragdoll.m_ragdollTree.m_rigidBodyIndex))
                {
                    weight = keyframedBodiesWeight;
                }
                this.SetBoneTo(this.Ragdoll.m_ragdollTree, weight, dynamicBodiesWeight, keyframedBodiesWeight, false);
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateCharacterPose - END");
                }
            }
        }

        public void UpdateRagdollAfterSimulation()
        {
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollAfterSimulation");
            }
            if ((this.m_inicialized && this.IsActive) && ((this.Ragdoll != null) && this.Ragdoll.InWorld))
            {
                MatrixD worldMatrix = this.Ragdoll.WorldMatrix;
                this.Ragdoll.UpdateWorldMatrixAfterSimulation();
                this.Ragdoll.UpdateLocalTransforms();
                bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
                this.PositionChanged = worldMatrix != this.Ragdoll.WorldMatrix;
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollAfterSimulation - END");
                }
            }
        }

        public void UpdateRagdollPose()
        {
            if ((this.Ragdoll != null) && (this.m_inicialized && this.IsActive))
            {
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollPose");
                }
                this.CalculateRagdollTransformsFromBones();
                this.UpdateRagdollRigidBodies();
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollPose - END");
                }
            }
        }

        public unsafe void UpdateRagdollPosition()
        {
            MatrixD worldMatrix;
            if (this.Ragdoll == null)
            {
                return;
            }
            if (!this.m_inicialized)
            {
                return;
            }
            else if (this.IsActive)
            {
                if (this.IsPartiallySimulated || this.IsKeyFramed)
                {
                    if (!this.m_character.IsDead)
                    {
                        worldMatrix = this.m_character.Physics.GetWorldMatrix();
                        MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
                        xdPtr2.Translation = this.m_character.Physics.WorldToCluster(worldMatrix.Translation);
                    }
                    else
                    {
                        worldMatrix = this.m_character.WorldMatrix;
                        MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                        xdPtr1.Translation = this.m_character.Physics.WorldToCluster(worldMatrix.Translation);
                        if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                        {
                        }
                    }
                    if (worldMatrix.IsValid() && (worldMatrix != MatrixD.Zero))
                    {
                        int num1;
                        double num = (worldMatrix.Translation - this.Ragdoll.WorldMatrix.Translation).LengthSquared();
                        double num2 = (worldMatrix.Forward - this.Ragdoll.WorldMatrix.Forward).LengthSquared();
                        double num3 = (worldMatrix.Up - this.Ragdoll.WorldMatrix.Up).LengthSquared();
                        if ((num > 1.0000000116860974E-07) || (num2 > 1.0000000116860974E-07))
                        {
                            num1 = 1;
                        }
                        else
                        {
                            num1 = (int) (num3 > 1.0000000116860974E-07);
                        }
                        this.m_changed = (bool) num1;
                        if (num > 10.0)
                        {
                            goto TR_0005;
                        }
                        else if (!this.m_character.m_positionResetFromServer)
                        {
                            if (this.m_changed)
                            {
                                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                                {
                                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollPosition");
                                }
                                this.Ragdoll.SetWorldMatrix(worldMatrix, true, false);
                            }
                        }
                        else
                        {
                            goto TR_0005;
                        }
                    }
                }
                return;
            }
            else
            {
                return;
            }
        TR_0005:
            this.m_character.m_positionResetFromServer = false;
            if (MyFakes.ENABLE_RAGDOLL_DEBUG)
            {
                MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollPosition");
            }
            this.Ragdoll.SetWorldMatrix(worldMatrix);
            bool flag1 = MyFakes.ENABLE_RAGDOLL_DEBUG;
        }

        private void UpdateRagdollRigidBodies()
        {
            if ((this.Ragdoll != null) && (this.m_inicialized && this.IsActive))
            {
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollRigidBodies");
                }
                foreach (int num in this.m_keyframedBodies)
                {
                    HkRigidBody local1 = this.Ragdoll.RigidBodies[num];
                    if (this.m_ragdollRigidBodiesAbsoluteTransforms[num].IsValid() && (this.m_ragdollRigidBodiesAbsoluteTransforms[num] != Matrix.Zero))
                    {
                        Matrix localTransform = this.m_boneToBodyRigTransforms[num] * this.m_ragdollRigidBodiesAbsoluteTransforms[num];
                        Quaternion quaternion = Quaternion.CreateFromRotationMatrix(localTransform.GetOrientation());
                        quaternion.Normalize();
                        localTransform = Matrix.CreateFromQuaternion(quaternion);
                        localTransform.Translation = localTransform.Translation;
                        this.Ragdoll.SetRigidBodyLocalTransform(num, localTransform);
                    }
                }
                if (MyFakes.ENABLE_RAGDOLL_DEBUG)
                {
                    MyLog.Default.WriteLine("MyRagdollMapper.UpdateRagdollRigidBodies - END");
                }
            }
        }

        internal void UpdateRigidBodiesTransformsSynced(int transformsCount, Matrix worldMatrix, Matrix[] transforms)
        {
            if ((this.m_inicialized && this.IsActive) && ((this.Ragdoll != null) && this.Ragdoll.InWorld))
            {
                List<Vector3> list = new List<Vector3>();
                List<Vector3> list2 = new List<Vector3>();
                if (transformsCount == this.m_ragdollRigidBodiesAbsoluteTransforms.Length)
                {
                    for (int i = 0; i < transformsCount; i++)
                    {
                        list.Add(this.Ragdoll.RigidBodies[i].LinearVelocity);
                        list2.Add(this.Ragdoll.RigidBodies[i].AngularVelocity);
                        this.Ragdoll.SetRigidBodyLocalTransform(i, transforms[i]);
                    }
                }
                MatrixD world = worldMatrix;
                world.Translation = this.m_character.Physics.WorldToCluster(worldMatrix.Translation);
                this.Ragdoll.SetWorldMatrix(world, false, false);
                foreach (int num2 in this.m_rigidBodiesToBonesIndices.Keys)
                {
                    this.Ragdoll.RigidBodies[num2].LinearVelocity = list[num2];
                    this.Ragdoll.RigidBodies[num2].AngularVelocity = list2[num2];
                }
            }
        }

        public bool IsKeyFramed =>
            ((this.Ragdoll != null) ? this.Ragdoll.IsKeyframed : false);

        public bool IsPartiallySimulated { get; private set; }

        public Dictionary<int, List<int>> RigidBodiesToBonesIndices =>
            this.m_rigidBodiesToBonesIndices;

        public bool IsActive { get; private set; }

        public HkRagdoll Ragdoll =>
            this.m_character?.Physics?.Ragdoll;
    }
}

