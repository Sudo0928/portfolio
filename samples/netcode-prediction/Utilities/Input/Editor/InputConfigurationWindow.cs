using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System과 동일한 GUI 형태의 Input Configuration 편집용 Custom Window
/// </summary>
public class InputConfigurationWindow : EditorWindow
{
    private InputTrackerAsset _configuration;
    private string _searchString = "";
    
    // 선택 상태
    private string _selectedMap = "";
    private int _selectedTrackerIndex = -1;
    
    // 스크롤 위치
    private Vector2 _mapScrollPosition;
    private Vector2 _actionsScrollPosition;
    private Vector2 _propertiesScrollPosition;
    
    // GUI 스타일
    private GUIStyle _headerStyle;
    private GUIStyle _itemStyle;
    private GUIStyle _selectedItemStyle;
    private GUIStyle _addButtonStyle;
    
    // 레이아웃
    private readonly float _toolbarHeight = 40f;
    private readonly float _columnMinWidth = 200f;
    private float _firstSplitterPos = 250f;
    private float _secondSplitterPos = 500f;
    private bool _isDraggingFirstSplitter = false;
    private bool _isDraggingSecondSplitter = false;
    
    // 새 트래커 추가용
    private bool _isAddingTracker = false;
    private string _newTrackerName = "";
    private string _newActionName = "";
    private TrackerType _newTrackerType = TrackerType.Button;
    private float _newDeadZone = 0.1f;
    private bool _newCollapseStartedPerformed = true;
    private string _newDescription = "";
    
    [MenuItem("Tools/Input System/Input Configuration")]
    public static void OpenWindow()
    {
        var window = GetWindow<InputConfigurationWindow>("Input Configuration");
        window.minSize = new Vector2(600, 300);
        window.Show();
    }
    
    public static void OpenWindow(InputTrackerAsset configuration)
    {
        var window = GetWindow<InputConfigurationWindow>("Input Configuration");
        window.minSize = new Vector2(600, 300);
        window._configuration = configuration;
        window.Show();
    }
    
    private void OnEnable()
    {
        InitializeStyles();
        LoadConfiguration();
    }
    
