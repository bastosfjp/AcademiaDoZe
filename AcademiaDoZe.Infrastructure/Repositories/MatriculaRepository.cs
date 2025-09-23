using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class MatriculaRepository : BaseRepository<Matricula>, IMatriculaRepository
    {
        public MatriculaRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }
        public override async Task<Matricula> Adicionar(Matricula entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();

                string query = _databaseType == DatabaseType.SqlServer

                ? $"INSERT INTO {TableName} (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, observacao_restricao, laudo ) "
                + "OUTPUT INSERTED.id_matricula "
                + "VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObservacaoRestricao, @Laudo);"
                : $"INSERT INTO {TableName} (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, observacao_restricao, laudo ) "
                + "VALUES (@AlunoId, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @ObservacaoRestricao, @Laudo); "
                + "SELECT LAST_INSERT_ID();";

                await using var command = DbProvider.CreateCommand(query, connection);

                command.Parameters.Add(DbProvider.CreateParameter("@AlunoId", entity.AlunoMatricula.Id, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Plano", entity.Plano, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataInicio", entity.DataInicio, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataFim", entity.DataFim, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Objetivo", entity.Objetivo, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@RestricaoMedica", entity.RestricoesMedicas, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@ObservacaoRestricao", entity.ObservacoesRestricoes, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Laudo", (object)entity.LaudoMedico ?? DBNull.Value, DbType.String, _databaseType));

                var id = await command.ExecuteScalarAsync();
                if (id != null && id != DBNull.Value)
                {
                    // Define o ID usando reflection
                    var idProperty = typeof(Entity).GetProperty("Id");
                    idProperty?.SetValue(entity, Convert.ToInt32(id));
                }
                return entity;
            }
            catch (DbException ex) { throw new InvalidOperationException($"Erro ao adicionar colaborador: {ex.Message}", ex); }
        }
        public override async Task<Matricula> Atualizar(Matricula entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();

                string query = $"UPDATE {TableName} "
                + "SET plano = @Plano, "
                + "data_inicio = @DataInicio, "
                + "data_fim = @DataFim, "
                + "objetivo = @Objetivo, "
                + "restricao_medica = @RestricaoMedica, "
                + "obs_restricao = @ObservacaoRestricao, "
                + "laudo_medico = @Laudo, "
                + "WHERE id_matricula = @Id";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@AlunoId", entity.AlunoMatricula.Id, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Plano", entity.Plano, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataInicio", entity.DataInicio, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataFim", entity.DataFim, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Objetivo", entity.Objetivo, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@RestricaoMedica", entity.RestricoesMedicas, DbType.Int64, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@ObservacaoRestricao", entity.ObservacoesRestricoes, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Laudo", (object)entity.LaudoMedico ?? DBNull.Value, DbType.String, _databaseType));

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Nenhuma matricula encontrado com o ID {entity.Id} para atualização.");
                }
                return entity;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao atualizar matricula com ID {entity.Id}: {ex.Message}", ex);
            }
        }

        protected override async Task<Matricula> MapAsync(DbDataReader reader)
        {
            try
            {
                var alunoRepository = new AlunoRepository(_connectionString, _databaseType);
                var aluno = await alunoRepository.ObterPorId((int)reader["aluno_id"]);

                if (aluno == null)
                    throw new InvalidOperationException($"Aluno com ID {reader["aluno_id"]} não encontrado.");

                var plano = (EMatriculaPlano)Convert.ToInt32(reader["plano"]);

                var dataInicio = DateOnly.FromDateTime(Convert.ToDateTime(reader["data_inicio"]));
                var dataFim = DateOnly.FromDateTime(Convert.ToDateTime(reader["data_fim"]));

                var objetivo = reader["objetivo"].ToString()!;

                var restricoes = (EMatriculaRestricoes)Convert.ToInt32(reader["restricao_medica"]);

                Arquivo? laudo = null;
                if (reader["laudo"] != DBNull.Value)
                {
                    // aqui depende do teu construtor de Arquivo
                    Arquivo.Criar((byte[])reader["foto"]);
                }

                var observacoes = reader["observacao_restricao"].ToString()!;

                var matricula = Matricula.Criar(
                    id: 0,
                    alunoMatricula: aluno,
                    plano: plano,
                    dataInicio: dataInicio,
                    dataFim: dataFim,
                    objetivo: objetivo,
                    restricoesMedicas: restricoes,
                    laudoMedico: laudo,
                    observacoesRestricoes: observacoes
                );

                // Define o ID herdado (reflection porque Id não tem setter público)
                var idProperty = typeof(Entity).GetProperty("Id");
                idProperty?.SetValue(matricula, Convert.ToInt32(reader["id_matricula"]));

                return matricula;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao mapear dados da matrícula: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId)
{
    try
    {
        await using var connection = await GetOpenConnectionAsync();

        string query = $"SELECT * FROM {TableName} WHERE aluno_id = @AlunoId";

        await using var command = DbProvider.CreateCommand(query, connection);
        command.Parameters.Add(DbProvider.CreateParameter("@AlunoId", alunoId, DbType.Int64, _databaseType));

        var matriculas = new List<Matricula>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            matriculas.Add(await MapAsync(reader));
        }

        return matriculas;
    }
    catch (DbException ex)
    {
        throw new InvalidOperationException($"Erro ao obter matrículas do aluno {alunoId}: {ex.Message}", ex);
    }
}

        public async Task<IEnumerable<Matricula>> ObterAtivas(int idAluno = 0)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT * FROM {TableName} WHERE data_fim >= {(_databaseType == DatabaseType.SqlServer ? "GETDATE()" :
                "CURRENT_DATE()")} {(idAluno > 0 ? "AND aluno_id = @id" : "")} ";
                await using var command = DbProvider.CreateCommand(query, connection);
                if (idAluno > 0)
                {
                    command.Parameters.Add(DbProvider.CreateParameter("@id", idAluno, DbType.Int32, _databaseType));
                }
                using var reader = await command.ExecuteReaderAsync();
                var matriculas = new List<Matricula>();
                while (await reader.ReadAsync())
                {
                    matriculas.Add(await MapAsync(reader));
                }
                return matriculas;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao obter matrículas ativas: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias)
{
    try
    {
        await using var connection = await GetOpenConnectionAsync();

        string dataLimite = _databaseType == DatabaseType.SqlServer
            ? $"DATEADD(DAY, @Dias, GETDATE())"
            : $"DATE_ADD(NOW(), INTERVAL @Dias DAY)";

        string query = $"SELECT * FROM {TableName} WHERE data_fim <= {dataLimite}";

        await using var command = DbProvider.CreateCommand(query, connection);
        command.Parameters.Add(DbProvider.CreateParameter("@Dias", dias, DbType.Int32, _databaseType));

        var matriculas = new List<Matricula>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            matriculas.Add(await MapAsync(reader));
        }

        return matriculas;
    }
    catch (DbException ex)
    {
        throw new InvalidOperationException($"Erro ao obter matrículas vencendo em {dias} dias: {ex.Message}", ex);
    }
}





    }
}