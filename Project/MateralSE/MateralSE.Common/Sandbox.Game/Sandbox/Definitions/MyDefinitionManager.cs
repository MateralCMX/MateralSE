namespace Sandbox.Definitions
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.AppCode;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.AI.Pathfinding;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data;
    using VRage.FileSystem;
    using VRage.Filesystem.FindFilesRegEx;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Graphics;
    using VRage.Game.Models;
    using VRage.Game.ObjectBuilders;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Library;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Stats;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;
    using VRageRender.Messages;
    using VRageRender.Utils;

    [PreloadRequired]
    public class MyDefinitionManager : MyDefinitionManagerBase
    {
        private Dictionary<string, DefinitionSet> m_modDefinitionSets = new Dictionary<string, DefinitionSet>();
        private DefinitionSet m_currentLoadingSet;
        private const string DUPLICATE_ENTRY_MESSAGE = "Duplicate entry of '{0}'";
        private const string UNKNOWN_ENTRY_MESSAGE = "Unknown type '{0}'";
        private const string WARNING_ON_REDEFINITION_MESSAGE = "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'";
        private bool m_transparentMaterialsInitialized;
        private ConcurrentDictionary<string, MyObjectBuilder_Definitions> m_preloadedDefinitionBuilders = new ConcurrentDictionary<string, MyObjectBuilder_Definitions>();
        private FastResourceLock m_voxelMaterialsLock = new FastResourceLock();
        private Lazy<List<MyVoxelMapStorageDefinition>> m_voxelMapStorageDefinitionsForProceduralRemovals;
        private Lazy<List<MyVoxelMapStorageDefinition>> m_voxelMapStorageDefinitionsForProceduralAdditions;
        private Lazy<List<MyVoxelMapStorageDefinition>> m_voxelMapStorageDefinitionsForProceduralPrimaryAdditions;
        private static Dictionary<string, bool> m_directoryExistCache = new Dictionary<string, bool>();

        static MyDefinitionManager()
        {
            MyDefinitionManagerBase.Static = new MyDefinitionManager();
        }

        private MyDefinitionManager()
        {
            this.Loading = false;
            base.m_definitions = new DefinitionSet();
            this.m_voxelMapStorageDefinitionsForProceduralRemovals = new Lazy<List<MyVoxelMapStorageDefinition>>(() => (from x in this.m_definitions.m_voxelMapStorages.Values
                where x.UseForProceduralRemovals
                select x).ToList<MyVoxelMapStorageDefinition>(), LazyThreadSafetyMode.PublicationOnly);
            this.m_voxelMapStorageDefinitionsForProceduralAdditions = new Lazy<List<MyVoxelMapStorageDefinition>>(() => (from x in this.m_definitions.m_voxelMapStorages.Values
                where x.UseForProceduralAdditions
                select x).ToList<MyVoxelMapStorageDefinition>(), LazyThreadSafetyMode.PublicationOnly);
            this.m_voxelMapStorageDefinitionsForProceduralPrimaryAdditions = new Lazy<List<MyVoxelMapStorageDefinition>>(() => (from x in this.m_definitions.m_voxelMapStorages.Values
                where x.UseAsPrimaryProceduralAdditionShape
                select x).ToList<MyVoxelMapStorageDefinition>(), LazyThreadSafetyMode.PublicationOnly);
        }

        private void AddBasePrefabName(DefinitionSet definitionSet, MyCubeSize size, bool isStatic, bool isCreative, string prefabName)
        {
            if (!string.IsNullOrEmpty(prefabName))
            {
                definitionSet.m_basePrefabNames[ComputeBasePrefabIndex(size, isStatic, isCreative)] = prefabName;
            }
        }

        private void AddEntriesToBlueprintClasses()
        {
            foreach (BlueprintClassEntry entry in this.m_definitions.m_blueprintClassEntries)
            {
                if (!entry.Enabled)
                {
                    continue;
                }
                MyBlueprintClassDefinition definition = null;
                MyBlueprintDefinitionBase blueprint = null;
                MyDefinitionId key = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintClassDefinition), entry.Class);
                this.m_definitions.m_blueprintClasses.TryGetValue(key, out definition);
                blueprint = this.FindBlueprintByClassEntry(entry);
                if ((blueprint != null) && (definition != null))
                {
                    definition.AddBlueprint(blueprint);
                }
            }
            this.m_definitions.m_blueprintClassEntries.Clear();
            foreach (KeyValuePair<MyDefinitionId, MyDefinitionBase> pair in this.m_definitions.m_definitionsById)
            {
                MyProductionBlockDefinition definition2 = pair.Value as MyProductionBlockDefinition;
                if (definition2 != null)
                {
                    definition2.LoadPostProcess();
                }
            }
        }

        private void AddEntriesToEnvironmentItemClasses()
        {
            foreach (EnvironmentItemsEntry entry in this.m_definitions.m_environmentItemsEntries)
            {
                if (!entry.Enabled)
                {
                    continue;
                }
                MyEnvironmentItemsDefinition definition = null;
                MyDefinitionId defId = new MyDefinitionId(MyObjectBuilderType.Parse(entry.Type), entry.Subtype);
                if (!this.TryGetDefinition<MyEnvironmentItemsDefinition>(defId, out definition))
                {
                    string message = "Environment items definition " + defId.ToString() + " not found!";
                    MyDefinitionErrors.Add(MyModContext.BaseGame, message, TErrorSeverity.Warning, true);
                    continue;
                }
                if (this.FindEnvironmentItemByEntry(definition, entry) != null)
                {
                    definition.AddItemDefinition(MyStringHash.GetOrCompute(entry.ItemSubtype), entry.Frequency, false);
                }
            }
            using (List<MyEnvironmentItemsDefinition>.Enumerator enumerator2 = this.GetDefinitionsOfType<MyEnvironmentItemsDefinition>().GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.RecomputeFrequencies();
                }
            }
            this.m_definitions.m_environmentItemsEntries.Clear();
        }

        public void AddMissingWheelModelDefinition(string wheelType)
        {
            MyLog.Default.WriteLine("Missing wheel models definition in WheelModels.sbc for " + wheelType);
            MyWheelModelsDefinition definition1 = new MyWheelModelsDefinition();
            definition1.AngularVelocityThreshold = float.MaxValue;
            this.m_definitions.m_wheelModels[wheelType] = definition1;
        }

        private void AfterLoad(MyModContext context, DefinitionSet definitionSet)
        {
            MyDefinitionPostprocessor.Bundle definitions = new MyDefinitionPostprocessor.Bundle {
                Context = context,
                Set = this.m_currentLoadingSet
            };
            foreach (MyDefinitionPostprocessor postprocessor in MyDefinitionManagerBase.m_postProcessors)
            {
                if (definitionSet.Definitions.TryGetValue(postprocessor.DefinitionType, out definitions.Definitions))
                {
                    postprocessor.AfterLoaded(ref definitions);
                }
            }
        }

        private void AfterPostprocess()
        {
            foreach (MyDefinitionPostprocessor postprocessor in MyDefinitionManagerBase.m_postProcessors)
            {
                Dictionary<MyStringHash, MyDefinitionBase> dictionary;
                if (this.m_definitions.Definitions.TryGetValue(postprocessor.DefinitionType, out dictionary))
                {
                    postprocessor.AfterPostprocess(this.m_definitions, dictionary);
                }
            }
        }

        private static void Check<T>(bool conditionResult, T identifier, bool failOnDebug = true, string messageFormat = "Duplicate entry of '{0}'")
        {
            if (!conditionResult)
            {
                string msg = string.Format(messageFormat, identifier.ToString());
                bool flag1 = failOnDebug;
                MySandboxGame.Log.WriteLine(msg);
            }
        }

        private void CheckCharacterPickup()
        {
            if (MyPerGameSettings.Game == GameEnum.ME_GAME)
            {
                HashSet<MyDefinitionId> set = new HashSet<MyDefinitionId> {
                    new MyDefinitionId(typeof(MyObjectBuilder_Character), "Peasant_male"),
                    new MyDefinitionId(typeof(MyObjectBuilder_Character), "Medieval_barbarian"),
                    new MyDefinitionId(typeof(MyObjectBuilder_Character), "Medieval_deer"),
                    new MyDefinitionId(typeof(MyObjectBuilder_Character), "Medieval_wolf")
                };
                MyContainerDefinition definition = null;
                string format = "Character definition {0} is missing a pickup component! You will not be able to pickup things with this character! See the player character in EntityContainers.sbc and EntityComponents.sbc for an example.";
                foreach (KeyValuePair<string, MyCharacterDefinition> pair in this.m_definitions.m_characters)
                {
                    MyCharacterDefinition definition2 = pair.Value;
                    if (!set.Contains(definition2.Id))
                    {
                        if (!this.TryGetContainerDefinition(definition2.Id, out definition))
                        {
                            MyDefinitionErrors.Add(MyModContext.UnknownContext, string.Format(format, definition2.Id.ToString()), TErrorSeverity.Warning, true);
                            continue;
                        }
                        bool flag = false;
                        using (List<MyContainerDefinition.DefaultComponent>.Enumerator enumerator2 = definition.DefaultComponents.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                Type builderType = (Type) enumerator2.Current.BuilderType;
                                if (typeof(MyObjectBuilder_CharacterPickupComponent).IsAssignableFrom(builderType))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            MyDefinitionErrors.Add(MyModContext.UnknownContext, string.Format(format, definition2.Id.ToString()), TErrorSeverity.Warning, true);
                        }
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void CheckComponentContainers()
        {
            if (this.m_definitions.m_entityContainers != null)
            {
                foreach (KeyValuePair<MyDefinitionId, MyContainerDefinition> pair in this.m_definitions.m_entityContainers)
                {
                    foreach (MyContainerDefinition.DefaultComponent component in pair.Value.DefaultComponents)
                    {
                        try
                        {
                            MyComponentFactory.CreateInstanceByTypeId(component.BuilderType);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void CheckDefinition(ref MyDefinitionId id)
        {
            this.CheckDefinition<MyDefinitionBase>(ref id);
        }

        private void CheckDefinition<T>(ref MyDefinitionId id) where T: MyDefinitionBase
        {
            try
            {
                bool flag1;
                MyDefinitionBase definition = base.GetDefinition<T>(id.SubtypeId);
                if (!((definition != null) || flag1))
                {
                    string msg = $"No definition '{(MyDefinitionId) id}'. Maybe a mistake in XML?";
                    MySandboxGame.Log.WriteLine(msg);
                }
                else
                {
                    flag1 = this.m_definitions.m_definitionsById.TryGetValue(id, out definition);
                    if (!(definition is T))
                    {
                        string msg = $"Definition '{(MyDefinitionId) id}' is not of desired type.";
                        MySandboxGame.Log.WriteLine(msg);
                    }
                }
            }
            catch (KeyNotFoundException)
            {
            }
        }

        [Conditional("DEBUG")]
        private void CheckEntityComponents()
        {
            if (this.m_definitions.m_entityComponentDefinitions != null)
            {
                foreach (KeyValuePair<MyDefinitionId, MyComponentDefinitionBase> pair in this.m_definitions.m_entityComponentDefinitions)
                {
                    try
                    {
                        MyComponentBase base2 = MyComponentFactory.CreateInstanceByTypeId(pair.Key.TypeId);
                        if (base2 == null)
                        {
                            continue;
                        }
                        base2.Init(pair.Value);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static MyObjectBuilder_Definitions CheckPrefabs(string file)
        {
            List<MyObjectBuilder_PrefabDefinition> prefabs = null;
            using (Stream stream = MyFileSystem.OpenRead(file))
            {
                if (stream != null)
                {
                    using (Stream stream2 = stream.UnwrapGZip())
                    {
                        if (stream2 != null)
                        {
                            CheckXmlForPrefabs(file, ref prefabs, stream2);
                        }
                    }
                }
            }
            MyObjectBuilder_Definitions definitions = null;
            if (prefabs != null)
            {
                definitions = new MyObjectBuilder_Definitions {
                    Prefabs = prefabs.ToArray()
                };
            }
            return definitions;
        }

        private void CheckWeaponRelatedDefinitions()
        {
            foreach (MyWeaponDefinition definition in this.m_definitions.m_weaponDefinitionsById.Values)
            {
                foreach (MyDefinitionId id in definition.AmmoMagazinesId)
                {
                    Check<MyDefinitionId>(this.m_definitions.m_definitionsById.ContainsKey(id), id, true, "Unknown type '{0}'");
                    MyAmmoMagazineDefinition ammoMagazineDefinition = this.GetAmmoMagazineDefinition(id);
                    Check<MyDefinitionId>(this.m_definitions.m_ammoDefinitionsById.ContainsKey(ammoMagazineDefinition.AmmoDefinitionId), ammoMagazineDefinition.AmmoDefinitionId, true, "Unknown type '{0}'");
                    MyAmmoDefinition ammoDefinition = this.GetAmmoDefinition(ammoMagazineDefinition.AmmoDefinitionId);
                    if (!definition.HasSpecificAmmoData(ammoDefinition))
                    {
                        StringBuilder builder = new StringBuilder("Weapon definition lacks ammo data properties for given ammo definition: ");
                        builder.Append(ammoDefinition.Id.SubtypeName);
                        MyDefinitionErrors.Add(definition.Context, builder.ToString(), TErrorSeverity.Critical, true);
                    }
                }
            }
        }

        private static void CheckXmlForPrefabs(string file, ref List<MyObjectBuilder_PrefabDefinition> prefabs, Stream readStream)
        {
            using (XmlReader reader = XmlReader.Create(readStream))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                    {
                        continue;
                    }
                    if (reader.Name != "SpawnGroups")
                    {
                        if (reader.Name != "Prefabs")
                        {
                            continue;
                        }
                        prefabs = new List<MyObjectBuilder_PrefabDefinition>();
                        while (reader.ReadToFollowing("Prefab"))
                        {
                            ReadPrefabHeader(file, ref prefabs, reader);
                        }
                    }
                    break;
                }
            }
        }

        private void CompatPhase(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            this.InitDefinitionsCompat(context, objBuilder.Fonts);
        }

        private static int ComputeBasePrefabIndex(MyCubeSize size, bool isStatic, bool isCreative) => 
            ((((int) (size * ((MyCubeSize) 4))) + (isStatic ? 2 : 0)) + (isCreative ? 1 : 0));

        private void CreateMapMultiBlockDefinitionToBlockDefinition()
        {
            if (MyFakes.ENABLE_MULTIBLOCKS)
            {
                ListReader<MyMultiBlockDefinition> multiBlockDefinitions = this.GetMultiBlockDefinitions();
                List<MyCubeBlockDefinition> list = this.m_definitions.m_definitionsById.Values.OfType<MyCubeBlockDefinition>().ToList<MyCubeBlockDefinition>();
                foreach (MyMultiBlockDefinition definition in multiBlockDefinitions)
                {
                    foreach (MyCubeBlockDefinition definition2 in list)
                    {
                        if (definition2.MultiBlock == definition.Id.SubtypeName)
                        {
                            if (!this.m_definitions.m_mapMultiBlockDefToCubeBlockDef.ContainsKey(definition.Id.SubtypeName))
                            {
                                this.m_definitions.m_mapMultiBlockDefToCubeBlockDef.Add(definition.Id.SubtypeName, definition2);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static void CreateTransparentMaterials()
        {
            List<string> texturesToLoad = new List<string>();
            HashSet<string> textures = new HashSet<string>();
            foreach (MyTransparentMaterialDefinition definition in Static.GetTransparentMaterialDefinitions())
            {
                MyTransparentMaterials.AddMaterial(new MyTransparentMaterial(MyStringId.GetOrCompute(definition.Id.SubtypeName), definition.TextureType, definition.Texture, definition.GlossTexture, definition.SoftParticleDistanceScale, definition.CanBeAffectedByLights, definition.AlphaMistingEnable, definition.Color, definition.ColorAdd, definition.ShadowMultiplier, definition.LightMultiplier, definition.IsFlareOccluder, definition.UseAtlas, definition.AlphaMistingStart, definition.AlphaMistingEnd, definition.AlphaSaturation, definition.Reflectivity, definition.AlphaCutout, new Vector2I?(definition.TargetSize), definition.Fresnel, definition.ReflectionShadow, definition.Gloss, definition.GlossTextureAdd, definition.SpecularColorFactor));
                if ((definition.TextureType == MyTransparentMaterialTextureType.FileTexture) && (!string.IsNullOrEmpty(definition.Texture) && Path.GetFileNameWithoutExtension(definition.Texture).StartsWith("Atlas_")))
                {
                    texturesToLoad.Add(definition.Texture);
                    textures.Add(definition.Texture);
                }
            }
            MyRenderProxy.PreloadTextures(texturesToLoad, TextureType.Particles);
            MyRenderProxy.AddToParticleTextureArray(textures);
            MyTransparentMaterials.Update();
        }

        private static void FailModLoading(MyModContext context, int phase = -1, int phaseNum = 0, Exception innerException = null)
        {
            string text1;
            if (innerException == null)
            {
                text1 = "";
            }
            else
            {
                string[] textArray1 = new string[] { ", Following Error occured:", VRage.Library.MyEnvironment.NewLine, innerException.Message, VRage.Library.MyEnvironment.NewLine, innerException.Source, VRage.Library.MyEnvironment.NewLine, innerException.StackTrace };
                text1 = string.Concat(textArray1);
            }
            string str = text1;
            if (phase == -1)
            {
                MyDefinitionErrors.Add(context, "MOD SKIPPED, Cannot load definition file" + str, TErrorSeverity.Critical, true);
            }
            else
            {
                MyDefinitionErrors.Add(context, string.Format("MOD PARTIALLY SKIPPED, LOADED ONLY {0}/{1} PHASES" + str, phase + 1, phaseNum), TErrorSeverity.Critical, true);
            }
            if (context.IsBaseGame)
            {
                throw new MyLoadingException(string.Format(MyTexts.GetString(MySpaceTexts.LoadingError_ModifiedOriginalContent), context.CurrentFile, MySession.Platform), innerException);
            }
        }

        private MyBlueprintDefinitionBase FindBlueprintByClassEntry(BlueprintClassEntry blueprintClassEntry)
        {
            if (!blueprintClassEntry.TypeId.IsNull)
            {
                MyDefinitionId blueprintId = new MyDefinitionId(blueprintClassEntry.TypeId, blueprintClassEntry.BlueprintSubtypeId);
                return this.GetBlueprintDefinition(blueprintId);
            }
            MyBlueprintDefinitionBase base2 = null;
            MyDefinitionId key = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), blueprintClassEntry.BlueprintSubtypeId);
            this.m_definitions.m_blueprintsById.TryGetValue(key, out base2);
            if (base2 == null)
            {
                key = new MyDefinitionId(typeof(MyObjectBuilder_CompositeBlueprintDefinition), blueprintClassEntry.BlueprintSubtypeId);
                this.m_definitions.m_blueprintsById.TryGetValue(key, out base2);
            }
            return base2;
        }

        private MyEnvironmentItemDefinition FindEnvironmentItemByEntry(MyEnvironmentItemsDefinition itemsDefinition, EnvironmentItemsEntry envItemEntry)
        {
            MyDefinitionId defId = new MyDefinitionId(itemsDefinition.ItemDefinitionType, envItemEntry.ItemSubtype);
            MyEnvironmentItemDefinition definition = null;
            this.TryGetDefinition<MyEnvironmentItemDefinition>(defId, out definition);
            return definition;
        }

        private void FixGeneratedBlocksIntegrity(DefinitionDictionary<MyCubeBlockDefinition> cubeBlocks)
        {
            foreach (KeyValuePair<MyDefinitionId, MyCubeBlockDefinition> pair in cubeBlocks)
            {
                MyCubeBlockDefinition definition = pair.Value;
                if (definition.GeneratedBlockDefinitions != null)
                {
                    foreach (MyDefinitionId id in definition.GeneratedBlockDefinitions)
                    {
                        MyCubeBlockDefinition definition2;
                        if (this.TryGetCubeBlockDefinition(id, out definition2) && (definition2.GeneratedBlockType != MyStringId.GetOrCompute("pillar")))
                        {
                            definition2.Components = definition.Components;
                            definition2.MaxIntegrity = definition.MaxIntegrity;
                        }
                    }
                }
            }
        }

        public DictionaryValuesReader<MyDefinitionId, MyDefinitionBase> GetAllDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyDefinitionBase>(this.m_definitions.m_definitionsById);

        public MyAmmoDefinition GetAmmoDefinition(MyDefinitionId id) => 
            this.m_definitions.m_ammoDefinitionsById[id];

        public MyAmmoMagazineDefinition GetAmmoMagazineDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyAmmoMagazineDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyAmmoMagazineDefinition);
        }

        public string GetAnimationDefinitionCompatibility(string animationSubtypeName)
        {
            MyDefinitionBase base2;
            string subtypeName = animationSubtypeName;
            if (!Static.TryGetDefinition<MyDefinitionBase>(new MyDefinitionId(typeof(MyObjectBuilder_AnimationDefinition), animationSubtypeName), out base2))
            {
                foreach (MyAnimationDefinition definition in Static.GetAnimationDefinitions())
                {
                    if (definition.AnimationModel == animationSubtypeName)
                    {
                        subtypeName = definition.Id.SubtypeName;
                        break;
                    }
                }
            }
            return subtypeName;
        }

        public ListReader<MyAnimationDefinition> GetAnimationDefinitions() => 
            new ListReader<MyAnimationDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyAnimationDefinition>().ToList<MyAnimationDefinition>());

        public Dictionary<string, MyAnimationDefinition> GetAnimationDefinitions(string skeleton) => 
            this.m_definitions.m_animationsBySkeletonType[skeleton];

        public MyAssetModifierDefinition GetAssetModifierDefinition(MyDefinitionId id)
        {
            MyAssetModifierDefinition definition = null;
            this.m_definitions.m_assetModifiers.TryGetValue(id, out definition);
            return definition;
        }

        public Dictionary<string, MyTextureChange> GetAssetModifierDefinitionForRender(string skinId) => 
            this.GetAssetModifierDefinitionForRender(MyStringHash.GetOrCompute(skinId));

        public Dictionary<string, MyTextureChange> GetAssetModifierDefinitionForRender(MyStringHash skinId)
        {
            Dictionary<string, MyTextureChange> dictionary;
            this.m_definitions.m_assetModifiersForRender.TryGetValue(skinId, out dictionary);
            return dictionary;
        }

        public DictionaryValuesReader<MyDefinitionId, MyAssetModifierDefinition> GetAssetModifierDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyAssetModifierDefinition>(this.m_definitions.m_assetModifiers);

        public DictionaryReader<MyStringHash, Dictionary<string, MyTextureChange>> GetAssetModifierDefinitionsForRender() => 
            new DictionaryReader<MyStringHash, Dictionary<string, MyTextureChange>>(this.m_definitions.m_assetModifiersForRender);

        public DictionaryReader<string, MyAsteroidGeneratorDefinition> GetAsteroidGeneratorDefinitions() => 
            new DictionaryReader<string, MyAsteroidGeneratorDefinition>(this.m_definitions.m_asteroidGenerators);

        internal ListReader<MyAudioEffectDefinition> GetAudioEffectDefinitions() => 
            new ListReader<MyAudioEffectDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyAudioEffectDefinition>().ToList<MyAudioEffectDefinition>());

        public void GetBaseBlockPrefabName(MyCubeSize size, bool isStatic, bool isCreative, out string prefabName)
        {
            prefabName = this.m_definitions.m_basePrefabNames[ComputeBasePrefabIndex(size, isStatic, isCreative)];
        }

        public MyBehaviorDefinition GetBehaviorDefinition(MyDefinitionId id) => 
            this.m_definitions.m_behaviorDefinitions[id];

        public ListReader<MyBehaviorDefinition> GetBehaviorDefinitions() => 
            new ListReader<MyBehaviorDefinition>(this.m_definitions.m_behaviorDefinitions.Values.ToList<MyBehaviorDefinition>());

        public MyBlueprintClassDefinition GetBlueprintClass(string className)
        {
            MyBlueprintClassDefinition definition = null;
            MyDefinitionId key = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintClassDefinition), className);
            this.m_definitions.m_blueprintClasses.TryGetValue(key, out definition);
            return definition;
        }

        public MyBlueprintDefinitionBase GetBlueprintDefinition(MyDefinitionId blueprintId)
        {
            if (this.m_definitions.m_blueprintsById.ContainsKey(blueprintId))
            {
                return this.m_definitions.m_blueprintsById[blueprintId];
            }
            MySandboxGame.Log.WriteLine($"No blueprint with Id '{blueprintId}'");
            return null;
        }

        public DictionaryValuesReader<MyDefinitionId, MyBlueprintDefinitionBase> GetBlueprintDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyBlueprintDefinitionBase>(this.m_definitions.m_blueprintsById);

        public MyBotDefinition GetBotDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyBotDefinition>(ref id);
            return (!this.m_definitions.m_definitionsById.ContainsKey(id) ? null : (this.m_definitions.m_definitionsById[id] as MyBotDefinition));
        }

        public ListReader<MyBotDefinition> GetBotDefinitions() => 
            new ListReader<MyBotDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyBotDefinition>().ToList<MyBotDefinition>());

        public Dictionary<string, MyGuiBlockCategoryDefinition> GetCategories() => 
            this.m_definitions.m_categories;

        public MyCubeBlockDefinition GetComponentBlockDefinition(MyDefinitionId componentDefId)
        {
            MyCubeBlockDefinition definition = null;
            this.m_definitions.m_componentIdToBlock.TryGetValue(componentDefId, out definition);
            return definition;
        }

        public MyComponentDefinition GetComponentDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyComponentDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyComponentDefinition);
        }

        public MyComponentGroupDefinition GetComponentGroup(MyDefinitionId groupDefId)
        {
            MyComponentGroupDefinition definition = null;
            this.m_definitions.m_componentGroups.TryGetValue(groupDefId, out definition);
            return definition;
        }

        public MyDefinitionId GetComponentId(MyCubeBlockDefinition blockDefinition)
        {
            MyCubeBlockDefinition.Component[] components = blockDefinition.Components;
            if ((components != null) && (components.Length != 0))
            {
                return components[0].Definition.Id;
            }
            return new MyDefinitionId();
        }

        public MyDefinitionId GetComponentId(MyDefinitionId defId)
        {
            MyCubeBlockDefinition blockDefinition = null;
            if (this.TryGetCubeBlockDefinition(defId, out blockDefinition))
            {
                return this.GetComponentId(blockDefinition);
            }
            return new MyDefinitionId();
        }

        public MyCompoundBlockTemplateDefinition GetCompoundBlockTemplateDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyCompoundBlockTemplateDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyCompoundBlockTemplateDefinition);
        }

        public ListReader<MyCompoundBlockTemplateDefinition> GetCompoundBlockTemplateDefinitions() => 
            new ListReader<MyCompoundBlockTemplateDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyCompoundBlockTemplateDefinition>().ToList<MyCompoundBlockTemplateDefinition>());

        public MyContainerDefinition GetContainerDefinition(MyDefinitionId containerId) => 
            this.m_definitions.m_entityContainers[containerId];

        public MyContainerTypeDefinition GetContainerTypeDefinition(string containerName)
        {
            MyContainerTypeDefinition definition;
            return (this.m_definitions.m_containerTypeDefinitions.TryGetValue(new MyDefinitionId(typeof(MyObjectBuilder_ContainerTypeDefinition), containerName), out definition) ? definition : null);
        }

        public MyCubeBlockDefinition GetCubeBlockDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyCubeBlockDefinition>(ref id);
            return (!this.m_definitions.m_definitionsById.ContainsKey(id) ? null : (this.m_definitions.m_definitionsById[id] as MyCubeBlockDefinition));
        }

        public MyCubeBlockDefinition GetCubeBlockDefinition(MyObjectBuilder_CubeBlock builder) => 
            this.GetCubeBlockDefinition(builder.GetId());

        public MyCubeBlockDefinition GetCubeBlockDefinitionForMultiBlock(string multiBlock)
        {
            MyCubeBlockDefinition definition;
            return (!this.m_definitions.m_mapMultiBlockDefToCubeBlockDef.TryGetValue(multiBlock, out definition) ? null : definition);
        }

        public Vector2I GetCubeBlockScreenPosition(string pairName)
        {
            Vector2I vectori;
            if (!this.m_definitions.m_blockPositions.TryGetValue(pairName, out vectori))
            {
                vectori = new Vector2I(-1, -1);
            }
            return vectori;
        }

        public float GetCubeSize(MyCubeSize gridSize) => 
            this.m_definitions.m_cubeSizes[(int) gridSize];

        public float GetCubeSizeOriginal(MyCubeSize gridSize) => 
            this.m_definitions.m_cubeSizesOriginal[(int) gridSize];

        public ListReader<MyDebrisDefinition> GetDebrisDefinitions() => 
            new ListReader<MyDebrisDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyDebrisDefinition>().ToList<MyDebrisDefinition>());

        public List<MyFactionDefinition> GetDefaultFactions()
        {
            List<MyFactionDefinition> list = new List<MyFactionDefinition>();
            foreach (MyFactionDefinition definition in this.m_definitions.m_factionDefinitionsByTag.Values)
            {
                if (definition.IsDefault)
                {
                    list.Add(definition);
                }
            }
            return list;
        }

        public MyVoxelMaterialDefinition GetDefaultVoxelMaterialDefinition() => 
            this.m_definitions.m_voxelMaterialsByIndex[0];

        public void GetDefinedEntityComponents(ref List<MyDefinitionId> definedComponents)
        {
            foreach (KeyValuePair<MyDefinitionId, MyComponentDefinitionBase> pair in this.m_definitions.m_entityComponentDefinitions)
            {
                definedComponents.Add(pair.Key);
            }
        }

        public void GetDefinedEntityContainers(ref List<MyDefinitionId> definedContainers)
        {
            foreach (KeyValuePair<MyDefinitionId, MyContainerDefinition> pair in this.m_definitions.m_entityContainers)
            {
                definedContainers.Add(pair.Key);
            }
        }

        public MyDefinitionBase GetDefinition(MyDefinitionId id)
        {
            MyDefinitionBase base3;
            MyDefinitionBase definition = base.GetDefinition<MyDefinitionBase>(id);
            if (definition != null)
            {
                return definition;
            }
            this.CheckDefinition(ref id);
            return (!this.m_definitions.m_definitionsById.TryGetValue(id, out base3) ? new MyDefinitionBase() : base3);
        }

        private List<Tuple<MyObjectBuilder_Definitions, string>> GetDefinitionBuilders(MyModContext context, HashSet<string> preloadSet = null)
        {
            ConcurrentBag<Tuple<MyObjectBuilder_Definitions, string>> definitionBuilders = new ConcurrentBag<Tuple<MyObjectBuilder_Definitions, string>>();
            ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();
            System.Threading.Tasks.Parallel.ForEach<string>((IEnumerable<string>) (from f in MyFileSystem.GetFiles(context.ModPathData, "*.sbc", MySearchOption.AllDirectories)
                where f.EndsWith(".sbc")
                select f), delegate (string file) {
                if (((preloadSet == null) || preloadSet.Contains(Path.GetFileName(file))) && (Path.GetFileName(file) != "DefinitionsToPreload.sbc"))
                {
                    MyObjectBuilder_Definitions definitions = null;
                    if ((ReferenceEquals(context, MyModContext.BaseGame) && (preloadSet == null)) && this.m_preloadedDefinitionBuilders.TryGetValue(file, out definitions))
                    {
                        definitionBuilders.Add(new Tuple<MyObjectBuilder_Definitions, string>(definitions, file));
                    }
                    else
                    {
                        context.CurrentFile = file;
                        try
                        {
                            definitions = CheckPrefabs(file);
                            this.ReSavePrefabsProtoBuffers(definitions);
                        }
                        catch (Exception exception)
                        {
                            FailModLoading(context, -1, 0, exception);
                            exceptions.Enqueue(exception);
                        }
                        if (definitions == null)
                        {
                            definitions = this.Load<MyObjectBuilder_Definitions>(file);
                        }
                        if (definitions == null)
                        {
                            FailModLoading(context, -1, 0, null);
                        }
                        else
                        {
                            Tuple<MyObjectBuilder_Definitions, string> item = new Tuple<MyObjectBuilder_Definitions, string>(definitions, file);
                            definitionBuilders.Add(item);
                            if ((ReferenceEquals(context, MyModContext.BaseGame) && (preloadSet == null)) && MyFakes.ENABLE_PRELOAD_DEFINITIONS)
                            {
                                this.m_preloadedDefinitionBuilders.TryAdd(file, definitions);
                            }
                        }
                    }
                }
            });
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
            List<Tuple<MyObjectBuilder_Definitions, string>> list1 = definitionBuilders.ToList<Tuple<MyObjectBuilder_Definitions, string>>();
            List<Tuple<MyObjectBuilder_Definitions, string>> list2 = definitionBuilders.ToList<Tuple<MyObjectBuilder_Definitions, string>>();
            list2.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return list2;
        }

        public MyCubeBlockDefinitionGroup GetDefinitionGroup(string groupName) => 
            this.m_definitions.m_blockGroups[groupName];

        public DictionaryKeysReader<string, MyCubeBlockDefinitionGroup> GetDefinitionPairNames() => 
            new DictionaryKeysReader<string, MyCubeBlockDefinitionGroup>(this.m_definitions.m_blockGroups);

        public Dictionary<string, MyCubeBlockDefinitionGroup> GetDefinitionPairs() => 
            this.m_definitions.m_blockGroups;

        public ListReader<T> GetDefinitionsOfType<T>() where T: MyDefinitionBase => 
            new ListReader<T>(this.m_definitions.m_definitionsById.Values.OfType<T>().ToList<T>());

        public MyDropContainerDefinition GetDropContainerDefinition(string id)
        {
            MyDropContainerDefinition definition;
            this.m_definitions.m_dropContainers.TryGetValue(id, out definition);
            return ((definition != null) ? ((definition.Prefab != null) ? definition : null) : null);
        }

        public DictionaryReader<string, MyDropContainerDefinition> GetDropContainerDefinitions() => 
            new DictionaryReader<string, MyDropContainerDefinition>(this.m_definitions.m_dropContainers);

        public MyEdgesDefinition GetEdgesDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyEdgesDefinition>(ref id);
            return (MyEdgesDefinition) this.m_definitions.m_definitionsById[id];
        }

        public ListReader<MyEdgesDefinition> GetEdgesDefinitions() => 
            new ListReader<MyEdgesDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyEdgesDefinition>().ToList<MyEdgesDefinition>());

        public MyComponentDefinitionBase GetEntityComponentDefinition(MyDefinitionId componentId) => 
            this.m_definitions.m_entityComponentDefinitions[componentId];

        public ListReader<MyComponentDefinitionBase> GetEntityComponentDefinitions() => 
            this.GetEntityComponentDefinitions<MyComponentDefinitionBase>();

        public ListReader<T> GetEntityComponentDefinitions<T>() => 
            new ListReader<T>(this.m_definitions.m_entityComponentDefinitions.Values.OfType<T>().ToList<T>());

        public ListReader<MyEnvironmentItemsDefinition> GetEnvironmentItemClassDefinitions() => 
            new ListReader<MyEnvironmentItemsDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyEnvironmentItemsDefinition>().ToList<MyEnvironmentItemsDefinition>());

        public MyEnvironmentItemDefinition GetEnvironmentItemDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyEnvironmentItemDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyEnvironmentItemDefinition);
        }

        public ListReader<MyEnvironmentItemDefinition> GetEnvironmentItemDefinitions() => 
            new ListReader<MyEnvironmentItemDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyEnvironmentItemDefinition>().ToList<MyEnvironmentItemDefinition>());

        public ListReader<MyDefinitionId> GetEnvironmentItemsDefinitions(int channel)
        {
            List<MyDefinitionId> list = null;
            this.m_definitions.m_channelEnvironmentItemsDefs.TryGetValue(channel, out list);
            return list;
        }

        public MyGlobalEventDefinition GetEventDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyGlobalEventDefinition>(ref id);
            MyDefinitionBase base2 = null;
            this.m_definitions.m_definitionsById.TryGetValue(id, out base2);
            return (MyGlobalEventDefinition) base2;
        }

        public string GetFirstRespawnShip()
        {
            if (this.m_definitions.m_respawnShips.Count > 0)
            {
                return this.m_definitions.m_respawnShips.FirstOrDefault<KeyValuePair<string, MyRespawnShipDefinition>>().Value.Id.SubtypeName;
            }
            return null;
        }

        public MyGridCreateToolDefinition GetGridCreator(MyStringHash name)
        {
            MyGridCreateToolDefinition definition;
            this.m_definitions.m_gridCreateDefinitions.TryGetValue(new MyDefinitionId(typeof(MyObjectBuilder_GridCreateToolDefinition), name), out definition);
            return definition;
        }

        public IEnumerable<MyGridCreateToolDefinition> GetGridCreatorDefinitions() => 
            this.m_definitions.m_gridCreateDefinitions.Values;

        public DictionaryValuesReader<string, MyGroupedIds> GetGroupedIds(string superGroup) => 
            new DictionaryValuesReader<string, MyGroupedIds>(this.m_definitions.m_groupedIds[superGroup]);

        public MyComponentGroupDefinition GetGroupForComponent(MyDefinitionId componentDefId, out int amount)
        {
            MyTuple<int, MyComponentGroupDefinition> tuple;
            if (this.m_definitions.m_componentGroupMembers.TryGetValue(componentDefId, out tuple))
            {
                amount = tuple.Item1;
                return tuple.Item2;
            }
            amount = 0;
            return null;
        }

        public DictionaryValuesReader<MyDefinitionId, MyHandItemDefinition> GetHandItemDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyHandItemDefinition>(this.m_definitions.m_handItemsById);

        public ListReader<MyLCDTextureDefinition> GetLCDTexturesDefinitions() => 
            new ListReader<MyLCDTextureDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyLCDTextureDefinition>().ToList<MyLCDTextureDefinition>());

        public override MyDefinitionSet GetLoadingSet() => 
            this.LoadingSet;

        public MyLootBagDefinition GetLootBagDefinition() => 
            this.m_definitions.m_lootBagDefinition;

        public DictionaryValuesReader<MyDefinitionId, MyMainMenuInventorySceneDefinition> GetMainMenuInventoryScenes() => 
            new DictionaryValuesReader<MyDefinitionId, MyMainMenuInventorySceneDefinition>(this.m_definitions.m_mainMenuInventoryScenes);

        public MyMultiBlockDefinition GetMultiBlockDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyMultiBlockDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyMultiBlockDefinition);
        }

        public ListReader<MyMultiBlockDefinition> GetMultiBlockDefinitions() => 
            new ListReader<MyMultiBlockDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyMultiBlockDefinition>().ToList<MyMultiBlockDefinition>());

        public MyObjectBuilder_DefinitionBase GetObjectBuilder(MyDefinitionBase definition) => 
            GetObjectFactory().CreateObjectBuilder<MyObjectBuilder_DefinitionBase>(definition);

        public void GetOreTypeNames(out string[] outNames)
        {
            List<string> list = new List<string>();
            foreach (MyDefinitionBase base2 in this.m_definitions.m_definitionsById.Values)
            {
                if (base2.Id.TypeId == typeof(MyObjectBuilder_Ore))
                {
                    list.Add(base2.Id.SubtypeName);
                }
            }
            outNames = list.ToArray();
        }

        public MyPhysicalItemDefinition GetPhysicalItemDefinition(MyDefinitionId id)
        {
            MyDefinitionBase base2;
            if (!this.m_definitions.m_definitionsById.ContainsKey(id))
            {
                MyLog.Default.Critical(new StringBuilder($"Definition of "{id.ToString()}" is missing."));
            }
            this.CheckDefinition<MyPhysicalItemDefinition>(ref id);
            return (!this.m_definitions.m_definitionsById.TryGetValue(id, out base2) ? null : (this.m_definitions.m_definitionsById[id] as MyPhysicalItemDefinition));
        }

        public MyPhysicalItemDefinition GetPhysicalItemDefinition(MyObjectBuilder_Base objectBuilder) => 
            this.GetPhysicalItemDefinition(objectBuilder.GetId());

        public ListReader<MyPhysicalItemDefinition> GetPhysicalItemDefinitions() => 
            new ListReader<MyPhysicalItemDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyPhysicalItemDefinition>().ToList<MyPhysicalItemDefinition>());

        public MyPhysicalItemDefinition GetPhysicalItemForHandItem(MyDefinitionId handItemId) => 
            (this.m_definitions.m_physicalItemsByHandItemId.ContainsKey(handItemId) ? this.m_definitions.m_physicalItemsByHandItemId[handItemId] : null);

        public MyPhysicalMaterialDefinition GetPhysicalMaterialDefinition(string name)
        {
            MyPhysicalMaterialDefinition definition = null;
            this.m_definitions.m_physicalMaterialsByName.TryGetValue(name, out definition);
            return definition;
        }

        public MyPhysicalMaterialDefinition GetPhysicalMaterialDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyPhysicalMaterialDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyPhysicalMaterialDefinition);
        }

        public ListReader<MyPhysicalMaterialDefinition> GetPhysicalMaterialDefinitions() => 
            new ListReader<MyPhysicalMaterialDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyPhysicalMaterialDefinition>().ToList<MyPhysicalMaterialDefinition>());

        public DictionaryValuesReader<MyDefinitionId, MyPirateAntennaDefinition> GetPirateAntennaDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyPirateAntennaDefinition>(this.m_definitions.m_pirateAntennaDefinitions);

        public IEnumerable<MyPlanetGeneratorDefinition> GetPlanetsGeneratorsDefinitions() => 
            this.m_definitions.GetDefinitionsOfType<MyPlanetGeneratorDefinition>();

        public DictionaryValuesReader<MyDefinitionId, MyPlanetPrefabDefinition> GetPlanetsPrefabsDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyPlanetPrefabDefinition>(this.m_definitions.m_planetPrefabDefinitions);

        public MyPrefabDefinition GetPrefabDefinition(string id)
        {
            MyPrefabDefinition definition;
            this.m_definitions.m_prefabs.TryGetValue(id, out definition);
            return definition;
        }

        public DictionaryReader<string, MyPrefabDefinition> GetPrefabDefinitions() => 
            new DictionaryReader<string, MyPrefabDefinition>(this.m_definitions.m_prefabs);

        public ListReader<MyPrefabThrowerDefinition> GetPrefabThrowerDefinitions() => 
            new ListReader<MyPrefabThrowerDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyPrefabThrowerDefinition>().ToList<MyPrefabThrowerDefinition>());

        private HashSet<string> GetPreloadSet()
        {
            HashSet<string> set = new HashSet<string>();
            string path = Path.Combine(MyModContext.BaseGame.ModPathData, "DefinitionsToPreload.sbc");
            if (!MyFileSystem.FileExists(path))
            {
                return null;
            }
            MyObjectBuilder_Definitions definitions = this.Load<MyObjectBuilder_Definitions>(path);
            if (definitions == null)
            {
                return null;
            }
            if (definitions.Definitions == null)
            {
                return null;
            }
            foreach (MyObjectBuilder_PreloadFileInfo info in ((MyObjectBuilder_DefinitionsToPreload) definitions.Definitions[0]).DefinitionFiles)
            {
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    set.Add(info.Name);
                }
                else if (info.LoadOnDedicated)
                {
                    set.Add(info.Name);
                }
            }
            return set;
        }

        public string GetRandomCharacterName()
        {
            if (this.m_definitions.m_characterNames.Count == 0)
            {
                return "";
            }
            int randomInt = MyUtils.GetRandomInt(this.m_definitions.m_characterNames.Count);
            return this.m_definitions.m_characterNames[randomInt].Name;
        }

        public MyEnvironmentItemsDefinition GetRandomEnvironmentClass(int channel)
        {
            MyEnvironmentItemsDefinition definition = null;
            List<MyDefinitionId> list = null;
            this.m_definitions.m_channelEnvironmentItemsDefs.TryGetValue(channel, out list);
            if (list != null)
            {
                MyDefinitionId defId = list[MyRandom.Instance.Next(0, list.Count)];
                Static.TryGetDefinition<MyEnvironmentItemsDefinition>(defId, out definition);
            }
            return definition;
        }

        public MyResearchBlockDefinition GetResearchBlock(MyDefinitionId id)
        {
            MyResearchBlockDefinition definition = null;
            this.m_definitions.m_researchBlocksDefinitions.TryGetValue(id, out definition);
            return definition;
        }

        public DictionaryValuesReader<MyDefinitionId, MyResearchBlockDefinition> GetResearchBlockDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyResearchBlockDefinition>(this.m_definitions.m_researchBlocksDefinitions);

        public MyResearchGroupDefinition GetResearchGroup(string subtype)
        {
            MyDefinitionId key = new MyDefinitionId(typeof(MyObjectBuilder_ResearchGroupDefinition), subtype);
            MyResearchGroupDefinition definition = null;
            this.m_definitions.m_researchGroupsDefinitions.TryGetValue(key, out definition);
            return definition;
        }

        public DictionaryValuesReader<MyDefinitionId, MyResearchGroupDefinition> GetResearchGroupDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyResearchGroupDefinition>(this.m_definitions.m_researchGroupsDefinitions);

        public MyRespawnShipDefinition GetRespawnShipDefinition(string id)
        {
            MyRespawnShipDefinition definition;
            this.m_definitions.m_respawnShips.TryGetValue(id, out definition);
            return ((definition != null) ? ((definition.Prefab != null) ? definition : null) : null);
        }

        public DictionaryReader<string, MyRespawnShipDefinition> GetRespawnShipDefinitions() => 
            new DictionaryReader<string, MyRespawnShipDefinition>(this.m_definitions.m_respawnShips);

        public MyRopeDefinition GetRopeDefinition(MyDefinitionId ropeDefId)
        {
            if (!this.m_definitions.m_idToRope.ContainsKey(ropeDefId))
            {
                MySandboxGame.Log.WriteLine($"No rope definition found '{ropeDefId}'");
            }
            return this.m_definitions.m_idToRope[ropeDefId];
        }

        public DictionaryValuesReader<MyDefinitionId, MyRopeDefinition> GetRopeDefinitions() => 
            this.m_definitions.m_idToRope;

        public MyScenarioDefinition GetScenarioDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyScenarioDefinition>(ref id);
            return (MyScenarioDefinition) this.m_definitions.m_definitionsById[id];
        }

        public ListReader<MyScenarioDefinition> GetScenarioDefinitions() => 
            new ListReader<MyScenarioDefinition>(this.m_definitions.m_scenarioDefinitions);

        public DictionaryValuesReader<MyDefinitionId, MyScriptedGroupDefinition> GetScriptedGroupDefinitions() => 
            new DictionaryValuesReader<MyDefinitionId, MyScriptedGroupDefinition>(this.m_definitions.m_scriptedGroupDefinitions);

        public MyShipSoundsDefinition GetShipSoundsDefinition(MyDefinitionId id)
        {
            this.CheckDefinition<MyShipSoundsDefinition>(ref id);
            return (this.m_definitions.m_definitionsById[id] as MyShipSoundsDefinition);
        }

        public ListReader<MySoundCategoryDefinition> GetSoundCategoryDefinitions() => 
            new ListReader<MySoundCategoryDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MySoundCategoryDefinition>().ToList<MySoundCategoryDefinition>());

        public MyAudioDefinition GetSoundDefinition(MyStringHash subtypeId) => 
            this.m_definitions.m_sounds[new MyDefinitionId(typeof(MyObjectBuilder_AudioDefinition), subtypeId)];

        public DictionaryValuesReader<MyDefinitionId, MyAudioDefinition> GetSoundDefinitions() => 
            this.m_definitions.m_sounds;

        public MySpawnGroupDefinition GetSpawnGroupDefinition(int index) => 
            this.m_definitions.m_spawnGroupDefinitions[index];

        public ListReader<MySpawnGroupDefinition> GetSpawnGroupDefinitions() => 
            new ListReader<MySpawnGroupDefinition>(this.m_definitions.m_spawnGroupDefinitions);

        public ListReader<MyTransparentMaterialDefinition> GetTransparentMaterialDefinitions() => 
            new ListReader<MyTransparentMaterialDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyTransparentMaterialDefinition>().ToList<MyTransparentMaterialDefinition>());

        public ListReader<MyVoxelHandDefinition> GetVoxelHandDefinitions() => 
            new ListReader<MyVoxelHandDefinition>(this.m_definitions.m_definitionsById.Values.OfType<MyVoxelHandDefinition>().ToList<MyVoxelHandDefinition>());

        public ListReader<MyVoxelMapStorageDefinition> GetVoxelMapStorageDefinitions() => 
            new ListReader<MyVoxelMapStorageDefinition>(this.m_definitions.m_voxelMapStorages.Values.ToList<MyVoxelMapStorageDefinition>());

        public ListReader<MyVoxelMapStorageDefinition> GetVoxelMapStorageDefinitionsForProceduralAdditions() => 
            this.m_voxelMapStorageDefinitionsForProceduralAdditions.Value;

        public ListReader<MyVoxelMapStorageDefinition> GetVoxelMapStorageDefinitionsForProceduralPrimaryAdditions() => 
            this.m_voxelMapStorageDefinitionsForProceduralPrimaryAdditions.Value;

        public ListReader<MyVoxelMapStorageDefinition> GetVoxelMapStorageDefinitionsForProceduralRemovals() => 
            this.m_voxelMapStorageDefinitionsForProceduralRemovals.Value;

        public MyVoxelMaterialDefinition GetVoxelMaterialDefinition(byte materialIndex)
        {
            using (this.m_voxelMaterialsLock.AcquireSharedUsing())
            {
                MyVoxelMaterialDefinition definition = null;
                this.m_definitions.m_voxelMaterialsByIndex.TryGetValue(materialIndex, out definition);
                return definition;
            }
        }

        public MyVoxelMaterialDefinition GetVoxelMaterialDefinition(string name)
        {
            using (this.m_voxelMaterialsLock.AcquireSharedUsing())
            {
                MyVoxelMaterialDefinition definition = null;
                this.m_definitions.m_voxelMaterialsByName.TryGetValue(name, out definition);
                return definition;
            }
        }

        public DictionaryValuesReader<string, MyVoxelMaterialDefinition> GetVoxelMaterialDefinitions()
        {
            using (this.m_voxelMaterialsLock.AcquireSharedUsing())
            {
                return this.m_definitions.m_voxelMaterialsByName;
            }
        }

        public MyWeaponDefinition GetWeaponDefinition(MyDefinitionId id) => 
            this.m_definitions.m_weaponDefinitionsById[id];

        public ListReader<MyPhysicalItemDefinition> GetWeaponDefinitions() => 
            new ListReader<MyPhysicalItemDefinition>(this.m_definitions.m_physicalItemDefinitions);

        public DictionaryReader<string, MyWheelModelsDefinition> GetWheelModelDefinitions() => 
            new DictionaryReader<string, MyWheelModelsDefinition>(this.m_definitions.m_wheelModels);

        public bool HandItemExistsFor(MyDefinitionId physicalItemId) => 
            this.m_definitions.m_handItemsByPhysicalItemId.ContainsKey(physicalItemId);

        public bool HasBlueprint(MyDefinitionId blueprintId) => 
            this.m_definitions.m_blueprintsById.ContainsKey(blueprintId);

        public bool HasRespawnShip(string id) => 
            this.m_definitions.m_respawnShips.ContainsKey(id);

        private void InitAIBehaviors(MyModContext context, DefinitionDictionary<MyBehaviorDefinition> output, MyObjectBuilder_DefinitionBase[] items, bool failOnDebug)
        {
            MyBehaviorDefinition[] definitionArray = new MyBehaviorDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyBehaviorDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitAmmoMagazines(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_AmmoMagazineDefinition[] magazines, bool failOnDebug = true)
        {
            MyAmmoMagazineDefinition[] definitionArray = new MyAmmoMagazineDefinition[magazines.Length];
            for (int i = 0; i < magazines.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyAmmoMagazineDefinition>(context, magazines[i]);
                Check<MyObjectBuilderType>(definitionArray[i].Id.TypeId == typeof(MyObjectBuilder_AmmoMagazine), definitionArray[i].Id.TypeId, failOnDebug, "Unknown type '{0}'");
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitAmmos(MyModContext context, DefinitionDictionary<MyAmmoDefinition> output, MyObjectBuilder_AmmoDefinition[] ammos, bool failOnDebug = true)
        {
            MyAmmoDefinition[] definitionArray = new MyAmmoDefinition[ammos.Length];
            for (int i = 0; i < ammos.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyAmmoDefinition>(context, ammos[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitAnimations(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_AnimationDefinition[] animations, Dictionary<string, Dictionary<string, MyAnimationDefinition>> animationsBySkeletonType, bool failOnDebug = true)
        {
            MyAnimationDefinition[] definitionArray = new MyAnimationDefinition[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyAnimationDefinition>(context, animations[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
                bool isBaseGame = context.IsBaseGame;
                Static.m_currentLoadingSet.AddOrRelaceDefinition(definitionArray[i]);
            }
            MyAnimationDefinition[] definitionArray2 = definitionArray;
            int index = 0;
            while (index < definitionArray2.Length)
            {
                MyAnimationDefinition definition = definitionArray2[index];
                string[] supportedSkeletons = definition.SupportedSkeletons;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= supportedSkeletons.Length)
                    {
                        index++;
                        break;
                    }
                    string key = supportedSkeletons[num3];
                    if (!animationsBySkeletonType.ContainsKey(key))
                    {
                        animationsBySkeletonType.Add(key, new Dictionary<string, MyAnimationDefinition>());
                    }
                    animationsBySkeletonType[key][definition.Id.SubtypeName] = definition;
                    num3++;
                }
            }
        }

        private static void InitAssetModifiers(MyModContext context, DefinitionDictionary<MyAssetModifierDefinition> output, MyObjectBuilder_AssetModifierDefinition[] items, bool failOnDebug = true)
        {
            MyAssetModifierDefinition[] definitionArray = new MyAssetModifierDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyAssetModifierDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitAssetModifiersForRender()
        {
            this.m_definitions.m_assetModifiersForRender = new Dictionary<MyStringHash, Dictionary<string, MyTextureChange>>();
            foreach (KeyValuePair<MyDefinitionId, MyAssetModifierDefinition> pair in this.m_definitions.m_assetModifiers)
            {
                Dictionary<string, MyTextureChange> dictionary = new Dictionary<string, MyTextureChange>();
                foreach (MyObjectBuilder_AssetModifierDefinition.MyAssetTexture texture in pair.Value.Textures)
                {
                    MyTextureChange change;
                    dictionary.TryGetValue(texture.Location, out change);
                    MyTextureType type = texture.Type;
                    switch (type)
                    {
                        case MyTextureType.ColorMetal:
                            change.ColorMetalFileName = texture.Filepath;
                            break;

                        case MyTextureType.NormalGloss:
                            change.NormalGlossFileName = texture.Filepath;
                            break;

                        case (MyTextureType.NormalGloss | MyTextureType.ColorMetal):
                            break;

                        case MyTextureType.Extensions:
                            change.ExtensionsFileName = texture.Filepath;
                            break;

                        default:
                            if (type == MyTextureType.Alphamask)
                            {
                                change.AlphamaskFileName = texture.Filepath;
                            }
                            break;
                    }
                    dictionary[texture.Location] = change;
                }
                this.m_definitions.m_assetModifiersForRender.Add(pair.Key.SubtypeId, dictionary);
            }
        }

        private static void InitAsteroidGenerators(MyModContext context, Dictionary<string, MyAsteroidGeneratorDefinition> outputDefinitions, MyObjectBuilder_AsteroidGeneratorDefinition[] asteroidGenerators, bool failOnDebug)
        {
            foreach (MyObjectBuilder_AsteroidGeneratorDefinition definition in asteroidGenerators)
            {
                int num2;
                if (!int.TryParse(definition.Id.SubtypeId, out num2))
                {
                    Check<string>(false, definition.Id.SubtypeId, failOnDebug, "Asteroid generator SubtypeId has to be number.");
                }
                else
                {
                    MyAsteroidGeneratorDefinition definition2 = InitDefinition<MyAsteroidGeneratorDefinition>(context, definition);
                    string subtypeName = definition2.Id.SubtypeName;
                    Check<string>(!outputDefinitions.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                    outputDefinitions[subtypeName] = definition2;
                }
            }
        }

        private void InitAudioEffects(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_AudioEffectDefinition[] audioEffects, bool failOnDebug)
        {
            foreach (MyObjectBuilder_AudioEffectDefinition definition in audioEffects)
            {
                MyAudioEffectDefinition definition2 = InitDefinition<MyAudioEffectDefinition>(context, definition);
                MyDefinitionId identifier = definition2.Id;
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(identifier), identifier, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions.AddDefinitionSafe<MyAudioEffectDefinition>(definition2, context, "<AudioEffect>", false);
            }
        }

        private static void InitBattle(MyModContext context, ref MyBattleDefinition output, MyObjectBuilder_BattleDefinition objBuilder, bool failOnDebug = true)
        {
            MyBattleDefinition definition = InitDefinition<MyBattleDefinition>(context, objBuilder);
            output = definition;
        }

        private void InitBlockGroups()
        {
            this.m_definitions.m_blockGroups = new Dictionary<string, MyCubeBlockDefinitionGroup>();
            for (int i = 0; i < this.m_definitions.m_cubeSizes.Length; i++)
            {
                foreach (KeyValuePair<MyDefinitionId, MyCubeBlockDefinition> pair in this.m_definitions.m_uniqueCubeBlocksBySize[i])
                {
                    MyCubeBlockDefinition definition = pair.Value;
                    MyCubeBlockDefinitionGroup group = null;
                    if (!this.m_definitions.m_blockGroups.TryGetValue(definition.BlockPairName, out group))
                    {
                        group = new MyCubeBlockDefinitionGroup();
                        this.m_definitions.m_blockGroups.Add(definition.BlockPairName, group);
                    }
                    group[(MyCubeSize) ((byte) i)] = definition;
                }
            }
        }

        private void InitBlockPositions(Dictionary<string, Vector2I> outputBlockPositions, MyBlockPosition[] positions, bool failOnDebug = true)
        {
            foreach (MyBlockPosition position in positions)
            {
                Check<string>(!outputBlockPositions.ContainsKey(position.Name), position.Name, failOnDebug, "Duplicate entry of '{0}'");
                outputBlockPositions[position.Name] = position.Position;
            }
        }

        private void InitBlueprintClassEntries(MyModContext context, HashSet<BlueprintClassEntry> output, BlueprintClassEntry[] entries, bool failOnDebug = true)
        {
            foreach (BlueprintClassEntry entry in entries)
            {
                Check<BlueprintClassEntry>(!output.Contains(entry), entry, failOnDebug, "Duplicate entry of '{0}'");
                output.Add(entry);
            }
        }

        private void InitBlueprintClasses(MyModContext context, DefinitionDictionary<MyBlueprintClassDefinition> output, MyObjectBuilder_BlueprintClassDefinition[] classes, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_BlueprintClassDefinition definition in classes)
            {
                MyBlueprintClassDefinition definition2 = InitDefinition<MyBlueprintClassDefinition>(context, definition);
                Check<SerializableDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definition.Id] = definition2;
            }
        }

        private void InitBlueprints(MyModContext context, Dictionary<MyDefinitionId, MyBlueprintDefinitionBase> output, DefinitionDictionary<MyBlueprintDefinitionBase> blueprintsByResult, MyObjectBuilder_BlueprintDefinition[] blueprints, bool failOnDebug = true)
        {
            for (int i = 0; i < blueprints.Length; i++)
            {
                MyBlueprintDefinitionBase base2 = InitDefinition<MyBlueprintDefinitionBase>(context, blueprints[i]);
                Check<MyDefinitionId>(!output.ContainsKey(base2.Id), base2.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[base2.Id] = base2;
                if (base2.Results.Length == 1)
                {
                    MyBlueprintDefinitionBase base3;
                    bool flag = true;
                    MyDefinitionId key = base2.Results[0].Id;
                    if (blueprintsByResult.TryGetValue(key, out base3))
                    {
                        if (base3.IsPrimary != base2.IsPrimary)
                        {
                            if (base3.IsPrimary)
                            {
                                flag = false;
                            }
                        }
                        else
                        {
                            string str = base2.IsPrimary ? "primary" : "non-primary";
                            object[] objArray1 = new object[9];
                            objArray1[0] = "Overriding ";
                            objArray1[1] = str;
                            objArray1[2] = " blueprint \"";
                            objArray1[3] = base3;
                            objArray1[4] = "\" with ";
                            objArray1[5] = str;
                            objArray1[6] = " blueprint \"";
                            objArray1[7] = base2;
                            objArray1[8] = "\"";
                            string msg = string.Concat(objArray1);
                            MySandboxGame.Log.WriteLine(msg);
                        }
                    }
                    if (flag)
                    {
                        blueprintsByResult[key] = base2;
                    }
                }
            }
        }

        private void InitBotCommands(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_AiCommandDefinition[] commands, bool failOnDebug = true)
        {
            MyAiCommandDefinition[] definitionArray = new MyAiCommandDefinition[commands.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyAiCommandDefinition>(context, commands[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitBots(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_BotDefinition[] bots, bool failOnDebug = true)
        {
            MyBotDefinition[] definitionArray = new MyBotDefinition[bots.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyBotDefinition>(context, bots[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitCategoryClasses(MyModContext context, List<MyGuiBlockCategoryDefinition> categories, MyObjectBuilder_GuiBlockCategoryDefinition[] classes, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_GuiBlockCategoryDefinition definition in classes)
            {
                if (definition.Public || MyFakes.ENABLE_NON_PUBLIC_CATEGORY_CLASSES)
                {
                    MyGuiBlockCategoryDefinition item = InitDefinition<MyGuiBlockCategoryDefinition>(context, definition);
                    categories.Add(item);
                }
            }
        }

        private void InitCharacterNames(MyModContext context, List<MyCharacterName> output, MyCharacterName[] names, bool failOnDebug)
        {
            foreach (MyCharacterName name in names)
            {
                output.Add(name);
            }
        }

        private static void InitCharacters(MyModContext context, Dictionary<string, MyCharacterDefinition> outputCharacters, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_CharacterDefinition[] characters, bool failOnDebug = true)
        {
            MyCharacterDefinition[] definitionArray = new MyCharacterDefinition[characters.Length];
            for (int i = 0; i < characters.Length; i++)
            {
                if (typeof(MyObjectBuilder_CharacterDefinition).IsAssignableFrom((Type) characters[i].Id.TypeId))
                {
                    characters[i].Id.TypeId = typeof(MyObjectBuilder_Character);
                }
                definitionArray[i] = InitDefinition<MyCharacterDefinition>(context, characters[i]);
                if (definitionArray[i].Id.TypeId.IsNull)
                {
                    MySandboxGame.Log.WriteLine("Invalid character Id found in mod !");
                    MyDefinitionErrors.Add(context, "Invalid character Id found in mod ! ", TErrorSeverity.Error, true);
                }
                else
                {
                    Check<string>(!outputCharacters.ContainsKey(definitionArray[i].Name), definitionArray[i].Name, failOnDebug, "Duplicate entry of '{0}'");
                    outputCharacters[definitionArray[i].Name] = definitionArray[i];
                    Check<string>(!outputDefinitions.ContainsKey(characters[i].Id), definitionArray[i].Name, failOnDebug, "Duplicate entry of '{0}'");
                    outputDefinitions[characters[i].Id] = definitionArray[i];
                }
            }
        }

        private void InitComponentBlocks(MyModContext context, HashSet<MyComponentBlockEntry> output, MyComponentBlockEntry[] objects, bool failOnDebug = true)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                MyComponentBlockEntry identifier = objects[i];
                Check<MyComponentBlockEntry>(!output.Contains(identifier), identifier, failOnDebug, "Duplicate entry of '{0}'");
                output.Add(identifier);
            }
        }

        private static void InitComponentGroups(MyModContext context, DefinitionDictionary<MyComponentGroupDefinition> output, MyObjectBuilder_ComponentGroupDefinition[] objects, bool failOnDebug = true)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                MyComponentGroupDefinition definition = InitDefinition<MyComponentGroupDefinition>(context, objects[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definition.Id] = definition;
            }
        }

        private static void InitComponents(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_ComponentDefinition[] components, bool failOnDebug = true)
        {
            MyComponentDefinition[] definitionArray = new MyComponentDefinition[components.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyComponentDefinition>(context, components[i]);
                Check<MyObjectBuilderType>(definitionArray[i].Id.TypeId == typeof(MyObjectBuilder_Component), definitionArray[i].Id.TypeId, failOnDebug, "Unknown type '{0}'");
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
                if (!context.IsBaseGame)
                {
                    MySandboxGame.Log.WriteLine("Loaded component: " + definitionArray[i].Id);
                }
            }
        }

        private static void InitComponentSubstitutions(MyModContext context, Dictionary<MyDefinitionId, MyComponentSubstitutionDefinition> output, MyObjectBuilder_ComponentSubstitutionDefinition[] objects, bool failOnDebug = true)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                MyComponentSubstitutionDefinition definition = InitDefinition<MyComponentSubstitutionDefinition>(context, objects[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definition.Id] = definition;
            }
        }

        private void InitConfiguration(DefinitionSet definitionSet, MyObjectBuilder_Configuration configuration)
        {
            definitionSet.m_cubeSizes[1] = configuration.CubeSizes.Small;
            definitionSet.m_cubeSizes[0] = configuration.CubeSizes.Large;
            definitionSet.m_cubeSizesOriginal[1] = (configuration.CubeSizes.SmallOriginal > 0f) ? configuration.CubeSizes.SmallOriginal : configuration.CubeSizes.Small;
            definitionSet.m_cubeSizesOriginal[0] = configuration.CubeSizes.Large;
            for (int i = 0; i < 2; i++)
            {
                bool isCreative = i == 0;
                MyObjectBuilder_Configuration.BaseBlockSettings settings = isCreative ? configuration.BaseBlockPrefabs : configuration.BaseBlockPrefabsSurvival;
                this.AddBasePrefabName(definitionSet, MyCubeSize.Small, true, isCreative, settings.SmallStatic);
                this.AddBasePrefabName(definitionSet, MyCubeSize.Small, false, isCreative, settings.SmallDynamic);
                this.AddBasePrefabName(definitionSet, MyCubeSize.Large, true, isCreative, settings.LargeStatic);
                this.AddBasePrefabName(definitionSet, MyCubeSize.Large, false, isCreative, settings.LargeDynamic);
            }
            if (configuration.LootBag != null)
            {
                definitionSet.m_lootBagDefinition = new MyLootBagDefinition();
                definitionSet.m_lootBagDefinition.Init(configuration.LootBag);
            }
        }

        private static void InitContainerTypes(MyModContext context, DefinitionDictionary<MyContainerTypeDefinition> output, MyObjectBuilder_ContainerTypeDefinition[] containers, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_ContainerTypeDefinition definition in containers)
            {
                Check<SerializableDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                MyContainerTypeDefinition definition2 = InitDefinition<MyContainerTypeDefinition>(context, definition);
                output[definition.Id] = definition2;
            }
        }

        private void InitControllerSchemas(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_ControllerSchemaDefinition[] schemas, bool failOnDebug)
        {
            foreach (MyObjectBuilder_ControllerSchemaDefinition definition in schemas)
            {
                MyControllerSchemaDefinition definition2 = InitDefinition<MyControllerSchemaDefinition>(context, definition);
                MyDefinitionId identifier = definition2.Id;
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(identifier), identifier, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions.AddDefinitionSafe<MyControllerSchemaDefinition>(definition2, context, "<ControllerSchema>", false);
            }
        }

        private static void InitCubeBlocks(MyModContext context, Dictionary<string, Vector2I> outputBlockPositions, MyObjectBuilder_CubeBlockDefinition[] cubeBlocks)
        {
            MyCubeBlockDefinition.ClearPreloadedConstructionModels();
            foreach (MyObjectBuilder_CubeBlockDefinition definition in cubeBlocks)
            {
                definition.BlockPairName = definition.BlockPairName ?? definition.DisplayName;
                if ((from component in definition.Components
                    where component.Subtype == "Computer"
                    select component).Count<MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent>() != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    Type producedType = MyCubeBlockFactory.GetProducedType(definition.Id.TypeId);
                    if (!producedType.IsSubclassOf(typeof(MyTerminalBlock)) && (producedType != typeof(MyTerminalBlock)))
                    {
                        MyDefinitionErrors.Add(context, stringBuilder.AppendFormat(MySpaceTexts.DefinitionError_BlockWithComputerNotTerminalBlock, definition.DisplayName).ToString(), TErrorSeverity.Error, true);
                    }
                }
            }
        }

        private void InitCurves(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_CurveDefinition[] curves, bool failOnDebug)
        {
            foreach (MyObjectBuilder_CurveDefinition definition in curves)
            {
                MyCurveDefinition definition2 = InitDefinition<MyCurveDefinition>(context, definition);
                MyDefinitionId identifier = definition2.Id;
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(identifier), identifier, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions.AddDefinitionSafe<MyCurveDefinition>(definition2, context, "<Curve>", false);
            }
        }

        private static void InitDebris(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_DebrisDefinition[] debris, bool failOnDebug = true)
        {
            MyDebrisDefinition[] definitionArray = new MyDebrisDefinition[debris.Length];
            for (int i = 0; i < debris.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyDebrisDefinition>(context, debris[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitDecalGlobals(MyModContext context, MyObjectBuilder_DecalGlobalsDefinition objBuilder, bool failOnDebug = true)
        {
            MyDecalGlobals globals = new MyDecalGlobals {
                DecalQueueSize = objBuilder.DecalQueueSize
            };
            MyRenderProxy.SetDecalGlobals(globals);
        }

        private static void InitDecals(MyModContext context, MyObjectBuilder_DecalDefinition[] objBuilders, bool failOnDebug = true)
        {
            List<string> list1 = new List<string>();
            Dictionary<string, List<MyDecalMaterialDesc>> descriptions = new Dictionary<string, List<MyDecalMaterialDesc>>();
            MyDecalMaterials.ClearMaterials();
            foreach (MyObjectBuilder_DecalDefinition definition in objBuilders)
            {
                List<MyDecalMaterialDesc> list;
                if (definition.MaxSize < definition.MinSize)
                {
                    definition.MaxSize = definition.MinSize;
                }
                MyDecalMaterial decalMaterial = new MyDecalMaterial(definition.Material, definition.Transparent, MyStringHash.GetOrCompute(definition.Target), MyStringHash.GetOrCompute(definition.Source), definition.MinSize, definition.MaxSize, definition.Depth, definition.Rotation);
                if (!descriptions.TryGetValue(decalMaterial.StringId, out list))
                {
                    list = new List<MyDecalMaterialDesc>();
                    descriptions[decalMaterial.StringId] = list;
                }
                list.Add(definition.Material);
                MyDecalMaterials.AddDecalMaterial(decalMaterial);
            }
            MyRenderProxy.RegisterDecals(descriptions);
        }

        private static T InitDefinition<T>(MyModContext context, MyObjectBuilder_DefinitionBase builder) where T: MyDefinitionBase
        {
            T local = GetObjectFactory().CreateInstance<T>(builder.GetType());
            local.Context = new MyModContext();
            local.Context.Init(context);
            if (!context.IsBaseGame)
            {
                UpdateModableContent(local.Context, builder);
            }
            local.Init(builder, local.Context);
            if (MyFakes.ENABLE_ALL_IN_SURVIVAL)
            {
                local.AvailableInSurvival = true;
            }
            return local;
        }

        private void InitDefinitionsCompat(MyModContext context, MyObjectBuilder_DefinitionBase[] definitions)
        {
            if (definitions != null)
            {
                foreach (MyObjectBuilder_DefinitionBase base2 in definitions)
                {
                    MyDefinitionBase def = InitDefinition<MyDefinitionBase>(context, base2);
                    this.m_currentLoadingSet.AddDefinition(def);
                }
            }
        }

        private static void InitDefinitionsEnvItems(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_EnvironmentItemDefinition[] items, bool failOnDebug = true)
        {
            MyEnvironmentItemDefinition[] definitionArray = new MyEnvironmentItemDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyEnvironmentItemDefinition>(context, items[i]);
                definitionArray[i].PhysicalMaterial = MyDestructionData.GetPhysicalMaterial(definitionArray[i], items[i].PhysicalMaterial);
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitDefinitionsGeneric<OBDefType, DefType>(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, OBDefType[] items, bool failOnDebug = true) where OBDefType: MyObjectBuilder_DefinitionBase where DefType: MyDefinitionBase
        {
            DefType[] localArray = new DefType[items.Length];
            for (int i = 0; i < localArray.Length; i++)
            {
                localArray[i] = InitDefinition<DefType>(context, items[i]);
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(localArray[i].Id), localArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[localArray[i].Id] = localArray[i];
            }
        }

        private static void InitDefinitionsGeneric<OBDefType, DefType>(MyModContext context, DefinitionDictionary<DefType> outputDefinitions, OBDefType[] items, bool failOnDebug = true) where OBDefType: MyObjectBuilder_DefinitionBase where DefType: MyDefinitionBase
        {
            DefType[] localArray = new DefType[items.Length];
            for (int i = 0; i < localArray.Length; i++)
            {
                localArray[i] = InitDefinition<DefType>(context, items[i]);
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(localArray[i].Id), localArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[localArray[i].Id] = localArray[i];
            }
        }

        private static void InitDestruction(MyModContext context, ref MyDestructionDefinition output, MyObjectBuilder_DestructionDefinition objBuilder, bool failOnDebug = true)
        {
            MyDestructionDefinition definition = InitDefinition<MyDestructionDefinition>(context, objBuilder);
            output = definition;
        }

        private static void InitDropContainers(MyModContext context, Dictionary<string, MyDropContainerDefinition> outputDefinitions, MyObjectBuilder_DropContainerDefinition[] dropContainers, bool failOnDebug)
        {
            foreach (MyObjectBuilder_DropContainerDefinition definition in dropContainers)
            {
                MyDropContainerDefinition definition2 = InitDefinition<MyDropContainerDefinition>(context, definition);
                string subtypeName = definition2.Id.SubtypeName;
                Check<string>(!outputDefinitions.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[subtypeName] = definition2;
            }
        }

        private static void InitEdges(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_EdgesDefinition[] edges, bool failOnDebug = true)
        {
            MyEdgesDefinition[] definitionArray = new MyEdgesDefinition[edges.Length];
            for (int i = 0; i < edges.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyEdgesDefinition>(context, edges[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitEmissiveColorPresets(MyModContext context, MyObjectBuilder_EmissiveColorStatePresetDefinition[] objBuilders, bool failOnDebug = true)
        {
            int index = 0;
            while (index < objBuilders.Length)
            {
                MyStringHash orCompute = MyStringHash.GetOrCompute(objBuilders[index].Id.SubtypeId);
                MyEmissiveColorPresets.AddPreset(orCompute, null, false);
                int num2 = 0;
                while (true)
                {
                    if (num2 >= objBuilders[index].EmissiveStates.Length)
                    {
                        index++;
                        break;
                    }
                    MyEmissiveColorState state = new MyEmissiveColorState(objBuilders[index].EmissiveStates[num2].EmissiveColorName, objBuilders[index].EmissiveStates[num2].DisplayColorName, objBuilders[index].EmissiveStates[num2].Emissivity);
                    MyEmissiveColorPresets.AddPresetState(orCompute, MyStringHash.GetOrCompute(objBuilders[index].EmissiveStates[num2].StateName), state, false);
                    num2++;
                }
            }
        }

        private static void InitEmissiveColors(MyModContext context, MyObjectBuilder_EmissiveColorDefinition[] objBuilders, bool failOnDebug = true)
        {
            for (int i = 0; i < objBuilders.Length; i++)
            {
                MyEmissiveColors.AddEmissiveColor(MyStringHash.GetOrCompute(objBuilders[i].Id.SubtypeId), new Color(objBuilders[i].ColorDefinition.R, objBuilders[i].ColorDefinition.G, objBuilders[i].ColorDefinition.B, objBuilders[i].ColorDefinition.A), false);
            }
        }

        private static void InitEnvironment(MyModContext context, DefinitionSet defSet, MyObjectBuilder_EnvironmentDefinition[] objBuilder, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_EnvironmentDefinition definition in objBuilder)
            {
                MyEnvironmentDefinition def = InitDefinition<MyEnvironmentDefinition>(context, definition);
                defSet.AddDefinition(def);
            }
        }

        private void InitEnvironmentItemsEntries(MyModContext context, HashSet<EnvironmentItemsEntry> output, EnvironmentItemsEntry[] entries, bool failOnDebug = true)
        {
            foreach (EnvironmentItemsEntry entry in entries)
            {
                Check<EnvironmentItemsEntry>(!output.Contains(entry), entry, failOnDebug, "Duplicate entry of '{0}'");
                output.Add(entry);
            }
        }

        private static void InitFlares(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_FlareDefinition[] objBuilders, bool failOnDebug = true)
        {
            MyFlareDefinition[] definitionArray = new MyFlareDefinition[objBuilders.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyFlareDefinition>(context, objBuilders[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitGenericObjects(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_DefinitionBase[] objects, bool failOnDebug = true)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                MyDefinitionBase base2 = InitDefinition<MyDefinitionBase>(context, objects[i]);
                Check<MyDefinitionId>(!output.ContainsKey(base2.Id), base2.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[base2.Id] = base2;
            }
        }

        private static void InitGlobalEvents(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_GlobalEventDefinition[] events, bool failOnDebug = true)
        {
            MyGlobalEventDefinition[] definitionArray = new MyGlobalEventDefinition[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyGlobalEventDefinition>(context, events[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitGridCreators(MyModContext context, DefinitionDictionary<MyGridCreateToolDefinition> gridCreateDefinitions, DefinitionDictionary<MyDefinitionBase> definitionsById, MyObjectBuilder_GridCreateToolDefinition[] gridCreators, bool failOnDebug)
        {
            foreach (MyObjectBuilder_GridCreateToolDefinition definition in gridCreators)
            {
                bool flag1 = gridCreateDefinitions.ContainsKey(definition.Id) & failOnDebug;
                MyGridCreateToolDefinition definition2 = InitDefinition<MyGridCreateToolDefinition>(context, definition);
                gridCreateDefinitions[definition.Id] = definition2;
                definitionsById[definition.Id] = definition2;
            }
        }

        private void InitGroupedIds(MyModContext context, string setName, Dictionary<string, Dictionary<string, MyGroupedIds>> output, MyGroupedIds[] groups, bool failOnDebug)
        {
            Dictionary<string, MyGroupedIds> dictionary;
            if (!output.TryGetValue(setName, out dictionary))
            {
                dictionary = new Dictionary<string, MyGroupedIds>();
                output.Add(setName, dictionary);
            }
            foreach (MyGroupedIds ids in groups)
            {
                dictionary[ids.Tag] = ids;
            }
        }

        private static void InitHandItems(MyModContext context, DefinitionDictionary<MyHandItemDefinition> output, MyObjectBuilder_HandItemDefinition[] items, bool failOnDebug = true)
        {
            MyHandItemDefinition[] definitionArray = new MyHandItemDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyHandItemDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitLCDTextureCategories(MyModContext context, DefinitionSet definitions, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_LCDTextureDefinition[] categories, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_LCDTextureDefinition definition in categories)
            {
                MyLCDTextureDefinition def = InitDefinition<MyLCDTextureDefinition>(context, definition);
                Check<SerializableDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definition.Id] = def;
                definitions.AddOrRelaceDefinition(def);
            }
        }

        private void InitMainMenuInventoryScenes(MyModContext context, DefinitionDictionary<MyMainMenuInventorySceneDefinition> output, MyObjectBuilder_MainMenuInventorySceneDefinition[] items, bool failOnDebug)
        {
            MyMainMenuInventorySceneDefinition[] definitionArray = new MyMainMenuInventorySceneDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyMainMenuInventorySceneDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitMaterialProperties(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_MaterialPropertiesDefinition[] properties)
        {
            foreach (MyObjectBuilder_MaterialPropertiesDefinition definition in properties)
            {
                MyPhysicalMaterialDefinition definition2;
                if (this.TryGetDefinition<MyPhysicalMaterialDefinition>(definition.Id, out definition2))
                {
                    definition2.Init(definition, context);
                }
            }
        }

        private void InitMultiBlockDefinitions()
        {
            if (MyFakes.ENABLE_MULTIBLOCKS)
            {
                foreach (MyMultiBlockDefinition definition in this.GetMultiBlockDefinitions())
                {
                    definition.Min = Vector3I.MaxValue;
                    definition.Max = Vector3I.MinValue;
                    foreach (MyMultiBlockDefinition.MyMultiBlockPartDefinition definition2 in definition.BlockDefinitions)
                    {
                        MyCubeBlockDefinition definition3;
                        if (Static.TryGetCubeBlockDefinition(definition2.Id, out definition3) && (definition3 != null))
                        {
                            MatrixI transformation = new MatrixI(definition2.Forward, definition2.Up);
                            Vector3I vectori = Vector3I.Abs(Vector3I.TransformNormal(definition3.Size - Vector3I.One, ref transformation));
                            definition2.Max = (Vector3I) (definition2.Min + vectori);
                            definition.Min = Vector3I.Min(definition.Min, definition2.Min);
                            definition.Max = Vector3I.Max(definition.Max, definition2.Max);
                        }
                    }
                }
            }
        }

        private void InitNavigationDefinitions(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_BlockNavigationDefinition[] definitions, bool failOnDebug = true)
        {
            MyBlockNavigationDefinition[] definitionArray = new MyBlockNavigationDefinition[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyBlockNavigationDefinition>(context, definitions[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitParticleEffects(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_ParticleEffect[] classes, bool failOnDebug = true)
        {
            if (!this.m_transparentMaterialsInitialized)
            {
                CreateTransparentMaterials();
                this.m_transparentMaterialsInitialized = true;
            }
            foreach (MyObjectBuilder_ParticleEffect effect in classes)
            {
                MyParticleEffect local1 = MyParticlesManager.EffectsPool.Allocate(false);
                local1.DeserializeFromObjectBuilder(effect);
                MyParticlesLibrary.AddParticleEffect(local1);
            }
        }

        private static void InitPhysicalItems(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, List<MyPhysicalItemDefinition> outputWeapons, MyObjectBuilder_PhysicalItemDefinition[] items, bool failOnDebug = true)
        {
            MyPhysicalItemDefinition[] definitionArray = new MyPhysicalItemDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyPhysicalItemDefinition>(context, items[i]);
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                if (definitionArray[i].Id.TypeId == typeof(MyObjectBuilder_PhysicalGunObject))
                {
                    outputWeapons.Add(definitionArray[i]);
                }
                outputDefinitions[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitPhysicalMaterials(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_PhysicalMaterialDefinition[] materials)
        {
            foreach (MyObjectBuilder_PhysicalMaterialDefinition definition in materials)
            {
                MyPhysicalMaterialDefinition definition2;
                if (this.TryGetDefinition<MyPhysicalMaterialDefinition>(definition.Id, out definition2))
                {
                    definition2.Init(definition, context);
                }
                else
                {
                    definition2 = InitDefinition<MyPhysicalMaterialDefinition>(context, definition);
                    outputDefinitions.AddDefinitionSafe<MyPhysicalMaterialDefinition>(definition2, context, "<PhysicalMaterials>", false);
                }
                this.m_definitions.m_physicalMaterialsByName[definition2.Id.SubtypeName] = definition2;
            }
        }

        [Obsolete]
        private void InitPlanetGeneratorDefinitions(MyModContext context, DefinitionSet defset, MyObjectBuilder_PlanetGeneratorDefinition[] planets, bool failOnDebug)
        {
            foreach (MyObjectBuilder_PlanetGeneratorDefinition definition in planets)
            {
                MyPlanetGeneratorDefinition def = InitDefinition<MyPlanetGeneratorDefinition>(context, definition);
                if (!context.IsBaseGame)
                {
                    foreach (MyCloudLayerSettings settings in def.CloudLayers)
                    {
                        for (int i = 0; i < settings.Textures.Count; i++)
                        {
                            settings.Textures[i] = context.ModPath + @"\" + settings.Textures[i];
                        }
                    }
                }
                if (def.Enabled)
                {
                    defset.AddOrRelaceDefinition(def);
                }
                else
                {
                    defset.RemoveDefinition(ref def.Id);
                }
            }
        }

        private void InitPlanetPrefabDefinitions(MyModContext context, ref DefinitionDictionary<MyPlanetPrefabDefinition> m_planetDefinitions, MyObjectBuilder_PlanetPrefabDefinition[] planets, bool failOnDebug)
        {
            foreach (MyObjectBuilder_PlanetPrefabDefinition definition in planets)
            {
                MyPlanetPrefabDefinition definition2 = InitDefinition<MyPlanetPrefabDefinition>(context, definition);
                MyDefinitionId key = definition2.Id;
                if (definition2.Enabled)
                {
                    m_planetDefinitions[key] = definition2;
                }
                else
                {
                    m_planetDefinitions.Remove(key);
                }
            }
        }

        private static void InitPrefabs(MyModContext context, Dictionary<string, MyPrefabDefinition> outputDefinitions, MyObjectBuilder_PrefabDefinition[] prefabs, bool failOnDebug)
        {
            foreach (MyObjectBuilder_PrefabDefinition definition in prefabs)
            {
                MyPrefabDefinition definition2 = InitDefinition<MyPrefabDefinition>(context, definition);
                string subtypeName = definition2.Id.SubtypeName;
                Check<string>(!outputDefinitions.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[subtypeName] = definition2;
                if (definition.RespawnShip)
                {
                    MyDefinitionErrors.Add(context, "Tag <RespawnShip /> is obsolete in prefabs. Use file \"RespawnShips.sbc\" instead.", TErrorSeverity.Warning, true);
                }
            }
        }

        private void InitPrefabThrowers(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_DefinitionBase[] items, bool failOnDebug)
        {
            MyPrefabThrowerDefinition[] definitionArray = new MyPrefabThrowerDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyPrefabThrowerDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitResearchBlocks(MyModContext context, ref DefinitionDictionary<MyResearchBlockDefinition> output, MyObjectBuilder_ResearchBlockDefinition[] items, bool failOnDebug)
        {
            MyResearchBlockDefinition[] definitionArray = new MyResearchBlockDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyResearchBlockDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitResearchGroups(MyModContext context, ref DefinitionDictionary<MyResearchGroupDefinition> output, MyObjectBuilder_ResearchGroupDefinition[] items, bool failOnDebug)
        {
            MyResearchGroupDefinition[] definitionArray = new MyResearchGroupDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyResearchGroupDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitRespawnShips(MyModContext context, Dictionary<string, MyRespawnShipDefinition> outputDefinitions, MyObjectBuilder_RespawnShipDefinition[] respawnShips, bool failOnDebug)
        {
            foreach (MyObjectBuilder_RespawnShipDefinition definition in respawnShips)
            {
                MyRespawnShipDefinition definition2 = InitDefinition<MyRespawnShipDefinition>(context, definition);
                string subtypeName = definition2.Id.SubtypeName;
                Check<string>(!outputDefinitions.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[subtypeName] = definition2;
            }
        }

        private void InitRopeDefinitions()
        {
            foreach (MyRopeDefinition definition in this.GetAllDefinitions())
            {
                if (definition != null)
                {
                    this.m_definitions.m_idToRope.Add(definition.Id, definition);
                }
            }
        }

        private static void InitScenarioDefinitions(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, List<MyScenarioDefinition> outputScenarios, MyObjectBuilder_ScenarioDefinition[] scenarios, bool failOnDebug = true)
        {
            MyScenarioDefinition[] definitionArray = new MyScenarioDefinition[scenarios.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyScenarioDefinition>(context, scenarios[i]);
                outputScenarios.Add(definitionArray[i]);
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitScriptedGroupsMap(MyModContext context, MyMappedId[] map, bool failOnDebug)
        {
            DictionaryValuesReader<MyDefinitionId, MyScriptedGroupDefinition> scriptedGroupDefinitions = this.GetScriptedGroupDefinitions();
            foreach (MyMappedId id in map)
            {
                MyObjectBuilderType type;
                MyDefinitionId key = new MyDefinitionId(typeof(MyObjectBuilder_ScriptedGroupDefinition), id.Group);
                MyScriptedGroupDefinition result = null;
                if (!MyObjectBuilderType.TryParse(id.TypeId, out type) || !scriptedGroupDefinitions.TryGetValue(key, out result))
                {
                    bool flag1 = failOnDebug;
                    MyLog.Default.WriteLine("Scripted group failed to load");
                }
                MyDefinitionId id3 = new MyDefinitionId(type, id.SubtypeName);
                result.Add(context, id3);
            }
        }

        private static void InitShadowTextureSets(MyModContext context, MyObjectBuilder_ShadowTextureSetDefinition[] objBuilders, bool failOnDebug = true)
        {
            MyGuiTextShadows.ClearShadowTextures();
            MyObjectBuilder_ShadowTextureSetDefinition[] definitionArray = objBuilders;
            int index = 0;
            while (index < definitionArray.Length)
            {
                MyObjectBuilder_ShadowTextureSetDefinition definition = definitionArray[index];
                List<ShadowTexture> textures = new List<ShadowTexture>();
                MyObjectBuilder_ShadowTexture[] shadowTextures = definition.ShadowTextures;
                int num2 = 0;
                while (true)
                {
                    if (num2 >= shadowTextures.Length)
                    {
                        MyGuiTextShadows.AddTextureSet(definition.Id.SubtypeName, textures);
                        index++;
                        break;
                    }
                    MyObjectBuilder_ShadowTexture texture = shadowTextures[num2];
                    textures.Add(new ShadowTexture(texture.Texture, texture.MinWidth, texture.GrowFactorWidth, texture.GrowFactorHeight, texture.DefaultAlpha));
                    num2++;
                }
            }
        }

        private static void InitShipSounds(MyModContext context, DefinitionDictionary<MyShipSoundsDefinition> output, MyObjectBuilder_ShipSoundsDefinition[] shipGroups, bool failOnDebug = true)
        {
            MyShipSoundsDefinition[] definitionArray = new MyShipSoundsDefinition[shipGroups.Length];
            for (int i = 0; i < shipGroups.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyShipSoundsDefinition>(context, shipGroups[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitShipSoundSystem(MyModContext context, ref MyShipSoundSystemDefinition output, MyObjectBuilder_ShipSoundSystemDefinition shipSystem, bool failOnDebug = true)
        {
            MyShipSoundSystemDefinition definition = InitDefinition<MyShipSoundSystemDefinition>(context, shipSystem);
            output = definition;
        }

        private void InitSoundCategories(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_SoundCategoryDefinition[] categories, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_SoundCategoryDefinition definition in categories)
            {
                MySoundCategoryDefinition definition2 = InitDefinition<MySoundCategoryDefinition>(context, definition);
                Check<SerializableDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definition.Id] = definition2;
            }
        }

        private void InitSounds(MyModContext context, DefinitionDictionary<MyAudioDefinition> output, MyObjectBuilder_AudioDefinition[] classes, bool failOnDebug = true)
        {
            foreach (MyObjectBuilder_AudioDefinition definition in classes)
            {
                output[definition.Id] = InitDefinition<MyAudioDefinition>(context, definition);
            }
        }

        private static void InitSpawnGroups(MyModContext context, List<MySpawnGroupDefinition> outputSpawnGroups, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_SpawnGroupDefinition[] spawnGroups)
        {
            foreach (MyObjectBuilder_SpawnGroupDefinition definition in spawnGroups)
            {
                MySpawnGroupDefinition item = InitDefinition<MySpawnGroupDefinition>(context, definition);
                item.Init(definition, context);
                if (item.IsValid)
                {
                    outputSpawnGroups.Add(item);
                    outputDefinitions.AddDefinitionSafe<MySpawnGroupDefinition>(item, context, context.CurrentFile, false);
                }
                else
                {
                    MySandboxGame.Log.WriteLine("Error loading spawn group " + item.DisplayNameString);
                    MyDefinitionErrors.Add(context, "Error loading spawn group " + item.DisplayNameString, TErrorSeverity.Warning, true);
                }
            }
        }

        private static void InitTransparentMaterials(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, MyObjectBuilder_TransparentMaterialDefinition[] materials)
        {
            foreach (MyObjectBuilder_TransparentMaterialDefinition definition in materials)
            {
                MyTransparentMaterialDefinition definition2 = InitDefinition<MyTransparentMaterialDefinition>(context, definition);
                definition2.Init(definition, context);
                outputDefinitions.AddDefinitionSafe<MyTransparentMaterialDefinition>(definition2, context, "<TransparentMaterials>", false);
            }
        }

        private static void InitVoxelHands(MyModContext context, DefinitionDictionary<MyDefinitionBase> output, MyObjectBuilder_VoxelHandDefinition[] items, bool failOnDebug = true)
        {
            MyVoxelHandDefinition[] definitionArray = new MyVoxelHandDefinition[items.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyVoxelHandDefinition>(context, items[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private void InitVoxelMapStorages(MyModContext context, Dictionary<string, MyVoxelMapStorageDefinition> output, MyObjectBuilder_VoxelMapStorageDefinition[] items, bool failOnDebug)
        {
            foreach (MyObjectBuilder_VoxelMapStorageDefinition definition in items)
            {
                MyVoxelMapStorageDefinition definition2 = InitDefinition<MyVoxelMapStorageDefinition>(context, definition);
                if (definition2.StorageFile != null)
                {
                    string subtypeName = definition2.Id.SubtypeName;
                    Check<string>(!output.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                    output[subtypeName] = definition2;
                }
            }
        }

        public void InitVoxelMaterials()
        {
            MyRenderVoxelMaterialData[] materials = new MyRenderVoxelMaterialData[this.m_definitions.m_voxelMaterialsByName.Count];
            MyVoxelMaterialDefinition.ResetIndexing();
            int index = 0;
            foreach (KeyValuePair<string, MyVoxelMaterialDefinition> pair in this.m_definitions.m_voxelMaterialsByName)
            {
                MyVoxelMaterialDefinition definition = pair.Value;
                definition.AssignIndex();
                this.m_definitions.m_voxelMaterialsByIndex[definition.Index] = definition;
                if (definition.IsRare)
                {
                    DefinitionSet definitions = this.m_definitions;
                    definitions.m_voxelMaterialRareCount++;
                }
                index++;
                materials[index] = definition.RenderParams;
            }
            MyRenderProxy.CreateRenderVoxelMaterials(materials);
        }

        private static void InitVoxelMaterials(MyModContext context, Dictionary<string, MyVoxelMaterialDefinition> output, MyObjectBuilder_VoxelMaterialDefinition[] materials, bool failOnDebug = true)
        {
            MyVoxelMaterialDefinition[] definitionArray = new MyVoxelMaterialDefinition[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyVoxelMaterialDefinition>(context, materials[i]);
                Check<string>(!output.ContainsKey(definitionArray[i].Id.SubtypeName), definitionArray[i].Id.SubtypeName, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id.SubtypeName] = definitionArray[i];
                if (!context.IsBaseGame)
                {
                    MySandboxGame.Log.WriteLine("Loaded voxel material: " + definitionArray[i].Id.SubtypeName);
                }
            }
        }

        private static void InitWeapons(MyModContext context, DefinitionDictionary<MyWeaponDefinition> output, MyObjectBuilder_WeaponDefinition[] weapons, bool failOnDebug = true)
        {
            MyWeaponDefinition[] definitionArray = new MyWeaponDefinition[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyWeaponDefinition>(context, weapons[i]);
                Check<MyDefinitionId>(!output.ContainsKey(definitionArray[i].Id), definitionArray[i].Id, failOnDebug, "Duplicate entry of '{0}'");
                output[definitionArray[i].Id] = definitionArray[i];
            }
        }

        private static void InitWheelModels(MyModContext context, Dictionary<string, MyWheelModelsDefinition> outputDefinitions, MyObjectBuilder_WheelModelsDefinition[] wheelDefinitions, bool failOnDebug)
        {
            foreach (MyObjectBuilder_WheelModelsDefinition definition in wheelDefinitions)
            {
                MyWheelModelsDefinition definition2 = InitDefinition<MyWheelModelsDefinition>(context, definition);
                string subtypeName = definition2.Id.SubtypeName;
                Check<string>(!outputDefinitions.ContainsKey(subtypeName), subtypeName, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[subtypeName] = definition2;
            }
        }

        public bool IsComponentBlock(MyDefinitionId blockDefinitionId) => 
            this.m_definitions.m_componentBlocks.Contains(blockDefinitionId);

        public MyDefinitionId? ItemIdFromWeaponId(MyDefinitionId weaponDefinition)
        {
            MyDefinitionId? nullable = null;
            if (weaponDefinition.TypeId == typeof(MyObjectBuilder_PhysicalGunObject))
            {
                nullable = new MyDefinitionId?(weaponDefinition);
            }
            else
            {
                MyPhysicalItemDefinition physicalItemForHandItem = Static.GetPhysicalItemForHandItem(weaponDefinition);
                if (physicalItemForHandItem != null)
                {
                    nullable = new MyDefinitionId?(physicalItemForHandItem.Id);
                }
            }
            return nullable;
        }

        private T Load<T>(string path) where T: MyObjectBuilder_Base
        {
            T objectBuilder = default(T);
            MyObjectBuilderSerializer.DeserializeXML<T>(path, out objectBuilder);
            return objectBuilder;
        }

        private T Load<T>(string path, Type useType) where T: MyObjectBuilder_Base
        {
            MyObjectBuilder_Base objectBuilder = null;
            MyObjectBuilderSerializer.DeserializeXML(path, out objectBuilder, useType);
            if (objectBuilder != null)
            {
                return (objectBuilder as T);
            }
            return default(T);
        }

        public void LoadData(List<MyObjectBuilder_Checkpoint.ModItem> mods)
        {
            MySandboxGame.Log.WriteLine("MyDefinitionManager.LoadData() - START");
            while (MySandboxGame.IsPreloading)
            {
                Thread.Sleep(1);
            }
            this.UnloadData();
            this.Loading = true;
            this.LoadScenarios();
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                if (!this.m_modDefinitionSets.ContainsKey(""))
                {
                    this.m_modDefinitionSets.Add("", new DefinitionSet());
                }
                List<MyModContext> contexts = new List<MyModContext>();
                List<DefinitionSet> definitionSets = new List<DefinitionSet>();
                contexts.Add(MyModContext.BaseGame);
                definitionSets.Add(this.m_modDefinitionSets[""]);
                foreach (MyObjectBuilder_Checkpoint.ModItem item in mods)
                {
                    MyModContext context = new MyModContext();
                    context.Init(item);
                    if (!this.m_modDefinitionSets.ContainsKey(context.ModPath))
                    {
                        DefinitionSet set2 = new DefinitionSet();
                        this.m_modDefinitionSets.Add(context.ModPath, set2);
                        contexts.Add(context);
                        definitionSets.Add(set2);
                    }
                }
                MySandboxGame.Log.WriteLine($"List of used mods ({mods.Count}) - START");
                MySandboxGame.Log.IncreaseIndent();
                foreach (MyObjectBuilder_Checkpoint.ModItem item2 in mods)
                {
                    MySandboxGame.Log.WriteLine($"Id = {item2.PublishedFileId}, Filename = '{item2.Name}', Name = '{item2.FriendlyName}'");
                }
                MySandboxGame.Log.DecreaseIndent();
                MySandboxGame.Log.WriteLine("List of used mods - END");
                this.LoadDefinitions(contexts, definitionSets, true, false);
                if (MySandboxGame.Static != null)
                {
                    this.LoadPostProcess();
                }
                if (MyFakes.TEST_MODELS && (MyExternalAppBase.Static == null))
                {
                    long timestamp = Stopwatch.GetTimestamp();
                    this.TestCubeBlockModels();
                    double num1 = ((double) (Stopwatch.GetTimestamp() - timestamp)) / ((double) Stopwatch.Frequency);
                }
                this.CheckCharacterPickup();
                if (MyFakes.ENABLE_ALL_IN_SURVIVAL)
                {
                    Dictionary<MyDefinitionId, MyBehaviorDefinition>.ValueCollection.Enumerator enumerator3;
                    using (List<MyPhysicalItemDefinition>.Enumerator enumerator2 = this.m_definitions.m_physicalItemDefinitions.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.AvailableInSurvival = true;
                        }
                    }
                    using (enumerator3 = this.m_definitions.m_behaviorDefinitions.Values.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            enumerator3.Current.AvailableInSurvival = true;
                        }
                    }
                    using (enumerator3 = this.m_definitions.m_behaviorDefinitions.Values.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            enumerator3.Current.AvailableInSurvival = true;
                        }
                    }
                    using (Dictionary<string, MyCharacterDefinition>.ValueCollection.Enumerator enumerator4 = this.m_definitions.m_characters.Values.GetEnumerator())
                    {
                        while (enumerator4.MoveNext())
                        {
                            enumerator4.Current.AvailableInSurvival = true;
                        }
                    }
                }
                foreach (MyEnvironmentItemsDefinition definition in Static.GetEnvironmentItemClassDefinitions())
                {
                    List<MyDefinitionId> list3 = null;
                    if (!this.m_definitions.m_channelEnvironmentItemsDefs.TryGetValue(definition.Channel, out list3))
                    {
                        list3 = new List<MyDefinitionId>();
                        this.m_definitions.m_channelEnvironmentItemsDefs[definition.Channel] = list3;
                    }
                    list3.Add(definition.Id);
                }
            }
            this.Loading = false;
            MySandboxGame.Log.WriteLine("MyDefinitionManager.LoadData() - END");
        }

        private void LoadDefinitions(List<MyModContext> contexts, List<DefinitionSet> definitionSets, bool failOnDebug = true, bool isPreload = false)
        {
            HashSet<string> preloadSet = null;
            Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>[] actionArray;
            int num2;
            int num3;
            if (isPreload)
            {
                preloadSet = this.GetPreloadSet();
                if (preloadSet == null)
                {
                    return;
                }
            }
            List<List<Tuple<MyObjectBuilder_Definitions, string>>> list = new List<List<Tuple<MyObjectBuilder_Definitions, string>>>();
            int num = 0;
            while (true)
            {
                if (num >= contexts.Count)
                {
                    actionArray = new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>[] { new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.CompatPhase), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase1), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase2), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase3), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase4), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase5) };
                    num2 = 0;
                    break;
                }
                if (!MyFileSystem.DirectoryExists(contexts[num].ModPathData))
                {
                    list.Add(null);
                }
                else
                {
                    definitionSets[num].Context = contexts[num];
                    this.m_transparentMaterialsInitialized = false;
                    List<Tuple<MyObjectBuilder_Definitions, string>> definitionBuilders = this.GetDefinitionBuilders(contexts[num], preloadSet);
                    list.Add(definitionBuilders);
                    if (definitionBuilders == null)
                    {
                        return;
                    }
                }
                num++;
            }
            goto TR_0016;
        TR_0007:
            num3++;
        TR_0013:
            while (true)
            {
                if (num3 >= contexts.Count)
                {
                    num2++;
                    break;
                }
                this.m_currentLoadingSet = definitionSets[num3];
                try
                {
                    foreach (Tuple<MyObjectBuilder_Definitions, string> tuple in list[num3])
                    {
                        contexts[num3].CurrentFile = tuple.Item2;
                        actionArray[num2](tuple.Item1, contexts[num3], definitionSets[num3], failOnDebug);
                    }
                }
                catch (Exception exception)
                {
                    FailModLoading(contexts[num3], num2, actionArray.Length, exception);
                    goto TR_0007;
                }
                this.MergeDefinitions();
                goto TR_0007;
            }
        TR_0016:
            while (true)
            {
                if (num2 < actionArray.Length)
                {
                    num3 = 0;
                    break;
                }
                for (int i = 0; i < contexts.Count; i++)
                {
                    this.AfterLoad(contexts[i], definitionSets[i]);
                }
                m_directoryExistCache.Clear();
                return;
            }
            goto TR_0013;
        }

        private void LoadDefinitions(MyModContext context, DefinitionSet definitionSet, bool failOnDebug = true, bool isPreload = false)
        {
            HashSet<string> preloadSet = null;
            if (isPreload)
            {
                preloadSet = this.GetPreloadSet();
                if (preloadSet == null)
                {
                    return;
                }
            }
            if (MyFileSystem.DirectoryExists(context.ModPathData))
            {
                this.m_currentLoadingSet = definitionSet;
                definitionSet.Context = context;
                this.m_transparentMaterialsInitialized = false;
                List<Tuple<MyObjectBuilder_Definitions, string>> definitionBuilders = this.GetDefinitionBuilders(context, preloadSet);
                if (definitionBuilders != null)
                {
                    Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>[] actionArray = new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>[] { new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.CompatPhase), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase1), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase2), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase3), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase4), new Action<MyObjectBuilder_Definitions, MyModContext, DefinitionSet, bool>(this.LoadPhase5) };
                    int index = 0;
                    while (true)
                    {
                        while (true)
                        {
                            if (index < actionArray.Length)
                            {
                                try
                                {
                                    foreach (Tuple<MyObjectBuilder_Definitions, string> tuple in definitionBuilders)
                                    {
                                        context.CurrentFile = tuple.Item2;
                                        actionArray[index](tuple.Item1, context, definitionSet, failOnDebug);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    FailModLoading(context, index, actionArray.Length, exception);
                                    return;
                                }
                            }
                            else
                            {
                                this.AfterLoad(context, definitionSet);
                                return;
                            }
                            break;
                        }
                        this.MergeDefinitions();
                        index++;
                    }
                }
            }
        }

        private static void LoadDroneBehaviorPresets(MyModContext context, DefinitionSet defSet, MyObjectBuilder_DroneBehaviorDefinition[] objBuilder, bool failOnDebug = true)
        {
            MyObjectBuilder_DroneBehaviorDefinition[] definitionArray = objBuilder;
            for (int i = 0; i < definitionArray.Length; i++)
            {
                MyDroneAIData preset = new MyDroneAIData(definitionArray[i]);
                MyObjectBuilder_DroneBehaviorDefinition definition1 = definitionArray[i];
                MyDroneAIDataStatic.SavePreset(definition1.Id.SubtypeId, preset);
            }
        }

        private MyHandItemDefinition[] LoadHandItems(string path, MyModContext context)
        {
            MyObjectBuilder_Definitions definitions = this.Load<MyObjectBuilder_Definitions>(path);
            MyHandItemDefinition[] definitionArray = new MyHandItemDefinition[definitions.HandItems.Length];
            for (int i = 0; i < definitionArray.Length; i++)
            {
                definitionArray[i] = InitDefinition<MyHandItemDefinition>(context, definitions.HandItems[i]);
            }
            return definitionArray;
        }

        private void LoadPhase1(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            if (objBuilder.Definitions != null)
            {
                foreach (MyObjectBuilder_DefinitionBase base2 in objBuilder.Definitions)
                {
                    MyDefinitionBase def = InitDefinition<MyDefinitionBase>(context, base2);
                    this.m_currentLoadingSet.AddDefinition(def);
                }
            }
            if (objBuilder.GridCreators != null)
            {
                MySandboxGame.Log.WriteLine("Loading grid creators");
                this.InitGridCreators(context, definitionSet.m_gridCreateDefinitions, definitionSet.m_definitionsById, objBuilder.GridCreators, failOnDebug);
            }
            if (objBuilder.Ammos != null)
            {
                MySandboxGame.Log.WriteLine("Loading ammo definitions");
                InitAmmos(context, definitionSet.m_ammoDefinitionsById, objBuilder.Ammos, failOnDebug);
            }
            if (objBuilder.AmmoMagazines != null)
            {
                MySandboxGame.Log.WriteLine("Loading ammo magazines");
                InitAmmoMagazines(context, definitionSet.m_definitionsById, objBuilder.AmmoMagazines, failOnDebug);
            }
            if (objBuilder.Animations != null)
            {
                MySandboxGame.Log.WriteLine("Loading animations");
                InitAnimations(context, definitionSet.m_definitionsById, objBuilder.Animations, definitionSet.m_animationsBySkeletonType, failOnDebug);
            }
            if (objBuilder.CategoryClasses != null)
            {
                MySandboxGame.Log.WriteLine("Loading category classes");
                this.InitCategoryClasses(context, definitionSet.m_categoryClasses, objBuilder.CategoryClasses, failOnDebug);
            }
            if (objBuilder.Debris != null)
            {
                MySandboxGame.Log.WriteLine("Loading debris");
                InitDebris(context, definitionSet.m_definitionsById, objBuilder.Debris, failOnDebug);
            }
            if (objBuilder.Edges != null)
            {
                MySandboxGame.Log.WriteLine("Loading edges");
                InitEdges(context, definitionSet.m_definitionsById, objBuilder.Edges, failOnDebug);
            }
            if (objBuilder.Factions != null)
            {
                MySandboxGame.Log.WriteLine("Loading factions");
                InitDefinitionsGeneric<MyObjectBuilder_FactionDefinition, MyFactionDefinition>(context, definitionSet.m_definitionsById, objBuilder.Factions, failOnDebug);
            }
            if (objBuilder.BlockPositions != null)
            {
                MySandboxGame.Log.WriteLine("Loading block positions");
                this.InitBlockPositions(definitionSet.m_blockPositions, objBuilder.BlockPositions, failOnDebug);
            }
            if (objBuilder.BlueprintClasses != null)
            {
                MySandboxGame.Log.WriteLine("Loading blueprint classes");
                this.InitBlueprintClasses(context, definitionSet.m_blueprintClasses, objBuilder.BlueprintClasses, failOnDebug);
            }
            if (objBuilder.BlueprintClassEntries != null)
            {
                MySandboxGame.Log.WriteLine("Loading blueprint class entries");
                this.InitBlueprintClassEntries(context, definitionSet.m_blueprintClassEntries, objBuilder.BlueprintClassEntries, failOnDebug);
            }
            if (objBuilder.Blueprints != null)
            {
                MySandboxGame.Log.WriteLine("Loading blueprints");
                this.InitBlueprints(context, definitionSet.m_blueprintsById, definitionSet.m_blueprintsByResultId, objBuilder.Blueprints, failOnDebug);
            }
            if (objBuilder.Components != null)
            {
                MySandboxGame.Log.WriteLine("Loading components");
                InitComponents(context, definitionSet.m_definitionsById, objBuilder.Components, failOnDebug);
            }
            if (objBuilder.Configuration != null)
            {
                MySandboxGame.Log.WriteLine("Loading configuration");
                Check<string>(failOnDebug, "Configuration", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitConfiguration(definitionSet, objBuilder.Configuration);
            }
            if (objBuilder.ContainerTypes != null)
            {
                MySandboxGame.Log.WriteLine("Loading container types");
                InitContainerTypes(context, definitionSet.m_containerTypeDefinitions, objBuilder.ContainerTypes, failOnDebug);
            }
            if (objBuilder.Environments != null)
            {
                MySandboxGame.Log.WriteLine("Loading environment definition");
                Check<string>(failOnDebug, "Environment", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitEnvironment(context, definitionSet, objBuilder.Environments, failOnDebug);
            }
            if (objBuilder.DroneBehaviors != null)
            {
                MySandboxGame.Log.WriteLine("Loading drone behaviors");
                Check<string>(failOnDebug, "DroneBehaviors", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                LoadDroneBehaviorPresets(context, definitionSet, objBuilder.DroneBehaviors, failOnDebug);
            }
            if (objBuilder.EnvironmentItemsEntries != null)
            {
                MySandboxGame.Log.WriteLine("Loading environment items entries");
                this.InitEnvironmentItemsEntries(context, definitionSet.m_environmentItemsEntries, objBuilder.EnvironmentItemsEntries, failOnDebug);
            }
            if (objBuilder.GlobalEvents != null)
            {
                MySandboxGame.Log.WriteLine("Loading event definitions");
                InitGlobalEvents(context, definitionSet.m_definitionsById, objBuilder.GlobalEvents, failOnDebug);
            }
            if (objBuilder.HandItems != null)
            {
                InitHandItems(context, definitionSet.m_handItemsById, objBuilder.HandItems, failOnDebug);
            }
            if (objBuilder.VoxelHands != null)
            {
                InitVoxelHands(context, definitionSet.m_definitionsById, objBuilder.VoxelHands, failOnDebug);
            }
            if (objBuilder.AssetModifiers != null)
            {
                InitAssetModifiers(context, definitionSet.m_assetModifiers, objBuilder.AssetModifiers, failOnDebug);
            }
            if (objBuilder.MainMenuInventoryScenes != null)
            {
                this.InitMainMenuInventoryScenes(context, definitionSet.m_mainMenuInventoryScenes, objBuilder.MainMenuInventoryScenes, failOnDebug);
            }
            if ((objBuilder.PrefabThrowers != null) && MyFakes.ENABLE_PREFAB_THROWER)
            {
                this.InitPrefabThrowers(context, definitionSet.m_definitionsById, objBuilder.PrefabThrowers, failOnDebug);
            }
            if (objBuilder.PhysicalItems != null)
            {
                MySandboxGame.Log.WriteLine("Loading physical items");
                InitPhysicalItems(context, definitionSet.m_definitionsById, definitionSet.m_physicalItemDefinitions, objBuilder.PhysicalItems, failOnDebug);
            }
            if (objBuilder.TransparentMaterials != null)
            {
                MySandboxGame.Log.WriteLine("Loading transparent material properties");
                InitTransparentMaterials(context, definitionSet.m_definitionsById, objBuilder.TransparentMaterials);
            }
            if ((objBuilder.VoxelMaterials != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading voxel material definitions");
                InitVoxelMaterials(context, definitionSet.m_voxelMaterialsByName, objBuilder.VoxelMaterials, failOnDebug);
            }
            if (objBuilder.Characters != null)
            {
                MySandboxGame.Log.WriteLine("Loading character definitions");
                InitCharacters(context, definitionSet.m_characters, definitionSet.m_definitionsById, objBuilder.Characters, failOnDebug);
            }
            if (objBuilder.CompoundBlockTemplates != null)
            {
                MySandboxGame.Log.WriteLine("Loading compound block template definitions");
                InitDefinitionsGeneric<MyObjectBuilder_CompoundBlockTemplateDefinition, MyCompoundBlockTemplateDefinition>(context, definitionSet.m_definitionsById, objBuilder.CompoundBlockTemplates, failOnDebug);
            }
            if (objBuilder.Sounds != null)
            {
                MySandboxGame.Log.WriteLine("Loading sound definitions");
                this.InitSounds(context, definitionSet.m_sounds, objBuilder.Sounds, failOnDebug);
            }
            if (objBuilder.MultiBlocks != null)
            {
                MySandboxGame.Log.WriteLine("Loading multi cube block definitions");
                InitDefinitionsGeneric<MyObjectBuilder_MultiBlockDefinition, MyMultiBlockDefinition>(context, definitionSet.m_definitionsById, objBuilder.MultiBlocks, failOnDebug);
            }
            if (objBuilder.SoundCategories != null)
            {
                MySandboxGame.Log.WriteLine("Loading sound categories");
                this.InitSoundCategories(context, definitionSet.m_definitionsById, objBuilder.SoundCategories, failOnDebug);
            }
            if (objBuilder.ShipSoundGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading ship sound groups");
                InitShipSounds(context, definitionSet.m_shipSounds, objBuilder.ShipSoundGroups, failOnDebug);
            }
            if (objBuilder.ShipSoundSystem != null)
            {
                MySandboxGame.Log.WriteLine("Loading ship sound groups");
                InitShipSoundSystem(context, ref definitionSet.m_shipSoundSystem, objBuilder.ShipSoundSystem, failOnDebug);
            }
            if (objBuilder.LCDTextures != null)
            {
                MySandboxGame.Log.WriteLine("Loading LCD texture categories");
                this.InitLCDTextureCategories(context, definitionSet, definitionSet.m_definitionsById, objBuilder.LCDTextures, failOnDebug);
            }
            if (objBuilder.AIBehaviors != null)
            {
                MySandboxGame.Log.WriteLine("Loading behaviors");
                this.InitAIBehaviors(context, definitionSet.m_behaviorDefinitions, objBuilder.AIBehaviors, failOnDebug);
            }
            if (objBuilder.VoxelMapStorages != null)
            {
                MySandboxGame.Log.WriteLine("Loading voxel map storage definitions");
                this.InitVoxelMapStorages(context, definitionSet.m_voxelMapStorages, objBuilder.VoxelMapStorages, failOnDebug);
            }
            if (objBuilder.RopeTypes != null)
            {
                MySandboxGame.Log.WriteLine("Loading Rope type definitions");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.RopeTypes, failOnDebug);
            }
            if (objBuilder.Bots != null)
            {
                MySandboxGame.Log.WriteLine("Loading agent definitions");
                this.InitBots(context, definitionSet.m_definitionsById, objBuilder.Bots, failOnDebug);
            }
            if (objBuilder.PhysicalMaterials != null)
            {
                MySandboxGame.Log.WriteLine("Loading physical material properties");
                this.InitPhysicalMaterials(context, definitionSet.m_definitionsById, objBuilder.PhysicalMaterials);
            }
            if (objBuilder.AiCommands != null)
            {
                MySandboxGame.Log.WriteLine("Loading bot commands");
                this.InitBotCommands(context, definitionSet.m_definitionsById, objBuilder.AiCommands, failOnDebug);
            }
            if (objBuilder.AreaMarkerDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading area definitions");
                InitDefinitionsGeneric<MyObjectBuilder_AreaMarkerDefinition, MyAreaMarkerDefinition>(context, definitionSet.m_definitionsById, objBuilder.AreaMarkerDefinitions, failOnDebug);
            }
            if (objBuilder.BlockNavigationDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading navigation definitions");
                this.InitNavigationDefinitions(context, definitionSet.m_definitionsById, objBuilder.BlockNavigationDefinitions, failOnDebug);
            }
            if (objBuilder.Cuttings != null)
            {
                MySandboxGame.Log.WriteLine("Loading cutting definitions");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.Cuttings, failOnDebug);
            }
            if (objBuilder.ControllerSchemas != null)
            {
                MySandboxGame.Log.WriteLine("Loading controller schemas definitions");
                this.InitControllerSchemas(context, definitionSet.m_definitionsById, objBuilder.ControllerSchemas, failOnDebug);
            }
            if (objBuilder.CurveDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading curve definitions");
                this.InitCurves(context, definitionSet.m_definitionsById, objBuilder.CurveDefinitions, failOnDebug);
            }
            if (objBuilder.CharacterNames != null)
            {
                MySandboxGame.Log.WriteLine("Loading character names");
                this.InitCharacterNames(context, definitionSet.m_characterNames, objBuilder.CharacterNames, failOnDebug);
            }
            if (objBuilder.Battle != null)
            {
                MySandboxGame.Log.WriteLine("Loading battle definition");
                Check<string>(failOnDebug, "Battle", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitBattle(context, ref definitionSet.m_battleDefinition, objBuilder.Battle, failOnDebug);
            }
            if (objBuilder.DecalGlobals != null)
            {
                MySandboxGame.Log.WriteLine("Loading decal global definitions");
                Check<string>(failOnDebug, "DecalGlobals", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDecalGlobals(context, objBuilder.DecalGlobals, failOnDebug);
            }
            if (objBuilder.EmissiveColors != null)
            {
                MySandboxGame.Log.WriteLine("Loading emissive color definitions");
                Check<string>(failOnDebug, "EmissiveColors", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitEmissiveColors(context, objBuilder.EmissiveColors, failOnDebug);
            }
            if (objBuilder.EmissiveColorStatePresets != null)
            {
                MySandboxGame.Log.WriteLine("Loading emissive color default states");
                Check<string>(failOnDebug, "EmissiveColorPresets", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitEmissiveColorPresets(context, objBuilder.EmissiveColorStatePresets, failOnDebug);
            }
            if (objBuilder.Decals != null)
            {
                MySandboxGame.Log.WriteLine("Loading decal definitions");
                Check<string>(failOnDebug, "Decals", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDecals(context, objBuilder.Decals, failOnDebug);
            }
            if (objBuilder.PlanetGeneratorDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading planet definition " + context.ModName);
                Check<string>(failOnDebug, "Planet", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitPlanetGeneratorDefinitions(context, definitionSet, objBuilder.PlanetGeneratorDefinitions, failOnDebug);
            }
            if (objBuilder.StatDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading stat definitions");
                Check<string>(failOnDebug, "Stat", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.StatDefinitions, failOnDebug);
            }
            if (objBuilder.GasProperties != null)
            {
                MySandboxGame.Log.WriteLine("Loading gas property definitions");
                Check<string>(failOnDebug, "Gas", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.GasProperties, failOnDebug);
            }
            if (objBuilder.ResourceDistributionGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading resource distribution groups");
                Check<string>(failOnDebug, "DistributionGroup", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.ResourceDistributionGroups, failOnDebug);
            }
            if (objBuilder.ComponentGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading component group definitions");
                Check<string>(failOnDebug, "Component groups", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitComponentGroups(context, definitionSet.m_componentGroups, objBuilder.ComponentGroups, failOnDebug);
            }
            if (objBuilder.ComponentSubstitutions != null)
            {
                MySandboxGame.Log.WriteLine("Loading component substitution definitions");
                Check<string>(failOnDebug, "Component groups", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitComponentSubstitutions(context, definitionSet.m_componentSubstitutions, objBuilder.ComponentSubstitutions, failOnDebug);
            }
            if (objBuilder.ComponentBlocks != null)
            {
                MySandboxGame.Log.WriteLine("Loading component block definitions");
                this.InitComponentBlocks(context, definitionSet.m_componentBlockEntries, objBuilder.ComponentBlocks, failOnDebug);
            }
            if (objBuilder.PlanetPrefabs != null)
            {
                MySandboxGame.Log.WriteLine("Loading planet prefabs");
                Check<string>(failOnDebug, "Planet prefabs", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitPlanetPrefabDefinitions(context, ref definitionSet.m_planetPrefabDefinitions, objBuilder.PlanetPrefabs, failOnDebug);
            }
            if (objBuilder.EnvironmentGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading environment groups");
                Check<string>(failOnDebug, "Environment groups", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitGroupedIds(context, "EnvGroups", definitionSet.m_groupedIds, objBuilder.EnvironmentGroups, failOnDebug);
            }
            if (objBuilder.ScriptedGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading scripted groups");
                Check<string>(failOnDebug, "Scripted groups", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDefinitionsGeneric<MyObjectBuilder_ScriptedGroupDefinition, MyScriptedGroupDefinition>(context, definitionSet.m_scriptedGroupDefinitions, objBuilder.ScriptedGroups, failOnDebug);
            }
            if (objBuilder.PirateAntennas != null)
            {
                MySandboxGame.Log.WriteLine("Loading pirate antennas");
                Check<string>(failOnDebug, "Pirate antennas", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDefinitionsGeneric<MyObjectBuilder_PirateAntennaDefinition, MyPirateAntennaDefinition>(context, definitionSet.m_pirateAntennaDefinitions, objBuilder.PirateAntennas, failOnDebug);
            }
            if (objBuilder.Destruction != null)
            {
                MySandboxGame.Log.WriteLine("Loading destruction definition");
                Check<string>(failOnDebug, "Destruction", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDestruction(context, ref definitionSet.m_destructionDefinition, objBuilder.Destruction, failOnDebug);
            }
            if (objBuilder.EntityComponents != null)
            {
                MySandboxGame.Log.WriteLine("Loading entity components");
                Check<string>(failOnDebug, "Entity components", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDefinitionsGeneric<MyObjectBuilder_ComponentDefinitionBase, MyComponentDefinitionBase>(context, definitionSet.m_entityComponentDefinitions, objBuilder.EntityComponents, failOnDebug);
            }
            if (objBuilder.EntityContainers != null)
            {
                MySandboxGame.Log.WriteLine("Loading component containers");
                Check<string>(failOnDebug, "Entity containers", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitDefinitionsGeneric<MyObjectBuilder_ContainerDefinition, MyContainerDefinition>(context, definitionSet.m_entityContainers, objBuilder.EntityContainers, failOnDebug);
            }
            if (objBuilder.ShadowTextureSets != null)
            {
                MySandboxGame.Log.WriteLine("Loading shadow textures definitions");
                Check<string>(failOnDebug, "Text shadow sets", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitShadowTextureSets(context, objBuilder.ShadowTextureSets, failOnDebug);
            }
            if (objBuilder.Flares != null)
            {
                MySandboxGame.Log.WriteLine("Loading flare definitions");
                Check<string>(failOnDebug, "Flares", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitFlares(context, definitionSet.m_definitionsById, objBuilder.Flares, failOnDebug);
            }
            if (objBuilder.ResearchGroups != null)
            {
                MySandboxGame.Log.WriteLine("Loading research groups definitions");
                Check<string>(failOnDebug, "Research Groups", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitResearchGroups(context, ref definitionSet.m_researchGroupsDefinitions, objBuilder.ResearchGroups, failOnDebug);
            }
            if (objBuilder.ResearchBlocks != null)
            {
                MySandboxGame.Log.WriteLine("Loading research blocks definitions");
                Check<string>(failOnDebug, "Research Blocks", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitResearchBlocks(context, ref definitionSet.m_researchBlocksDefinitions, objBuilder.ResearchBlocks, failOnDebug);
            }
        }

        private void LoadPhase2(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            if (objBuilder.ParticleEffects != null)
            {
                MySandboxGame.Log.WriteLine("Loading particle effect definitions");
                this.InitParticleEffects(context, definitionSet.m_definitionsById, objBuilder.ParticleEffects, failOnDebug);
            }
            if (objBuilder.EnvironmentItems != null)
            {
                MySandboxGame.Log.WriteLine("Loading environment item definitions");
                InitDefinitionsEnvItems(context, definitionSet.m_definitionsById, objBuilder.EnvironmentItems, failOnDebug);
            }
            if (objBuilder.EnvironmentItemsDefinitions != null)
            {
                MySandboxGame.Log.WriteLine("Loading environment items definitions");
                InitDefinitionsGeneric<MyObjectBuilder_EnvironmentItemsDefinition, MyEnvironmentItemsDefinition>(context, definitionSet.m_definitionsById, objBuilder.EnvironmentItemsDefinitions, failOnDebug);
            }
            if (objBuilder.MaterialProperties != null)
            {
                MySandboxGame.Log.WriteLine("Loading physical material properties");
                this.InitMaterialProperties(context, definitionSet.m_definitionsById, objBuilder.MaterialProperties);
            }
            if (objBuilder.Weapons != null)
            {
                MySandboxGame.Log.WriteLine("Loading weapon definitions");
                InitWeapons(context, definitionSet.m_weaponDefinitionsById, objBuilder.Weapons, failOnDebug);
            }
            if (objBuilder.AudioEffects != null)
            {
                MySandboxGame.Log.WriteLine("Audio effects definitions");
                this.InitAudioEffects(context, definitionSet.m_definitionsById, objBuilder.AudioEffects, failOnDebug);
            }
            if (objBuilder.FloraElements != null)
            {
                MySandboxGame.Log.WriteLine("Loading flora elements definitions");
                Check<string>(failOnDebug, "Flora", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                InitGenericObjects(context, definitionSet.m_definitionsById, objBuilder.FloraElements, failOnDebug);
            }
            if (objBuilder.ScriptedGroupsMap != null)
            {
                MySandboxGame.Log.WriteLine("Loading scripted groups map");
                Check<string>(failOnDebug, "Scripted groups map", failOnDebug, "WARNING: Unexpected behaviour may occur due to redefinition of '{0}'");
                this.InitScriptedGroupsMap(context, objBuilder.ScriptedGroupsMap, failOnDebug);
            }
        }

        private void LoadPhase3(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            if (objBuilder.CubeBlocks != null)
            {
                MySandboxGame.Log.WriteLine("Loading cube blocks");
                InitCubeBlocks(context, definitionSet.m_blockPositions, objBuilder.CubeBlocks);
                ToDefinitions(context, definitionSet.m_definitionsById, definitionSet.m_uniqueCubeBlocksBySize, objBuilder.CubeBlocks, failOnDebug);
                MySandboxGame.Log.WriteLine("Created block definitions");
                foreach (DefinitionDictionary<MyCubeBlockDefinition> dictionary in definitionSet.m_uniqueCubeBlocksBySize)
                {
                    PrepareBlockBlueprints(context, definitionSet.m_blueprintsById, dictionary, true);
                }
            }
        }

        private void LoadPhase4(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            if ((objBuilder.Prefabs != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading prefab: " + context.CurrentFile);
                InitPrefabs(context, definitionSet.m_prefabs, objBuilder.Prefabs, failOnDebug);
            }
            if (MyFakes.ENABLE_GENERATED_INTEGRITY_FIX)
            {
                foreach (DefinitionDictionary<MyCubeBlockDefinition> dictionary in definitionSet.m_uniqueCubeBlocksBySize)
                {
                    this.FixGeneratedBlocksIntegrity(dictionary);
                }
            }
        }

        private void LoadPhase5(MyObjectBuilder_Definitions objBuilder, MyModContext context, DefinitionSet definitionSet, bool failOnDebug)
        {
            if ((objBuilder.SpawnGroups != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading spawn groups");
                InitSpawnGroups(context, definitionSet.m_spawnGroupDefinitions, definitionSet.m_definitionsById, objBuilder.SpawnGroups);
            }
            if ((objBuilder.RespawnShips != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading respawn ships");
                InitRespawnShips(context, definitionSet.m_respawnShips, objBuilder.RespawnShips, failOnDebug);
            }
            if ((objBuilder.DropContainers != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading drop containers");
                InitDropContainers(context, definitionSet.m_dropContainers, objBuilder.DropContainers, failOnDebug);
            }
            if ((objBuilder.WheelModels != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading wheel speeds");
                InitWheelModels(context, definitionSet.m_wheelModels, objBuilder.WheelModels, failOnDebug);
            }
            if ((objBuilder.AsteroidGenerators != null) && (MySandboxGame.Static != null))
            {
                MySandboxGame.Log.WriteLine("Loading asteroid generators");
                InitAsteroidGenerators(context, definitionSet.m_asteroidGenerators, objBuilder.AsteroidGenerators, failOnDebug);
            }
        }

        private void LoadPostProcess()
        {
            this.InitVoxelMaterials();
            if (!this.m_transparentMaterialsInitialized)
            {
                CreateTransparentMaterials();
                this.m_transparentMaterialsInitialized = true;
            }
            this.InitRopeDefinitions();
            this.InitBlockGroups();
            this.PostprocessComponentGroups();
            this.PostprocessComponentBlocks();
            this.PostprocessBlueprints();
            this.AddEntriesToBlueprintClasses();
            this.AddEntriesToEnvironmentItemClasses();
            this.PairPhysicalAndHandItems();
            this.CheckWeaponRelatedDefinitions();
            this.SetShipSoundSystem();
            this.MoveNonPublicBlocksToSpecialCategory();
            if (MyAudio.Static != null)
            {
                ListReader<MySoundData> soundDataFromDefinitions = MyAudioExtensions.GetSoundDataFromDefinitions();
                ListReader<MyAudioEffect> effectData = MyAudioExtensions.GetEffectData();
                if (!MyFakes.ENABLE_SOUNDS_ASYNC_PRELOAD)
                {
                    MyAudio.Static.ReloadData(soundDataFromDefinitions, effectData);
                }
                else
                {
                    SoundsData data1 = new SoundsData();
                    data1.SoundData = soundDataFromDefinitions;
                    data1.EffectData = effectData;
                    data1.Priority = WorkPriority.VeryLow;
                    SoundsData workData = data1;
                    ParallelTasks.Parallel.Start(new Action<WorkData>(this.LoadSoundAsync), new Action<WorkData>(this.OnLoadSoundsComplete), workData);
                }
            }
            this.PostprocessPirateAntennas();
            this.InitMultiBlockDefinitions();
            this.CreateMapMultiBlockDefinitionToBlockDefinition();
            this.PostprocessAllDefinitions();
            this.InitAssetModifiersForRender();
            this.AfterPostprocess();
        }

        public void LoadScenarios()
        {
            MySandboxGame.Log.WriteLine("MyDefinitionManager.LoadScenarios() - START");
            while (MySandboxGame.IsPreloading)
            {
                Thread.Sleep(1);
            }
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                MyDataIntegrityChecker.ResetHash();
                if (!this.m_modDefinitionSets.ContainsKey(""))
                {
                    this.m_modDefinitionSets.Add("", new DefinitionSet());
                }
                DefinitionSet definitionSet = this.m_modDefinitionSets[""];
                foreach (MyScenarioDefinition definition in this.m_definitions.m_scenarioDefinitions)
                {
                    definitionSet.m_definitionsById.Remove(definition.Id);
                }
                foreach (MyScenarioDefinition definition2 in this.m_definitions.m_scenarioDefinitions)
                {
                    this.m_definitions.m_definitionsById.Remove(definition2.Id);
                }
                this.m_definitions.m_scenarioDefinitions.Clear();
                this.LoadScenarios(MyModContext.BaseGame, definitionSet, true);
            }
            MySandboxGame.Log.WriteLine("MyDefinitionManager.LoadScenarios() - END");
        }

        private void LoadScenarios(MyModContext context, DefinitionSet definitionSet, bool failOnDebug = true)
        {
            string path = Path.Combine(context.ModPathData, "Scenarios.sbx");
            if (MyFileSystem.FileExists(path))
            {
                MyDataIntegrityChecker.HashInFile(path);
                MyObjectBuilder_ScenarioDefinitions definitions = this.Load<MyObjectBuilder_ScenarioDefinitions>(path);
                if (definitions == null)
                {
                    MyDefinitionErrors.Add(context, "Scenarios: Cannot load definition file, see log for details", TErrorSeverity.Error, true);
                }
                else
                {
                    if (definitions.Scenarios != null)
                    {
                        MySandboxGame.Log.WriteLine("Loading scenarios");
                        InitScenarioDefinitions(context, definitionSet.m_definitionsById, definitionSet.m_scenarioDefinitions, definitions.Scenarios, failOnDebug);
                    }
                    this.MergeDefinitions();
                }
            }
        }

        private void LoadSoundAsync(WorkData workData)
        {
            SoundsData data = workData as SoundsData;
            if (data != null)
            {
                MyAudio.Static.ReloadData(data.SoundData, data.EffectData);
            }
        }

        private void LoadVoxelMaterials(string path, MyModContext context, List<MyVoxelMaterialDefinition> res)
        {
            MyObjectBuilder_Definitions definitions = this.Load<MyObjectBuilder_Definitions>(path);
            for (int i = 0; i < definitions.VoxelMaterials.Length; i++)
            {
                MyDx11VoxelMaterialDefinition item = new MyDx11VoxelMaterialDefinition();
                item.Init(definitions.VoxelMaterials[i], context);
                res.Add(item);
            }
        }

        private T LoadWithProtobuffers<T>(string path) where T: MyObjectBuilder_Base
        {
            T objectBuilder = default(T);
            string str = path + MyObjectBuilderSerializer.ProtobufferExtension;
            if (!MyFileSystem.FileExists(str))
            {
                MyObjectBuilderSerializer.DeserializeXML<T>(path, out objectBuilder);
                if ((objectBuilder != null) && !MyFileSystem.FileExists(str))
                {
                    MyObjectBuilderSerializer.SerializePB(str, false, objectBuilder);
                }
            }
            else
            {
                MyObjectBuilderSerializer.DeserializePB<T>(str, out objectBuilder);
                if (objectBuilder == null)
                {
                    MyObjectBuilderSerializer.DeserializeXML<T>(path, out objectBuilder);
                    if (objectBuilder != null)
                    {
                        MyObjectBuilderSerializer.SerializePB(str, false, objectBuilder);
                    }
                }
            }
            return objectBuilder;
        }

        private static MyBlueprintDefinitionBase MakeBlueprintFromComponentStack(MyModContext context, MyCubeBlockDefinition cubeBlockDefinition)
        {
            MyCubeBlockFactory.GetProducedType(cubeBlockDefinition.Id.TypeId);
            MyObjectBuilder_CompositeBlueprintDefinition builder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_CompositeBlueprintDefinition>();
            builder.Id = new SerializableDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), cubeBlockDefinition.Id.ToString().Replace("MyObjectBuilder_", ""));
            Dictionary<MyDefinitionId, MyFixedPoint> dictionary = new Dictionary<MyDefinitionId, MyFixedPoint>();
            MyCubeBlockDefinition.Component[] components = cubeBlockDefinition.Components;
            int index = 0;
            while (true)
            {
                string displayNameText;
                if (index < components.Length)
                {
                    MyCubeBlockDefinition.Component component = components[index];
                    MyDefinitionId key = component.Definition.Id;
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = 0;
                    }
                    Dictionary<MyDefinitionId, MyFixedPoint> dictionary2 = dictionary;
                    MyDefinitionId id2 = key;
                    dictionary2[id2] += component.Count;
                    index++;
                    continue;
                }
                builder.Blueprints = new BlueprintItem[dictionary.Count];
                int num = 0;
                using (Dictionary<MyDefinitionId, MyFixedPoint>.Enumerator enumerator = dictionary.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        KeyValuePair<MyDefinitionId, MyFixedPoint> current = enumerator.Current;
                        MyBlueprintDefinitionBase base2 = null;
                        base2 = Static.TryGetBlueprintDefinitionByResultId(current.Key);
                        if (base2 != null)
                        {
                            BlueprintItem item1 = new BlueprintItem();
                            item1.Id = new SerializableDefinitionId(base2.Id.TypeId, base2.Id.SubtypeName);
                            item1.Amount = current.Value.ToString();
                            builder.Blueprints[num] = item1;
                            num++;
                            continue;
                        }
                        MyDefinitionErrors.Add(context, "Could not find component blueprint for " + current.Key.ToString(), TErrorSeverity.Error, true);
                        return null;
                    }
                }
                builder.Icons = cubeBlockDefinition.Icons;
                if (cubeBlockDefinition.DisplayNameEnum == null)
                {
                    displayNameText = cubeBlockDefinition.DisplayNameText;
                }
                else
                {
                    displayNameText = cubeBlockDefinition.DisplayNameEnum.Value.ToString();
                }
                builder.DisplayName = displayNameText;
                builder.Public = cubeBlockDefinition.Public;
                return InitDefinition<MyBlueprintDefinitionBase>(context, builder);
            }
        }

        private void MergeDefinitions()
        {
            this.m_definitions.Clear(false);
            foreach (KeyValuePair<string, DefinitionSet> pair in this.m_modDefinitionSets)
            {
                this.m_definitions.OverrideBy(pair.Value);
            }
        }

        private void MoveNonPublicBlocksToSpecialCategory()
        {
            if (MyFakes.ENABLE_NON_PUBLIC_BLOCKS)
            {
                MyGuiBlockCategoryDefinition definition1 = new MyGuiBlockCategoryDefinition();
                definition1.DescriptionString = "Non public blocks";
                definition1.DisplayNameString = "Non public";
                definition1.Enabled = true;
                definition1.Id = new MyDefinitionId(typeof(MyObjectBuilder_GuiBlockCategoryDefinition));
                definition1.IsBlockCategory = true;
                definition1.IsShipCategory = false;
                definition1.Name = "Non public";
                definition1.Public = true;
                definition1.SearchBlocks = true;
                definition1.ShowAnimations = false;
                definition1.ItemIds = new HashSet<string>();
                MyGuiBlockCategoryDefinition definition = definition1;
                foreach (string str in this.GetDefinitionPairNames())
                {
                    MyCubeBlockDefinitionGroup definitionGroup = Static.GetDefinitionGroup(str);
                    definition.ItemIds.Add(definitionGroup.Any.Id.ToString());
                }
                this.m_definitions.m_categories.Add("NonPublic", definition);
            }
        }

        private void OnLoadSoundsComplete(WorkData workData)
        {
        }

        private void PairPhysicalAndHandItems()
        {
            foreach (KeyValuePair<MyDefinitionId, MyHandItemDefinition> pair in this.m_definitions.m_handItemsById)
            {
                MyHandItemDefinition definition = pair.Value;
                MyPhysicalItemDefinition physicalItemDefinition = this.GetPhysicalItemDefinition(definition.PhysicalItemId);
                Check<MyDefinitionId>(!this.m_definitions.m_physicalItemsByHandItemId.ContainsKey(definition.Id), definition.Id, true, "Duplicate entry of '{0}'");
                Check<MyDefinitionId>(!this.m_definitions.m_handItemsByPhysicalItemId.ContainsKey(physicalItemDefinition.Id), physicalItemDefinition.Id, true, "Duplicate entry of '{0}'");
                this.m_definitions.m_physicalItemsByHandItemId[definition.Id] = physicalItemDefinition;
                this.m_definitions.m_handItemsByPhysicalItemId[physicalItemDefinition.Id] = definition;
            }
        }

        private void PostprocessAllDefinitions()
        {
            using (Dictionary<MyDefinitionId, MyDefinitionBase>.ValueCollection.Enumerator enumerator = this.m_definitions.m_definitionsById.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Postprocess();
                }
            }
        }

        private void PostprocessBlueprints()
        {
            CachingList<MyBlueprintDefinitionBase> list = new CachingList<MyBlueprintDefinitionBase>();
            foreach (KeyValuePair<MyDefinitionId, MyBlueprintDefinitionBase> pair in this.m_definitions.m_blueprintsById)
            {
                MyBlueprintDefinitionBase entity = pair.Value;
                if (entity.PostprocessNeeded)
                {
                    list.Add(entity);
                }
            }
            list.ApplyAdditions();
            int count = -1;
            while ((list.Count != 0) && (list.Count != count))
            {
                count = list.Count;
                foreach (MyBlueprintDefinitionBase base3 in list)
                {
                    MyCompositeBlueprintDefinition definition1 = base3 as MyCompositeBlueprintDefinition;
                    base3.Postprocess();
                    if (!base3.PostprocessNeeded)
                    {
                        list.Remove(base3, false);
                    }
                }
                list.ApplyRemovals();
            }
            if (list.Count != 0)
            {
                StringBuilder builder = new StringBuilder("Following blueprints could not be post-processed: ");
                foreach (MyBlueprintDefinitionBase base4 in list)
                {
                    builder.Append(base4.Id.ToString());
                    builder.Append(", ");
                }
                MyDefinitionErrors.Add(MyModContext.BaseGame, builder.ToString(), TErrorSeverity.Error, true);
            }
        }

        private void PostprocessComponentBlocks()
        {
            foreach (MyComponentBlockEntry entry in this.m_definitions.m_componentBlockEntries)
            {
                if (!entry.Enabled)
                {
                    continue;
                }
                MyDefinitionId item = new MyDefinitionId(MyObjectBuilderType.Parse(entry.Type), entry.Subtype);
                this.m_definitions.m_componentBlocks.Add(item);
                if (entry.Main)
                {
                    MyCubeBlockDefinition blockDefinition = null;
                    this.TryGetCubeBlockDefinition(item, out blockDefinition);
                    if ((blockDefinition.Components.Length == 1) && (blockDefinition.Components[0].Count == 1))
                    {
                        this.m_definitions.m_componentIdToBlock[blockDefinition.Components[0].Definition.Id] = blockDefinition;
                    }
                }
            }
            this.m_definitions.m_componentBlockEntries.Clear();
        }

        private void PostprocessComponentGroups()
        {
            foreach (KeyValuePair<MyDefinitionId, MyComponentGroupDefinition> pair in this.m_definitions.m_componentGroups)
            {
                MyComponentGroupDefinition definition = pair.Value;
                definition.Postprocess();
                if (definition.IsValid)
                {
                    int componentNumber = definition.GetComponentNumber();
                    for (int i = 1; i <= componentNumber; i++)
                    {
                        MyComponentDefinition componentDefinition = definition.GetComponentDefinition(i);
                        this.m_definitions.m_componentGroupMembers.Add(componentDefinition.Id, new MyTuple<int, MyComponentGroupDefinition>(i, definition));
                    }
                }
            }
        }

        private void PostprocessPirateAntennas()
        {
            using (Dictionary<MyDefinitionId, MyPirateAntennaDefinition>.ValueCollection.Enumerator enumerator = this.m_definitions.m_pirateAntennaDefinitions.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Postprocess();
                }
            }
        }

        public void PreloadDefinitions()
        {
            MySandboxGame.Log.WriteLine("MyDefinitionManager.PreloadDefinitions() - START");
            this.m_definitions.Clear(false);
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                if (!this.m_modDefinitionSets.ContainsKey(""))
                {
                    this.m_modDefinitionSets.Add("", new DefinitionSet());
                }
                DefinitionSet definitionSet = this.m_modDefinitionSets[""];
                this.LoadDefinitions(MyModContext.BaseGame, definitionSet, false, true);
            }
            MySandboxGame.Log.WriteLine("MyDefinitionManager.PreloadDefinitions() - END");
        }

        public List<Tuple<MyObjectBuilder_Definitions, string>> PrepareBaseDefinitions()
        {
            MySandboxGame.Log.WriteLine("MyDefinitionManager.PrepareBaseDefinitions() - START");
            List<Tuple<MyObjectBuilder_Definitions, string>> definitionBuilders = null;
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                if (MyFakes.ENABLE_PRELOAD_DEFINITIONS)
                {
                    definitionBuilders = this.GetDefinitionBuilders(MyModContext.BaseGame, null);
                }
            }
            MySandboxGame.Log.WriteLine("MyDefinitionManager.PrepareBaseDefinitions() - END");
            return definitionBuilders;
        }

        private static void PrepareBlockBlueprints(MyModContext context, Dictionary<MyDefinitionId, MyBlueprintDefinitionBase> output, Dictionary<MyDefinitionId, MyCubeBlockDefinition> cubeBlocks, bool failOnDebug = true)
        {
            foreach (KeyValuePair<MyDefinitionId, MyCubeBlockDefinition> pair in cubeBlocks)
            {
                MyCubeBlockDefinition definition = pair.Value;
                if (!context.IsBaseGame)
                {
                    MySandboxGame.Log.WriteLine("Loading cube block: " + pair.Key);
                }
                if (MyFakes.ENABLE_NON_PUBLIC_BLOCKS || definition.Public)
                {
                    MyCubeBlockDefinition uniqueVersion = definition.UniqueVersion;
                    Check<MyDefinitionId>(!output.ContainsKey(definition.Id), definition.Id, failOnDebug, "Duplicate entry of '{0}'");
                    if (!output.ContainsKey(uniqueVersion.Id))
                    {
                        MyBlueprintDefinitionBase base2 = MakeBlueprintFromComponentStack(context, uniqueVersion);
                        if (base2 != null)
                        {
                            output[base2.Id] = base2;
                        }
                    }
                }
            }
        }

        private static void ProcessContentFilePath(MyModContext context, ref string contentFile, object[] extensions, bool logNoExtensions)
        {
            if (!string.IsNullOrEmpty(contentFile))
            {
                string extension = Path.GetExtension(contentFile);
                if (extensions.IsNullOrEmpty<object>())
                {
                    if (logNoExtensions)
                    {
                        MyDefinitionErrors.Add(context, "List of supported file extensions not found. (Internal error)", TErrorSeverity.Warning, true);
                    }
                }
                else if (string.IsNullOrEmpty(extension))
                {
                    MyDefinitionErrors.Add(context, "File does not have a proper extension: " + contentFile, TErrorSeverity.Warning, true);
                }
                else if (!extensions.Contains<object>(extension))
                {
                    MyDefinitionErrors.Add(context, "File extension of: " + contentFile + " is not supported.", TErrorSeverity.Warning, true);
                }
                else
                {
                    bool flag;
                    string key = Path.Combine(context.ModPath, contentFile);
                    if (!m_directoryExistCache.TryGetValue(key, out flag))
                    {
                        flag = MyFileSystem.DirectoryExists(Path.GetDirectoryName(key)) && MyFileSystem.GetFiles(Path.GetDirectoryName(key), Path.GetFileName(key), MySearchOption.TopDirectoryOnly).Any<string>();
                        m_directoryExistCache.Add(key, flag);
                    }
                    if (flag)
                    {
                        contentFile = key;
                    }
                    else if (!MyFileSystem.FileExists(Path.Combine(MyFileSystem.ContentPath, contentFile)))
                    {
                        if (contentFile.EndsWith(".mwm"))
                        {
                            MyDefinitionErrors.Add(context, "Resource not found, setting to error model. Resource path: " + key, TErrorSeverity.Error, true);
                            contentFile = @"Models\Debug\Error.mwm";
                        }
                        else
                        {
                            MyDefinitionErrors.Add(context, "Resource not found, setting to null. Resource path: " + key, TErrorSeverity.Error, true);
                            contentFile = null;
                        }
                    }
                }
            }
        }

        private static void ProcessField(MyModContext context, object fieldOwnerInstance, FieldInfo field, bool includeMembers = true)
        {
            string[] extensions = (from s in field.GetCustomAttributes(typeof(ModdableContentFileAttribute), true).Cast<ModdableContentFileAttribute>() select from ex in s.FileExtensions select "." + ex).ToArray<string>();
            if ((extensions.Length != 0) && (field.FieldType == typeof(string)))
            {
                string contentFile = (string) field.GetValue(fieldOwnerInstance);
                ProcessContentFilePath(context, ref contentFile, extensions, true);
                field.SetValue(fieldOwnerInstance, contentFile);
            }
            else if (!(field.FieldType == typeof(string[])))
            {
                if (includeMembers && (field.FieldType.IsClass || (field.FieldType.IsValueType && !field.FieldType.IsPrimitive)))
                {
                    object instance = field.GetValue(fieldOwnerInstance);
                    IEnumerable enumerable = instance as IEnumerable;
                    if (enumerable != null)
                    {
                        foreach (object obj3 in enumerable)
                        {
                            FieldInfo[] fields = obj3.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                            if (fields.Length != 0)
                            {
                                foreach (FieldInfo info in fields)
                                {
                                    ProcessField(context, obj3, info, false);
                                }
                            }
                        }
                    }
                    else if (instance != null)
                    {
                        ProcessSubfields(context, field, instance);
                    }
                }
            }
            else
            {
                string[] strArray2 = (string[]) field.GetValue(fieldOwnerInstance);
                if (strArray2 != null)
                {
                    for (int i = 0; i < strArray2.Length; i++)
                    {
                        object[] objArray = extensions;
                        ProcessContentFilePath(context, ref strArray2[i], objArray, false);
                    }
                    field.SetValue(fieldOwnerInstance, strArray2);
                }
            }
        }

        private static void ProcessSubfields(MyModContext context, FieldInfo field, object instance)
        {
            foreach (FieldInfo info in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                ProcessField(context, instance, info, true);
            }
        }

        private static void ReadPrefabHeader(string file, ref List<MyObjectBuilder_PrefabDefinition> prefabs, XmlReader reader)
        {
            string name;
            MyObjectBuilder_PrefabDefinition item = new MyObjectBuilder_PrefabDefinition {
                PrefabPath = file
            };
            reader.ReadToFollowing("Id");
            bool flag = false;
            if (reader.AttributeCount >= 2)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    name = reader.Name;
                    if (name == "Type")
                    {
                        item.Id.TypeIdString = reader.Value;
                        flag = true;
                    }
                    else if (name == "Subtype")
                    {
                        item.Id.SubtypeId = reader.Value;
                    }
                }
            }
            if (!flag)
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        name = reader.Name;
                        if (name == "TypeId")
                        {
                            reader.Read();
                            item.Id.TypeIdString = reader.Value;
                            continue;
                        }
                        if (name != "SubtypeId")
                        {
                            continue;
                        }
                        reader.Read();
                        item.Id.SubtypeId = reader.Value;
                        continue;
                    }
                    if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Name == "Id"))
                    {
                        break;
                    }
                }
            }
            prefabs.Add(item);
        }

        public void RegisterFactionDefinition(MyFactionDefinition definition)
        {
            if (this.Loading)
            {
                if (this.m_definitions.m_factionDefinitionsByTag.ContainsKey(definition.Tag))
                {
                    string msg = "Faction with tag " + definition.Tag + " is already registered in the definition manager. Overwriting...";
                    MySandboxGame.Log.WriteLine(msg);
                }
                this.m_definitions.m_factionDefinitionsByTag.Add(definition.Tag, definition);
            }
        }

        public void ReloadDecalMaterials()
        {
            MyObjectBuilder_Definitions definitions = this.Load<MyObjectBuilder_Definitions>(Path.Combine(MyModContext.BaseGame.ModPathData, "Decals.sbc"));
            if (definitions.Decals != null)
            {
                InitDecals(MyModContext.BaseGame, definitions.Decals, true);
            }
            if (definitions.DecalGlobals != null)
            {
                InitDecalGlobals(MyModContext.BaseGame, definitions.DecalGlobals, true);
            }
        }

        public void ReloadHandItems()
        {
            MyModContext baseGame = MyModContext.BaseGame;
            MySandboxGame.Log.WriteLine("Loading hand items");
            string path = Path.Combine(baseGame.ModPathData, "HandItems.sbc");
            MyHandItemDefinition[] definitionArray = this.LoadHandItems(path, baseGame);
            if (this.m_definitions.m_handItemsById == null)
            {
                this.m_definitions.m_handItemsById = new DefinitionDictionary<MyHandItemDefinition>(definitionArray.Length);
            }
            else
            {
                this.m_definitions.m_handItemsById.Clear();
            }
            foreach (MyHandItemDefinition definition in definitionArray)
            {
                this.m_definitions.m_handItemsById[definition.Id] = definition;
            }
        }

        public void ReloadParticles()
        {
            MySandboxGame.Log.WriteLine("Loading particles");
            string path = Path.Combine(MyModContext.BaseGame.ModPathData, "Particles.sbc");
            if (!this.m_transparentMaterialsInitialized)
            {
                CreateTransparentMaterials();
                this.m_transparentMaterialsInitialized = true;
            }
            MyParticlesLibrary.Close();
            foreach (MyObjectBuilder_ParticleEffect effect in this.Load<MyObjectBuilder_Definitions>(path).ParticleEffects)
            {
                MyParticleEffect local1 = MyParticlesManager.EffectsPool.Allocate(false);
                local1.DeserializeFromObjectBuilder(effect);
                MyParticlesLibrary.AddParticleEffect(local1);
            }
        }

        public void ReloadPrefabsFromFile(string filePath)
        {
            MyObjectBuilder_Definitions definitions = this.LoadWithProtobuffers<MyObjectBuilder_Definitions>(filePath);
            if (definitions.Prefabs != null)
            {
                foreach (MyObjectBuilder_PrefabDefinition definition in definitions.Prefabs)
                {
                    MyPrefabDefinition prefabDefinition = this.GetPrefabDefinition(definition.Id.SubtypeId);
                    if (prefabDefinition != null)
                    {
                        prefabDefinition.InitLazy(definition);
                    }
                }
            }
        }

        public void ReloadVoxelMaterials()
        {
            using (this.m_voxelMaterialsLock.AcquireExclusiveUsing())
            {
                MyModContext baseGame = MyModContext.BaseGame;
                MyVoxelMaterialDefinition.ResetIndexing();
                MySandboxGame.Log.WriteLine("ReloadVoxelMaterials");
                List<MyVoxelMaterialDefinition> res = new List<MyVoxelMaterialDefinition>();
                this.LoadVoxelMaterials(Path.Combine(baseGame.ModPathData, "VoxelMaterials_asteroids.sbc"), baseGame, res);
                this.LoadVoxelMaterials(Path.Combine(baseGame.ModPathData, "VoxelMaterials_planetary.sbc"), baseGame, res);
                if (this.m_definitions.m_voxelMaterialsByIndex == null)
                {
                    this.m_definitions.m_voxelMaterialsByIndex = new Dictionary<byte, MyVoxelMaterialDefinition>();
                }
                else
                {
                    this.m_definitions.m_voxelMaterialsByIndex.Clear();
                }
                if (this.m_definitions.m_voxelMaterialsByName == null)
                {
                    this.m_definitions.m_voxelMaterialsByName = new Dictionary<string, MyVoxelMaterialDefinition>();
                }
                else
                {
                    this.m_definitions.m_voxelMaterialsByName.Clear();
                }
                foreach (MyVoxelMaterialDefinition definition in res)
                {
                    definition.AssignIndex();
                    this.m_definitions.m_voxelMaterialsByIndex[definition.Index] = definition;
                    this.m_definitions.m_voxelMaterialsByName[definition.Id.SubtypeName] = definition;
                }
            }
        }

        private void ReSavePrefabsProtoBuffers(MyObjectBuilder_Definitions builder)
        {
            if (MyFakes.ENABLE_RESAVE_PREFABS_TO_PROTOBUFFERS && (builder != null))
            {
                MyObjectBuilder_PrefabDefinition[] prefabs = builder.Prefabs;
                int index = 0;
                while (index < prefabs.Length)
                {
                    MyObjectBuilder_PrefabDefinition definition = prefabs[index];
                    string path = definition.PrefabPath + MyObjectBuilderSerializer.ProtobufferExtension;
                    if (MyFileSystem.FileExists(path))
                    {
                        File.Delete(path);
                    }
                    MyObjectBuilder_Definitions objectBuilder = this.LoadWithProtobuffers<MyObjectBuilder_Definitions>(definition.PrefabPath);
                    MyObjectBuilder_PrefabDefinition[] definitionArray2 = objectBuilder.Prefabs;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= definitionArray2.Length)
                        {
                            MyObjectBuilderSerializer.SerializePB(path, false, objectBuilder);
                            index++;
                            break;
                        }
                        MyObjectBuilder_PrefabDefinition definition2 = definitionArray2[num2];
                        if (definition2.CubeGrid != null)
                        {
                            definition2.CubeGrids = new MyObjectBuilder_CubeGrid[] { definition2.CubeGrid };
                        }
                        num2++;
                    }
                }
            }
        }

        public void Save(string filePattern = "*.*")
        {
            Regex regex = FindFilesPatternToRegex.Convert(filePattern);
            Dictionary<string, List<MyDefinitionBase>> dictionary = new Dictionary<string, List<MyDefinitionBase>>();
            foreach (KeyValuePair<MyDefinitionId, MyDefinitionBase> pair in this.m_definitions.m_definitionsById)
            {
                if (string.IsNullOrEmpty(pair.Value.Context.CurrentFile))
                {
                    continue;
                }
                if (regex.IsMatch(Path.GetFileName(pair.Value.Context.CurrentFile)))
                {
                    List<MyDefinitionBase> list = null;
                    if (!dictionary.ContainsKey(pair.Value.Context.CurrentFile))
                    {
                        dictionary.Add(pair.Value.Context.CurrentFile, list = new List<MyDefinitionBase>());
                    }
                    else
                    {
                        list = dictionary[pair.Value.Context.CurrentFile];
                    }
                    list.Add(pair.Value);
                }
            }
            foreach (KeyValuePair<string, List<MyDefinitionBase>> pair2 in dictionary)
            {
                MyObjectBuilder_Definitions objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
                List<MyObjectBuilder_DefinitionBase> source = new List<MyObjectBuilder_DefinitionBase>();
                using (List<MyDefinitionBase>.Enumerator enumerator3 = pair2.Value.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        MyObjectBuilder_DefinitionBase item = enumerator3.Current.GetObjectBuilder();
                        source.Add(item);
                    }
                }
                objectBuilder.CubeBlocks = source.OfType<MyObjectBuilder_CubeBlockDefinition>().ToArray<MyObjectBuilder_CubeBlockDefinition>();
                MyObjectBuilderSerializer.SerializeXML(pair2.Key, false, objectBuilder, null);
            }
        }

        public void SaveHandItems()
        {
            MyObjectBuilder_Definitions definitions = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
            List<MyObjectBuilder_HandItemDefinition> list = new List<MyObjectBuilder_HandItemDefinition>();
            using (Dictionary<MyDefinitionId, MyHandItemDefinition>.ValueCollection.Enumerator enumerator = this.m_definitions.m_handItemsById.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyObjectBuilder_HandItemDefinition objectBuilder = (MyObjectBuilder_HandItemDefinition) enumerator.Current.GetObjectBuilder();
                    list.Add(objectBuilder);
                }
            }
            definitions.HandItems = list.ToArray();
            definitions.Save(Path.Combine(MyFileSystem.ContentPath, "Data", "HandItems.sbc"));
        }

        public void SetDefaultNavDef(MyCubeBlockDefinition blockDefinition)
        {
            MyBlockNavigationDefinition definition2;
            MyObjectBuilder_BlockNavigationDefinition defaultObjectBuilder = MyBlockNavigationDefinition.GetDefaultObjectBuilder(blockDefinition);
            this.TryGetDefinition<MyBlockNavigationDefinition>(defaultObjectBuilder.Id, out definition2);
            if (definition2 != null)
            {
                blockDefinition.NavigationDefinition = definition2;
            }
            else
            {
                MyBlockNavigationDefinition.CreateDefaultTriangles(defaultObjectBuilder);
                MyBlockNavigationDefinition definition3 = InitDefinition<MyBlockNavigationDefinition>(blockDefinition.Context, defaultObjectBuilder);
                Check<SerializableDefinitionId>(!this.m_definitions.m_definitionsById.ContainsKey(defaultObjectBuilder.Id), defaultObjectBuilder.Id, true, "Duplicate entry of '{0}'");
                this.m_definitions.m_definitionsById[defaultObjectBuilder.Id] = definition3;
                blockDefinition.NavigationDefinition = definition3;
            }
        }

        internal void SetEntityContainerDefinition(MyContainerDefinition newDefinition)
        {
            if ((this.m_definitions != null) && (this.m_definitions.m_entityContainers != null))
            {
                if (!this.m_definitions.m_entityContainers.ContainsKey(newDefinition.Id))
                {
                    this.m_definitions.m_entityContainers.Add(newDefinition.Id, newDefinition);
                }
                else
                {
                    this.m_definitions.m_entityContainers[newDefinition.Id] = newDefinition;
                }
            }
        }

        public void SetShipSoundSystem()
        {
            MyShipSoundComponent.ClearShipSounds();
            foreach (DefinitionSet set in this.m_modDefinitionSets.Values)
            {
                if (set.m_shipSounds == null)
                {
                    continue;
                }
                if (set.m_shipSounds.Count > 0)
                {
                    foreach (KeyValuePair<MyDefinitionId, MyShipSoundsDefinition> pair in set.m_shipSounds)
                    {
                        MyShipSoundComponent.AddShipSounds(pair.Value);
                    }
                    if (set.m_shipSoundSystem != null)
                    {
                        MyShipSoundComponent.SetDefinition(set.m_shipSoundSystem);
                    }
                }
            }
            MyShipSoundComponent.ActualizeGroups();
        }

        private void TestCubeBlockModel(MyCubeBlockDefinition block)
        {
            if (block != null)
            {
                if (block.Model != null)
                {
                    this.TestCubeBlockModel(block.Model);
                }
                foreach (MyCubeBlockDefinition.BuildProgressModel model in block.BuildProgressModels)
                {
                    this.TestCubeBlockModel(model.File);
                }
            }
        }

        private void TestCubeBlockModel(string file)
        {
            Path.Combine(MyFileSystem.ContentPath, file);
            MyModel modelOnlyData = MyModels.GetModelOnlyData(file);
            if (MyFakes.TEST_MODELS_WRONG_TRIANGLES)
            {
                int trianglesCount = modelOnlyData.GetTrianglesCount();
                for (int i = 0; i < trianglesCount; i++)
                {
                    MyTriangleVertexIndices triangle = modelOnlyData.GetTriangle(i);
                    if (MyUtils.IsWrongTriangle(modelOnlyData.GetVertex(triangle.I0), modelOnlyData.GetVertex(triangle.I1), modelOnlyData.GetVertex(triangle.I2)))
                    {
                        break;
                    }
                }
            }
            if (modelOnlyData.LODs != null)
            {
                foreach (MyLODDescriptor descriptor in modelOnlyData.LODs)
                {
                    this.TestCubeBlockModel(descriptor.Model);
                }
            }
            modelOnlyData.UnloadData();
        }

        private void TestCubeBlockModels()
        {
            WorkOptions? options = null;
            ParallelTasks.Parallel.ForEach<string>(this.GetDefinitionPairNames(), delegate (string pair) {
                MyCubeBlockDefinitionGroup definitionGroup = this.GetDefinitionGroup(pair);
                this.TestCubeBlockModel(definitionGroup.Small);
                this.TestCubeBlockModel(definitionGroup.Large);
            }, WorkPriority.Normal, options, false);
        }

        private static void ToDefinitions(MyModContext context, DefinitionDictionary<MyDefinitionBase> outputDefinitions, DefinitionDictionary<MyCubeBlockDefinition>[] outputCubeBlocks, MyObjectBuilder_CubeBlockDefinition[] cubeBlocks, bool failOnDebug = true)
        {
            for (int i = 0; i < cubeBlocks.Length; i++)
            {
                MyObjectBuilder_CubeBlockDefinition builder = cubeBlocks[i];
                MyCubeBlockDefinition definition2 = InitDefinition<MyCubeBlockDefinition>(context, builder);
                definition2.UniqueVersion = definition2;
                outputCubeBlocks[(int) definition2.CubeSize][definition2.Id] = definition2;
                Check<MyDefinitionId>(!outputDefinitions.ContainsKey(definition2.Id), definition2.Id, failOnDebug, "Duplicate entry of '{0}'");
                outputDefinitions[definition2.Id] = definition2;
                if (!context.IsBaseGame)
                {
                    MySandboxGame.Log.WriteLine("Created definition for: " + definition2.DisplayNameText);
                }
            }
        }

        public MyAnimationDefinition TryGetAnimationDefinition(string animationSubtypeName)
        {
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_AnimationDefinition), animationSubtypeName);
            this.CheckDefinition<MyAnimationDefinition>(ref id);
            MyDefinitionBase base2 = null;
            this.m_definitions.m_definitionsById.TryGetValue(id, out base2);
            return (base2 as MyAnimationDefinition);
        }

        public MyBlueprintDefinitionBase TryGetBlueprintDefinitionByResultId(MyDefinitionId resultId) => 
            this.m_definitions.m_blueprintsByResultId.GetValueOrDefault<MyDefinitionId, MyBlueprintDefinitionBase>(resultId);

        public bool TryGetBotDefinition(MyDefinitionId id, out MyBotDefinition botDefinition)
        {
            if (this.m_definitions.m_definitionsById.ContainsKey(id))
            {
                botDefinition = this.m_definitions.m_definitionsById[id] as MyBotDefinition;
                return true;
            }
            botDefinition = null;
            return false;
        }

        public MyCubeBlockDefinition TryGetComponentBlockDefinition(MyDefinitionId componentDefId)
        {
            MyCubeBlockDefinition definition = null;
            this.m_definitions.m_componentIdToBlock.TryGetValue(componentDefId, out definition);
            return definition;
        }

        public bool TryGetComponentBlueprintDefinition(MyDefinitionId componentId, out MyBlueprintDefinitionBase componentBlueprint)
        {
            using (IEnumerator<MyBlueprintDefinitionBase> enumerator = this.GetBlueprintClass("Components").GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyBlueprintDefinitionBase current = enumerator.Current;
                    if ((current.InputItemType == typeof(MyObjectBuilder_Ingot)) && (current.Results[0].Id.SubtypeId == componentId.SubtypeId))
                    {
                        componentBlueprint = current;
                        return true;
                    }
                }
            }
            componentBlueprint = null;
            return false;
        }

        public bool TryGetComponentDefinition(MyDefinitionId id, out MyComponentDefinition definition)
        {
            MyComponentDefinition definition2;
            definition = (MyComponentDefinition) (definition2 = null);
            MyDefinitionBase base2 = definition2;
            if (!this.m_definitions.m_definitionsById.TryGetValue(id, out base2))
            {
                return false;
            }
            definition = base2 as MyComponentDefinition;
            return (definition != null);
        }

        public bool TryGetComponentSubstitutionDefinition(MyDefinitionId componentDefId, out MyComponentSubstitutionDefinition substitutionDefinition)
        {
            substitutionDefinition = null;
            using (Dictionary<MyDefinitionId, MyComponentSubstitutionDefinition>.Enumerator enumerator = this.m_definitions.m_componentSubstitutions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyDefinitionId, MyComponentSubstitutionDefinition> current = enumerator.Current;
                    if (current.Value.RequiredComponent == componentDefId)
                    {
                        substitutionDefinition = current.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetContainerDefinition(MyDefinitionId containerId, out MyContainerDefinition definition) => 
            this.m_definitions.m_entityContainers.TryGetValue(containerId, out definition);

        public bool TryGetCubeBlockDefinition(MyDefinitionId defId, out MyCubeBlockDefinition blockDefinition)
        {
            MyDefinitionBase base2;
            if (!this.m_definitions.m_definitionsById.TryGetValue(defId, out base2))
            {
                blockDefinition = null;
                return false;
            }
            blockDefinition = base2 as MyCubeBlockDefinition;
            return (blockDefinition != null);
        }

        public bool TryGetDefinition<T>(MyDefinitionId defId, out T definition) where T: MyDefinitionBase
        {
            if (!defId.TypeId.IsNull)
            {
                MyDefinitionBase base2;
                definition = base.GetDefinition<T>(defId);
                if (((T) definition) != null)
                {
                    return true;
                }
                if (this.m_definitions.m_definitionsById.TryGetValue(defId, out base2))
                {
                    definition = base2 as T;
                    return (((T) definition) != null);
                }
            }
            definition = default(T);
            return false;
        }

        public MyCubeBlockDefinitionGroup TryGetDefinitionGroup(string groupName) => 
            (this.m_definitions.m_blockGroups.ContainsKey(groupName) ? this.m_definitions.m_blockGroups[groupName] : null);

        public void TryGetDefinitionsByTypeId(MyObjectBuilderType typeId, HashSet<MyDefinitionId> definitions)
        {
            foreach (MyDefinitionId id in this.m_definitions.m_definitionsById.Keys)
            {
                if (!(id.TypeId == typeId))
                {
                    continue;
                }
                if (!definitions.Contains(id))
                {
                    definitions.Add(id);
                }
            }
        }

        public bool TryGetEntityComponentDefinition(MyDefinitionId componentId, out MyComponentDefinitionBase definition) => 
            this.m_definitions.m_entityComponentDefinitions.TryGetValue(componentId, out definition);

        public MyFactionDefinition TryGetFactionDefinition(string tag)
        {
            MyFactionDefinition definition = null;
            this.m_definitions.m_factionDefinitionsByTag.TryGetValue(tag, out definition);
            return definition;
        }

        public bool TryGetGetRopeDefinition(MyDefinitionId ropeDefId, out MyRopeDefinition definition) => 
            this.m_definitions.m_idToRope.TryGetValue(ropeDefId, out definition);

        public MyHandItemDefinition TryGetHandItemDefinition(ref MyDefinitionId id)
        {
            MyHandItemDefinition definition;
            this.m_definitions.m_handItemsById.TryGetValue(id, out definition);
            return definition;
        }

        public MyHandItemDefinition TryGetHandItemForPhysicalItem(MyDefinitionId physicalItemId)
        {
            if (this.m_definitions.m_handItemsByPhysicalItemId.ContainsKey(physicalItemId))
            {
                return this.m_definitions.m_handItemsByPhysicalItemId[physicalItemId];
            }
            MySandboxGame.Log.WriteLine($"No hand item for physical item '{physicalItemId}'");
            return null;
        }

        public bool TryGetIngotBlueprintDefinition(MyDefinitionId oreId, out MyBlueprintDefinitionBase ingotBlueprint)
        {
            using (IEnumerator<MyBlueprintDefinitionBase> enumerator = this.GetBlueprintClass("Ingots").GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyBlueprintDefinitionBase current = enumerator.Current;
                    if ((current.InputItemType == typeof(MyObjectBuilder_Ore)) && (current.Prerequisites[0].Id.SubtypeId == oreId.SubtypeId))
                    {
                        ingotBlueprint = current;
                        return true;
                    }
                }
            }
            ingotBlueprint = null;
            return false;
        }

        public bool TryGetIngotBlueprintDefinition(MyObjectBuilder_Base oreBuilder, out MyBlueprintDefinitionBase ingotBlueprint) => 
            this.TryGetIngotBlueprintDefinition(oreBuilder.GetId(), out ingotBlueprint);

        public MyMultiBlockDefinition TryGetMultiBlockDefinition(MyDefinitionId id)
        {
            if (this.m_definitions.m_definitionsById.ContainsKey(id))
            {
                return (this.m_definitions.m_definitionsById[id] as MyMultiBlockDefinition);
            }
            MySandboxGame.Log.WriteLine($"No multiblock definition '{id}'");
            return null;
        }

        public MyPhysicalItemDefinition TryGetPhysicalItemDefinition(MyDefinitionId id)
        {
            MyDefinitionBase base2;
            return (this.TryGetDefinition<MyDefinitionBase>(id, out base2) ? (base2 as MyPhysicalItemDefinition) : null);
        }

        public bool TryGetPhysicalItemDefinition(MyDefinitionId id, out MyPhysicalItemDefinition definition)
        {
            MyDefinitionBase base2;
            if (!this.TryGetDefinition<MyDefinitionBase>(id, out base2))
            {
                definition = null;
                return false;
            }
            definition = base2 as MyPhysicalItemDefinition;
            return (definition != null);
        }

        public bool TryGetProvidingComponentDefinition(MyDefinitionId componentDefId, out MyComponentSubstitutionDefinition substitutionDefinition)
        {
            substitutionDefinition = null;
            using (Dictionary<MyDefinitionId, MyComponentSubstitutionDefinition>.Enumerator enumerator = this.m_definitions.m_componentSubstitutions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<MyDefinitionId, MyComponentSubstitutionDefinition> current = enumerator.Current;
                    if (current.Value.ProvidingComponents.Keys.Contains<MyDefinitionId>(componentDefId))
                    {
                        substitutionDefinition = current.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetVoxelMapStorageDefinition(string name, out MyVoxelMapStorageDefinition definition) => 
            this.m_definitions.m_voxelMapStorages.TryGetValue(name, out definition);

        public bool TryGetVoxelMaterialDefinition(string name, out MyVoxelMaterialDefinition definition)
        {
            using (this.m_voxelMaterialsLock.AcquireSharedUsing())
            {
                return this.m_definitions.m_voxelMaterialsByName.TryGetValue(name, out definition);
            }
        }

        public bool TryGetWeaponDefinition(MyDefinitionId defId, out MyWeaponDefinition definition)
        {
            MyWeaponDefinition definition2;
            if (defId.TypeId.IsNull || !this.m_definitions.m_weaponDefinitionsById.TryGetValue(defId, out definition2))
            {
                definition = null;
                return false;
            }
            definition = definition2;
            return (definition != null);
        }

        public void UnloadData()
        {
            this.m_modDefinitionSets.Clear();
            MyCubeBlockDefinition.ClearPreloadedConstructionModels();
            this.m_definitions.Clear(true);
            this.m_definitions.m_channelEnvironmentItemsDefs.Clear();
        }

        private static void UpdateModableContent(MyModContext context, MyObjectBuilder_DefinitionBase builder)
        {
            using (Stats.Generic.Measure("UpdateModableContent", MyStatTypeEnum.Counter | MyStatTypeEnum.CurrentValue | MyStatTypeEnum.KeepInactiveLongerFlag, 200, 1, -1))
            {
                foreach (FieldInfo info in builder.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    ProcessField(context, builder, info, true);
                }
            }
        }

        public static MyDefinitionManager Static =>
            (MyDefinitionManagerBase.Static as MyDefinitionManager);

        private DefinitionSet m_definitions =>
            ((DefinitionSet) base.m_definitions);

        internal DefinitionSet LoadingSet =>
            this.m_currentLoadingSet;

        public bool Loading { get; private set; }

        public MyEnvironmentDefinition EnvironmentDefinition =>
            MySector.EnvironmentDefinition;

        public DictionaryValuesReader<string, MyCharacterDefinition> Characters =>
            new DictionaryValuesReader<string, MyCharacterDefinition>(this.m_definitions.m_characters);

        public MyShipSoundSystemDefinition GetShipSoundSystemDefinition =>
            this.m_definitions.m_shipSoundSystem;

        public int VoxelMaterialCount
        {
            get
            {
                using (this.m_voxelMaterialsLock.AcquireSharedUsing())
                {
                    return this.m_definitions.m_voxelMaterialsByName.Count;
                }
            }
        }

        public int VoxelMaterialRareCount =>
            this.m_definitions.m_voxelMaterialRareCount;

        public MyBattleDefinition BattleDefinition =>
            this.m_definitions.m_battleDefinition;

        public MyDestructionDefinition DestructionDefinition =>
            this.m_definitions.m_destructionDefinition;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDefinitionManager.<>c <>9 = new MyDefinitionManager.<>c();
            public static Func<MyVoxelMapStorageDefinition, bool> <>9__20_3;
            public static Func<MyVoxelMapStorageDefinition, bool> <>9__20_4;
            public static Func<MyVoxelMapStorageDefinition, bool> <>9__20_5;
            public static Func<string, bool> <>9__32_0;
            public static Comparison<Tuple<MyObjectBuilder_Definitions, string>> <>9__32_2;
            public static Func<MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent, bool> <>9__102_0;
            public static Func<string, string> <>9__342_1;
            public static Func<ModdableContentFileAttribute, IEnumerable<string>> <>9__342_0;

            internal bool <.ctor>b__20_3(MyVoxelMapStorageDefinition x) => 
                x.UseForProceduralRemovals;

            internal bool <.ctor>b__20_4(MyVoxelMapStorageDefinition x) => 
                x.UseForProceduralAdditions;

            internal bool <.ctor>b__20_5(MyVoxelMapStorageDefinition x) => 
                x.UseAsPrimaryProceduralAdditionShape;

            internal bool <GetDefinitionBuilders>b__32_0(string f) => 
                f.EndsWith(".sbc");

            internal int <GetDefinitionBuilders>b__32_2(Tuple<MyObjectBuilder_Definitions, string> x, Tuple<MyObjectBuilder_Definitions, string> y) => 
                x.Item2.CompareTo(y.Item2);

            internal bool <InitCubeBlocks>b__102_0(MyObjectBuilder_CubeBlockDefinition.CubeBlockComponent component) => 
                (component.Subtype == "Computer");

            internal IEnumerable<string> <ProcessField>b__342_0(ModdableContentFileAttribute s) => 
                (from ex in s.FileExtensions select "." + ex);

            internal string <ProcessField>b__342_1(string ex) => 
                ("." + ex);
        }

        internal class DefinitionDictionary<V> : Dictionary<MyDefinitionId, V> where V: MyDefinitionBase
        {
            public DefinitionDictionary(int capacity) : base(capacity, MyDefinitionId.Comparer)
            {
            }

            public void AddDefinitionSafe<T>(T definition, MyModContext context, string file, bool checkDuplicates = false) where T: V
            {
                if (!(definition.Id.TypeId != MyObjectBuilderType.Invalid))
                {
                    MyDefinitionErrors.Add(context, "Invalid definition id", TErrorSeverity.Error, true);
                }
                else
                {
                    if ((checkDuplicates || context.IsBaseGame) && base.ContainsKey(definition.Id))
                    {
                        object[] objArray1 = new object[] { "Duplicate definition ", definition.Id, " in ", file };
                        string msg = string.Concat(objArray1);
                        MyLog.Default.WriteLine(msg);
                    }
                    base[definition.Id] = definition;
                }
            }

            public void Merge(MyDefinitionManager.DefinitionDictionary<V> other)
            {
                foreach (KeyValuePair<MyDefinitionId, V> pair in other)
                {
                    if (pair.Value.Enabled)
                    {
                        base[pair.Key] = pair.Value;
                        continue;
                    }
                    base.Remove(pair.Key);
                }
            }
        }

        internal class DefinitionSet : MyDefinitionSet
        {
            private static MyDefinitionManager.DefinitionDictionary<MyDefinitionBase> m_helperDict = new MyDefinitionManager.DefinitionDictionary<MyDefinitionBase>(100);
            internal float[] m_cubeSizes;
            internal float[] m_cubeSizesOriginal;
            internal string[] m_basePrefabNames;
            internal MyDefinitionManager.DefinitionDictionary<MyCubeBlockDefinition>[] m_uniqueCubeBlocksBySize;
            internal MyDefinitionManager.DefinitionDictionary<MyDefinitionBase> m_definitionsById;
            internal MyDefinitionManager.DefinitionDictionary<MyBlueprintDefinitionBase> m_blueprintsById;
            internal MyDefinitionManager.DefinitionDictionary<MyHandItemDefinition> m_handItemsById;
            internal MyDefinitionManager.DefinitionDictionary<MyPhysicalItemDefinition> m_physicalItemsByHandItemId;
            internal MyDefinitionManager.DefinitionDictionary<MyHandItemDefinition> m_handItemsByPhysicalItemId;
            internal Dictionary<string, MyPhysicalMaterialDefinition> m_physicalMaterialsByName = new Dictionary<string, MyPhysicalMaterialDefinition>();
            internal Dictionary<string, MyVoxelMaterialDefinition> m_voxelMaterialsByName;
            internal Dictionary<byte, MyVoxelMaterialDefinition> m_voxelMaterialsByIndex;
            internal int m_voxelMaterialRareCount;
            internal List<MyPhysicalItemDefinition> m_physicalItemDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyWeaponDefinition> m_weaponDefinitionsById;
            internal MyDefinitionManager.DefinitionDictionary<MyAmmoDefinition> m_ammoDefinitionsById;
            internal List<MySpawnGroupDefinition> m_spawnGroupDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyContainerTypeDefinition> m_containerTypeDefinitions;
            internal List<MyScenarioDefinition> m_scenarioDefinitions;
            internal Dictionary<string, MyCharacterDefinition> m_characters;
            internal Dictionary<string, Dictionary<string, MyAnimationDefinition>> m_animationsBySkeletonType;
            internal MyDefinitionManager.DefinitionDictionary<MyBlueprintClassDefinition> m_blueprintClasses;
            internal List<MyGuiBlockCategoryDefinition> m_categoryClasses;
            internal Dictionary<string, MyGuiBlockCategoryDefinition> m_categories;
            internal HashSet<BlueprintClassEntry> m_blueprintClassEntries;
            internal HashSet<EnvironmentItemsEntry> m_environmentItemsEntries;
            internal HashSet<MyComponentBlockEntry> m_componentBlockEntries;
            public HashSet<MyDefinitionId> m_componentBlocks;
            public Dictionary<MyDefinitionId, MyCubeBlockDefinition> m_componentIdToBlock;
            internal MyDefinitionManager.DefinitionDictionary<MyBlueprintDefinitionBase> m_blueprintsByResultId;
            internal Dictionary<string, MyPrefabDefinition> m_prefabs;
            internal Dictionary<string, MyRespawnShipDefinition> m_respawnShips;
            internal Dictionary<string, MyDropContainerDefinition> m_dropContainers;
            internal MyDefinitionManager.DefinitionDictionary<MyAssetModifierDefinition> m_assetModifiers;
            internal Dictionary<MyStringHash, Dictionary<string, MyTextureChange>> m_assetModifiersForRender;
            internal Dictionary<string, MyWheelModelsDefinition> m_wheelModels;
            internal Dictionary<string, MyAsteroidGeneratorDefinition> m_asteroidGenerators;
            internal Dictionary<string, MyCubeBlockDefinitionGroup> m_blockGroups;
            internal Dictionary<string, Vector2I> m_blockPositions;
            internal MyDefinitionManager.DefinitionDictionary<MyAudioDefinition> m_sounds;
            internal MyDefinitionManager.DefinitionDictionary<MyShipSoundsDefinition> m_shipSounds;
            internal MyShipSoundSystemDefinition m_shipSoundSystem = new MyShipSoundSystemDefinition();
            internal MyDefinitionManager.DefinitionDictionary<MyBehaviorDefinition> m_behaviorDefinitions;
            public Dictionary<string, MyVoxelMapStorageDefinition> m_voxelMapStorages;
            public readonly Dictionary<int, List<MyDefinitionId>> m_channelEnvironmentItemsDefs = new Dictionary<int, List<MyDefinitionId>>();
            internal List<MyCharacterName> m_characterNames;
            internal MyBattleDefinition m_battleDefinition;
            internal MyDefinitionManager.DefinitionDictionary<MyPlanetGeneratorDefinition> m_planetGeneratorDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyComponentGroupDefinition> m_componentGroups;
            internal Dictionary<MyDefinitionId, MyTuple<int, MyComponentGroupDefinition>> m_componentGroupMembers;
            internal Dictionary<MyDefinitionId, MyComponentSubstitutionDefinition> m_componentSubstitutions;
            internal MyDefinitionManager.DefinitionDictionary<MyPlanetPrefabDefinition> m_planetPrefabDefinitions;
            internal Dictionary<string, Dictionary<string, MyGroupedIds>> m_groupedIds;
            internal MyDefinitionManager.DefinitionDictionary<MyScriptedGroupDefinition> m_scriptedGroupDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyPirateAntennaDefinition> m_pirateAntennaDefinitions;
            internal MyDestructionDefinition m_destructionDefinition;
            internal Dictionary<string, MyCubeBlockDefinition> m_mapMultiBlockDefToCubeBlockDef = new Dictionary<string, MyCubeBlockDefinition>();
            internal Dictionary<string, MyFactionDefinition> m_factionDefinitionsByTag = new Dictionary<string, MyFactionDefinition>();
            internal Dictionary<MyDefinitionId, MyRopeDefinition> m_idToRope = new Dictionary<MyDefinitionId, MyRopeDefinition>(MyDefinitionId.Comparer);
            internal MyDefinitionManager.DefinitionDictionary<MyGridCreateToolDefinition> m_gridCreateDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyComponentDefinitionBase> m_entityComponentDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyContainerDefinition> m_entityContainers;
            internal MyLootBagDefinition m_lootBagDefinition;
            internal MyDefinitionManager.DefinitionDictionary<MyMainMenuInventorySceneDefinition> m_mainMenuInventoryScenes;
            internal MyDefinitionManager.DefinitionDictionary<MyResearchGroupDefinition> m_researchGroupsDefinitions;
            internal MyDefinitionManager.DefinitionDictionary<MyResearchBlockDefinition> m_researchBlocksDefinitions;

            public DefinitionSet()
            {
                this.Clear(false);
            }

            public void Clear(bool unload = false)
            {
                base.Clear();
                this.m_cubeSizes = new float[typeof(MyCubeSize).GetEnumValues().Length];
                this.m_cubeSizesOriginal = new float[typeof(MyCubeSize).GetEnumValues().Length];
                this.m_basePrefabNames = new string[this.m_cubeSizes.Length * 4];
                this.m_definitionsById = new MyDefinitionManager.DefinitionDictionary<MyDefinitionBase>(100);
                this.m_voxelMaterialsByName = new Dictionary<string, MyVoxelMaterialDefinition>(10);
                this.m_voxelMaterialsByIndex = new Dictionary<byte, MyVoxelMaterialDefinition>(10);
                this.m_voxelMaterialRareCount = 0;
                this.m_physicalItemDefinitions = new List<MyPhysicalItemDefinition>(10);
                this.m_weaponDefinitionsById = new MyDefinitionManager.DefinitionDictionary<MyWeaponDefinition>(10);
                this.m_ammoDefinitionsById = new MyDefinitionManager.DefinitionDictionary<MyAmmoDefinition>(10);
                this.m_blockPositions = new Dictionary<string, Vector2I>(10);
                this.m_uniqueCubeBlocksBySize = new MyDefinitionManager.DefinitionDictionary<MyCubeBlockDefinition>[this.m_cubeSizes.Length];
                for (int i = 0; i < this.m_cubeSizes.Length; i++)
                {
                    this.m_uniqueCubeBlocksBySize[i] = new MyDefinitionManager.DefinitionDictionary<MyCubeBlockDefinition>(10);
                }
                this.m_blueprintsById = new MyDefinitionManager.DefinitionDictionary<MyBlueprintDefinitionBase>(10);
                this.m_spawnGroupDefinitions = new List<MySpawnGroupDefinition>(10);
                this.m_containerTypeDefinitions = new MyDefinitionManager.DefinitionDictionary<MyContainerTypeDefinition>(10);
                this.m_handItemsById = new MyDefinitionManager.DefinitionDictionary<MyHandItemDefinition>(10);
                this.m_physicalItemsByHandItemId = new MyDefinitionManager.DefinitionDictionary<MyPhysicalItemDefinition>(this.m_handItemsById.Count);
                this.m_handItemsByPhysicalItemId = new MyDefinitionManager.DefinitionDictionary<MyHandItemDefinition>(this.m_handItemsById.Count);
                this.m_scenarioDefinitions = new List<MyScenarioDefinition>(10);
                this.m_characters = new Dictionary<string, MyCharacterDefinition>();
                this.m_animationsBySkeletonType = new Dictionary<string, Dictionary<string, MyAnimationDefinition>>();
                this.m_blueprintClasses = new MyDefinitionManager.DefinitionDictionary<MyBlueprintClassDefinition>(10);
                this.m_blueprintClassEntries = new HashSet<BlueprintClassEntry>();
                this.m_blueprintsByResultId = new MyDefinitionManager.DefinitionDictionary<MyBlueprintDefinitionBase>(10);
                this.m_assetModifiers = new MyDefinitionManager.DefinitionDictionary<MyAssetModifierDefinition>(10);
                this.m_mainMenuInventoryScenes = new MyDefinitionManager.DefinitionDictionary<MyMainMenuInventorySceneDefinition>(10);
                this.m_environmentItemsEntries = new HashSet<EnvironmentItemsEntry>();
                this.m_componentBlockEntries = new HashSet<MyComponentBlockEntry>();
                this.m_componentBlocks = new HashSet<MyDefinitionId>(MyDefinitionId.Comparer);
                this.m_componentIdToBlock = new Dictionary<MyDefinitionId, MyCubeBlockDefinition>(MyDefinitionId.Comparer);
                this.m_categoryClasses = new List<MyGuiBlockCategoryDefinition>(0x19);
                this.m_categories = new Dictionary<string, MyGuiBlockCategoryDefinition>(0x19);
                this.m_prefabs = new Dictionary<string, MyPrefabDefinition>();
                this.m_respawnShips = new Dictionary<string, MyRespawnShipDefinition>();
                this.m_dropContainers = new Dictionary<string, MyDropContainerDefinition>();
                this.m_wheelModels = new Dictionary<string, MyWheelModelsDefinition>();
                this.m_asteroidGenerators = new Dictionary<string, MyAsteroidGeneratorDefinition>();
                this.m_sounds = new MyDefinitionManager.DefinitionDictionary<MyAudioDefinition>(10);
                this.m_shipSounds = new MyDefinitionManager.DefinitionDictionary<MyShipSoundsDefinition>(10);
                this.m_behaviorDefinitions = new MyDefinitionManager.DefinitionDictionary<MyBehaviorDefinition>(10);
                this.m_voxelMapStorages = new Dictionary<string, MyVoxelMapStorageDefinition>(0x40);
                this.m_characterNames = new List<MyCharacterName>(0x20);
                this.m_battleDefinition = new MyBattleDefinition();
                this.m_planetGeneratorDefinitions = new MyDefinitionManager.DefinitionDictionary<MyPlanetGeneratorDefinition>(5);
                this.m_componentGroups = new MyDefinitionManager.DefinitionDictionary<MyComponentGroupDefinition>(4);
                this.m_componentGroupMembers = new Dictionary<MyDefinitionId, MyTuple<int, MyComponentGroupDefinition>>();
                this.m_planetPrefabDefinitions = new MyDefinitionManager.DefinitionDictionary<MyPlanetPrefabDefinition>(5);
                this.m_groupedIds = new Dictionary<string, Dictionary<string, MyGroupedIds>>();
                this.m_scriptedGroupDefinitions = new MyDefinitionManager.DefinitionDictionary<MyScriptedGroupDefinition>(10);
                this.m_pirateAntennaDefinitions = new MyDefinitionManager.DefinitionDictionary<MyPirateAntennaDefinition>(4);
                this.m_componentSubstitutions = new Dictionary<MyDefinitionId, MyComponentSubstitutionDefinition>();
                this.m_destructionDefinition = new MyDestructionDefinition();
                this.m_mapMultiBlockDefToCubeBlockDef = new Dictionary<string, MyCubeBlockDefinition>();
                this.m_factionDefinitionsByTag.Clear();
                this.m_idToRope = new Dictionary<MyDefinitionId, MyRopeDefinition>(MyDefinitionId.Comparer);
                this.m_gridCreateDefinitions = new MyDefinitionManager.DefinitionDictionary<MyGridCreateToolDefinition>(3);
                this.m_entityComponentDefinitions = new MyDefinitionManager.DefinitionDictionary<MyComponentDefinitionBase>(10);
                this.m_entityContainers = new MyDefinitionManager.DefinitionDictionary<MyContainerDefinition>(10);
                if (unload)
                {
                    this.m_physicalMaterialsByName = new Dictionary<string, MyPhysicalMaterialDefinition>();
                }
                this.m_lootBagDefinition = null;
                this.m_researchBlocksDefinitions = new MyDefinitionManager.DefinitionDictionary<MyResearchBlockDefinition>(250);
                this.m_researchGroupsDefinitions = new MyDefinitionManager.DefinitionDictionary<MyResearchGroupDefinition>(30);
            }

            private static void MergeDefinitionLists<T>(List<T> output, List<T> input) where T: MyDefinitionBase
            {
                m_helperDict.Clear();
                foreach (MyDefinitionBase base2 in output)
                {
                    m_helperDict[base2.Id] = base2;
                }
                foreach (T local in input)
                {
                    if (local.Enabled)
                    {
                        m_helperDict[local.Id] = local;
                        continue;
                    }
                    m_helperDict.Remove(local.Id);
                }
                output.Clear();
                foreach (MyDefinitionBase base3 in m_helperDict.Values)
                {
                    output.Add((T) base3);
                }
                m_helperDict.Clear();
            }

            public void OverrideBy(MyDefinitionManager.DefinitionSet definitionSet)
            {
                base.OverrideBy(definitionSet);
                foreach (KeyValuePair<MyDefinitionId, MyGridCreateToolDefinition> pair in definitionSet.m_gridCreateDefinitions)
                {
                    this.m_gridCreateDefinitions[pair.Key] = pair.Value;
                }
                for (int i = 0; i < definitionSet.m_cubeSizes.Length; i++)
                {
                    float num2 = definitionSet.m_cubeSizes[i];
                    if (num2 != 0f)
                    {
                        this.m_cubeSizes[i] = num2;
                        this.m_cubeSizesOriginal[i] = definitionSet.m_cubeSizesOriginal[i];
                    }
                }
                for (int j = 0; j < definitionSet.m_basePrefabNames.Length; j++)
                {
                    if (!string.IsNullOrEmpty(definitionSet.m_basePrefabNames[j]))
                    {
                        this.m_basePrefabNames[j] = definitionSet.m_basePrefabNames[j];
                    }
                }
                this.m_definitionsById.Merge(definitionSet.m_definitionsById);
                foreach (KeyValuePair<string, MyVoxelMaterialDefinition> pair2 in definitionSet.m_voxelMaterialsByName)
                {
                    this.m_voxelMaterialsByName[pair2.Key] = pair2.Value;
                }
                foreach (KeyValuePair<string, MyPhysicalMaterialDefinition> pair3 in definitionSet.m_physicalMaterialsByName)
                {
                    this.m_physicalMaterialsByName[pair3.Key] = pair3.Value;
                }
                MergeDefinitionLists<MyPhysicalItemDefinition>(this.m_physicalItemDefinitions, definitionSet.m_physicalItemDefinitions);
                foreach (KeyValuePair<string, Vector2I> pair4 in definitionSet.m_blockPositions)
                {
                    this.m_blockPositions[pair4.Key] = pair4.Value;
                }
                for (int k = 0; k < definitionSet.m_uniqueCubeBlocksBySize.Length; k++)
                {
                    foreach (KeyValuePair<MyDefinitionId, MyCubeBlockDefinition> pair5 in definitionSet.m_uniqueCubeBlocksBySize[k])
                    {
                        this.m_uniqueCubeBlocksBySize[k][pair5.Key] = pair5.Value;
                    }
                }
                this.m_blueprintsById.Merge(definitionSet.m_blueprintsById);
                MergeDefinitionLists<MySpawnGroupDefinition>(this.m_spawnGroupDefinitions, definitionSet.m_spawnGroupDefinitions);
                this.m_containerTypeDefinitions.Merge(definitionSet.m_containerTypeDefinitions);
                this.m_handItemsById.Merge(definitionSet.m_handItemsById);
                MergeDefinitionLists<MyScenarioDefinition>(this.m_scenarioDefinitions, definitionSet.m_scenarioDefinitions);
                foreach (KeyValuePair<string, MyCharacterDefinition> pair6 in definitionSet.m_characters)
                {
                    if (pair6.Value.Enabled)
                    {
                        this.m_characters[pair6.Key] = pair6.Value;
                        continue;
                    }
                    this.m_characters.Remove(pair6.Key);
                }
                this.m_blueprintClasses.Merge(definitionSet.m_blueprintClasses);
                foreach (MyGuiBlockCategoryDefinition definition in definitionSet.m_categoryClasses)
                {
                    this.m_categoryClasses.Add(definition);
                    string key = definition.Name;
                    MyGuiBlockCategoryDefinition definition2 = null;
                    if (!this.m_categories.TryGetValue(key, out definition2))
                    {
                        this.m_categories.Add(key, definition);
                        continue;
                    }
                    definition2.ItemIds.UnionWith(definition.ItemIds);
                }
                foreach (BlueprintClassEntry entry in definitionSet.m_blueprintClassEntries)
                {
                    if (this.m_blueprintClassEntries.Contains(entry))
                    {
                        if (entry.Enabled)
                        {
                            continue;
                        }
                        this.m_blueprintClassEntries.Remove(entry);
                        continue;
                    }
                    if (entry.Enabled)
                    {
                        this.m_blueprintClassEntries.Add(entry);
                    }
                }
                this.m_blueprintsByResultId.Merge(definitionSet.m_blueprintsByResultId);
                foreach (EnvironmentItemsEntry entry2 in definitionSet.m_environmentItemsEntries)
                {
                    if (this.m_environmentItemsEntries.Contains(entry2))
                    {
                        if (entry2.Enabled)
                        {
                            continue;
                        }
                        this.m_environmentItemsEntries.Remove(entry2);
                        continue;
                    }
                    if (entry2.Enabled)
                    {
                        this.m_environmentItemsEntries.Add(entry2);
                    }
                }
                foreach (MyComponentBlockEntry entry3 in definitionSet.m_componentBlockEntries)
                {
                    if (this.m_componentBlockEntries.Contains(entry3))
                    {
                        if (entry3.Enabled)
                        {
                            continue;
                        }
                        this.m_componentBlockEntries.Remove(entry3);
                        continue;
                    }
                    if (entry3.Enabled)
                    {
                        this.m_componentBlockEntries.Add(entry3);
                    }
                }
                foreach (KeyValuePair<string, MyPrefabDefinition> pair7 in definitionSet.m_prefabs)
                {
                    if (pair7.Value.Enabled)
                    {
                        this.m_prefabs[pair7.Key] = pair7.Value;
                        continue;
                    }
                    this.m_prefabs.Remove(pair7.Key);
                }
                foreach (KeyValuePair<string, MyRespawnShipDefinition> pair8 in definitionSet.m_respawnShips)
                {
                    if (pair8.Value.Enabled)
                    {
                        this.m_respawnShips[pair8.Key] = pair8.Value;
                        continue;
                    }
                    this.m_respawnShips.Remove(pair8.Key);
                }
                foreach (KeyValuePair<string, MyDropContainerDefinition> pair9 in definitionSet.m_dropContainers)
                {
                    if (pair9.Value.Enabled)
                    {
                        this.m_dropContainers[pair9.Key] = pair9.Value;
                        continue;
                    }
                    this.m_dropContainers.Remove(pair9.Key);
                }
                foreach (KeyValuePair<string, MyWheelModelsDefinition> pair10 in definitionSet.m_wheelModels)
                {
                    if (pair10.Value.Enabled)
                    {
                        this.m_wheelModels[pair10.Key] = pair10.Value;
                        continue;
                    }
                    this.m_wheelModels.Remove(pair10.Key);
                }
                foreach (KeyValuePair<string, MyAsteroidGeneratorDefinition> pair11 in definitionSet.m_asteroidGenerators)
                {
                    if (pair11.Value.Enabled)
                    {
                        this.m_asteroidGenerators[pair11.Key] = pair11.Value;
                        continue;
                    }
                    this.m_asteroidGenerators.Remove(pair11.Key);
                }
                foreach (KeyValuePair<MyDefinitionId, MyAssetModifierDefinition> pair12 in definitionSet.m_assetModifiers)
                {
                    if (pair12.Value.Enabled)
                    {
                        this.m_assetModifiers[pair12.Key] = pair12.Value;
                        continue;
                    }
                    this.m_assetModifiers.Remove(pair12.Key);
                }
                foreach (KeyValuePair<MyDefinitionId, MyMainMenuInventorySceneDefinition> pair13 in definitionSet.m_mainMenuInventoryScenes)
                {
                    if (pair13.Value.Enabled)
                    {
                        this.m_mainMenuInventoryScenes[pair13.Key] = pair13.Value;
                        continue;
                    }
                    this.m_mainMenuInventoryScenes.Remove(pair13.Key);
                }
                foreach (KeyValuePair<string, Dictionary<string, MyAnimationDefinition>> pair14 in definitionSet.m_animationsBySkeletonType)
                {
                    foreach (KeyValuePair<string, MyAnimationDefinition> pair15 in pair14.Value)
                    {
                        if (!pair15.Value.Enabled)
                        {
                            this.m_animationsBySkeletonType[pair14.Key].Remove(pair15.Value.Id.SubtypeName);
                            continue;
                        }
                        if (!this.m_animationsBySkeletonType.ContainsKey(pair14.Key))
                        {
                            this.m_animationsBySkeletonType[pair14.Key] = new Dictionary<string, MyAnimationDefinition>();
                        }
                        this.m_animationsBySkeletonType[pair14.Key][pair15.Value.Id.SubtypeName] = pair15.Value;
                    }
                }
                foreach (KeyValuePair<MyDefinitionId, MyAudioDefinition> pair16 in definitionSet.m_sounds)
                {
                    this.m_sounds[pair16.Key] = pair16.Value;
                }
                this.m_weaponDefinitionsById.Merge(definitionSet.m_weaponDefinitionsById);
                this.m_ammoDefinitionsById.Merge(definitionSet.m_ammoDefinitionsById);
                this.m_behaviorDefinitions.Merge(definitionSet.m_behaviorDefinitions);
                foreach (KeyValuePair<string, MyVoxelMapStorageDefinition> pair17 in definitionSet.m_voxelMapStorages)
                {
                    this.m_voxelMapStorages[pair17.Key] = pair17.Value;
                }
                foreach (MyCharacterName name in definitionSet.m_characterNames)
                {
                    this.m_characterNames.Add(name);
                }
                if ((definitionSet.m_battleDefinition != null) && definitionSet.m_battleDefinition.Enabled)
                {
                    this.m_battleDefinition.Merge(definitionSet.m_battleDefinition);
                }
                foreach (KeyValuePair<MyDefinitionId, MyComponentSubstitutionDefinition> pair18 in definitionSet.m_componentSubstitutions)
                {
                    this.m_componentSubstitutions[pair18.Key] = pair18.Value;
                }
                this.m_componentGroups.Merge(definitionSet.m_componentGroups);
                foreach (KeyValuePair<MyDefinitionId, MyPlanetGeneratorDefinition> pair19 in definitionSet.m_planetGeneratorDefinitions)
                {
                    if (pair19.Value.Enabled)
                    {
                        this.m_planetGeneratorDefinitions[pair19.Key] = pair19.Value;
                        continue;
                    }
                    this.m_planetGeneratorDefinitions.Remove(pair19.Key);
                }
                foreach (KeyValuePair<MyDefinitionId, MyPlanetPrefabDefinition> pair20 in definitionSet.m_planetPrefabDefinitions)
                {
                    if (pair20.Value.Enabled)
                    {
                        this.m_planetPrefabDefinitions[pair20.Key] = pair20.Value;
                        continue;
                    }
                    this.m_planetPrefabDefinitions.Remove(pair20.Key);
                }
                foreach (KeyValuePair<string, Dictionary<string, MyGroupedIds>> pair21 in definitionSet.m_groupedIds)
                {
                    if (this.m_groupedIds.ContainsKey(pair21.Key))
                    {
                        Dictionary<string, MyGroupedIds> dictionary = this.m_groupedIds[pair21.Key];
                        foreach (KeyValuePair<string, MyGroupedIds> pair22 in pair21.Value)
                        {
                            dictionary[pair22.Key] = pair22.Value;
                        }
                        continue;
                    }
                    this.m_groupedIds[pair21.Key] = pair21.Value;
                }
                this.m_scriptedGroupDefinitions.Merge(definitionSet.m_scriptedGroupDefinitions);
                this.m_pirateAntennaDefinitions.Merge(definitionSet.m_pirateAntennaDefinitions);
                if ((definitionSet.m_destructionDefinition != null) && definitionSet.m_destructionDefinition.Enabled)
                {
                    this.m_destructionDefinition.Merge(definitionSet.m_destructionDefinition);
                }
                foreach (KeyValuePair<string, MyCubeBlockDefinition> pair23 in definitionSet.m_mapMultiBlockDefToCubeBlockDef)
                {
                    if (this.m_mapMultiBlockDefToCubeBlockDef.ContainsKey(pair23.Key))
                    {
                        this.m_mapMultiBlockDefToCubeBlockDef.Remove(pair23.Key);
                    }
                    this.m_mapMultiBlockDefToCubeBlockDef.Add(pair23.Key, pair23.Value);
                }
                foreach (KeyValuePair<MyDefinitionId, MyRopeDefinition> pair24 in definitionSet.m_idToRope)
                {
                    this.m_idToRope[pair24.Key] = pair24.Value;
                }
                this.m_entityComponentDefinitions.Merge(definitionSet.m_entityComponentDefinitions);
                this.m_entityContainers.Merge(definitionSet.m_entityContainers);
                this.m_lootBagDefinition = definitionSet.m_lootBagDefinition;
                this.m_researchBlocksDefinitions.Merge(definitionSet.m_researchBlocksDefinitions);
                this.m_researchGroupsDefinitions.Merge(definitionSet.m_researchGroupsDefinitions);
            }
        }

        private class SoundsData : WorkData
        {
            public ListReader<MySoundData> SoundData { get; set; }

            public ListReader<MyAudioEffect> EffectData { get; set; }
        }
    }
}

