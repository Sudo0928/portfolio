using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ProjectRaid.Core;
using ProjectRaid.Character;

public class UserManager : NetworkBehaviour
{
    private int dataID;

    private string userName;
    public string UserName => userName;

    private float playTime;
    public float PlayTime => playTime;

    private readonly SyncVar<UserInventory> inventory = new();
    public UserInventory Inventory => inventory.Value;

    private readonly SyncVar<UserQuickSlot> quickSlot = new();
    public UserQuickSlot QuickSlot => quickSlot.Value;

    private readonly SyncVar<UserRecord> record = new();
    public UserRecord Record => record.Value;

    private readonly SyncVar<UserQuest> quest = new();
    public UserQuest Quest => quest.Value;
    
    private readonly SyncVar<PlayerCharacter> userCharacter = new();
    public PlayerCharacter UserCharacter => userCharacter.Value;

    private readonly SyncVar<UserCurrency> currency = new();
    public UserCurrency Currency => currency.Value;

    private readonly SyncVar<UserDialogue> dialogue = new();
    public UserDialogue Dialogue => dialogue.Value;

    private readonly SyncVar<UserEquipment> equipment = new();
    public UserEquipment Equipment => equipment.Value;

    private readonly Dictionary<int, CharacterData> characterDatas = new();
    private readonly SyncVar<int> currentCharacterID = new();
    public int CurrentCharacterID => currentCharacterID.Value;

    private Vector3 currentPosition;

    public event Action onChangeCharacter;

    [Server]
    public void Init(UserData userData)
    {
        if(UserCharacter == null)
            userData = new UserData();

        dataID   = userData.dataID;
        userName = userData.userName;
        playTime = userData.playTime;

        foreach (var characterData in userData.characterDatas)
        {
            characterDatas[characterData.characterID] = characterData;
        }

        inventory.Value = GetComponentInChildren<UserInventory>();
        Inventory.Init(this, userData.inventoryData);

        quickSlot.Value = GetComponentInChildren<UserQuickSlot>();
        QuickSlot.Init(this, userData.quickSlotData);

        record.Value = GetComponentInChildren<UserRecord>();
        Record.Init(this, userData.recordData);

        quest.Value = GetComponentInChildren<UserQuest>();
        Quest.Init(this, userData.questData);

        currency.Value = GetComponentInChildren<UserCurrency>();
        Currency.Init(this, userData.currencyData);

        dialogue.Value = GetComponentInChildren<UserDialogue>();
        Dialogue.Init(this, userData.dialogueData);

        equipment.Value = GetComponentInChildren<UserEquipment>();
        Equipment.Init(this, userData.characterDatas.Find(_ => _.characterID == userData.currentCharacterID)?.equipmentData.weapon);

        // SpawnCharacter(userData.currentCharacterID, connection);
    }
    
    private void Update()
    {
        if(!IsServerStarted) return;

        UpdatePlayTime();
    }

    [Server]
    public void UpdatePlayTime()
    {
        playTime += Time.deltaTime;
    }

    [Server]
    public void SetCurrentCharacterID(int characterID)
    {
        currentCharacterID.Value = characterID;
    }

    [Server]
    public void SpawnCharacter(int characterID, Vector3 position, Quaternion rotation = default)
    {
        {
            if(Managers.Data.CharacterDataLoader.TryGetByID(characterID, out ProjectRaid.Character.CharacterData characterData))
            {
                userCharacter.Value = Instantiate(characterData.PlayerCharacter, position, rotation);
                InstanceFinder.ServerManager.Spawn(userCharacter.Value.NetworkObject, Owner);
                InstanceFinder.SceneManager.AddOwnerToDefaultScene(userCharacter.Value.NetworkObject);
                currentCharacterID.Value = characterID;
            }
        }
        {
            //userCharacter.Value = Instantiate(characterPrefab, position, Quaternion.identity);
            //InstanceFinder.ServerManager.Spawn(userCharacter.Value.NetworkObject, Owner);
            //InstanceFinder.SceneManager.AddOwnerToDefaultScene(userCharacter.Value.NetworkObject);
            //currentCharacterID.Value = characterID;
            //CharacterData characterData = GetCharacterData(characterID);
            //userCharacter.Value.Initialize(characterData);
        }
    }

