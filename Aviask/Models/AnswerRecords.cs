using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aviask.Models
{
    public class AnswerRecords 
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [DataType(DataType.Date)] public DateTime Date { get; set; }

        [ForeignKey(nameof(Question))] public int QuestionId { get; set; }
        public Question Question { get; set; }

        public string Answered { get; set; }
        [DisplayName("Correct Answer")] public bool CorrectAnswer { get; set; }
    }
}
