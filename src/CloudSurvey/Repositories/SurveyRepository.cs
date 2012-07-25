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
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using CloudSurvey.Models;

    public class SurveyRepository : ISurveyRepository
    {
        private readonly SurveyContext surveyContext;

        public SurveyRepository()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SurveyConnection"].ConnectionString;
            this.surveyContext = new SurveyContext(connectionString);
        }

        public IEnumerable<Survey> GetAll()
        {
            return this.surveyContext.Surveys;
        }

        public Survey Get(Guid surveyId)
        {
            if (surveyId == Guid.Empty)
            {
                throw new ArgumentNullException("surveyId", "Parameter surveyId cannot be empty.");
            }

            return this.surveyContext.Surveys.Where(s => s.Id.Equals(surveyId)).FirstOrDefault();
        }

        public Survey GetBySlug(string surveySlug)
        {
            if (string.IsNullOrWhiteSpace(surveySlug))
            {
                throw new ArgumentNullException("surveySlug", "Parameter surveySlug cannot be null.");
            }

            return this.surveyContext.Surveys.Where(s => s.Slug.Equals(surveySlug, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public void Update(Survey survey, ICollection<SurveyQuestion> updatedQuestions)
        {
            if (survey == null)
            {
                throw new ArgumentNullException("survey", "Parameter survey cannot be null.");
            }

            var entry = this.surveyContext.Entry(survey);
            var attachedSurvey = this.surveyContext.Surveys.Attach(survey);

            // TODO: Review this (there should be a better way to update survey questions automatically - Review DB design)
            this.UpdateQuestions(attachedSurvey.Questions, updatedQuestions);
            
            entry.State = EntityState.Modified;
            this.surveyContext.SaveChanges();
        }

        public Survey Insert(Survey survey)
        {
            if (survey == null)
            {
                throw new ArgumentNullException("survey", "Parameter survey cannot be null.");
            }

            var addedSurvey = this.surveyContext.Surveys.Add(survey);
            this.surveyContext.SaveChanges();

            return addedSurvey;
        }

        public void Remove(Guid surveyId)
        {
            if (surveyId == Guid.Empty)
            {
                throw new ArgumentNullException("surveyId", "Parameter surveyId cannot be empty.");
            }

            var survey = this.surveyContext.Surveys.Find(surveyId);
            if (survey != null)
            {
                this.surveyContext.Surveys.Remove(survey);
                this.surveyContext.SaveChanges();
            }
        }

        public IEnumerable<SurveySubmission> GetSurveySubmissions(Guid surveyId)
        {
            if (surveyId == Guid.Empty)
            {
                throw new ArgumentNullException("surveyId", "Parameter surveyId cannot be empty.");
            }

            return this.surveyContext.SurveySubmissions.Where(s => s.Survey.Id == surveyId);
        }

        public SurveySubmission InsertSurveySubmission(SurveySubmission surveySubmission)
        {
            if (surveySubmission == null)
            {
                throw new ArgumentNullException("surveySubmission", "Parameter surveySubmission cannot be null.");
            }

            var addedSurveySubmission = this.surveyContext.SurveySubmissions.Add(surveySubmission);
            this.surveyContext.SaveChanges();

            return addedSurveySubmission;
        }

        public void Dispose()
        {
            if (this.surveyContext != null)
            {
                this.surveyContext.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void UpdateQuestions(ICollection<SurveyQuestion> currentQuestions, ICollection<SurveyQuestion> updatedQuestions)
        {
            List<Guid> currentQuestionsIds = currentQuestions.Select(q => q.Id).ToList();
            List<Guid> updatedQuestionsIds = updatedQuestions.Select(q => q.Id).ToList();
            List<Guid> deletedQuestionsIds = currentQuestionsIds.Except(updatedQuestionsIds).ToList();

            foreach (var questionId in deletedQuestionsIds)
            {
                var questionToRemove = currentQuestions.Where(q => q.Id == questionId).FirstOrDefault();

                if (questionToRemove != null)
                {
                    this.surveyContext.SurveyQuestions.Remove(questionToRemove);
                }
            }

            foreach (var question in updatedQuestions)
            {
                var questionToUpdate = currentQuestions.Where(q => q.Id == question.Id).FirstOrDefault();

                if (question.Id.Equals(Guid.Empty) || (questionToUpdate == null))
                {
                    currentQuestions.Add(question);
                }
                else
                {
                    questionToUpdate.Index = question.Index;
                    questionToUpdate.Description = question.Description;
                    questionToUpdate.TypeValue = question.TypeValue;
                }
            }
        }
    }
}