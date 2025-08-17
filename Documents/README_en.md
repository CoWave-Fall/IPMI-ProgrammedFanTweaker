<p align="center">
  <a href="./README.md">简体中文</a> | <a href="./README_en.md">English</a>
</p>

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker)
[![Latest Release](https://img.shields.io/github/v/release/CoWave-Fall/ProgrammedFanTweaker)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases)
[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](./Documents/LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Code with Gemini](https://img.shields.io/badge/Code%20with-Gemini-1f425f.svg)](https://gemini.google.com/)

<p align="center">
  <img src="/Documents/AboutP.png" alt="Programmed Fan Tweaker Logo" width="400"/>
</p>

<h1 align="center">ProgrammedFanTweaker - Server Thermal Manager</h1>

<h3 align="center">A precise and modern server fan control utility</h3>
<p align="center">
  A WinUI 3 application designed for servers, enabling precise fan speed control via IPMI. It provides a modern UI to monitor CPU temperatures and adjust fan strategies, achieving the optimal balance between performance and noise.
</p>

---

## Table of Contents

- [Introduction](#introduction)
- [Core Features](#core-features)
- [Screenshots](#screenshots)
- [Tech Stack & Acknowledgements](#tech-stack--acknowledgements)
- [Installation & Usage](#installation--usage)
- [Contribution Guide](#contribution-guide)
- [Disclaimer](#disclaimer)
- [License](#license)

---

## Introduction

**ProgrammedFanTweaker** Server Thermal Manager is a modern Windows desktop application designed to solve the problem of excessive fan noise from servers, especially in home or office environments. By leveraging the IPMI protocol, this tool allows users to bypass the server's default, often aggressive, fan control policies and implement custom manual or automatic fan speed adjustments. Its intuitive WinUI 3 interface not only displays real-time critical CPU temperature data but also shows historical temperature changes in a chart, helping you easily find the sweet spot between server performance and quiet operation.

## Core Features

*   🌬️ **IPMI Fan Control:** Directly override and control server fan speeds using IPMI commands.
*   🌡️ **CPU Temperature Monitoring:** Display real-time CPU core temperatures and analyze temperature trends with a historical line chart.
*   🤖 **Automatic & Manual Modes:**
    *   **Automatic Mode:** Automatically adjusts fan speeds based on temperature thresholds you set.
    *   **Manual Mode:** Locks the fans to run at a specified speed.
*   🚨 **Safety Protection Mechanism:** When CPU temperature reaches a critical threshold, the fans are automatically ramped up to high speed to ensure hardware safety.
*   🖥️ **Visual Server Layout:** Provides a graphical layout of the server's internal components, intuitively displaying temperature and fan status for each zone.
*   🎨 **Personalized Themes:** Supports System, Light, and Dark theme modes for a comfortable visual experience.
*   🌐 **Multi-language Support (i18n):** Built-in support for English and Simplified Chinese for users of different languages.
*   ⚙️ **System Tray Integration:** Can be minimized to the system tray to run quietly in the background without disturbing your work.

## Screenshots

<!-- Insert screenshots or GIFs of your application here to visually demonstrate its interface and features to users -->
<p align="center">
  <img src="/Documents/Screenshot.png" alt="Application Main Interface"/>
  <em>Application Main Interface: Temperature Monitoring & Fan Control</em>
</p>

## Tech Stack & Acknowledgements

This project was made possible by the following excellent open-source technologies and projects. A heartfelt thank you to all of them:

*   **UI Framework:** WinUI 3
*   **Core Logic:** .NET 8
*   **Charting Library:** [LiveCharts](https://lvcharts.com/) (MIT License)
*   **System Tray:** [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon) (MIT License)
*   **Hardware Monitoring:** [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) (MPL-2.0 License)
*   **CPU Temperature Reading:** [GetCoreTempInfoNET](https://github.com/Software-Hardware-Codes-Development/GetCoreTempInfoNET) (MIT License)
*   **IPMI Communication:** [ipmitool](https://www.net.in.tum.de/projects/ipmitool/) (BSD License) - Invoked as an external tool
*   **JSON Handling:** [Newtonsoft.Json](https://www.newtonsoft.com/json) (MIT License)

#### Project Inspiration

The development of this project was greatly inspired by the following projects:

*   [cw1997/dell_fans_controller](https://github.com/cw1997/dell_fans_controller)
*   [jiafeng5513/dell_fans_controller](https://github.com/jiafeng5513/dell_fans_controller)

Special thanks to **Google Gemini** for providing invaluable advice and assistance during the development process.

## Installation & Usage

### Prerequisites

1.  **Operating System:** Windows 10 or later.
2.  **.NET Runtime:** [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
3.  **Server:** A server that supports IPMI. (Currently only Dell PowerEdge R730/R730 XD is supported).
4.  **IPMI Configuration:** Ensure "IPMI over LAN" is enabled in your server's iDRAC.
5.  **(Optional) Core Temp:** If you need to obtain CPU temperature via Core Temp, ensure it is running.

Note: The current version only supports and has been tested on Dell PowerEdge R730/R730 XD. Support for other servers will be added in future versions.
Note: The current version only supports reading processor temperature via Core Temp. Other methods for obtaining processor temperature will be added in future versions.

### Installation Steps

#### Method 1: Download from Releases (Recommended)

1.  Go to the project's [**Releases Page**](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases).
2.  Download the latest release version (usually a `.zip` or `.msix` installer).
3.  Unzip or run the installer to complete the installation.

#### Method 2: Build from Source

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker.git
    cd ProgrammedFanTweaker
    ```2.  **Build the project:**
    Open the solution (`.sln`) file with Visual Studio 2022 and build, or use the following command:
    ```bash
    dotnet build -c Release
    ```

### Configuration Requirements

1.  **iDRAC Settings:** Log in to iDRAC, navigate to `iDRAC Settings` -> `Network` -> `Services`, and ensure the `IPMI Over LAN` option is enabled.
2.  **In-app Configuration:** Enter your iDRAC IP address, username, and password in the application.

## Contribution Guide

We welcome all forms of contributions, whether it's submitting bug reports, feature suggestions, or code Pull Requests.

1.  **Fork** this repository.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push your branch to the remote repository (`git push origin feature/AmazingFeature`).
5.  Open a **Pull Request**.

Please ensure your code style is consistent with the existing project style and add appropriate comments for new features.

## Disclaimer

This software is a powerful tool that can directly modify your server's hardware behavior (fan speed). Improper configuration (e.g., setting the fan speed too low, leading to hardware overheating) **may cause permanent damage to your server hardware**.

**Please ensure you understand how it works and assume all risks associated with its use. The developers are not liable for any hardware damage or data loss resulting from the use of this software.**

## License

This project is licensed under the **[GNU General Public License v3.0](./Documents/LICENSE)**.

This means you are free to:
*   **Use**: Run the program for any purpose on any computer.
*   **Share**: Freely distribute copies of the program.
*   **Modify**: Study and change how the program works.

However, you must adhere to the following core terms:
*   **Keep it Open**: If you distribute modified versions or derivative works based on this project, you **must** provide the complete source code under the same GPLv3 license.
*   **Credit the Author**: You must retain the original author's copyright notices and license information in your derivative works.

For more details, please refer to the `LICENSE` file in the project's root directory.

---

<p align="center">
  <img src="/Documents/AboutA.png" alt="Footer Image A" width="300"/>
</p>
