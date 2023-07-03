namespace SimpleContact.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends a main contact form email message to Optoma
    /// </summary>
    /// <param name="name">The name of the sender</param>
    /// <param name="email">The email address of the sender</param>
    /// <param name="message">The message to send</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Any parameter passed in a null or whitespace only will throw this error</exception>
    /// <exception cref="InvalidOperationException">Any parameter passed in a null or whitespace only will throw this error</exception>
    void SendContactEmail(string name, string email, string message);
}