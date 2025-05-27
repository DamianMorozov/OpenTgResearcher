// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using TL;

namespace TgBusinessLogic.Services;

/// <summary> Base connection client </summary>
public abstract partial class TgConnectClientBase : TgWebDisposable, ITgConnectClient
{
    #region Public and private methods - Client events

    private async Task OnClientOtherAsync(TL.IObject obj)
    {
        if (!IsClientUpdateStatus)
            return;
        if (obj is Auth_SentCodeBase authSentCode)
            await OnClientOtherAuthSentCodeAsync(authSentCode);
    }

    private async Task OnOwnUpdatesClientAsync(TL.IObject obj)
    {
        if (!IsClientUpdateStatus)
            return;
        await Task.CompletedTask;
    }

    public async Task OnUpdatesClientAsync(TL.UpdatesBase updateBase)
    {
        if (!IsClientUpdateStatus)
            return;
        if (updateBase is UpdateShort updateShort)
            await OnUpdateShortClientAsync(updateShort);
        if (updateBase is UpdatesBase updates)
            await OnUpdateClientUpdatesAsync(updates);
    }

    #endregion

    #region Public and private methods - Bot events

    private async Task OnErrorBotAsync(Exception ex, HandleErrorSource source)
    {
        if (!IsBotUpdateStatus)
            return;
#if DEBUG
        Debug.WriteLine(source);
#endif
        switch (source)
        {
            case HandleErrorSource.PollingError:
                break;
            case HandleErrorSource.FatalError:
                break;
            case HandleErrorSource.HandleUpdateError:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
        await Task.CompletedTask;
    }

    private async Task OnMessageBotAsync(WTelegram.Types.Message message, UpdateType updateType)
    {
        if (!IsBotUpdateStatus)
            return;
#if DEBUG
        Debug.WriteLine(message);
        Debug.WriteLine(updateType);
#endif
        switch (updateType)
        {
            case UpdateType.Unknown:
                break;
            case UpdateType.BusinessConnection:
                break;
            case UpdateType.BusinessMessage:
                break;
            case UpdateType.CallbackQuery:
                break;
            case UpdateType.ChannelPost:
                break;
            case UpdateType.ChatBoost:
                break;
            case UpdateType.ChatJoinRequest:
                break;
            case UpdateType.ChatMember:
                break;
            case UpdateType.ChosenInlineResult:
                break;
            case UpdateType.DeletedBusinessMessages:
                break;
            case UpdateType.EditedBusinessMessage:
                break;
            case UpdateType.EditedChannelPost:
                break;
            case UpdateType.EditedMessage:
                break;
            case UpdateType.InlineQuery:
                break;
            case UpdateType.Message:
                break;
            case UpdateType.MessageReaction:
                break;
            case UpdateType.MessageReactionCount:
                break;
            case UpdateType.MyChatMember:
                break;
            case UpdateType.Poll:
                break;
            case UpdateType.PollAnswer:
                break;
            case UpdateType.PreCheckoutQuery:
                break;
            case UpdateType.PurchasedPaidMedia:
                break;
            case UpdateType.RemovedChatBoost:
                break;
            case UpdateType.ShippingQuery:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateType), updateType, null);
        }
        await Task.CompletedTask;
    }

    private async Task OnUpdateBotAsync(WTelegram.Types.Update update)
    {
        if (!IsBotUpdateStatus)
            return;
#if DEBUG
        Debug.WriteLine(update.TLUpdate);
#endif
        await Task.CompletedTask;
    }

    #endregion
}
