namespace Sandbox.AppCode
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.ExternalApp;

    public class MyExternalAppBase : IExternalApp
    {
        public static MySandboxGame Static;
        private static bool m_isEditorActive;
        private static bool m_isPresent;

        public void AddParticleToLibrary(MyParticleEffect effect)
        {
            MyParticlesLibrary.AddParticleEffect(effect);
        }

        public MyParticleGeneration AllocateGeneration()
        {
            MyParticleGeneration generation;
            MyParticlesManager.GenerationsPool.AllocateOrCreate(out generation);
            return generation;
        }

        public MyParticleGPUGeneration AllocateGPUGeneration()
        {
            MyParticleGPUGeneration generation;
            MyParticlesManager.GPUGenerationsPool.AllocateOrCreate(out generation);
            return generation;
        }

        public MyParticleLight AllocateParticleLight()
        {
            MyParticleLight light;
            MyParticlesManager.LightsPool.AllocateOrCreate(out light);
            return light;
        }

        public MyParticleSound AllocateParticleSound()
        {
            MyParticleSound sound;
            MyParticlesManager.SoundsPool.AllocateOrCreate(out sound);
            return sound;
        }

        public MyParticleEffect CreateLibraryEffect() => 
            MyParticlesManager.EffectsPool.Allocate(false);

        public MyParticleEffect CreateParticle(string name, MatrixD worldMatrix)
        {
            Vector3D translation = worldMatrix.Translation;
            return MyParticlesLibrary.CreateParticleEffect(name, ref worldMatrix, ref translation, uint.MaxValue);
        }

        public void Dispose()
        {
            Static.Dispose();
            Static = null;
        }

        public virtual void Draw(bool canDraw)
        {
        }

        public void EndLoop()
        {
            Static.EndLoop();
        }

        public void FlushParticles()
        {
            using (List<string>.Enumerator enumerator = new List<string>(MyParticlesLibrary.GetParticleEffectsNames()).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyParticlesLibrary.RemoveParticleEffect(enumerator.Current, true);
                }
            }
        }

        public virtual void GameExit()
        {
        }

        public virtual void GameLoaded(object sender, EventArgs e)
        {
            IsEditorActive = true;
            IsPresent = true;
        }

        public IReadOnlyDictionary<int, MyParticleEffect> GetLibraryEffects() => 
            MyParticlesLibrary.GetParticleEffectsById();

        public IReadOnlyDictionary<string, MyParticleEffect> GetParticleEffectsByName() => 
            MyParticlesLibrary.GetParticleEffectsByName();

        public MatrixD GetSpectatorMatrix() => 
            ((MySpectatorCameraController.Static == null) ? MatrixD.Identity : MatrixD.Invert(MySpectatorCameraController.Static.GetViewMatrix()));

        public float GetStepInSeconds() => 
            0.01666667f;

        public virtual void Initialize(Sandbox.Engine.Platform.Game game)
        {
        }

        public void LoadDefinitions()
        {
            MyDefinitionManager.Static.LoadData(new List<MyObjectBuilder_Checkpoint.ModItem>());
        }

        public void LoadParticlesLibrary(string file)
        {
            if (file.Contains(".mwl"))
            {
                MyParticlesLibrary.Deserialize(file);
            }
            else
            {
                MyDataIntegrityChecker.HashInFile(file);
                MyObjectBuilder_Definitions objectBuilder = null;
                MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(file, out objectBuilder);
                if ((objectBuilder != null) && (objectBuilder.ParticleEffects != null))
                {
                    MyParticlesLibrary.Close();
                    foreach (MyObjectBuilder_ParticleEffect effect in objectBuilder.ParticleEffects)
                    {
                        MyParticleEffect local1 = MyParticlesManager.EffectsPool.Allocate(false);
                        local1.DeserializeFromObjectBuilder(effect);
                        MyParticlesLibrary.AddParticleEffect(local1);
                    }
                }
            }
        }

        public virtual void MySession_AfterLoading()
        {
        }

        public virtual void MySession_BeforeLoading()
        {
        }

        public void RemoveParticle(MyParticleEffect effect)
        {
            MyParticlesLibrary.RemoveParticleEffectInstance(effect);
        }

        public void RemoveParticleFromLibrary(string name)
        {
            MyParticlesLibrary.RemoveParticleEffect(name, true);
        }

        public void Run(IntPtr windowHandle, bool customRenderLoop = false, MySandboxGame game = null)
        {
            MyLog.Default = MySandboxGame.Log;
            MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING = false;
            Static = (game != null) ? game : new MySandboxExternal(this, null, windowHandle);
            this.Initialize(Static);
            Static.OnGameLoaded += new EventHandler(this.GameLoaded);
            Static.OnGameExit += new Action(this.GameExit);
            MySession.AfterLoading += new Action(this.MySession_AfterLoading);
            MySession.BeforeLoading += new Action(this.MySession_BeforeLoading);
            Static.Run(customRenderLoop, null);
            if (!customRenderLoop)
            {
                this.Dispose();
            }
        }

        public void RunSingleFrame()
        {
            Static.RunSingleFrame();
        }

        public void SaveParticlesLibrary(string file)
        {
            MyParticlesLibrary.Serialize(file);
        }

        public virtual void Update(bool canDraw)
        {
        }

        public virtual void UpdateMainThread()
        {
        }

        void IExternalApp.Draw()
        {
            this.Draw(false);
        }

        void IExternalApp.Update()
        {
            this.Update(true);
        }

        void IExternalApp.UpdateMainThread()
        {
            this.UpdateMainThread();
        }

        public static bool IsEditorActive
        {
            get => 
                m_isEditorActive;
            set => 
                (m_isEditorActive = value);
        }

        public static bool IsPresent
        {
            get => 
                m_isPresent;
            set => 
                (m_isPresent = value);
        }
    }
}

