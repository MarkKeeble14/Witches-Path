using UnityEngine;
using TMPro;

public class EndGameScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI numText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Set(EndGameScoreData data)
    {
        labelText.text = data.Label;
        numText.text = data.Num.ToString();
        scoreText.text = data.Score.ToString();
    }
}
