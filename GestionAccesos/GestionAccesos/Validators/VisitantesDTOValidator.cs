using FluentValidation;
using GestionAccesos.DTO;
using GestionAccesos.Services;

namespace GestionAccesos.Validators;

public class VisitantesDTOValidator : AbstractValidator<VisitanteDTO>
{
    public VisitantesDTOValidator(TranslationService translation)
    {
        RuleFor(x => x.PrimerApellido)
            .NotEmpty().WithMessage(translation["Error_PrimerApellido_Obligatorio"]);

        RuleFor(x => x.Telefono)
            .NotNull().WithMessage(translation["Error_NumeroDeTelefono_Obligatorio"])
            .InclusiveBetween(100000000, 999999999)
            .WithMessage(translation["Error_NumeroDeTelefono_Formato"]);

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage(translation["Error_Correo_Obligatorio"])
            .EmailAddress()
            .WithMessage(translation["Error_Correo_Formato"]);
    }
}