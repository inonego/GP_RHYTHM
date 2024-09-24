using System;
using System.Collections.Generic;

/// <summary>
/// Do not use Insert() or AddRange() :)
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class SortedList<T> : List<T>
{
    /// <summary>
    /// Adds the item to the list, keeps the list sorted. O(log n)
    /// </summary>
    /// <param name="item">item to be added</param>
    public new void Add(T item)
    {
        int index = BinarySearch(item);

        if (index >= 0)
        {
            base.Insert(index, item);
        }
        else
        {
            index = ~index;

            if (index < Count)
            {
                base.Insert(index, item);
            }
            else
            {
                base.Add(item);
            }
        }
    }

    public new void Insert(int index, T item)
    {
        throw new Exception("Do not use Insert!");
    }

    [Obsolete]
    public new void AddRange(IEnumerable<T> collection)
    {
        throw new Exception("Do not use AddRange!");
    }
}