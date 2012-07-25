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

namespace CloudSurvey.Repositories
{
    using System;
    using System.Collections.Generic;
    using CloudSurvey.Models;

    public interface ISurveyRepository : IDisposable
    {
        Survey Get(Guid surveyId);

        Survey GetBySlug(string surveySlug);

        IEnumerable<Survey> GetAll();

        Survey Insert(Survey survey);

        void Update(Survey survey, ICollection<SurveyQuestion> updatedQuestions);

        void Remove(Guid surveyId);

        IEnumerable<SurveySubmission> GetSurveySubmissions(Guid surveyId);

        SurveySubmission InsertSurveySubmission(SurveySubmission surveySubmission);
    }
}