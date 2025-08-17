<p align="center">
  <a href="../README.md">简体中文</a> | <a href="./README_en.md">English</a>
</p>

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases)
[![Latest Release](https://img.shields.io/github/v/release/CoWave-Fall/ProgrammedFanTweaker)](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases)
[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](./LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Code with Gemini](https://img.shields.io/badge/Code%20with-Gemini-1f425f.svg)](https://gemini.google.com/)

<p align="center">
  <img src="/Documents/AboutP.png" alt="Programmed Fan Tweaker Logo" width="400"/>
</p>


<h1 align="center">Programmed Fan Tweaker</h1>

<h3 align="center">A Precise and Modern Fan Control Tool for Dell Servers</h3>

<p align="center">
  A WinUI3 application designed for Dell servers, providing precise fan speed control via IPMI. It features a modern user interface for monitoring CPU temperatures and adjusting fan behavior to achieve the optimal balance between performance and acoustics.
</p>

---

## Table of Contents

- [Introduction](#introduction)
- [Core Features](#core-features)
- [Screenshots](#screenshots)
- [Technology Stack](#technology-stack)
- [Installation and Usage](#installation-and-usage)
  - [Prerequisites](#prerequisites)
  - [Installation Steps](#installation-steps)
  - [Configuration](#configuration)
- [Contribution Guide](#contribution-guide)
- [Acknowledgements](#acknowledgements)
- [License](#license)

---

## Introduction

**Programmed Fan Tweaker** is a modern Windows desktop application aimed at solving the excessive fan noise issue in Dell servers, especially in home or office environments. By leveraging the IPMI protocol, this tool allows users to bypass the server's default, often aggressive, fan control policies and implement custom manual or automatic fan speed adjustments. Its intuitive WinUI3 interface not only monitors critical CPU temperature data in real-time but also displays historical temperature changes through charts, helping you easily find the sweet spot between server performance and quiet operation.

## Core Features

*   🌬️ **IPMI-based Fan Control:** Directly override and manage Dell server fan speeds via IPMI commands.
*   🌡️ **CPU Temperature Monitoring:** Real-time display of CPU core temperatures with historical charting to analyze trends.
*   🤖 **Automatic & Manual Modes:**
    *   **Auto Mode:** Automatically adjusts fan speeds based on your predefined temperature thresholds.
    *   **Manual Mode:** Locks fan speeds to a specific percentage.
*   🚨 **Protection Mechanism:** Automatically boosts fan speeds to high when CPU temperatures reach critical levels to ensure hardware safety.
*   🖥️ **Visual Server Layout:** Provides a graphical representation of the server's internal layout, offering an intuitive view of temperature and fan status in different zones.
*   🎨 **Personalized Themes:** Supports System, Light, and Dark themes for a comfortable and personalized visual experience.
*   🌐 **Internationalization (i18n):** Available in multiple languages (English and Simplified Chinese) for user convenience.
*   ⚙️ **System Tray Integration:** Minimize the application to the system tray to run quietly in the background without interrupting your workflow.

## Screenshots

<!-- Insert application screenshots or GIFs here to visually demonstrate your app's interface and features. -->
<p align="center">
  <img src="https://via.placeholder.com/800x450.png?text=Application+Main+Interface" alt="Application Main Interface"/>
  <em>Main Interface: Temperature Monitoring & Fan Control</em>
</p>
<p align="center">
  <img src="https://via.placeholder.com/800x450.png?text=Historical+Temperature+Chart" alt="Historical Temperature Chart"/>
  <em>Historical Temperature Chart</em>
</p>

## Technology Stack

*   **UI Framework:** WinUI 3
*   **Core Logic:** .NET 8
*   **Charting Library:** [LiveCharts](https://lvcharts.com/)
*   **System Tray:** [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon)
*   **Hardware Monitoring:** [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)
*   **CPU Temperature Reading:** [GetCoreTempInfoNET](https://github.com/Software-Hardware-Codes-Development/GetCoreTempInfoNET)
*   **IPMI Communication:** [ipmitool](https://www.net.in.tum.de/projects/ipmitool/) (invoked as an external utility)
*   **JSON Handling:** [Newtonsoft.Json](https://www.newtonsoft.com/json)

## Installation and Usage

### Prerequisites

1.  **Operating System:** Windows 10 or newer.
2.  **.NET Runtime:** [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
3.  **Server:** A Dell server that supports IPMI.
4.  **IPMI Configuration:** Ensure "IPMI over LAN" is enabled in your server's iDRAC settings.
5.  **(Optional) Core Temp:** If you wish to get CPU temperatures via Core Temp, ensure it is running.

### Installation Steps

#### Method 1: Download from Releases (Recommended)

1.  Go to the project's [**Releases Page**](https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases/releases).
2.  Download the latest release (usually a `.zip` or `.msix` installer).
3.  Unzip the file or run the installer to set up the application.

#### Method 2: Build from Source

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/CoWave-Fall/IPMI-ProgrammedFanTweaker/releases.git
    cd ProgrammedFanTweaker
    ```

2.  **Build the project:**
    Open the solution (`.sln`) file with Visual Studio 2022 and build, or use the following command:
    ```bash
    dotnet build -c Release
    ```

3.  **Run the application:**
    After a successful build, the executable will be located in the `bin/Release/net8.0-windows10.0.19041.0/` directory.

### Configuration

1.  **iDRAC Settings:**
    *   Log in to your iDRAC.
    *   Navigate to `iDRAC Settings` -> `Network` -> `Services`.
    *   Ensure the `IPMI Over LAN` option is enabled.
    *   Enter your iDRAC IP address, username, and password into the application.

2.  **Core Temp Integration (Optional):**
    *   Run Core Temp.
    *   If the application cannot read temperatures automatically, check if a firewall or security software is blocking inter-program communication.

## Contribution Guide

We welcome all forms of contributions, whether it's bug reports, feature suggestions, or pull requests.

1.  **Fork** this repository.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a **Pull Request**.

Please ensure your code style is consistent with the project and add appropriate comments for new features.

## Acknowledgements

The development of this project was made possible by the inspiration and foundational work from the following projects. A heartfelt thank you to them:

*   [cw1997/dell_fans_controller](https://github.com/cw1997/dell_fans_controller)
*   [jiafeng5513/dell_fans_controller](https://github.com/jiafeng5513/dell_fans_controller)

Special thanks to Google Gemini for the invaluable assistance and guidance throughout the development process.

## License

This project is licensed under the [GPL-3.0 License](./LICENSE). See the `LICENSE` file for details.

---

<p align="center">
  <img src="/Documents/AboutA.png" alt="Footer Image A" width="300"/>
  <!-- img src="/程控静扇/Assets/AboutL.png" alt="Footer Image L" width="300"/ -->
</p>
