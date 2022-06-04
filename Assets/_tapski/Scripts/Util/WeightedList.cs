using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A list of weighted items that can be randomly selected from.
/// Adapted from https://gamedev.stackexchange.com/questions/162976/how-do-i-create-a-weighted-collection-and-then-pick-a-random-element-from-it
/// <para>Usage:</para>
/// var items = new WeightedList();<br/>
/// items.Add(new WeightedItem { Prefab = prefab1, Weight = 1 });<br/>
/// items.Add(new WeightedItem { Prefab = prefab2, Weight = 1 });<br/>
/// var randomPrefab = items.Random();<br/>
/// </summary>
public class WeightedList
{
    #region Private Fields
    private List<WeightedItem> Items = new List<WeightedItem>();
    private int TotalWeight;
    #endregion

    #region Initialization
    public WeightedList() { }

    public WeightedList(List<WeightedItem> prefabs)
    {
        foreach (WeightedItem item in prefabs)
        {
            Add(item);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Add an item to the list to be randomly chosen
    /// </summary>
    public void Add(WeightedItem choice)
    {
        TotalWeight += choice.Weight;
        choice.AccumulatedWeight = TotalWeight;
        Items.Add(choice);
    }

    /// <summary>
    /// Add an item to the list to be randomly chosen
    /// </summary>
    public void Add(GameObject obj, int weight)
    {
        Add(new WeightedItem
        {
            Value = obj,
            Weight = weight
        });
    }

    /// <summary>
    /// Removes an item from the list and recalculates accumulated weights
    /// </summary>
    public void Remove(WeightedItem obj)
    {
        Items.Remove(obj);

        TotalWeight = 0;
        for (int i = 0; i < Items.Count; i++)
        {
            TotalWeight += Items[i].Weight;
            Items[i].AccumulatedWeight = TotalWeight;
        }
    }

    /// <summary>
    /// Retrieve a random item instance from the list
    /// </summary>
    public WeightedItem Choose()
    {
        int chance = UnityEngine.Random.Range(0, TotalWeight);

        foreach (WeightedItem entry in Items)
        {
            if (entry.AccumulatedWeight >= chance)
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// Calls <see cref="Choose"/> and returns the result after calling <see cref="Remove"/>
    /// </summary>
    /// <returns></returns>
    public WeightedItem Pop()
    {
        WeightedItem obj = Choose();
        Remove(obj);
        return obj;
    }
    #endregion
}