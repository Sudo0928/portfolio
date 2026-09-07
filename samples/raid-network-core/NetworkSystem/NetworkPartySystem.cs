using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using GameKit.Dependencies.Utilities;
using Sirenix.Utilities;

public class NetworkPartySystem : NetworkSystem
{
    [SerializeField] private UserParty userPartyPrefab;

    // int = party condition, List<Party> = parties
    private readonly SyncDictionary<int, UserParty> _parties = new SyncDictionary<int, UserParty>();
    private readonly SyncDictionary<int, int> _partyIDByClientID = new SyncDictionary<int, int>();
    public IReadOnlyList<UserParty> Parties => _parties.Values.ToList();

    // Called when the your party is changed
    public event Action<int> onJoinParty;
    public event Action<int> onChangedParty;
    public event Action<int> onLeaveParty;
    public event Action<int> onKickedParty;

    private int nextPartyID = 0;
    private HashSet<int> reclaimedIds = new HashSet<int>();

    public override void OnStartClient()
    {
        _parties.OnChange += OnChangeParty;
    }

    private void OnChangeParty(SyncDictionaryOperation op, int key, UserParty value, bool asServer)
    {
        foreach (var party in Parties)
        {
            UnityEngine.Debug.Log($"PartyID : {party.PartyID} | MemberCount : {party.PartyMembers.Count}");
        }
    }

    public UserParty GetParty(int id)
    {
        UserParty party = null;

        for (int i = 0; i < _parties.Count; i++)
        {
            if (_parties[i].PartyID == id)
                party = _parties[i];
        }

        return party;
    }

    /// <summary>
    /// 본인의 파티 ID 반환
    /// </summary>
    /// <returns>int: partyID</returns>
    public int GetPartyID()
    {
        var clientID = InstanceFinder.ClientManager.Connection.ClientId;
        if (_partyIDByClientID.TryGetValue(clientID, out var partyID))
        {
            return partyID;
        }
        return -1;
    }

    /// <summary>
    /// 다른 플레이어의 파티 ID 반환
    /// </summary>
    /// <param name="clientID"></param>
    /// <returns>int: partyID</returns>
    public int GetPartyID(int clientID)
    {
        if (_partyIDByClientID.TryGetValue(clientID, out var partyID))
        {
            return partyID;
        }
        return -1;
    }

    /// <summary>
    /// 파티 생성
    /// </summary>
    /// <param name="questID"></param>
    /// <param name="partyLeader"></param>
    [ServerRpc(RequireOwnership = false)]
    public void CreateParty(int questID, int clientID)
    {
        if (!IsValidQuest(questID, clientID)) return;

        int partyID = reclaimedIds.Count > 0 ? reclaimedIds.First() : nextPartyID++;

        var party = Instantiate(userPartyPrefab);
        party.NetworkObject.SetIsGlobal(true);
        InstanceFinder.ServerManager.Spawn(party.NetworkObject);
        party.Init(partyID, questID, clientID);

        _parties.Add(partyID, party);
        reclaimedIds.Remove(partyID);
        _partyIDByClientID.Add(clientID, partyID);

        if(InstanceFinder.ServerManager.Clients.TryGetValueIL2CPP(clientID, out var connection))
        {
            OnJoinParty(connection, partyID);
        }
    }

    /// <summary>
    /// 파티 삭제
    /// </summary>
    /// <param name="partyID"></param>
    [Server]
    private void RemoveParty(int partyID)
    {
        _parties.Remove(partyID);
        reclaimedIds.Add(partyID);
    }

    /// <summary>
    /// 파티 참가
    /// </summary>
    /// <param name="partyID"></param>
    /// <param name="connection"></param>
    [ServerRpc(RequireOwnership = false)]
    public void JoinParty(int partyID, NetworkConnection connection = null)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            var clientID = connection.ClientId;
            if (!IsValidParty(party, clientID)) return;

            party.AddPartyMember(clientID);
            _partyIDByClientID.Add(clientID, partyID);
            OnJoinParty(connection, partyID);

