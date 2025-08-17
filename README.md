<p align="center">
  <a href="./README.md">简体中文</a> | <a href="./Documents/README_en.md">English</a>
</p>

[![构建状态](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker)
[![最新版本](https://img.shields.io/github/v/release/CoWave-Fall/ProgrammedFanTweaker)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases)
[![许可证](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](./Documents/LICENSE)
[![.NET 版本](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Code with Gemini](https://img.shields.io/badge/Code%20with-Gemini-1f425f.svg)](https://gemini.google.com/)

<p align="center">
  <img src="/Documents/AboutP.png" alt="Programmed Fan Tweaker Logo" width="400"/>
</p>

<h1 align="center">程控静扇服务器热管理器</h1>

<h3 align="center">精准、现代化的服务器风扇控制工具</h3>
<p align="center">
  一款为服务器设计的 WinUI3 应用，通过 IPMI 实现对风扇转速的精准控制，提供现代化的 UI 界面来监控 CPU 温度并调整风扇策略，从而实现性能与噪音的最佳平衡。
</p>

---

## 目录

- [项目简介](#项目简介)
- [核心功能](#核心功能)
- [截图演示](#截图演示)
- [技术栈与致谢](#技术栈与致谢)
- [安装与使用](#安装与使用)
- [贡献指南](#贡献指南)
- [免责声明](#免责声明)
- [许可证](#许可证)

---

## 项目简介

**程控静扇** 服务器热管理器是一款现代化的 Windows 桌面应用程序，旨在解决服务器（尤其是在家庭或办公环境下）风扇噪音过大的问题。通过利用 IPMI 协议，本工具允许用户绕过服务器默认的、通常较为激进的风扇控制策略，实现自定义的手动或自动风扇调速。其直观的 WinUI3 界面不仅可以实时监控关键的 CPU 温度数据，还能以图表形式展示历史温度变化，帮助您轻松找到服务器性能与静音的最佳结合点。

## 核心功能

*   🌬️ **IPMI 风扇控制:** 通过 IPMI 命令直接覆盖和控制服务器的风扇转速。
*   🌡️ **CPU 温度监控:** 实时展示 CPU 核心温度，并通过历史曲线图表帮助分析温度趋势。
*   🤖 **自动与手动模式:**
    *   **自动模式:** 根据您设定的温度阈值自动调整风扇转速。
    *   **手动模式:** 将风扇锁定在指定的转速运行。
*   🚨 **安全保护机制:** 当 CPU 温度达到危险阈值时，自动将风扇提升至高速运转，确保硬件安全。
*   🖥️ **可视化服务器布局:** 提供服务器内部组件的图形化布局，直观展示各区域的温度和风扇状态。
*   🎨 **个性化主题:** 支持系统、浅色和深色三种主题模式，提供舒适的视觉体验。
*   🌐 **多语言支持 (i18n):** 内置英语和简体中文，方便不同语言用户使用。
*   ⚙️ **系统托盘集成:** 可最小化至系统托盘，在后台安静运行，不打扰您的工作。

## 截图演示

<!-- 在这里插入应用的截图或 GIF 动图，向用户直观展示您的应用界面和功能 -->
<p align="center">
  <img src="/Documents/Screenshot.png" alt="应用主界面"/>
  <em>应用主界面：温度监控与风扇控制</em>
</p>

## 技术栈与致谢

本项目的构建离不开以下优秀的开源技术和项目，在此表示衷心感谢：

*   **UI 框架:** WinUI 3
*   **核心逻辑:** .NET 8
*   **图表库:** [LiveCharts](https://lvcharts.com/) (MIT License)
*   **系统托盘:** [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon) (MIT License)
*   **硬件监控:** [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) (MPL-2.0 License)
*   **CPU 温度读取:** [GetCoreTempInfoNET](https://github.com/Software-Hardware-Codes-Development/GetCoreTempInfoNET) (MIT License)
*   **IPMI 通信:** [ipmitool](https://www.net.in.tum.de/projects/ipmitool/) (BSD License) - 作为外部工具调用
*   **JSON 处理:** [Newtonsoft.Json](https://www.newtonsoft.com/json) (MIT License)

#### 项目启发

本项目的开发受到了以下项目的极大启发：

*   [cw1997/dell_fans_controller](https://github.com/cw1997/dell_fans_controller)
*   [jiafeng5513/dell_fans_controller](https://github.com/jiafeng5513/dell_fans_controller)

同时，特别感谢 **Google Gemini** 在开发过程中提供的宝贵建议和帮助。

## 安装与使用

### 先决条件

1.  **操作系统:** Windows 10 或更高版本。
2.  **.NET 运行环境:** [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)。
3.  **服务器:** 一台支持 IPMI 的服务器。（目前仅支持Dell PowerEdge R730/R730 XD）
4.  **IPMI 配置:** 确保服务器的 iDRAC 已启用 "IPMI over LAN"。
5.  **(可选) Core Temp:** 若需通过 Core Temp 获取 CPU 温度，请确保其正在运行。

注：当前版本下仅支持Dell PowerEdge R730/R730 XD，并已测试通过。其他的服务器支持会在后续版本增加。

注：当前版本下仅支持通过 Core Temp 读取处理器温度。其他的处理器温度获取方式会在后续版本增加。

### 安装步骤

#### 方式一：从 Releases 下载 (推荐)

1.  前往本项目的 [**Releases 页面**](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases)。
2.  下载最新的发行版本（通常是一个 `.zip` 或 `.msix` 安装包）。
3.  解压或直接运行安装程序即可。

#### 方式二：从源码构建

1.  **克隆仓库:**
    ```bash
    git clone https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker.git
    cd ProgrammedFanTweaker
    ```
2.  **构建项目:**
    使用 Visual Studio 2022 打开解决方案 (`.sln`) 文件并生成，或使用以下命令：
    ```bash
    dotnet build -c Release
    ```

### 配置要求

1.  **iDRAC 设置:** 登录 iDRAC，导航至 `iDRAC 设置` -> `网络` -> `服务`，确保 `IPMI Over LAN` 选项已启用。
2.  **应用内配置:** 在应用中填写您的 iDRAC IP 地址、用户名和密码。

## 贡献指南

我们欢迎任何形式的贡献！无论是提交 Bug 报告、功能建议还是代码 Pull Request。

1.  **Fork** 本仓库。
2.  创建您的功能分支 (`git checkout -b feature/AmazingFeature`)。
3.  提交您的更改 (`git commit -m 'Add some AmazingFeature'`)。
4.  将您的分支推送到远程仓库 (`git push origin feature/AmazingFeature`)。
5.  开启一个 **Pull Request**。

请确保您的代码风格与项目现有风格保持一致，并为新功能添加适当的注释。

## 免责声明

本软件是一个强大的工具，能够直接修改您服务器的硬件行为（风扇转速）。不当的配置（例如，将风扇转速设置得过低而导致硬件过热）**可能会对您的服务器硬件造成永久性损坏**。

**请您务必了解其工作原理并自行承担使用风险。开发者对因使用本软件而导致的任何硬件损坏或数据丢失不承担任何责任。**

## 许可证

本项目采用 **[GNU General Public License v3.0](./Documents/LICENSE)** 进行授权。

这意味着您可以自由地：
*   **使用**：在任何计算机上、为任何目的运行本程序。
*   **分享**：自由地分发本程序的副本。
*   **修改**：研究和修改本程序。

但您必须遵守以下核心条款：
*   **代码开源**：当您分发修改后的版本或基于本项目的衍生作品时，您**必须**以同样的 GPLv3 许可证提供完整的源代码。
*   **版权声明**：您必须在您的衍生作品中保留原作者的版权声明和许可证信息。

详情请参阅项目根目录下的 `LICENSE` 文件。

---

<p align="center">
  <img src="/Documents/AboutA.png" alt="Footer Image A" width="300"/>
</p>
