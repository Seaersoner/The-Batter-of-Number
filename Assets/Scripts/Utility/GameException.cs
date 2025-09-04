using System;

/// <summary>
/// 游戏异常处理系统
/// </summary>
namespace GameExceptions
{
    /// <summary>
    /// 游戏基础异常类
    /// </summary>
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
        public GameException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    /// <summary>
    /// 无效卡牌操作异常
    /// </summary>
    public class InvalidCardOperationException : GameException
    {
        public string CardId { get; }
        public string Operation { get; }
        
        public InvalidCardOperationException(string cardId, string operation, string message) 
            : base($"无效的卡牌操作 - 卡牌: {cardId}, 操作: {operation}, 原因: {message}")
        {
            CardId = cardId;
            Operation = operation;
        }
    }
    
    /// <summary>
    /// 游戏状态异常
    /// </summary>
    public class InvalidGameStateException : GameException
    {
        public GameManager.GamePhase CurrentPhase { get; }
        public string ExpectedPhase { get; }
        
        public InvalidGameStateException(GameManager.GamePhase currentPhase, string expectedPhase, string operation)
            : base($"无效的游戏状态操作 - 当前阶段: {currentPhase}, 期望阶段: {expectedPhase}, 操作: {operation}")
        {
            CurrentPhase = currentPhase;
            ExpectedPhase = expectedPhase;
        }
    }
    
    /// <summary>
    /// 玩家操作异常
    /// </summary>
    public class InvalidPlayerOperationException : GameException
    {
        public string PlayerId { get; }
        
        public InvalidPlayerOperationException(string playerId, string message)
            : base($"无效的玩家操作 - 玩家: {playerId}, 原因: {message}")
        {
            PlayerId = playerId;
        }
    }
    
    /// <summary>
    /// 配置异常
    /// </summary>
    public class GameConfigException : GameException
    {
        public string ConfigKey { get; }
        
        public GameConfigException(string configKey, string message)
            : base($"游戏配置错误 - 配置项: {configKey}, 原因: {message}")
        {
            ConfigKey = configKey;
        }
    }
}

/// <summary>
/// 安全操作工具类
/// </summary>
public static class SafeOperations
{
    /// <summary>
    /// 安全获取列表元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="list">列表</param>
    /// <param name="index">索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>元素或默认值</returns>
    public static T SafeGet<T>(System.Collections.Generic.List<T> list, int index, T defaultValue = default(T))
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            return defaultValue;
        }
        return list[index];
    }
    
    /// <summary>
    /// 安全获取数组元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="array">数组</param>
    /// <param name="index">索引</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>元素或默认值</returns>
    public static T SafeGet<T>(T[] array, int index, T defaultValue = default(T))
    {
        if (array == null || index < 0 || index >= array.Length)
        {
            return defaultValue;
        }
        return array[index];
    }
    
    /// <summary>
    /// 安全执行操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>是否成功执行</returns>
    public static bool SafeExecute(System.Action action, string errorMessage = "操作执行失败")
    {
        try
        {
            action?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"{errorMessage}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 安全执行带返回值的操作
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="func">要执行的函数</param>
    /// <param name="defaultValue">默认返回值</param>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>函数结果或默认值</returns>
    public static T SafeExecute<T>(System.Func<T> func, T defaultValue = default(T), string errorMessage = "操作执行失败")
    {
        try
        {
            return func != null ? func.Invoke() : defaultValue;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"{errorMessage}: {ex.Message}");
            return defaultValue;
        }
    }
    
    /// <summary>
    /// 验证游戏状态
    /// </summary>
    /// <param name="expectedPhase">期望的游戏阶段</param>
    /// <param name="operation">操作名称</param>
    /// <throws>InvalidGameStateException</throws>
    public static void ValidateGamePhase(GameManager.GamePhase expectedPhase, string operation)
    {
        if (GameManager.Instance == null)
        {
            throw new GameExceptions.InvalidGameStateException(GameManager.GamePhase.MainMenu, expectedPhase.ToString(), operation);
        }
        
        if (GameManager.Instance.CurrentPhase != expectedPhase)
        {
            throw new GameExceptions.InvalidGameStateException(GameManager.Instance.CurrentPhase, expectedPhase.ToString(), operation);
        }
    }
    
    /// <summary>
    /// 验证卡牌索引
    /// </summary>
    /// <param name="index">索引</param>
    /// <param name="maxIndex">最大索引</param>
    /// <param name="operation">操作名称</param>
    /// <throws>InvalidCardOperationException</throws>
    public static void ValidateCardIndex(int index, int maxIndex, string operation)
    {
        if (index < 0 || index >= maxIndex)
        {
            throw new GameExceptions.InvalidCardOperationException($"Index{index}", operation, $"索引超出范围 (0-{maxIndex - 1})");
        }
    }
    
    /// <summary>
    /// 验证对象非空
    /// </summary>
    /// <param name="obj">要验证的对象</param>
    /// <param name="objectName">对象名称</param>
    /// <param name="operation">操作名称</param>
    /// <throws>InvalidPlayerOperationException</throws>
    public static void ValidateNotNull(object obj, string objectName, string operation)
    {
        if (obj == null)
        {
            throw new GameExceptions.InvalidPlayerOperationException(objectName, $"{operation}: {objectName}为空");
        }
    }
}