(function ($) {
    $(document).ready(function () {
        App = Ember.Application.create();

        /* Helpers */
        App.adjustDetailsViewHeight = function () {
            var delta = $('div.description textarea:visible').length > 0 ? 420 : 350;
            $('div.survey-questions').css('height', $(window).height() - delta);
        }

        /* Model */
        App.Survey = Ember.Object.extend({
            Id: '',
            Name: '',
            Description: '',
            Slug: '',
            Questions: [],

            createUrl: function () {
                var slug = this.get('Slug');
                if (!slug) {
                    return null;
                }

                return baseUrl + "/" + slug;
            }.property('Slug'),

            createSummaryUrl: function () {
                var slug = this.get('Slug');
                if (!slug) {
                    return null;
                }

                return baseUrl + "/summary/" + slug;
            }.property('Slug'),

            sortValue: function () {
                return this.get('Name');
            }.property('Name')
        });

        App.QuestionTypes = ['Text', 'Boolean', 'Scale'];

        /* Controllers */
        App.surveysController = Ember.ArrayController.create({
            content: [],

            add: function (survey) {
                var length = this.get('length'), idx;

                idx = this.binarySearch(survey.get('sortValue'), 0, length);
                this.insertAt(idx, survey);
            },

            binarySearch: function (value, low, high) {
                var mid, midValue;

                if (low === high) {
                    return low;
                }

                mid = low + Math.floor((high - low) / 2);
                midValue = this.objectAt(mid).get('sortValue');

                if (value > midValue) {
                    return this.binarySearch(value, mid + 1, high);
                } else if (value < midValue) {
                    return this.binarySearch(value, low, mid);
                }

                return mid;
            },

            remove: function (survey) {
                this.removeObject(survey);
            },
           
            newSurvey: function () {
                var newSurvey = {
                    Id: '',
                    Name: 'Your New Survey',
                    Slug: '',
                    Description: '',
                    Questions: []
                };

                this.add(App.Survey.create(newSurvey));
            },

            deleteSurvey: function () {
                var survey = App.selectedSurveyController.content;

                if (survey && confirm('\'' + survey.Name + '\'\n\r\n\rAre you sure you want to delete this survey?')) {

                    this.remove(survey);
                    App.selectedSurveyController.set('content', null);

                    if (survey.Id) {
                        $.ajax({
                            url: '/api/surveys/' + survey.Id,
                            type: 'DELETE',
                            success: function (response) { },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert('Unexpected Error: ' + jqXHR.statusText);
                            }
                        });
                    }
                }
            },

            loadSurveys: function () {
                var self = this;

                $.ajax({
                    url: '/api/surveys',
                    success: function (data) {
                        var surveys = data;
                        surveys = surveys.map(function (item) {
                            return self.createSurveyFromJSON(item);
                        });
                    },

                    error: function (jqXHR, textStatus, errorThrown) {
                        alert('Unexpected Error: ' + jqXHR.statusText);
                    }
                });
            },

            createSurveyFromJSON: function (json) {
                json.Questions = json.Questions.map(function (question, index) {
                    return {
                        id: question.Id,
                        description: question.Description,
                        type: App.QuestionTypes[question.Type],
                        index: question.Index
                    };
                });

                var survey = App.Survey.create(json);
                this.add(survey);

                return survey;
            },

            saveSurvey: function (survey) {
                if (!survey) {
                    survey = App.selectedSurveyController.get('content');
                }

                if (!survey.Name || 0 === survey.Name.length) {
                    alert('Survey name cannot be empty. Changes will not be saved until you specify a survey name.');
                } else {
                    var content = { Id: survey.Id, Name: survey.Name, Description: survey.Description, Questions: survey.Questions }

                    $.ajax({
                        url: '/api/surveys/' + survey.Id,
                        type: survey.Id ? 'PUT' : 'POST',
                        data: content,
                        success: function (response) {
                            if (!survey.Id) {
                                survey.set('Id', response.Id);
                            }

                            survey.set('Slug', response.Slug);                            

                            var questions = response.Questions.map(function (question, index) {
                                return {
                                    id: question.Id,
                                    description: question.Description,
                                    type: App.QuestionTypes[question.Type],
                                    index: question.Index
                                };
                            });

                            survey.set('Questions', questions);
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert('Unexpected Error: ' + jqXHR.statusText);
                        }
                    });
                }
            }
        });

        App.surveysController.loadSurveys();

        App.selectedSurveyController = Ember.Object.create({
            content: null
        });

        App.DeleteQuestionView = Ember.View.extend({
            classNames: ['delete-question-view'],
    
            click: function () {
                if (confirm('Are you sure you want to delete this question?')) {
                    var question = this.get('content');
                    var survey = this.getPath('contentView.content');
                    survey.get('Questions').removeObject(question);

                    App.surveysController.saveSurvey(survey);
                }
            },

            touchEnd: function () {
                this.click();
            }
        });

        App.EditField = Ember.View.extend({
            tagName: 'span',
            templateName: this.templateName ? this.templateName : 'edit-field',
            isMultiline: false,
            required: true,
            previousValue:"",

            isValid: function () {
                return (!this.required || (this.value && this.value.length > 0));
            },

            doubleClick: function () {
                this.previousValue = this.value;
                this.set('isEditing', true);
                App.adjustDetailsViewHeight();
                return false;
            },

            touchEnd: function () {
                // Double tap support
                var touchTime = new Date();

                if (this._lastTouchTime && touchTime - this._lastTouchTime < 250) {
                    this.doubleClick();
                    this._lastTouchTime = null;
                } else {
                    this._lastTouchTime = touchTime;
                }

                // Prevent zooming
                return false;
            },

            focusOut: function () {
                if (!this.isValid()) {
                    this.set('value', this.previousValue);
                } else {
                    App.surveysController.saveSurvey();
                }

                this.set('isEditing', false);
            },

            keyUp: function (evt) {
                if (this.isValid()) {
                    this.$('input:first').removeClass('input-validation-error');
                }

                switch (evt.keyCode) {
                    case 13:
                        if (!this.isValid()) {
                            this.$('input:first').addClass('input-validation-error').focus();
                        }
                        else {
                            this.set('isEditing', false);
                            App.surveysController.saveSurvey();
                        }

                        break;
                    case 27:
                        this.set('value', this.previousValue);                        
                        this.set('isEditing', false);
                        break;
                }
            }
        });

        App.SelectField = Ember.Select.extend({
            didInsertElement: function () {
                this.$().focus();
            }
        });

        App.TextField = Ember.TextField.extend({
            didInsertElement: function () {
                this.$().focus();
            }
        });

        App.TextArea = Ember.TextArea.extend({
            didInsertElement: function () {
                App.adjustDetailsViewHeight();
                this.$().focus();
            }
        });

        Ember.Handlebars.registerHelper('editable', function (path, options) {
            options.hash.valueBinding = path;
            return Ember.Handlebars.helpers.view.call(this, App.EditField, options);
        });

        Ember.Handlebars.registerHelper('button', function (options) {
            var hash = options.hash;

            if (!hash.target) {
                hash.target = "App.surveysController";
            }

            return Ember.Handlebars.helpers.view.call(this, Ember.Button, options);
        });

        App.SurveyListView = Ember.View.extend({
            classNameBindings: ['isSelected'],

            click: function (item) {
                var content = this.get('content');
                $('.more').hide();
                $('li.more', item.currentTarget).show();
                App.selectedSurveyController.set('content', content);
            },

            touchEnd: function () {
                this.click();
            },

            isSelected: function () {
                var selectedItem = App.selectedSurveyController.get('content'),
                    content = this.get('content');

                if (content === selectedItem) { return true; }
            }.property('App.selectedSurveyController.content')
        });

        App.SurveyDetailsView = Ember.View.extend({
            contentBinding: 'App.selectedSurveyController.content',
            classNames: ['surveyInfo'],

            addQuestion: function () {
                var questions = this.getPath('content.Questions');
                questions.pushObject({ id: '', description: 'Your New Question', type: App.QuestionTypes[0], index: questions.length + 1 });
            },

            didInsertElement: function () {
                App.adjustDetailsViewHeight();
            },

            fixQuestionsIndexes: function (sender, property, questions) {
                if (questions) {
                    questions.sort(function (a, b) {
                        return a.index - b.index;
                    });

                    $.each(questions, function (index) {
                        this.index = index + 1;
                    });
                }
            }.observes('content.Questions')
        });

        Ember.Link = Ember.View.extend({
            classNames: ['ember-link'],
            tagName: 'a',
            href: '#',
            target: 'parentView',
            value: '',
            attributeBindings: ['href', 'target', 'value']
        });

        $(window).resize(function (e) {
            App.adjustDetailsViewHeight();
        });
    });
})(jQuery)