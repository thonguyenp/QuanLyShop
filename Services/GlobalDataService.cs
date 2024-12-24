namespace QuanLyShop.Services
{
    public class GlobalDataService
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public object Get(string key)
        {
            if (_data.TryGetValue(key, out var value))
            {
                return value;
            }

            // Nếu khóa không tồn tại, trả về giá trị mặc định
            return 0;

        }

        public void Set(string key, object value)
        {
            _data[key] = value;
        }

        public void InitializeData()
        {
            Set("HomNay", 0);
            Set("HomQua", 0);
            Set("TuanNay", 0);
            Set("TuanTruoc", 0);
            Set("ThangNay", 0);
            Set("ThangTruoc", 0);
            Set("TatCa", 0);
            Set("visitors_online", 0);
        }

    }
}
