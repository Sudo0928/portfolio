// Guard.cs — Guard 실패 시 Fallback 실행 기능 + 트리거 모드/토글 추가 버전
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ProjectRaid.Character.Module;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    [Serializable]
    public sealed class Guard : BTDecorator
    {
        [SerializeField, EnumToggleButtons, LabelText("Mode")]
        private ConditionMode _mode = ConditionMode.And;

        [SerializeField, LabelText("Editor Keys (Optional)")]
        private BTBlackBoard _editorKeys;

        [SerializeField, ListDrawerSettings(DraggableItems = true, ShowFoldout = true)]
        private List<GuardCondition> _conditions = new();

        // ====== Fallback 확장 ======
        public enum BlockedReturnPolicy { Failure, Success, FallbackResult }
        public enum FallbackTriggerMode
        {
            AlwaysOnGuardFailure,           // Guard 실패면 무조건 Fallback
            OnlyWhenAbortingFromRunning     // 직전 틱에 자식을 돌리던 중(Running) Guard가 실패로 바뀔 때만
        }

        [Title("Fallback on Guard-Blocked")]
        [ToggleLeft, LabelText("Use Fallback")]
        [SerializeField] private bool _useFallback = true;

        [ShowIf(nameof(_useFallback))]
        [SerializeReference, InlineProperty, HideLabel]
        private BTNode _onBlocked;

        [ShowIf(nameof(_useFallback))]
        [LabelText("Trigger")]
        [SerializeField] private FallbackTriggerMode _triggerMode = FallbackTriggerMode.AlwaysOnGuardFailure;

        [ShowIf(nameof(_useFallback))]
        [LabelText("Blocked Return")]
        [SerializeField] private BlockedReturnPolicy _blockedReturn = BlockedReturnPolicy.Failure;

        // ====== 런타임 캐시/상태 ======
        [NonSerialized] private Func<IBlackboard, bool> _compiledEval;

        // 한 틱 내 중복 Guard 평가 방지
        [NonSerialized] private bool _checkedThisTick;
        [NonSerialized] private bool _cachedCanTick;

        // Fallback 진행 중 표시
        [NonSerialized] private bool _fallbackActive;

        // "직전 틱에 자식을 돌리고 있었는지" 추적 (Abort 판별용)
        // Guard 통과하여 자식으로 넘길 때 true, 트리 종료/중단/리셋 시 false
        [NonSerialized] private bool _wasChildTicking;

        public override void OnInit(BTContext ctx)
        {
            if (_editorKeys == null && ctx?.Blackboard is BTBlackBoard btb)
                _editorKeys = btb;

            _compiledEval = CompileGuardTree(_mode, _conditions);

            _onBlocked?.OnInit(ctx);
            base.OnInit(ctx);
        }

        // base.OnTick의 CanTick 호출 전에 TryTickOverride로 Guard를 선평가 하므로,
        // 여기서는 캐시된 값을 그대로 돌려준다.
        protected override bool CanTick(BTContext ctx)
        {
            if (_checkedThisTick)
            {
                var v = _cachedCanTick;
                _checkedThisTick = false; // 소비
                return v;
            }
            return _compiledEval == null ? true : _compiledEval(ctx.Blackboard);
        }

        // === Guard 실패 처리(옵션 포함)의 핵심 ===
        protected override bool TryTickOverride(BTContext ctx, out BTStatus status)
        {
            // Guard 선평가 (한 틱 1회)
            if (!_checkedThisTick)
            {
                _cachedCanTick = _compiledEval == null ? true : _compiledEval(ctx.Blackboard);
                _checkedThisTick = true;
            }

            if (_cachedCanTick)
            {
                // 자식을 틱하게 되므로 "자식 실행 중" 상태 기록
                _wasChildTicking = true;
                status = default;
                return false; // 자식으로 진행
            }

            // === 여기서부터 Guard 실패 ===
            bool shouldRunFallback = false;

            if (_useFallback)
            {
                if (_triggerMode == FallbackTriggerMode.AlwaysOnGuardFailure)
                {
                    shouldRunFallback = true;
                }
                else // OnlyWhenAbortingFromRunning
                {
                    // 바로 전 프레임에 자식을 돌리고 있었는데(=Running 페이즈),
                    // 이번 프레임에 Guard가 실패로 변했다면 Abort → Fallback
                    shouldRunFallback = _wasChildTicking;
                }
            }

            if (!shouldRunFallback)
            {
                // Fallback 미사용 혹은 트리거 조건 불충족 → 즉시 Failure 반환(기존 Guard와 동일한 거동)
                _fallbackActive = false;
                _wasChildTicking = false; // 흐름 끊김
                status = BTStatus.Failure;
                return true;
            }

            // === Fallback 실행 경로 ===
            _fallbackActive = true;
            _wasChildTicking = false; // 자식은 돌지 않음

            _child.Reset();

            if (_onBlocked == null)
            {
                status = DecideFinalStatus(BTStatus.Failure);
                _fallbackActive = false;
                return true;
            }

            var s = _onBlocked.OnTick(ctx);
            if (s == BTStatus.Running)
            {
                status = BTStatus.Running;
                return true; // Fallback이 끝날 때까지 대기
            }

            // 완료 → 리셋 후 정책에 따른 최종 리턴
            _onBlocked.Reset();
            _fallbackActive = false;
            status = DecideFinalStatus(s);
            return true;
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            // 자식이 Running이 끝났다는 뜻이므로 child-running 플래그를 내린다.
            _wasChildTicking = false;

            if (_fallbackActive)
            {
                _onBlocked?.Reset();
                _fallbackActive = false;
            }
        }

        public override void Reset()
        {
            base.Reset();
            _onBlocked?.Reset();
            _fallbackActive = false;
            _checkedThisTick = false;
            _wasChildTicking = false;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Guard
            {
                _child = clonedChild,
                _mode = _mode,
                _editorKeys = _editorKeys,
                _conditions = _conditions?.Select(x => x?.Clone()).ToList(),
                _useFallback = _useFallback,
                _onBlocked = _onBlocked?.Clone(),
                _triggerMode = _triggerMode,
                _blockedReturn = _blockedReturn
            };
        }

        private BTStatus DecideFinalStatus(BTStatus fallbackStatus)
        {
            return _blockedReturn switch
            {
                BlockedReturnPolicy.Success => BTStatus.Success,
                BlockedReturnPolicy.FallbackResult =>
                    (fallbackStatus == BTStatus.Running) ? BTStatus.Running : fallbackStatus,
                _ => BTStatus.Failure
            };
        }

#if UNITY_EDITOR
        [OnInspectorGUI, PropertyOrder(-9999)]
        private void __WireEditorKeySource()
        {
            if (_conditions == null) return;
            foreach (var c in _conditions) c?.__SetEditorKeys(_editorKeys);
        }
#endif

        // ======== 이하: 기존 Guard 조건 컴파일러/유틸 ========

        private static Func<IBlackboard, bool> CompileGuardTree(ConditionMode mode, List<GuardCondition> list)
        {
            if (list == null || list.Count == 0)
                return _ => true;

            var evals = list.Select(c => c.Compile()).ToArray();
            if (mode == ConditionMode.And)
                return bb => { for (int i = 0; i < evals.Length; i++) if (!evals[i](bb)) return false; return true; };
            else
                return bb => { for (int i = 0; i < evals.Length; i++) if (evals[i](bb)) return true; return false; };
        }
    }

    public enum GuardConditionKind { Predicate, Group }
    public enum ConditionMode { And, Or }
    public enum CompareOperator
    {
        Exists, NotExists, Equal, NotEqual, Less, LessOrEqual, GreaterOrEqual, Greater,
    }

    [Serializable]
    public sealed class GuardCondition
    {
        [SerializeField, EnumToggleButtons, LabelText("Kind")]
        private GuardConditionKind _kind = GuardConditionKind.Predicate;

        [SerializeField, ShowIf("@_kind == GuardConditionKind.Group"), EnumToggleButtons, LabelText("Group Mode")]
        private ConditionMode _groupMode = ConditionMode.And;

        [SerializeField, ShowIf("@_kind == GuardConditionKind.Group"),
         ListDrawerSettings(DraggableItems = true, ShowFoldout = true)]
        private List<GuardCondition> _children = new();

        [ShowIf("@_kind == GuardConditionKind.Predicate && _op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [ValueDropdown("GetKeyOptions"), LabelText("Key")]
        [SerializeField] private string _lhsKey;

        [ShowIf("@_kind == GuardConditionKind.Predicate && (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)")]
        [LabelText("Key (manual)")]
        [SerializeField] private string _lhsKeyManual;

        [ShowIf("@_kind == GuardConditionKind.Predicate")]
        [ValueDropdown("GetOperatorOptions"), LabelText("Op")]
        [SerializeField] private CompareOperator _op = CompareOperator.Equal;

        [ShowIf("@_kind == GuardConditionKind.Predicate && _op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField, LabelText("Compare to Other Key?")]
        private bool _compareWithKey = false;

        [ShowIf("@_kind == GuardConditionKind.Predicate && _compareWithKey && _op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [ValueDropdown("GetComparableKeyOptions"), LabelText("Other Key")]
        [SerializeField] private string _rhsKey;

        [ShowIf("@_kind == GuardConditionKind.Predicate && !_compareWithKey && _op != CompareOperator.Exists && _op != CompareOperator.NotExists")]
        [SerializeField, InlineProperty, HideLabel]
        private BlackboardHolder _rhsConst = new BlackboardHolder();

        [NonSerialized] private BTBlackBoard __editorKeys;

        // --- 이하 Compile() 등 기존 로직 유지 ---
        private enum ValueTypeTag { Bool, Enum, String, Vector3, Numeric, Other }
        private static ValueTypeTag GetValueTypeTag(Type t)
        {
            if (t == typeof(bool)) return ValueTypeTag.Bool;
            if (t == typeof(string)) return ValueTypeTag.String;
            if (t == typeof(Vector3)) return ValueTypeTag.Vector3;
            if (t.IsEnum) return ValueTypeTag.Enum;
            if (t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double)) return ValueTypeTag.Numeric;
            return ValueTypeTag.Other;
        }

        public Func<IBlackboard, bool> Compile()
        {
            if (_kind == GuardConditionKind.Group)
            {
                var compiled = _children?.Select(c => c.Compile()).ToArray() ?? Array.Empty<Func<IBlackboard, bool>>();
                if (_groupMode == ConditionMode.And)
                    return bb => { for (int i = 0; i < compiled.Length; i++) if (!compiled[i](bb)) return false; return true; };
                else
                    return bb => { for (int i = 0; i < compiled.Length; i++) if (compiled[i](bb)) return true; return false; };
            }

            string key = CurrentKey();

            if (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)
            {
                bool want = (_op == CompareOperator.Exists);
                return bb => string.IsNullOrEmpty(key) ? true : (KeyExists(bb, key) == want);
            }

            object rhsConstObj = null;
            Type rhsConstType = null;
            if (!_compareWithKey)
            {
                var v = _rhsConst?._blackBoard;
                if (v != null)
                {
                    rhsConstType = v.Type;
                    rhsConstObj = ParseStringToType(v.ValueString, rhsConstType);
                }
            }

            Type lhsType = null;
            Type rhsType = null;
            return bb =>
            {
                if (string.IsNullOrEmpty(key)) return true;

                if (lhsType == null)
                {
                    if (bb.TryGet<int>(key, out _)) lhsType = typeof(int);
                    else if (bb.TryGet<long>(key, out _)) lhsType = typeof(long);
                    else if (bb.TryGet<float>(key, out _)) lhsType = typeof(float);
                    else if (bb.TryGet<double>(key, out _)) lhsType = typeof(double);
                    else if (bb.TryGet<bool>(key, out _)) lhsType = typeof(bool);
                    else if (bb.TryGet<string>(key, out _)) lhsType = typeof(string);
                    else if (bb.TryGet<Vector3>(key, out _)) lhsType = typeof(Vector3);
                    else if (bb.TryGet<MovementModule.AnimationMode>(key, out _)) lhsType = typeof(MovementModule.AnimationMode);
                    else return false;
                }

                object lhsValObj;
                if (lhsType == typeof(int)) { if (!bb.TryGet<int>(key, out var av)) return false; lhsValObj = av; }
                else if (lhsType == typeof(long)) { if (!bb.TryGet<long>(key, out var lv)) return false; lhsValObj = lv; }
                else if (lhsType == typeof(float)) { if (!bb.TryGet<float>(key, out var fv)) return false; lhsValObj = fv; }
                else if (lhsType == typeof(double)) { if (!bb.TryGet<double>(key, out var dv)) return false; lhsValObj = dv; }
                else if (lhsType == typeof(bool)) { if (!bb.TryGet<bool>(key, out var bv)) return false; lhsValObj = bv; }
                else if (lhsType == typeof(string)) { if (!bb.TryGet<string>(key, out var sv)) return false; lhsValObj = sv; }
                else if (lhsType == typeof(Vector3)) { if (!bb.TryGet<Vector3>(key, out var vv)) return false; lhsValObj = vv; }
                else if (lhsType == typeof(MovementModule.AnimationMode)) { if (!bb.TryGet<MovementModule.AnimationMode>(key, out var av2)) return false; lhsValObj = av2; }
                else { return false; }

                object rhsValObj;
                if (_compareWithKey)
                {
                    string rhsKeyLocal = _rhsKey;
                    if (rhsType == null)
                    {
                        if (bb.TryGet<int>(rhsKeyLocal, out _)) rhsType = typeof(int);
                        else if (bb.TryGet<long>(rhsKeyLocal, out _)) rhsType = typeof(long);
                        else if (bb.TryGet<float>(rhsKeyLocal, out _)) rhsType = typeof(float);
                        else if (bb.TryGet<double>(rhsKeyLocal, out _)) rhsType = typeof(double);
                        else if (bb.TryGet<bool>(rhsKeyLocal, out _)) rhsType = typeof(bool);
                        else if (bb.TryGet<string>(rhsKeyLocal, out _)) rhsType = typeof(string);
                        else if (bb.TryGet<Vector3>(rhsKeyLocal, out _)) rhsType = typeof(Vector3);
                        else if (bb.TryGet<MovementModule.AnimationMode>(rhsKeyLocal, out _)) rhsType = typeof(MovementModule.AnimationMode);
                        else return false;
                    }
                    if (rhsType == typeof(int)) { if (!bb.TryGet<int>(rhsKeyLocal, out var rvInt)) return false; rhsValObj = rvInt; }
                    else if (rhsType == typeof(long)) { if (!bb.TryGet<long>(rhsKeyLocal, out var rvLong)) return false; rhsValObj = rvLong; }
                    else if (rhsType == typeof(float)) { if (!bb.TryGet<float>(rhsKeyLocal, out var rvFloat)) return false; rhsValObj = rvFloat; }
                    else if (rhsType == typeof(double)) { if (!bb.TryGet<double>(rhsKeyLocal, out var rvDouble)) return false; rhsValObj = rvDouble; }
                    else if (rhsType == typeof(bool)) { if (!bb.TryGet<bool>(rhsKeyLocal, out var rvBool)) return false; rhsValObj = rvBool; }
                    else if (rhsType == typeof(string)) { if (!bb.TryGet<string>(rhsKeyLocal, out var rvStr)) return false; rhsValObj = rvStr; }
                    else if (rhsType == typeof(Vector3)) { if (!bb.TryGet<Vector3>(rhsKeyLocal, out var rvVec)) return false; rhsValObj = rvVec; }
                    else if (rhsType == typeof(MovementModule.AnimationMode)) { if (!bb.TryGet<MovementModule.AnimationMode>(rhsKeyLocal, out var rvAnimMode)) return false; rhsValObj = rvAnimMode; }
                    else { return false; }
                }
                else
                {
                    if (rhsConstObj == null) return false;
                    rhsValObj = rhsConstObj;
                }

                ValueTypeTag tag = GetValueTypeTag(Nullable.GetUnderlyingType(lhsType) ?? lhsType);
                switch (tag)
                {
                    case ValueTypeTag.Bool:
                        bool aa = lhsValObj is bool ba ? ba : Convert.ToBoolean(lhsValObj);
                        bool bbv = rhsValObj is bool bb2 ? bb2 : Convert.ToBoolean(rhsValObj);
                        return _op == CompareOperator.Equal ? aa == bbv
                             : _op == CompareOperator.NotEqual ? aa != bbv
                             : false;

                    case ValueTypeTag.Enum:
                        long aval = Convert.ToInt64(lhsValObj, CultureInfo.InvariantCulture);
                        long bval = Convert.ToInt64(rhsValObj, CultureInfo.InvariantCulture);
                        return _op == CompareOperator.Equal ? aval == bval
                             : _op == CompareOperator.NotEqual ? aval != bval
                             : false;

                    case ValueTypeTag.String:
                        string sa = lhsValObj as string ?? lhsValObj.ToString();
                        string sb = rhsValObj as string ?? rhsValObj.ToString();
                        int cmpStr = string.Compare(sa, sb, StringComparison.Ordinal);
                        switch (_op)
                        {
                            case CompareOperator.Equal: return cmpStr == 0;
                            case CompareOperator.NotEqual: return cmpStr != 0;
                            case CompareOperator.Less: return cmpStr < 0;
                            case CompareOperator.LessOrEqual: return cmpStr <= 0;
                            case CompareOperator.GreaterOrEqual: return cmpStr >= 0;
                            case CompareOperator.Greater: return cmpStr > 0;
                            default: return false;
                        }

                    case ValueTypeTag.Vector3:
                        Vector3 va = (Vector3)lhsValObj;
                        Vector3 vb2 = (Vector3)rhsValObj;
                        bool eq = va == vb2;
                        return _op == CompareOperator.Equal ? eq
                             : _op == CompareOperator.NotEqual ? !eq
                             : false;

                    case ValueTypeTag.Numeric:
                        double da = Convert.ToDouble(lhsValObj, CultureInfo.InvariantCulture);
                        double db = Convert.ToDouble(rhsValObj, CultureInfo.InvariantCulture);
                        switch (_op)
                        {
                            case CompareOperator.Equal: return da == db;
                            case CompareOperator.NotEqual: return da != db;
                            case CompareOperator.Less: return da < db;
                            case CompareOperator.LessOrEqual: return da <= db;
                            case CompareOperator.GreaterOrEqual: return da >= db;
                            case CompareOperator.Greater: return da > db;
                            default: return false;
                        }

                    default:
                        return _op == CompareOperator.Equal ? Equals(lhsValObj, rhsValObj)
                             : _op == CompareOperator.NotEqual ? !Equals(lhsValObj, rhsValObj)
                             : false;
                }
            };
        }

        private string CurrentKey()
        {
            return (_op == CompareOperator.Exists || _op == CompareOperator.NotExists)
                ? _lhsKeyManual
                : _lhsKey;
        }

        private static bool KeyExists(IBlackboard bb, string key)
        {
            if (bb is BTBlackBoard btb)
            {
                try { return (bool)typeof(BTBlackBoard).GetMethod("ContainsKey").Invoke(btb, new object[] { key }); }
                catch { }
            }
            if (bb.TryGet<int>(key, out _)) return true;
            if (bb.TryGet<long>(key, out _)) return true;
            if (bb.TryGet<float>(key, out _)) return true;
            if (bb.TryGet<double>(key, out _)) return true;
            if (bb.TryGet<bool>(key, out _)) return true;
            if (bb.TryGet<string>(key, out _)) return true;
            if (bb.TryGet<Vector3>(key, out _)) return true;
            if (bb.TryGet<MovementModule.AnimationMode>(key, out _)) return true;
            return false;
        }

        private static object ParseStringToType(string s, Type t)
        {
            if (t == typeof(string)) return s ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s))
            {
                if (t == typeof(bool)) return false;
                if (t == typeof(Vector3)) return Vector3.zero;
                return Activator.CreateInstance(t);
            }

            if (t == typeof(bool)) return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            if (t.IsEnum) return Enum.Parse(t, s, ignoreCase: true);
            if (t == typeof(int)) return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            if (t == typeof(long)) return Convert.ToInt64(s, CultureInfo.InvariantCulture);
            if (t == typeof(float)) return Convert.ToSingle(s, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return Convert.ToDouble(s, CultureInfo.InvariantCulture);
            if (t == typeof(Vector3)) return ParseVector3(s);
            if (t == typeof(MovementModule.AnimationMode)) return Enum.Parse(t, s, ignoreCase: true);

            return Convert.ChangeType(s, t, CultureInfo.InvariantCulture);
        }

        private static Vector3 ParseVector3(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return Vector3.zero;
            var txt = s.Replace("(", "").Replace(")", "").Replace(" ", "");
            var parts = txt.Split(',');
            if (parts.Length < 3) return Vector3.zero;
            try
            {
                float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }
            catch { return Vector3.zero; }
        }

        public GuardCondition Clone()
        {
            var copy = new GuardCondition
            {
                _kind = _kind,
                _groupMode = _groupMode,
                _lhsKey = _lhsKey,
                _lhsKeyManual = _lhsKeyManual,
                _op = _op,
                _compareWithKey = _compareWithKey,
                _rhsKey = _rhsKey,
                _rhsConst = _rhsConst,
                _children = _children?.Select(c => c?.Clone()).ToList()
            };
            return copy;
        }

#if UNITY_EDITOR
        public void __SetEditorKeys(BTBlackBoard keys)
        {
            __editorKeys = keys;
            if (_children != null)
                foreach (var ch in _children) ch?.__SetEditorKeys(keys);
        }

        private IEnumerable<string> GetKeyOptions()
        {
            if (__editorKeys == null) yield break;
            IReadOnlyList<(string, Type)> list = null;
            try { list = __editorKeys.GetAllKeysAndTypes(); }
            catch { yield break; }

            if (list != null)
                foreach (var (k, _) in list)
                    yield return k;
        }

        private IEnumerable<string> GetComparableKeyOptions() => GetKeyOptions();
        private IEnumerable<CompareOperator> GetOperatorOptions()
            => Enum.GetValues(typeof(CompareOperator)).Cast<CompareOperator>();
#endif
    }
}
