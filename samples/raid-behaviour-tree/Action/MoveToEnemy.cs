using ECM2;
using UnityEngine;
// using UnityEngine.AI; // 필요시

namespace ProjectRaid.Runtime.BehaviourTree.Action
{
    public sealed class MoveToEnemy : BTActionNode
    {
        private NavMeshCharacter _nav;
        private Collider _enemy;

        // 튜닝 파라미터
        [SerializeField] private bool _planar = true;          // XZ 평면 거리로 판정
        [SerializeField] private float _successBuffer = 0.05f;  // stoppingDistance 여유
        [SerializeField] private float _repathThreshold = 0.1f; // 목적지 변화가 이정도 이상일 때만 재지정

        private Vector3 _lastSetDestination;
        private bool _hasSetDestination;

        public override void OnInit(BTContext ctx)
        {
            _nav = ctx.Owner.GetComponent<NavMeshCharacter>();
        }

        protected override void OnEnter(BTContext ctx)
        {
            _enemy = null;
            if (ctx.Blackboard.TryGet<Collider>("enemy", out var enemy))
                _enemy = enemy;

            _hasSetDestination = false;
            _lastSetDestination = Vector3.zero;
        }

        public override BTStatus OnExecute(BTContext ctx)
        {
            if (_nav == null || _nav.agent == null) return BTStatus.Failure;
            if (_enemy == null) return BTStatus.Failure;
            if (!_nav.agent.isOnNavMesh) return BTStatus.Failure;

            var dest = _enemy.transform.position;

            // 1) 경로/상태와 무관하게 "실제 거리"로 도착 판정 (재진입 시 hasPath=false도 성공 처리됨)
            if (ReachedByDistance(_nav.agent.transform.position, dest, _nav.agent.stoppingDistance, _successBuffer, _planar))
                return BTStatus.Success;

            // 2) 필요할 때만 목적지 갱신 (사소한 변동으로 인한 재계산 방지)
            if (!_hasSetDestination || NeedsRepath(_lastSetDestination, dest, _repathThreshold, _planar))
            {
                _nav.MoveToDestination(dest);
                _lastSetDestination = dest;
                _hasSetDestination = true;
            }

            // 3) 경로 준비/진행 상황으로 러닝/성공 보조 판정
            var agent = _nav.agent;

            // 경로 계산 중
            if (agent.pathPending)
                return BTStatus.Running;

            // 경로가 있다면 남은 거리로 판정
            if (agent.hasPath)
            {
                if (agent.remainingDistance <= agent.stoppingDistance + _successBuffer)
                    return BTStatus.Success;

                return BTStatus.Running;
            }

            // 경로가 없고 계산도 안 하는데 아직 거리가 멀다 → 에이전트가 정지 상태면 다시 목적지 지정 유도
            // (다음 틱에 ReachedByDistance가 성공으로 바뀔 수도 있으므로 Running 유지)
            return BTStatus.Running;
        }

        public override BTNode Clone() => new MoveToEnemy();

        // ── helpers ─────────────────────────────────────────────────────────────

        private static bool ReachedByDistance(Vector3 from, Vector3 to, float stop, float buffer, bool planar)
        {
            float distSq;
            if (planar)
            {
                var dx = to.x - from.x;
                var dz = to.z - from.z;
                distSq = dx * dx + dz * dz;
            }
            else
            {
                distSq = (to - from).sqrMagnitude;
            }

            var r = stop + Mathf.Max(buffer, 0f);
            return distSq <= r * r;
        }

        private static bool NeedsRepath(Vector3 last, Vector3 next, float threshold, bool planar)
        {
            if (threshold <= 0f) return true;

            if (planar)
            {
                var dx = next.x - last.x;
                var dz = next.z - last.z;
                return (dx * dx + dz * dz) >= (threshold * threshold);
            }
            else
            {
                return (next - last).sqrMagnitude >= (threshold * threshold);
            }
        }
    }
}
