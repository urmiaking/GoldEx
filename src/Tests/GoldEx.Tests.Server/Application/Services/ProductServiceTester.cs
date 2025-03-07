using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Validators;
using GoldEx.Shared.Enums;

namespace GoldEx.Tests.Server.Application.Services;

//[TestFixture]
//public class ProductServiceTester
//{
//    private CreateProductValidator<Product> _createProductValidator;
//    private DeleteProductValidator<Product> _deleteProductValidator;

//    [SetUp]
//    public void SetUp()
//    {
//        _createProductValidator = new CreateProductValidator<Product>();
//        _deleteProductValidator = new DeleteProductValidator<Product>();
//    }

//    [Test]
//    public void CreateMoltenGoldProduct_ValidateProduct_ReturnsWithoutError()
//    {
//        Arrange
//       var validProduct = new Product("test", "12345453", 1.4, null, ProductType.MoltenGold, null, CaratType.Eighteen,
//           Guid.NewGuid());
//        var invalidProduct = new Product("test2", "12345453", 1.4, 1, ProductType.MoltenGold, WageType.Dollar,
//            CaratType.Eighteen, Guid.NewGuid());
//        Act
//       var validProductValidation = _createProductValidator.Validate(validProduct);
//        var invalidProductValidation = _createProductValidator.Validate(invalidProduct);
//        Assert
//        Assert.That(validProductValidation.IsValid);
//        Assert.That(!invalidProductValidation.IsValid);
//        Assert.That(invalidProductValidation.Errors.Count == 2);
//    }

//    [Test]
//    public void CreateUsedGoldProduct_ValidateProduct_ReturnWithoutError()
//    {
//        Arrange
//       var validProduct = new Product("test", "12345453", 1.4, null, ProductType.UsedGold, null, CaratType.Eighteen,
//           Guid.NewGuid());
//        var invalidProduct = new Product("test2", "12345453", 1.4, 1, ProductType.UsedGold, WageType.Dollar,
//            CaratType.Eighteen, Guid.NewGuid());
//        Act
//       var validProductValidation = _createProductValidator.Validate(validProduct);
//        var invalidProductValidation = _createProductValidator.Validate(invalidProduct);
//        Assert
//        Assert.That(validProductValidation.IsValid);
//        Assert.That(!invalidProductValidation.IsValid);
//        Assert.That(invalidProductValidation.Errors.Count == 2);
//    }

//    [Test]
//    public void CreateCoinProduct_ValidateProduct_ReturnWithoutError()
//    {
//        Arrange
//       var validProduct = new Product("test", "12345453", 1.4, null, ProductType.Coin, null, CaratType.Eighteen,
//           Guid.NewGuid());
//        var invalidProduct = new Product("test2", "12345453", 1.4, 1, ProductType.Coin, WageType.Dollar,
//            CaratType.Eighteen, Guid.NewGuid());
//        Act
//       var validProductValidation = _createProductValidator.Validate(validProduct);
//        var invalidProductValidation = _createProductValidator.Validate(invalidProduct);
//        Assert
//        Assert.That(validProductValidation.IsValid);
//        Assert.That(!invalidProductValidation.IsValid);
//        Assert.That(invalidProductValidation.Errors.Count == 2);
//    }

//    [Test]
//    public void CreateJewelryProduct_ValidateProduct_ReturnWithoutError()
//    {
//        Arrange
//       var validProduct = new Product("test", "12345453", 1.4, 5, ProductType.Jewelry, WageType.Dollar,
//           CaratType.Eighteen, Guid.NewGuid());
//        var invalidProduct = new Product("test2", "12345453", 1.4, null, ProductType.Jewelry, WageType.Percent,
//            CaratType.Eighteen, Guid.NewGuid());
//        Act
//       var validProductValidation = _createProductValidator.Validate(validProduct);
//        var invalidProductValidation = _createProductValidator.Validate(invalidProduct);
//        Assert
//        Assert.That(validProductValidation.IsValid);
//        Assert.That(!invalidProductValidation.IsValid);
//        Assert.That(invalidProductValidation.Errors.Count == 2);
//    }

//    [Test]
//    public void CreateGoldProduct_ValidateProduct_ReturnWithoutError()
//    {
//        Arrange
//       var validProduct = new Product("test", "12345453", 1.4, 5, ProductType.Gold, WageType.Percent,
//           CaratType.Eighteen, Guid.NewGuid());
//        var invalidProduct = new Product("test2", "12345453", 1.4, null, ProductType.Gold, WageType.Dollar,
//            CaratType.Eighteen, Guid.NewGuid());
//        Act
//       var validProductValidation = _createProductValidator.Validate(validProduct);
//        var invalidProductValidation = _createProductValidator.Validate(invalidProduct);
//        Assert
//        Assert.That(validProductValidation.IsValid);
//        Assert.That(!invalidProductValidation.IsValid);
//        Assert.That(invalidProductValidation.Errors.Count == 2);
//    }
//}