using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.AttributeCommands;
using Telegram.Bot.AttributeCommands.Exceptions;
using TelegramAttributeCommands.Commads;

namespace TelegramAttributeCommands
{
    public class BotUpdateHandler
    {
        private readonly TelegramBotClient botClient;
        private readonly CancellationTokenSource cts = new();
        private readonly AttributeCommands Commands = new();

        public BotUpdateHandler(string BotToken)
        {
            try
            {
                Commands.RegisterCommands(typeof(TestCommands));
            }
            catch (CommandExistsException)
            {
                //do
            }

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
                    {
                        if (message.ReplyToMessage != null)
                            await BotOnReplyMessage(message, cts);
                        else if (message.Text != null)
                            await BotOnMessageReceived(message, cts);
                    }

                    break;

                case { CallbackQuery: { } callbackQuery }:
                    await BotOnCallbackQueryReceived(callbackQuery, cts);
                    break;

                default:
                    await UnknownUpdateHandlerAsync(update, cts);
                    break;
            }
        }

        private async Task BotOnReplyMessage(Message message, CancellationTokenSource cts)
        {
            try
            {
                await Commands.ProcessCommand(message.ReplyToMessage!.Text!, new object[] { botClient, message });
            }
            catch (CommandNotFoundException ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, ex.Message);
            }
            catch (CommandBadArgumentType)
            {
                //do
            }
            catch (CommandArgumentsCountError)
            {
                //do
            }
        }

        private async Task UnknownUpdateHandlerAsync(Update update, CancellationTokenSource cts)
        {
            await botClient.SendTextMessageAsync(update.Message!.Chat.Id, "Unknown update", cancellationToken: cts.Token);
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationTokenSource cts)
        {
            try
            {
                await Commands.ProcessCommand(callbackQuery.Data!, new object[] { botClient, callbackQuery });
            }
            catch (CommandNotFoundException ex)
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, ex.Message);
            }
            catch (CommandBadArgumentType)
            {
                //do
            }
            catch (CommandArgumentsCountError)
            {
                //do
            }
        }

        private async Task BotOnMessageReceived(Message message, CancellationTokenSource cts)
        {
            try
            {
                await Commands.ProcessCommand(message.Text!, new object[] { botClient, message });
            }
            catch (CommandNotFoundException ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, ex.Message);
            }
            catch (CommandBadArgumentType)
            {
                //do
            }
            catch (CommandArgumentsCountError)
            {
                //do
            }
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