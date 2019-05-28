namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;

    public static class MyDefinitionErrors
    {
        private static readonly object m_lockObject = new object();
        private static readonly List<Error> m_errors = new List<Error>();
        private static readonly ErrorComparer m_comparer = new ErrorComparer();

        public static void Add(MyModContext context, string message, TErrorSeverity severity, bool writeToLog = true)
        {
            Error error1 = new Error();
            error1.ModName = context.ModName;
            error1.ErrorFile = context.CurrentFile;
            error1.Message = message;
            error1.Severity = severity;
            Error item = error1;
            object lockObject = m_lockObject;
            lock (lockObject)
            {
                m_errors.Add(item);
            }
            string modName = context.ModName;
            if (writeToLog)
            {
                WriteError(item);
            }
            if (severity == TErrorSeverity.Critical)
            {
                ShouldShowModErrors = true;
            }
        }

        public static void Clear()
        {
            object lockObject = m_lockObject;
            lock (lockObject)
            {
                m_errors.Clear();
            }
        }

        public static ListReader<Error> GetErrors()
        {
            object lockObject = m_lockObject;
            lock (lockObject)
            {
                m_errors.Sort(m_comparer);
                return new ListReader<Error>(m_errors);
            }
        }

        public static void WriteError(Error e)
        {
            MyLog.Default.WriteLine($"{e.ErrorSeverity}: {e.ModName ?? string.Empty}");
            MyLog.Default.WriteLine("  in file: " + e.ErrorFile);
            MyLog.Default.WriteLine("  " + e.Message);
        }

        public static bool ShouldShowModErrors
        {
            [CompilerGenerated]
            get => 
                <ShouldShowModErrors>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShouldShowModErrors>k__BackingField = value);
        }

        public class Error
        {
            public string ModName;
            public string ErrorFile;
            public string Message;
            public TErrorSeverity Severity;
            private static Color[] severityColors = new Color[] { Color.Gray, Color.Gray, Color.White, new Color(1f, 0.25f, 0.1f) };
            private static string[] severityName = new string[] { "notice", "warning", "error", "critical error" };
            private static string[] severityNamePlural = new string[] { "notices", "warnings", "errors", "critical errors" };

            public Color GetSeverityColor() => 
                GetSeverityColor(this.Severity);

            public static Color GetSeverityColor(TErrorSeverity severity)
            {
                try
                {
                    return severityColors[(int) severity];
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"Error type does not have color assigned: message: {exception.Message}, stack:{exception.StackTrace}");
                    return Color.White;
                }
            }

            public static string GetSeverityName(TErrorSeverity severity, bool plural)
            {
                try
                {
                    return (!plural ? severityName[(int) severity] : severityNamePlural[(int) severity]);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine($"Error type does not have name assigned: message: {exception.Message}, stack:{exception.StackTrace}");
                    return (plural ? "Errors" : "Error");
                }
            }

            public override string ToString()
            {
                object[] objArray1 = new object[4];
                objArray1[0] = this.ErrorSeverity;
                objArray1[1] = this.ModName ?? string.Empty;
                object[] args = objArray1;
                args[2] = this.ErrorFile;
                args[3] = this.Message;
                return string.Format("{0}: {1}, in file: {2}\n{3}", args);
            }

            public string ErrorId =>
                ((this.ModName == null) ? "definition_" : "mod_");

            public string ErrorSeverity
            {
                get
                {
                    string errorId = this.ErrorId;
                    switch (this.Severity)
                    {
                        case TErrorSeverity.Notice:
                            errorId = errorId + "notice";
                            break;

                        case TErrorSeverity.Warning:
                            errorId = errorId + "warning";
                            break;

                        case TErrorSeverity.Error:
                            errorId = (errorId + "error").ToUpperInvariant();
                            break;

                        case TErrorSeverity.Critical:
                            errorId = (errorId + "critical_error").ToUpperInvariant();
                            break;

                        default:
                            break;
                    }
                    return errorId;
                }
            }
        }

        public class ErrorComparer : IComparer<MyDefinitionErrors.Error>
        {
            public int Compare(MyDefinitionErrors.Error x, MyDefinitionErrors.Error y) => 
                ((int) (y.Severity - x.Severity));
        }
    }
}

