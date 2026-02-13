using Microsoft.AspNetCore.SignalR;

namespace CourseWork.Hubs
{
    public class ShopHub : Hub
    {
        public async Task UpdateProductStock(int productId, int variantId, int newStock)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", productId, variantId, newStock);
        }
    }
}