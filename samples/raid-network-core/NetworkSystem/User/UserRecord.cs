using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class UserRecord : NetworkBehaviour
{
    private readonly SyncDictionary<int, float> records = new();

    public IReadOnlyDictionary<int, float> Records => records;

    private UserManager userManager;

    [Server] 
    public void Init(UserManager userManager, UserRecordData userRecordData)
    {
        this.userManager = userManager;

        foreach (var record in userRecordData.records)
        {
            records[record.Key] = record.Value;
        }
    }

    [ServerRpc]
    public void RequestUpdateRecord(int bossID, float record)
    {
        if (records.ContainsKey(bossID))
        {
            records[bossID] = record;
        }
        else
        {
            records.Add(bossID, record);
        }
    }
}
