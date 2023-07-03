using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Cryptography;
using SimpleContact.Controllers;
using SimpleContact.Models;
using System.IO;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SimpleContact.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger, IHostEnvironment env)
    {
        _config = config;
        _logger = logger;
        _env = env;
    }


    public async Task<bool> SendContactEmailAsync(EmailData emailData, string? folderName)
    {
        //Server side validation
        ValidateParams(emailData.Name, emailData.Email, emailData.Message);

        try
        {
            if (folderName is { })
            {
                //Call method to send an email to optoma with attachments.
                await SendVirtualEmailWithAttachmentAsync(emailData, folderName);
            }
            else
            {
                //Call method to send an email to optoma without attachments
                await SendVirtualEmailAsync(emailData);
            }
            return true; //success indicator
        }
        catch
        {
            //TODO Log any errors here.
            return false; //failure indicator
        }

    }



    private void ValidateParams(string? name, string? email, string? message)
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
    }



    private async Task SendVirtualEmailAsync(EmailData emailData)
    {

        var smtpServer = _config.GetSection("EmailSettings:EmailHost").Value;
        int portNo;
        if (!int.TryParse(_config.GetSection("EmailSettings:EmailPort").Value, out portNo))
        {
            throw new InvalidOperationException("No valid port number was found in Config");
        }
        var smtpUsername = _config.GetSection("EmailSettings:EmailUsername").Value;
        var smtpPassword = _config.GetSection("EmailSettings:EmailPassword").Value;
        var smtpRecipient = _config.GetSection("EmailSettings:EmailRecipient").Value;

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(smtpUsername));
        email.To.Add(MailboxAddress.Parse(smtpRecipient));
        email.Subject = "Email Submission From " + emailData.Name;
        email.Body = CreateMessageBody(emailData.Name!, emailData.Email!, emailData.Message!);

        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        {
            try
            {
                await smtp.ConnectAsync(smtpServer, portNo, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                await smtp.SendAsync(email);
            }
            catch
            {
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }



    private async Task SendVirtualEmailWithAttachmentAsync(EmailData emailData, string folderName)
    {
        UploadFilesLocal(folderName, emailData.Attachments!);

        var smtpServer = _config.GetSection("EmailSettings:EmailHost").Value;
        int portNo;
        if (!int.TryParse(_config.GetSection("EmailSettings:EmailPort").Value, out portNo))
        {
            throw new InvalidOperationException("No valid port number was found in Config");
        }
        var smtpUsername = _config.GetSection("EmailSettings:EmailUsername").Value;
        var smtpPassword = _config.GetSection("EmailSettings:EmailPassword").Value;
        var smtpRecipient = _config.GetSection("EmailSettings:EmailRecipient").Value;

        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(smtpUsername));
        email.To.Add(MailboxAddress.Parse(smtpRecipient));
        email.Subject = "Email Submission From " + emailData.Name;

        var body = CreateMessageBody(emailData.Name!, emailData.Email!, emailData.Message!);

        // now create the multipart/mixed container to hold the message text and the image attachments
        var multipart = new Multipart("mixed");
        multipart.Add(body);

        var fullPath = _env.ContentRootPath + "Uploads\\" + folderName;

        if (Directory.Exists(fullPath))
        {
            string[]? fileEntries = Directory.GetFiles(fullPath);

            foreach(string filename in fileEntries)
            {
                AddImageAttachment(multipart, filename);
            }
        }

        // now set the multipart/mixed as the message body
        email.Body = multipart;

        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
        {
            try
            {
                await smtp.ConnectAsync(smtpServer, portNo, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                await smtp.SendAsync(email);
            }
            catch
            {
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true);
                multipart.Dispose(); //need to free up the file handles.

                //delete the temp folder
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true); //must be recursive.
                }
            }
        }
    }


    private TextPart CreateMessageBody(string name, string emailAddr, string message)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<h2>The following message was submitted to the Email Submission Form.</h2>");
        sb.AppendLine("Message From: " + name + "<BR>");
        sb.AppendLine("Reply Email Address: " + emailAddr + "<BR>");
        sb.AppendLine("<BR>");
        sb.AppendLine("Message Content:");
        sb.AppendLine("<p>" + message + "</p>");

        return new TextPart(MimeKit.Text.TextFormat.Html) { Text = sb.ToString() };
    }



    private void UploadFilesLocal(string folderName, IFormFile[] formFiles)
    {
        var path = _env.ContentRootPath + "Uploads\\" + folderName;

        //create folder if not exist
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        foreach (var file in formFiles)
        {
            string fileNameWithPath = Path.Combine(path, file.FileName);

            if (file.ContentType.ToLower() == "image/jpg" || file.ContentType.ToLower() == "image/jpeg" || file.ContentType.ToLower() == "image/gif")
            {
                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
        }
    }

    private void AddImageAttachment(Multipart multipart, string fileName)
    {
        string extension = Path.GetExtension(fileName);
        string subType;

        switch (extension.ToLower())
        {
            case ".gif":
                subType = "gif";
                break;
            case ".jpg":
                subType = "jpg";
                break;
            case ".jpeg":
                subType = "jpg";
                break;
            default:
                throw new InvalidOperationException(nameof(extension) + " is an invalid file type");
        }

        // create an image attachment for the file located at path
        var attachment = new MimePart("image", subType)
        {
            Content = new MimeContent(File.OpenRead(fileName)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName(fileName)
        };
        multipart.Add(attachment);
    }

}