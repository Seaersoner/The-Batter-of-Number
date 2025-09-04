using UnityEngine;

/// <summary>
/// 卡牌数据定义，使用ScriptableObject存储卡牌信息
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本信息")]
    [SerializeField] private int cardId;
    [SerializeField] private string cardName;
    [TextArea(2, 4)]
    [SerializeField] private string description;
    
    [Header("视觉效果")]
    [SerializeField] private Sprite cardSprite;
    [SerializeField] private Color cardColor = Color.white;
    
    [Header("游戏属性")]
    [SerializeField] private int cost = 1;
    [SerializeField] private int attack = 1;
    [SerializeField] private int defense = 1;
    [SerializeField] private CardType cardType = CardType.Normal;
    [SerializeField] private CardRarity rarity = CardRarity.Common;
    
    [Header("特殊效果")]
    [SerializeField] private bool hasSpecialEffect = false;
    [TextArea(1, 3)]
    [SerializeField] private string specialEffectDescription;
    
    // 卡牌类型枚举
    public enum CardType
    {
        Normal,     // 普通卡牌
        Attack,     // 攻击卡牌
        Defense,    // 防御卡牌
        Special,    // 特殊卡牌
        Magic       // 魔法卡牌
    }
    
    // 卡牌稀有度枚举
    public enum CardRarity
    {
        Common,     // 普通
        Uncommon,   // 不常见
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
    
    // 属性访问器
    public int CardId => cardId;
    public string CardName => cardName;
    public string Description => description;
    public Sprite CardSprite => cardSprite;
    public Color CardColor => cardColor;
    public int Cost => cost;
    public int Attack => attack;
    public int Defense => defense;
    public CardType Type => cardType;
    public CardRarity Rarity => rarity;
    public bool HasSpecialEffect => hasSpecialEffect;
    public string SpecialEffectDescription => specialEffectDescription;
    
    /// <summary>
    /// 获取稀有度对应的颜色
    /// </summary>
    /// <returns>稀有度颜色</returns>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case CardRarity.Common:
                return Color.white;
            case CardRarity.Uncommon:
                return Color.green;
            case CardRarity.Rare:
                return Color.blue;
            case CardRarity.Epic:
                return Color.magenta;
            case CardRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 获取卡牌总价值（用于AI评估等）
    /// </summary>
    /// <returns>卡牌总价值</returns>
    public int GetTotalValue()
    {
        int baseValue = attack + defense;
        
        // 根据类型调整价值
        switch (cardType)
        {
            case CardType.Attack:
                baseValue += attack / 2;
                break;
            case CardType.Defense:
                baseValue += defense / 2;
                break;
            case CardType.Special:
            case CardType.Magic:
                baseValue += hasSpecialEffect ? 2 : 1;
                break;
        }
        
        // 根据稀有度调整价值
        switch (rarity)
        {
            case CardRarity.Uncommon:
                baseValue = Mathf.RoundToInt(baseValue * 1.2f);
                break;
            case CardRarity.Rare:
                baseValue = Mathf.RoundToInt(baseValue * 1.5f);
                break;
            case CardRarity.Epic:
                baseValue = Mathf.RoundToInt(baseValue * 2f);
                break;
            case CardRarity.Legendary:
                baseValue = Mathf.RoundToInt(baseValue * 3f);
                break;
        }
        
        return baseValue;
    }
    
    /// <summary>
    /// 检查卡牌是否可以使用
    /// </summary>
    /// <param name="availableResources">可用资源</param>
    /// <returns>是否可以使用</returns>
    public bool CanPlay(int availableResources)
    {
        return availableResources >= cost;
    }
    
    /// <summary>
    /// 创建卡牌的副本（用于实例化）
    /// </summary>
    /// <returns>卡牌数据副本</returns>
    public CardData CreateCopy()
    {
        CardData copy = CreateInstance<CardData>();
        copy.cardId = this.cardId;
        copy.cardName = this.cardName;
        copy.description = this.description;
        copy.cardSprite = this.cardSprite;
        copy.cardColor = this.cardColor;
        copy.cost = this.cost;
        copy.attack = this.attack;
        copy.defense = this.defense;
        copy.cardType = this.cardType;
        copy.rarity = this.rarity;
        copy.hasSpecialEffect = this.hasSpecialEffect;
        copy.specialEffectDescription = this.specialEffectDescription;
        
        return copy;
    }
    
    /// <summary>
    /// 获取卡牌的调试信息
    /// </summary>
    /// <returns>调试信息字符串</returns>
    public override string ToString()
    {
        return $"Card: {cardName} (ID: {cardId}) - Type: {cardType}, Cost: {cost}, ATK: {attack}, DEF: {defense}, Rarity: {rarity}";
    }
}