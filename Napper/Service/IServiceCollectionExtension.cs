namespace Napper.Service
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddNapperService(this IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetService<IConfiguration>();
            string? connectionString = null;
            string? dbTypeName = null;
            connectionString = config?.GetConnectionString("Default");
            dbTypeName = config?.GetConnectionString("DbTypeName");

            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (dbTypeName == null) throw new ArgumentNullException(nameof(dbTypeName));

            services.Configure<NapperOption>(options =>
            {
                options.DbTypeName = dbTypeName;
                options.ConnectionString = connectionString;
            }).AddScoped<NapperService>();

            return services;
        }
    }
}
