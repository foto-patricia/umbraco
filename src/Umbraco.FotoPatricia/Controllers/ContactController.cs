using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.FotoPatricia.Models;

namespace Umbraco.FotoPatricia.Controllers;

public class ContactFormController : SurfaceController
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactFormController> _logger;

    public ContactFormController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IEmailSender emailSender,
        IConfiguration configuration,
        ILogger<ContactFormController> logger) 
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Submit(ContactFormViewModel model)
    {
        _logger.LogInformation("Got a post to submit contact formular...");
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Contact Form invalid!");
            return CurrentUmbracoPage();
        }

        _logger.LogInformation("Contact Form valid, going to send email...");

        var smtpSettings = new SmtpSettings();
        _configuration.GetSection("Umbraco:CMS:Global:Smtp").Bind(smtpSettings);
        var to = _configuration.GetValue<string>("To");

        if (!string.IsNullOrEmpty(to))
        {
            _logger.LogInformation("Everything configured, building message and sending it...");
            var emailMessage = new EmailMessage(
                smtpSettings.From, 
                to, 
                "Contact Form",
                $"Contact Form: {model.Name} ({model.Email}, {model.PhoneNumber}): {model.Message}", 
                false);

            _emailSender.SendAsync(
                emailMessage, 
                Constants.Web.EmailTypes.Notification, 
                true);
        }
        else
        {
            _logger.LogWarning("No recipient email address (To) configured!");
        }

        return RedirectToCurrentUmbracoPage();
    }
}
