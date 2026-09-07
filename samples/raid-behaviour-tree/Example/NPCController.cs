using ECM2;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ProjectRaid.Character;
using ProjectRaid.Character.Module;
using ProjectRaid.Runtime.BehaviourTree;
using Sirenix.OdinInspector;
using UnityEngine;
using ProjectRaid.Combat;

public class NPCController : NetworkBehaviour, IDamageable
{
    public Transform Transform => transform;
    public GameObject GameObject => gameObject;

    public bool isStun = false;
    [SerializeField] private PlayerCharacter player;
    public PlayerCharacter Player => player;
    [SerializeField] private NavMeshCharacter navMeshCharacter;
    public NavMeshCharacter NavMeshCharacter => navMeshCharacter;
    [SerializeField, InlineEditor] private BTDataset _dataset;
    private BTContext _context;

    private bool isInitialized = false;

    private readonly SyncVar<float> maxHealth = new SyncVar<float>(100f);
    private readonly SyncVar<float> currentHealth = new SyncVar<float>(100f);


    public void Awake()
    {
        navMeshCharacter = GetComponent<NavMeshCharacter>();
    }

    [ContextMenu("StunTest")]
    public void StunTest()
    {
        TakeDamage(100f);
    }

    [Server]
    public void TakeDamage(float amount)
    {
        if(amount <= 0 || isStun || player.Movement.CurrentAnimationMode != MovementModule.AnimationMode.Combat) return;
        currentHealth.Value -= Mathf.Clamp(amount, 0f, currentHealth.Value);
        _context.Blackboard.Set("CurrentHealth", currentHealth.Value);

        if(currentHealth.Value <= 0) 
        {
            isStun = true;
            _context.Blackboard.Set("IsStuned", true);
        }
    }

    [Server]
    public float TakeDamage(DamageInfo info)
    {
        TakeDamage(info.damageAmount);
        return info.damageAmount;
    }

    public void Heal(float amount)
    {
        if(currentHealth.Value >= maxHealth.Value) return;
        currentHealth.Value += Mathf.Clamp(amount, 0f, maxHealth.Value - currentHealth.Value);
        _context.Blackboard.Set("CurrentHealth", currentHealth.Value);
    }

    public void Init(PlayerCharacter player)
    {
        if (isInitialized) return;

        this.player = player;

        _dataset = _dataset.Clone();
        _context = new BTContext(gameObject, _dataset.BlackBoard, Time.fixedDeltaTime);
        _dataset.Root.OnInit(_context);

        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsServerStarted || !isInitialized) return;

        _context.DeltaTime = Time.fixedDeltaTime;
        _dataset.Root.OnTick(_context);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        _dataset.Root.OnDrawGizmos(gameObject);
    }
#endif
}
