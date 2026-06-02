using FluentValidation;
using GbfsQuiz.Application.Features.Games.Requests;

namespace GbfsQuiz.Application.Features.Games.Validators;

public sealed class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
{
    public SubmitAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.ChoiceId).NotEmpty();
    }
}
