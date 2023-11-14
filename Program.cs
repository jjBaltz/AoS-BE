using AoS.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000",
                                "http://localhost:7271")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<AoSDbContext>(builder.Configuration["AoSDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

//Add for Cors 
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//USER API CALLS
app.MapGet("/users", (AoSDbContext db) =>
{
    return db.Users.ToList();
});

app.MapGet("/users/{id}", (AoSDbContext db, int id) =>
{
    return db.Users.Single(user => user.UserId == id);
});

app.MapPost("/users", (AoSDbContext db, User user) =>
{
    try
    {
        db.Users.Add(user);
        db.SaveChanges();
        return Results.Created($"/users/{user.UserId}", user);
    }
    catch (DbUpdateException)
    {
        return Results.NotFound();
    }
});

app.MapPut("/users/{id}", (AoSDbContext db, int id, User user) =>
{
    User userToUpdate = db.Users.SingleOrDefault(user => user.UserId == id);
    if (userToUpdate == null)
    {
        return Results.NotFound();
    }
    userToUpdate.FirstName = user.FirstName;
    userToUpdate.LastName = user.LastName;
    userToUpdate.Email = user.Email;
    userToUpdate.ImageUrl = user.ImageUrl;

    db.Update(userToUpdate);
    db.SaveChanges();
    return Results.Ok(userToUpdate);
});

app.MapGet("/checkuser/{uid}", (AoSDbContext db, string uid) =>
{
    var userExist = db.Users.Where(user => user.UID == uid).FirstOrDefault();
    if (userExist == null)
    {
        return Results.BadRequest("User is not a member");
    }
    return Results.Ok(userExist);
});

//MEMORY API CALLS
app.MapGet("/memories", (AoSDbContext db) =>
{
    return db.Memories.ToList();
});

app.MapGet("/memories/{id}", (AoSDbContext db, int id) =>
{
    return db.Memories.Single(memory => memory.MemoryId == id);
});


app.UseHttpsRedirection();

app.Run();
