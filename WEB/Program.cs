using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WEB.Data;
using WEB.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connectionstring");

builder.Services.AddDbContext<DataContextDB>( opt =>
{
    opt.UseSqlServer(connectionString);
});

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContextDB>();
    await db.Database.EnsureCreatedAsync();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new {ping = "pong"}));

app.MapGet("/getAllProducts", async (DataContextDB db) => 
{
    var all = await db.Products.ToListAsync();

    if (all.Count > 0) 
    {
        return Results.Ok(all);
    }
    return Results.NotFound("Not products");
});

app.MapGet("/getAllAdmins", async (DataContextDB db) =>
{
    var all = await db.Admins.ToListAsync();

    if (all.Count > 0)
    {
        return Results.Ok(all);
    }
    return Results.NotFound("Not admins");
});


app.MapGet("/auth", async (DataContextDB db, string Email, string Password) =>
{
    if (string.IsNullOrEmpty(Email)) throw new ArgumentException("Email is empty");
    if (string.IsNullOrEmpty(Password)) throw new ArgumentException("Password is empty");

    var admin =  await db.Admins
                            .Where(a => a.Email == Email && a.Password == Password)
                            .ToListAsync();

    if (admin.Count <= 0)
    {
        return Results.NotFound("Ошибка входа");
    }

    return Results.Ok(new { accepted = "true"});
});

app.MapGet("/categories/{nameCategory}", async (string name, DataContextDB db) =>
{
    var category = await db.Categories
        .Include(c => c.Products)
        .FirstOrDefaultAsync(c => c.Name == name);

    if (category == null)
        return Results.NotFound(new { message = "Категория не найдена" });

    return Results.Ok(category);
});

app.Run();