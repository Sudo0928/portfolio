using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputTracker", menuName = "Input/InputTracker")]
public class InputTrackerAsset : ScriptableObject
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActionAsset;

    [Header("Generated Code Settings")]
    [SerializeField] private string _generatedCodePath;
    [SerializeField] private string _generatedCodeClassName;
    [SerializeField] private string _generatedCodeNamespace;

    [Header("Trackers")]
    [SerializeField] private List<InputTrackerConfiguration> _inputTrackerConfigurations = new List<InputTrackerConfiguration>();

    public InputActionAsset InputActionAsset => inputActionAsset;
    public string GeneratedCodePath => _generatedCodePath;
    public string GeneratedCodeClassName => _generatedCodeClassName;
    public string GeneratedCodeNamespace => _generatedCodeNamespace;
    public List<InputTrackerConfiguration> InputTrackerConfigurations => _inputTrackerConfigurations;

    /// <summary>
    /// 트래커 추가
    /// </summary>
    public void AddTracker(InputTrackerConfiguration tracker)
    {
        if (_inputTrackerConfigurations.Any(t => t.Name == tracker.Name))
        {
            Debug.LogWarning($"Tracker with name '{tracker.Name}' already exists!");
            return;
        }
        
        _inputTrackerConfigurations.Add(tracker);
    }
    
    /// <summary>
    /// 트래커 제거
    /// </summary>
    public void RemoveTracker(string name)
    {
        _inputTrackerConfigurations.RemoveAll(t => t.Name == name);
    }
    
    /// <summary>
    /// 사용 가능한 InputAction 목록 가져오기
    /// </summary>
    public List<string> GetAvailableActions()
    {
        if (inputActionAsset == null) return new List<string>();
        
        var actions = new List<string>();
        foreach (var actionMap in inputActionAsset.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                actions.Add($"{actionMap.name}/{action.name}");
            }
        }
        return actions;
    }
    
    /// <summary>
    /// 유효성 검사
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();
        
        if (inputActionAsset == null)
            errors.Add("InputActionAsset is not assigned");
        
        if (string.IsNullOrEmpty(GeneratedCodeClassName))
            errors.Add("Generated class name is empty");
        
        if (string.IsNullOrEmpty(GeneratedCodePath))
            errors.Add("Output path is empty");
        
        // 중복 이름 검사
        var duplicates = _inputTrackerConfigurations.GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        
        foreach (var duplicate in duplicates)
            errors.Add($"Duplicate tracker name: {duplicate}");
        
        // 존재하지 않는 액션 검사
        var availableActions = GetAvailableActions();
        foreach (var tracker in _inputTrackerConfigurations)
        {
            if (!availableActions.Contains(tracker.ActionPath))
                errors.Add($"Action '{tracker.ActionPath}' not found in InputActionAsset");
        }
        
        return errors.Count == 0;
    }
}

[System.Serializable]
public class InputTrackerConfiguration
{
    [SerializeField] private string _name;
    [SerializeField] private string _actionPath;
    [SerializeField] private TrackerType _type;
    [SerializeField] private float _deadZone = 0.01f;
    [SerializeField] private bool _collapseStartedPerformed = true;
    [SerializeField] private string _description;

    public string Name => _name;
    public string ActionPath => _actionPath;
    public TrackerType Type => _type;
    public float DeadZone => _deadZone;
    public bool CollapseStartedPerformed => _collapseStartedPerformed;
    public string Description => _description;

    public InputTrackerConfiguration(string name, string actionPath, TrackerType type)
    {
        _name = name;
        _actionPath = actionPath;
        _type = type;
    }
}

public enum TrackerType
{
    Button,
    Axis,
    Stick
}