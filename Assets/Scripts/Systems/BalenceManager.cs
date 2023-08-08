using System;
using System.Collections.Generic;
using UnityEngine;

public class BalenceManager : MonoBehaviour
{
    public static BalenceManager _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> artifactSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> bookSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> afflictionSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> spellSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> mapNodeSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField]
    private SerializableDictionary<string, SerializableDictionary<string, float>> eventSpecDict = new SerializableDictionary<string, SerializableDictionary<string, float>>();

    [SerializeField] private SerializableDictionary<ReforgeModifier, ReforgeModifierEffect> reforgeModifierEffects = new SerializableDictionary<ReforgeModifier, ReforgeModifierEffect>();
    public ReforgeModifierEffect GetReforgeModifierEffect(ReforgeModifier reforgeModifier)
    {
        return reforgeModifierEffects[reforgeModifier];
    }
    public float GetValue(ArtifactLabel artifactLabel, string identifier)
    {
        return artifactSpecDict[artifactLabel.ToString()][identifier];
    }

    public bool UpdateValue(BookLabel bookLabel, string identifier, float changeBy)
    {
        if (bookSpecDict[bookLabel.ToString()].ContainsKey(identifier))
        {
            float currentValue = bookSpecDict[bookLabel.ToString()][identifier];
            bookSpecDict[bookLabel.ToString()][identifier] = currentValue += changeBy;
            return true;
        }
        return false;
    }

    public float GetValue(BookLabel bookLabel, string specIdentifier)
    {
        return bookSpecDict[bookLabel.ToString()][specIdentifier];
    }

    public float GetValue(AfflictionType afflictionType, string identifier)
    {
        return afflictionSpecDict[afflictionType.ToString()][identifier];
    }

    public float GetValue(SpellLabel spellLabel, string identifier)
    {
        return spellSpecDict[spellLabel.ToString()][identifier];
    }

    public float GetValue(MapNodeType type, string identifier)
    {
        return mapNodeSpecDict[type.ToString()][identifier];
    }

    public float GetValue(EventLabel type, string identifier)
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
