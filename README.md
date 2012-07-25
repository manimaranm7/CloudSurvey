# Cloud Survey Sample
The Cloud Survey sample demonstrates how to build modern web applications using the latest tools such as ASP.NET MVC 4, Ember.js, and SignalR. This sample consists of a survey designer page, a survey page, and a survey summary page. Each of the pages uses Ember.js to update the UI in real time and perform data binding. The survey summary page also uses SignalR to update the survey results in real-time as surveys are completed.

This sample is designed to be deployed to Windows Azure Web Sites and uses a Windows Azure SQL Database for persistence. You can run this site in either shared or reserved mode on Windows Azure Web Sites with any number of instances.

Below you will find system requirements and deployment instructions.

## System Requirements
* Visual Studio 2012 RC
* ASP.NET MVC 4 RC
* [NuGet (latest version)](http://www.nuget.org)
* Windows Azure Account - [Sign up for a free trial](https://www.windowsazure.com/en-us/pricing/free-trial/).

## Additional Resources
Click the links below for more information on the technologies used in this sample.

* Blog Post: [Windows Azure Web Sites Modern Application Sample - Cloud Survey](http://blog.ntotten.com/2012/07/24/windows-azure-web-sites-modern-application-sample-cloud-survey/)
* Screencast: [Intro to the Cloud Survey Sample](http://channel9.msdn.com/posts/Windows-Azure-Web-Sites-Modern-Application-Sample-Cloud-Survey)
* [Live Demo](http://cloudsurvey.azurewebsites.net) (username: admin; password: Contoso123!)
* [ASP.NET MVC 4](http://www.asp.net/mvc/mvc4)
* [ASP.NET Web API](http://www.asp.net/web-api/overview)
* [SignalR](http://signalr.net/)
* [Ember.js](http://emberjs.com/)

## Deployment Instructions

1. To begin, either download the latest version of the sample by clicking the [Zip button](https://github.com/WindowsAzure-Samples/CloudSurvey/zipball/master) on this page or by performing a git clone of this repository. You can either use the git command line tool or [Github for Windows](github-windows://openRepo/https://github.com/WindowsAzure-Samples/CloudSurvey) to clone the repository.

2. Next, you will need to create a Windows Azure Web Site and SQL Database. [Navigate to the Windows Azure Management Portal](http://manage.windowsazure.com), sign-in using your Microsoft Account, and then click the New button in the lower left corner of the page.

3. Click "Web Site" and then "Create with Database".

  ![New Web Site](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/newwebsite.png)

4. Complete the create web site form by setting you site URL, region, and subscription. Click the arrow to continue.

  ![Create Web Site](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/createwebsite1.png)
  
5. Next, specify the database information. You can use an existing SQL Database server or create a new one. After you complete the form, click the arrow to continue.

  ![Specify database settings](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/createwebsite2.png)
  
6. If you selected to create a new database server you will be asked to provide the credentials and region. If you used an existing server you will be asked to provide your existing SQL credentials. After you have completed the form, click the button to create your Web Site and database.

  ![Create a server](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/createwebsite2.png)
  
7. It will take a few seconds for your site and database to be created. After they are ready click the site to enter the dashboard. In the dashboard click the link to "Download publish profile." If you haven't already done this before you will need to setup your deployment credentials first. You can do this by clicking the deployment credentials link.

  > The publish profile contains all information needed to publish an application to your new Windows Azure Web Site. It includes deployment credentials and database credentials. Make sure to delete this file when you are finished or store it in a secure location.
 
  ![Download publish profile](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/downloadpublishprofile.png)
  
8. Now open the Cloud Survey application in Visual Studio. You can find the solution file at /src/CloudSurvey.sln. After the solution opens, right-click the CloudSurvey project and click "Publish".

  ![Publish Web Site](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/publishwebsite.png)
  
9. Next the Publish web application dialog will open. Click the import button. Select the publish profile you downloaded previously. Click publish to continue.

  ![Publish web application](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/publishweb1.png)
  
10. After you have imported your publish profile the settings neccisary to publish the will be configured automatically. You will see the various Web Deploy settings such as Service URL and Username have been populated. Click publish to continue.

  ![Publish web application](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/publishweb2.png)
  
11. Next you are asked to set the connection strings to the databases. This information has also been configured from the publish profile and is set to the database you created for this web site. Click publish to publish the application.

  ![Publish web application](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/publishweb3.png)
  
12. Publishing the site will take a few seconds. After it is complete Visual Studio will open the site in the browser. You will first be presented with the login page.

  > The default credentials are 'admin' and 'Contoso123!'. You can change the default credentials by modifying the following code in the Global.asax.cs file.

  ```csharp
  // Create an admin account
  if (Membership.FindUsersByName("admin").Cast<MembershipUser>().FirstOrDefault() == null)
  {
      Membership.CreateUser("admin", "Contoso123!", "admin@contoso.com");
  }
  ```

  ![Cloud Survey Login](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/cloudsurvey_login.png)
  
13. After you login you can create your first survey. To create a survey click the "New Survey" button.

  > Fields in the survey editor can be modified by double clicking the text. You can see below how you can edit the survey title.
  
  ![Create new survey](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/cloudsurvey_createsurvey.png)
  
14. Add questions by clicking the "Add question" button. You can select three different question types: text, boolean, or scale.

  ![Create question](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/cloudsurvey_addquestion.png)
  
15. Click the survey link to go to go to the survey form. You can see each of your questions are availible immediately without having to explicitly save your survey.

  ![Survey Form](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/cloudsurvey_surveyform.png)
  
16. Admins can view the survey results on the summary page. In the survey editor click "view summary" to view the results.

  > Notice that if you complete the survey form again, the survey summary page will update automatically. The application uses SignalR to push updates to the UI in real-time.
  
  ![Summary](https://github.com/WindowsAzure-Samples/CloudSurvey/raw/master/assets/cloudsurvey_summary.png)
  