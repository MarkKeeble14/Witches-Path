using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public enum SemiUniversalSpellInfoType
{
    NumAttacks,
    Cooldown,
    PrepTime,
    ManaCost,
}

[System.Serializable]
public class SemiUniversalSpellInfoContainer : MonoBehaviour, ToolTippable, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI text;
    public TextMeshProUGUI Text => text;
    [SerializeField] private GameObject container;
    public GameObject Container => container;

    [SerializeField] private VisualSpellDisplay partOfDisplay;

    [SerializeField] private SemiUniversalSpellInfoType infoType;
    [SerializeField] private float delayBeforeSpawningToolTip;
    private bool isMousedOver;

    private GameObject spawnedToolTip;

    [SerializeField] private float onHoverScaleTo = 1.25f;
    [SerializeField] private float onHoverScaleDuration = .1f;

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
        return Utils.SplitOnCapitalLetters(infoType.ToString());
    }

    public string GetToolTipText()
    {
        switch (infoType)
        {
            case SemiUniversalSpellInfoType.Cooldown:
                return "This Spell goes on a " + partOfDisplay.GetCooldown() + " Spell Cooldown when Queued";
            case SemiUniversalSpellInfoType.ManaCost:
                return "This Spell Costs " + partOfDisplay.GetManaCost() + " Mana to Use";
            case SemiUniversalSpellInfoType.NumAttacks:
                int numNotes = partOfDisplay.GetNumNotes();
                return "This Spell will call " + numNotes + " Basic Attack" + (numNotes > 1 ? "s" : "") + " when Cast";
            case SemiUniversalSpellInfoType.PrepTime:
                int prepTime = partOfDisplay.GetPrepTime();
                return "This Spell will take " + prepTime + " Turn" + (prepTime > 1 ? "s" : "") + " to Cast";
            default:
                throw new UnhandledSwitchCaseException();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMousedOver = true;
        StartCoroutine(SpawnToolTipsAfterDelay());
        transform.DOScale(onHoverScaleTo, onHoverScaleDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMousedOver = false;
        transform.DOScale(1, onHoverScaleDuration);
        if (spawnedToolTip)
        {
            Destroy(spawnedToolTip.gameObject);
        }
    }

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTip)
        {
            if (!isMousedOver)
            {
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }
        // Disallow tool tip under certain conditions
        if (!isMousedOver)
        {
            yield break;
        }

        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(this, transform);
    }
}
