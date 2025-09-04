using UnityEngine;

/// <summary>
/// 游戏配置类，统一管理游戏参数
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("游戏规则")]
    [Tooltip("获胜所需的局数")]
    public int winConditionScore = 5;
    
    [Tooltip("最大回合数")]
    public int maxRounds = 9;
    
    [Tooltip("每个阶段的时间限制（秒）")]
    public float phaseTimeLimit = 60f;
    
    [Tooltip("AI思考时间（秒）")]
    public float aiThinkingTime = 2f;
    
    [Header("卡牌设置")]
    [Tooltip("手牌数量")]
    public int handCardCount = 9;
    
    [Tooltip("卡牌ID数组")]
    public string[] cardIds = { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
    
    [Header("布局设置")]
    [Tooltip("卡牌间距")]
    public float cardSpacing = 120f;
    
    [Tooltip("行间距")]
    public float rowSpacing = 150f;
    
    [Tooltip("卡牌尺寸")]
    public Vector2 cardSize = new Vector2(100f, 140f);
    
    [Header("UI设置")]
    [Tooltip("动画持续时间")]
    public float animationDuration = 0.3f;
    
    [Tooltip("是否启用动画")]
    public bool enableAnimations = true;
    
    [Tooltip("是否显示调试信息")]
    public bool showDebugInfo = false;
    
    [Header("音效设置")]
    [Tooltip("主音量")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Tooltip("音效音量")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Tooltip("背景音乐音量")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.7f;
    
    [Header("AI设置")]
    [Tooltip("AI难度等级")]
    public AIPlayer.AILevel defaultAILevel = AIPlayer.AILevel.Normal;
    
    [Tooltip("AI攻击性")]
    [Range(0f, 1f)]
    public float aiAggressiveness = 0.5f;
    
    [Tooltip("AI防御性")]
    [Range(0f, 1f)]
    public float aiDefensiveness = 0.5f;
    
    [Tooltip("AI随机性")]
    [Range(0f, 1f)]
    public float aiRandomness = 0.2f;
    
    [Header("性能设置")]
    [Tooltip("目标帧率")]
    public int targetFrameRate = 60;
    
    [Tooltip("是否启用垂直同步")]
    public bool enableVSync = true;
    
    [Tooltip("对象池初始大小")]
    public int objectPoolSize = 50;
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    public void ValidateConfig()
    {
        // 确保基本参数的有效性
        winConditionScore = Mathf.Max(1, winConditionScore);
        maxRounds = Mathf.Max(1, maxRounds);
        phaseTimeLimit = Mathf.Max(10f, phaseTimeLimit);
        aiThinkingTime = Mathf.Max(0.1f, aiThinkingTime);
        handCardCount = Mathf.Clamp(handCardCount, 1, 9);
        
        // 确保卡牌ID数组的长度匹配手牌数量
        if (cardIds.Length != handCardCount)
        {
            Debug.LogWarning($"CardIds数组长度({cardIds.Length})与handCardCount({handCardCount})不匹配");
        }
        
        // 确保布局参数合理
        cardSpacing = Mathf.Max(50f, cardSpacing);
        rowSpacing = Mathf.Max(100f, rowSpacing);
        cardSize.x = Mathf.Max(50f, cardSize.x);
        cardSize.y = Mathf.Max(70f, cardSize.y);
        
        // 确保音量范围正确
        masterVolume = Mathf.Clamp01(masterVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        
        // 确保AI参数范围正确
        aiAggressiveness = Mathf.Clamp01(aiAggressiveness);
        aiDefensiveness = Mathf.Clamp01(aiDefensiveness);
        aiRandomness = Mathf.Clamp01(aiRandomness);
        
        // 确保性能参数合理
        targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 120);
        objectPoolSize = Mathf.Max(10, objectPoolSize);
    }
    
    /// <summary>
    /// 应用配置到游戏
    /// </summary>
    public void ApplyConfig()
    {
        // 设置帧率
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        
        // 应用音量设置
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MasterVolume = masterVolume;
            AudioManager.Instance.SfxVolume = sfxVolume;
            AudioManager.Instance.MusicVolume = bgmVolume;
        }
        
        Debug.Log("游戏配置已应用");
    }
    
    /// <summary>
    /// 重置为默认值
    /// </summary>
    public void ResetToDefaults()
    {
        winConditionScore = 5;
        maxRounds = 9;
        phaseTimeLimit = 60f;
        aiThinkingTime = 2f;
        handCardCount = 9;
        cardIds = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        cardSpacing = 120f;
        rowSpacing = 150f;
        cardSize = new Vector2(100f, 140f);
        animationDuration = 0.3f;
        enableAnimations = true;
        showDebugInfo = false;
        masterVolume = 1f;
        sfxVolume = 1f;
        bgmVolume = 0.7f;
        defaultAILevel = AIPlayer.AILevel.Normal;
        aiAggressiveness = 0.5f;
        aiDefensiveness = 0.5f;
        aiRandomness = 0.2f;
        targetFrameRate = 60;
        enableVSync = true;
        objectPoolSize = 50;
        
        Debug.Log("配置已重置为默认值");
    }
    
    /// <summary>
    /// 获取配置摘要
    /// </summary>
    /// <returns>配置信息字符串</returns>
    public string GetConfigSummary()
    {
        return $"游戏配置摘要:\n" +
               $"- 胜利条件: {winConditionScore}局\n" +
               $"- 最大回合: {maxRounds}\n" +
               $"- 阶段时限: {phaseTimeLimit}秒\n" +
               $"- AI等级: {defaultAILevel}\n" +
               $"- 目标帧率: {targetFrameRate}FPS\n" +
               $"- 动画: {(enableAnimations ? "启用" : "禁用")}";
    }
    
    private void OnValidate()
    {
        // 在Inspector中修改时自动验证
        ValidateConfig();
    }
}