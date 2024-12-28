namespace MinimalApi_Test.Services
{
    public class ItemService
    {
        private readonly List<string> _items = new List<string>();

        public IEnumerable<string> GetAll() => _items;

        public string? Get(int index) => index >= 0 && index < _items.Count ? _items[index] : null;

        public void Add(string item) => _items.Add(item);

        public bool Update(int index, string updatedItem)
        {
            if (index < 0 || index >= _items.Count) return false;
            _items[index] = updatedItem;
            return true;
        }

        public bool Delete(int index)
        {
            if (index < 0 || index >= _items.Count) return false;
            _items.RemoveAt(index);
            return true;
        }
    }
}
