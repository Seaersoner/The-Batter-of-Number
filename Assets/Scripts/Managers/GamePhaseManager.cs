using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 游戏阶段管理器，控制数字博弈游戏的各个阶段流程
/// </summary>
public class GamePhaseManager : MonoBehaviour
{
    [Header("阶段设置")]
    [SerializeField] private float phaseTransitionDelay = 1f; // 阶段切换延迟
    [SerializeField] private bool autoProgressPhases = true;  // 是否自动进入下一阶段
    
    // 当前游戏状态
    private GameManager.GamePhase currentPhase = GameManager.GamePhase.MainMenu;
    private int currentRound = 1;
    private int maxRounds = 9; // 最多9轮（每轮出一张牌）
    
    // 玩家引用
    private NumberPlayer humanPlayer;
    private NumberPlayer aiPlayer;
    
    // 当前回合的比牌卡牌
    private NumberCard humanSelectedCard;
    private NumberCard aiSelectedCard;
    
    // 事件
    public System.Action<GameManager.GamePhase> OnPhaseChanged;
    public System.Action<int> OnRoundChanged;
    public System.Action<NumberPlayer> OnPlayerAction;
    public System.Action<NumberCard, NumberCard, int> OnBattleResult; // 人类卡牌, AI卡牌, 结果
    
    // 属性
    public GameManager.GamePhase CurrentPhase => currentPhase;
    public int CurrentRound => currentRound;
    public int MaxRounds => maxRounds;
    
    private void Start()
    {
        InitializePhaseManager();
    }
    
    /// <summary>
    /// 初始化阶段管理器
    /// </summary>
    private void InitializePhaseManager()
    {
        // 获取玩家引用
        if (GameManager.Instance != null)
        {
            // 这里需要从GameManager获取玩家引用
            // humanPlayer = GameManager.Instance.HumanPlayer;
            // aiPlayer = GameManager.Instance.AIPlayer;
        }
        
        // 如果没有找到玩家，创建临时的
        if (humanPlayer == null)
        {
            GameObject humanObj = new GameObject("HumanPlayer");
            humanPlayer = humanObj.AddComponent<NumberPlayer>();
        }
        
        if (aiPlayer == null)
        {
            GameObject aiObj = new GameObject("AIPlayer");
            aiPlayer = aiObj.AddComponent<NumberPlayer>();
        }
        
        Debug.Log("游戏阶段管理器初始化完成");
    }
    
    /// <summary>
    /// 开始新游戏
    /// </summary>
    public void StartNewGame()
    {
        currentRound = 1;
        humanSelectedCard = null;
        aiSelectedCard = null;
        
        // 重置玩家数据
        humanPlayer?.ResetForNewGame();
        aiPlayer?.ResetForNewGame();
        
        // 进入摆牌阶段
        ChangePhase(GameManager.GamePhase.CardPlacement);
    }
    
    /// <summary>
    /// 改变游戏阶段
    /// </summary>
    /// <param name="newPhase">新阶段</param>
    public void ChangePhase(GameManager.GamePhase newPhase)
    {
        if (currentPhase != newPhase)
        {
            GameManager.GamePhase previousPhase = currentPhase;
            currentPhase = newPhase;
            
            OnPhaseChanged?.Invoke(currentPhase);
            
            Debug.Log($"游戏阶段切换：{previousPhase} → {currentPhase}");
            
            // 处理阶段切换逻辑
            HandlePhaseTransition(newPhase);
        }
    }
    
    /// <summary>
    /// 处理阶段切换逻辑
    /// </summary>
    /// <param name="phase">新阶段</param>
    private void HandlePhaseTransition(GameManager.GamePhase phase)
    {
        switch (phase)
        {
            case GameManager.GamePhase.CardPlacement:
                HandleCardPlacementPhase();
                break;
                
            case GameManager.GamePhase.ModifierPlacement:
                HandleModifierPlacementPhase();
                break;
                
            case GameManager.GamePhase.CardReveal:
                HandleCardRevealPhase();
                break;
                
            case GameManager.GamePhase.CardPlay:
                HandleCardPlayPhase();
                break;
                
            case GameManager.GamePhase.CardBattle:
                HandleCardBattlePhase();
                break;
                
            case GameManager.GamePhase.GameOver:
                HandleGameOverPhase();
                break;
        }
    }
    
