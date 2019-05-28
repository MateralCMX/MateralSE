namespace Sandbox.Engine.Multiplayer
{
    using System;

    public enum MyMessageId : byte
    {
        FLUSH = 2,
        RPC = 3,
        REPLICATION_CREATE = 4,
        REPLICATION_DESTROY = 5,
        SERVER_DATA = 6,
        SERVER_STATE_SYNC = 7,
        CLIENT_READY = 8,
        CLIENT_ACKS = 0x11,
        CLIENT_UPDATE = 9,
        REPLICATION_READY = 10,
        REPLICATION_STREAM_BEGIN = 11,
        REPLICATION_ISLAND_DONE = 0x12,
        REPLICATION_REQUEST = 0x13,
        JOIN_RESULT = 12,
        WORLD_DATA = 13,
        CLIENT_CONNNECTED = 14,
        WORLD = 20,
        PLAYER_DATA = 0x15
    }
}

