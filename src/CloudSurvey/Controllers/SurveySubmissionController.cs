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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using CloudSurvey.Helpers;
    using CloudSurvey.Models;
    using CloudSurvey.Repositories;
    using CloudSurvey.Services;
    using SignalR;

    [Authorize]
    public class SurveySubmissionController : ApiController
    {
        private readonly ISurveyRepository surveyRepository;
        private readonly ISubmissionSummaryService submissionSummaryService;

        public SurveySubmissionController(ISurveyRepository surveyRepository, ISubmissionSummaryService submissionSummaryService)
        {
            if (surveyRepository == null)
            {
                throw new ArgumentNullException("surveyRepository");
            }

            if (submissionSummaryService == null)
            {
                throw new ArgumentNullException("submissionSummaryService");
            }

            this.surveyRepository = surveyRepository;
            this.submissionSummaryService = submissionSummaryService;
        }

        public Dictionary<Guid, AnswersSummary> Get(Guid surveyId)
        {
            var survey = this.surveyRepository.Get(surveyId);

            if (survey == null)
            {
                throw this.WebException(HttpStatusCode.NotFound);
            }

            var submissionSummaryService = new SubmissionSummaryService(this.surveyRepository);
            var summary = submissionSummaryService.GetSubmissionsSummary(surveyId);

            return summary;
        }

        // POST api/surveys/{surveyId}/submissions
        [AllowAnonymous]
        public HttpResponseMessage Post(Guid surveyId, SurveySubmission submission)
        {
            var survey = this.surveyRepository.Get(surveyId);

            if (survey == null)
            {
                throw this.WebException(HttpStatusCode.NotFound);
            }

            var surveySubmission = new SurveySubmission
            {
                Survey = survey
            };

            submission.Answers.ToList().ForEach(
                a =>
                {
                    var question = survey.Questions.FirstOrDefault(q => q.Id == a.Question.Id);
                    if (question != null)
                    {
                        surveySubmission.Answers.Add(
                            new SurveyAnswer
                            {
                                Value = a.Value,
                                Question = question
                            });
                    }
                });

            var addedSurveySubmission = this.surveyRepository.InsertSurveySubmission(surveySubmission);

            // Notify Submission to SignalR clients
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SurveyHub>();
            if (hubContext.Clients != null)
            {
                var answers = this.submissionSummaryService.GetSubmissionsSummary(surveyId);
                hubContext.Clients.updateSurveyResults(new { SurveyId = surveyId, GroupedAnswers = answers });
            }

            // Response Message
            var response = this.Request.CreateResponse<SurveySubmission>(HttpStatusCode.Created, addedSurveySubmission);
            var surveyUri = Url.Link("SubmissionsApi", new { SurveyId = surveyId, SubmissionId = addedSurveySubmission.Id });
            response.Headers.Location = new Uri(surveyUri);

            return response;
        }
    }
}