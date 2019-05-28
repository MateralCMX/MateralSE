namespace VRage
{
    using System;
    using VRage.ModAPI;

    public class DuplicateIdException : Exception
    {
        public IMyEntity NewEntity;
        public IMyEntity OldEntity;

        public DuplicateIdException(IMyEntity newEntity, IMyEntity oldEntity)
        {
            this.NewEntity = newEntity;
            this.OldEntity = oldEntity;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "newEntity: ", this.OldEntity.GetType(), ", oldEntity: ", this.NewEntity.GetType(), base.ToString() };
            return string.Concat(objArray1);
        }
    }
}

