using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 数字博弈游戏玩家类
/// </summary>
public class NumberPlayer : MonoBehaviour
{
    [Header("玩家信息")]
    [SerializeField] private string playerName = "Player";
    [SerializeField] private NumberCard.PlayerTeam team;
    [SerializeField] private bool isAI = false;
    
    [Header("游戏数据")]
    [SerializeField] private int currentScore = 0;      // 当前积分
    [SerializeField] private int roundsWon = 0;         // 赢得的回合数
    [SerializeField] private int roundsLost = 0;        // 失败的回合数
    [SerializeField] private int roundsTied = 0;        // 平局的回合数
    
    // 卡牌数据
    private List<NumberCard> handCards = new List<NumberCard>();           // 手牌(1-9)
    private List<ModifierCard> modifierCards = new List<ModifierCard>();   // 加减牌
    private List<NumberCard> destroyedCards = new List<NumberCard>();      // 已销毁的卡牌
    
    // 当前回合状态
    private NumberCard selectedCardForPlay = null;     // 选择出牌的卡牌
    private bool hasPlacedModifiersThisRound = false;  // 本回合是否已放置加减牌
    
    // 胜利记录（用于计算胜利点数和）
    private List<int> winningCardValues = new List<int>(); // 记录每次胜利时出牌的数值
    
    // 事件
    public System.Action<NumberPlayer> OnScoreChanged;
    public System.Action<NumberPlayer, NumberCard> OnCardPlayed;
    public System.Action<NumberPlayer, ModifierCard> OnModifierPlaced;
    public System.Action<NumberPlayer, NumberCard> OnCardRevealed;
    
    // 属性
    public string PlayerName => playerName;
    public NumberCard.PlayerTeam Team => team;
    public bool IsAI => isAI;
    public int CurrentScore => currentScore;
    public int RoundsWon => roundsWon;
    public int RoundsLost => roundsLost;
    public int RoundsTied => roundsTied;
    public int TotalRounds => roundsWon + roundsLost + roundsTied;
    public List<NumberCard> HandCards => new List<NumberCard>(handCards);
    public List<NumberCard> AvailableCards => handCards.Where(card => card.CanPlay()).ToList();
    public List<NumberCard> DestroyedCards => new List<NumberCard>(destroyedCards);
    public bool HasPlacedModifiersThisRound => hasPlacedModifiersThisRound;
    
    private void Start()
    {
        InitializePlayer();
    }
    
    /// <summary>
    /// 初始化玩家
    /// </summary>
    public void InitializePlayer()
    {
        CreateInitialHandCards();
        currentScore = 0;
        roundsWon = 0;
        roundsLost = 0;
        roundsTied = 0;
        
        Debug.Log($"{playerName} ({team}队) 初始化完成");
    }
    
    /// <summary>
    /// 创建初始手牌(1-9)
    /// </summary>
    private void CreateInitialHandCards()
    {
        handCards.Clear();
        string[] cardIds = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        
        for (int i = 0; i < 9; i++)
        {
            int cardValue = i + 1; // 1-9
            NumberCard card = new NumberCard(cardValue, cardIds[i], team);
            handCards.Add(card);
        }
        
        // 打乱手牌顺序（摆牌阶段的准备）
        ShuffleHandCards();
    }
    
