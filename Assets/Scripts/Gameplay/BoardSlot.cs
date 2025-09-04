using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 棋盘格逻辑，挂载在每个格子上，处理放置逻辑
/// </summary>
public class BoardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("格子设置")]
    [SerializeField] private int slotId = 0;
    [SerializeField] private Vector2Int gridPosition = Vector2Int.zero;
    [SerializeField] private SlotType slotType = SlotType.Normal;
    
    [Header("视觉效果")]
    [SerializeField] private SpriteRenderer slotRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color occupiedColor = Color.gray;
    [SerializeField] private Color invalidColor = Color.red;
    
    [Header("卡牌显示")]
    [SerializeField] private Transform cardTransform; // 卡牌放置的位置
    [SerializeField] private GameObject cardPrefab; // 卡牌预制体
    
    // 格子类型枚举
    public enum SlotType
    {
        Normal,     // 普通格子
        Special,    // 特殊格子
        Blocked,    // 阻挡格子
        Bonus       // 奖励格子
    }
    
    // 当前状态
    private CardData currentCard = null;
    private Player cardOwner = null;
    private GameObject cardObject = null;
    private bool isHighlighted = false;
    
    // 事件
    public System.Action<BoardSlot, CardData, Player> OnCardPlaced;
    public System.Action<BoardSlot, CardData, Player> OnCardRemoved;
    public System.Action<BoardSlot> OnSlotHighlighted;
    public System.Action<BoardSlot> OnSlotUnhighlighted;
    
    // 属性
    public int SlotId => slotId;
    public Vector2Int GridPosition => gridPosition;
    public SlotType Type => slotType;
    public CardData CurrentCard => currentCard;
    public Player CardOwner => cardOwner;
    public bool IsEmpty() => currentCard == null;
    public bool IsOccupied() => currentCard != null;
    public bool CanPlaceCard() => IsEmpty() && slotType != SlotType.Blocked;
    
    private void Start()
    {
        InitializeSlot();
    }
    
    /// <summary>
    /// 初始化格子
    /// </summary>
    private void InitializeSlot()
    {
        // 设置格子名称
        if (string.IsNullOrEmpty(gameObject.name) || gameObject.name.Contains("GameObject"))
        {
            gameObject.name = $"BoardSlot_{slotId}_{gridPosition.x}_{gridPosition.y}";
        }
        
        // 初始化渲染器
        if (slotRenderer == null)
        {
            slotRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 设置初始颜色
        UpdateSlotVisual();
        
        // 确保有碰撞器用于检测
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
        
        // 设置标签
        if (!gameObject.CompareTag("BoardSlot"))
        {
            gameObject.tag = "BoardSlot";
        }
    }
    
    /// <summary>
    /// 放置卡牌到格子
    /// </summary>
    /// <param name="card">要放置的卡牌</param>
    /// <param name="owner">卡牌拥有者</param>
    /// <returns>是否成功放置</returns>
    public bool PlaceCard(CardData card, Player owner)
    {
        if (!CanPlaceCard())
        {
            Debug.Log($"格子 {gameObject.name} 无法放置卡牌");
            return false;
        }
        
        if (card == null || owner == null)
        {
            Debug.Log("无效的卡牌或玩家");
            return false;
        }
        
        // 设置卡牌数据
        currentCard = card;
        cardOwner = owner;
        
        // 创建卡牌视觉对象
        CreateCardVisual();
        
        // 更新格子外观
        UpdateSlotVisual();
        
        // 触发事件
        OnCardPlaced?.Invoke(this, card, owner);
        
        Debug.Log($"卡牌 {card.CardName} 被 {owner.PlayerName} 放置到格子 {gameObject.name}");
        
        // 播放放置音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardPlaceSound();
        }
        
        return true;
    }
    
    /// <summary>
    /// 从格子移除卡牌
    /// </summary>
    /// <returns>被移除的卡牌</returns>
    public CardData RemoveCard()
    {
        if (IsEmpty())
        {
            return null;
        }
        
        CardData removedCard = currentCard;
        Player previousOwner = cardOwner;
        
        // 清除数据
        currentCard = null;
        cardOwner = null;
        
        // 销毁卡牌视觉对象
        DestroyCardVisual();
        
        // 更新格子外观
        UpdateSlotVisual();
        
        // 触发事件
        OnCardRemoved?.Invoke(this, removedCard, previousOwner);
        
        Debug.Log($"卡牌 {removedCard.CardName} 从格子 {gameObject.name} 被移除");
        
        return removedCard;
    }
    
    /// <summary>
    /// 清空格子
    /// </summary>
    public void ClearSlot()
    {
        RemoveCard();
    }
    
    /// <summary>
    /// 创建卡牌视觉对象
    /// </summary>
    private void CreateCardVisual()
    {
        if (currentCard == null) return;
        
        // 如果已有卡牌对象，先销毁
        DestroyCardVisual();
        
        // 创建新的卡牌对象
        if (cardPrefab != null)
        {
            Vector3 cardPosition = cardTransform != null ? cardTransform.position : transform.position;
            cardObject = Instantiate(cardPrefab, cardPosition, Quaternion.identity, transform);
            
            // 设置卡牌数据
            Card cardComponent = cardObject.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.SetCardData(currentCard);
                cardComponent.SetOwner(cardOwner);
            }
        }
        else
        {
            // 如果没有预制体，创建简单的视觉表示
            GameObject simpleCard = new GameObject($"Card_{currentCard.CardName}");
            simpleCard.transform.SetParent(transform);
            simpleCard.transform.localPosition = Vector3.zero;
            
            SpriteRenderer cardRenderer = simpleCard.AddComponent<SpriteRenderer>();
            cardRenderer.sprite = currentCard.CardSprite;
            cardRenderer.color = currentCard.CardColor;
            
            cardObject = simpleCard;
        }
    }
    
    /// <summary>
    /// 销毁卡牌视觉对象
    /// </summary>
    private void DestroyCardVisual()
    {
        if (cardObject != null)
        {
            DestroyImmediate(cardObject);
            cardObject = null;
        }
    }
    
    /// <summary>
    /// 更新格子视觉效果
    /// </summary>
    private void UpdateSlotVisual()
    {
        if (slotRenderer == null) return;
        
        Color targetColor = normalColor;
        
        switch (slotType)
        {
            case SlotType.Blocked:
                targetColor = invalidColor;
                break;
            case SlotType.Special:
                targetColor = Color.blue;
                break;
            case SlotType.Bonus:
                targetColor = Color.green;
                break;
            default:
                if (IsOccupied())
                {
                    targetColor = occupiedColor;
                }
                else if (isHighlighted)
                {
                    targetColor = highlightColor;
                }
                else
                {
                    targetColor = normalColor;
                }
                break;
        }
        
        slotRenderer.color = targetColor;
    }
    
    /// <summary>
    /// 高亮格子
    /// </summary>
    public void Highlight()
    {
        if (!isHighlighted)
        {
            isHighlighted = true;
            UpdateSlotVisual();
            OnSlotHighlighted?.Invoke(this);
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
            OnSlotUnhighlighted?.Invoke(this);
        }
    }
    
    /// <summary>
    /// 检查是否可以被指定玩家使用
    /// </summary>
    /// <param name="player">玩家</param>
    /// <returns>是否可以使用</returns>
    public bool CanBeUsedBy(Player player)
    {
        if (!CanPlaceCard()) return false;
        
        // 这里可以添加更多的规则检查
        // 比如某些格子只能被特定玩家使用等
        
        return true;
    }
    
    /// <summary>
    /// 获取相邻格子
    /// </summary>
    /// <returns>相邻格子列表</returns>
    public List<BoardSlot> GetAdjacentSlots()
    {
        List<BoardSlot> adjacentSlots = new List<BoardSlot>();
        
        if (GameManager.Instance != null && GameManager.Instance.BoardSlots != null)
        {
            foreach (BoardSlot slot in GameManager.Instance.BoardSlots)
            {
                if (slot != this && IsAdjacent(slot))
                {
                    adjacentSlots.Add(slot);
                }
            }
        }
        
        return adjacentSlots;
    }
    
    /// <summary>
    /// 检查是否与另一个格子相邻
    /// </summary>
    /// <param name="other">另一个格子</param>
    /// <returns>是否相邻</returns>
    private bool IsAdjacent(BoardSlot other)
    {
        int deltaX = Mathf.Abs(gridPosition.x - other.gridPosition.x);
        int deltaY = Mathf.Abs(gridPosition.y - other.gridPosition.y);
        
        return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
    }
    
    // Unity事件系统接口实现
    public void OnDrop(PointerEventData eventData)
    {
        // 处理拖拽放置
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            Draggable draggable = draggedObject.GetComponent<Draggable>();
            if (draggable != null && draggable.CardData != null)
            {
                // 尝试放置卡牌
                Player currentPlayer = GameManager.Instance?.HumanPlayer;
                if (currentPlayer != null)
                {
                    currentPlayer.PlayCard(draggable.CardData, this);
                }
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标进入时高亮
        if (CanPlaceCard())
        {
            Highlight();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标离开时取消高亮
        Unhighlight();
    }
    
    /// <summary>
    /// 获取格子信息字符串
    /// </summary>
    /// <returns>格子信息</returns>
    public override string ToString()
    {
        string cardInfo = IsOccupied() ? $"卡牌: {currentCard.CardName}" : "空";
        return $"格子 {slotId} ({gridPosition.x}, {gridPosition.y}) - {slotType} - {cardInfo}";
    }
}