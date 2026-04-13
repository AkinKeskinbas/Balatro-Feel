// === VIEW ===
// CardVisual is purely responsible for animations and rendering.
// It subscribes to Card (ViewModel) events and never drives game logic.

using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    private bool initialized = false;

    [Header("Card")] public Card parentCard;
    private Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    private Vector3 movementDelta;
    private Canvas canvas;

    // Cached once — never looked up per-frame
    private Camera mainCamera;
    private Vector3 mouseWorldPosition;

    [Header("References")] public Transform visualShadow;
    private float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Image cardImage;

    [Header("Follow Parameters")] [SerializeField]
    private float followSpeed = 30;

    [Header("Rotation Parameters")] [SerializeField]
    private float rotationAmount = 20;

    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")] [SerializeField]
    private bool scaleAnimations = true;

    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")] [SerializeField]
    private float selectPunchAmount = 20;

    [Header("Hover Parameters")] [SerializeField]
    private float hoverPunchAngle = 5;

    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")] [SerializeField]
    private bool swapAnimations = true;

    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")] [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;

    [Header("Debug Face")] [SerializeField]
    private TextMeshProUGUI rankText;

    [SerializeField] private TextMeshProUGUI suitText;

    [Header("Contribution")] [SerializeField]
    private GameObject contributionRoot;

    [SerializeField] private TextMeshProUGUI chipContributionText;
    private PlayedCardDisplay playedCardDisplay;

    private void Start()
    {
        shadowDistance = visualShadow.localPosition;
    }

    public void Initialize(Card target, int index = 0)
    {
        parentCard = target;
        cardTransform = target.transform;
        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();
        mainCamera = Camera.main;
        playedCardDisplay = GetComponent<PlayedCardDisplay>();

        // Subscribe to ViewModel events
        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);
        UpdateDebugFace();
        HideContribution();
        initialized = true;
    }

    private void UpdateDebugFace()
    {
        if (parentCard == null)
            return;

        if (playedCardDisplay != null)
        {
            playedCardDisplay.Setup(parentCard);
            return;
        }

        if (rankText != null)
            rankText.text = GetRankLabel(parentCard.Rank);

        if (suitText != null)
        {
            suitText.text = GetSuitLabel(parentCard.Suit);
            suitText.color = GetSuitColor(parentCard.Suit);
        }
    }

    private string GetRankLabel(Rank rank)
    {
        return rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
            Rank.Nine => "9",
            Rank.Eight => "8",
            Rank.Seven => "7",
            Rank.Six => "6",
            Rank.Five => "5",
            Rank.Four => "4",
            Rank.Three => "3",
            Rank.Two => "2",
            _ => "?"
        };
    }

    private string GetSuitLabel(Suit suit)
    {
        return suit switch
        {
            Suit.Spades => "♠",
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            _ => "?"
        };
    }

    private Color GetSuitColor(Suit suit)
    {
        return suit switch
        {
            Suit.Hearts => Color.red,
            Suit.Diamonds => Color.red,
            Suit.Spades => Color.black,
            Suit.Clubs => Color.black,
            _ => Color.white
        };
    }

    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    void Update()
    {
        if (!initialized || parentCard == null) return;

        // Cache once per frame instead of calling per-method
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }

    public void ShowChipContribution(int amount)
    {
        if (playedCardDisplay != null)
        {
            playedCardDisplay.ShowChipContributionAnimated(amount);
            return;
        }

        if (contributionRoot == null || chipContributionText == null)
            return;

        contributionRoot.SetActive(true);
        chipContributionText.text = $"+{amount}";

        contributionRoot.transform.localScale = Vector3.one * 0.8f;
        contributionRoot.transform.DOKill(true);

        contributionRoot.transform
            .DOScale(1f, 0.15f)
            .SetEase(Ease.OutBack);
    }

    public void HideContribution()
    {
        if (playedCardDisplay != null)
        {
            playedCardDisplay.HideContribution();
            return;
        }

        if (contributionRoot == null)
            return;

        contributionRoot.SetActive(false);
    }

    private void HandPositioning()
    {
        curveYOffset = (curve.positioning.Evaluate(parentCard.NormalizedPosition()) * curve.positioningInfluence) *
                       parentCard.SiblingAmount();
        curveYOffset = parentCard.SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition());
    }

    private void SmoothFollow()
    {
        Vector3 verticalOffset = Vector3.up * (parentCard.isDragging ? 0 : curveYOffset);
        transform.position = Vector3.Lerp(transform.position, cardTransform.position + verticalOffset,
            followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = transform.position - cardTransform.position;
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,
            Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        savedIndex = parentCard.isDragging ? savedIndex : parentCard.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

        // mouseWorldPosition already computed in Update()
        Vector3 offset = transform.position - mouseWorldPosition;
        float tiltX = parentCard.isHovering ? (offset.y * -1 * manualTiltAmount) : 0;
        float tiltY = parentCard.isHovering ? (offset.x * manualTiltAmount) : 0;
        float tiltZ = parentCard.isDragging
            ? tiltParent.eulerAngles.z
            : curveRotationOffset * (curve.rotationInfluence * parentCard.SiblingAmount());

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount),
            tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount),
            tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    // --- Event handlers (bound to Card ViewModel) ---

    private void Select(Card card, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

        if (scaleAnimations)
            transform.DOScale(state ? scaleOnSelect : 1f, scaleTransition).SetEase(scaleEase);
    }

    public void Swap(float dir = 1)
    {
        if (!swapAnimations) return;

        DOTween.Kill(3, true);
        shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1)
            .SetId(3);
    }

    private void BeginDrag(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    private void EndDrag(Card card)
    {
        canvas.overrideSorting = false;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(Card card)
    {
        if (!parentCard.wasDragged)
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(Card card, bool longPress)
    {
        if (scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = false;
        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    private void PointerDown(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        visualShadow.localPosition += -Vector3.up * shadowOffset;
        shadowCanvas.overrideSorting = false;
    }

    public void KillTweens()
    {
        transform.DOKill(true);

        if (shakeParent != null)
            shakeParent.DOKill(true);

        if (tiltParent != null)
            tiltParent.DOKill(true);

        if (visualShadow != null)
            visualShadow.DOKill(true);

        if (cardImage != null)
            cardImage.DOKill(true);
    }

    private void OnDestroy()
    {
        KillTweens();

        if (parentCard == null)
            return;

        parentCard.PointerEnterEvent.RemoveListener(PointerEnter);
        parentCard.PointerExitEvent.RemoveListener(PointerExit);
        parentCard.BeginDragEvent.RemoveListener(BeginDrag);
        parentCard.EndDragEvent.RemoveListener(EndDrag);
        parentCard.PointerDownEvent.RemoveListener(PointerDown);
        parentCard.PointerUpEvent.RemoveListener(PointerUp);
        parentCard.SelectEvent.RemoveListener(Select);
    }
}
