using GadgetsOnline.Data;
using GadgetsOnline.Models;
using Microsoft.AspNetCore.Http;

namespace GadgetsOnline.Services
{
    public class OrderProcessing : IOrderProcessing
    {
        private readonly OrderRepository _orderRepository;
        private readonly IShoppingCart _shoppingCart;

        public OrderProcessing(OrderRepository orderRepository, IShoppingCart shoppingCart)
        {
            _orderRepository = orderRepository;
            _shoppingCart = shoppingCart;
        }

        public bool ProcessOrder(Order order, HttpContext httpContext)
        {
            // Resolve the current cart id, then place the entire order in a
            // single transactional stored procedure (header + line items +
            // total + cart cleanup all commit or roll back together).
            var cartId = _shoppingCart.GetCartId(httpContext);
            _orderRepository.PlaceOrder(cartId, order);
            return true;
        }
    }
}
