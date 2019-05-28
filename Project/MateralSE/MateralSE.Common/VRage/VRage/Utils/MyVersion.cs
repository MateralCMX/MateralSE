namespace VRage.Utils
{
    using System;
    using System.Text;

    public class MyVersion
    {
        public readonly int Version;
        public readonly StringBuilder FormattedText;
        public readonly StringBuilder FormattedTextFriendly;
        private readonly System.Version _version;

        public MyVersion(int version)
        {
            this.Version = version;
            this.FormattedText = new StringBuilder(MyBuildNumbers.ConvertBuildNumberFromIntToString(version));
            string str = MyBuildNumbers.ConvertBuildNumberFromIntToStringFriendly(version, ".");
            this.FormattedTextFriendly = new StringBuilder(str);
            this._version = new System.Version(str);
        }

        public static implicit operator MyVersion(int version) => 
            new MyVersion(version);

        public static implicit operator int(MyVersion version) => 
            version.Version;

        public static implicit operator System.Version(MyVersion version) => 
            version._version;

        public override string ToString() => 
            this.Version.ToString();
    }
}

