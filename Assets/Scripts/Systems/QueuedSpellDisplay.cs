using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class QueuedSpellDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ToolTippable
{
    [Header("Settings")]
    [SerializeField] private Vector2 minMaxScale;
    [SerializeField] private Transform toScale;
    [SerializeField] private float uninteractableAlpha = 0.5f;

    [Header("Animate")]
    [SerializeField] private float animateScaleSpeed;
    [SerializeField] private float animateAlphaSpeed;
    private float goalScale;

    [SerializeField] private CanvasGroup cv;
    [SerializeField] private Outline outline;
    [SerializeField] private Image mainImage;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ShowSpellStatChangeDisplay showSpellStatChangePrefab;
    [SerializeField] private TextMeshProUGUI prepTimeText;

    [SerializeField] private SemicircleLayoutGroup spellEffects;
    [SerializeField] private QueuedSpellEffectDisplay spellEffectPrefab;

    [SerializeField] private float spellEffectRadiusRatio = 5;
    [SerializeField] private float onHoverScaleTo;
    [SerializeField] private float onHoverScaleDuration;
    [SerializeField] private RectTransform onHoverToScale;

    [SerializeField] private float delayBeforeSpawningToolTips = .5f;
    private bool isMousedOver;

    private Spell representingSpell;
    private GameObject spawnedToolTip;
    private bool allowScale = false;
    private int prepTime;

    public void SetSpell(Spell spell)
    {
        SetMainColor(UIManager._Instance.GetSpellPrimaryFunctionColor(spell.PrimaryFunction));
        text.text = spell.Name;

        foreach (SpellEffect effect in spell.GetSpellEffects())
        {
            QueuedSpellEffectDisplay effectDisplay = Instantiate(spellEffectPrefab, spellEffects.transform);
            effectDisplay.Set(effect);
        }
        name = spell.Label + "(QueuedSpellDisplay)";
        representingSpell = spell;
    }

    public void ShowStatChange(SpellStat type, Sign sign)
    {
        // Debug.Log(name + ": Changed - " + type);
        ShowSpellStatChangeDisplay spawned = Instantiate(showSpellStatChangePrefab, transform);
        spawned.Set(type, sign, null);
    }

    public void SetMainColor(SpellColorInfo colorInfo)
    {
        mainImage.color = colorInfo.Color;
        prepTimeText.color = colorInfo.TextColor;
    }

    public void SetOutlineColor(Color color)
    {
        outline.effectColor = color;
    }

    public void SetAllowScale(bool allowScale)
    {
        this.allowScale = allowScale;
    }

    public void SetPrepTime(int prepTime)
    {
        this.prepTime = prepTime;
        prepTimeText.text = prepTime.ToString();
    }

    public int GetPrepTime()
    {
        return prepTime;
    }

    private void Update()
    {
        // Destroy ToolTip if need be
        if (!CombatManager._Instance.AllowGameSpaceToolTips && spawnedToolTip != null)
        {
            DestroyToolTip();
        }

        spellEffects.SetRadius(transform.localScale.x * spellEffectRadiusRatio);

        // Update Scale
        if (allowScale)
        {
            goalScale = MathHelper.Normalize(CombatManager._Instance.CurrentSpellEffectivenessMultiplier, CombatManager._Instance.MinSpellEffectivenessMultiplier,
                CombatManager._Instance.MaxSpellEffectivenessMultiplier, minMaxScale.x, minMaxScale.y);
        }
        else
        {
            goalScale = MathHelper.Normalize(CombatManager._Instance.DefaultSpellEffectivenessMultiplier, CombatManager._Instance.MinSpellEffectivenessMultiplier,
                CombatManager._Instance.MaxSpellEffectivenessMultiplier, minMaxScale.x, minMaxScale.y);
        }
        toScale.localScale = Vector3.Lerp(toScale.localScale, goalScale * Vector3.one, Time.deltaTime * animateScaleSpeed);

        // Update Blocks raycast and Alpha repsectively
        if (GameManager._Instance.OverlaidUIOpen || MapManager._Instance.MapOpen)
        {
            cv.blocksRaycasts = false;
            cv.alpha = 0;
        }
        else
        {
            cv.blocksRaycasts = CombatManager._Instance.AllowGameSpaceToolTips;
            if (cv.blocksRaycasts)
            {
                cv.alpha = Mathf.Lerp(cv.alpha, 1, Time.deltaTime * animateAlphaSpeed);
            }
            else
            {
                cv.alpha = Mathf.Lerp(cv.alpha, uninteractableAlpha, Time.deltaTime * animateAlphaSpeed);
            }
        }

    }

    private IEnumerator SpawnToolTipsAfterDelay()
    {
        float t = 0;
        while (t < delayBeforeSpawningToolTips)
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

        spawnedToolTip = UIManager._Instance.SpawnEqualListingToolTips(new List<ToolTippable>() { representingSpell, this }, transform);
    }

    private void SpawnToolTip()
    {
        StartCoroutine(SpawnToolTipsAfterDelay());
    }

    private void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMousedOver = true;
        if (CombatManager._Instance.AllowGameSpaceToolTips)
        {
            onHoverToScale.DOScale(onHoverScaleTo, onHoverScaleDuration);
            SpawnToolTip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMousedOver = false;
        onHoverToScale.DOScale(1, onHoverScaleDuration);
        DestroyToolTip();
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
        return "Prep Time";
    }

    public string GetToolTipText()
    {
        string text = "This Spell will be Cast";
        if (prepTime == 1)
        {
            text += " this Turn";
        }
        else
        {
            text += " in " + prepTime + " Turn" + (prepTime > 1 ? "s" : "");
        }
        return text;
    }
}
