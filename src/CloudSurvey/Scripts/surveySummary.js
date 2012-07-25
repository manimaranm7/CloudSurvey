(function ($) {
    $(document).ready(function () {
        App = Ember.Application.create();

        App.QuestionTypes = ['Text', 'Boolean', 'Scale'];

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
            Results: null,

            isTextType: function() {
                return this.get('Type') == '0';
            },

            isBooleanType: function() {
                return this.get('Type') == '1';
            },
            
            isScaleType: function() {
                return this.get('Type') == '2';
            },

            getTypeName: function() {
                return App.QuestionTypes[this.get('Type')];
            }
        });

        /* Controllers */
        App.surveySummaryController = Ember.Object.create({
            content: null,

            statusMessage: null,

            orderQuestions: function (sender, property, questions) {
                if (questions) {
                    questions.sort(function (a, b) {
                        return a.Index - b.Index;
                    });
                }
            }.observes('content.Questions'),

            loadSurvey: function () {
                var self = this;

                self.set('statusMessage', App.StatusMessage.create({ title: 'loading...', description: 'The survey is being requested.' }));

                $.ajax({
                    url: '/api/surveys/?slug=' + surveySlug,
                    dataType: 'json',
                    success: function (data) {
                        var survey = data;
                        survey = self.createSurveyFromJSON(survey);

                        self.set('content', survey);

                        $.ajax({
                            url: '/api/surveys/' + survey.Id + '/submissions/',
                            dataType: 'json',
                            success: function (data) {
                                self.updateSurveyResults(data);
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert('Unexpected Error: ' + jqXHR.statusText);
                            }
                        });
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

            updateSurveyResults: function (results) {
                var self = this;
                var survey = self.get('content');

                if (survey) {
                    survey.set('Questions', survey.Questions.map(function (question, index) {
                        var questionResults = results[question.Id];
 
                        if (question.isTextType()) {
                            if (questionResults.Answers.length > 0) {
                                question.set('Results', questionResults.Answers);
                            }
                        };
                        if (question.isScaleType()) {
                            if (questionResults.Average > 0) {
                                question.set('Results', { average: questionResults.Average });
                            }
                        }
                        if (question.isBooleanType()) {
                            if (questionResults.Yes > 0 || questionResults.No > 0) {
                                question.set('Results', { yes: questionResults.Yes, no: questionResults.No });
                            }
                        }

                        return question;
                    }));
                }
            }
        });

        App.ResultsArea = Ember.View.extend({
            tagName: 'div',
            classNameBindings: ['resultType', 'questionId'],

            resultType: function () {
                return 'result' + this.value.getTypeName();
            }.property(),

            questionId: function () {
                return this.value.Id;
            }.property(),

            didInsertElement: function () {
                var question = this.value;

                if (question.Results) {
                    var area = $('.' + question.Id + '.result' + question.getTypeName())[0];
                    eval('this.update' + question.getTypeName() + 'Results(area, question);');
                }
            },

            updateBooleanResults: function (area, question) {
                var yesCount = [[0, question.Results ? question.Results.yes : 0]],
                    noCount = [[0, question.Results ? question.Results.no : 0]];

                Flotr.draw(
                  area, [
                    { data: yesCount, label: 'Yes' },
                    { data: noCount, label: 'No' },
                  ], {
                      HtmlText: false,
                      grid: { verticalLines: false, horizontalLines: false, outlineWidth: 0 },
                      xaxis: { showLabels: false },
                      yaxis: { showLabels: false },
                      pie: { show: true },
                      legend: { show: true, position: 'se' }
                  });
            },

            updateScaleResults: function (area, question) {
                var average = [[[question.Results ? question.Results.average : 0, 0]]];

                Flotr.draw(
                    area,
                    average,
                    {
                        grid: { verticalLines: true, horizontalLines: false, outlineWidth: 0 },
                        bars: { show: true, horizontal: true },
                        yaxis: { min: 0, autoscaleMargin: 1, showLabels: false },
                        xaxis: { min: 0, max: 5, showLabels: true }
                    }
                );
            },

            updateTextResults: function (area, question) {
                $(question.Results).each(function (index, result) {
                    var element = $('<p></p>').append(document.createTextNode(result));
                    $(area).append(element);
                });
            }
        });

        App.surveySummaryController.loadSurvey();

        // SignalR - Update Survey Results
        $(function () {
            $.connection.surveyHub.updateSurveyResults = function (results) {
                if (App.surveySummaryController.get('content').Id == results.SurveyId) {
                    App.surveySummaryController.updateSurveyResults(results.GroupedAnswers);
                }
            };

            $.connection.hub.start(function () { });
        });

        App.SurveyDetailsView = Ember.View.extend({
            contentBinding: 'App.surveySummaryController.content'
        });

        App.StatusMessageView = Ember.View.extend({
            contentBinding: 'App.surveySummaryController.statusMessage'
        });
    });
})(jQuery)