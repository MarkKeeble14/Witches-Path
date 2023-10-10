using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyNameToolTipSpawner : MonoBehaviour, ToolTippable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float delayBeforeSpawningToolTips = 0.5f;
    private bool isMouseOver;
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
        return CombatManager._Instance.CurrentEnemy.Name;
    }

    public string GetToolTipText()
    {
        return "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        Destroy(spawnedToolTip);
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
        // Disallow tool tip under certain conditions
        if (!isMouseOver || !CombatManager._Instance.AllowGameSpaceToolTips)
        {
            yield break;
        }

        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(this, transform);
    }
}
