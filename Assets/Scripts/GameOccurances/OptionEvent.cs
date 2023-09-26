using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventLabel
{
    Treasure,
    HomeFree,
    WitchesHut,
    TravellersDelivery,
    ArmShapedHole,
    TheCuriousChild,
    SealedChamber,
    GainColoredCards,
    GoldAtACost,
    LifeForReward,
    SpellbookManagement,
    TheCostOfGreed,
    RemoveCurseForGold,
    TavernkeepRescue,
    TraumaRecall,
    BookOfTheAmnesiac,
    BookOfTheRepurposed,
    TakeASip,
    RemoveTwoForAnger,
    ASuspiciousTome,
}

public abstract class OptionEvent
{
    public abstract EventLabel EventLabel { get; }
    public abstract Sprite EventArt { get; }
    public abstract string EventName { get; }
    public string EventText => eventText;
    protected abstract string defaultEventText { get; }
    public abstract UseEventUI EventUI { get; }
    private string eventText;

    protected void UpdateEventText(string newText)
    {
        eventText = newText;
    }

    private List<ConditionalOption> options = new List<ConditionalOption>();

    protected void AddOptions(params ConditionalOption[] options)
    {
        this.options.AddRange(options);
    }

    public OptionEvent()
    {
        InitializeEventData();
        UpdateEventText(defaultEventText);
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
        return new EventOption(hintText, effectText, null, null, MakeOutcomeMap(outcomes), lockCondition);
    }

    protected EventOption MakeEventOption(
        string hintText,
        string effectText,
        Func<bool> lockCondition,
        Action<Transform> onPointerEnter,
        Action onPointerExit,
        params KeyValuePair<int, EventOptionOutcome>[] outcomes
        )
    {
        return new EventOption(hintText, effectText, onPointerEnter, onPointerExit, MakeOutcomeMap(outcomes), lockCondition);
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

    /// <summary>
    /// Note, on End Action must include one of the following: EventManager._Instance.SetWait(false) or EventManager._instance.Resolve(); 
    /// if Chaining, Will need to Call SetWait(false)
    /// if not, Call Resolve
    /// </summary>
    /// <param name="combat"></param>
    /// <param name="isChain"></param>
    /// <param name="onWin"></param>
    protected void StartFight(Combat combat, bool isChain, Action onWin, bool allowSpellSelection)
    {
        if (isChain)
        {
            EventManager._Instance.ChainEvent(this);
            EventManager._Instance.SetWait(true);
        }

        // Swap around UI
        GameOccuranceUIManager._Instance.ForceChangeGameOccurance(MapNodeType.MinorFight, true);
        GameOccuranceUIManager._Instance.ForceChangeGameOccurance(MapNodeType.Options, false);

        CombatManager._Instance.StartCoroutine(CombatManager._Instance.StartCombat(combat, delegate
        {
            if (GameManager._Instance.GetCurrentCharacterHP() > 0)
            {
                onWin?.Invoke();

                // Change UI
                GameOccuranceUIManager._Instance.ForceChangeGameOccurance(MapNodeType.MinorFight, false);
                if (isChain)
                {
                    GameOccuranceUIManager._Instance.ForceChangeGameOccurance(MapNodeType.Options, true);
                }
            }
            else
            {
                GameManager._Instance.StartCoroutine(GameManager._Instance.GameOverSequence());
            }
        }, allowSpellSelection));
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
            case EventLabel.ArmShapedHole:
                return new ArmShapedHole();
            case EventLabel.TheCuriousChild:
                return new TheCuriousChild();
            case EventLabel.SealedChamber:
                return new SealedChamber();
            // From Here Down will be re-named
            case EventLabel.GainColoredCards:
                return new GainColoredCards();
            case EventLabel.GoldAtACost:
                return new GoldAtACost();
            case EventLabel.LifeForReward:
                return new LifeForReward();
            case EventLabel.SpellbookManagement:
                return new SpellbookManagement();
            case EventLabel.TheCostOfGreed:
                return new TheCostOfGreed();
            case EventLabel.RemoveCurseForGold:
                return new RemoveCurseForGold();
            case EventLabel.TavernkeepRescue:
                return new TavernkeeperRescue();
            case EventLabel.TraumaRecall:
                return new TraumaRecall();
            case EventLabel.BookOfTheAmnesiac:
                return new BookOfTheAmnesiac();
            case EventLabel.BookOfTheRepurposed:
                return new BookOfTheRepurposed();
            case EventLabel.RemoveTwoForAnger:
                return new RemoveTwoForAnger();
            case EventLabel.ASuspiciousTome:
                return new ASuspiciousTome();
            case EventLabel.TakeASip:
                return new TakeASip();
            default:
                throw new UnhandledSwitchCaseException();
        }
    }
}


