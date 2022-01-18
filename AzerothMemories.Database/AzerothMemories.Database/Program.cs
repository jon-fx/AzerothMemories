// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

//builder.Services.AddFluentMigratorCore()
//    .ConfigureRunner(rb => rb
//        .AddPostgres()
//        .WithGlobalConnectionString(config.DatabaseConnectionString)
//        .ScanIn(typeof(Migration0001).Assembly).For.Migrations());

//var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
//    //runner.MigrateDown(0);
//    runner.MigrateUp();
//}