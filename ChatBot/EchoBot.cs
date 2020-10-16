// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FinanceBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EchoBot : IBot
    {
        protected LuisRecognizer _luis;

        protected IConfiguration Configuration;

        protected DialogHelper dh;

        protected DataAccessLayer dal;

        private readonly EchoBotAccessors _accessors;

        private readonly DialogSet _dialogs;

        private readonly TextToSpeechService _ttsService;

        public EchoBot(EchoBotAccessors accessors, LuisRecognizer luisRecognizer, IConfiguration configuration)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            _dialogs = new DialogSet(_accessors.ConversationDialogState);

            _luis = luisRecognizer;

            Configuration = configuration;

            dh = new DialogHelper();

            dal = new DataAccessLayer(Configuration);

            _ttsService = new TextToSpeechService();

            var waterfallSteps = new WaterfallStep[]
    {
            ZeroStepAsync,
            FirstStepAsync,
            SecondStepAsync,
            ThirdStepAsync,
            FourthStepAsync,
            FifthStepAsync,
    };

            //_dialogs.Add(new ReservationDialog(_accessors.UserDataState, Configuration));
            _dialogs.Add(new WaterfallDialog("start", waterfallSteps));
            _dialogs.Add(new TextPrompt("login"));
            _dialogs.Add(new TextPrompt("HR1"));
            _dialogs.Add(new TextPrompt("HR2"));
            _dialogs.Add(new TextPrompt("T1"));
            _dialogs.Add(new TextPrompt("T2"));
        }

        // Add QnAMaker

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            //string uid = turnContext.Activity.From.Id;
            //string uname = turnContext.Activity.From.Name;
            //state.UserID = "Cand_1";
            //state.UserName = "Wicked Ashif";
            //state.UserType = "azure";
            //state.UserType = dal.FetchType(state.UserID);
            var state = await _accessors.UserDataState.GetAsync(turnContext, () => new UserData());
            string usertext = string.Empty;
            string reply = string.Empty;
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                try
                {
                    JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                    usertext = commandToken["x"].Value<string>();
                    turnContext.Activity.Text = usertext;
                }
                catch(Exception ex)
                {
                    try
                    {
                        usertext = turnContext.Activity.Text.ToString();
                    }
                    catch
                    {
                        usertext = turnContext.Activity.Value.ToString();
                        turnContext.Activity.Text = usertext;
                    }

                }
                try
                {
                    //dal.InsertChat(usertext, state.UserID, turnContext.Activity.Conversation.Id);
                    var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                    var dialogResult = await dialogContext.ContinueDialogAsync(cancellationToken);
                    if (!turnContext.Responded)
                    {
                        switch (dialogResult.Status)
                        {
                            case DialogTurnStatus.Empty:
                                if (usertext == "proceed")
                                {
                                    await dialogContext.BeginDialogAsync("start", cancellationToken);
                                }
                                break;
                            case DialogTurnStatus.Waiting:
                                // The active dialog is waiting for a response from the user, so do nothing.
                                break;
                            case DialogTurnStatus.Complete:
                                await dialogContext.EndDialogAsync();
                                break;
                            default:
                                await dialogContext.CancelAllDialogsAsync();
                                break;
                        }
                    }
                // Save states in the accessor
                // Get the conversation state from the turn context.

                // Set the property using the accessor.
                    await _accessors.UserDataState.SetAsync(turnContext, state);
                // Save the new state into the conversation state.
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                    await _accessors.UserState.SaveChangesAsync(turnContext);
                }
                catch (Exception ex)
                {
                    //dal.InsertErrorLog(state.UserID, "OnTurnAsync", ex.Message.ToString(), "Tech");
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                var response1 = dh.welcomedefault(turnContext);
                await turnContext.SendActivityAsync(response1, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ZeroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            try
            {
                var response1 = dh.login(stepContext.Context);
                var response = new PromptOptions { Prompt = response1 };
                return await stepContext.PromptAsync("login", response, cancellationToken);
            }
            catch (Exception ex)
            {
                string msg = "Error in ZeroStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string mobile = string.Empty;
            string token = string.Empty;
            string usertext = string.Empty;
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            try
            {
                JToken commandToken = JToken.Parse(stepContext.Context.Activity.Value.ToString());
                mobile = commandToken["mobile"].Value<string>();
                token = commandToken["token"].Value<string>();
            }
            catch (Exception ex)
            {
                usertext = stepContext.Context.Activity.Text.ToString().ToLower();
            }
            try
            {
                state.UserID = mobile;
                state.UserName = token;
                state.UserType = dal.FetchType(state.UserID,state.UserName);
                string type = state.UserType;
                if (state.UserType == "error")
                {
                    string msg = "Incorrect mobile/token";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                    return await stepContext.ReplaceDialogAsync("start", cancellationToken);
                }
                else
                {
                    dal.examattended(state.UserID);
                    var response1 = dh.questions(stepContext.Context, "HR1.json", type);
                    var response = new PromptOptions { Prompt = response1 };
                    string text = "Please click on the mic icon to answer the below question";
                    string voice = "Why are you leaving your current job? Explain in one line";
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(text, _ttsService.GenerateSsml(voice, "en-US", "en-US"), InputHints.ExpectingInput));
                    return await stepContext.PromptAsync("HR1", response, cancellationToken);
                }
            }
            catch(Exception ex)
            {
                string msg = "Error in FirstStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SecondStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            string evaluation = string.Empty;
            try
            {
                string type = state.UserType;
                string usertext = stepContext.Context.Activity.Text.ToString().ToLower();
                if(usertext.Length >12 && usertext.Length <25)
                {
                    evaluation = "pass";
                }
                else
                {
                    evaluation = "fail";
                }
                var response1 = dh.questions(stepContext.Context, "HR2.json",type);
                var response = new PromptOptions { Prompt = response1 };
                string text = "Answer the next question within 10 words";
                string voice = "How would your current manager describe you?";
                dal.InsertAnswers(state.UserID, usertext, evaluation, "HR1");
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(text, _ttsService.GenerateSsml(voice, "en-US", "en-US"), InputHints.ExpectingInput));
                return await stepContext.PromptAsync("HR2", response, cancellationToken);
            }
            catch (Exception ex)
            {
                string msg = "Error in SecondStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
        private async Task<DialogTurnResult> ThirdStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            string evaluation = string.Empty;
            try
            {
                string voice = "";
                string type = state.UserType;
                string usertext = stepContext.Context.Activity.Text.ToString().ToLower();
                if (usertext.Length > 12 && usertext.Length < 25)
                {
                    evaluation = "pass";
                }
                else
                {
                    evaluation = "fail";
                }
                dal.InsertAnswers(state.UserID, usertext,evaluation,"HR2");
                var response1 = dh.questions(stepContext.Context, "T1.json",type);
                var response = new PromptOptions { Prompt = response1 };
                string text = "The following question is MCQ. Please reply with the entire correct answer. For example: Option B Answer OR Answer";
                if(type=="aws")
                {
                    voice = "Which of the following is the central application in the AWS portfolio";
                }
                else if(type=="azure")
                {
                    voice = "Which Service in Azure is used to manage resources";
                }
                else
                {
                    voice = "At what level of an organisation does a corporate manager operate";
                }
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(text, _ttsService.GenerateSsml(voice, "en-US", "en-US")));
                return await stepContext.PromptAsync("T1", response, cancellationToken);
            }
            catch (Exception ex)
            {
                string msg = "Error in ThirdStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FourthStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            string evaluation = string.Empty;
            try
            {
                string voice = "";
                string type = state.UserType;
                string usertext = stepContext.Context.Activity.Text.ToString().ToLower();
                evaluation = correctAnswerQ1(usertext, type);
                dal.InsertAnswers(state.UserID, usertext, evaluation,"T1");
                var response1 = dh.questions(stepContext.Context, "T2.json",type);
                var response = new PromptOptions { Prompt = response1 };
                string text = "The following question is MCQ. Please reply with the entire correct answer. For example: Option B Answer OR Answer";
                if (type == "aws")
                {
                    voice = "Which of the following feature is used for scaling of EC2 sites";
                }
                else if (type == "azure")
                {
                    voice = "Which of the following element is a non-relational storage system for large-scale storage";
                }
                else
                {
                    voice = "Which one is not a recognised key skill of management";
                }
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(text, _ttsService.GenerateSsml(voice, "en-US", "en-US")));
                return await stepContext.PromptAsync("T2", response, cancellationToken);
            }
            catch (Exception ex)
            {
                string msg = "Error in FourthStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
        private async Task<DialogTurnResult> FifthStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var state = await _accessors.UserDataState.GetAsync(stepContext.Context, () => new UserData(), cancellationToken);
            string evaluation = string.Empty;
            try
            {
                string voice = "";
                string type = state.UserType;
                string usertext = stepContext.Context.Activity.Text.ToString().ToLower();
                evaluation = correctAnswerQ2(usertext, type);
                dal.InsertAnswers(state.UserID, usertext, evaluation,"T2");
                var response1 = dh.questions(stepContext.Context, "thanks.json",type);
                string text = "Interview over, the browser can be closed now.";
                voice = "Interview over, the browser can be closed now";
                await stepContext.Context.SendActivityAsync(response1, cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(text, _ttsService.GenerateSsml(voice, "en-US", "en-US")));
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                string msg = "Error in FirstStepAsync";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
        private string correctAnswerQ1(string text, string type)
        {
            string evalutation = string.Empty;
            if (type == "aws")
            {
                if (text.Contains("a. amazon") || text.Contains("compute") || text.Contains("cloud") || text.Contains("elastic"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            else if (type == "azure")
            {
                if (text.Contains("a. azure") || text.Contains("resource") || text.Contains("manager"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            else
            {
                if (text.Contains("d. top") || text.Contains("top") || text.Contains("top level"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            return evalutation;
        }
        private string correctAnswerQ2(string text, string type)
        {
            string evalutation = string.Empty;
            if (type == "aws")
            {
                if (text.Contains("b. auto") || text.Contains("auto scaling") || text.Contains("scaling"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            else if (type == "azure")
            {
                if (text.Contains("a. compute") || text.Contains("compute"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            else
            {
                if (text.Contains("d. writing") || text.Contains("writing") || text.Contains("writing skills"))
                {
                    evalutation = "pass";
                }
                else
                {
                    evalutation = "fail";
                }
            }
            return evalutation;
        }
    }
}
