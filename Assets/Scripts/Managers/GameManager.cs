using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 游戏总管理器，控制游戏流程和状态
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("游戏设置")]
    [SerializeField] private int maxHandSize = 5;
    [SerializeField] private float turnTimeLimit = 30f;
    
    [Header("玩家")]
    [SerializeField] private HumanPlayer humanPlayer;
    [SerializeField] private AIPlayer aiPlayer;
    
    [Header("棋盘")]
    [SerializeField] private BoardSlot[] boardSlots;
    
    // 游戏状态
    public enum GameState
    {
        MainMenu,
        GameStart,
        HumanTurn,
        AITurn,
        GameOver
    }
    
    [Header("游戏状态")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    // 事件
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<Player> OnTurnChanged;
    public System.Action<Player> OnGameOver;
    
    // 属性
    public GameState CurrentState => currentState;
    public HumanPlayer HumanPlayer => humanPlayer;
    public AIPlayer AIPlayer => aiPlayer;
    public BoardSlot[] BoardSlots => boardSlots;
    public int MaxHandSize => maxHandSize;
    public float TurnTimeLimit => turnTimeLimit;
    
    private void Start()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 初始化棋盘
        if (boardSlots == null || boardSlots.Length == 0)
        {
            boardSlots = FindObjectsOfType<BoardSlot>();
        }
        
        // 初始化玩家
        if (humanPlayer == null)
            humanPlayer = FindObjectOfType<HumanPlayer>();
        if (aiPlayer == null)
            aiPlayer = FindObjectOfType<AIPlayer>();
        
        // 设置初始状态
        ChangeGameState(GameState.MainMenu);
    }
    
    /// <summary>
    /// 开始新游戏
    /// </summary>
    public void StartNewGame()
    {
        ChangeGameState(GameState.GameStart);
        
        // 初始化玩家手牌
        humanPlayer?.InitializeHand();
        aiPlayer?.InitializeHand();
        
        // 清空棋盘
        foreach (var slot in boardSlots)
        {
            slot.ClearSlot();
        }
        
        // 开始人类玩家回合
        StartHumanTurn();
    }
    
    /// <summary>
    /// 开始人类玩家回合
    /// </summary>
    public void StartHumanTurn()
    {
        ChangeGameState(GameState.HumanTurn);
        OnTurnChanged?.Invoke(humanPlayer);
        
        // 启动计时器
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartTurnTimer(turnTimeLimit);
        }
    }
    
    /// <summary>
    /// 开始AI回合
    /// </summary>
    public void StartAITurn()
    {
        ChangeGameState(GameState.AITurn);
        OnTurnChanged?.Invoke(aiPlayer);
        
        // AI执行决策
        aiPlayer?.MakeMove();
    }
    
    /// <summary>
    /// 结束当前回合
    /// </summary>
    public void EndCurrentTurn()
    {
        switch (currentState)
        {
            case GameState.HumanTurn:
                StartAITurn();
                break;
            case GameState.AITurn:
                StartHumanTurn();
                break;
        }
    }
    
    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    public bool CheckGameOver()
    {
        // TODO: 实现胜负判定逻辑
        // 这里需要根据具体游戏规则来实现
        
        return false;
    }
    
    /// <summary>
    /// 结束游戏
    /// </summary>
    /// <param name="winner">获胜玩家</param>
    public void EndGame(Player winner)
    {
        ChangeGameState(GameState.GameOver);
        OnGameOver?.Invoke(winner);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverUI(winner);
        }
    }
    
    /// <summary>
    /// 改变游戏状态
    /// </summary>
    /// <param name="newState">新状态</param>
    private void ChangeGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
            
            Debug.Log($"游戏状态改变为: {currentState}");
        }
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}