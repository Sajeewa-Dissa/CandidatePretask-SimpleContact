# Candidate take-home task

This project contains a simple contact form on the home page of a default project.
I have chosen to investigate the virtual email server provided at EtherealEmail to allow sending a realistic SMTP message.
I have developed the form to allow sending attachments (restricted to gif and jpeg only to simplify mime-type processing).

# Running the Application

Simply complete the form and click the Send button. You are also able to attach multiple jpeg and gif files.
A number of sample image files are placed in the Uploads folder for testing.

The submitted emails are available at the location https://ethereal.email
Login using the username and password found in the appsettings.json file to view all submitted emails.
You can generate new login credentials with a simple button-click and use this to create a new recipient mailbox to send message to.
In theory, the form will send real SMTP messages to any actual mail server if supplied with valid smtp credentials in config.

# Ideas for Further Development

- Maybe find a more efficient way of uploading attachments without having to physically save them to disk temporarily.
- Replace default MVC error handling with simple success/flags and logged errors (throwing exceptions is expensive).
- Replace success page with Bootstrap's built-in toasts. Also use this for reporting problems.
- investigate adding client-side validation to ASP.NET Core, without jQuery or unobtrusive validation.

# Caveats

- Please note I have not tested any server side-exception handling. It may not bubble-up correctly.

