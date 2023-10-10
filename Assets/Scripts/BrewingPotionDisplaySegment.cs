using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BrewingPotionDisplaySegment : MonoBehaviour
{
    [SerializeField] private RectTransform total;
    [SerializeField] private RectTransform clearButton;
    [SerializeField] private RectTransform text;
    [SerializeField] private TextMeshProUGUI displayText;

    private PotionIngredient representing;
    private GameObject spawnedToolTip;

    public void SetRepresenting(PotionIngredient ingredient)
    {
        representing = ingredient;
    }

    // Update is called once per frame
    void Update()
    {
        // Set text
        displayText.text = representing != null ? representing.Name : "-";

        // Set Sizes
        if (clearButton.gameObject.activeInHierarchy)
        {
            text.sizeDelta = new Vector2(total.sizeDelta.x - clearButton.sizeDelta.x, text.sizeDelta.y);
        }
        else
        {
            text.sizeDelta = new Vector2(total.sizeDelta.x, text.sizeDelta.y);
        }
    }


    public void SpawnToolTip()
    {
        if (representing == null) return;
        spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(representing, transform);
    }

    public void DestroyTooltip()
    {
        Destroy(spawnedToolTip);
    }
}
