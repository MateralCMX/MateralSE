namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using System;
    using VRageRender;
    using VRageRender.Animations;
    using VRageRender.Messages;

    internal class MyRenderComponentSkinnedEntity : MyRenderComponent
    {
        private bool m_sentSkeletonMessage;
        protected MySkinnedEntity m_skinnedEntity;

        public override void AddRenderObjects()
        {
            if ((base.m_model != null) && !base.IsRenderObjectAssigned(0))
            {
                this.SetRenderObjectID(0, MyRenderProxy.CreateRenderCharacter(base.Container.Entity.DisplayName, base.m_model.AssetName, base.Container.Entity.PositionComp.WorldMatrix, new Color?(base.m_diffuseColor), new Vector3?(base.ColorMaskHsv), this.GetRenderFlags(), base.FadeIn));
                this.m_sentSkeletonMessage = false;
                base.SetVisibilityUpdates(true);
                this.UpdateCharacterSkeleton();
            }
        }

        public override void Draw()
        {
            base.Draw();
            this.UpdateCharacterSkeleton();
            MyRenderProxy.SetCharacterTransforms(base.RenderObjectIDs[0], this.m_skinnedEntity.BoneAbsoluteTransforms, this.m_skinnedEntity.DecalBoneUpdates);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_skinnedEntity = base.Container.Entity as MySkinnedEntity;
        }

        private void UpdateCharacterSkeleton()
        {
            if (!this.m_sentSkeletonMessage)
            {
                this.m_sentSkeletonMessage = true;
                MyCharacterBone[] characterBones = this.m_skinnedEntity.AnimationController.CharacterBones;
                MySkeletonBoneDescription[] skeletonBones = new MySkeletonBoneDescription[characterBones.Length];
                int index = 0;
                while (true)
                {
                    if (index >= characterBones.Length)
                    {
                        MyRenderProxy.SetCharacterSkeleton(base.RenderObjectIDs[0], skeletonBones, base.Model.Animations.Skeleton.ToArray());
                        break;
                    }
                    skeletonBones[index].Parent = (characterBones[index].Parent != null) ? characterBones[index].Parent.Index : -1;
                    skeletonBones[index].SkinTransform = characterBones[index].SkinTransform;
                    index++;
                }
            }
        }
    }
}

