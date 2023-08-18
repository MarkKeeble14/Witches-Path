using System;
using UnityEngine;

[System.Serializable]
public class EventOption
{
    [SerializeField] private string hintText;
    [SerializeField] private string effectText;
    [SerializeField] private bool locked;
    private string finalizedEffectText;
    public string HintText => hintText;
    public string FinalizedEffectText => finalizedEffectText;
    public bool Locked => locked;


    [SerializeField] private PercentageMap<EventOptionOutcome> possibleOutcomes = new PercentageMap<EventOptionOutcome>();

    public EventOptionOutcome GetOutcome()
    {
        return possibleOutcomes.GetOption();
    }

    public void FillEffect(EventLabel label)
    {
        bool inParam = false;
        string param = "";
        string res = "";

        for (int i = 0; i < effectText.Length; i++)
        {
            char c = effectText[i];

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

                // Check if param is correct
                if (BalenceManager._Instance.EventHasValue(label, param))
                {
                    // Check if value is negative, if so, make the number positive as the accompanying text will indicate the direction of the value, i.e., "Lose 50 Gold" instead of "Gain 50 Gold"
                    float v = BalenceManager._Instance.GetValue(label, param);
                    if (v < 0)
                    {
                        v *= -1;
                    }
                    // Add the correct value to the string
                    res += v;
                }
                else
                {
                    // Param is incorrect
                    throw new Exception();
                }

                // no longer in param
                inParam = false;
                param = "";
            }
        }

        finalizedEffectText = res;
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