/// <summary>
/// Data Containers & the Like
/// </summary>
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

    public Action<Transform> OnPointerEnter { get; private set; }
    public Action OnPointerExit { get; private set; }

    // A list of possible outcomes
    private PercentageMap<EventOptionOutcome> possibleOutcomes;

    public void UpdateEffectText(string newText)
    {
        effectText = newText;
    }

    public EventOption(string hintText, string effectText, Action<Transform> onPointerEnter, Action onPointerExit, PercentageMap<EventOptionOutcome> possibleOutcomes, Func<bool> lockCondition)
    {
        this.hintText = hintText;
        this.effectText = effectText;
        this.possibleOutcomes = possibleOutcomes;
        this.lockCondition = lockCondition;
        this.OnPointerEnter = onPointerEnter;
        this.OnPointerExit = onPointerExit;
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


/// <summary>
/// Types of Option Events
/// </summary>
public class Treasure : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.Treasure;
    public override Sprite EventArt => null;
    public override string EventName => "Treasure";
    protected override string defaultEventText => "In front of you stands a Lecturn, a Chest, and a heaping bag of Gold. A faint voice can be heard whispering, " +
        "<shake> Whichever you do not choose shall be lost.</> Which do you approach?";
    public override UseEventUI EventUI => UseEventUI.Story;

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
                    RewardManager._Instance.AddGoldReward(goldAmount);
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
    protected override string defaultEventText => "A wayward traveller approaches you waving a knitted knapsack over his head. He claims to have a gift for you, but requires payment for his services...";
    public override UseEventUI EventUI => UseEventUI.Story;

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
                    GameManager._Instance.AlterGold(-acceptCost);
                    RewardManager._Instance.AddRandomArtifactReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 2: Attempt to Rob the Traveller, 50% Chance to Gain a Random Artifact & Gain some Gold, 50% Chance to Lose some HP
        ConditionalOption rob = new ConditionalOption(() => true,
            MakeEventOption("Rob", "50%: Gain a Random Artifact, Gain " + robGoldGain + " Gold OR 50%: Lose " + loseHPAmount + " HP", () => false,
                MakeEventOptionOutcomeWithChance(50, "You assault the traveller before he comes to a complete stop, catching him totally by surprise. He stands no chance.", delegate
                {
                    GameManager._Instance.AlterGold(robGoldGain);
                    RewardManager._Instance.AddRandomArtifactReward();
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(50, "The traveller notices your windup and manages to kick your shin before running off.", delegate
                {
                    GameManager._Instance.AlterPlayerCurrentHP(-loseHPAmount, DamageType.Physical);
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
    protected override string defaultEventText => "In the distance, you notice a magical portal gurgling in the wind...";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        // 2 Options

        // Get all Variables
        int gainGoldAmount = GetEventSpec("GainGoldAmount");
        Artifact removeArtifact = GameManager._Instance.GetRandomOwnedArtifact();
        if (GameManager._Instance.NumArtifacts <= 0)
        {
            throw new Exception();
        }

        // 1: Approach
        ConditionalOption approach = new ConditionalOption(() => true,
            MakeEventOption("Approach", "Gain " + gainGoldAmount + " Gold, Lose " + removeArtifact.Name,
            () => GameManager._Instance.NumArtifacts <= 0,
            t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(removeArtifact, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "As you approach the portal, your " + removeArtifact.Name + " is " +
                    "sucked from your bag straight into the portal's waiting ripples. " +
                    "A moment later, a sack of money flies out, nearing taking off your head before landing by your feet.", delegate
                    {
                        GameManager._Instance.RemoveArtifact(removeArtifact);
                        GameManager._Instance.AlterGold(gainGoldAmount);
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
    protected override string defaultEventText => "You come across a hut with a cauldron smoking up the nearby area. " +
        "Surrounding the cauldron are several ingredients you know can be used to brew potions. There appears to be no one in sight.";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        // 4 Options

        // Get all Variables
        int loseHPAmount = GetEventSpec("LoseHPAmount");
        int option1NumIngredients = GetEventSpec("NumIngredients1");
        int option2NumIngredients = GetEventSpec("NumIngredients2");
        int option3NumIngredients = GetEventSpec("NumIngredients3");

        string failText = "The witch who owns the hut storms through the door just before you can pocket the ingredients. " +
                    "She aggressively escorts you out of her abode, but not without a swift slap to your rear on the way out.";
        string successText = "You pocket the ingredient without any disturbance.";

        // 1: Take 1
        ConditionalOption take1 = new ConditionalOption(() => true,
            MakeEventOption("Take One", "90%: Gain " + option1NumIngredients + " Potion Ingredients OR 10%: Lose " + loseHPAmount + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(90, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(option1NumIngredients);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(10, MakeEventOptionOutcome(failText, delegate
                {
                    GameManager._Instance.AlterPlayerCurrentHP(-loseHPAmount, DamageType.Physical);
                })
                )));

        // 2: Take 3
        ConditionalOption take2 = new ConditionalOption(() => true,
            MakeEventOption("Take Two", "60%: Gain " + option2NumIngredients + " Potion Ingredients OR 40%: Lose " + (loseHPAmount * 2) + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(60, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(option2NumIngredients);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(40, MakeEventOptionOutcome(failText, delegate
                {
                    GameManager._Instance.AlterPlayerCurrentHP(-(loseHPAmount * 2), DamageType.Physical);
                })
                )));

        // 3: Take 3
        ConditionalOption take3 = new ConditionalOption(() => true,
            MakeEventOption("Take Three", "25%: Gain " + option3NumIngredients + " Potion Ingredients OR 75%: Lose " + (loseHPAmount * 3) + " HP",
            () => false,
                MakeEventOptionOutcomeWithChance(25, successText, delegate
                {
                    RewardManager._Instance.AddRandomPotionIngredientReward(option3NumIngredients);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                }),
                MakeEventOptionOutcomeWithChance(75, MakeEventOptionOutcome(failText, delegate
                {
                    GameManager._Instance.AlterPlayerCurrentHP(-(loseHPAmount * 3), DamageType.Physical);
                })
                )));

        // 4: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Surely this is someones house you think, ultimately deciding against making yourself at home...", null)));

        // Add Options
        AddOptions(take1, take2, take3, leave);
    }
}

public class ArmShapedHole : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.ArmShapedHole;
    public override Sprite EventArt => null;
    public override string EventName => "Pit of Treasures?";
    protected override string defaultEventText => "A deep and dark hole stands in front of you. There could be any number of <shake>dangers</shake> " +
        "within, but possibly treasure as well?. If you just stick your arm in, who knows what may come out?";
    public override UseEventUI EventUI => UseEventUI.Story;

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
                UpdateEventText("<shake>" + GetHurtPhrase() + "...</shake> Try Again?");
                GameManager._Instance.AlterPlayerCurrentHP(-loseHPAmount, DamageType.Physical);
                loseHPAmount += 1;
                successOdds += 5;
                EventManager._Instance.ChainEvent(this);
            }
        ));
        ConditionalOption reachConditionOption = new ConditionalOption(() => true, reach);

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Deciding that the hole appears too dangerous, you move on swiftly", null)));

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
    protected override string defaultEventText => "A young child notices and approaches you.. \"Oh! A visitor... " +
        "Say, I found this thingy in the den, I'd be happy to give it to you in exchange for something?\"";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        // 2 Options

        // Get all Variables
        // Artifact to Gain
        Artifact gainArtifact = Artifact.GetArtifactOfType(GameManager._Instance.GetRandomArtifact());

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
            MakeEventOption("Give Spell", "Gain " + gainArtifact.Name + ", Lose " + (loseSpell != null ? loseSpell.Name : "Random Spell"),
            () => GameManager._Instance.NumSpells <= 1,
            t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(gainArtifact, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "\"Woah! Mom's gonna hate this!\", the child exclaims.", delegate
                {
                    GameManager._Instance.AddArtifact(gainArtifact);
                    GameManager._Instance.RemoveSpellFromSpellBook(loseSpell);
                })));

        // 2. Give Potion
        ConditionalOption givePotion = new ConditionalOption(() => true,
            MakeEventOption("Give Potion", "Gain " + gainArtifact.Name + ", Lose " + (losePotion != null ? losePotion.GetToolTipLabel() : "Random Potion"),
            () => GameManager._Instance.NumPotions <= 1,
            t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(gainArtifact, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "The child excitedly accepts, \"That's got a funky color to it doesn't it! Say, you wouldn't know if this is safe to drink, would you?\" " +
                "Nodding sheepishly, you step away.", delegate
                {
                    GameManager._Instance.AddArtifact(gainArtifact);
                    GameManager._Instance.RemovePotion(losePotion);
                })));

        // 3: Give Gold
        ConditionalOption giveGold = new ConditionalOption(() => true,
            MakeEventOption("Give Gold", "Gain " + gainArtifact.Name + ", Lose " + finalGoldCost + " Gold",
            () => currentPlayerGold < finalGoldCost,
            t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(gainArtifact, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "After giving the child the gold, while stepping away, you overhear him comment, \"I can buy so many things with this! " +
                "I wonder what I'll splurge for first...\".", delegate
                {
                    GameManager._Instance.AddArtifact(gainArtifact);
                    GameManager._Instance.AlterGold(-finalGoldCost);
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
    protected override string defaultEventText => "Stepping through the halls of the mansion, you take notice of a peculiar looking slot in the wall. " +
        "It appears to be the same size as one of your spell tomes, perhaps placing one into the slot may prove fruitful?";
    public override UseEventUI EventUI => UseEventUI.Story;

    private bool preOffer;
    private Spell offerredSpell;

    protected override void InitializeEventData()
    {
        // Set Variables
        preOffer = true;

        // 1: Offer
        ConditionalOption place = new ConditionalOption(() => preOffer,
            MakeEventOption("Place Spell Tome", "Select a Spell to Remove and Gain some Reward based on it's Rarity",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    preOffer = false;
                    EventManager._Instance.SetWait(true);
                    EventManager._Instance.StartCoroutine(GameManager._Instance.SelectSpellSequence(spell =>
                    {
                        offerredSpell = spell;
                        GameManager._Instance.RemoveSpellFromSpellBook(offerredSpell);
                    }, 1, spell => true, () => EventManager._Instance.SetWait(false)));
                    EventManager._Instance.ChainEvent(this);
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => preOffer,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "You decide not to give up any of your knowledge.", null)));

        // 3: Leave
        ConditionalOption leavePostPlace = new ConditionalOption(() => !preOffer,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Was it worth it?", null)));

        // Add Options
        AddOptions(place, leave, leavePostPlace);
    }

    public override void UpdateEventData()
    {
        if (offerredSpell == null) return;
        switch (offerredSpell.Rarity)
        {
            case Rarity.Basic:
                UpdateEventText("The slot glows moderately brightly.\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber, unfortunately with nothing inside.");
                return;
            case Rarity.Common:
                UpdateEventText("The slot glows <shake>extremely</> faintly...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with some money inside.");
                RewardManager._Instance.AddGoldReward(GetEventSpec("GoldAmount"));
                EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                return;
            case Rarity.Uncommon:
                UpdateEventText("The slot glows noticibly bright...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with some medicine inside.");
                GameManager._Instance.AlterPlayerCurrentHP(GetEventSpec("HealAmount"), DamageType.Heal);
                return;
            case Rarity.Rare:
                UpdateEventText("The slot glows <shake>bright</>!\nThe wall behind it slowly rumbles aside. Despite the shimmering lights, you don't notice anything in the room. However as you " +
                    "take a step forward to investigate further, you're overcome by a powerful warmth. You feel as though you've become stronger somehow...");
                GameManager._Instance.AlterPlayerMaxHP(GetEventSpec("MaxHPAmount"));
                return;
            case Rarity.Event:
                UpdateEventText("The slot glows quite bright...\nThe wall behind it slowly rumbles aside, leaving an newly opened chamber with an Artifact inside.");
                RewardManager._Instance.AddRandomArtifactReward();
                EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                return;
        }
    }
}

public class GainColoredCards : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.GainColoredCards;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        // Set Variables
        int damagePerTome = GetEventSpec("DamagePerTome");
        SpellColor spellColor = GameManager._Instance.GetCharacterColor();
        Func<Spell, bool> viableFunc = spell => spell.Color == spellColor;

        // 1: Take 1
        ConditionalOption take1 = new ConditionalOption(() => true,
            MakeEventOption("Touch", "View 1 Spell Tome, Take " + damagePerTome + " Damage",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "...", delegate
                {
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    GameManager._Instance.AlterPlayerCurrentHP(-damagePerTome, DamageType.Electric);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 2: Take 2
        ConditionalOption take2 = new ConditionalOption(() => true,
            MakeEventOption("Grab", "View 2 Spell Tomes, Take " + (damagePerTome * 2) + " Damage",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "...", delegate
                {
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    GameManager._Instance.AlterPlayerCurrentHP(-damagePerTome * 2, DamageType.Electric);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 3: Take 3
        ConditionalOption take3 = new ConditionalOption(() => true,
            MakeEventOption("Embrace", "View 3 Spell Tomes, Take " + (damagePerTome * 3) + " Damage",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "...", delegate
                {
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    RewardManager._Instance.AddChooseSpellReward(viableFunc);
                    GameManager._Instance.AlterPlayerCurrentHP(-damagePerTome * 3, DamageType.Electric);
                    EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // Add Options
        AddOptions(take1, take2, take3);
    }
}

public class GoldAtACost : OptionEvent
{
    // Booby Trap?

    public override EventLabel EventLabel => EventLabel.GoldAtACost;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        int gainGoldAmount = GetEventSpec("GoldAmount");
        int loseHPAmount = Mathf.RoundToInt(((float)GetEventSpec("DamageAmount") / 100) * GameManager._Instance.GetMaxPlayerHP());

        Spell injure = Spell.GetSpellOfType(SpellLabel.Injure);

        // 1: Offer
        ConditionalOption determined = new ConditionalOption(() => true,
            MakeEventOption("Be Determined", "Gain " + gainGoldAmount + " Gold, Become Cursed - Injure",
            () => false,
            t => spawnedToolTip = UIManager._Instance.SpawnSpellToolTip(injure, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(gainGoldAmount);
                    GameManager._Instance.AddSpellToSpellBook(injure);
                })));

        ConditionalOption tactful = new ConditionalOption(() => true,
            MakeEventOption("Be Tactful", "Gain " + gainGoldAmount + " Gold, Lose a random Spell",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(gainGoldAmount);
                    GameManager._Instance.RemoveSpellFromSpellBook(GameManager._Instance.GetRandomOwnedSpell());
                })));

        ConditionalOption reckless = new ConditionalOption(() => true,
            MakeEventOption("Be Reckless", "Gain " + gainGoldAmount + " Gold, Take " + loseHPAmount + " Damage",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(gainGoldAmount);
                    GameManager._Instance.AlterPlayerCurrentHP(-loseHPAmount, DamageType.Physical);
                })));

        // Add Options
        AddOptions(determined, tactful, reckless);
    }
}

public class LifeForReward : OptionEvent
{
    // Life Drained
    public override EventLabel EventLabel => EventLabel.LifeForReward;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        int loseMaxHP = Mathf.RoundToInt(((float)GetEventSpec("LoseMaxHP") / 100) * GameManager._Instance.GetMaxPlayerHP());

        // 1: Offer
        ConditionalOption giveHand = new ConditionalOption(() => true,
            MakeEventOption("Give Hand", "Upgrade " + GameManager._Instance.GetEquippedBook(0).Name + ", Lose " + loseMaxHP + " Max HP",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterPlayerMaxHP(-loseMaxHP);
                    GameManager._Instance.UpgradeBooks();
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", null)));

        // Add Options
        AddOptions(giveHand, leave);
    }
}

public class SpellbookManagement : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.SpellbookManagement;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        // 1: Remove
        ConditionalOption remove = new ConditionalOption(() => true,
            MakeEventOption("Remove", "Remove a Spell from your Spellbook",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.RemoveSpellSequence(spell => true));
                })));

        // 1: Transform
        ConditionalOption transform = new ConditionalOption(() => true,
            MakeEventOption("Transform", "Transform a Spell from your Spellbook",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.TransformSpellSequence(spell => true));
                })));

        // 1: Duplicate
        ConditionalOption duplicate = new ConditionalOption(() => true,
            MakeEventOption("Duplicate", "Duplicate a Spell from your Spellbook",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.DuplicateSpellSequence(spell => true));
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", null)));

        // Add Options
        AddOptions(remove, transform, duplicate, leave);
    }
}

public class TheCostOfGreed : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TheCostOfGreed;
    public override Sprite EventArt => null;
    public override string EventName => "The Cost of Greed";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        int gainMoreGoldAmount = RandomHelper.RandomIntExclusive(GetEventSpec("GainMoreGoldAmountMin"), GetEventSpec("GainMoreGoldAmountMax"));
        int gainLessGoldAmount = RandomHelper.RandomIntExclusive(GetEventSpec("GainLessGoldAmountMin"), GetEventSpec("GainLessGoldAmountMax"));

        Spell greed = Spell.GetSpellOfType(SpellLabel.Greed);

        // 1: Gain Gold & Greed
        ConditionalOption steal = new ConditionalOption(() => true,
            MakeEventOption("Steal", "Gain " + gainMoreGoldAmount + " Gold, Become Cursed - Greed",
            () => false,
            t => spawnedToolTip = UIManager._Instance.SpawnSpellToolTip(greed, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(gainMoreGoldAmount);
                    GameManager._Instance.AddSpellToSpellBook(SpellLabel.Greed);
                }))); ;

        // 2: Gain Less Gold, but no Greed
        ConditionalOption noGreed = new ConditionalOption(() => true,
            MakeEventOption("Barter", "Pay " + gainLessGoldAmount + " Gold, Choose a Spell to Remove",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(-gainLessGoldAmount);
                    EventManager._Instance.StartCoroutine(GameManager._Instance.RemoveSpellSequence(spell => true));
                })));

        // Add Options
        AddOptions(steal, noGreed);
    }
}

