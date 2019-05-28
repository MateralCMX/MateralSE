namespace VRageRender.Profiler
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Library.Utils;
    using VRage.Profiler;
    using VRageMath;

    public abstract class MyRenderProfilerRendering : MyRenderProfiler
    {
        private bool m_initialized;
        private readonly NumberFormatInfo m_numberInfo = new NumberFormatInfo();
        private readonly Stack<MyProfiler.TaskInfo> m_taskStack = new Stack<MyProfiler.TaskInfo>(10);

        protected MyRenderProfilerRendering()
        {
        }

        protected abstract void BeginDraw();
        protected abstract void BeginLineBatch();
        protected abstract void BeginPrimitiveBatch();
        protected sealed override void Draw(MyProfiler drawProfiler, int lastFrameIndex, int frameToDraw)
        {
            if (!this.m_initialized)
            {
                this.Init();
                this.m_initialized = true;
                MyRenderProfiler.FpsBlock.Start(false);
            }
            MyTimeSpan? customTime = null;
            MyRenderProfiler.FpsBlock.End(false, customTime);
            float seconds = (float) MyRenderProfiler.FpsBlock.Elapsed.Seconds;
            float num2 = (seconds > 0f) ? (1f / seconds) : 0f;
            MyRenderProfiler.m_fpsPctg = (0.9f * MyRenderProfiler.m_fpsPctg) + (0.1f * num2);
            MyRenderProfiler.FpsBlock.CustomValues[lastFrameIndex] = MyRenderProfiler.FpsBlock.CustomValue;
            MyRenderProfiler.FpsBlock.Reset();
            MyRenderProfiler.FpsBlock.Start(false);
            if (MyRenderProfiler.m_enabled)
            {
                this.BeginDraw();
                if (MyRenderProfiler.m_graphContent != ProfilerGraphContent.Tasks)
                {
                    float num3 = 20f;
                    float num4 = 28f;
                    float y = (this.ViewportSize.Y / 2f) - (11f * num4);
                    base.Text.Clear();
                    switch (MyRenderProfiler.m_dataType)
                    {
                        case SnapshotType.Online:
                            base.Text.Append("Online");
                            break;

                        case SnapshotType.Server:
                            base.Text.Append("Server");
                            break;

                        case SnapshotType.Snapshot:
                            base.Text.Append("Snapshot");
                            break;

                        default:
                            break;
                    }
                    base.Text.AppendLine();
                    string displayName = MyRenderProfiler.m_selectedProfiler.DisplayName;
                    base.Text.ConcatFormat<int, int, string>("\"{2}\" ({0}/{1})", (MyRenderProfiler.ThreadProfilers.IndexOf(MyRenderProfiler.m_selectedProfiler) + 1), MyRenderProfiler.ThreadProfilers.Count, (displayName ?? "Invalid"), null).AppendLine();
                    base.Text.Append("Level limit: ");
                    if (MyRenderProfiler.m_dataType == SnapshotType.Online)
                    {
                        base.Text.AppendInt32(MyRenderProfiler.m_selectedProfiler.LevelLimit);
                    }
                    else
                    {
                        base.Text.Append("Unavailable");
                    }
                    this.DrawText(new Vector2(20f, y), base.Text, Color.LightGray, 1f);
                    y += (num4 * 3f) + 10f;
                    base.Text.Clear();
                    base.Text.Append("Profile type: ").Append(MyRenderProfiler.m_selectedProfiler.PendingShallowProfileState ? "Shallow" : "Deep");
                    this.DrawText(new Vector2(20f, y), base.Text, Color.LightGray, 1f);
                    y += (num4 * 2f) + 10f;
                    base.Text.Clear();
                    base.Text.Append("Frame: ").AppendInt32(frameToDraw).AppendLine();
                    base.Text.Append("Local area: ").AppendInt32(MyRenderProfiler.m_frameLocalArea);
                    this.DrawText(new Vector2(20f, y), base.Text, Color.Yellow, 1f);
                    y += (num4 * 2f) + 10f;
                    base.Text.Clear();
                    base.Text.Append(MyRenderProfiler.FpsBlock.Name).Append(" ");
                    if (!MyRenderProfiler.m_useCustomFrame)
                    {
                        base.Text.AppendDecimal(MyRenderProfiler.m_fpsPctg, 3);
                    }
                    base.Text.AppendLine();
                    base.Text.Append("Total calls: ").AppendInt32(IsValidIndex(frameToDraw, lastFrameIndex) ? MyRenderProfiler.m_selectedProfiler.TotalCalls[frameToDraw] : -1);
                    this.DrawText(new Vector2(20f, y), base.Text, Color.Red, 1f);
                    y += num4;
                    base.Text.Clear();
                    base.Text.Append("MyCompilationSymbols.PerformanceProfiling NOT ENABLED!").AppendLine();
                    if (!MyRenderProfiler.ProfilerProcessingEnabled)
                    {
                        base.Text.Append("Profiler processing disabled, F12 -> Profiler").AppendLine();
                    }
                    this.DrawText(new Vector2(0f, 0f), base.Text, Color.Yellow, 0.6f);
                    y = this.ViewportSize.Y / 2f;
                    List<MyProfilerBlock> selectedRootChildren = MyRenderProfiler.m_selectedProfiler.SelectedRootChildren;
                    List<MyProfilerBlock> sortedChildren = GetSortedChildren(frameToDraw);
                    base.Text.Clear();
                    MyProfilerBlock selectedRoot = MyRenderProfiler.m_selectedProfiler.SelectedRoot;
                    while (true)
                    {
                        if (selectedRoot != null)
                        {
                            if (((selectedRoot.Name.Length + 3) + base.Text.Length) <= 170)
                            {
                                if (base.Text.Length > 0)
                                {
                                    base.Text.Insert(0, " > ");
                                }
                                base.Text.Insert(0, selectedRoot.Name);
                                selectedRoot = selectedRoot.Parent;
                                continue;
                            }
                            base.Text.Insert(0, "... > ");
                        }
                        this.DrawTextShadow(new Vector2(20f, y), base.Text, Color.White, 0.7f);
                        y += num3;
                        if (MyRenderProfiler.m_selectedProfiler.SelectedRoot != null)
                        {
                            Color white = Color.White;
                            this.DrawEvent(y, MyRenderProfiler.m_selectedProfiler.SelectedRoot, MyRenderProfiler.m_selectedProfiler.EnableOptimizations, -1, frameToDraw, lastFrameIndex, ref white, true);
                            y += num3;
                        }
                        if (sortedChildren.Count <= 0)
                        {
                            base.Text.Clear().Append("No more blocks at this point!");
                            y += num3;
                            this.DrawTextShadow(new Vector2(20f, y), base.Text, Color.White, 0.7f);
                            y += num3;
                        }
                        else
                        {
                            float num6;
                            base.Text.Clear().Append(@"\/ ");
                            switch (MyRenderProfiler.m_sortingOrder)
                            {
                                case RenderProfilerSortingOrder.Id:
                                    num6 = 20f;
                                    base.Text.Append("ASC");
                                    break;

                                case RenderProfilerSortingOrder.MillisecondsLastFrame:
                                    num6 = 660f;
                                    base.Text.Append("DESC");
                                    break;

                                case RenderProfilerSortingOrder.AllocatedLastFrame:
                                    num6 = 845f;
                                    base.Text.Append("DESC");
                                    break;

                                case RenderProfilerSortingOrder.MillisecondsAverage:
                                    num6 = 1270f;
                                    base.Text.Append("DESC");
                                    break;

                                default:
                                    throw new Exception("Unhandled enum value " + MyRenderProfiler.m_sortingOrder);
                            }
                            this.DrawTextShadow(new Vector2(num6, y), base.Text, Color.White, 0.7f);
                            y += num3;
                            for (int i = 0; i < sortedChildren.Count; i++)
                            {
                                MyProfilerBlock item = sortedChildren[i];
                                Color darkRed = Color.DarkRed;
                                if (!item.IsOptimized)
                                {
                                    darkRed = IndexToColor(selectedRootChildren.IndexOf(item));
                                }
                                this.DrawEvent(y, item, MyRenderProfiler.m_selectedProfiler.EnableOptimizations, i, frameToDraw, lastFrameIndex, ref darkRed, false);
                                y += num3;
                            }
                        }
                        break;
                    }
                }
                this.BeginLineBatch();
                this.BeginPrimitiveBatch();
                this.DrawPerfEvents(lastFrameIndex);
                this.EndPrimitiveBatch();
                this.EndLineBatch();
            }
            if (!MyRenderProfiler.Paused)
            {
                MyRenderProfiler.m_selectedFrame = lastFrameIndex;
            }
        }

        private void DrawBlockLine(MyProfilerBlock.DataReader data, int start, int end, MyDrawArea area, Color color)
        {
            Vector3 zero = Vector3.Zero;
            Vector3 vector2 = Vector3.Zero;
            for (int i = start + 1; i <= end; i++)
            {
                zero.X = (-1f + area.XStart) + ((area.XScale * (i - 1)) / 512f);
                zero.Y = area.YStart + ((data[i - 1] * area.YScale) / area.YRange);
                zero.Z = 0f;
                vector2.X = (-1f + area.XStart) + ((area.XScale * i) / 512f);
                vector2.Y = area.YStart + ((data[i] * area.YScale) / area.YRange);
                vector2.Z = 0f;
                if (((zero.Y - area.YStart) > 0.001f) || ((vector2.Y - area.YStart) > 0.001f))
                {
                    this.DrawOnScreenLine(zero, vector2, color);
                }
            }
        }

        private void DrawBlockLineSeparated(MyProfilerBlock.DataReader data, int lastFrameIndex, int windowEnd, MyDrawArea scale, Color color)
        {
            if (lastFrameIndex > windowEnd)
            {
                this.DrawBlockLine(data, windowEnd, lastFrameIndex, scale, color);
            }
            else
            {
                this.DrawBlockLine(data, 0, lastFrameIndex, scale, color);
                this.DrawBlockLine(data, windowEnd, MyProfiler.MAX_FRAMES - 1, scale, color);
            }
        }

        private void DrawCustomFrameLine()
        {
            if ((MyRenderProfiler.m_useCustomFrame && (MyRenderProfiler.m_selectedFrame >= 0)) && (MyRenderProfiler.m_selectedFrame < MyProfiler.MAX_FRAMES))
            {
                Vector3 vector;
                Vector3 vector2;
                vector.X = (-1f + MyRenderProfiler.MemoryGraphScale.XStart) + ((MyRenderProfiler.MemoryGraphScale.XScale * MyRenderProfiler.m_selectedFrame) / 512f);
                vector.Y = MyRenderProfiler.MemoryGraphScale.YStart;
                vector.Z = 0f;
                vector2.X = vector.X;
                vector2.Y = 1f;
                vector2.Z = 0f;
                this.DrawOnScreenLine(vector, vector2, Color.Yellow);
            }
        }

        private void DrawEvent(float textPosY, MyProfilerBlock profilerBlock, bool useOptimizations, int blockIndex, int frameIndex, int lastValidFrame, ref Color color, bool isHeaderLine)
        {
            int num6;
            MyProfilerBlock.DataReader allocationsReader;
            int num = -1;
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            if (IsValidIndex(frameIndex, lastValidFrame))
            {
                num = profilerBlock.NumCallsArray[frameIndex];
                num3 = profilerBlock.CustomValues[frameIndex];
                num4 = profilerBlock.GetMillisecondsReader(useOptimizations)[frameIndex];
                allocationsReader = profilerBlock.GetAllocationsReader(useOptimizations);
                num2 = allocationsReader[frameIndex];
            }
            if (blockIndex >= 0)
            {
                base.Text.Clear().Append((int) (blockIndex + 1)).Append(" ");
            }
            else
            {
                base.Text.Clear().Append("- ");
            }
            if (MyRenderProfiler.m_selectedProfiler.ShallowProfileEnabled && profilerBlock.IsDeepTreeRoot)
            {
                base.Text.Append("[S] ");
            }
            switch (profilerBlock.BlockType)
            {
                case MyProfilerBlock.BlockTypes.Diffed:
                    base.Text.Append("[D] ");
                    break;

                case MyProfilerBlock.BlockTypes.Inverted:
                    base.Text.Append("[I] ");
                    break;

                case MyProfilerBlock.BlockTypes.Added:
                    base.Text.Append("[A] ");
                    break;

                default:
                    break;
            }
            BlockRender blockRender = MyRenderProfiler.m_blockRender;
            if (blockRender == BlockRender.Name)
            {
                base.Text.Append(profilerBlock.Name);
            }
            else if (blockRender == BlockRender.Source)
            {
                string file = profilerBlock.Key.File;
                int startIndex = 0;
                int length = file.Length;
                if (length > 40)
                {
                    startIndex = length - 40;
                    length = 40;
                }
                base.Text.Append(file, startIndex, length).Append(':').Append(profilerBlock.Key.Line);
            }
            this.DrawTextShadow(new Vector2(20f, textPosY), base.Text, color, 0.7f);
            float num5 = 500f;
            base.Text.Clear();
            base.Text.Append("(").Append(profilerBlock.Children.Count).Append(") ");
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
            num5 += 35f;
            base.Text.Clear();
            num5 += 108.5f;
            if (isHeaderLine)
            {
                float num10 = 0f;
                foreach (MyProfilerBlock block in profilerBlock.Children)
                {
                    allocationsReader = block.GetMillisecondsReader(useOptimizations);
                    num10 += allocationsReader[frameIndex];
                }
                base.Text.Clear();
                base.Text.ConcatFormat<float>(profilerBlock.TimeFormat ?? "{0:.00}", num10, null);
                this.DrawTextShadow(new Vector2((20f + num5) - 63f, textPosY), base.Text, color, 0.7f);
                base.Text.Clear().Append('/');
                this.DrawTextShadow(new Vector2((20f + num5) - 14f, textPosY), base.Text, color, 0.7f);
            }
            base.Text.Clear();
            base.Text.ConcatFormat<float>(profilerBlock.TimeFormat ?? "{0:.00}ms", num4, null);
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
            num5 += 108.5f;
            base.Text.Clear();
            base.Text.Append(isHeaderLine ? "-- / -- B" : "    -- B");
            this.DrawTextShadow(new Vector2(100f + num5, textPosY), base.Text, color, 0.7f);
            num5 += 150.6f;
            base.Text.Clear();
            num5 += 68f;
            base.Text.ConcatFormat<int>(profilerBlock.CallFormat ?? "{0} calls", num, null);
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
            num5 += 105f;
            base.Text.Clear();
            base.Text.ConcatFormat<float>(profilerBlock.ValueFormat ?? "Custom: {0:.00}", num3, null);
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
            num5 += 175f;
            float num7 = FindMaxWrap(profilerBlock.GetMillisecondsReader(useOptimizations), frameIndex - (MyRenderProfiler.m_frameLocalArea / 2), frameIndex + (MyRenderProfiler.m_frameLocalArea / 2), lastValidFrame, out num6);
            base.Text.Clear();
            base.Text.ConcatFormat<float>(profilerBlock.TimeFormat ?? "{0:.00}ms", num7, null);
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
            this.DrawTextShadow(new Vector2(20f + num5, textPosY), base.Text, color, 0.7f);
        }

        private void DrawGraphs(int lastFrameIndex)
        {
            int windowEnd = GetWindowEnd(lastFrameIndex);
            MyDrawArea currentGraphScale = GetCurrentGraphScale();
            List<MyProfilerBlock> selectedRootChildren = MyRenderProfiler.m_selectedProfiler.SelectedRootChildren;
            if ((MyRenderProfiler.m_selectedProfiler.SelectedRoot != null) && (!MyRenderProfiler.m_selectedProfiler.IgnoreRoot || (selectedRootChildren.Count == 0)))
            {
                this.DrawBlockLineSeparated(GetGraphData(MyRenderProfiler.m_selectedProfiler.SelectedRoot), lastFrameIndex, windowEnd, currentGraphScale, Color.White);
            }
            for (int i = 0; i < selectedRootChildren.Count; i++)
            {
                this.DrawBlockLineSeparated(GetGraphData(selectedRootChildren[i]), lastFrameIndex, windowEnd, currentGraphScale, IndexToColor(i));
            }
        }

        private void DrawLegend()
        {
            Color color = new Color(200, 200, 200);
            Color color2 = new Color(130, 130, 130);
            MyDrawArea currentGraphScale = GetCurrentGraphScale();
            float xStart = currentGraphScale.XStart;
            float num2 = 0.01f;
            this.DrawOnScreenLine(new Vector3(-1f + xStart, 0f, 0f), new Vector3(-1f + xStart, currentGraphScale.YScale, 0f), color);
            float x = this.ViewportSize.X;
            float y = this.ViewportSize.Y;
            int num5 = 0;
            float yLegendMsIncrement = currentGraphScale.YLegendMsIncrement;
            while ((yLegendMsIncrement != ((int) yLegendMsIncrement)) && (num5 < 5))
            {
                yLegendMsIncrement *= 10f;
                num5++;
            }
            this.m_numberInfo.NumberDecimalDigits = num5;
            for (int i = 0; i <= currentGraphScale.YLegendMsCount; i++)
            {
                base.Text.Clear();
                base.Text.ConcatFormat<float>("{0}", i * currentGraphScale.YLegendMsIncrement, this.m_numberInfo);
                Vector2 vector = this.MeasureText(base.Text, 0.7f);
                this.DrawText(new Vector2(((((0.5f * x) * xStart) - vector.X) - 6f) + (3f * num2), (-10f + (0.5f * y)) - (((currentGraphScale.YLegendIncrement * i) * 0.5f) * y)), base.Text, Color.Silver, 0.7f);
                Vector3 vector2 = new Vector3(-1f + xStart, i * currentGraphScale.YLegendIncrement, 0f);
                Vector3 vector3 = new Vector3(vector2.X + (currentGraphScale.XScale * 2f), i * currentGraphScale.YLegendIncrement, 0f);
                this.DrawOnScreenLine(vector2, vector3, color2);
            }
            base.Text.Clear().Append((MyRenderProfiler.m_graphContent == ProfilerGraphContent.Elapsed) ? MyRenderProfiler.m_selectedProfiler.AxisName : "[B/Tick]");
            this.DrawText(new Vector2((((0.5f * x) * xStart) - 25f) + (3f * num2), (-10f + (0.5f * y)) - (((currentGraphScale.YScale * 0.5f) * y) * 1.05f)), base.Text, Color.Silver, 0.7f);
        }

        protected abstract void DrawOnScreenLine(Vector3 v0, Vector3 v1, Color color);
        private void DrawOnScreenLine(float x1, float y1, float x2, float y2, Color color)
        {
            this.DrawOnScreenLine(new Vector3(x1, y1, 0f), new Vector3(x2, y2, 0f), color);
        }

        private void DrawOnScreenQuad(float x1, float y1, float x2, float y2, Color color)
        {
            this.DrawOnScreenQuad(new Vector3(x1, y1, 0f), new Vector3(x2, y1, 0f), new Vector3(x2, y2, 0f), new Vector3(x1, y2, 0f), color);
        }

        protected abstract void DrawOnScreenQuad(Vector3 v0, Vector3 v1, Vector3 v3, Vector3 v4, Color color);
        private void DrawOnScreenQuadOutline(float x1, float y1, float x2, float y2, Color color)
        {
            this.DrawOnScreenLine(x1, y1, x2, y1, color);
            this.DrawOnScreenLine(x2, y1, x2, y2, color);
            this.DrawOnScreenLine(x2, y2, x1, y2, color);
            this.DrawOnScreenLine(x1, y2, x1, y1, color);
        }

        protected abstract void DrawOnScreenTriangle(Vector3 v0, Vector3 v1, Vector3 v3, Color color);
        private void DrawPerfEvents(int lastFrameIndex)
        {
            if (MyRenderProfiler.m_graphContent == ProfilerGraphContent.Tasks)
            {
                this.DrawTasks(GetCurrentGraphScale());
            }
            else
            {
                UpdateAutoScale(lastFrameIndex);
                this.DrawLegend();
                this.DrawGraphs(lastFrameIndex);
                this.DrawCustomFrameLine();
            }
        }

        private void DrawProfilerInfo(MyProfiler profiler, MyProfiler.TaskInfo? taskArg, float y, Color color)
        {
            float x = 100f;
            base.Text.Clear();
            base.Text.Append(profiler.DisplayName);
            this.DrawTextShadow(x, y, base.Text, color, 0.7f);
            x += 245f;
            if (taskArg == null)
            {
                base.Text.Clear();
                base.Text.Append("No tasks");
                this.DrawTextShadow(x, y, base.Text, color, 0.7f);
                x += 770f;
            }
            else
            {
                MyProfiler.TaskInfo info = taskArg.Value;
                base.Text.Clear();
                base.Text.Append(info.Name);
                this.DrawTextShadow(x, y, base.Text, color, 0.7f);
                x += 560f;
                base.Text.Clear();
                base.Text.AppendDecimal(MyTimeSpan.FromTicks(info.Finished - info.Started).Milliseconds, 2);
                this.DrawTextShadow(x, y, base.Text, color, 0.7f);
                x += 70f;
                base.Text.Clear();
                if (info.Scheduled <= 0L)
                {
                    base.Text.Append("-----");
                }
                else
                {
                    long ticks = info.Started - info.Scheduled;
                    base.Text.AppendDecimal(MyTimeSpan.FromTicks(ticks).Milliseconds, 2);
                }
                this.DrawTextShadow(x, y, base.Text, color, 0.7f);
                x += 70f;
                base.Text.Clear();
                base.Text.Append(info.CustomValue);
                this.DrawTextShadow(x, y, base.Text, color, 0.7f);
                x += 70f;
            }
        }

        private void DrawTask(ref MyProfiler.TaskInfo task, long timeBegin, long timeEnd, float y, MyDrawArea area, Color color, int taskDepth, bool isSelected = false)
        {
            long started = task.Started;
            if (started < timeBegin)
            {
                started = timeBegin;
            }
            float single1 = -1f + area.XStart;
            float num2 = single1 + (area.XScale * 2f);
            float yStart = area.YStart;
            float num4 = yStart + area.YScale;
            float num5 = timeEnd - timeBegin;
            float num6 = started - timeBegin;
            float num9 = MathHelper.Lerp(single1, num2, num6 / num5);
            float num10 = MathHelper.Lerp(single1, num2, ((float) (task.Finished - timeBegin)) / num5);
            float num11 = taskDepth * 0.01f;
            float num12 = MathHelper.Lerp(yStart, num4, y);
            float num13 = MathHelper.Lerp(yStart, num4, (y + 0.05f) - num11);
            float a = 1f;
            bool flag1 = (num10 - num9) > 0.004;
            if (flag1)
            {
                a = MathHelper.Lerp(0.4f, 0.9f, MathHelper.Clamp((float) ((num5 - 10000f) / 1000000f), (float) 0f, (float) 1f));
            }
            this.DrawOnScreenQuad(num9, num12, num10, num13, color.Shade(1.1f).Alpha(a));
            if (isSelected)
            {
                color = Color.Black;
            }
            if (flag1)
            {
                this.DrawOnScreenQuadOutline(num9, num12, num10, num13, color);
            }
        }

        private void DrawTaskProfilerHeader()
        {
            float x = 100f;
            Color white = Color.White;
            float y = 0.53f * this.ViewportSize.Y;
            base.Text.Clear();
            base.Text.Append("Thread name");
            this.DrawTextShadow(x, y, base.Text, white, 0.7f);
            x = (x + 245f) - 14f;
            base.Text.Clear();
            base.Text.Append("Task name");
            this.DrawTextShadow(x, y, base.Text, white, 0.7f);
            x += 560f;
            base.Text.Clear();
            base.Text.Append("Duration");
            this.DrawTextShadow(x, y, base.Text, white, 0.7f);
            x += 70f;
            base.Text.Clear();
            base.Text.Append("Run delay");
            this.DrawTextShadow(x, y, base.Text, white, 0.7f);
            x += 70f;
            base.Text.Clear();
            base.Text.Append("Custom");
            this.DrawTextShadow(x, y, base.Text, white, 0.7f);
            x += 70f;
        }

        private void DrawTasks(MyDrawArea drawArea)
        {
            long num;
            long num2;
            this.DrawTaskProfilerHeader();
            if (MyRenderProfiler.Paused)
            {
                num = MyRenderProfiler.m_targetTaskRenderTime + MyRenderProfiler.m_taskRenderDispersion;
                num2 = MyRenderProfiler.m_targetTaskRenderTime - MyRenderProfiler.m_taskRenderDispersion;
            }
            else
            {
                num = MyProfiler.LastFrameTime + MyTimeSpan.FromMilliseconds(1.0).Ticks;
                num2 = num - (MyRenderProfiler.m_taskRenderDispersion * 2L);
            }
            double num3 = -1f + drawArea.XStart;
            double num4 = num3 + (drawArea.XScale * 2f);
            double yStart = drawArea.YStart;
            double y = yStart + drawArea.YScale;
            double num7 = num - num2;
            foreach (MyRenderProfiler.FrameInfo info in MyRenderProfiler.FrameTimestamps)
            {
                long time = info.Time;
                if ((time > num2) && (time < num))
                {
                    double x = MathHelper.Lerp(num3, num4, ((double) (time - num2)) / num7);
                    Vector3 vector = new Vector3(x, yStart, 0.0);
                    Vector3 vector2 = new Vector3(x, y, 0.0);
                    Color yellow = Color.Yellow;
                    yellow.A = 20;
                    this.DrawOnScreenLine(vector, vector2, yellow);
                    base.Text.Clear();
                    base.Text.Append((long) (info.FrameNumber % ((long) 100)));
                    this.DrawTextShadow((((float) (1.0 + x)) * this.ViewportSize.X) / 2f, (((float) (yStart + 1.0)) * this.ViewportSize.Y) / 2f, base.Text, Color.Red, 0.9f);
                }
            }
            List<MyProfiler> threadProfilers = MyRenderProfiler.ThreadProfilers;
            lock (threadProfilers)
            {
                float num11 = 0.05f;
                using (List<MyProfiler>.Enumerator enumerator2 = MyRenderProfiler.ThreadProfilers.GetEnumerator())
                {
                    goto TR_0030;
                TR_0004:
                    num11 += 0.06f;
                TR_0030:
                    while (true)
                    {
                        if (!enumerator2.MoveNext())
                        {
                            break;
                        }
                        MyProfiler current = enumerator2.Current;
                        string displayName = current.DisplayName;
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            object taskLock = current.TaskLock;
                            lock (taskLock)
                            {
                                MyProfiler.TaskInfo info2;
                                Color red;
                                if (current.FinishedTasks.Count == 0)
                                {
                                    continue;
                                }
                                double num12 = (-MathHelper.Lerp(yStart, y, (double) (num11 + 0.05f)) + 1.0) / 2.0;
                                base.Text.Clear();
                                base.Text.Append(displayName);
                                this.DrawTextShadow((0.1f * this.ViewportSize.X) / 2f, ((float) num12) * this.ViewportSize.Y, base.Text, Color.White, 0.5f);
                                float num13 = ((float) MathHelper.Lerp(yStart, y, (double) num11)) + 0.002f;
                                this.DrawOnScreenLine(-1f, num13, 1f, num13, Color.Black.Alpha(0.5f));
                                int taskDepth = 0;
                                Color white = Color.White;
                                MyProfiler.TaskInfo? taskArg = null;
                                float num15 = (((num11 + 0.1f) + 1f) / 2f) * this.ViewportSize.Y;
                                MyQueue<MyProfiler.TaskInfo> finishedTasks = current.FinishedTasks;
                                this.m_taskStack.Clear();
                                int num16 = 0;
                                goto TR_0027;
                            TR_000A:
                                num16++;
                                goto TR_0027;
                            TR_000D:
                                if (taskArg != null)
                                {
                                    MyProfiler.TaskInfo task = taskArg.Value;
                                    this.DrawTask(ref task, num2, num, num11, drawArea, white, taskDepth, false);
                                }
                                white = red;
                                taskArg = new MyProfiler.TaskInfo?(info2);
                                taskDepth = this.m_taskStack.Count - 1;
                                goto TR_000A;
                            TR_0017:
                                if (!(red != Color.Transparent))
                                {
                                    goto TR_000A;
                                }
                                else
                                {
                                    while (true)
                                    {
                                        if ((this.m_taskStack.Count != 0) && (this.m_taskStack.Peek().Finished < info2.Finished))
                                        {
                                            this.m_taskStack.Pop();
                                            continue;
                                        }
                                        this.m_taskStack.Push(info2);
                                        if ((taskArg == null) || ((info2.Started < num2) && (info2.Finished < taskArg.Value.Finished)))
                                        {
                                            break;
                                        }
                                        this.DrawTask(ref info2, num2, num, num11, drawArea, red, this.m_taskStack.Count - 1, false);
                                        goto TR_000A;
                                    }
                                }
                                goto TR_000D;
                            TR_0018:
                                red = Color.White.Alpha(0.1f);
                                goto TR_0017;
                            TR_0027:
                                while (true)
                                {
                                    if (num16 < finishedTasks.Count)
                                    {
                                        info2 = finishedTasks[num16];
                                        if ((info2.Finished >= num2) && (num >= info2.Started))
                                        {
                                            MyProfiler.TaskType taskType = info2.TaskType;
                                            switch (taskType)
                                            {
                                                case MyProfiler.TaskType.None:
                                                case MyProfiler.TaskType.WorkItem:
                                                case MyProfiler.TaskType.Precalc:
                                                    goto TR_0018;

                                                case MyProfiler.TaskType.Wait:
                                                    red = Color.Transparent;
                                                    goto TR_0017;

                                                case MyProfiler.TaskType.SyncWait:
                                                    break;

                                                case MyProfiler.TaskType.Block:
                                                    red = Color.AliceBlue;
                                                    goto TR_0017;

                                                case MyProfiler.TaskType.Physics:
                                                    red = Color.LightGreen;
                                                    goto TR_0017;

                                                case MyProfiler.TaskType.RenderCull:
                                                case MyProfiler.TaskType.PreparePass:
                                                case MyProfiler.TaskType.RenderPass:
                                                case MyProfiler.TaskType.ClipMap:
                                                    red = Color.Blue;
                                                    goto TR_0017;

                                                case MyProfiler.TaskType.Voxels:
                                                    red = Color.Orange;
                                                    goto TR_0017;

                                                case MyProfiler.TaskType.Deformations:
                                                    red = Color.DarkGreen;
                                                    goto TR_0017;

                                                default:
                                                    if (taskType == MyProfiler.TaskType.HK_AwaitTasks)
                                                    {
                                                        break;
                                                    }
                                                    goto TR_0018;
                                            }
                                            red = Color.Red;
                                            goto TR_0017;
                                        }
                                        goto TR_000A;
                                    }
                                    else
                                    {
                                        if (taskArg != null)
                                        {
                                            MyProfiler.TaskInfo task = taskArg.Value;
                                            this.DrawTask(ref task, num2, num, num11, drawArea, white.Shade(0.5f), taskDepth, true);
                                        }
                                        this.DrawProfilerInfo(current, taskArg, num15, white.Alpha(1f));
                                        goto TR_0004;
                                    }
                                    break;
                                }
                                goto TR_0018;
                            }
                            goto TR_0004;
                        }
                    }
                }
            }
        }

        protected abstract float DrawText(Vector2 screenCoord, StringBuilder text, Color color, float scale);
        protected abstract float DrawTextShadow(Vector2 screenCoord, StringBuilder text, Color color, float scale);
        private void DrawTextShadow(float x, float y, StringBuilder text, Color color, float scale)
        {
            this.DrawText(new Vector2(x, y), text, color, scale);
        }

        protected abstract void EndLineBatch();
        protected abstract void EndPrimitiveBatch();
        private static float GetAppropriateMemoryUnits(float value, out string units)
        {
            if (value < 10240f)
            {
                units = "B";
                return 1f;
            }
            if (value < 1.048576E+07f)
            {
                units = "KB";
                return 0.0009765625f;
            }
            units = "MB";
            return 9.536743E-07f;
        }

        protected abstract void Init();
        protected abstract Vector2 MeasureText(StringBuilder text, float scale);

        protected abstract Vector2 ViewportSize { get; }
    }
}

