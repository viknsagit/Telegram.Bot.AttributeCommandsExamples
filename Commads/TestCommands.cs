using Telegram.Bot;
using Telegram.Bot.AttributeCommands.Attributes;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramAttributeCommands.Commads
{
    public class TestCommands
    {
        private static CancellationTokenSource cts = new();

        private static InlineKeyboardMarkup testMarkup = new(new[]
        {
             InlineKeyboardButton.WithCallbackData("test")
        });

        [TextCommand("/test")]
        public static async Task TestCommand(TelegramBotClient client, Message message)
        {
            await client.SendTextMessageAsync(message.Chat.Id, "Test message", replyMarkup: testMarkup, cancellationToken: cts.Token);
        }

        [CallbackCommand("test")]
        public static async Task TestCallback(TelegramBotClient client, CallbackQuery callbackQuery)
        {
            await client.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, "Test callback msg", cancellationToken: cts.Token);
        }
    }
}