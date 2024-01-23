// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace Examples.AspNet.Controllers;

public partial class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;

    public HomeController(ILogger<HomeController> logger)
    {
        this.logger = logger;
    }

    // For testing traditional routing. Ex: https://localhost:XXXX/
    [HttpGet]
    public ActionResult Index()
    {
        Logs.IndexRequested(this.logger);

        return this.View();
    }

    [HttpGet]
    [Route("about_attr_route/{customerId}")] // For testing attribute routing. Ex: https://localhost:XXXX/about_attr_route
    public ActionResult About(int? customerId)
    {
        Logs.AboutRequested(this.logger, customerId);

        this.ViewBag.Message = $"Your application description page for customer {customerId}.";

        return this.View();
    }

    private static partial class Logs
    {
        [LoggerMessage(LogLevel.Information, "Index requested")]
        public static partial void IndexRequested(ILogger logger);

        [LoggerMessage(LogLevel.Information, "About requested for CustomerId: {CustomerId}.")]
        public static partial void AboutRequested(ILogger logger, int? customerId);
    }
}
