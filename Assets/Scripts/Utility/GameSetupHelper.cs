using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏设置助手，帮助快速创建基础UI和场景结构
/// </summary>
public class GameSetupHelper : MonoBehaviour
{
    [Header("自动设置选项")]
    [SerializeField] private bool createUIStructure = true;
    [SerializeField] private bool createGameBoard = true;
    [SerializeField] private bool createManagers = true;
    [SerializeField] private bool createPlayers = true;
    
    [Header("UI设置")]
    [SerializeField] private Font defaultFont;
    [SerializeField] private Sprite defaultButtonSprite;
    [SerializeField] private Color primaryColor = Color.blue;
    [SerializeField] private Color secondaryColor = Color.white;
    
    // 创建的对象引用
    private Canvas mainCanvas;
    private GameObject gameManager;
    private GameObject uiManager;
    private GameObject audioManager;
    private GameObject humanPlayer;
    private GameObject aiPlayer;
    private GameObject gameBoard;
    
    /// <summary>
    /// 一键设置游戏场景
    /// </summary>
    [ContextMenu("Setup Complete Game Scene")]
    public void SetupCompleteGameScene()
    {
        Debug.Log("=== 开始自动设置游戏场景 ===");
        
        if (createManagers)
        {
            CreateManagers();
        }
        
        if (createUIStructure)
        {
            CreateUIStructure();
        }
        
        if (createGameBoard)
        {
            CreateGameBoard();
        }
        
        if (createPlayers)
        {
            CreatePlayers();
        }
        
        ConfigureReferences();
        
        Debug.Log("=== 游戏场景设置完成 ===");
        Debug.Log("请检查各个管理器的配置，并根据需要调整参数。");
    }
    
    /// <summary>
    /// 创建核心管理器
    /// </summary>
    private void CreateManagers()
    {
        Debug.Log("创建核心管理器...");
        
        // 创建GameManager
        gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        gameManager.AddComponent<GamePhaseManager>();
        gameManager.AddComponent<ResourceManager>();
        gameManager.AddComponent<PerformanceMonitor>();
        
        // 创建UIManager
        uiManager = new GameObject("UIManager");
        uiManager.AddComponent<UIManager>();
        
        // 创建AudioManager
        audioManager = new GameObject("AudioManager");
        audioManager.AddComponent<AudioManager>();
        
        // 创建音频源
        GameObject musicSource = new GameObject("MusicSource");
        musicSource.transform.SetParent(audioManager.transform);
        AudioSource musicAudio = musicSource.AddComponent<AudioSource>();
        musicAudio.loop = true;
        musicAudio.playOnAwake = false;
        
        GameObject sfxSource = new GameObject("SFXSource");
        sfxSource.transform.SetParent(audioManager.transform);
        AudioSource sfxAudio = sfxSource.AddComponent<AudioSource>();
        sfxAudio.playOnAwake = false;
        
        Debug.Log("核心管理器创建完成");
    }
    
    /// <summary>
    /// 创建UI结构
    /// </summary>
    private void CreateUIStructure()
    {
        Debug.Log("创建UI结构...");
        
        // 创建主Canvas
        GameObject canvasObj = new GameObject("MainCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // 创建UI面板结构
        CreateMainMenuPanel();
        CreateGameplayPanel();
        CreateGameOverPanel();
        
        Debug.Log("UI结构创建完成");
    }
    
    /// <summary>
    /// 创建主菜单面板
    /// </summary>
    private void CreateMainMenuPanel()
    {
        GameObject mainMenuPanel = CreateUIPanel("MainMenuPanel", mainCanvas.transform);
        
        // 背景
        GameObject background = CreateUIImage("Background", mainMenuPanel.transform);
        Image bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
        
        // 标题
        GameObject titleText = CreateUIText("TitleText", mainMenuPanel.transform, "数字博弈游戏");
        Text title = titleText.GetComponent<Text>();
        title.fontSize = 48;
        title.color = primaryColor;
        title.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 200);
        
        // 开始游戏按钮
        GameObject startButton = CreateUIButton("StartButton", mainMenuPanel.transform, "开始游戏");
        RectTransform startRect = startButton.GetComponent<RectTransform>();
        startRect.anchoredPosition = new Vector2(0, 50);
        
        // 退出游戏按钮
        GameObject quitButton = CreateUIButton("QuitButton", mainMenuPanel.transform, "退出游戏");
        RectTransform quitRect = quitButton.GetComponent<RectTransform>();
        quitRect.anchoredPosition = new Vector2(0, -50);
    }
    
