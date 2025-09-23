/*
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class AcessoRepository : BaseRepository<Acesso>, IAcessoRepository
    {
        public AcessoRepository(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        public override async Task<Acesso> Adicionar(Acesso entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = _databaseType == DatabaseType.SqlServer
                    ? $"INSERT INTO {TableName} (pessoa_id, data_hora, entrada, saida) OUTPUT INSERTED.id_acesso VALUES (@PessoaId, @DataHora, @Entrada, @Saida);"
                    : $"INSERT INTO {TableName} (pessoa_id, data_hora, entrada, saida) VALUES (@PessoaId, @DataHora, @Entrada, @Saida); SELECT LAST_INSERT_ID();";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@PessoaId", entity.AlunoColaborador.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataHora", entity.DataHora, DbType.DateTime, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Entrada", entity.Entrada, DbType.DateTime, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Saida", (object?)entity.Saida ?? DBNull.Value, DbType.DateTime, _databaseType));

                var id = await command.ExecuteScalarAsync();
                if (id != null && id != DBNull.Value)
                {
                    var idProperty = typeof(Entity).GetProperty("Id");
                    idProperty?.SetValue(entity, Convert.ToInt32(id));
                }

                return entity;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao adicionar acesso: {ex.Message}", ex);
            }
        }

        public override async Task<Acesso> Atualizar(Acesso entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"UPDATE {TableName} SET pessoa_id=@PessoaId, data_hora=@DataHora, entrada=@Entrada, saida=@Saida WHERE id_acesso=@Id";
                await using var command = DbProvider.CreateCommand(query, connection);

                command.Parameters.Add(DbProvider.CreateParameter("@Id", entity.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@PessoaId", entity.AlunoColaborador.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataHora", entity.DataHora, DbType.DateTime, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Entrada", entity.Entrada, DbType.DateTime, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Saida", (object?)entity.Saida ?? DBNull.Value, DbType.DateTime, _databaseType));

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    throw new InvalidOperationException($"Nenhum acesso encontrado com o ID {entity.Id} para atualização.");

                return entity;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao atualizar acesso com ID {entity.Id}: {ex.Message}", ex);
            }
        }

        public override async Task<Acesso?> ObterPorId(int id)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT * FROM {TableName} WHERE id_acesso=@Id";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));

                await using var reader = await command.ExecuteReaderAsync();
                return await reader.ReadAsync() ? await MapAsync(reader) : null;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao obter acesso com ID {id}: {ex.Message}", ex);
            }
        }

        public override async Task<IEnumerable<Acesso>> ObterTodos()
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT * FROM {TableName}";
                await using var command = DbProvider.CreateCommand(query, connection);

                var acessos = new List<Acesso>();
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    acessos.Add(await MapAsync(reader));
                }
                return acessos;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao obter acessos: {ex.Message}", ex);
            }
        }

        protected override async Task<Acesso> MapAsync(DbDataReader reader)
        {
            try
            {
                int pessoaId = Convert.ToInt32(reader["pessoa_id"]);
                DateTime entrada = Convert.ToDateTime(reader["entrada"]);
                DateTime? saida = reader["saida"] is DBNull ? null : Convert.ToDateTime(reader["saida"]);

                // Placeholder de Pessoa. Pode ser substituído por repositório de Aluno/Colaborador
                var pessoa = new PessoaPlaceholder { Id = pessoaId };

                var acesso = new Acesso(pessoa, entrada);
                if (saida.HasValue)
                    acesso.RegistrarSaida(saida.Value);

                var idProperty = typeof(Entity).GetProperty("Id");
                idProperty?.SetValue(acesso, Convert.ToInt32(reader["id_acesso"]));
                return acesso;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao mapear dados do acesso: {ex.Message}", ex);
            }
        }

        // Métodos específicos da interface ainda como NotImplemented
        public Task<IEnumerable<Acesso>> GetAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null)
            => throw new NotImplementedException();

        public Task<IEnumerable<Acesso>> GetAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null)
            => throw new NotImplementedException();

        public Task<Dictionary<TimeOnly, int>> GetHorarioMaisProcuradoPorMes(int mes)
            => throw new NotImplementedException();

        public Task<Dictionary<int, TimeSpan>> GetPermanenciaMediaPorMes(int mes)
            => throw new NotImplementedException();

        public Task<IEnumerable<Aluno>> GetAlunosSemAcessoNosUltimosDias(int dias)
            => throw new NotImplementedException();

        // Placeholder para mapear Pessoa sem precisar do repositório de Aluno/Colaborador
        private class PessoaPlaceholder : Pessoa
        {
            public PessoaPlaceholder()
                : base("Placeholder", "00000000000", DateOnly.MinValue, "00000000000", "placeholder@email.com", null!, "", "", "senha123", null) { }
        }
    }
}
*/