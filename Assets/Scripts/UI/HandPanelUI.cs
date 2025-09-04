using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 手牌区UI控制，管理手牌的显示和交互
/// </summary>
public class HandPanelUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Transform cardContainer; // 卡牌容器
    [SerializeField] private GameObject cardUIPrefab; // 卡牌UI预制体
    [SerializeField] private ScrollRect scrollRect; // 滚动视图
    
    [Header("布局设置")]
    [SerializeField] private float cardSpacing = 10f;
    [SerializeField] private float cardWidth = 100f;
    [SerializeField] private float cardHeight = 140f;
    [SerializeField] private int maxVisibleCards = 7;
    
    [Header("动画设置")]
    [SerializeField] private bool enableCardAnimation = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("交互设置")]
    [SerializeField] private bool enableCardSelection = true;
    [SerializeField] private bool enableCardHover = true;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float selectedScale = 1.2f;
    
    [Header("视觉效果")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color disabledColor = Color.gray;
    
    // 当前手牌数据
    private List<CardData> currentHand = new List<CardData>();
    private List<GameObject> cardUIObjects = new List<GameObject>();
    private GameObject selectedCardUI = null;
    private int selectedCardIndex = -1;
    
    // 事件
    public System.Action<CardData, int> OnCardSelected;
    public System.Action<CardData, int> OnCardDeselected;
    public System.Action<CardData, int> OnCardHover;
    public System.Action<CardData, int> OnCardClick;
    public System.Action<CardData, int> OnCardDoubleClick;
    
    // 属性
    public List<CardData> CurrentHand => new List<CardData>(currentHand);
    public int HandCount => currentHand.Count;
    public CardData SelectedCard => selectedCardIndex >= 0 && selectedCardIndex < currentHand.Count ? currentHand[selectedCardIndex] : null;
    public int SelectedCardIndex => selectedCardIndex;
    
    private void Start()
    {
        InitializeHandPanel();
    }
    
    /// <summary>
    /// 初始化手牌面板
    /// </summary>
    private void InitializeHandPanel()
    {
        // 确保有卡牌容器
        if (cardContainer == null)
        {
            cardContainer = transform;
        }
        
        // 设置布局组件
        SetupLayoutGroup();
        
        // 清空初始内容
        ClearHand();
    }
    
    /// <summary>
    /// 设置布局组件
    /// </summary>
    private void SetupLayoutGroup()
    {
        if (cardContainer.GetComponent<HorizontalLayoutGroup>() == null)
        {
            HorizontalLayoutGroup layoutGroup = cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = cardSpacing;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }
        
        // 确保有Content Size Fitter
        if (cardContainer.GetComponent<ContentSizeFitter>() == null)
        {
            ContentSizeFitter sizeFitter = cardContainer.gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
    
    /// <summary>
    /// 更新手牌显示
    /// </summary>
    /// <param name="newHand">新的手牌数据</param>
    public void UpdateHand(List<CardData> newHand)
    {
        if (newHand == null)
        {
            newHand = new List<CardData>();
        }
        
        // 检查是否有变化
        if (IsHandSame(newHand))
        {
            return;
        }
        
        currentHand = new List<CardData>(newHand);
        
        if (enableCardAnimation)
        {
            StartCoroutine(AnimateHandUpdate());
        }
        else
        {
            UpdateHandImmediate();
        }
        
        Debug.Log($"手牌更新：{currentHand.Count} 张卡牌");
    }
    
    /// <summary>
    /// 检查手牌是否相同
    /// </summary>
    private bool IsHandSame(List<CardData> newHand)
    {
        if (currentHand.Count != newHand.Count)
        {
            return false;
        }
        
        for (int i = 0; i < currentHand.Count; i++)
        {
            if (currentHand[i] != newHand[i])
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 立即更新手牌
    /// </summary>
    private void UpdateHandImmediate()
    {
        ClearHand();
        CreateCardUIs();
        UpdateScrollView();
    }
    
    /// <summary>
    /// 动画更新手牌
    /// </summary>
    private IEnumerator AnimateHandUpdate()
    {
        // 淡出旧卡牌
        yield return StartCoroutine(FadeOutCards());
        
        // 清空并创建新卡牌
        ClearHand();
        CreateCardUIs();
        
        // 淡入新卡牌
        yield return StartCoroutine(FadeInCards());
        
        UpdateScrollView();
    }
    
    /// <summary>
    /// 淡出卡牌动画
    /// </summary>
    private IEnumerator FadeOutCards()
    {
        float elapsedTime = 0f;
        List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
        
        // 为每个卡牌添加CanvasGroup
        foreach (GameObject cardUI in cardUIObjects)
        {
            if (cardUI != null)
            {
                CanvasGroup canvasGroup = cardUI.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = cardUI.AddComponent<CanvasGroup>();
                }
                canvasGroups.Add(canvasGroup);
            }
        }
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 淡入卡牌动画
    /// </summary>
    private IEnumerator FadeInCards()
    {
        float elapsedTime = 0f;
        List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
        
        // 为每个新卡牌添加CanvasGroup并设置初始alpha
        foreach (GameObject cardUI in cardUIObjects)
        {
            if (cardUI != null)
            {
                CanvasGroup canvasGroup = cardUI.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = cardUI.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                canvasGroups.Add(canvasGroup);
            }
        }
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            float alpha = Mathf.Lerp(0f, 1f, progress);
            
            foreach (CanvasGroup canvasGroup in canvasGroups)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 创建卡牌UI对象
    /// </summary>
    private void CreateCardUIs()
    {
        for (int i = 0; i < currentHand.Count; i++)
        {
            CardData cardData = currentHand[i];
            GameObject cardUI = CreateCardUI(cardData, i);
            cardUIObjects.Add(cardUI);
        }
    }
    
    /// <summary>
    /// 创建单个卡牌UI
    /// </summary>
    /// <param name="cardData">卡牌数据</param>
    /// <param name="index">索引</param>
    /// <returns>卡牌UI对象</returns>
    private GameObject CreateCardUI(CardData cardData, int index)
    {
        GameObject cardUI;
        
        if (cardUIPrefab != null)
        {
            cardUI = Instantiate(cardUIPrefab, cardContainer);
        }
        else
        {
            // 创建简单的卡牌UI
            cardUI = CreateSimpleCardUI(cardData);
        }
        
        // 设置卡牌数据
        CardUI cardUIComponent = cardUI.GetComponent<CardUI>();
        if (cardUIComponent == null)
        {
            cardUIComponent = cardUI.AddComponent<CardUI>();
        }
        
        cardUIComponent.SetCardData(cardData);
        cardUIComponent.SetIndex(index);
        
        // 设置交互事件
        SetupCardInteraction(cardUI, cardData, index);
        
        // 设置尺寸
        RectTransform rectTransform = cardUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(cardWidth, cardHeight);
        }
        
        return cardUI;
    }
    
    /// <summary>
    /// 创建简单的卡牌UI
    /// </summary>
    private GameObject CreateSimpleCardUI(CardData cardData)
    {
        GameObject cardUI = new GameObject($"Card_{cardData.CardName}");
        cardUI.transform.SetParent(cardContainer);
        
        // 添加Image组件
        Image cardImage = cardUI.AddComponent<Image>();
        cardImage.sprite = cardData.CardSprite;
        cardImage.color = cardData.CardColor;
        
        // 添加按钮组件
        Button cardButton = cardUI.AddComponent<Button>();
        
        // 添加文本显示卡牌名称
        GameObject textObj = new GameObject("CardName");
        textObj.transform.SetParent(cardUI.transform);
        
        Text cardText = textObj.AddComponent<Text>();
        cardText.text = cardData.CardName;
        cardText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cardText.fontSize = 12;
        cardText.color = Color.black;
        cardText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return cardUI;
    }
    
    /// <summary>
    /// 设置卡牌交互
    /// </summary>
    private void SetupCardInteraction(GameObject cardUI, CardData cardData, int index)
    {
        Button button = cardUI.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnCardClicked(cardData, index));
        }
        
        // 添加悬停效果
        if (enableCardHover)
        {
            CardHoverEffect hoverEffect = cardUI.GetComponent<CardHoverEffect>();
            if (hoverEffect == null)
            {
                hoverEffect = cardUI.AddComponent<CardHoverEffect>();
            }
            
            hoverEffect.SetHoverScale(hoverScale);
            hoverEffect.OnCardHover += () => OnCardHover?.Invoke(cardData, index);
        }
    }
    
    /// <summary>
    /// 卡牌点击处理
    /// </summary>
    private void OnCardClicked(CardData cardData, int index)
    {
        OnCardClick?.Invoke(cardData, index);
        
        if (enableCardSelection)
        {
            if (selectedCardIndex == index)
            {
                // 取消选择
                DeselectCard();
            }
            else
            {
                // 选择新卡牌
                SelectCard(index);
            }
        }
    }
    
    /// <summary>
    /// 选择卡牌
    /// </summary>
    /// <param name="index">卡牌索引</param>
    public void SelectCard(int index)
    {
        if (index < 0 || index >= cardUIObjects.Count)
        {
            return;
        }
        
        // 取消之前的选择
        DeselectCard();
        
        selectedCardIndex = index;
        selectedCardUI = cardUIObjects[index];
        
        // 更新视觉效果
        UpdateCardVisual(selectedCardUI, selectedColor, selectedScale);
        
        OnCardSelected?.Invoke(currentHand[index], index);
        
        Debug.Log($"选择了卡牌：{currentHand[index].CardName}");
    }
    
    /// <summary>
    /// 取消选择卡牌
    /// </summary>
    public void DeselectCard()
    {
        if (selectedCardUI != null && selectedCardIndex >= 0)
        {
            UpdateCardVisual(selectedCardUI, normalColor, 1f);
            
            OnCardDeselected?.Invoke(currentHand[selectedCardIndex], selectedCardIndex);
            
            selectedCardUI = null;
            selectedCardIndex = -1;
        }
    }
    
    /// <summary>
    /// 更新卡牌视觉效果
    /// </summary>
    private void UpdateCardVisual(GameObject cardUI, Color color, float scale)
    {
        if (cardUI == null) return;
        
        Image cardImage = cardUI.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.color = color;
        }
        
        cardUI.transform.localScale = Vector3.one * scale;
    }
    
    /// <summary>
    /// 清空手牌
    /// </summary>
    public void ClearHand()
    {
        foreach (GameObject cardUI in cardUIObjects)
        {
            if (cardUI != null)
            {
                DestroyImmediate(cardUI);
            }
        }
        
        cardUIObjects.Clear();
        selectedCardUI = null;
        selectedCardIndex = -1;
    }
    
    /// <summary>
    /// 更新滚动视图
    /// </summary>
    private void UpdateScrollView()
    {
        if (scrollRect != null && cardUIObjects.Count > maxVisibleCards)
        {
            scrollRect.enabled = true;
        }
        else if (scrollRect != null)
        {
            scrollRect.enabled = false;
        }
    }
    
    /// <summary>
    /// 设置卡牌可用状态
    /// </summary>
    /// <param name="availableResources">可用资源</param>
    public void UpdateCardAvailability(int availableResources)
    {
        for (int i = 0; i < cardUIObjects.Count && i < currentHand.Count; i++)
        {
            bool canPlay = currentHand[i].CanPlay(availableResources);
            Color targetColor = canPlay ? normalColor : disabledColor;
            
            if (i == selectedCardIndex)
            {
                targetColor = selectedColor;
            }
            
            UpdateCardVisual(cardUIObjects[i], targetColor, 1f);
            
            // 更新按钮可交互性
            Button button = cardUIObjects[i].GetComponent<Button>();
            if (button != null)
            {
                button.interactable = canPlay;
            }
        }
    }
    
    /// <summary>
    /// 滚动到指定卡牌
    /// </summary>
    /// <param name="index">卡牌索引</param>
    public void ScrollToCard(int index)
    {
        if (scrollRect != null && index >= 0 && index < cardUIObjects.Count)
        {
            float targetPosition = (float)index / (cardUIObjects.Count - 1);
            scrollRect.horizontalNormalizedPosition = targetPosition;
        }
    }
    
    /// <summary>
    /// 设置手牌面板可见性
    /// </summary>
    /// <param name="visible">是否可见</param>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}