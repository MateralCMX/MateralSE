namespace Sandbox.Game.GUI.HudViewers
{
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Gui;
    using VRage.Generics;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyHudMarkerRender : MyHudMarkerRenderBase
    {
        private static float m_friendAntennaRange = MyPerGameSettings.MaxAntennaDrawDistance;
        private static bool m_disableFading = false;
        private bool m_disableFadingToggle;
        private MyHudNotification m_signalModeNotification;
        private static float m_ownerAntennaRange = MyPerGameSettings.MaxAntennaDrawDistance;
        private static float m_enemyAntennaRange = MyPerGameSettings.MaxAntennaDrawDistance;
        private MyDynamicObjectPool<PointOfInterest> m_pointOfInterestPool;
        private List<PointOfInterest> m_pointsOfInterest;

        public MyHudMarkerRender(MyGuiScreenHudBase hudScreen) : base(hudScreen)
        {
            this.m_pointOfInterestPool = new MyDynamicObjectPool<PointOfInterest>(0x20);
            this.m_pointsOfInterest = new List<PointOfInterest>();
        }

        public void AddButtonMarker(Vector3D worldPosition, string name)
        {
            PointOfInterest item = this.m_pointOfInterestPool.Allocate();
            item.Reset();
            item.AlwaysVisible = true;
            item.SetState(worldPosition, PointOfInterest.PointOfInterestType.ButtonMarker, MyRelationsBetweenPlayerAndBlock.Owner);
            item.SetText(name);
            this.m_pointsOfInterest.Add(item);
        }

        public void AddEntity(MyEntity entity, MyRelationsBetweenPlayerAndBlock relationship, StringBuilder entityName, bool IsScenarioMarker = false)
        {
            if ((SignalDisplayMode != SignalMode.Off) && (entity != null))
            {
                Vector3D position = entity.PositionComp.GetPosition();
                PointOfInterest.PointOfInterestType unknownEntity = PointOfInterest.PointOfInterestType.UnknownEntity;
                if (entity is MyCharacter)
                {
                    if (ReferenceEquals(entity, MySession.Static.LocalCharacter))
                    {
                        return;
                    }
                    unknownEntity = PointOfInterest.PointOfInterestType.Character;
                    position += entity.WorldMatrix.Up * 1.2999999523162842;
                }
                else
                {
                    MyCubeBlock block = entity as MyCubeBlock;
                    if ((block != null) && (block.CubeGrid != null))
                    {
                        unknownEntity = (block.CubeGrid.GridSizeEnum != MyCubeSize.Small) ? (block.CubeGrid.IsStatic ? PointOfInterest.PointOfInterestType.StaticEntity : PointOfInterest.PointOfInterestType.LargeEntity) : PointOfInterest.PointOfInterestType.SmallEntity;
                    }
                }
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                if (IsScenarioMarker)
                {
                    unknownEntity = PointOfInterest.PointOfInterestType.Scenario;
                }
                item.SetState(position, unknownEntity, relationship);
                item.SetEntity(entity);
                item.SetText(entityName);
            }
        }

        public void AddGPS(MyGps gps)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.DefaultColor = gps.GPSColor;
                item.Reset();
                item.SetState(gps.Coords, gps.IsObjective ? PointOfInterest.PointOfInterestType.Objective : PointOfInterest.PointOfInterestType.GPS, MyRelationsBetweenPlayerAndBlock.Owner);
                if (string.IsNullOrEmpty(gps.DisplayName))
                {
                    item.SetText(gps.Name);
                }
                else
                {
                    item.SetText(gps.DisplayName);
                }
                item.AlwaysVisible = gps.AlwaysVisible;
                item.ContainerRemainingTime = gps.ContainerRemainingTime;
            }
        }

        public void AddHacking(Vector3D worldPosition, StringBuilder name)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                item.SetState(worldPosition, PointOfInterest.PointOfInterestType.Hack, MyRelationsBetweenPlayerAndBlock.Owner);
                item.SetText(name);
            }
        }

        public void AddOre(Vector3D worldPosition, string name)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                item.SetState(worldPosition, PointOfInterest.PointOfInterestType.Ore, MyRelationsBetweenPlayerAndBlock.NoOwnership);
                item.SetText(name);
            }
        }

        public void AddPOI(Vector3D worldPosition, StringBuilder name, MyRelationsBetweenPlayerAndBlock relationship)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                item.SetState(worldPosition, PointOfInterest.PointOfInterestType.GPS, relationship);
                item.SetText(name);
            }
        }

        public void AddProxyEntity(Vector3D worldPosition, MyRelationsBetweenPlayerAndBlock relationship, StringBuilder name)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                item.SetState(worldPosition, PointOfInterest.PointOfInterestType.UnknownEntity, relationship);
                item.SetText(name);
            }
        }

        public void AddTarget(Vector3D worldPosition)
        {
            if (SignalDisplayMode != SignalMode.Off)
            {
                PointOfInterest item = this.m_pointOfInterestPool.Allocate();
                this.m_pointsOfInterest.Add(item);
                item.Reset();
                item.SetState(worldPosition, PointOfInterest.PointOfInterestType.Target, MyRelationsBetweenPlayerAndBlock.Enemies);
            }
        }

        public static void AppendDistance(StringBuilder stringBuilder, double distance)
        {
            if (stringBuilder != null)
            {
                double num1 = Math.Abs(distance);
                distance = num1;
                if (distance > 9.460730473E+15)
                {
                    stringBuilder.AppendDecimal(Math.Round((double) (distance / 9.460730473E+15), 2), 2);
                    stringBuilder.Append("ly");
                }
                else if (distance > 299792458.00013667)
                {
                    stringBuilder.AppendDecimal(Math.Round((double) (distance / 299792458.00013667), 2), 2);
                    stringBuilder.Append("ls");
                }
                else if (distance <= 1000.0)
                {
                    stringBuilder.AppendDecimal(Math.Round(distance, 2), 1);
                    stringBuilder.Append("m");
                }
                else
                {
                    if (distance > 1000000.0)
                    {
                        stringBuilder.AppendDecimal(Math.Round((double) (distance / 1000.0), 2), 1);
                    }
                    else
                    {
                        stringBuilder.AppendDecimal(Math.Round((double) (distance / 1000.0), 2), 2);
                    }
                    stringBuilder.Append("km");
                }
            }
        }

        public static float Denormalize(float value) => 
            DenormalizeLog(value, 0.1f, MyPerGameSettings.MaxAntennaDrawDistance);

        private static float DenormalizeLog(float f, float min, float max) => 
            MathHelper.Clamp(MathHelper.InterpLog(f, min, max), min, max);

        public override void Draw()
        {
            int num;
            Vector3D position = MySector.MainCamera.Position;
            List<PointOfInterest> list = new List<PointOfInterest>();
            if (SignalDisplayMode != SignalMode.FullDisplay)
            {
                num = 0;
            }
            else
            {
                list.AddRange(this.m_pointsOfInterest);
                goto TR_0023;
            }
            goto TR_003E;
        TR_0023:
            list.Sort((a, b) => b.DistanceToCam.CompareTo(a.DistanceToCam));
            List<Vector2D> list2 = new List<Vector2D>(list.Count);
            List<Vector2> list3 = new List<Vector2>(list.Count);
            List<bool> list4 = new List<bool>(list.Count);
            if (!m_disableFading && (SignalDisplayMode != SignalMode.FullDisplay))
            {
                int num3 = list.Count - 1;
                while (num3 >= 0)
                {
                    Vector3D worldPosition = list[num3].WorldPosition;
                    worldPosition = MySector.MainCamera.WorldToScreen(ref worldPosition);
                    Vector2D item = new Vector2D(worldPosition.X, worldPosition.Y);
                    MatrixD cameraMatrix = CameraMatrix;
                    bool flag = Vector3D.Dot(list[num3].WorldPosition - CameraMatrix.Translation, cameraMatrix.Forward) < 0.0;
                    float maxValue = float.MaxValue;
                    int num7 = 0;
                    while (true)
                    {
                        if (num7 >= list2.Count)
                        {
                            float num5;
                            float num6;
                            if (maxValue > 0.022f)
                            {
                                num5 = 1f;
                                num6 = 1f;
                            }
                            else if (maxValue > 0.011f)
                            {
                                num5 = (81.81f * maxValue) - 0.8f;
                                num6 = (90f * maxValue) - 0.98f;
                            }
                            else
                            {
                                num5 = 0.1f;
                                num6 = 0.01f;
                            }
                            list2.Add(item);
                            list3.Add(new Vector2(num5, num6));
                            list4.Add(flag);
                            num3--;
                            break;
                        }
                        if (flag == list4[num7])
                        {
                            float num8 = (float) (list2[num7] - item).LengthSquared();
                            if (num8 < maxValue)
                            {
                                maxValue = num8;
                            }
                        }
                        num7++;
                    }
                }
            }
            if (m_disableFading || (SignalDisplayMode == SignalMode.FullDisplay))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int count = list.Count;
                    list[i].Draw(this, 1f, 1f, (list[i].POIType == PointOfInterest.PointOfInterestType.Objective) ? ((float) 2) : ((float) 1), list[i].POIType != PointOfInterest.PointOfInterestType.Objective);
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int num11 = (list.Count - i) - 1;
                    list[i].Draw(this, list3[num11].X, list3[num11].Y, (list[i].POIType == PointOfInterest.PointOfInterestType.Objective) ? ((float) 2) : ((float) 1), list[i].POIType != PointOfInterest.PointOfInterestType.Objective);
                }
            }
            foreach (PointOfInterest interest4 in this.m_pointsOfInterest)
            {
                interest4.Reset();
                this.m_pointOfInterestPool.Deallocate(interest4);
            }
            this.m_pointsOfInterest.Clear();
            return;
        TR_003E:
            while (true)
            {
                if (num < this.m_pointsOfInterest.Count)
                {
                    PointOfInterest item = this.m_pointsOfInterest[num];
                    PointOfInterest interest2 = null;
                    if (item.AlwaysVisible)
                    {
                        list.Add(item);
                    }
                    else
                    {
                        if (!item.AllowsCluster)
                        {
                            if ((item.POIType == PointOfInterest.PointOfInterestType.Target) && ((position - item.WorldPosition).Length() > 2000.0))
                            {
                                break;
                            }
                        }
                        else
                        {
                            int index = num + 1;
                            while (index < this.m_pointsOfInterest.Count)
                            {
                                PointOfInterest objA = this.m_pointsOfInterest[index];
                                if (ReferenceEquals(objA, item))
                                {
                                    index++;
                                    continue;
                                }
                                if (!objA.AllowsCluster)
                                {
                                    index++;
                                    continue;
                                }
                                if (!item.IsPOINearby(objA, position, 10.0))
                                {
                                    index++;
                                    continue;
                                }
                                if (interest2 == null)
                                {
                                    interest2 = this.m_pointOfInterestPool.Allocate();
                                    interest2.Reset();
                                    interest2.SetState(Vector3D.Zero, PointOfInterest.PointOfInterestType.Group, MyRelationsBetweenPlayerAndBlock.NoOwnership);
                                    interest2.AddPOI(item);
                                }
                                interest2.AddPOI(objA);
                                this.m_pointsOfInterest.RemoveAt(index);
                            }
                        }
                        if (interest2 != null)
                        {
                            list.Add(interest2);
                        }
                        else
                        {
                            list.Add(item);
                        }
                    }
                }
                else
                {
                    goto TR_0023;
                }
                break;
            }
            num++;
            goto TR_003E;
        }

        public override void DrawLocationMarkers(MyHudLocationMarkers locationMarkers)
        {
            float num = m_ownerAntennaRange * m_ownerAntennaRange;
            float num2 = m_friendAntennaRange * m_friendAntennaRange;
            float num3 = m_enemyAntennaRange * m_enemyAntennaRange;
            foreach (MyHudEntityParams @params in locationMarkers.MarkerEntities.Values)
            {
                if ((@params.ShouldDraw == null) || @params.ShouldDraw())
                {
                    double num4 = (@params.Position - GetDistanceMeasuringMatrix().Translation).LengthSquared();
                    MyRelationsBetweenPlayerAndBlock relationship = MyIDModule.GetRelation(@params.Owner, MySession.Static.LocalHumanPlayer.Identity.IdentityId, @params.Share, MyRelationsBetweenPlayerAndBlock.Enemies, MyRelationsBetweenFactions.Enemies, MyRelationsBetweenPlayerAndBlock.FactionShare);
                    switch (relationship)
                    {
                        case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        {
                            if (num4 <= num2)
                            {
                                break;
                            }
                            continue;
                        }
                        case MyRelationsBetweenPlayerAndBlock.Owner:
                        {
                            if (num4 <= num)
                            {
                                break;
                            }
                            continue;
                        }
                        case MyRelationsBetweenPlayerAndBlock.Neutral:
                        case MyRelationsBetweenPlayerAndBlock.Enemies:
                        {
                            if (num4 <= num3)
                            {
                                break;
                            }
                            continue;
                        }
                        default:
                            break;
                    }
                    MyEntity entity = @params.Entity as MyEntity;
                    if (entity != null)
                    {
                        this.AddEntity(entity, relationship, @params.Text, IsScenarioObjective(entity));
                    }
                    else
                    {
                        this.AddProxyEntity(@params.Position, relationship, @params.Text);
                    }
                }
            }
            base.m_hudScreen.DrawTexts();
        }

        public static MatrixD GetDistanceMeasuringMatrix()
        {
            MatrixD? controlledEntityMatrix = ControlledEntityMatrix;
            if ((controlledEntityMatrix == null) || (!MySession.Static.CameraOnCharacter && MySession.Static.IsCameraUserControlledSpectator()))
            {
                return CameraMatrix;
            }
            MatrixD? localCharacterMatrix = LocalCharacterMatrix;
            if (!MySession.Static.CameraOnCharacter || (localCharacterMatrix == null))
            {
                return controlledEntityMatrix.Value;
            }
            return localCharacterMatrix.Value;
        }

        private static bool IsScenarioObjective(MyEntity entity) => 
            ((entity != null) ? ((entity.Name != null) && ((entity.Name.Length >= 13) && entity.Name.Substring(0, 13).Equals("MissionStart_"))) : false);

        public static float Normalize(float value) => 
            NormalizeLog(value, 0.1f, MyPerGameSettings.MaxAntennaDrawDistance);

        private static float NormalizeLog(float f, float min, float max) => 
            MathHelper.Clamp(MathHelper.InterpLogInv(f, min, max), 0f, 1f);

        public override void Update()
        {
            m_disableFading = MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND);
            if ((!MyHud.IsHudMinimal && (!MyHud.MinimalHud && (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TOGGLE_SIGNALS) && !MyInput.Static.IsAnyCtrlKeyPressed()))) && (MyScreenManager.FocusedControl == null))
            {
                MyGuiAudio.PlaySound(MyGuiSounds.HudClick);
                SignalDisplayMode += 1;
                if (SignalDisplayMode >= SignalMode.MaxSignalModes)
                {
                    SignalDisplayMode = SignalMode.DefaultMode;
                }
                if (this.m_signalModeNotification != null)
                {
                    MyHud.Notifications.Remove(this.m_signalModeNotification);
                    this.m_signalModeNotification = null;
                }
                switch (SignalDisplayMode)
                {
                    case SignalMode.DefaultMode:
                        this.m_signalModeNotification = new MyHudNotification(MyCommonTexts.SignalMode_Switch_DefaultMode, 0x3e8, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        break;

                    case SignalMode.FullDisplay:
                        this.m_signalModeNotification = new MyHudNotification(MyCommonTexts.SignalMode_Switch_FullDisplay, 0x3e8, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        break;

                    case SignalMode.NoNames:
                        this.m_signalModeNotification = new MyHudNotification(MyCommonTexts.SignalMode_Switch_NoNames, 0x3e8, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        break;

                    case SignalMode.Off:
                        this.m_signalModeNotification = new MyHudNotification(MyCommonTexts.SignalMode_Switch_Off, 0x3e8, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
                        break;

                    default:
                        break;
                }
                if (this.m_signalModeNotification != null)
                {
                    MyHud.Notifications.Add(this.m_signalModeNotification);
                }
            }
        }

        public static SignalMode SignalDisplayMode
        {
            [CompilerGenerated]
            get => 
                <SignalDisplayMode>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<SignalDisplayMode>k__BackingField = value);
        }

        public static float FriendAntennaRange
        {
            get => 
                NormalizeLog(m_friendAntennaRange, 0.1f, MyPerGameSettings.MaxAntennaDrawDistance);
            set => 
                (m_friendAntennaRange = Denormalize(value));
        }

        public static float OwnerAntennaRange
        {
            get => 
                NormalizeLog(m_ownerAntennaRange, 0.1f, MyPerGameSettings.MaxAntennaDrawDistance);
            set => 
                (m_ownerAntennaRange = Denormalize(value));
        }

        public static float EnemyAntennaRange
        {
            get => 
                NormalizeLog(m_enemyAntennaRange, 0.1f, MyPerGameSettings.MaxAntennaDrawDistance);
            set => 
                (m_enemyAntennaRange = Denormalize(value));
        }

        private static MatrixD? ControlledEntityMatrix
        {
            get
            {
                if (MySession.Static.ControlledEntity != null)
                {
                    return new MatrixD?(MySession.Static.ControlledEntity.Entity.PositionComp.WorldMatrix);
                }
                return null;
            }
        }

        private static MatrixD? LocalCharacterMatrix
        {
            get
            {
                if (MySession.Static.LocalCharacter != null)
                {
                    return new MatrixD?(MySession.Static.LocalCharacter.WorldMatrix);
                }
                return null;
            }
        }

        private static MatrixD CameraMatrix =>
            MySector.MainCamera.WorldMatrix;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyHudMarkerRender.<>c <>9 = new MyHudMarkerRender.<>c();
            public static Comparison<MyHudMarkerRender.PointOfInterest> <>9__43_0;

            internal int <Draw>b__43_0(MyHudMarkerRender.PointOfInterest a, MyHudMarkerRender.PointOfInterest b) => 
                b.DistanceToCam.CompareTo(a.DistanceToCam);
        }

        private class PointOfInterest
        {
            public const double ClusterAngle = 10.0;
            public const int MaxTextLength = 0x40;
            public const double ClusterNearDistance = 3500.0;
            public const double ClusterScaleDistance = 20000.0;
            public const double MinimumTargetRange = 2000.0;
            public const double OreDistance = 200.0;
            private const double AngleConversion = 0.0087266462599716477;
            public Color DefaultColor = new Color(0x75, 0xc9, 0xf1);
            public List<MyHudMarkerRender.PointOfInterest> m_group = new List<MyHudMarkerRender.PointOfInterest>(10);
            private bool m_alwaysVisible;

            public PointOfInterest()
            {
                this.WorldPosition = Vector3D.Zero;
                this.POIType = PointOfInterestType.Unknown;
                this.Relationship = MyRelationsBetweenPlayerAndBlock.Owner;
                this.Text = new StringBuilder(0x40, 0x40);
            }

            public bool AddPOI(MyHudMarkerRender.PointOfInterest poi)
            {
                if (this.POIType != PointOfInterestType.Group)
                {
                    return false;
                }
                Vector3D vectord = this.WorldPosition * this.m_group.Count;
                this.m_group.Add(poi);
                this.Text.Clear();
                this.Text.Append(this.m_group.Count);
                switch (this.GetGroupRelation())
                {
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        this.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Signal_Mixed));
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Owner:
                        this.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Signal_Own));
                        break;

                    case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        this.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Signal_Friendly));
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Neutral:
                        this.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Signal_Neutral));
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Enemies:
                        this.Text.AppendStringBuilder(MyTexts.Get(MySpaceTexts.Signal_Enemy));
                        break;

                    default:
                        break;
                }
                vectord += poi.WorldPosition;
                this.WorldPosition = vectord / ((double) this.m_group.Count);
                this.Distance = (this.WorldPosition - MyHudMarkerRender.GetDistanceMeasuringMatrix().Translation).Length();
                this.DistanceToCam = (this.WorldPosition - MyHudMarkerRender.CameraMatrix.Translation).Length();
                if (poi.Relationship > this.Relationship)
                {
                    this.Relationship = poi.Relationship;
                }
                return true;
            }

            private int ComparePointOfInterest(MyHudMarkerRender.PointOfInterest poiA, MyHudMarkerRender.PointOfInterest poiB)
            {
                int num = this.IsPoiAtHighAlert(poiA).CompareTo(this.IsPoiAtHighAlert(poiB));
                if (num != 0)
                {
                    return num;
                }
                if ((poiA.POIType >= PointOfInterestType.UnknownEntity) && (poiB.POIType >= PointOfInterestType.UnknownEntity))
                {
                    int num2 = poiA.POIType.CompareTo(poiB.POIType);
                    if (num2 != 0)
                    {
                        return num2;
                    }
                }
                if (poiA.IsGrid() && poiB.IsGrid())
                {
                    MyCubeBlock entity = poiA.Entity as MyCubeBlock;
                    MyCubeBlock block2 = poiB.Entity as MyCubeBlock;
                    if ((entity != null) && (block2 != null))
                    {
                        int num3 = entity.CubeGrid.BlocksCount.CompareTo(block2.CubeGrid.BlocksCount);
                        if (num3 != 0)
                        {
                            return num3;
                        }
                    }
                }
                return poiB.Distance.CompareTo(poiA.Distance);
            }

            public unsafe void Draw(MyHudMarkerRender renderer, float alphaMultiplierMarker = 1f, float alphaMultiplierText = 1f, float scale = 1f, bool drawBox = true)
            {
                Vector2 zero = Vector2.Zero;
                bool isBehind = false;
                if (TryComputeScreenPoint(this.WorldPosition, out zero, out isBehind))
                {
                    Vector2 hudSize = MyGuiManager.GetHudSize();
                    Vector2 hudSizeHalf = MyGuiManager.GetHudSizeHalf();
                    float num = new Vector2((float) MyGuiManager.GetSafeFullscreenRectangle().Width, (float) MyGuiManager.GetSafeFullscreenRectangle().Height).Y / 1080f;
                    zero *= hudSize;
                    Color white = Color.White;
                    Color fontColor = Color.White;
                    string font = "White";
                    this.GetPOIColorAndFontInformation(out white, out fontColor, out font);
                    Vector2 vector4 = zero - hudSizeHalf;
                    Vector3D vectord = Vector3D.Transform(this.WorldPosition, MySector.MainCamera.ViewMatrix);
                    float num2 = 0.04f;
                    if (((zero.X >= num2) && ((zero.X <= (hudSize.X - num2)) && ((zero.Y >= num2) && (zero.Y <= (hudSize.Y - num2))))) && (vectord.Z <= 0.0))
                    {
                        float halfWidth = ((scale * 0.006667f) / num) / num;
                        if (this.POIType == PointOfInterestType.Target)
                        {
                            renderer.AddTexturedQuad(MyHudTexturesEnum.TargetTurret, zero, -Vector2.UnitY, Color.White, halfWidth, halfWidth);
                            return;
                        }
                        if (drawBox)
                        {
                            renderer.AddTexturedQuad(MyHudTexturesEnum.Target_neutral, zero, -Vector2.UnitY, white, halfWidth, halfWidth);
                        }
                    }
                    else
                    {
                        if (this.POIType == PointOfInterestType.Target)
                        {
                            return;
                        }
                        zero = hudSizeHalf + ((hudSizeHalf * Vector2.Normalize(vector4)) * 0.77f);
                        vector4 = zero - hudSizeHalf;
                        if (vector4.LengthSquared() > 9.999999E-11f)
                        {
                            vector4.Normalize();
                        }
                        else
                        {
                            vector4 = new Vector2(1f, 0f);
                        }
                        float halfWidth = (0.0053336f / num) / num;
                        renderer.AddTexturedQuad(MyHudTexturesEnum.DirectionIndicator, zero, vector4, white, halfWidth, halfWidth);
                        zero -= (vector4 * 0.006667f) * 2f;
                    }
                    float num3 = 0.03f;
                    float num4 = 0.07f;
                    float num5 = 0.15f;
                    int num6 = 0;
                    float val = 1f;
                    float num8 = 1f;
                    float num9 = vector4.Length();
                    if (num9 <= num3)
                    {
                        val = 1f;
                        num8 = 1f;
                        num6 = 0;
                    }
                    else if ((num9 > num3) && (num9 < num4))
                    {
                        float num15 = num5 - num3;
                        val = 1f - ((num9 - num3) / num15);
                        val *= val;
                        num15 = num4 - num3;
                        num8 = 1f - ((num9 - num3) / num15);
                        num8 *= num8;
                        num6 = 1;
                    }
                    else if ((num9 < num4) || (num9 >= num5))
                    {
                        val = 0f;
                        num8 = 0f;
                        num6 = 2;
                    }
                    else
                    {
                        float num16 = num5 - num3;
                        val = 1f - ((num9 - num3) / num16);
                        val *= val;
                        num16 = num5 - num4;
                        num8 = 1f - ((num9 - num4) / num16);
                        num8 *= num8;
                        num6 = 2;
                    }
                    float num10 = MathHelper.Clamp((float) ((num9 - 0.2f) / 0.5f), (float) 0f, (float) 1f);
                    val = MyMath.Clamp(val, 0f, 1f);
                    if ((MyHudMarkerRender.m_disableFading || (MyHudMarkerRender.SignalDisplayMode == MyHudMarkerRender.SignalMode.FullDisplay)) || this.AlwaysVisible)
                    {
                        val = 1f;
                        num8 = 1f;
                        num10 = 1f;
                        num6 = 0;
                    }
                    Vector2 vector5 = new Vector2(0f, ((scale * num) * 24f) / ((float) MyGuiManager.GetFullscreenRectangle().Width));
                    if (((((MyHudMarkerRender.SignalDisplayMode != MyHudMarkerRender.SignalMode.NoNames) || ((this.POIType == PointOfInterestType.ButtonMarker) || MyHudMarkerRender.m_disableFading)) || this.AlwaysVisible) && (val > float.Epsilon)) && (this.Text.Length > 0))
                    {
                        MyHudText text2 = renderer.m_hudScreen.AllocateText();
                        if (text2 != null)
                        {
                            fontColor.A = (byte) ((255f * alphaMultiplierText) * val);
                            text2.Start(font, zero - vector5, fontColor, 0.7f / num, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                            text2.Append(this.Text);
                        }
                    }
                    MyHudText text = null;
                    if (this.POIType != PointOfInterestType.Group)
                    {
                        white.A = (byte) ((255f * alphaMultiplierMarker) * num10);
                        DrawIcon(renderer, this.POIType, this.Relationship, zero, white, scale);
                        white.A = white.A;
                        text = renderer.m_hudScreen.AllocateText();
                        if (text != null)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            MyHudMarkerRender.AppendDistance(stringBuilder, this.Distance);
                            fontColor.A = (byte) (alphaMultiplierText * 255f);
                            text.Start(font, zero + (vector5 * (0.7f + (0.3f * val))), fontColor, (0.5f + (0.2f * val)) / num, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                            text.Append(stringBuilder);
                        }
                        if (!string.IsNullOrEmpty(this.ContainerRemainingTime))
                        {
                            MyHudText text1 = renderer.m_hudScreen.AllocateText();
                            text1.Start(font, zero + (vector5 * (1.6f + (0.3f * val))), fontColor, (0.5f + (0.2f * val)) / num, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                            text1.Append(this.ContainerRemainingTime);
                        }
                    }
                    else
                    {
                        Dictionary<MyRelationsBetweenPlayerAndBlock, List<MyHudMarkerRender.PointOfInterest>> significantGroupPOIs = this.GetSignificantGroupPOIs();
                        Vector2[] vectorArray = new Vector2[] { new Vector2(-6f, -4f), new Vector2(6f, -4f), new Vector2(-6f, 4f), new Vector2(6f, 4f), new Vector2(0f, 12f) };
                        Vector2[] vectorArray2 = new Vector2[] { new Vector2(16f, -4f), new Vector2(16f, 4f), new Vector2(16f, 12f), new Vector2(16f, 20f), new Vector2(16f, 28f) };
                        for (int i = 0; i < vectorArray.Length; i++)
                        {
                            float num19 = (num6 < 2) ? 1f : num8;
                            float y = vectorArray[i].Y;
                            vectorArray[i].X = (vectorArray[i].X + (22f * num19)) / ((float) MyGuiManager.GetFullscreenRectangle().Width);
                            vectorArray[i].Y = (y / 1080f) / num;
                            if (MyVideoSettingsManager.IsTripleHead())
                            {
                                float* singlePtr1 = (float*) ref vectorArray[i].X;
                                singlePtr1[0] /= 0.33f;
                            }
                            if (vectorArray[i].Y <= float.Epsilon)
                            {
                                vectorArray[i].Y = y / 1080f;
                            }
                            y = vectorArray2[i].Y;
                            vectorArray2[i].X = (vectorArray2[i].X / ((float) MyGuiManager.GetFullscreenRectangle().Width)) / num;
                            vectorArray2[i].Y = (y / 1080f) / num;
                            if (MyVideoSettingsManager.IsTripleHead())
                            {
                                float* singlePtr2 = (float*) ref vectorArray2[i].X;
                                singlePtr2[0] /= 0.33f;
                            }
                            if (vectorArray2[i].Y <= float.Epsilon)
                            {
                                vectorArray2[i].Y = y / 1080f;
                            }
                        }
                        int index = 0;
                        if (significantGroupPOIs.Count > 1)
                        {
                            foreach (MyRelationsBetweenPlayerAndBlock block in new MyRelationsBetweenPlayerAndBlock[] { MyRelationsBetweenPlayerAndBlock.Owner, MyRelationsBetweenPlayerAndBlock.FactionShare, MyRelationsBetweenPlayerAndBlock.Neutral, MyRelationsBetweenPlayerAndBlock.Enemies })
                            {
                                if (significantGroupPOIs.ContainsKey(block))
                                {
                                    List<MyHudMarkerRender.PointOfInterest> list = significantGroupPOIs[block];
                                    if (list.Count != 0)
                                    {
                                        MyHudMarkerRender.PointOfInterest poi = list[0];
                                        if (poi != null)
                                        {
                                            this.GetColorAndFontForRelationship(block, out white, out fontColor, out font);
                                            float num22 = (num6 == 0) ? 1f : num8;
                                            if (num6 >= 2)
                                            {
                                                num22 = 0f;
                                            }
                                            Vector2 vector9 = Vector2.Lerp(vectorArray[index], vectorArray2[index], num22);
                                            string iconForRelationship = GetIconForRelationship(block);
                                            Color* colorPtr1 = (Color*) ref white;
                                            colorPtr1.A = (byte) (alphaMultiplierMarker * white.A);
                                            DrawIcon(renderer, iconForRelationship, zero + vector9, white, 0.75f / num);
                                            if (this.IsPoiAtHighAlert(poi))
                                            {
                                                Color markerColor = Color.White;
                                                markerColor.A = (byte) (alphaMultiplierMarker * 255f);
                                                DrawIcon(renderer, @"Textures\HUD\marker_alert.dds", zero + vector9, markerColor, 0.75f / num);
                                            }
                                            if ((((MyHudMarkerRender.SignalDisplayMode != MyHudMarkerRender.SignalMode.NoNames) || MyHudMarkerRender.m_disableFading) || this.AlwaysVisible) && (poi.Text.Length > 0))
                                            {
                                                MyHudText text3 = renderer.m_hudScreen.AllocateText();
                                                if (text3 != null)
                                                {
                                                    float num23 = 1f;
                                                    if (num6 == 1)
                                                    {
                                                        num23 = num8;
                                                    }
                                                    else if (num6 > 1)
                                                    {
                                                        num23 = 0f;
                                                    }
                                                    fontColor.A = (byte) ((255f * alphaMultiplierText) * num23);
                                                    Vector2 vector10 = new Vector2(8f / ((float) MyGuiManager.GetFullscreenRectangle().Width), 0f);
                                                    float* singlePtr3 = (float*) ref vector10.X;
                                                    singlePtr3[0] /= num;
                                                    text3.Start(font, (zero + vector9) + vector10, fontColor, 0.55f / num, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                                                    text3.Append(poi.Text);
                                                }
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<MyRelationsBetweenPlayerAndBlock, List<MyHudMarkerRender.PointOfInterest>> pair in significantGroupPOIs)
                            {
                                MyRelationsBetweenPlayerAndBlock key = pair.Key;
                                if (significantGroupPOIs.ContainsKey(key))
                                {
                                    List<MyHudMarkerRender.PointOfInterest> list2 = pair.Value;
                                    for (int j = 0; (j < 4) && (j < list2.Count); j++)
                                    {
                                        MyHudMarkerRender.PointOfInterest poi = list2[j];
                                        if (poi != null)
                                        {
                                            if (poi.POIType == PointOfInterestType.Scenario)
                                            {
                                                poi.GetPOIColorAndFontInformation(out white, out fontColor, out font);
                                            }
                                            else
                                            {
                                                this.GetColorAndFontForRelationship(key, out white, out fontColor, out font);
                                            }
                                            float num25 = (num6 == 0) ? 1f : num8;
                                            if (num6 >= 2)
                                            {
                                                num25 = 0f;
                                            }
                                            Vector2 vector11 = Vector2.Lerp(vectorArray[index], vectorArray2[index], num25);
                                            string centerIconSprite = (poi.POIType != PointOfInterestType.Scenario) ? GetIconForRelationship(key) : @"Textures\HUD\marker_scenario.dds";
                                            Color* colorPtr2 = (Color*) ref white;
                                            colorPtr2.A = (byte) (alphaMultiplierMarker * white.A);
                                            DrawIcon(renderer, centerIconSprite, zero + vector11, white, 0.75f / num);
                                            if (this.ShouldDrawHighAlertMark(poi))
                                            {
                                                Color markerColor = Color.White;
                                                markerColor.A = (byte) (alphaMultiplierMarker * 255f);
                                                DrawIcon(renderer, @"Textures\HUD\marker_alert.dds", zero + vector11, markerColor, 0.75f / num);
                                            }
                                            if ((((MyHudMarkerRender.SignalDisplayMode != MyHudMarkerRender.SignalMode.NoNames) || MyHudMarkerRender.m_disableFading) || this.AlwaysVisible) && (poi.Text.Length > 0))
                                            {
                                                MyHudText text4 = renderer.m_hudScreen.AllocateText();
                                                if (text4 != null)
                                                {
                                                    float num26 = 1f;
                                                    if (num6 == 1)
                                                    {
                                                        num26 = num8;
                                                    }
                                                    else if (num6 > 1)
                                                    {
                                                        num26 = 0f;
                                                    }
                                                    fontColor.A = (byte) ((255f * alphaMultiplierText) * num26);
                                                    Vector2 vector12 = new Vector2(8f / ((float) MyGuiManager.GetFullscreenRectangle().Width), 0f);
                                                    float* singlePtr4 = (float*) ref vector12.X;
                                                    singlePtr4[0] /= num;
                                                    text4.Start(font, (zero + vector11) + vector12, fontColor, 0.55f / num, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                                                    text4.Append(poi.Text);
                                                }
                                            }
                                            index++;
                                        }
                                    }
                                }
                            }
                        }
                        this.GetPOIColorAndFontInformation(out white, out fontColor, out font);
                        float amount = (num6 == 0) ? 1f : num8;
                        if (num6 >= 2)
                        {
                            amount = 0f;
                        }
                        Vector2 vector6 = Vector2.Lerp(vectorArray[4], vectorArray2[index], amount);
                        Vector2 vector7 = Vector2.Lerp(Vector2.Zero, new Vector2(0.02222222f / num, 0.003703704f / num), amount);
                        text = renderer.m_hudScreen.AllocateText();
                        if (text != null)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            MyHudMarkerRender.AppendDistance(stringBuilder, this.Distance);
                            fontColor.A = (byte) (alphaMultiplierText * 255f);
                            text.Start(font, (zero + vector6) + vector7, fontColor, (0.5f + (0.2f * val)) / num, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                            text.Append(stringBuilder);
                        }
                    }
                }
            }

            private static void DrawIcon(MyHudMarkerRender renderer, string centerIconSprite, Vector2 screenPosition, Color markerColor, float sizeScale = 1f)
            {
                Vector2 vector = new Vector2(8f, 8f);
                vector *= sizeScale;
                renderer.AddTexturedQuad(centerIconSprite, screenPosition, -Vector2.UnitY, markerColor, vector.X, vector.Y);
            }

            private static void DrawIcon(MyHudMarkerRender renderer, PointOfInterestType poiType, MyRelationsBetweenPlayerAndBlock relationship, Vector2 screenPosition, Color markerColor, float sizeScale = 1f)
            {
                MyHudTexturesEnum corner = MyHudTexturesEnum.corner;
                string str = string.Empty;
                Vector2 vector = new Vector2(12f, 12f);
                switch (poiType)
                {
                    case PointOfInterestType.Unknown:
                    case PointOfInterestType.UnknownEntity:
                    case PointOfInterestType.Character:
                    case PointOfInterestType.SmallEntity:
                    case PointOfInterestType.LargeEntity:
                    case PointOfInterestType.StaticEntity:
                    {
                        string iconForRelationship = GetIconForRelationship(relationship);
                        DrawIcon(renderer, iconForRelationship, screenPosition, markerColor, sizeScale);
                        return;
                    }
                    case PointOfInterestType.Target:
                        corner = MyHudTexturesEnum.TargetTurret;
                        break;

                    case PointOfInterestType.Ore:
                        corner = MyHudTexturesEnum.HudOre;
                        break;

                    case PointOfInterestType.Hack:
                        corner = MyHudTexturesEnum.hit_confirmation;
                        break;

                    case PointOfInterestType.GPS:
                    case PointOfInterestType.Objective:
                    {
                        string centerIconSprite = @"Textures\HUD\marker_gps.dds";
                        DrawIcon(renderer, centerIconSprite, screenPosition, markerColor, sizeScale);
                        return;
                    }
                    case PointOfInterestType.Scenario:
                    {
                        string centerIconSprite = @"Textures\HUD\marker_scenario.dds";
                        DrawIcon(renderer, centerIconSprite, screenPosition, markerColor, sizeScale);
                        return;
                    }
                    default:
                        return;
                }
                if (!string.IsNullOrWhiteSpace(str))
                {
                    vector *= sizeScale;
                    renderer.AddTexturedQuad(str, screenPosition, -Vector2.UnitY, markerColor, vector.X, vector.Y);
                }
                else
                {
                    float halfWidth = 0.0053336f * sizeScale;
                    renderer.AddTexturedQuad(corner, screenPosition, -Vector2.UnitY, markerColor, halfWidth, halfWidth);
                }
            }

            public void GetColorAndFontForRelationship(MyRelationsBetweenPlayerAndBlock relationship, out Color color, out Color fontColor, out string font)
            {
                color = Color.White;
                fontColor = Color.White;
                font = "White";
                switch (relationship)
                {
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                    case MyRelationsBetweenPlayerAndBlock.Neutral:
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Owner:
                        color = new Color(0x75, 0xc9, 0xf1);
                        fontColor = new Color(0x75, 0xc9, 0xf1);
                        font = "Blue";
                        return;

                    case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        color = new Color(0x65, 0xb2, 90);
                        font = "Green";
                        return;

                    case MyRelationsBetweenPlayerAndBlock.Enemies:
                        color = new Color(0xe3, 0x3e, 0x3f);
                        font = "Red";
                        break;

                    default:
                        return;
                }
            }

            private MyRelationsBetweenPlayerAndBlock GetGroupRelation()
            {
                if ((this.m_group != null) && (this.m_group.Count != 0))
                {
                    MyRelationsBetweenPlayerAndBlock relationship = this.m_group[0].Relationship;
                    int num = 1;
                    while (true)
                    {
                        if (num >= this.m_group.Count)
                        {
                            return ((relationship != MyRelationsBetweenPlayerAndBlock.NoOwnership) ? relationship : MyRelationsBetweenPlayerAndBlock.Neutral);
                        }
                        if (this.m_group[num].Relationship != relationship)
                        {
                            if ((relationship == MyRelationsBetweenPlayerAndBlock.Owner) && (this.m_group[num].Relationship == MyRelationsBetweenPlayerAndBlock.FactionShare))
                            {
                                relationship = MyRelationsBetweenPlayerAndBlock.FactionShare;
                            }
                            else
                            {
                                if (relationship != MyRelationsBetweenPlayerAndBlock.FactionShare)
                                {
                                    break;
                                }
                                if (this.m_group[num].Relationship != MyRelationsBetweenPlayerAndBlock.Owner)
                                {
                                    break;
                                }
                                relationship = MyRelationsBetweenPlayerAndBlock.FactionShare;
                            }
                        }
                        num++;
                    }
                }
                return MyRelationsBetweenPlayerAndBlock.NoOwnership;
            }

            public static string GetIconForRelationship(MyRelationsBetweenPlayerAndBlock relationship)
            {
                string str = string.Empty;
                switch (relationship)
                {
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership:
                    case MyRelationsBetweenPlayerAndBlock.Neutral:
                        str = @"Textures\HUD\marker_neutral.dds";
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Owner:
                        str = @"Textures\HUD\marker_self.dds";
                        break;

                    case MyRelationsBetweenPlayerAndBlock.FactionShare:
                        str = @"Textures\HUD\marker_friendly.dds";
                        break;

                    case MyRelationsBetweenPlayerAndBlock.Enemies:
                        str = @"Textures\HUD\marker_enemy.dds";
                        break;

                    default:
                        break;
                }
                return str;
            }

            public void GetPOIColorAndFontInformation(out Color poiColor, out Color fontColor, out string font)
            {
                poiColor = Color.White;
                fontColor = Color.White;
                font = "White";
                PointOfInterestType pOIType = this.POIType;
                switch (pOIType)
                {
                    case PointOfInterestType.Unknown:
                    case PointOfInterestType.Ore:
                        poiColor = Color.White;
                        font = "White";
                        fontColor = Color.White;
                        return;

                    case PointOfInterestType.Target:
                        break;

                    case PointOfInterestType.Group:
                    {
                        MyRelationsBetweenPlayerAndBlock groupRelation = this.GetGroupRelation();
                        this.GetColorAndFontForRelationship(groupRelation, out poiColor, out fontColor, out font);
                        return;
                    }
                    default:
                        switch (pOIType)
                        {
                            case PointOfInterestType.GPS:
                                poiColor = this.DefaultColor;
                                fontColor = this.DefaultColor;
                                font = "Blue";
                                return;

                            case PointOfInterestType.Objective:
                                poiColor = this.DefaultColor * 1.3f;
                                fontColor = this.DefaultColor * 1.3f;
                                font = "Blue";
                                return;

                            case PointOfInterestType.Scenario:
                                poiColor = Color.DarkOrange;
                                fontColor = Color.DarkOrange;
                                font = "White";
                                return;

                            default:
                                break;
                        }
                        break;
                }
                this.GetColorAndFontForRelationship(this.Relationship, out poiColor, out fontColor, out font);
            }

            private Dictionary<MyRelationsBetweenPlayerAndBlock, List<MyHudMarkerRender.PointOfInterest>> GetSignificantGroupPOIs()
            {
                Dictionary<MyRelationsBetweenPlayerAndBlock, List<MyHudMarkerRender.PointOfInterest>> dictionary = new Dictionary<MyRelationsBetweenPlayerAndBlock, List<MyHudMarkerRender.PointOfInterest>>();
                if ((this.m_group == null) || (this.m_group.Count == 0))
                {
                    return dictionary;
                }
                bool flag = true;
                MyRelationsBetweenPlayerAndBlock relationship = this.m_group[0].Relationship;
                int num = 1;
                while (true)
                {
                    if (num < this.m_group.Count)
                    {
                        if (this.m_group[num].Relationship == relationship)
                        {
                            num++;
                            continue;
                        }
                        flag = false;
                    }
                    if (flag)
                    {
                        this.m_group.Sort(new Comparison<MyHudMarkerRender.PointOfInterest>(this.ComparePointOfInterest));
                        dictionary[relationship] = new List<MyHudMarkerRender.PointOfInterest>();
                        for (int i = this.m_group.Count - 1; i >= 0; i--)
                        {
                            dictionary[relationship].Add(this.m_group[i]);
                            if (dictionary[relationship].Count >= 4)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.m_group.Count; i++)
                        {
                            MyHudMarkerRender.PointOfInterest item = this.m_group[i];
                            relationship = item.Relationship;
                            if (relationship == MyRelationsBetweenPlayerAndBlock.NoOwnership)
                            {
                                relationship = MyRelationsBetweenPlayerAndBlock.Neutral;
                            }
                            if (!dictionary.ContainsKey(relationship))
                            {
                                dictionary[relationship] = new List<MyHudMarkerRender.PointOfInterest>();
                                dictionary[relationship].Add(item);
                            }
                            else if (this.ComparePointOfInterest(item, dictionary[relationship][0]) > 0)
                            {
                                dictionary[relationship].Clear();
                                dictionary[relationship].Add(item);
                            }
                        }
                    }
                    return dictionary;
                }
            }

            private bool IsGrid() => 
                ((this.POIType == PointOfInterestType.SmallEntity) || ((this.POIType == PointOfInterestType.LargeEntity) || (this.POIType == PointOfInterestType.StaticEntity)));

            private bool IsPoiAtHighAlert(MyHudMarkerRender.PointOfInterest poi)
            {
                if (poi.Relationship != MyRelationsBetweenPlayerAndBlock.Neutral)
                {
                    if (poi.POIType == PointOfInterestType.Scenario)
                    {
                        return true;
                    }
                    using (List<MyHudMarkerRender.PointOfInterest>.Enumerator enumerator = this.m_group.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyHudMarkerRender.PointOfInterest current = enumerator.Current;
                            if (this.IsRelationHostile(poi.Relationship, current.Relationship) && ((current.WorldPosition - poi.WorldPosition).LengthSquared() < 1000000f))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public bool IsPOINearby(MyHudMarkerRender.PointOfInterest poi, Vector3D cameraPosition, double angle = 10.0)
            {
                Vector3D vectord = (Vector3D) (0.5 * (this.WorldPosition - poi.WorldPosition));
                double num = (cameraPosition - (poi.WorldPosition + vectord)).Length();
                double num1 = Math.Sin(angle * 0.0087266462599716477) * num;
                return (vectord.LengthSquared() <= (num1 * num1));
            }

            private bool IsRelationHostile(MyRelationsBetweenPlayerAndBlock relationshipA, MyRelationsBetweenPlayerAndBlock relationshipB)
            {
                if ((relationshipA == MyRelationsBetweenPlayerAndBlock.Owner) || (relationshipA == MyRelationsBetweenPlayerAndBlock.FactionShare))
                {
                    return (relationshipB == MyRelationsBetweenPlayerAndBlock.Enemies);
                }
                return (((relationshipB == MyRelationsBetweenPlayerAndBlock.Owner) || (relationshipB == MyRelationsBetweenPlayerAndBlock.FactionShare)) && (relationshipA == MyRelationsBetweenPlayerAndBlock.Enemies));
            }

            public void Reset()
            {
                this.WorldPosition = Vector3D.Zero;
                this.POIType = PointOfInterestType.Unknown;
                this.Relationship = MyRelationsBetweenPlayerAndBlock.Owner;
                this.Entity = null;
                this.Text.Clear();
                this.m_group.Clear();
                this.Distance = 0.0;
                this.DistanceToCam = 0.0;
                this.AlwaysVisible = false;
                this.ContainerRemainingTime = null;
            }

            public void SetEntity(MyEntity entity)
            {
                this.Entity = entity;
            }

            public void SetState(Vector3D position, PointOfInterestType type, MyRelationsBetweenPlayerAndBlock relationship)
            {
                this.WorldPosition = position;
                this.POIType = type;
                this.Relationship = relationship;
                this.Distance = (position - MyHudMarkerRender.GetDistanceMeasuringMatrix().Translation).Length();
                this.DistanceToCam = (this.WorldPosition - MyHudMarkerRender.CameraMatrix.Translation).Length();
            }

            public void SetText(string text)
            {
                this.Text.Clear();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    this.Text.Append(text, 0, Math.Min(text.Length, 0x40));
                }
            }

            public void SetText(StringBuilder text)
            {
                this.Text.Clear();
                if (text != null)
                {
                    this.Text.AppendSubstring(text, 0, Math.Min(text.Length, 0x40));
                }
            }

            private bool ShouldDrawHighAlertMark(MyHudMarkerRender.PointOfInterest poi) => 
                ((poi.POIType != PointOfInterestType.Scenario) && this.IsPoiAtHighAlert(poi));

            public override string ToString()
            {
                object[] objArray1 = new object[] { this.POIType.ToString(), ": ", this.Text, " (", this.Distance, ")" };
                return string.Concat(objArray1);
            }

            public static unsafe bool TryComputeScreenPoint(Vector3D worldPosition, out Vector2 projectedPoint2D, out bool isBehind)
            {
                Vector3D position = Vector3D.Transform(worldPosition, MySector.MainCamera.ViewMatrix);
                Vector4D vectord = Vector4D.Transform(position, MySector.MainCamera.ProjectionMatrix);
                if (position.Z > 0.0)
                {
                    double* numPtr1 = (double*) ref vectord.X;
                    numPtr1[0] *= -1.0;
                    double* numPtr2 = (double*) ref vectord.Y;
                    numPtr2[0] *= -1.0;
                }
                if (vectord.W == 0.0)
                {
                    projectedPoint2D = Vector2.Zero;
                    isBehind = false;
                    return false;
                }
                projectedPoint2D = new Vector2(((float) ((vectord.X / vectord.W) / 2.0)) + 0.5f, (((float) (-vectord.Y / vectord.W)) / 2f) + 0.5f);
                if (MyVideoSettingsManager.IsTripleHead())
                {
                    projectedPoint2D.X = (projectedPoint2D.X - 0.3333333f) / 0.3333333f;
                }
                Vector3D vectord2 = worldPosition - MyHudMarkerRender.CameraMatrix.Translation;
                vectord2.Normalize();
                double num = Vector3D.Dot(MySector.MainCamera.ForwardVector, vectord2);
                isBehind = num < 0.0;
                return true;
            }

            public Vector3D WorldPosition { get; private set; }

            public PointOfInterestType POIType { get; private set; }

            public MyRelationsBetweenPlayerAndBlock Relationship { get; private set; }

            public MyEntity Entity { get; private set; }

            public StringBuilder Text { get; private set; }

            public double Distance { get; private set; }

            public double DistanceToCam { get; private set; }

            public string ContainerRemainingTime { get; set; }

            public bool AlwaysVisible
            {
                get
                {
                    if ((this.POIType != PointOfInterestType.Ore) || (this.Distance >= 200.0))
                    {
                        return this.m_alwaysVisible;
                    }
                    return true;
                }
                set => 
                    (this.m_alwaysVisible = value);
            }

            public bool AllowsCluster =>
                (!this.AlwaysVisible ? ((this.POIType != PointOfInterestType.Target) ? ((this.POIType != PointOfInterestType.Ore) || (this.Distance >= 200.0)) : false) : false);

            public enum PointOfInterestState
            {
                NonDirectional,
                Directional
            }

            public enum PointOfInterestType
            {
                Unknown,
                Target,
                Group,
                Ore,
                Hack,
                UnknownEntity,
                Character,
                SmallEntity,
                LargeEntity,
                StaticEntity,
                GPS,
                ButtonMarker,
                Objective,
                Scenario
            }
        }

        public enum SignalMode
        {
            DefaultMode,
            FullDisplay,
            NoNames,
            Off,
            MaxSignalModes
        }
    }
}

