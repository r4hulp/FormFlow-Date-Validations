using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FormFlow.DateValidations.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await this.WelcomeMessageAsync(context);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            //// calculate something for us to return
            //int length = (activity.Text ?? string.Empty).Length;

            //// return our reply to the user
            //await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }

        private async Task WelcomeMessageAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var options = new[]
            {
                "Date Range Validation (Simple)",
                "Date Range Validation (Smart)"
            };

            reply.AddKeyboardCard("Chose an option", options);

            await context.PostAsync(reply);

            context.Wait(this.OnOptionSelected);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text == "Date Range Validation (Simple)")
            {
                context.Call(new SimpleValidation(), this.AfterSimpleValidation);

            }
            else if (message.Text == "Date Range Validation (Smart)")
            {
                context.Call(new RecognizerValidation(), this.AfterRecognizerValidation);
            }
            else
            {
                await this.StartOverAsync(context, "Not a valid option, starting over again");
            }
        }

        private async Task AfterRecognizerValidation(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("You are out of smart form");
            await this.StartAsync(context);
        }

        private async Task AfterSimpleValidation(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("You are out of simple form");
            await this.StartAsync(context);
        }

        private async Task StartOverAsync(IDialogContext context, string text)
        {
            var message = context.MakeMessage();
            message.Text = text;
            await this.StartOverAsync(context, message);
        }

        private async Task StartOverAsync(IDialogContext context, IMessageActivity message)
        {
            await context.PostAsync(message);
            await this.WelcomeMessageAsync(context);
        }
    }
}