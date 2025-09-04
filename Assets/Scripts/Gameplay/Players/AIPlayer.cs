using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// AI玩家类，继承Player，封装AI决策逻辑
/// </summary>
public class AIPlayer : Player
{
    [Header("AI设置")]
    [SerializeField] private AILevel aiLevel = AILevel.Normal;
    [SerializeField] private float thinkingTime = 2f; // AI思考时间
    [SerializeField] private bool showThinkingProcess = true; // 是否显示思考过程
    
    [Header("AI行为参数")]
    [Range(0f, 1f)]
    [SerializeField] private float aggressiveness = 0.5f; // 攻击性
    [Range(0f, 1f)]
    [SerializeField] private float defensiveness = 0.5f; // 防御性
    [Range(0f, 1f)]
    [SerializeField] private float randomness = 0.2f; // 随机性
    
    // AI难度等级
    public enum AILevel
    {
        Easy,       // 简单：随机选择
        Normal,     // 普通：基础策略
        Hard,       // 困难：高级策略
        Expert      // 专家：完美策略
    }
    
    // AI决策数据结构
    private struct AIDecision
    {
        public CardData card;
        public BoardSlot targetSlot;
        public float score;
        public string reasoning;
        
        public AIDecision(CardData card, BoardSlot slot, float score, string reasoning)
        {
            this.card = card;
            this.targetSlot = slot;
            this.score = score;
            this.reasoning = reasoning;
        }
    }
    
    // 属性
    public AILevel Level => aiLevel;
    public float Aggressiveness => aggressiveness;
    public float Defensiveness => defensiveness;
    
    protected override void Start()
    {
        base.Start();
        playerName = $"AI Player ({aiLevel})";
    }
    
    /// <summary>
    /// 实现抽象方法：AI执行回合动作
    /// </summary>
    public override void MakeMove()
    {
        StartCoroutine(ExecuteAITurn());
    }
    
    /// <summary>
    /// 执行AI回合的协程
    /// </summary>
    private IEnumerator ExecuteAITurn()
    {
        Debug.Log($"{playerName}: 开始思考...");
        
        // AI思考时间
        yield return new WaitForSeconds(thinkingTime);
        
        // 获取所有可能的决策
        List<AIDecision> possibleMoves = GetPossibleMoves();
        
        if (possibleMoves.Count == 0)
        {
            Debug.Log($"{playerName}: 没有可执行的动作，结束回合");
            EndAITurn();
            yield break;
        }
        
        // 根据AI等级选择最佳动作
        AIDecision bestMove = SelectBestMove(possibleMoves);
        
        if (showThinkingProcess)
        {
            Debug.Log($"{playerName}: 选择执行 - {bestMove.reasoning}");
        }
        
        // 执行选择的动作
        bool success = PlayCard(bestMove.card, bestMove.targetSlot);
        
        if (success)
        {
            Debug.Log($"{playerName}: 成功打出 {bestMove.card.CardName}");
            
            // 短暂延迟后检查是否继续行动
            yield return new WaitForSeconds(0.5f);
            
            // 如果还有资源和可用卡牌，继续思考
            if (GetPlayableCards().Count > 0 && GetAvailableSlots().Count > 0)
            {
                // 递归调用，但限制次数防止无限循环
                if (currentResources > 0)
                {
                    StartCoroutine(ExecuteAITurn());
                }
                else
                {
                    EndAITurn();
                }
            }
            else
            {
                EndAITurn();
            }
        }
        else
        {
            Debug.Log($"{playerName}: 执行动作失败，结束回合");
            EndAITurn();
        }
    }
    
    /// <summary>
    /// 获取所有可能的移动
    /// </summary>
    private List<AIDecision> GetPossibleMoves()
    {
        List<AIDecision> moves = new List<AIDecision>();
        List<CardData> playableCards = GetPlayableCards();
        List<BoardSlot> availableSlots = GetAvailableSlots();
        
        foreach (CardData card in playableCards)
        {
            foreach (BoardSlot slot in availableSlots)
            {
                float score = EvaluateMove(card, slot);
                string reasoning = GenerateReasoning(card, slot, score);
                moves.Add(new AIDecision(card, slot, score, reasoning));
            }
        }
        
        return moves;
    }
    
    /// <summary>
    /// 选择最佳移动
    /// </summary>
    private AIDecision SelectBestMove(List<AIDecision> possibleMoves)
    {
        switch (aiLevel)
        {
            case AILevel.Easy:
                return SelectRandomMove(possibleMoves);
            
            case AILevel.Normal:
                return SelectNormalMove(possibleMoves);
            
            case AILevel.Hard:
                return SelectHardMove(possibleMoves);
            
            case AILevel.Expert:
                return SelectExpertMove(possibleMoves);
            
            default:
                return SelectNormalMove(possibleMoves);
        }
    }
    
    /// <summary>
    /// 简单AI：随机选择
    /// </summary>
    private AIDecision SelectRandomMove(List<AIDecision> moves)
    {
        return moves[Random.Range(0, moves.Count)];
    }
    
