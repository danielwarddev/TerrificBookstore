using System.Net.Http.Json;
using System.Net;
using TerrificBookstore.API.Books;
using AutoFixture;
using NSubstitute;
using RichardSzalay.MockHttp;
using FluentAssertions;

namespace TerrificBookstore.API.UnitTests.Books;

public class BookClientTests
{
    private readonly BookClient _bookClient;
    private readonly Fixture _fixture = new();
    private readonly string _baseAddress = "https://www.example.com";
    private readonly HttpMessageHandler _nSubHandler = Substitute.For<HttpMessageHandler>();
    private readonly MockHttpMessageHandler _mockHandler = new();

    public BookClientTests()
    {
        // Using NSubstitute with HttpMessageHandler
        //var httpClient = new HttpClient(_nSubHandler)
        //{
        //    BaseAddress = new Uri(_baseAddress)
        //};
        //_bookClient = new BookClient(httpClient);

        // Using RichardSzalay.MockHttp
        var httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        _bookClient = new BookClient(httpClient);
    }

    [Fact]
    public async Task When_Getting_A_Book_Returns_Successful_Response_Then_Returns_The_Book()
    {
        // 1. Arrange
        var expectedBook = _fixture.Create<Book>();

        var handler = new MyMockHttpMessageHandler(HttpStatusCode.OK, expectedBook);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseAddress)
        };
        var bookClient = new BookClient(httpClient);

        // 2. Act
        var actualBook = await bookClient.GetBook(expectedBook.Id);

        // 3. Assert
        expectedBook.Should().BeEquivalentTo(actualBook);
        //Assert.Equivalent(expectedBook, actualBook);
    }

    [Fact]
    public async Task When_Getting_A_Book_Returns_404_Then_Returns_Null()
    {
        var bookId = _fixture.Create<int>();

        _nSubHandler
            .SetupRequest(HttpMethod.Get, $"{_baseAddress}/books/{bookId}")
            .ReturnsResponse(HttpStatusCode.NotFound);

        var result = await _bookClient.GetBook(bookId);

        var test = _nSubHandler.ReceivedCalls();
        _nSubHandler.ShouldHaveReceived(HttpMethod.Get, $"{_baseAddress}/books/{bookId}");
        result.Should().BeNull();
    }

    [Fact]
    public async Task Calls_Endpoint_With_Date_Range_From_Given_Month()
    {
        var month = GetRandomMonth();
        var dates = month.GetDateRange();
        var expectedBooks = _fixture.CreateMany<Book>().ToList();

        _mockHandler
            .Expect(HttpMethod.Get, $"{_baseAddress}/books?beginDate={dates.Startdate}&endDate={dates.EndDate}")
            .Respond(HttpStatusCode.OK, JsonContent.Create(expectedBooks));

        var actualBooks = await _bookClient.GetBooksAddedInMonth(month);

        actualBooks.Should().BeEquivalentTo(expectedBooks);
        // Assert.Equal(expectedBooks, actualBooks); // Doesn't work!
    }

    [Theory, CombinatorialData]
    public async Task When_Extension_And_Size_Are_Valid_Then_Uploads_Photo(
        [CombinatorialValues(".jpg", ".jpeg", ".png")] string fileExtension,
        [CombinatorialValues(1000, 3000, 10000)] int fileSize)
    {
        var fileName = $"myFile{fileExtension}";
        var fileData = new byte[fileSize];

        _mockHandler
            .Expect(HttpMethod.Post, $"{_baseAddress}/photos")
            .Respond(HttpStatusCode.OK);

        await _bookClient.UploadBookPhoto(fileName, fileData);
    }

    [Theory, CombinatorialData]
    public async Task When_Extension_Is_Invalid_Then_Throws_Exception(
        [CombinatorialValues(".gif", ".txt", ".bmp")] string fileExtension,
        [CombinatorialValues(1000, 3000, 10000)] int fileSize)
    {
        var fileName = $"myFile{fileExtension}";
        var fileData = new byte[fileSize];

        var uploadPhoto = async () => await _bookClient.UploadBookPhoto(fileName, fileData);
        await uploadPhoto.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid file extension");
    }

    [Theory, CombinatorialData]
    public async Task When_Size_Is_Over_10kb_Then_Throws_Exception(
        [CombinatorialValues(".jpg", ".jpeg", ".png")] string fileExtension,
        [CombinatorialValues(10001, 20000, 100000)] int fileSize)
    {
        var fileName = $"myFile{fileExtension}";
        var fileData = new byte[fileSize];

        var uploadPhoto = async () => await _bookClient.UploadBookPhoto(fileName, fileData);
        await uploadPhoto.Should().ThrowAsync<ArgumentException>().WithMessage("File size must be under 10kb");
    }

    private Month GetRandomMonth()
    {
        var random = new Random();
        var allMonths = Enum.GetValues(typeof(Month));
        var randomIndex = random.Next(allMonths.Length);

        return (Month)allMonths.GetValue(randomIndex)!;
    }
}