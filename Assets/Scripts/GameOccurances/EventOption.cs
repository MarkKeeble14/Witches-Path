using UnityEngine;

[System.Serializable]
public class EventOption
{
    [SerializeField] private string hintText;
    [SerializeField] private string effectText;
    public string HintText => hintText;
    public string EffectText => effectText;

    [SerializeField] private PercentageMap<EventOptionOutcome> possibleOutcomes = new PercentageMap<EventOptionOutcome>();

    public EventOptionOutcome GetOutcome()
    {
        return possibleOutcomes.GetOption();
    }
}

[System.Serializable]
public class EventOptionOutcome
{
    [SerializeField] private string resultText;
    public string ResultText => resultText;
    [SerializeField] private string codeString;
    public string CodeString => codeString;
}