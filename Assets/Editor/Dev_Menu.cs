using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Dev_Menu : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/Dev_Menu")]
    public static void ShowExample()
    {
        Dev_Menu wnd = GetWindow<Dev_Menu>();
        wnd.titleContent = new GUIContent("Dev_Menu");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
