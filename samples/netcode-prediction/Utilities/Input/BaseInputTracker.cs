using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A generic input value wrapper that provides Unity Input System-like behavior
/// with customizable state management and event handling.
/// </summary>
/// <typeparam name="T">The value type that must be a struct and implement IEquatable</typeparam>
[Serializable]
public abstract class BaseInputTracker<T> : IDisposable where T : struct, IEquatable<T>
{
    #region Fields
    /// <summary>
    /// The dead zone threshold for determining if a value should be considered idle
    /// </summary>
    protected readonly float DeadZone;
    
    /// <summary>
    /// Previous input value for delta comparison
    /// </summary>
    protected T _prevValue;
    
    /// <summary>
    /// Current input value
    /// </summary>
    protected T _value;
    
    /// <summary>
    /// Whether to collapse Started and Performed phases into a single event
    /// </summary>
    private bool _collapseStartedPerformed = true;
    
    /// <summary>
    /// Previous input action phase
    /// </summary>
    protected InputActionPhase _prevPhase;
    
    /// <summary>
    /// Current input action phase
    /// </summary>
    protected InputActionPhase _phase = InputActionPhase.Disabled;
    
    /// <summary>
    /// Lock object for thread safety
    /// </summary>
    private readonly object _lockObject = new object();
    
    /// <summary>
    /// Cached default value to avoid repeated allocations
    /// </summary>
    private static readonly T DefaultValue = default(T);
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the current input value
    /// </summary>
    public T Value 
    { 
        get 
        {
            lock (_lockObject)
            {
                return _value;
            }
        } 
        set => Feed(value); 
    }

    /// <summary>
    /// Gets or sets whether to collapse Started and Performed phases
    /// </summary>
    public bool CollapseStartedPerformed 
    { 
        get 
        {
            lock (_lockObject)
            {
                return _collapseStartedPerformed;
            }
        } 
        set 
        {
            lock (_lockObject)
            {
                _collapseStartedPerformed = value;
            }
        } 
    }

    /// <summary>
    /// Gets the current input action phase
    /// </summary>
    public InputActionPhase Phase 
    { 
        get 
        {
            lock (_lockObject)
            {
                return _phase;
            }
        } 
    }

    /// <summary>
    /// Gets whether the input is currently enabled
    /// </summary>
    public bool IsEnabled => Phase != InputActionPhase.Disabled;
    #endregion

    #region Events
    /// <summary>
    /// Invoked when input transitions from idle to active
    /// </summary>
    public event Action<T> started;
    
    /// <summary>
    /// Invoked when input is actively being performed
    /// </summary>
    public event Action<T> performed;
    
    /// <summary>
    /// Invoked when input transitions from active to idle
    /// </summary>
    public event Action<T> canceled;
    #endregion

    #region Abstract Methods
    /// <summary>
    /// Determines if the given value should be considered idle/inactive
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True if the value is considered idle</returns>
    protected abstract bool IsIdle(in T value);
    #endregion

    #region Virtual Methods
    /// <summary>
    /// Determines if there's a significant difference between two values
    /// </summary>
    /// <param name="a">First value</param>
    /// <param name="b">Second value</param>
    /// <returns>True if values are different enough to trigger events</returns>
    protected virtual bool HasDelta(in T a, in T b) => !a.Equals(b);
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new InputValue with default dead zone
    /// </summary>
    public BaseInputTracker() : this(0.01f)
    {
    }