    private void LoadConfiguration()
    {
        if (_configuration == null)
        {
            var configs = AssetDatabase.FindAssets("t:InputTrackerAsset");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                _configuration = AssetDatabase.LoadAssetAtPath<InputTrackerAsset>(path);
            }
        }
    }
    
    private void InitializeStyles()
    {
        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11,
            margin = new RectOffset(5, 5, 5, 5)
        };
    }
    
    private void UpdateStyles()
    {
        if (_itemStyle == null)
        {
            _itemStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(10, 10, 2, 2),
                margin = new RectOffset(0, 0, 0, 0)
            };
        }
        
        if (_selectedItemStyle == null)
        {
            _selectedItemStyle = new GUIStyle(_itemStyle);
            _selectedItemStyle.normal.background = MakeTexture(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.3f));
            _selectedItemStyle.normal.textColor = Color.white;
        }
        
        if (_addButtonStyle == null)
        {
            _addButtonStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
        }
    }
    
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        
        var texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private void OnGUI()
    {
        UpdateStyles();
        
        if (_configuration == null)
        {
            DrawNoConfigurationGUI();
            return;
        }
        
        // 툴바
        DrawToolbar();
        
        // 3열 레이아웃
        var contentRect = new Rect(0, _toolbarHeight, position.width, position.height - _toolbarHeight);
        
        // 첫 번째 스플리터
        var firstSplitterRect = new Rect(_firstSplitterPos - 2, contentRect.y, 4, contentRect.height);
        DrawSplitter(firstSplitterRect, ref _isDraggingFirstSplitter, ref _firstSplitterPos, _columnMinWidth, _secondSplitterPos - 10);
        
        // 두 번째 스플리터
        var secondSplitterRect = new Rect(_secondSplitterPos - 2, contentRect.y, 4, contentRect.height);
        DrawSplitter(secondSplitterRect, ref _isDraggingSecondSplitter, ref _secondSplitterPos, _firstSplitterPos + 10, position.width - _columnMinWidth);
        
        // Action Maps 열
        var mapsRect = new Rect(0, contentRect.y, _firstSplitterPos - 2, contentRect.height);
        DrawActionMapsColumn(mapsRect);
        
        // Actions 열
        var actionsRect = new Rect(_firstSplitterPos + 2, contentRect.y, _secondSplitterPos - _firstSplitterPos - 4, contentRect.height);
        DrawActionsColumn(actionsRect);
        
        // Properties 열
        var propertiesRect = new Rect(_secondSplitterPos + 2, contentRect.y, position.width - _secondSplitterPos - 2, contentRect.height);
        DrawPropertiesColumn(propertiesRect);
    }
    
    private void DrawNoConfigurationGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginVertical();
        GUILayout.Label("No Input Configuration Selected", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        _configuration = EditorGUILayout.ObjectField(_configuration, typeof(InputTrackerAsset), false, GUILayout.Width(300)) as InputTrackerAsset;
        
        if (GUILayout.Button("Create New", GUILayout.Width(300)))
        {
            CreateNewConfiguration();
        }
        
        GUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
    
    private void DrawToolbar()
    {
        var toolbarRect = new Rect(0, 0, position.width, _toolbarHeight);
        GUI.Box(toolbarRect, "", EditorStyles.toolbar);
        
        GUILayout.BeginArea(toolbarRect);
        GUILayout.BeginHorizontal();
        
        // 왼쪽 섹션
        GUILayout.Space(10);
        
        if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            SaveConfiguration();
        }
        
        GUILayout.FlexibleSpace();
        
        // 오른쪽 섹션
        if (GUILayout.Toggle(false, "Auto-Save", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            // Auto-Save 토글 (현재는 구현하지 않음)
        }
        
        GUILayout.Space(10);
        
        // 검색
        _searchString = GUILayout.TextField(_searchString, EditorStyles.toolbarSearchField, GUILayout.Width(150));
        
        GUILayout.Space(10);
        
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    
    private void DrawSplitter(Rect rect, ref bool isDragging, ref float position, float minPos, float maxPos)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
        
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            isDragging = true;
            Event.current.Use();
        }
        
        if (isDragging)
        {
            position = Mathf.Clamp(Event.current.mousePosition.x, minPos, maxPos);
            Repaint();
        }
        
        if (Event.current.type == EventType.MouseUp)
        {
            isDragging = false;
        }
    }
    
    private void DrawActionMapsColumn(Rect rect)
    {
        GUI.Box(rect, "", GUI.skin.box);
        
        GUILayout.BeginArea(rect);
        
        // 헤더
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Action Maps", _headerStyle);
        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
        {
            // Action Map 추가 (현재는 하나만 사용)
        }
        GUILayout.EndHorizontal();
        
        // 내용
        _mapScrollPosition = GUILayout.BeginScrollView(_mapScrollPosition);
        
        if (_configuration.InputActionAsset != null)
        {
            var maps = _configuration.InputActionAsset.actionMaps;
            foreach (var map in maps)
            {
                var isSelected = _selectedMap == map.name;
                
                if (GUILayout.Button(map.name, isSelected ? _selectedItemStyle : _itemStyle, GUILayout.ExpandWidth(true)))
                {
                    _selectedMap = map.name;
                    _selectedTrackerIndex = -1;
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Input Action Asset assigned", MessageType.Info);
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawActionsColumn(Rect rect)
    {
        GUI.Box(rect, "", GUI.skin.box);
        
        GUILayout.BeginArea(rect);
        
        // 헤더
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Actions", _headerStyle);
        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
        {
            StartAddingTracker();
        }
        GUILayout.EndHorizontal();
        
        // 내용
        _actionsScrollPosition = GUILayout.BeginScrollView(_actionsScrollPosition);
        
        if (!string.IsNullOrEmpty(_selectedMap))
        {
            // 선택된 Map의 트래커들만 표시
            var trackersForMap = _configuration.InputTrackerConfigurations
                .Where((t, index) => {
                    if (t.ActionPath.StartsWith(_selectedMap + "/"))
                    {
                        return true;
                    }
                    return false;
                })
                .ToList();
            
            for (int i = 0; i < _configuration.InputTrackerConfigurations.Count; i++)
            {
                var tracker = _configuration.InputTrackerConfigurations[i];
                if (!tracker.ActionPath.StartsWith(_selectedMap + "/"))
                    continue;
                
                var actionName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.ActionPath;
                
                // 검색 필터
                if (!string.IsNullOrEmpty(_searchString) && 
                    !actionName.ToLower().Contains(_searchString.ToLower()))
                    continue;
                
                var isSelected = _selectedTrackerIndex == i;
                var displayName = actionName; // Unity Input System의 Action 이름 사용
                
                if (GUILayout.Button(displayName, isSelected ? _selectedItemStyle : _itemStyle, GUILayout.ExpandWidth(true)))
                {
                    _selectedTrackerIndex = i;
                    _isAddingTracker = false;
                }
            }
            
            // 새 액션 추가 영역
            if (_isAddingTracker)
            {
                // 사용 가능한 액션 찾기
                if (_configuration.InputActionAsset != null)
                {
                    var map = _configuration.InputActionAsset.FindActionMap(_selectedMap);
                    if (map != null)
                    {
                        var actions = map.actions.ToList();
                        
                        // 이미 사용 중인 액션 필터링
                        var usedActions = _configuration.InputTrackerConfigurations
                            .Where(t => t.ActionPath.StartsWith(_selectedMap + "/"))
                            .Select(t => t.ActionPath.Split('/')[1])
                            .ToList();
                        
                        var availableActions = actions.Where(a => !usedActions.Contains(a.name)).ToList();
                        
                        if (availableActions.Count > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            
                            // 드롭다운으로 액션 선택
                            var actionNames = availableActions.Select(a => a.name).ToArray();
                            var currentIndex = Array.IndexOf(actionNames, _newActionName);
                            if (currentIndex < 0) currentIndex = 0;
                            
                            var newIndex = EditorGUILayout.Popup(currentIndex, actionNames, GUILayout.ExpandWidth(true));
                            _newActionName = actionNames[newIndex];
                            _newTrackerName = _newActionName; // 트래커 이름을 액션 이름과 동일하게 설정
                            
                            if (GUILayout.Button("✓", GUILayout.Width(20)))
                            {
                                AddNewTracker();
                            }
                            
                            if (GUILayout.Button("✕", GUILayout.Width(20)))
                            {
                                CancelAddingTracker();
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("All actions in this map are already configured.", MessageType.Info);
                            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                            {
                                CancelAddingTracker();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            GUILayout.Label("Select an Action Map", _addButtonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawPropertiesColumn(Rect rect)
    {
        GUI.Box(rect, "", GUI.skin.box);
        
        GUILayout.BeginArea(rect);
        
        // 헤더
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Action Properties", _headerStyle);
        GUILayout.EndHorizontal();
        
        // 내용
        _propertiesScrollPosition = GUILayout.BeginScrollView(_propertiesScrollPosition);
        
        if (_selectedTrackerIndex >= 0 && _selectedTrackerIndex < _configuration.InputTrackerConfigurations.Count)
        {
            DrawTrackerProperties();
        }
        else if (_isAddingTracker)
        {
            DrawNewTrackerProperties();
        }
        else
        {
            GUILayout.Label("Select an Action", _addButtonStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawTrackerProperties()
    {
        var tracker = _configuration.InputTrackerConfigurations[_selectedTrackerIndex];
        var serializedObject = new SerializedObject(_configuration);
        var trackersProperty = serializedObject.FindProperty("_inputTrackerConfigurations");
        var trackerProperty = trackersProperty.GetArrayElementAtIndex(_selectedTrackerIndex);
        
        EditorGUILayout.Space(10);
        
        // Action 섹션
        EditorGUILayout.LabelField("▼ Action", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        // Action 이름 표시 (읽기 전용)
        var actionPath = trackerProperty.FindPropertyRelative("_actionPath").stringValue;
        var actionName = actionPath.Contains("/") ? actionPath.Split('/')[1] : actionPath;
        GUI.enabled = false;
        EditorGUILayout.TextField("Action", actionName);
        GUI.enabled = true;
        
        var typeProperty = trackerProperty.FindPropertyRelative("_type");
        EditorGUILayout.PropertyField(typeProperty, new GUIContent("Action Type"));
        
        // Action Type에 따라 Control Type 설정
        var trackerType = (TrackerType)typeProperty.enumValueIndex;
        var controlType = GetControlTypeForTrackerType(trackerType);
        GUI.enabled = false;
        EditorGUILayout.TextField("Control Type", controlType);
        GUI.enabled = true;
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        // Tracker Settings 섹션
        EditorGUILayout.LabelField("▼ Tracker Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        // Action Type에 따라 필요한 설정만 표시
        if (trackerType != TrackerType.Button)
        {
            var deadZoneProperty = trackerProperty.FindPropertyRelative("_deadZone");
            EditorGUILayout.PropertyField(deadZoneProperty, new GUIContent("Dead Zone"));
        }
        
        var collapseProperty = trackerProperty.FindPropertyRelative("_collapseStartedPerformed");
        EditorGUILayout.PropertyField(collapseProperty, new GUIContent("Collapse Events"));
        
        var descriptionProperty = trackerProperty.FindPropertyRelative("_description");
        EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("Description"));
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        // Interactions 섹션
        EditorGUILayout.LabelField("▼ Interactions", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("No Interactions have been added.", EditorStyles.miniLabel);
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        // Processors 섹션
        EditorGUILayout.LabelField("▼ Processors", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("No Processors have been added.", EditorStyles.miniLabel);
        EditorGUI.indentLevel--;
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(20);
        
        // 삭제 버튼
        if (GUILayout.Button("Remove Tracker", GUILayout.Height(25)))
        {
            RemoveTracker(_selectedTrackerIndex);
        }
    }
    
    private void DrawNewTrackerProperties()
    {
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("▼ New Action", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        // 사용 가능한 액션 찾기
        if (_configuration.InputActionAsset != null && !string.IsNullOrEmpty(_selectedMap))
        {
            var map = _configuration.InputActionAsset.FindActionMap(_selectedMap);
            if (map != null)
            {
                var actions = map.actions.ToList();
                var actionNames = actions.Select(a => a.name).ToList();
                
                // 이미 사용 중인 액션 필터링
                var usedActions = _configuration.InputTrackerConfigurations
                    .Where(t => t.ActionPath.StartsWith(_selectedMap + "/"))
                    .Select(t => t.ActionPath.Split('/')[1])
                    .ToList();
                
                var availableActions = actionNames.Where(a => !usedActions.Contains(a)).ToList();
                
                if (availableActions.Count > 0)
                {
                    var currentIndex = availableActions.IndexOf(_newActionName);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    var newIndex = EditorGUILayout.Popup("Source Action", currentIndex, availableActions.ToArray());
                    _newActionName = availableActions[newIndex];
                    _newTrackerName = _newActionName; // 트래커 이름을 액션 이름과 동일하게 설정
                    
                    // Action의 expectedControlType에 따라 TrackerType 자동 설정
                    var actionPath = $"{_selectedMap}/{_newActionName}";
                    _newTrackerType = GetTrackerTypeForAction(actionPath);
                    
                    // Action 이름 표시 (읽기 전용)
                    GUI.enabled = false;
                    EditorGUILayout.TextField("Action", _newActionName);
                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("All actions in this map are already configured.", MessageType.Info);
                }
            }
        }
        
        GUI.enabled = false;
        _newTrackerType = (TrackerType)EditorGUILayout.EnumPopup("Action Type", _newTrackerType);
        
        // Control Type 표시
        var controlType = GetControlTypeForTrackerType(_newTrackerType);
        EditorGUILayout.TextField("Control Type", controlType);
        GUI.enabled = true;
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("▼ Tracker Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        // Action Type에 따라 필요한 설정만 표시
        if (_newTrackerType != TrackerType.Button)
        {
            _newDeadZone = EditorGUILayout.FloatField("Dead Zone", _newDeadZone);
        }
        
        _newCollapseStartedPerformed = EditorGUILayout.Toggle("Collapse Events", _newCollapseStartedPerformed);
        _newDescription = EditorGUILayout.TextField("Description", _newDescription);
        
        EditorGUI.indentLevel--;
    }
    
    private string GetControlTypeForTrackerType(TrackerType type)
    {
        return type switch
        {
            TrackerType.Button => "Button",
            TrackerType.Axis => "Axis",
            TrackerType.Stick => "Vector 2",
            _ => "Any"
        };
    }
    
    private TrackerType GetTrackerTypeForAction(string actionPath)
    {
        if (_configuration.InputActionAsset == null || string.IsNullOrEmpty(actionPath))
            return TrackerType.Button;
        
        var parts = actionPath.Split('/');
        if (parts.Length != 2)
            return TrackerType.Button;
        
        var map = _configuration.InputActionAsset.FindActionMap(parts[0]);
        var action = map?.FindAction(parts[1]);
        
        if (action != null)
        {
            var expectedType = action.expectedControlType;
            if (!string.IsNullOrEmpty(expectedType))
            {
                return expectedType switch
                {
                    "Vector2" => TrackerType.Stick,
                    "Button" => TrackerType.Button,
                    "Axis" => TrackerType.Axis,
                    _ => TrackerType.Button
                };
            }
        }
        
        return TrackerType.Button;
    }
    
    private string GetControlTypeForAction(string actionPath)
    {
        if (_configuration.InputActionAsset == null || string.IsNullOrEmpty(actionPath))
            return "Any";
        
        var parts = actionPath.Split('/');
        if (parts.Length != 2)
            return "Any";
        
        var map = _configuration.InputActionAsset.FindActionMap(parts[0]);
        var action = map?.FindAction(parts[1]);
        
        if (action != null)
        {
            // Unity Input System의 expectedControlType 가져오기
            var expectedType = action.expectedControlType;
            if (!string.IsNullOrEmpty(expectedType))
            {
                // 타입 이름 정리 (예: "Vector2" -> "Vector 2")
                if (expectedType == "Vector2") return "Vector 2";
                if (expectedType == "Button") return "Button";
                if (expectedType == "Axis") return "Axis";
                return expectedType;
            }
        }
        
        return "Any";
    }
    
    private void StartAddingTracker()
    {
        if (string.IsNullOrEmpty(_selectedMap))
        {
            EditorUtility.DisplayDialog("No Map Selected", "Please select an Action Map first.", "OK");
            return;
        }
        
        _isAddingTracker = true;
        
        // 첫 번째 사용 가능한 액션을 기본값으로 설정
        if (_configuration.InputActionAsset != null)
        {
            var map = _configuration.InputActionAsset.FindActionMap(_selectedMap);
            if (map != null)
            {
                var actions = map.actions.ToList();
                var usedActions = _configuration.InputTrackerConfigurations
                    .Where(t => t.ActionPath.StartsWith(_selectedMap + "/"))
                    .Select(t => t.ActionPath.Split('/')[1])
                    .ToList();
                
                var availableActions = actions.Where(a => !usedActions.Contains(a.name)).ToList();
                if (availableActions.Count > 0)
                {
                    _newActionName = availableActions[0].name;
                    _newTrackerName = _newActionName;
                    
                    // 첫 번째 액션의 타입 자동 설정
                    var actionPath = $"{_selectedMap}/{_newActionName}";
                    _newTrackerType = GetTrackerTypeForAction(actionPath);
                }
                else
                {
                    // 사용 가능한 액션이 없으면 추가 모드 취소
                    _isAddingTracker = false;
                    EditorUtility.DisplayDialog("No Actions Available", "All actions in this map are already configured.", "OK");
                    return;
                }
            }
        }
        
        _newDeadZone = 0.1f;
        _newCollapseStartedPerformed = true;
        _newDescription = "";
    }
    
    private void CancelAddingTracker()
    {
        _isAddingTracker = false;
        _newTrackerName = "";
    }
    
    private void AddNewTracker()
    {
        if (string.IsNullOrEmpty(_newTrackerName) || string.IsNullOrEmpty(_newActionName))
            return;
        
        var actionPath = $"{_selectedMap}/{_newActionName}";
        
        // InputActionAsset에서 Action의 expectedControlType을 기반으로 TrackerType 자동 설정
        var trackerType = GetTrackerTypeForAction(actionPath);
        
        var tracker = new InputTrackerConfiguration(_newTrackerName, actionPath, trackerType);
        _configuration.AddTracker(tracker);
        
        // SerializedObject를 통해 추가 속성들 설정
        var serializedObject = new SerializedObject(_configuration);
        var trackersProperty = serializedObject.FindProperty("_inputTrackerConfigurations");
        var lastIndex = trackersProperty.arraySize - 1;
        var trackerProperty = trackersProperty.GetArrayElementAtIndex(lastIndex);
        
        // Button 타입이 아닐 때만 DeadZone 설정
        if (trackerType != TrackerType.Button)
        {
            trackerProperty.FindPropertyRelative("_deadZone").floatValue = _newDeadZone;
        }
        
        trackerProperty.FindPropertyRelative("_collapseStartedPerformed").boolValue = _newCollapseStartedPerformed;
        trackerProperty.FindPropertyRelative("_description").stringValue = _newDescription;
        
        serializedObject.ApplyModifiedProperties();
        
        _selectedTrackerIndex = lastIndex;
        _isAddingTracker = false;
        
        EditorUtility.SetDirty(_configuration);
    }
    
    private void RemoveTracker(int index)
    {
        if (index >= 0 && index < _configuration.InputTrackerConfigurations.Count)
        {
            _configuration.InputTrackerConfigurations.RemoveAt(index);
            EditorUtility.SetDirty(_configuration);
            
            _selectedTrackerIndex = -1;
        }
    }
    
    private void SaveConfiguration()
    {
        if (_configuration != null)
        {
            EditorUtility.SetDirty(_configuration);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    
    private void CreateNewConfiguration()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Create Input Configuration",
            "InputConfiguration",
            "asset",
            "Create new Input Configuration"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            var config = CreateInstance<InputTrackerAsset>();
            config.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            _configuration = config;
        }
    }
}

/// <summary>
/// C# 코드 생성기
/// </summary>
public class InputCodeGenerator
{
    private readonly InputTrackerAsset _configuration;
    
    public InputCodeGenerator(InputTrackerAsset configuration)
    {
        _configuration = configuration;
    }
    
    public bool GenerateCode()
    {
        try
        {
            var code = GenerateClassCode();
            var filePath = Path.Combine(_configuration.GeneratedCodePath, $"{_configuration.GeneratedCodeClassName}.cs");
            
            // 디렉토리 생성
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            File.WriteAllText(filePath, code);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to generate code: {ex.Message}");
            return false;
        }
    }
    
    private string GenerateClassCode()
    {
        var sb = new StringBuilder();
        
        // Using statements
        sb.AppendLine("using System;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.InputSystem;");
        sb.AppendLine();
        
        // Namespace
        if (!string.IsNullOrEmpty(_configuration.GeneratedCodeNamespace))
        {
            sb.AppendLine($"namespace {_configuration.GeneratedCodeNamespace}");
            sb.AppendLine("{");
        }
        
        // Class declaration
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Auto-generated input actions class from InputConfiguration");
        sb.AppendLine($"    /// </summary>");
        sb.AppendLine($"    public class {_configuration.GeneratedCodeClassName} : MonoBehaviour");
        sb.AppendLine($"    {{");
        
        // Fields
        sb.AppendLine($"        [SerializeField] private InputManager _inputManager;");
        sb.AppendLine($"        [SerializeField] private InputActionAsset _inputActionAsset;");
        sb.AppendLine();
        
        // Properties for each tracker
        foreach (var tracker in _configuration.InputTrackerConfigurations)
        {
            var trackerTypeName = GetTrackerTypeName(tracker.Type);
            var propertyName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.Name;
            sb.AppendLine($"        public {trackerTypeName} {propertyName} {{ get; private set; }}");
        }
        sb.AppendLine();
        
        // Unity methods
        sb.AppendLine($"        private void Awake()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            InitializeTrackers();");
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        sb.AppendLine($"        private void OnEnable()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (_inputActionAsset != null)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                _inputActionAsset.Enable();");
        sb.AppendLine($"                RegisterInputEvents();");
        sb.AppendLine($"            }}");
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        sb.AppendLine($"        private void OnDisable()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (_inputActionAsset != null)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                UnregisterInputEvents();");
        sb.AppendLine($"                _inputActionAsset.Disable();");
        sb.AppendLine($"            }}");
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        sb.AppendLine($"        private void InitializeTrackers()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (_inputManager == null)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                Debug.LogError(\"InputManager is not assigned!\");");
        sb.AppendLine($"                return;");
        sb.AppendLine($"            }}");
        sb.AppendLine();
        
        // Initialize each tracker
        foreach (var tracker in _configuration.InputTrackerConfigurations)
        {
            var methodName = GetAddMethodName(tracker.Type);
            var actionName = tracker.ActionPath.Split('/').Last();
            var propertyName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.Name;
            
            // Button 타입은 deadZone 매개변수 없이
            if (tracker.Type == TrackerType.Button)
            {
                sb.AppendLine($"            {propertyName} = new ButtonInputTracker();");
            }
            else
            {
                sb.AppendLine($"            {propertyName} = new {GetTrackerTypeName(tracker.Type)}({tracker.DeadZone}f);");
            }
            
            if (!tracker.CollapseStartedPerformed)
            {
                sb.AppendLine($"            {propertyName}.CollapseStartedPerformed = false;");
            }
        }
        
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        // Register input events
        sb.AppendLine($"        private void RegisterInputEvents()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (_inputActionAsset == null) return;");
        sb.AppendLine();
        
        foreach (var tracker in _configuration.InputTrackerConfigurations)
        {
            var actionName = tracker.ActionPath.Split('/').Last();
            var propertyName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.Name;
            
            sb.AppendLine($"            var {propertyName.ToLower()}Action = _inputActionAsset.FindAction(\"{tracker.ActionPath}\");");
            sb.AppendLine($"            if ({propertyName.ToLower()}Action != null)");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                {propertyName.ToLower()}Action.started += On{propertyName}Started;");
            sb.AppendLine($"                {propertyName.ToLower()}Action.performed += On{propertyName}Performed;");
            sb.AppendLine($"                {propertyName.ToLower()}Action.canceled += On{propertyName}Canceled;");
            sb.AppendLine($"            }}");
            sb.AppendLine();
        }
        
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        // Unregister input events
        sb.AppendLine($"        private void UnregisterInputEvents()");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            if (_inputActionAsset == null) return;");
        sb.AppendLine();
        
        foreach (var tracker in _configuration.InputTrackerConfigurations)
        {
            var actionName = tracker.ActionPath.Split('/').Last();
            var propertyName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.Name;
            
            sb.AppendLine($"            var {propertyName.ToLower()}Action = _inputActionAsset.FindAction(\"{tracker.ActionPath}\");");
            sb.AppendLine($"            if ({propertyName.ToLower()}Action != null)");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                {propertyName.ToLower()}Action.started -= On{propertyName}Started;");
            sb.AppendLine($"                {propertyName.ToLower()}Action.performed -= On{propertyName}Performed;");
            sb.AppendLine($"                {propertyName.ToLower()}Action.canceled -= On{propertyName}Canceled;");
            sb.AppendLine($"            }}");
            sb.AppendLine();
        }
        
        sb.AppendLine($"        }}");
        sb.AppendLine();
        
        // Event handler methods
        foreach (var tracker in _configuration.InputTrackerConfigurations)
        {
            var propertyName = tracker.ActionPath.Contains("/") ? tracker.ActionPath.Split('/')[1] : tracker.Name;
            
            // Started event
            sb.AppendLine($"        private void On{propertyName}Started(InputAction.CallbackContext context)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            // Handle {propertyName} started event");
            sb.AppendLine($"            // Add your custom logic here");
            sb.AppendLine($"        }}");
            sb.AppendLine();
            
            // Performed event
            sb.AppendLine($"        private void On{propertyName}Performed(InputAction.CallbackContext context)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            // Handle {propertyName} performed event");
            sb.AppendLine($"            // Add your custom logic here");
            
            // 타입에 따른 기본 처리 로직 추가
            if (tracker.Type == TrackerType.Button)
            {
                sb.AppendLine($"            if ({propertyName} != null)");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                // {propertyName}.OnPressed();");
                sb.AppendLine($"            }}");
            }
            else if (tracker.Type == TrackerType.Axis)
            {
                sb.AppendLine($"            if ({propertyName} != null)");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                float value = context.ReadValue<float>();");
                sb.AppendLine($"                // {propertyName}.OnAxisChanged(value);");
                sb.AppendLine($"            }}");
            }
            else if (tracker.Type == TrackerType.Stick)
            {
                sb.AppendLine($"            if ({propertyName} != null)");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                Vector2 value = context.ReadValue<Vector2>();");
                sb.AppendLine($"                // {propertyName}.OnStickChanged(value);");
                sb.AppendLine($"            }}");
            }
            
            sb.AppendLine($"        }}");
            sb.AppendLine();
            
            // Canceled event
            sb.AppendLine($"        private void On{propertyName}Canceled(InputAction.CallbackContext context)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            // Handle {propertyName} canceled event");
            sb.AppendLine($"            // Add your custom logic here");
            
            if (tracker.Type == TrackerType.Button)
            {
                sb.AppendLine($"            if ({propertyName} != null)");
                sb.AppendLine($"            {{");
                sb.AppendLine($"                // {propertyName}.OnReleased();");
                sb.AppendLine($"            }}");
            }
            
            sb.AppendLine($"        }}");
            sb.AppendLine();
        }
        
        // Close class
        sb.AppendLine($"    }}");
        
        // Close namespace
        if (!string.IsNullOrEmpty(_configuration.GeneratedCodeNamespace))
        {
            sb.AppendLine("}");
        }
        
        return sb.ToString();
    }
    
    private string GetTrackerTypeName(TrackerType type)
    {
        return type switch
        {
            TrackerType.Button => "ButtonInputTracker",
            TrackerType.Axis => "AxisInputTracker",
            TrackerType.Stick => "StickInputTracker",
            _ => "ButtonInputTracker"
        };
    }
    
    private string GetAddMethodName(TrackerType type)
    {
        return type switch
        {
            TrackerType.Button => "AddButton",
            TrackerType.Axis => "AddAxis",
            TrackerType.Stick => "AddStick",
            _ => "AddButton"
        };
    }
}