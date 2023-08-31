using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventLabel
{
    Treasure,
    HomeFree,
    WitchesHut,
    TravellersDelivery,
    PitOfTreasures,
    TheCuriousChild,
    SealedChamber
}

[CreateAssetMenu(fileName = "OptionEvent", menuName = "GameOccurance/OptionEvent")]
public class OptionEventGameOccurance : GameOccurance
{
    [SerializeField] private EventLabel eventLabel;
    public EventLabel EventLabel => eventLabel;
    public override MapNodeType Type => MapNodeType.Event;

    protected override IEnumerator OnResolve()
    {
        Debug.Log(name + ": OnResolve");
        yield return null;
    }

    protected override IEnumerator OnStart()
    {
        Debug.Log(name + ": OnStart");

        yield return GameManager._Instance.StartCoroutine(EventManager._Instance.StartOptionEvent(OptionEvent.GetOptionEventOfType(EventLabel)));
    }
}

public abstract class OptionEvent
{
    public abstract EventLabel EventLabel { get; }
    public abstract Sprite EventArt { get; }
    public abstract string EventName { get; }
    public abstract string EventText { get; }

    private List<ConditionalOption> options = new List<ConditionalOption>();

    protected void AddOptions(params ConditionalOption[] options)
    {
        this.options.AddRange(options);
    }

    public OptionEvent()
    {
        InitializeEventData();
    }

    protected abstract void InitializeEventData();
    public virtual void UpdateEventData()
    {
        //
    }

    protected KeyValuePair<int, EventOptionOutcome> MakeEventOptionOutcomeWithChance(int chance, string effectText, Action effect
    )
    {
        return new KeyValuePair<int, EventOptionOutcome>(chance, MakeEventOptionOutcome(effectText, effect));
    }

    protected KeyValuePair<int, EventOptionOutcome> MakeEventOptionOutcomeWithChance(int chance, EventOptionOutcome outcome, Action additionalEffects = null)
    {
        outcome.AddEffect(additionalEffects);
        return new KeyValuePair<int, EventOptionOutcome>(chance, outcome);
    }

    protected EventOptionOutcome MakeEventOptionOutcome(string effectText, Action effect)
    {
        return new EventOptionOutcome(effectText, effect);
    }

    protected EventOption MakeEventOption(
        string hintText,
        string effectText,
        Func<bool> lockCondition,
        params KeyValuePair<int, EventOptionOutcome>[] outcomes
        )
    {
        return new EventOption(hintText, effectText, MakeOutcomeMap(outcomes), lockCondition);
    }

    private PercentageMap<EventOptionOutcome> MakeOutcomeMap(params KeyValuePair<int, EventOptionOutcome>[] outcomes)
    {
        PercentageMap<EventOptionOutcome> outcomeMap = new PercentageMap<EventOptionOutcome>();
        foreach (KeyValuePair<int, EventOptionOutcome> outcome in outcomes)
        {
            outcomeMap.AddOption(new SerializableKeyValuePair<EventOptionOutcome, int>(outcome.Value, outcome.Key));
        }
        return outcomeMap;
    }

    protected int GetEventSpec(string identifier)
    {
        return BalenceManager._Instance.GetValue(EventLabel, identifier);
    }

    public List<EventOption> GetViableEventOptions()
    {
        // Debug.Log("Getting Viable Options: " + this);
        List<EventOption> viableOptions = new List<EventOption>();
        foreach (ConditionalOption option in options)
        {
            // Debug.Log("Option: " + option);
            if (option.Viable)
            {
                // Debug.Log("Viable");
                viableOptions.Add(option.EventOption);
            }
        }
        return viableOptions;
    }

