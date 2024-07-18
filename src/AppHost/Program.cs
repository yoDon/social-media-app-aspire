var builder = DistributedApplication.CreateBuilder(args);

// Add a parameter
var pAdmin = builder.AddParameter("postgres-admin");
var admin = builder.AddParameter("admin");
var password = builder.AddParameter("admin-password", secret: true);

var redis = builder.AddRedis("cache");
var messageBus = builder
    .AddRabbitMQ("messaging", admin, password, port: 5672)
    .WithDataVolume()
    .WithManagementPlugin();

var usersDb = builder
    .AddPostgres("dbserver", pAdmin, password)
    //.WithDataVolume()
    .WithDataBindMount("../../volumes/PostgresUsersDb")
    .WithPgAdmin(c => c.WithHostPort(5050))
    .AddDatabase("users-db");

var postsDb = builder
    .AddMongoDB("posts-mongodb")
    //.WithDataVolume()
    .WithDataBindMount("../../volumes/MongoDbPostsDb")
    .WithMongoExpress(c => c.WithHostPort(8081))
    .AddDatabase("posts-db");

var elastic = builder
    .AddElasticsearch("elasticsearch", password, port: 9200)
    //.WithDataVolume()
    .WithDataBindMount("../../volumes/Elasticsearch");

var api = builder
    .AddProject<Projects.Api>("api")
    .WithReference(usersDb)
    .WithReference(postsDb)
    .WithReference(elastic)
    .WithReference(redis)
    .WithReference(messageBus);

var migrator = builder
    .AddProject<Projects.MigrationService>("migrator")
    .WithReference(postsDb)
    .WithReference(elastic)
    .WithReference(usersDb);

builder.Build().Run();
