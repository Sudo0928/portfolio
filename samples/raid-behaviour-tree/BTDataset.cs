using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [CreateAssetMenu(fileName = "New BTDataset", menuName = "ProjectRaid/BT/Dataset")]
    public class BTDataset : ScriptableObject
    {
        [SerializeField, InlineEditor, LabelText("Block Board")] private BTBlackBoard _blackBoard;
        public BTBlackBoard BlackBoard => _blackBoard;
        [SerializeReference, LabelText("Root")] private BTNode _root;
        public BTNode Root => _root;

        public BTDataset Clone()
        {
            return new BTDataset() { _blackBoard = (BTBlackBoard)_blackBoard.Clone(), _root = _root.Clone() };
        }
    }
}