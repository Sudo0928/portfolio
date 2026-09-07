using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public abstract class BTDecorator : BTNode
    {
        [SerializeReference, InlineProperty, HideLabel, PropertyOrder(1000)]
        protected BTNode _child;
        private bool _entered = false;
        public void SetChild(BTNode child) => _child = child;
        public BTNode GetChild() => _child;

        protected virtual bool CanTick(BTContext ctx) => true;

        public override void OnInit(BTContext ctx)
        {
            _child?.OnInit(ctx);
        }
        protected override void OnEnter(BTContext ctx) { }
        protected override void OnExit(BTContext ctx, ref BTStatus status) { }

        // ✅ 새 훅: 자식 틱 전에 개입해서 Running/Failure/Success를 직접 반환할 수 있음.
        // 기본 구현은 미사용(false).
        protected virtual bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            status = default;
            return false;
        }

        public sealed override BTStatus OnTick(BTContext ctx)
        {
            // ✅ Gate 같은 프리-액션이 필요할 때 여기서 Running/Failure 등을 직접 반환 가능
            if (TryTickOverride(ctx, out var overridden)) return Mark(overridden);
            if (!CanTick(ctx)) { if (_entered) { _child?.Reset(); var s = BTStatus.Failure; OnExit(ctx, ref s); _entered = false; } return Mark(BTStatus.Failure); }
            if (!_entered) { OnEnter(ctx); _entered = true; }
            var result = _child?.OnTick(ctx) ?? BTStatus.Failure;
            if (result != BTStatus.Running) { OnExit(ctx, ref result); if (result != BTStatus.Running) _entered = false; }
            return Mark(result);
        }

        public override void Reset()
        {
            _entered = false;
            _child?.Reset();
        }

        public override BTNode Clone()
        {
            var clonedChild = _child?.Clone();
            return CreateClone(clonedChild);
        }

        protected abstract BTNode CreateClone(BTNode clonedChild);

