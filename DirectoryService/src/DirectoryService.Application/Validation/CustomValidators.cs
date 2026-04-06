using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;

namespace DirectoryService.Application.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TProperty> MustBeValueObject<T, TProperty, TValueObject>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, Result<TValueObject, Errors>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Errors> result = factoryMethod.Invoke(value);

            if (result.IsSuccess)
                return;

            foreach (var error in result.Error)
            {
                context.AddFailure(error.Serialize());
            }
        });
    }
}