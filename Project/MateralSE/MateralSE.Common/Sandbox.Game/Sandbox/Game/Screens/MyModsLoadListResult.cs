namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox.Engine.Networking;
    using Sandbox.Game.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.GameServices;

    public class MyModsLoadListResult : IMyAsyncResult
    {
        public MyModsLoadListResult(HashSet<ulong> ids)
        {
            this.Task = Parallel.Start(delegate {
                this.SubscribedMods = new List<MyWorkshopItem>(ids.Count);
                this.SetMods = new List<MyWorkshopItem>();
                if (MyGameService.IsOnline && MyWorkshop.GetSubscribedModsBlocking(this.SubscribedMods))
                {
                    HashSet<ulong> publishedFileIds = new HashSet<ulong>(ids);
                    foreach (MyWorkshopItem item in this.SubscribedMods)
                    {
                        publishedFileIds.Remove(item.Id);
                    }
                    if (publishedFileIds.Count > 0)
                    {
                        MyWorkshop.GetItemsBlockingUGC(publishedFileIds, this.SetMods);
                    }
                }
            });
        }

        public bool IsCompleted =>
            this.Task.IsComplete;

        public ParallelTasks.Task Task { get; private set; }

        public List<MyWorkshopItem> SubscribedMods { get; private set; }

        public List<MyWorkshopItem> SetMods { get; private set; }
    }
}

