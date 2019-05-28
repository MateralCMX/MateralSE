namespace Sandbox.Game.Components
{
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentInventoryItem : MyRenderComponent
    {
        private MyBaseInventoryItemEntity m_invetoryItem;

        public override unsafe void Draw()
        {
            base.Draw();
            Vector3 position = (Vector3) Vector3.Transform((Vector3) base.Container.Entity.PositionComp.GetPosition(), MySector.MainCamera.ViewMatrix);
            Vector4 vector = Vector4.Transform(position, (Matrix) MySector.MainCamera.ProjectionMatrix);
            if (position.Z > 0f)
            {
                float* singlePtr1 = (float*) ref vector.X;
                singlePtr1[0] *= -1f;
                float* singlePtr2 = (float*) ref vector.Y;
                singlePtr2[0] *= -1f;
            }
            if (vector.W > 0f)
            {
                Vector2 normalizedCoord = new Vector2(((vector.X / vector.W) / 2f) + 0.5f, ((-vector.Y / vector.W) / 2f) + 0.5f);
                normalizedCoord = MyGuiManager.GetHudPixelCoordFromNormalizedCoord(normalizedCoord);
                for (int i = 0; i < this.m_invetoryItem.IconTextures.Length; i++)
                {
                    MyGuiManager.DrawSprite(this.m_invetoryItem.IconTextures[i], normalizedCoord, new Rectangle(0, 0, 0x80, 0x80), Color.White, 0f, new Vector2(64f, 64f), new Vector2(0.5f), SpriteEffects.None, 0f, true);
                }
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_invetoryItem = base.Container.Entity as MyBaseInventoryItemEntity;
        }
    }
}

