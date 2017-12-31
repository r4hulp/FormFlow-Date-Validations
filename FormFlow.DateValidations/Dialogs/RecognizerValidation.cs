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
    [Serializable]
    public class RecognizerValidation : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            try
            {

                var form = new FormDialog<SmartLeave>(
                new SmartLeave(),
                SmartLeave.BuildSimpleForm,
                FormOptions.PromptInStart);

                context.Call<SmartLeave>(form, this.AfterLeaveFormAsync);

            }
            catch (Exception ex)
            {
                await context.PostAsync("Something went wrong..");
                context.Done("");
            }
        }

        private async Task AfterLeaveFormAsync(IDialogContext context, IAwaitable<SmartLeave> result)
        {
            var message = await result;

            context.Done("");
        }
    }
}