using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using JobTrackerService.Jobs.Application;
using JobTrackerService.Jobs.Domain;
using JobTrackerService.Jobs.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Jobs")
	?? "Host=localhost;Database=job_tracker;Username=postgres;Password=postgres";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
	?? ["http://localhost:3000", "http://127.0.0.1:3000"];

builder.Services.AddMediatR(configuration =>
	configuration.RegisterServicesFromAssemblyContaining<CreateJobCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateJobCommand>();
builder.Services.AddSingleton<InsertOutboxMessagesInterceptor>();
builder.Services.AddDbContext<JobsDbContext>((provider, options) => options
	.UseNpgsql(connectionString, npgsql =>
	{
		npgsql.EnableRetryOnFailure();
		npgsql.MigrationsAssembly("JobTrackerService.Migrations");
	})
	.UseSnakeCaseNamingConvention()
	.AddInterceptors(provider.GetRequiredService<InsertOutboxMessagesInterceptor>()));
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<JobsDbContext>());
builder.Services.AddScoped<OutboxDispatcher>();
builder.Services.AddHangfire(configuration => configuration.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings().UseStorage(new MemoryStorage()));
builder.Services.AddHangfireServer();
builder.Services.AddCors(options =>
	{
		options.AddPolicy("FrontendPolicy", policy =>
		{
			policy.WithOrigins(allowedOrigins)
				.AllowAnyHeader()
				.AllowAnyMethod();
		});
	});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("FrontendPolicy");
app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<OutboxPollingJob>("jobs-outbox", job => job.RunAsync(), "*/1 * * * *");
app.MapControllers();
app.Run();
