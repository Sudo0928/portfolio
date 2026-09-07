using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    public class BTRunner : MonoBehaviour
    {
        [SerializeField, InlineEditor] private BTDataset _dataset;
        private BTContext _context;

        public void Start()
        {
            _dataset = _dataset.Clone();
            _context = new BTContext(gameObject, _dataset.BlackBoard, Time.fixedDeltaTime);
            _dataset.Root.OnInit(_context);
        }

        private void FixedUpdate()
        {
            _context.DeltaTime = Time.fixedDeltaTime;
            var status = _dataset.Root.OnTick(_context);
            //if(status != BTStatus.Running) _root.Reset();
        }
    }
}