    public static OptionEvent GetOptionEventOfType(EventLabel eventLabel)
    {
        switch (eventLabel)
        {
            case EventLabel.Treasure:
                return new Treasure();
            case EventLabel.TravellersDelivery:
                return new TravellersDelivery();
            case EventLabel.HomeFree:
                return new HomeFree();
            case EventLabel.WitchesHut:
                return new WitchesHut();
            case EventLabel.PitOfTreasures:
                return new PitOfTreasures();
            case EventLabel.TheCuriousChild:
                return new TheCuriousChild();
            case EventLabel.SealedChamber:
                return new SealedChamber();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}

// Conditional Option 
// The Option speicified will only appear in the option list if the condition evaluates to be true
public class ConditionalOption
{
    protected Func<bool> condition;
    public bool Viable => condition();
    public EventOption EventOption { get; }

    public ConditionalOption(Func<bool> condition, EventOption eventOption)
    {
        this.condition = condition;
        EventOption = eventOption;
    }
}

// Conditional Option houses an EventOption
// Contains a list of possible Outcomes
public class EventOption
{
    private string hintText;
    public string Hint => hintText;
    private string effectText;
    public string EffectText => effectText;
    private Func<bool> lockCondition;
    public bool Locked => lockCondition();

    // A list of possible outcomes
    private PercentageMap<EventOptionOutcome> possibleOutcomes;

    public void UpdateEffectText(string newText)
    {
        effectText = newText;
    }

    public EventOption(string hintText, string effectText, PercentageMap<EventOptionOutcome> possibleOutcomes, Func<bool> lockCondition)
    {
        this.hintText = hintText;
        this.effectText = effectText;
        this.possibleOutcomes = possibleOutcomes;
        this.lockCondition = lockCondition;
    }

    public EventOptionOutcome GetOutcome()
    {
        return possibleOutcomes.GetOption();
    }
}

// The EventOption houses a PercentageMap of OptionEventOutcomes
public class EventOptionOutcome
{
    public string ResultText { get; }
    private Action effect { get; set; }

    public EventOptionOutcome(string resultText, Action effect)
    {
        ResultText = resultText;
        this.effect += effect;
    }

    public void CallEffect()
    {
        effect?.Invoke();
    }

    public void AddEffect(Action effect)
    {
        this.effect += effect;
    }
}

public class Treasure : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.Treasure;

    public override Sprite EventArt => null;

    public override string EventName => "Treasure";

    public override string EventText => "In front of you stands a Lecturn, a Chest, and a heaping bag of Gold. A faint voice can be heard whispering, " +
        "<shake> Whichever you do not choose shall be lost.</> Which do you approach?";

    protected override void InitializeEventData()
    {
        // 3 Options

        // Get all Variables
        int goldAmount = GetEventSpec("GoldAmount");

        // 1: Accept the Delivery, 100% Chance to Add a Random Artifact & Lose Gold
        ConditionalOption chest = new ConditionalOption(() => true,
            MakeEventOption("Approach Chest", "Gain a Random Artifact", () => false,
                MakeEventOptionOutcomeWithChance(100, "You open the chest and find an artifact within.", delegate
                {
                    RewardManager._Instance.AddRandomArtifactReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 2: Attempt to Rob the Traveller, 50% Chance to Gain a Random Artifact & Gain some Gold, 50% Chance to Lose some HP
        ConditionalOption lecturn = new ConditionalOption(() => true,
            MakeEventOption("Approach Lecturn", "View a Random Book", () => false,
                MakeEventOptionOutcomeWithChance(100, "You approach the Lecturn and consider the worth of the book which sits upon it.", delegate
                {
                    RewardManager._Instance.AddRandomBookReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 3: Leave
        ConditionalOption bagOfGold = new ConditionalOption(() => true,
            MakeEventOption("Approach Brimming Bag of Gold", "Gain " + goldAmount + " Gold", () => false,
                MakeEventOptionOutcomeWithChance(100, "You approach the Bag just as its fabric begins to rip and tear. The Gold previous contained within is now" +
                " clanging on the ground, just waiting to be picked up.", delegate
                {
                    RewardManager._Instance.AddCurrencyReward(goldAmount);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // Add Options
        AddOptions(chest, lecturn, bagOfGold);
    }
}

public class TravellersDelivery : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TravellersDelivery;

    public override Sprite EventArt => null;

    public override string EventName => "Travellers Delivery";

    public override string EventText => "A wayward traveller approaches you waving a knitted knapsack over his head. He claims to have a gift for you, but requires payment for his services...";

    protected override void InitializeEventData()
    {
        // 3 Options

        // Get all Variables
        int acceptCost = GetEventSpec("AcceptCost");
        int robGoldGain = GetEventSpec("RobGoldGain");
        int loseHPAmount = GetEventSpec("LoseHPAmount");

        // 1: Accept the Delivery, 100% Chance to Add a Random Artifact & Lose Gold
        ConditionalOption accept = new ConditionalOption(() => true,
            MakeEventOption("Accept", "Gain a Random Artifact, Lose " + acceptCost + " Gold", () => GameManager._Instance.GetPlayerCurrency() < acceptCost,
                MakeEventOptionOutcomeWithChance(100, "The traveller stops, opens his bag and out pops a trinket alongside a card from your mother wishing you good luck.", delegate
                {
                    GameManager._Instance.AlterCurrency(-acceptCost);
                    RewardManager._Instance.AddRandomArtifactReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 2: Attempt to Rob the Traveller, 50% Chance to Gain a Random Artifact & Gain some Gold, 50% Chance to Lose some HP
        ConditionalOption rob = new ConditionalOption(() => true,
            MakeEventOption("Rob", "50%: Gain a Random Artifact, Gain " + robGoldGain + " Gold OR 50%: Lose " + loseHPAmount + " HP", () => false,
                MakeEventOptionOutcomeWithChance(50, "You assault the traveller before he comes to a complete stop, catching him totally by surprise. He stands no chance.", delegate
                {
                    GameManager._Instance.AlterCurrency(robGoldGain);
                    RewardManager._Instance.AddRandomArtifactReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(50, "The traveller notices your windup and manages to kick your shin before running off.", delegate
                {
                    GameManager._Instance.AlterPlayerHP(-loseHPAmount, DamageType.Default);
                })));

        // 3: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "You apoligize profusely, but stand firm in relaying that you don't carry change", null)));

        // Add Options
        AddOptions(accept, rob, leave);
    }
}

public class HomeFree : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.HomeFree;

    public override Sprite EventArt => null;

    public override string EventName => "Home Free";

    public override string EventText => "In the distance, you notice a magical portal gurgling in the wind...";

    protected override void InitializeEventData()
    {
        // 2 Options

        // Get all Variables
        int gainGoldAmount = GetEventSpec("GainGoldAmount");
        ArtifactLabel removeArtifact = ArtifactLabel.Crown;
        if (GameManager._Instance.NumArtifacts > 0)
        {
            removeArtifact = GameManager._Instance.GetRandomOwnedArtifact();
        }

        // 1: Approach
        ConditionalOption approach = new ConditionalOption(() => true,
            MakeEventOption("Approach", "Gain " + gainGoldAmount + " Gold, Lose " + (GameManager._Instance.NumArtifacts > 0 ? removeArtifact : "a Random Artifact"),
            () => GameManager._Instance.NumArtifacts <= 0,
                MakeEventOptionOutcomeWithChance(100, "As you approach the portal, your " + removeArtifact + " is " +
                    "sucked from your bag straight into the portal's waiting ripples. " +
                    "A moment later, a sack of money flies out, nearing taking off your head before landing by your feet.", delegate
                {
                    GameManager._Instance.RemoveArtifact(removeArtifact);
                    GameManager._Instance.AlterCurrency(gainGoldAmount);
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "The sounds of the portal are enough to make you wary as you decide against approaching it.", null)));

        // Add Options
        AddOptions(approach, leave);
    }
}

public class WitchesHut : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.WitchesHut;

    public override Sprite EventArt => null;

    public override string EventName => "Witches Hut";

    public override string EventText => "You come across a hut with a cauldron smoking up the nearby area. " +
        "Surrounding the cauldron are several ingredients you know can be used to brew potions. There appears to be no one in sight.";

    protected override void InitializeEventData()
    {
        // 4 Options

        // Get all Variables
        int loseHPAmount = GetEventSpec("LoseHPAmount");

        string failText = "The witch who owns the hut storms through the door just as you pocket the ingredient. " +
                    "She aggressively escorts you out of her abode, but not without a swift slap to your rear on the way out.";
        string successText = "You pocket the ingredient without any disturbance.";

        // 1: Take 1
        ConditionalOption take1 = new ConditionalOption(() => true,
            MakeEventOption("Take One", "90%: Gain 1 Potion Ingredient OR 10%: Lose " + loseHPAmount + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(90, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(1);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(10, MakeEventOptionOutcome(failText, delegate
                    {
                        GameManager._Instance.AlterPlayerHP(-loseHPAmount, DamageType.Default);
                    })
                )));

        // 2: Take 3
        ConditionalOption take2 = new ConditionalOption(() => true,
            MakeEventOption("Take Two", "60%: Gain 2 Potion Ingredient OR 40%: Lose " + (loseHPAmount * 2) + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(60, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(2);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(40, MakeEventOptionOutcome(failText, delegate
                {
                    GameManager._Instance.AlterPlayerHP(-(loseHPAmount * 2), DamageType.Default);
                })
                )));

        // 3: Take 3
        ConditionalOption take3 = new ConditionalOption(() => true,
            MakeEventOption("Take Three", "25%: Gain 3 Potion Ingredient OR 75%: Lose " + (loseHPAmount * 3) + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(25, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(3);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(75, MakeEventOptionOutcome(failText, delegate
                {
                    GameManager._Instance.AlterPlayerHP(-(loseHPAmount * 3), DamageType.Default);
                })
                )));

        // 4: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Surely this is someones house you think, ultimately deciding against making yourself at home.", null)));

        // Add Options
        AddOptions(take1, take2, take3, leave);
    }
}

public class PitOfTreasures : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.PitOfTreasures;

    public override Sprite EventArt => null;

    public override string EventName => "Pit of Treasures?";

    private string eventText = "A deep and dark pit sits in front of you. There could be any number of <shake>dangers</shake> " +
        "within, but possibly an equal number of treasures. If you just stick your arm in, who knows what may come out?";
    public override string EventText => eventText;

    private EventOption reach;
    private int loseHPAmount;
    private int successOdds;
    private int failureOdds => 100 - successOdds;
    private string effectString => successOdds + "%: Gain a Random Artifact OR " + failureOdds + "%: Lose " + loseHPAmount + " HP";

    public override void UpdateEventData()
    {
        reach.UpdateEffectText(effectString);
    }

    protected override void InitializeEventData()
    {
        // 2 Options

        // Get all Variables
        loseHPAmount = GetEventSpec("LoseHPAmount");
        successOdds = GetEventSpec("SuccessOdds");
        int failureOdds = 100 - successOdds;

        // 1: Reach
        // Upon reaching, if successful, gain a random artifact
        // If unsuccessful, increase the cost to reach but also the liklihood of success, then call again
        reach = MakeEventOption("Reach", effectString, () => GameManager._Instance.GetCurrentCharacterHP() < loseHPAmount,
            MakeEventOptionOutcomeWithChance(successOdds, "Success! You've found an artifact hiding in the depths of the pit.", delegate
            {
                RewardManager._Instance.AddRandomArtifactReward();
                EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
            }),
            MakeEventOptionOutcomeWithChance(failureOdds, "N/A", delegate
            {
                eventText = "<shake>" + GetHurtPhrase() + "...</shake> Try Again?";
                GameManager._Instance.AlterPlayerHP(-loseHPAmount, DamageType.Default);
                loseHPAmount += 1;
                successOdds += 5;
                EventManager._Instance.ChainEvent(this);
            }
        ));
        ConditionalOption reachConditionOption = new ConditionalOption(() => true, reach);

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Deciding that the pit appears too dangerous, you move on swiftly", null)));

        // Add Options
        AddOptions(reachConditionOption, leave);
    }

    private string lastHurtPhrase;
    private string GetHurtPhrase()
    {
        string hurtPhrase = RandomHelper.GetRandomFromList(new List<string>() { "Ouch", "Oof", "Owie", "Ugh", "AHHH" });
        if (hurtPhrase == lastHurtPhrase)
        {
            return GetHurtPhrase();
        }
        else
        {
            lastHurtPhrase = hurtPhrase;
            return hurtPhrase;
        }
    }
}

public class TheCuriousChild : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TheCuriousChild;

    public override Sprite EventArt => null;

    public override string EventName => "The Curious Child";

    public override string EventText => "A young child notices and approaches you.. \"Oh! A visitor... " +
        "Say, I found this thingy in the den, I'd be happy to give it to you in exchange for something?\"";

    protected override void InitializeEventData()
    {
        // 2 Options

        // Get all Variables
        // Artifact to Gain
        ArtifactLabel gainArtifact = GameManager._Instance.GetRandomArtifact();

        // Determine gold Cost
        int minGoldCost = GetEventSpec("MinGoldCost");
        int maxGoldCost = GetEventSpec("MaxGoldCost");
        int currentPlayerGold = GameManager._Instance.GetPlayerCurrency();
        int finalGoldCost = minGoldCost;

        // if the player has more gold than the max gold cost, just randomly pick cost
        if (currentPlayerGold >= maxGoldCost)
        {
            finalGoldCost = RandomHelper.RandomIntInclusive(minGoldCost, maxGoldCost);
        }
        else if (currentPlayerGold >= minGoldCost) // if the player has less than the max gold cost, but more than the minimum, pick cost from min gold cost to current player gold
        {
            finalGoldCost = RandomHelper.RandomIntInclusive(minGoldCost, currentPlayerGold);
        }

        // Spell to Lose
        Spell loseSpell = GameManager._Instance.GetRandomOwnedSpell();
        Potion losePotion = GameManager._Instance.GetRandomOwnedPotion();

        // 1: Give Spell
        ConditionalOption giveSpell = new ConditionalOption(() => true,
            MakeEventOption("Give Spell", "Gain " + gainArtifact + ", Lose " + (loseSpell != null ? loseSpell.Name : "Random Spell"),
            () => GameManager._Instance.NumSpells <= 1,
                MakeEventOptionOutcomeWithChance(100, "\"Woah! Mom's gonna hate this!\", the child exclaims.", delegate
                    {
                        GameManager._Instance.AddArtifact(gainArtifact);
                        GameManager._Instance.RemoveSpellFromSpellBook(loseSpell);
                    })));

        // 2. Give Potion
        ConditionalOption givePotion = new ConditionalOption(() => true,
            MakeEventOption("Give Potion", "Gain " + gainArtifact + ", Lose " + (losePotion != null ? losePotion.GetToolTipLabel() : "Random Potion"),
            () => GameManager._Instance.NumPotions <= 1,
                MakeEventOptionOutcomeWithChance(100, "The child excitedly accepts, \"That's got a funky color to it doesn't it! Say, you wouldn't know if this is safe to drink, would you?\" " +
                "Nodding sheepishly, you step away.", delegate
                {
                    GameManager._Instance.AddArtifact(gainArtifact);
                    GameManager._Instance.RemovePotion(losePotion);
                })));

        // 3: Give Gold
        ConditionalOption giveGold = new ConditionalOption(() => true,
            MakeEventOption("Give Gold", "Gain " + gainArtifact + ", Lose " + finalGoldCost + " Gold",
            () => currentPlayerGold < finalGoldCost,
                MakeEventOptionOutcomeWithChance(100, "After giving the child the gold, while stepping away, you overhear him comment, \"I can buy so many things with this! " +
                "I wonder what I'll splurge for first...\".", delegate
                {
                    GameManager._Instance.AddArtifact(gainArtifact);
                    GameManager._Instance.AlterCurrency(-finalGoldCost);
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "The child slowly sulks away after you apologize for having nothing to give him.", null)));

        // Add Options
        AddOptions(giveSpell, givePotion, giveGold, leave);
    }
}

public class SealedChamber : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.SealedChamber;

    public override Sprite EventArt => null;

    public override string EventName => "Sealed Chamber";

    private string eventText = "Stepping through the halls of the mansion, you take notice of a peculiar looking slot in the wall. " +
        "It appears to be the same size as one of your spell tomes, perhaps placing one into the slot may prove fruitful?";

    public override string EventText => eventText;

    private bool allowPlace;
    private Spell selectedSpell;
    private int commonGoldAmount;
    private int uncommonHealAmount;

    protected override void InitializeEventData()
    {
        // Set Variables
        allowPlace = true;

        // 1: Offer
        ConditionalOption place = new ConditionalOption(() => allowPlace,
            MakeEventOption("Place Spell Tome", "Select a Spell to Lose and Gain some Reward based on it's Rarity",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    allowPlace = false;
                    EventManager._Instance.SetWait(true);
                    EventManager._Instance.StartCoroutine(GameManager._Instance.SelectSpellSequence(spell =>
                    {
                        selectedSpell = spell;
                        EventManager._Instance.SetWait(false);
                        GameManager._Instance.RemoveSpellFromSpellBook(selectedSpell);
                    }));
                    EventManager._Instance.ChainEvent(this);
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => allowPlace,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Considering what else you've found in this house of horrors, perhaps some caution is understandable.", null)));

        // 3: Leave
        ConditionalOption leavePostPlace = new ConditionalOption(() => !allowPlace,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Was it worth it?", null)));

        // Add Options
        AddOptions(place, leave, leavePostPlace);
    }

    public override void UpdateEventData()
    {
        if (selectedSpell == null) return;
        switch (selectedSpell.Rarity)
        {
            case Rarity.Basic:
                eventText = "The slot glows moderately brightly.\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber, unfortunately with nothing inside.";
                return;
            case Rarity.Common:
                eventText = "The slot glows <shake>extremely</> faintly...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with some money inside.";
                RewardManager._Instance.AddCurrencyReward(GetEventSpec("GoldAmount"));
                EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                return;
            case Rarity.Uncommon:
                eventText = "The slot glows noticibly bright...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with some medicine inside.";
                GameManager._Instance.AlterPlayerHP(GetEventSpec("HealAmount"), DamageType.Heal);
                return;
            case Rarity.Rare:
                eventText = "The slot glows <shake>bright</>!\nThe wall behind it slowly rumbles aside. Despite the shimmering lights, you don't notice anything in the room. However as you " +
                    "take a step forward to investigate further, you're overcome by a powerful warmth. You feel as though you've become stronger somehow...";
                GameManager._Instance.AlterPlayerMaxHP(GetEventSpec("MaxHPAmount"));
                return;
            case Rarity.Event:
                eventText = "The slot glows quite bright...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with an Artifact inside.";
                RewardManager._Instance.AddRandomArtifactReward();
                EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                return;
        }
    }
}