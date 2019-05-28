namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.Components;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class MyCubeGrids : MySessionComponentBase
    {
        [CompilerGenerated]
        private static Action<MyCubeGrid, MySlimBlock> BlockBuilt;
        [CompilerGenerated]
        private static Action<MyCubeGrid, MySlimBlock> BlockDestroyed;
        [CompilerGenerated]
        private static Action<MyCubeGrid, MySlimBlock, bool> BlockFinished;
        [CompilerGenerated]
        private static Action<MyCubeGrid, MySlimBlock, bool> BlockFunctional;

        public static  event Action<MyCubeGrid, MySlimBlock> BlockBuilt
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, MySlimBlock> blockBuilt = BlockBuilt;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock> a = blockBuilt;
                    Action<MyCubeGrid, MySlimBlock> action3 = (Action<MyCubeGrid, MySlimBlock>) Delegate.Combine(a, value);
                    blockBuilt = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock>>(ref BlockBuilt, action3, a);
                    if (ReferenceEquals(blockBuilt, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, MySlimBlock> blockBuilt = BlockBuilt;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock> source = blockBuilt;
                    Action<MyCubeGrid, MySlimBlock> action3 = (Action<MyCubeGrid, MySlimBlock>) Delegate.Remove(source, value);
                    blockBuilt = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock>>(ref BlockBuilt, action3, source);
                    if (ReferenceEquals(blockBuilt, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyCubeGrid, MySlimBlock> BlockDestroyed
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, MySlimBlock> blockDestroyed = BlockDestroyed;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock> a = blockDestroyed;
                    Action<MyCubeGrid, MySlimBlock> action3 = (Action<MyCubeGrid, MySlimBlock>) Delegate.Combine(a, value);
                    blockDestroyed = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock>>(ref BlockDestroyed, action3, a);
                    if (ReferenceEquals(blockDestroyed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, MySlimBlock> blockDestroyed = BlockDestroyed;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock> source = blockDestroyed;
                    Action<MyCubeGrid, MySlimBlock> action3 = (Action<MyCubeGrid, MySlimBlock>) Delegate.Remove(source, value);
                    blockDestroyed = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock>>(ref BlockDestroyed, action3, source);
                    if (ReferenceEquals(blockDestroyed, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyCubeGrid, MySlimBlock, bool> BlockFinished
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, MySlimBlock, bool> blockFinished = BlockFinished;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock, bool> a = blockFinished;
                    Action<MyCubeGrid, MySlimBlock, bool> action3 = (Action<MyCubeGrid, MySlimBlock, bool>) Delegate.Combine(a, value);
                    blockFinished = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock, bool>>(ref BlockFinished, action3, a);
                    if (ReferenceEquals(blockFinished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, MySlimBlock, bool> blockFinished = BlockFinished;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock, bool> source = blockFinished;
                    Action<MyCubeGrid, MySlimBlock, bool> action3 = (Action<MyCubeGrid, MySlimBlock, bool>) Delegate.Remove(source, value);
                    blockFinished = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock, bool>>(ref BlockFinished, action3, source);
                    if (ReferenceEquals(blockFinished, source))
                    {
                        return;
                    }
                }
            }
        }

        public static  event Action<MyCubeGrid, MySlimBlock, bool> BlockFunctional
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid, MySlimBlock, bool> blockFunctional = BlockFunctional;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock, bool> a = blockFunctional;
                    Action<MyCubeGrid, MySlimBlock, bool> action3 = (Action<MyCubeGrid, MySlimBlock, bool>) Delegate.Combine(a, value);
                    blockFunctional = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock, bool>>(ref BlockFunctional, action3, a);
                    if (ReferenceEquals(blockFunctional, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid, MySlimBlock, bool> blockFunctional = BlockFunctional;
                while (true)
                {
                    Action<MyCubeGrid, MySlimBlock, bool> source = blockFunctional;
                    Action<MyCubeGrid, MySlimBlock, bool> action3 = (Action<MyCubeGrid, MySlimBlock, bool>) Delegate.Remove(source, value);
                    blockFunctional = Interlocked.CompareExchange<Action<MyCubeGrid, MySlimBlock, bool>>(ref BlockFunctional, action3, source);
                    if (ReferenceEquals(blockFunctional, source))
                    {
                        return;
                    }
                }
            }
        }

        internal static void NotifyBlockBuilt(MyCubeGrid grid, MySlimBlock block)
        {
            BlockBuilt.InvokeIfNotNull<MyCubeGrid, MySlimBlock>(grid, block);
        }

        internal static void NotifyBlockDestroyed(MyCubeGrid grid, MySlimBlock block)
        {
            BlockDestroyed.InvokeIfNotNull<MyCubeGrid, MySlimBlock>(grid, block);
        }

        internal static void NotifyBlockFinished(MyCubeGrid grid, MySlimBlock block, bool handWelded)
        {
            BlockFinished.InvokeIfNotNull<MyCubeGrid, MySlimBlock, bool>(grid, block, handWelded);
        }

        internal static void NotifyBlockFunctional(MyCubeGrid grid, MySlimBlock block, bool handWelded)
        {
            BlockFunctional.InvokeIfNotNull<MyCubeGrid, MySlimBlock, bool>(grid, block, handWelded);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            BlockBuilt = null;
            BlockDestroyed = null;
        }

        private long Now =>
            DateTime.Now.Ticks;
    }
}

