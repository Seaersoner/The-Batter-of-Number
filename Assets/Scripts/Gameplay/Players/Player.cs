using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家基类，存储手牌、胜负数据等
/// </summary>
public abstract class Player : MonoBehaviour
{
    [Header("玩家信息")]
    [SerializeField] protected string playerName = "Player";
    [SerializeField] protected int playerId = 0;
    
    [Header("游戏数据")]
    [SerializeField] protected int maxHandSize = 5;
    [SerializeField] protected int currentResources = 3;
    [SerializeField] protected int maxResources = 10;
    
    [Header("统计数据")]
    [SerializeField] protected int wins = 0;
    [SerializeField] protected int losses = 0;
    [SerializeField] protected int gamesPlayed = 0;
    
    // 手牌
    protected List<CardData> hand = new List<CardData>();
    
    // 卡牌库（可用的卡牌）
    [SerializeField] protected List<CardData> cardDeck = new List<CardData>();
    
    // 事件
    public System.Action<CardData> OnCardPlayed;
    public System.Action<CardData> OnCardDrawn;
    public System.Action<List<CardData>> OnHandChanged;
    public System.Action<int> OnResourcesChanged;
    
    // 属性
    public string PlayerName => playerName;
    public int PlayerId => playerId;
    public List<CardData> Hand => new List<CardData>(hand); // 返回副本以防外部修改
    public int HandCount => hand.Count;
    public int MaxHandSize => maxHandSize;
    public int CurrentResources => currentResources;
    public int MaxResources => maxResources;
    public int Wins => wins;
    public int Losses => losses;
    public int GamesPlayed => gamesPlayed;
    public float WinRate => gamesPlayed > 0 ? (float)wins / gamesPlayed : 0f;
    
    protected virtual void Start()
    {
        InitializePlayer();
    }
    
