namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Entity.UseObject;
    using VRage.Game.Gui;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    [MyEntityType(typeof(MyObjectBuilder_AreaMarker), true)]
    public class MyAreaMarker : MyEntity, IMyUseObject
    {
        protected MyAreaMarkerDefinition m_definition;
        private static List<MyPlaceArea> m_tmpPlaceAreas = new List<MyPlaceArea>();
        private MatrixD m_localActivationMatrix;

        public MyAreaMarker()
        {
        }

        public MyAreaMarker(MyPositionAndOrientation positionAndOrientation, MyAreaMarkerDefinition definition)
        {
            this.m_definition = definition;
            if (definition != null)
            {
                MatrixD worldMatrix = MatrixD.CreateWorld((Vector3D) positionAndOrientation.Position, (Vector3) positionAndOrientation.Forward, (Vector3) positionAndOrientation.Up);
                base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                if (MyPerGameSettings.LimitedWorld)
                {
                    this.ClampToWorld();
                }
                this.InitInternal();
            }
        }

        public virtual unsafe void AddHudMarker()
        {
            MyHudEntityParams* paramsPtr1;
            MyHudEntityParams hudParams = new MyHudEntityParams {
                FlagsEnum = MyHudIndicatorFlagsEnum.SHOW_TEXT
            };
            paramsPtr1.Text = (this.m_definition.DisplayNameEnum != null) ? MyTexts.Get(this.m_definition.DisplayNameEnum.Value) : new StringBuilder();
            paramsPtr1 = (MyHudEntityParams*) ref hudParams;
            hudParams.Owner = 0L;
            MyHud.LocationMarkers.RegisterMarker(this, hudParams);
        }

        protected override void Closing()
        {
            MyHud.LocationMarkers.UnregisterMarker(this);
            base.Closing();
        }

        public virtual unsafe MyActionDescription GetActionInfo(UseActionEnum actionEnum)
        {
            MyActionDescription* descriptionPtr1;
            MyActionDescription description = new MyActionDescription {
                Text = MyStringId.GetOrCompute("NotificationRemoveAreaMarker")
            };
            object[] objArray1 = new object[] { "[" + MyInput.Static.GetGameControl(MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? MyControlsSpace.PICK_UP : MyControlsSpace.USE) + "]" };
            descriptionPtr1->FormatParams = new object[] { "[" + MyInput.Static.GetGameControl(MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? MyControlsSpace.PICK_UP : MyControlsSpace.USE) + "]" };
            descriptionPtr1 = (MyActionDescription*) ref description;
            return description;
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            MyObjectBuilder_AreaMarker objectBuilder = base.GetObjectBuilder(copy) as MyObjectBuilder_AreaMarker;
            objectBuilder.SubtypeName = this.m_definition.Id.SubtypeName;
            return objectBuilder;
        }

        public bool HandleInput() => 
            false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyDefinitionManager.Static.TryGetDefinition<MyAreaMarkerDefinition>(objectBuilder.GetId(), out this.m_definition);
            if (this.m_definition != null)
            {
                m_tmpPlaceAreas.Clear();
                MyPlaceAreas.Static.GetAllAreas(m_tmpPlaceAreas);
                MyPlaceArea area = null;
                int num = 0;
                foreach (MyPlaceArea area2 in m_tmpPlaceAreas)
                {
                    if (area2.AreaType == this.m_definition.Id.SubtypeId)
                    {
                        if (area == null)
                        {
                            area = area2;
                        }
                        num++;
                    }
                }
                if ((this.m_definition.MaxNumber >= 0) && (num >= this.m_definition.MaxNumber))
                {
                    MyEntities.SendCloseRequest(area.Entity);
                }
                m_tmpPlaceAreas.Clear();
                this.InitInternal();
            }
        }

        private void InitInternal()
        {
            float? scale = null;
            base.Init(null, this.m_definition.Model, null, scale, null);
            base.Render.ColorMaskHsv = this.m_definition.ColorHSV;
            base.Render.Transparency = 0.25f;
            base.Render.AddRenderObjects();
            MyRenderProxy.ChangeMaterialTexture(base.Render.RenderObjectIDs[0], "BotFlag", this.m_definition.ColorMetalTexture, null, this.m_definition.AddMapsTexture, null);
            this.m_localActivationMatrix = MatrixD.CreateScale(base.PositionComp.LocalAABB.HalfExtents * 2f) * MatrixD.CreateTranslation(base.PositionComp.LocalAABB.Center);
            HkBoxShape shape = new HkBoxShape((Vector3) this.m_localActivationMatrix.Scale);
            MyPhysicsBody body = new MyPhysicsBody(this, RigidBodyFlag.RBF_DISABLE_COLLISION_RESPONSE);
            base.Physics = body;
            HkMassProperties? massProperties = null;
            body.CreateFromCollisionObject((HkShape) shape, Vector3.Zero, base.WorldMatrix, massProperties, 0x18);
            body.Enabled = true;
            base.Components.Add<MyPlaceArea>(new MySpherePlaceArea(10f, this.m_definition.Id.SubtypeId));
            this.AddHudMarker();
        }

        public void OnSelectionLost()
        {
        }

        public void SetInstanceID(int id)
        {
        }

        public void SetRenderID(uint id)
        {
        }

        public virtual void Use(UseActionEnum actionEnum, IMyEntity user)
        {
            base.Close();
        }

        public MyAreaMarkerDefinition Definition =>
            this.m_definition;

        public override Vector3D LocationForHudMarker =>
            (base.PositionComp.GetPosition() + Vector3D.TransformNormal(this.m_definition.MarkerPosition, base.PositionComp.WorldMatrix));

        IMyEntity IMyUseObject.Owner =>
            this;

        MyModelDummy IMyUseObject.Dummy =>
            null;

        public float InteractiveDistance =>
            5f;

        public MatrixD ActivationMatrix =>
            (this.m_localActivationMatrix * base.WorldMatrix);

        public uint RenderObjectID =>
            base.Render.RenderObjectIDs[0];

        public int InstanceID =>
            -1;

        public bool ShowOverlay =>
            true;

        public UseActionEnum SupportedActions =>
            (MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? UseActionEnum.PickUp : UseActionEnum.Manipulate);

        public UseActionEnum PrimaryAction =>
            (MyFakes.ENABLE_SEPARATE_USE_AND_PICK_UP_KEY ? UseActionEnum.PickUp : UseActionEnum.Manipulate);

        public UseActionEnum SecondaryAction =>
            UseActionEnum.None;

        public bool ContinuousUsage =>
            false;

        bool IMyUseObject.PlayIndicatorSound =>
            true;

        MatrixD IMyUseObject.WorldMatrix =>
            base.WorldMatrix;
    }
}

