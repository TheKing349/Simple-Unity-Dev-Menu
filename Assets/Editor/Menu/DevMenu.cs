using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DevMenu : EditorWindow
{
    #region Initialization
    
    [SerializeField] private VisualTreeAsset visualTree;
    private VisualElement root;

    private Button framerateLimiterButton;
    private SliderInt framerateLimiterSlider;
    
    private Button variableWatchingButton;
    private TextField variableTextField;
    private TextField valueTextField;
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

        variableTextField = root.Q<TextField>("VariableWatchingVariableTextField");
        variableTextField.RegisterValueChangedCallback(WatchVariableText);

        valueTextField = root.Q<TextField>("VariableWatchingValueTextField");
        
        variableWatchingObjectField = root.Q<ObjectField>("VariableWatchingObjectField");
        variableWatchingObjectField.RegisterValueChangedCallback(WatchVariableObjectField);

        #endregion
    }

    private static void EnterPlaymode()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.EnterPlaymode();
        }
    }
    
    #region UI Events
    
    #region Framerate Limiter
    private void LimitFramerateButton(ClickEvent evt)
    {
        if (!isLimitingFramerate) EnterPlaymode();
        
        isLimitingFramerate = !isLimitingFramerate;
        framerateLimiterButton.text = isLimitingFramerate ? "Stop Limiting Framerate" : "Limit Framerate";

        int targetFramerate = isLimitingFramerate ? framerateLimiterSlider.value : defaultTargetFramerate;

        LimitFramerate(targetFramerate);
    }

    private void LimitFramerateSlider(ChangeEvent<int> targetFramerate)
    {
        if (isLimitingFramerate) LimitFramerate(targetFramerate.newValue);
    }

    private static void LimitFramerate(int targetFramerate)
    {
        Application.targetFrameRate = targetFramerate;
    }
    #endregion

    #region Variable Watching
    private void WatchVariableButton(ClickEvent evt)
    {
        string targetVariable = variableTextField.value;
        
        isWatchingVariable = !isWatchingVariable;
        variableWatchingButton.text = isWatchingVariable ? "Stop Watching" : "Watch Variable";
        if (!isWatchingVariable) valueTextField.value = "";
        
        if (isWatchingVariable)
        {
            GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
            valueTextField.value = VariableWatching.Watch(targetVariable, targetGameObject);
        }
        
    }

    private void WatchVariableText(ChangeEvent<string> targetVariable)
    {
        if (isWatchingVariable)
        {
            GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
            valueTextField.value = VariableWatching.Watch(targetVariable.newValue, targetGameObject);
        }
    }

    private void WatchVariableObjectField(ChangeEvent<Object> evt)
    {
        if (isWatchingVariable)
        {
            GameObject targetGameObject = evt.newValue as GameObject;
            valueTextField.value = VariableWatching.Watch(variableTextField.value, targetGameObject);
        }
    }
    
    #endregion

    #endregion
}