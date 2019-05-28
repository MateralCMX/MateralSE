namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.ObjectBuilder;
    using VRage.Serialization;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 0x7d0, typeof(MyObjectBuilder_SessionComponentReplay), (Type) null)]
    public class MySessionComponentReplay : MySessionComponentBase
    {
        public static MySessionComponentReplay Static;
        private static Dictionary<long, Dictionary<int, PerFrameData>> m_recordedEntities = new Dictionary<long, Dictionary<int, PerFrameData>>();
        private ulong m_replayingStart = ulong.MaxValue;
        private ulong m_recordingStart = ulong.MaxValue;

        public MySessionComponentReplay()
        {
            Static = this;
        }

        public void DeleteRecordings()
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_DeleteRecordings_Confirm), MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_ReplayTool_DeleteRecordings), okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnDeleteRecordingsClicked), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SessionComponentReplay replay = new MyObjectBuilder_SessionComponentReplay();
            if (m_recordedEntities.Count > 0)
            {
                replay.EntityReplayData = new MySerializableList<PerEntityData>();
                foreach (KeyValuePair<long, Dictionary<int, PerFrameData>> pair in m_recordedEntities)
                {
                    PerEntityData item = new PerEntityData {
                        EntityId = pair.Key,
                        Data = new SerializableDictionary<int, PerFrameData>()
                    };
                    foreach (KeyValuePair<int, PerFrameData> pair2 in pair.Value)
                    {
                        item.Data[pair2.Key] = pair2.Value;
                    }
                    replay.EntityReplayData.Add(item);
                }
            }
            return replay;
        }

        public bool HasEntityReplayData(long entityId) => 
            m_recordedEntities.ContainsKey(entityId);

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyObjectBuilder_SessionComponentReplay replay = sessionComponent as MyObjectBuilder_SessionComponentReplay;
            if (replay.EntityReplayData != null)
            {
                foreach (PerEntityData data in replay.EntityReplayData)
                {
                    Dictionary<int, PerFrameData> dictionary = new Dictionary<int, PerFrameData>(data.Data.Dictionary);
                    if (!m_recordedEntities.ContainsKey(data.EntityId))
                    {
                        m_recordedEntities.Add(data.EntityId, dictionary);
                    }
                }
            }
        }

        public bool IsEntityBeingRecorded(long entityId) => 
            (this.IsRecording && ((MySession.Static.ControlledEntity != null) && ((MySession.Static.ControlledEntity.Entity != null) && (MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).EntityId == entityId))));

        public bool IsEntityBeingReplayed(long entityId) => 
            (this.IsReplaying && (m_recordedEntities.ContainsKey(entityId) && !this.IsEntityBeingRecorded(entityId)));

        public bool IsEntityBeingReplayed(long entityId, out PerFrameData perFrameData)
        {
            Dictionary<int, PerFrameData> dictionary;
            if ((this.IsReplaying && (this.IsEntityBeingReplayed(entityId) && m_recordedEntities.TryGetValue(entityId, out dictionary))) && dictionary.TryGetValue((int) (MySandboxGame.Static.SimulationFrameCounter - this.m_replayingStart), out perFrameData))
            {
                return true;
            }
            perFrameData = new PerFrameData();
            return false;
        }

        private void OnDeleteRecordingsClicked(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                m_recordedEntities.Clear();
            }
        }

        public void ProvideEntityRecordData(long entityId, PerFrameData data)
        {
            Dictionary<int, PerFrameData> dictionary;
            PerFrameData data2;
            if (!m_recordedEntities.TryGetValue(entityId, out dictionary))
            {
                dictionary = new Dictionary<int, PerFrameData>();
                m_recordedEntities[entityId] = dictionary;
            }
            int key = (int) (MySandboxGame.Static.SimulationFrameCounter - this.m_recordingStart);
            if (!dictionary.TryGetValue(key, out data2))
            {
                data2 = data;
            }
            else
            {
                if (data.MovementData != null)
                {
                    data2.MovementData = data.MovementData;
                }
                if (data.SwitchWeaponData != null)
                {
                    data2.SwitchWeaponData = data.SwitchWeaponData;
                }
                if (data.ShootData != null)
                {
                    data2.ShootData = data.ShootData;
                }
                if (data.AnimationData != null)
                {
                    data2.AnimationData = data.AnimationData;
                }
                if (data.ControlSwitchesData != null)
                {
                    data2.ControlSwitchesData = data.ControlSwitchesData;
                }
                if (data.UseData != null)
                {
                    data2.UseData = data.UseData;
                }
            }
            dictionary[key] = data2;
        }

        public void StartRecording()
        {
            m_recordedEntities.Remove(MySession.Static.ControlledEntity.Entity.GetTopMostParent(null).EntityId);
            this.m_recordingStart = MySandboxGame.Static.SimulationFrameCounter;
        }

        public void StartReplay()
        {
            this.m_replayingStart = MySandboxGame.Static.SimulationFrameCounter;
        }

        public void StopRecording()
        {
            this.m_recordingStart = ulong.MaxValue;
        }

        public void StopReplay()
        {
            this.m_replayingStart = ulong.MaxValue;
        }

        public bool HasRecordedData =>
            (m_recordedEntities.Count > 0);

        public bool IsRecording =>
            (this.m_recordingStart != ulong.MaxValue);

        public bool IsReplaying =>
            (this.m_replayingStart != ulong.MaxValue);

        public bool HasAnyData =>
            (m_recordedEntities.Count > 0);

        public delegate void ActionRef<T>(ref T item);
    }
}

