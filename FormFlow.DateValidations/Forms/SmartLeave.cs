using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormFlow.DateValidations.Forms
{


    [Serializable]
    public class SmartLeave
    {
        [Prompt("Type of leave {||}")]
        public LeaveTypeOptions LossType;

        [Prompt("Vacations from date (e.g. 12 January 2018) {||}")]
        public string From;

        [Prompt("To date (e.g. 16 January 2018) {||}")]
        public string To;

        [Prompt("Do you want to skip weekends in calculation of your leaves duration? {||}")]
        public bool SkipWeekends;


        [Prompt("How many days?")]
        public int Days { get; set; }

        public static IForm<SmartLeave> BuildSimpleForm()
        {
            OnCompletionAsyncDelegate<SmartLeave> wrapUpRequest = async (context, state) =>
            {
                string wrapUpMessage = $"You are going on leaves for {state.Days} days starting from {DateTime.Parse(state.From).ToShortDateString()} to {DateTime.Parse(state.To).ToShortDateString()}.";

                var msg = context.MakeMessage();
                msg.Text = wrapUpMessage;
                await context.PostAsync(msg);
            };

            return new FormBuilder<SmartLeave>()
                .Field(nameof(LossType))
                .Field(nameof(From),
                    validate: async (state, value) =>
                    {
                        var myCulture = Culture.English;
                        var model = DateTimeRecognizer.GetInstance().GetDateTimeModel(myCulture);
                        var results = model.Parse(value.ToString());

                        DateTime _val = DateTime.MinValue;

                        // Check there are valid results
                        if (results.Count > 0 && results.First().TypeName.StartsWith("datetimeV2"))
                        {
                            var first = results.First();
                            var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                            var subType = first.TypeName.Split('.').Last();
                            if (subType.Contains("date") && !subType.Contains("range"))
                            {
                                var expectedDate = resolutionValues.Where(v => DateTime.Parse(v["value"]) > DateTime.UtcNow).FirstOrDefault();

                                if(expectedDate == null)
                                {
                                    return new ValidateResult() { IsValid = false, Feedback = "No such date found" };
                                }

                                DateTime.TryParse(expectedDate["value"], out _val);
                            }
                        }

                        if(_val == DateTime.MinValue)
                            return new ValidateResult() { IsValid = false, Feedback = "No such date found" };

                        if (_val < DateTime.Now.Date)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "From date can not be from past." };
                        }

                        if (!string.IsNullOrEmpty(state.To))
                        {
                            DateTime toDate = DateTime.Parse(state.To);
                            if (_val > toDate)
                            {
                                return new ValidateResult() { IsValid = false, Feedback = "From date can not greater than To date." };
                            }
                        }

                        return new ValidateResult() { IsValid = true, Value = _val.ToString() };
                    })
                .Field(nameof(To),
                    validate: async (state, value) =>
                    {
                        var myCulture = Culture.English;
                        var model = DateTimeRecognizer.GetInstance().GetDateTimeModel(myCulture);
                        var results = model.Parse(value.ToString());

                        DateTime _val = DateTime.MinValue;

                        // Check there are valid results
                        if (results.Count > 0 && results.First().TypeName.StartsWith("datetimeV2"))
                        {
                            var first = results.First();
                            var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                            var subType = first.TypeName.Split('.').Last();
                            if (subType.Contains("date") && !subType.Contains("range"))
                            {
                                var expectedDate = resolutionValues.Where(v => DateTime.Parse(v["value"]) > DateTime.UtcNow).FirstOrDefault();

                                if (expectedDate == null)
                                {
                                    return new ValidateResult() { IsValid = false, Feedback = "No such date found" };
                                }

                                DateTime.TryParse(expectedDate["value"], out _val);
                            }
                        }

                        if (_val == DateTime.MinValue)
                            return new ValidateResult() { IsValid = false, Feedback = "No such date found" };

                        DateTime fromDate = DateTime.Parse(state.From);
                        if (_val < fromDate)
                        {
                            return new ValidateResult() { IsValid = false, Feedback = "To date can not be less than From date." };
                        }
                        return new ValidateResult() { IsValid = true, Value = _val.ToString() };
                    })
                .Field(nameof(SkipWeekends))
                .Confirm(async (state) =>
                {
                    DateTime from = DateTime.Parse(state.From);

                    DateTime to = DateTime.Parse(state.To);

                    int days = (to - from).Days + 1;
                    if (state.SkipWeekends)
                    {
                        days = BusinessDaysUntil(from, to);
                    }

                    state.Days = days;

                    return new PromptAttribute($"You are applying for total {days} days. Are you sure? {{||}}");
                })
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