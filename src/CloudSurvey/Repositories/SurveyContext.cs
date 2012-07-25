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
    using System.Configuration;
    using System.Data.Entity;
    using CloudSurvey.Models;

    public class SurveyContext : DbContext
    {
        public SurveyContext()
            : base(ConfigurationManager.ConnectionStrings["SurveyConnection"].ConnectionString)
        {
        }

        public SurveyContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<Survey> Surveys { get; set; }

        public DbSet<SurveySubmission> SurveySubmissions { get; set; }

        public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

        public DbSet<SurveyQuestion> SurveyQuestions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Survey>().HasMany(e => e.Questions).WithOptional().WillCascadeOnDelete(true);
        }
    }
}