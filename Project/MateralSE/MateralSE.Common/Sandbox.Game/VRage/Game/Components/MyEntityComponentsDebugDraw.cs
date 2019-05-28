namespace VRage.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [PreloadRequired]
    public class MyEntityComponentsDebugDraw
    {
        public static void DebugDraw()
        {
            if ((MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_ENTITY_COMPONENTS) && (MySector.MainCamera != null))
            {
                double num = 1.5;
                double lineSize = num * 0.045;
                double num3 = 0.5;
                Vector3D position = MySector.MainCamera.Position;
                Vector3D up = MySector.MainCamera.WorldMatrix.Up;
                Vector3D right = MySector.MainCamera.WorldMatrix.Right;
                Vector3D forwardVector = MySector.MainCamera.ForwardVector;
                BoundingSphereD boundingSphere = new BoundingSphereD(position, 5.0);
                List<MyEntity> entitiesInSphere = MyEntities.GetEntitiesInSphere(ref boundingSphere);
                Vector3D zero = Vector3D.Zero;
                Vector3D vectord6 = Vector3D.Zero;
                MatrixD viewProjectionMatrix = MySector.MainCamera.ViewProjectionMatrix;
                Rectangle safeGuiRectangle = MyGuiManager.GetSafeGuiRectangle();
                float num4 = ((float) safeGuiRectangle.Height) / ((float) safeGuiRectangle.Width);
                float num5 = 600f;
                float num6 = num5 * num4;
                Vector3D vectord1 = position + (1.0 * forwardVector);
                Vector3D vectord7 = Vector3D.Transform(vectord1, viewProjectionMatrix);
                Vector3D vectord8 = Vector3D.Transform(vectord1 + (Vector3D.Right * 0.10000000149011612), viewProjectionMatrix);
                Vector3D vectord9 = Vector3D.Transform(vectord1 + (Vector3D.Up * 0.10000000149011612), viewProjectionMatrix);
                Vector3D vectord10 = Vector3D.Transform(vectord1 + (Vector3D.Backward * 0.10000000149011612), viewProjectionMatrix);
                Vector2 vector = new Vector2(((float) vectord7.X) * num5, (((float) vectord7.Y) * -num6) * num4);
                Vector2 vector2 = new Vector2(((float) vectord8.X) * num5, (((float) vectord8.Y) * -num6) * num4) - vector;
                Vector2 vector3 = new Vector2(((float) vectord9.X) * num5, (((float) vectord9.Y) * -num6) * num4) - vector;
                Vector2 vector4 = new Vector2(((float) vectord10.X) * num5, (((float) vectord10.Y) * -num6) * num4) - vector;
                float num7 = 150f;
                Vector2 vector1 = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + new Vector2(-num7, 0f);
                Vector2 vector6 = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + new Vector2(0f, -num7);
                Matrix? projection = null;
                Vector2 pointFrom = (MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + (MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + new Vector2(-num7, -num7))) * 0.5f;
                MyRenderProxy.DebugDrawLine2D(pointFrom, ((MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + (MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(new Vector2(1f, 1f), false) + new Vector2(-num7, -num7))) * 0.5f) + vector2, Color.Red, Color.Red, projection, false);
                projection = null;
                MyRenderProxy.DebugDrawLine2D(pointFrom, pointFrom + vector3, Color.Green, Color.Green, projection, false);
                projection = null;
                MyRenderProxy.DebugDrawLine2D(pointFrom, pointFrom + vector4, Color.Blue, Color.Blue, projection, false);
                MyRenderProxy.DebugDrawText2D(pointFrom + vector2, "World X", Color.Red, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
                MyRenderProxy.DebugDrawText2D(pointFrom + vector3, "World Y", Color.Green, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
                MyRenderProxy.DebugDrawText2D(pointFrom + vector4, "World Z", Color.Blue, 0.5f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false);
                MyComponentsDebugInputComponent.DetectedEntities.Clear();
                foreach (MyEntity entity in entitiesInSphere)
                {
                    if (entity.PositionComp != null)
                    {
                        Vector3D vectord12;
                        Vector3D vectord11 = entity.PositionComp.GetPosition();
                        Vector3D vectord13 = (vectord11 + (up * 0.10000000149011612)) - (right * num3);
                        if (Vector3D.Dot(Vector3D.Normalize(vectord11 - position), forwardVector) < 0.9995)
                        {
                            MyRenderProxy.DebugDrawSphere(vectord11, 0.01f, Color.White, 1f, false, false, true, false);
                            MyRenderProxy.DebugDrawArrow3D(vectord11, vectord11 + (entity.PositionComp.WorldMatrix.Right * 0.30000001192092896), Color.Red, new Color?(Color.Red), false, 0.1, "X", 0.5f, false);
                            MyRenderProxy.DebugDrawArrow3D(vectord11, vectord11 + (entity.PositionComp.WorldMatrix.Up * 0.30000001192092896), Color.Green, new Color?(Color.Green), false, 0.1, "Y", 0.5f, false);
                            MyRenderProxy.DebugDrawArrow3D(vectord11, vectord11 + (entity.PositionComp.WorldMatrix.Backward * 0.30000001192092896), Color.Blue, new Color?(Color.Blue), false, 0.1, "Z", 0.5f, false);
                            continue;
                        }
                        if (Vector3D.Distance(vectord11, zero) < 0.01)
                        {
                            vectord6 += right * 0.30000001192092896;
                            up = -up;
                            vectord12 = vectord11 + (up * 0.10000000149011612);
                            vectord13 = vectord12 - (right * num3);
                        }
                        zero = vectord11;
                        double num9 = Math.Atan(num / Math.Max(Vector3D.Distance(vectord13, position), 0.001));
                        float num10 = 0f;
                        Dictionary<Type, MyComponentBase>.ValueCollection.Enumerator enumerator = entity.Components.GetEnumerator();
                        MyComponentBase component = null;
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                num10 = (num10 + 1f) - GetComponentLines(component, true);
                                enumerator.Dispose();
                                Vector3D pointTo = vectord13 + (((num10 + 0.5f) * up) * lineSize);
                                Vector3D worldCoord = (vectord13 + (((num10 + 1f) * up) * lineSize)) + (0.0099999997764825821 * right);
                                MyRenderProxy.DebugDrawLine3D(vectord11, vectord12, Color.White, Color.White, false, false);
                                MyRenderProxy.DebugDrawLine3D(vectord13, vectord12, Color.White, Color.White, false, false);
                                MyRenderProxy.DebugDrawLine3D(vectord13, pointTo, Color.White, Color.White, false, false);
                                MyRenderProxy.DebugDrawLine3D(pointTo, pointTo + (right * 1.0), Color.White, Color.White, false, false);
                                MyRenderProxy.DebugDrawText3D(worldCoord, entity.GetType().ToString() + " - " + entity.DisplayName, Color.Orange, (float) num9, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
                                MyComponentsDebugInputComponent.DetectedEntities.Add(entity);
                                foreach (MyComponentBase base3 in entity.Components)
                                {
                                    worldCoord = vectord13 + ((num10 * up) * lineSize);
                                    DebugDrawComponent(base3, worldCoord, right, up, lineSize, (float) num9);
                                    MyEntityComponentBase base4 = base3 as MyEntityComponentBase;
                                    string text = (base4 == null) ? "" : base4.ComponentTypeDebugString;
                                    MyRenderProxy.DebugDrawText3D(worldCoord - (0.019999999552965164 * right), text, Color.Yellow, (float) num9, false, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, -1, false);
                                    num10 -= GetComponentLines(base3, true);
                                }
                                break;
                            }
                            component = enumerator.Current;
                            num10 += GetComponentLines(component, true);
                        }
                    }
                }
                entitiesInSphere.Clear();
            }
        }

        private static void DebugDrawComponent(MyComponentBase component, Vector3D origin, Vector3D rightVector, Vector3D upVector, double lineSize, float textSize)
        {
            Vector3D vectord = rightVector * 0.02500000037252903;
            Vector3D vectord2 = origin + (vectord * 3.5);
            MyRenderProxy.DebugDrawLine3D(origin, origin + (2.0 * vectord), Color.White, Color.White, false, false);
            MyRenderProxy.DebugDrawText3D((origin + (2.0 * vectord)) + (rightVector * 0.014999999664723873), component.ToString(), Color.White, textSize, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
            if ((component is IMyComponentAggregate) && ((component as IMyComponentAggregate).ChildList.Reader.Count != 0))
            {
                MyRenderProxy.DebugDrawLine3D(vectord2 - ((0.5 * lineSize) * upVector), vectord2 - (((GetComponentLines(component, false) - 1) * lineSize) * upVector), Color.White, Color.White, false, false);
                vectord2 -= (1.0 * lineSize) * upVector;
                foreach (MyComponentBase local1 in (component as IMyComponentAggregate).ChildList.Reader)
                {
                    int componentLines = GetComponentLines(local1, true);
                    DebugDrawComponent(local1, vectord2, rightVector, upVector, lineSize, textSize);
                    vectord2 -= (componentLines * lineSize) * upVector;
                }
            }
        }

        private static int GetComponentLines(MyComponentBase component, bool countAll = true)
        {
            int num = 1;
            if (component is IMyComponentAggregate)
            {
                int count = (component as IMyComponentAggregate).ChildList.Reader.Count;
                int num3 = 0;
                foreach (MyComponentBase base2 in (component as IMyComponentAggregate).ChildList.Reader)
                {
                    num3++;
                    num = !((num3 < count) | countAll) ? (num + 1) : (num + GetComponentLines(base2, true));
                }
            }
            return num;
        }
    }
}

