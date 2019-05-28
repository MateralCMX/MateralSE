namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyGps : IMyGps
    {
        internal static readonly int DROP_NONFINAL_AFTER_SEC = 180;
        private Vector3D m_coords;
        private IMyEntity m_entity;
        private bool m_unupdatedEntityLocation;
        private long m_entityId;
        private string m_displayName;

        public MyGps()
        {
            this.m_displayName = string.Empty;
            this.GPSColor = new Color(0x75, 0xc9, 0xf1);
            this.SetDiscardAt();
        }

        public MyGps(MyObjectBuilder_Gps.Entry builder)
        {
            this.m_displayName = string.Empty;
            this.Name = builder.name;
            this.DisplayName = builder.DisplayName;
            this.Description = builder.description;
            this.Coords = builder.coords;
            this.ShowOnHud = builder.showOnHud;
            this.AlwaysVisible = builder.alwaysVisible;
            this.IsObjective = builder.isObjective;
            if ((builder.color == Color.Transparent) || (builder.color == Color.Black))
            {
                this.GPSColor = new Color(0x75, 0xc9, 0xf1);
            }
            else
            {
                this.GPSColor = builder.color;
            }
            if (!builder.isFinal)
            {
                this.SetDiscardAt();
            }
            this.SetEntityId(builder.entityId);
            this.UpdateHash();
        }

        public void Close()
        {
            if (this.m_entity != null)
            {
                this.m_entity.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
                this.m_entity.OnClose -= new Action<IMyEntity>(this.m_entity_OnClose);
            }
        }

        public override int GetHashCode() => 
            this.Hash;

        private void m_entity_OnClose(IMyEntity obj)
        {
            if (this.m_entity != null)
            {
                this.m_entity.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
                this.m_entity.OnClose -= new Action<IMyEntity>(this.m_entity_OnClose);
                this.m_entity = null;
            }
        }

        private void PositionComp_OnPositionChanged(MyPositionComponentBase obj)
        {
            if (this.m_entity != null)
            {
                this.Coords = this.m_entity.PositionComp.GetPosition();
            }
        }

        public void SetDiscardAt()
        {
            this.DiscardAt = new TimeSpan?(TimeSpan.FromSeconds(MySession.Static.ElapsedPlayTime.TotalSeconds + DROP_NONFINAL_AFTER_SEC));
        }

        public void SetEntity(IMyEntity entity)
        {
            if (entity != null)
            {
                this.m_entity = entity;
                this.m_entityId = entity.EntityId;
                this.m_entity.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.PositionComp_OnPositionChanged);
                this.m_entity.NeedsWorldMatrix = true;
                this.m_entity.OnClose += new Action<IMyEntity>(this.m_entity_OnClose);
                this.Coords = this.m_entity.PositionComp.GetPosition();
            }
        }

        public void SetEntityId(long entityId)
        {
            if (entityId != 0)
            {
                this.m_entityId = entityId;
            }
        }

        public void ToClipboard()
        {
            MyClipboardHelper.SetClipboard(this.ToString());
        }

        public override string ToString()
        {
            StringBuilder builder1 = new StringBuilder("GPS:", 0x100);
            builder1.Append(this.Name);
            builder1.Append(":");
            builder1.Append(this.Coords.X.ToString(CultureInfo.InvariantCulture));
            builder1.Append(":");
            builder1.Append(this.Coords.Y.ToString(CultureInfo.InvariantCulture));
            builder1.Append(":");
            builder1.Append(this.Coords.Z.ToString(CultureInfo.InvariantCulture));
            builder1.Append(":");
            return builder1.ToString();
        }

        public void UpdateHash()
        {
            int hash = MyUtils.GetHash(this.Name, -2128831035);
            if (this.m_entityId != 0)
            {
                hash *= this.m_entityId.GetHashCode();
            }
            else
            {
                hash = MyUtils.GetHash(this.Coords.X, hash);
                hash = MyUtils.GetHash(this.Coords.Y, hash);
                hash = MyUtils.GetHash(this.Coords.Z, hash);
            }
            this.Hash = hash;
        }

        public string Name { get; set; }

        public bool IsObjective { get; set; }

        public string DisplayName
        {
            get => 
                (!this.m_unupdatedEntityLocation ? this.m_displayName : (this.m_displayName + " (last known location)"));
            set => 
                (this.m_displayName = value);
        }

        public string Description { get; set; }

        public Vector3D Coords
        {
            get
            {
                if (this.CoordsFunc != null)
                {
                    return this.CoordsFunc();
                }
                if ((this.m_entityId != 0) && (this.m_entity == null))
                {
                    IMyEntity entityById = MyEntities.GetEntityById(this.m_entityId, false);
                    if (entityById == null)
                    {
                        this.m_unupdatedEntityLocation = true;
                    }
                    else
                    {
                        this.m_unupdatedEntityLocation = false;
                        this.SetEntity(entityById);
                    }
                }
                return this.m_coords;
            }
            set => 
                (this.m_coords = value);
        }

        public Color GPSColor { get; set; }

        public bool ShowOnHud { get; set; }

        public bool AlwaysVisible { get; set; }

        public TimeSpan? DiscardAt { get; set; }

        public bool IsLocal { get; set; }

        public Func<Vector3D> CoordsFunc { get; set; }

        public long EntityId =>
            this.m_entityId;

        public bool IsContainerGPS { get; set; }

        public string ContainerRemainingTime { get; set; }

        public int Hash { get; private set; }

        string IMyGps.Name
        {
            get => 
                this.Name;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Value must not be null!");
                }
                this.Name = value;
            }
        }

        string IMyGps.Description
        {
            get => 
                this.Description;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Value must not be null!");
                }
                this.Description = value;
            }
        }

        Vector3D IMyGps.Coords
        {
            get => 
                this.Coords;
            set => 
                (this.Coords = value);
        }

        bool IMyGps.ShowOnHud
        {
            get => 
                this.ShowOnHud;
            set => 
                (this.ShowOnHud = value);
        }

        TimeSpan? IMyGps.DiscardAt
        {
            get => 
                this.DiscardAt;
            set => 
                (this.DiscardAt = value);
        }
    }
}

