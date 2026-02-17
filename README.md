# Unity-HonkaiImpact3rd

# Unity 游戏项目 | Unity Game Project
[![Unity Version](https://img.shields.io/badge/Unity-2022.3.35-000000.svg?style=flat&logo=unity)](https://unity3d.com)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![ARPG](https://img.shields.io/badge/类型-ARPG-green.svg)]()
[![Status](https://img.shields.io/badge/状态-持续开发中-orange.svg)]()

## 📋 项目概述 | Project Overview

**第28届Unity个人练习ARPG作品集 | The 28th Unity Personal Practice ARPG Portfolio**  
这是一个基于Unity 2022.3.35开发的ARPG(动作角色扮演)游戏项目，参考了《崩坏3》的核心界面布局和交互逻辑。项目重点展示了游戏系统架构的实现和模块化开发能力。  
This is an ARPG (Action Role-Playing Game) project developed with Unity 2022.3.35, referencing the core interface layout and interaction logic of "Honkai Impact 3rd". The project focuses on demonstrating game system architecture implementation and modular development capabilities.

**核心目标 | Core Objectives:**
- ✅ 实现完整的UI界面流程和交互逻辑 | Implement complete UI interface flows and interaction logic
- ✅ 构建模块化、可扩展的游戏系统架构 | Build modular, scalable game system architecture
- ✅ 展示ARPG游戏核心系统的实现能力 | Demonstrate implementation capability of ARPG game core systems
- ✅ 体现良好的代码规范和工程实践 | Reflect good coding standards and engineering practices

**项目特点 | Project Characteristics:**
- 🎨 实现了完整的UI界面流程 | Complete UI interface flow implementation
- ⚡ 流畅的动画效果，使用DoTween优化交互体验 | Smooth animation effects using DoTween to optimize interaction experience
- 🏗️ 模块化的代码架构，便于维护和扩展 | Modular code architecture for easy maintenance and expansion
- 📱 基本的UI适配，支持主流分辨率 | Basic UI adaptation supporting mainstream resolutions

## 🛠️ 技术栈 | Tech Stack

### 核心引擎 | Core Engine
- **Unity 2022.3.35f1** - 主要开发引擎 | Main development engine
- **.NET 6** - 脚本运行时 | Scripting runtime
- **C#** - 主要编程语言 | Main programming language

### 核心插件 | Core Plugins
- **DoTween** - 动画系统 | Animation system
- **TextMeshPro** - 文本渲染 | Text rendering
- **Cinemachine** - 相机控制 | Camera control
- **Input System** - 输入管理 | Input management

### 开发工具 | Development Tools
- **Visual Studio 2022** - IDE
- **Git** - 版本控制 | Version control

## 📊 开发进度 | Development Progress

| 模块 | 时间段 | 状态 | 技术要点 |
|------|--------|------|----------|
| **登录与主页**<br>Login & Home | -2026.01.21 | ✅ 完成<br>Completed | 场景切换、基础UI交互<br>Scene switching, basic UI interaction |
| **任务界面**<br>Task UI | 2026.01.22-25 | ✅ 完成<br>Completed | 列表展示、数据管理<br>List display, data management |
| **女武神界面**<br>Valkyrie UI | 2026.01.26-02.01 | ✅ 完成<br>Completed | 角色数据管理、装备系统<br>Character data management, equipment system |
| **装备界面**<br>Equipment UI | 2026.02.02-02.05 | ✅ 完成<br>Completed | 装备管理、属性计算、对象池管理<br>Equipment management, attribute calculation, object pooling |
| **装备详情界面**<br>EquipmentDetail UI | 2026.02.06-02.09 | ✅ 完成<br>Completed | 装备进化、装备强化、特效系统<br>Equipment evolution, equipment enhancement, VFX system |
| **补给界面**<br>Supply UI | 2026.02.10-02.17 | ✅ 完成<br>Completed | 抽卡逻辑、概率系统<br>Gacha logic, probability system |
| **模块优化完善**<br>Module Optimization | 2026.02.17-至今<br>Present | 🔄 开发中<br>In Progress |  |

## 🏗️ 架构设计 | Architecture Design

### 设计模式应用 | Design Pattern Applications
- **MVC/MVP模式** - UI与逻辑分离 | UI and logic separation
- **单例模式** - 全局管理器 | Global managers
- **观察者模式** - 事件系统 | Event system
- **工厂模式** - 对象创建 | Object creation

### 核心系统 | Core Systems
```
├── Core/
│   ├── Managers/          # 管理器 | Managers
│   ├── Events/           # 事件系统 | Event system
│   ├── Data/             # 数据管理 | Data management
│   └── Utils/            # 工具类 | Utility classes
├── UI/
│   ├── Controllers/      # UI控制器 | UI controllers
│   ├── Views/           # UI视图 | UI views
│   └── Components/       # UI组件 | UI components
├── Gameplay/
│   ├── Characters/       # 角色系统 | Character system
│   ├── Equipment/       # 装备系统 | Equipment system
│   └── Inventory/        # 背包系统 | Inventory system
└── Services/
    ├── Audio/           # 音频管理 | Audio management
    ├── Config/          # 配置管理 | Configuration management
```

### 技术实现重点 | Key Technical Implementations
1. **模块化架构** - 系统间解耦设计 | Modular architecture - decoupled system design
2. **事件驱动通信** - 降低系统耦合度 | Event-driven communication - reducing system coupling
3. **数据管理** - ScriptableObject配置系统 | Data management - ScriptableObject configuration system
4. **资源管理** - 基本的资源加载管理 | Resource management - basic resource loading management
5. **代码结构** - 清晰的目录组织和命名规范 | Code structure - clear directory organization and naming conventions

## 🚀 快速开始 | Quick Start

### 环境要求 | Environment Requirements
- Unity 2022.3.35f1 或更高版本 | Unity 2022.3.35f1 or higher
- .NET 6
- 4GB+ RAM

### 运行步骤 | Running Steps
```bash
# 1. 克隆仓库 | Clone repository
git clone https://github.com/zimo222/Unity-HonkaiImpact3rd.git

# 2. 使用Unity Hub打开项目 | Open project with Unity Hub
# 3. 打开场景 Assets/Scenes/LoginScene.unity | Open scene Assets/Scenes/LoginScene.unity
# 4. 点击播放按钮 | Click play button
```

### 开发说明 | Development Notes
1. 所有脚本位于 `Assets/Scripts/` 目录 | All scripts are in `Assets/Scripts/` directory
2. UI预制体位于 `Assets/Prefabs/UI/` | UI prefabs are in `Assets/Prefabs/UI/`
3. 配置文件在 `Assets/Resources/` 目录 | Configuration files are in `Assets/Resources/` directory

## 🎮 功能实现 | Features Implementation

### 已完成功能 | Completed Features
- [x] **登录系统** - 用户登录和数据加载 | **Login system** - user login and data loading
- [x] **主界面** - 基础导航和功能入口 | **Main interface** - basic navigation and function entries
- [x] **角色系统** - 女武神管理和属性展示 | **Character system** - Valkyrie management and attribute display
- [x] **装备系统** - 装备穿戴和强化 | **Equipment system** - equipment wearing and enhancement
- [x] **任务系统** - 任务列表和进度追踪 | **Task system** - task list and progress tracking
- [x] **界面交互** - 基础的UI交互动画 | **Interface interaction** - basic UI interaction animations

### 技术实现 | Technical Implementation
1. **界面流程** - 完整的界面跳转逻辑 | **Interface flow** - complete interface navigation logic
2. **数据管理** - 角色和装备数据管理 | **Data management** - character and equipment data management
3. **事件系统** - 系统间通信机制 | **Event system** - inter-system communication mechanism
4. **代码结构** - 清晰的模块划分 | **Code structure** - clear module division

## 🔧 遇到的问题与解决方案 | Challenges & Solutions

### 挑战1：系统架构设计 | Challenge 1: System Architecture Design
**问题** | **Problem**: 多个系统间数据通信复杂 | Complex data communication between multiple systems  
**解决方案** | **Solution**: 
- 设计事件总线系统 | Designed event bus system
- 采用观察者模式解耦 | Used observer pattern for decoupling
- 统一数据管理接口 | Unified data management interface

### 挑战2：UI状态管理 | Challenge 2: UI State Management
**问题** | **Problem**: UI状态切换逻辑混乱 | Confusing UI state switching logic  
**解决方案** | **Solution**:
- 实现状态管理模式 | Implemented state management pattern
- 分离UI显示和业务逻辑 | Separated UI display and business logic
- 使用有限状态机管理界面状态 | Used finite state machine for interface state management

### 挑战3：资源组织 | Challenge 3: Resource Organization
**问题** | **Problem**: 资源文件管理混乱 | Chaotic resource file management  
**解决方案** | **Solution**:
- 建立清晰的目录结构 | Established clear directory structure
- 统一命名规范 | Unified naming conventions
- 实现基本的资源加载管理 | Implemented basic resource loading management

### 挑战4：大量预制体变化导致性能问题 | Challenge 4: Performance Issues from Frequent Prefab Changes
**问题** | **Problem**: 装备界面中大量装备预制体的频繁创建和销毁导致性能下降 | Frequent creation and destruction of equipment prefabs in equipment interface causing performance degradation  
**解决方案** | **Solution**:
- 实现对象池管理系统 | Implemented object pooling management system
- 重用已创建的装备预制体 | Reuse created equipment prefabs
- 减少实例化和销毁的开销 | Reduce instantiation and destruction overhead

## 📈 工程实践 | Engineering Practices

### 代码规范 | Code Standards
- ✅ 统一的代码格式和命名规范 | Unified code formatting and naming conventions
- ✅ 合理的类和方法划分 | Reasonable class and method division
- ✅ 适当的注释和文档 | Appropriate comments and documentation

### 项目结构 | Project Structure
- ✅ 清晰的目录组织 | Clear directory organization
- ✅ 模块化的代码设计 | Modular code design
- ✅ 资源文件分类管理 | Categorized resource file management

## 🎯 学习收获 | Learnings & Takeaways

### 技术能力提升 | Technical Skill Improvement
1. **Unity开发** - 掌握了Unity UI系统和基础功能 | **Unity development** - mastered Unity UI system and basic functions
2. **架构设计** - 学习了模块化系统设计方法 | **Architecture design** - learned modular system design methods
3. **工程实践** - 了解了游戏项目开发流程 | **Engineering practice** - understood game project development process
4. **问题解决** - 提升了调试和问题解决能力 | **Problem solving** - improved debugging and problem-solving skills

### 项目经验 | Project Experience
1. **完整开发流程** - 从设计到实现的全过程 | **Complete development process** - full process from design to implementation
2. **系统思维** - 整体考虑系统间的协作 | **System thinking** - holistic consideration of system collaboration
3. **代码管理** - 版本控制和代码维护实践 | **Code management** - version control and code maintenance practice

## 🔮 后续计划 | Future Plans

### 短期计划 | Short-term Plans
- [ ] 完成补给系统基础功能 | Complete basic functions of supply system
- [ ] 优化现有系统性能 | Optimize performance of existing systems
- [ ] 完善数据持久化功能 | Improve data persistence functionality

### 技术改进 | Technical Improvements
- [ ] 添加单元测试 | Add unit tests
- [ ] 优化资源加载机制 | Optimize resource loading mechanism
- [ ] 完善错误处理 | Improve error handling

## 📄 许可证 | License

本项目采用 [MIT 许可证](LICENSE)。  
This project is licensed under the [MIT License](LICENSE).

## 👤 作者信息 | Author Info

**ZMStarrySky**  
游戏开发学习者 | Game Development Learner

- **GitHub**: [zimo222](https://github.com/zimo222)
- **QQ**: [2754285866](http://wpa.qq.com/msgrd?v=3&uin=2754285866&site=qq&menu=yes)

### 学习方向 | Learning Focus
- Unity游戏开发 | Unity game development
- C#编程与设计模式 | C# programming and design patterns
- 游戏系统架构 | Game system architecture
- 性能优化基础 | Performance optimization basics

## 🙏 说明 | Notes

这是一个学习性质的项目，重点在于技术实现和工程实践。  
This is a learning project focusing on technical implementation and engineering practice.

界面设计主要参考现有游戏布局，重点在于功能实现和代码结构。  
The interface design mainly references existing game layouts, with focus on functional implementation and code structure.

---

*最后更新 | Last Updated: 2026年2月 | February 2026*  
*项目状态 | Project Status: 开发中 | In Development*

---

**备注** | **Note**: 此项目为个人学习作品，主要用于技术学习和实践。  
This project is a personal learning project, mainly for technical study and practice.