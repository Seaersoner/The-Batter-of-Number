using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 拖拽组件，可挂载在任何需要拖拽的物体上
/// </summary>
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("拖拽设置")]
    [SerializeField] private bool isDraggable = true;
    [SerializeField] private bool returnToOriginalPosition = false;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private bool constrainToScreen = true;
    
    [Header("视觉效果")]
    [SerializeField] private bool scaleDuringDrag = true;
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private bool fadeOtherObjects = true;
    [SerializeField] private float fadeAlpha = 0.5f;
    
    [Header("物理设置")]
    [SerializeField] private bool disablePhysicsDuringDrag = true;
    [SerializeField] private LayerMask dragLayer = -1;
    
    // 拖拽状态
    private bool isDragging = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalLayer;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    
    // 物理组件
    private Rigidbody2D rb2D;
    private Collider2D col2D;
    private bool originalKinematic;
    
    // 卡牌数据（如果是卡牌）
    private CardData cardData;
    
    // 事件
    public System.Action<Draggable> OnDragStart;
    public System.Action<Draggable> OnDragEnd;
    public System.Action<Draggable, Vector3> OnDragUpdate;
    public System.Action<Draggable, GameObject> OnDroppedOn;
    
    // 属性
    public bool IsDraggable
    {
        get => isDraggable;
        set => isDraggable = value;
    }
    
    public bool IsDragging => isDragging;
    public CardData CardData => cardData;
    public Vector3 OriginalPosition => originalPosition;
    
    private void Awake()
    {
        InitializeDraggable();
    }
    
    private void Start()
    {
        SetupComponents();
    }
    
    /// <summary>
    /// 初始化拖拽组件
    /// </summary>
    private void InitializeDraggable()
    {
        // 获取卡牌数据（如果存在）
        Card cardComponent = GetComponent<Card>();
        if (cardComponent != null)
        {
            cardData = cardComponent.CardData;
        }
        
        // 记录原始状态
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalLayer = gameObject.layer;
    }
    
    /// <summary>
    /// 设置组件
    /// </summary>
    private void SetupComponents()
    {
        // 获取Canvas
        parentCanvas = GetComponentInParent<Canvas>();
        
        // 获取或创建CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 获取物理组件
        rb2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<Collider2D>();
        
        if (rb2D != null)
        {
            originalKinematic = rb2D.isKinematic;
        }
        
        // 确保有GraphicRaycaster用于检测
        if (parentCanvas != null && parentCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    /// <summary>
    /// 设置卡牌数据
    /// </summary>
    /// <param name="data">卡牌数据</param>
    public void SetCardData(CardData data)
    {
        cardData = data;
    }
    
    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        isDragging = true;
        originalPosition = transform.position;
        
        // 视觉效果
        ApplyDragVisualEffects(true);
        
        // 物理设置
        if (disablePhysicsDuringDrag && rb2D != null)
        {
            rb2D.isKinematic = true;
        }
        
        // 设置拖拽层级
        if (dragLayer != -1)
        {
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(dragLayer.value, 2));
        }
        
        // 触发事件
        OnDragStart?.Invoke(this);
        
        Debug.Log($"开始拖拽: {gameObject.name}");
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("CardDrag");
        }
    }
    
    /// <summary>
    /// 拖拽中
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || !isDragging) return;
        
        Vector3 worldPosition;
        
        // 根据Canvas类型计算位置
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            worldPosition = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out worldPosition
            );
        }
        
        // 限制在屏幕内
        if (constrainToScreen)
        {
            worldPosition = ConstrainToScreen(worldPosition);
        }
        
        transform.position = worldPosition;
        
        // 触发更新事件
        OnDragUpdate?.Invoke(this, worldPosition);
    }
    
    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        isDragging = false;
        
        // 检查放置目标
        GameObject dropTarget = GetDropTarget(eventData);
        
        bool validDrop = false;
        
        if (dropTarget != null)
        {
            // 检查是否是有效的放置目标
            BoardSlot boardSlot = dropTarget.GetComponent<BoardSlot>();
            if (boardSlot != null && boardSlot.CanPlaceCard())
            {
                // 尝试放置卡牌
                if (cardData != null && GameManager.Instance != null)
                {
                    Player currentPlayer = GameManager.Instance.HumanPlayer;
                    if (currentPlayer != null && currentPlayer.PlayCard(cardData, boardSlot))
                    {
                        validDrop = true;
                        OnDroppedOn?.Invoke(this, dropTarget);
                        
                        // 成功放置，销毁拖拽对象或隐藏
                        HandleSuccessfulDrop();
                    }
                }
            }
        }
        
        if (!validDrop)
        {
            // 无效放置，返回原位置
            HandleInvalidDrop();
        }
        
        // 恢复视觉效果
        ApplyDragVisualEffects(false);
        
        // 恢复物理设置
        if (disablePhysicsDuringDrag && rb2D != null)
        {
            rb2D.isKinematic = originalKinematic;
        }
        
        // 恢复层级
        gameObject.layer = originalLayer;
        
        // 触发事件
        OnDragEnd?.Invoke(this);
        
        Debug.Log($"结束拖拽: {gameObject.name}, 有效放置: {validDrop}");
    }
    
    /// <summary>
    /// 指针按下
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        // 可以在这里添加按下效果
    }
    
    /// <summary>
    /// 指针抬起
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        // 可以在这里添加抬起效果
    }
    
    /// <summary>
    /// 应用拖拽视觉效果
    /// </summary>
    private void ApplyDragVisualEffects(bool isDragging)
    {
        if (scaleDuringDrag)
        {
            float targetScale = isDragging ? dragScale : 1f;
            transform.localScale = originalScale * targetScale;
        }
        
        if (fadeOtherObjects && canvasGroup != null)
        {
            canvasGroup.alpha = isDragging ? fadeAlpha : 1f;
        }
        
        // 设置raycast目标
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = !isDragging;
        }
    }
    
    /// <summary>
    /// 获取放置目标
    /// </summary>
    private GameObject GetDropTarget(PointerEventData eventData)
    {
        // 使用EventSystem进行射线检测
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject != gameObject && result.gameObject.CompareTag("BoardSlot"))
            {
                return result.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 限制位置在屏幕内
    /// </summary>
    private Vector3 ConstrainToScreen(Vector3 position)
    {
        if (Camera.main == null) return position;
        
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        
        position.x = Mathf.Clamp(position.x, -screenBounds.x, screenBounds.x);
        position.y = Mathf.Clamp(position.y, -screenBounds.y, screenBounds.y);
        
        return position;
    }
    
    /// <summary>
    /// 处理成功放置
    /// </summary>
    private void HandleSuccessfulDrop()
    {
        // 可以添加成功放置的特效
        // 比如粒子效果、音效等
        
        // 隐藏或销毁拖拽对象
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 处理无效放置
    /// </summary>
    private void HandleInvalidDrop()
    {
        if (returnToOriginalPosition)
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
        else
        {
            transform.position = originalPosition;
        }
    }
    
    /// <summary>
    /// 返回原位置的协程
    /// </summary>
    private IEnumerator ReturnToOriginalPosition()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        float duration = Vector3.Distance(startPosition, originalPosition) / returnSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            transform.position = Vector3.Lerp(startPosition, originalPosition, progress);
            
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// 强制返回原位置
    /// </summary>
    public void ForceReturnToOriginalPosition()
    {
        if (isDragging)
        {
            isDragging = false;
            ApplyDragVisualEffects(false);
        }
        
        StopAllCoroutines();
        
        if (returnToOriginalPosition)
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
        else
        {
            transform.position = originalPosition;
        }
    }
    
    /// <summary>
    /// 设置原始位置
    /// </summary>
    public void SetOriginalPosition(Vector3 position)
    {
        originalPosition = position;
    }
    
    /// <summary>
    /// 重置到原始状态
    /// </summary>
    public void ResetToOriginal()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        gameObject.layer = originalLayer;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        isDragging = false;
    }
    
    /// <summary>
    /// 启用/禁用拖拽
    /// </summary>
    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
        
        if (!draggable && isDragging)
        {
            ForceReturnToOriginalPosition();
        }
    }
}