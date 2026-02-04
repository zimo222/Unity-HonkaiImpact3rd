[System.Serializable]
public class AccountInfo
{
    public string Username;
    public string PasswordHash; // 存储哈希值，而非明文密码！
    public string LinkedPlayerDataID; // 关联的PlayerData的ID

    public AccountInfo(string username, string passwordHash, string playerDataId)
    {
        Username = username;
        PasswordHash = passwordHash;
        LinkedPlayerDataID = playerDataId;
    }
}