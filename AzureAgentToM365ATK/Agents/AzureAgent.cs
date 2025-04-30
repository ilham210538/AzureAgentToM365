// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

namespace AzureAgentToM365ATK.Agent;

public class AzureAgent : AgentApplication
{
    private readonly AIProjectClient _aiProjectClient;
    private AgentsClient _agentsClient;
    private AzureAIAgent _existingAgent;
    private string _agentId; 

    public AzureAgent(AgentApplicationOptions options, IConfiguration configuration) : base(options)
    {
        OnConversationUpdate(ConversationUpdateEvents.MembersAdded, WelcomeMessageAsync);
        OnActivity(ActivityTypes.Message, MessageActivityAsync, rank: RouteRank.Last);

        // TO DO: get the connection string of your Azure AI Foundry project in the portal
        string connectionString = configuration["AIServices:AzureAIFoundryProjectConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("AzureAIFoundryProjectConnectionString is not configured.");
        }
        // TO DO: Get the assistant ID in the Azure AI Foundry project portal for your agent
        this._agentId = configuration["AIServices:AgentID"];
        if (string.IsNullOrEmpty(this._agentId))
        {
            throw new InvalidOperationException("AgentID is not configured.");
        }

        _aiProjectClient = AzureAIAgent.CreateAzureAIClient(connectionString, new AzureCliCredential());
    }

    protected async Task MessageActivityAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        if (_existingAgent == null)
        {
            await InitializeAzureAgent(cancellationToken);
        }
        Console.WriteLine($"\nUser message received: {turnContext.Activity.Text}\n");
        ChatHistory chatHistory = turnState.GetValue("conversation.chatHistory", () => new ChatHistory());
        await SendMessageToAzureAgent(turnContext.Activity.Text, turnContext);
    }

    protected async Task InitializeAzureAgent(CancellationToken cancellationToken)
    {
        _agentsClient = _aiProjectClient.GetAgentsClient();
        Response<Azure.AI.Projects.Agent> agentResponse = await _agentsClient.GetAgentAsync(this._agentId, cancellationToken);
        Azure.AI.Projects.Agent existingAzureAgent = agentResponse.Value; // Access the Agent object from the Response
        _existingAgent = new(existingAzureAgent, _agentsClient);
    }

    protected async Task SendMessageToAzureAgent(string Text, ITurnContext turnContext)
    {
        try
        {
            // Start a Streaming Process 
            await turnContext.StreamingResponse.QueueInformativeUpdateAsync("Working on a response for you");

            Microsoft.SemanticKernel.Agents.AgentThread agentThread = new AzureAIAgentThread(_existingAgent.Client);

            // Create a new message to send to the Azure agent
            ChatMessageContent message = new(AuthorRole.User, Text);
            // Send the message to the Azure agent and get the response
            await foreach (StreamingChatMessageContent response in _existingAgent.InvokeStreamingAsync(message, agentThread))
            {
                turnContext.StreamingResponse.QueueTextChunk(response.Content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to Azure agent: {ex.Message}");
            turnContext.StreamingResponse.QueueTextChunk("An error occurred while processing your request.");
        }
        finally
        {
            await turnContext.StreamingResponse.EndStreamAsync(); // End the streaming response
        }
    }

    protected async Task WelcomeMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        if (_existingAgent == null)
        {
            await InitializeAzureAgent(cancellationToken);
        }

        foreach (ChannelAccount member in turnContext.Activity.MembersAdded)
        {
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                await SendMessageToAzureAgent("Create a welcome message for a new user", turnContext);
            }
        }
    }
}
