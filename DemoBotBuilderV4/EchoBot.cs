using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace DemoBotBuilderV4
{
    public class EchoBot : IBot
    {
        public async Task OnTurn(ITurnContext context)
        {
            // 當收到活動類型為 Messages 的訊息時執行以下程序
            if (context.Activity.Type is ActivityTypes.Message)
            {
                // 取得儲存在對話狀態中的自訂資訊
                var state = context.GetConversationState<EchoState>();

                // 增加對話計數器
                state.TurnCount++;

                // 回傳使用者傳送的的對話訊息
                await context.SendActivity($"Turn {state.TurnCount}: You sent '{context.Activity.Text}'");
            }
        }
    }
}