    /// <summary>
    /// Initializes a new InputValue with specified dead zone
    /// </summary>
    /// <param name="deadZone">The dead zone threshold</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when deadZone is negative</exception>
    public BaseInputTracker(float deadZone)
    {
        if (deadZone < 0f)
            throw new ArgumentOutOfRangeException(nameof(deadZone), "Dead zone cannot be negative");

        DeadZone = deadZone;
        _value = DefaultValue;
        _prevValue = DefaultValue;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Enables input processing
    /// </summary>
    public void Enable()
    {
        lock (_lockObject)
        {
            _phase = _phase == InputActionPhase.Disabled ? InputActionPhase.Waiting : _phase;
        }
    }

    /// <summary>
    /// Disables input processing
    /// </summary>
    public void Disable()
    {
        lock (_lockObject)
        {
            _phase = InputActionPhase.Disabled;
        }
    }

    /// <summary>
    /// Clears transient states (Started, Canceled) back to Waiting
    /// </summary>
    public void ClearTransient()
    {
        lock (_lockObject)
        {
            if (_phase is InputActionPhase.Started or InputActionPhase.Canceled)
            {
                _phase = InputActionPhase.Waiting;
            }
        }
    }

    /// <summary>
    /// Releases all event subscriptions
    /// </summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            started = null;
            performed = null;
            canceled = null;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Feeds a new input value and processes state transitions
    /// </summary>
    /// <param name="newValue">The new input value</param>
    private void Feed(in T newValue)
    {
        lock (_lockObject)
        {
            if (_phase == InputActionPhase.Disabled) 
                return;

            if(newValue.Equals(_value))
                return;

            try
            {
                ProcessStateTransition(newValue);
                UpdateValues(newValue);
                InvokeEvents(newValue);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing input value: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Processes the state transition based on current state and new value
    /// </summary>
    /// <param name="newValue">The new input value</param>
    private void ProcessStateTransition(in T newValue)
    {
        bool isIdle = IsIdle(newValue);
        _prevPhase = _phase;

        _phase = _phase switch
        {
            InputActionPhase.Waiting => HandleWaitingState(isIdle),
            InputActionPhase.Started => HandleStartedState(),
            InputActionPhase.Performed => HandlePerformedState(isIdle, newValue),
            InputActionPhase.Canceled => HandleCanceledState(),
            _ => _phase
        };
    }

    /// <summary>
    /// Handles state transition from Waiting state
    /// </summary>
    /// <param name="isIdle">Whether the new value is idle</param>
    /// <returns>The next phase</returns>
    private InputActionPhase HandleWaitingState(bool isIdle)
    {
        return isIdle ? InputActionPhase.Waiting : InputActionPhase.Started;
    }

    /// <summary>
    /// Handles state transition from Started state
    /// </summary>
    /// <returns>The next phase</returns>
    private InputActionPhase HandleStartedState()
    {
        return InputActionPhase.Performed;
    }

    /// <summary>
    /// Handles state transition from Performed state
    /// </summary>
    /// <param name="isIdle">Whether the new value is idle</param>
    /// <param name="newValue">The new input value</param>
    /// <returns>The next phase</returns>
    private InputActionPhase HandlePerformedState(bool isIdle, in T newValue)
    {
        if (isIdle)
            return InputActionPhase.Canceled;
        
        return HasDelta(_value, newValue) ? InputActionPhase.Performed : _phase;
    }

    /// <summary>
    /// Handles state transition from Canceled state
    /// </summary>
    /// <returns>The next phase</returns>
    private InputActionPhase HandleCanceledState()
    {
        return InputActionPhase.Waiting;
    }

    /// <summary>
    /// Updates the stored values
    /// </summary>
    /// <param name="newValue">The new input value</param>
    private void UpdateValues(in T newValue)
    {
        _prevValue = _value;
        _value = newValue;
    }

    /// <summary>
    /// Invokes appropriate events based on state transitions
    /// </summary>
    /// <param name="newValue">The new input value</param>
    private void InvokeEvents(in T newValue)
    {
        try
        {
            // Handle Started -> Performed (with optional collapse)
            if (_prevPhase == InputActionPhase.Waiting && _phase == InputActionPhase.Started)
            {
                InvokeStarted(newValue);
                
                if (_collapseStartedPerformed)
                {
                    _phase = InputActionPhase.Performed;
                    InvokePerformed(newValue);
                }
                return;
            }

            // Handle explicit Performed events
            if (ShouldInvokePerformed())
            {
                InvokePerformed(newValue);
            }

            // Handle Canceled events
            if (_prevPhase == InputActionPhase.Performed && _phase == InputActionPhase.Canceled)
            {
                InvokeCanceled(newValue);
                _phase = InputActionPhase.Waiting;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error invoking input events for type {typeof(T).Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Determines if Performed event should be invoked
    /// </summary>
    /// <returns>True if Performed should be invoked</returns>
    private bool ShouldInvokePerformed()
    {
        return (_prevPhase == InputActionPhase.Started && _phase == InputActionPhase.Performed) ||
               (_prevPhase == InputActionPhase.Performed && _phase == InputActionPhase.Performed && HasDelta(_prevValue, _value));
    }

    /// <summary>
    /// Safely invokes the started event
    /// </summary>
    /// <param name="value">The input value</param>
    private void InvokeStarted(T value)
    {
        try
        {
            started?.Invoke(value);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in 'started' event handler for {typeof(T).Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Safely invokes the performed event
    /// </summary>
    /// <param name="value">The input value</param>
    private void InvokePerformed(T value)
    {
        try
        {
            performed?.Invoke(value);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in 'performed' event handler for {typeof(T).Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Safely invokes the canceled event
    /// </summary>
    /// <param name="value">The input value</param>
    private void InvokeCanceled(T value)
    {
        try
        {
            canceled?.Invoke(value);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in 'canceled' event handler for {typeof(T).Name}: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    public void DisableActionFor(float seconds)
    {
        DisableAction(seconds).Forget();
    }

    private async UniTaskVoid DisableAction(float seconds)
    {
        Disable();

        await UniTask.Delay(TimeSpan.FromSeconds(seconds));

        Enable();
    }
    #endregion
}
