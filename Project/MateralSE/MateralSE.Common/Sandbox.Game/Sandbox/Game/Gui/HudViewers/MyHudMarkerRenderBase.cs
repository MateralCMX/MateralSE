namespace Sandbox.Game.GUI.HudViewers
{
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Gui;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyHudMarkerRenderBase
    {
        protected const double LS_METRES = 299792458.00013667;
        protected const double LY_METRES = 9.460730473E+15;
        protected MyGuiScreenHudBase m_hudScreen;
        protected List<MyMarkerStyle> m_markerStyles;
        protected int[] m_markerStylesForBlocks;
        protected DistanceComparer m_distanceComparer = new DistanceComparer();

        public MyHudMarkerRenderBase(MyGuiScreenHudBase hudScreen)
        {
            this.m_hudScreen = hudScreen;
            this.m_markerStyles = new List<MyMarkerStyle>();
            int num = this.AllocateMarkerStyle("White", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_neutral, MyHudConstants.MARKER_COLOR_WHITE);
            int num2 = this.AllocateMarkerStyle("Red", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_enemy, MyHudConstants.MARKER_COLOR_WHITE);
            int num3 = this.AllocateMarkerStyle("DarkBlue", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_me, MyHudConstants.MARKER_COLOR_WHITE);
            int num4 = this.AllocateMarkerStyle("Green", MyHudTexturesEnum.DirectionIndicator, MyHudTexturesEnum.Target_friend, MyHudConstants.MARKER_COLOR_WHITE);
            this.m_markerStylesForBlocks = new int[MyUtils.GetMaxValueFromEnum<MyRelationsBetweenPlayerAndBlock>() + 1];
            this.m_markerStylesForBlocks[3] = num;
            this.m_markerStylesForBlocks[4] = num2;
            this.m_markerStylesForBlocks[1] = num3;
            this.m_markerStylesForBlocks[2] = num4;
            this.m_markerStylesForBlocks[0] = num4;
        }

        protected unsafe void AddTexturedQuad(string texture, Vector2 position, Vector2 upVector, Color color, float halfWidth, float halfHeight)
        {
            Vector2 vector1 = new Vector2((float) MyGuiManager.GetSafeFullscreenRectangle().Width, (float) MyGuiManager.GetSafeFullscreenRectangle().Height);
            float num = vector1.X / MyGuiManager.GetHudSize().X;
            float num2 = vector1.Y / MyGuiManager.GetHudSize().Y;
            if (MyVideoSettingsManager.IsTripleHead())
            {
                float* singlePtr1 = (float*) ref position.X;
                singlePtr1[0]++;
            }
            float* singlePtr2 = (float*) ref position.X;
            singlePtr2[0] *= num;
            float* singlePtr3 = (float*) ref position.Y;
            singlePtr3[0] *= num2;
            RectangleF destination = new RectangleF(position.X - halfWidth, position.Y - halfHeight, halfWidth * 2f, halfHeight * 2f);
            Rectangle? sourceRectangle = null;
            MyRenderProxy.DrawSprite(texture, ref destination, false, ref sourceRectangle, color, 0f, new Vector2(1f, 0f), ref Vector2.Zero, SpriteEffects.None, 0f, true, null);
        }

        protected unsafe void AddTexturedQuad(MyHudTexturesEnum texture, Vector2 position, Vector2 upVector, Color color, float halfWidth, float halfHeight)
        {
            Vector2 rightVector = new Vector2(-upVector.Y, upVector.X);
            MyAtlasTextureCoordinate textureCoord = this.m_hudScreen.GetTextureCoord(texture);
            Vector2 vector1 = new Vector2((float) MyGuiManager.GetSafeFullscreenRectangle().Width, (float) MyGuiManager.GetSafeFullscreenRectangle().Height);
            float x = vector1.X / MyGuiManager.GetHudSize().X;
            float y = vector1.Y / MyGuiManager.GetHudSize().Y;
            Vector2 vector2 = position;
            if (MyVideoSettingsManager.IsTripleHead())
            {
                float* singlePtr1 = (float*) ref vector2.X;
                singlePtr1[0]++;
            }
            float num3 = vector1.Y / 1080f;
            halfWidth *= num3;
            halfHeight *= num3;
            MyRenderProxy.DrawSpriteAtlas(this.m_hudScreen.TextureAtlas, vector2, textureCoord.Offset, textureCoord.Size, rightVector, new Vector2(x, y), color, new Vector2(halfWidth, halfHeight), null);
        }

        public int AllocateMarkerStyle(string font, MyHudTexturesEnum directionIcon, MyHudTexturesEnum targetIcon, Color color)
        {
            this.m_markerStyles.Add(new MyMarkerStyle(font, directionIcon, targetIcon, color, 0f, 1f));
            return this.m_markerStyles.Count;
        }

        public virtual void Draw()
        {
        }

        public virtual void DrawLocationMarkers(MyHudLocationMarkers locationMarkers)
        {
        }

        public int GetStyleForRelation(MyRelationsBetweenPlayerAndBlock relation) => 
            this.m_markerStylesForBlocks[(int) relation];

        public void OverrideStyleForRelation(MyRelationsBetweenPlayerAndBlock relation, string font, MyHudTexturesEnum directionIcon, MyHudTexturesEnum targetIcon, Color color)
        {
            int styleForRelation = this.GetStyleForRelation(relation);
            this.m_markerStyles[styleForRelation] = new MyMarkerStyle(font, directionIcon, targetIcon, color, 0f, 1f);
        }

        public virtual void Update()
        {
        }

        public class DistanceComparer : IComparer<MyHudEntityParams>
        {
            public int Compare(MyHudEntityParams x, MyHudEntityParams y) => 
                Vector3D.DistanceSquared(MySector.MainCamera.Position, y.Position).CompareTo(Vector3D.DistanceSquared(MySector.MainCamera.Position, x.Position));
        }

        public class MyMarkerStyle
        {
            public MyMarkerStyle(string font, MyHudTexturesEnum textureDirectionIndicator, MyHudTexturesEnum textureTarget, VRageMath.Color color, float textureTargetRotationSpeed = 0f, float textureTargetScale = 1f)
            {
                this.Font = font;
                this.TextureDirectionIndicator = textureDirectionIndicator;
                this.TextureTarget = textureTarget;
                this.Color = color;
                this.TextureTargetRotationSpeed = textureTargetRotationSpeed;
                this.TextureTargetScale = textureTargetScale;
            }

            public string Font { get; set; }

            public MyHudTexturesEnum TextureDirectionIndicator { get; set; }

            public MyHudTexturesEnum TextureTarget { get; set; }

            public VRageMath.Color Color { get; set; }

            public float TextureTargetRotationSpeed { get; set; }

            public float TextureTargetScale { get; set; }
        }
    }
}

