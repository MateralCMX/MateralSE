namespace Sandbox.Game.VoiceChat
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game.Components;
    using VRage.GameServices;
    using VRage.Library;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation), StaticEventOwner]
    public class MyVoiceChatSessionComponent : MySessionComponentBase
    {
        private bool m_recording;
        private byte[] m_compressedVoiceBuffer;
        private byte[] m_uncompressedVoiceBuffer;
        private Dictionary<ulong, MyEntity3DSoundEmitter> m_voices;
        private Dictionary<ulong, ReceivedData> m_receivedVoiceData;
        private int m_frameCount;
        private List<ulong> m_keys;
        private IMyVoiceChatLogic m_voiceChatLogic;
        private bool m_enabled;
        private const uint COMPRESSED_SIZE = 0x2000;
        private const uint UNCOMPRESSED_SIZE = 0x5800;
        private Dictionary<ulong, bool> m_debugSentVoice = new Dictionary<ulong, bool>();
        private Dictionary<ulong, MyTuple<int, TimeSpan>> m_debugReceivedVoice = new Dictionary<ulong, MyTuple<int, TimeSpan>>();
        private int lastMessageTime;
        private static SendBuffer Recievebuffer;

        static MyVoiceChatSessionComponent()
        {
            SendBuffer buffer1 = new SendBuffer();
            buffer1.CompressedVoiceBuffer = new byte[0x2000];
            Recievebuffer = buffer1;
        }

        public void ClearDebugData()
        {
            this.m_debugSentVoice.Clear();
        }

        private unsafe void DebugDraw()
        {
            Vector2 screenCoord = new Vector2(300f, 100f);
            MyRenderProxy.DebugDrawText2D(screenCoord, "Sent voice to:", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr1 = (float*) ref screenCoord.Y;
            singlePtr1[0] += 30f;
            foreach (KeyValuePair<ulong, bool> pair in this.m_debugSentVoice)
            {
                MyRenderProxy.DebugDrawText2D(screenCoord, $"id: {pair.Key} => {pair.Value ? "SENT" : "NOT"}", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr2 = (float*) ref screenCoord.Y;
                singlePtr2[0] += 30f;
            }
            MyRenderProxy.DebugDrawText2D(screenCoord, "Received voice from:", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr3 = (float*) ref screenCoord.Y;
            singlePtr3[0] += 30f;
            foreach (KeyValuePair<ulong, MyTuple<int, TimeSpan>> pair2 in this.m_debugReceivedVoice)
            {
                MyTuple<int, TimeSpan> tuple = pair2.Value;
                string text = $"id: {pair2.Key} => size: {pair2.Value.Item1} (timestamp {tuple.Item2.ToString()})";
                MyRenderProxy.DebugDrawText2D(screenCoord, text, Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr4 = (float*) ref screenCoord.Y;
                singlePtr4[0] += 30f;
            }
            MyRenderProxy.DebugDrawText2D(screenCoord, "Uncompressed buffers:", Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
            float* singlePtr5 = (float*) ref screenCoord.Y;
            singlePtr5[0] += 30f;
            foreach (KeyValuePair<ulong, ReceivedData> pair3 in this.m_receivedVoiceData)
            {
                string text = $"id: {pair3.Key} => size: {pair3.Value.UncompressedBuffer.Count}";
                MyRenderProxy.DebugDrawText2D(screenCoord, text, Color.White, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr6 = (float*) ref screenCoord.Y;
                singlePtr6[0] += 30f;
            }
        }

        public override unsafe void Draw()
        {
            base.Draw();
            if (this.m_receivedVoiceData != null)
            {
                if (MyDebugDrawSettings.DEBUG_DRAW_VOICE_CHAT && MyFakes.ENABLE_VOICE_CHAT_DEBUGGING)
                {
                    this.DebugDraw();
                }
                foreach (KeyValuePair<ulong, ReceivedData> pair in this.m_receivedVoiceData)
                {
                    if (!(pair.Value.SpeakerTimestamp != MyTimeSpan.Zero))
                    {
                        continue;
                    }
                    MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(pair.Key, 0));
                    if ((playerById != null) && (playerById.Character != null))
                    {
                        Color white = Color.White;
                        MyGuiDrawAlignEnum drawAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
                        MyGuiPaddedTexture texture = MyGuiConstants.TEXTURE_VOICE_CHAT;
                        Vector3D vectord = Vector3D.Transform((playerById.Character.PositionComp.GetPosition() + (playerById.Character.PositionComp.LocalAABB.Height * playerById.Character.PositionComp.WorldMatrix.Up)) + (playerById.Character.PositionComp.WorldMatrix.Up * 0.20000000298023224), MySector.MainCamera.ViewMatrix * MySector.MainCamera.ProjectionMatrix);
                        if (vectord.Z < 1.0)
                        {
                            Vector2 hudPos = new Vector2((float) vectord.X, (float) vectord.Y);
                            hudPos = (hudPos * 0.5f) + (0.5f * Vector2.One);
                            Vector2* vectorPtr1 = (Vector2*) ref hudPos;
                            vectorPtr1->Y = 1f - hudPos.Y;
                            Vector2 normalizedCoord = MyGuiScreenHudBase.ConvertHudToNormalizedGuiPosition(ref hudPos);
                            MyGuiManager.DrawSpriteBatch(texture.Texture, normalizedCoord, texture.SizeGui * 0.5f, white, drawAlign, false, true);
                        }
                    }
                }
            }
        }

        private bool IsCharacterValid(MyCharacter character) => 
            ((character != null) && (!character.IsDead && !character.MarkedForClose));

        public override void LoadData()
        {
            base.LoadData();
            Static = this;
            MyGameService.InitializeVoiceRecording();
            this.m_voiceChatLogic = Activator.CreateInstance(MyPerGameSettings.VoiceChatLogic) as IMyVoiceChatLogic;
            this.m_recording = false;
            this.m_compressedVoiceBuffer = new byte[0x2000];
            this.m_uncompressedVoiceBuffer = new byte[0x5800];
            this.m_voices = new Dictionary<ulong, MyEntity3DSoundEmitter>();
            this.m_receivedVoiceData = new Dictionary<ulong, ReceivedData>();
            this.m_keys = new List<ulong>();
            Sync.Players.PlayerRemoved += new Action<MyPlayer.PlayerId>(this.Players_PlayerRemoved);
            this.m_enabled = MyAudio.Static.EnableVoiceChat;
            MyAudio.Static.VoiceChatEnabled += new Action<bool>(this.Static_VoiceChatEnabled);
            MyHud.VoiceChat.VisibilityChanged += new Action<bool>(this.VoiceChat_VisibilityChanged);
        }

        [Event(null, 0x1c9), Reliable, Broadcast]
        public static void MutePlayer_Implementation(ulong playerSettingMute, bool mute)
        {
            HashSet<ulong> dontSendVoicePlayers = MySandboxGame.Config.DontSendVoicePlayers;
            if (mute)
            {
                dontSendVoicePlayers.Add(playerSettingMute);
            }
            else
            {
                dontSendVoicePlayers.Remove(playerSettingMute);
            }
            MySandboxGame.Config.DontSendVoicePlayers = dontSendVoicePlayers;
            MySandboxGame.Config.Save();
        }

        public static void MutePlayerRequest(ulong mutedPlayerId, bool mute)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong, bool>(x => new Action<ulong, bool>(MyVoiceChatSessionComponent.MutePlayerRequest_Implementation), mutedPlayerId, mute, targetEndpoint, position);
        }

        [Event(null, 450), Reliable, Server]
        private static void MutePlayerRequest_Implementation(ulong mutedPlayerId, bool mute)
        {
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ulong, bool>(x => new Action<ulong, bool>(MyVoiceChatSessionComponent.MutePlayer_Implementation), MyEventContext.Current.Sender.Value, mute, new EndpointId(mutedPlayerId), position);
        }

        private void Players_PlayerRemoved(MyPlayer.PlayerId pid)
        {
            if (pid.SerialId == 0)
            {
                ulong steamId = pid.SteamId;
                if (this.m_receivedVoiceData.ContainsKey(steamId))
                {
                    this.m_receivedVoiceData.Remove(steamId);
                }
                if (this.m_voices.ContainsKey(steamId))
                {
                    this.m_voices[steamId].StopSound(true, true);
                    this.m_voices[steamId].Cleanup();
                    this.m_voices[steamId] = null;
                    this.m_voices.Remove(steamId);
                }
            }
        }

        private void PlayVoice(byte[] uncompressedBuffer, int uncompressedSize, ulong playerId, MySoundDimensions dimension, float maxDistance)
        {
            if (!this.m_voices.ContainsKey(playerId))
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(playerId));
                this.m_voices[playerId] = new MyEntity3DSoundEmitter(playerById.Character, false, 1f);
            }
            this.m_voices[playerId].PlaySound(uncompressedBuffer, uncompressedSize, MyGameService.GetVoiceSampleRate(), MyAudio.Static.VolumeVoiceChat, maxDistance, dimension);
        }

        private void ProcessBuffer(byte[] compressedBuffer, int bufferSize, ulong sender)
        {
            uint num;
            if (MyGameService.DecompressVoice(compressedBuffer, (uint) bufferSize, this.m_uncompressedVoiceBuffer, out num) == MyVoiceResult.OK)
            {
                ReceivedData data;
                if (!this.m_receivedVoiceData.TryGetValue(sender, out data))
                {
                    data = new ReceivedData {
                        UncompressedBuffer = new List<byte>(),
                        Timestamp = MyTimeSpan.Zero
                    };
                }
                if (data.Timestamp == MyTimeSpan.Zero)
                {
                    data.Timestamp = MySandboxGame.Static.TotalTime;
                }
                data.SpeakerTimestamp = MySandboxGame.Static.TotalTime;
                data.UncompressedBuffer.AddArray<byte>(this.m_uncompressedVoiceBuffer, (int) num);
                this.m_receivedVoiceData[sender] = data;
            }
        }

        [Event(null, 0x1d8), Server]
        private static void SendVoice(ulong user, BitReaderWriter data)
        {
            data.ReadData(Recievebuffer, false, true);
            if (user == Sync.MyId)
            {
                Static.VoiceMessageReceived((ulong) Recievebuffer.SenderUserId);
            }
            else
            {
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<ulong, BitReaderWriter>(x => new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoicePlayer), user, (BitReaderWriter) Recievebuffer, new EndpointId(user), position);
            }
        }

        [Event(null, 0x1e6), Client]
        private static void SendVoicePlayer(ulong user, BitReaderWriter data)
        {
            data.ReadData(Recievebuffer, false, true);
            Static.VoiceMessageReceived((ulong) Recievebuffer.SenderUserId);
        }

        public void StartRecording()
        {
            if (this.m_enabled)
            {
                this.m_recording = true;
                MyGameService.StartVoiceRecording();
                MyHud.VoiceChat.Show();
            }
        }

        private void Static_VoiceChatEnabled(bool isEnabled)
        {
            this.m_enabled = isEnabled;
            if (!this.m_enabled)
            {
                if (this.m_recording)
                {
                    this.m_recording = false;
                    this.StopRecording();
                }
                foreach (KeyValuePair<ulong, MyEntity3DSoundEmitter> pair in this.m_voices)
                {
                    pair.Value.StopSound(true, true);
                    pair.Value.Cleanup();
                }
                this.m_voices.Clear();
                this.m_receivedVoiceData.Clear();
            }
        }

        public void StopRecording()
        {
            if (this.m_enabled)
            {
                MyGameService.StopVoiceRecording();
                MyHud.VoiceChat.Hide();
            }
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            if (this.m_recording)
            {
                this.StopRecording();
            }
            foreach (KeyValuePair<ulong, MyEntity3DSoundEmitter> pair in this.m_voices)
            {
                this.m_voices[pair.Key].StopSound(true, true);
                this.m_voices[pair.Key].Cleanup();
            }
            this.m_compressedVoiceBuffer = null;
            this.m_uncompressedVoiceBuffer = null;
            this.m_voiceChatLogic = null;
            MyGameService.DisposeVoiceRecording();
            Static = null;
            this.m_receivedVoiceData = null;
            this.m_voices = null;
            this.m_keys = null;
            Sync.Players.PlayerRemoved -= new Action<MyPlayer.PlayerId>(this.Players_PlayerRemoved);
            MyAudio.Static.VoiceChatEnabled -= new Action<bool>(this.Static_VoiceChatEnabled);
            MyHud.VoiceChat.VisibilityChanged -= new Action<bool>(this.VoiceChat_VisibilityChanged);
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_enabled && this.IsCharacterValid(MySession.Static.LocalCharacter))
            {
                if (this.m_recording)
                {
                    this.UpdateRecording();
                }
                this.UpdatePlayback();
            }
        }

        private void UpdatePlayback()
        {
            if (this.m_voiceChatLogic != null)
            {
                MyTimeSpan totalTime = MySandboxGame.Static.TotalTime;
                float num = 1000f;
                this.m_keys.AddRange(this.m_receivedVoiceData.Keys);
                foreach (ulong num2 in this.m_keys)
                {
                    float num4;
                    MySoundDimensions dimensions;
                    bool flag = false;
                    ulong steamId = num2;
                    ReceivedData data = this.m_receivedVoiceData[num2];
                    MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(steamId));
                    if (((playerById != null) && (data.Timestamp != MyTimeSpan.Zero)) && this.m_voiceChatLogic.ShouldPlayVoice(playerById, data.Timestamp, out dimensions, out num4))
                    {
                        if (!MySandboxGame.Config.MutedPlayers.Contains(playerById.Id.SteamId))
                        {
                            this.PlayVoice(data.UncompressedBuffer.ToArray(), data.UncompressedBuffer.Count, steamId, dimensions, num4);
                            data.ClearData();
                            flag = true;
                        }
                        else if ((this.lastMessageTime == 0) || (VRage.Library.MyEnvironment.TickCount > (this.lastMessageTime + 0x1388)))
                        {
                            MutePlayerRequest(playerById.Id.SteamId, true);
                            this.lastMessageTime = VRage.Library.MyEnvironment.TickCount;
                        }
                    }
                    if ((data.SpeakerTimestamp != MyTimeSpan.Zero) && ((totalTime - data.SpeakerTimestamp).Milliseconds > num))
                    {
                        data.ClearSpeakerTimestamp();
                        flag = true;
                    }
                    if (flag)
                    {
                        this.m_receivedVoiceData[num2] = data;
                    }
                }
                this.m_keys.Clear();
            }
        }

        private void UpdateRecording()
        {
            uint size = 0;
            MyVoiceResult availableVoice = MyGameService.GetAvailableVoice(out size);
            if (availableVoice == MyVoiceResult.OK)
            {
                availableVoice = MyGameService.GetVoice(this.m_compressedVoiceBuffer, out size);
                if (MyFakes.ENABLE_VOICE_CHAT_DEBUGGING)
                {
                    this.ProcessBuffer(this.m_compressedVoiceBuffer, (int) size, Sync.MyId);
                }
                foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                {
                    if (((player.Id.SerialId != 0) || ((player.Id.SteamId == MySession.Static.LocalHumanPlayer.Id.SteamId) || (!this.IsCharacterValid(player.Character) || !this.m_voiceChatLogic.ShouldSendVoice(player)))) || MySandboxGame.Config.DontSendVoicePlayers.Contains(player.Id.SteamId))
                    {
                        if (MyFakes.ENABLE_VOICE_CHAT_DEBUGGING)
                        {
                            this.m_debugSentVoice[player.Id.SteamId] = false;
                        }
                        continue;
                    }
                    int num2 = 0;
                    while (true)
                    {
                        Vector3D? nullable;
                        if (num2 >= (((size / 1) / 0x400) + 1))
                        {
                            if (MyFakes.ENABLE_VOICE_CHAT_DEBUGGING)
                            {
                                this.m_debugSentVoice[player.Id.SteamId] = true;
                            }
                            break;
                        }
                        SendBuffer buffer1 = new SendBuffer();
                        buffer1.CompressedVoiceBuffer = this.m_compressedVoiceBuffer;
                        buffer1.StartingKilobyte = (byte) num2;
                        buffer1.NumElements = (num2 > 0) ? ((int) (((ulong) (size / 1)) % ((long) (num2 * 0x400)))) : ((int) (size / 1));
                        SendBuffer local1 = buffer1;
                        local1.SenderUserId = (long) MySession.Static.LocalHumanPlayer.Id.SteamId;
                        SendBuffer buffer = local1;
                        if (Sync.IsServer)
                        {
                            nullable = null;
                            MyMultiplayer.RaiseStaticEvent<ulong, BitReaderWriter>(x => new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoicePlayer), player.Id.SteamId, (BitReaderWriter) buffer, new EndpointId(player.Id.SteamId), nullable);
                        }
                        else
                        {
                            EndpointId targetEndpoint = new EndpointId();
                            nullable = null;
                            MyMultiplayer.RaiseStaticEvent<ulong, BitReaderWriter>(x => new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoice), player.Id.SteamId, (BitReaderWriter) buffer, targetEndpoint, nullable);
                        }
                        num2++;
                    }
                }
            }
            else if (availableVoice == MyVoiceResult.NotRecording)
            {
                this.m_recording = false;
                if (MyFakes.ENABLE_VOICE_CHAT_DEBUGGING)
                {
                    ulong myId = Sync.MyId;
                    if (!this.m_voices.ContainsKey(myId))
                    {
                        this.m_voices[myId] = new MyEntity3DSoundEmitter(Sync.Players.GetPlayerById(new MyPlayer.PlayerId(myId)).Character, false, 1f);
                    }
                    MyEntity3DSoundEmitter emitter = this.m_voices[myId];
                    if (this.m_receivedVoiceData.ContainsKey(myId))
                    {
                        ReceivedData data = this.m_receivedVoiceData[myId];
                        emitter.PlaySound(data.UncompressedBuffer.ToArray(), data.UncompressedBuffer.Count, MyGameService.GetVoiceSampleRate(), 1f, 0f, MySoundDimensions.D3);
                        data.ClearData();
                        data.ClearSpeakerTimestamp();
                        this.m_receivedVoiceData[myId] = data;
                    }
                }
            }
        }

        private void VoiceChat_VisibilityChanged(bool isVisible)
        {
            if (this.m_recording != isVisible)
            {
                if (this.m_recording)
                {
                    this.m_recording = false;
                    this.StopRecording();
                }
                else
                {
                    this.StartRecording();
                }
            }
        }

        private void VoiceMessageReceived(ulong sender)
        {
            if (this.m_enabled && this.IsCharacterValid(MySession.Static.LocalCharacter))
            {
                this.ProcessBuffer(Recievebuffer.CompressedVoiceBuffer, Recievebuffer.NumElements / 1, sender);
            }
        }

        public static MyVoiceChatSessionComponent Static
        {
            [CompilerGenerated]
            get => 
                <Static>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<Static>k__BackingField = value);
        }

        public bool IsRecording =>
            this.m_recording;

        public override bool IsRequiredByGame =>
            (MyGameService.IsActive && MyPerGameSettings.VoiceChatEnabled);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVoiceChatSessionComponent.<>c <>9 = new MyVoiceChatSessionComponent.<>c();
            public static Func<IMyEventOwner, Action<ulong, BitReaderWriter>> <>9__40_0;
            public static Func<IMyEventOwner, Action<ulong, BitReaderWriter>> <>9__40_1;
            public static Func<IMyEventOwner, Action<ulong, bool>> <>9__41_0;
            public static Func<IMyEventOwner, Action<ulong, bool>> <>9__42_0;
            public static Func<IMyEventOwner, Action<ulong, BitReaderWriter>> <>9__44_0;

            internal Action<ulong, bool> <MutePlayerRequest_Implementation>b__42_0(IMyEventOwner x) => 
                new Action<ulong, bool>(MyVoiceChatSessionComponent.MutePlayer_Implementation);

            internal Action<ulong, bool> <MutePlayerRequest>b__41_0(IMyEventOwner x) => 
                new Action<ulong, bool>(MyVoiceChatSessionComponent.MutePlayerRequest_Implementation);

            internal Action<ulong, BitReaderWriter> <SendVoice>b__44_0(IMyEventOwner x) => 
                new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoicePlayer);

            internal Action<ulong, BitReaderWriter> <UpdateRecording>b__40_0(IMyEventOwner x) => 
                new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoicePlayer);

            internal Action<ulong, BitReaderWriter> <UpdateRecording>b__40_1(IMyEventOwner x) => 
                new Action<ulong, BitReaderWriter>(MyVoiceChatSessionComponent.SendVoice);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReceivedData
        {
            public List<byte> UncompressedBuffer;
            public MyTimeSpan Timestamp;
            public MyTimeSpan SpeakerTimestamp;
            public void ClearData()
            {
                this.UncompressedBuffer.Clear();
                this.Timestamp = MyTimeSpan.Zero;
            }

            public void ClearSpeakerTimestamp()
            {
                this.SpeakerTimestamp = MyTimeSpan.Zero;
            }
        }

        private class SendBuffer : IBitSerializable
        {
            public byte[] CompressedVoiceBuffer;
            public byte StartingKilobyte;
            public int NumElements;
            public long SenderUserId;

            public static implicit operator BitReaderWriter(MyVoiceChatSessionComponent.SendBuffer buffer) => 
                new BitReaderWriter(buffer);

            public bool Serialize(BitStream stream, bool validate, bool acceptAndSetValue = true)
            {
                if (stream.Reading)
                {
                    this.SenderUserId = stream.ReadInt64(0x40);
                    this.NumElements = stream.ReadInt32(0x20);
                    stream.ReadBytes(this.CompressedVoiceBuffer, 0, this.NumElements);
                }
                else
                {
                    stream.WriteInt64(this.SenderUserId, 0x40);
                    stream.WriteInt32(this.NumElements, 0x20);
                    stream.WriteBytes(this.CompressedVoiceBuffer, this.StartingKilobyte * 0x400, this.NumElements);
                }
                return true;
            }
        }
    }
}

