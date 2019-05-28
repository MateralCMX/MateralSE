namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Engine.Voxels.Storage;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Game", "Voxels")]
    public class MyGuiScreenDebugVoxels : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_filesCombo;
        private MyGuiControlCombobox m_materialsCombo;
        private MyGuiControlCombobox m_shapesCombo;
        private MyGuiControlCombobox m_rangesCombo;
        private string m_selectedVoxelFile;
        private string m_selectedVoxelMaterial;
        private static MyRenderQualityEnum m_voxelRangesQuality;

        public MyGuiScreenDebugVoxels() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void filesCombo_OnSelect()
        {
            if (this.m_filesCombo.GetSelectedKey() != 0)
            {
                this.m_selectedVoxelFile = Path.Combine(MyFileSystem.ContentPath, this.m_filesCombo.GetSelectedValue().ToString() + ".vx2");
            }
        }

        private void ForceVoxelizeAllVoxelMaps(MyGuiControlBase sender)
        {
            DictionaryValuesReader<long, MyVoxelBase> instances = MySession.Static.VoxelMaps.Instances;
            int num = 0;
            using (Dictionary<long, MyVoxelBase>.ValueCollection.Enumerator enumerator = instances.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    num++;
                    MyOctreeStorage storage = enumerator.Current.Storage as MyOctreeStorage;
                    if (storage != null)
                    {
                        storage.Voxelize(MyStorageDataTypeFlags.Content);
                    }
                }
            }
        }

        private void GeneratePhysics(MyGuiControlButton sender)
        {
            foreach (MyVoxelBase base2 in MySession.Static.VoxelMaps.Instances)
            {
                if (base2.Physics != null)
                {
                    (base2.Physics as MyVoxelPhysicsBody).GenerateAllShapes();
                }
            }
        }

        private void GenerateRender(MyGuiControlButton sender)
        {
            foreach (MyVoxelBase local1 in MySession.Static.VoxelMaps.Instances)
            {
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugVoxels";

        private MyGuiControlCombobox MakeComboFromFiles(string path, string filter = "*", MySearchOption search = 1)
        {
            Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlCombobox combobox = base.AddCombo(null, textColor, size, 10);
            long key = 0L + 1L;
            int? sortOrder = null;
            combobox.AddItem(key, "", sortOrder, null);
            foreach (string str in MyFileSystem.GetFiles(path, filter, search))
            {
                key += 1L;
                sortOrder = null;
                combobox.AddItem(key, Path.GetFileNameWithoutExtension(str), sortOrder, null);
            }
            combobox.SelectItemByIndex(0);
            return combobox;
        }

        private void materialsCombo_OnSelect()
        {
            this.m_selectedVoxelMaterial = this.m_materialsCombo.GetSelectedValue().ToString();
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            m_voxelRangesQuality = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality;
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Voxels", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            base.AddSlider("Max precalc time", 0f, 20f, null, MemberHelper.GetMember<float>(Expression.Lambda<Func<float>>(Expression.Field(null, fieldof(MyFakes.MAX_PRECALC_TIME_IN_MILLIS)), Array.Empty<ParameterExpression>())), color);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Enable yielding", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_YIELDING_IN_PRECALC_TASK)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Enable storage cache", MyVoxelOperationsSessionComponent.EnableCache, (Action<MyGuiControlCheckbox>) (x => (MyVoxelOperationsSessionComponent.EnableCache = x.IsChecked)), true, null, color, captionOffset);
            this.m_filesCombo = this.MakeComboFromFiles(Path.Combine(MyFileSystem.ContentPath, "VoxelMaps"), "*", MySearchOption.AllDirectories);
            this.m_filesCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.filesCombo_OnSelect);
            color = null;
            captionOffset = null;
            this.m_materialsCombo = base.AddCombo(null, color, captionOffset, 10);
            foreach (MyVoxelMaterialDefinition definition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                int? sortOrder = null;
                this.m_materialsCombo.AddItem((long) definition.Index, new StringBuilder(definition.Id.SubtypeName), sortOrder, null);
            }
            this.m_materialsCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.materialsCombo_OnSelect);
            this.m_materialsCombo.SelectItemByIndex(0);
            color = null;
            base.AddCombo<MyVoxelDebugDrawMode>(null, MemberHelper.GetMember<MyVoxelDebugDrawMode>(Expression.Lambda<Func<MyVoxelDebugDrawMode>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXELS_MODE)), Array.Empty<ParameterExpression>())), true, 10, null, color);
            base.AddLabel("Voxel ranges", Color.Yellow.ToVector4(), 0.7f, null, "Debug");
            color = null;
            base.AddCombo<MyRenderQualityEnum>(null, MemberHelper.GetMember<MyRenderQualityEnum>(Expression.Lambda<Func<MyRenderQualityEnum>>(Expression.Property(null, (MethodInfo) methodof(MyGuiScreenDebugVoxels.get_VoxelRangesQuality)), Array.Empty<ParameterExpression>())), true, 10, null, color);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Remove all"), new Action<MyGuiControlButton>(this.RemoveAllAsteroids), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Generate render"), new Action<MyGuiControlButton>(this.GenerateRender), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Generate physics"), new Action<MyGuiControlButton>(this.GeneratePhysics), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Reset all"), new Action<MyGuiControlButton>(this.ResetAll), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Sweep all"), new Action<MyGuiControlButton>(this.SweepAll), null, color, captionOffset, true, true);
            color = null;
            captionOffset = null;
            base.AddButton(new StringBuilder("Revert first"), new Action<MyGuiControlButton>(this.RevertFirst), null, color, captionOffset, true, true);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Freeze terrain queries", MyRenderProxy.Settings.FreezeTerrainQueries, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.FreezeTerrainQueries = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw clipmap cells", MyRenderProxy.Settings.DebugRenderClipmapCells, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DebugRenderClipmapCells = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw edited cells", MyDebugDrawSettings.DEBUG_DRAW_VOXEL_ACCESS, (Action<MyGuiControlCheckbox>) (x => (MyDebugDrawSettings.DEBUG_DRAW_VOXEL_ACCESS = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Wireframe", MyRenderProxy.Settings.Wireframe, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.Wireframe = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug texture lod colors", MyRenderProxy.Settings.DebugTextureLodColor, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DebugTextureLodColor = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Enable physics shape discard", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_VOXEL_PHYSICS_SHAPE_DISCARDING)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Use triangle cache", this, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Property(Expression.Constant(this, typeof(MyGuiScreenDebugVoxels)), (MethodInfo) methodof(MyGuiScreenDebugVoxels.get_UseTriangleCache)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Use storage cache", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyStorageBase.UseStorageCache)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Voxel AO", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_VOXEL_COMPUTED_OCCLUSION)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
        }

        private void RemoveAllAsteroids(MyGuiControlButton sender)
        {
            MySession.Static.VoxelMaps.Clear();
        }

        private void ResavePrefabs(MyGuiControlButton sender)
        {
            string[] strArray = MyFileSystem.GetFiles(MyFileSystem.ContentPath, "*.vx2", MySearchOption.AllDirectories).ToArray<string>();
            for (int i = 0; i < strArray.Length; i++)
            {
                byte[] buffer;
                string absoluteFilePath = strArray[i];
                MyStorageBase.LoadFromFile(absoluteFilePath, null, true).Save(out buffer);
                using (Stream stream = MyFileSystem.OpenWrite(absoluteFilePath, FileMode.Open))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private void ResetAll(MyGuiControlBase sender)
        {
            DictionaryValuesReader<long, MyVoxelBase> instances = MySession.Static.VoxelMaps.Instances;
            int num = 0;
            foreach (MyVoxelBase base2 in instances)
            {
                num++;
                if (!(base2 is MyVoxelPhysics))
                {
                    MyOctreeStorage storage = base2.Storage as MyOctreeStorage;
                    if (storage != null)
                    {
                        storage.Reset(MyStorageDataTypeFlags.All);
                    }
                }
            }
        }

        private void RevertFirst(MyGuiControlBase sender)
        {
            DictionaryValuesReader<long, MyVoxelBase> instances = MySession.Static.VoxelMaps.Instances;
            int num = 0;
            foreach (MyVoxelBase base2 in instances)
            {
                num++;
                if (!(base2 is MyVoxelPhysics))
                {
                    MyStorageBase storage = base2.Storage as MyStorageBase;
                    if (storage != null)
                    {
                        storage.AccessDeleteFirst();
                    }
                }
            }
        }

        private void SweepAll(MyGuiControlBase sender)
        {
            DictionaryValuesReader<long, MyVoxelBase> instances = MySession.Static.VoxelMaps.Instances;
            int num = 0;
            foreach (MyVoxelBase base2 in instances)
            {
                num++;
                if (!(base2 is MyVoxelPhysics))
                {
                    MyStorageBase storage = base2.Storage as MyStorageBase;
                    if (storage != null)
                    {
                        storage.Sweep(MyStorageDataTypeFlags.All);
                    }
                }
            }
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        private static MyRenderQualityEnum VoxelRangesQuality
        {
            get => 
                m_voxelRangesQuality;
            set
            {
                if (m_voxelRangesQuality != value)
                {
                    m_voxelRangesQuality = value;
                    MyRenderComponentVoxelMap.SetLodQuality(m_voxelRangesQuality);
                }
            }
        }

        private bool UseTriangleCache
        {
            get => 
                false;
            set
            {
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugVoxels.<>c <>9 = new MyGuiScreenDebugVoxels.<>c();
            public static Action<MyGuiControlCheckbox> <>9__15_2;
            public static Action<MyGuiControlCheckbox> <>9__15_5;
            public static Action<MyGuiControlCheckbox> <>9__15_6;
            public static Action<MyGuiControlCheckbox> <>9__15_7;
            public static Action<MyGuiControlCheckbox> <>9__15_8;
            public static Action<MyGuiControlCheckbox> <>9__15_9;

            internal void <RecreateControls>b__15_2(MyGuiControlCheckbox x)
            {
                MyVoxelOperationsSessionComponent.EnableCache = x.IsChecked;
            }

            internal void <RecreateControls>b__15_5(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.FreezeTerrainQueries = x.IsChecked;
            }

            internal void <RecreateControls>b__15_6(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DebugRenderClipmapCells = x.IsChecked;
            }

            internal void <RecreateControls>b__15_7(MyGuiControlCheckbox x)
            {
                MyDebugDrawSettings.DEBUG_DRAW_VOXEL_ACCESS = x.IsChecked;
            }

            internal void <RecreateControls>b__15_8(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.Wireframe = x.IsChecked;
            }

            internal void <RecreateControls>b__15_9(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DebugTextureLodColor = x.IsChecked;
            }
        }
    }
}

