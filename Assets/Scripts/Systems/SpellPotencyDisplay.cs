using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellPotencyDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    [SerializeField] private Vector2 minMaxPotency;
    [SerializeField] private Vector2 minMaxScale;
    [SerializeField] private Transform toScale;

    [Header("Animate")]
    [SerializeField] private float animateScaleSpeed;
    [SerializeField] private float animateAlphaSpeed;
    private float currentPotency;
    private float goalScale;

    [SerializeField] private CanvasGroup cv;
    [SerializeField] private Outline outline;
    [SerializeField] private Image mainImage;

    private ActiveSpell representingSpell;
    private GameObject spawnedToolTip;

    private bool canShowToolTips;

    public void SetSpell(ActiveSpell spell)
    {
        representingSpell = spell;
        SetMainColor(UIManager._Instance.GetDamageTypeColor(spell.MainDamageType));
        SetCanShowToolTips(true);
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

    private void Update()
    {
        goalScale = MathHelper.Normalize(currentPotency, minMaxPotency.x, minMaxPotency.y, minMaxScale.x, minMaxScale.y);
        toScale.localScale = Vector3.Lerp(toScale.localScale, goalScale * Vector3.one, Time.deltaTime * animateScaleSpeed);
        cv.alpha = Mathf.Lerp(cv.alpha, 1, Time.deltaTime * animateAlphaSpeed);
    }

    public Vector2 GetMinMaxPotency()
    {
        return minMaxPotency;
    }

    private void SpawnToolTip()
    {
        if (canShowToolTips)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representingSpell, transform);
    }

    private void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canShowToolTips)
            SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyToolTip();
    }

    public bool GetCanShowToolTips()
    {
        return canShowToolTips;
    }

    public void SetCanShowToolTips(bool b)
    {
        if (!b)
        {
            DestroyToolTip();
        }
        canShowToolTips = b;
    }
}
