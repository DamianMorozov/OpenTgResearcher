// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

namespace TgBusinessLogic.Models;

/// <summary> Chat cache </summary>
public sealed class TgChatCache
{
    #region Public and private fields, properties, constructor

    /// <summary> ConcurrentDictionary with chats and access hash </summary>
    private readonly ConcurrentDictionary<long, long> _chatsWithHashes = new();
    /// <summary> ConcurrentDictionary with chats and saved flags </summary>
    private readonly ConcurrentDictionary<long, bool> _chatsWithSaved = new();
    /// <summary> ConcurrentDictionary with chats and directories </summary>
    private readonly ConcurrentDictionary<long, string> _chatsWithDirectories = new();

    #endregion

    #region Public and private methods

    /// <summary> Try to add a chat to the dictionary </summary>
    public bool TryAddChat(long chatId, long accessHash, string directory) => 
        _chatsWithHashes.TryAdd(chatId, accessHash) && 
        _chatsWithSaved.TryAdd(chatId, false) && 
        _chatsWithDirectories.TryAdd(chatId, directory);

    /// <summary> Try to update a chat in the dictionary </summary>
    public bool TryUpdateChat(long chatId, long newAccessHash, bool isSaved, string directory) => 
        _chatsWithHashes.TryGetValue(chatId, out var currentHash) && _chatsWithHashes.TryUpdate(chatId, newAccessHash, currentHash) &&
        _chatsWithSaved.TryGetValue(chatId, out var currentSaved) && _chatsWithSaved.TryUpdate(chatId, isSaved, currentSaved) &&
        _chatsWithDirectories.TryGetValue(chatId, out var currentDirectory) && _chatsWithDirectories.TryUpdate(chatId, directory, currentDirectory);

    /// <summary> Try to get a chat from the dictionary </summary>
    public bool TryGetChat(long chatId, out long accessHash) => _chatsWithHashes.TryGetValue(chatId, out accessHash);

    /// <summary> Try to remove a chat from the dictionary </summary>
    public bool TryRemoveChat(long chatId) =>
        _chatsWithHashes.TryRemove(chatId, out _) &&
        _chatsWithSaved.TryRemove(chatId, out _) &&
        _chatsWithDirectories.TryRemove(chatId, out _);

    /// <summary> Check if the chat exists in the dictionary </summary>
    public bool ContainsChat(long chatId) => _chatsWithHashes.ContainsKey(chatId);

    /// <summary> Clear all chats from the dictionary </summary>
    public void ClearChats()
    {
        _chatsWithHashes.Clear();
        _chatsWithSaved.Clear();
        _chatsWithDirectories.Clear();
    }

    /// <summary> Returns a snapshot copy of the chat ID ↔ accessHash dictionary to ensure thread safety </summary>
    public IReadOnlyDictionary<long, long> GetChatsSnapshot() => new Dictionary<long, long>(_chatsWithHashes);

    /// <summary> Get flag indicating whether the chat is saved </summary>
    public bool IsSaved(long chatId) => _chatsWithSaved.TryGetValue(chatId, out var saved) && saved;

    /// <summary> Mark the chat as saved </summary>
    public void MarkAsSaved(long chatId) => _chatsWithSaved.AddOrUpdate(chatId, true, (_, _) => true);

    /// <summary> Mark the chat as not saved </summary>
    public void ResetSavedFlag(long chatId) => _chatsWithSaved.AddOrUpdate(chatId, false, (_, _) => false);

    /// <summary> Get the chat directory </summary>
    public string GetDirectory(long chatId) => _chatsWithDirectories.TryGetValue(chatId, out var directory) ? directory : string.Empty;

    /// <summary> Check the exists chat directory </summary>
    public bool CheckExistsDirectory(long chatId) => _chatsWithDirectories.TryGetValue(chatId, out var dir) && Directory.Exists(dir);

    /// <summary> Set the chat directory </summary>
    public void SetDirectory(long chatId, string directory) => _chatsWithDirectories.AddOrUpdate(chatId, directory, (_, _) => directory);

    /// <summary> Create a deep copy of the current TgMessageDownloadSettings instance </summary>
    public TgChatCache Clone()
    {
        var copy = new TgChatCache();

        foreach (var kvp in _chatsWithHashes)
            copy._chatsWithHashes.TryAdd(kvp.Key, kvp.Value);

        foreach (var kvp in _chatsWithSaved)
            copy._chatsWithSaved.TryAdd(kvp.Key, kvp.Value);

        foreach (var kvp in _chatsWithDirectories)
            copy._chatsWithDirectories.TryAdd(kvp.Key, kvp.Value);

        return copy;
    }

    #endregion
}
