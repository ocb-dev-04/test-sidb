using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using SB.Solicitudes.Aplicacion.Abstractions.Persistence;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.AddComment;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.ChangeServiceRequestStatus;
using SB.Solicitudes.Aplicacion.Features.ServiceRequests.CreateServiceRequest;
using SB.Solicitudes.Dominio.Catalogs;
using SB.Solicitudes.Dominio.Common;
using SB.Solicitudes.Dominio.ServiceRequests;
using SB.Solicitudes.Dominio.Users;

namespace SB.Solicitudes.Aplicacion.Tests.Features.ServiceRequests;

public sealed class ServiceRequestCommandHandlerTests
{
    private const string AREA_NAME = "Tecnología";
    private const string REQUEST_TYPE_NAME = "Soporte técnico";
    private const string REQUEST_TYPE_DESCRIPTION = "Incidencias de equipos y sistemas";

    private readonly IServiceRequestRepository _serviceRequestRepository = Substitute.For<IServiceRequestRepository>();
    private readonly IAreaRepository _areaRepository = Substitute.For<IAreaRepository>();
    private readonly IRequestTypeRepository _requestTypeRepository = Substitute.For<IRequestTypeRepository>();
    private readonly IServiceRequestCodeGenerator _codeGenerator = Substitute.For<IServiceRequestCodeGenerator>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateShouldPersistRequestWithGeneratedCodeWhenCatalogsAreActive()
    {
        Area area = Area.Create(AREA_NAME);
        RequestType requestType = RequestType.Create(REQUEST_TYPE_NAME, REQUEST_TYPE_DESCRIPTION);
        _areaRepository.GetByIdAsync(TestData.AREA_ID, Arg.Any<CancellationToken>()).Returns(area);
        _requestTypeRepository.GetByIdAsync(TestData.REQUEST_TYPE_ID, Arg.Any<CancellationToken>()).Returns(requestType);
        _codeGenerator.GenerateAsync(TestData.UTC_NOW.Year, Arg.Any<CancellationToken>()).Returns(TestData.CODE);
        CreateServiceRequestCommandHandler handler = CreateHandler(UserRole.Requester, TestData.REQUESTER_ID);

        Result<ServiceRequestCreatedResponse> result = await handler.HandleAsync(ValidCreateCommand(), CancellationToken.None);

        result.Value.Code.ShouldBe(TestData.CODE);
        _serviceRequestRepository.Received(1).Add(Arg.Is<ServiceRequest>(serviceRequest =>
            serviceRequest.Code == TestData.CODE && serviceRequest.RequesterId == TestData.REQUESTER_ID));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateShouldFailWhenAreaIsInactive()
    {
        Area area = Area.Create(AREA_NAME);
        area.Update(AREA_NAME, isActive: false);
        _areaRepository.GetByIdAsync(TestData.AREA_ID, Arg.Any<CancellationToken>()).Returns(area);
        CreateServiceRequestCommandHandler handler = CreateHandler(UserRole.Requester, TestData.REQUESTER_ID);

        Result<ServiceRequestCreatedResponse> result = await handler.HandleAsync(ValidCreateCommand(), CancellationToken.None);

        result.Error.ShouldBe(CatalogErrors.AREA_INACTIVE);
        await _codeGenerator.DidNotReceiveWithAnyArgs().GenerateAsync(default, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task ChangeStatusShouldRejectRequesters()
    {
        ChangeServiceRequestStatusCommandHandler handler = CreateChangeStatusHandler(UserRole.Requester, TestData.REQUESTER_ID);

        Result<Unit> result = await handler.HandleAsync(
            new ChangeServiceRequestStatusCommand(Guid.NewGuid(), ServiceRequestStatus.InAnalysis, null),
            CancellationToken.None);

        result.Error.ShouldBe(ServiceRequestErrors.MANAGEMENT_REQUIRED);
    }

    [Fact]
    public async Task ChangeStatusShouldHideRequestsAssignedToAnotherAnalyst()
    {
        ServiceRequest serviceRequest = TestData.NewServiceRequest();
        serviceRequest.Assign(TestData.OTHER_ANALYST_ID, TestData.ADMINISTRATOR_ID, TestData.UTC_NOW);
        _serviceRequestRepository.GetByIdAsync(serviceRequest.Id, Arg.Any<CancellationToken>()).Returns(serviceRequest);
        ChangeServiceRequestStatusCommandHandler handler = CreateChangeStatusHandler(UserRole.Analyst, TestData.ANALYST_ID);

        Result<Unit> result = await handler.HandleAsync(
            new ChangeServiceRequestStatusCommand(serviceRequest.Id, ServiceRequestStatus.InAnalysis, null),
            CancellationToken.None);

        result.Error.ShouldBe(ServiceRequestErrors.NOT_FOUND);
        serviceRequest.Status.ShouldBe(ServiceRequestStatus.Registered);
    }

    [Fact]
    public async Task ChangeStatusShouldSaveWhenAnalystManagesUnassignedRequest()
    {
        ServiceRequest serviceRequest = TestData.NewServiceRequest();
        _serviceRequestRepository.GetByIdAsync(serviceRequest.Id, Arg.Any<CancellationToken>()).Returns(serviceRequest);
        ChangeServiceRequestStatusCommandHandler handler = CreateChangeStatusHandler(UserRole.Analyst, TestData.ANALYST_ID);

        Result<Unit> result = await handler.HandleAsync(
            new ChangeServiceRequestStatusCommand(serviceRequest.Id, ServiceRequestStatus.InAnalysis, null),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        serviceRequest.Status.ShouldBe(ServiceRequestStatus.InAnalysis);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCommentShouldRejectInternalCommentsFromRequesters()
    {
        ServiceRequest serviceRequest = TestData.NewServiceRequest();
        _serviceRequestRepository.GetByIdAsync(serviceRequest.Id, Arg.Any<CancellationToken>()).Returns(serviceRequest);
        AddServiceRequestCommentCommandHandler handler = new(
            _serviceRequestRepository,
            _userRepository,
            _unitOfWork,
            new TestCurrentUser(TestData.REQUESTER_ID, UserRole.Requester),
            TestData.Clock);

        Result<CommentResponse> result = await handler.HandleAsync(
            new AddServiceRequestCommentCommand(serviceRequest.Id, TestData.COMMENT, CommentVisibility.Internal),
            CancellationToken.None);

        result.Error.ShouldBe(ServiceRequestErrors.INTERNAL_COMMENT_NOT_ALLOWED);
        serviceRequest.Comments.ShouldBeEmpty();
    }

    private static CreateServiceRequestCommand ValidCreateCommand()
        => new(TestData.TITLE, TestData.DESCRIPTION, ServiceRequestPriority.High, TestData.REQUEST_TYPE_ID, TestData.AREA_ID, null);

    private CreateServiceRequestCommandHandler CreateHandler(UserRole role, Guid userId)
        => new(
            _serviceRequestRepository,
            _areaRepository,
            _requestTypeRepository,
            _codeGenerator,
            _unitOfWork,
            new TestCurrentUser(userId, role),
            TestData.Clock,
            NullLogger<CreateServiceRequestCommandHandler>.Instance);

    private ChangeServiceRequestStatusCommandHandler CreateChangeStatusHandler(UserRole role, Guid userId)
        => new(
            _serviceRequestRepository,
            _unitOfWork,
            new TestCurrentUser(userId, role),
            TestData.Clock,
            NullLogger<ChangeServiceRequestStatusCommandHandler>.Instance);
}
