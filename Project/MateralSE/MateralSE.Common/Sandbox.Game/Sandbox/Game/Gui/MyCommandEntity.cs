namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Game.Entity;

    [PreloadRequired]
    public class MyCommandEntity : MyCommand
    {
        static MyCommandEntity()
        {
            MyConsole.AddCommand(new MyCommandEntity());
        }

        private MyCommandEntity()
        {
            MyCommand.MyCommandAction action1 = new MyCommand.MyCommandAction();
            action1.AutocompleteHint = new StringBuilder("long_EntityId string_NewDisplayName");
            action1.Parser = x => this.ParseDisplayName(x);
            action1.CallAction = x => this.ChangeDisplayName(x);
            base.m_methods.Add("SetDisplayName", action1);
            MyCommand.MyCommandAction action2 = new MyCommand.MyCommandAction();
            action2.Parser = x => this.ParseDisplayName(x);
            action2.CallAction = x => this.ChangeDisplayName(x);
            base.m_methods.Add("MethodA", action2);
            MyCommand.MyCommandAction action3 = new MyCommand.MyCommandAction();
            action3.Parser = x => this.ParseDisplayName(x);
            action3.CallAction = x => this.ChangeDisplayName(x);
            base.m_methods.Add("MethodB", action3);
            MyCommand.MyCommandAction action4 = new MyCommand.MyCommandAction();
            action4.Parser = x => this.ParseDisplayName(x);
            action4.CallAction = x => this.ChangeDisplayName(x);
            base.m_methods.Add("MethodC", action4);
            MyCommand.MyCommandAction action5 = new MyCommand.MyCommandAction();
            action5.Parser = x => this.ParseDisplayName(x);
            action5.CallAction = x => this.ChangeDisplayName(x);
            base.m_methods.Add("MethodD", action5);
        }

        private StringBuilder ChangeDisplayName(MyCommandArgs args)
        {
            MyEntity entity;
            MyCommandArgsDisplayName name = args as MyCommandArgsDisplayName;
            if (!MyEntities.TryGetEntityById(name.EntityId, out entity, false))
            {
                return new StringBuilder().Append("Entity not found");
            }
            if (name.newDisplayName == null)
            {
                return new StringBuilder().Append("Invalid Display name");
            }
            string displayName = entity.DisplayName;
            entity.DisplayName = name.newDisplayName;
            return new StringBuilder().Append("Changed name from entity ").Append(name.EntityId).Append(" from ").Append(displayName).Append(" to ").Append(entity.DisplayName);
        }

        private MyCommandArgs ParseDisplayName(List<string> args)
        {
            MyCommandArgsDisplayName name1 = new MyCommandArgsDisplayName();
            name1.EntityId = long.Parse(args[0]);
            name1.newDisplayName = args[1];
            return name1;
        }

        public override string Prefix() => 
            "Entity";

        private class MyCommandArgsDisplayName : MyCommandArgs
        {
            public long EntityId;
            public string newDisplayName;
        }
    }
}

