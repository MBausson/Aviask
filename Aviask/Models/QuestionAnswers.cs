using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Aviask.Models
{
    public class QuestionAnswers
    {
        public int Id { get; set; }
        public int NumberOfAnswers { get; set; }

        [Required] public string Answer1 { get; set; }
        [Required] public string Answer2 { get; set; }
        public string? Answer3 { get; set; }
        public string? Answer4 { get; set; }

        [Required][DisplayName("Correct answer")] public string CorrectAnswer { get; set; }
        public string Explications { get; set; }

        public override string ToString() => $"<Answers #{Id} ({NumberOfAnswers}) : {Answer1}, {Answer2} => {CorrectAnswer}>";
        public IEnumerable<string> GetAnswers()
        {
            var answers = new List<string>() { Answer1, Answer2 };

            if (Answer3 != null)
            {
                answers.Add(Answer3);
            }
            if (Answer4 != null)
            {
                answers.Add(Answer4);
            }

            return answers;
        }
    }
}
