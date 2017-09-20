using System.Collections.Generic;

namespace FileManager
{
    /// <summary>
    /// A helper class to add extensions to List<T>
    /// </summary>
    public static class ListHelper
    {
        /// <summary>
        /// An extension to get a clone of a List's data
        /// </summary>
        /// <typeparam name="T">The type of the List's data</typeparam>
        /// <param name="original">The original List object</param>
        /// <returns>A clone of the original List</returns>
        public static List<T> Clone<T>(this List<T> original)
        {
            T[] data = new T[original.Count];
            original.CopyTo(data);
            return new List<T>(data);
        }
    }
}