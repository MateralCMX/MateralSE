namespace VRage.Library.Utils
{
    using System;
    using System.Runtime.CompilerServices;

    public static class MyIterableComplementSetExtensions
    {
        public static void AddOrEnsureOnComplement<T>(this MyIterableComplementSet<T> self, T item)
        {
            if (!self.Contains(item))
            {
                self.AddToComplement(item);
            }
            else if (!self.IsInComplement(item))
            {
                self.MoveToComplement(item);
            }
        }

        public static void AddOrEnsureOnSet<T>(this MyIterableComplementSet<T> self, T item)
        {
            if (!self.Contains(item))
            {
                self.Add(item);
            }
            else if (self.IsInComplement(item))
            {
                self.MoveToSet(item);
            }
        }

        public static void EnsureOnComplementIfContained<T>(this MyIterableComplementSet<T> self, T item)
        {
            if (self.Contains(item) && !self.IsInComplement(item))
            {
                self.MoveToComplement(item);
            }
        }

        public static void EnsureOnSetIfContained<T>(this MyIterableComplementSet<T> self, T item)
        {
            if (self.Contains(item) && self.IsInComplement(item))
            {
                self.MoveToSet(item);
            }
        }

        public static void RemoveIfContained<T>(this MyIterableComplementSet<T> self, T item)
        {
            if (self.Contains(item))
            {
                self.Remove(item);
            }
        }
    }
}

