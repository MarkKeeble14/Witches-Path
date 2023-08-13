using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI additionalText;

    [Header("Animations")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    private float targetScale;
    [SerializeField] private float changeScaleSpeed = 1f;

    private PowerupItem setTo;
    private ToolTip spawnedToolTip;

    [SerializeField] private float toolTipXOffset;
    private string finalizedToolTipText;

    protected void Start()
    {
        // Set Target Scale Initially
        targetScale = regularScale;
    }

    protected void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Transforms Actual Scale Scale
        image.transform.localScale = targetScale * Vector3.one;

        // Update additional text if neccessary
        string itemAdditionalText = setTo.GetAdditionalText();
        if (itemAdditionalText.Length > 0)
        {
            additionalText.text = itemAdditionalText;
        }
        else
        {
            additionalText.gameObject.SetActive(false);
        }
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public virtual void SetItem(PowerupItem i)
    {
        // Set item
        setTo = i;

        // Set fundamentals
        image.sprite = i.GetSprite();
        text.text = i.Name;

        // Enable additional text object if the represented item has such
        additionalText.gameObject.SetActive(i.GetAdditionalText().Length > 0);
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnToolTip(setTo.ToolTipText, transform, new Vector3(toolTipXOffset, 0, 0));
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip.gameObject);
    }
}