#if UNITY_EDITOR
        // 자식 Decorator가 인라인으로 붙을 때 좌측에 약간의 이격을 주기 위한 픽셀 값
        private const float __NestedIndentPx = 6f;
        // 부모 박스(컨텐츠 영역)의 시작 위치를 기록해서 스트립을 부모 기준으로 그리기 위함
        private Rect __boxContentStartRect;
        // 자식 영역 시작 지점(자식 포함 아랫 부분만 스트립을 그리기 위해 사용)
        private Rect __childStartRect;
        // 버튼 수직 정렬 미세 보정 (픽셀)
        private const float __BtnVerticalNudge = -6f;
        private const float __PopupWidth = 260f;
        private const float __PopupHeight = 360f; // 대략 값, 화면 밖 방지용 클램프

        private static System.Collections.Generic.List<Type> __GetWrapperTypes()
        {
            var list = new System.Collections.Generic.List<Type>();
            foreach (var t in UnityEditor.TypeCache.GetTypesDerivedFrom<BTNode>())
            {
                if (t.IsAbstract) continue;
                if (typeof(BTActionNode).IsAssignableFrom(t)) continue; // Action 제외
                //if (!typeof(BTDecorator).IsAssignableFrom(t) && !typeof(BTComposite).IsAssignableFrom(t)) continue; // 감싸기 가능한 타입만
                if (t.GetConstructor(Type.EmptyTypes) == null) continue; // 기본 생성자 필요
                list.Add(t);
            }
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return list;
        }

        private void __ShowWrapChildMenu(Rect atRect)
        {
            var types = __GetWrapperTypes();
            // Odin SerializeReference와 동일 팝업 스타일 + 커스텀 라벨(네임스페이스 축약)
            var selector = new Sirenix.OdinInspector.Editor.GenericSelector<Type>(
                string.Empty,
                types,
                false,
                t => __BuildTypePath(t)
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
            // 가로 방향: 뷰 폭 기준으로 오른쪽 넘침 시 좌측으로 이동
            float viewW = UnityEditor.EditorGUIUtility.currentViewWidth;
            if (anchor.x + popupWidth > viewW)
                anchor.x = Mathf.Max(0f, anchor.x - popupWidth);

            // 세로 방향: 현재 포커스된 윈도우 높이를 기준으로 하단 넘침 시 위로 이동
            var win = UnityEditor.EditorWindow.focusedWindow;
            if (win != null)
            {
                float winH = (float)win.position.height;
                if (anchor.y + popupHeight > winH)
                    anchor.y = Mathf.Max(0f, anchor.y - popupHeight);
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

            if (newNode is BTDecorator deco)
            {
                deco.SetChild(original);
            }
            else if (newNode is BTComposite comp)
            {
                if (original != null) comp.Add(original);
            }
            else if (newNode is BTService service)
            {
                service.SetChild(original);
            }

            _child = newNode;
            GUI.changed = true;
            var target = UnityEditor.Selection.activeObject;
            if (target != null)
            {
                UnityEditor.Undo.RecordObject(target, "Wrap BTNode");
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }
        // Composite의 OnBeginListElementGUI와 동일 컨셉: 요소 앞에 박스 시작
        [OnInspectorGUI, PropertyOrder(-10000)]
        private void __BeginChildBox()
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox();
            // 부모 박스 컨텐츠 영역의 시작 좌표를 확보
            __boxContentStartRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));

            // 내부 컨텐츠에 소폭 들여쓰기(이격) 적용
            GUILayout.BeginHorizontal();
            GUILayout.Space(__NestedIndentPx);
            GUILayout.BeginVertical();
        }

        // 자식 그리기 직전의 레이아웃 위치를 기록하여, 스트립을 자식 포함 아랫 부분부터만 그린다
        [OnInspectorGUI, PropertyOrder(999)]
        private void __MarkChildStart()
        {
            __childStartRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
        }

        // 요소가 모두 그려진 뒤 마지막 Rect에 얇은 좌측 스트립만 색상 적용
        [OnInspectorGUI, PropertyOrder(10000)]
        private void __EndChildBox()
        {
            var contentRect = GUILayoutUtility.GetLastRect();
            // 컨텐츠 영역 닫기
            GUILayout.EndVertical();
            // 컨텐츠와 버튼 사이 이격 1px, 버튼 폭은 딱 버튼 크기만 확보
            GUILayout.Space(1f);
            var btnLayoutRect = GUILayoutUtility.GetRect(16f, 16f, GUILayout.Width(16f));
            // 레이아웃 그룹 닫기
            GUILayout.EndHorizontal();

            var color = _child != null ? _child.GetStatusColor() : Color.gray;

            // 부모 박스의 실제 컨텐츠 좌측을 기준으로 스트립을 그리되,
            // y 범위는 자식이 시작되는 지점부터(자식 포함 아랫 부분만) 하단까지로 제한
            float yMin = (_child != null ? __childStartRect.y : __boxContentStartRect.y) + 3f;
            float yMax = contentRect.yMax - 3f;
            float h = Mathf.Max(1f, yMax - yMin);
            var strip = new Rect(__boxContentStartRect.x + 3f, yMin, 4f, h);
            UnityEditor.EditorGUI.DrawRect(strip, color);

            // 우측 [+] 버튼 (자식 헤더 수직 중앙 정렬)
            float btnSize = 16f;
            float headerY = (_child != null ? __childStartRect.y : __boxContentStartRect.y) + 6f;
            float headerH = UnityEditor.EditorGUIUtility.singleLineHeight + 4f;
            float btnY = Mathf.Round(headerY + (headerH - btnSize) * 0.5f + __BtnVerticalNudge);
            var btnRect = new Rect(btnLayoutRect.x, btnY, btnSize, btnSize);
            if (GUI.Button(btnRect, "+"))
            {
                __ShowWrapChildMenu(btnRect);
            }

            // (선택) 헤더 라인만 살짝 덧칠하고 싶을 때:
            // var headerH = UnityEditor.EditorGUIUtility.singleLineHeight + 4f;
            // var header = new Rect(__boxContentStartRect.x + 8f, __boxContentStartRect.y + 6f, lastRect.width - 16f, headerH);
            // var headerColor = new Color(color.r, color.g, color.b, 0.18f);
            // UnityEditor.EditorGUI.DrawRect(header, headerColor);

            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }

        public override void OnDrawGizmos(GameObject obj)
        {
            _child?.OnDrawGizmos(obj);
        }
#endif 
    }
}