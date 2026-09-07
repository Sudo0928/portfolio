using System;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public abstract class BTNode
    {
        [SerializeField, HideInInspector] private string _description;
        public abstract void OnInit(BTContext ctx);
        protected virtual void OnEnter(BTContext ctx) { }
        public abstract BTStatus OnTick(BTContext ctx);
        protected virtual void OnExit(BTContext ctx, ref BTStatus status) { }
        public virtual void Reset() { }
        public abstract BTNode Clone();

#if UNITY_EDITOR
        private const int HoldFrames = 5;
        [NonSerialized] public BTStatus status = (BTStatus)(-1); // Unticked state sentinel
        [NonSerialized] public int frame = -1;

        public Color GetStatusColor()
        {
            // 아직 한 번도 틱되지 않았거나, 최근 프레임 갱신이 오래되었으면 회색
            if ((int)status == -1) return Color.gray;
            if (frame < 0 || Time.frameCount - frame > HoldFrames) return Color.gray;

            switch (status)
            {
                case BTStatus.Running: return Color.yellow;
                case BTStatus.Success: return Color.green;
                case BTStatus.Failure: return Color.red;
            }
            return Color.gray;
        }

        public virtual void OnDrawGizmos(GameObject obj) {}
#endif

        protected BTStatus Mark(BTStatus status)
        {
#if UNITY_EDITOR
            if(this.status != status)
            {
                if (Application.isPlaying)
                    Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            }

            this.status = status;
            this.frame = Time.frameCount;
#endif
            return status;
        }
    }
}
