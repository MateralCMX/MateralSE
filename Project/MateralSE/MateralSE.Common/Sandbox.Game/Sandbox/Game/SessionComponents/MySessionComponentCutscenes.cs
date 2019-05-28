namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Audio;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 900, typeof(MyObjectBuilder_CutsceneSessionComponent), (Type) null)]
    public class MySessionComponentCutscenes : MySessionComponentBase
    {
        private MyObjectBuilder_CutsceneSessionComponent m_objectBuilder;
        private Dictionary<string, Cutscene> m_cutsceneLibrary = new Dictionary<string, Cutscene>();
        private Cutscene m_currentCutscene;
        private CutsceneSequenceNode m_currentNode;
        private float m_currentTime;
        private float m_currentFOV = 70f;
        private int m_currentNodeIndex;
        private bool m_nodeActivated;
        private float MINIMUM_FOV = 10f;
        private float MAXIMUM_FOV = 300f;
        private float m_eventDelay = float.MaxValue;
        private bool m_releaseCamera;
        private bool m_overlayEnabled;
        private bool m_registerEvents = true;
        private string m_cameraOverlay = "";
        private string m_cameraOverlayOriginal = "";
        private MatrixD m_nodeStartMatrix;
        private float m_nodeStartFOV = 70f;
        private MatrixD m_nodeEndMatrix;
        private MatrixD m_currentCameraMatrix;
        private MyEntity m_lookTarget;
        private MyEntity m_rotateTarget;
        private MyEntity m_moveTarget;
        private MyEntity m_attachedPositionTo;
        private Vector3D m_attachedPositionOffset = Vector3D.Zero;
        private MyEntity m_attachedRotationTo;
        private MatrixD m_attachedRotationOffset;
        private Vector3D m_lastUpVector = Vector3D.Up;
        private List<MatrixD> m_waypoints = new List<MatrixD>();
        private IMyCameraController m_originalCameraController;
        private MyCutsceneCamera m_cameraEntity = new MyCutsceneCamera();

        public override void BeforeStart()
        {
            if (this.m_objectBuilder != null)
            {
                using (List<string>.Enumerator enumerator = this.m_objectBuilder.VoxelPrecachingWaypointNames.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityByName(enumerator.Current, out entity))
                        {
                            continue;
                        }
                        MyRenderProxy.PointsForVoxelPrecache.Add(entity.PositionComp.GetPosition());
                    }
                }
            }
        }

        public void CutsceneEnd(bool releaseCamera = true)
        {
            MyHudWarnings.EnableWarnings = true;
            if (this.m_currentCutscene != null)
            {
                if (MyVisualScriptLogicProvider.CutsceneEnded != null)
                {
                    MyVisualScriptLogicProvider.CutsceneEnded(this.m_currentCutscene.Name);
                }
                this.m_currentCutscene = null;
                if (releaseCamera)
                {
                    this.m_cameraEntity.FOV = MathHelper.ToDegrees(MySandboxGame.Config.FieldOfView);
                    this.m_releaseCamera = true;
                }
                MyHudCameraOverlay.TextureName = this.m_cameraOverlayOriginal;
                MyHudCameraOverlay.Enabled = this.m_overlayEnabled;
            }
        }

        public void CutsceneNext(bool setToZero)
        {
            this.m_nodeActivated = false;
            this.m_currentNodeIndex++;
            this.m_currentTime -= setToZero ? this.m_currentTime : this.m_currentNode.Time;
        }

        public void CutsceneSkip()
        {
            if (this.m_currentCutscene != null)
            {
                if (this.m_currentCutscene.CanBeSkipped)
                {
                    if ((this.m_currentCutscene.FireEventsDuringSkip && (MyVisualScriptLogicProvider.CutsceneNodeEvent != null)) && this.m_registerEvents)
                    {
                        if (((this.m_currentNode != null) && (this.m_currentNode.EventDelay > 0f)) && (this.m_eventDelay != float.MaxValue))
                        {
                            MyVisualScriptLogicProvider.CutsceneNodeEvent(this.m_currentNode.Event);
                        }
                        for (int i = this.m_currentNodeIndex + 1; i < this.m_currentCutscene.SequenceNodes.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(this.m_currentCutscene.SequenceNodes[i].Event))
                            {
                                MyVisualScriptLogicProvider.CutsceneNodeEvent(this.m_currentCutscene.SequenceNodes[i].Event);
                            }
                        }
                    }
                    this.m_currentNodeIndex = this.m_currentCutscene.SequenceNodes.Count;
                    MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                }
                else
                {
                    MyGuiAudio.PlaySound(MyGuiSounds.HudUnable);
                }
            }
        }

        public void CutsceneUpdate()
        {
            if (!this.m_nodeActivated)
            {
                this.m_nodeActivated = true;
                this.m_nodeStartMatrix = this.m_currentCameraMatrix;
                this.m_nodeEndMatrix = this.m_currentCameraMatrix;
                this.m_nodeStartFOV = this.m_currentFOV;
                this.m_moveTarget = null;
                this.m_rotateTarget = null;
                this.m_waypoints.Clear();
                this.m_eventDelay = float.MaxValue;
                if ((this.m_registerEvents && ((this.m_currentNode.Event != null) && (this.m_currentNode.Event.Length > 0))) && (MyVisualScriptLogicProvider.CutsceneNodeEvent != null))
                {
                    if (this.m_currentNode.EventDelay <= 0f)
                    {
                        MyVisualScriptLogicProvider.CutsceneNodeEvent(this.m_currentNode.Event);
                    }
                    else
                    {
                        this.m_eventDelay = this.m_currentNode.EventDelay;
                    }
                }
                if (((this.m_currentNode.LookAt != null) && (this.m_currentNode.LookAt.Length > 0)) && (MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.LookAt) != null))
                {
                    this.m_nodeStartMatrix = MatrixD.CreateLookAtInverse(this.m_currentCameraMatrix.Translation, this.m_rotateTarget.PositionComp.GetPosition(), this.m_currentCameraMatrix.Up);
                    this.m_nodeEndMatrix = this.m_nodeStartMatrix;
                }
                if ((this.m_currentNode.SetRorationLike != null) && (this.m_currentNode.SetRorationLike.Length > 0))
                {
                    MyEntity entityByName = MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.SetRorationLike);
                    if (entityByName != null)
                    {
                        this.m_nodeStartMatrix = entityByName.WorldMatrix;
                        this.m_nodeEndMatrix = this.m_nodeStartMatrix;
                    }
                }
                if ((this.m_currentNode.RotateLike != null) && (this.m_currentNode.RotateLike.Length > 0))
                {
                    MyEntity entityByName = MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.RotateLike);
                    if (entityByName != null)
                    {
                        this.m_nodeEndMatrix = entityByName.WorldMatrix;
                    }
                }
                if ((this.m_currentNode.RotateTowards != null) && (this.m_currentNode.RotateTowards.Length > 0))
                {
                    this.m_rotateTarget = (this.m_currentNode.RotateTowards.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.RotateTowards) : null;
                }
                if (this.m_currentNode.LockRotationTo != null)
                {
                    this.m_lookTarget = (this.m_currentNode.LockRotationTo.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.LockRotationTo) : null;
                }
                this.m_nodeStartMatrix.Translation = this.m_currentCameraMatrix.Translation;
                this.m_nodeEndMatrix.Translation = this.m_currentCameraMatrix.Translation;
                if ((this.m_currentNode.SetPositionTo != null) && (this.m_currentNode.SetPositionTo.Length > 0))
                {
                    MyEntity entityByName = MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.SetPositionTo);
                    if (entityByName != null)
                    {
                        this.m_nodeStartMatrix.Translation = entityByName.WorldMatrix.Translation;
                        MatrixD worldMatrix = entityByName.WorldMatrix;
                        this.m_nodeEndMatrix.Translation = worldMatrix.Translation;
                    }
                }
                if (this.m_currentNode.AttachTo != null)
                {
                    if (this.m_currentNode.AttachTo != null)
                    {
                        this.m_attachedPositionTo = (this.m_currentNode.AttachTo.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.AttachTo) : null;
                        this.m_attachedPositionOffset = (this.m_attachedPositionTo != null) ? Vector3D.Transform(this.m_currentCameraMatrix.Translation, this.m_attachedPositionTo.PositionComp.WorldMatrixInvScaled) : Vector3D.Zero;
                        this.m_attachedRotationTo = this.m_attachedPositionTo;
                        this.m_attachedRotationOffset = this.m_currentCameraMatrix * this.m_attachedRotationTo.PositionComp.WorldMatrixInvScaled;
                        this.m_attachedRotationOffset.Translation = Vector3D.Zero;
                    }
                }
                else
                {
                    if (this.m_currentNode.AttachPositionTo != null)
                    {
                        this.m_attachedPositionTo = (this.m_currentNode.AttachPositionTo.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.AttachPositionTo) : null;
                        this.m_attachedPositionOffset = (this.m_attachedPositionTo != null) ? Vector3D.Transform(this.m_currentCameraMatrix.Translation, this.m_attachedPositionTo.PositionComp.WorldMatrixInvScaled) : Vector3D.Zero;
                    }
                    if (this.m_currentNode.AttachRotationTo != null)
                    {
                        this.m_attachedRotationTo = (this.m_currentNode.AttachRotationTo.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.AttachRotationTo) : null;
                        this.m_attachedRotationOffset = this.m_currentCameraMatrix * this.m_attachedRotationTo.PositionComp.WorldMatrixInvScaled;
                        this.m_attachedRotationOffset.Translation = Vector3D.Zero;
                    }
                }
                if ((this.m_currentNode.MoveTo != null) && (this.m_currentNode.MoveTo.Length > 0))
                {
                    this.m_moveTarget = (this.m_currentNode.MoveTo.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentNode.MoveTo) : null;
                }
                if ((this.m_currentNode.Waypoints != null) && (this.m_currentNode.Waypoints.Count > 0))
                {
                    bool flag = true;
                    foreach (CutsceneSequenceNodeWaypoint waypoint in this.m_currentNode.Waypoints)
                    {
                        if (waypoint.Name.Length <= 0)
                        {
                            continue;
                        }
                        MyEntity entityByName = MyVisualScriptLogicProvider.GetEntityByName(waypoint.Name);
                        if (entityByName != null)
                        {
                            this.m_waypoints.Add(entityByName.WorldMatrix);
                            if (flag)
                            {
                                this.m_lastUpVector = entityByName.WorldMatrix.Up;
                                flag = false;
                            }
                        }
                    }
                    if (this.m_waypoints.Count > 0)
                    {
                        if (this.m_waypoints.Count < 3)
                        {
                            this.m_nodeEndMatrix.Translation = this.m_waypoints[this.m_waypoints.Count - 1].Translation;
                            this.m_waypoints.Clear();
                        }
                        else if (this.m_waypoints.Count == 2)
                        {
                            this.m_nodeStartMatrix = this.m_waypoints[0];
                            this.m_nodeEndMatrix = this.m_waypoints[1];
                        }
                    }
                }
                this.m_currentCameraMatrix = this.m_nodeStartMatrix;
            }
            this.m_currentTime += 0.01666667f;
            float amount = (this.m_currentNode.Time > 0f) ? MathHelper.Clamp((float) (this.m_currentTime / this.m_currentNode.Time), (float) 0f, (float) 1f) : 1f;
            if (this.m_registerEvents && (this.m_currentTime >= this.m_eventDelay))
            {
                this.m_eventDelay = float.MaxValue;
                MyVisualScriptLogicProvider.CutsceneNodeEvent(this.m_currentNode.Event);
            }
            if (this.m_moveTarget != null)
            {
                this.m_nodeEndMatrix.Translation = this.m_moveTarget.PositionComp.GetPosition();
            }
            Vector3D translation = this.m_currentCameraMatrix.Translation;
            if (this.m_attachedPositionTo != null)
            {
                if (!this.m_attachedPositionTo.Closed)
                {
                    translation = Vector3D.Transform(this.m_attachedPositionOffset, this.m_attachedPositionTo.PositionComp.WorldMatrix);
                }
            }
            else if (this.m_waypoints.Count <= 2)
            {
                if (this.m_nodeStartMatrix.Translation != this.m_nodeEndMatrix.Translation)
                {
                    translation = new Vector3D(MathHelper.SmoothStep(this.m_nodeStartMatrix.Translation.X, this.m_nodeEndMatrix.Translation.X, (double) amount), MathHelper.SmoothStep(this.m_nodeStartMatrix.Translation.Y, this.m_nodeEndMatrix.Translation.Y, (double) amount), MathHelper.SmoothStep(this.m_nodeStartMatrix.Translation.Z, this.m_nodeEndMatrix.Translation.Z, (double) amount));
                }
            }
            else
            {
                double num2 = 1f / ((float) (this.m_waypoints.Count - 1));
                int num3 = (int) Math.Floor((double) (((double) amount) / num2));
                if (num3 > (this.m_waypoints.Count - 2))
                {
                    num3 = this.m_waypoints.Count - 2;
                }
                double t = (amount - (num3 * num2)) / num2;
                if (num3 == 0)
                {
                    translation = MathHelper.CalculateBezierPoint(t, this.m_waypoints[num3].Translation, this.m_waypoints[num3].Translation, this.m_waypoints[num3 + 1].Translation - ((this.m_waypoints[num3 + 2].Translation - this.m_waypoints[num3].Translation) / 4.0), this.m_waypoints[num3 + 1].Translation);
                }
                else if (num3 >= (this.m_waypoints.Count - 2))
                {
                    translation = MathHelper.CalculateBezierPoint(t, this.m_waypoints[num3].Translation, this.m_waypoints[num3].Translation + ((this.m_waypoints[num3 + 1].Translation - this.m_waypoints[num3 - 1].Translation) / 4.0), this.m_waypoints[num3 + 1].Translation, this.m_waypoints[num3 + 1].Translation);
                }
                else
                {
                    translation = MathHelper.CalculateBezierPoint(t, this.m_waypoints[num3].Translation, this.m_waypoints[num3].Translation + ((this.m_waypoints[num3 + 1].Translation - this.m_waypoints[num3 - 1].Translation) / 4.0), this.m_waypoints[num3 + 1].Translation - ((this.m_waypoints[num3 + 2].Translation - this.m_waypoints[num3].Translation) / 4.0), this.m_waypoints[num3 + 1].Translation);
                }
            }
            if (this.m_rotateTarget != null)
            {
                this.m_nodeEndMatrix = MatrixD.CreateLookAtInverse(this.m_currentCameraMatrix.Translation, this.m_rotateTarget.PositionComp.GetPosition(), this.m_nodeStartMatrix.Up);
            }
            if (this.m_lookTarget != null)
            {
                if (!this.m_lookTarget.Closed)
                {
                    this.m_currentCameraMatrix = MatrixD.CreateLookAtInverse(translation, this.m_lookTarget.PositionComp.GetPosition(), (this.m_waypoints.Count > 2) ? this.m_lastUpVector : this.m_currentCameraMatrix.Up);
                }
            }
            else if (this.m_attachedRotationTo != null)
            {
                this.m_currentCameraMatrix = this.m_attachedRotationOffset * this.m_attachedRotationTo.WorldMatrix;
            }
            else if (this.m_waypoints.Count <= 2)
            {
                if (!this.m_nodeStartMatrix.EqualsFast(ref this.m_nodeEndMatrix, 0.0001))
                {
                    QuaternionD quaternion = QuaternionD.Slerp(QuaternionD.CreateFromRotationMatrix(this.m_nodeStartMatrix), QuaternionD.CreateFromRotationMatrix(this.m_nodeEndMatrix), MathHelper.SmoothStepStable((double) amount));
                    this.m_currentCameraMatrix = MatrixD.CreateFromQuaternion(quaternion);
                }
            }
            else
            {
                float num5 = 1f / ((float) (this.m_waypoints.Count - 1));
                int num6 = (int) Math.Floor((double) (amount / num5));
                if (num6 > (this.m_waypoints.Count - 2))
                {
                    num6 = this.m_waypoints.Count - 2;
                }
                QuaternionD quaternion = QuaternionD.Slerp(QuaternionD.CreateFromRotationMatrix(this.m_waypoints[num6]), QuaternionD.CreateFromRotationMatrix(this.m_waypoints[num6 + 1]), MathHelper.SmoothStepStable((double) ((amount - (num6 * num5)) / num5)));
                this.m_currentCameraMatrix = MatrixD.CreateFromQuaternion(quaternion);
            }
            this.m_currentCameraMatrix.Translation = translation;
            if (this.m_currentNode.ChangeFOVTo > this.MINIMUM_FOV)
            {
                this.m_currentFOV = MathHelper.SmoothStep(this.m_nodeStartFOV, MathHelper.Clamp(this.m_currentNode.ChangeFOVTo, this.MINIMUM_FOV, this.MAXIMUM_FOV), amount);
            }
            this.m_cameraEntity.FOV = this.m_currentFOV;
            if (this.m_currentTime >= this.m_currentNode.Time)
            {
                this.CutsceneNext(false);
            }
        }

        public Cutscene GetCutscene(string name) => 
            (!this.m_cutsceneLibrary.ContainsKey(name) ? null : this.m_cutsceneLibrary[name]);

        public Cutscene GetCutsceneCopy(string name)
        {
            if (!this.m_cutsceneLibrary.ContainsKey(name))
            {
                return null;
            }
            Cutscene cutscene = this.m_cutsceneLibrary[name];
            Cutscene cutscene2 = new Cutscene {
                CanBeSkipped = cutscene.CanBeSkipped,
                FireEventsDuringSkip = cutscene.FireEventsDuringSkip,
                Name = cutscene.Name,
                NextCutscene = cutscene.NextCutscene,
                StartEntity = cutscene.StartEntity,
                StartingFOV = cutscene.StartingFOV,
                StartLookAt = cutscene.StartLookAt
            };
            if (cutscene.SequenceNodes != null)
            {
                cutscene2.SequenceNodes = new List<CutsceneSequenceNode>();
                for (int i = 0; i < cutscene.SequenceNodes.Count; i++)
                {
                    cutscene2.SequenceNodes.Add(new CutsceneSequenceNode());
                    cutscene2.SequenceNodes[i].AttachPositionTo = cutscene.SequenceNodes[i].AttachPositionTo;
                    cutscene2.SequenceNodes[i].AttachRotationTo = cutscene.SequenceNodes[i].AttachRotationTo;
                    cutscene2.SequenceNodes[i].AttachTo = cutscene.SequenceNodes[i].AttachTo;
                    cutscene2.SequenceNodes[i].ChangeFOVTo = cutscene.SequenceNodes[i].ChangeFOVTo;
                    cutscene2.SequenceNodes[i].Event = cutscene.SequenceNodes[i].Event;
                    cutscene2.SequenceNodes[i].EventDelay = cutscene.SequenceNodes[i].EventDelay;
                    cutscene2.SequenceNodes[i].LockRotationTo = cutscene.SequenceNodes[i].LockRotationTo;
                    cutscene2.SequenceNodes[i].LookAt = cutscene.SequenceNodes[i].LookAt;
                    cutscene2.SequenceNodes[i].MoveTo = cutscene.SequenceNodes[i].MoveTo;
                    cutscene2.SequenceNodes[i].RotateLike = cutscene.SequenceNodes[i].RotateLike;
                    cutscene2.SequenceNodes[i].RotateTowards = cutscene.SequenceNodes[i].RotateTowards;
                    cutscene2.SequenceNodes[i].SetPositionTo = cutscene.SequenceNodes[i].SetPositionTo;
                    cutscene2.SequenceNodes[i].SetRorationLike = cutscene.SequenceNodes[i].SetRorationLike;
                    cutscene2.SequenceNodes[i].Time = cutscene.SequenceNodes[i].Time;
                    if ((cutscene.SequenceNodes[i].Waypoints != null) && (cutscene.SequenceNodes[i].Waypoints.Count > 0))
                    {
                        cutscene2.SequenceNodes[i].Waypoints = new List<CutsceneSequenceNodeWaypoint>();
                        for (int j = 0; j < cutscene.SequenceNodes[i].Waypoints.Count; j++)
                        {
                            cutscene2.SequenceNodes[i].Waypoints.Add(new CutsceneSequenceNodeWaypoint());
                            cutscene2.SequenceNodes[i].Waypoints[j].Name = cutscene.SequenceNodes[i].Waypoints[j].Name;
                            cutscene2.SequenceNodes[i].Waypoints[j].Time = cutscene.SequenceNodes[i].Waypoints[j].Time;
                        }
                    }
                }
            }
            return cutscene2;
        }

        public Dictionary<string, Cutscene> GetCutscenes() => 
            this.m_cutsceneLibrary;

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            this.m_objectBuilder.Cutscenes = new Cutscene[this.m_cutsceneLibrary.Count];
            int index = 0;
            foreach (Cutscene cutscene in this.m_cutsceneLibrary.Values)
            {
                this.m_objectBuilder.Cutscenes[index] = cutscene;
                index++;
            }
            return this.m_objectBuilder;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            this.m_objectBuilder = sessionComponent as MyObjectBuilder_CutsceneSessionComponent;
            if (((this.m_objectBuilder != null) && (this.m_objectBuilder.Cutscenes != null)) && (this.m_objectBuilder.Cutscenes.Length != 0))
            {
                foreach (Cutscene cutscene in this.m_objectBuilder.Cutscenes)
                {
                    if (((cutscene.Name != null) && (cutscene.Name.Length > 0)) && !this.m_cutsceneLibrary.ContainsKey(cutscene.Name))
                    {
                        this.m_cutsceneLibrary.Add(cutscene.Name, cutscene);
                    }
                }
            }
        }

        public bool PlayCutscene(string cutsceneName, bool registerEvents = true, string overlay = "")
        {
            if (this.m_cutsceneLibrary.ContainsKey(cutsceneName))
            {
                return this.PlayCutscene(this.m_cutsceneLibrary[cutsceneName], registerEvents, overlay);
            }
            this.CutsceneEnd(true);
            return false;
        }

        public bool PlayCutscene(Cutscene cutscene, bool registerEvents = true, string overlay = "")
        {
            if (cutscene == null)
            {
                this.CutsceneEnd(true);
                return false;
            }
            MySandboxGame.Log.WriteLineAndConsole("Cutscene start: " + cutscene.Name);
            if (this.IsCutsceneRunning)
            {
                this.CutsceneEnd(false);
            }
            else
            {
                this.m_cameraOverlayOriginal = MyHudCameraOverlay.TextureName;
                this.m_overlayEnabled = MyHudCameraOverlay.Enabled;
            }
            this.m_registerEvents = registerEvents;
            this.m_cameraOverlay = overlay;
            this.m_currentCutscene = cutscene;
            this.m_currentNode = null;
            this.m_currentNodeIndex = 0;
            this.m_currentTime = 0f;
            this.m_nodeActivated = false;
            this.m_lookTarget = null;
            this.m_attachedPositionTo = null;
            this.m_attachedRotationTo = null;
            this.m_rotateTarget = null;
            this.m_moveTarget = null;
            this.m_currentFOV = MathHelper.Clamp(this.m_currentCutscene.StartingFOV, this.MINIMUM_FOV, this.MAXIMUM_FOV);
            MyGuiScreenGamePlay.DisableInput = true;
            if (MyCubeBuilder.Static.IsActivated)
            {
                MyCubeBuilder.Static.Deactivate();
            }
            MyHud.CutsceneHud = true;
            MyHudCameraOverlay.TextureName = overlay;
            MyHudCameraOverlay.Enabled = overlay.Length > 0;
            MyHudWarnings.EnableWarnings = false;
            MatrixD identity = MatrixD.Identity;
            MyEntity entityByName = (this.m_currentCutscene.StartEntity.Length > 0) ? MyVisualScriptLogicProvider.GetEntityByName(this.m_currentCutscene.StartEntity) : null;
            if (entityByName != null)
            {
                identity = entityByName.WorldMatrix;
            }
            if ((this.m_currentCutscene.StartLookAt.Length > 0) && !this.m_currentCutscene.StartLookAt.Equals(this.m_currentCutscene.StartEntity))
            {
                entityByName = MyVisualScriptLogicProvider.GetEntityByName(this.m_currentCutscene.StartLookAt);
                if (entityByName != null)
                {
                    identity = MatrixD.CreateLookAtInverse(identity.Translation, entityByName.PositionComp.GetPosition(), identity.Up);
                }
            }
            this.m_nodeStartMatrix = identity;
            this.m_currentCameraMatrix = identity;
            this.m_originalCameraController = MySession.Static.CameraController;
            this.m_cameraEntity.WorldMatrix = identity;
            Vector3D? position = null;
            MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this.m_cameraEntity, position);
            return true;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            this.m_cutsceneLibrary.Clear();
            MyHudWarnings.EnableWarnings = true;
        }

        public override void UpdateBeforeSimulation()
        {
            Vector3D? nullable;
            if (this.m_releaseCamera && (MySession.Static.ControlledEntity != null))
            {
                this.m_releaseCamera = false;
                nullable = null;
                MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.ControlledEntity.Entity, nullable);
                MyHud.CutsceneHud = false;
                MyGuiScreenGamePlay.DisableInput = false;
            }
            if (this.IsCutsceneRunning)
            {
                if (!ReferenceEquals(MySession.Static.CameraController, this.m_cameraEntity))
                {
                    this.m_originalCameraController = MySession.Static.CameraController;
                    nullable = null;
                    MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, this.m_cameraEntity, nullable);
                }
                if ((this.m_currentCutscene.SequenceNodes != null) && (this.m_currentCutscene.SequenceNodes.Count > this.m_currentNodeIndex))
                {
                    this.m_currentNode = this.m_currentCutscene.SequenceNodes[this.m_currentNodeIndex];
                    this.CutsceneUpdate();
                }
                else if ((this.m_currentCutscene.NextCutscene == null) || (this.m_currentCutscene.NextCutscene.Length <= 0))
                {
                    this.CutsceneEnd(true);
                }
                else
                {
                    this.PlayCutscene(this.m_currentCutscene.NextCutscene, this.m_registerEvents, "");
                }
                this.m_cameraEntity.WorldMatrix = this.m_currentCameraMatrix;
            }
        }

        public MatrixD CameraMatrix =>
            this.m_currentCameraMatrix;

        public bool IsCutsceneRunning =>
            (this.m_currentCutscene != null);
    }
}

