using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 卡牌槽类，用于数字博弈游戏的卡牌放置
/// </summary>
public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("槽位信息")]
    [SerializeField] private string slotId = "A";                    // 槽位ID (A-I)
    [SerializeField] private int slotIndex = 0;                      // 槽位索引 (0-8)
    [SerializeField] private NumberCard.PlayerTeam ownerTeam;        // 所属队伍
    [SerializeField] private SlotType slotType = SlotType.HandCard;  // 槽位类型
    
    [Header("视觉效果")]
    [SerializeField] private SpriteRenderer slotRenderer;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.6f);
    [SerializeField] private Color occupiedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color teamColorRed = new Color(1f, 0.3f, 0.3f, 0.4f);
    [SerializeField] private Color teamColorBlue = new Color(0.3f, 0.3f, 1f, 0.4f);
    
    // 槽位类型枚举
    public enum SlotType
    {
        HandCard,    // 手牌槽
        Modifier,    // 加减牌槽
        Battle,      // 比牌槽
        Destroyed    // 销毁区槽
    }
    
    // 当前状态
    private NumberCard currentCard = null;
    private GameObject cardVisualObject = null;
    private bool isHighlighted = false;
    private bool isOccupied = false;
    
    // 事件
    public System.Action<CardSlot, NumberCard> OnCardPlaced;
    public System.Action<CardSlot, NumberCard> OnCardRemoved;
    public System.Action<CardSlot> OnSlotClicked;
    
    // 属性
    public string SlotId => slotId;
    public int SlotIndex => slotIndex;
    public NumberCard.PlayerTeam OwnerTeam => ownerTeam;
    public SlotType Type => slotType;
    public NumberCard CurrentCard => currentCard;
    public bool IsEmpty => currentCard == null;
    public bool IsOccupied => isOccupied;
    public bool CanAcceptCard => !isOccupied && IsValidForCurrentPhase();
    
    private void Start()
    {
        InitializeSlot();
    }
    
    /// <summary>
    /// 初始化槽位
    /// </summary>
    private void InitializeSlot()
    {
        // 获取或创建渲染器
        if (slotRenderer == null)
        {
            slotRenderer = GetComponent<SpriteRenderer>();
            if (slotRenderer == null)
            {
                slotRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // 设置槽位精灵
        if (ResourceManager.Instance != null)
        {
            string slotTypeName = slotType.ToString().ToLower();
            Sprite slotSprite = ResourceManager.Instance.GetSlotSprite(slotTypeName);
            if (slotSprite != null)
            {
                slotRenderer.sprite = slotSprite;
            }
        }
        
        // 确保有碰撞器
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(100f, 140f); // 卡牌尺寸
        }
        
        // 设置初始视觉效果
        UpdateSlotVisual();
        
        Debug.Log($"卡牌槽 {slotId} 初始化完成 - 类型: {slotType}, 队伍: {ownerTeam}");
    }
    
    /// <summary>
    /// 设置槽位信息
    /// </summary>
    /// <param name="id">槽位ID</param>
    /// <param name="index">槽位索引</param>
    /// <param name="team">所属队伍</param>
    public void SetSlotInfo(string id, int index, NumberCard.PlayerTeam team)
    {
        slotId = id;
        slotIndex = index;
        ownerTeam = team;
        
        gameObject.name = $"Slot_{slotType}_{id}";
        UpdateSlotVisual();
    }
    
    /// <summary>
    /// 设置槽位类型
    /// </summary>
    /// <param name="type">槽位类型</param>
    public void SetSlotType(SlotType type)
    {
        slotType = type;
        UpdateSlotVisual();
    }
    
    /// <summary>
    /// 放置卡牌
    /// </summary>
    /// <param name="card">要放置的卡牌</param>
    /// <returns>是否成功放置</returns>
    public bool PlaceCard(NumberCard card)
    {
        if (!CanAcceptCard || card == null)
        {
            Debug.LogWarning($"槽位 {slotId} 无法接受卡牌");
            return false;
        }
        
        currentCard = card;
        isOccupied = true;
        
        // 创建卡牌的视觉表示
        CreateCardVisual(card);
        
        // 更新槽位外观
        UpdateSlotVisual();
        
        // 触发事件
        OnCardPlaced?.Invoke(this, card);
        
        Debug.Log($"卡牌 {card.CardId}({card.CurrentValue}) 放置到槽位 {slotId}");
        return true;
    }
    
    /// <summary>
    /// 移除卡牌
    /// </summary>
    /// <returns>被移除的卡牌</returns>
    public NumberCard RemoveCard()
    {
        if (currentCard == null)
        {
            return null;
        }
        
        NumberCard removedCard = currentCard;
        currentCard = null;
        isOccupied = false;
        
        // 销毁视觉对象
        DestroyCardVisual();
        
        // 更新槽位外观
        UpdateSlotVisual();
        
        // 触发事件
        OnCardRemoved?.Invoke(this, removedCard);
        
        Debug.Log($"卡牌 {removedCard.CardId} 从槽位 {slotId} 移除");
        return removedCard;
    }
    
    /// <summary>
    /// 清空槽位
    /// </summary>
    public void ClearSlot()
    {
        RemoveCard();
    }
    
    /// <summary>
    /// 创建卡牌视觉表示
    /// </summary>
    /// <param name="card">卡牌数据</param>
    private void CreateCardVisual(NumberCard card)
    {
        // 销毁之前的视觉对象
        DestroyCardVisual();
        
        // 创建新的视觉对象
        cardVisualObject = new GameObject($"Card_{card.CardId}");
        cardVisualObject.transform.SetParent(transform);
        cardVisualObject.transform.localPosition = Vector3.zero;
        
        // 添加背景
        SpriteRenderer cardRenderer = cardVisualObject.AddComponent<SpriteRenderer>();
        
        // 尝试获取卡牌精灵
        if (ResourceManager.Instance != null)
        {
            Sprite cardSprite = ResourceManager.Instance.GetCardSprite(card.OriginalValue);
            if (cardSprite != null)
            {
                cardRenderer.sprite = cardSprite;
            }
        }
        
        cardRenderer.color = card.GetTeamColor();
        cardRenderer.sortingOrder = 1;
        
        // 添加卡牌信息文本
        CreateCardText(card);
        
        // 如果卡牌未揭开且属于对方，显示卡背
        if (!card.IsRevealed && card.Owner != ownerTeam && slotType == SlotType.HandCard)
        {
            ShowCardBack();
        }
    }
    
    /// <summary>
    /// 创建卡牌文本
    /// </summary>
    /// <param name="card">卡牌数据</param>
    private void CreateCardText(NumberCard card)
    {
        GameObject textObj = new GameObject("CardText");
        textObj.transform.SetParent(cardVisualObject.transform);
        textObj.transform.localPosition = Vector3.zero;
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = GetCardDisplayText(card);
        textMesh.fontSize = 24;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // 设置文本层级
        MeshRenderer textRenderer = textObj.GetComponent<MeshRenderer>();
        textRenderer.sortingOrder = 2;
    }
    
    /// <summary>
    /// 获取卡牌显示文本
    /// </summary>
    /// <param name="card">卡牌数据</param>
    /// <returns>显示文本</returns>
    private string GetCardDisplayText(NumberCard card)
    {
        if (!card.IsRevealed && card.Owner != ownerTeam && slotType == SlotType.HandCard)
        {
            return $"{card.CardId}\n?";
        }
        
        string text = $"{card.CardId}\n{card.CurrentValue}";
        
        // 如果当前值与原始值不同，显示原始值
        if (card.CurrentValue != card.OriginalValue)
        {
            text += $"\n({card.OriginalValue})";
        }
        
        return text;
    }
    
    /// <summary>
    /// 显示卡背
    /// </summary>
    private void ShowCardBack()
    {
        if (cardVisualObject != null)
        {
            SpriteRenderer cardRenderer = cardVisualObject.GetComponent<SpriteRenderer>();
            if (cardRenderer != null)
            {
                cardRenderer.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 深灰色卡背
            }
        }
    }
    
    /// <summary>
    /// 销毁卡牌视觉对象
    /// </summary>
    private void DestroyCardVisual()
    {
        if (cardVisualObject != null)
        {
            DestroyImmediate(cardVisualObject);
            cardVisualObject = null;
        }
    }
    
    /// <summary>
    /// 更新槽位视觉效果
    /// </summary>
    private void UpdateSlotVisual()
    {
        if (slotRenderer == null) return;
        
        Color targetColor;
        
        if (isOccupied)
        {
            targetColor = occupiedColor;
        }
        else if (isHighlighted)
        {
            targetColor = highlightColor;
        }
        else
        {
            // 根据队伍设置颜色
            switch (ownerTeam)
            {
                case NumberCard.PlayerTeam.Red:
                    targetColor = teamColorRed;
                    break;
                case NumberCard.PlayerTeam.Blue:
                    targetColor = teamColorBlue;
                    break;
                default:
                    targetColor = normalColor;
                    break;
            }
        }
        
        slotRenderer.color = targetColor;
    }
    
    /// <summary>
    /// 高亮槽位
    /// </summary>
    public void Highlight()
    {
        if (!isHighlighted)
        {
            isHighlighted = true;
            UpdateSlotVisual();
        }
    }
    
    /// <summary>
    /// 取消高亮
    /// </summary>
    public void Unhighlight()
    {
        if (isHighlighted)
        {
            isHighlighted = false;
            UpdateSlotVisual();
        }
    }
    
    /// <summary>
    /// 检查当前游戏阶段是否允许放置卡牌
    /// </summary>
    /// <returns>是否有效</returns>
    private bool IsValidForCurrentPhase()
    {
        if (GameManager.Instance == null)
            return true;
        
        GameManager.GamePhase currentPhase = GameManager.Instance.CurrentPhase;
        
        switch (slotType)
        {
            case SlotType.HandCard:
                return currentPhase == GameManager.GamePhase.CardPlacement;
            
            case SlotType.Modifier:
                return currentPhase == GameManager.GamePhase.ModifierPlacement;
            
            case SlotType.Battle:
                return currentPhase == GameManager.GamePhase.CardPlay;
            
            case SlotType.Destroyed:
                return true; // 销毁区总是可用
            
            default:
                return false;
        }
    }
    
    // Unity事件系统接口实现
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            // 处理拖拽放置逻辑
            Draggable draggable = draggedObject.GetComponent<Draggable>();
            if (draggable != null && draggable.CardData != null)
            {
                // 尝试将卡牌放置到此槽位
                // 这里需要与游戏逻辑集成
                Debug.Log($"尝试将卡牌拖拽到槽位 {slotId}");
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanAcceptCard)
        {
            Highlight();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Unhighlight();
    }
    
    /// <summary>
    /// 鼠标点击事件
    /// </summary>
    private void OnMouseDown()
    {
        OnSlotClicked?.Invoke(this);
        Debug.Log($"点击了槽位 {slotId}");
    }
    
    /// <summary>
    /// 更新卡牌显示（当卡牌被揭开时调用）
    /// </summary>
    public void UpdateCardDisplay()
    {
        if (currentCard != null && cardVisualObject != null)
        {
            // 重新创建卡牌视觉
            CreateCardVisual(currentCard);
        }
    }
    
    /// <summary>
    /// 获取槽位描述
    /// </summary>
    /// <returns>槽位描述</returns>
    public string GetSlotDescription()
    {
        string cardInfo = IsOccupied ? $"卡牌: {currentCard.CardId}({currentCard.CurrentValue})" : "空";
        return $"槽位 {slotId} ({slotType}, {ownerTeam}队) - {cardInfo}";
    }
    
    /// <summary>
    /// 重写ToString方法
    /// </summary>
    /// <returns>槽位信息</returns>
    public override string ToString()
    {
        return $"Slot_{slotId}({slotType})";
    }
}