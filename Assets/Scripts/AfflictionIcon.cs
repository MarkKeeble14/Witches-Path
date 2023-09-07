using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AfflictionIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI stacksRemainingText;
    private Affliction setTo;

    [SerializeField] private bool useNameText;

    [Header("Animations")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float isMousedOverTargetScale;
    [SerializeField] private float changeScaleSpeed = 1f;
    [SerializeField] private Transform toScale;
    private float targetScale;
    private bool isMousedOver;

    private GameObject spawnedToolTip;

    private void Awake()
    {
        // Set Target Scale Initially
        targetScale = regularScale;

        nameText.gameObject.SetActive(useNameText);
    }

    private void Update()
    {
        if (isMousedOver)
        {
            targetScale = Mathf.Lerp(targetScale, isMousedOverTargetScale, Time.deltaTime * changeScaleSpeed);
        }
        else
        {
            // Allow target scale to fall back to regular scale
            if (targetScale != regularScale)
            {
                targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
            }
        }


        // Set Transforms Actual Scale Scale
        toScale.localScale = targetScale * Vector3.one;
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void SetAffliction(Affliction aff)
    {
        setTo = aff;
        nameText.text = aff.Name;
        image.sprite = UIManager._Instance.GetAfflictionIcon(aff.Type);
    }

    public void UpdateAfflictionStacks()
    {
        int num = setTo.GetStacks();
        stacksRemainingText.text = num.ToString();
        stacksRemainingText.color = num > 0 ? Color.green : Color.red;
    }

    public void SpawnToolTip()
    {
        if (CombatManager._Instance.AllowGameSpaceToolTips)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(setTo, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMousedOver = true;
        SpawnToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMousedOver = false;
        DestroyToolTip();
    }
}