public class RemoveCurseForGold : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.RemoveCurseForGold;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        int minCost = GetEventSpec("MinCost");
        int maxCost = GetEventSpec("MaxCost");
        int removalCost = RandomHelper.RandomIntExclusive(minCost, maxCost);

        int numCurses = GetEventSpec("NumCurses");
        bool hasSufficientCurses = GameManager._Instance.Spellbook.GetNumEntriesMatching(spell => spell.Color == SpellColor.Curse) > numCurses;

        Artifact deadCockroach = Artifact.GetArtifactOfType(ArtifactLabel.CockroachCarcass);

        // 1:
        ConditionalOption standard = new ConditionalOption(() => true,
            MakeEventOption("Standard Service", "Remove 1 Random Curse, Pay " + removalCost + " Gold",
            () => GameManager._Instance.GetPlayerCurrency() < removalCost,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.RemoveRandomSpellOfColor(SpellColor.Curse);
                    GameManager._Instance.AlterGold(-removalCost);
                })));

        // 2: 
        ConditionalOption premium = new ConditionalOption(() => true,
            MakeEventOption("Premium Service", "Remove " + numCurses + " Random Curses, Pay " + maxCost + " Gold",
            () => !hasSufficientCurses || GameManager._Instance.GetPlayerCurrency() < maxCost,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    for (int i = 0; i < numCurses; i++)
                    {
                        GameManager._Instance.RemoveRandomSpellOfColor(SpellColor.Curse);
                    }
                    GameManager._Instance.AlterGold(-maxCost);
                })));

        // 3:
        ConditionalOption vip = new ConditionalOption(() => true,
             MakeEventOption("Backroom Service", "Gain a Unique Artifact",
             () => !hasSufficientCurses,
             t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(deadCockroach, t), () => GameObject.Destroy(spawnedToolTip),
                 MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                 {
                     GameManager._Instance.AddArtifact(deadCockroach);
                 })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", null)));

        // Add Options
        AddOptions(standard, premium, vip, leave);
    }
}

