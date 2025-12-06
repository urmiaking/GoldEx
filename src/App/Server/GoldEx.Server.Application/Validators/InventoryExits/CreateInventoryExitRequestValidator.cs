using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.InventoryExits;

namespace GoldEx.Server.Application.Validators.InventoryExits;

[ScopedService]
internal class CreateInventoryExitRequestValidator : AbstractValidator<CreateInventoryExitRequest>
{
    
}