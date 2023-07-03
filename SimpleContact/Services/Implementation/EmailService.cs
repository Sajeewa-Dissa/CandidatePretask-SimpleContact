namespace SimpleContact.Services.Implementation;

public class EmailService : IEmailService
{
    public void SendContactEmail(string name, string email, string message)
    {
        //Check nulls
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));
        
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));


        if (name.Length > 128)
            throw new InvalidOperationException("Name is bigger then 128 characters");
        
        if (email.Length > 256)
            throw new InvalidOperationException("Email is bigger then 256 characters");
        
        if (message.Length > 2048)
            throw new InvalidOperationException("Message is bigger then 2048 characters");
        
        
        //This function would then send an email to optoma.
        //You do NOT need to complete this method.

    }
}