public class TavernkeeperRescue : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TavernkeepRescue;
    public override Sprite EventArt => null;
    public override string EventName => "Tavernkeep Rescue";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        int interveneDamageAmount = Mathf.RoundToInt(((float)GetEventSpec("InterveneDamageAmount") / 100) * GameManager._Instance.GetMaxPlayerHP());
        int distractDamageAmount = Mathf.RoundToInt(((float)GetEventSpec("DistractDamageAmount") / 100) * GameManager._Instance.GetMaxPlayerHP());
        int distractGoldAmount = GetEventSpec("DistractGoldAmount");

        Artifact vipCard = Artifact.GetArtifactOfType(ArtifactLabel.VIPCard);

        // 1:
        ConditionalOption intervene = new ConditionalOption(() => true,
            MakeEventOption("Intervene", "Take " + interveneDamageAmount + " Damage, Recieve VIP Card",
            () => false, t => spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(vipCard, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterPlayerCurrentHP(-interveneDamageAmount, DamageType.Physical);
                    GameManager._Instance.AddArtifact(vipCard);
                })));

        ConditionalOption distract = new ConditionalOption(() => true,
            MakeEventOption("Distract", "Gain " + distractGoldAmount + " Gold, 75% Chance to Take " + distractDamageAmount + " Damage",
            () => false,
                MakeEventOptionOutcomeWithChance(25, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(distractGoldAmount);
                }),
                MakeEventOptionOutcomeWithChance(75, "Outcome Text", delegate
                {
                    GameManager._Instance.AlterGold(distractGoldAmount);
                    GameManager._Instance.AlterPlayerCurrentHP(-distractDamageAmount, DamageType.Physical);
                })));

        // 2: Leave
        ConditionalOption ignore = new ConditionalOption(() => true,
            MakeEventOption("Ignore", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", null)));

        // Add Options
        AddOptions(intervene, distract, ignore);
    }
}

