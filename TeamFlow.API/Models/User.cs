namespace TeamFlow.API.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }


    // Navigation Properties

    public ICollection<OrganizationMember> OrganizationMemberships
    {
        get;
        set;
    } = new List<OrganizationMember>();

    public ICollection<ProjectMember> ProjectMemberships
    {
        get;
        set;
    } = new List<ProjectMember>();

    public ICollection<TaskItem> CreatedTasks
    {
        get;
        set;
    } = new List<TaskItem>();

    public ICollection<TaskItem> AssignedTasks
    {
        get;
        set;
    } = new List<TaskItem>();

    public ICollection<Comment> Comments
    {
        get;
        set;
    } = new List<Comment>();

    public ICollection<FileAttachment> UploadedFiles
    {
        get;
        set;
    } = new List<FileAttachment>();

    public ICollection<Notification> Notifications
    {
        get;
        set;
    } = new List<Notification>();
}