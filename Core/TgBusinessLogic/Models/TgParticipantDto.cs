namespace TgBusinessLogic.Models;

/// <summary> Participant DTO </summary>
public sealed class TgParticipantDto(TL.User user)
{
    #region Fields, properties, constructor

    public TL.User User { get; set; } = user;
    public bool IsAdmin { get; set; }
    public long Id => User.id;

    public TgParticipantDto(TL.User user, bool isAdmin) : this(user)
    {
        IsAdmin = isAdmin;
    }

    #endregion
}
