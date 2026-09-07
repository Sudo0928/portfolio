using UnityEngine;

public class Managers : Singleton<Managers>
{
    [SerializeField] private NetworkManagerEx networkManagerEx;
    [SerializeField] private InputManager inputManager;

    public static NetworkManagerEx Network => Instance.networkManagerEx;
    public static InputManager Input => Instance.inputManager;

    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    public void Init()
    {
        Network.Init();
        Input.Init();
    }

    public void OnEnable()
    {
        Input.OnEnable();
    }

    public void OnDisable()
    {
        Input.OnDisable();
    }

    public void Update()
    {
        Input.Update();
    }

    public void ChangeYakTransport() => Network.ChangeTransport<FishNet.Transporting.Yak.Yak>();
    public void ChangeSteamTransport() => Network.ChangeTransport<FishySteamworks.FishySteamworks>();
}
