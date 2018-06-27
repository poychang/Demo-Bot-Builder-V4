using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoBotBuilderV4
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBot<EchoBot>(options =>
            {
                // 對話機器人需要一組 Microsoft App ID，這通常會存在 appsettings.json 或應用程式環境變數中
                // 透過傳入 Configuration 給 CredentialProvider，藉此取得相關資訊
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);

                // 為對話機器人加入處理最高層級的例外(Exception)控制，當有例外發生時，會執行以下動作
                // TraceActivity() 只會傳訊息給對話機器人模擬器，而 SendActivity() 會傳訊息給使用者
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity("EchoBot Exception", exception);
                    await context.SendActivity("Sorry, it looks like something went wrong!");
                }));

                // 本機偵錯時，建議使用記憶體來存放對話狀態資訊，每次重新啟動都會清空所存放的資訊
                IStorage dataStore = new MemoryStorage();

                // 若你的對話機器人運行在單一機器，且希望每次重新啟動能存取上一次的對話狀態資訊，請參考下列作法將資訊存成暫存檔案
                // IStorage dataStore = new FileStorage(System.IO.Path.GetTempPath());

                // 對於正是環境的對話機器人，你可以利用 Azure Table Store、Azure Blob 或 Azure CosmosDB 來保留對話狀態資訊
                // 請先至 NuGet 套件管理工具中搜尋並安裝 Microsoft.Bot.Builder.Azure，請參考以下作法將資訊存至 Azure 平台
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureTableStorage("AzureTablesConnectionString", "TableName");
                // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage("AzureBlobConnectionString", "containerName");

                options.Middleware.Add(new ConversationState<EchoState>(dataStore));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                // 使網站可以開啟 index.html 網頁
                .UseStaticFiles()
                // 將所註冊的 Bot 加入網站運行的程序中
                .UseBotFramework();
        }
    }
}
