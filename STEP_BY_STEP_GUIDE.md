# 🎯 数字博弈游戏 - 分步使用指导

## 🚀 **快速开始（推荐新手）**

### **方法1: 使用自动设置助手**

1. **导入项目**:
   - 将整个 `Assets` 文件夹复制到Unity项目中
   - 等待脚本编译完成

2. **一键设置**:
   - 在Hierarchy中创建空GameObject
   - 添加 `GameSetupHelper` 脚本
   - 在Inspector中点击 `Setup Complete Game Scene`
   - 等待自动创建完成

3. **配置参数**:
   - 选择 `GameManager` 对象
   - 在Inspector中设置玩家引用
   - 配置其他管理器参数

4. **运行测试**:
   - 按播放按钮
   - 游戏应该能正常启动

---

## 🔧 **手动设置（完全控制）**

### **第一步: 创建场景基础结构**

#### **1.1 创建主Canvas**
```
Hierarchy右键 → UI → Canvas
重命名为: MainCanvas
设置:
- Render Mode: Screen Space - Overlay
- UI Scale Mode: Scale With Screen Size  
- Reference Resolution: 1920 x 1080
```

#### **1.2 创建EventSystem** (如果没有)
```
Hierarchy右键 → UI → Event System
保持默认设置
```

#### **1.3 设置主相机**
```
选择Main Camera:
- Position: (0, 300, -10)
- Orthographic Size: 600
- Background: 深蓝色 (0.1, 0.1, 0.3)
```

### **第二步: 创建核心管理器**

#### **2.1 创建GameManager**
```
Hierarchy右键 → Create Empty
重命名为: GameManager
添加脚本:
- GameManager
- GamePhaseManager  
- ResourceManager
- PerformanceMonitor
```

#### **2.2 创建UIManager**
```
Hierarchy右键 → Create Empty
重命名为: UIManager
添加脚本:
- UIManager
```

#### **2.3 创建AudioManager**
```
Hierarchy右键 → Create Empty
重命名为: AudioManager
添加脚本:
- AudioManager

创建子对象:
- MusicSource (添加AudioSource组件)
- SFXSource (添加AudioSource组件)
```

### **第三步: 创建UI面板结构**

#### **3.1 主菜单面板**
```
MainCanvas右键 → UI → Panel
重命名为: MainMenuPanel
添加子对象:
├── Background (Image - 深色半透明)
├── TitleText (Text - "数字博弈游戏")
├── StartButton (Button - "开始游戏")
└── QuitButton (Button - "退出游戏")
```

#### **3.2 游戏界面面板**
```
MainCanvas右键 → UI → Panel  
重命名为: GameplayPanel
设置为不激活: SetActive(false)
添加子对象:
├── TopUI (Panel)
│   ├── TimerUI (添加TimerUI脚本)
│   ├── PhaseText (Text - 显示当前阶段)
│   └── ScoreText (Text - 显示分数)
├── GameBoardArea (空GameObject)
├── HandPanelUI (添加HandPanelUI脚本)
└── ControlButtons (Panel)
    ├── EndTurnButton (Button)
    └── PauseButton (Button)
```

#### **3.3 游戏结束面板**
```
MainCanvas右键 → UI → Panel
重命名为: GameOverPanel  
设置为不激活: SetActive(false)
添加子对象:
├── Background (Image - 黑色半透明)
├── ResultText (Text - 显示结果)
├── RestartButton (Button - "重新开始")
└── MainMenuButton (Button - "返回主菜单")
```

### **第四步: 创建游戏棋盘**

#### **4.1 创建游戏棋盘**
```
Hierarchy右键 → Create Empty
重命名为: GameBoard
添加脚本: GameBoardLayout
位置: (0, 300, 0)
```

#### **4.2 创建卡牌槽位** (自动创建)
```
GameBoardLayout脚本会自动创建:
- HumanHandArea (9个槽位 A-I)
- HumanModifierArea (9个槽位)
- BattleArea (9个槽位)
- AIModifierArea (9个槽位)  
- AIHandArea (9个槽位)
- DestroyedArea (销毁区域)
```

### **第五步: 创建玩家对象**

#### **5.1 创建人类玩家**
```
Hierarchy右键 → Create Empty
重命名为: HumanPlayer
添加脚本: NumberPlayer
设置参数:
- Player Name: "Human Player"
- Team: Red
- Is AI: false (取消勾选)
```

#### **5.2 创建AI玩家**
```
Hierarchy右键 → Create Empty
重命名为: AIPlayer  
添加脚本: NumberPlayer
设置参数:
- Player Name: "AI Player"
- Team: Blue
- Is AI: true (勾选)
```

### **第六步: 配置脚本引用**

