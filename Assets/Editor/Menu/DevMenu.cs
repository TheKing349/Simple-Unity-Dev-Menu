using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class DevMenu : EditorWindow
{
    #region Initialization
    
    [SerializeField] private VisualTreeAsset visualTree;
    private VisualElement root;

    private Button framerateLimiterButton;
    private Button variableWatchingButton;

    private SliderInt framerateLimiterSlider;

    private TextField variableWatchingVariableTextField;
    private TextField variableWatchingValueTextField;

    private ObjectField variableWatchingObjectField;


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

        #region UI Definitions
        
        framerateLimiterButton = root.Q<Button>("FramerateLimiterButton");
        framerateLimiterButton.RegisterCallback<ClickEvent>(LimitFramerateButton);
        framerateLimiterButton.text = isLimitingFramerate ? "Stop Limiting Framerate" : "Limit Framerate";

        framerateLimiterSlider = root.Q<SliderInt>("FramerateSlider");
        framerateLimiterSlider.RegisterValueChangedCallback(LimitFramerateSlider);

        variableWatchingButton = root.Q<Button>("VariableWatchingButton");
        variableWatchingButton.RegisterCallback<ClickEvent>(WatchVariableButton);
        variableWatchingButton.text = isWatchingVariable ? "Stop Watching" : "Watch Variable";

        variableWatchingVariableTextField = root.Q<TextField>("VariableWatchingVariableTextField");
        variableWatchingVariableTextField.RegisterValueChangedCallback(WatchVariableText);

        variableWatchingValueTextField = root.Q<TextField>("VariableWatchingValueTextField");
        
        variableWatchingObjectField = root.Q<ObjectField>("VariableWatchingObjectField");
        variableWatchingObjectField.RegisterValueChangedCallback(WatchVariableObjectField);

        #endregion
    }

    #region Button Events
    
    #region Framerate Limiter
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
    #endregion

    #region Variable Watching
    private void WatchVariableButton(ClickEvent evt)
    {
        string targetVariable = variableWatchingVariableTextField.value;
        
        isWatchingVariable = !isWatchingVariable;
        variableWatchingButton.text = isWatchingVariable ? "Stop Watching" : "Watch Variable";
        if (!isWatchingVariable) variableWatchingValueTextField.value = "";
        
        if (!isWatchingVariable) return;
        
        GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
        variableWatchingValueTextField.value = VariableWatching.Watch(targetVariable, targetGameObject);
    }

    private void WatchVariableText(ChangeEvent<string> targetVariable)
    {
        if (!isWatchingVariable) return;
        
        GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
        variableWatchingValueTextField.value = VariableWatching.Watch(targetVariable.newValue, targetGameObject);
    }

    private void WatchVariableObjectField(ChangeEvent<Object> evt)
    {
        if (!isWatchingVariable) return;
        
        GameObject targetGameObject = evt.newValue as GameObject;
        variableWatchingValueTextField.value = VariableWatching.Watch(variableWatchingVariableTextField.value, targetGameObject);
    }
    
    #endregion

    #endregion
}