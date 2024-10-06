using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.IdentityModel.Tokens;

namespace AviaskApi.Entities;

public class QuestionAnswers
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Answer1 { get; set; } = null!;
    public string Answer2 { get; set; } = null!;
    public string? Answer3 { get; set; }
    public string? Answer4 { get; set; }

    public string CorrectAnswer { get; set; } = null!;
    public string? Explications { get; set; }

    public List<string> GetPossibleAnswers()
    {
        List<string> possible =
        [
            Answer1,
            Answer2
        ];

        if (!Answer3.IsNullOrEmpty()) possible.Add(Answer3!);

        if (!Answer4.IsNullOrEmpty()) possible.Add(Answer4!);

        return possible;
    }
}