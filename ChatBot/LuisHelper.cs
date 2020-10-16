// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceBot
{
    public static class LuisHelper
    {
        public static async Task<LuisData> ExecuteLuisQuery(IConfiguration configuration, ITurnContext turnContext, CancellationToken cancellationToken)
        {
                // Create the LUIS settings from configuration.
            var luisApplication = new LuisApplication(
                    configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    configuration["LuisAPIHostName"]
                );
            var recognizer = new LuisRecognizer(luisApplication);

                // The actual call to LUIS
            var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);
            var (intent, score) = recognizerResult.GetTopScoringIntent();
            string topintent = intent.ToString().ToLower();
            string entity=recognizerResult.Entities["claimno"]?.FirstOrDefault()?.ToString().ToLower();
            LuisData ld = new LuisData();
            ld.intent = topintent;
            ld.claimnumber = entity;
            return ld;
        }
    }
}
