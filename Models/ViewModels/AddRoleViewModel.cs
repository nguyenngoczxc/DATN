namespace TTTN3.Models.ViewModels
{
    public class AddRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> RoleNames { get; set; }
        public string SelectedRoleName { get; set; }
    }

}
