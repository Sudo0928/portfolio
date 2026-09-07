using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class Attacks : BTActionNode
    {
        private int _currentIndex = 0;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private List<AttackData> _Attackdata;
        private HashSet<Collider> _hitColliders = new HashSet<Collider>();

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            foreach (var data in _Attackdata)
            {
                data._playAnimatorStateAction.OnInit(ctx);
            }
        }

        protected override void OnEnter(BTContext ctx)
        {
            _hitColliders.Clear();
            _currentIndex = 0;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (_currentIndex >= _Attackdata.Count) return BTStatus.Success;

            var data = _Attackdata[_currentIndex];
            var status = data._playAnimatorStateAction.OnTick(ctx);
            if (status == BTStatus.Success) _currentIndex++;
            else if (status == BTStatus.Running)
            {
                if (!data._playAnimatorStateAction.TryGetTargetStateNormalizedTime(out var normalizedTime)) return Mark(BTStatus.Running);
                if (normalizedTime < data._hitStartPoint || normalizedTime > data._hitEndPoint)
                {
                    if (data._hitColliderType == AttackData.HitColliderType.Object) data._collider.enabled = false;
                    return Mark(BTStatus.Running);
                }

                Collider[] hits = null;
                switch (data._hitColliderType)
                {
                    case AttackData.HitColliderType.Object:
                        break;
                    case AttackData.HitColliderType.Box:
                        GetBoxWorld(data, ctx.Owner.transform, out var boxCenter, out var halfExtents, out var rot);
                        hits = Physics.OverlapBox(boxCenter, halfExtents, rot, _layerMask);
                        break;
                    case AttackData.HitColliderType.Capsule:
                        GetCapsuleWorld(data, ctx.Owner.transform, out var p0, out var p1, out var Capsuleradius);
                        hits = Physics.OverlapCapsule(p0, p1, Capsuleradius, _layerMask);
                        break;
                    case AttackData.HitColliderType.Sphere:
                        GetSphereWorld(data, ctx.Owner.transform, out var center, out var radius);
                        hits = Physics.OverlapSphere(center, radius, _layerMask);
                        break;
                }

                if (data._hitColliderType == AttackData.HitColliderType.Object)
                {
                    data._collider.enabled = true;
                }
                else
                {
                    if (hits.Length > 0)
                    {
                        foreach (var hit in hits)
                        {
                            if (_hitColliders.Contains(hit) || hit.gameObject == ctx.Owner.gameObject) continue;
                            Debug.Log("공격 당함: " + hit.gameObject.name);
                            _hitColliders.Add(hit);
                        }
                    }
                    return Mark(BTStatus.Running);
                }
            }

            if(_currentIndex >= _Attackdata.Count) return Mark(BTStatus.Success);
            return Mark(BTStatus.Running);
        }

        public override void Reset()
        {
            base.Reset();
            foreach (var data in _Attackdata)
            {
                data._playAnimatorStateAction.Reset();
            }
            _currentIndex = 0;
            _hitColliders.Clear();
        }

        public void GetBoxWorld(in AttackData data, Transform t,
        out Vector3 center, out Vector3 halfExtents, out Quaternion rot)
        {
            // Gizmo: t.localToWorldMatrix * TRS(pivot, localRot, 1)
            // OverlapBox는 월드 기준 파라미터 필요
            center = t.TransformPoint(data._pivotPoint);
            rot = t.rotation; // data에 별도 로컬 회전을 두면 곱해주기: t.rotation * data.LocalRotation
                              // 비균일 스케일까지 동일하게 반영
            var s = t.lossyScale;
            halfExtents = Vector3.Scale(data._boxSize * 0.5f, Abs(s));
        }

        // ───────── SPHERE ─────────
        public void GetSphereWorld(in AttackData data, Transform t,
            out Vector3 center, out float radius)
        {
            center = t.TransformPoint(data._pivotPoint);
            var s = Abs(t.lossyScale);
            // 구는 축과 무관하므로 가장 큰 축을 반경 스케일로 사용
            radius = data._sphereRadius * Mathf.Max(s.x, Mathf.Max(s.y, s.z));
        }

        // ───────── CAPSULE ─────────
        public void GetCapsuleWorld(in AttackData data, Transform t,
            out Vector3 p0, out Vector3 p1, out float radius)
        {
            Vector3 axisLocal = data._capsuleDirection switch
            {
                AttackData.CapsuleDirection.XAxis => Vector3.right,
                AttackData.CapsuleDirection.YAxis => Vector3.up,
                _ => Vector3.forward,
            };

            var s = Abs(t.lossyScale);
            // 축/수직축 스케일 분리 (로컬 캡슐 축 가정)
            float scaleAlongAxis = data._capsuleDirection switch
            {
                AttackData.CapsuleDirection.XAxis => s.x,
                AttackData.CapsuleDirection.YAxis => s.y,
                _ => s.z,
            };
            float scalePerp = data._capsuleDirection switch
            {
                AttackData.CapsuleDirection.XAxis => Mathf.Max(s.y, s.z),
                AttackData.CapsuleDirection.YAxis => Mathf.Max(s.x, s.z),
                _ => Mathf.Max(s.x, s.y),
            };

            // 월드 반경/몸통 길이
            radius = data._capsuleRadius * scalePerp;
            float cylLen = Mathf.Max(0f, data._capsuleHeight - 2f * data._capsuleRadius) * scaleAlongAxis;
            float halfCyl = 0.5f * cylLen;

            Vector3 center = t.TransformPoint(data._pivotPoint);
            Vector3 upWorld = t.rotation * axisLocal;

            p0 = center + upWorld * halfCyl; // 상단 구 중심
            p1 = center - upWorld * halfCyl; // 하단 구 중심

            // 높이가 2R보다 작아 실질적으로 구인 경우(수치 안전)
            if (cylLen <= 1e-6f) { p0 = p1 = center; }
        }

        public Vector3 Abs(in Vector3 v) => new(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public override BTNode Clone()
        {
            return new Attacks
            {
                _layerMask = _layerMask,
                _Attackdata = _Attackdata.Select(data => data.Clone()).ToList(),
            };
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos(GameObject obj)
        {
            var old = Gizmos.matrix;

            if (_Attackdata.Count <= 0) return;
            foreach (var data in _Attackdata)
            {
                if (!data._useGizmos) continue;

                Gizmos.matrix = obj.transform.localToWorldMatrix * Matrix4x4.TRS(data._pivotPoint, Quaternion.identity, Vector3.one);

                switch (data._hitColliderType)
                {
                    case AttackData.HitColliderType.Object:
                        Gizmos.DrawWireCube(obj.transform.position + data._collider.bounds.center, data._collider.bounds.size);
                        break;
                    case AttackData.HitColliderType.Box:
                        Gizmos.DrawWireCube(Vector3.zero, data._boxSize);
                        break;
                    case AttackData.HitColliderType.Capsule:
                        // 캡슐의 위쪽 반구와 아래쪽 반구를 그리기 위한 위치 계산
                        Vector3 top = Vector3.zero;
                        Vector3 bottom = Vector3.zero;

                        switch (data._capsuleDirection)
                        {
                            case AttackData.CapsuleDirection.YAxis:
                                top = new Vector3(0, data._capsuleHeight / 2 - data._capsuleRadius, 0);
                                bottom = new Vector3(0, data._capsuleHeight / 2 - data._capsuleRadius, 0);
                                break;
                            case AttackData.CapsuleDirection.XAxis:
                                top = new Vector3(data._capsuleHeight / 2 - data._capsuleRadius, 0, 0);
                                bottom = new Vector3(data._capsuleHeight / 2 - data._capsuleRadius, 0, 0);
                                break;
                            case AttackData.CapsuleDirection.ZAxis:
                                top = new Vector3(0, 0, data._capsuleHeight / 2 - data._capsuleRadius);
                                bottom = new Vector3(0, 0, data._capsuleHeight / 2 - data._capsuleRadius);
                                break;
                        }

                        // 상단 반구 그리기 (와이어프레임)
                        Gizmos.DrawWireSphere(top, data._capsuleRadius);

                        // 하단 반구 그리기 (와이어프레임)
                        Gizmos.DrawWireSphere(bottom, data._capsuleRadius);

                        // 두 반구를 연결하는 선 그리기
                        Gizmos.DrawLine(top + Vector3.right * data._capsuleRadius, bottom + Vector3.right * data._capsuleRadius);
                        Gizmos.DrawLine(top - Vector3.right * data._capsuleRadius, bottom - Vector3.right * data._capsuleRadius);
                        Gizmos.DrawLine(top + Vector3.forward * data._capsuleRadius, bottom + Vector3.forward * data._capsuleRadius);
                        Gizmos.DrawLine(top - Vector3.forward * data._capsuleRadius, bottom - Vector3.forward * data._capsuleRadius);
                        break;
                    case AttackData.HitColliderType.Sphere:
                        Gizmos.DrawWireSphere(Vector3.zero, data._sphereRadius);
                        break;
                }
            }

            Gizmos.matrix = old;
        }
#endif
    }

    [Serializable]
    public class AttackData
    {
        public enum HitColliderType { Object, Box, Capsule, Sphere }
        public enum CapsuleDirection { YAxis, XAxis, ZAxis }

        [Title("Attack")]
        [FoldoutGroup("Hit Collider")][Range(0f, 1f)] public float _hitStartPoint = 0f;
        [FoldoutGroup("Hit Collider")][Range(0f, 1f)] public float _hitEndPoint = 1f;

        [Title("Hit Collider")]
        [FoldoutGroup("Hit Collider")] public HitColliderType _hitColliderType;
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Object)] public Collider _collider;
        [FoldoutGroup("Hit Collider")][HideIf("_hitColliderType", HitColliderType.Object)] public bool _useGizmos;
        [FoldoutGroup("Hit Collider")][HideIf("_hitColliderType", HitColliderType.Object)] public Vector3 _pivotPoint = Vector3.up;
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Box)] public Vector3 _boxSize = new Vector3(1f, 1f, 1f);
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Capsule)] public float _capsuleRadius = 0.5f;
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Capsule)] public float _capsuleHeight = 2f;
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Capsule)] public CapsuleDirection _capsuleDirection = CapsuleDirection.YAxis;
        [FoldoutGroup("Hit Collider")][ShowIf("_hitColliderType", HitColliderType.Sphere)] public float _sphereRadius = 1f;

        [FoldoutGroup("Hit Collider")] public PlayAnimatorStateAction _playAnimatorStateAction;

        public AttackData Clone()
        {
            return new AttackData
            {
                _hitStartPoint = _hitStartPoint,
                _hitEndPoint = _hitEndPoint,
                _hitColliderType = _hitColliderType,
                _collider = _collider,
                _useGizmos = _useGizmos,
                _pivotPoint = _pivotPoint,
                _boxSize = _boxSize,
                _capsuleRadius = _capsuleRadius,
                _capsuleHeight = _capsuleHeight,
                _capsuleDirection = _capsuleDirection,
                _sphereRadius = _sphereRadius,
                _playAnimatorStateAction = _playAnimatorStateAction.Clone() as PlayAnimatorStateAction,
            };
        }
    }
}