public class TraumaRecall : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TraumaRecall;
    public override Sprite EventArt => null;
    public override string EventName => "Trauma Recall";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedCurseToolTip;
    private GameObject spawnedWitchesWardToolTip;

    protected override void InitializeEventData()
    {
        Spell specifiedCurse = GameManager._Instance.GetRandomSpellWithConditions(spell => spell.Color == SpellColor.Curse);


        // 1:
        ConditionalOption acknowledge = new ConditionalOption(() => true,
            MakeEventOption("Acknowledge", "Become Cursed - " + specifiedCurse.Name,
            () => false, t => spawnedCurseToolTip = UIManager._Instance.SpawnSpellToolTip(specifiedCurse, t), () => GameObject.Destroy(spawnedCurseToolTip),
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AddSpellToSpellBook(specifiedCurse);
                })));

        ConditionalOption ignore = new ConditionalOption(() => true,
            MakeEventOption("Ignore", "Become Cursed - Random (Not " + specifiedCurse.Name + ")",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    GameManager._Instance.AddSpellToSpellBook(GameManager._Instance.GetRandomSpellWithConditions(
                        spell => spell.Color == SpellColor.Curse && spell.Label != specifiedCurse.Label));
                })));

        ConditionalOption wardOff = new ConditionalOption(() => true,
            MakeEventOption("Ward Off", "Lose Witches Ward",
            () => GameManager._Instance.Spellbook.GetNumEntriesMatching(spell => spell.Label == SpellLabel.WitchesWard) <= 0,
            t => spawnedWitchesWardToolTip = UIManager._Instance.SpawnSpellToolTip(Spell.GetSpellOfType(SpellLabel.WitchesWard), t), () => GameObject.Destroy(spawnedWitchesWardToolTip),
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.RemoveSpellSequence(spell => spell.Label == SpellLabel.WitchesWard, 1));
                })));

        // Add Options
        AddOptions(acknowledge, ignore, wardOff);
    }
}

