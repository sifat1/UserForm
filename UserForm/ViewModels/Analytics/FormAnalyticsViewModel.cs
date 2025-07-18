namespace UserForm.ViewModels.Analytics;

    public class FormAnalyticsViewModel
    {
        public int FormId { get; set; }
        public string FormTitle { get; set; }
        public string FormTopic { get; set; } 
        public int TotalResponses { get; set; }       

        public List<QuestionAnalyticsModel> Questions { get; set; } = new();
    }