#### **6.1 配置GameManager**
```
选择GameManager对象，在Inspector中设置:

Game Settings:
- Phase Time Limit: 60
- Win Condition Score: 5

Players:
- Human Player: 拖拽HumanPlayer对象
- AI Player: 拖拽AIPlayer对象

Game Areas:
- Hand Card Slots: 拖拽手牌槽位数组 (A-I)
- Modifier Card Area: 拖拽HumanModifierArea
- Battle Area: 拖拽BattleArea  
- Destroyed Area: 拖拽DestroyedArea
```

#### **6.2 配置UIManager**
```
选择UIManager对象，在Inspector中设置:

Main UI Panels:
- Main Menu Panel: 拖拽MainMenuPanel
- Gameplay Panel: 拖拽GameplayPanel
- Game Over Panel: 拖拽GameOverPanel

Gameplay UI:
- Timer UI: 拖拽TimerUI对象
- Hand Panel UI: 拖拽HandPanelUI对象
- Current Player Text: 拖拽PhaseText
- End Turn Button: 拖拽EndTurnButton

Game Over UI:
- Game Over Text: 拖拽ResultText
- Restart Button: 拖拽RestartButton
- Main Menu Button: 拖拽MainMenuButton
```

#### **6.3 配置AudioManager**
```
选择AudioManager对象，在Inspector中设置:

Audio Sources:
- Music Source: 拖拽MusicSource子对象
- SFX Source: 拖拽SFXSource子对象

Volume Settings:
- Master Volume: 1.0
- Music Volume: 0.7
- SFX Volume: 1.0
```

#### **6.4 配置按钮事件**
```
StartButton:
- On Click() → UIManager.OnStartGameButtonClicked

QuitButton:  
- On Click() → UIManager.OnQuitGameButtonClicked

EndTurnButton:
- On Click() → 需要连接到游戏逻辑

RestartButton:
- On Click() → UIManager.OnRestartButtonClicked

MainMenuButton:
- On Click() → UIManager.OnMainMenuButtonClicked
```

---

## 🧪 **第七步: 测试运行**

### **7.1 基础测试**
1. **编译检查**:
   - 确保Console没有错误
   - 所有脚本都正确编译

2. **资源检查**:
   - 运行游戏
   - 查看Console输出
   - 确认ResourceManager正常工作

3. **UI测试**:
   - 测试主菜单显示
   - 测试按钮点击
   - 测试界面切换

### **7.2 功能测试**
1. **游戏流程**:
   - 点击"开始游戏"
   - 观察阶段切换
   - 测试AI决策

2. **交互测试**:
   - 测试卡牌点击
   - 测试拖拽功能
   - 测试音效播放

### **7.3 边界测试**
1. **异常情况**:
   - 快速点击按钮
   - 长时间运行
   - 内存使用检查

---

## 🔍 **调试和优化**

### **常用调试命令**
```csharp
// 在Console中使用
ResourceManager.Instance.GetResourceStats()        // 查看资源状态
PerformanceMonitor.Instance.GetPerformanceReport() // 查看性能报告
GameManager.Instance.CurrentPhase                  // 查看当前游戏阶段
```

### **性能监控**
1. 启用PerformanceMonitor的屏幕显示
2. 观察FPS和内存使用
3. 必要时调用强制垃圾回收

### **资源验证**
1. 添加ResourceValidator到场景
2. 运行资源验证检查
3. 查看详细的资源报告

---

## ✅ **完成检查清单**

### **场景结构**:
- [ ] MainCanvas已创建并配置
- [ ] 三个主要UI面板已创建
- [ ] GameManager等核心管理器已添加
- [ ] 游戏棋盘已创建
- [ ] 玩家对象已创建

### **脚本配置**:
- [ ] GameManager引用已设置
- [ ] UIManager引用已设置  
- [ ] AudioManager音频源已配置
- [ ] 按钮事件已连接
- [ ] 玩家参数已设置

### **功能测试**:
- [ ] 游戏能正常启动
- [ ] 主菜单功能正常
- [ ] 游戏流程能正常进行
- [ ] AI决策功能正常
- [ ] 音效能正常播放
- [ ] 资源生成功能正常

---

## 🎯 **预期结果**

完成所有步骤后，您应该看到：

1. **主菜单界面**: 有标题和开始/退出按钮
2. **游戏界面**: 显示1x9的卡牌布局，有计时器和分数
3. **自动生成的卡牌**: 9张不同颜色的数字卡牌
4. **AI对战功能**: AI会自动进行决策和操作
5. **完整的游戏流程**: 5个阶段循环进行
6. **音效反馈**: 所有操作都有相应的音效

**如果遇到问题，请参考SETUP_GUIDE.md中的常见问题解决部分。**

**游戏现在可以完整运行了！** 🎉