    /// <summary>
    /// 初始化玩家
    /// </summary>
    protected virtual void InitializePlayer()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Player {playerId}";
        }
        
        // 如果没有设置卡牌库，创建默认卡牌库
        if (cardDeck.Count == 0)
        {
            CreateDefaultDeck();
        }
    }
    
    /// <summary>
    /// 创建默认卡牌库（子类可以重写）
    /// </summary>
    protected virtual void CreateDefaultDeck()
    {
        // 这里可以加载默认卡牌或者从资源中读取
        Debug.Log($"{playerName}: 创建默认卡牌库");
    }
    
    /// <summary>
    /// 初始化手牌
    /// </summary>
    public virtual void InitializeHand()
    {
        hand.Clear();
        currentResources = 3; // 重置资源
        
        // 抽取初始手牌
        for (int i = 0; i < maxHandSize && i < cardDeck.Count; i++)
        {
            DrawCard();
        }
        
        OnHandChanged?.Invoke(Hand);
        OnResourcesChanged?.Invoke(currentResources);
    }
    
    /// <summary>
    /// 抽取卡牌
    /// </summary>
    /// <returns>是否成功抽取</returns>
    public virtual bool DrawCard()
    {
        if (hand.Count >= maxHandSize)
        {
            Debug.Log($"{playerName}: 手牌已满，无法抽取卡牌");
            return false;
        }
        
        if (cardDeck.Count == 0)
        {
            Debug.Log($"{playerName}: 卡牌库为空，无法抽取卡牌");
            return false;
        }
        
        // 随机抽取一张卡牌
        int randomIndex = Random.Range(0, cardDeck.Count);
        CardData drawnCard = cardDeck[randomIndex].CreateCopy();
        
        hand.Add(drawnCard);
        
        OnCardDrawn?.Invoke(drawnCard);
        OnHandChanged?.Invoke(Hand);
        
        Debug.Log($"{playerName}: 抽取了卡牌 {drawnCard.CardName}");
        
        // 播放抽卡音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardDrawSound();
        }
        
        return true;
    }
    
    /// <summary>
    /// 打出卡牌
    /// </summary>
    /// <param name="cardIndex">手牌中的卡牌索引</param>
    /// <param name="targetSlot">目标棋盘格</param>
    /// <returns>是否成功打出</returns>
    public virtual bool PlayCard(int cardIndex, BoardSlot targetSlot)
    {
        if (cardIndex < 0 || cardIndex >= hand.Count)
        {
            Debug.Log($"{playerName}: 无效的卡牌索引 {cardIndex}");
            return false;
        }
        
        CardData cardToPlay = hand[cardIndex];
        
        // 检查资源是否足够
        if (!cardToPlay.CanPlay(currentResources))
        {
            Debug.Log($"{playerName}: 资源不足，无法打出 {cardToPlay.CardName} (需要 {cardToPlay.Cost}，拥有 {currentResources})");
            return false;
        }
        
        // 检查目标格子是否可用
        if (targetSlot == null || !targetSlot.IsEmpty())
        {
            Debug.Log($"{playerName}: 目标格子不可用");
            return false;
        }
        
        // 消耗资源
        currentResources -= cardToPlay.Cost;
        
        // 从手牌中移除
        hand.RemoveAt(cardIndex);
        
        // 将卡牌放置到目标格子
        targetSlot.PlaceCard(cardToPlay, this);
        
        OnCardPlayed?.Invoke(cardToPlay);
        OnHandChanged?.Invoke(Hand);
        OnResourcesChanged?.Invoke(currentResources);
        
        Debug.Log($"{playerName}: 打出了卡牌 {cardToPlay.CardName}");
        
        // 播放放置音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardPlaceSound();
        }
        
        return true;
    }
    
    /// <summary>
    /// 打出卡牌（通过卡牌引用）
    /// </summary>
    /// <param name="card">要打出的卡牌</param>
    /// <param name="targetSlot">目标棋盘格</param>
    /// <returns>是否成功打出</returns>
    public virtual bool PlayCard(CardData card, BoardSlot targetSlot)
    {
        int cardIndex = hand.IndexOf(card);
        return PlayCard(cardIndex, targetSlot);
    }
    
    /// <summary>
    /// 获取可以打出的卡牌列表
    /// </summary>
    /// <returns>可打出的卡牌列表</returns>
    public virtual List<CardData> GetPlayableCards()
    {
        List<CardData> playableCards = new List<CardData>();
        
        foreach (CardData card in hand)
        {
            if (card.CanPlay(currentResources))
            {
                playableCards.Add(card);
            }
        }
        
        return playableCards;
    }
    
    /// <summary>
    /// 增加资源
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public virtual void AddResources(int amount)
    {
        currentResources = Mathf.Clamp(currentResources + amount, 0, maxResources);
        OnResourcesChanged?.Invoke(currentResources);
    }
    
    /// <summary>
    /// 回合开始时调用
    /// </summary>
    public virtual void OnTurnStart()
    {
        // 增加资源
        AddResources(1);
        
        // 抽取一张卡牌
        DrawCard();
        
        Debug.Log($"{playerName}: 回合开始，当前资源: {currentResources}，手牌数量: {hand.Count}");
    }
    
    /// <summary>
    /// 回合结束时调用
    /// </summary>
    public virtual void OnTurnEnd()
    {
        Debug.Log($"{playerName}: 回合结束");
    }
    
    /// <summary>
    /// 记录游戏结果
    /// </summary>
    /// <param name="won">是否获胜</param>
    public virtual void RecordGameResult(bool won)
    {
        gamesPlayed++;
        
        if (won)
        {
            wins++;
            Debug.Log($"{playerName}: 获得胜利！总胜场: {wins}");
        }
        else
        {
            losses++;
            Debug.Log($"{playerName}: 遭遇失败！总败场: {losses}");
        }
        
        Debug.Log($"{playerName}: 胜率: {WinRate:P1}");
    }
    
    /// <summary>
    /// 抽象方法：执行回合动作
    /// 子类必须实现具体的行动逻辑
    /// </summary>
    public abstract void MakeMove();
    
    /// <summary>
    /// 获取玩家信息字符串
    /// </summary>
    /// <returns>玩家信息</returns>
    public override string ToString()
    {
        return $"{playerName} (ID: {playerId}) - 手牌: {hand.Count}/{maxHandSize}, 资源: {currentResources}/{maxResources}, 胜率: {WinRate:P1}";
    }
}