<div align="center">

# One (一个就够了)

一个基于 Avalonia 的现代化桌面工具箱。

</div>

&nbsp; ![GitHub issues](https://img.shields.io/github/issues/KleinPan/Avalonia.One.Toolbox)
&nbsp; [![博客地址](https://img.shields.io/badge/cnblogs-Link-brightgreen)](https://www.cnblogs.com/KevinBran/)
&nbsp; ![GitHub](https://img.shields.io/github/license/KleinPan/Avalonia.One.Toolbox)

![Alt](https://repobeats.axiom.co/api/embed/4fb7dc32557eadd8782eafb3e3f4564a73996dd1.svg "Repobeats analytics image")

---

# 项目架构

本项目采用 **MVVM** 架构开发，基于 **.NET 10.0** 和 **Avalonia 11.3.11**。

## 模块结构

| 模块 | 说明 |
|------|------|
| **One.Base** | 基础功能库（扩展方法、辅助类、加密、网络、日志等） |
| **One.Control** | 基础控件库（国际化绑定、转换器、自定义控件等） |
| **One.Toolbox** | 工具箱主应用 |
| **One.SimpleLog** | 轻量级日志系统 |
| **One.ConsoleTest** | 控制台测试项目 |

---

# 工具模块

## 🏠 Home - 仪表盘

系统信息概览与快捷入口：

- 每日一言（集成一言API）
- GitHub releases 信息展示
- 系统状态概览

---

## 🔌 Serialport - 串口调试工具

| 功能 | 说明 |
|------|------|
| 串口选择 | 自动扫描可用串口 |
| 参数配置 | 波特率、数据位、停止位、流控制 |
| 数据发送 | 支持文本/Hex模式发送 |
| 数据接收 | 实时显示接收数据 |
| 定时发送 | 可配置的自动发送功能 |
| 快捷发送 | 保存常用发送内容 |
| 数据日志 | 自动记录通信数据 |

---

## 🖼️ Images - Bing每日壁纸

- 自动获取 Bing 今日壁纸
- 历史壁纸浏览
- 一键下载保存

---

## 📝 Texts - 数据处理工具集

### 编码转换

| 功能 | 说明 |
|------|------|
| Base64 | 字符串 ↔ Base64 编码 |
| Hex | 字符串 ↔ 16进制（支持分隔符） |
| URL | URL 编码/解码 |
| HTML | HTML 实体编码/解码 |
| Unicode | 字符串 ↔ Unicode |

### 文本工具

- 大小写转换
- 空格移除/添加
- 字符串格式化

---

## 📒 Notes - 备忘录

- 简单笔记管理
- Markdown 支持
- 本地持久化存储
- 快速搜索

---

## 🔒 HashTools - 哈希工具

| 功能 | 说明 |
|------|------|
| MD5 | 32位十六进制哈希 |
| SHA-1 | 40位十六进制哈希 |
| SHA-256 | 64位十六进制哈希 |
| SHA-512 | 128位十六进制哈希 |

- 文件哈希计算
- 文本哈希计算

---

## 🧪 RegTestTool - 正则测试器

- 实时正则表达式测试
- 匹配结果高亮显示
- 分组提取结果查看
- 常用正则模板

---

## 📱 QRCode - 二维码生成

- 文本转二维码
- 支持自定义尺寸
- 保存为图片

---

## ⏰ TimeConvert - Unix时间戳转换

- Unix时间 ↔ 日期时间互转
- 毫秒/秒精度切换
- 时区转换
- 批量转换

---

## ⚙️ Settings - 系统设置

- 应用配置管理
- 主题切换（亮色/暗色）
- 语言设置
- 数据导出/导入
- 快捷键配置

---

# 技术栈

## 主要依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| Avalonia | 11.3.11 | UI框架 |
| CommunityToolkit.Mvvm | 8.x | MVVM支持 |
| Semi.Avalonia | 11.x | UI主题 |
| Ursa.Avalonia | 1.x | 高级控件 |
| AvaloniaEdit | 11.x | 代码编辑器 |
| Markdown.Avalonia | 11.0.3-a1 | Markdown渲染 |

## 技术特点

- ✅ **跨平台**: Windows / macOS / Linux
- ✅ **MVVM架构**: 代码结构清晰，易于维护
- ✅ **多语言支持**: 国际化设计
- ✅ **主题支持**: 亮色/暗色主题
- ✅ **AOT兼容**: 除反射相关模块外均支持

---

# 开发指南

## 环境要求

- .NET 10.0 SDK
- Visual Studio 2022 / JetBrains Rider / VS Code

## 编译

```bash
# 编译所有项目
dotnet build

# 单独编译
dotnet build One.Toolbox/One.Toolbox.csproj
```

## 项目结构

```
Avalonia.One/
├── One.Base/              # 基础库
│   ├── ExtensionMethods/     # 扩展方法
│   ├── Helpers/              # 辅助工具类
│   │   ├── DataProcessHelpers/
│   │   ├── EncryptionHelpers/
│   │   ├── HttpHelper/
│   │   └── NetHelpers/
│   └── Models/
├── One.Control/           # 控件库
│   ├── Converters/           # 值转换器
│   ├── Markup/               # 标记扩展
│   └── Controls/             # 自定义控件
├── One.Toolbox/           # 工具箱应用
│   ├── ViewModels/           # 视图模型
│   ├── Views/                # 视图
│   ├── Services/             # 服务类
│   └── Assets/               # 资源文件
├── One.SimpleLog/         # 日志系统
├── One.ConsoleTest/       # 测试项目
└── docs/                  # 文档
```

---

# 贡献指南

欢迎提交 Issue 和 Pull Request！

---

# 许可证

MIT License

---

# 联系方式

- 📖 [博客](https://www.cnblogs.com/KevinBran/)
- 🐛 [问题反馈](https://github.com/KleinPan/Avalonia.One.Toolbox/issues)

---

*最后更新: 2026-03-06*