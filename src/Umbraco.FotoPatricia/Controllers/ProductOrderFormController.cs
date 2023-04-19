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
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.FotoPatricia.Models;

namespace Umbraco.FotoPatricia.Controllers;

public class ProductOrderFormController : SurfaceController
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContactFormController> _logger;

    public ProductOrderFormController(
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
    public IActionResult Submit(ProductOrderViewModel model)
    {
        _logger.LogInformation("Got a post to submit product order...");
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Product Order form invalid!");
            return CurrentUmbracoPage();
        }

        _logger.LogInformation("Product Order valid, going to send email...");

        var smtpSettings = new SmtpSettings();
        _configuration.GetSection("Umbraco:CMS:Global:Smtp").Bind(smtpSettings);
        var to = _configuration.GetValue<string>("To");

        if (!string.IsNullOrEmpty(to))
        {
            _logger.LogInformation("Everything configured, building message and sending it...");
            var message = @$"
{model.Product}

{model.Name} ({model.Email})
{model.PhoneNumber}
{model.Address})
Wochentag: {model.PreferredWeekday1} / {model.PreferredWeekday2}

{model.Message}
";
            var emailMessage = new EmailMessage(
                smtpSettings.From, 
                to, 
                $"{model.Product} wurde bestellt von {model.Name}",
                message,
                false);

            _emailSender.SendAsync(
                emailMessage, 
                Constants.Web.EmailTypes.Notification, 
                true);

            TempData.Add("IsSuccess", true);
        }
        else
        {
            _logger.LogWarning("No recipient email address (To) configured!");
        }

        return RedirectToCurrentUmbracoPage();
    }
}