using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree.Decorator
{
    public sealed class Timeout : BTDecorator
    {
        [SerializeField] private float _timeout;
        private bool isTimeout = false;

        private MonoBehaviour _monoBehaviour;
        private Coroutine _coroutine;

        protected override bool CanTick(BTContext ctx)
        {
            if(!isTimeout) return true;
            
            return isTimeout = false;
        }

        public override void OnInit(BTContext ctx)
        {
            base.OnInit(ctx);
            _monoBehaviour = ctx.Owner.GetComponent<MonoBehaviour>();
        }

        protected override void OnEnter(BTContext ctx)
        {
            if(_coroutine != null) _monoBehaviour.StopCoroutine(_coroutine);
            isTimeout = false;
            _coroutine = _monoBehaviour.StartCoroutine(WaitForTimeout());
        }

        protected override void OnExit(BTContext ctx, ref BTStatus status)
        {
            if(_coroutine != null) _monoBehaviour.StopCoroutine(_coroutine);
            isTimeout = false;
        }

        private IEnumerator WaitForTimeout()
        {
            yield return new WaitForSeconds(_timeout);
            isTimeout = true;
        }

        public override void Reset()
        {
            base.Reset();
            if(_coroutine != null) _monoBehaviour.StopCoroutine(_coroutine);
            isTimeout = false;
        }

        protected override BTNode CreateClone(BTNode clonedChild)
        {
            return new Timeout { _child = clonedChild, _timeout = _timeout };
        }
    }
}