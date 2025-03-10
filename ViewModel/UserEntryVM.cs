using Luxa.Models;

namespace Luxa.ViewModel
{
    public class UserEntryVM
    {
        public UserModel User { get; set; } = default!;
        public IList<string> Roles { get; set; } = default!;
    }
}
