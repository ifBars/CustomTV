# 📺 CustomTV - Play Your Own Videos on the TV
Ever wanted to hijack the TV and play your own videos? Now you can.

## 🧩 Features
- ✅ Full MP4 playback support using Unity's built-in Video Player
- ✅ Multi-Video support
- ✅ Manual pause/resume & previous/skip controls via hotkeys
- ✅ Automatically plays next video
- ✅ Shuffles video list on mod load.

## ⌨️ Controls
While holding **Shift + Ctrl**:

- **-** or **Numpad Minus** → Pause video
- **+** or **Numpad Plus** → Resume video
- **]** → Skip video
- **[** → Previous video
- **Right Arrow** → Seek Forwards
- **Left Arrow** → Seek Backwards

## 📁 How to Install
1. Make sure you have **Melon Loader** installed for *Schedule I*.
2. Drop the **CustomTV.dll** into your `Mods` folder.
3. Create a folder called **TV** inside `Mods` if it doesn't exist.
4. Place your **mp4** files inside `Mods/TV`.

```
📁 Schedule I
 └── 📁 Mods
      ├── 📄 CustomTV.dll
      └── 📁 TV
           └── 📄 example.mp4
	   └── 📄 example.mp4
```

## 🧪 Compatibility
- **Game Version:** IL2CPP (main version of Schedule I)
- **Framework:** Melon Loader

## ⚙️ Configuration

You can tweak the keybinds and adjust the audio volume to your liking through the `CustomTVConfig.ini` file:

### 1. Where to Find the Config File  
The `CustomTVConfig.ini` file will be in the `Mods/TV` folder. If it doesn't exist, it will be created automatically with default settings.

### 2. Config Example (`CustomTVConfig.ini`):
```ini
; Valid key names: https://docs.unity3d.com/ScriptReference/KeyCode.html

[Keybinds]
Pause = Minus
Resume = Equals
Skip = RightBracket
Previous = LeftBracket
Seek Forward = RightArrow
Seek Backward = LeftArrow

[Values]
Volume = 100
Seek Amount = 5
```
*You still must press both Ctrl and Shift before the set keybind.

## ❓ FAQ

**Q: Can I use .webm or other formats?**  
A: No, Unity’s VideoPlayer works best with `.mp4` (H.264/AAC).

**Q: The screen is black!**  
A: Make sure your video has the **mp4** extension and uses a supported codec.

**Q: Keybinds aren't working.**  
A: Be sure to press both ctrl and shift before the keybind, for example ctrl-shift-] to skip video.