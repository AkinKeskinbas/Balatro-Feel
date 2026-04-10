using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RoundHUDView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RunStateHolder runStateHolder;

    [Header("Score Section")]
    [SerializeField] private TextMeshProUGUI targetScoreText;
    [SerializeField] private TextMeshProUGUI roundScoreText;
    [SerializeField] private TextMeshProUGUI rewardText;

    [Header("Combo Section")]
    [SerializeField] private TextMeshProUGUI handTypeText;
    [SerializeField] private TextMeshProUGUI chipsText;
    [SerializeField] private TextMeshProUGUI multText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Top Info Row")]
    [SerializeField] private TextMeshProUGUI handsValueText;
    [SerializeField] private TextMeshProUGUI discardsValueText;

    [Header("Field Section")]
    [SerializeField] private TextMeshProUGUI fieldNameText;
    [SerializeField] private TextMeshProUGUI fieldDescText;

    [Header("Bottom Info Row")]
    [SerializeField] private TextMeshProUGUI goldValueText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI nextPhaseText;

    private Coroutine handPreviewRoutine;
    private Coroutine roundScoreRoutine;
    private int animatedChipValue;

    private void Start()
    {
        if (roundManager == null)
            roundManager = FindAnyObjectByType<RoundManager>();

        if (runStateHolder == null)
            runStateHolder = FindAnyObjectByType<RunStateHolder>();

        ClearHandPreview();
        RefreshStaticInfo();
    }

    private void Update()
    {
        RefreshStaticInfo();
    }

    private void OnDisable()
    {
        StopActiveAnimations();
    }

    private void OnDestroy()
    {
        StopActiveAnimations();
    }

    public void ResetAnimatedHandValues(HandScoreBreakdown breakdown)
    {
        if (breakdown == null)
            return;

        animatedChipValue = breakdown.BaseChips;

        if (handTypeText != null)
            handTypeText.text = $"{FormatHandName(breakdown.HandType)} lvl.1";

        if (chipsText != null)
            chipsText.text = animatedChipValue.ToString();

        if (multText != null)
            multText.text = breakdown.BaseMult.ToString();

        if (finalScoreText != null)
            finalScoreText.text = "0";
    }
    public IEnumerator AnimateChipContributionStep(int amount)
    {
        float duration = 0.18f;

        int from = animatedChipValue;
        int to = animatedChipValue + amount;
        animatedChipValue = to;

        if (chipsText != null)
        {
            yield return chipsText
                .DOCountInt(from, to, duration)
                .WaitForCompletion();

            chipsText.DOPunchScaleText();
        }
    }
    private void RefreshStaticInfo()
    {
        if (roundManager != null)
        {
            if (targetScoreText != null)
                targetScoreText.text = roundManager.TargetScore.ToString();

            if (rewardText != null)
                rewardText.text = $"Reward: ${roundManager.RoundRewardGold}";

            if (handsValueText != null)
                handsValueText.text = roundManager.HandsRemaining.ToString();

            if (discardsValueText != null)
                discardsValueText.text = roundManager.DiscardsRemaining.ToString();
        }

        if (runStateHolder != null && runStateHolder.CurrentRunState != null)
        {
            RunState run = runStateHolder.CurrentRunState;

            if (fieldNameText != null && run.SelectedGlobalField != null)
                fieldNameText.text = run.SelectedGlobalField.DisplayName;

            if (fieldDescText != null && run.SelectedGlobalField != null)
                fieldDescText.text = run.SelectedGlobalField.Description;

            if (goldValueText != null)
                goldValueText.text = $"${run.Gold}";

            if (stageText != null)
                stageText.text = $"Stage {run.CycleIndex}";

            if (roundText != null)
                roundText.text = $"Round {GetRoundInCycle(run.RoundIndex)} / 4";

            if (nextPhaseText != null)
                nextPhaseText.text = GetNextPhaseText(run.RoundIndex);
        }
    }

    public void ShowHandPreviewInstant(HandScoreBreakdown breakdown)
    {
        if (breakdown == null)
            return;

        if (handTypeText != null)
            handTypeText.text = $"{FormatHandName(breakdown.HandType)} lvl.1";

        if (chipsText != null)
            chipsText.text = breakdown.BaseChips.ToString();

        if (multText != null)
            multText.text = breakdown.BaseMult.ToString();

        if (finalScoreText != null)
            finalScoreText.text = "0";
    }

    public void PlayHandScoreSequence(HandScoreBreakdown breakdown, int roundScoreBefore, int roundScoreAfter)
    {
        if (handPreviewRoutine != null)
            StopCoroutine(handPreviewRoutine);

        handPreviewRoutine = StartCoroutine(PlayHandScoreSequenceRoutine(breakdown, roundScoreBefore, roundScoreAfter));
    }

    private IEnumerator PlayHandScoreSequenceRoutine(HandScoreBreakdown breakdown, int roundScoreBefore, int roundScoreAfter)
    {
        if (breakdown == null)
            yield break;

        int totalChips = breakdown.TotalChips;
        int baseMult = breakdown.BaseMult;
        int totalMult = breakdown.TotalMult;
        int finalScore = breakdown.FinalScore;

        // Hand type burada değişmesin
        // Chips burada değişmesin
        // Çünkü chips zaten contribution callbackleriyle doğru final değere geldi

        if (multText != null)
            multText.text = baseMult.ToString();

        if (finalScoreText != null)
            finalScoreText.text = "0";

        yield return new WaitForSeconds(0.1f);

        // Mult artışı (şimdilik base -> total, eğer aynıysa sabit kalır)
        if (multText != null)
        {
            yield return multText
                .DOCountInt(baseMult, totalMult, 0.3f)
                .WaitForCompletion();

            multText.DOPunchScaleText();
        }

        yield return new WaitForSeconds(0.12f);

        // Final score = mevcut total chips * total mult
        if (finalScoreText != null)
        {
            yield return finalScoreText
                .DOCountInt(0, finalScore, 0.55f)
                .WaitForCompletion();

            finalScoreText.DOPunchScaleText(1.2f, 0.22f);
        }

        yield return new WaitForSeconds(0.18f);

        // Round total en son artsın
        if (roundScoreText != null)
        {
            yield return roundScoreText
                .DOCountInt(roundScoreBefore, roundScoreAfter, 0.45f)
                .WaitForCompletion();

            roundScoreText.DOPunchScaleText();
        }
    }

    public void ClearHandPreview()
    {
        StopActiveAnimations();

        if (handTypeText != null)
            handTypeText.text = "-";

        if (chipsText != null)
            chipsText.text = "0";

        if (multText != null)
            multText.text = "0";

        if (finalScoreText != null)
            finalScoreText.text = "0";
    }

    public void ResetForNewRound()
    {
        StopActiveAnimations();
        RefreshStaticInfo();

        if (roundScoreText != null)
            roundScoreText.text = "0";

        if (handTypeText != null)
            handTypeText.text = "-";

        if (chipsText != null)
            chipsText.text = "0";

        if (multText != null)
            multText.text = "0";

        if (finalScoreText != null)
            finalScoreText.text = "0";
    }

    private void StopActiveAnimations()
    {
        if (handPreviewRoutine != null)
        {
            StopCoroutine(handPreviewRoutine);
            handPreviewRoutine = null;
        }

        if (roundScoreRoutine != null)
        {
            StopCoroutine(roundScoreRoutine);
            roundScoreRoutine = null;
        }

        targetScoreText?.DOKill(true);
        roundScoreText?.DOKill(true);
        rewardText?.DOKill(true);
        handTypeText?.DOKill(true);
        chipsText?.DOKill(true);
        multText?.DOKill(true);
        finalScoreText?.DOKill(true);
        handsValueText?.DOKill(true);
        discardsValueText?.DOKill(true);
        fieldNameText?.DOKill(true);
        fieldDescText?.DOKill(true);
        goldValueText?.DOKill(true);
        stageText?.DOKill(true);
        roundText?.DOKill(true);
        nextPhaseText?.DOKill(true);
    }

    private int GetRoundInCycle(int absoluteRoundIndex)
    {
        int mod = absoluteRoundIndex % 4;
        return mod == 0 ? 4 : mod;
    }

    private string GetNextPhaseText(int roundIndex)
    {
        int nextRound = roundIndex + 1;
        bool nextIsPvP = nextRound % 4 == 0;
        return nextIsPvP ? "Next: PvP" : "Next: PvE";
    }

    private string FormatHandName(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.HighCard => "High Card",
            PokerHandType.OnePair => "One Pair",
            PokerHandType.TwoPair => "Two Pair",
            PokerHandType.ThreeOfAKind => "Three of a Kind",
            PokerHandType.Straight => "Straight",
            PokerHandType.Flush => "Flush",
            PokerHandType.FullHouse => "Full House",
            PokerHandType.FourOfAKind => "Four of a Kind",
            PokerHandType.StraightFlush => "Straight Flush",
            _ => handType.ToString()
        };
    }
}
