using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI)), DisallowMultipleComponent]
public class FetchFont : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TMP_FontAsset font = UIManager._Instance.DefaultFont;
        foreach (TextMeshProUGUI text in GetComponents<TextMeshProUGUI>())
        {
            text.font = font;
        }
    }
}
