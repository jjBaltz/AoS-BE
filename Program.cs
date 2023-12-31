using AoS.Models;
using AoS.DTOs;
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

app.MapGet("/users/uid/{id}", (AoSDbContext db, string id) =>
{
    return db.Users.Single(user => user.UID == id);
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

app.MapGet("/memories/user/{userId}", (AoSDbContext db, int UserId) =>
{
    var user = db.Users.FirstOrDefault(u => u.UserId == UserId);

    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    var memoriesByUser = db.Memories
        .Where(a => a.UserId == UserId)
        .ToList();

    return Results.Ok(memoriesByUser);
});

app.MapGet("/memories/{id}", (AoSDbContext db, int id) =>
{
    return db.Memories.Single(memory => memory.MemoryId == id);
});

app.MapPost("/memories/{activityId}", (AoSDbContext db, Memory memory, int activityId) =>
{
    Activity activity = db.Activities.Single(a => a.ActivityId == activityId);
    activity.IsUsed = true;
    db.Activities.Update(activity);
    memory.ActivityId = activityId;

    try
    {
        db.Memories.Add(memory);
        db.SaveChanges();
        return Results.Created($"/memories/{memory.MemoryId}", memory);
    }
    catch (DbUpdateException)
    {
        return Results.NotFound();
    }
});

//ACTIVITY API CALLS
app.MapGet("/activities", (AoSDbContext db) =>
{
    return db.Activities.ToList();
});

app.MapGet("/activities/users/{userId}", (AoSDbContext db, int UserId) =>
{
    var user = db.Users.FirstOrDefault(u => u.UserId == UserId);

    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    var activitiesByUser = db.Activities
        .Where(a => a.UserId == UserId)
        .ToList();

    return Results.Ok(activitiesByUser);
});

app.MapGet("/activities/{id}", (AoSDbContext db, int id) =>
{
    return db.Activities.Single(activity => activity.ActivityId == id);
});

app.MapPost("/activities", (AoSDbContext db, CreateActivityDTO activity) =>
{
    User user = db.Users.Single(u => u.UID == activity.UID);
    Activity newActivity = new Activity
    {
        Description = activity.Description,
        IsUsed = false,
        UserId = user.UserId,
        Tags = new(),
    };

    try
    {
        foreach(var Id in activity.TagIds)
        {
            var tag = db.Tags.Find(Id);
            newActivity.Tags.Add(tag);
        }
        db.Activities.Add(newActivity);
        db.SaveChanges();
        return Results.Created($"/activities/{newActivity.ActivityId}", newActivity);
    }
    catch (DbUpdateException)
    {
        return Results.NotFound();
    }
});

app.MapPost("/activities/five", (AoSDbContext db, SubmitActivitiesDTO activities) =>
{
    List<string> descriptions = new List<string>();
    descriptions.Add(activities.Description1);
    descriptions.Add(activities.Description2);
    descriptions.Add(activities.Description3);
    descriptions.Add(activities.Description4);
    descriptions.Add(activities.Description5);
    User user = db.Users.Single(u => u.UID == activities.UID);

    try
    {
        foreach (string description in descriptions)
        {
            Activity newActivity = new Activity
            {
                Description = description,
                IsUsed = false,
                UserId = user.UserId,
            };
            db.Activities.Add(newActivity);
        }
        db.SaveChanges();
        return Results.Ok("created");
    }
    catch (DbUpdateException)
    {
        return Results.NoContent();
    }
});

app.MapPut("/activities/{id}", (AoSDbContext db, int id, Activity activity) =>
{
    Activity activityToUpdate = db.Activities.SingleOrDefault(activity => activity.ActivityId == id);
    if (activityToUpdate == null)
    {
        return Results.NotFound();
    }
    activityToUpdate.Description = activity.Description;

    db.Update(activityToUpdate);
    db.SaveChanges();
    return Results.Ok(activityToUpdate);
});

app.MapPost("/activities/{activityId}/memories/{memoryId}", (AoSDbContext db, int activityId, int memoryId) =>
{
    var activity = db.Activities.Include(a => a.Memories)
                                .FirstOrDefault(m => m.ActivityId == activityId);
    if (activity == null)
    {
        return Results.NotFound("Activity Not Found");
    }

    var memoryToAdd = db.Memories?.Find(memoryId);

    if (memoryToAdd == null)
    {
        return Results.NotFound("Memory not found");
    }

    activity?.Memories?.Add(memoryToAdd);
    activity.IsUsed = true;
    db.Activities.Update(activity);
    db.SaveChanges();
    return Results.Ok(activity);
});

app.MapPost("/activities/{activityId}/tags/{tagId}", (AoSDbContext db, int activityId, int tagId) =>
{
    var activity = db.Activities.Include(a => a.Tags)
                                .FirstOrDefault(t => t.ActivityId == activityId);
    if (activity == null)
    {
        return Results.NotFound("Activity Not Found");
    }

    var tagToAdd = db.Tags?.Find(tagId);

    if (tagToAdd == null)
    {
        return Results.NotFound("Tag not found");
    }

    activity?.Tags?.Add(tagToAdd);
    db.SaveChanges();
    return Results.Ok(activity);
});

app.MapDelete("/activity/{activityId}/tags/{tagId}", (AoSDbContext db, int activityId, int tagId) =>
{
    var activity = db.Activities.Include(m => m.Tags)
                                .FirstOrDefault(t => t.ActivityId == activityId);
    if (activity == null)
    {
        return Results.NotFound("Activity not found");
    }

    var tagToDelete = db.Tags?.Find(tagId);


    if (tagToDelete == null)
    {
        return Results.NotFound("Tag not found");
    }

    activity?.Tags?.Remove(tagToDelete);
    db.SaveChanges();
    return Results.Ok(activity);
});

app.MapGet("/activities/open", (AoSDbContext db) =>
{
    return db.Activities.Where(activity => activity.IsUsed.Equals(false)).ToList();
});

//TAG API CALLS
app.MapGet("/tags", (AoSDbContext db) =>
{
    return db.Tags.ToList();
});

app.MapGet("/tags/{id}", (AoSDbContext db, int id) =>
{
    return db.Tags.SingleOrDefault(tag => tag.TagId == id);
});

//app.MapPost("/tags/{activityId}", (AoSDbContext db, Tag tag, int activityId) =>
//{
//    Activity activity = db.Activities.Single(a => a.ActivityId == activityId);
//    db.Activities.Update(activity);
//    tag.ActivityId = activityId;

//    try
//    {
//        db.Memories.Add(tag);
//        db.SaveChanges();
//        return Results.Created($"/tags/{tag.TagId}", tag);
//    }
//    catch (DbUpdateException)
//    {
//        return Results.NotFound();
//    }
//});

app.UseHttpsRedirection();

app.Run();
