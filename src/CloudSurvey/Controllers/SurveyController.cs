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

namespace CloudSurvey.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using CloudSurvey.Helpers;
    using CloudSurvey.Models;
    using CloudSurvey.Repositories;
    using CloudSurvey.Services;

    [Authorize]
    public class SurveyController : ApiController
    {
        private readonly ISurveyRepository surveyRepository;

        public SurveyController(ISurveyRepository surveyRepository)
        {
            if (surveyRepository == null)
            {
                throw new ArgumentNullException("surveyRepository");
            }

            this.surveyRepository = surveyRepository;
        }

        public IEnumerable<Survey> Get()
        {
            return this.surveyRepository.GetAll();
        }

        public Survey Get(Guid id)
        {
            var survey = this.surveyRepository.Get(id);

            if (survey == null)
            {
                throw this.WebException(HttpStatusCode.NotFound);
            }

            return survey;
        }

        [AllowAnonymous]
        public Survey GetBySlug(string slug)
        {
            var survey = this.surveyRepository.GetBySlug(slug);

            if (survey == null)
            {
                throw this.WebException(HttpStatusCode.NotFound);
            }

            return survey;
        }

        public HttpResponseMessage Post(Survey survey)
        {
            if (string.IsNullOrWhiteSpace(survey.Name))
            {
                throw this.WebException(HttpStatusCode.BadRequest, "Survey name cannot be empty");
            }

            survey.Slug = survey.Name.GenerateSlug();
            var addedSurvey = this.surveyRepository.Insert(survey);

            // Response Message
            var response = this.Request.CreateResponse<Survey>(HttpStatusCode.Created, addedSurvey);
            var surveyUri = Url.Link("SurveysApi", new { Id = addedSurvey.Id });
            response.Headers.Location = new Uri(surveyUri);
            
            return response;
        }

        public HttpResponseMessage Put(Guid id, Survey survey)
        {
            if (string.IsNullOrWhiteSpace(survey.Name))
            {
                throw this.WebException(HttpStatusCode.BadRequest, "Survey name cannot be empty");
            }

            var currentSurvey = this.surveyRepository.Get(id);
            if (currentSurvey != null)
            {
                if (!currentSurvey.Name.Equals(survey.Name, StringComparison.InvariantCulture))
                {
                    currentSurvey.Name = survey.Name;
                    currentSurvey.Slug = currentSurvey.Name.GenerateSlug();
                }

                currentSurvey.Description = survey.Description;

                this.surveyRepository.Update(currentSurvey, survey.Questions);

                // Response Message
                return this.Request.CreateResponse<Survey>(HttpStatusCode.OK, currentSurvey);
            }
            else
            {
                // We are not supporting creation through PUT
                throw this.WebException(HttpStatusCode.NotFound);
            }
        }

        public HttpResponseMessage Delete(Guid id)
        {
            this.surveyRepository.Remove(id);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}