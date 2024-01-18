using TerrificBookstore.API.Books;
using TerrificBookstore.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IBookClient, BookClient>(client =>
{
    client.BaseAddress = new Uri("https://gutendex.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddDbContext<BookstoreContext>(options => options.UseNpgsql("Host=localhost;Database=bookstore;Username=postgres;Password=postgres"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }