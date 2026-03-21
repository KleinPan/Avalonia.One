# Avalonia.One

一个基于 Avalonia 的跨平台桌面工具箱，采用 MVVM 架构，包含串口调试、数据处理、笔记、哈希、二维码等常用功能。

## 项目简介

Avalonia.One 的目标是把开发与调试中最常用的小工具集中在一个轻量桌面应用里，减少在多个独立工具之间来回切换的成本。  
项目以模块化方式组织功能，保持 UI 与业务逻辑分离，方便后续扩展新工具页、复用基础能力并持续演进。

## 当前基线

- TargetFramework: `net8.0`
- Avalonia: `11.3.12`
- 解决方案: `Avalonia.One.sln`

## 项目结构

- `One.Base`: 纯逻辑与基础能力（扩展、加密、网络、数据处理等）
- `One.Control`: 自定义控件与标记扩展
- `One.SimpleLog`: 轻量日志组件
- `One.Toolbox`: 桌面应用主体
- `One.Toolbox.Desktop`: 打包与发布项目
- `One.Base.Tests`: `One.Base` 的单元测试项目

## 本次整理（2026-03-21）

- 稳定性收敛
- 修复 `SettingService` 串口配置加载异常时错误重置全局配置的问题
- 清理关键 `async void` 命令（Cloud/Bing/Note/Dashboard 等）
- 串口后台读取与定时发送补充取消机制，UI 更新统一切回 UI 线程

- 架构减负
- 主导航改为按需创建页面，避免启动时一次性创建全部页面
- Bing 图片与云同步逻辑下沉至 `Services`

- 质量基线
- 串口端口枚举增加平台守卫，压制 `CA1416` 关键路径警告
- 新增 `One.Base.Tests`，补充 `NumberHelper` 与 `RegexHelper` 单元测试

- 工程整理
- README 统一为 UTF-8，修复中文乱码
- TargetFramework 与 CI 版本保持在 .NET 8

## 本地构建

```bash
dotnet restore
dotnet build Avalonia.One.sln -c Debug
```

## 运行测试

```bash
dotnet test One.Base.Tests/One.Base.Tests.csproj -c Debug
```

## 发布

GitHub Actions 工作流位于 `.github/workflows/dotnet-desktop.yml`，触发条件为 `v*.*.*` tag。
