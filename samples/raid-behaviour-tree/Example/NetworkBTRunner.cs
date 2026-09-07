using FishNet.Object;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public class NetworkBTRunner : NetworkBehaviour
    {
        [SerializeField, InlineEditor] private BTDataset _dataset;
        private BTContext _context;

        public override void OnStartServer()
        {
            _dataset = _dataset.Clone();
            _context = new BTContext(gameObject, _dataset.BlackBoard, Time.fixedDeltaTime);
            _dataset.Root.OnInit(_context);
        }

        private void FixedUpdate()
        {
            if(!IsServerStarted) return;
            
            _context.DeltaTime = Time.fixedDeltaTime;
            var status = _dataset.Root.OnTick(_context);
            if(status != BTStatus.Running) _dataset.Root.Reset();
        }
    }
}