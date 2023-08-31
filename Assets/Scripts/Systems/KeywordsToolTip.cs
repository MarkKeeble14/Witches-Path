using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeywordsToolTip : MonoBehaviour, ToolTippable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ToolTipKeyword keyword;
    private GameObject spawnedToolTip;
    private bool isMouseOver;
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        DestroyToolTip();
    }

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTips)
        {
            if (!isMouseOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (CombatManager._Instance.AllowGameSpaceToolTips)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(this, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

}
