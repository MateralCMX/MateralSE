namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyCharacterBone
    {
        private readonly MyCharacterBone m_parent;
        private readonly List<MyCharacterBone> m_children;
        private Matrix m_bindTransform = Matrix.Identity;
        private Matrix m_bindTransformInv = Matrix.Identity;
        private Quaternion m_bindRotationInv = Quaternion.Identity;
        private Vector3 m_translation = Vector3.Zero;
        private Quaternion m_rotation = Quaternion.Identity;
        private bool m_changed = true;
        public string Name = "";
        private Matrix[] m_relativeStorage;
        private Matrix[] m_absoluteStorage;

        public MyCharacterBone(string name, MyCharacterBone parent, Matrix bindTransform, int index, Matrix[] relativeStorage, Matrix[] absoluteStorage)
        {
            this.Index = index;
            this.m_relativeStorage = relativeStorage;
            this.m_absoluteStorage = absoluteStorage;
            this.Name = name;
            this.m_parent = parent;
            this.Depth = this.GetHierarchyDepth();
            this.m_bindTransform = bindTransform;
            this.m_bindTransformInv = Matrix.Invert(bindTransform);
            this.m_bindRotationInv = Quaternion.CreateFromRotationMatrix(this.m_bindTransformInv);
            this.m_children = new List<MyCharacterBone>();
            if (this.m_parent != null)
            {
                this.m_parent.AddChild(this);
            }
            this.ComputeAbsoluteTransform(true);
            this.SkinTransform = Matrix.Invert(this.AbsoluteTransform);
        }

        internal void AddChild(MyCharacterBone child)
        {
            this.m_children.Add(child);
        }

        public void ComputeAbsoluteTransform(bool propagateTransformToChildren = true)
        {
            if (this.HasThisOrAnyParentChanged)
            {
                this.m_changed = this.ComputeBoneTransform();
                if (this.Parent != null)
                {
                    Matrix.Multiply(ref this.m_relativeStorage[this.Index], ref this.m_absoluteStorage[this.Parent.Index], out this.m_absoluteStorage[this.Index]);
                }
                else
                {
                    this.m_absoluteStorage[this.Index] = this.m_relativeStorage[this.Index];
                }
                if (propagateTransformToChildren)
                {
                    this.PropagateTransform();
                }
                this.m_changed = false;
            }
        }

        public static void ComputeAbsoluteTransforms(MyCharacterBone[] bones)
        {
            int num;
            MyCharacterBone[] boneArray = bones;
            for (num = 0; num < boneArray.Length; num++)
            {
                MyCharacterBone bone = boneArray[num];
                if (bone.Parent != null)
                {
                    bone.m_changed = bone.ComputeBoneTransform() || bone.Parent.m_changed;
                    if (bone.m_changed)
                    {
                        Matrix.Multiply(ref bone.m_relativeStorage[bone.Index], ref bone.m_absoluteStorage[bone.Parent.Index], out bone.m_absoluteStorage[bone.Index]);
                    }
                }
                else
                {
                    bone.m_changed = bone.ComputeBoneTransform();
                    if (bone.m_changed)
                    {
                        bone.m_absoluteStorage[bone.Index] = bone.m_relativeStorage[bone.Index];
                    }
                }
            }
            boneArray = bones;
            for (num = 0; num < boneArray.Length; num++)
            {
                boneArray[num].m_changed = false;
            }
        }

        public bool ComputeBoneTransform()
        {
            if (!this.m_changed)
            {
                return false;
            }
            Matrix.CreateFromQuaternion(ref this.m_rotation, out this.m_relativeStorage[this.Index]);
            this.m_relativeStorage[this.Index].M41 = this.m_translation.X;
            this.m_relativeStorage[this.Index].M42 = this.m_translation.Y;
            this.m_relativeStorage[this.Index].M43 = this.m_translation.Z;
            Matrix.Multiply(ref this.m_relativeStorage[this.Index], ref this.m_bindTransform, out this.m_relativeStorage[this.Index]);
            this.m_changed = false;
            return true;
        }

        public Matrix GetAbsoluteRigTransform()
        {
            MyCharacterBone parent = this.m_parent;
            if (parent == null)
            {
                return this.m_bindTransform;
            }
            Matrix bindTransform = this.m_bindTransform;
            while (parent != null)
            {
                bindTransform *= parent.m_bindTransform;
                parent = parent.Parent;
            }
            return bindTransform;
        }

        public MyCharacterBone GetChildBone(int childIndex)
        {
            if (((this.m_children == null) || (childIndex < 0)) || (childIndex >= this.m_children.Count))
            {
                return null;
            }
            return this.m_children[childIndex];
        }

        public void GetCompleteTransform(ref Vector3 translation, ref Quaternion rotation, out Vector3 completeTranslation, out Quaternion completeRotation)
        {
            Vector3.Transform(ref translation, ref this.m_bindTransformInv, out completeTranslation);
            Quaternion.Multiply(ref this.m_bindRotationInv, ref rotation, out completeRotation);
        }

        private int GetHierarchyDepth()
        {
            int num = 0;
            MyCharacterBone parent = this.m_parent;
            while (parent != null)
            {
                parent = parent.Parent;
                num++;
            }
            return num;
        }

        private void PropagateTransform()
        {
            using (List<MyCharacterBone>.Enumerator enumerator = this.m_children.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ComputeAbsoluteTransform(true);
                }
            }
        }

        internal void SetBindTransform(Matrix bindTransform)
        {
            this.m_changed = true;
            this.m_bindTransform = bindTransform;
            this.m_bindTransformInv = Matrix.Invert(bindTransform);
            this.m_bindRotationInv = Quaternion.CreateFromRotationMatrix(this.m_bindTransformInv);
        }

        public void SetCompleteBindTransform()
        {
            this.m_changed = true;
            this.Translation = Vector3.Zero;
            this.Rotation = Quaternion.Identity;
        }

        public void SetCompleteRotation(ref Quaternion rotation)
        {
            Quaternion.Multiply(ref this.m_bindRotationInv, ref rotation, out this.m_rotation);
            this.m_changed = true;
        }

        public void SetCompleteTransform(ref Vector3 translation, ref Quaternion rotation)
        {
            Vector3.Transform(ref translation, ref this.m_bindTransformInv, out this.m_translation);
            Quaternion.Multiply(ref this.m_bindRotationInv, ref rotation, out this.m_rotation);
            this.m_changed = true;
        }

        public void SetCompleteTransform(ref Vector3 translation, ref Quaternion rotation, float weight)
        {
            Vector3 vector;
            Quaternion quaternion;
            this.m_changed = true;
            Vector3.Transform(ref translation, ref this.m_bindTransformInv, out vector);
            this.Translation = Vector3.Lerp(this.Translation, vector, weight);
            Quaternion.Multiply(ref this.m_bindRotationInv, ref rotation, out quaternion);
            this.Rotation = Quaternion.Slerp(this.Rotation, quaternion, weight);
        }

        public void SetCompleteTransformFromAbsoluteMatrix(ref Matrix absoluteMatrix, bool onlyRotation)
        {
            Matrix identity = Matrix.Identity;
            if (this.Parent != null)
            {
                identity = this.Parent.AbsoluteTransform;
            }
            Matrix matrix = (absoluteMatrix * Matrix.Invert(identity)) * this.m_bindTransformInv;
            this.Rotation = Quaternion.CreateFromRotationMatrix(matrix);
            if (!onlyRotation)
            {
                this.Translation = matrix.Translation;
            }
        }

        public void SetCompleteTransformFromAbsoluteMatrix(Matrix absoluteMatrix, bool onlyRotation)
        {
            this.SetCompleteTransformFromAbsoluteMatrix(ref absoluteMatrix, onlyRotation);
        }

        public override string ToString() => 
            (this.Name + " [MyCharacterBone]");

        public static unsafe void TranslateAllBones(MyCharacterBone[] characterBones, Vector3 translationModelSpace)
        {
            if ((characterBones != null) && (characterBones.Length >= 0))
            {
                foreach (MyCharacterBone bone in characterBones)
                {
                    if (bone.Parent == null)
                    {
                        bone.Translation += translationModelSpace;
                        bone.ComputeBoneTransform();
                        bone.m_changed = false;
                    }
                    Matrix* matrixPtr1 = (Matrix*) ref bone.m_absoluteStorage[bone.Index];
                    matrixPtr1.Translation += translationModelSpace;
                }
            }
        }

        public int Index { get; private set; }

        public Matrix BindTransform =>
            this.m_bindTransform;

        public Matrix BindTransformInv =>
            this.m_bindTransformInv;

        public Matrix SkinTransform { get; set; }

        public Quaternion Rotation
        {
            get => 
                this.m_rotation;
            set
            {
                this.m_rotation = value;
                this.m_changed = true;
            }
        }

        public Vector3 Translation
        {
            get => 
                this.m_translation;
            set
            {
                this.m_translation = value;
                this.m_changed = true;
            }
        }

        public MyCharacterBone Parent =>
            this.m_parent;

        public Matrix AbsoluteTransform =>
            this.m_absoluteStorage[this.Index];

        public Matrix RelativeTransform =>
            this.m_relativeStorage[this.Index];

        private bool HasThisOrAnyParentChanged
        {
            get
            {
                MyCharacterBone parent = this;
                while (!parent.m_changed)
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public int Depth { get; private set; }
    }
}

