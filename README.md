# Azure AI Foundry Agent To M365 Copilot
Making an Azure AI Foundry Agent available in M365 Copilot, using M365 Agents SDK/Toolkit

To use this sample, you'll need:

- Visual Studio 2022 17.14 (May 2025)
- Microsoft 365 Agents Toolkit (feature named 'Microsoft Teams development tools')

Then, create an Agent in the Azure AI Foundry portal, under the project part. Configure the model, instructions, tools, etc. you'd like.

Open the solution in VS 2022 and modify '**appsettings.json**' to update:

- **AzureAIFoundryProjectConnectionString** with the value coming from your Azure AI Foundry portal
- **AgentID** is the ID of the agent you've created. It starts by 'asst_....'

# Without M365 Agents Toolkit

You'll need only the AzureAgentToM365ATK C# project. Select it as the project to debug and set the debugging to 'Start Project'

Press F5. It will run locally the agent on your machine. 

You can install the Bot Framework Emulator (v4) and connect to the Agent using http://localhost:5130/api/messages 

Next step is to make it available via a public URL with a Dev Tunnel, create a Azure Bot Registration in the Azure Portal and fill the various properties in '**appsettings.json**' such as the ClientID, BOT_ID, BOT_TENANT_ID, etc.

To deploy it in Teams or M365 Copilot, you'll need to also update the '**manifest.json**' file, zip the folder and upload it to Teams / M365 Copilot via the App maangement UX. 

# With M365 Agents Toolkit

Press F5, it will start the local ASP.NET server to host the agent on your machine and will open the 'Microsoft 365 Agents Playground' tool. Try that you can discuss with the agent in the emulator.

Now, to try the experience in Teams or Microsoft 365 Copilot, you need a M365 tenant and be logged in.

Right-click the 'M365Agent' project, select 'Microsoft 365 Agents Toolkit', 'Select Microsoft 365 Account' and select the right account where you'd like to deploy your M365 Agent. 

If not done yet, create a Dev Tunnel and then select it.

Change the debbuging target to 'Microsoft Teams (browser)'. If you have multiple Edge profiles, select the one matching the M365 Account you've been using before. You potentially need to create a new browser profile in VS by selecting 'Browse With...' and create a new decidated one for the targeted M365 Tenant.

Then, create a new browser profile using that:

- Program: C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe
- Arguments: --profile-directory="Profile x" (find the profile number matching your account)
- Friendly name: Edge M365 Tenant

Make this Edge profile as the default one.

Press F5. 

M365 Agents Toolkit should go through 8 different steps, to create various registration, packaging, sideloading, etc. And if everything worked fine, the selected browser will open to install the Agent in Teams. You should be able to open it either in Teams or M365 Copilot.

To try it directly in M365 Copilot, go to https://m365copilot.com/ and your agent should be visible in the right trail. 
