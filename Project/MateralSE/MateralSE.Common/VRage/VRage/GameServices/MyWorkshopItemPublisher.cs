namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class MyWorkshopItemPublisher : IDisposable
    {
        public List<string> Tags = new List<string>();
        public List<ulong> Dependencies = new List<ulong>();
        public HashSet<uint> DLCs = new HashSet<uint>();
        [CompilerGenerated]
        private PublishItemResult ItemPublished;
        public static IComparer<MyWorkshopItem> NameComparer = new MyNameComparer();

        public event PublishItemResult ItemPublished
        {
            [CompilerGenerated] add
            {
                PublishItemResult itemPublished = this.ItemPublished;
                while (true)
                {
                    PublishItemResult a = itemPublished;
                    PublishItemResult result3 = (PublishItemResult) Delegate.Combine(a, value);
                    itemPublished = Interlocked.CompareExchange<PublishItemResult>(ref this.ItemPublished, result3, a);
                    if (ReferenceEquals(itemPublished, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                PublishItemResult itemPublished = this.ItemPublished;
                while (true)
                {
                    PublishItemResult source = itemPublished;
                    PublishItemResult result3 = (PublishItemResult) Delegate.Remove(source, value);
                    itemPublished = Interlocked.CompareExchange<PublishItemResult>(ref this.ItemPublished, result3, source);
                    if (ReferenceEquals(itemPublished, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyWorkshopItemPublisher()
        {
        }

        public virtual void Dispose()
        {
            if (this.Tags != null)
            {
                this.Tags.Clear();
            }
            if (this.Dependencies != null)
            {
                this.Dependencies.Clear();
            }
            if (this.DLCs != null)
            {
                this.DLCs.Clear();
            }
            this.Metadata = null;
        }

        public override bool Equals(object obj) => 
            ((obj != null) ? (!ReferenceEquals(this, obj) ? (!(obj.GetType() != base.GetType()) ? this.Equals((MyWorkshopItem) obj) : false) : true) : false);

        protected bool Equals(MyWorkshopItem other) => 
            (this.Id == other.Id);

        ~MyWorkshopItemPublisher()
        {
            this.Dispose();
        }

        public override int GetHashCode() => 
            this.Id.GetHashCode();

        protected virtual void OnItemPublished(MyGameServiceCallResult result, ulong publishedId)
        {
            PublishItemResult itemPublished = this.ItemPublished;
            if (itemPublished != null)
            {
                itemPublished(result, publishedId);
            }
        }

        public virtual void Publish()
        {
        }

        public override string ToString()
        {
            string title = this.Title;
            return $"[{this.Id}] {(title ?? "N/A")}";
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Thumbnail { get; set; }

        public string Folder { get; set; }

        public ulong Id { get; set; }

        public MyModMetadata Metadata { get; set; }

        public MyPublishedFileVisibility Visibility { get; set; }

        private class MyNameComparer : IComparer<MyWorkshopItem>
        {
            public int Compare(MyWorkshopItem x, MyWorkshopItem y)
            {
                if ((x == null) && (y == null))
                {
                    return 0;
                }
                if ((x != null) && (y == null))
                {
                    return 1;
                }
                if ((x != null) || (y == null))
                {
                    return string.CompareOrdinal(x.Title, y.Title);
                }
                return -1;
            }
        }

        public delegate void PublishItemResult(MyGameServiceCallResult result, ulong publishedId);
    }
}

