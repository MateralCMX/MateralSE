namespace Sandbox
{
    using Microsoft.Win32;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Xml.Serialization;
    using VRage;
    using VRage.Common.Utils;
    using VRage.Cryptography;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Library;
    using VRage.Library.Exceptions;
    using VRage.Utils;

    public static class MyInitializer
    {
        private static string m_appName;
        private static HashSet<string> m_ignoreList = new HashSet<string>();
        private static object m_exceptionSyncRoot = new object();

        private static string CheckFor45DotVersion(int releaseKey) => 
            ((releaseKey < 0x605fb) ? ((releaseKey < 0x6040e) ? ((releaseKey < 0x6004f) ? ((releaseKey < 0x5cbf5) ? ((releaseKey < 0x5c733) ? ((releaseKey < 0x5c615) ? "No 4.5 or later version detected" : ("4.5 or later (" + releaseKey + ")")) : ("4.5.1 or later (" + releaseKey + ")")) : ("4.5.2 or later (" + releaseKey + ")")) : ("4.6 or later (" + releaseKey + ")")) : ("4.6.1 or later (" + releaseKey + ")")) : ("4.6.2 or later (" + releaseKey + ")"));

        private static void ChecksumFailed(string filename, string hash)
        {
            if (!m_ignoreList.Contains(filename))
            {
                m_ignoreList.Add(filename);
                MySandboxGame.Log.WriteLine($"Error: checksum of file '{filename}' failed {hash}");
            }
        }

        private static void ChecksumNotFound(IFileVerifier verifier, string filename)
        {
            MyChecksumVerifier verifier2 = (MyChecksumVerifier) verifier;
            if ((!m_ignoreList.Contains(filename) && filename.StartsWith(verifier2.BaseChecksumDir, StringComparison.InvariantCultureIgnoreCase)) && filename.Substring(Math.Min(filename.Length, verifier2.BaseChecksumDir.Length + 1)).StartsWith("Data", StringComparison.InvariantCultureIgnoreCase))
            {
                MySandboxGame.Log.WriteLine($"Error: no checksum found for file '{filename}'");
                m_ignoreList.Add(filename);
            }
        }

        private static string GetInfoCPU(out uint frequency)
        {
            frequency = 0;
            string str = "";
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor"))
                {
                    using (new ManagementObjectSearcher("select * from Win32_ComputerSystem"))
                    {
                        using (new ManagementObjectSearcher("select * from Win32_PhysicalMemory"))
                        {
                            foreach (ManagementObject obj2 in searcher.Get())
                            {
                                str = obj2["Name"].ToString();
                                frequency = (uint) obj2["MaxClockSpeed"];
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine("Couldn't get cpu info: " + exception);
            }
            return str;
        }

        public static StringBuilder GetLogName(string appName, bool addDateToLog)
        {
            StringBuilder builder = new StringBuilder(appName);
            if (addDateToLog)
            {
                builder.Append("_");
                builder.Append(new StringBuilder().GetFormatedDateTimeForFilename(DateTime.Now));
            }
            builder.Append(".log");
            return builder;
        }

        private static string GetNETFromRegistry()
        {
            string versionFromRegistry;
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                if ((key == null) || (key.GetValue("Release") == null))
                {
                    versionFromRegistry = GetVersionFromRegistry();
                }
                else
                {
                    versionFromRegistry = CheckFor45DotVersion((int) key.GetValue("Release"));
                }
            }
            return versionFromRegistry;
        }

        private static string GetOsName()
        {
            string str = "";
            try
            {
                str = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>().FirstOrDefault<ManagementObject>().GetPropertyValue("Caption").ToString().Trim();
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine("Couldn't get friendly OS name" + exception);
            }
            object[] objArray1 = new object[] { str, " (", Environment.OSVersion, ")" };
            return string.Concat(objArray1);
        }

        private static string GetVersionFromRegistry()
        {
            string str7;
            using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                string str = "";
                string[] subKeyNames = key.GetSubKeyNames();
                int index = 0;
                while (true)
                {
                    if (index >= subKeyNames.Length)
                    {
                        str7 = str;
                        break;
                    }
                    string name = subKeyNames[index];
                    if (name.StartsWith("v"))
                    {
                        RegistryKey key2 = key.OpenSubKey(name);
                        string str3 = (string) key2.GetValue("Version", "");
                        string str4 = key2.GetValue("SP", "").ToString();
                        string str5 = key2.GetValue("Install", "").ToString();
                        if (str5 == "")
                        {
                            str = str + name + ": " + str3;
                        }
                        else if ((str4 != "") && (str5 == "1"))
                        {
                            string[] textArray1 = new string[] { str, name, ": ", str3, ", SP", str4, "; " };
                            str = string.Concat(textArray1);
                        }
                        if (str3 == "")
                        {
                            string[] strArray2 = key2.GetSubKeyNames();
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= strArray2.Length)
                                {
                                    str = str.Remove(str.Length - 2) + "; ";
                                    break;
                                }
                                string str6 = strArray2[num2];
                                RegistryKey key3 = key2.OpenSubKey(str6);
                                str3 = (string) key3.GetValue("Version", "");
                                if (str3 != "")
                                {
                                    str4 = key3.GetValue("SP", "").ToString();
                                }
                                str5 = key3.GetValue("Install", "").ToString();
                                if (str5 == "")
                                {
                                    string[] textArray2 = new string[] { str, name, ": ", str3, "; " };
                                    str = string.Concat(textArray2);
                                }
                                else if ((str4 != "") && (str5 == "1"))
                                {
                                    string[] textArray3 = new string[] { str, str6, ", ", str3, ", SP", str4, ", " };
                                    str = string.Concat(textArray3);
                                }
                                else if (str5 == "1")
                                {
                                    string[] textArray4 = new string[] { str, str6, ", ", str3, ", " };
                                    str = string.Concat(textArray4);
                                }
                                num2++;
                            }
                        }
                    }
                    index++;
                }
            }
            return str7;
        }

        private static void HandleSpecialExceptions(Exception exception)
        {
            if (exception != null)
            {
                ReflectionTypeLoadException exception2 = exception as ReflectionTypeLoadException;
                if (exception2 != null)
                {
                    foreach (Exception exception4 in exception2.LoaderExceptions)
                    {
                        MySandboxGame.Log.AppendToClosedLog(exception4);
                    }
                }
                OutOfMemoryException e = exception as OutOfMemoryException;
                if (e != null)
                {
                    MySandboxGame.Log.AppendToClosedLog("Handling out of memory exception... " + MySandboxGame.Config);
                    if ((MySandboxGame.Config.LowMemSwitchToLow == MyConfig.LowMemSwitch.ARMED) && !MySandboxGame.Config.IsSetToLowQuality())
                    {
                        MySandboxGame.Log.AppendToClosedLog("Creating switch to low request");
                        MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.TRIGGERED;
                        MySandboxGame.Config.Save();
                        MySandboxGame.Log.AppendToClosedLog("Switch to low request created");
                    }
                    MySandboxGame.Log.AppendToClosedLog(e);
                }
                HandleSpecialExceptions(exception.InnerException);
            }
        }

        public static void InitCheckSum()
        {
            try
            {
                string path = Path.Combine(MyFileSystem.ContentPath, "checksum.xml");
                if (!File.Exists(path))
                {
                    MySandboxGame.Log.WriteLine("Checksum file is missing, game will run as usual but file integrity won't be verified");
                }
                else
                {
                    using (FileStream stream = File.OpenRead(path))
                    {
                        MyChecksums checksums = (MyChecksums) new XmlSerializer(typeof(MyChecksums)).Deserialize(stream);
                        MyChecksumVerifier verifier1 = new MyChecksumVerifier(checksums, MyFileSystem.ContentPath);
                        verifier1.ChecksumFailed += new Action<string, string>(MyInitializer.ChecksumFailed);
                        verifier1.ChecksumNotFound += new Action<IFileVerifier, string>(MyInitializer.ChecksumNotFound);
                        stream.Position = 0L;
                        SHA256 sha1 = MySHA256.Create();
                        sha1.Initialize();
                        byte[] inArray = sha1.ComputeHash(stream);
                        string str2 = "BgIAAACkAABSU0ExAAQAAAEAAQClSibD83Y6Akok8tAtkbMz4IpueWFra0QkkKcodwe2pV/RJAfyq5mLUGsF3JdTdu3VWn93VM+ZpL9CcMKS8HaaHmBZJn7k2yxNvU4SD+8PhiZ87iPqpkN2V+rz9nyPWTHDTgadYMmenKk2r7w4oYOooo5WXdkTVjAD50MroAONuQ==";
                        MySandboxGame.Log.WriteLine("Checksum file hash: " + Convert.ToBase64String(inArray));
                        MySandboxGame.Log.WriteLine($"Checksum public key valid: {checksums.PublicKey == str2}, Key: {checksums.PublicKey}");
                        MyFileSystem.FileVerifier = verifier1;
                    }
                }
            }
            catch
            {
            }
        }

        public static void InitExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyInitializer.UnhandledExceptionHandler);
            Thread.CurrentThread.Name = "Main thread";
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            if (MyFakes.ENABLE_MINIDUMP_SENDING && MyFileSystem.IsInitialized)
            {
                string[] strArray = Directory.GetFiles(MyFileSystem.UserDataPath, "Minidump*.dmp", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < strArray.Length; i++)
                {
                    File.Delete(strArray[i]);
                }
            }
        }

        public static void InvokeAfterRun()
        {
            MySandboxGame.Log.Close();
        }

        public static void InvokeBeforeRun(uint appId, string appName, string userDataPath, bool addDateToLog = false)
        {
            uint num;
            m_appName = appName;
            StringBuilder logName = GetLogName(appName, addDateToLog);
            string fullName = new FileInfo(MyFileSystem.ExePath).Directory.FullName;
            MyFileSystem.Init(Path.Combine(fullName, "Content"), userDataPath, "Mods", null);
            bool flag = SteamHelpers.IsSteamPath(fullName);
            bool flag2 = SteamHelpers.IsAppManifestPresent(fullName, appId);
            Sandbox.Engine.Platform.Game.IsPirated = !flag && !flag2;
            MySandboxGame.Log.Init(logName.ToString(), MyFinalBuildConstants.APP_VERSION_STRING);
            MySandboxGame.Log.WriteLine("Steam build: Always true");
            object[] objArray1 = new object[4];
            objArray1[0] = true;
            objArray1[1] = MyObfuscation.Enabled ? "[O]" : "[NO]";
            object[] local3 = objArray1;
            object[] local4 = objArray1;
            local4[2] = flag ? "[IS]" : "[NIS]";
            object[] local1 = local4;
            object[] args = local4;
            args[3] = flag2 ? "[AMP]" : "[NAMP]";
            MySandboxGame.Log.WriteLineAndConsole(string.Format("Is official: {0} {1}{2}{3}", args));
            MySandboxGame.Log.WriteLineAndConsole("Environment.ProcessorCount: " + MyEnvironment.ProcessorCount);
            MySandboxGame.Log.WriteLineAndConsole("Environment.OSVersion: " + GetOsName());
            MySandboxGame.Log.WriteLineAndConsole("Environment.CommandLine: " + Environment.CommandLine);
            MySandboxGame.Log.WriteLineAndConsole("Environment.Is64BitProcess: " + MyEnvironment.Is64BitProcess.ToString());
            MySandboxGame.Log.WriteLineAndConsole("Environment.Is64BitOperatingSystem: " + Environment.Is64BitOperatingSystem.ToString());
            MySandboxGame.Log.WriteLineAndConsole("Environment.Version: " + GetNETFromRegistry());
            MySandboxGame.Log.WriteLineAndConsole("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            MySandboxGame.Log.WriteLineAndConsole("CPU Info: " + GetInfoCPU(out num));
            MySandboxGame.CPUFrequency = num;
            MySandboxGame.Log.WriteLine("IntPtr.Size: " + IntPtr.Size.ToString());
            MySandboxGame.Log.WriteLine("Default Culture: " + CultureInfo.CurrentCulture.Name);
            MySandboxGame.Log.WriteLine("Default UI Culture: " + CultureInfo.CurrentUICulture.Name);
            MySandboxGame.Log.WriteLine("IsAdmin: " + new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator).ToString());
            MyLog.Default = MySandboxGame.Log;
            InitExceptionHandling();
            MySandboxGame.Config = new MyConfig(appName + ".cfg");
            MySandboxGame.Config.Load();
        }

        private static bool IsModCrash(Exception e) => 
            (e is ModCrashedException);

        private static bool IsOutOfMemory(Exception e)
        {
            if (e == null)
            {
                return false;
            }
            SharpDXException exception = e as SharpDXException;
            if ((exception == null) || (exception.ResultCode != Result.OutOfMemory))
            {
                return (!(e is OutOfMemoryException) ? IsOutOfMemory(e.InnerException) : true);
            }
            return true;
        }

        private static bool IsOutOfVideoMemory(Exception e)
        {
            if (e == null)
            {
                return false;
            }
            SharpDXException exception = e as SharpDXException;
            if ((exception == null) || (exception.ResultCode.Code != -2005532292))
            {
                return IsOutOfVideoMemory(e.InnerException);
            }
            return true;
        }

        private static bool IsUnsupportedGpu(Exception e)
        {
            SharpDXException exception = e as SharpDXException;
            return ((exception != null) && (exception.Descriptor.NativeApiCode == "DXGI_ERROR_UNSUPPORTED"));
        }

        private static void OnCrash(string logPath, string gameName, string minimumRequirementsPage, bool requiresDX11, Exception e)
        {
            try
            {
                if (MyVideoSettingsManager.GpuUnderMinimum)
                {
                    MyErrorReporter.ReportGpuUnderMinimumCrash(gameName, logPath, minimumRequirementsPage);
                }
                else if (!Sandbox.Engine.Platform.Game.IsDedicated && IsOutOfMemory(e))
                {
                    MyErrorReporter.ReportOutOfMemory(gameName, logPath, minimumRequirementsPage);
                }
                else if (!Sandbox.Engine.Platform.Game.IsDedicated && IsOutOfVideoMemory(e))
                {
                    MyErrorReporter.ReportOutOfVideoMemory(gameName, logPath, minimumRequirementsPage);
                }
                else
                {
                    string text1;
                    bool result = false;
                    if (e.Data.Contains("Silent"))
                    {
                        bool.TryParse((string) e.Data["Silent"], out result);
                    }
                    if (!requiresDX11 || !IsUnsupportedGpu(e))
                    {
                        text1 = "report";
                    }
                    else
                    {
                        text1 = "reporX";
                    }
                    string str = text1;
                    if (MyFakes.ENABLE_MINIDUMP_SENDING)
                    {
                        MyMiniDump.Write(Path.Combine(MyFileSystem.UserDataPath, "Minidump.dmp"), MyMiniDump.Options.Normal | MyMiniDump.Options.WithProcessThreadData | MyMiniDump.Options.WithThreadInfo, MyMiniDump.ExceptionInfo.Present);
                    }
                    if (!result)
                    {
                        if (IsModCrash(e))
                        {
                            new ModCrashedTheGameMesageBox((ModCrashedException) e, logPath).ShowDialog();
                        }
                        else
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.Arguments = string.Format("-{2} \"{0}\" \"{1}\"", logPath, gameName, str);
                            startInfo.FileName = Assembly.GetEntryAssembly().Location;
                            startInfo.UseShellExecute = false;
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.RedirectStandardInput = true;
                            Process.Start(startInfo).StandardInput.Close();
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (MySpaceAnalytics.Instance != null)
                {
                    MySpaceAnalytics.Instance.ReportGameCrash(e);
                    MySpaceAnalytics.Instance.EndSession();
                    MyAnalyticsManager.Instance.FlushAndDispose();
                }
            }
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MySandboxGame.Log.AppendToClosedLog(args.ExceptionObject as Exception);
            HandleSpecialExceptions(args.ExceptionObject as Exception);
            if (!Debugger.IsAttached)
            {
                object exceptionSyncRoot = m_exceptionSyncRoot;
                lock (exceptionSyncRoot)
                {
                    try
                    {
                        MySandboxGame.Log.AppendToClosedLog("Hiding window");
                        MySandboxGame.Log.AppendToClosedLog("Hiding window done");
                    }
                    catch
                    {
                    }
                    MySandboxGame.Log.AppendToClosedLog("Showing message");
                    if (!Sandbox.Engine.Platform.Game.IsDedicated || MyPerGameSettings.SendLogToKeen)
                    {
                        OnCrash(MySandboxGame.Log.GetFilePath(), MyPerGameSettings.GameName, MyPerGameSettings.MinimumRequirementsPage, MyPerGameSettings.RequiresDX11, args.ExceptionObject as Exception);
                    }
                    MySandboxGame.Log.Flush();
                    MySimpleProfiler.LogPerformanceTestResults();
                    Process.GetCurrentProcess().Kill();
                }
            }
        }
    }
}