public class BookOfTheAmnesiac : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.BookOfTheAmnesiac;
    public override Sprite EventArt => null;
    public override string EventName => "Book of the Amnesiac";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        // 1:
        ConditionalOption read = new ConditionalOption(() => true,
            MakeEventOption("Read", "Choose a Spell to Remove from your Spellbook",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.RemoveSpellSequence(spell => true));
                })));

        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                })));

        // Add Options
        AddOptions(read, leave);
    }
}

public class BookOfTheRepurposed : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.BookOfTheRepurposed;
    public override Sprite EventArt => null;
    public override string EventName => "Book of the Repurposed";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        // 1:
        ConditionalOption read = new ConditionalOption(() => true,
            MakeEventOption("Read", "Choose a Spell to Transform",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.TransformSpellSequence(spell => true));
                })));

        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {
                })));

        // Add Options
        AddOptions(read, leave);
    }
}

public class RemoveTwoForAnger : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.RemoveTwoForAnger;
    public override Sprite EventArt => null;
    public override string EventName => "Name";
    protected override string defaultEventText => "Text";
    public override UseEventUI EventUI => UseEventUI.Story;

    private GameObject spawnedToolTip;

    protected override void InitializeEventData()
    {
        Spell anger = Spell.GetSpellOfType(SpellLabel.Anger);

        // 1:
        ConditionalOption option = new ConditionalOption(() => true,
            MakeEventOption("Remove", "Choose 2 Spells to Remove, Become Cursed - Anger",
            () => false, t => spawnedToolTip = UIManager._Instance.SpawnSpellToolTip(anger, t), () => GameObject.Destroy(spawnedToolTip),
                MakeEventOptionOutcomeWithChance(100, "You suddenly feel very upset about the loss of those cards...", delegate
                {
                    EventManager._Instance.StartCoroutine(GameManager._Instance.RemoveSpellSequence(spell => true, 2,
                        () => GameManager._Instance.AddSpellToSpellBook(anger)));
                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "", null)));

        // Add Options
        AddOptions(option, leave);
    }
}

