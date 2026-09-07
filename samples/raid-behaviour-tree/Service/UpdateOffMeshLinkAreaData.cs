using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; // NavMeshLink

namespace ProjectRaid.Runtime.BehaviourTree.Service
{
    /// <summary>
    /// - 같은 NavMeshLink 위에 있는 동안: 해당 area 유지(블랙보드 갱신 필요 시에만)
    /// - 다른 NavMeshLink로 바뀌는 순간: 0(기본값) 기록
    /// - 링크에서 벗어난 것이 확정되는 순간(ExitGrace 경과): 0(기본값) 기록
    /// </summary>
    public class UpdateOffMeshLinkAreaData : BTService
    {
#pragma warning disable CS0414 // HACK: 변수가 사용되지 않고있어서 경고 무시 (나중에 사용 할 수 있음)
        [SerializeField] private string _key;
        private NavMeshAgent agent;

        private bool start = false;
        Vector3 endPos = Vector3.zero;
#pragma warning restore CS0414

        public override void OnInit(BTContext ctx)
        {
            agent = ctx.Owner.GetComponent<NavMeshAgent>();
            base.OnInit(ctx);
        }

        protected override void Execute(BTContext ctx)
        {
            if (agent.isOnOffMeshLink)
            {
                OffMeshLinkData data = agent.currentOffMeshLinkData;
                endPos = data.endPos + Vector3.up * agent.baseOffset;
                
                NavMeshLink link = agent.navMeshOwner as NavMeshLink;
                ctx.Blackboard.Set(_key, link.area);
            }

            if (agent.transform.position == endPos) 
            {
                ctx.Blackboard.Set(_key, 0);
                endPos = Vector3.zero;
            }
        }

        public override BTNode Clone()
        {
            var clone = new UpdateOffMeshLinkAreaData
            {
                policy = this.policy,
                interval = this.interval,
                accumulateAcrossRuns = this.accumulateAcrossRuns,
                executeOnEnterForInterval = this.executeOnEnterForInterval,

                _key = this._key,
                _child = this._child?.Clone(),
            };
            clone.SetChild(_child?.Clone());
            return clone;
        }
    }
}
