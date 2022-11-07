using System;

namespace Game
{
    public static class ArrayExtensions
    {
        private static readonly Random _random = new(Guid.NewGuid().GetHashCode());
        
        /// <summary>
        /// Returns random element from the array.
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Random array element.</returns>
        /// <exception cref="ArgumentException">when array contains no elements.</exception>
        public static T GetRandom<T>(this T[] array)
        {
            var length = array.Length;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (length == 0) throw new ArgumentException("Can't get a random element from empty array", nameof(array));

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (length == 1)
                return array[0];

            return array[_random.Next(0, length)];
        }
    }
}