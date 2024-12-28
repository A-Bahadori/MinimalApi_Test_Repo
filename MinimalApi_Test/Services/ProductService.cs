using MinimalApi_Test.Models;

namespace MinimalApi_Test.Services
{
    public class ProductService
    {
        private readonly List<Product> _products = new();
        private int _nextId = 1;

        public IEnumerable<Product> GetAll() => _products;

        public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

        public void Add(Product product)
        {
            product.Id = _nextId++;
            _products.Add(product);
        }

        public bool Update(int id, Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null) return false;

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            return true;
        }

        public bool Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null) return false;

            _products.Remove(product);
            return true;
        }
    }

}