    /// <summary>
    /// 处理摆牌阶段
    /// </summary>
    private void HandleCardPlacementPhase()
    {
        Debug.Log("=== 摆牌阶段开始 ===");
        Debug.Log("双方玩家打乱手牌并放置到A-I位置");
        
        // 玩家已在初始化时打乱了手牌
        // 这里可以添加UI提示或动画
        
        if (autoProgressPhases)
        {
            StartCoroutine(AutoProgressToNextPhase(phaseTransitionDelay));
        }
    }
    
    /// <summary>
    /// 处理放置加减牌阶段
    /// </summary>
    private void HandleModifierPlacementPhase()
    {
        Debug.Log($"=== 第{currentRound}轮 - 放置加减牌阶段开始 ===");
        Debug.log("每位玩家必须放置一张加牌和一张减牌");
        
        StartCoroutine(HandleModifierPlacementCoroutine());
    }
    
    /// <summary>
    /// 处理放置加减牌的协程
    /// </summary>
    private IEnumerator HandleModifierPlacementCoroutine()
    {
        // 重置回合状态
        humanPlayer.ResetRoundState();
        aiPlayer.ResetRoundState();
        
        // 人类玩家放置加减牌（需要等待输入）
        Debug.Log("等待人类玩家放置加减牌...");
        yield return StartCoroutine(WaitForHumanModifierPlacement());
        
        // AI玩家放置加减牌
        Debug.Log("AI玩家放置加减牌...");
        var aiTargets = aiPlayer.AIDecideModifierTargets();
        aiPlayer.PlaceModifierCards(aiTargets.addTarget, aiTargets.subtractTarget, currentRound);
        
        yield return new WaitForSeconds(1f);
        
        // 进入下一阶段
        ChangePhase(GameManager.GamePhase.CardReveal);
    }
    