            // 본인을 제외한 파티원에게 파티 정보 업데이트 이벤트 호출
            party.PartyMembers.Where(member => member != clientID).ForEach(member => { if(InstanceFinder.ServerManager.Clients.TryGetValueIL2CPP(member, out var conn)) OnChangedParty(conn, partyID); });
        }
    }

    /// <summary>
    /// 파티 탈퇴
    /// </summary>
    /// <param name="partyID"></param>
    /// <param name="connection"></param>
    [ServerRpc(RequireOwnership = false)]
    public void LeaveParty(int partyID, NetworkConnection connection = null)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            var clientID = connection.ClientId;
            party.RemovePartyMember(clientID);

            OnLeaveParty(connection, partyID);
            if (party.PartyMembers.Count <= 0) RemoveParty(partyID);
            _partyIDByClientID.Remove(clientID);
            party.PartyMembers.Where(member => member != clientID).ForEach(member => { if(InstanceFinder.ServerManager.Clients.TryGetValueIL2CPP(member, out var conn)) OnChangedParty(conn, partyID); });
        }
        else Debug.LogWarning($"파티가 존재하지 않거나, 파티 구성원이 아닙니다. partyID: {partyID}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void LeaveParty(NetworkConnection connection = null)
    {
        var clientID = InstanceFinder.ClientManager.Connection.ClientId;
        if (_partyIDByClientID.TryGetValue(clientID, out var partyID)) LeaveParty(partyID, connection);
        else Debug.LogWarning($"파티에 참가하지 않았습니다. clientID: {clientID}");
    }

    /// <summary>
    /// 파티원 강퇴
    /// </summary>
    /// <param name="partyID"></param>
    /// <param name="connection"></param>
    [ServerRpc(RequireOwnership = false)]
    public void KickPartyMember(int partyID, NetworkConnection connection = null)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            var clientID = connection.ClientId;
            if (party.PartyLeader != clientID) return;

            party.RemovePartyMember(clientID);
            _partyIDByClientID.Remove(clientID);
            OnKickedParty(connection, partyID);
            party.PartyMembers.Where(member => member != clientID).ForEach(member => { if(InstanceFinder.ServerManager.Clients.TryGetValueIL2CPP(member, out var conn)) OnChangedParty(conn, partyID); });
        }
    }

    /// <summary>
    /// 파티 유효성 검사
    /// </summary>
    /// <param name="party"></param>
    /// <param name="connection"></param>
    /// <returns>bool: isValid</returns>
    private bool IsValidParty(UserParty party, int clientID)
    {
        if (party.PartyMembers.Count >= party.MaxMemberCount) return false;
        if (party.PartyMembers.Contains(clientID)) return false;

        if (!IsValidQuest(party.QuestID, clientID)) return false;
        return true;
    }

    /// <summary>
    /// 퀘스트 유효성 검사
    /// </summary>
    /// <param name="questID"></param>
    /// <param name="connection"></param>
    /// <returns>bool: isValid</returns>
    private bool IsValidQuest(int questID, int clientID)
    {
        UserQuest userQuest = Managers.Network.Systems.UserManagement.GetUser(clientID).Quest;
        var activeQuestIds = userQuest.ActiveQuests.Keys.ToList();
        var completedQuestIds = userQuest.CompletedQuestIds.ToList();

        if (activeQuestIds.Any(id => questID == id)) return true;
        if (completedQuestIds.Any(id => questID == id)) return true;
        return false;
    }

    /// <summary>
    /// 파티 변경 이벤트
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="partyID"></param>
    [TargetRpc]
    private void OnChangedParty(NetworkConnection connection, int partyID)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            onChangedParty?.Invoke(partyID);
        }
    }

    /// <summary>
    /// 파티 참가 이벤트
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="partyID"></param>
    [TargetRpc]
    private void OnJoinParty(NetworkConnection connection, int partyID)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            onJoinParty?.Invoke(partyID);
        }
    }

    /// <summary>
    /// 파티 탈퇴 이벤트
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="partyID"></param>
    [TargetRpc]
    private void OnLeaveParty(NetworkConnection connection, int partyID)
    {
        onLeaveParty?.Invoke(partyID);
    }

    /// <summary>
    /// 파티원 강퇴 이벤트
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="partyID"></param>
    [TargetRpc]
    private void OnKickedParty(NetworkConnection connection, int partyID)
    {
        if (_parties.TryGetValue(partyID, out var party))
        {
            onKickedParty?.Invoke(partyID);
        }
    }
}