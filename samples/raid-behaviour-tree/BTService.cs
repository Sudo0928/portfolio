using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectRaid.Runtime.BehaviourTree
{
    /// <summary>
    /// Behavior Tree Service 노드. 자식을 감싸면서 주기적으로 Execute 로직을 수행합니다.
    /// </summary>
    [Serializable]
    public abstract class BTService : BTNode
    {
        [Title("Service Settings")]
        [EnumToggleButtons, LabelText("Execution Policy")]
        public ServicePolicy policy = ServicePolicy.EveryTick;

        [ShowIf("@policy == ServicePolicy.Interval")]
        [LabelText("Interval (seconds)"), MinValue(0.0001f)]
        public float interval = 1f;

        // Interval일 때, 자식의 한 회차가 끝나도 경과 시간을 보존할지(= 누적 유지)
        [ShowIf("@policy == ServicePolicy.Interval")]
        [LabelText("Accumulate Across Runs")]
        public bool accumulateAcrossRuns = true;

        // Interval일 때, 진입 즉시 1회 실행할지
        [ShowIf("@policy == ServicePolicy.Interval")]
        [LabelText("Execute On Enter (Interval)")]
        public bool executeOnEnterForInterval = false;

        [NonSerialized] private float _elapsedTime = 0f;
        [NonSerialized] private bool _entered = false;

        [SerializeReference, InlineProperty, HideLabel, PropertyOrder(1000)]
        protected BTNode _child;

        public void SetChild(BTNode child) => _child = child;
        public BTNode GetChild() => _child;

        public override void OnInit(BTContext ctx)
        {
            _child?.OnInit(ctx);
        }

        protected override void OnEnter(BTContext ctx)
        {
            // Interval: 회차 시작 시 즉시 실행 옵션
            if (policy == ServicePolicy.Interval && executeOnEnterForInterval)
                Execute(ctx);

            // OnEnterOnce: 진입 시 1회 실행
            if (policy == ServicePolicy.OnEnterOnce)
                Execute(ctx);

            // Interval 누적 유지가 꺼져있다면 회차 시작에 타이머 초기화
            if (!(policy == ServicePolicy.Interval && accumulateAcrossRuns))
                _elapsedTime = 0f;
        }

        public sealed override BTStatus OnTick(BTContext ctx)
        {
            if (!_entered)
            {
                OnEnter(ctx);
                _entered = true;
            }

            // 실행 정책에 따른 서비스 호출
            switch (policy)
            {
                case ServicePolicy.EveryTick:
                    Execute(ctx);
                    break;

                case ServicePolicy.Interval:
                {
                    var dt = ctx.DeltaTime;
                    if (dt <= 0f) dt = Time.fixedDeltaTime; // 안전장치

                    _elapsedTime += dt;
                    // interval 보호(에디터에서 0으로 설정된 경우 대비)
                    var iv = (interval <= 0f) ? 0.0001f : interval;

                    if (_elapsedTime >= iv)
                    {
                        Execute(ctx);
                        _elapsedTime -= iv; // 정확도를 위해 잔여분 남김(누적 오차 감소)
                        // 필요 시 깔끔히 0으로 초기화하려면 아래로 교체:
                        // _elapsedTime = 0f;
                    }
                    break;
                }

                case ServicePolicy.OnEnterOnce:
                    // OnEnter에서 이미 1회 실행
                    break;
            }

            // 자식 노드 실행
            var result = _child?.OnTick(ctx) ?? BTStatus.Failure;

            if (result != BTStatus.Running)
            {
                OnExit(ctx, ref result);

                // 다음 회차로 넘어가되, Interval+누적유지면 시간은 보존
                if (!(policy == ServicePolicy.Interval && accumulateAcrossRuns))
                    _elapsedTime = 0f;

                _entered = false;
            }

            return Mark(result);
        }

        public override void Reset()
        {
            _entered = false;
            _elapsedTime = 0f;
            _child?.Reset();
        }

        /// <summary>
        /// 자식 실행 이전에 주기적으로 호출되는 서비스 로직.
        /// </summary>
        protected abstract void Execute(BTContext ctx);

#if UNITY_EDITOR
        // ===== 아래는 에디터 편의(랩핑 UI) =====
        private const float __NestedIndentPx = 6f;
        private Rect __boxContentStartRect;
        private Rect __childStartRect;
        private const float __BtnVerticalNudge = -6f;
        private const float __PopupWidth = 260f;
        private const float __PopupHeight = 360f;

        private static System.Collections.Generic.List<Type> __GetWrapperTypes()
        {
            var list = new System.Collections.Generic.List<Type>();
            foreach (var t in UnityEditor.TypeCache.GetTypesDerivedFrom<BTNode>())
            {
                if (t.IsAbstract) continue;
                if (typeof(BTActionNode).IsAssignableFrom(t)) continue; // Action 제외
                if (t.GetConstructor(Type.EmptyTypes) == null) continue; // 기본 생성자 필요
                list.Add(t);
            }
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return list;
        }

        private void __ShowWrapChildMenu(Rect atRect)
        {
            var types = __GetWrapperTypes();
            var selector = new Sirenix.OdinInspector.Editor.GenericSelector<Type>(
                string.Empty, types, false, t => __BuildTypePath(t)
            );
            selector.EnableSingleClickToSelect();
            selector.SelectionConfirmed += sel =>
            {
                foreach (var picked in sel) { __WrapChildWith(picked); break; }
            };
            var popupRect = __ClampPopupRect(atRect, __PopupWidth, __PopupHeight);
            selector.ShowInPopup(popupRect);
        }

        private static Rect __ClampPopupRect(Rect anchor, float popupWidth, float popupHeight)
        {
            anchor.width = popupWidth;
            float viewW = UnityEditor.EditorGUIUtility.currentViewWidth;
            if (anchor.x + popupWidth > viewW) anchor.x = Mathf.Max(0f, anchor.x - popupWidth);
            var win = UnityEditor.EditorWindow.focusedWindow;
            if (win != null)
            {
                float winH = (float)win.position.height;
                if (anchor.y + popupHeight > winH) anchor.y = Mathf.Max(0f, anchor.y - popupHeight);
            }
            return anchor;
        }

        private static string __BuildTypePath(Type t)
        {
            var baseNs = typeof(BTNode).Namespace ?? string.Empty;
            var ns = t.Namespace ?? string.Empty;
            var trimmedNs = ns.StartsWith(baseNs, StringComparison.Ordinal) ? ns.Substring(baseNs.Length).TrimStart('.') : ns;
            var name = t.Name;
            if (name.EndsWith("Node", StringComparison.Ordinal)) name = name.Substring(0, name.Length - 4);
            if (string.IsNullOrEmpty(trimmedNs)) return name;
            return trimmedNs.Replace('.', '/') + "/" + name;
        }

        private void __WrapChildWith(Type t)
        {
            var original = _child;
            var newNode = Activator.CreateInstance(t) as BTNode;
            if (newNode == null) return;

            if (newNode is BTDecorator deco) deco.SetChild(original);
            else if (newNode is BTComposite comp) { if (original != null) comp.Add(original); }
            else if (newNode is BTService service) service.SetChild(original);

            _child = newNode;
            GUI.changed = true;
            var target = UnityEditor.Selection.activeObject;
            if (target != null)
            {
                UnityEditor.Undo.RecordObject(target, "Wrap BTNode");
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }

        [OnInspectorGUI, PropertyOrder(-10000)]
        private void __BeginChildBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox();
            __boxContentStartRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            GUILayout.Space(__NestedIndentPx);
            GUILayout.BeginVertical();
        }

        [OnInspectorGUI, PropertyOrder(999)]
        private void __MarkChildStart()
        {
            __childStartRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
        }

        [OnInspectorGUI, PropertyOrder(10000)]
        private void __EndChildBox()
        {
            var contentRect = GUILayoutUtility.GetLastRect();
            GUILayout.EndVertical();
            GUILayout.Space(1f);
            var btnLayoutRect = GUILayoutUtility.GetRect(16f, 16f, GUILayout.Width(16f));
            GUILayout.EndHorizontal();

            var color = _child != null ? _child.GetStatusColor() : Color.gray;
            float yMin = (_child != null ? __childStartRect.y : __boxContentStartRect.y) + 3f;
            float yMax = contentRect.yMax - 3f;
            float h = Mathf.Max(1f, yMax - yMin);
            var strip = new Rect(__boxContentStartRect.x + 3f, yMin, 4f, h);
            UnityEditor.EditorGUI.DrawRect(strip, color);

            float btnSize = 16f;
            float headerY = (_child != null ? __childStartRect.y : __boxContentStartRect.y) + 6f;
            float headerH = UnityEditor.EditorGUIUtility.singleLineHeight + 4f;
            float btnY = Mathf.Round(headerY + (headerH - btnSize) * 0.5f + __BtnVerticalNudge);
            var btnRect = new Rect(btnLayoutRect.x, btnY, btnSize, btnSize);
            if (GUI.Button(btnRect, "+")) __ShowWrapChildMenu(btnRect);

            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }

        public override void OnDrawGizmos(GameObject obj)
        {
            _child?.OnDrawGizmos(obj);
        }
#endif
    }

    public enum ServicePolicy
    {
        EveryTick,
        Interval,
        OnEnterOnce
    }
}
