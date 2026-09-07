using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class UserParty : NetworkBehaviour
{
    private readonly SyncVar<int> _partyID          = new();
    private readonly SyncVar<int> _questID          = new();
    private readonly SyncVar<int> _targetEnemy      = new();
    private readonly SyncVar<int> _partyLeader      = new();
    private readonly SyncVar<int> _maxMemberCount   = new();
    private readonly SyncHashSet<int> _partyMembers = new();

    public int PartyID            => _partyID.Value;
    public int QuestID            => _questID.Value;
    public int TargetEnemy        => _targetEnemy.Value;
    public int PartyLeader        => _partyLeader.Value;
    public int MaxMemberCount     => _maxMemberCount.Value;
    public List<int> PartyMembers => _partyMembers.ToList();

    [Server]
    public void Init(int partyID, int questID, int partyLeaderClientID)
    {
        _partyID.Value        = partyID;
        _questID.Value        = questID;
        _partyLeader.Value    = partyLeaderClientID;
        _maxMemberCount.Value = 4;

        _partyMembers.Add(partyLeaderClientID);
        PartyMembers.Add(partyLeaderClientID);
    }

    [Server]
    public void AddPartyMember(int clientID)
    {
        _partyMembers.Add(clientID);
        PartyMembers.Add(clientID);
    }

    [Server]
    public void RemovePartyMember(int clientID)
    {
        if(!_partyMembers.Contains(clientID)) return;

        _partyMembers.Remove(clientID);
        PartyMembers.Remove(clientID);
        
        if( PartyMembers.Count != 0 && PartyLeader == clientID)
            _partyLeader.Value = PartyMembers.First();
    }
}