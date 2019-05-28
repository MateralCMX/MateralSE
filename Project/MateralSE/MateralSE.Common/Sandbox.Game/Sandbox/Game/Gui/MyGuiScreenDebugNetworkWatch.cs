namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Replication;
    using Sandbox.Game.Replication.History;
    using Sandbox.Game.Replication.StateGroups;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRageMath;

    [MyDebugScreen("VRage", "Network Watch")]
    internal class MyGuiScreenDebugNetworkWatch : MyGuiScreenDebugBase
    {
        private MyEntity m_currentEntity;
        private MyGuiControlSlider m_up;
        private MyGuiControlSlider m_right;
        private MyGuiControlSlider m_forward;
        private MyGuiControlButton m_kickButton;
        private MyGuiControlLabel m_debugEntityLabel;
        private MyGuiControlLabel m_watchLabel;
        private bool m_debugEntityLocked;
        private const float FORCED_PRIORITY = 1f;
        private readonly MyPredictedSnapshotSyncSetup m_kickSetup;
        private bool m_debugEntityMyself;

        public MyGuiScreenDebugNetworkWatch() : base(nullable, false)
        {
            MyPredictedSnapshotSyncSetup setup1 = new MyPredictedSnapshotSyncSetup();
            setup1.AllowForceStop = false;
            setup1.ApplyPhysicsAngular = false;
            setup1.ApplyPhysicsLinear = false;
            setup1.ApplyRotation = false;
            setup1.ApplyPosition = true;
            setup1.ExtrapolationSmoothing = true;
            this.m_kickSetup = setup1;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugNetworkWatch";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 1f;
            Vector2? captionOffset = null;
            base.AddCaption("Network Watch", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            this.m_debugEntityLabel = base.AddLabel("", (Vector4) Color.Yellow, 1f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            base.AddCheckBox("Sync VDB camera", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyPhysics.SyncVDBCamera)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Debug Myself", false, (Action<MyGuiControlCheckbox>) (x => (this.m_debugEntityMyself = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Lock Debug Entity", false, (Action<MyGuiControlCheckbox>) (x => (this.m_debugEntityLocked = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Skip corrections for Debug Entity", MyPredictedSnapshotSync.SKIP_CORRECTIONS_FOR_DEBUG_ENTITY, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.SKIP_CORRECTIONS_FOR_DEBUG_ENTITY = x.IsChecked)), true, null, color, captionOffset);
            base.AddLabel("Cendos Desync Simulator (tm)", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.m_up = base.AddSlider("Up", (float) 0f, (float) -50f, (float) 50f, color);
            color = null;
            this.m_right = base.AddSlider("Right", (float) 0f, (float) -50f, (float) 50f, color);
            color = null;
            this.m_forward = base.AddSlider("Forward", (float) 0f, (float) -50f, (float) 50f, color);
            color = null;
            captionOffset = null;
            this.m_kickButton = base.AddButton("Kick", delegate (MyGuiControlButton x) {
                MatrixD worldMatrix = this.m_currentEntity.WorldMatrix;
                MySnapshot snapshot = new MySnapshot(this.m_currentEntity, false, true);
                Vector3D* vectordPtr1 = (Vector3D*) ref snapshot.Position;
                vectordPtr1[0] += Vector3.TransformNormal(new Vector3(this.m_up.Value, this.m_right.Value, this.m_forward.Value), worldMatrix);
                MySnapshotCache.Add(this.m_currentEntity, ref snapshot, this.m_kickSetup, true);
                MySnapshotCache.Apply();
            }, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.m_kickButton = base.AddButton("Log hierarchy", delegate (MyGuiControlButton x) {
                MyCubeGrid currentEntity = this.m_currentEntity as MyCubeGrid;
                if (currentEntity != null)
                {
                    currentEntity.LogHierarchy();
                }
            }, null, color, captionOffset);
            this.m_watchLabel = base.AddLabel("", (Vector4) Color.Yellow, 1f, null, "Debug");
        }

        public override bool Update(bool hasFocus)
        {
            bool flag = base.Update(hasFocus);
            if (((MySession.Static != null) && (this.m_kickButton != null)) && (this.m_debugEntityLabel != null))
            {
                MyEntity objA = null;
                if (!this.m_debugEntityLocked)
                {
                    LineD line = new LineD(MyBlockBuilderBase.IntersectionStart, MyBlockBuilderBase.IntersectionStart + (MyBlockBuilderBase.IntersectionDirection * 500.0));
                    MyIntersectionResultLineTriangleEx? nullable = MyEntities.GetIntersectionWithLine(ref line, MySession.Static.LocalCharacter, null, true, false, false, IntersectionFlags.ALL_TRIANGLES, 0f, false);
                    if (nullable != null)
                    {
                        objA = nullable.Value.Entity as MyEntity;
                    }
                }
                if (this.m_debugEntityMyself)
                {
                    objA = MySession.Static.TopMostControlledEntity;
                }
                if (!ReferenceEquals(objA, this.m_currentEntity))
                {
                    this.m_currentEntity = objA;
                    this.m_kickButton.Enabled = this.m_currentEntity != null;
                    this.m_debugEntityLabel.Text = (this.m_currentEntity != null) ? this.m_currentEntity.DisplayName : "";
                    MySnapshotCache.DEBUG_ENTITY_ID = (this.m_currentEntity != null) ? this.m_currentEntity.EntityId : 0L;
                    MyFakes.VDB_ENTITY = this.m_currentEntity;
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(MyMultiplayerBase.OnSetDebugEntity), (this.m_currentEntity == null) ? 0L : this.m_currentEntity.EntityId, targetEndpoint, position);
                }
            }
            if (this.m_currentEntity != null)
            {
                int supported;
                float single1;
                float single2;
                MyCharacter currentEntity = this.m_currentEntity as MyCharacter;
                MyCubeGrid grid = this.m_currentEntity as MyCubeGrid;
                MyExternalReplicable replicable = MyExternalReplicable.FindByObject(this.m_currentEntity);
                if (replicable != null)
                {
                    IMyStateGroup physicsSync = replicable.PhysicsSync;
                }
                MyCharacterPhysicsStateGroup group = (replicable != null) ? (replicable.PhysicsSync as MyCharacterPhysicsStateGroup) : null;
                MyEntity entity = null;
                if (currentEntity != null)
                {
                    MyEntities.TryGetEntityById(currentEntity.ClosestParentId, out entity, false);
                }
                else if (grid != null)
                {
                    MyEntities.TryGetEntityById(grid.ClosestParentId, out entity, false);
                }
                object[] index = new object[7];
                object[] objArray2 = new object[7];
                objArray2[0] = (group != null) ? group.AverageCorrection : 0.0;
                object[] objArray3 = index;
                if ((currentEntity == null) || (currentEntity.Physics.CharacterProxy == null))
                {
                    supported = 0;
                }
                else
                {
                    supported = (int) currentEntity.Physics.CharacterProxy.Supported;
                }
                index[index] = (bool) supported;
                int local7 = 1;
                local7[2] = (entity != null) ? ((int) entity.DebugName) : ((int) "-");
                int local4 = local7;
                int local5 = local7;
                local5[3] = (grid != null) ? grid.Physics.PredictedContactsCounter : 0;
                int local2 = local5;
                int local3 = local5;
                local3[4] = (grid != null) ? ((int) grid.IsClientPredicted) : ((currentEntity == null) ? 0 : ((int) currentEntity.IsClientPredicted));
                int num2 = local2;
                if (this.m_currentEntity.Physics == null)
                {
                    single1 = 0f;
                }
                else
                {
                    single1 = this.m_currentEntity.Physics.LinearVelocity.Length();
                }
                local2[local2] = (int) single1;
                if (this.m_currentEntity.Physics == null)
                {
                    single2 = 0f;
                }
                else
                {
                    single2 = this.m_currentEntity.Physics.LinearVelocityLocal.Length();
                }
                5[6] = (int) single2;
                this.m_watchLabel.Text = string.Format("Predicted: {4}\nCorrection: {0}\nSupport: {1}\nParentId: {2}\nPredictedContactsCounter: {3}\nLinearVelocity: {5}\nLinearVelocityLocal: {6}\n", 5);
            }
            return flag;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugNetworkWatch.<>c <>9 = new MyGuiScreenDebugNetworkWatch.<>c();
            public static Action<MyGuiControlCheckbox> <>9__12_3;
            public static Func<IMyEventOwner, Action<long>> <>9__13_0;

            internal void <RecreateControls>b__12_3(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.SKIP_CORRECTIONS_FOR_DEBUG_ENTITY = x.IsChecked;
            }

            internal Action<long> <Update>b__13_0(IMyEventOwner x) => 
                new Action<long>(MyMultiplayerBase.OnSetDebugEntity);
        }
    }
}

