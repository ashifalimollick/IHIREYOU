﻿using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace FinanceBot
{
  public class ConfigurationCredentialProvider : SimpleCredentialProvider
  {
    public ConfigurationCredentialProvider(IConfiguration configuration)
        : base(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"])
    {
    }
  }
}
