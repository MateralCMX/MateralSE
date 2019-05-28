namespace Sandbox
{
    using Sandbox.Engine.Utils;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using VRage;
    using VRageRender;

    public class MyErrorReporter
    {
        public static string SUPPORT_EMAIL = "support@keenswh.com";
        public static string MESSAGE_BOX_CAPTION = "{LOCG:Error_Message_Caption}";
        public static string APP_ALREADY_RUNNING = "{LOCG:Error_AlreadyRunning}";
        public static string APP_ERROR_CAPTION = "{LOCG:Error_Error_Caption}";
        public static string APP_LOG_REPORT_FAILED = "{LOCG:Error_Failed}";
        public static string APP_LOG_REPORT_THANK_YOU = "{LOCG:Error_ThankYou}";
        public static string APP_ERROR_MESSAGE = "{LOCG:Error_Error_Message}";
        public static string APP_ERROR_MESSAGE_DX11_NOT_AVAILABLE = "{LOCG:Error_DX11}";
        public static string APP_ERROR_MESSAGE_LOW_GPU = "{LOCG:Error_GPU_Low}";
        public static string APP_ERROR_MESSAGE_NOT_DX11_GPU = "{LOCG:Error_GPU_NotDX11}";
        public static string APP_ERROR_MESSAGE_DRIVER_NOT_INSTALLED = "{LOCG:Error_GPU_Drivers}";
        public static string APP_WARNING_MESSAGE_OLD_DRIVER = "{LOCG:Error_GPU_OldDriver}";
        public static string APP_WARNING_MESSAGE_UNSUPPORTED_GPU = "{LOCG:Error_GPU_Unsupported}";
        public static string APP_ERROR_OUT_OF_MEMORY = "{LOCG:Error_OutOfMemmory}";
        public static string APP_ERROR_OUT_OF_VIDEO_MEMORY = "{LOCG:Error_GPU_OutOfMemory}";

        private static bool AllowSendDialog(string gameName, string logfile, string errorMessage)
        {
            string text = string.Format(errorMessage, gameName, logfile);
            return (MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal | Sandbox.MessageBoxOptions.YesNo) == MessageBoxResult.Yes);
        }

        private static int CountCharOccurrences(string str, char chr)
        {
            int num = 0;
            string str2 = str;
            for (int i = 0; i < str2.Length; i++)
            {
                if (str2[i] == chr)
                {
                    num++;
                }
            }
            return num;
        }

        private static bool DisplayCommonError(string logContent)
        {
            foreach (ErrorInfo info in MyErrorTexts.Infos)
            {
                if (logContent.Contains(info.Match))
                {
                    MessageBox(info.Caption, info.Message);
                    return true;
                }
            }
            return false;
        }

        private static bool LoadAndDisplayCommonError(string logName)
        {
            try
            {
                if ((logName != null) && System.IO.File.Exists(logName))
                {
                    using (FileStream stream = System.IO.File.Open(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string text1 = reader.ReadToEnd();
                            return DisplayCommonError(text1 ?? string.Empty);
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void MessageBox(string caption, string text)
        {
            MyMessageBox.Show(IntPtr.Zero, text, caption, Sandbox.MessageBoxOptions.OkOnly | Sandbox.MessageBoxOptions.SetForeground);
        }

        public static void Report(string logName, string gameName, string id, string errorMessage)
        {
            if (!LoadAndDisplayCommonError(logName) && (AllowSendDialog(gameName, logName, errorMessage) && (logName != null)))
            {
                ReportInternal(logName, gameName, id, null);
            }
        }

        public static void ReportAppAlreadyRunning(string gameName)
        {
            System.Windows.Forms.MessageBox.Show(string.Format(MyTexts.SubstituteTexts(APP_ALREADY_RUNNING, null).ToString().Replace(@"\n", "\r\n"), gameName), string.Format(MyTexts.SubstituteTexts(MESSAGE_BOX_CAPTION, null).ToString().Replace(@"\n", "\r\n"), gameName));
        }

        public static void ReportGeneral(string logName, string gameName, string id)
        {
            if (!LoadAndDisplayCommonError(logName))
            {
                MyMessageBoxCrashForm form = new MyMessageBoxCrashForm(gameName, logName);
                if ((form.ShowDialog() == DialogResult.Yes) && (logName != null))
                {
                    int num = string.IsNullOrWhiteSpace(form.Message) ? 0 : (CountCharOccurrences(form.Message, '\n') + 1);
                    string additionalInfo = $"Email: {form.Email}
Feedback({num}): {form.Message.Replace(@"\n", "\r\n")}
";
                    ReportInternal(logName, gameName, id, additionalInfo);
                }
            }
        }

        public static void ReportGpuUnderMinimumCrash(string gameName, string logfile, string minimumRequirementsPage)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_ERROR_MESSAGE_LOW_GPU, null).ToString().Replace(@"\n", "\r\n"), logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        private static void ReportInternal(string logName, string gameName, string id, string additionalInfo = null)
        {
            string str;
            HttpStatusCode code;
            SendReport(logName, id, out str, out code, additionalInfo);
            if ((str == string.Empty) || (code == HttpStatusCode.OK))
            {
                MessageBox(gameName, MyTexts.SubstituteTexts(APP_LOG_REPORT_THANK_YOU, null).ToString().Replace(@"\n", "\r\n"));
            }
            else
            {
                MessageBox(string.Format(MyTexts.SubstituteTexts(APP_ERROR_CAPTION, null).ToString().Replace(@"\n", "\r\n"), gameName), string.Format(MyTexts.SubstituteTexts(APP_LOG_REPORT_FAILED, null).ToString().Replace(@"\n", "\r\n"), gameName, logName, MyTexts.SubstituteTexts(SUPPORT_EMAIL, null).ToString()));
            }
        }

        public static void ReportNotCompatibleGPU(string gameName, string logfile, string minimumRequirementsPage)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_WARNING_MESSAGE_UNSUPPORTED_GPU, null).ToString().Replace(@"\n", "\r\n"), logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        public static void ReportNotDX11GPUCrash(string gameName, string logfile, string minimumRequirementsPage)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_ERROR_MESSAGE_NOT_DX11_GPU, null).ToString().Replace(@"\n", "\r\n"), logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        public static void ReportNotInteractive(string logName, string id)
        {
            if (logName != null)
            {
                string str;
                HttpStatusCode code;
                SendReport(logName, id, out str, out code, null);
            }
        }

        public static MessageBoxResult ReportOldDrivers(string gameName, string cardName, string driverUpdateLink)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_WARNING_MESSAGE_OLD_DRIVER, null).ToString().Replace(@"\n", "\r\n"), gameName, cardName, driverUpdateLink);
            return MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.AbortRetryIgnore | Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.OkCancel | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        public static void ReportOutOfMemory(string gameName, string logfile, string minimumRequirementsPage)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_ERROR_OUT_OF_MEMORY, null).ToString().Replace(@"\n", "\r\n"), logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        public static void ReportOutOfVideoMemory(string gameName, string logfile, string minimumRequirementsPage)
        {
            string text = string.Format(MyTexts.SubstituteTexts(APP_ERROR_OUT_OF_VIDEO_MEMORY, null).ToString().Replace(@"\n", "\r\n"), logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        public static void ReportRendererCrash(string logfile, string gameName, string minimumRequirementsPage, MyRenderExceptionEnum type)
        {
            string format = (type == MyRenderExceptionEnum.DriverNotInstalled) ? MyTexts.SubstituteTexts(APP_ERROR_MESSAGE_DRIVER_NOT_INSTALLED, null).ToString().Replace(@"\n", "\r\n") : ((type != MyRenderExceptionEnum.GpuNotSupported) ? MyTexts.SubstituteTexts(APP_ERROR_MESSAGE_LOW_GPU, null).ToString().Replace(@"\n", "\r\n") : MyTexts.SubstituteTexts(APP_ERROR_MESSAGE_LOW_GPU, null).ToString().Replace(@"\n", "\r\n"));
            string text = string.Format(format, logfile, gameName, minimumRequirementsPage);
            MyMessageBox.Show(IntPtr.Zero, text, gameName, Sandbox.MessageBoxOptions.IconExclamation | Sandbox.MessageBoxOptions.SetForeground | Sandbox.MessageBoxOptions.SystemModal);
        }

        private static void SendReport(string logName, string id, out string log, out HttpStatusCode code, string additionalInfo = null)
        {
            log = null;
            code = HttpStatusCode.MethodNotAllowed;
            byte[] buffer = Array.Empty<byte>();
            if (additionalInfo == null)
            {
                additionalInfo = string.Empty;
            }
            try
            {
                string[] strArray;
                int num;
                HttpWebRequest request;
                if ((logName != null) && System.IO.File.Exists(logName))
                {
                    using (FileStream stream = System.IO.File.Open(logName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            log = additionalInfo + reader.ReadToEnd();
                        }
                    }
                }
                goto TR_003F;
            TR_0005:
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    code = response.StatusCode;
                }
                return;
            TR_0011:
                if (string.IsNullOrEmpty(log))
                {
                    return;
                }
                else
                {
                    request = (HttpWebRequest) WebRequest.Create("http://www.minerwars.com/SubmitLog.aspx?id=" + id);
                    request.Method = "POST";
                    request.ContentType = "application/octet-stream";
                    using (Stream stream6 = request.GetRequestStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream6))
                        {
                            writer.Write(log);
                            writer.Write(additionalInfo);
                            if (MyFakes.ENABLE_MINIDUMP_SENDING)
                            {
                                writer.Write(buffer.Length);
                                writer.Write(buffer);
                            }
                        }
                    }
                }
                goto TR_0005;
            TR_002E:
                if (MyFakes.ENABLE_MINIDUMP_SENDING)
                {
                    try
                    {
                        bool flag = false;
                        using (MemoryStream stream3 = new MemoryStream())
                        {
                            using (ZipArchive archive = new ZipArchive(stream3, ZipArchiveMode.Create, true))
                            {
                                strArray = Directory.GetFiles(Path.GetDirectoryName(logName), "Minidump*.dmp", SearchOption.TopDirectoryOnly);
                                num = 0;
                                goto TR_0027;
                            TR_0018:
                                num++;
                            TR_0027:
                                while (true)
                                {
                                    if (num >= strArray.Length)
                                    {
                                        break;
                                    }
                                    string path = strArray[num];
                                    if (((path != null) && System.IO.File.Exists(path)) && ((System.IO.File.GetCreationTime(path) - DateTime.Now).Minutes < 5))
                                    {
                                        flag = true;
                                        using (Stream stream4 = archive.CreateEntry(Path.GetFileName(path)).Open())
                                        {
                                            using (FileStream stream5 = System.IO.File.Open(path, FileMode.Open))
                                            {
                                                stream5.CopyTo(stream4);
                                            }
                                        }
                                    }
                                    goto TR_0018;
                                }
                            }
                            if (flag)
                            {
                                buffer = stream3.ToArray();
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                goto TR_0011;
            TR_003F:
                try
                {
                    strArray = Directory.GetFiles(Path.GetDirectoryName(logName), "VRageRender*.log", SearchOption.TopDirectoryOnly);
                    num = 0;
                    goto TR_003D;
                TR_002F:
                    num++;
                TR_003D:
                    while (true)
                    {
                        if (num < strArray.Length)
                        {
                            string path = strArray[num];
                            if ((path != null) && System.IO.File.Exists(path))
                            {
                                using (FileStream stream2 = System.IO.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    using (StreamReader reader2 = new StreamReader(stream2))
                                    {
                                        string text1 = reader2.ReadToEnd();
                                        string text2 = additionalInfo;
                                        additionalInfo = text2 + (text1 ?? string.Empty);
                                    }
                                }
                            }
                        }
                        else
                        {
                            goto TR_002E;
                        }
                        break;
                    }
                    goto TR_002F;
                }
                catch
                {
                }
                goto TR_002E;
            }
            catch
            {
            }
        }
    }
}