public class ASuspiciousTome : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.ASuspiciousTome;
    public override Sprite EventArt => null;
    public override string EventName => "A Suspicious Tome";
    protected override string defaultEventText => "As you walk through the halls, a strange sound comes from one of the bookshelves. Investigating further, you notice a specific book which sits apart from all the rest." +
        " Of note, this particularly book seems to have one <shake>eye</>...";
    public override UseEventUI EventUI => UseEventUI.Story;

    protected override void InitializeEventData()
    {
        Combat combat = (Combat)MapManager._Instance.GetUniqueGameOccurance("PossessedTomeCombat");

        int stage = 0;

        // 1: Choice Between Reaching and Leaving
        // Choosing Reach will Chain into another Option where the only option is to fight
        ConditionalOption wonFight = new ConditionalOption(() => stage == 0,
            MakeEventOption("Reach", "?",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    UpdateEventText("The tome <shake>shakes</> violently before lifting up off of the bookcase. It whirls it's face around. You note it actually has two eyes before it charges...");
                    EventManager._Instance.ChainEvent(this);
                    stage = 1;
                })));

        // 2: Only Given the Optoin to Fight
        ConditionalOption fight = new ConditionalOption(() => stage == 1,
            MakeEventOption("Fight", "Fight the attacking Book",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    UpdateEventText("The book now lays on the ground, now lifeless as it should've been.");
                    StartFight(combat, true, delegate
                    {
                        stage = 2;
                        EventManager._Instance.SetWait(false);
                    }, false);
                })));

        // 3: View Reward
        ConditionalOption viewBook = new ConditionalOption(() => stage == 2,
            MakeEventOption("Read", "View the now Deceased Book",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "A frightening ordeal, hopefully one not to be repeated.", delegate
                {
                    RewardManager._Instance.AddRandomBookReward();
                    RewardManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen());
                })));

        // 4: Leave
        ConditionalOption leave = new ConditionalOption(() => stage == 0,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "A bookcase may be the least of your worries...", null)));

        // Add Options
        AddOptions(fight, wonFight, viewBook, leave);
    }
}

