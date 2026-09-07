using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputTrackerAsset))]
public class InputTrackerAssetEditor : Editor
{
    private SerializedProperty _inputActionAsset;
    private SerializedProperty _generatedCodePath;
    private SerializedProperty _generatedCodeClassName;
    private SerializedProperty _generatedCodeNamespace;
    private SerializedProperty _inputTrackerConfigurations;
    
    private bool _showConfigurationSettings = true;
    private bool _showTrackers = true;
    
    private void OnEnable()
    {
        _inputActionAsset = serializedObject.FindProperty("inputActionAsset");
        _generatedCodePath = serializedObject.FindProperty("_generatedCodePath");
        _generatedCodeClassName = serializedObject.FindProperty("_generatedCodeClassName");
        _generatedCodeNamespace = serializedObject.FindProperty("_generatedCodeNamespace");
        _inputTrackerConfigurations = serializedObject.FindProperty("_inputTrackerConfigurations");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var asset = target as InputTrackerAsset;
        
        // 헤더
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Input Tracker Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Input Action Asset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_inputActionAsset, new GUIContent("Input Action Asset"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.Space(10);
        
        // Configuration Settings (접을 수 있는 섹션)
        _showConfigurationSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showConfigurationSettings, "Code Generation Settings");
        if (_showConfigurationSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_generatedCodeClassName, new GUIContent("Class Name"));
            EditorGUILayout.PropertyField(_generatedCodeNamespace, new GUIContent("Namespace"));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_generatedCodePath, new GUIContent("Output Path"));
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", _generatedCodePath.stringValue, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // 상대 경로로 변환
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    _generatedCodePath.stringValue = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            
            // 버튼들
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Open Configuration Window", GUILayout.Height(25)))
            {
                InputConfigurationWindow.OpenWindow(asset);
            }
            
            GUI.enabled = asset.Validate(out var errors);
            if (GUILayout.Button("Generate C# Class", GUILayout.Height(25)))
            {
                var generator = new InputCodeGenerator(asset);
                if (generator.GenerateCode())
                {
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Success", "Code generated successfully!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to generate code. Check console for details.", "OK");
                }
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        
        // Trackers Preview (읽기 전용)
        _showTrackers = EditorGUILayout.BeginFoldoutHeaderGroup(_showTrackers, $"Configured Trackers ({_inputTrackerConfigurations.arraySize})");
        if (_showTrackers)
        {
            EditorGUI.indentLevel++;
            
            if (_inputTrackerConfigurations.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No trackers configured. Use the Configuration Window to add trackers.", MessageType.Info);
            }
            else
            {
                GUI.enabled = false;
                for (int i = 0; i < _inputTrackerConfigurations.arraySize; i++)
                {
                    var tracker = _inputTrackerConfigurations.GetArrayElementAtIndex(i);
                    var name = tracker.FindPropertyRelative("_name").stringValue;
                    var type = (TrackerType)tracker.FindPropertyRelative("_type").enumValueIndex;
                    var actionPath = tracker.FindPropertyRelative("_actionPath").stringValue;
                    
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField($"{name} ({type})", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Action: {actionPath}");
                    EditorGUILayout.EndVertical();
                }
                GUI.enabled = true;
            }
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space(10);
        
        // Validation Status
        if (!asset.Validate(out var validationErrors))
        {
            EditorGUILayout.HelpBox($"Configuration has {validationErrors.Count} error(s):\n" + string.Join("\n", validationErrors), MessageType.Error);
        }
        else
        {
            EditorGUILayout.HelpBox("Configuration is valid", MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
} 