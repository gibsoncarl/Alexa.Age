using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Alexa.Age
{
    public class Function
    {
        private const string CANCEL = "Goodbye!";
        private const string HELP = "You can say tell me Edie's age! or how old is Elodie?";

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse
            {
                Response = new ResponseBody()
            };

            response.Response.ShouldEndSession = true;

            IOutputSpeech innerResponse = null;
            var log = context.Logger;

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Age");
                innerResponse = new PlainTextOutputSpeech
                {
                    Text = GetAge()
                };
            }
            else if (input.Request is IntentRequest intentRequest)
            {
                var outputSpeech = new PlainTextOutputSpeech();
                innerResponse = outputSpeech;

                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        outputSpeech.Text = CANCEL;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        outputSpeech.Text = CANCEL;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        outputSpeech.Text = HELP;
                        break;
                    case "ElodieAgeIntent":
                        log.LogLine($"ElodieAgeIntent : send age.");
                        outputSpeech.Text = GetAge();
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        outputSpeech.Text = HELP;
                        break;
                }
            }

            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";

            return response;
        }

        private string GetAge()
        {
            FullAge age = new FullAge(new DateTime(2017, 4, 26, 13, 26, 00), DateTime.Now);

            return age.ToString();
        }
    }

    public class FullAge
    {
        DateTime _birthDate;
        DateTime _currentDate;
        TimeSpan _ageSpan;

        public FullAge(DateTime birthDate, DateTime currentDate)
        {
            _birthDate = birthDate;
            _currentDate = currentDate;

            _ageSpan = currentDate - birthDate;
        }

        public int WholeYears
        {
            get
            {
                // get the difference in years
                // subtract another year if we're before the
                // birth day in the current year
                int totalYears = _currentDate.Year - _birthDate.Year;
                if (_currentDate.Month < _birthDate.Month || (_currentDate.Month == _birthDate.Month && _currentDate.Day < _birthDate.Day))
                {
                    totalYears--;
                }

                return totalYears;
            }
        }

        public int WholeMonths
        {
            get
            {
                // Months difference corrected for current day.
                int totalMonths = _currentDate.Month - _birthDate.Month;
                if (_currentDate.Day < _birthDate.Day)
                {
                    totalMonths--;
                }
                else if (_currentDate.Day == _birthDate.Day && _currentDate.TimeOfDay < _birthDate.TimeOfDay)
                {
                    totalMonths--;
                }

                return totalMonths;
            }
        }

        public int WholeWeeks => WholeDays / 7;

        public int WholeDays => (int)_ageSpan.TotalDays;

        public int WholeHours => (int)_ageSpan.TotalHours;

        public int WholeMinutes => (int)_ageSpan.TotalMinutes;

        public override string ToString()
        {
            string fullAge = $"{WholeYears} years, {RemainderMonths} months, {RemainderWeeks} weeks, {RemainderDays} days, {RemainderHours} hours, {RemainderMinutes} minutes.";

            return fullAge;
        }

        private int RemainderMonths => WholeMonths % 12;

        private int RemainderWeeks
        {
            get
            {
                // get the whole weeks between the birthdate + years + months
                TimeSpan t = _currentDate - _birthDate.AddYears(WholeYears).AddMonths(WholeMonths);

                return (int)t.TotalDays / 7;
            }
        }

        private int RemainderDays
        {
            get
            {
                // get the whole days between the birthdate + totalweeks
                TimeSpan t = _currentDate - _birthDate.AddYears(WholeYears).AddMonths(WholeMonths).AddDays(RemainderWeeks * 7);

                return (int)t.TotalDays;
            }
        }

        private int RemainderHours
        {
            get
            {
                // get the whole weeks between the birthdate + totalDays
                TimeSpan t = _currentDate - _birthDate.AddDays(WholeDays);

                return (int)t.TotalHours;
            }
        }

        private int RemainderMinutes
        {
            get
            {
                // get the whole weeks between the birthdate + total hours
                TimeSpan t = _currentDate - _birthDate.AddHours(WholeHours);

                return (int)t.TotalMinutes;
            }
        }
    }
}
