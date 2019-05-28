namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyWorkshopQuery : IDisposable
    {
        public WorkshopItemType ItemType;
        public WorkshopListType ListType;
        [CompilerGenerated]
        private QueryCompletedDelegate QueryCompleted;

        public event QueryCompletedDelegate QueryCompleted
        {
            [CompilerGenerated] add
            {
                QueryCompletedDelegate queryCompleted = this.QueryCompleted;
                while (true)
                {
                    QueryCompletedDelegate a = queryCompleted;
                    QueryCompletedDelegate delegate4 = (QueryCompletedDelegate) Delegate.Combine(a, value);
                    queryCompleted = Interlocked.CompareExchange<QueryCompletedDelegate>(ref this.QueryCompleted, delegate4, a);
                    if (ReferenceEquals(queryCompleted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                QueryCompletedDelegate queryCompleted = this.QueryCompleted;
                while (true)
                {
                    QueryCompletedDelegate source = queryCompleted;
                    QueryCompletedDelegate delegate4 = (QueryCompletedDelegate) Delegate.Remove(source, value);
                    queryCompleted = Interlocked.CompareExchange<QueryCompletedDelegate>(ref this.QueryCompleted, delegate4, source);
                    if (ReferenceEquals(queryCompleted, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyWorkshopQuery()
        {
        }

        public virtual void Dispose()
        {
        }

        ~MyWorkshopQuery()
        {
            this.Dispose();
        }

        protected virtual void OnQueryCompleted(MyGameServiceCallResult result)
        {
            QueryCompletedDelegate queryCompleted = this.QueryCompleted;
            if (queryCompleted != null)
            {
                queryCompleted(result);
            }
        }

        public virtual void Run()
        {
        }

        public List<MyWorkshopItem> Items { get; protected set; }

        public uint TotalResults { get; protected set; }

        public virtual uint ItemsPerPage { get; protected set; }

        public virtual bool IsRunning { get; protected set; }

        public string SearchString { get; set; }

        public List<string> RequiredTags { get; set; }

        public bool RequireAllTags { get; set; }

        public List<string> ExcludedTags { get; set; }

        public ulong UserId { get; set; }

        public List<ulong> ItemIds { get; set; }

        public delegate void QueryCompletedDelegate(MyGameServiceCallResult result);
    }
}

