using System.Collections.Generic;
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
    
    private ListView valueListView;
    private TextField variableTextField;
    private Button variableWatchingButton;
    private ObjectField variableWatchingObjectField;

    private int defaultTargetFramerate;
    
    private bool isLimitingFramerate;
    private bool isWatchingVariable;

    [MenuItem("Dev/Menu #%d")]
    public static void ShowWindow()
    {
        DevMenu window = GetWindow<DevMenu>();
        window.titleContent = new GUIContent("Dev Menu");
    }
    
    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }
    
    #endregion

    public void CreateGUI()
    {
        root = rootVisualElement;

        visualTree.CloneTree(root);

        if (Application.targetFrameRate < 60) Application.targetFrameRate = 60;
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

        valueListView = root.Q<ListView>("VariableWatchingListView");
        valueListView.ClearSelection();
        
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

    private void OnEditorUpdate()
    {
        if (!isWatchingVariable) return;
        
        string targetVariable = variableTextField.value;
        GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
        UpdateListView(VariableWatcher.Watch(targetVariable, targetGameObject));
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
        
        if (!isWatchingVariable) UpdateListView(new List<string> { "" });
        else
        {
            GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
            UpdateListView(VariableWatcher.Watch(targetVariable, targetGameObject));
        }
        
    }

    private void WatchVariableText(ChangeEvent<string> targetVariable)
    {
        if (!isWatchingVariable) return;
        
        GameObject targetGameObject = variableWatchingObjectField.value as GameObject;
        UpdateListView(VariableWatcher.Watch(targetVariable.newValue, targetGameObject));
    }

    private void WatchVariableObjectField(ChangeEvent<Object> evt)
    {
        if (!isWatchingVariable) return;
        
        GameObject targetGameObject = evt.newValue as GameObject;
        UpdateListView(VariableWatcher.Watch(variableTextField.value, targetGameObject));
    }

    private void UpdateListView(List<string> items)
    {
        valueListView.style.height = items.Count == 0 ? 0 : Mathf.Min(items.Count * 20, 135);
        
        valueListView.ClearSelection();
        valueListView.itemsSource = items;
        valueListView.Rebuild();
    }
    
    #endregion

    #endregion
}