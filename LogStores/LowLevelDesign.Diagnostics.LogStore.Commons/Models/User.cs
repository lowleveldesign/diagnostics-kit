using Microsoft.AspNet.Identity;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public sealed class User : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public bool Enabled { get; set; }
    }
}
