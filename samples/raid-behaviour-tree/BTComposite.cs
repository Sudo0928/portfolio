using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public abstract class BTComposite : BTNode
    {
        protected int _currentIndex = 0;

        [SerializeReference, HideLabel, PropertyOrder(1000)]
        [ListDrawerSettings(OnBeginListElementGUI = "OnBeginChildGUI", OnEndListElementGUI = "OnEndChildGUI")]
        protected List<BTNode> _children = new();
        public void Add(BTNode child) => _children.Add(child);
        public void Insert(BTNode child, int index) => _children.Insert(index, child);
        public void Remove(BTNode child) => _children.Remove(child);
        public void RemoveAt(int index) => _children.RemoveAt(index);
        public void Clear() => _children.Clear();
        public IReadOnlyList<BTNode> GetList() => _children;
        public override void Reset()
        {
            _currentIndex = 0;
            foreach (var child in _children)
            {
                child.Reset();
            }
        }

        public override void OnInit(BTContext ctx)
        {
            foreach (var child in _children)
            {
                child.OnInit(ctx);
            }
        }

        public void DecorateChildAt(int index, Func<BTNode, BTDecorator> factory)
        {
            var original = _children[index];
            var decorator = factory(original);
            if (decorator.GetChild() == null) decorator.SetChild(original);
            _children[index] = decorator;
        }

        public bool DecorateChild(BTNode child, Func<BTNode, BTDecorator> factory)
        {
            var i = _children.IndexOf(child);
            if (i == -1) return false;
            DecorateChildAt(i, factory);
            return true;
        }

#if UNITY_EDITOR
        private Rect __elementStartRect; // 요소 박스의 시작 위치(헤더 정렬 기준)
        private const float __BtnVerticalNudge = -6f; // 버튼 수직 정렬 미세 보정
        private const float __PopupWidth = 260f;
        private const float __PopupHeight = 360f;

        private static List<Type> __GetWrapperTypes()
        {
            var list = new List<Type>();
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

        private void __ShowWrapChildMenu(int index, Rect atRect)
        {
            var types = __GetWrapperTypes();
            var selector = new Sirenix.OdinInspector.Editor.GenericSelector<Type>(
                string.Empty,
                types,
                false,
                t => __BuildTypePath(t)
            );
            selector.EnableSingleClickToSelect();
            selector.SelectionConfirmed += sel =>
            {
                foreach (var picked in sel) { __WrapChildWith(index, picked); break; }
            };
            var popupRect = __ClampPopupRect(atRect, __PopupWidth, __PopupHeight);
            selector.ShowInPopup(popupRect);
        }

        private static Rect __ClampPopupRect(Rect anchor, float popupWidth, float popupHeight)
        {
            anchor.width = popupWidth;
            float viewW = UnityEditor.EditorGUIUtility.currentViewWidth;
            if (anchor.x + popupWidth > viewW)
                anchor.x = Mathf.Max(0f, anchor.x - popupWidth);

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

        private void __WrapChildWith(int index, Type t)
        {
            if (index < 0 || index >= _children.Count) return;
            var original = _children[index];
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

            _children[index] = newNode;
            GUI.changed = true;
            var target = UnityEditor.Selection.activeObject;
            if (target != null)
            {
                UnityEditor.Undo.RecordObject(target, "Wrap BTNode");
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }
        private void OnBeginChildGUI(int index)
        {
            // 기본 박스 스타일만 사용 (색 푸시하지 않음)
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox();
            // 헤더가 위치할 요소 박스 시작 Y를 확보
            __elementStartRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
            // 수평 레이아웃: 좌측 컨텐츠, 우측 버튼 고정 영역
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
        }

        private void OnEndChildGUI(int index)
        {
            // 컨텐츠 영역의 마지막 Rect(노드 본문 크기)
            var contentRect = GUILayoutUtility.GetLastRect();
            // 컨텐츠 세로 영역 종료 후, 컨텐츠와 버튼 사이 이격 1px, 버튼 폭은 딱 버튼 크기만 확보
            GUILayout.EndVertical();
            GUILayout.Space(1f);
            var btnLayoutRect = GUILayoutUtility.GetRect(16f, 16f, GUILayout.Width(16f));
            // 수평 레이아웃 종료
            GUILayout.EndHorizontal();
            var node = (index >= 0 && index < _children.Count) ? _children[index] : null;
            var color = node != null ? node.GetStatusColor() : Color.gray;

            // 1) 왼쪽 얇은 상태 스트립만 색칠 (컨텐츠 기준)
            float yMin = contentRect.y + 3f;
            float yMax = contentRect.yMax - 3f;
            float h = Mathf.Max(1f, yMax - yMin);
            var strip = new Rect(contentRect.x + 3f, yMin, 4f, h);
            UnityEditor.EditorGUI.DrawRect(strip, color);

            // 1.5) 우측 [+] 버튼 (헤더 라인 수직 중앙 정렬)
            float btnSize = 16f;
            float headerY = __elementStartRect.y + 6f;
            float headerH = UnityEditor.EditorGUIUtility.singleLineHeight + 4f;
            float btnY = Mathf.Round(headerY + (headerH - btnSize) * 0.5f + __BtnVerticalNudge);
            var btnRect = new Rect(btnLayoutRect.x, btnY, btnSize, btnSize);
            if (GUI.Button(btnRect, "+"))
            {
                __ShowWrapChildMenu(index, btnRect);
            }

            // 2) 헤더 라인만 살짝 색칠하고 싶다면(선택):
            // var headerH = UnityEditor.EditorGUIUtility.singleLineHeight + 4f;
            // var header = new Rect(rect.x + 8, rect.y + 6, rect.width - 16, headerH);
            // var headerColor = new Color(color.r, color.g, color.b, 0.18f);
            // UnityEditor.EditorGUI.DrawRect(header, headerColor);

            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }

        public override void OnDrawGizmos(GameObject obj)
        {
            foreach (var child in _children)
            {
                child.OnDrawGizmos(obj);
            }
        }
#endif
    }
}