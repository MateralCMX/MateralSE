namespace Sandbox.Game.Gui
{
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Utils;

    public class MyHudShipInfo
    {
        private static StringBuilder m_formattingCache = new StringBuilder();
        private MyMultipleEnabledEnum m_reflectorLights;
        private int m_mass;
        public bool SpeedInKmH;
        private float m_speed;
        private float m_powerUsage;
        private float m_reactors;
        private int m_landingGearsInProximity;
        private int m_landingGearsLocked;
        private int m_landingGearsTotal;
        private int m_thrustCount;
        private int m_gyroCount;
        private int m_numberOfBatteries;
        private float m_fuelRemainingTime;
        private MyResourceStateEnum m_resourceState;
        private bool m_dampenersEnabled;
        private bool m_needsRefresh = true;
        private MyHudNameValueData m_data = new MyHudNameValueData(typeof(LineEnum).GetEnumValues().Length, "Blue", "White", 0.025f, false);

        public MyHudShipInfo()
        {
            this.Reload();
        }

        public void Hide()
        {
            this.Visible = false;
        }

        private void Refresh()
        {
            string str;
            this.m_needsRefresh = false;
            MyHudNameValueData data = this.Data;
            data[0].Name.Clear().AppendStringBuilder((this.ReflectorLights == MyMultipleEnabledEnum.AllDisabled) ? MyTexts.Get(MySpaceTexts.HudInfoReflectorsOff) : ((this.ReflectorLights == MyMultipleEnabledEnum.NoObjects) ? MyTexts.Get(MySpaceTexts.HudInfoNoReflectors) : MyTexts.Get(MySpaceTexts.HudInfoReflectorsOn)));
            if (this.Mass == 0)
            {
                data[1].Value.Clear().Append("-").Append(" kg");
            }
            else
            {
                data[1].Value.Clear().AppendInt32(this.Mass).Append(" kg");
            }
            if (this.SpeedInKmH)
            {
                data[2].Value.Clear().AppendDecimal(((float) (this.Speed * 3.6f)), 1).Append(" km/h");
            }
            else
            {
                data[2].Value.Clear().AppendDecimal(this.Speed, 1).Append(" m/s");
            }
            Sandbox.Game.Gui.MyHudNameValueData.Data data2 = data[10];
            if (this.ResourceState != MyResourceStateEnum.NoPower)
            {
                data2.Visible = false;
            }
            else
            {
                data2.Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNoPower));
                data2.Visible = true;
            }
            Sandbox.Game.Gui.MyHudNameValueData.Data data3 = data[3];
            if ((this.ResourceState == MyResourceStateEnum.OverloadBlackout) || (this.ResourceState == MyResourceStateEnum.OverloadAdaptible))
            {
                data3.ValueFont = str = "Red";
                data3.NameFont = str;
            }
            else
            {
                data3.ValueFont = (string) (str = null);
                data3.NameFont = str;
            }
            data3.Value.Clear();
            if (this.ResourceState == MyResourceStateEnum.OverloadBlackout)
            {
                data3.Value.AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoPowerOverload));
            }
            else
            {
                data3.Value.AppendDecimal(((float) (this.PowerUsage * 100f)), 2).Append(" %");
            }
            StringBuilder output = data[4].Value;
            output.Clear();
            MyValueFormatter.AppendWorkInBestUnit(this.Reactors, output);
            Sandbox.Game.Gui.MyHudNameValueData.Data data4 = data[8];
            data4.Value.Clear();
            if (this.ResourceState == MyResourceStateEnum.NoPower)
            {
                data4.Visible = false;
            }
            else
            {
                MyValueFormatter.AppendTimeInBestUnit(this.FuelRemainingTime * 3600f, data4.Value);
                data4.Visible = true;
            }
            data[9].Value.Clear().AppendInt32(this.NumberOfBatteries);
            Sandbox.Game.Gui.MyHudNameValueData.Data data5 = data[7];
            data5.Value.Clear().AppendInt32(this.GyroCount);
            if (this.GyroCount == 0)
            {
                data5.ValueFont = str = "Red";
                data5.NameFont = str;
            }
            else
            {
                data5.ValueFont = (string) (str = null);
                data5.NameFont = str;
            }
            Sandbox.Game.Gui.MyHudNameValueData.Data data6 = data[5];
            data6.Value.Clear().AppendInt32(this.ThrustCount);
            if (this.ThrustCount == 0)
            {
                data6.ValueFont = str = "Red";
                data6.NameFont = str;
            }
            else
            {
                data6.ValueFont = (string) (str = null);
                data6.NameFont = str;
            }
            data[6].Value.Clear().AppendStringBuilder(MyTexts.Get(this.DampenersEnabled ? MySpaceTexts.HudInfoOn : MySpaceTexts.HudInfoOff));
            Sandbox.Game.Gui.MyHudNameValueData.Data data7 = data[11];
            Sandbox.Game.Gui.MyHudNameValueData.Data data8 = data[12];
            if (this.LandingGearsLocked > 0)
            {
                data[12].Name.Clear().Append("  ").AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameLocked));
                data7.Value.Clear().Append(this.LandingGearsTotal);
                data8.Value.Clear().AppendInt32(this.LandingGearsLocked);
            }
            else
            {
                data[12].Name.Clear().Append("  ").AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameInProximity));
                data7.Value.Clear().Append(this.LandingGearsTotal);
                data8.Value.Clear().AppendInt32(this.LandingGearsInProximity);
            }
        }

        public void Reload()
        {
            MyHudNameValueData data = this.Data;
            data[1].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameMass));
            data[2].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameSpeed));
            data[3].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNamePowerUsage));
            data[4].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameReactors));
            data[8].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameFuelTime));
            data[9].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameNumberOfBatteries));
            data[7].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameGyroscopes));
            data[5].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameThrusts));
            data[6].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameDampeners));
            data[11].Name.Clear().AppendStringBuilder(MyTexts.Get(MySpaceTexts.HudInfoNameLandingGear));
            this.m_needsRefresh = true;
        }

        public void Show(Action<MyHudShipInfo> propertiesInit)
        {
            this.Visible = true;
            if (propertiesInit != null)
            {
                propertiesInit(this);
            }
        }

        public MyMultipleEnabledEnum ReflectorLights
        {
            get => 
                this.m_reflectorLights;
            set
            {
                if (this.m_reflectorLights != value)
                {
                    this.m_reflectorLights = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int Mass
        {
            get => 
                this.m_mass;
            set
            {
                if (this.m_mass != value)
                {
                    this.m_mass = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public float Speed
        {
            get => 
                this.m_speed;
            set
            {
                if (this.m_speed != value)
                {
                    this.m_speed = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public float PowerUsage
        {
            get => 
                this.m_powerUsage;
            set
            {
                if (this.m_powerUsage != value)
                {
                    this.m_powerUsage = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public float Reactors
        {
            get => 
                this.m_reactors;
            set
            {
                if (this.m_reactors != value)
                {
                    this.m_reactors = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int LandingGearsInProximity
        {
            get => 
                this.m_landingGearsInProximity;
            set
            {
                if (this.m_landingGearsInProximity != value)
                {
                    this.m_landingGearsInProximity = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int LandingGearsLocked
        {
            get => 
                this.m_landingGearsLocked;
            set
            {
                if (this.m_landingGearsLocked != value)
                {
                    this.m_landingGearsLocked = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int LandingGearsTotal
        {
            get => 
                this.m_landingGearsTotal;
            set
            {
                if (this.m_landingGearsTotal != value)
                {
                    this.m_landingGearsTotal = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int ThrustCount
        {
            get => 
                this.m_thrustCount;
            set
            {
                if (this.m_thrustCount != value)
                {
                    this.m_thrustCount = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int GyroCount
        {
            get => 
                this.m_gyroCount;
            set
            {
                if (this.m_gyroCount != value)
                {
                    this.m_gyroCount = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public int NumberOfBatteries
        {
            get => 
                this.m_numberOfBatteries;
            set
            {
                if (this.m_numberOfBatteries != value)
                {
                    this.m_numberOfBatteries = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public float FuelRemainingTime
        {
            get => 
                this.m_fuelRemainingTime;
            set
            {
                if (this.m_fuelRemainingTime != value)
                {
                    this.m_fuelRemainingTime = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public MyResourceStateEnum ResourceState
        {
            get => 
                this.m_resourceState;
            set
            {
                if (this.m_resourceState != value)
                {
                    this.m_resourceState = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public bool DampenersEnabled
        {
            get => 
                this.m_dampenersEnabled;
            set
            {
                if (this.m_dampenersEnabled != value)
                {
                    this.m_dampenersEnabled = value;
                    this.m_needsRefresh = true;
                }
            }
        }

        public MyHudNameValueData Data
        {
            get
            {
                if (this.m_needsRefresh)
                {
                    this.Refresh();
                }
                return this.m_data;
            }
        }

        public bool Visible { get; private set; }

        private enum LineEnum
        {
            ReflectorLights,
            Mass,
            Speed,
            PowerUsage,
            ReactorsMaxOutput,
            ThrustCount,
            DampenersState,
            GyroCount,
            FuelTime,
            NumberOfBatteries,
            PowerState,
            LandingGearState,
            LandingGearStateSecondLine
        }
    }
}

