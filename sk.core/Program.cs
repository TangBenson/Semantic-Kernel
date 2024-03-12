using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using sk.core.Plugins.HolidayPlugin;


var modleId = "gpt-4";
var ApiKey = "sk-";
string currentDirectory = Directory.GetCurrentDirectory();

Kernel kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(modleId, ApiKey)
                .Build();

// Load prompt from resource
using StreamReader reader = new(
    Path.Combine(currentDirectory,
    "Plugins",
    "ChatPlugin",
    "Chat.yaml"));
KernelFunction prompt = kernel.CreateFunctionFromPromptYaml(
    reader.ReadToEnd(),
    promptTemplateFactory: new HandlebarsPromptTemplateFactory()
);

using StreamReader reader2 = new(Path.Combine(
    currentDirectory,
    "Plugins",
    "LoginPlugin",
    "Login.yaml"));
KernelFunction loginPlugin = kernel.CreateFunctionFromPromptYaml(
    reader2.ReadToEnd());

//Load Plugins
kernel.Plugins.AddFromType<HolidayPlugin>();


#pragma warning disable SKEXP0004 

kernel.PromptRendered += (sender, args) =>
{
    Console.WriteLine("=========== PromptRendered Start ===========");
    Console.WriteLine(args.RenderedPrompt);
    Console.WriteLine("=========== PromptRendered End ===========\n\n");
};


kernel.FunctionInvoking += (sender, args) =>
{
    Console.WriteLine("=========== FunctionInvoking Start ===========");
    Console.WriteLine(args.Function.Name);
    Console.WriteLine("=========== FunctionInvoking End ===========\n\n");

};

#pragma warning restore SKEXP0004 


// Create the chat history
ChatHistory chatMessages = [];


System.Console.Write("User > ");
var userMessage = Console.ReadLine()!;
chatMessages.AddUserMessage(userMessage);

//要求登入
string logininfo = (await kernel.InvokeAsync(
    loginPlugin, new()
    {
        { "topic", userMessage }
    })).ToString();
System.Console.WriteLine($"Assistant > {logininfo}");



while (true)
{
    System.Console.Write("User > ");
    userMessage = Console.ReadLine()!;
    chatMessages.AddUserMessage(userMessage);

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // 從OpenAI取得回應
    var result = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
        prompt,
        arguments: new(openAIPromptExecutionSettings) {
            { "messages", chatMessages }
        });

    // Print the chat completions
    ChatMessageContent? chatMessageContent = null;

    await foreach (var content in result)
    {
        System.Console.Write(content);
        if (chatMessageContent == null)
        {
            System.Console.Write("Assistant > ");
            chatMessageContent = new ChatMessageContent(
                content.Role ?? AuthorRole.Assistant,
                content.ModelId!,
                content.Content!,
                content.InnerContent,
                content.Encoding,
                content.Metadata);
        }
        else
        {
            chatMessageContent.Content += content;
        }
    }
    System.Console.WriteLine("\n");

    chatMessages.Add(chatMessageContent!);

}
