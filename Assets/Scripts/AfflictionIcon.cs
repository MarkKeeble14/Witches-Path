using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfflictionIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI stacksRemainingText;
    private Affliction setTo;

    [SerializeField] private bool useNameText;

    [Header("Animations")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    private float targetScale;
    [SerializeField] private float changeScaleSpeed = 1f;

    private GameObject spawnedToolTip;

    private void Awake()
    {
        // Set Target Scale Initially
        targetScale = regularScale;

        nameText.gameObject.SetActive(useNameText);
    }

    private void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Transforms Actual Scale Scale
        image.transform.localScale = targetScale * Vector3.one;
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
}
