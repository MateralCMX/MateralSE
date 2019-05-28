namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Screens.DebugScreens;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;

    public class MyCubeBuilderGizmo
    {
        private MyGizmoSpaceProperties[] m_spaces = new MyGizmoSpaceProperties[8];
        public MyRotationOptionsEnum RotationOptions;
        public static MySymmetryAxisEnum CurrentBlockMirrorAxis;
        public static MySymmetryAxisEnum CurrentBlockMirrorOption;

        public MyCubeBuilderGizmo()
        {
            for (int i = 0; i < 8; i++)
            {
                this.m_spaces[i] = new MyGizmoSpaceProperties();
            }
            this.m_spaces[0].Enabled = true;
            this.m_spaces[1].SourceSpace = MyGizmoSpaceEnum.Default;
            this.m_spaces[1].SymmetryPlane = MySymmetrySettingModeEnum.NoPlane;
            this.m_spaces[1].SourceSpace = MyGizmoSpaceEnum.Default;
            this.m_spaces[1].SymmetryPlane = MySymmetrySettingModeEnum.XPlane;
            this.m_spaces[2].SourceSpace = MyGizmoSpaceEnum.Default;
            this.m_spaces[2].SymmetryPlane = MySymmetrySettingModeEnum.YPlane;
            this.m_spaces[3].SourceSpace = MyGizmoSpaceEnum.Default;
            this.m_spaces[3].SymmetryPlane = MySymmetrySettingModeEnum.ZPlane;
            this.m_spaces[4].SourceSpace = MyGizmoSpaceEnum.SymmetryX;
            this.m_spaces[4].SymmetryPlane = MySymmetrySettingModeEnum.YPlane;
            this.m_spaces[5].SourceSpace = MyGizmoSpaceEnum.SymmetryY;
            this.m_spaces[5].SymmetryPlane = MySymmetrySettingModeEnum.ZPlane;
            this.m_spaces[6].SourceSpace = MyGizmoSpaceEnum.SymmetryX;
            this.m_spaces[6].SymmetryPlane = MySymmetrySettingModeEnum.ZPlane;
            this.m_spaces[7].SourceSpace = MyGizmoSpaceEnum.SymmetryXZ;
            this.m_spaces[7].SymmetryPlane = MySymmetrySettingModeEnum.YPlane;
        }

        public unsafe void AddFastBuildParts(MyGizmoSpaceProperties gizmoSpace, MyCubeBlockDefinition cubeBlockDefinition, MyCubeGrid grid)
        {
            if (((cubeBlockDefinition != null) && (gizmoSpace.m_startBuild != null)) && (gizmoSpace.m_continueBuild != null))
            {
                Vector3I vectori = Vector3I.Min(gizmoSpace.m_startBuild.Value, gizmoSpace.m_continueBuild.Value);
                Vector3I vectori2 = Vector3I.Max(gizmoSpace.m_startBuild.Value, gizmoSpace.m_continueBuild.Value);
                Vector3I vectori3 = new Vector3I();
                int count = gizmoSpace.m_cubeMatricesTemp.Count;
                vectori3.X = vectori.X;
                while (vectori3.X <= vectori2.X)
                {
                    vectori3.Y = vectori.Y;
                    while (true)
                    {
                        if (vectori3.Y > vectori2.Y)
                        {
                            int* numPtr3 = (int*) ref vectori3.X;
                            numPtr3[0] += cubeBlockDefinition.Size.X;
                            break;
                        }
                        vectori3.Z = vectori.Z;
                        while (true)
                        {
                            if (vectori3.Z > vectori2.Z)
                            {
                                int* numPtr2 = (int*) ref vectori3.Y;
                                numPtr2[0] += cubeBlockDefinition.Size.Y;
                                break;
                            }
                            if ((vectori3 - gizmoSpace.m_startBuild.Value) != Vector3.Zero)
                            {
                                Vector3D vectord = (grid != null) ? Vector3D.Transform((Vector3) (vectori3 * grid.GridSize), grid.WorldMatrix) : ((Vector3D) (vectori3 * MyDefinitionManager.Static.GetCubeSize(cubeBlockDefinition.CubeSize)));
                                for (int i = 0; i < count; i++)
                                {
                                    gizmoSpace.m_cubeModelsTemp.Add(gizmoSpace.m_cubeModelsTemp[i]);
                                    MatrixD item = gizmoSpace.m_cubeMatricesTemp[i];
                                    item.Translation = vectord;
                                    gizmoSpace.m_cubeMatricesTemp.Add(item);
                                }
                            }
                            int* numPtr1 = (int*) ref vectori3.Z;
                            numPtr1[0] += cubeBlockDefinition.Size.Z;
                        }
                    }
                }
            }
        }

        private void AddGizmoCubeParts(MyGizmoSpaceProperties gizmoSpace, MyBlockBuilderRenderData renderData, ref MatrixD invGridWorldMatrix, MyCubeBlockDefinition definition)
        {
            Vector3UByte[] numArray = null;
            MyTileDefinition[] cubeTiles = null;
            MatrixD orientation = invGridWorldMatrix.GetOrientation();
            float range = 1f;
            if ((definition != null) && (definition.Skeleton != null))
            {
                cubeTiles = MyCubeGridDefinitions.GetCubeTiles(definition);
                range = MyDefinitionManager.Static.GetCubeSize(definition.CubeSize);
            }
            for (int i = 0; i < gizmoSpace.m_cubeModelsTemp.Count; i++)
            {
                string item = gizmoSpace.m_cubeModelsTemp[i];
                gizmoSpace.m_cubeModels.Add(item);
                gizmoSpace.m_cubeMatrices.Add(gizmoSpace.m_cubeMatricesTemp[i]);
                if (cubeTiles != null)
                {
                    int index = i % cubeTiles.Length;
                    MatrixD matrix = (Matrix.Transpose(cubeTiles[index].LocalMatrix) * gizmoSpace.m_cubeMatricesTemp[i].GetOrientation()) * orientation;
                    numArray = new Vector3UByte[9];
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= 9)
                        {
                            MyModel model = MyModels.GetModel(item);
                            if (model.BoneMapping != null)
                            {
                                int num5 = 0;
                                while (num5 < Math.Min(model.BoneMapping.Length, 9))
                                {
                                    Vector3I vectori = Vector3I.Round(Vector3.Transform(((Vector3) model.BoneMapping[num5]) - Vector3.One, cubeTiles[index].LocalMatrix) + Vector3.One);
                                    int num6 = 0;
                                    while (true)
                                    {
                                        if (num6 < definition.Skeleton.Count)
                                        {
                                            VRage.Game.BoneInfo info = definition.Skeleton[num6];
                                            if (info.BonePosition != vectori)
                                            {
                                                num6++;
                                                continue;
                                            }
                                            numArray[num5] = Vector3UByte.Normalize((Vector3) Vector3.Transform(Vector3UByte.Denormalize((Vector3UByte) info.BoneOffset, range), matrix), range);
                                        }
                                        num5++;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        numArray[num4] = new Vector3UByte(0x80, 0x80, 0x80);
                        num4++;
                    }
                }
                Vector3UByte[] bones = numArray;
                renderData.AddInstance(MyModel.GetId(item), gizmoSpace.m_cubeMatricesTemp[i], ref invGridWorldMatrix, MyPlayer.SelectedColor, bones, range);
            }
        }

        public void Clear()
        {
            MyGizmoSpaceProperties[] spaces = this.m_spaces;
            for (int i = 0; i < spaces.Length; i++)
            {
                spaces[i].Clear();
            }
        }

        public static bool DefaultGizmoCloseEnough(ref MatrixD invGridWorldMatrix, BoundingBoxD gizmoBox, float gridSize, float intersectionDistance)
        {
            MatrixD matrix = invGridWorldMatrix;
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter == null)
            {
                return false;
            }
            Vector3D position = MySector.MainCamera.Position;
            double num = (localCharacter.GetHeadMatrix(true, true, false, false, false).Translation - MySector.MainCamera.Position).Length();
            Vector3D translation = localCharacter.GetHeadMatrix(true, true, false, false, false).Translation;
            Vector3 point = (Vector3) Vector3D.Transform(translation, matrix);
            Vector3 to = (Vector3) Vector3D.Transform(position + (MySector.MainCamera.ForwardVector * (intersectionDistance + ((float) num))), matrix);
            LineD line = new LineD(Vector3D.Transform(position, matrix), to);
            float num2 = 0.025f * gridSize;
            gizmoBox.Inflate((double) num2);
            double maxValue = double.MaxValue;
            if (!gizmoBox.Intersects(ref line, out maxValue))
            {
                return false;
            }
            double num4 = gizmoBox.Distance(point);
            return (!(MySession.Static.ControlledEntity is MyShipController) ? ((MyCubeBuilder.Static.CubeBuilderState.CurrentBlockDefinition.CubeSize != MyCubeSize.Large) ? (num4 <= MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistSmallSurvivalCharacter) : (num4 <= MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistLargeSurvivalCharacter)) : ((MyCubeBuilder.Static.CubeBuilderState.CurrentBlockDefinition.CubeSize != MyCubeSize.Large) ? (num4 <= MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistSmallSurvivalShip) : (num4 <= MyBlockBuilderBase.CubeBuilderDefinition.BuildingDistLargeSurvivalShip)));
        }

        private unsafe void EnableGizmoSpace(MyGizmoSpaceEnum gizmoSpaceEnum, bool enable, Vector3I? planePos, bool isOdd, MyCubeBlockDefinition cubeBlockDefinition, MyCubeGrid cubeGrid)
        {
            MyGizmoSpaceProperties targetSpace = this.m_spaces[(int) gizmoSpaceEnum];
            targetSpace.Enabled = enable;
            if (enable)
            {
                if (planePos != null)
                {
                    targetSpace.SymmetryPlanePos = planePos.Value;
                }
                targetSpace.SymmetryIsOdd = isOdd;
                targetSpace.m_buildAllowed = false;
                if (cubeBlockDefinition != null)
                {
                    Vector3I vectori;
                    Vector3I vectori3;
                    Vector3I vectori4;
                    MyBlockOrientation orientation = new MyBlockOrientation(ref targetSpace.LocalOrientation);
                    MyCubeGridDefinitions.GetRotatedBlockSize(cubeBlockDefinition, ref targetSpace.m_localMatrixAdd, out vectori);
                    Vector3I center = cubeBlockDefinition.Center;
                    Vector3I.TransformNormal(ref center, ref targetSpace.m_localMatrixAdd, out vectori3);
                    Vector3I* vectoriPtr1 = (Vector3I*) new Vector3I((Math.Sign(vectori.X) == Math.Sign(targetSpace.m_addDir.X)) ? vectori3.X : (Math.Sign(targetSpace.m_addDir.X) * ((Math.Abs(vectori.X) - Math.Abs(vectori3.X)) - 1)), (Math.Sign(vectori.Y) == Math.Sign(targetSpace.m_addDir.Y)) ? vectori3.Y : (Math.Sign(targetSpace.m_addDir.Y) * ((Math.Abs(vectori.Y) - Math.Abs(vectori3.Y)) - 1)), (Math.Sign(vectori.Z) == Math.Sign(targetSpace.m_addDir.Z)) ? vectori3.Z : (Math.Sign(targetSpace.m_addDir.Z) * ((Math.Abs(vectori.Z) - Math.Abs(vectori3.Z)) - 1)));
                    targetSpace.m_positions.Clear();
                    targetSpace.m_positionsSmallOnLarge.Clear();
                    if (MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE && (targetSpace.m_addPosSmallOnLarge != null))
                    {
                        float num = MyDefinitionManager.Static.GetCubeSize(cubeBlockDefinition.CubeSize) / cubeGrid.GridSize;
                        targetSpace.m_minSmallOnLarge = Vector3.MaxValue;
                        targetSpace.m_maxSmallOnLarge = Vector3.MinValue;
                        vectoriPtr1 = (Vector3I*) ref vectori4;
                        targetSpace.m_centerPosSmallOnLarge = targetSpace.m_addPosSmallOnLarge.Value + (num * vectori4);
                        targetSpace.m_buildAllowed = true;
                        Vector3I vectori5 = new Vector3I {
                            X = 0
                        };
                        while (vectori5.X < cubeBlockDefinition.Size.X)
                        {
                            vectori5.Y = 0;
                            while (true)
                            {
                                if (vectori5.Y >= cubeBlockDefinition.Size.Y)
                                {
                                    int* numPtr3 = (int*) ref vectori5.X;
                                    numPtr3[0]++;
                                    break;
                                }
                                vectori5.Z = 0;
                                while (true)
                                {
                                    Vector3I vectori6;
                                    if (vectori5.Z >= cubeBlockDefinition.Size.Z)
                                    {
                                        int* numPtr2 = (int*) ref vectori5.Y;
                                        numPtr2[0]++;
                                        break;
                                    }
                                    Vector3I normal = vectori5 - center;
                                    Vector3I.TransformNormal(ref normal, ref targetSpace.m_localMatrixAdd, out vectori6);
                                    Vector3 vector = targetSpace.m_addPosSmallOnLarge.Value + (num * (vectori6 + vectori4));
                                    targetSpace.m_minSmallOnLarge = Vector3.Min(vector, targetSpace.m_minSmallOnLarge);
                                    targetSpace.m_maxSmallOnLarge = Vector3.Max(vector, targetSpace.m_maxSmallOnLarge);
                                    targetSpace.m_positionsSmallOnLarge.Add(vector);
                                    int* numPtr1 = (int*) ref vectori5.Z;
                                    numPtr1[0]++;
                                }
                            }
                        }
                    }
                    else
                    {
                        targetSpace.m_min = Vector3I.MaxValue;
                        targetSpace.m_max = Vector3I.MinValue;
                        targetSpace.m_centerPos = (Vector3I) (targetSpace.m_addPos + vectori4);
                        targetSpace.m_buildAllowed = true;
                        Vector3I vectori8 = new Vector3I {
                            X = 0
                        };
                        while (vectori8.X < cubeBlockDefinition.Size.X)
                        {
                            vectori8.Y = 0;
                            while (true)
                            {
                                if (vectori8.Y >= cubeBlockDefinition.Size.Y)
                                {
                                    int* numPtr6 = (int*) ref vectori8.X;
                                    numPtr6[0]++;
                                    break;
                                }
                                vectori8.Z = 0;
                                while (true)
                                {
                                    Vector3I vectori9;
                                    if (vectori8.Z >= cubeBlockDefinition.Size.Z)
                                    {
                                        int* numPtr5 = (int*) ref vectori8.Y;
                                        numPtr5[0]++;
                                        break;
                                    }
                                    Vector3I normal = vectori8 - center;
                                    Vector3I.TransformNormal(ref normal, ref targetSpace.m_localMatrixAdd, out vectori9);
                                    Vector3I vectori11 = (Vector3I) ((targetSpace.m_addPos + vectori9) + vectori4);
                                    targetSpace.m_min = Vector3I.Min(vectori11, targetSpace.m_min);
                                    targetSpace.m_max = Vector3I.Max(vectori11, targetSpace.m_max);
                                    if (((cubeGrid != null) && (cubeBlockDefinition.CubeSize == cubeGrid.GridSizeEnum)) && !cubeGrid.CanAddCube(vectori11, new MyBlockOrientation?(orientation), cubeBlockDefinition, false))
                                    {
                                        targetSpace.m_buildAllowed = false;
                                    }
                                    targetSpace.m_positions.Add(vectori11);
                                    int* numPtr4 = (int*) ref vectori8.Z;
                                    numPtr4[0]++;
                                }
                            }
                        }
                    }
                }
                if (targetSpace.SymmetryPlane != MySymmetrySettingModeEnum.Disabled)
                {
                    this.MirrorGizmoSpace(targetSpace, this.m_spaces[(int) targetSpace.SourceSpace], targetSpace.SymmetryPlane, planePos.Value, isOdd, cubeBlockDefinition, cubeGrid);
                }
            }
        }

        public void EnableGizmoSpaces(MyCubeBlockDefinition cubeBlockDefinition, MyCubeGrid cubeGrid, bool useSymmetry)
        {
            Vector3I? planePos = null;
            this.EnableGizmoSpace(MyGizmoSpaceEnum.Default, true, planePos, false, cubeBlockDefinition, cubeGrid);
            if (cubeGrid == null)
            {
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryX, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryY, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryZ, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryXY, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryYZ, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryXZ, false, planePos, false, cubeBlockDefinition, null);
                planePos = null;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryXYZ, false, planePos, false, cubeBlockDefinition, null);
            }
            else
            {
                int num1;
                int num2;
                int num3;
                int num4;
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryX, useSymmetry && (cubeGrid.XSymmetryPlane != null), cubeGrid.XSymmetryPlane, cubeGrid.XSymmetryOdd, cubeBlockDefinition, cubeGrid);
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryY, useSymmetry && (cubeGrid.YSymmetryPlane != null), cubeGrid.YSymmetryPlane, cubeGrid.YSymmetryOdd, cubeBlockDefinition, cubeGrid);
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryZ, useSymmetry && (cubeGrid.ZSymmetryPlane != null), cubeGrid.ZSymmetryPlane, cubeGrid.ZSymmetryOdd, cubeBlockDefinition, cubeGrid);
                if (!useSymmetry || (cubeGrid.XSymmetryPlane == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) (cubeGrid.YSymmetryPlane != null);
                }
                4.EnableGizmoSpace((MyGizmoSpaceEnum) this, (bool) num1, cubeGrid.YSymmetryPlane, cubeGrid.YSymmetryOdd, cubeBlockDefinition, cubeGrid);
                if (!useSymmetry || (cubeGrid.YSymmetryPlane == null))
                {
                    num2 = 0;
                }
                else
                {
                    num2 = (int) (cubeGrid.ZSymmetryPlane != null);
                }
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryYZ, (bool) num2, cubeGrid.ZSymmetryPlane, cubeGrid.ZSymmetryOdd, cubeBlockDefinition, cubeGrid);
                if (!useSymmetry || (cubeGrid.XSymmetryPlane == null))
                {
                    num3 = 0;
                }
                else
                {
                    num3 = (int) (cubeGrid.ZSymmetryPlane != null);
                }
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryXZ, (bool) num3, cubeGrid.ZSymmetryPlane, cubeGrid.ZSymmetryOdd, cubeBlockDefinition, cubeGrid);
                if ((!useSymmetry || (cubeGrid.XSymmetryPlane == null)) || (cubeGrid.YSymmetryPlane == null))
                {
                    num4 = 0;
                }
                else
                {
                    num4 = (int) (cubeGrid.ZSymmetryPlane != null);
                }
                this.EnableGizmoSpace(MyGizmoSpaceEnum.SymmetryXYZ, (bool) num4, cubeGrid.YSymmetryPlane, cubeGrid.YSymmetryOdd, cubeBlockDefinition, cubeGrid);
            }
        }

        private void GetGizmoPointTestVariables(ref MatrixD invGridWorldMatrix, float gridSize, out BoundingBoxD bb, out MatrixD m, MyGizmoSpaceEnum gizmo, float inflate = 0f, bool onVoxel = false, bool dynamicMode = false)
        {
            m = invGridWorldMatrix * MatrixD.CreateScale((double) (1f / gridSize));
            MyGizmoSpaceProperties properties = this.m_spaces[(int) gizmo];
            if (dynamicMode)
            {
                m = invGridWorldMatrix;
                bb = new BoundingBoxD((Vector3D) ((-properties.m_blockDefinition.Size * gridSize) * 0.5f), (Vector3D) ((properties.m_blockDefinition.Size * gridSize) * 0.5f));
            }
            else if (onVoxel)
            {
                m = invGridWorldMatrix;
                Vector3D vectord = MyCubeGrid.StaticGlobalGrid_UGToWorld((Vector3D) properties.m_min, gridSize, MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter) - (Vector3D.Half * gridSize);
                Vector3D vectord2 = MyCubeGrid.StaticGlobalGrid_UGToWorld((Vector3D) properties.m_max, gridSize, MyBlockBuilderBase.CubeBuilderDefinition.BuildingSettings.StaticGridAlignToCenter) + (Vector3D.Half * gridSize);
                bb = new BoundingBoxD(vectord - new Vector3D((double) (inflate * gridSize)), vectord2 + new Vector3D((double) (inflate * gridSize)));
            }
            else if (!MyFakes.ENABLE_STATIC_SMALL_GRID_ON_LARGE || (properties.m_addPosSmallOnLarge == null))
            {
                Vector3 vector3 = ((Vector3) properties.m_min) - new Vector3(0.5f);
                Vector3 vector4 = properties.m_max + new Vector3(0.5f);
                bb = new BoundingBoxD(vector3 - new Vector3(inflate), vector4 + new Vector3(inflate));
            }
            else
            {
                float num = MyDefinitionManager.Static.GetCubeSize(properties.m_blockDefinition.CubeSize) / gridSize;
                Vector3 vector = properties.m_minSmallOnLarge - new Vector3(0.5f * num);
                Vector3 vector2 = properties.m_maxSmallOnLarge + new Vector3(0.5f * num);
                bb = new BoundingBoxD(vector - new Vector3(inflate), vector2 + new Vector3(inflate));
            }
        }

        private static unsafe Vector3I MirrorBlockByPlane(MySymmetrySettingModeEnum mirror, Vector3I mirrorPosition, bool isOdd, Vector3I sourcePosition)
        {
            Vector3I vectori = sourcePosition;
            if (mirror == MySymmetrySettingModeEnum.XPlane)
            {
                vectori = new Vector3I(mirrorPosition.X - (sourcePosition.X - mirrorPosition.X), sourcePosition.Y, sourcePosition.Z);
                if (isOdd)
                {
                    int* numPtr1 = (int*) ref vectori.X;
                    numPtr1[0]--;
                }
            }
            if (mirror == MySymmetrySettingModeEnum.YPlane)
            {
                vectori = new Vector3I(sourcePosition.X, mirrorPosition.Y - (sourcePosition.Y - mirrorPosition.Y), sourcePosition.Z);
                if (isOdd)
                {
                    int* numPtr2 = (int*) ref vectori.Y;
                    numPtr2[0]--;
                }
            }
            if (mirror == MySymmetrySettingModeEnum.ZPlane)
            {
                vectori = new Vector3I(sourcePosition.X, sourcePosition.Y, mirrorPosition.Z - (sourcePosition.Z - mirrorPosition.Z));
                if (isOdd)
                {
                    int* numPtr3 = (int*) ref vectori.Z;
                    numPtr3[0]++;
                }
            }
            return vectori;
        }

        private static Vector3I MirrorDirByPlane(MySymmetrySettingModeEnum mirror, Vector3I mirrorDir, bool isOdd, Vector3I sourceDir)
        {
            Vector3I vectori = sourceDir;
            if (mirror == MySymmetrySettingModeEnum.XPlane)
            {
                vectori = new Vector3I(-sourceDir.X, sourceDir.Y, sourceDir.Z);
            }
            if (mirror == MySymmetrySettingModeEnum.YPlane)
            {
                vectori = new Vector3I(sourceDir.X, -sourceDir.Y, sourceDir.Z);
            }
            if (mirror == MySymmetrySettingModeEnum.ZPlane)
            {
                vectori = new Vector3I(sourceDir.X, sourceDir.Y, -sourceDir.Z);
            }
            return vectori;
        }

        private unsafe void MirrorGizmoSpace(MyGizmoSpaceProperties targetSpace, MyGizmoSpaceProperties sourceSpace, MySymmetrySettingModeEnum mirrorPlane, Vector3I mirrorPosition, bool isOdd, MyCubeBlockDefinition cubeBlockDefinition, MyCubeGrid cubeGrid)
        {
            targetSpace.m_addPos = MirrorBlockByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_addPos);
            targetSpace.m_localMatrixAdd.Translation = (Vector3) targetSpace.m_addPos;
            targetSpace.m_addDir = MirrorDirByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_addDir);
            targetSpace.m_removePos = MirrorBlockByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_removePos);
            targetSpace.m_removeBlock = cubeGrid.GetCubeBlock(targetSpace.m_removePos);
            targetSpace.m_startBuild = (sourceSpace.m_startBuild == null) ? null : new Vector3I?(MirrorBlockByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_startBuild.Value));
            targetSpace.m_continueBuild = (sourceSpace.m_continueBuild == null) ? null : new Vector3I?(MirrorBlockByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_continueBuild.Value));
            targetSpace.m_startRemove = (sourceSpace.m_startRemove == null) ? null : new Vector3I?(MirrorBlockByPlane(mirrorPlane, mirrorPosition, isOdd, sourceSpace.m_startRemove.Value));
            Vector3 zero = Vector3.Zero;
            if (mirrorPlane == MySymmetrySettingModeEnum.XPlane)
            {
                zero = Vector3.Right;
            }
            else if (mirrorPlane == MySymmetrySettingModeEnum.YPlane)
            {
                zero = Vector3.Up;
            }
            else if (mirrorPlane == MySymmetrySettingModeEnum.ZPlane)
            {
                zero = Vector3.Forward;
            }
            CurrentBlockMirrorAxis = MySymmetryAxisEnum.None;
            if (MyUtils.IsZero((float) (Math.Abs(Vector3.Dot(sourceSpace.m_localMatrixAdd.Right, zero)) - 1f), 1E-05f))
            {
                CurrentBlockMirrorAxis = MySymmetryAxisEnum.X;
            }
            else if (MyUtils.IsZero((float) (Math.Abs(Vector3.Dot(sourceSpace.m_localMatrixAdd.Up, zero)) - 1f), 1E-05f))
            {
                CurrentBlockMirrorAxis = MySymmetryAxisEnum.Y;
            }
            else if (MyUtils.IsZero((float) (Math.Abs(Vector3.Dot(sourceSpace.m_localMatrixAdd.Forward, zero)) - 1f), 1E-05f))
            {
                CurrentBlockMirrorAxis = MySymmetryAxisEnum.Z;
            }
            CurrentBlockMirrorOption = MySymmetryAxisEnum.None;
            MySymmetryAxisEnum enum2 = (MyGuiScreenDebugCubeBlocks.DebugXMirroringAxis != null) ? MyGuiScreenDebugCubeBlocks.DebugXMirroringAxis.Value : cubeBlockDefinition.SymmetryX;
            MySymmetryAxisEnum enum3 = (MyGuiScreenDebugCubeBlocks.DebugYMirroringAxis != null) ? MyGuiScreenDebugCubeBlocks.DebugYMirroringAxis.Value : cubeBlockDefinition.SymmetryY;
            MySymmetryAxisEnum enum4 = (MyGuiScreenDebugCubeBlocks.DebugZMirroringAxis != null) ? MyGuiScreenDebugCubeBlocks.DebugZMirroringAxis.Value : cubeBlockDefinition.SymmetryZ;
            switch (CurrentBlockMirrorAxis)
            {
                case MySymmetryAxisEnum.X:
                    CurrentBlockMirrorOption = enum2;
                    break;

                case MySymmetryAxisEnum.Y:
                    CurrentBlockMirrorOption = enum3;
                    break;

                case MySymmetryAxisEnum.Z:
                    CurrentBlockMirrorOption = enum4;
                    break;

                default:
                    break;
            }
            switch (CurrentBlockMirrorOption)
            {
                case MySymmetryAxisEnum.X:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.Y:
                case MySymmetryAxisEnum.YThenOffsetX:
                case MySymmetryAxisEnum.YThenOffsetXOdd:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.Z:
                case MySymmetryAxisEnum.ZThenOffsetX:
                case MySymmetryAxisEnum.ZThenOffsetXOdd:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.XHalfY:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.YHalfY:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.ZHalfY:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.XHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.YHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.ZHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.XHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.YHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.ZHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(-1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.MinusHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.MinusHalfY:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.MinusHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.HalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(-1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.HalfY:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(-1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.HalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(-1.570796f) * sourceSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.XMinusHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.YMinusHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.ZMinusHalfZ:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.XMinusHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.YMinusHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationY(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.ZMinusHalfX:
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationZ(3.141593f) * sourceSpace.m_localMatrixAdd;
                    targetSpace.m_localMatrixAdd = Matrix.CreateRotationX(1.570796f) * targetSpace.m_localMatrixAdd;
                    break;

                case MySymmetryAxisEnum.OffsetX:
                case MySymmetryAxisEnum.OffsetXOddTest:
                    targetSpace.m_localMatrixAdd = sourceSpace.m_localMatrixAdd;
                    break;

                default:
                    targetSpace.m_localMatrixAdd = sourceSpace.m_localMatrixAdd;
                    break;
            }
            targetSpace.m_blockDefinition = string.IsNullOrEmpty(sourceSpace.m_blockDefinition.MirroringBlock) ? sourceSpace.m_blockDefinition : MyDefinitionManager.Static.GetCubeBlockDefinition(new MyDefinitionId(sourceSpace.m_blockDefinition.Id.TypeId, sourceSpace.m_blockDefinition.MirroringBlock));
            if (((enum2 == MySymmetryAxisEnum.None) && (enum3 == MySymmetryAxisEnum.None)) && (enum4 == MySymmetryAxisEnum.None))
            {
                BoundingBox box = new BoundingBox(((Vector3) (sourceSpace.m_min * cubeGrid.GridSize)) - new Vector3(cubeGrid.GridSize / 2f), (sourceSpace.m_max * cubeGrid.GridSize) + new Vector3(cubeGrid.GridSize / 2f));
                if (((box.Size.X > (1f * cubeGrid.GridSize)) || (box.Size.Y > (1f * cubeGrid.GridSize))) || (box.Size.Z > (1f * cubeGrid.GridSize)))
                {
                    Vector3 position = (Vector3) (sourceSpace.m_addPos * cubeGrid.GridSize);
                    Vector3D vectord = Vector3D.Transform(position, cubeGrid.WorldMatrix);
                    Vector3 vector5 = (Vector3) ((mirrorPosition - sourceSpace.m_addPos) * cubeGrid.GridSize);
                    if (isOdd)
                    {
                        float* singlePtr1 = (float*) ref vector5.X;
                        singlePtr1[0] -= cubeGrid.GridSize / 2f;
                        float* singlePtr2 = (float*) ref vector5.Y;
                        singlePtr2[0] -= cubeGrid.GridSize / 2f;
                        float* singlePtr3 = (float*) ref vector5.Z;
                        singlePtr3[0] += cubeGrid.GridSize / 2f;
                    }
                    Vector3 normal = vector5;
                    Vector3 vector7 = Vector3.Clamp(position + vector5, box.Min, box.Max) - position;
                    Vector3 vector8 = Vector3.Clamp(position + (vector5 * 100f), box.Min, box.Max) - position;
                    Vector3 vector9 = Vector3.Clamp(position - (vector5 * 100f), box.Min, box.Max) - position;
                    if ((mirrorPlane == MySymmetrySettingModeEnum.XPlane) || (mirrorPlane == MySymmetrySettingModeEnum.XPlaneOdd))
                    {
                        vector9.Y = 0f;
                        vector9.Z = 0f;
                        vector7.Y = 0f;
                        vector7.Z = 0f;
                        normal.Y = 0f;
                        normal.Z = 0f;
                        vector8.Y = 0f;
                        vector8.Z = 0f;
                    }
                    else if ((mirrorPlane == MySymmetrySettingModeEnum.YPlane) || (mirrorPlane == MySymmetrySettingModeEnum.YPlaneOdd))
                    {
                        vector9.X = 0f;
                        vector9.Z = 0f;
                        vector7.X = 0f;
                        vector7.Z = 0f;
                        normal.X = 0f;
                        normal.Z = 0f;
                        vector8.X = 0f;
                        vector8.Z = 0f;
                    }
                    else if ((mirrorPlane == MySymmetrySettingModeEnum.ZPlane) || (mirrorPlane == MySymmetrySettingModeEnum.ZPlaneOdd))
                    {
                        vector9.Y = 0f;
                        vector9.X = 0f;
                        vector7.Y = 0f;
                        vector7.X = 0f;
                        normal.Y = 0f;
                        normal.X = 0f;
                        vector8.Y = 0f;
                        vector8.X = 0f;
                    }
                    Vector3D.TransformNormal(vector7, cubeGrid.WorldMatrix);
                    Vector3D vectord2 = Vector3D.TransformNormal(normal, cubeGrid.WorldMatrix);
                    Vector3D.TransformNormal(vector9, cubeGrid.WorldMatrix);
                    bool flag = false;
                    if (normal.LengthSquared() < vector8.LengthSquared())
                    {
                        flag = true;
                    }
                    Vector3 vector10 = -vector9;
                    Vector3 local1 = normal - vector7;
                    Vector3.TransformNormal(local1, cubeGrid.WorldMatrix);
                    Vector3.TransformNormal(vector10, cubeGrid.WorldMatrix);
                    Vector3D vectord1 = vectord + vectord2;
                    Vector3 vector11 = local1 + vector10;
                    Vector3D.TransformNormal(vector11, cubeGrid.WorldMatrix);
                    Vector3 vector12 = sourceSpace.m_addPos + ((normal + vector11) / cubeGrid.GridSize);
                    if (flag)
                    {
                        targetSpace.m_mirroringOffset = Vector3I.Zero;
                        targetSpace.m_addPos = sourceSpace.m_addPos;
                        targetSpace.m_removePos = sourceSpace.m_removePos;
                        targetSpace.m_removeBlock = cubeGrid.GetCubeBlock(sourceSpace.m_removePos);
                    }
                    else
                    {
                        Vector3D.TransformNormal(vector12, cubeGrid.WorldMatrix);
                        Vector3 xyz = vector12;
                        targetSpace.m_mirroringOffset = new Vector3I(xyz) - targetSpace.m_addPos;
                        targetSpace.m_addPos = (Vector3I) (targetSpace.m_addPos + targetSpace.m_mirroringOffset);
                        targetSpace.m_addDir = sourceSpace.m_addDir;
                        targetSpace.m_localMatrixAdd.Translation = (Vector3) targetSpace.m_addPos;
                        if (targetSpace.m_startBuild != null)
                        {
                            Vector3I? nullable1;
                            Vector3I? startBuild = targetSpace.m_startBuild;
                            Vector3I mirroringOffset = targetSpace.m_mirroringOffset;
                            if (startBuild != null)
                            {
                                nullable1 = new Vector3I?(startBuild.GetValueOrDefault() + mirroringOffset);
                            }
                            else
                            {
                                nullable1 = null;
                            }
                            targetSpace.m_startBuild = nullable1;
                        }
                    }
                }
            }
            Vector3I vectori = Vector3I.Zero;
            switch (CurrentBlockMirrorOption)
            {
                case MySymmetryAxisEnum.ZThenOffsetX:
                case MySymmetryAxisEnum.YThenOffsetX:
                case MySymmetryAxisEnum.OffsetX:
                    vectori = new Vector3I(targetSpace.m_localMatrixAdd.Left);
                    break;

                case MySymmetryAxisEnum.ZThenOffsetXOdd:
                {
                    Vector3 left = Vector3.Left;
                    if (mirrorPlane == MySymmetrySettingModeEnum.XPlane)
                    {
                        left = Vector3.Up;
                    }
                    else if (mirrorPlane == MySymmetrySettingModeEnum.YPlane)
                    {
                        left = Vector3.Forward;
                    }
                    else if (mirrorPlane == MySymmetrySettingModeEnum.ZPlane)
                    {
                        left = Vector3.Left;
                    }
                    if (Math.Abs(Vector3.Dot(targetSpace.m_localMatrixAdd.Left, left)) > 0.9f)
                    {
                        vectori = new Vector3I(targetSpace.m_localMatrixAdd.Left);
                    }
                    break;
                }
                case MySymmetryAxisEnum.YThenOffsetXOdd:
                {
                    Vector3 left = Vector3.Left;
                    if (mirrorPlane == MySymmetrySettingModeEnum.XPlane)
                    {
                        left = Vector3.Up;
                    }
                    else if (mirrorPlane == MySymmetrySettingModeEnum.YPlane)
                    {
                        left = Vector3.Forward;
                    }
                    else if (mirrorPlane == MySymmetrySettingModeEnum.ZPlane)
                    {
                        left = Vector3.Left;
                    }
                    if (Math.Abs(Vector3.Dot(targetSpace.m_localMatrixAdd.Left, left)) > 0.9f)
                    {
                        vectori = Vector3I.Round(targetSpace.m_localMatrixAdd.Left);
                    }
                    break;
                }
                case MySymmetryAxisEnum.OffsetXOddTest:
                {
                    Vector3 left = Vector3.Left;
                    switch (CurrentBlockMirrorAxis)
                    {
                        case MySymmetryAxisEnum.X:
                            left = Vector3.Forward;
                            break;

                        case MySymmetryAxisEnum.Y:
                            left = Vector3.Up;
                            break;

                        case MySymmetryAxisEnum.Z:
                            left = Vector3.Left;
                            break;

                        default:
                            break;
                    }
                    if (Math.Abs(Vector3.Dot(targetSpace.m_localMatrixAdd.Left, left)) > 0.9f)
                    {
                        vectori = Vector3I.Round(targetSpace.m_localMatrixAdd.Left);
                    }
                    break;
                }
                default:
                    break;
            }
            MySymmetryAxisEnum currentBlockMirrorOption = CurrentBlockMirrorOption;
            if ((currentBlockMirrorOption == MySymmetryAxisEnum.None) || ((currentBlockMirrorOption - MySymmetryAxisEnum.ZThenOffsetX) <= MySymmetryAxisEnum.YHalfY))
            {
                targetSpace.m_mirroringOffset = vectori;
                targetSpace.m_addPos = (Vector3I) (targetSpace.m_addPos + targetSpace.m_mirroringOffset);
                targetSpace.m_removePos = (Vector3I) (targetSpace.m_removePos + targetSpace.m_mirroringOffset);
                targetSpace.m_removeBlock = cubeGrid.GetCubeBlock(targetSpace.m_removePos);
                Matrix* matrixPtr1 = (Matrix*) ref targetSpace.m_localMatrixAdd;
                matrixPtr1.Translation += vectori;
            }
            targetSpace.m_worldMatrixAdd = targetSpace.m_localMatrixAdd * cubeGrid.WorldMatrix;
        }

        public bool PointInsideGizmo(Vector3D point, MyGizmoSpaceEnum gizmo, ref MatrixD invGridWorldMatrix, float gridSize, float inflate = 0f, bool onVoxel = false, bool dynamicMode = false)
        {
            MatrixD m = new MatrixD();
            BoundingBoxD bb = new BoundingBoxD();
            this.GetGizmoPointTestVariables(ref invGridWorldMatrix, gridSize, out bb, out m, gizmo, inflate, onVoxel, dynamicMode);
            return (bb.Contains(Vector3D.Transform(point, m)) == ContainmentType.Contains);
        }

        public bool PointsAABBIntersectsGizmo(List<Vector3D> points, MyGizmoSpaceEnum gizmo, ref MatrixD invGridWorldMatrix, float gridSize, float inflate = 0f, bool onVoxel = false, bool dynamicMode = false)
        {
            MatrixD m = new MatrixD();
            BoundingBoxD bb = new BoundingBoxD();
            this.GetGizmoPointTestVariables(ref invGridWorldMatrix, gridSize, out bb, out m, gizmo, inflate, onVoxel, dynamicMode);
            BoundingBoxD xd3 = BoundingBoxD.CreateInvalid();
            using (List<Vector3D>.Enumerator enumerator = points.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Vector3D point = Vector3D.Transform(enumerator.Current, m);
                    if (bb.Contains(point) != ContainmentType.Contains)
                    {
                        xd3.Include(point);
                        continue;
                    }
                    return true;
                }
            }
            return xd3.Intersects(ref bb);
        }

        public void RemoveGizmoCubeParts()
        {
            foreach (MyGizmoSpaceProperties properties in this.m_spaces)
            {
                this.RemoveGizmoCubeParts(properties);
            }
        }

        private void RemoveGizmoCubeParts(MyGizmoSpaceProperties gizmoSpace)
        {
            gizmoSpace.m_cubeMatrices.Clear();
            gizmoSpace.m_cubeModels.Clear();
        }

        public void RotateAxis(ref MatrixD rotatedMatrix)
        {
            this.SpaceDefault.m_localMatrixAdd = (Matrix) rotatedMatrix;
            this.SpaceDefault.m_localMatrixAdd.Forward = (Vector3) Vector3I.Round(this.SpaceDefault.m_localMatrixAdd.Forward);
            this.SpaceDefault.m_localMatrixAdd.Up = (Vector3) Vector3I.Round(this.SpaceDefault.m_localMatrixAdd.Up);
            this.SpaceDefault.m_localMatrixAdd.Right = (Vector3) Vector3I.Round(this.SpaceDefault.m_localMatrixAdd.Right);
        }

        public void SetupLocalAddMatrix(MyGizmoSpaceProperties gizmoSpace, Vector3I normal)
        {
            Vector3I vectori = -normal;
            Matrix matrix1 = Matrix.CreateWorld(Vector3.Zero, (Vector3) vectori, (Vector3) Vector3I.Shift(vectori));
            Matrix matrix = Matrix.Invert(matrix1);
            Vector3I vectori2 = Vector3I.Round((matrix1 * gizmoSpace.m_localMatrixAdd).Up);
            if ((vectori2 == gizmoSpace.m_addDir) || (vectori2 == -gizmoSpace.m_addDir))
            {
                vectori2 = Vector3I.Shift(vectori2);
            }
            gizmoSpace.m_localMatrixAdd = matrix * Matrix.CreateWorld(Vector3.Zero, (Vector3) gizmoSpace.m_addDir, (Vector3) vectori2);
        }

        public void UpdateGizmoCubeParts(MyGizmoSpaceProperties gizmoSpace, MyBlockBuilderRenderData renderData, ref MatrixD invGridWorldMatrix, MyCubeBlockDefinition definition = null)
        {
            this.RemoveGizmoCubeParts(gizmoSpace);
            this.AddGizmoCubeParts(gizmoSpace, renderData, ref invGridWorldMatrix, definition);
        }

        public MyGizmoSpaceProperties SpaceDefault =>
            this.m_spaces[0];

        public MyGizmoSpaceProperties[] Spaces =>
            this.m_spaces;

        public class MyGizmoSpaceProperties
        {
            public bool Enabled;
            public MyGizmoSpaceEnum SourceSpace;
            public MySymmetrySettingModeEnum SymmetryPlane;
            public Vector3I SymmetryPlanePos;
            public bool SymmetryIsOdd;
            public MatrixD m_worldMatrixAdd = Matrix.Identity;
            public Matrix m_localMatrixAdd = Matrix.Identity;
            public Vector3I m_addDir = Vector3I.Up;
            public Vector3I m_addPos;
            public Vector3I m_min;
            public Vector3I m_max;
            public Vector3I m_centerPos;
            public Vector3I m_removePos;
            public MySlimBlock m_removeBlock;
            public ushort? m_blockIdInCompound;
            public Vector3I? m_startBuild;
            public Vector3I? m_continueBuild;
            public Vector3I? m_startRemove;
            public List<Vector3I> m_positions = new List<Vector3I>();
            public List<Vector3> m_cubeNormals = new List<Vector3>();
            public List<Vector4UByte> m_patternOffsets = new List<Vector4UByte>();
            public Vector3? m_addPosSmallOnLarge;
            public Vector3 m_minSmallOnLarge;
            public Vector3 m_maxSmallOnLarge;
            public Vector3 m_centerPosSmallOnLarge;
            public List<Vector3> m_positionsSmallOnLarge = new List<Vector3>();
            public List<string> m_cubeModels = new List<string>();
            public List<MatrixD> m_cubeMatrices = new List<MatrixD>();
            public List<string> m_cubeModelsTemp = new List<string>();
            public List<MatrixD> m_cubeMatricesTemp = new List<MatrixD>();
            public bool m_buildAllowed;
            public bool m_showGizmoCube;
            public Quaternion m_rotation;
            public Vector3I m_mirroringOffset;
            public MyCubeBlockDefinition m_blockDefinition;
            public bool m_dynamicBuildAllowed;
            public HashSet<Tuple<MySlimBlock, ushort?>> m_removeBlocksInMultiBlock = new HashSet<Tuple<MySlimBlock, ushort?>>();
            public MatrixD m_animationLastMatrix = MatrixD.Identity;
            public Vector3D m_animationLastPosition = Vector3D.Zero;
            public float m_animationProgress = 1f;

            public void Clear()
            {
                this.m_startBuild = null;
                this.m_startRemove = null;
                this.m_removeBlock = null;
                this.m_blockIdInCompound = null;
                this.m_positions.Clear();
                this.m_cubeNormals.Clear();
                this.m_patternOffsets.Clear();
                this.m_cubeModels.Clear();
                this.m_cubeMatrices.Clear();
                this.m_cubeModelsTemp.Clear();
                this.m_cubeMatricesTemp.Clear();
                this.m_mirroringOffset = Vector3I.Zero;
                this.m_addPosSmallOnLarge = null;
                this.m_positionsSmallOnLarge.Clear();
                this.m_dynamicBuildAllowed = false;
                this.m_removeBlocksInMultiBlock.Clear();
            }

            public Quaternion LocalOrientation =>
                Quaternion.CreateFromRotationMatrix(this.m_localMatrixAdd);
        }
    }
}

