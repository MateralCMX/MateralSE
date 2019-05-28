namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI.DebugInputComponents;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    [MyDebugScreen("Game", "Cutscenes")]
    internal class MyGuiScreenDebugCutscenes : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_comboCutscenes;
        private MyGuiControlCombobox m_comboNodes;
        private MyGuiControlCombobox m_comboWaypoints;
        private MyGuiControlButton m_playButton;
        private MyGuiControlSlider m_nodeTimeSlider;
        private MyGuiControlButton m_spawnButton;
        private MyGuiControlButton m_removeAllButton;
        private MyGuiControlButton m_addNodeButton;
        private MyGuiControlButton m_deleteNodeButton;
        private MyGuiControlButton m_addCutsceneButton;
        private MyGuiControlButton m_deleteCutsceneButton;
        private Cutscene m_selectedCutscene;
        private CutsceneSequenceNode m_selectedCutsceneNode;

        public MyGuiScreenDebugCutscenes() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCubeBlocks";

        private void m_comboCutscenes_ItemSelected()
        {
            this.m_selectedCutscene = MySession.Static.GetComponent<MySessionComponentCutscenes>().GetCutscene(this.m_comboCutscenes.GetSelectedValue().ToString());
            this.m_comboNodes.ClearItems();
            if (this.m_selectedCutscene.SequenceNodes != null)
            {
                int num = 0;
                foreach (CutsceneSequenceNode node in this.m_selectedCutscene.SequenceNodes)
                {
                    int? sortOrder = null;
                    this.m_comboNodes.AddItem((long) num, node.Time.ToString(), sortOrder, null);
                    num++;
                }
            }
            if (this.m_comboNodes.GetItemsCount() > 0)
            {
                this.m_comboNodes.SelectItemByIndex(0);
            }
        }

        private void m_comboNodes_ItemSelected()
        {
            this.m_selectedCutsceneNode = this.m_selectedCutscene.SequenceNodes[(int) this.m_comboNodes.GetSelectedKey()];
            this.m_nodeTimeSlider.Value = this.m_selectedCutsceneNode.Time;
            this.m_comboWaypoints.ClearItems();
            if (this.m_selectedCutsceneNode.Waypoints != null)
            {
                foreach (CutsceneSequenceNodeWaypoint waypoint in this.m_selectedCutsceneNode.Waypoints)
                {
                    int? sortOrder = null;
                    this.m_comboWaypoints.AddItem((long) waypoint.Name.GetHashCode(), waypoint.Name, sortOrder, null);
                }
                if (this.m_comboWaypoints.GetItemsCount() > 0)
                {
                    this.m_comboWaypoints.SelectItemByIndex(0);
                }
            }
        }

        private void m_comboWaypoints_ItemSelected()
        {
        }

        private void onClick_AddCutsceneButton(MyGuiControlButton sender)
        {
            MySessionComponentCutscenes component = MySession.Static.GetComponent<MySessionComponentCutscenes>();
            string key = "Cutscene" + component.GetCutscenes().Count;
            component.GetCutscenes().Add(key, new Cutscene());
            this.m_comboCutscenes.ClearItems();
            foreach (string str2 in component.GetCutscenes().Keys)
            {
                int? sortOrder = null;
                this.m_comboCutscenes.AddItem((long) str2.GetHashCode(), str2, sortOrder, null);
            }
            this.m_comboCutscenes.SelectItemByKey((long) key.GetHashCode(), true);
        }

        private void onClick_AddNodeButton(MyGuiControlButton sender)
        {
            List<CutsceneSequenceNode> list1 = new List<CutsceneSequenceNode>();
            list1.Add(new CutsceneSequenceNode());
            List<CutsceneSequenceNode> second = list1;
            if (this.m_selectedCutscene.SequenceNodes != null)
            {
                this.m_selectedCutscene.SequenceNodes = this.m_selectedCutscene.SequenceNodes.Union<CutsceneSequenceNode>(second).ToList<CutsceneSequenceNode>();
            }
            else
            {
                this.m_selectedCutscene.SequenceNodes = second;
            }
        }

        private void onClick_DeleteCutsceneButton(MyGuiControlButton sender)
        {
            MySessionComponentCutscenes component = MySession.Static.GetComponent<MySessionComponentCutscenes>();
            if (this.m_selectedCutscene != null)
            {
                this.m_comboNodes.ClearItems();
                this.m_comboWaypoints.ClearItems();
                this.m_selectedCutsceneNode = null;
                component.GetCutscenes().Remove(this.m_selectedCutscene.Name);
                this.m_comboCutscenes.RemoveItem((long) this.m_selectedCutscene.Name.GetHashCode());
                if (component.GetCutscenes().Count == 0)
                {
                    this.m_selectedCutscene = null;
                }
                else
                {
                    this.m_comboCutscenes.SelectItemByIndex(component.GetCutscenes().Count - 1);
                }
            }
        }

        private void onClick_DeleteNodeButton(MyGuiControlButton sender)
        {
            if (this.m_selectedCutscene.SequenceNodes != null)
            {
                this.m_selectedCutscene.SequenceNodes = (from x in this.m_selectedCutscene.SequenceNodes
                    where !ReferenceEquals(x, this.m_selectedCutsceneNode)
                    select x).ToList<CutsceneSequenceNode>();
            }
        }

        private void onClick_PlayButton(MyGuiControlButton sender)
        {
            if (this.m_comboCutscenes.GetItemsCount() > 0)
            {
                MySession.Static.GetComponent<MySessionComponentCutscenes>().PlayCutscene(this.m_comboCutscenes.GetSelectedValue().ToString(), true, "");
            }
        }

        private void onEntitySpawned(MyEntity entity)
        {
            if (this.m_selectedCutsceneNode != null)
            {
                this.m_selectedCutsceneNode.MoveTo = entity.Name;
                this.m_selectedCutsceneNode.RotateTowards = entity.Name;
            }
        }

        private void OnNodeTimeChanged(MyGuiControlSlider slider)
        {
            if (this.m_selectedCutsceneNode != null)
            {
                this.m_selectedCutsceneNode.Time = slider.Value;
            }
        }

        private void onRemoveAllButton(MyGuiControlButton sender)
        {
            MySession.Static.GetComponent<MySessionComponentCutscenes>().GetCutscenes().Clear();
        }

        private void onSpawnButton(MyGuiControlButton sender)
        {
            MyVisualScriptingDebugInputComponent.SpawnEntity(new Action<MyEntity>(this.onEntitySpawned));
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Cutscenes", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? textColor = null;
            captionOffset = null;
            this.m_comboCutscenes = base.AddCombo(null, textColor, captionOffset, 10);
            textColor = null;
            captionOffset = null;
            this.m_playButton = base.AddButton(new StringBuilder("Play"), new Action<MyGuiControlButton>(this.onClick_PlayButton), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            this.m_addCutsceneButton = base.AddButton(new StringBuilder("Add cutscene"), new Action<MyGuiControlButton>(this.onClick_AddCutsceneButton), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            this.m_deleteCutsceneButton = base.AddButton(new StringBuilder("Delete cutscene"), new Action<MyGuiControlButton>(this.onClick_DeleteCutsceneButton), null, textColor, captionOffset, true, true);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Nodes", Color.Yellow.ToVector4(), 1f, null, "Debug");
            textColor = null;
            captionOffset = null;
            this.m_comboNodes = base.AddCombo(null, textColor, captionOffset, 10);
            this.m_comboNodes.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_comboNodes_ItemSelected);
            textColor = null;
            captionOffset = null;
            this.m_addNodeButton = base.AddButton(new StringBuilder("Add node"), new Action<MyGuiControlButton>(this.onClick_AddNodeButton), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            this.m_deleteNodeButton = base.AddButton(new StringBuilder("Delete node"), new Action<MyGuiControlButton>(this.onClick_DeleteNodeButton), null, textColor, captionOffset, true, true);
            textColor = null;
            this.m_nodeTimeSlider = base.AddSlider("Node time", 0f, 0f, 100f, new Action<MyGuiControlSlider>(this.OnNodeTimeChanged), textColor);
            this.m_comboCutscenes.ClearItems();
            foreach (string str in MySession.Static.GetComponent<MySessionComponentCutscenes>().GetCutscenes().Keys)
            {
                int? sortOrder = null;
                this.m_comboCutscenes.AddItem((long) str.GetHashCode(), str, sortOrder, null);
            }
            this.m_comboCutscenes.SortItemsByValueText();
            this.m_comboCutscenes.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_comboCutscenes_ItemSelected);
            base.AddLabel("Waypoints", Color.Yellow.ToVector4(), 1f, null, "Debug");
            textColor = null;
            captionOffset = null;
            this.m_comboWaypoints = base.AddCombo(null, textColor, captionOffset, 10);
            this.m_comboWaypoints.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_comboWaypoints_ItemSelected);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            textColor = null;
            captionOffset = null;
            this.m_spawnButton = base.AddButton(new StringBuilder("Spawn entity"), new Action<MyGuiControlButton>(this.onSpawnButton), null, textColor, captionOffset, true, true);
            textColor = null;
            captionOffset = null;
            this.m_removeAllButton = base.AddButton(new StringBuilder("Remove all"), new Action<MyGuiControlButton>(this.onRemoveAllButton), null, textColor, captionOffset, true, true);
            if (this.m_comboCutscenes.GetItemsCount() > 0)
            {
                this.m_comboCutscenes.SelectItemByIndex(0);
            }
        }
    }
}

