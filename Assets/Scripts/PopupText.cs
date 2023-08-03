using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private Vector2 minMaxHorizontalOffset = new Vector2(-1, 1);
    [SerializeField] private Vector2 minMaxVerticalOffset = new Vector2(-1, 1);

    public void Set(string text, Color c)
    {
        this.text.text = text;
        this.text.color = c;

        // Offset
        transform.position += new Vector3(RandomHelper.RandomFloat(minMaxHorizontalOffset), RandomHelper.RandomFloat(minMaxVerticalOffset), 0);
    }

    public void Cleanup()
    {
        Destroy(gameObject);
    }
}
