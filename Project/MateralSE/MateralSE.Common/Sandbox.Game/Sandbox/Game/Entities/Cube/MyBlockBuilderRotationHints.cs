namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyBlockBuilderRotationHints
    {
        private static readonly MyStringId ID_SQUARE_FULL_COLOR = MyStringId.GetOrCompute("SquareFullColor");
        private static readonly MyStringId ID_ARROW_LEFT_GREEN = MyStringId.GetOrCompute("ArrowLeftGreen");
        private static readonly MyStringId ID_ARROW_RIGHT_GREEN = MyStringId.GetOrCompute("ArrowRightGreen");
        private static readonly MyStringId ID_ARROW_GREEN = MyStringId.GetOrCompute("ArrowGreen");
        private static readonly MyStringId ID_ARROW_LEFT_RED = MyStringId.GetOrCompute("ArrowLeftRed");
        private static readonly MyStringId ID_ARROW_RIGHT_RED = MyStringId.GetOrCompute("ArrowRightRed");
        private static readonly MyStringId ID_ARROW_RED = MyStringId.GetOrCompute("ArrowRed");
        private static readonly MyStringId ID_ARROW_LEFT_BLUE = MyStringId.GetOrCompute("ArrowLeftBlue");
        private static readonly MyStringId ID_ARROW_RIGHT_BLUE = MyStringId.GetOrCompute("ArrowRightBlue");
        private static readonly MyStringId ID_ARROW_BLUE = MyStringId.GetOrCompute("ArrowBlue");
        private Vector3D[] m_cubeVertices = new Vector3D[8];
        private List<BoxEdge> m_cubeEdges = new List<BoxEdge>(3);
        private MyBillboardViewProjection m_viewProjection;
        private const MyBillboard.BlendTypeEnum HINT_CUBE_BLENDTYPE = MyBillboard.BlendTypeEnum.LDR;

        public MyBlockBuilderRotationHints()
        {
            this.Clear();
        }

        public unsafe void CalculateRotationHints(MatrixD drawMatrix, bool draw, bool fixedAxes = false, bool hideForwardAndUpArrows = false)
        {
            int num6;
            string controlButtonName;
            string str2;
            string str3;
            string str4;
            string str5;
            string str6;
            Vector3D zero;
            Vector3D vectord14;
            Vector3D vectord15;
            Vector3D vectord16;
            Vector3D vectord17;
            Vector3D vectord18;
            Vector3D vectord19;
            Vector3D vectord20;
            Vector3D vectord21;
            Vector3D vectord22;
            Vector3D vectord23;
            Vector3D vectord24;
            int num13;
            int num14;
            int num15;
            int num16;
            int num17;
            int num18;
            int num20;
            int num21;
            float num22;
            Vector3D vectord25;
            Vector3D vectord26;
            Vector3D vectord27;
            bool flag2;
            bool flag3;
            bool flag5;
            bool flag6;
            Vector3D vectord29;
            Vector3D vectord30;
            Vector3D vectord31;
            Vector3D vectord32;
            int num28;
            bool flag7;
            Vector3 vector3;
            Vector3D vectord38;
            Vector3 vector8;
            Vector3D vectord39;
            Vector3 vector9;
            int num29;
            bool flag8;
            Vector3 vector10;
            Matrix viewMatrix = (Matrix) MySector.MainCamera.ViewMatrix;
            MatrixD matrix = MatrixD.Invert(viewMatrix);
            if (!drawMatrix.IsValid())
            {
                return;
            }
            else
            {
                if (viewMatrix.IsValid())
                {
                    int num7;
                    int num8;
                    int num9;
                    int num10;
                    int num11;
                    int num12;
                    MatrixD* xdPtr1 = (MatrixD*) ref matrix;
                    xdPtr1.Translation = ((drawMatrix.Translation - (7.0 * matrix.Forward)) + (1.0 * matrix.Left)) - (0.60000002384185791 * matrix.Up);
                    MatrixD* xdPtr2 = (MatrixD*) ref drawMatrix;
                    xdPtr2.Translation -= matrix.Translation;
                    this.m_viewProjection.CameraPosition = matrix.Translation;
                    matrix.Translation = Vector3D.Zero;
                    Matrix matrix2 = (Matrix) MatrixD.Transpose(matrix);
                    this.m_viewProjection.ViewAtZero = matrix2;
                    float num = 2.75f;
                    Vector2 screenSizeFromNormalizedSize = MyGuiManager.GetScreenSizeFromNormalizedSize(Vector2.One, false);
                    int num2 = (int) (screenSizeFromNormalizedSize.X / num);
                    int num3 = (int) (screenSizeFromNormalizedSize.Y / num);
                    int num4 = 0;
                    int num5 = 0;
                    this.m_viewProjection.Viewport = new MyViewport((float) ((((int) MySector.MainCamera.Viewport.Width) - num2) - num4), (float) num5, (float) num2, (float) num3);
                    this.m_viewProjection.Projection = Matrix.CreatePerspectiveFieldOfView(0.7853982f, ((float) num2) / ((float) num3), 0.1f, 10f);
                    BoundingBoxD localbox = new BoundingBoxD(-new Vector3(MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large) * 0.5f), new Vector3(MyDefinitionManager.Static.GetCubeSize(MyCubeSize.Large)) * 0.5f);
                    num6 = 0;
                    MyRenderProxy.AddBillboardViewProjection(num6, this.m_viewProjection);
                    if (draw)
                    {
                        Color red = Color.Red;
                        Color green = Color.Green;
                        Color blue = Color.Blue;
                        Color white = Color.White;
                        Color color5 = Color.White;
                        Color color6 = Color.White;
                        Color wire = Color.White;
                        MyStringId? lineMaterial = null;
                        MySimpleObjectDraw.DrawTransparentBox(ref drawMatrix, ref localbox, ref red, ref green, ref blue, ref white, ref color5, ref color6, ref wire, MySimpleObjectRasterizer.Solid, 1, 0.04f, new MyStringId?(ID_SQUARE_FULL_COLOR), lineMaterial, false, num6, MyBillboard.BlendTypeEnum.LDR, 1f, null);
                    }
                    new MyOrientedBoundingBoxD(Vector3D.Transform(localbox.Center, drawMatrix), localbox.HalfExtents, Quaternion.CreateFromRotationMatrix(drawMatrix)).GetCorners(this.m_cubeVertices, 0);
                    GetClosestCubeEdge(this.m_cubeVertices, Vector3D.Zero, MyOrientedBoundingBox.StartXVertices, MyOrientedBoundingBox.EndXVertices, out num7, out num8);
                    Vector3D from = this.m_cubeVertices[MyOrientedBoundingBox.StartXVertices[num7]];
                    Vector3D to = this.m_cubeVertices[MyOrientedBoundingBox.EndXVertices[num7]];
                    Vector3D vectord3 = this.m_cubeVertices[MyOrientedBoundingBox.StartXVertices[num8]];
                    Vector3D vectord4 = this.m_cubeVertices[MyOrientedBoundingBox.EndXVertices[num8]];
                    GetClosestCubeEdge(this.m_cubeVertices, Vector3D.Zero, MyOrientedBoundingBox.StartYVertices, MyOrientedBoundingBox.EndYVertices, out num9, out num10);
                    Vector3D vectord5 = this.m_cubeVertices[MyOrientedBoundingBox.StartYVertices[num9]];
                    Vector3D vectord6 = this.m_cubeVertices[MyOrientedBoundingBox.EndYVertices[num9]];
                    Vector3D vectord7 = this.m_cubeVertices[MyOrientedBoundingBox.StartYVertices[num10]];
                    Vector3D vectord8 = this.m_cubeVertices[MyOrientedBoundingBox.EndYVertices[num10]];
                    GetClosestCubeEdge(this.m_cubeVertices, Vector3D.Zero, MyOrientedBoundingBox.StartZVertices, MyOrientedBoundingBox.EndZVertices, out num11, out num12);
                    Vector3D vectord9 = this.m_cubeVertices[MyOrientedBoundingBox.StartZVertices[num11]];
                    Vector3D vectord10 = this.m_cubeVertices[MyOrientedBoundingBox.EndZVertices[num11]];
                    Vector3D vectord11 = this.m_cubeVertices[MyOrientedBoundingBox.StartZVertices[num12]];
                    Vector3D vectord12 = this.m_cubeVertices[MyOrientedBoundingBox.EndZVertices[num12]];
                    this.m_cubeEdges.Clear();
                    BoxEdge item = new BoxEdge {
                        Axis = 0,
                        Edge = new LineD(from, to)
                    };
                    this.m_cubeEdges.Add(item);
                    item = new BoxEdge {
                        Axis = 1,
                        Edge = new LineD(vectord5, vectord6)
                    };
                    this.m_cubeEdges.Add(item);
                    item = new BoxEdge {
                        Axis = 2,
                        Edge = new LineD(vectord9, vectord10)
                    };
                    this.m_cubeEdges.Add(item);
                    if (!fixedAxes)
                    {
                        int num23;
                        this.RotationRightAxis = GetBestAxis(this.m_cubeEdges, MySector.MainCamera.WorldMatrix.Right, out num23);
                        this.RotationRightDirection = num23;
                        this.RotationUpAxis = GetBestAxis(this.m_cubeEdges, MySector.MainCamera.WorldMatrix.Up, out num23);
                        this.RotationUpDirection = num23;
                        this.RotationForwardAxis = GetBestAxis(this.m_cubeEdges, MySector.MainCamera.WorldMatrix.Forward, out num23);
                        this.RotationForwardDirection = num23;
                    }
                    controlButtonName = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    str2 = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    str3 = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    str4 = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    str5 = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    str6 = MyInput.Static.GetGameControl(MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE).GetControlButtonName(MyGuiInputDeviceEnum.Keyboard);
                    if (MyInput.Static.IsJoystickConnected() && MyInput.Static.IsJoystickLastUsed)
                    {
                        controlButtonName = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_POSITIVE).ToString();
                        str2 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_HORISONTAL_NEGATIVE).ToString();
                        str3 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_VERTICAL_POSITIVE).ToString();
                        str4 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_VERTICAL_NEGATIVE).ToString();
                        str5 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_ROLL_POSITIVE).ToString();
                        str6 = MyControllerHelper.GetCodeForControl(MySpaceBindingCreator.CX_BUILD_MODE, MyControlsSpace.CUBE_ROTATE_ROLL_NEGATIVE).ToString();
                    }
                    zero = Vector3D.Zero;
                    vectord14 = Vector3D.Zero;
                    vectord15 = Vector3D.Zero;
                    vectord16 = Vector3D.Zero;
                    vectord17 = Vector3D.Zero;
                    vectord18 = Vector3D.Zero;
                    vectord19 = Vector3D.Zero;
                    vectord20 = Vector3D.Zero;
                    vectord21 = Vector3D.Zero;
                    vectord22 = Vector3D.Zero;
                    vectord23 = Vector3D.Zero;
                    vectord24 = Vector3D.Zero;
                    num13 = -1;
                    num14 = -1;
                    num15 = -1;
                    num16 = -1;
                    num17 = -1;
                    num18 = -1;
                    int num19 = -1;
                    num20 = -1;
                    num21 = -1;
                    if (this.RotationRightAxis == 0)
                    {
                        zero = from;
                        vectord14 = to;
                        vectord19 = vectord3;
                        vectord20 = vectord4;
                        num13 = 0;
                        num16 = num7;
                        num19 = num8;
                    }
                    else if (this.RotationRightAxis == 1)
                    {
                        zero = vectord5;
                        vectord14 = vectord6;
                        vectord19 = vectord7;
                        vectord20 = vectord8;
                        num13 = 1;
                        num16 = num9;
                        num19 = num10;
                    }
                    else if (this.RotationRightAxis == 2)
                    {
                        zero = vectord9;
                        vectord14 = vectord10;
                        vectord19 = vectord11;
                        vectord20 = vectord12;
                        num13 = 2;
                        num16 = num11;
                        num19 = num12;
                    }
                    if (this.RotationUpAxis == 0)
                    {
                        vectord15 = from;
                        vectord16 = to;
                        vectord21 = vectord3;
                        vectord22 = vectord4;
                        num14 = 0;
                        num17 = num7;
                        num20 = num8;
                    }
                    else if (this.RotationUpAxis == 1)
                    {
                        vectord15 = vectord5;
                        vectord16 = vectord6;
                        vectord21 = vectord7;
                        vectord22 = vectord8;
                        num14 = 1;
                        num17 = num9;
                        num20 = num10;
                    }
                    else if (this.RotationUpAxis == 2)
                    {
                        vectord15 = vectord9;
                        vectord16 = vectord10;
                        vectord21 = vectord11;
                        vectord22 = vectord12;
                        num14 = 2;
                        num17 = num11;
                        num20 = num12;
                    }
                    if (this.RotationForwardAxis == 0)
                    {
                        vectord17 = from;
                        vectord18 = to;
                        vectord23 = vectord3;
                        vectord24 = vectord4;
                        num15 = 0;
                        num18 = num7;
                        num21 = num8;
                    }
                    else if (this.RotationForwardAxis == 1)
                    {
                        vectord17 = vectord5;
                        vectord18 = vectord6;
                        vectord23 = vectord7;
                        vectord24 = vectord8;
                        num15 = 1;
                        num18 = num9;
                        num21 = num10;
                    }
                    else if (this.RotationForwardAxis == 2)
                    {
                        vectord17 = vectord9;
                        vectord18 = vectord10;
                        vectord23 = vectord11;
                        vectord24 = vectord12;
                        num15 = 2;
                        num18 = num11;
                        num21 = num12;
                    }
                    num22 = 0.5448648f;
                    if (!draw)
                    {
                        return;
                    }
                    else
                    {
                        vectord25 = Vector3.Normalize(vectord14 - zero);
                        vectord26 = Vector3.Normalize(vectord16 - vectord15);
                        vectord27 = Vector3.Normalize(vectord18 - vectord17);
                        Vector3D forwardVector = MySector.MainCamera.ForwardVector;
                        float num24 = Math.Abs(Vector3.Dot((Vector3) forwardVector, (Vector3) vectord25));
                        float num25 = Math.Abs(Vector3.Dot((Vector3) forwardVector, (Vector3) vectord26));
                        float num26 = Math.Abs(Vector3.Dot((Vector3) forwardVector, (Vector3) vectord27));
                        bool flag = false;
                        flag2 = false;
                        flag3 = false;
                        bool flag4 = false;
                        flag5 = false;
                        flag6 = false;
                        float num27 = 0.4f;
                        if (num24 < num27)
                        {
                            if (num25 < num27)
                            {
                                flag6 = true;
                                flag = true;
                                flag2 = true;
                            }
                            else if (num26 >= num27)
                            {
                                flag2 = true;
                                flag3 = true;
                            }
                            else
                            {
                                flag5 = true;
                                flag = true;
                                flag3 = true;
                            }
                        }
                        else if (num25 < num27)
                        {
                            if (num24 < num27)
                            {
                                flag6 = true;
                                flag = true;
                                flag2 = true;
                            }
                            else if (num26 >= num27)
                            {
                                flag = true;
                                flag3 = true;
                            }
                            else
                            {
                                flag4 = true;
                                flag2 = true;
                                flag3 = true;
                            }
                        }
                        else if (num26 < num27)
                        {
                            if (num24 < num27)
                            {
                                flag5 = true;
                                flag = true;
                                flag3 = true;
                            }
                            else if (num25 >= num27)
                            {
                                flag2 = true;
                                flag = true;
                            }
                            else
                            {
                                flag5 = true;
                                flag = true;
                                flag3 = true;
                            }
                        }
                        if (hideForwardAndUpArrows && (this.RotationRightAxis != 1))
                        {
                            goto TR_0025;
                        }
                        if (!flag4)
                        {
                            if (!flag)
                            {
                                Vector3 vector4;
                                Vector3 vector5;
                                MyOrientedBoundingBox.GetNormalBetweenEdges(num13, num16, num16 + 1, out vector5);
                                MyOrientedBoundingBox.GetNormalBetweenEdges(num13, num16, num16 - 1, out vector4);
                                Vector3D vectord34 = (zero + vectord14) * 0.5;
                                Vector3D vectord35 = Vector3D.TransformNormal(vector5, drawMatrix);
                                Vector3D vectord36 = Vector3D.TransformNormal(vector4, drawMatrix);
                                MyTransparentGeometry.AddBillboardOriented(ID_ARROW_GREEN, Vector4.One, (vectord34 + (vectord35 * 0.30000001192092896)) - (vectord36 * 0.0099999997764825821), (Vector3) vectord25, (Vector3) vectord35, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                                MyTransparentGeometry.AddBillboardOriented(ID_ARROW_GREEN, Vector4.One, (vectord34 + (vectord36 * 0.30000001192092896)) - (vectord35 * 0.0099999997764825821), (Vector3) vectord25, (Vector3) vectord36, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                                MyRenderProxy.DebugDrawText3D((vectord34 + (vectord35 * 0.30000001192092896)) - (vectord36 * 0.0099999997764825821), (this.RotationRightDirection < 0) ? controlButtonName : str2, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                                MyRenderProxy.DebugDrawText3D((vectord34 + (vectord36 * 0.30000001192092896)) - (vectord35 * 0.0099999997764825821), (this.RotationRightDirection < 0) ? str2 : controlButtonName, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                                goto TR_0025;
                            }
                            else
                            {
                                Vector3 vector;
                                Vector3 vector2;
                                MyOrientedBoundingBox.GetNormalBetweenEdges(num13, num16, num19, out vector2);
                                vectord29 = (zero + vectord14) * 0.5;
                                vectord30 = Vector3D.TransformNormal(vector2, drawMatrix);
                                MyOrientedBoundingBox.GetNormalBetweenEdges(num13, num19, num16, out vector);
                                vectord31 = (vectord19 + vectord20) * 0.5;
                                vectord32 = Vector3D.TransformNormal(vector, drawMatrix);
                                flag7 = false;
                                if ((num16 == 0) && (num19 == 3))
                                {
                                    num28 = num16 + 1;
                                    goto TR_0029;
                                }
                                if ((num16 >= num19) && ((num16 != 3) || (num19 != 0)))
                                {
                                    num28 = num16 + 1;
                                    goto TR_0029;
                                }
                            }
                        }
                        else
                        {
                            Vector3D vectord28 = (((vectord17 + vectord18) + vectord23) + vectord24) * 0.25;
                            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_LEFT_GREEN, Vector4.One, (vectord28 - ((this.RotationForwardDirection * vectord27) * 0.20000000298023224)) - ((this.RotationRightDirection * vectord25) * 0.0099999997764825821), (Vector3) (-this.RotationUpDirection * vectord26), (Vector3) (-this.RotationForwardDirection * vectord27), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RIGHT_GREEN, Vector4.One, (vectord28 + ((this.RotationForwardDirection * vectord27) * 0.20000000298023224)) - ((this.RotationRightDirection * vectord25) * 0.0099999997764825821), (Vector3) (this.RotationUpDirection * vectord26), (Vector3) (this.RotationForwardDirection * vectord27), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                            MyRenderProxy.DebugDrawText3D((vectord28 - ((this.RotationForwardDirection * vectord27) * 0.20000000298023224)) - ((this.RotationRightDirection * vectord25) * 0.0099999997764825821), str2, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                            MyRenderProxy.DebugDrawText3D((vectord28 + ((this.RotationForwardDirection * vectord27) * 0.20000000298023224)) - ((this.RotationRightDirection * vectord25) * 0.0099999997764825821), controlButtonName, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                            goto TR_0025;
                        }
                        goto TR_002B;
                    }
                    goto TR_0029;
                }
                else
                {
                    return;
                }
                goto TR_002B;
            }
            goto TR_0029;
        TR_0013:
            if (!hideForwardAndUpArrows || (this.RotationForwardAxis == 1))
            {
                if (flag6)
                {
                    Vector3D vectord41 = (((zero + vectord14) + vectord19) + vectord20) * 0.25;
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_LEFT_BLUE, Vector4.One, (vectord41 + ((this.RotationUpDirection * vectord26) * 0.20000000298023224)) - ((this.RotationForwardDirection * vectord27) * 0.0099999997764825821), (Vector3) (-this.RotationRightDirection * vectord25), (Vector3) (this.RotationUpDirection * vectord26), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RIGHT_BLUE, Vector4.One, (vectord41 - ((this.RotationUpDirection * vectord26) * 0.20000000298023224)) - ((this.RotationForwardDirection * vectord27) * 0.0099999997764825821), (Vector3) (this.RotationRightDirection * vectord25), (Vector3) (-this.RotationUpDirection * vectord26), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyRenderProxy.DebugDrawText3D((vectord41 + ((this.RotationUpDirection * vectord26) * 0.20000000298023224)) - ((this.RotationForwardDirection * vectord27) * 0.0099999997764825821), str5, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, num6, false);
                    MyRenderProxy.DebugDrawText3D((vectord41 - ((this.RotationUpDirection * vectord26) * 0.20000000298023224)) - ((this.RotationForwardDirection * vectord27) * 0.0099999997764825821), str6, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, num6, false);
                    return;
                }
                if (!flag3)
                {
                    Vector3 vector22;
                    Vector3 vector23;
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num15, num18, num18 + 1, out vector23);
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num15, num18, num18 - 1, out vector22);
                    Vector3D vectord44 = (vectord17 + vectord18) * 0.5;
                    Vector3 upVector = Vector3.TransformNormal(vector23, drawMatrix);
                    Vector3 vector25 = Vector3.TransformNormal(vector22, drawMatrix);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_BLUE, Vector4.One, (vectord44 + (upVector * 0.3f)) - (vector25 * 0.01f), (Vector3) vectord27, upVector, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_BLUE, Vector4.One, (vectord44 + (vector25 * 0.3f)) - (upVector * 0.01f), (Vector3) vectord27, vector25, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyRenderProxy.DebugDrawText3D((vectord44 + (upVector * 0.3f)) - (vector25 * 0.01f), (this.RotationForwardDirection < 0) ? str5 : str6, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                    MyRenderProxy.DebugDrawText3D((vectord44 + (vector25 * 0.3f)) - (upVector * 0.01f), (this.RotationForwardDirection < 0) ? str6 : str5, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                }
                else
                {
                    Vector3 vector16;
                    Vector3 vector17;
                    int num30;
                    Vector3 vector20;
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num15, num18, num21, out vector17);
                    Vector3D vectord42 = (vectord17 + vectord18) * 0.5;
                    Vector3 upVector = Vector3.TransformNormal(vector17, drawMatrix);
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num15, num21, num18, out vector16);
                    Vector3D vectord43 = (vectord23 + vectord24) * 0.5;
                    Vector3 vector19 = Vector3.TransformNormal(vector16, drawMatrix);
                    bool flag9 = false;
                    if ((num18 == 0) && (num21 == 3))
                    {
                        num30 = num18 + 1;
                    }
                    else if ((num18 >= num21) && ((num18 != 3) || (num21 != 0)))
                    {
                        num30 = num18 + 1;
                    }
                    else
                    {
                        num30 = num18 - 1;
                        flag9 = true;
                    }
                    if (this.RotationForwardDirection < 0)
                    {
                        flag9 = !flag9;
                    }
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num15, num18, num30, out vector20);
                    Vector3 vector21 = Vector3.TransformNormal(vector20, drawMatrix);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_BLUE, Vector4.One, (vectord42 + (upVector * 0.4f)) - (vector21 * 0.01f), (Vector3) vectord27, vector19, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_BLUE, Vector4.One, (vectord43 + (vector19 * 0.4f)) - (vector21 * 0.01f), (Vector3) vectord27, upVector, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyRenderProxy.DebugDrawText3D((vectord42 + (upVector * 0.3f)) - (vector21 * 0.01f), flag9 ? str5 : str6, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                    MyRenderProxy.DebugDrawText3D((vectord43 + (vector19 * 0.3f)) - (vector21 * 0.01f), flag9 ? str6 : str5, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                    return;
                }
            }
            return;
        TR_0017:
            if (this.RotationUpDirection < 0)
            {
                flag8 = !flag8;
            }
            MyOrientedBoundingBox.GetNormalBetweenEdges(num14, num17, num29, out vector10);
            Vector3 vector11 = Vector3.TransformNormal(vector10, drawMatrix);
            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RED, Vector4.One, (vectord38 + (vector8 * 0.4f)) - (vector11 * 0.01f), (Vector3) vectord26, vector9, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RED, Vector4.One, (vectord39 + (vector9 * 0.4f)) - (vector11 * 0.01f), (Vector3) vectord26, vector8, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
            MyRenderProxy.DebugDrawText3D((vectord38 + (vector8 * 0.3f)) - (vector11 * 0.01f), flag8 ? str4 : str3, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
            MyRenderProxy.DebugDrawText3D((vectord39 + (vector9 * 0.3f)) - (vector11 * 0.01f), flag8 ? str3 : str4, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
            goto TR_0013;
        TR_0025:
            if (hideForwardAndUpArrows && (this.RotationUpAxis != 1))
            {
                goto TR_0013;
            }
            if (!flag5)
            {
                if (!flag2)
                {
                    Vector3 vector12;
                    Vector3 vector13;
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num14, num17, num17 + 1, out vector13);
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num14, num17, num17 - 1, out vector12);
                    Vector3D vectord40 = (vectord15 + vectord16) * 0.5;
                    Vector3 upVector = Vector3.TransformNormal(vector13, drawMatrix);
                    Vector3 vector15 = Vector3.TransformNormal(vector12, drawMatrix);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RED, Vector4.One, (vectord40 + (upVector * 0.3f)) - (vector15 * 0.01f), (Vector3) vectord26, upVector, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RED, Vector4.One, (vectord40 + (vector15 * 0.3f)) - (upVector * 0.01f), (Vector3) vectord26, vector15, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
                    MyRenderProxy.DebugDrawText3D((vectord40 + (upVector * 0.6f)) - (vector15 * 0.01f), (this.RotationUpDirection > 0) ? str3 : str4, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                    MyRenderProxy.DebugDrawText3D((vectord40 + (vector15 * 0.6f)) - (upVector * 0.01f), (this.RotationUpDirection > 0) ? str4 : str3, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                    goto TR_0013;
                }
                else
                {
                    Vector3 vector6;
                    Vector3 vector7;
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num14, num17, num20, out vector7);
                    vectord38 = (vectord15 + vectord16) * 0.5;
                    vector8 = Vector3.TransformNormal(vector7, drawMatrix);
                    MyOrientedBoundingBox.GetNormalBetweenEdges(num14, num20, num17, out vector6);
                    vectord39 = (vectord21 + vectord22) * 0.5;
                    vector9 = Vector3.TransformNormal(vector6, drawMatrix);
                    flag8 = false;
                    if ((num17 == 0) && (num20 == 3))
                    {
                        num29 = num17 + 1;
                        goto TR_0017;
                    }
                    if ((num17 >= num20) && ((num17 != 3) || (num20 != 0)))
                    {
                        num29 = num17 + 1;
                        goto TR_0017;
                    }
                }
            }
            else
            {
                Vector3D vectord37 = (((vectord17 + vectord18) + vectord23) + vectord24) * 0.25;
                MyTransparentGeometry.AddBillboardOriented(ID_ARROW_LEFT_RED, Vector4.One, (vectord37 - ((this.RotationRightDirection * vectord25) * 0.20000000298023224)) - ((this.RotationUpDirection * vectord26) * 0.0099999997764825821), (Vector3) (-this.RotationForwardDirection * vectord27), (Vector3) (-this.RotationRightDirection * vectord25), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                MyTransparentGeometry.AddBillboardOriented(ID_ARROW_RIGHT_RED, Vector4.One, (vectord37 + ((this.RotationRightDirection * vectord25) * 0.20000000298023224)) - ((this.RotationUpDirection * vectord26) * 0.0099999997764825821), (Vector3) (this.RotationForwardDirection * vectord27), (Vector3) (this.RotationRightDirection * vectord25), 0.2f, num6, MyBillboard.BlendTypeEnum.LDR);
                MyRenderProxy.DebugDrawText3D((vectord37 - ((this.RotationRightDirection * vectord25) * 0.20000000298023224)) - ((this.RotationUpDirection * vectord26) * 0.0099999997764825821), str3, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                MyRenderProxy.DebugDrawText3D((vectord37 + ((this.RotationRightDirection * vectord25) * 0.20000000298023224)) - ((this.RotationUpDirection * vectord26) * 0.0099999997764825821), str4, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
                goto TR_0013;
            }
            num29 = num17 - 1;
            flag8 = true;
            goto TR_0017;
        TR_0029:
            if (this.RotationRightDirection < 0)
            {
                flag7 = !flag7;
            }
            MyOrientedBoundingBox.GetNormalBetweenEdges(num13, num16, num28, out vector3);
            Vector3D vectord33 = Vector3D.TransformNormal(vector3, drawMatrix);
            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_GREEN, Vector4.One, (vectord29 + (vectord30 * 0.40000000596046448)) - (vectord33 * 0.0099999997764825821), (Vector3) vectord25, (Vector3) vectord32, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
            MyTransparentGeometry.AddBillboardOriented(ID_ARROW_GREEN, Vector4.One, (vectord31 + (vectord32 * 0.40000000596046448)) - (vectord33 * 0.0099999997764825821), (Vector3) vectord25, (Vector3) vectord30, 0.5f, num6, MyBillboard.BlendTypeEnum.LDR);
            MyRenderProxy.DebugDrawText3D((vectord29 + (vectord30 * 0.30000001192092896)) - (vectord33 * 0.0099999997764825821), flag7 ? controlButtonName : str2, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
            MyRenderProxy.DebugDrawText3D((vectord31 + (vectord32 * 0.30000001192092896)) - (vectord33 * 0.0099999997764825821), flag7 ? str2 : controlButtonName, Color.White, num22, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, num6, false);
            goto TR_0025;
        TR_002B:
            num28 = num16 - 1;
            flag7 = true;
            goto TR_0029;
        }

        public void Clear()
        {
            this.RotationRightAxis = -1;
            this.RotationRightDirection = -1;
            this.RotationUpAxis = -1;
            this.RotationUpDirection = -1;
            this.RotationForwardAxis = -1;
            this.RotationForwardDirection = -1;
        }

        private static int GetBestAxis(List<BoxEdge> edgeList, Vector3D fitVector, out int direction)
        {
            double maxValue = double.MaxValue;
            int index = -1;
            direction = 0;
            for (int i = 0; i < edgeList.Count; i++)
            {
                double num4 = Vector3D.Dot(fitVector, edgeList[i].Edge.Direction);
                int num5 = Math.Sign(num4);
                num4 = 1.0 - Math.Abs(num4);
                if (num4 < maxValue)
                {
                    maxValue = num4;
                    index = i;
                    direction = num5;
                }
            }
            edgeList.RemoveAt(index);
            return edgeList[index].Axis;
        }

        private static void GetClosestCubeEdge(Vector3D[] vertices, Vector3D cameraPosition, int[] startIndices, int[] endIndices, out int edgeIndex, out int edgeIndex2)
        {
            edgeIndex = -1;
            edgeIndex2 = -1;
            float maxValue = float.MaxValue;
            float num2 = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3D vectord = (vertices[startIndices[i]] + vertices[endIndices[i]]) * 0.5;
                float num4 = (float) Vector3D.Distance(cameraPosition, vectord);
                if (num4 < maxValue)
                {
                    edgeIndex2 = edgeIndex;
                    edgeIndex = i;
                    num2 = maxValue;
                    maxValue = num4;
                }
                else if (num4 < num2)
                {
                    edgeIndex2 = i;
                    num2 = num4;
                }
            }
        }

        public void ReleaseRenderData()
        {
            MyRenderProxy.RemoveBillboardViewProjection(0);
        }

        public int RotationRightAxis { get; private set; }

        public int RotationRightDirection { get; private set; }

        public int RotationUpAxis { get; private set; }

        public int RotationUpDirection { get; private set; }

        public int RotationForwardAxis { get; private set; }

        public int RotationForwardDirection { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct BoxEdge
        {
            public int Axis;
            public LineD Edge;
        }
    }
}