    /// <summary>
    /// 创建游戏界面面板
    /// </summary>
    private void CreateGameplayPanel()
    {
        GameObject gameplayPanel = CreateUIPanel("GameplayPanel", mainCanvas.transform);
        gameplayPanel.SetActive(false);
        
        // 顶部UI
        GameObject topUI = CreateUIPanel("TopUI", gameplayPanel.transform);
        RectTransform topRect = topUI.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 0.85f);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.offsetMin = Vector2.zero;
        topRect.offsetMax = Vector2.zero;
        
        // 计时器UI
        GameObject timerUI = new GameObject("TimerUI");
        timerUI.transform.SetParent(topUI.transform);
        timerUI.AddComponent<TimerUI>();
        
        RectTransform timerRect = timerUI.AddComponent<RectTransform>();
        timerRect.anchoredPosition = new Vector2(-200, 0);
        timerRect.sizeDelta = new Vector2(200, 100);
        
        // 添加计时器组件
        Image timerBg = timerUI.AddComponent<Image>();
        timerBg.color = new Color(0, 0, 0, 0.5f);
        
        GameObject timerText = CreateUIText("TimerText", timerUI.transform, "00:00");
        timerText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        
        // 阶段显示
        GameObject phaseText = CreateUIText("PhaseText", topUI.transform, "摆牌阶段");
        Text phaseTextComp = phaseText.GetComponent<Text>();
        phaseTextComp.fontSize = 32;
        phaseTextComp.alignment = TextAnchor.MiddleCenter;
        
        RectTransform phaseRect = phaseText.GetComponent<RectTransform>();
        phaseRect.anchoredPosition = new Vector2(0, 0);
        
        // 分数显示
        GameObject scoreText = CreateUIText("ScoreText", topUI.transform, "人类: 0 - AI: 0");
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        scoreRect.anchoredPosition = new Vector2(200, 0);
        
        // 手牌面板
        GameObject handPanel = new GameObject("HandPanelUI");
        handPanel.transform.SetParent(gameplayPanel.transform);
        handPanel.AddComponent<HandPanelUI>();
        
        RectTransform handRect = handPanel.AddComponent<RectTransform>();
        handRect.anchorMin = new Vector2(0, 0);
        handRect.anchorMax = new Vector2(1, 0.2f);
        handRect.offsetMin = Vector2.zero;
        handRect.offsetMax = Vector2.zero;
        
        // 控制按钮
        GameObject controlButtons = CreateUIPanel("ControlButtons", gameplayPanel.transform);
        RectTransform controlRect = controlButtons.GetComponent<RectTransform>();
        controlRect.anchorMin = new Vector2(0.8f, 0.2f);
        controlRect.anchorMax = new Vector2(1, 0.8f);
        controlRect.offsetMin = Vector2.zero;
        controlRect.offsetMax = Vector2.zero;
        
        GameObject endTurnButton = CreateUIButton("EndTurnButton", controlButtons.transform, "结束回合");
        RectTransform endTurnRect = endTurnButton.GetComponent<RectTransform>();
        endTurnRect.anchoredPosition = new Vector2(0, 100);
        
