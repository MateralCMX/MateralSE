namespace System
{
    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            this.maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; i++)
            {
                this.maxLengths[i] = array.GetLength(i) - 1;
            }
            this.Position = new int[array.Rank];
        }

        public unsafe bool Step()
        {
            for (int i = 0; i < this.Position.Length; i++)
            {
                if (this.Position[i] < this.maxLengths[i])
                {
                    int* numPtr1 = (int*) ref this.Position[i];
                    numPtr1[0]++;
                    for (int j = 0; j < i; j++)
                    {
                        this.Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

