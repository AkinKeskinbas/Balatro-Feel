using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Round Config")]
    [SerializeField] private int targetScore = 300;
    [SerializeField] private int maxHandsPerRound = 4;
    [SerializeField] private int maxDiscardsPerRound = 2;

    [Header("Runtime")]
    [SerializeField] private int currentScore;
    [SerializeField] private int handsRemaining;
    [SerializeField] private int discardsRemaining;
    [SerializeField] private RoundResultState roundResult = RoundResultState.InProgress;

    public int TargetScore => targetScore;
    public int CurrentScore => currentScore;
    public int HandsRemaining => handsRemaining;
    public int DiscardsRemaining => discardsRemaining;
    public RoundResultState RoundResult => roundResult;

    public bool IsRoundOver => roundResult != RoundResultState.InProgress;
    public bool HasWon => roundResult == RoundResultState.Won;
    public bool HasLost => roundResult == RoundResultState.Lost;
    [Header("Round Rewards")]
    [SerializeField] private int roundRewardGold = 18;

    public int RoundRewardGold => roundRewardGold;
    private bool rewardGranted;
    private void Start()
    {
        InitializeRunState();
        StartRound();
    }

    public void StartRound()
    {
        currentScore = 0;
        rewardGranted = false;
        handsRemaining = maxHandsPerRound;
        discardsRemaining = maxDiscardsPerRound;
        roundResult = RoundResultState.InProgress;

        Debug.Log("=== ROUND STARTED ===");
        Debug.Log($"Target Score: {targetScore}");
        Debug.Log($"Hands Remaining: {handsRemaining}");
        Debug.Log($"Discards Remaining: {discardsRemaining}");
        Debug.Log($"Reward Gold: {roundRewardGold}");
    }
    private void InitializeRunState()
    {
        RunStateHolder holder = FindAnyObjectByType<RunStateHolder>();

        if (holder == null)
        {
            Debug.LogError("RunStateHolder not found in scene.");
            return;
        }

        // TEST CHARACTER
        CharacterData character = new CharacterData(
            id: "seer",
            displayName: "Seer",
            description: "Reveals next duel field before final prep.",
            passiveType: CharacterPassiveType.RevealNextDuelFieldBeforePrep
        );

        // TEST FIELD
        GlobalFieldData field = new GlobalFieldData(
            id: "mana_surge",
            displayName: "Mana Surge",
            description: "Increase chip gain.",
            effectType: FieldEffectType.BonusChipsPercent,
            effectValue: 10
        );

        RunState run = new RunState(character, field);

        holder.InitializeRun(run);

        Debug.Log("RunState initialized.");
    }
    public bool CanPlayHand()
    {
        return !IsRoundOver && handsRemaining > 0;
    }

    public bool CanDiscard()
    {
        return !IsRoundOver && discardsRemaining > 0;
    }

    public void ConsumeDiscard()
    {
        if (!CanDiscard())
            return;

        discardsRemaining--;
        Debug.Log($"Discard used. Remaining: {discardsRemaining}");
    }

    public void PlayHand(HandScoreBreakdown breakdown)
    {
        if (!CanPlayHand())
        {
            Debug.LogWarning("Cannot play hand. Round is over or no hands remaining.");
            return;
        }

        handsRemaining--;
        currentScore += breakdown.FinalScore;

        Debug.Log("=== HAND PLAYED ===");
        foreach (var line in breakdown.DebugLines)
            Debug.Log(line);

        Debug.Log($"Round Score: {currentScore}/{targetScore}");
        Debug.Log($"Hands Remaining: {handsRemaining}");

        EvaluateRoundState();
    }

    private void EvaluateRoundState()
    {
        if (currentScore >= targetScore)
        {
            roundResult = RoundResultState.Won;

            if (!rewardGranted)
            {
                RunStateHolder holder = FindAnyObjectByType<RunStateHolder>();
                if (holder != null && holder.CurrentRunState != null)
                {
                    holder.CurrentRunState.AddGold(roundRewardGold);
                    rewardGranted = true;
                }
            }

            Debug.Log("=== ROUND WON ===");
            return;
        }

        if (handsRemaining <= 0)
        {
            roundResult = RoundResultState.Lost;
            Debug.Log("=== ROUND LOST ===");
        }
    }

    public void SetTargetScore(int newTargetScore)
    {
        targetScore = Mathf.Max(1, newTargetScore);
    }
}