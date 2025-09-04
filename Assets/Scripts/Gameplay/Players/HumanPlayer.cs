using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 人类玩家类，继承Player，处理玩家输入
/// </summary>
public class HumanPlayer : Player
{
    [Header("人类玩家设置")]
    [SerializeField] private bool enableDragAndDrop = true;
    [SerializeField] private float inputTimeout = 30f; // 输入超时时间
    
    // 当前选中的卡牌
    private CardData selectedCard = null;
    private int selectedCardIndex = -1;
    
    // 输入状态
    private bool isWaitingForInput = false;
    private float inputTimer = 0f;
    
    // 事件
    public System.Action<CardData> OnCardSelected;
    public System.Action OnCardDeselected;
    public System.Action OnInputTimeout;
    
    // 属性
    public CardData SelectedCard => selectedCard;
    public bool IsWaitingForInput => isWaitingForInput;
    public bool EnableDragAndDrop => enableDragAndDrop;
    
    protected override void Start()
    {
        base.Start();
        playerName = "Human Player";
    }
    
    private void Update()
    {
        if (isWaitingForInput)
        {
            HandleInputTimer();
            HandleInput();
        }
    }
    
    /// <summary>
    /// 处理输入计时器
    /// </summary>
    private void HandleInputTimer()
    {
        inputTimer += Time.deltaTime;
        
        if (inputTimer >= inputTimeout)
        {
            OnInputTimeout?.Invoke();
            EndTurn();
        }
    }
    
    /// <summary>
    /// 处理玩家输入
    /// </summary>
    private void HandleInput()
    {
        // 处理鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        
        // 处理键盘输入
        HandleKeyboardInput();
    }
    
    /// <summary>
    /// 处理鼠标点击
    /// </summary>
    private void HandleMouseClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        
        if (hit.collider != null)
        {
            // 点击卡牌
            if (hit.collider.CompareTag("Card"))
            {
                HandleCardClick(hit.collider.gameObject);
            }
            // 点击棋盘格
            else if (hit.collider.CompareTag("BoardSlot"))
            {
                BoardSlot slot = hit.collider.GetComponent<BoardSlot>();
                HandleSlotClick(slot);
            }
        }
        else
        {
            // 点击空白处，取消选择
            DeselectCard();
        }
    }
    
    /// <summary>
    /// 处理卡牌点击
    /// </summary>
    /// <param name="cardObject">卡牌游戏对象</param>
    private void HandleCardClick(GameObject cardObject)
    {
        // 这里需要从卡牌对象获取CardData
        // 假设卡牌对象有一个Card组件
        Card cardComponent = cardObject.GetComponent<Card>();
        if (cardComponent != null)
        {
            SelectCard(cardComponent.CardData);
        }
    }
    
    /// <summary>
    /// 处理棋盘格点击
    /// </summary>
    /// <param name="slot">棋盘格</param>
    private void HandleSlotClick(BoardSlot slot)
    {
        if (selectedCard != null && slot != null)
        {
            // 尝试将选中的卡牌放置到点击的格子
            if (PlayCard(selectedCard, slot))
            {
                DeselectCard();
            }
        }
    }
    
    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        // 数字键选择卡牌
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectCardByIndex(i - 1);
                break;
            }
        }
        
        // ESC键取消选择
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectCard();
        }
        
        // 空格键结束回合
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
        
        // Tab键查看手牌
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowHandCards();
        }
    }
    
    /// <summary>
    /// 选择卡牌
    /// </summary>
    /// <param name="card">要选择的卡牌</param>
    public void SelectCard(CardData card)
    {
        if (card == null || !hand.Contains(card))
        {
            Debug.Log("无效的卡牌选择");
            return;
        }
        
        selectedCard = card;
        selectedCardIndex = hand.IndexOf(card);
        
        OnCardSelected?.Invoke(selectedCard);
        
        Debug.Log($"选择了卡牌: {selectedCard.CardName}");
    }
    
    /// <summary>
    /// 通过索引选择卡牌
    /// </summary>
    /// <param name="index">卡牌索引</param>
    public void SelectCardByIndex(int index)
    {
        if (index >= 0 && index < hand.Count)
        {
            SelectCard(hand[index]);
        }
    }
    
    /// <summary>
    /// 取消选择卡牌
    /// </summary>
    public void DeselectCard()
    {
        if (selectedCard != null)
        {
            selectedCard = null;
            selectedCardIndex = -1;
            
            OnCardDeselected?.Invoke();
            
            Debug.Log("取消卡牌选择");
        }
    }
    
    /// <summary>
    /// 显示手牌信息
    /// </summary>
    private void ShowHandCards()
    {
        Debug.Log($"=== {playerName} 的手牌 ===");
        for (int i = 0; i < hand.Count; i++)
        {
            CardData card = hand[i];
            string canPlay = card.CanPlay(currentResources) ? "[可用]" : "[不可用]";
            Debug.Log($"{i + 1}. {card.CardName} - 费用: {card.Cost} {canPlay}");
        }
        Debug.Log($"当前资源: {currentResources}/{maxResources}");
    }
    
    /// <summary>
    /// 实现抽象方法：执行回合动作
    /// 人类玩家需要等待玩家输入
    /// </summary>
    public override void MakeMove()
    {
        isWaitingForInput = true;
        inputTimer = 0f;
        
        Debug.Log($"{playerName}: 开始回合，等待玩家操作...");
        Debug.Log("操作提示：点击卡牌选择，点击格子放置，空格键结束回合，Tab键查看手牌");
        
        // 显示可用卡牌
        List<CardData> playableCards = GetPlayableCards();
        if (playableCards.Count > 0)
        {
            Debug.Log($"可用卡牌数量: {playableCards.Count}");
        }
        else
        {
            Debug.Log("没有可用的卡牌，考虑结束回合");
        }
    }
    
    /// <summary>
    /// 结束回合
    /// </summary>
    public void EndTurn()
    {
        isWaitingForInput = false;
        inputTimer = 0f;
        DeselectCard();
        
        Debug.Log($"{playerName}: 结束回合");
        
        // 通知游戏管理器结束回合
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCurrentTurn();
        }
    }
    
    /// <summary>
    /// 重写打出卡牌方法，添加人类玩家特定逻辑
    /// </summary>
    public override bool PlayCard(int cardIndex, BoardSlot targetSlot)
    {
        bool success = base.PlayCard(cardIndex, targetSlot);
        
        if (success)
        {
            // 重置输入计时器
            inputTimer = 0f;
            
            // 如果没有可用卡牌了，提示结束回合
            List<CardData> playableCards = GetPlayableCards();
            if (playableCards.Count == 0)
            {
                Debug.Log("没有更多可用卡牌，考虑结束回合");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 回合开始时的处理
    /// </summary>
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        
        // 更新UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHandUI(this);
        }
    }
    
    /// <summary>
    /// 回合结束时的处理
    /// </summary>
    public override void OnTurnEnd()
    {
        base.OnTurnEnd();
        
        isWaitingForInput = false;
        DeselectCard();
    }
    
    /// <summary>
    /// 设置拖拽功能开关
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetDragAndDropEnabled(bool enabled)
    {
        enableDragAndDrop = enabled;
    }
    
    /// <summary>
    /// 设置输入超时时间
    /// </summary>
    /// <param name="timeout">超时时间（秒）</param>
    public void SetInputTimeout(float timeout)
    {
        inputTimeout = Mathf.Max(0f, timeout);
    }
}