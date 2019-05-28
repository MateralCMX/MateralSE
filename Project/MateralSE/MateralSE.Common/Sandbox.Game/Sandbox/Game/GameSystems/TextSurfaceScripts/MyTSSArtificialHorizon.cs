namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRageMath;

    [MyTextSurfaceScript("TSS_ArtificialHorizon", "DisplayName_TSS_ArtificialHorizon")]
    public class MyTSSArtificialHorizon : MyTSSCommon
    {
        private const int HUD_SCALING = 0x4b0;
        private const double PLANET_GRAVITY_THRESHOLD_SQ = 0.0025;
        private const float LADDER_TEXT_SIZE_MULTIPLIER = 0.7f;
        private const int ALTITUDE_WARNING_TIME_THRESHOLD = 0x18;
        private const int RADAR_ALTITUDE_THRESHOLD = 500;
        private static readonly float ANGLE_STEP = 0.08726645f;
        private MyCubeGrid m_grid;
        private MatrixD m_ownerTransform;
        private Vector2 m_innerSize;
        private float m_maxScale;
        private float m_screenDiag;
        private int m_tickCounter;
        private int m_lastRadarAlt;
        private double m_lastSeaLevelAlt;
        private bool m_showAltWarning;
        private int m_altWarningShownAt;
        private readonly Vector2 m_textBoxSize;
        private readonly Vector2 m_textOffsetInsideBox;
        private readonly Vector2 m_ladderStepSize;
        private readonly Vector2 m_ladderStepTextOffset;
        private MyPlanet m_nearestPlanet;

        public MyTSSArtificialHorizon(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            if (base.m_block != null)
            {
                this.m_grid = base.m_block.CubeGrid as MyCubeGrid;
            }
            this.m_maxScale = Math.Min(base.m_scale.X, base.m_scale.Y);
            this.m_innerSize = new Vector2(1.2f, 1f);
            FitRect(size, ref this.m_innerSize);
            this.m_screenDiag = (float) Math.Sqrt((double) ((this.m_innerSize.X * this.m_innerSize.X) + (this.m_innerSize.Y * this.m_innerSize.Y)));
            base.m_fontScale = 1f * this.m_maxScale;
            base.m_fontId = "White";
            this.m_ownerTransform = this.m_grid.PositionComp.WorldMatrix;
            this.m_ownerTransform.Translation = base.m_block.GetPosition();
            this.m_nearestPlanet = MyGamePruningStructure.GetClosestPlanet(this.m_ownerTransform.Translation);
            this.m_textBoxSize = new Vector2(89f, 32f) * this.m_maxScale;
            this.m_textOffsetInsideBox = new Vector2(5f, 0f) * this.m_maxScale;
            this.m_ladderStepSize = new Vector2(150f, 31f) * this.m_maxScale;
            this.m_ladderStepTextOffset = new Vector2(0f, this.m_ladderStepSize.Y * 0.5f);
        }

        private double DrawAltimeter(MySpriteDrawFrame frame, MatrixD worldTrans, MyPlanet nearestPlanet, int radarAltitude, Vector2 textBoxSize)
        {
            string text1;
            double num = Vector3D.Distance(nearestPlanet.PositionComp.GetPosition(), worldTrans.Translation) - nearestPlanet.AverageRadius;
            if (radarAltitude >= 500)
            {
                text1 = ((int) num).ToString();
            }
            else
            {
                text1 = radarAltitude.ToString();
            }
            string text = text1;
            Vector2 vector = base.m_halfSize + (new Vector2(115f, 80f) * this.m_maxScale);
            base.AddTextBox(frame, vector + (textBoxSize * 0.5f), textBoxSize, text, base.m_fontId, base.m_fontScale, base.m_foregroundColor, base.m_foregroundColor, "AH_TextBox", this.m_textOffsetInsideBox.X);
            if (radarAltitude < 500)
            {
                MySprite sprite = MySprite.CreateText("R", base.m_fontId, base.m_foregroundColor, base.m_fontScale, TextAlignment.LEFT);
                sprite.Position = new Vector2?(((vector + (textBoxSize * 0.5f)) + (new Vector2(textBoxSize.X, -textBoxSize.Y) * 0.5f)) + this.m_textOffsetInsideBox);
                frame.Add(sprite);
            }
            double num2 = (num - this.m_lastSeaLevelAlt) * 6.0;
            base.AddTextBox(frame, vector + new Vector2(textBoxSize.X * 0.5f, -textBoxSize.Y * 0.5f), textBoxSize, ((int) num2).ToString(), base.m_fontId, base.m_fontScale, base.m_foregroundColor, base.m_foregroundColor, null, this.m_textOffsetInsideBox.X);
            return num;
        }

        private int DrawAltitudeWarning(MySpriteDrawFrame frame, MatrixD worldTrans, MyPlanet nearestPlanet)
        {
            float num2 = 100f + this.m_grid.PositionComp.LocalAABB.Height;
            int num3 = (int) Vector3D.Distance(nearestPlanet.GetClosestSurfacePointGlobal(worldTrans.Translation), worldTrans.Translation);
            if ((this.m_lastRadarAlt >= num2) && (num3 < num2))
            {
                this.m_showAltWarning = true;
                this.m_altWarningShownAt = this.m_tickCounter;
            }
            if ((this.m_tickCounter - this.m_altWarningShownAt) > 0x18)
            {
                this.m_showAltWarning = false;
            }
            if (this.m_showAltWarning)
            {
                StringBuilder text = MyTexts.Get(MySpaceTexts.DisplayName_TSS_ArtificialHorizon_AltitudeWarning);
                Vector2 vector = MyGuiManager.MeasureStringRaw(base.m_fontId, text, base.m_fontScale);
                MySprite sprite = MySprite.CreateText(text.ToString(), base.m_fontId, base.m_foregroundColor, base.m_fontScale, TextAlignment.LEFT);
                sprite.Position = new Vector2?((base.m_halfSize + new Vector2(0f, 100f)) - (vector * 0.5f));
                frame.Add(sprite);
            }
            return num3;
        }

        private void DrawBoreSight(MySpriteDrawFrame frame)
        {
            MySprite sprite = new MySprite(SpriteType.TEXTURE, "AH_BoreSight", new Vector2?((base.m_size * 0.5f) + (new Vector2(0f, 19f) * this.m_maxScale)), new Vector2?(new Vector2(50f, 50f) * this.m_maxScale), new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, -1.570796f);
            frame.Add(sprite);
        }

        private void DrawHorizon(MySpriteDrawFrame frame, Vector2 screenForward2D, double rollAngle)
        {
            Vector2 vector = new Vector2(this.m_screenDiag);
            Vector2 vector2 = new Vector2(0f, this.m_screenDiag * 0.5f);
            vector2.Rotate(rollAngle);
            MySprite sprite = new MySprite(SpriteType.TEXTURE, "Grid", new Vector2?((base.m_halfSize + vector2) + screenForward2D), new Vector2?(vector), new Color(base.m_foregroundColor, 0.5f), null, TextAlignment.CENTER, (float) rollAngle);
            frame.Add(sprite);
            sprite.Position = new Vector2?((base.m_halfSize - vector2) + screenForward2D);
            frame.Add(sprite);
            vector2 = new Vector2(0f, this.m_screenDiag * 1.5f);
            vector2.Rotate(rollAngle);
            sprite.Position = new Vector2?((base.m_halfSize + vector2) + screenForward2D);
            frame.Add(sprite);
            sprite.Position = new Vector2?((base.m_halfSize - vector2) + screenForward2D);
            frame.Add(sprite);
            MySprite sprite2 = new MySprite(SpriteType.TEXTURE, "SquareTapered", new Vector2?(base.m_halfSize + screenForward2D), new Vector2(this.m_screenDiag, 3f * this.m_maxScale), new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, (float) rollAngle);
            frame.Add(sprite2);
        }

        private void DrawLadder(MySpriteDrawFrame frame, Vector3 gravity, MatrixD worldTrans, double pitchAngle, Vector3D horizonForward, double rollAngle)
        {
            int num = (int) Math.Round((double) (pitchAngle / ((double) ANGLE_STEP)));
            for (int i = num - 5; i <= (num + 5); i++)
            {
                if (i != 0)
                {
                    string text1;
                    Vector3D vectord = Vector3D.TransformNormal(Vector3D.Reject((MatrixD.CreateRotationX((double) (i * ANGLE_STEP)) * MatrixD.CreateWorld(worldTrans.Translation, horizonForward, -gravity)).Forward, worldTrans.Forward), MatrixD.Invert(worldTrans));
                    Vector2 vector = (new Vector2((float) vectord.X, -((float) vectord.Y)) * 1200f) * this.m_maxScale;
                    MySprite sprite = new MySprite(SpriteType.TEXTURE, ((i * ANGLE_STEP) < 0f) ? "AH_GravityHudNegativeDegrees" : "AH_GravityHudPositiveDegrees", new Vector2?(base.m_halfSize + vector), new Vector2?(this.m_ladderStepSize), new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, (float) rollAngle);
                    frame.Add(sprite);
                    float scale = base.m_fontScale * 0.7f;
                    int num4 = Math.Abs((int) (i * 5));
                    if (i <= 0x12)
                    {
                        text1 = num4.ToString();
                    }
                    else
                    {
                        text1 = (180 - (i * 5)).ToString();
                    }
                    Vector2 vector2 = new Vector2(-this.m_ladderStepSize.X * 0.55f, 0f);
                    vector2.Rotate(rollAngle);
                    string text = text1;
                    MySprite sprite2 = MySprite.CreateText(text, base.m_fontId, base.m_foregroundColor, scale, TextAlignment.RIGHT);
                    sprite2.Position = new Vector2?(((base.m_halfSize + vector) + vector2) - this.m_ladderStepTextOffset);
                    frame.Add(sprite2);
                    vector2 = new Vector2(this.m_ladderStepSize.X * 0.55f, 0f);
                    vector2.Rotate(rollAngle);
                    MySprite sprite3 = MySprite.CreateText(text, base.m_fontId, base.m_foregroundColor, scale, TextAlignment.LEFT);
                    sprite3.Position = new Vector2?(((base.m_halfSize + vector) + vector2) - this.m_ladderStepTextOffset);
                    frame.Add(sprite3);
                }
            }
        }

        private void DrawPlanetDisplay(MySpriteDrawFrame frame, Vector3 gravity, MatrixD worldTrans)
        {
            gravity.Normalize();
            Vector3D vectord = Vector3D.Reject(worldTrans.Forward, gravity);
            vectord.Normalize();
            Vector3D vectord2 = Vector3D.TransformNormal(Vector3D.Reject(vectord, worldTrans.Forward), MatrixD.Invert(worldTrans));
            Vector2 vector = (new Vector2((float) vectord2.X, -((float) vectord2.Y)) * 1200f) * this.m_maxScale;
            double rollAngle = -(Math.Acos((double) Vector3.Dot((Vector3) Vector3D.Normalize(Vector3D.Reject(gravity, worldTrans.Forward)), (Vector3) worldTrans.Left)) - 1.570796012878418);
            if (gravity.Dot((Vector3) worldTrans.Up) >= 0f)
            {
                rollAngle = 3.1415926535897931 - rollAngle;
            }
            double pitchAngle = Math.Acos((double) gravity.Dot((Vector3) worldTrans.Forward)) - 1.570796012878418;
            this.DrawHorizon(frame, vector, rollAngle);
            this.DrawLadder(frame, gravity, worldTrans, pitchAngle, vectord, rollAngle);
            if ((this.m_tickCounter % 100) == 0)
            {
                this.m_nearestPlanet = MyGamePruningStructure.GetClosestPlanet(worldTrans.Translation);
            }
            if (this.m_nearestPlanet != null)
            {
                int radarAltitude = this.DrawAltitudeWarning(frame, worldTrans, this.m_nearestPlanet);
                this.m_lastSeaLevelAlt = this.DrawAltimeter(frame, worldTrans, this.m_nearestPlanet, radarAltitude, this.m_textBoxSize);
                this.m_lastRadarAlt = radarAltitude;
            }
            Vector3 linearVelocity = this.m_grid.Physics.LinearVelocity;
            this.DrawPullUpWarning(frame, linearVelocity, worldTrans, rollAngle);
            Vector2 drawPos = base.m_halfSize + (new Vector2(-205f, 80f) * this.m_maxScale);
            MySpriteDrawFrame frame1 = this.DrawSpeedIndicator(frame, drawPos, this.m_textBoxSize, linearVelocity);
            frame = frame1;
            this.DrawVelocityVector(frame, linearVelocity, worldTrans);
            this.DrawBoreSight(frame);
        }

        private void DrawPullUpWarning(MySpriteDrawFrame frame, Vector3 velocity, MatrixD worldTrans, double rollAngle)
        {
            Vector3 vector = (Vector3) ((this.m_grid.Mass / 16000f) * velocity);
            if (((MyPhysics.CastRay(worldTrans.Translation, worldTrans.Translation + vector, 14) != null) && (this.m_tickCounter >= 0)) && ((this.m_tickCounter % 10) > 2))
            {
                MySprite sprite = new MySprite(SpriteType.TEXTURE, "AH_PullUp", new Vector2?(base.m_halfSize), new Vector2(150f, 180f), new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, (float) rollAngle);
                frame.Add(sprite);
            }
        }

        private void DrawSpaceDisplay(MySpriteDrawFrame frame, MatrixD worldTrans)
        {
            base.AddBackground(frame, new Color(base.m_foregroundColor, 0.66f));
            Vector3 linearVelocity = this.m_grid.Physics.LinearVelocity;
            this.DrawVelocityVector(frame, linearVelocity, worldTrans);
            this.DrawBoreSight(frame);
            Vector2 drawPos = base.m_halfSize + (new Vector2(-205f, 80f) * this.m_maxScale);
            this.DrawSpeedIndicator(frame, drawPos, this.m_textBoxSize, linearVelocity);
            Color barBgColor = new Color(base.m_foregroundColor, 0.1f);
            float num = Math.Max(MyGridPhysics.ShipMaxLinearVelocity(), 1f);
            Vector2 size = new Vector2((((base.m_halfSize + (new Vector2(205f, 80f) * this.m_maxScale)).X - drawPos.X) - this.m_textBoxSize.X) - this.m_textOffsetInsideBox.X, this.m_textBoxSize.Y);
            base.AddProgressBar(frame, drawPos + new Vector2(((size.X * 0.5f) + this.m_textBoxSize.X) + this.m_textOffsetInsideBox.X, this.m_textBoxSize.Y / 2f), size, linearVelocity.Length() / num, barBgColor, base.m_foregroundColor, null, null);
        }

        private MySpriteDrawFrame DrawSpeedIndicator(MySpriteDrawFrame frame, Vector2 drawPos, Vector2 textBoxSize, Vector3 velocity)
        {
            base.AddTextBox(frame, drawPos + (textBoxSize * 0.5f), textBoxSize, ((int) velocity.Length()).ToString(), base.m_fontId, base.m_fontScale, base.m_foregroundColor, base.m_foregroundColor, "AH_TextBox", this.m_textOffsetInsideBox.X);
            return frame;
        }

        private void DrawVelocityVector(MySpriteDrawFrame frame, Vector3 velocity, MatrixD worldTrans)
        {
            if (Vector3.Dot(velocity, (Vector3) worldTrans.Forward) >= -0.1f)
            {
                velocity.Normalize();
                Vector3D vectord = Vector3D.TransformNormal(Vector3D.Reject(velocity, worldTrans.Forward), MatrixD.Invert(worldTrans));
                Vector2 vector = (new Vector2((float) vectord.X, -((float) vectord.Y)) * 1200f) * this.m_maxScale;
                if (velocity.LengthSquared() < 9f)
                {
                    vector = new Vector2(0f, 0f);
                }
                MySprite sprite = new MySprite(SpriteType.TEXTURE, "AH_VelocityVector", new Vector2?(base.m_halfSize + vector), new Vector2?(new Vector2(50f, 50f) * this.m_maxScale), new Color?(base.m_foregroundColor), null, TextAlignment.CENTER, 0f);
                frame.Add(sprite);
            }
        }

        public override void Run()
        {
            base.Run();
            using (MySpriteDrawFrame frame = base.m_surface.DrawFrame())
            {
                if ((this.m_grid != null) && (this.m_grid.Physics != null))
                {
                    Matrix matrix;
                    base.m_block.Orientation.GetMatrix(out matrix);
                    this.m_ownerTransform = matrix * this.m_grid.PositionComp.WorldMatrix;
                    this.m_ownerTransform.Translation = base.m_block.GetPosition();
                    this.m_ownerTransform.Orthogonalize();
                    Vector3 gravity = MyGravityProviderSystem.CalculateNaturalGravityInPoint(this.m_ownerTransform.Translation);
                    if (gravity.LengthSquared() >= 0.0025)
                    {
                        this.DrawPlanetDisplay(frame, gravity, this.m_ownerTransform);
                    }
                    else
                    {
                        this.DrawSpaceDisplay(frame, this.m_ownerTransform);
                    }
                    this.m_tickCounter++;
                }
            }
        }

        public override ScriptUpdate NeedsUpdate =>
            ScriptUpdate.Update10;
    }
}

