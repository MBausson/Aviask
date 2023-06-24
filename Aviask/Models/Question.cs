using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aviask.Models;

public enum Visibility
{
    Free,   //  Makes a question available to any person - registered or not
    Public, //  Makes a question available to any logged in person
    Private //  Makes a question available only to certain people : To be implemented or revised TODO
}

public class Question
{
    public int Id { get; set; }

    [Required] public string Title { get; set; }
    [Required] public string Description { get; set; }
    [Required] public MainCategoryType Category { get; set; }
    [Required] public Visibility Visibility { get; set; }
    [Required] [DisplayName("Sub-category")] public SubCategoriesType SubCategory { get; set; }
    public string? Source { get; set; }

    [ForeignKey("QuestionAnswers")]
    public int QuestionAnswersId { get; set; }
    public QuestionAnswers QuestionAnswers { get; set; }

    [ForeignKey(nameof(Publisher))]
    public string? PublisherId { get; set; }
    public IdentityUser? Publisher { get; set; }

    [DisplayName("Illustration Image")] public string? IllustrationPath { get; set; }

    public override string ToString() => $"<Question '{Title}' #{Id} : {QuestionAnswers}";
}