    /// <summary>
    /// 等待人类玩家放置加减牌
    /// </summary>
    private IEnumerator WaitForHumanModifierPlacement()
    {
        while (!humanPlayer.HasPlacedModifiersThisRound)
        {
            // 这里需要UI系统来处理用户输入
            // 暂时用简单的自动逻辑代替
            if (autoProgressPhases)
            {
                // 自动为人类玩家选择目标
                var humanTargets = humanPlayer.AIDecideModifierTargets();
                humanPlayer.PlaceModifierCards(humanTargets.addTarget, humanTargets.subtractTarget, currentRound);
                break;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 处理揭牌阶段
    /// </summary>
    private void HandleCardRevealPhase()
    {
        Debug.Log("=== 揭牌阶段开始 ===");
        Debug.Log("双方可以互相揭开对方的一张手牌");
        
        StartCoroutine(HandleCardRevealCoroutine());
    }
    
    /// <summary>
    /// 处理揭牌的协程
    /// </summary>
    private IEnumerator HandleCardRevealCoroutine()
    {
        // 人类玩家揭牌
        Debug.Log("人类玩家揭开AI的一张卡牌...");
        NumberCard aiCardToReveal = aiPlayer.AIDecideCardToReveal(aiPlayer.HandCards);
        if (aiCardToReveal != null)
        {
            humanPlayer.RevealOpponentCard(aiCardToReveal);
        }
        
        yield return new WaitForSeconds(1f);
        
        // AI玩家揭牌
        Debug.Log("AI玩家揭开人类的一张卡牌...");
        NumberCard humanCardToReveal = aiPlayer.AIDecideCardToReveal(humanPlayer.HandCards);
        if (humanCardToReveal != null)
        {
            aiPlayer.RevealOpponentCard(humanCardToReveal);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 进入下一阶段
        ChangePhase(GameManager.GamePhase.CardPlay);
    }
    
    /// <summary>
    /// 处理出牌阶段
    /// </summary>
    private void HandleCardPlayPhase()
    {
        Debug.Log("=== 出牌阶段开始 ===");
        Debug.Log("双方选择一张手牌出牌");
        
        StartCoroutine(HandleCardPlayCoroutine());
    }
    
    /// <summary>
    /// 处理出牌的协程
    /// </summary>
    private IEnumerator HandleCardPlayCoroutine()
    {
        // 人类玩家出牌
        Debug.Log("等待人类玩家出牌...");
        yield return StartCoroutine(WaitForHumanCardPlay());
        
        // AI玩家出牌
        Debug.Log("AI玩家出牌...");
        string aiCardId = aiPlayer.AIDecideCardToPlay();
        if (aiCardId != null)
        {
            aiPlayer.SelectCardForPlay(aiCardId);
            aiSelectedCard = aiPlayer.GetSelectedCard();
        }
        
        yield return new WaitForSeconds(1f);
        
        // 进入比牌阶段
        ChangePhase(GameManager.GamePhase.CardBattle);
    }
    
    /// <summary>
    /// 等待人类玩家出牌
    /// </summary>
    private IEnumerator WaitForHumanCardPlay()
    {
        while (humanPlayer.GetSelectedCard() == null)
        {
            // 这里需要UI系统来处理用户输入
            // 暂时用自动逻辑代替
            if (autoProgressPhases)
            {
                string humanCardId = humanPlayer.AIDecideCardToPlay();
                if (humanCardId != null)
                {
                    humanPlayer.SelectCardForPlay(humanCardId);
                    humanSelectedCard = humanPlayer.GetSelectedCard();
                }
                break;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 处理比牌阶段
    /// </summary>
    private void HandleCardBattlePhase()
    {
        Debug.Log("=== 比牌阶段开始 ===");
        
        StartCoroutine(HandleCardBattleCoroutine());
    }
    
    /// <summary>
    /// 处理比牌的协程
    /// </summary>
    private IEnumerator HandleCardBattleCoroutine()
    {
        if (humanSelectedCard != null && aiSelectedCard != null)
        {
            Debug.Log($"比牌：人类 {humanSelectedCard} VS AI {aiSelectedCard}");
            
            // 进行比牌
            int result = humanPlayer.BattleCards(humanSelectedCard, aiSelectedCard);
            
            // 触发比牌结果事件
            OnBattleResult?.Invoke(humanSelectedCard, aiSelectedCard, result);
            
            yield return new WaitForSeconds(2f);
            
            // 回收加减牌
            humanPlayer.RecycleModifierCards();
            aiPlayer.RecycleModifierCards();
            
            // 检查游戏是否结束
            if (CheckGameEndConditions())
            {
                ChangePhase(GameManager.GamePhase.GameOver);
            }
            else
            {
                // 进入下一轮
                StartNextRound();
            }
        }
        else
        {
            Debug.LogError("比牌失败：缺少卡牌");
            ChangePhase(GameManager.GamePhase.GameOver);
        }
    }
    
    /// <summary>
    /// 检查游戏结束条件
    /// </summary>
    /// <returns>是否游戏结束</returns>
    private bool CheckGameEndConditions()
    {
        // 条件1：有玩家达到4分
        if (humanPlayer.CheckWinCondition(4) || aiPlayer.CheckWinCondition(4))
        {
            return true;
        }
        
        // 条件2：达到最大轮数
        if (currentRound >= maxRounds)
        {
            return true;
        }
        
        // 条件3：没有更多可用卡牌
        if (humanPlayer.AvailableCards.Count == 0 || aiPlayer.AvailableCards.Count == 0)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 开始下一轮
    /// </summary>
    private void StartNextRound()
    {
        currentRound++;
        humanSelectedCard = null;
        aiSelectedCard = null;
        
        OnRoundChanged?.Invoke(currentRound);
        
        Debug.Log($"=== 第{currentRound}轮开始 ===");
        
        // 返回到放置加减牌阶段
        ChangePhase(GameManager.GamePhase.ModifierPlacement);
    }
    
    /// <summary>
    /// 处理游戏结束阶段
    /// </summary>
    private void HandleGameOverPhase()
    {
        Debug.Log("=== 游戏结束 ===");
        
        // 确定获胜者
        NumberPlayer winner = DetermineWinner();
        
        if (winner != null)
        {
            Debug.Log($"游戏获胜者：{winner.PlayerName}");
        }
        else
        {
            Debug.Log("游戏平局！");
        }
        
        // 显示最终统计
        Debug.Log($"最终结果：");
        Debug.Log($"人类玩家：{humanPlayer.GetGameStats()}");
        Debug.Log($"AI玩家：{aiPlayer.GetGameStats()}");
    }
    
    /// <summary>
    /// 确定获胜者
    /// </summary>
    /// <returns>获胜的玩家，如果平局则返回null</returns>
    private NumberPlayer DetermineWinner()
    {
        // 首先检查是否有玩家达到4分
        if (humanPlayer.CurrentScore >= 4)
        {
            return humanPlayer;
        }
        
        if (aiPlayer.CurrentScore >= 4)
        {
            return aiPlayer;
        }
        
        // 如果都没有达到4分，比较最终分数（剩余卡牌总值）
        int humanFinalScore = humanPlayer.CalculateFinalScore();
        int aiFinalScore = aiPlayer.CalculateFinalScore();
        
        if (humanFinalScore > aiFinalScore)
        {
            return humanPlayer;
        }
        else if (aiFinalScore > humanFinalScore)
        {
            return aiPlayer;
        }
        
        // 平局
        return null;
    }
    
    /// <summary>
    /// 自动进入下一阶段的协程
    /// </summary>
    private IEnumerator AutoProgressToNextPhase(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        switch (currentPhase)
        {
            case GameManager.GamePhase.CardPlacement:
                ChangePhase(GameManager.GamePhase.ModifierPlacement);
                break;
        }
    }
    
    /// <summary>
    /// 手动进入下一阶段（供UI调用）
    /// </summary>
    public void ProgressToNextPhase()
    {
        switch (currentPhase)
        {
            case GameManager.GamePhase.CardPlacement:
                ChangePhase(GameManager.GamePhase.ModifierPlacement);
                break;
            case GameManager.GamePhase.ModifierPlacement:
                ChangePhase(GameManager.GamePhase.CardReveal);
                break;
            case GameManager.GamePhase.CardReveal:
                ChangePhase(GameManager.GamePhase.CardPlay);
                break;
            case GameManager.GamePhase.CardPlay:
                ChangePhase(GameManager.GamePhase.CardBattle);
                break;
        }
    }
    
    /// <summary>
    /// 重置游戏
    /// </summary>
    public void ResetGame()
    {
        currentRound = 1;
        humanSelectedCard = null;
        aiSelectedCard = null;
        
        ChangePhase(GameManager.GamePhase.MainMenu);
    }
    
    /// <summary>
    /// 获取当前阶段的描述
    /// </summary>
    /// <returns>阶段描述</returns>
    public string GetCurrentPhaseDescription()
    {
        switch (currentPhase)
        {
            case GameManager.GamePhase.MainMenu:
                return "主菜单";
            case GameManager.GamePhase.GameStart:
                return "游戏开始";
            case GameManager.GamePhase.CardPlacement:
                return "摆牌阶段 - 打乱手牌并放置到A-I位置";
            case GameManager.GamePhase.ModifierPlacement:
                return $"第{currentRound}轮 - 放置加减牌阶段";
            case GameManager.GamePhase.CardReveal:
                return "揭牌阶段 - 互相揭开对方一张卡牌";
            case GameManager.GamePhase.CardPlay:
                return "出牌阶段 - 选择一张卡牌出牌";
            case GameManager.GamePhase.CardBattle:
                return "比牌阶段 - 比较卡牌大小";
            case GameManager.GamePhase.GameOver:
                return "游戏结束";
            default:
                return "未知阶段";
        }
    }
}