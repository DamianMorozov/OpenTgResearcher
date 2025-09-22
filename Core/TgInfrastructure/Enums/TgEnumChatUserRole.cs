namespace TgInfrastructure.Enums;

/// <summary> Chat user role </summary>
public enum TgEnumChatUserRole
{
    /// <summary> Regular member with no special permissions </summary>
    Member = 0,
    /// <summary> User with elevated permissions to manage chat content or members </summary>
    Admin = 1,
    /// <summary> Creator or owner of the chat with full control </summary>
    Owner = 2,
    /// <summary> User responsible for moderating discussions and enforcing rules </summary>
    Moderator = 3,
    /// <summary> User who has been banned from the chat </summary>
    Banned = 4,
    /// <summary> User who has left the chat voluntarily </summary>
    Left = 5
}
