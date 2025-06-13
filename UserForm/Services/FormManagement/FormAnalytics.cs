using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;

namespace UserForm.Services.FormManagement;

public class FormAnalytics(AppDbContext context)
{
    public async Task<List<object>> GetQFreq(int userFormId)
    {
        var submissions = await context.UserSubmittedForms
            .Where(f => f.UserFormsId == userFormId)
            .Include(f => f.BaseQuestion)
            .Include(f => f.Submittedoption)
            .ToListAsync();

        var processed = submissions.Select(s =>
        {
            var question = s.BaseQuestion;
            string answer;

            if (s.Submittedoption != null)
            {
                answer = s.Submittedoption.OptionText ?? "Unknown";
            }
            else if (!string.IsNullOrEmpty(s.Submittedtext))
            {
                answer = s.Submittedtext;
            }
            else
            {
                answer = "Unknown";
            }

            return new
            {
                QuestionText = question.Questiontxt,
                Answer = answer
            };
        });

        var result = processed
            .GroupBy(p => p.QuestionText)
            .Select(g => new
            {
                Question = g.Key,
                Answers = g.GroupBy(a => a.Answer)
                    .Select(ag => new
                    {
                        Answer = ag.Key,
                        Count = ag.Count()
                    }).ToList()
            })
            .Cast<object>()
            .ToList();

        return result;
    }

}