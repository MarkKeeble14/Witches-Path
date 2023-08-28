using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyBasicAttackDamageDisplay : SingleVariableCustomToolTipLabelAndText
{
    protected override float Variable => CombatManager._Instance.CurrentEnemy.GetBasicAttackDamage();
}

public abstract class CustomToolTipLabelAndText : MonoBehaviour, ToolTippable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string label;
    private GameObject spawnedToolTip;

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

    protected virtual void AddKeywords()
    {
        //
    }

    public string GetToolTipLabel()
    {
        return label;
    }

    public abstract string GetToolTipText();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager._Instance.AllowGameSpaceToolTips)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(this, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(spawnedToolTip);
    }
}
