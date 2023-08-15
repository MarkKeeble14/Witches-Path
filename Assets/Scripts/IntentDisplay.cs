using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum IntentType
{
    Attack,
    Defend,
    GainAffliction,
    ApplyAffliction
}

public class IntentDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI intentText;
    [SerializeField] private Image intentImage;

    [SerializeField] private SerializableDictionary<IntentType, Sprite> intentSpriteMap = new SerializableDictionary<IntentType, Sprite>();

    private List<IntentType> addedIntents = new List<IntentType>();

    public void ClearIntents()
    {
        addedIntents.Clear();
    }

    private void Update()
    {
        intentImage.gameObject.SetActive(addedIntents.Count > 0);
    }

    public void SetIntent(IntentType type)
    {
        addedIntents.Add(type);
        intentImage.sprite = intentSpriteMap[type];
    }

    public void SetIntentText(string t)
    {
        intentText.text = t;
    }
}
