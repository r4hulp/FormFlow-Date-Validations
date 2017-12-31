using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormFlow.DateValidations.Forms
{
    public enum LeaveTypeOptions
    {
        PaidLeave = 1,
        SickLeave = 2,
        LossOfPay = 3,
        CompOff = 5
    }


    [Serializable]
    public class Leave
    {
        [Prompt("Type of leave {||}")]
        public LeaveTypeOptions LossType;

        [Prompt("Vacations from date (e.g. 12 January 2018) {||}")]
        public DateTime From;

        [Prompt("To date (e.g. 16 January 2018) {||}")]
        public DateTime To;

        [Prompt("How many days?")]
        public int Days { get; set; }

        public static IForm<Leave> BuildSimpleForm()
        {
            OnCompletionAsyncDelegate<Leave> wrapUpRequest = async (context, state) =>
            {
                string wrapUpMessage = $"You are going on leaves for {state.Days} days starting from {state.From.ToShortDateString()} to {state.To.ToShortDateString()}.";

                var msg = context.MakeMessage();
                msg.Text = wrapUpMessage;
                await context.PostAsync(msg);

            };
            return new FormBuilder<Leave>().Message
            ("Start filling this form.")
                .Field(nameof(LossType))
                .Field(nameof(From),
                    validate: async (state, value) =>
                    {
                        DateTime _val = (DateTime)value;
                        if (_val < DateTime.Now.Date)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "From date can not be from past." };
                        }

                        if (state.To != DateTime.MinValue)
                        {
                            if (_val > state.To)
                            {
                                return new ValidateResult() { IsValid = false, Feedback = "From date can not greater than To date." };
                            }

                        }
                        return new ValidateResult() { IsValid = true, Value = value };
                    })
                .Field(nameof(To),
                    validate: async (state, value) =>
                    {
                        DateTime _val = (DateTime)value;
                        if (_val < state.From)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "To date can not be less than From date." };
                        }
                        return new ValidateResult() { IsValid = true, Value = value };
                    })
                .Confirm(async (state) =>
                {
                    var businessDays = BusinessDaysUntil(state.From, state.To);

                    return new PromptAttribute($"You are applying for total {businessDays} days. Are you sure? {{||}}");
                })
                .Field(nameof(Days))
                .OnCompletion(wrapUpRequest)
                .Build();
        }

        public static IForm<Leave> BuildSmartForm()
        {
            OnCompletionAsyncDelegate<Leave> wrapUpRequest = async (context, state) =>
            {
                string wrapUpMessage = $"You are going on leaves for {state.Days} days starting from {state.From.ToShortDateString()} to {state.To.ToShortDateString()}.";

                var msg = context.MakeMessage();
                msg.Text = wrapUpMessage;
                await context.PostAsync(msg);

            };
            return new FormBuilder<Leave>().Message
            ("Start filling this form.")
                .Field(nameof(LossType))
                .Field(nameof(From),
                    validate: async (state, value) =>
                    {
                        DateTime _val = (DateTime)value;
                        if (_val < DateTime.Now.Date)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "From date can not be from past." };
                        }

                        if (state.To != DateTime.MinValue)
                        {
                            if (_val > state.To)
                            {
                                return new ValidateResult() { IsValid = false, Feedback = "From date can not greater than To date." };
                            }

                        }
                        return new ValidateResult() { IsValid = true, Value = value };
                    })
                .Field(nameof(To),
                    validate: async (state, value) =>
                    {
                        DateTime _val = (DateTime)value;
                        if (_val < state.From)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "To date can not be less than From date." };
                        }
                        return new ValidateResult() { IsValid = true, Value = value };
                    })
                .Confirm(async (state) =>
                {
                    var businessDays = BusinessDaysUntil(state.From, state.To);

                    return new PromptAttribute($"You are applying for total {businessDays} days. Are you sure? {{||}}");
                })
                .Field(nameof(Days))
                .OnCompletion(wrapUpRequest)
                .Build();
        }

        /// <summary>
        /// Calculates number of business days, taking into account:
        ///  - weekends (Saturdays and Sundays)
        ///  - bank holidays in the middle of the week
        /// </summary>
        /// <param name="firstDay">First day in the time interval</param>
        /// <param name="lastDay">Last day in the time interval</param>
        /// <param name="bankHolidays">List of bank holidays excluding weekends</param>
        /// <returns>Number of business days during the 'span'</returns>
        public static int BusinessDaysUntil(DateTime firstDay, DateTime lastDay, params DateTime[] bankHolidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (firstDay <= bh && bh <= lastDay)
                    --businessDays;
            }

            return businessDays;
        }
    }
}