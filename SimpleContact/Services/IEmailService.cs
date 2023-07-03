using SimpleContact.Models;

namespace SimpleContact.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends a main contact form email message to Optoma (or the configured recipient)
    /// </summary>
    /// <param name="data">DTO object holding email data</param>
    /// <param name="folderName">Optional folder to upload attached files</param>
    /// <returns>Success indicator wrappped in a task</returns>
    Task<bool> SendContactEmailAsync(EmailData data, string? folderName);
   
}