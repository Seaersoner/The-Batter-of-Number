# 🎮 数字博弈游戏完整使用指南

## 📋 **目录**
1. [项目导入和设置](#项目导入和设置)
2. [必需的UI组件](#必需的ui组件)
3. [预制体创建](#预制体创建)
4. [场景设置](#场景设置)
5. [脚本配置](#脚本配置)
6. [测试和运行](#测试和运行)
7. [常见问题解决](#常见问题解决)

---

## 🚀 **1. 项目导入和设置**

### **步骤1.1: 创建Unity项目**
1. 打开Unity Hub
2. 创建新项目 → 选择 **2D模板**
3. 项目名称: `The-Batter-of-Number`
4. 选择合适的位置并创建

### **步骤1.2: 导入代码**
1. 将所有 `Assets/Scripts/` 文件夹复制到Unity项目的Assets目录下
2. 在Unity中等待脚本编译完成
3. 检查Console窗口，确保没有编译错误

### **步骤1.3: 创建Resources目录**
```
Assets/
├── Resources/           # 创建这个文件夹
│   ├── Sprites/        # 存放精灵图片
│   ├── Audio/          # 存放音效文件
│   └── Materials/      # 存放材质文件
```

---

## 🎨 **2. 必需的UI组件**

### **步骤2.1: 创建主Canvas**
1. 右键 Hierarchy → UI → Canvas
2. 重命名为 `MainCanvas`
3. Canvas设置：
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

### **步骤2.2: 创建UI面板结构**
在MainCanvas下创建以下UI结构：

```
MainCanvas
├── MainMenuPanel          # 主菜单面板
│   ├── Background         # 背景图片
│   ├── TitleText         # 游戏标题
│   ├── StartButton       # 开始游戏按钮
│   └── QuitButton        # 退出游戏按钮
├── GameplayPanel         # 游戏界面面板
│   ├── TopUI             # 顶部UI
│   │   ├── TimerUI       # 计时器UI
│   │   ├── PhaseText     # 当前阶段显示
│   │   └── ScoreUI       # 分数显示
│   ├── GameBoard         # 游戏棋盘区域
│   ├── HandPanel         # 手牌面板
│   └── ControlButtons    # 控制按钮
│       ├── EndTurnButton # 结束回合按钮
│       └── PauseButton   # 暂停按钮
└── GameOverPanel         # 游戏结束面板
    ├── ResultText        # 结果显示
    ├── RestartButton     # 重新开始按钮
    └── MainMenuButton    # 返回主菜单按钮
```

### **步骤2.3: 创建具体UI组件**

#### **TimerUI组件**:
1. 创建空GameObject → 命名为 `TimerUI`
2. 添加组件：
   - `TimerUI` 脚本
   - `Image` 组件（计时器背景）
   - 子对象 `TimerText` (Text组件)
   - 子对象 `TimerFill` (Image组件，用于填充效果)

#### **HandPanelUI组件**:
1. 创建空GameObject → 命名为 `HandPanelUI`
2. 添加组件：
   - `HandPanelUI` 脚本
   - `Horizontal Layout Group`
   - `Content Size Fitter`
3. 子对象 `CardContainer` 用于放置卡牌

#### **ScoreUI组件**:
1. 创建UI → Text → 命名为 `ScoreText`
2. 设置文本格式：
   - Font Size: 24
   - Color: White
   - Alignment: Center

---

## 🎯 **3. 预制体创建**

### **步骤3.1: 创建卡牌预制体**

#### **创建Card预制体**:
1. 创建空GameObject → 命名为 `Card_Prefab`
2. 添加组件：
   - `SpriteRenderer` (显示卡牌图片)
   - `BoxCollider2D` (点击检测)
   - `Draggable` 脚本 (拖拽功能)
3. 子对象结构：
```
Card_Prefab
├── Background (SpriteRenderer)    # 卡牌背景
├── CardText (TextMesh)           # 卡牌数字显示
└── EffectArea (SpriteRenderer)   # 特效区域
```
4. 保存为预制体：拖拽到 `Assets/Art/Prefabs/` 文件夹

#### **创建CardSlot预制体**:
1. 创建空GameObject → 命名为 `CardSlot_Prefab`
2. 添加组件：
   - `SpriteRenderer` (槽位背景)
   - `BoxCollider2D` (放置检测)
   - `CardSlot` 脚本
3. 设置Collider尺寸：100x140 (与卡牌尺寸匹配)
4. 保存为预制体

### **步骤3.2: 创建特效预制体**

#### **创建粒子特效**:
1. GameObject → Effects → Particle System
2. 命名为 `CardPlaceEffect`
3. 配置粒子参数：
   - Duration: 0.5
   - Start Lifetime: 1.0
   - Start Speed: 2
   - Start Color: Yellow
4. 保存为预制体

---

## 🏗️ **4. 场景设置**

### **步骤4.1: 创建主游戏场景**

#### **场景基本设置**:
1. File → New Scene → 2D
2. 保存为 `MainScene`
3. 删除默认的Main Camera标签，保留相机

#### **创建核心管理器**:
1. 创建空GameObject → 命名为 `GameManager`
2. 添加脚本：
   - `GameManager`
   - `GamePhaseManager`
   - `ResourceManager`
   - `PerformanceMonitor`

3. 创建空GameObject → 命名为 `UIManager`
4. 添加脚本：
   - `UIManager`

5. 创建空GameObject → 命名为 `AudioManager`
6. 添加脚本：
   - `AudioManager`
   - 添加两个子对象：
     - `MusicSource` (AudioSource组件)
     - `SFXSource` (AudioSource组件)

### **步骤4.2: 创建游戏棋盘**

#### **使用GameBoardLayout自动创建**:
1. 创建空GameObject → 命名为 `GameBoard`
2. 添加 `GameBoardLayout` 脚本
3. 设置参数：
   - Card Spacing: 120
   - Row Spacing: 150
   - Card Size: (100, 140)
4. 运行游戏，脚本会自动创建所有槽位

#### **手动创建（可选）**:
如果需要手动控制，可以按以下结构创建：
```
GameBoard
├── HumanHandArea      # 人类手牌区 (Y: 0)
│   ├── Slot_A
│   ├── Slot_B
│   └── ... (共9个槽位)
├── HumanModifierArea  # 人类加减牌区 (Y: 150)
├── BattleArea         # 比牌区 (Y: 300)
├── AIModifierArea     # AI加减牌区 (Y: 450)
├── AIHandArea         # AI手牌区 (Y: 600)
└── DestroyedArea      # 销毁区 (X: 1200)
```

### **步骤4.3: 创建玩家对象**

#### **人类玩家**:
1. 创建空GameObject → 命名为 `HumanPlayer`
2. 添加 `NumberPlayer` 脚本
3. 设置参数：
   - Player Name: "Human Player"
   - Team: Red
   - Is AI: false

#### **AI玩家**:
1. 创建空GameObject → 命名为 `AIPlayer`
2. 添加 `NumberPlayer` 脚本
3. 设置参数：
   - Player Name: "AI Player"
   - Team: Blue
   - Is AI: true

---

## ⚙️ **5. 脚本配置**

### **步骤5.1: 配置GameManager**
在GameManager的Inspector中设置：
```
Game Settings:
├── Phase Time Limit: 60
├── Win Condition Score: 5
Players:
├── Human Player: 拖拽HumanPlayer对象
├── AI Player: 拖拽AIPlayer对象
Game Areas:
├── Hand Card Slots: 拖拽A-I槽位数组
├── Modifier Card Area: 拖拽加减牌区域
├── Battle Area: 拖拽比牌区域
└── Destroyed Area: 拖拽销毁区域
```

### **步骤5.2: 配置UIManager**
在UIManager的Inspector中设置：
```
Main UI Panels:
├── Main Menu Panel: 拖拽主菜单面板
├── Gameplay Panel: 拖拽游戏界面面板
├── Game Over Panel: 拖拽游戏结束面板
└── Pause Panel: 拖拽暂停面板

Gameplay UI:
├── Timer UI: 拖拽TimerUI对象
├── Hand Panel UI: 拖拽HandPanelUI对象
├── Current Player Text: 拖拽显示当前玩家的Text
└── End Turn Button: 拖拽结束回合按钮

Game Over UI:
├── Game Over Text: 拖拽结果显示Text
├── Restart Button: 拖拽重新开始按钮
└── Main Menu Button: 拖拽返回主菜单按钮
```

### **步骤5.3: 配置AudioManager**
```
Audio Sources:
├── Music Source: 拖拽MusicSource对象
└── SFX Source: 拖拽SFXSource对象

Background Music:
└── Background Music: 拖拽背景音乐AudioClip数组

Sound Effects:
├── Card Place Sound: 放置卡牌音效
├── Card Draw Sound: 抽卡音效
├── Button Click Sound: 按钮点击音效
├── Game Over Sound: 游戏结束音效
├── Victory Sound: 胜利音效
└── Timer Warning Sound: 计时器警告音效

Volume Settings:
├── Master Volume: 1.0
├── Music Volume: 0.7
└── SFX Volume: 1.0
```

### **步骤5.4: 配置ResourceManager**
```
Default Resources:
├── Default Card Sprite: 默认卡牌精灵
├── Default Slot Sprite: 默认槽位精灵
├── Default Sound Effect: 默认音效
└── Default Card Material: 默认卡牌材质

Procedural Generation Settings:
├── Enable Procedural Generation: true
└── Default Card Colors: 设置9种颜色数组
```

---

## 🧪 **6. 测试和运行**

### **步骤6.1: 基础功能测试**
1. **资源检查测试**:
   - 运行游戏
   - 查看Console输出，确认ResourceManager正常工作
   - 确认所有精灵和音效都能正常生成或加载

2. **UI功能测试**:
   - 测试主菜单按钮
   - 测试游戏界面切换
   - 测试计时器显示

3. **游戏逻辑测试**:
   - 测试游戏阶段切换
   - 测试AI决策功能
   - 测试卡牌放置和比牌逻辑

### **步骤6.2: 完整游戏流程测试**
1. 启动游戏 → 主菜单显示
2. 点击"开始游戏" → 进入摆牌阶段
3. 自动进入加减牌阶段 → AI自动放置
4. 进入揭牌阶段 → 双方互相揭牌
5. 进入出牌阶段 → 选择卡牌出牌
6. 进入比牌阶段 → 比较大小
7. 重复步骤3-6直到游戏结束

### **步骤6.3: 边界条件测试**
1. 测试没有可用卡牌的情况
2. 测试AI决策边界情况
3. 测试资源缺失的情况
4. 测试快速点击操作

---

## 🔧 **7. 常见问题解决**

### **问题1: 编译错误**
**症状**: Console显示脚本编译错误
**解决方案**:
1. 检查所有脚本是否正确放置在Scripts目录下
2. 确保没有缺失的依赖脚本
3. 重新导入脚本：右键Scripts文件夹 → Reimport

### **问题2: UI不显示**
**症状**: 游戏运行但UI界面空白
**解决方案**:
1. 检查Canvas设置是否正确
2. 确认UI组件都添加了正确的脚本
3. 检查UIManager中的引用是否正确设置

### **问题3: 卡牌不显示**
**症状**: 游戏运行但看不到卡牌
**解决方案**:
1. 检查ResourceManager是否正常工作
2. 确认程序化生成功能已启用
3. 查看Console是否有资源生成的日志

### **问题4: AI不工作**
**症状**: AI玩家不进行任何操作
**解决方案**:
1. 检查AIPlayer的Is AI选项是否为true
2. 确认GamePhaseManager正确引用了AI玩家
3. 查看Console是否有AI决策的错误日志

### **问题5: 音效不播放**
**症状**: 游戏静音或音效缺失
**解决方案**:
1. 检查AudioManager配置
2. 确认音频源组件正确设置
3. 验证ResourceManager的音效生成功能

### **问题6: 游戏卡住不动**
**症状**: 游戏在某个阶段停止响应
**解决方案**:
1. 检查GamePhaseManager的阶段切换逻辑
2. 查看Console是否有错误或警告
3. 确认所有必需的组件都已正确配置

---

## 📝 **快速检查清单**

### **场景设置检查**:
- [ ] MainCanvas已创建并正确配置
- [ ] 所有UI面板都已创建
- [ ] GameManager等核心管理器已添加
- [ ] 玩家对象已创建并配置
- [ ] 游戏棋盘已设置

### **脚本配置检查**:
- [ ] GameManager引用已正确设置
- [ ] UIManager引用已正确设置
- [ ] AudioManager音频源已配置
- [ ] ResourceManager已启用程序化生成
- [ ] 所有按钮事件已连接

### **功能测试检查**:
- [ ] 游戏可以正常启动
- [ ] 主菜单功能正常
- [ ] 游戏阶段可以正常切换
- [ ] AI决策功能正常
- [ ] 音效播放正常
- [ ] 资源生成功能正常

---

## 🎯 **总结**

按照这个指南，您应该能够：
1. ✅ 正确设置Unity项目和导入代码
2. ✅ 创建完整的UI界面和预制体
3. ✅ 配置所有必需的场景组件
4. ✅ 正确设置脚本参数和引用
5. ✅ 成功运行和测试游戏
6. ✅ 解决常见的配置问题

**如果遇到任何问题，请检查Console窗口的错误信息，并参考常见问题解决部分。**

**游戏现在已经可以完整运行了！** 🚀