using System;
using System.Collections.Generic;
using UnityEngine;

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> artifactSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> bookSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> afflictionSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> spellSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> mapNodeSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int>> eventSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, int[]>> ingredientSpecDict = new SerializableDictionary<string, SerializableDictionary<string, int[]>>();

    [SerializeField] private SerializableDictionary<ReforgeModifier, ReforgeModifierEffect> reforgeModifierEffects = new SerializableDictionary<ReforgeModifier, ReforgeModifierEffect>();

    public ReforgeModifierEffect GetReforgeModifierEffect(ReforgeModifier reforgeModifier)
    {
        return reforgeModifierEffects[reforgeModifier];
    }

    public int GetValue(PotionIngredientType ingredientType, string identifier, int potency)
    {
        return ingredientSpecDict[ingredientType.ToString()][identifier][potency];
    }


    public int GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    public int GetValue(ContentType type, string label, string identifier)
    {
        switch (type)
        {
            case ContentType.Artifact:
                return artifactSpecDict[label][identifier];
            case ContentType.Book:
                return bookSpecDict[label][identifier];
            case ContentType.ActiveSpell:
                return spellSpecDict[label][identifier];
            case ContentType.PassiveSpell:
                return spellSpecDict[label][identifier];
            default:
                throw new UnhandledSwitchCaseException();
        }
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

    public int GetValue(SpellLabel spellLabel, string identifier)
    {
        return spellSpecDict[spellLabel.ToString()][identifier];
    }

    public int GetValue(MapNodeType type, string identifier)
    {
        return mapNodeSpecDict[type.ToString()][identifier];
    }

    public int GetValue(EventLabel type, string identifier)
    {
        return eventSpecDict[type.ToString()][identifier];
    }

    public bool EventHasValue(EventLabel label, string valueIdentifier)
    {
        return eventSpecDict.ContainsKey(label.ToString());
    }

    private void Awake()
    {
        _Instance = this;
    }
}
