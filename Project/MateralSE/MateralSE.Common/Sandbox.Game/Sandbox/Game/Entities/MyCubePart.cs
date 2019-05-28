namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Models;
    using VRage.Utils;
    using VRageMath;

    public class MyCubePart
    {
        public MyCubeInstanceData InstanceData;
        public MyModel Model;
        public MyStringHash SkinSubtypeId;

        public void Init(MyModel model, MyStringHash skinSubtypeId, Matrix matrix, float rescaleModel = 1f)
        {
            this.Model = model;
            model.Rescale(rescaleModel);
            model.LoadData();
            this.SkinSubtypeId = skinSubtypeId;
            this.InstanceData.LocalMatrix = matrix;
        }
    }
}

