using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ProjectRaid.Runtime.BehaviourTree;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public class Vector3Value : Value<Vector3>
    {
        public Vector3Value() { value = Vector3.zero; }

        public override string ValueString
        {
            get
            {
                return $"({value.x:G7}, {value.y:G7}, {value.z:G7})";
            }
            set
            {
                if(!CanParse(value, out var vector3)) return;
                this.value = vector3;
            }
        }

        public override void SetValue(string value)
        {
            if(CanParse(value, out var vector3))
            {
                this.value = vector3;
            }
        }

        public override bool CanParse(string s)
        {
            return CanParse(s, out var vector3);
        }

        public bool CanParse(string value, out Vector3 vector3)
        {
            var parts = value.Replace("(", "").Replace(")", "").Replace(" ", "").Split(',');
            try
            {
                float[] floats = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    floats[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);
                }
                vector3 = new Vector3(floats[0], floats[1], floats[2]);
            } catch { vector3 = Vector3.zero; return false; }
            return true;
        }
    }
}
