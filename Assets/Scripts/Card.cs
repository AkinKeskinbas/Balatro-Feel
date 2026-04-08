using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class Card : MonoBehaviour,
    IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerUpHandler, IPointerDownHandler
{
    [Header("Data")]
    [SerializeField] private CardData cardData;

    public CardData CardData => cardData;
    public Rank Rank => cardData.rank;
    public Suit Suit => cardData.suit;
    public int RankValue => (int)cardData.rank;

    [Header("Visual")]
    [SerializeField] private bool instantiateVisual = true;
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50f;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50f;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    private Canvas canvas;
    private Image imageComponent;
    private Camera mainCamera;
    private GraphicRaycaster graphicRaycaster;
    private VisualCardsHandler visualHandler;
    private Vector3 dragOffset;
    private float pointerDownTime;
    private float pointerUpTime;

    private void Awake()
    {
        if (cardData == null)
            cardData = new CardData(Rank.Two, Suit.Clubs);
    }

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        mainCamera = Camera.main;
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();

        if (!instantiateVisual)
            return;

        visualHandler = VisualCardsHandler.instance;
        cardVisual = Instantiate(
            cardVisualPrefab,
            visualHandler ? visualHandler.transform : canvas.transform
        ).GetComponent<CardVisual>();

        cardVisual.Initialize(this);
    }

    private void Update()
    {
        ClampPosition();

        if (!isDragging)
            return;

        Vector2 targetPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition) - dragOffset;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        Vector2 velocity = direction * Mathf.Min(
            moveSpeedLimit,
            Vector2.Distance(transform.position, targetPosition) / Time.deltaTime
        );

        transform.Translate(velocity * Time.deltaTime);
    }

    public void SetData(CardData data)
    {
        cardData = data;
    }

    public void SetData(Rank rank, Suit suit)
    {
        cardData = new CardData(rank, suit);
    }

    public string GetCardName()
    {
        return $"{Rank} of {Suit}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = mousePosition - (Vector2)transform.position;

        isDragging = true;
        graphicRaycaster.enabled = false;
        imageComponent.raycastTarget = false;
        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);

        isDragging = false;
        graphicRaycaster.enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(ResetDraggedFlagNextFrame());
    }

    private IEnumerator ResetDraggedFlagNextFrame()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;
        bool isLongPress = pointerUpTime - pointerDownTime > 0.2f;

        PointerUpEvent.Invoke(this, isLongPress);

        if (isLongPress || wasDragged)
            return;

        ToggleSelection();
    }

    private void ToggleSelection()
    {
        selected = !selected;
        SelectEvent.Invoke(this, selected);

        transform.localPosition = selected
            ? cardVisual.transform.up * selectionOffset
            : Vector3.zero;
    }

    public void Deselect()
    {
        if (!selected)
            return;

        selected = false;
        SelectEvent.Invoke(this, false);
        transform.localPosition = Vector3.zero;
    }

    private void ClampPosition()
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(
            new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z)
        );

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot")
            ? transform.parent.GetSiblingIndex()
            : 0;
    }

    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot")
            ? transform.parent.parent.childCount - 1
            : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot")
            ? ExtensionMethods.Remap(
                ParentIndex(),
                0,
                transform.parent.parent.childCount - 1,
                0,
                1
            )
            : 0;
    }
    public void KillTweens()
    {
        transform.DOKill(true);

        if (cardVisual != null)
        {
            cardVisual.KillTweens();
            cardVisual.transform.DOKill(true);
            cardVisual.DOKill(true);
        }

        foreach (Transform child in transform)
        {
            child.DOKill(true);
        }
    }

    private void OnDestroy()
    {
        if (cardVisual != null)
            Destroy(cardVisual.gameObject);
        
    }
}