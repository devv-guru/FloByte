namespace FloByte.Designer.Components.Topbar;

using Microsoft.AspNetCore.Components;

public class TopBarBase : ComponentBase
{
    /// Tells MainLayout to toggle the sidebar
    [Parameter] public EventCallback ToggleSidebar { get; set; }

    /// If provided, these will render as breadcrumbs
    [Parameter] public IReadOnlyList<BreadcrumbItem>? Breadcrumbs { get; set; }

    /// User display name pieces
    [Parameter] public string FirstName { get; set; } = "";
    [Parameter] public string LastName  { get; set; } = "";

    /// Optional gravatar or custom image
    [Parameter] public string? AvatarUrl { get; set; }

    protected bool _profileOpen;

    protected string Initials
        => $"{(string.IsNullOrEmpty(FirstName) ? "" : FirstName[0].ToString())}"
           + $"{(string.IsNullOrEmpty(LastName)  ? "" : LastName[0].ToString())}"
               .ToUpper();

    protected async Task OnToggleSidebar()
        => await ToggleSidebar.InvokeAsync(null);

    protected void ToggleProfileMenu()
        => _profileOpen = !_profileOpen;
}
