using System.Text.Json;
using DirectoryService.Shared.CustomErrors;
using FluentValidation.Results;

namespace DirectoryService.Application.Validation;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult result)
    {
        var listErrors = new List<Error>();

        foreach (var error in result.Errors)
        {
            var deserializedError = Error.Deserialize(error.ErrorMessage);
            listErrors.Add(deserializedError);
        }

        return new Errors(listErrors);
    }
}