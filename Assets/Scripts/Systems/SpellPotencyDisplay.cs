using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SpellPotencyDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private Vector2 minMaxScale;
    [SerializeField] private Transform toScale;
    [SerializeField] private float uninteractableAlpha = 0.5f;

    [Header("Animate")]
    [SerializeField] private float animateScaleSpeed;
    [SerializeField] private float animateAlphaSpeed;
    private float currentPotency;
    private float goalScale;

    [SerializeField] private CanvasGroup cv;
    [SerializeField] private Outline outline;
    [SerializeField] private Image mainImage;
    [SerializeField] private TextMeshProUGUI text;

    private Spell representingSpell;
    private GameObject spawnedToolTip;
    private float maxPotency;

    public void SetSpell(Spell spell)
    {
        representingSpell = spell;
        SetMainColor(UIManager._Instance.GetDamageTypeColor(spell.MainDamageType));
        text.text = spell.Name;
    }

    public void SetMainColor(Color color)
    {
        mainImage.color = color;
    }

    public void SetOutlineColor(Color color)
    {
        outline.effectColor = color;
    }

    public void SetCurrentPotency(float v)
    {
        currentPotency = v;
    }

    public void SetMaxPotency(float v)
    {
        maxPotency = v;
    }


    private void Update()
    {
        // Destroy ToolTip if need be
        if (!CombatManager._Instance.AllowGameSpaceToolTips && spawnedToolTip != null)
        {
            DestroyToolTip();
        }

        goalScale = MathHelper.Normalize(currentPotency, 0, maxPotency, minMaxScale.x, minMaxScale.y);
        toScale.localScale = Vector3.Lerp(toScale.localScale, goalScale * Vector3.one, Time.deltaTime * animateScaleSpeed);

        //
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
            SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }
}
