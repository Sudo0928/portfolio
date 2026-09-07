using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisInputTracker : BaseInputTracker<float>
{
    public AxisInputTracker(float deadZone = 0.1f) : base(deadZone) { }

    protected override bool IsIdle(in float value)
    {
        return Mathf.Abs(value) <= DeadZone;
    }
}
