﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [Header("Books")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> bookSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Afflictions")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> afflictionSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Map")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> mapNodeSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [Header("Events")]
    [SerializeField]
    private SerializableDictionary<EventLabel, SerializableDictionary<string, int>> eventSpecDict
        = new SerializableDictionary<EventLabel, SerializableDictionary<string, int>>();

    [Header("Potions")]
    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int[]>> ingredientSpecDict
        = new SerializableDictionary<string, SerializableDictionary<string, int[]>>();

    private void Awake()
    {
        _Instance = this;
    }

    public bool PotionHasSpec(PotionIngredientType type, string param)
    {
        return ingredientSpecDict[type.ToString()].ContainsKey(param);
    }

    public int GetValue(PotionIngredientType ingredientType, string identifier, int potency)
    {
        return ingredientSpecDict[ingredientType.ToString()][identifier][potency - 1];
    }

    public int GetValue(PotionIngredientType ingredientType, string identifier)
    {
        return ingredientSpecDict[ingredientType.ToString()][identifier][0];
    }

    public int UpdateValue(BookLabel bookLabel, string identifier, int changeBy)
    {
        if (bookSpecDict[bookLabel.ToString()].ContainsKey(identifier))
        {
            int currentValue = bookSpecDict[bookLabel.ToString()][identifier];
            int newValue = currentValue + changeBy;
            bookSpecDict[bookLabel.ToString()][identifier] = newValue;
            return newValue;
        }
        return Utils.StandardSentinalValue;
    }

    // Simply returns the value found in the dict
    public int GetValue(BookLabel bookLabel, string specIdentifier)
    {
        return bookSpecDict[bookLabel.ToString()][specIdentifier];
    }

    public int GetValue(AfflictionType afflictionType, string identifier)
    {
        return afflictionSpecDict[afflictionType.ToString()][identifier];
    }

    public int GetValue(MapNodeType type, string identifier)
    {
        return mapNodeSpecDict[type.ToString()][identifier];
    }

    public int GetValue(EventLabel label, string identifier)
    {
        return eventSpecDict[label][identifier];
    }

    public bool EventHasValue(EventLabel label, string valueIdentifier)
    {
        if (!eventSpecDict.ContainsKey(label)) return false;
        return eventSpecDict[label].ContainsKey(valueIdentifier);
    }
}
