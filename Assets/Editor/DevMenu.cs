using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DevMenu : EditorWindow
{
    #region Initialization
    [SerializeField]
    private VisualTreeAsset visualTree;
    private VisualElement root;

    private Button framerateLimiterButton;
    private Button variableWatchingButton;

    private SliderInt framerateLimiterSlider;

    private int defaultTargetFramerate;
    
    private bool isLimitingFramerate;
    private bool isWatchingVariable;

    [MenuItem("Dev/Menu")]
    public static void ShowWindow()
    {
        DevMenu window = GetWindow<DevMenu>();
        window.titleContent = new GUIContent("Dev Menu");
    }
    #endregion

    public void CreateGUI()
    {
        root = rootVisualElement;

        visualTree.CloneTree(root);
        
        defaultTargetFramerate = Application.targetFrameRate;

        #region Button Definitions
        
        framerateLimiterButton = root.Q<Button>("FramerateLimiterButton");
        framerateLimiterButton.RegisterCallback<ClickEvent>(LimitFramerateButton);

        framerateLimiterSlider = root.Q<SliderInt>("FramerateSlider");
        framerateLimiterSlider.RegisterValueChangedCallback(LimitFramerateSlider);

        variableWatchingButton = root.Q<Button>("VariableWatchingButton");
        variableWatchingButton.RegisterCallback<ClickEvent>(WatchVariable);

        #endregion
    }

    #region Button Events
    private void LimitFramerateButton(ClickEvent evt)
    {
        int targetFramerate = root.Q<SliderInt>("FramerateSlider").value;

        isLimitingFramerate = !isLimitingFramerate;
        framerateLimiterButton.text = isLimitingFramerate ? "Stop Limiting Framerate" : "Limit Framerate";

        if (isLimitingFramerate) LimitFramerate(targetFramerate);
    }

    private void LimitFramerateSlider(ChangeEvent<int> targetFramerate)
    {
        if (isLimitingFramerate) LimitFramerate(targetFramerate.newValue);
    }

    private void LimitFramerate(int targetFramerate)
    {
        Application.targetFrameRate = isLimitingFramerate ? targetFramerate : defaultTargetFramerate;
    }

    private void WatchVariable(ClickEvent evt)
    {
        isWatchingVariable = !isWatchingVariable;
        variableWatchingButton.text = isWatchingVariable ? "Stop Watching" : "Watch Variable";
    }
    #endregion
}