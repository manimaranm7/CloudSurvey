﻿// ---------------------------------------------------------------------------------- 
// Microsoft Developer & Platform Evangelism 
//  
// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// ---------------------------------------------------------------------------------- 
// The example companies, organizations, products, domain names, 
// e-mail addresses, logos, people, places, and events depicted 
// herein are fictitious.  No association with any real company, 
// organization, product, domain name, email address, logo, person, 
// places, or events is intended or should be inferred. 
// ---------------------------------------------------------------------------------- 

namespace CloudSurvey
{
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "SurveysApi",
                routeTemplate: "api/surveys/{id}",
                defaults: new { Controller = "Survey", id = RouteParameter.Optional });

            routes.MapHttpRoute(
                name: "SubmissionsApi",
                routeTemplate: "api/surveys/{surveyId}/submissions/{submissionId}",
                defaults: new { Controller = "SurveySubmission", submissionId = RouteParameter.Optional });

            routes.MapRoute(
                name: "SurveySummary",
                url: "Summary/{surveySlug}",
                defaults: new { controller = "Admin", action = "Summary" });

            routes.MapRoute(
                name: "SurveyForm",
                url: "{surveySlug}",
                defaults: new { controller = "SurveyForm", action = "Index" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional });
        }
    }
}