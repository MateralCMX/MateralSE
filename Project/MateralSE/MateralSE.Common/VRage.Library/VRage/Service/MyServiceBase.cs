namespace VRage.Service
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.ServiceProcess;
    using System.Threading;

    [InstallerType(typeof(ServiceProcessInstaller))]
    public class MyServiceBase : Component
    {
        private VRage.Service.NativeMethods.SERVICE_STATUS status;
        public const int MaxNameLength = 80;
        private IntPtr statusHandle;
        private VRage.Service.NativeMethods.ServiceControlCallback commandCallback;
        private VRage.Service.NativeMethods.ServiceControlCallbackEx commandCallbackEx;
        private VRage.Service.NativeMethods.ServiceMainCallback mainCallback;
        private IntPtr handleName;
        private ManualResetEvent startCompletedSignal;
        private int acceptedCommands = 1;
        private bool autoLog;
        private string serviceName;
        private System.Diagnostics.EventLog eventLog;
        private bool nameFrozen;
        private bool commandPropsFrozen;
        private bool disposed;
        private bool initialized;
        private bool isServiceHosted;

        public MyServiceBase()
        {
            this.AutoLog = true;
            this.ServiceName = "";
        }

        private unsafe void DeferredContinue()
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            try
            {
                this.OnContinue();
                this.WriteEventLogEntry(GetString("ContinueSuccessful"));
                this.status.currentState = 4;
            }
            catch (Exception exception)
            {
                this.status.currentState = 7;
                object[] args = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("ContinueFailed", args), EventLogEntryType.Error);
                throw;
            }
            finally
            {
                VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
            }
            fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
            {
                return;
            }
        }

        private void DeferredCustomCommand(int command)
        {
            try
            {
                this.OnCustomCommand(command);
                this.WriteEventLogEntry(GetString("CommandSuccessful"));
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("CommandFailed", args), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredPause()
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            try
            {
                this.OnPause();
                this.WriteEventLogEntry(GetString("PauseSuccessful"));
                this.status.currentState = 7;
            }
            catch (Exception exception)
            {
                this.status.currentState = 4;
                object[] args = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("PauseFailed", args), EventLogEntryType.Error);
                throw;
            }
            finally
            {
                VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
            }
            fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
            {
                return;
            }
        }

        private void DeferredPowerEvent(int eventType, IntPtr eventData)
        {
            try
            {
                this.OnPowerEvent((PowerBroadcastStatus) eventType);
                this.WriteEventLogEntry(GetString("PowerEventOK"));
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("PowerEventFailed", args), EventLogEntryType.Error);
                throw;
            }
        }

        private void DeferredSessionChange(int eventType, int sessionId)
        {
            try
            {
                this.OnSessionChange(VRage.Service.NativeMethods.CreateSessionChangeDescription((SessionChangeReason) eventType, sessionId));
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("SessionChangeFailed", args), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredShutdown()
        {
            try
            {
                this.OnShutdown();
                this.WriteEventLogEntry(GetString("ShutdownOK"));
                if ((this.status.currentState == 7) || (this.status.currentState == 4))
                {
                    try
                    {
                        VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
                        this.status.checkPoint = 0;
                        this.status.waitHint = 0;
                        this.status.currentState = 1;
                        VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                        if (this.isServiceHosted)
                        {
                            try
                            {
                                AppDomain.Unload(AppDomain.CurrentDomain);
                            }
                            catch (CannotUnloadAppDomainException exception)
                            {
                                object[] args = new object[] { AppDomain.CurrentDomain.FriendlyName, exception.Message };
                                this.WriteEventLogEntry(GetString("FailedToUnloadAppDomain", args), EventLogEntryType.Error);
                            }
                        }
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception exception2)
            {
                object[] args = new object[] { exception2.ToString() };
                this.WriteEventLogEntry(GetString("ShutdownFailed", args), EventLogEntryType.Error);
                throw;
            }
        }

        private unsafe void DeferredStop()
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            int currentState = this.status.currentState;
            this.status.checkPoint = 0;
            this.status.waitHint = 0;
            this.status.currentState = 3;
            VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
            try
            {
                this.OnStop();
                this.WriteEventLogEntry(GetString("StopSuccessful"));
                this.status.currentState = 1;
                VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                if (this.isServiceHosted)
                {
                    try
                    {
                        AppDomain.Unload(AppDomain.CurrentDomain);
                    }
                    catch (CannotUnloadAppDomainException exception)
                    {
                        object[] args = new object[] { AppDomain.CurrentDomain.FriendlyName, exception.Message };
                        this.WriteEventLogEntry(GetString("FailedToUnloadAppDomain", args), EventLogEntryType.Error);
                    }
                }
            }
            catch (Exception exception2)
            {
                this.status.currentState = currentState;
                VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                object[] args = new object[] { exception2.ToString() };
                this.WriteEventLogEntry(GetString("StopFailed", args), EventLogEntryType.Error);
                throw;
            }
            fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
            {
                return;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.handleName != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.handleName);
                this.handleName = IntPtr.Zero;
            }
            this.nameFrozen = false;
            this.commandPropsFrozen = false;
            this.disposed = true;
            base.Dispose(disposing);
        }

        private VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY GetEntry()
        {
            VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY service_table_entry = new VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY();
            this.nameFrozen = true;
            service_table_entry.callback = this.mainCallback;
            service_table_entry.name = this.handleName;
            return service_table_entry;
        }

        private static string GetString(string str) => 
            str;

        private static string GetString(string str, params object[] args)
        {
            foreach (object obj2 in args)
            {
                string text1 = str + ", " + obj2;
                str = text1;
            }
            return str;
        }

        private void Initialize(bool multipleServices)
        {
            if (!this.initialized)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                this.status.serviceType = multipleServices ? 0x20 : 0x10;
                this.status.currentState = 2;
                this.status.controlsAccepted = 0;
                this.status.win32ExitCode = 0;
                this.status.serviceSpecificExitCode = 0;
                this.status.checkPoint = 0;
                this.status.waitHint = 0;
                this.mainCallback = new VRage.Service.NativeMethods.ServiceMainCallback(this.ServiceMainCallback);
                this.commandCallback = new VRage.Service.NativeMethods.ServiceControlCallback(this.ServiceCommandCallback);
                this.commandCallbackEx = new VRage.Service.NativeMethods.ServiceControlCallbackEx(this.ServiceCommandCallbackEx);
                this.handleName = Marshal.StringToHGlobalUni(this.ServiceName);
                this.initialized = true;
            }
        }

        private static void LateBoundMessageBoxShow(string message, string title)
        {
            int num = 0;
            if (IsRTLResources)
            {
                num |= 0x180000;
            }
            Type type = Type.GetType("System.Windows.Forms.MessageBoxButtons, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type enumType = Type.GetType("System.Windows.Forms.MessageBoxIcon, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type type4 = Type.GetType("System.Windows.Forms.MessageBoxDefaultButton, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Type type5 = Type.GetType("System.Windows.Forms.MessageBoxOptions, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            object[] args = new object[] { message, title, Enum.ToObject(type, 0), Enum.ToObject(enumType, 0), Enum.ToObject(type4, 0), Enum.ToObject(type5, num) };
            Type.GetType("System.Windows.Forms.MessageBox, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").InvokeMember("Show", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, args, CultureInfo.InvariantCulture);
        }

        protected virtual void OnContinue()
        {
        }

        protected virtual void OnCustomCommand(int command)
        {
        }

        protected virtual void OnPause()
        {
        }

        protected virtual bool OnPowerEvent(PowerBroadcastStatus powerStatus) => 
            true;

        protected virtual void OnSessionChange(SessionChangeDescription changeDescription)
        {
        }

        protected virtual void OnShutdown()
        {
        }

        protected virtual void OnStart(string[] args)
        {
        }

        protected virtual void OnStop()
        {
        }

        [ComVisible(false)]
        public unsafe void RequestAdditionalTime(int milliseconds)
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            if (((this.status.currentState != 5) && ((this.status.currentState != 2) && (this.status.currentState != 3))) && (this.status.currentState != 6))
            {
                throw new InvalidOperationException(GetString("NotInPendingState"));
            }
            this.status.waitHint = milliseconds;
            int* numPtr1 = (int*) ref this.status.checkPoint;
            numPtr1[0]++;
            VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
            fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
            {
                return;
            }
        }

        public static void Run(MyServiceBase[] services)
        {
            if ((services == null) || (services.Length == 0))
            {
                throw new ArgumentException(GetString("NoServices"));
            }
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                LateBoundMessageBoxShow(GetString("CantRunOnWin9x"), GetString("CantRunOnWin9xTitle"));
            }
            else
            {
                IntPtr entry = Marshal.AllocHGlobal((IntPtr) ((services.Length + 1) * Marshal.SizeOf(typeof(VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY))));
                VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY[] service_table_entryArray = new VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY[services.Length];
                bool multipleServices = services.Length > 1;
                for (int i = 0; i < services.Length; i++)
                {
                    services[i].Initialize(multipleServices);
                    service_table_entryArray[i] = services[i].GetEntry();
                    IntPtr ptr = (IntPtr) (((long) entry) + (Marshal.SizeOf(typeof(VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY)) * i));
                    Marshal.StructureToPtr(service_table_entryArray[i], ptr, true);
                }
                VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY structure = new VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY {
                    callback = null,
                    name = IntPtr.Zero
                };
                Marshal.StructureToPtr(structure, (IntPtr) (((long) entry) + (Marshal.SizeOf(typeof(VRage.Service.NativeMethods.SERVICE_TABLE_ENTRY)) * services.Length)), true);
                bool flag2 = VRage.Service.NativeMethods.StartServiceCtrlDispatcher(entry);
                string message = "";
                if (!flag2)
                {
                    message = new Win32Exception().Message;
                    string str2 = GetString("CantStartFromCommandLine");
                    if (Environment.UserInteractive)
                    {
                        LateBoundMessageBoxShow(str2, GetString("CantStartFromCommandLineTitle"));
                    }
                    else
                    {
                        Console.WriteLine(str2);
                    }
                }
                foreach (MyServiceBase base2 in services)
                {
                    base2.Dispose();
                    if (!flag2 && (base2.EventLog.Source.Length != 0))
                    {
                        object[] args = new object[] { message };
                        base2.WriteEventLogEntry(GetString("StartFailed", args), EventLogEntryType.Error);
                    }
                }
            }
        }

        public static void Run(MyServiceBase service)
        {
            if (service == null)
            {
                throw new ArgumentException(GetString("NoServices"));
            }
            MyServiceBase[] services = new MyServiceBase[] { service };
            Run(services);
        }

        private unsafe void ServiceCommandCallback(int command)
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            if (command == 4)
            {
                VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
            }
            else if (((this.status.currentState != 5) && ((this.status.currentState != 2) && (this.status.currentState != 3))) && (this.status.currentState != 6))
            {
                switch (command)
                {
                    case 1:
                    {
                        int currentState = this.status.currentState;
                        if ((this.status.currentState == 7) || (this.status.currentState == 4))
                        {
                            this.status.currentState = 3;
                            VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                            this.status.currentState = currentState;
                            new DeferredHandlerDelegate(this.DeferredStop).BeginInvoke(null, null);
                        }
                        break;
                    }
                    case 2:
                        if (this.status.currentState == 4)
                        {
                            this.status.currentState = 6;
                            VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                            new DeferredHandlerDelegate(this.DeferredPause).BeginInvoke(null, null);
                        }
                        break;

                    case 3:
                        if (this.status.currentState == 7)
                        {
                            this.status.currentState = 5;
                            VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                            new DeferredHandlerDelegate(this.DeferredContinue).BeginInvoke(null, null);
                        }
                        break;

                    case 5:
                        new DeferredHandlerDelegate(this.DeferredShutdown).BeginInvoke(null, null);
                        break;

                    default:
                        new DeferredHandlerDelegateCommand(this.DeferredCustomCommand).BeginInvoke(command, null, null);
                        break;
                }
            }
            fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
            {
                return;
            }
        }

        private int ServiceCommandCallbackEx(int command, int eventType, IntPtr eventData, IntPtr eventContext)
        {
            if (command == 13)
            {
                new DeferredHandlerDelegateAdvanced(this.DeferredPowerEvent).BeginInvoke(eventType, eventData, null, null);
            }
            else if (command != 14)
            {
                this.ServiceCommandCallback(command);
            }
            else
            {
                DeferredHandlerDelegateAdvancedSession session = new DeferredHandlerDelegateAdvancedSession(this.DeferredSessionChange);
                VRage.Service.NativeMethods.WTSSESSION_NOTIFICATION structure = new VRage.Service.NativeMethods.WTSSESSION_NOTIFICATION();
                Marshal.PtrToStructure(eventData, structure);
                session.BeginInvoke(eventType, structure.sessionId, null, null);
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Never), ComVisible(false)]
        public unsafe void ServiceMainCallback(int argCount, IntPtr argPointer)
        {
            VRage.Service.NativeMethods.SERVICE_STATUS* status = &this.status;
            string[] state = null;
            if (argCount > 0)
            {
                char** chPtr = (char**) argPointer.ToPointer();
                this.UsedServiceName = Marshal.PtrToStringUni(*((IntPtr*) chPtr));
                state = new string[argCount - 1];
                for (int i = 0; i < state.Length; i++)
                {
                    chPtr++;
                    state[i] = Marshal.PtrToStringUni(*((IntPtr*) chPtr));
                }
            }
            if (!this.initialized)
            {
                this.isServiceHosted = true;
                this.Initialize(true);
            }
            this.statusHandle = (Environment.OSVersion.Version.Major < 5) ? VRage.Service.NativeMethods.RegisterServiceCtrlHandler(this.ServiceName, this.commandCallback) : VRage.Service.NativeMethods.RegisterServiceCtrlHandlerEx(this.ServiceName, this.commandCallbackEx, IntPtr.Zero);
            this.nameFrozen = true;
            if (this.statusHandle == IntPtr.Zero)
            {
                object[] args = new object[] { new Win32Exception().Message };
                this.WriteEventLogEntry(GetString("StartFailed", args), EventLogEntryType.Error);
            }
            this.status.controlsAccepted = this.acceptedCommands;
            this.commandPropsFrozen = true;
            if ((this.status.controlsAccepted & 1) != 0)
            {
                this.status.controlsAccepted |= 4;
            }
            if (Environment.OSVersion.Version.Major < 5)
            {
                int* numPtr1 = (int*) ref this.status.controlsAccepted;
                numPtr1[0] &= -65;
            }
            this.status.currentState = 2;
            if (VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status))
            {
                this.startCompletedSignal = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServiceQueuedMainCallback), state);
                this.startCompletedSignal.WaitOne();
                if (!VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status))
                {
                    object[] args = new object[] { new Win32Exception().Message };
                    this.WriteEventLogEntry(GetString("StartFailed", args), EventLogEntryType.Error);
                    this.status.currentState = 1;
                    VRage.Service.NativeMethods.SetServiceStatus(this.statusHandle, status);
                }
                fixed (VRage.Service.NativeMethods.SERVICE_STATUS* service_statusRef = null)
                {
                    return;
                }
            }
        }

        private void ServiceQueuedMainCallback(object state)
        {
            string[] args = (string[]) state;
            try
            {
                this.OnStart(args);
                this.WriteEventLogEntry(GetString("StartSuccessful"));
                this.status.checkPoint = 0;
                this.status.waitHint = 0;
                this.status.currentState = 4;
            }
            catch (Exception exception)
            {
                object[] objArray1 = new object[] { exception.ToString() };
                this.WriteEventLogEntry(GetString("StartFailed", objArray1), EventLogEntryType.Error);
                this.status.currentState = 1;
            }
            this.startCompletedSignal.Set();
        }

        public void Stop()
        {
            this.DeferredStop();
        }

        private static bool ValidServiceName(string serviceName)
        {
            if (((serviceName == null) || (serviceName.Length > 80)) || (serviceName.Length == 0))
            {
                return false;
            }
            foreach (char ch in serviceName.ToCharArray())
            {
                if ((ch == '/') || (ch == '\\'))
                {
                    return false;
                }
            }
            return true;
        }

        private void WriteEventLogEntry(string message)
        {
            try
            {
                if (this.AutoLog)
                {
                    this.EventLog.WriteEntry(message);
                }
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
            }
        }

        private void WriteEventLogEntry(string message, EventLogEntryType errorType)
        {
            try
            {
                if (this.AutoLog)
                {
                    this.EventLog.WriteEntry(message, errorType);
                }
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
            }
        }

        public string UsedServiceName { get; private set; }

        [DefaultValue(true), ServiceProcessDescription("SBAutoLog")]
        public bool AutoLog
        {
            get => 
                this.autoLog;
            set => 
                (this.autoLog = value);
        }

        [ComVisible(false)]
        public int ExitCode
        {
            get => 
                this.status.win32ExitCode;
            set => 
                (this.status.win32ExitCode = value);
        }

        [DefaultValue(false)]
        public bool CanHandlePowerEvent
        {
            get => 
                ((this.acceptedCommands & 0x40) != 0);
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 0x40;
                }
                else
                {
                    this.acceptedCommands &= -65;
                }
            }
        }

        [ComVisible(false), DefaultValue(false)]
        public bool CanHandleSessionChangeEvent
        {
            get => 
                ((this.acceptedCommands & 0x80) != 0);
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 0x80;
                }
                else
                {
                    this.acceptedCommands &= -129;
                }
            }
        }

        [DefaultValue(false)]
        public bool CanPauseAndContinue
        {
            get => 
                ((this.acceptedCommands & 2) != 0);
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 2;
                }
                else
                {
                    this.acceptedCommands &= -3;
                }
            }
        }

        [DefaultValue(false)]
        public bool CanShutdown
        {
            get => 
                ((this.acceptedCommands & 4) != 0);
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 4;
                }
                else
                {
                    this.acceptedCommands &= -5;
                }
            }
        }

        [DefaultValue(true)]
        public bool CanStop
        {
            get => 
                ((this.acceptedCommands & 1) != 0);
            set
            {
                if (this.commandPropsFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeProperties"));
                }
                if (value)
                {
                    this.acceptedCommands |= 1;
                }
                else
                {
                    this.acceptedCommands &= -2;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual System.Diagnostics.EventLog EventLog
        {
            get
            {
                if (this.eventLog == null)
                {
                    this.eventLog = new System.Diagnostics.EventLog();
                    this.eventLog.Source = this.ServiceName;
                    this.eventLog.Log = "Application";
                }
                return this.eventLog;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected IntPtr ServiceHandle
        {
            get
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                return this.statusHandle;
            }
        }

        [ServiceProcessDescription("SBServiceName"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string ServiceName
        {
            get => 
                this.serviceName;
            set
            {
                if (this.nameFrozen)
                {
                    throw new InvalidOperationException(GetString("CannotChangeName"));
                }
                if ((value == "") || ValidServiceName(value))
                {
                    this.serviceName = value;
                }
                else
                {
                    object[] args = new object[] { value, 80.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(GetString("ServiceName", args));
                }
            }
        }

        private static bool IsRTLResources =>
            false;

        private delegate void DeferredHandlerDelegate();

        private delegate void DeferredHandlerDelegateAdvanced(int eventType, IntPtr eventData);

        private delegate void DeferredHandlerDelegateAdvancedSession(int eventType, int sessionId);

        private delegate void DeferredHandlerDelegateCommand(int command);
    }
}

