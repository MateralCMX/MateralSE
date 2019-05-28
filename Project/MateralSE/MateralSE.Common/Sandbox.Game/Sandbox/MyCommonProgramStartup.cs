namespace Sandbox
{
    using ParallelTasks;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ObjectBuilder;
    using VRage.GameServices;
    using VRage.Library;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRage.Win32;
    using VRageRender;

    public class MyCommonProgramStartup
    {
        private string[] m_args;
        private static MySplashScreen splashScreen;
        private static IMyRender m_renderer;

        public MyCommonProgramStartup(string[] args)
        {
            int? gameVersion = this.GameInfo.GameVersion;
            MyFinalBuildConstants.APP_VERSION = (gameVersion != null) ? ((MyVersion) gameVersion.GetValueOrDefault()) : null;
            this.m_args = args;
        }

        public bool Check64Bit()
        {
            if (VRage.Library.MyEnvironment.Is64BitProcess || (AssemblyExtensions.TryGetArchitecture("SteamSDK.dll") != ProcessorArchitecture.Amd64))
            {
                return true;
            }
            string text = (((this.GameInfo.GameName + " cannot be started in 64-bit mode, ") + "because 64-bit version of .NET framework is not available or is broken." + VRage.Library.MyEnvironment.NewLine + VRage.Library.MyEnvironment.NewLine) + "Do you want to open website with more information about this particular issue?" + VRage.Library.MyEnvironment.NewLine + VRage.Library.MyEnvironment.NewLine) + "Press Yes to open website with info" + VRage.Library.MyEnvironment.NewLine;
            string[] textArray1 = new string[] { text, "Press No to run in 32-bit mode (smaller potential of ", this.GameInfo.GameName, "!)", VRage.Library.MyEnvironment.NewLine };
            text = string.Concat(textArray1) + "Press Cancel to close this dialog";
            MessageBoxResult result = Sandbox.MyMessageBox.Show(IntPtr.Zero, text, ".NET Framework 64-bit error", MessageBoxOptions.AbortRetryIgnore | MessageBoxOptions.OkCancel);
            if (result == MessageBoxResult.Yes)
            {
                MyBrowserHelper.OpenInternetBrowser("http://www.spaceengineersgame.com/64-bit-start-up-issue.html");
            }
            else if (result == MessageBoxResult.No)
            {
                string location = Assembly.GetEntryAssembly().Location;
                string path = Path.Combine(new FileInfo(location).Directory.Parent.FullName, "Bin", Path.GetFileName(location));
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = path;
                startInfo.WorkingDirectory = Path.GetDirectoryName(path);
                startInfo.Arguments = "-fallback";
                startInfo.UseShellExecute = false;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                Process.Start(startInfo);
            }
            return false;
        }

        public bool CheckSingleInstance()
        {
            if (new MySingleProgramInstance(MyFileSystem.MainAssemblyName).IsSingleInstance)
            {
                return true;
            }
            MyErrorReporter.ReportAppAlreadyRunning(this.GameInfo.GameName);
            return false;
        }

        public bool CheckSteamRunning()
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                if (!MyGameService.IsActive)
                {
                    if ((!MyGameService.IsActive || !MyGameService.OwnsGame) && !MyFakes.ENABLE_RUN_WITHOUT_STEAM)
                    {
                        MessageBoxWrapper("Steam is not running!", "Please run this game from Steam." + VRage.Library.MyEnvironment.NewLine + "(restart Steam if already running)");
                        return false;
                    }
                }
                else
                {
                    MyGameService.SetNotificationPosition(NotificationPosition.TopLeft);
                    MySandboxGame.Log.WriteLineAndConsole("Steam.IsActive: " + MyGameService.IsActive.ToString());
                    MySandboxGame.Log.WriteLineAndConsole("Steam.IsOnline: " + MyGameService.IsOnline.ToString());
                    MySandboxGame.Log.WriteLineAndConsole("Steam.OwnsGame: " + MyGameService.OwnsGame.ToString());
                    MySandboxGame.Log.WriteLineAndConsole("Steam.UserId: " + MyGameService.UserId);
                    MySandboxGame.Log.WriteLineAndConsole("Steam.UserName: " + MyGameService.UserName);
                    MySandboxGame.Log.WriteLineAndConsole("Steam.Branch: " + MyGameService.BranchName);
                    MySandboxGame.Log.WriteLineAndConsole("Build date: " + MySandboxGame.BuildDateTime.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture));
                    MySandboxGame.Log.WriteLineAndConsole("Build version: " + MySandboxGame.BuildVersion.ToString());
                }
            }
            return true;
        }

        public void DetectSharpDxLeaksAfterRun()
        {
        }

        public void DetectSharpDxLeaksBeforeRun()
        {
        }

        public void DisposeSplashScreen()
        {
            if (splashScreen != null)
            {
                splashScreen.Hide();
                splashScreen.Dispose();
            }
        }

        public static void ForceStaticCtor(Type[] types)
        {
            Type[] typeArray = types;
            for (int i = 0; i < typeArray.Length; i++)
            {
                RuntimeHelpers.RunClassConstructor(typeArray[i].TypeHandle);
            }
        }

        public string GetAppDataPath()
        {
            string fullPath = null;
            int index = Array.IndexOf<string>(this.m_args, "-appdata") + 1;
            if ((index != 0) && (this.m_args.Length > index))
            {
                string name = this.m_args[index];
                if (!name.StartsWith("-"))
                {
                    fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(name));
                }
            }
            if (fullPath == null)
            {
                fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.GameInfo.ApplicationName);
            }
            return fullPath;
        }

        public void InitSplashScreen()
        {
            if (MyFakes.ENABLE_SPLASHSCREEN && !this.m_args.Contains<string>("-nosplash"))
            {
                splashScreen = new MySplashScreen(this.GameInfo.SplashScreenImage, new PointF(0.7f, 0.7f));
                splashScreen.Draw();
            }
        }

        public bool IsRenderUpdateSyncEnabled() => 
            this.m_args.Contains<string>("-render_sync");

        public bool IsVideoRecordingEnabled() => 
            this.m_args.Contains<string>("-video_record");

        public static void MessageBoxWrapper(string caption, string text)
        {
            WinApi.MessageBox(IntPtr.Zero, text, caption, 0);
        }

        public void PerformAutoconnect()
        {
            if (MyFakes.ENABLE_CONNECT_COMMAND_LINE && this.m_args.Contains<string>("+connect"))
            {
                int index = this.m_args.ToList<string>().IndexOf("+connect");
                if (((index + 1) < this.m_args.Length) && IPAddressExtensions.TryParseEndpoint(this.m_args[index + 1], out Sandbox.Engine.Platform.Game.ConnectToServer))
                {
                    Console.WriteLine(this.GameInfo.GameName + " " + MyFinalBuildConstants.APP_VERSION_STRING);
                    Console.WriteLine("Obfuscated: " + MyObfuscation.Enabled.ToString() + ", Platform: " + (VRage.Library.MyEnvironment.Is64BitProcess ? " 64-bit" : " 32-bit"));
                    Console.WriteLine("Connecting to: " + this.m_args[index + 1]);
                }
            }
        }

        public bool PerformColdStart()
        {
            if (!this.m_args.Contains<string>("-coldstart"))
            {
                return false;
            }
            MyGlobalTypeMetadata.Static.Init(false);
            Parallel.Scheduler = new PrioritizedScheduler(1);
            int num = -1;
            List<string> list = new List<string>();
            Queue<AssemblyName> queue = new Queue<AssemblyName>();
            queue.Enqueue(Assembly.GetEntryAssembly().GetName());
            while (queue.Count > 0)
            {
                AssemblyName assemblyRef = queue.Dequeue();
                if (!list.Contains(assemblyRef.FullName))
                {
                    list.Add(assemblyRef.FullName);
                    try
                    {
                        Assembly assembly = Assembly.Load(assemblyRef);
                        PreloadTypesFrom(assembly);
                        num++;
                        foreach (AssemblyName name2 in assembly.GetReferencedAssemblies())
                        {
                            queue.Enqueue(name2);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            if (MyFakes.ENABLE_NGEN)
            {
                ProcessStartInfo info1 = new ProcessStartInfo(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen"));
                info1.Verb = "runas";
                ProcessStartInfo startInfo = info1;
                startInfo.Arguments = "install SpaceEngineers.exe /silent /nologo";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                try
                {
                    Process.Start(startInfo).WaitForExit();
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine("NGEN failed: " + exception);
                }
            }
            return true;
        }

        public void PerformNotInteractiveReport()
        {
            MyErrorReporter.ReportNotInteractive(MyLog.Default.GetFilePath(), this.GameInfo.GameAcronym);
        }

        public bool PerformReporting()
        {
            if (this.m_args.Contains<string>("-report"))
            {
                MyErrorReporter.ReportGeneral(this.m_args[1], this.m_args[2], this.GameInfo.GameAcronym);
                return true;
            }
            if (!this.m_args.Contains<string>("-reporX"))
            {
                return false;
            }
            string errorMessage = string.Format(MyTexts.SubstituteTexts(MyErrorReporter.APP_ERROR_MESSAGE_DX11_NOT_AVAILABLE, null).ToString().Replace(@"\n", "\r\n"), this.m_args[1], this.m_args[2], this.GameInfo.MinimumRequirementsWeb);
            MyErrorReporter.Report(this.m_args[1], this.m_args[2], this.GameInfo.GameAcronym, errorMessage);
            return true;
        }

        private static void PreloadTypesFrom(Assembly assembly)
        {
            if (assembly != null)
            {
                ForceStaticCtor((from type in assembly.GetTypes()
                    where Attribute.IsDefined(type, typeof(PreloadRequiredAttribute))
                    select type).ToArray<Type>());
            }
        }

        private MyBasicGameInfo GameInfo =>
            MyPerGameSettings.BasicGameInfo;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCommonProgramStartup.<>c <>9 = new MyCommonProgramStartup.<>c();
            public static Func<Type, bool> <>9__13_0;

            internal bool <PreloadTypesFrom>b__13_0(Type type) => 
                Attribute.IsDefined(type, typeof(PreloadRequiredAttribute));
        }
    }
}

