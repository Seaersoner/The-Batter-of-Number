using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI管理器，控制所有界面切换和更新
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("主要UI面板")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("游戏内UI")]
    [SerializeField] private TimerUI timerUI;
    [SerializeField] private HandPanelUI handPanelUI;
    [SerializeField] private Text currentPlayerText;
    [SerializeField] private Button endTurnButton;
    
    [Header("游戏结束UI")]
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 设置按钮事件
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        
        // 默认显示主菜单
        ShowMainMenu();
    }
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnTurnChanged += OnTurnChanged;
            GameManager.Instance.OnGameOver += OnGameOver;
        }
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
            GameManager.Instance.OnGameOver -= OnGameOver;
        }
    }
    
    /// <summary>
    /// 显示主菜单
    /// </summary>
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    /// <summary>
    /// 显示游戏界面
    /// </summary>
    public void ShowGameplay()
    {
        SetActivePanel(gameplayPanel);
    }
    
    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    /// <param name="winner">获胜玩家</param>
    public void ShowGameOverUI(Player winner)
    {
        SetActivePanel(gameOverPanel);
        
        if (gameOverText != null)
        {
            if (winner != null)
            {
                gameOverText.text = $"{winner.name} 获胜！";
            }
            else
            {
                gameOverText.text = "游戏结束";
            }
        }
    }
    
    /// <summary>
    /// 显示暂停界面
    /// </summary>
    public void ShowPauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 隐藏暂停界面
    /// </summary>
    public void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 设置当前激活的面板
    /// </summary>
    /// <param name="activePanel">要激活的面板</param>
    private void SetActivePanel(GameObject activePanel)
    {
        // 隐藏所有面板
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // 显示指定面板
        if (activePanel != null)
        {
            activePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 开始回合计时器
    /// </summary>
    /// <param name="timeLimit">时间限制</param>
    public void StartTurnTimer(float timeLimit)
    {
        if (timerUI != null)
        {
            timerUI.StartTimer(timeLimit);
        }
    }
    
    /// <summary>
    /// 停止计时器
    /// </summary>
    public void StopTimer()
    {
        if (timerUI != null)
        {
            timerUI.StopTimer();
        }
    }
    
    /// <summary>
    /// 更新手牌UI
    /// </summary>
    /// <param name="player">玩家</param>
    public void UpdateHandUI(Player player)
    {
        if (handPanelUI != null)
        {
            handPanelUI.UpdateHand(player.Hand);
        }
    }
    
    // 事件回调
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameManager.GameState.GameStart:
            case GameManager.GameState.HumanTurn:
            case GameManager.GameState.AITurn:
                ShowGameplay();
                break;
            case GameManager.GameState.GameOver:
                // 游戏结束UI会在OnGameOver中处理
                break;
        }
    }
    
    private void OnTurnChanged(Player currentPlayer)
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"当前回合: {currentPlayer.name}";
        }
        
        // 更新结束回合按钮状态
        if (endTurnButton != null)
        {
            endTurnButton.interactable = (currentPlayer is HumanPlayer);
        }
        
        // 更新手牌UI
        if (currentPlayer is HumanPlayer)
        {
            UpdateHandUI(currentPlayer);
        }
    }
    
    private void OnGameOver(Player winner)
    {
        StopTimer();
    }
    
    // 按钮事件处理
    private void OnEndTurnButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndCurrentTurn();
        }
    }
    
    private void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
    }
    
    private void OnMainMenuButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameManager.GameState.MainMenu);
        }
    }
    
    /// <summary>
    /// 开始新游戏按钮事件
    /// </summary>
    public void OnStartGameButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
    }
    
    /// <summary>
    /// 退出游戏按钮事件
    /// </summary>
    public void OnQuitGameButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}