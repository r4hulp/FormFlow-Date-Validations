using FormFlow.DateValidations.Forms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FormFlow.DateValidations.Dialogs
{
    public class RecognizerValidation : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            try
            {

                var form = new FormDialog<Leave>(
                new Leave(),
                Leave.BuildForm,
                FormOptions.PromptInStart);

                context.Call<Leave>(form, this.AfterLeaveFormAsync);

            }
            catch (Exception ex)
            {
                await context.PostAsync("Something went wrong..");
                context.Done("");
            }
        }

        private async Task AfterLeaveFormAsync(IDialogContext context, IAwaitable<Leave> result)
        {
            var message = await result;

            context.Done("");
        }
    }
}