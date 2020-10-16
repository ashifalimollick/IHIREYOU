using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace FinanceBot
{
    public class DialogHelper
    {
        string type;

        public DialogHelper()
        {
            this.type = string.Empty;
        }

        public Activity welcomedefault(ITurnContext turnContext)
        {
            var response = generateAdaptiveCard(turnContext.Activity, "welcomeCard.json",string.Empty);
            return response;
        }
        public Activity login(ITurnContext turnContext)
        {
            var response = generateAdaptiveCard(turnContext.Activity, "login.json", string.Empty);
            return response;
        }
        public Activity questions(ITurnContext turnContext,string card,string type)
        {
                var response = generateAdaptiveCard(turnContext.Activity, card,type);
                return response;
        }

        public Activity generateAdaptiveCard(IActivity activity,string cardName, string type)
        {
            if (cardName.Contains("T1")|| cardName.Contains("T2"))
            {
                var attachment = CreateAdaptiveCardAttachmentQuestions(cardName, type);
                var response = ((Activity)activity).CreateReply();
                response.Attachments = new List<Attachment>() { attachment };
                return response;
            }
            else
            {
            var attachment = CreateAdaptiveCardAttachment(cardName);
            var response = ((Activity)activity).CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
            }
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment(String cardName)
        {
            // combine path for cross platform support
            string[] paths = { ".", "Cards", cardName };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
        private Attachment CreateAdaptiveCardAttachmentQuestions(String cardName,string type)
        {
            string card = string.Empty;
            // combine path for cross platform support
            
            if (type.Contains("aws"))
            {
                if (cardName.Contains("T1"))
                {
                    card = "aws1.json";
                }
                else
                {
                    card = "aws2.json";
                }
            }
            else if (type.Contains("azure"))
            {
                    if (cardName.Contains("T1"))
                    {
                        card = "azure1.json";
                    }
                    else
                    {
                        card = "azure2.json";
                    }
            }
            else
            {
                if (cardName.Contains("T1"))
                {
                    card = "data1.json";
                }
                else
                {
                    card = "data2.json";
                }
            }

            string[] paths = { ".", "Cards", card };
            string fullPath = Path.Combine(paths);
            var adaptiveCard = File.ReadAllText(fullPath);
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }
}
