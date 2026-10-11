using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Authentication;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Tests.Abstractions;

public sealed class MediatorPipelineTests
{
    private const string TITLE_PROPERTY = "title";
    private const string AREA_PROPERTY = "areaId";

    private readonly IServiceRequestCodeGenerator _codeGenerator = Substitute.For<IServiceRequestCodeGenerator>();
    private readonly IServiceRequestRepository _serviceRequestRepository = Substitute.For<IServiceRequestRepository>();

    [Fact]
    public async Task SendShouldReturnValidationErrorsWithoutInvokingHandlerWhenCommandIsInvalid()
    {
        using ServiceProvider provider = BuildProvider();
        using IServiceScope scope = provider.CreateScope();
        ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
        CreateServiceRequestCommand command = new(string.Empty, TestData.DESCRIPTION, ServiceRequestPriority.High, TestData.REQUEST_TYPE_ID, 0, null);

        Result<ServiceRequestCreatedResponse> result = await sender.Send(command);

        ValidationError validationError = result.Error.ShouldBeOfType<ValidationError>();
        validationError.Errors.Keys.ShouldBe([TITLE_PROPERTY, AREA_PROPERTY], ignoreOrder: true);
        await _codeGenerator.DidNotReceiveWithAnyArgs().GenerateAsync(default, default);
        _serviceRequestRepository.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddApplication();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(TestData.Clock);
        services.AddSingleton<ICurrentUser>(new TestCurrentUser(TestData.REQUESTER_ID, UserRole.Requester));
        services.AddSingleton(_codeGenerator);
        services.AddSingleton(_serviceRequestRepository);
        services.AddSingleton(Substitute.For<IAreaRepository>());
        services.AddSingleton(Substitute.For<IRequestTypeRepository>());
        services.AddSingleton(Substitute.For<IUnitOfWork>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
}
