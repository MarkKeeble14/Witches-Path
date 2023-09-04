using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class EnemyInfoBlockIntentDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ToolTippable
{
    [SerializeField] private TextMeshProUGUI text;

    private List<EnemyIntent> setIntents;

    private GameObject spawnedToolTip;

    public void SetTextDirectly(string text)
    {
        this.text.text = text;
    }

    public void Set(List<EnemyIntent> enemyIntents)
    {
        setIntents = enemyIntents;

        string result = "";
        for (int i = 0; i < setIntents.Count; i++)
        {
            result += setIntents[i].GetIntentText();

            // One Intent
            if (setIntents.Count == 1)
            {
                // 
            }
            else if (setIntents.Count == 2) // Two Intents
            {
                if (i == setIntents.Count - 2)
                {
                    result += " and ";
                }
            }
            else if (setIntents.Count > 2) // More than Two Intents
            {
                if (i == setIntents.Count - 2)
                {
                    result += ", and ";
                }
                else if (i < setIntents.Count - 2)
                {
                    result += ", ";
                }
            }
        }
        text.text = result;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (setIntents == null) return;
        if (setIntents.Count <= 0) return;

        if (CombatManager._Instance.AllowGameSpaceToolTips)
        {
            spawnedToolTip = UIManager._Instance.SpawnOnlyAfflictionAndKeywordsToolTips(this, transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(spawnedToolTip);
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        List<AfflictionType> intentAfflictions = new List<AfflictionType>();
        foreach (EnemyIntent action in setIntents)
        {
            foreach (AfflictionType type in action.GetAfflictionKeyWords())
            {
                if (!intentAfflictions.Contains(type))
                {
                    intentAfflictions.Add(type);
                }
            }
        }
        return intentAfflictions;
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        List<ToolTipKeyword> intentKeywords = new List<ToolTipKeyword>();
        foreach (EnemyIntent action in setIntents)
        {
            foreach (ToolTipKeyword type in action.GetGeneralKeyWords())
            {
                if (!intentKeywords.Contains(type))
                {
                    intentKeywords.Add(type);
                }
            }
        }
        return intentKeywords;
    }

    public List<ToolTippable> GetOtherToolTippables()
    {
        return new List<ToolTippable>();
    }

    public string GetToolTipLabel()
    {
        return "";
    }

    public string GetToolTipText()
    {
        return "";
    }
}
