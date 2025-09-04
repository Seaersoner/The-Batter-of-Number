using UnityEngine;

/// <summary>
/// 加减牌类，用于修改数字卡牌的数值
/// </summary>
[System.Serializable]
public class ModifierCard
{
    [Header("修饰符信息")]
    [SerializeField] private ModifierType type;     // 修饰符类型(加/减)
    [SerializeField] private int value;             // 修饰符数值(通常为1)
    [SerializeField] private string targetCardId;   // 目标卡牌ID
    [SerializeField] private NumberCard.PlayerTeam owner; // 拥有者
    
    [Header("状态信息")]
    [SerializeField] private bool isUsed = false;   // 是否已使用
    [SerializeField] private int roundPlaced = 0;   // 放置的回合数
    
    // 修饰符类型枚举
    public enum ModifierType
    {
        Add,        // 加法
        Subtract    // 减法
    }
    
    // 属性访问器
    public ModifierType Type => type;
    public int Value => value;
    public string TargetCardId => targetCardId;
    public NumberCard.PlayerTeam Owner => owner;
    public bool IsUsed => isUsed;
    public int RoundPlaced => roundPlaced;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="modifierType">修饰符类型</param>
    /// <param name="modifierValue">修饰符数值</param>
    /// <param name="target">目标卡牌ID</param>
    /// <param name="team">拥有者队伍</param>
    /// <param name="round">放置回合</param>
    public ModifierCard(ModifierType modifierType, int modifierValue, string target, NumberCard.PlayerTeam team, int round)
    {
        type = modifierType;
        value = Mathf.Abs(modifierValue); // 确保数值为正
        targetCardId = target;
        owner = team;
        roundPlaced = round;
        isUsed = false;
    }
    
    /// <summary>
    /// 默认构造函数（数值为1）
    /// </summary>
    /// <param name="modifierType">修饰符类型</param>
    /// <param name="target">目标卡牌ID</param>
    /// <param name="team">拥有者队伍</param>
    /// <param name="round">放置回合</param>
    public ModifierCard(ModifierType modifierType, string target, NumberCard.PlayerTeam team, int round)
        : this(modifierType, 1, target, team, round)
    {
    }
    
    /// <summary>
    /// 应用修饰符到目标卡牌
    /// </summary>
    /// <param name="targetCard">目标卡牌</param>
    /// <returns>是否成功应用</returns>
    public bool ApplyToCard(NumberCard targetCard)
    {
        if (targetCard == null || isUsed)
        {
            Debug.LogWarning("无法应用修饰符：目标卡牌为空或修饰符已使用");
            return false;
        }
        
        if (targetCard.CardId != targetCardId)
        {
            Debug.LogWarning($"目标卡牌ID不匹配：期望 {targetCardId}，实际 {targetCard.CardId}");
            return false;
        }
        
        // 应用修饰符效果
        switch (type)
        {
            case ModifierType.Add:
                targetCard.ApplyAddition(value);
                break;
            case ModifierType.Subtract:
                targetCard.ApplySubtraction(value);
                break;
        }
        
        isUsed = true;
        Debug.Log($"{owner}队的{GetTypeString()}修饰符({value})应用到卡牌{targetCardId}");
        return true;
    }
    
    /// <summary>
    /// 获取修饰符类型的字符串表示
    /// </summary>
    /// <returns>类型字符串</returns>
    public string GetTypeString()
    {
        switch (type)
        {
            case ModifierType.Add:
                return "加法";
            case ModifierType.Subtract:
                return "减法";
            default:
                return "未知";
        }
    }
    
    /// <summary>
    /// 获取修饰符的符号表示
    /// </summary>
    /// <returns>符号字符串</returns>
    public string GetSymbol()
    {
        switch (type)
        {
            case ModifierType.Add:
                return $"+{value}";
            case ModifierType.Subtract:
                return $"-{value}";
            default:
                return "?";
        }
    }
    
    /// <summary>
    /// 获取修饰符的颜色
    /// </summary>
    /// <returns>颜色</returns>
    public Color GetColor()
    {
        switch (type)
        {
            case ModifierType.Add:
                return Color.green;
            case ModifierType.Subtract:
                return new Color(1f, 0.5f, 0f); // 橙色
            default:
                return Color.gray;
        }
    }
    
    /// <summary>
    /// 检查是否可以叠加到指定位置
    /// </summary>
    /// <param name="cardId">目标卡牌ID</param>
    /// <param name="existingModifiers">已存在的修饰符列表</param>
    /// <returns>是否可以叠加</returns>
    public static bool CanStackOn(string cardId, System.Collections.Generic.List<ModifierCard> existingModifiers)
    {
        // 游戏规则：可以无限叠加加减牌
        return true;
    }
    
    /// <summary>
    /// 计算指定卡牌位置的总修饰符效果
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <param name="modifiers">修饰符列表</param>
    /// <returns>总效果值</returns>
    public static int CalculateTotalEffect(string cardId, System.Collections.Generic.List<ModifierCard> modifiers)
    {
        int totalEffect = 0;
        
        foreach (ModifierCard modifier in modifiers)
        {
            if (modifier.targetCardId == cardId && !modifier.isUsed)
            {
                switch (modifier.type)
                {
                    case ModifierType.Add:
                        totalEffect += modifier.value;
                        break;
                    case ModifierType.Subtract:
                        totalEffect -= modifier.value;
                        break;
                }
            }
        }
        
        return totalEffect;
    }
    
    /// <summary>
    /// 回收修饰符（标记为未使用，可以重新使用）
    /// </summary>
    public void Recycle()
    {
        isUsed = false;
        Debug.Log($"{owner}队的{GetTypeString()}修饰符被回收");
    }
    
    /// <summary>
    /// 创建修饰符的副本
    /// </summary>
    /// <returns>修饰符副本</returns>
    public ModifierCard CreateCopy()
    {
        ModifierCard copy = new ModifierCard(type, value, targetCardId, owner, roundPlaced);
        copy.isUsed = this.isUsed;
        return copy;
    }
    
    /// <summary>
    /// 获取修饰符的详细描述
    /// </summary>
    /// <returns>描述字符串</returns>
    public string GetDescription()
    {
        string description = $"{owner}队的{GetTypeString()}修饰符";
        description += $" - 目标: {targetCardId}, 数值: {GetSymbol()}";
        description += $", 回合: {roundPlaced}";
        
        if (isUsed)
            description += " [已使用]";
        else
            description += " [未使用]";
            
        return description;
    }
    
    /// <summary>
    /// 重写ToString方法
    /// </summary>
    /// <returns>修饰符信息字符串</returns>
    public override string ToString()
    {
        return $"{GetSymbol()}→{targetCardId}";
    }
}