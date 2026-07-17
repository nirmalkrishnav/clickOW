# 🖱️ ClickOw

**See every click, live.** ClickOw is a lightweight Windows tray app that highlights your mouse clicks in real time — so your audience always knows exactly where and when you clicked.

Built for **live demos, screen shares, UX reviews, and teaching**, ClickOw shines in the moment. Screen recorders like Screen Studio and CleanShot add click effects *after* the fact. ClickOw is for the live moment itself — no editing, no interruptions, just clarity.

<!-- 📸 Screenshot placeholder — replace with your own -->
<p align="center">
  <img src="docs/screenshot.png" alt="ClickOw screenshot" width="640">
</p>

<!-- 🎬 Demo GIF placeholder — replace with your own -->
<p align="center">
  <img src="docs/demo.gif" alt="ClickOw demo" width="640">
</p>

## ✨ Features

- **Click highlights everywhere** — works across every Windows app, browser page, and desktop, on all your monitors.
- **Distinct visuals per action** — separate effects for press, release, right-click, and drag, so nothing is ambiguous.
- **Laser pointer mode** — draw fading freehand strokes while you drag to guide attention like a real laser pointer.
- **Global shortcuts** — flip things on and off without breaking your flow.
- **Fully customizable** — tune colors, sizes, durations, and thresholds from a simple settings window.
- **Click-through overlay** — highlights never intercept your clicks; the app underneath always receives them.
- **Runs at startup** — set it and forget it.

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
| --- | --- |
| `Ctrl` + `Alt` + `C` | Toggle click highlights on/off |
| `Ctrl` + `Alt` + `L` | Toggle laser pointer mode |

Both modes work independently — run clicks, laser, both, or neither.

## ⬇️ Download

Grab the latest build from the releases page — no installation or .NET runtime required, just run the `.exe`:

- **[Download ClickOw (latest release)](https://github.com/nirmalkrishnav/clickow/releases/latest/download/ClickOw.exe)**
- [All releases](https://github.com/nirmalkrishnav/clickow/releases)

> ClickOw is currently in **beta**. Feedback and issues are welcome!

## 🚀 Getting Started

**Requirements:** Windows 10/11. The downloadable `.exe` is self-contained and needs no runtime. To build from source you need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
git clone https://github.com/nirmalkrishnav/clickow.git
cd clickow
dotnet run
```

Or build a standalone executable:

```powershell
dotnet publish -c Release
```

Once running, look for the **ClickOw** icon in your system tray. Right-click it for options, or double-click to open **Settings**.

## ⚙️ Settings

Open the settings window from the tray icon to adjust:

- Colors for press, release, right-click, drag, and laser
- Click effect size and duration
- Drag threshold, laser thickness, and laser fade time
- Run at Windows startup

Settings are saved to `%AppData%\ClickOw\settings.json`.

## 🙏 Credits

ClickOw was inspired by [**ClickLight** by aurorascharff](https://github.com/aurorascharff/ClickLight) — a macOS menu bar app that highlights your clicks for demos, recordings, UX reviews, and better click visibility. ClickOw brings the same idea to **Windows**.

## 📄 License

[MIT](LICENSE)