    /// <summary>
    /// 打乱手牌顺序
    /// </summary>
    public void ShuffleHandCards()
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            NumberCard temp = handCards[i];
            int randomIndex = Random.Range(i, handCards.Count);
            handCards[i] = handCards[randomIndex];
            handCards[randomIndex] = temp;
        }
        
        // 重新分配卡牌ID - 修复：保持卡牌状态
        string[] cardIds = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        for (int i = 0; i < handCards.Count; i++)
        {
            NumberCard oldCard = handCards[i];
            // 创建新卡牌并保持当前状态
            NumberCard newCard = new NumberCard(oldCard.OriginalValue, cardIds[i], team);
            
            // 保持当前数值（包括修饰符效果）
            if (oldCard.CurrentValue != oldCard.OriginalValue)
            {
                int difference = oldCard.CurrentValue - oldCard.OriginalValue;
                if (difference > 0)
                {
                    for (int j = 0; j < difference; j++)
                    {
                        newCard.ApplyAddition(1);
                    }
                }
                else if (difference < 0)
                {
                    for (int j = 0; j < -difference; j++)
                    {
                        newCard.ApplySubtraction(1);
                    }
                }
            }
            
            // 保持揭开状态
            if (oldCard.IsRevealed)
            {
                newCard.Reveal();
            }
            
            handCards[i] = newCard;
        }
        
        Debug.Log($"{playerName} 手牌已打乱");
    }
    
    /// <summary>
    /// 放置加减牌
    /// </summary>
    /// <param name="addTargetId">加法目标卡牌ID</param>
    /// <param name="subtractTargetId">减法目标卡牌ID</param>
    /// <param name="currentRound">当前回合数</param>
    /// <returns>是否成功放置</returns>
    public bool PlaceModifierCards(string addTargetId, string subtractTargetId, int currentRound)
    {
        if (hasPlacedModifiersThisRound)
        {
            Debug.LogWarning($"{playerName} 本回合已放置过加减牌");
            return false;
        }
        
        // 创建加法修饰符
        ModifierCard addModifier = new ModifierCard(ModifierCard.ModifierType.Add, addTargetId, team, currentRound);
        
        // 创建减法修饰符
        ModifierCard subtractModifier = new ModifierCard(ModifierCard.ModifierType.Subtract, subtractTargetId, team, currentRound);
        
        // 应用修饰符到对应的手牌
        NumberCard addTarget = GetHandCardById(addTargetId);
        NumberCard subtractTarget = GetHandCardById(subtractTargetId);
        
        if (addTarget != null && subtractTarget != null)
        {
            addModifier.ApplyToCard(addTarget);
            subtractModifier.ApplyToCard(subtractTarget);
            
            modifierCards.Add(addModifier);
            modifierCards.Add(subtractModifier);
            
            hasPlacedModifiersThisRound = true;
            
            OnModifierPlaced?.Invoke(this, addModifier);
            OnModifierPlaced?.Invoke(this, subtractModifier);
            
            Debug.Log($"{playerName} 放置加减牌：+1→{addTargetId}, -1→{subtractTargetId}");
            return true;
        }
        
        Debug.LogError($"{playerName} 无法找到目标卡牌：{addTargetId} 或 {subtractTargetId}");
        return false;
    }
    
    /// <summary>
    /// 根据ID获取手牌
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <returns>对应的手牌，如果不存在则返回null</returns>
    public NumberCard GetHandCardById(string cardId)
    {
        return handCards.FirstOrDefault(card => card.CardId == cardId);
    }
    
    /// <summary>
    /// 揭开对方的一张卡牌
    /// </summary>
    /// <param name="targetCard">目标卡牌</param>
    /// <returns>是否成功揭开</returns>
    public bool RevealOpponentCard(NumberCard targetCard)
    {
        if (targetCard != null && !targetCard.IsRevealed && targetCard.Owner != team)
        {
            targetCard.Reveal();
            OnCardRevealed?.Invoke(this, targetCard);
            
            Debug.Log($"{playerName} 揭开了对方的卡牌 {targetCard.CardId}({targetCard.CurrentValue})");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 选择一张卡牌出牌
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <returns>是否成功选择</returns>
    public bool SelectCardForPlay(string cardId)
    {
        NumberCard card = GetHandCardById(cardId);
        
        if (card != null && card.CanPlay())
        {
            selectedCardForPlay = card;
            card.Play();
            
            OnCardPlayed?.Invoke(this, card);
            
            Debug.Log($"{playerName} 选择出牌：{card.CardId}({card.CurrentValue})");
            return true;
        }
        
        Debug.LogWarning($"{playerName} 无法出牌：{cardId}");
        return false;
    }
    
    /// <summary>
    /// 获取当前选择的出牌
    /// </summary>
    /// <returns>选择的卡牌</returns>
    public NumberCard GetSelectedCard()
    {
        return selectedCardForPlay;
    }
    
    /// <summary>
    /// 比牌并处理结果
    /// </summary>
    /// <param name="myCard">自己的卡牌</param>
    /// <param name="opponentCard">对手的卡牌</param>
    /// <returns>比牌结果：1=胜利，0=平局，-1=失败</returns>
    public int BattleCards(NumberCard myCard, NumberCard opponentCard)
    {
        if (myCard == null || opponentCard == null)
        {
            Debug.LogError("比牌失败：卡牌为空");
            return 0;
        }
        
        int result = myCard.CompareTo(opponentCard);
        
        // 处理比牌结果
        switch (result)
        {
            case 1: // 胜利
                roundsWon++;
                currentScore++;
                winningCardValues.Add(myCard.CurrentValue); // 记录胜利时的卡牌数值
                Debug.Log($"{playerName} 胜利！{myCard}({myCard.CurrentValue}) > {opponentCard}({opponentCard.CurrentValue})");
                
                // 销毁对手卡牌
                opponentCard.Destroy();
                break;
                
            case -1: // 失败
                roundsLost++;
                Debug.Log($"{playerName} 失败！{myCard}({myCard.CurrentValue}) < {opponentCard}({opponentCard.CurrentValue})");
                
                // 销毁自己的卡牌
                myCard.Destroy();
                destroyedCards.Add(myCard);
                break;
                
            case 0: // 平局
                roundsTied++;
                Debug.Log($"{playerName} 平局！{myCard}({myCard.CurrentValue}) = {opponentCard}({opponentCard.CurrentValue})");
                
                // 双方卡牌都销毁
                myCard.Destroy();
                opponentCard.Destroy();
                destroyedCards.Add(myCard);
                break;
        }
        
        OnScoreChanged?.Invoke(this);
        return result;
    }
    
    /// <summary>
    /// 回收本回合的加减牌
    /// </summary>
    public void RecycleModifierCards()
    {
        foreach (ModifierCard modifier in modifierCards)
        {
            modifier.Recycle();
        }
        
        hasPlacedModifiersThisRound = false;
        
        Debug.Log($"{playerName} 回收了所有加减牌");
    }
    
    /// <summary>
    /// 重置回合状态
    /// </summary>
    public void ResetRoundState()
    {
        selectedCardForPlay = null;
        hasPlacedModifiersThisRound = false;
    }
    
    /// <summary>
    /// 检查是否达到胜利条件
    /// </summary>
    /// <param name="winCondition">胜利条件分数</param>
    /// <returns>是否获胜</returns>
    public bool CheckWinCondition(int winCondition)
    {
        return currentScore >= winCondition;
    }
    
    /// <summary>
    /// 计算最终分数（所有剩余卡牌的数值总和）
    /// </summary>
    /// <returns>最终分数</returns>
    public int CalculateFinalScore()
    {
        int finalScore = 0;
        
        foreach (NumberCard card in handCards)
        {
            if (!card.IsDestroyed && !card.IsPlayed)
            {
                finalScore += card.CurrentValue;
            }
        }
        
        Debug.Log($"{playerName} 最终分数：{finalScore}");
        return finalScore;
    }
    
    /// <summary>
    /// 计算胜利点数和（所有胜利卡牌的数值总和）
    /// 根据游戏规则：点数和越小的胜利
    /// </summary>
    /// <returns>胜利点数和</returns>
    public int CalculateWinPointsSum()
    {
        int winPointsSum = 0;
        
        foreach (int cardValue in winningCardValues)
        {
            winPointsSum += cardValue;
        }
        
        Debug.Log($"{playerName} 胜利点数和：{winPointsSum} (胜利{roundsWon}局)");
        return winPointsSum;
    }
    
    /// <summary>
    /// 获取游戏统计信息
    /// </summary>
    /// <returns>统计信息字符串</returns>
    public string GetGameStats()
    {
        return $"{playerName} ({team}队) - 积分: {currentScore}, 胜: {roundsWon}, 负: {roundsLost}, 平: {roundsTied}";
    }
    
    /// <summary>
    /// 重置玩家数据（新游戏）
    /// </summary>
    public void ResetForNewGame()
    {
        currentScore = 0;
        roundsWon = 0;
        roundsLost = 0;
        roundsTied = 0;
        modifierCards.Clear();
        destroyedCards.Clear();
        winningCardValues.Clear(); // 清理胜利记录
        selectedCardForPlay = null;
        hasPlacedModifiersThisRound = false;
        
        CreateInitialHandCards();
        
        Debug.Log($"{playerName} 重置完成，准备新游戏");
    }
    
    /// <summary>
    /// AI决策：选择放置加减牌的目标
    /// </summary>
    /// <returns>加法和减法目标的元组</returns>
    public (string addTarget, string subtractTarget) AIDecideModifierTargets()
    {
        if (!isAI) return (null, null);
        
        List<NumberCard> availableCards = AvailableCards;
        
        // 修复：检查是否有可用卡牌
        if (availableCards.Count == 0)
        {
            Debug.LogWarning($"{playerName} 没有可用卡牌进行AI决策");
            return (null, null);
        }
        
        if (availableCards.Count == 1)
        {
            // 只有一张卡牌时，加减都选择同一张
            return (availableCards[0].CardId, availableCards[0].CardId);
        }
        
        // 简单AI策略：
        // 1. 给最小的卡牌加分
        // 2. 给最大的卡牌减分（如果不会变成0）
        
        NumberCard smallestCard = availableCards.OrderBy(card => card.CurrentValue).First();
        NumberCard largestCard = availableCards.OrderByDescending(card => card.CurrentValue).First();
        
        string addTarget = smallestCard.CardId;
        string subtractTarget = largestCard.CurrentValue > 1 ? largestCard.CardId : smallestCard.CardId;
        
        return (addTarget, subtractTarget);
    }
    
    /// <summary>
    /// AI决策：选择要出的卡牌
    /// </summary>
    /// <returns>选择的卡牌ID</returns>
    public string AIDecideCardToPlay()
    {
        if (!isAI) return null;
        
        List<NumberCard> availableCards = AvailableCards;
        
        // 修复：添加空检查和详细日志
        if (availableCards == null || availableCards.Count == 0)
        {
            Debug.LogWarning($"{playerName} AI无可用卡牌出牌");
            return null;
        }
        
        // 简单AI策略：选择数值最大的卡牌
        NumberCard bestCard = availableCards.OrderByDescending(card => card.CurrentValue).First();
        
        return bestCard.CardId;
    }
    
    /// <summary>
    /// AI决策：选择要揭开的对方卡牌
    /// </summary>
    /// <param name="opponentCards">对方的卡牌列表</param>
    /// <returns>要揭开的卡牌</returns>
    public NumberCard AIDecideCardToReveal(List<NumberCard> opponentCards)
    {
        if (!isAI) return null;
        
        List<NumberCard> unrevealed = opponentCards.Where(card => !card.IsRevealed && card.CanPlay()).ToList();
        
        if (unrevealed.Count == 0) return null;
        
        // 随机选择一张未揭开的卡牌
        return unrevealed[Random.Range(0, unrevealed.Count)];
    }
}