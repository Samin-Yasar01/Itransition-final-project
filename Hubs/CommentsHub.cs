using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FormsApp.Hubs
{
    public class CommentsHub : Hub
    {
        public async Task SendComment(string user, string comment, string templateId)
        {
            await Clients.All.SendAsync("ReceiveComment", user, comment, templateId);
        }
    }
}