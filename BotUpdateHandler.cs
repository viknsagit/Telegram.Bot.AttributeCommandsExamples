using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramAttributeCommands
{
    public class BotUpdateHandler
    {
        private readonly TelegramBotClient botClient;
        private CancellationTokenSource cts = new();

        public BotUpdateHandler(string BotToken)
        {
            botClient = new TelegramBotClient(BotToken);
            StartListeningUpdate();
        }

        private void StartListeningUpdate()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update)
            {
                case { Message: { } message }:

                    if (message.Text != null)
                        await BotOnMessageReceived(message, cts);
                    break;

                case { CallbackQuery: { } callbackQuery }:
                    await BotOnCallbackQueryReceived(callbackQuery, cts);
                    break;

                default:
                    await UnknownUpdateHandlerAsync(update, cts);
                    break;
            }
        }

        private async Task UnknownUpdateHandlerAsync(Update update, CancellationTokenSource cts)
        {
            await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Unknown update", cancellationToken: cts.Token);
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationTokenSource cts)
        {
            await Task.CompletedTask;
        }

        private async Task BotOnMessageReceived(Message message, CancellationTokenSource cts)
        {
            switch (message.Text)
            {
                case "/test":
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "TestCommand", cancellationToken: cts.Token);
                    }
                    break;

                default:
                    break;
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}