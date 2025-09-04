using UnityEngine;

/// <summary>
/// 数字卡牌类，表示1-9的手牌
/// </summary>
[System.Serializable]
public class NumberCard
{
    [Header("卡牌基本信息")]
    [SerializeField] private int originalValue; // 原始数值(1-9)
    [SerializeField] private int currentValue;  // 当前数值(经过加减后)
    [SerializeField] private string cardId;     // 卡牌ID(A-I)
    [SerializeField] private PlayerTeam owner;  // 卡牌拥有者
    
    [Header("状态信息")]
    [SerializeField] private bool isRevealed = false;    // 是否已被揭开
    [SerializeField] private bool isPlayed = false;      // 是否已出牌
    [SerializeField] private bool isDestroyed = false;   // 是否已销毁
    
    // 玩家队伍枚举
    public enum PlayerTeam
    {
        Red,    // 红队
        Blue    // 蓝队
    }
    
    // 属性访问器
    public int OriginalValue => originalValue;
    public int CurrentValue => currentValue;
    public string CardId => cardId;
    public PlayerTeam Owner => owner;
    public bool IsRevealed => isRevealed;
    public bool IsPlayed => isPlayed;
    public bool IsDestroyed => isDestroyed;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value">卡牌数值(1-9)</param>
    /// <param name="id">卡牌ID(A-I)</param>
    /// <param name="team">拥有者队伍</param>
    public NumberCard(int value, string id, PlayerTeam team)
    {
        originalValue = Mathf.Clamp(value, 1, 9);
        currentValue = originalValue;
        cardId = id;
        owner = team;
        isRevealed = false;
        isPlayed = false;
        isDestroyed = false;
    }
    
    /// <summary>
    /// 应用加法修饰符
    /// </summary>
    /// <param name="amount">增加的数值</param>
    public void ApplyAddition(int amount = 1)
    {
        currentValue += amount;
        Debug.Log($"卡牌 {cardId} 增加 {amount}，当前数值: {currentValue}");
    }
    
    /// <summary>
    /// 应用减法修饰符
    /// </summary>
    /// <param name="amount">减少的数值</param>
    public void ApplySubtraction(int amount = 1)
    {
        // 确保数值不会小于0
        currentValue = Mathf.Max(0, currentValue - amount);
        Debug.Log($"卡牌 {cardId} 减少 {amount}，当前数值: {currentValue}");
    }
    
    /// <summary>
    /// 重置卡牌到原始数值
    /// </summary>
    public void ResetToOriginal()
    {
        currentValue = originalValue;
        Debug.Log($"卡牌 {cardId} 重置为原始数值: {originalValue}");
    }
    
    /// <summary>
    /// 揭开卡牌
    /// </summary>
    public void Reveal()
    {
        if (!isRevealed)
        {
            isRevealed = true;
            Debug.Log($"卡牌 {cardId} 被揭开，数值: {currentValue}");
        }
    }
    
    /// <summary>
    /// 出牌
    /// </summary>
    public void Play()
    {
        if (!isPlayed && !isDestroyed)
        {
            isPlayed = true;
            Debug.Log($"玩家 {owner} 出牌 {cardId}，数值: {currentValue}");
        }
    }
    
    /// <summary>
    /// 销毁卡牌
    /// </summary>
    public void Destroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            Debug.Log($"卡牌 {cardId} 被销毁");
        }
    }
    
    /// <summary>
    /// 检查卡牌是否可以出牌
    /// </summary>
    /// <returns>是否可以出牌</returns>
    public bool CanPlay()
    {
        return !isPlayed && !isDestroyed;
    }
    
    /// <summary>
    /// 比较两张卡牌的大小
    /// </summary>
    /// <param name="otherCard">另一张卡牌</param>
    /// <returns>比较结果：1=胜利，0=平局，-1=失败</returns>
    public int CompareTo(NumberCard otherCard)
    {
        if (otherCard == null) return 1;
        
        if (currentValue > otherCard.currentValue)
            return 1;
        else if (currentValue < otherCard.currentValue)
            return -1;
        else
            return 0;
    }
    
    /// <summary>
    /// 获取卡牌的显示颜色
    /// </summary>
    /// <returns>队伍对应的颜色</returns>
    public Color GetTeamColor()
    {
        switch (owner)
        {
            case PlayerTeam.Red:
                return Color.red;
            case PlayerTeam.Blue:
                return Color.blue;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 创建卡牌的副本
    /// </summary>
    /// <returns>卡牌副本</returns>
    public NumberCard CreateCopy()
    {
        NumberCard copy = new NumberCard(originalValue, cardId, owner);
        copy.currentValue = this.currentValue;
        copy.isRevealed = this.isRevealed;
        copy.isPlayed = this.isPlayed;
        copy.isDestroyed = this.isDestroyed;
        return copy;
    }
    
    /// <summary>
    /// 获取卡牌状态描述
    /// </summary>
    /// <returns>状态字符串</returns>
    public string GetStatusDescription()
    {
        string status = $"卡牌 {cardId} ({owner}队)";
        status += $" - 原值: {originalValue}, 当前值: {currentValue}";
        
        if (isDestroyed)
            status += " [已销毁]";
        else if (isPlayed)
            status += " [已出牌]";
        else if (isRevealed)
            status += " [已揭开]";
        else
            status += " [未揭开]";
            
        return status;
    }
    
    /// <summary>
    /// 重写ToString方法
    /// </summary>
    /// <returns>卡牌信息字符串</returns>
    public override string ToString()
    {
        return $"{cardId}({currentValue})";
    }
}