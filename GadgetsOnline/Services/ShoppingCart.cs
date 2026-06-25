using System;
using System.Collections.Generic;
using GadgetsOnline.Data;
using GadgetsOnline.Models;
using Microsoft.AspNetCore.Http;

namespace GadgetsOnline.Services
{
    public class ShoppingCart : IShoppingCart
    {
        private readonly CartRepository _cartRepository;
        private readonly OrderRepository _orderRepository;

        public ShoppingCart(CartRepository cartRepository, OrderRepository orderRepository)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
        }

        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public ShoppingCart GetCart(HttpContext context)
        {
            ShoppingCartId = GetCartId(context);
            return this;
        }

        internal int CreateOrder(Order order)
        {
            decimal orderTotal = 0;
            var cartItems = GetCartItems();

            // Create an order detail for each cart item and accumulate the total.
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Count
                };
                orderTotal += (item.Count * item.Product.Price);
                _orderRepository.AddOrderDetail(orderDetail);
            }

            // Persist the computed total on the order.
            order.Total = orderTotal;
            _orderRepository.UpdateTotal(order.OrderId, orderTotal);

            // Empty the shopping cart.
            _cartRepository.EmptyCart(ShoppingCartId);

            // Return the OrderId as the confirmation number.
            return order.OrderId;
        }

        public string GetCartId(HttpContext context)
        {
            if (context.Session.GetString(CartSessionKey) == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session.SetString(CartSessionKey, context.User.Identity.Name);
                }
                else
                {
                    // Generate a new random GUID to identify the anonymous cart.
                    Guid tempCartId = Guid.NewGuid();
                    context.Session.SetString(CartSessionKey, tempCartId.ToString());
                }
            }

            return context.Session.GetString(CartSessionKey);
        }

        public void AddToCart(int id)
        {
            _cartRepository.AddToCart(ShoppingCartId, id);
        }

        public int GetCount()
        {
            return _cartRepository.GetCount(ShoppingCartId);
        }

        internal int RemoveFromCart(int id)
        {
            return _cartRepository.RemoveFromCart(ShoppingCartId, id);
        }

        public List<Cart> GetCartItems()
        {
            return _cartRepository.GetCartItems(ShoppingCartId);
        }

        public decimal GetTotal()
        {
            return _cartRepository.GetTotal(ShoppingCartId);
        }
    }
}
