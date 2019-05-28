namespace Sandbox.Game.Screens
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public class MyFilterBool : IMyFilterOption
    {
        public MyFilterBool(bool? value = new bool?())
        {
            this.Value = value;
        }

        public void Configure(string value)
        {
            if (value == "0")
            {
                this.Value = false;
            }
            else if (value == "1")
            {
                this.Value = true;
            }
            else
            {
                if (value != "2")
                {
                    throw new InvalidBranchException();
                }
                this.Value = null;
            }
        }

        public bool IsMatch(object value)
        {
            if (this.Value == null)
            {
                return true;
            }
            bool? nullable = this.Value;
            bool? nullable2 = (bool?) value;
            return ((nullable.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((nullable != null) == (nullable2 != null)));
        }

        public bool? Value { get; set; }

        public CheckStateEnum CheckValue
        {
            get
            {
                bool? nullable = this.Value;
                if (nullable == null)
                {
                    return CheckStateEnum.Indeterminate;
                }
                bool valueOrDefault = nullable.GetValueOrDefault();
                if (!valueOrDefault)
                {
                    return CheckStateEnum.Unchecked;
                }
                if (!valueOrDefault)
                {
                    throw new InvalidBranchException();
                }
                return CheckStateEnum.Checked;
            }
            set
            {
                switch (value)
                {
                    case CheckStateEnum.Checked:
                        this.Value = true;
                        return;

                    case CheckStateEnum.Unchecked:
                        this.Value = false;
                        return;

                    case CheckStateEnum.Indeterminate:
                        this.Value = null;
                        return;
                }
                throw new ArgumentOutOfRangeException("value", value, null);
            }
        }

        public string SerializedValue
        {
            get
            {
                bool? nullable = this.Value;
                if (nullable == null)
                {
                    return "2";
                }
                bool valueOrDefault = nullable.GetValueOrDefault();
                if (!valueOrDefault)
                {
                    return "0";
                }
                if (!valueOrDefault)
                {
                    throw new InvalidBranchException();
                }
                return "1";
            }
        }
    }
}

