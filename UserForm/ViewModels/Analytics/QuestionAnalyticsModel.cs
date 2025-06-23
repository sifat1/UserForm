namespace UserForm.ViewModels.Analytics;

    public class QuestionAnalyticsModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }

        // For number/text questions: average value or some stats
        public double? AverageNumberAnswer { get; set; }

        // For multiple choice questions: option frequencies
        public Dictionary<string, int> OptionFrequency { get; set; } = new();
    }