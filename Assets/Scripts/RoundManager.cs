using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RunStateHolder runStateHolder;
    [SerializeField] private PlayerHandZone playerHandZone;
    [SerializeField] private RoundHUDView roundHUDView;
    [SerializeField] private List<JokerBase> debugStartingJokers = new();

    [Header("Round Config")]
    [SerializeField] private int baseTargetScore = 300;
    [SerializeField] private int targetScorePerRoundIncrease = 75;
    [SerializeField] private int maxHandsPerRound = 4;
    [SerializeField] private int maxDiscardsPerRound = 2;

    [Header("Runtime")]
    [SerializeField] private int targetScore = 300;
    [SerializeField] private int currentScore;
    [SerializeField] private int handsRemaining;
    [SerializeField] private int discardsRemaining;
    [SerializeField] private RoundResultState roundResult = RoundResultState.InProgress;
    [SerializeField] private RoundPhaseType currentPhaseType;
    [SerializeField] private ShopPhaseType currentShopPhaseType;
    [SerializeField] private bool isInShopPhase;

    public int TargetScore => targetScore;
    public int CurrentScore => currentScore;
    public int HandsRemaining => handsRemaining;
    public int DiscardsRemaining => discardsRemaining;
    public RoundResultState RoundResult => roundResult;
    public RoundPhaseType CurrentPhaseType => currentPhaseType;
    public ShopPhaseType CurrentShopPhaseType => currentShopPhaseType;
    public bool IsInShopPhase => isInShopPhase;

    public bool IsRoundOver => roundResult != RoundResultState.InProgress;
    public bool HasWon => roundResult == RoundResultState.Won;
    public bool HasLost => roundResult == RoundResultState.Lost;
    public bool IsPvPRound => currentPhaseType == RoundPhaseType.PvP;
    [Header("Round Rewards")]
    [SerializeField] private int roundRewardGold = 18;

    public int RoundRewardGold => roundRewardGold;
    private bool rewardGranted;

    private void Start()
    {
        if (runStateHolder == null)
            runStateHolder = FindAnyObjectByType<RunStateHolder>();

        if (playerHandZone == null)
            playerHandZone = FindAnyObjectByType<PlayerHandZone>();

        if (roundHUDView == null)
            roundHUDView = FindAnyObjectByType<RoundHUDView>();

        InitializeRunStateIfNeeded();
        StartRound();
    }

    public void StartRound()
    {
        RunState run = GetRunState();
        if (run == null)
            return;

        currentPhaseType = IsPvPRoundIndex(run.RoundIndex)
            ? RoundPhaseType.PvP
            : RoundPhaseType.PvE;

        isInShopPhase = false;
        currentScore = 0;
        rewardGranted = false;
        handsRemaining = maxHandsPerRound;
        discardsRemaining = maxDiscardsPerRound;
        roundResult = RoundResultState.InProgress;
        targetScore = baseTargetScore + ((run.RoundIndex - 1) * targetScorePerRoundIncrease);

        Debug.Log("=== ROUND STARTED ===");
        Debug.Log($"Stage: {run.CycleIndex}");
        Debug.Log($"Round: {run.RoundIndex}");
        Debug.Log($"Phase: {currentPhaseType}");
        Debug.Log($"Target Score: {targetScore}");
        Debug.Log($"Hands Remaining: {handsRemaining}");
        Debug.Log($"Discards Remaining: {discardsRemaining}");
        Debug.Log($"Lives Remaining: {run.Lives}");
        Debug.Log($"Reward Gold: {roundRewardGold}");

        if (roundHUDView != null)
            roundHUDView.ResetForNewRound();

        DispatchRoundEvent(JokerEventType.RoundStarted, run);
    }

    private void InitializeRunStateIfNeeded()
    {
        if (runStateHolder == null)
        {
            Debug.LogError("RunStateHolder not found in scene.");
            return;
        }

        if (HasValidRunState())
            return;

        if (runStateHolder.CurrentRunState != null)
            Debug.LogWarning("RoundManager found invalid RunState. Reinitializing run state.");

        CharacterData character = new CharacterData(
            id: "seer",
            displayName: "Seer",
            description: "Reveals next duel field before final prep.",
            passiveType: CharacterPassiveType.RevealNextDuelFieldBeforePrep
        );

        GlobalFieldData field = new GlobalFieldData(
            id: "mana_surge",
            displayName: "Mana Surge",
            description: "Increase chip gain.",
            effectType: FieldEffectType.BonusChipsPercent,
            effectValue: 10
        );

        RunState run = new RunState(character, field);
        run.SetEquippedJokers(debugStartingJokers);
        runStateHolder.InitializeRun(run);

        Debug.Log("RunState initialized.");
    }

    public bool CanPlayHand()
    {
        return !IsRoundOver && !isInShopPhase && handsRemaining > 0;
    }

    public bool CanDiscard()
    {
        return !IsRoundOver && !isInShopPhase && discardsRemaining > 0;
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
                RunState run = GetRunState();
                if (run != null)
                {
                    run.AddGold(roundRewardGold);
                    rewardGranted = true;
                }
            }

            Debug.Log("=== ROUND WON ===");
            Debug.Log(IsPvPRound ? "PvP round won." : "PvE round won.");
            Debug.Log($"Reward Gold: {roundRewardGold}");
            DispatchRoundEvent(JokerEventType.RoundWon, GetRunState());
            return;
        }

        if (handsRemaining <= 0)
        {
            roundResult = RoundResultState.Lost;

            RunState run = GetRunState();
            if (run == null)
                return;

            run.LoseLife();

            Debug.Log("=== ROUND LOST ===");
            Debug.Log(IsPvPRound ? "PvP round lost." : "PvE round lost.");
            Debug.Log($"Lives remaining: {run.Lives}");
            DispatchRoundEvent(JokerEventType.RoundLost, run);
        }
    }

    public void CompleteRoundFlow()
    {
        if (!IsRoundOver)
            return;

        RunState run = GetRunState();
        if (run == null)
            return;

        Debug.Log("=== ROUND TRANSITION ===");
        Debug.Log($"Current Stage: {run.CycleIndex}");
        Debug.Log($"Current Round: {run.RoundIndex}");
        Debug.Log($"Current Lives: {run.Lives}");
        Debug.Log($"Current Gold: {run.Gold}");
        Debug.Log($"Current Round Score: {currentScore}/{targetScore}");

        if (run.IsGameOver())
        {
            HandleGameOver();
            return;
        }

        EnterShopPhase();
        run.AdvanceToNextRound();

        Debug.Log("=== NEXT ROUND READY ===");
        Debug.Log($"Next Stage: {run.CycleIndex}");
        Debug.Log($"Next Round: {run.RoundIndex}");
        Debug.Log($"Lives: {run.Lives}");
        Debug.Log($"Gold: {run.Gold}");
        Debug.Log($"Next Target Score: {baseTargetScore + ((run.RoundIndex - 1) * targetScorePerRoundIncrease)}");

        StartNextRound();
    }

    private void EnterShopPhase()
    {
        RunState run = GetRunState();
        if (run == null)
            return;

        int nextRoundIndex = run.RoundIndex + 1;
        currentShopPhaseType = IsPvPRoundIndex(nextRoundIndex)
            ? ShopPhaseType.FinalPrep
            : ShopPhaseType.Normal;

        isInShopPhase = true;

        Debug.Log("=== SHOP PHASE ===");
        Debug.Log($"Shop Type: {currentShopPhaseType}");
        Debug.Log(currentShopPhaseType == ShopPhaseType.FinalPrep
            ? "FinalPrep shop opened before PvP round."
            : "Normal shop opened.");

        JokerEventDispatcher.DispatchShopEvent(new JokerShopContext(
            run,
            currentShopPhaseType,
            run.GetEquippedJokers()));
    }

    private void StartNextRound()
    {
        if (playerHandZone != null)
            playerHandZone.ResetHandForNewRound();

        StartRound();
    }

    private void HandleGameOver()
    {
        Debug.Log("=== GAME OVER ===");
        ResetRun();
    }

    public void ResetRun()
    {
        if (runStateHolder == null)
            return;

        CharacterData character = new CharacterData(
            id: "seer",
            displayName: "Seer",
            description: "Reveals next duel field before final prep.",
            passiveType: CharacterPassiveType.RevealNextDuelFieldBeforePrep
        );

        GlobalFieldData field = new GlobalFieldData(
            id: "mana_surge",
            displayName: "Mana Surge",
            description: "Increase chip gain.",
            effectType: FieldEffectType.BonusChipsPercent,
            effectValue: 10
        );

        RunState freshRun = new RunState(character, field, startingLives: 3);
        freshRun.SetEquippedJokers(debugStartingJokers);
        runStateHolder.InitializeRun(freshRun);

        isInShopPhase = false;
        currentShopPhaseType = ShopPhaseType.Normal;

        if (playerHandZone != null)
            playerHandZone.ResetDeckAndHandForNewRun();

        StartRound();
    }

    public void SetTargetScore(int newTargetScore)
    {
        baseTargetScore = Mathf.Max(1, newTargetScore);
        targetScore = baseTargetScore;
    }

    private RunState GetRunState()
    {
        if (runStateHolder == null || runStateHolder.CurrentRunState == null)
        {
            Debug.LogError("RoundManager: RunState is missing.");
            return null;
        }

        if (!runStateHolder.CurrentRunState.IsValidRuntimeState)
        {
            Debug.LogError(
                $"RoundManager: RunState is invalid. Lives={runStateHolder.CurrentRunState.Lives}, Stage={runStateHolder.CurrentRunState.CycleIndex}, Round={runStateHolder.CurrentRunState.RoundIndex}");
            return null;
        }

        return runStateHolder.CurrentRunState;
    }

    private bool IsPvPRoundIndex(int roundIndex)
    {
        return roundIndex % 4 == 0;
    }

    private bool HasValidRunState()
    {
        return runStateHolder != null &&
               runStateHolder.CurrentRunState != null &&
               runStateHolder.CurrentRunState.IsValidRunStartState;
    }

    private void DispatchRoundEvent(JokerEventType eventType, RunState runState)
    {
        if (runState == null)
            return;

        JokerEventDispatcher.DispatchRoundEvent(
            eventType,
            new JokerRoundContext(
                runState,
                currentPhaseType,
                targetScore,
                currentScore,
                handsRemaining,
                discardsRemaining,
                runState.GetEquippedJokers()));
    }
}
