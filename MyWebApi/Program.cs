using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApi.Contexts;
using MyWebApi.Entities;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("defaultConnection")
    )
);


//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c=> { c.SwaggerDoc("v1", new() { AssemblyTitleAttribute})});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

var message= builder.Configuration.GetValue<string>("Message");
app.MapGet("/CallMessage", () => message);




app.MapGet("/people", async (ApplicationDbContext context) =>
{
    var people = await context.People.ToListAsync();
    return people.Count()<1 ? Results.NotFound() : TypedResults.Ok(people);
}
);


app.MapGet("/people/{id}", async (int id,ApplicationDbContext context) =>
{
    var people = await context.People.FindAsync(id);
    if (people is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(people);
}
).WithName("GetPersonWithId");

app.MapPost("/people", async (Person person, ApplicationDbContext context) =>
{
    context.Add(person);
    await context.SaveChangesAsync();

    var _data = Results.CreatedAtRoute("GetPersonWithID", person, new { id = person.Id });
    return _data;
}
);

app.MapPut("/people", async (Person person, ApplicationDbContext context) =>
{
    var people = await context.People.AnyAsync(p => p.Id == person.Id);
    if (!people)
    {
        return Results.NotFound();
    }

    context.Update(person);
    await context.SaveChangesAsync();

    return  Results.Ok(await context.People.FindAsync(person.Id)); //Results.CreatedAtRoute("GetPersonWithID", person, new { id = person.Id });
}
);

app.MapDelete("/people/{id}", async (int id, ApplicationDbContext context) =>
{
    var people = await context.People.Where(p => p.Id == id).ExecuteDeleteAsync();
    if (people>0)
    {
        return Results.Ok(people + " Record(s) Deleted.");
    }

    return Results.NotFound();
}
);





// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


