using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.AttributeCommands;
using TelegramAttributeCommands.Commads;

namespace TelegramAttributeCommands
{
    public class BotUpdateHandler
    {
        private readonly TelegramBotClient botClient;
        private readonly CancellationTokenSource cts = new();
        private readonly AttributeCommands commands = new();

        public BotUpdateHandler(string BotToken)
        {
            commands.RegisterTextCommands(typeof(TestCommands));
            commands.RegisterCallbackCommands(typeof(TestCommands));

            botClient = new TelegramBotClient(BotToken);
            StartListeningUpdate();
        }

        private void StartListeningUpdate()
        {
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
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
            await commands.ProcessCallbackCommand(callbackQuery.Data!, new object[] { botClient, callbackQuery });
        }

        private async Task BotOnMessageReceived(Message message, CancellationTokenSource cts)
        {
            await commands.ProcessTextCommand(message.Text!, new object[] { botClient, message });
        }

        private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            await Task.CompletedTask;
        }
    }
}