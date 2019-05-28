namespace VRage.GameServices
{
    using System;

    public enum MyVoiceResult
    {
        OK,
        NotInitialized,
        NotRecording,
        NoData,
        BufferTooSmall,
        DataCorrupted,
        Restricted,
        UnsupportedCodec
    }
}

