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

namespace CloudSurvey.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CloudSurvey.Models;
    using CloudSurvey.Repositories;

    public class SubmissionSummaryService : ISubmissionSummaryService
    {
        private readonly ISurveyRepository surveyRepository;

        public SubmissionSummaryService()
            : this(new SurveyRepository())
        { 
        }

        public SubmissionSummaryService(ISurveyRepository surveyRepository)
        {
            this.surveyRepository = surveyRepository;
        }

        public Dictionary<Guid, AnswersSummary> GetSubmissionsSummary(Guid surveyId)
        {
            // TODO: Persist summaries (or use cache)
            var survey = this.surveyRepository.Get(surveyId);
            var submissions = this.surveyRepository.GetSurveySubmissions(surveyId).ToList();
            var answers = new Dictionary<Guid, AnswersSummary>();

            foreach (var question in survey.Questions)
            {
                AnswersSummary answersSummary = null;

                switch (question.Type)
                {
                    case QuestionType.Boolean:
                        answersSummary = SummarizeYesNoAnswers(submissions, question);
                        break;
                    case QuestionType.Scale:
                        answersSummary = SummarizeScaleAnswers(submissions, question);
                        break;
                    case QuestionType.Text:
                        answersSummary = SummarizeTextAnswers(submissions, question);
                        break;
                }

                if (answersSummary != null)
                {
                    answers.Add(question.Id, answersSummary);
                }
            }

            return answers;
        }

        private static YesNoAnswersSummary SummarizeYesNoAnswers(IEnumerable<SurveySubmission> submissions, SurveyQuestion question)
        {
            var answersSummary = new YesNoAnswersSummary
            {
                Yes = 0,
                No = 0
            };

            foreach (var submission in submissions)
            {
                var answer = submission.Answers.SingleOrDefault(a => a.Question.Id == question.Id);

                if (answer != null && !string.IsNullOrEmpty(answer.Value))
                {
                    if (answer.Value.Equals("Yes"))
                    {
                        answersSummary.Yes++;
                    }
                    else
                    {
                        answersSummary.No++;
                    }
                }
            }

            return answersSummary;
        }

        private static ScaleAnswersSummary SummarizeScaleAnswers(IEnumerable<SurveySubmission> submissions, SurveyQuestion question)
        {
            var answersCount = 0;
            var total = 0;

            foreach (var submission in submissions)
            {
                var answer = submission.Answers.SingleOrDefault(a => a.Question.Id == question.Id);

                if (answer != null && !string.IsNullOrEmpty(answer.Value))
                {
                    var value = 0;
                    if (int.TryParse(answer.Value, out value))
                    {
                        total += value;
                        answersCount++;
                    }
                }
            }

            return new ScaleAnswersSummary
            {
                Average = answersCount > 0 ? Math.Round((decimal)total / answersCount, 2) : 0
            };
        }

        private static TextAnswersSummary SummarizeTextAnswers(IEnumerable<SurveySubmission> submissions, SurveyQuestion question)
        {
            var answersSummary = new TextAnswersSummary
            {
                Answers = new List<string>()
            };

            foreach (var submission in submissions)
            {
                var answer = submission.Answers.SingleOrDefault(a => a.Question.Id == question.Id);

                if (answer != null && !string.IsNullOrEmpty(answer.Value))
                {
                    answersSummary.Answers.Add(answer.Value);
                }
            }

            return answersSummary;
        }
    }
}