using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aviask.Models;

public enum Visibility
{
    Free,
    Public,
    Private
}

public class Question
{
    public int Id { get; set; }

    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    [Required] public MainCategoryType Category { get; set; }
    [Required] public Visibility Visibility { get; set; }
    [Required] [DisplayName("Sub-category")] public SubCategoriesType SubCategory { get; set; }

    [ForeignKey("QuestionAnswers")]
    public int QuestionAnswersId { get; set; }
    public QuestionAnswers QuestionAnswers { get; set; }

    public override string ToString() => $"<Question '{Title}' #{Id} : {QuestionAnswers}";
}

