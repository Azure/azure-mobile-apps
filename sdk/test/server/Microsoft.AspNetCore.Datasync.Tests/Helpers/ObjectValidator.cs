// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Datasync.Tests.Helpers;

/// <summary>
/// An object validator so that TryValidateModel will work on a controller.
/// </summary>
[ExcludeFromCodeCoverage]
internal class ObjectValidator : IObjectModelValidator
{
    public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model)
    {
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        if (!isValid)
        {
            results.ForEach(r => actionContext.ModelState.AddModelError(r.MemberNames.FirstOrDefault() ?? "", r.ErrorMessage));
        }
    }
}
