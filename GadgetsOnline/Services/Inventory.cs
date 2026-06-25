using System.Collections.Generic;
using GadgetsOnline.Data;
using GadgetsOnline.Models;

namespace GadgetsOnline.Services
{
    public class Inventory : IInventory
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;

        public Inventory(ProductRepository productRepository, CategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public List<Product> GetBestSellers(int count)
        {
            return _productRepository.GetBestSellers(count);
        }

        public List<Product> GetFrequentlyBoughtTogether(int productId, int count)
        {
            return _productRepository.GetFrequentlyBoughtTogether(productId, count);
        }

        public List<Category> GetAllCategories()
        {
            return _categoryRepository.GetAll();
        }

        public List<Product> GetAllProductsInCategory(string category)
        {
            return _productRepository.GetByCategoryName(category);
        }

        public Product GetProductById(int id)
        {
            return _productRepository.GetById(id);
        }

        public string GetProductNameById(int id)
        {
            return _productRepository.GetById(id)?.Name;
        }
    }
}
