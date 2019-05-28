namespace VRage.Utils
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Text;
    using System.Threading.Tasks;

    public class MyConsolePipeWriter : TextWriter
    {
        private static object lockObject = new object();
        private NamedPipeClientStream m_pipeStream;
        private StreamWriter m_writer;
        private bool isConnecting;

        public MyConsolePipeWriter(string name)
        {
            this.m_pipeStream = new NamedPipeClientStream(name);
            this.m_writer = new StreamWriter(this.m_pipeStream);
            this.StartConnectThread();
        }

        public override void Close()
        {
            base.Close();
            try
            {
                if (this.m_pipeStream.IsConnected)
                {
                    this.m_pipeStream.WaitForPipeDrain();
                    this.m_writer.Close();
                    this.m_writer.Dispose();
                    this.m_pipeStream.Close();
                    this.m_pipeStream.Dispose();
                }
            }
            catch
            {
            }
        }

        private void StartConnectThread()
        {
            object lockObject = MyConsolePipeWriter.lockObject;
            lock (lockObject)
            {
                if (!this.isConnecting)
                {
                    this.isConnecting = true;
                }
                else
                {
                    return;
                }
            }
            Task.Run(delegate {
                this.m_pipeStream.Connect();
                object obj2 = MyConsolePipeWriter.lockObject;
                lock (obj2)
                {
                    this.isConnecting = false;
                }
            });
        }

        public override void Write(string value)
        {
            if (!this.m_pipeStream.IsConnected)
            {
                this.StartConnectThread();
            }
            else
            {
                try
                {
                    this.m_writer.Write(value);
                    this.m_writer.Flush();
                }
                catch (IOException)
                {
                    this.StartConnectThread();
                }
            }
        }

        public override void WriteLine(string value)
        {
            if (!this.m_pipeStream.IsConnected)
            {
                this.StartConnectThread();
            }
            else
            {
                try
                {
                    this.m_writer.WriteLine(value);
                    this.m_writer.Flush();
                }
                catch (IOException)
                {
                    this.StartConnectThread();
                }
            }
        }

        public override System.Text.Encoding Encoding =>
            System.Text.Encoding.UTF8;
    }
}

