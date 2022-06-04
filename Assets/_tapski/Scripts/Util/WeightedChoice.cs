using UnityEngine;

[System.Serializable]
public class WeightedItem
{
    [Tooltip("The Prefab or GameObject instance to be used")]
    public GameObject Value;

    [Tooltip("The weight of this individual item")]
    public int Weight;

    /// <summary>
    /// The total weight of the list at the time it is added to a <see cref="WeightedList"/>
    /// </summary>
    [System.NonSerialized]
    public int AccumulatedWeight;
}