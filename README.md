# 📺 CustomTV - Play Your Own Videos on the TV

**Ever wanted to hijack the TV and play your own videos? Now you can.**

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
```

## 🧪 Compatibility
- **Game Version:** IL2CPP (main version of Schedule I)
- **Framework:** Melon Loader

## ⚙️ Configuration
You can tweak the keybinds and adjust the audio volume to your liking through the **CustomTVConfig.ini** file:

1. **Where to Find the Config File**  
   The **CustomTVConfig.ini** file will be in the **Mods/TV** folder. If it doesn't exist, it will be created automatically with default settings.

2. **Config Example (*CustomTVConfig.ini*):**

```
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

## 🔀 CustomTV Sorting Guide

If shuffle mode is disabled, CustomTV sorts your video files in a specific order to keep episodes and seasons in a logical sequence. To ensure your videos are sorted correctly, follow these simple naming rules:

**Recommended Naming Format:**

1. **Season and Episode Numbers First**  
Name your video files starting with the season and episode numbers, formatted as *SxxExx*.  
Example: *S01E01 - ShowName.mp4*

2. **Optional Leading Numbers**  
If you have files with a leading number (like special episodes, bonuses, or collections), place the number at the start of the file name.  
Example: *001 - SpecialEpisode.mp4* or *10 - BonusEpisode.mp4*

3. **Natural Sorting**  
After following the above formats, CustomTV will handle sorting your files in a natural order as usually displayed in file explorer (e.g., *S01E01* will come before *S01E10*).

**Important Notes:**
- **Season and episode number (*SxxExx*)** is the most important for sorting.
- Leading numbers are used for episodes or specials outside the main series.
- Files will be ordered by season first, then episode number, then any numbers at the start of the filename.

By following these simple naming conventions, CustomTV will automatically sort your episodes in a way that makes sense for watching!

## ❓ FAQ
**Q: Can I use .webm or other formats?**  
A: No, Unity’s VideoPlayer works best with `.mp4` (H.264/AAC).

**Q: The screen is black!**  
A: Make sure your video has the **mp4** extension and uses a supported codec.

**Q: Keybinds aren't working.**  
A: Be sure to press both ctrl and shift before the keybind, for example ctrl-shift-] to skip video.
