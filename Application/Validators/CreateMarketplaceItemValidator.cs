using FluentValidation;
using resultsService.Application.DTOs;

namespace resultsService.Application.Validators;

/// <summary>
/// Validator for CreateMarketplaceItemDto
/// </summary>
public class CreateMarketplaceItemValidator : AbstractValidator<CreateMarketplaceItemDto>
{
    public CreateMarketplaceItemValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ItemId is required")
            .MaximumLength(255)
            .WithMessage("ItemId cannot exceed 255 characters");

        RuleFor(x => x.ItemTitle)
            .NotEmpty()
            .WithMessage("ItemTitle is required")
            .MaximumLength(500)
            .WithMessage("ItemTitle cannot exceed 500 characters");

        RuleFor(x => x.PlatformId)
            .GreaterThan(0)
            .WithMessage("PlatformId must be greater than 0");

        RuleFor(x => x.ItemImageUrl)
            .MaximumLength(1000)
            .WithMessage("ItemImageUrl cannot exceed 1000 characters")
            .Must(BeValidUrl)
            .WithMessage("ItemImageUrl must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.ItemImageUrl));

        RuleFor(x => x.ItemUrl)
            .MaximumLength(1000)
            .WithMessage("ItemUrl cannot exceed 1000 characters")
            .Must(BeValidUrl)
            .WithMessage("ItemUrl must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.ItemUrl));

        RuleFor(x => x.SearchTerm)
            .MaximumLength(255)
            .WithMessage("SearchTerm cannot exceed 255 characters");

        RuleFor(x => x.Quantity)
            .MaximumLength(100)
            .WithMessage("Quantity cannot exceed 100 characters");

        RuleFor(x => x.QuantityNumber)
            .GreaterThanOrEqualTo(0)
            .WithMessage("QuantityNumber must be greater than or equal to 0")
            .When(x => x.QuantityNumber.HasValue);

        RuleFor(x => x.Price)
            .MaximumLength(100)
            .WithMessage("Price cannot exceed 100 characters");

        RuleFor(x => x.PriceUsd)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PriceUsd must be greater than or equal to 0")
            .When(x => x.PriceUsd.HasValue);

        RuleFor(x => x.ProductId)
            .MaximumLength(255)
            .WithMessage("ProductId cannot exceed 255 characters");

        RuleFor(x => x.SellerId)
            .MaximumLength(255)
            .WithMessage("SellerId cannot exceed 255 characters");

        RuleFor(x => x.SellerName)
            .MaximumLength(255)
            .WithMessage("SellerName cannot exceed 255 characters");

        RuleFor(x => x.SellerUrl)
            .MaximumLength(1000)
            .WithMessage("SellerUrl cannot exceed 1000 characters")
            .Must(BeValidUrl)
            .WithMessage("SellerUrl must be a valid URL")
            .When(x => !string.IsNullOrWhiteSpace(x.SellerUrl));

        RuleFor(x => x.SellerLocation)
            .MaximumLength(255)
            .WithMessage("SellerLocation cannot exceed 255 characters");

        RuleFor(x => x.DetectedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1))
            .WithMessage("DetectedDate cannot be in the future")
            .When(x => x.DetectedDate.HasValue);
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
