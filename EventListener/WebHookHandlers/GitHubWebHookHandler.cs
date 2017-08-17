﻿using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Toadstool.Utility;
using Toadstool.WebhookEvents;

namespace Toadstool.Handlers
{
    public class GitHubWebHookHandler : WebHookHandler
    {
        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            if(receiver.ToLower() == Properties.Settings.Default.WebhookRecieverClient.ToLower())
            {
                JObject content = context.GetDataOrDefault<JObject>();
                var serializedContent = JsonConvert.SerializeObject(content);
                var util = new JsonUtilities();
                var action = util.GetFirstInstance<string>("action", serializedContent);
                var actionUser = content["sender"]["login"].Value<string>(); //Get the user performing the action
                var repositoryName = util.GetFirstInstance<string>("name", serializedContent);
                IWebhookEvent webhookEvent = null;

                switch(action.ToLower())
                {
                    case "deleted":
                        webhookEvent = new DeleteEvent(repositoryName, actionUser, action);
                        break;
                    case "created":
                        webhookEvent = new CreatedEvent(repositoryName, actionUser, action);
                        break;
                    default:
                        break;
                }

                if(webhookEvent != null)
                    return webhookEvent.CreateRepositoryIssue();
            }

            return Task.FromResult(true);
        }

    }
}