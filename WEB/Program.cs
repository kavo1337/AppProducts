using Microsoft.EntityFrameworkCore;
using WEB.Data;
using WEB.Models;
using static WEB.DTO.ALLDTO;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connectionstring");

builder.Services.AddDbContext<DataContextDB>( opt =>
{
    opt.UseSqlServer(connectionString);
});

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContextDB>();
    await db.Database.EnsureCreatedAsync();
}


app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Ping", () => Results.Ok(new {ping = "pong"}));

app.MapGet("/GetAllProducts", async (DataContextDB db) => 
{
    var all = await db.Products.ToListAsync();

    if (all.Count > 0) 
    {
        return Results.Ok(all);
    }
    return Results.NotFound("Not products");
});

app.MapGet("/GetAllCategory", async (DataContextDB db) =>
{
    var all = await db.Categories.ToListAsync();

    if (all.Count > 0)
    {
        return Results.Ok(all);
    }
    return Results.NotFound("Not category");
});

app.MapGet("/GetAllAdmins", async (DataContextDB db) =>
{
    var all = await db.Admins.ToListAsync();

    if (all.Count > 0)
    {
        return Results.Ok(all);
    }
    return Results.NotFound("Not admins");
});


app.MapPost("/Auth", async (DataContextDB db, AuthRequestDTO request) =>
{
    if (string.IsNullOrEmpty(request.Email)) throw new ArgumentException("Email is empty");
    if (string.IsNullOrEmpty(request.Password)) throw new ArgumentException("Password is empty");

    var admin = await db.Admins
                        .Where(a => a.Email == request.Email && a.Password == request.Password)
                        .FirstOrDefaultAsync();

    if (admin is null)
    {
        return Results.NotFound(new { accepted = false, message = "Неверный логин или пароль" });
    }

    return Results.Ok(new { accepted = true, message = "Успешная аутентификация" });
});

app.MapGet("/Categorie/{nameCategory}", async (string name, DataContextDB db) =>
{
    var category = await db.Categories
        .Include(c => c.Products)
        .FirstOrDefaultAsync(c => c.Name == name);

    if (category == null)
        return Results.NotFound(new { message = "Не удалось найти категорию" });

    return Results.Ok(category);
});

app.MapPost("/AddProduct", async (DataContextDB db, Product input) =>
{

    if (input == null)
        return Results.BadRequest(new { error = "Данные продукта пустые" });

    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { error = "Имя пустое" });

    if (string.IsNullOrWhiteSpace(input.Description))
        return Results.BadRequest(new { error = "Описание пустое" });

    if (string.IsNullOrWhiteSpace(input.CategoryName))
        return Results.BadRequest(new { error = "Название категории пустое" });

    var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == input.CategoryName);
    if (category == null)
    {
        category = new Category { Name = input.CategoryName };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
    }

    var entity = new Product
    {
        Name = input.Name,
        Description = input.Description,
        CategoryName = input.CategoryName,
        Category = category 
    };

    db.Products.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/AddProducts/{entity.Id}", entity);

});

app.MapDelete("/DeleteProduct/{id}", async (DataContextDB db, int id) =>
{
    var product = await db.Products.FindAsync(id);
    if (product == null)
        return Results.NotFound(new { error = "Продукт не найден" });

    db.Products.Remove(product);
    await db.SaveChangesAsync();

    return Results.Ok("Продукт удален");
});

app.MapPost("/EditProduct/{id}", (DataContextDB db, int id, EditProductDTO dto) =>
{
    var product = db.Products.FirstOrDefault(a => a.Id == id);
    if (product == null) 
        return Results.NotFound(new { error = "Продукт не найден" });

    product.Name = dto.Name;
    product.Description = dto.Description;
    product.CategoryName = dto.CategoryName;

    db.SaveChanges();

    return Results.Ok("Продукт изменен");
});


app.Run();