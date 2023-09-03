using System;
using System.Collections.Generic;

public class Pile<T>
{
    private List<T> entries = new List<T>();
    public int Count => entries.Count;

    public void Add(T entry)
    {
        entries.Add(entry);
    }

    public void Add(IEnumerable<T> entries)
    {
        this.entries.AddRange(entries);
    }

    public void AddAt(T entry, int index)
    {
        entries.Insert(index, entry);
    }

    public void Remove(T entry)
    {
        entries.Remove(entry);
    }

    public void RemoveFromIndex(int index)
    {
        entries.RemoveAt(index);
    }

    public void Clear()
    {
        entries.Clear();
    }

    public bool Contains(T entry)
    {
        return entries.Contains(entry);
    }

    public T DrawTop()
    {
        T top = entries[entries.Count - 1];
        Remove(top);
        return top;
    }

    public T DrawFromIndex(int index)
    {
        T atGivenIndex = entries[index];
        RemoveFromIndex(index);
        return atGivenIndex;
    }

    public T Get(T entry)
    {
        return entries.Find(x => x.Equals(entry));
    }


    public List<T> GetSpells()
    {
        return entries;
    }

    public void ActOnEachSpellInPile(Action<T> func)
    {
        foreach (T entry in entries)
        {
            func(entry);
        }
    }

    public void ActOnThenRemoveEveryEntry(Action<T> func)
    {
        while (entries.Count > 0)
        {
            T e = entries[0];
            entries.RemoveAt(0);
            func(e);
        }
    }

    public int GetNumEntriesMatching(Func<T, bool> matchingFunc)
    {
        return GetEntriesMatching(matchingFunc).Count;
    }

    public List<T> GetEntriesMatching(Func<T, bool> matchingFunc)
    {
        List<T> toReturn = new List<T>();
        foreach (T t in entries)
        {
            if (matchingFunc(t)) toReturn.Add(t);
        }
        return toReturn;
    }

    public void TransferEntries(Pile<T> transferTo, bool shuffle)
    {
        ActOnThenRemoveEveryEntry(t => transferTo.Add(t));
        if (shuffle) transferTo.Shuffle();
    }

    public void Shuffle()
    {
        RandomHelper.Shuffle(entries);
    }
}
