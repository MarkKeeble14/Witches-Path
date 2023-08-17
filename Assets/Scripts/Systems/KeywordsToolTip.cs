using System.Collections.Generic;
using UnityEngine;

public class KeywordsToolTip : MonoBehaviour, ToolTippable
{
    [SerializeField] private ToolTipKeyword keyword;
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

    public string GetToolTipLabel()
    {
        return UIManager._Instance.HighlightKeywords(keyword.ToString());
    }

    public string GetToolTipText()
    {
        return UIManager._Instance.HighlightKeywords(UIManager._Instance.GetKeyWordText(keyword.ToString()));
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(this, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
