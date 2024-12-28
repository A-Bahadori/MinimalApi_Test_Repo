using MinimalApi_Test.DTOs.User;

namespace MinimalApi_Test.Result
{
    public class SearchUsersResult
    {
        public List<UserDto> Items { get; set; }
        public int TotalCount { get; set; }

        public SearchUsersResult((List<UserDto> Items, int TotalCount) tuple)
        {
            Items = tuple.Items;
            TotalCount = tuple.TotalCount;
        }
    }
}
