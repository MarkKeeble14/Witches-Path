using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class EnemyActionInfoBlockDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ToolTippable
{
    [SerializeField] private TextMeshProUGUI text;

    private Spell setTo;

    private GameObject spawnedToolTip;

    public void SetTextDirectly(string text)
    {
        this.text.text = text;
    }

    public void Set(Spell spell)
    {
        setTo = spell;
        text.text = setTo.Name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager._Instance.AllowGameSpaceToolTips)
        {
            spawnedToolTip = UIManager._Instance.SpawnSpellToolTip(setTo, transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(spawnedToolTip);
    }

    public List<AfflictionType> GetAfflictionKeyWords()
    {
        return new List<AfflictionType>();
    }

    public List<ToolTipKeyword> GetGeneralKeyWords()
    {
        return new List<ToolTipKeyword>();
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