    /// <summary>
    /// 普通AI：基础评分系统
    /// </summary>
    private AIDecision SelectNormalMove(List<AIDecision> moves)
    {
        // 添加随机性
        foreach (var move in moves)
        {
            float randomFactor = Random.Range(-randomness, randomness);
            moves[moves.IndexOf(move)] = new AIDecision(
                move.card, 
                move.targetSlot, 
                move.score + randomFactor, 
                move.reasoning
            );
        }
        
        // 选择评分最高的
        AIDecision bestMove = moves[0];
        foreach (var move in moves)
        {
            if (move.score > bestMove.score)
            {
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    /// <summary>
    /// 困难AI：考虑更多因素
    /// </summary>
    private AIDecision SelectHardMove(List<AIDecision> moves)
    {
        // 对每个移动进行更深入的评估
        foreach (var move in moves)
        {
            float enhancedScore = move.score;
            
            // 考虑位置战略价值
            enhancedScore += EvaluatePositionalValue(move.targetSlot);
            
            // 考虑与其他卡牌的协同效应
            enhancedScore += EvaluateSynergy(move.card, move.targetSlot);
            
            moves[moves.IndexOf(move)] = new AIDecision(
                move.card, 
                move.targetSlot, 
                enhancedScore, 
                move.reasoning
            );
        }
        
        return SelectNormalMove(moves);
    }
    
    /// <summary>
    /// 专家AI：完美决策
    /// </summary>
    private AIDecision SelectExpertMove(List<AIDecision> moves)
    {
        // 使用minimax算法或其他高级AI算法
        // 这里简化为最优评分选择
        AIDecision bestMove = moves[0];
        foreach (var move in moves)
        {
            float expertScore = CalculateExpertScore(move);
            if (expertScore > CalculateExpertScore(bestMove))
            {
                bestMove = move;
            }
        }
        
        return bestMove;
    }
    
    /// <summary>
    /// 评估移动的分数
    /// </summary>
    private float EvaluateMove(CardData card, BoardSlot slot)
    {
        float score = 0f;
        
        // 基础卡牌价值
        score += card.GetTotalValue();
        
        // 攻击性考虑
        score += card.Attack * aggressiveness * 2f;
        
        // 防御性考虑
        score += card.Defense * defensiveness * 2f;
        
        // 资源效率
        if (card.Cost > 0)
        {
            score += (float)(card.Attack + card.Defense) / card.Cost;
        }
        
        // 位置价值（中心位置更有价值）
        score += EvaluateSlotPosition(slot);
        
        return score;
    }
    
    /// <summary>
    /// 评估格子位置价值
    /// </summary>
    private float EvaluateSlotPosition(BoardSlot slot)
    {
        // 这里需要根据具体的棋盘布局来实现
        // 假设中心位置更有价值
        return Random.Range(0f, 2f); // 临时实现
    }
    
    /// <summary>
    /// 评估位置战略价值
    /// </summary>
    private float EvaluatePositionalValue(BoardSlot slot)
    {
        // 评估该位置的战略重要性
        return Random.Range(0f, 1f); // 临时实现
    }
    
    /// <summary>
    /// 评估协同效应
    /// </summary>
    private float EvaluateSynergy(CardData card, BoardSlot slot)
    {
        // 评估与周围卡牌的协同效应
        return Random.Range(0f, 1f); // 临时实现
    }
    
    /// <summary>
    /// 计算专家级评分
    /// </summary>
    private float CalculateExpertScore(AIDecision move)
    {
        float score = move.score;
        
        // 添加更复杂的评估逻辑
        // 考虑长期战略、对手可能的反应等
        
        return score;
    }
    
    /// <summary>
    /// 生成决策理由
    /// </summary>
    private string GenerateReasoning(CardData card, BoardSlot slot, float score)
    {
        return $"打出 {card.CardName} 到位置 {slot.name}，评分: {score:F1}";
    }
    
    /// <summary>
    /// 获取可用的棋盘格
    /// </summary>
    private List<BoardSlot> GetAvailableSlots()
    {
        List<BoardSlot> availableSlots = new List<BoardSlot>();
        
        if (GameManager.Instance != null && GameManager.Instance.BoardSlots != null)
        {
            foreach (BoardSlot slot in GameManager.Instance.BoardSlots)
            {
                if (slot.IsEmpty())
                {
                    availableSlots.Add(slot);
                }
            }
        }
        
        return availableSlots;
    }
    
    /// <summary>
    /// 结束AI回合
    /// </summary>
    private void EndAITurn()
    {
        Debug.Log($"{playerName}: 回合结束");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCurrentTurn();
        }
    }
    
    /// <summary>
    /// 设置AI等级
    /// </summary>
    public void SetAILevel(AILevel level)
    {
        aiLevel = level;
        playerName = $"AI Player ({aiLevel})";
        Debug.Log($"AI难度设置为: {aiLevel}");
    }
    
    /// <summary>
    /// 设置AI行为参数
    /// </summary>
    public void SetAIBehavior(float aggressiveness, float defensiveness, float randomness)
    {
        this.aggressiveness = Mathf.Clamp01(aggressiveness);
        this.defensiveness = Mathf.Clamp01(defensiveness);
        this.randomness = Mathf.Clamp01(randomness);
        
        Debug.Log($"AI行为参数更新 - 攻击性: {this.aggressiveness}, 防御性: {this.defensiveness}, 随机性: {this.randomness}");
    }
    
    /// <summary>
    /// 设置思考时间
    /// </summary>
    public void SetThinkingTime(float time)
    {
        thinkingTime = Mathf.Max(0.1f, time);
    }
}