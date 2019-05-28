namespace DShowNET
{
    using DirectShowLib;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Collections;

    public abstract class VideoPlayer : DShowNET.ISampleGrabberCB, IDisposable
    {
        private Guid MEDIATYPE_Video = new Guid(0x73646976, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
        private Guid MEDIATYPE_Audio = new Guid(0x73647561, 0, 0x10, 0x80, 0, 0, 170, 0, 0x38, 0x9b, 0x71);
        private Guid MEDIASUBTYPE_RGB24 = new Guid(0xe436eb7d, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        private Guid MEDIASUBTYPE_RGB32 = new Guid(0xe436eb7e, 0x524f, 0x11ce, 0x9f, 0x53, 0, 0x20, 0xaf, 11, 0xa7, 0x70);
        private Guid FORMAT_VideoInfo = new Guid(0x5589f80, 0xc356, 0x11ce, 0xbf, 1, 0, 170, 0, 0x55, 0x59, 90);
        private object m_comObject;
        protected IGraphBuilder m_graphBuilder;
        private DShowNET.IMediaControl m_mediaControl;
        private DShowNET.IMediaEventEx m_mediaEvent;
        private DShowNET.IMediaPosition m_mediaPosition;
        private DShowNET.IBasicAudio m_basicAudio;
        private DShowNET.IMediaSeeking m_mediaSeeking;
        private MySwapQueue<byte[]> m_videoDataRgba;
        private int videoWidth;
        private int videoHeight;
        private long avgTimePerFrame;
        private int bitRate;
        private VideoState currentState;
        private bool isDisposed;
        private long currentPosition;
        private long videoDuration;
        private byte alphaTransparency = 0xff;

        protected VideoPlayer(string FileName)
        {
            try
            {
                this.currentState = VideoState.Stopped;
                this.m_graphBuilder = (IGraphBuilder) new FilterGraph();
                this.BuildGraph(this.m_graphBuilder, FileName);
                this.m_mediaControl = (DShowNET.IMediaControl) this.m_graphBuilder;
                this.m_mediaEvent = (DShowNET.IMediaEventEx) this.m_graphBuilder;
                this.m_mediaSeeking = (DShowNET.IMediaSeeking) this.m_graphBuilder;
                this.m_mediaPosition = (DShowNET.IMediaPosition) this.m_graphBuilder;
                this.m_basicAudio = (DShowNET.IBasicAudio) this.m_graphBuilder;
                this.m_mediaSeeking.GetDuration(out this.videoDuration);
                this.Play();
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to Load or Play the video file", exception);
            }
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            byte[] write = this.m_videoDataRgba.Write;
            byte alphaTransparency = this.alphaTransparency;
            Marshal.Copy(pBuffer, write, 0, BufferLen);
            for (int i = 3; i < BufferLen; i += 4)
            {
                write[i] = alphaTransparency;
            }
            this.m_videoDataRgba.CommitWrite();
            return 0;
        }

        private void BuildGraph(IGraphBuilder pGraph, string filename)
        {
            IBaseFilter filter;
            int hr = 0;
            ICaptureGraphBuilder2 builder = (ICaptureGraphBuilder2) new CaptureGraphBuilder2();
            checkHR(builder.SetFiltergraph(pGraph), "Can't SetFilterGraph", true);
            checkHR(pGraph.AddSourceFilter(filename, filename, out filter), "Can't add source filter to graph", true);
            IBaseFilter pFilter = (IBaseFilter) new WMAsfReader();
            checkHR(pGraph.AddFilter(pFilter, "WM ASF Reader"), "Can't add WM ASF Reader to graph", true);
            IFileSourceFilter filter3 = pFilter as IFileSourceFilter;
            if (filter3 == null)
            {
                checkHR(-2147467262, "Can't get IFileSourceFilter", true);
            }
            checkHR(filter3.Load(filename, null), "Can't load file", true);
            IBaseFilter filter4 = (IBaseFilter) new DMOWrapperFilter();
            IDMOWrapperFilter filter5 = filter4 as IDMOWrapperFilter;
            if (filter5 == null)
            {
                checkHR(-2147467262, "Can't get WMVidero Decoder DMO", true);
            }
            checkHR(filter5.Init(Clsid.WMVideoDecoderDMO, Clsid.WMVideoDecoderDMO_cat), "DMO wrapper init failed", true);
            checkHR(pGraph.AddFilter(filter4, "WMVideo Decoder DMO"), "Can't add WMVideo Decoder DMO to graph", true);
            checkHR(pGraph.ConnectDirect(GetPin(pFilter, "Raw Video"), GetPin(filter4, "in0"), null), "Can't connect WM ASF Reader and WMVideo Decoder DMO", true);
            IBaseFilter filter6 = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber));
            checkHR(pGraph.AddFilter(filter6, "Sample Grabber"), "Can't add Sample Grabber to graph", true);
            ((DShowNET.IVideoWindow) this.m_graphBuilder).put_AutoShow(0);
            AMMediaType pmt = new AMMediaType {
                majorType = this.MEDIATYPE_Video,
                subType = MediaSubType.RGB32,
                formatType = this.FORMAT_VideoInfo
            };
            hr = ((DShowNET.ISampleGrabber) filter6).SetMediaType(pmt);
            checkHR(((DShowNET.ISampleGrabber) filter6).SetBufferSamples(true), "Can't set buffer samples", true);
            checkHR(((DShowNET.ISampleGrabber) filter6).SetOneShot(false), "Can't set One shot false", true);
            checkHR(((DShowNET.ISampleGrabber) filter6).SetCallback(this, 1), "Can't set callback", true);
            checkHR(pGraph.ConnectDirect(GetPin(filter4, "out0"), GetPin(filter6, "Input"), null), "Can't connect WMVideo Decoder DMO to Sample Grabber", true);
            AMMediaType type2 = new AMMediaType();
            checkHR(((DShowNET.ISampleGrabber) filter6).GetConnectedMediaType(type2), "Can't get media type", true);
            DShowNET.VideoInfoHeader structure = new DShowNET.VideoInfoHeader();
            Marshal.PtrToStructure<DShowNET.VideoInfoHeader>(type2.formatPtr, structure);
            IBaseFilter filter7 = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.NullRenderer));
            checkHR(pGraph.AddFilter(filter7, "Null Renderer"), "Can't add Null Renderer to graph", true);
            hr = builder.RenderStream(null, this.MEDIATYPE_Audio, filter, null, null);
            if ((hr != -2147467259) || (hr == 0))
            {
                checkHR(hr, "Can't add Audio Renderer to graph", false);
            }
            checkHR(pGraph.ConnectDirect(GetPin(filter6, "Output"), GetPin(filter7, "In"), null), "Can't connect Sample grabber to Null renderer", true);
            this.videoHeight = structure.BmiHeader.Height;
            this.videoWidth = structure.BmiHeader.Width;
            this.avgTimePerFrame = structure.AvgTimePerFrame;
            this.bitRate = structure.BitRate;
            this.m_videoDataRgba = new MySwapQueue<byte[]>(() => new byte[(this.videoHeight * this.videoWidth) * 4]);
        }

        private static void checkHR(int hr, string msg, bool throwException = true)
        {
            if (hr < 0)
            {
                string[] textArray1 = new string[] { "\n", hr.ToString(), "  ", msg, "\n" };
                Console.Write(string.Concat(textArray1));
                if (throwException)
                {
                    DsError.ThrowExceptionForHR(hr);
                }
            }
        }

        private void CloseInterfaces()
        {
            if (this.m_mediaEvent != null)
            {
                this.m_mediaControl.Stop();
                this.m_mediaEvent.SetNotifyWindow(IntPtr.Zero, 0x8001, IntPtr.Zero);
            }
            this.m_mediaControl = null;
            this.m_mediaEvent = null;
            this.m_graphBuilder = null;
            this.m_mediaSeeking = null;
            this.m_mediaPosition = null;
            this.m_basicAudio = null;
            if (this.m_comObject != null)
            {
                Marshal.ReleaseComObject(this.m_comObject);
            }
            this.m_comObject = null;
        }

        public virtual void Dispose()
        {
            this.isDisposed = true;
            this.Stop();
            this.CloseInterfaces();
            this.m_videoDataRgba = null;
        }

        private static IPin GetPin(IBaseFilter filter, string pinname)
        {
            IEnumPins pins;
            checkHR(filter.EnumPins(out pins), "Can't enumerate pins", true);
            IntPtr pcFetched = Marshal.AllocCoTaskMem(4);
            IPin[] ppPins = new IPin[1];
            while (pins.Next(1, ppPins, pcFetched) == 0)
            {
                PinInfo info;
                ppPins[0].QueryPinInfo(out info);
                bool flag = info.name.Contains(pinname);
                DsUtils.FreePinInfo(info);
                if (flag)
                {
                    return ppPins[0];
                }
            }
            checkHR(-1, pinname + "  Pin not found \n", true);
            return null;
        }

        protected abstract void OnFrame(byte[] frameData);
        public void Pause()
        {
            this.m_mediaControl.Stop();
            this.currentState = VideoState.Paused;
        }

        public void Play()
        {
            if (this.currentState != VideoState.Playing)
            {
                checkHR(this.m_mediaControl.Run(), "Can't run the graph", true);
                this.currentState = VideoState.Playing;
            }
        }

        public void Rewind()
        {
            this.Stop();
            this.Play();
        }

        public int SampleCB(double SampleTime, DShowNET.IMediaSample pSample) => 
            0;

        public void Stop()
        {
            this.m_mediaControl.Stop();
            this.m_mediaSeeking.SetPositions(new DsOptInt64(0L), SeekingFlags.AbsolutePositioning, new DsOptInt64(0L), SeekingFlags.NoPositioning);
            this.currentState = VideoState.Stopped;
        }

        public void Update()
        {
            if (this.m_videoDataRgba.RefreshRead())
            {
                this.OnFrame(this.m_videoDataRgba.Read);
            }
            this.m_mediaSeeking.GetCurrentPosition(out this.currentPosition);
            if (this.currentPosition >= this.videoDuration)
            {
                this.currentState = VideoState.Stopped;
            }
        }

        public int VideoWidth =>
            this.videoWidth;

        public int VideoHeight =>
            this.videoHeight;

        public double CurrentPosition
        {
            get => 
                (((double) this.currentPosition) / 10000000.0);
            set
            {
                if (value < 0.0)
                {
                    value = 0.0;
                }
                if (value > this.Duration)
                {
                    value = this.Duration;
                }
                this.m_mediaPosition.put_CurrentPosition(value);
                this.currentPosition = ((long) value) * 0x989680L;
            }
        }

        public string CurrentPositionAsTimeString
        {
            get
            {
                double num = ((double) this.currentPosition) / 10000000.0;
                double num2 = num / 60.0;
                int num4 = (int) Math.Floor((double) (num2 / 60.0));
                int num5 = (int) Math.Floor((double) (num2 - (num4 * 60)));
                int num6 = (int) Math.Floor((double) (num - (num5 * 60)));
                string[] textArray1 = new string[5];
                string[] textArray2 = new string[5];
                textArray2[0] = (num4 < 10) ? ("0" + num4.ToString()) : num4.ToString();
                string[] local1 = textArray2;
                local1[1] = ":";
                local1[2] = (num5 < 10) ? ("0" + num5.ToString()) : num5.ToString();
                string[] local2 = local1;
                local2[3] = ":";
                local2[4] = (num6 < 10) ? ("0" + num6.ToString()) : num6.ToString();
                return string.Concat(local2);
            }
        }

        public double Duration =>
            (((double) this.videoDuration) / 10000000.0);

        public string DurationAsTimeString
        {
            get
            {
                double num = ((double) this.videoDuration) / 10000000.0;
                double num2 = num / 60.0;
                int num4 = (int) Math.Floor((double) (num2 / 60.0));
                int num5 = (int) Math.Floor((double) (num2 - (num4 * 60)));
                int num6 = (int) Math.Floor((double) (num - (num5 * 60)));
                string[] textArray1 = new string[5];
                string[] textArray2 = new string[5];
                textArray2[0] = (num4 < 10) ? ("0" + num4.ToString()) : num4.ToString();
                string[] local1 = textArray2;
                local1[1] = ":";
                local1[2] = (num5 < 10) ? ("0" + num5.ToString()) : num5.ToString();
                string[] local2 = local1;
                local2[3] = ":";
                local2[4] = (num6 < 10) ? ("0" + num6.ToString()) : num6.ToString();
                return string.Concat(local2);
            }
        }

        public VideoState CurrentState
        {
            get => 
                this.currentState;
            set
            {
                switch (value)
                {
                    case VideoState.Playing:
                        this.Play();
                        return;

                    case VideoState.Paused:
                        this.Pause();
                        return;

                    case VideoState.Stopped:
                        this.Stop();
                        return;
                }
            }
        }

        public bool IsDisposed =>
            this.isDisposed;

        public int FramesPerSecond
        {
            get
            {
                if (this.avgTimePerFrame == 0)
                {
                    return -1;
                }
                return (int) Math.Round((double) (1f / (((float) this.avgTimePerFrame) / 1E+07f)), 0, MidpointRounding.ToEven);
            }
        }

        public float MillisecondsPerFrame =>
            ((this.avgTimePerFrame != 0) ? (((float) this.avgTimePerFrame) / 10000f) : -1f);

        public byte AlphaTransparency
        {
            get => 
                this.alphaTransparency;
            set => 
                (this.alphaTransparency = value);
        }

        public float Volume
        {
            get
            {
                int num;
                this.m_basicAudio.get_Volume(out num);
                return ((((float) num) / 10000f) + 1f);
            }
            set => 
                this.m_basicAudio.put_Volume((int) ((value - 1f) * 10000f));
        }
    }
}

