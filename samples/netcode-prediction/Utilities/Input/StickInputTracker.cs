using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickInputTracker : BaseInputTracker<Vector2>
{
    public StickInputTracker(float deadZone = 0.1f) : base(deadZone) { }

    protected override bool IsIdle(in Vector2 value)
    {
        return value.magnitude <= DeadZone;
    }

    protected override bool HasDelta(in Vector2 a, in Vector2 b)
    {
        return Vector2.Distance(a, b) > 0.01f;
    }
}
