var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter("username", secret: true);
var dbPassword = builder.AddParameter("password", secret: true);
var databaseName = "OrderDb";
var creationScript = $$"""
    -- Create the database
    CREATE DATABASE {{databaseName}};

    """;

var postgres = builder.AddPostgres("postgres", dbUsername, dbPassword)

                      .WithPgWeb(pgWeb => pgWeb.WithHostPort(5050));

var postgresdb = postgres.AddDatabase("postgresdb").WithCreationScript(creationScript);

builder.AddProject<Projects.Api>("order-management-api").WithReference(postgresdb).WaitFor(postgresdb);

builder.Build().Run();