public class TakeASip : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.TakeASip;
    public override Sprite EventArt => null;
    public override string EventName => "Take a Sip?";
    protected override string defaultEventText => "You come across an ornate looking chalice. There are strange vibes emanating from it's direction, but stress, worry, and <bounce>adventure</> have made you quite parched.";
    public override UseEventUI EventUI => UseEventUI.Combat;

    protected override void InitializeEventData()
    {
        Combat testCombat = (Combat)MapManager._Instance.GetUniqueGameOccurance("HolyGrailCombat");
        bool fightDefeated = false;
        bool abandonedFight = false;

        // 
        ConditionalOption fight = new ConditionalOption(() => !fightDefeated && !abandonedFight,
            MakeEventOption("Sip from the Chalice", "Fight",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    UpdateEventText("Having aquired the spoils of war, you carry on...");
                    StartFight(testCombat, true, delegate
                    {
                        fightDefeated = true;

                        RewardManager._Instance.AddReward(ArtifactLabel.HarmonicChalice);
                        RewardManager._Instance.AddGoldReward(RandomHelper.RandomIntExclusive(25, 40));
                        RewardManager._Instance.AddReward(Potion.GetRandomPotion(false));
                        EventManager._Instance.StartCoroutine(RewardManager._Instance.ShowRewardScreen(() => EventManager._Instance.SetWait(false)));

                    }, false);
                })));

        //
        ConditionalOption leavePreFight = new ConditionalOption(() => !fightDefeated && !abandonedFight,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "", delegate
                {
                    UpdateEventText("Not quite thirsty enough to take that kind of risk...");
                    abandonedFight = true;
                    EventManager._Instance.ChainEvent(this);
                })));

        //
        ConditionalOption leave = new ConditionalOption(() => fightDefeated || abandonedFight,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "", () => EventManager._Instance.Resolve())));

        // Add Options
        AddOptions(fight, leavePreFight, leave);
    }
}


/*
public class OptionEvent : OptionEvent
{
    public override EventLabel EventLabel => EventLabel.;

    public override Sprite EventArt => null;

    public override string EventName => "Name";
    public override string EventText => "Text";

    protected override void InitializeEventData()
    {
        // 1:
        ConditionalOption option = new ConditionalOption(() => true,
            MakeEventOption("Hint Text", "Effect Text",
            () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", delegate
                {

                })));

        // 2: Leave
        ConditionalOption leave = new ConditionalOption(() => true,
            MakeEventOption("Leave", "", () => false,
                MakeEventOptionOutcomeWithChance(100, "Outcome Text", null)));

        // Add Options
        AddOptions(option, leave);
    }
}
*/