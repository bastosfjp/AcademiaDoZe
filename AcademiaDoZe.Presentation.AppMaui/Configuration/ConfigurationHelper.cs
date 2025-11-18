using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration
{
    public static class ConfigurationHelper
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // dados conexão
            const string dbServer = "127.0.0.1,1433";   // IP e porta do container SQL Server
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "sa";                // usuário padrão do SQL Server
            const string dbPassword = "abcBolinhas12345";

            // Connection string para SQL Server
            const string connectionString =
                $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;Encrypt=False;";

            const EAppDatabaseType databaseType = EAppDatabaseType.SqlServer; // aqui sim SQL Server

            // Configura a fábrica de repositórios com a string de conexão e tipo de banco
            services.AddSingleton(new RepositoryConfig
            {
                ConnectionString = connectionString,
                DatabaseType = databaseType
            });

            // configura os serviços da camada de aplicação
            services.AddApplicationServices();
        }
    }
}