        GameObject pauseButton = CreateUIButton("PauseButton", controlButtons.transform, "暂停");
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        pauseRect.anchoredPosition = new Vector2(0, 0);
    }
    
    /// <summary>
    /// 创建游戏结束面板
    /// </summary>
    private void CreateGameOverPanel()
    {
        GameObject gameOverPanel = CreateUIPanel("GameOverPanel", mainCanvas.transform);
        gameOverPanel.SetActive(false);
        
        // 背景
        GameObject background = CreateUIImage("Background", gameOverPanel.transform);
        Image bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        // 结果文本
        GameObject resultText = CreateUIText("ResultText", gameOverPanel.transform, "游戏结束");
        Text result = resultText.GetComponent<Text>();
        result.fontSize = 40;
        result.alignment = TextAnchor.MiddleCenter;
        
        RectTransform resultRect = resultText.GetComponent<RectTransform>();
        resultRect.anchoredPosition = new Vector2(0, 100);
        
        // 重新开始按钮
        GameObject restartButton = CreateUIButton("RestartButton", gameOverPanel.transform, "重新开始");
        RectTransform restartRect = restartButton.GetComponent<RectTransform>();
        restartRect.anchoredPosition = new Vector2(0, 0);
        
        // 返回主菜单按钮
        GameObject mainMenuButton = CreateUIButton("MainMenuButton", gameOverPanel.transform, "返回主菜单");
        RectTransform mainMenuRect = mainMenuButton.GetComponent<RectTransform>();
        mainMenuRect.anchoredPosition = new Vector2(0, -100);
    }
    
    /// <summary>
    /// 创建游戏棋盘
    /// </summary>
    private void CreateGameBoard()
    {
        Debug.Log("创建游戏棋盘...");
        
        gameBoard = new GameObject("GameBoard");
        GameBoardLayout boardLayout = gameBoard.AddComponent<GameBoardLayout>();
        
        // 设置棋盘参数
        // 注意：由于GameBoardLayout的字段是private，我们需要在Inspector中手动设置
        // 或者修改GameBoardLayout脚本让字段public
        
        Debug.Log("游戏棋盘创建完成 - 请在GameBoardLayout组件中设置参数");
    }
    
    /// <summary>
    /// 创建玩家对象
    /// </summary>
    private void CreatePlayers()
    {
        Debug.Log("创建玩家对象...");
        
        // 创建人类玩家
        humanPlayer = new GameObject("HumanPlayer");
        NumberPlayer humanPlayerScript = humanPlayer.AddComponent<NumberPlayer>();
        // 注意：需要通过Inspector设置玩家参数
        
        // 创建AI玩家
        aiPlayer = new GameObject("AIPlayer");
        NumberPlayer aiPlayerScript = aiPlayer.AddComponent<NumberPlayer>();
        // 注意：需要通过Inspector设置AI参数
        
        Debug.Log("玩家对象创建完成 - 请在Inspector中配置玩家参数");
    }
    
    /// <summary>
    /// 配置组件引用
    /// </summary>
    private void ConfigureReferences()
    {
        Debug.Log("配置组件引用...");
        
        // 这里可以尝试自动设置一些引用
        // 但由于很多字段是private，大部分配置需要在Inspector中手动完成
        
        Debug.Log("基础引用配置完成 - 请在Inspector中完成详细配置");
    }
    
    /// <summary>
    /// 创建UI面板
    /// </summary>
    private GameObject CreateUIPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return panel;
    }
    
    /// <summary>
    /// 创建UI文本
    /// </summary>
    private GameObject CreateUIText(string name, Transform parent, string content)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = defaultFont != null ? defaultFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = secondaryColor;
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        
        return textObj;
    }
    
    /// <summary>
    /// 创建UI按钮
    /// </summary>
    private GameObject CreateUIButton(string name, Transform parent, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = primaryColor;
        
        Button button = buttonObj.AddComponent<Button>();
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);
        
        // 按钮文本
        GameObject textObj = CreateUIText("Text", buttonObj.transform, text);
        Text buttonText = textObj.GetComponent<Text>();
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
    
    /// <summary>
    /// 创建UI图片
    /// </summary>
    private GameObject CreateUIImage(string name, Transform parent)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent);
        
        Image image = imageObj.AddComponent<Image>();
        
        RectTransform rect = imageObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        return imageObj;
    }
    
    /// <summary>
    /// 验证场景设置
    /// </summary>
    [ContextMenu("Validate Scene Setup")]
    public void ValidateSceneSetup()
    {
        Debug.Log("=== 验证场景设置 ===");
        
        // 检查必需的组件
        bool allValid = true;
        
        if (FindObjectOfType<GameManager>() == null)
        {
            Debug.LogError("缺少 GameManager 组件");
            allValid = false;
        }
        
        if (FindObjectOfType<UIManager>() == null)
        {
            Debug.LogError("缺少 UIManager 组件");
            allValid = false;
        }
        
        if (FindObjectOfType<Canvas>() == null)
        {
            Debug.LogError("缺少 Canvas 组件");
            allValid = false;
        }
        
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.LogError("缺少 EventSystem 组件");
            allValid = false;
        }
        
        if (allValid)
        {
            Debug.Log("✅ 场景设置验证通过！");
        }
        else
        {
            Debug.LogWarning("❌ 场景设置不完整，请检查缺少的组件");
        }
    }
    
    /// <summary>
    /// 清理场景
    /// </summary>
    [ContextMenu("Clear Scene Setup")]
    public void ClearSceneSetup()
    {
        if (UnityEditor.EditorUtility.DisplayDialog("清理场景", "这将删除所有自动创建的游戏对象，确定继续吗？", "确定", "取消"))
        {
            // 删除创建的对象
            if (mainCanvas != null) DestroyImmediate(mainCanvas.gameObject);
            if (gameManager != null) DestroyImmediate(gameManager);
            if (uiManager != null) DestroyImmediate(uiManager);
            if (audioManager != null) DestroyImmediate(audioManager);
            if (humanPlayer != null) DestroyImmediate(humanPlayer);
            if (aiPlayer != null) DestroyImmediate(aiPlayer);
            if (gameBoard != null) DestroyImmediate(gameBoard);
            
            Debug.Log("场景已清理");
        }
    }
}