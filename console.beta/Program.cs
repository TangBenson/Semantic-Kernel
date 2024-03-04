using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace console.beta;

public class Program
{
    protected Program()
    {
    }
    public static async Task Main(string[] args)
    {
        //Azure OpenAI 服務資訊 
        var DeployName = "👉模型佈署名稱👈";
        var Endpoint = "https://👉API端點👈.openai.azure.com/";
        var ApiKey = "👉ApiKey👈";

        // 建立 kernel builder, 掛上 Azure OpenAI
        var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(DeployName, Endpoint, ApiKey);
        builder.Plugins.AddFromType<LightPlugin>(); // 將 LightPlugin 加入 Kernel
        Kernel kernel = builder.Build();

        // 建立 chat history 物件，並且加入系統提示訊息(System Prompt)
        var history = new ChatHistory();
        history.AddSystemMessage("你是一個親切的智能家庭助理，可以協助用戶回答問題，交談時請使用中文。");

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // 開始對談
        Console.Write("User > ");
        string? userInput;
        while (!string.IsNullOrEmpty(userInput = Console.ReadLine()))
        {
            // Add user input
            history.AddUserMessage(userInput);

            // Enable auto function calling
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // 從 AI 取得對談結果
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // 顯示結果
            Console.WriteLine("Assistant > " + result);

            // 將對話加入歷史訊息
            history.AddMessage(result.Role, result.Content ?? string.Empty);

            // Get user input again
            Console.Write("User > ");
        }
    }
}
