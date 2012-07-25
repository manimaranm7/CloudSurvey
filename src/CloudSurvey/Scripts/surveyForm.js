(function ($) {
    $(document).ready(function () {
        App = Ember.Application.create();

        /* Models */
        App.StatusMessage = Ember.Object.extend({
            title: '',
            description: ''
        });

        App.Survey = Ember.Object.extend({
            Id: '',
            Name: '',
            Questions: []
        });        

        App.Question = Ember.Object.extend({
            Id: '',
            Index: 0,
            Description: '',
            Type: '',
            answer: '',

            isTextType: function() {
                return this.get('Type') == '0';
            },

            isBooleanType: function() {
                return this.get('Type') == '1';
            },
            
            isScaleType: function() {
                return this.get('Type') == '2';
            }
        });

        /* Controllers */
        App.surveyFormController = Ember.Object.create({
            content: null,

            submitEnabled: true,

            statusMessage: null,

            createSurveyCookie: function () {
                document.cookie = surveySlug + "=true";
            },

            surveyCookieExists: function () {
                var i,x,y,cookies=document.cookie.split(";");
                for (i=0;i<cookies.length;i++)
                {
                    x=cookies[i].substr(0,cookies[i].indexOf("="));
                    y=cookies[i].substr(cookies[i].indexOf("=")+1);
                    x=x.replace(/^\s+|\s+$/g,"");
                    if (x == surveySlug)
                    {
                        return true;
                    }
                }

                return false;
            },

            orderQuestions: function (sender, property, questions) {
                if (questions) {
                    questions.sort(function (a, b) {
                        return a.Index - b.Index;
                    });
                }
            }.observes('content.Questions'),

            submitSurvey: function () {
                var self = this;

                var survey = App.surveyFormController.content;
                var json = self.createJSONFromSurvey(survey);

                $.ajax({
                    url: 'api/surveys/' + survey.Id + '/submissions/',
                    type: 'POST',
                    data: json,
                    success: function (response) {
                        self.createSurveyCookie();
                        self.set('content', null);
                        self.set('statusMessage', App.StatusMessage.create({ title: 'thank you', description: 'The survey has been submitted.' }));                        
                    },

                    error: function (jqXHR, textStatus, errorThrown) {
                        alert('Unexpected Error: ' + jqXHR.statusText);
                        self.set('submitEnabled', true);
                    }
                });
            },

            loadSurvey: function () {
                var self = this;

                if (self.surveyCookieExists()) {
                    self.set('statusMessage', App.StatusMessage.create({ title: 'thank you', description: 'You have already submitted this survey.' }));
                    return;
                }

                self.set('statusMessage', App.StatusMessage.create({ title: 'loading...', description: 'The survey is being requested.' }));

                $.ajax({
                    url: '/api/surveys/?slug=' + surveySlug,
                    dataType: 'json',
                    success: function (data) {
                        var survey = data;
                        survey = self.createSurveyFromJSON(survey);

                        self.set('content', survey);
                    },

                    error: function (jqXHR, textStatus, errorThrown) {               
                        if (jqXHR.status == "404") {
                            self.set('statusMessage', App.StatusMessage.create({ title: 'sorry.', description: 'The survey you are requesting is not available. Please try again later.' }));
                        } else {
                            alert('Unexpected Error: ' + jqXHR.statusText);
                        }
                    }
                });
            },

            createSurveyFromJSON: function (json) {
                json.Questions = json.Questions.map(function (question) {
                    return App.Question.create(question);
                });

                var survey = App.Survey.create(json);
                return survey;
            },

            createJSONFromSurvey: function (survey) {
                var json = {
                    Survey: {
                        Id: survey.Id
                    },

                    Answers: survey.Questions.map(function (question) {
                        return {
                            Question: { Id: question.Id },
                            Value: question.answer
                        }
                    })
                };

                return json;
            }
        })

        App.surveyFormController.loadSurvey();

        App.BooleanValues = ['Yes', 'No'];

        App.BooleanSelect = Ember.Select.extend({
            content: App.BooleanValues
        });

        App.ScaleValues = ['1', '2', '3', '4', '5'];

        App.ScaleSelect = Ember.Select.extend({
            content: App.ScaleValues
        });

        App.SurveyDetailsView = Ember.View.extend({
            contentBinding: 'App.surveyFormController.content'
        });

        App.StatusMessageView = Ember.View.extend({
            contentBinding: 'App.surveyFormController.statusMessage'
        });

        App.SubmitButton = Ember.Button.extend({
            disabledBinding: Ember.Binding.not('App.surveyFormController.submitEnabled'),

            click: function () {
                App.surveyFormController.set('submitEnabled', false);
                App.surveyFormController.submitSurvey();
            }
        });

        Ember.Handlebars.registerHelper('isTextQuestion', function (block) {
            if (this.isTextType()) {
                return block(this);
            } else {
                return block.inverse(this);
            }
        });

        Ember.Handlebars.registerHelper('isBooleanQuestion', function (block) {
            if (this.isBooleanType()) {
                return block(this);
            } else {
                return block.inverse(this);
            }
        });

        Ember.Handlebars.registerHelper('isScaleQuestion', function (block) {
            if (this.isScaleType()) {
                return block(this);
            } else {
                return block.inverse(this);
            }
        });

        Ember.Handlebars.registerHelper('button', function (options) {
            var hash = options.hash;

            if (!hash.target) {
                hash.target = "App.surveyFormController";
            }

            return Ember.Handlebars.helpers.view.call(this, Ember.Button, options);
        });
    });
})(jQuery)