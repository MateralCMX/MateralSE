namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AmmoMagazineDefinition), (Type) null)]
    public class MyAmmoMagazineDefinition : MyPhysicalItemDefinition
    {
        public int Capacity;
        public MyAmmoCategoryEnum Category;
        public MyDefinitionId AmmoDefinitionId;

        private MyDefinitionId GetAmmoDefinitionIdFromCategory(MyAmmoCategoryEnum category)
        {
            switch (category)
            {
                case MyAmmoCategoryEnum.SmallCaliber:
                    return new MyDefinitionId(typeof(MyObjectBuilder_AmmoDefinition), "SmallCaliber");

                case MyAmmoCategoryEnum.LargeCaliber:
                    return new MyDefinitionId(typeof(MyObjectBuilder_AmmoDefinition), "LargeCaliber");

                case MyAmmoCategoryEnum.Missile:
                    return new MyDefinitionId(typeof(MyObjectBuilder_AmmoDefinition), "Missile");
            }
            throw new NotImplementedException();
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AmmoMagazineDefinition definition = builder as MyObjectBuilder_AmmoMagazineDefinition;
            this.Capacity = definition.Capacity;
            this.Category = definition.Category;
            if (definition.AmmoDefinitionId != null)
            {
                this.AmmoDefinitionId = new MyDefinitionId(definition.AmmoDefinitionId.Type, definition.AmmoDefinitionId.Subtype);
            }
            else
            {
                this.AmmoDefinitionId = this.GetAmmoDefinitionIdFromCategory(this.Category);
            }
        }
    }
}

