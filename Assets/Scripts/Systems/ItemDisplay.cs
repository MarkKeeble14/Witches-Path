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

    private void Start()
    {
        targetScale = regularScale;
    }

    private void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Scale
        image.transform.localScale = targetScale * Vector3.one;

        // if (setTo == null) return;

        // Update additional text if neccessary
        if (setTo.HasAdditionalText)
        {
            additionalText.text = setTo.GetAdditionalText();
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

    public void SetItem(PowerupItem i)
    {
        // Set item
        setTo = i;

        // Set fundamentals
        image.sprite = i.GetSprite();
        text.text = i.Name;

        // Enable additional text object if the represented item has such
        additionalText.gameObject.SetActive(i.HasAdditionalText);

        switch (i)
        {
            case Artifact artifact:
                finalizedToolTipText = GameManager._Instance.FillToolTipText(ContentType.Artifact, artifact.GetLabel().ToString(), artifact.ToolTipText);
                break;
            case Book book:
                finalizedToolTipText = GameManager._Instance.FillToolTipText(ContentType.Book, book.GetLabel().ToString(), book.ToolTipText);
                break;
        }
    }

    public void SpawnToolTip()
    {
        spawnedToolTip = UIManager._Instance.SpawnToolTip(finalizedToolTipText, transform, new Vector3(toolTipXOffset, 0, 0));
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip.gameObject);
    }
}
