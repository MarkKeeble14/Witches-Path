using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class QueuedSpellDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    private Spell representingSpell;
    private GameObject spawnedToolTip;
    private bool allowScale = false;

    public void SetSpell(Spell spell)
    {
        representingSpell = spell;
        SetMainColor(UIManager._Instance.GetSpellColor(spell.Color));
        text.text = spell.Name;
        name = spell.Name + "(QueuedSpellDisplay)";
    }

    public void ShowStatChange(SpellStat type)
    {
        Debug.Log(name + ": Changed - " + type);
    }

    public void SetMainColor(SpellColorInfo colorInfo)
    {
        mainImage.color = colorInfo.Color;
        text.color = colorInfo.TextColor;
    }

    public void SetOutlineColor(Color color)
    {
        outline.effectColor = color;
    }

    public void SetAllowScale(bool allowScale)
    {
        this.allowScale = allowScale;
    }

    private void Update()
    {
        // Destroy ToolTip if need be
        if (!CombatManager._Instance.AllowGameSpaceToolTips && spawnedToolTip != null)
        {
            DestroyToolTip();
        }

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
        cv.blocksRaycasts = CombatManager._Instance.AllowGameSpaceToolTips && !(MapManager._Instance.MapOpen || GameManager._Instance.OverlaidUIOpen);
        if (cv.blocksRaycasts)
        {
            cv.alpha = Mathf.Lerp(cv.alpha, 1, Time.deltaTime * animateAlphaSpeed);
        }
        else
        {
            cv.alpha = Mathf.Lerp(cv.alpha, uninteractableAlpha, Time.deltaTime * animateAlphaSpeed);
        }

    }

    private void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingSpell, transform);
    }

    private void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager._Instance.AllowGameSpaceToolTips)
        {
            SpawnToolTip();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
