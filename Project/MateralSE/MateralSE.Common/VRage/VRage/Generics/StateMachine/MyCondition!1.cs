namespace VRage.Generics.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage.Generics;
    using VRage.Library.Utils;
    using VRage.Utils;

    public class MyCondition<T> : IMyCondition where T: struct
    {
        private readonly IMyVariableStorage<T> m_storage;
        private readonly MyOperation<T> m_operation;
        private readonly MyStringId m_leftSideStorage;
        private readonly MyStringId m_rightSideStorage;
        private readonly T m_leftSideValue;
        private readonly T m_rightSideValue;

        public MyCondition(IMyVariableStorage<T> storage, MyOperation<T> operation, string leftSideStorage, string rightSideStorage)
        {
            this.m_storage = storage;
            this.m_operation = operation;
            this.m_leftSideStorage = MyStringId.GetOrCompute(leftSideStorage);
            this.m_rightSideStorage = MyStringId.GetOrCompute(rightSideStorage);
        }

        public MyCondition(IMyVariableStorage<T> storage, MyOperation<T> operation, string leftSideStorage, T rightSideValue)
        {
            this.m_storage = storage;
            this.m_operation = operation;
            this.m_leftSideStorage = MyStringId.GetOrCompute(leftSideStorage);
            this.m_rightSideStorage = MyStringId.NullOrEmpty;
            this.m_rightSideValue = rightSideValue;
        }

        public MyCondition(IMyVariableStorage<T> storage, MyOperation<T> operation, T leftSideValue, string rightSideStorage)
        {
            this.m_storage = storage;
            this.m_operation = operation;
            this.m_leftSideStorage = MyStringId.NullOrEmpty;
            this.m_rightSideStorage = MyStringId.GetOrCompute(rightSideStorage);
            this.m_leftSideValue = leftSideValue;
        }

        public MyCondition(IMyVariableStorage<T> storage, MyOperation<T> operation, T leftSideValue, T rightSideValue)
        {
            this.m_storage = storage;
            this.m_operation = operation;
            this.m_leftSideStorage = MyStringId.NullOrEmpty;
            this.m_rightSideStorage = MyStringId.NullOrEmpty;
            this.m_leftSideValue = leftSideValue;
            this.m_rightSideValue = rightSideValue;
        }

        public bool Evaluate()
        {
            T leftSideValue;
            T rightSideValue;
            if (!(this.m_leftSideStorage != MyStringId.NullOrEmpty))
            {
                leftSideValue = this.m_leftSideValue;
            }
            else if (!this.m_storage.GetValue(this.m_leftSideStorage, out leftSideValue))
            {
                return false;
            }
            if (!(this.m_rightSideStorage != MyStringId.NullOrEmpty))
            {
                rightSideValue = this.m_rightSideValue;
            }
            else if (!this.m_storage.GetValue(this.m_rightSideStorage, out rightSideValue))
            {
                return false;
            }
            int num = Comparer<T>.Default.Compare(leftSideValue, rightSideValue);
            switch (this.m_operation)
            {
                case MyOperation<T>.AlwaysFalse:
                    return false;

                case MyOperation<T>.AlwaysTrue:
                    return true;

                case MyOperation<T>.NotEqual:
                    return (num != 0);

                case MyOperation<T>.Less:
                    return (num < 0);

                case MyOperation<T>.LessOrEqual:
                    return (num <= 0);

                case MyOperation<T>.Equal:
                    return (num == 0);

                case MyOperation<T>.GreaterOrEqual:
                    return (num >= 0);

                case MyOperation<T>.Greater:
                    return (num > 0);
            }
            return false;
        }

        public override string ToString()
        {
            if (this.m_operation == MyOperation<T>.AlwaysTrue)
            {
                return "true";
            }
            if (this.m_operation == MyOperation<T>.AlwaysFalse)
            {
                return "false";
            }
            StringBuilder builder = new StringBuilder(0x80);
            if (this.m_leftSideStorage != MyStringId.NullOrEmpty)
            {
                builder.Append(this.m_leftSideStorage.ToString());
            }
            else
            {
                builder.Append(this.m_leftSideValue);
            }
            builder.Append(" ");
            switch (this.m_operation)
            {
                case MyOperation<T>.NotEqual:
                    builder.Append("!=");
                    break;

                case MyOperation<T>.Less:
                    builder.Append("<");
                    break;

                case MyOperation<T>.LessOrEqual:
                    builder.Append("<=");
                    break;

                case MyOperation<T>.Equal:
                    builder.Append("==");
                    break;

                case MyOperation<T>.GreaterOrEqual:
                    builder.Append(">=");
                    break;

                case MyOperation<T>.Greater:
                    builder.Append(">");
                    break;

                default:
                    builder.Append("???");
                    break;
            }
            builder.Append(" ");
            if (this.m_rightSideStorage != MyStringId.NullOrEmpty)
            {
                builder.Append(this.m_rightSideStorage.ToString());
            }
            else
            {
                builder.Append(this.m_rightSideValue);
            }
            return builder.ToString();
        }

        public enum MyOperation
        {
            public const MyCondition<T>.MyOperation AlwaysFalse = MyCondition<T>.MyOperation.AlwaysFalse;,
            public const MyCondition<T>.MyOperation AlwaysTrue = MyCondition<T>.MyOperation.AlwaysTrue;,
            public const MyCondition<T>.MyOperation NotEqual = MyCondition<T>.MyOperation.NotEqual;,
            public const MyCondition<T>.MyOperation Less = MyCondition<T>.MyOperation.Less;,
            public const MyCondition<T>.MyOperation LessOrEqual = MyCondition<T>.MyOperation.LessOrEqual;,
            public const MyCondition<T>.MyOperation Equal = MyCondition<T>.MyOperation.Equal;,
            public const MyCondition<T>.MyOperation GreaterOrEqual = MyCondition<T>.MyOperation.GreaterOrEqual;,
            public const MyCondition<T>.MyOperation Greater = MyCondition<T>.MyOperation.Greater;
        }
    }
}

