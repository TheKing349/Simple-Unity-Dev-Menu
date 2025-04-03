# Unity Dev Menu - v1.0.0

A clean, modern developer menu for Unity built with the **UI Toolkit**, featuring runtime tools and automatic build exclusion. Styles adapt to Unity's light/dark theme and remains editor-only through `Assets/Editor` storage.

![Screenshot](https://github.com/TheKing349/Simple-Unity-Dev-Menu/blob/main/Dev%20Menu%20Screenshot.png)

## Why This Exists
Provides essential development tools without cluttering production builds:
1. Debugging features excluded from final builds
2. Unified UI matching Unity's native theme
3. Extensible architecture for team-specific tools

## Features
- **Framerate Control**  
  - Live FPS slider (1-165) with toggle override
  - Auto-applies when entering Play Mode
- **Variable Watcher** 
  - Type any GameObject variable name for live monitoring
  - Pseudo syntax-highlighting via **Rich Text**
- **Theme-Aware UI**
  - Automatic light/dark mode matching
  - Collapsible, state-saving foldouts
- **Safe for Production**  
  - Entire system stored in `Assets/Editor`  
  - Zero compile-time references in built games

## How It Works
1. **UI Rendering** - Uses UI Toolkit with custom UCSS theming
2. **Feature Execution** 
   - Framerate: Sets `Application.targetFrameRate`  
   - Variable Watch: Uses `FieldInfo` reflection with custom formatting
   - Play Mode: Triggers `EditorApplication.EnterPlaymode`

## Prerequisites
- Unity 2021.3+ (UI Toolkit required)
## Installation

### **Option 1: Package Import**
1. **Download** the [latest release](../../releases)
2. **Extract** to `YourProject/Assets/Editor`
3. **Open Menu** with `Ctrl/Cmd+Shift+D`

### **Option 2: Git Submodule**
```sh
git submodule add https://github.com/TheKing349/Simple-Unity-Dev-Menu.git Assets/Editor/
```

## Usage
### Framerate Control
1. Ensure **vSync** is turned **OFF**
	- In the **Game** UI Panel before the **Scale** modifier
2. Drag slider to desired FPS
3. Toggle **Limit Framerate**

### Variable Watching
1. Attach a GameObject to the **Target Game Object** field
2. Type exact variable name
3. Click **Watch Variable** to start monitoring
4. Toggle again to stop

### UI Structure

```yaml
ScrollView
├─ [Foldout] Framerate Limiter
│  └─ Slider + Toggle
├─ [Foldout] Variable Watching
│  └─ ObjectField + TextField + Watch Button + Value
```

## Contributing
1. **Fork** the repository
2. **Branch** per feature
3. **Test** changes in multiple Unity Versions
4. **Submit Pull Request** with:
   - Updated documentation
   - Screenshots of visual changes

## License
The project is licensed under the [MIT LICENSE](https://github.com/TheKing349/Simple-Unity-Dev-Menu/blob/main/LICENSE)
