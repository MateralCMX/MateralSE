namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Collections;

    public class MyWorkshopItem : IDisposable
    {
        protected List<string> m_tags = new List<string>();
        protected List<ulong> m_dependencies = new List<ulong>();
        protected List<uint> m_DLCs = new List<uint>();
        [CompilerGenerated]
        private DownloadItemResult ItemDownloaded;
        public static IComparer<MyWorkshopItem> NameComparer = new MyWorkshopItemComparer();

        public event DownloadItemResult ItemDownloaded
        {
            [CompilerGenerated] add
            {
                DownloadItemResult itemDownloaded = this.ItemDownloaded;
                while (true)
                {
                    DownloadItemResult a = itemDownloaded;
                    DownloadItemResult result3 = (DownloadItemResult) Delegate.Combine(a, value);
                    itemDownloaded = Interlocked.CompareExchange<DownloadItemResult>(ref this.ItemDownloaded, result3, a);
                    if (ReferenceEquals(itemDownloaded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                DownloadItemResult itemDownloaded = this.ItemDownloaded;
                while (true)
                {
                    DownloadItemResult source = itemDownloaded;
                    DownloadItemResult result3 = (DownloadItemResult) Delegate.Remove(source, value);
                    itemDownloaded = Interlocked.CompareExchange<DownloadItemResult>(ref this.ItemDownloaded, result3, source);
                    if (ReferenceEquals(itemDownloaded, source))
                    {
                        return;
                    }
                }
            }
        }

        public virtual void Dispose()
        {
            if (this.m_tags != null)
            {
                this.m_tags.Clear();
            }
            if (this.m_dependencies != null)
            {
                this.m_dependencies.Clear();
            }
            this.Metadata = null;
        }

        public virtual void Download()
        {
        }

        public override bool Equals(object obj) => 
            ((obj != null) ? (!ReferenceEquals(this, obj) ? (!(obj.GetType() != base.GetType()) ? this.Equals((MyWorkshopItem) obj) : false) : true) : false);

        protected bool Equals(MyWorkshopItem other) => 
            (this.Id == other.Id);

        ~MyWorkshopItem()
        {
            this.Dispose();
        }

        public override int GetHashCode() => 
            this.Id.GetHashCode();

        public virtual MyWorkshopItemPublisher GetPublisher() => 
            null;

        public virtual bool IsUpToDate() => 
            (this.State.HasFlag(MyWorkshopItemState.Installed) && !this.State.HasFlag(MyWorkshopItemState.NeedsUpdate));

        protected virtual void OnItemDownloaded(MyGameServiceCallResult result, ulong publishedId)
        {
            DownloadItemResult itemDownloaded = this.ItemDownloaded;
            if (itemDownloaded != null)
            {
                itemDownloaded(result, publishedId);
            }
        }

        public virtual void Subscribe()
        {
        }

        public override string ToString()
        {
            string title = this.Title;
            return $"[{this.Id}] {(title ?? "N/A")}";
        }

        public virtual void UpdateState()
        {
        }

        public string Title { get; protected set; }

        public string Description { get; protected set; }

        public string Thumbnail { get; protected set; }

        public string Folder { get; protected set; }

        public MyWorkshopItemType ItemType { get; protected set; }

        public ulong Id { get; set; }

        public ulong OwnerId { get; protected set; }

        public DateTime TimeUpdated { get; protected set; }

        public DateTime LocalTimeUpdated { get; protected set; }

        public DateTime TimeCreated { get; protected set; }

        public virtual ulong BytesDownloaded { get; protected set; }

        public virtual ulong BytesTotal { get; protected set; }

        public virtual float DownloadProgress { get; protected set; }

        public ulong Size { get; protected set; }

        public MyModMetadata Metadata { get; protected set; }

        public MyPublishedFileVisibility Visibility { get; protected set; }

        public MyWorkshopItemState State { get; protected set; }

        public MyModCompatibility Compatibility { get; set; }

        public ListReader<string> Tags =>
            this.m_tags;

        public ListReader<ulong> Dependencies =>
            this.m_dependencies;

        public ListReader<uint> DLCs =>
            this.m_DLCs;

        public float Score { get; protected set; }

        public delegate void DownloadItemResult(MyGameServiceCallResult result, ulong publishedId);

        private class MyWorkshopItemComparer : IComparer<MyWorkshopItem>
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
    }
}

