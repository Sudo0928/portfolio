using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInputTracker : BaseInputTracker<bool>
{
    public ButtonInputTracker() : base() { }

    protected override bool IsIdle(in bool value)
    {
        return !value;
    }
}