    [Server]
    public void DespawnCharacter()
    {
        if (UserCharacter == null) return;
        currentPosition = UserCharacter.transform.position;
        characterDatas[currentCharacterID.Value] = ConvertCharacterData();
        InstanceFinder.ServerManager.Despawn(UserCharacter.NetworkObject);
    }

    [ServerRpc]
    public void ChangeCharacter(int characterID)
    {
        DespawnCharacter();
        SpawnCharacter(characterID, currentPosition);
        OnChangeCharacter();
    }

    [ObserversRpc]
    private void OnChangeCharacter()
    {
        onChangeCharacter?.Invoke();
    }

    [Server]
    public CharacterData GetCharacterData(int characterID)
    {
        if (characterDatas.TryGetValue(characterID, out CharacterData characterData))
        {
            return characterData;
        }
        return null;
    }

    [Server]
    public CharacterData ConvertCharacterData()
    {
        PlayerCharacter player = UserCharacter;

        CharacterData characterData = new();

        CharacterLevelData levelData = new CharacterLevelData();
        levelData.level = player.Stats.CurrentLevel;
        // levelData.currentExp = player.Stats.CurrentExp;
        characterData.levelData = levelData;

        CharacterResourceData resourceData = new CharacterResourceData();
        resourceData.currentHealth = player.CurrentHealth;
        resourceData.currentStamina = player.CurrentStamina;
        characterData.resourceData = resourceData;

        CharacterEquipmentData equipmentData = new CharacterEquipmentData();
        equipmentData.weapon = Equipment.Weapon;
        // equipmentData.helmet = player.Equipment.equipment[EquipmentSlot.Helmet];
        // equipmentData.chest = player.Equipment.equipment[EquipmentSlot.Top];
        // equipmentData.pants = player.Equipment.equipment[EquipmentSlot.Bottom];
        // equipmentData.gloves = player.Equipment.equipment[EquipmentSlot.Gloves];
        // equipmentData.shoes = player.Equipment.equipment[EquipmentSlot.Shoes];
        characterData.equipmentData = equipmentData;

        return characterData;
    }

    [Server]
    public UserData ConvertUserData()
    {
        if(UserCharacter != null) characterDatas[currentCharacterID.Value] = ConvertCharacterData();

        UserData userData = new()
        {
            dataID             = dataID,
            userName           = userName,
            playTime           = playTime,
            currentCharacterID = currentCharacterID.Value,
            characterDatas     = characterDatas.Values.ToList(),

            inventoryData      = new UserInventoryData() {
                items          = new Dictionary<ItemType, List<ItemInstance>>(Inventory.Items)
            },

            quickSlotData      = new UserQuickSlotData() {
                quickSlots     = QuickSlot.QuickSlots.Select(x => x?.itemId ?? 0).ToList()
            },

            recordData         = new UserRecordData() {
                records        = new Dictionary<int, float>(Record.Records.ToDictionary(x => x.Key, x => x.Value))
            },

            currencyData       = new UserCurrencyData() {
                gold           = Currency.Gold
            },
        };

        UserQuestData userQuestData = new()
        {
            activeQuests       = new Dictionary<int, QuestProgressData>(),
            completedQuestIds  = Quest.CompletedQuestIds.ToList()
        };

        foreach (var quest in Quest.ActiveQuests.Values)
        {
            if (!userQuestData.activeQuests.TryGetValue(quest.ID, out var qp))
            {
                qp = new QuestProgressData
                {
                    questId = quest.ID,
                    taskProgressData = new Dictionary<int, int>()
                };
                userQuestData.activeQuests[quest.ID] = qp;
            }
            foreach (var task in quest.Tasks)
            {
                qp.taskProgressData[task.ID] = task.CurrentSuccess;
            }
        }

        userQuestData.completedQuestIds = Quest.CompletedQuestIds.ToList();
        userData.questData = userQuestData;

        return userData;
    }
}