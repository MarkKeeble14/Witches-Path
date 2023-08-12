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
                FillToolTipText(ContentType.Artifact, artifact.GetLabel().ToString(), artifact.ToolTipText);
                break;
            case Book book:
                FillToolTipText(ContentType.Book, book.GetLabel().ToString(), book.ToolTipText);
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

    public void FillToolTipText(ContentType type, string label, string text)
    {
        bool inParam = false;
        string param = "";
        string res = "";

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // if the current char is an open curly bracket, that indicates that we are reading a parameter here
            if (c.Equals('{'))
            {
                inParam = true;
            }

            // if we're currently getting the name of the parameter, we don't add the current char to the final string
            if (inParam)
            {
                param += c;
            }
            else // if we're NOT currently getting the name of the parameter, we DO
            {
                res += c;
            }

            // the current char is a closed curly bracket, signifying the end of the parameter
            if (c.Equals('}'))
            {
                // Substring the param to remove '{' and '}'
                param = param.Substring(1, param.Length - 2);

                // Check if value is negative, if so, make the number positive as the accompanying text will indicate the direction of the value, i.e., "Lose 50 Gold" instead of "Gain 50 Gold"
                float v = BalenceManager._Instance.GetValue(type, label, param);
                if (v < 0)
                {
                    v *= -1;
                }
                // Add the correct value to the string
                res += v;

                // no longer in param
                inParam = false;
                param = "";
            }

        }
        finalizedToolTipText = res;
    }
}
