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

    private TextField variableWatchingVariableTextField;

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
        variableWatchingButton.RegisterCallback<ClickEvent>(WatchVariableButton);

        variableWatchingVariableTextField = root.Q<TextField>("VariableWatchingVariableTextField");
        variableWatchingVariableTextField.RegisterValueChangedCallback(WatchVariableText);

        #endregion
    }

    #region Button Events
    private void LimitFramerateButton(ClickEvent evt)
    {
        isLimitingFramerate = !isLimitingFramerate;
        framerateLimiterButton.text = isLimitingFramerate ? "Stop Limiting Framerate" : "Limit Framerate";

        int targetFramerate = isLimitingFramerate ? framerateLimiterSlider.value : defaultTargetFramerate;

        LimitFramerate(targetFramerate);
    }

    private void LimitFramerateSlider(ChangeEvent<int> targetFramerate)
    {
        if (isLimitingFramerate) LimitFramerate(targetFramerate.newValue);
    }

    private void LimitFramerate(int targetFramerate)
    {
        Application.targetFrameRate = targetFramerate;
    }

    private void WatchVariableButton(ClickEvent evt)
    {
        string targetVariable = variableWatchingVariableTextField.value;
        
        isWatchingVariable = !isWatchingVariable;
        variableWatchingButton.text = isWatchingVariable ? "Stop Watching" : "Watch Variable";
        
        WatchVariable(targetVariable);
    }

    private void WatchVariableText(ChangeEvent<string> targetVariable)
    {
        WatchVariable(targetVariable.newValue);
    }

    private void WatchVariable(string targetVariable)
    {
        Debug.Log("NOT IMPLEMENTED");
    }
    #endregion
}