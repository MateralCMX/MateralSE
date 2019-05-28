namespace Sandbox
{
    using System;
    using VRage.Input;
    using VRage.Network;
    using VRage.Profiler;
    using VRage.Replication;
    using VRageRender;

    [StaticEventOwner]
    public class MyRenderProfiler
    {
        public static Action<RenderProfilerCommand, int> ServerInvoke;
        private static bool m_enabled;

        public static void EnableAutoscale(string threadName)
        {
            MyRenderProxy.RenderProfilerInput(RenderProfilerCommand.EnableAutoScale, 0, threadName);
        }

        public static void HandleInput()
        {
            bool flag = false;
            RenderProfilerCommand? nullable = null;
            int num = 0;
            if (MyInput.Static.IsAnyAltKeyPressed())
            {
                if (m_enabled)
                {
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 > 9)
                        {
                            if (MyInput.Static.IsAnyCtrlKeyPressed() && !MyInput.Static.IsKeyPress(MyKeys.Space))
                            {
                                num += 10;
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsKeyPress(MyKeys.Space))
                            {
                                num += 20;
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.IncreaseLocalArea);
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.DecreaseLocalArea);
                                }
                            }
                            else if (MyInput.Static.IsAnyShiftKeyPressed())
                            {
                                if (MyInput.Static.IsKeyPress(MyKeys.Add))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.NextThread);
                                }
                                if (MyInput.Static.IsKeyPress(MyKeys.Subtract))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.PreviousThread);
                                }
                            }
                            else
                            {
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Add))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.NextThread);
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Subtract))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.PreviousThread);
                                }
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.Pause);
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.PageDown))
                                {
                                    num = 1;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.PreviousFrame);
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.PageUp))
                                {
                                    num = 1;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.NextFrame);
                                }
                            }
                            else if (MyInput.Static.IsAnyShiftKeyPressed())
                            {
                                if (MyInput.Static.IsKeyPress(MyKeys.PageDown))
                                {
                                    num = 10;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.PreviousFrame);
                                }
                                if (MyInput.Static.IsKeyPress(MyKeys.PageUp))
                                {
                                    num = 10;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.NextFrame);
                                }
                            }
                            else
                            {
                                if (MyInput.Static.IsKeyPress(MyKeys.PageDown))
                                {
                                    num = 1;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.PreviousFrame);
                                }
                                if (MyInput.Static.IsKeyPress(MyKeys.PageUp))
                                {
                                    num = 1;
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.NextFrame);
                                }
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.Insert))
                            {
                                if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsAnyShiftKeyPressed())
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.ToggleAutoScale);
                                }
                                else if (MyInput.Static.IsAnyCtrlKeyPressed())
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.Reset);
                                }
                                else
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.ChangeSortingOrder);
                                }
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.Home))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.IncreaseRange);
                            }
                            else if (MyInput.Static.IsNewKeyPressed(MyKeys.End))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.DecreaseRange);
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed())
                            {
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.IncreaseLevel);
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Divide))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.DecreaseLevel);
                                }
                            }
                            else
                            {
                                if (MyInput.Static.IsKeyPress(MyKeys.Multiply))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.IncreaseLevel);
                                }
                                if (MyInput.Static.IsKeyPress(MyKeys.Divide))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.DecreaseLevel);
                                }
                            }
                            if (MyInput.Static.IsAnyShiftKeyPressed())
                            {
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Divide))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.CopyPathToClipboard);
                                }
                                else if (MyInput.Static.IsNewKeyPressed(MyKeys.Multiply))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.TryGoToPathInClipboard);
                                }
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.Home))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.JumpToRoot);
                            }
                            if (MyInput.Static.IsAnyCtrlKeyPressed() && MyInput.Static.IsNewKeyPressed(MyKeys.End))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.DisableFrameSelection);
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.S))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.GetFomServer);
                            }
                            if (MyInput.Static.IsNewKeyPressed(MyKeys.C))
                            {
                                nullable = new RenderProfilerCommand?(RenderProfilerCommand.GetFromClient);
                            }
                            byte num3 = 0;
                            while (true)
                            {
                                if (num3 < 9)
                                {
                                    if (!MyInput.Static.IsNewKeyPressed((MyKeys) ((byte) (0x31 + num3))))
                                    {
                                        num3 = (byte) (num3 + 1);
                                        continue;
                                    }
                                    if (MyInput.Static.IsRightCtrlKeyPressed())
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.SubtractFromFile);
                                    }
                                    else if (MyInput.Static.IsLeftCtrlKeyPressed())
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.SaveToFile);
                                    }
                                    else
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.LoadFromFile);
                                    }
                                    if (MyInput.Static.IsAnyShiftKeyPressed())
                                    {
                                        flag = true;
                                    }
                                    num = num3 + 1;
                                }
                                if (MyInput.Static.IsKeyPress(MyKeys.Z))
                                {
                                    RenderProfilerCommand? nullable2 = nullable;
                                    RenderProfilerCommand jumpToLevel = RenderProfilerCommand.JumpToLevel;
                                    if ((((RenderProfilerCommand) nullable2.GetValueOrDefault()) == jumpToLevel) & (nullable2 != null))
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.SwapBlockOptimized);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.Decimal))
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.ResetAllOptimizations);
                                    }
                                    else if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.ToggleOptimizationsEnabled);
                                    }
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.Q))
                                {
                                    if (MyInput.Static.IsAnyShiftKeyPressed())
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.SwitchBlockRender);
                                    }
                                    else
                                    {
                                        nullable = new RenderProfilerCommand?(RenderProfilerCommand.SwitchGraphContent);
                                    }
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.E))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.SwitchShallowProfile);
                                    if (MyInput.Static.IsKeyPress(MyKeys.Shift))
                                    {
                                        flag = true;
                                    }
                                }
                                if (MyInput.Static.IsNewKeyPressed(MyKeys.A))
                                {
                                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.SwitchAverageTimes);
                                    if (MyInput.Static.IsKeyPress(MyKeys.Shift))
                                    {
                                        flag = true;
                                    }
                                }
                                break;
                            }
                            break;
                        }
                        MyKeys key = (MyKeys) ((byte) (0x60 + num2));
                        if (MyInput.Static.IsNewKeyPressed(key))
                        {
                            num = num2;
                            nullable = new RenderProfilerCommand?(RenderProfilerCommand.JumpToLevel);
                        }
                        num2++;
                    }
                }
                if ((nullable == null) && (MyInput.Static.IsNewKeyPressed(MyKeys.Decimal) || (MyInput.Static.IsNewKeyPressed(MyKeys.NumPad0) && !m_enabled)))
                {
                    m_enabled = !m_enabled;
                    nullable = new RenderProfilerCommand?(RenderProfilerCommand.ToggleEnabled);
                }
            }
            if (nullable != null)
            {
                if (flag && (ServerInvoke != null))
                {
                    ServerInvoke(nullable.Value, num);
                }
                else
                {
                    MyRenderProxy.RenderProfilerInput(nullable.Value, num, null);
                }
            }
        }

        [Event(null, 0x137), Reliable, Server]
        public static void OnCommandReceived(RenderProfilerCommand cmd, int payload)
        {
            VRage.Profiler.MyRenderProfiler.HandleInput(cmd, payload, null);
        }

        public static void ToggleProfiler(string threadName)
        {
            m_enabled = !m_enabled;
            MyRenderProxy.RenderProfilerInput(RenderProfilerCommand.ToggleEnabled, 0, threadName);
        }

        private static bool MultiplayerActive =>
            (MyMultiplayerMinimalBase.Instance != null);

        private static bool IsServer =>
            (!MultiplayerActive || MyMultiplayerMinimalBase.Instance.IsServer);
    }
}

