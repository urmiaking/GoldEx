using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class BarcodeReservationServiceTester
{
    private Mock<IBarcodeReservationRepository> _reservationRepositoryMock = null!;
    private Mock<IBarcodeGeneratorService> _generatorMock = null!;
    private BarcodeReservationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _reservationRepositoryMock = new Mock<IBarcodeReservationRepository>();
        _generatorMock = new Mock<IBarcodeGeneratorService>();
        _service = new BarcodeReservationService(
            _reservationRepositoryMock.Object,
            _generatorMock.Object
        );
    }

    [Test]
    public async Task IssueNextAsync_WhenNoConflict_ReturnsBarcode()
    {
        // Arrange
        var request = new IssueNextBarcodeRequest(ProductType.Gold, null, null);
        _generatorMock.Setup(g => g.BuildFullPrefixAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA");
        _generatorMock.Setup(g => g.GenerateNextAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA001");
        _reservationRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.IssueNextAsync(request);

        // Assert
        Assert.That(result.Prefix, Is.EqualTo("GAA"));
        Assert.That(result.Barcode, Is.EqualTo("GAA001"));
        _reservationRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IssueNextAsync_WhenUniqueConstraintConflict_RetriesAndSucceeds()
    {
        // Arrange
        var request = new IssueNextBarcodeRequest(ProductType.Gold, null, null);
        var callCount = 0;

        _generatorMock.Setup(g => g.BuildFullPrefixAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA");
        _generatorMock.Setup(g => g.GenerateNextAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return $"GAA{callCount:D3}"; // Returns GAA001, GAA002, etc.
            });

        // Simulate unique constraint violation with inner exception (common pattern in EF Core)
        var innerException = new Exception("UNIQUE constraint failed");
        _reservationRepositoryMock.SetupSequence(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Error saving changes", innerException))
            .ThrowsAsync(new DbUpdateException("Error saving changes", innerException))
            .Returns(Task.CompletedTask); // Third attempt succeeds

        // Act
        var result = await _service.IssueNextAsync(request);

        // Assert
        Assert.That(result.Prefix, Is.EqualTo("GAA"));
        Assert.That(result.Barcode, Is.EqualTo("GAA003")); // Third barcode
        _reservationRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Test]
    public void IssueNextAsync_WhenAllRetriesFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new IssueNextBarcodeRequest(ProductType.Gold, null, null);
        var innerException = new Exception("UNIQUE constraint failed");

        _generatorMock.Setup(g => g.BuildFullPrefixAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA");
        _generatorMock.Setup(g => g.GenerateNextAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA001");
        _reservationRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Error saving changes", innerException));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.IssueNextAsync(request));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.InnerException, Is.InstanceOf<DbUpdateException>());
        _reservationRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
    }

    [Test]
    public void IssueNextAsync_WhenNonUniqueConstraintError_ThrowsDbUpdateException()
    {
        // Arrange
        var request = new IssueNextBarcodeRequest(ProductType.Gold, null, null);
        // Simulate a different database error (not unique constraint)
        var innerException = new Exception("Connection timeout");

        _generatorMock.Setup(g => g.BuildFullPrefixAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA");
        _generatorMock.Setup(g => g.GenerateNextAsync(ProductType.Gold, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync("GAA001");
        _reservationRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Error saving changes", innerException));

        // Act & Assert - Should throw DbUpdateException, not retry
        var ex = Assert.ThrowsAsync<DbUpdateException>(async () => await _service.IssueNextAsync(request));
        Assert.That(ex, Is.Not.Null);
        // Should only try once since it's not a unique constraint violation
        _reservationRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BarcodeReservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
