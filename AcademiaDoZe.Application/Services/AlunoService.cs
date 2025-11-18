using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Application.Security;

namespace AcademiaDoZe.Application.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly Func<IAlunoRepository> _repoFactory;

        public AlunoService(Func<IAlunoRepository> repoFactory)
        {
            _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
        }

        private IAlunoRepository Repo => _repoFactory();

        // ========================================================
        public async Task<AlunoDTO> ObterPorIdAsync(int id)
        {
            var aluno = await Repo.ObterPorId(id);
            return aluno?.ToDto();
        }

        // ========================================================
        public async Task<IEnumerable<AlunoDTO>> ObterTodosAsync()
        {
            var alunos = await Repo.ObterTodos();
            return alunos.Select(a => a.ToDto());
        }

        // ========================================================
        public async Task<AlunoDTO> AdicionarAsync(AlunoDTO alunoDto)
        {
            // Normaliza CPF
            alunoDto.Cpf = ApenasNumeros(alunoDto.Cpf);

            if (await Repo.CpfJaExiste(alunoDto.Cpf))
                throw new InvalidOperationException($"Já existe um aluno cadastrado com o CPF {alunoDto.Cpf}.");

            // Aplica hash somente se senha enviada
            if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
                alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);

            var aluno = alunoDto.ToEntity();

            await Repo.Adicionar(aluno);

            return aluno.ToDto();
        }

        // ========================================================
        public async Task<AlunoDTO> AtualizarAsync(AlunoDTO alunoDto)
        {
            var alunoExistente = await Repo.ObterPorId(alunoDto.Id)
                ?? throw new KeyNotFoundException($"Aluno ID {alunoDto.Id} não encontrado.");

            alunoDto.Cpf = ApenasNumeros(alunoDto.Cpf);

            if (await Repo.CpfJaExiste(alunoDto.Cpf, alunoDto.Id))
                throw new InvalidOperationException($"Já existe outro aluno com o CPF {alunoDto.Cpf}.");

            // Hash somente se realmente informou uma nova senha
            if (!string.IsNullOrWhiteSpace(alunoDto.Senha))
                alunoDto.Senha = PasswordHasher.Hash(alunoDto.Senha);

            var alunoAtualizado = alunoExistente.UpdateFromDto(alunoDto);

            await Repo.Atualizar(alunoAtualizado);

            return alunoAtualizado.ToDto();
        }

        // ========================================================
        public async Task<bool> RemoverAsync(int id)
        {
            var aluno = await Repo.ObterPorId(id);

            if (aluno is null)
                return false;

            await Repo.Remover(id);
            return true;
        }

        // ========================================================
        public async Task<AlunoDTO> ObterPorCpfAsync(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF não pode ser vazio.", nameof(cpf));

            cpf = ApenasNumeros(cpf);

            var aluno = await Repo.ObterPorCpf(cpf);

            return aluno?.ToDto();
        }

        // ========================================================
        public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null)
        {
            cpf = ApenasNumeros(cpf);
            return await Repo.CpfJaExiste(cpf, id);
        }

        // ========================================================
        public async Task<bool> TrocarSenhaAsync(int id, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(novaSenha))
                throw new ArgumentException("Nova senha inválida.", nameof(novaSenha));

            var hash = PasswordHasher.Hash(novaSenha);
            return await Repo.TrocarSenha(id, hash);
        }

        // ========================================================
        private static string ApenasNumeros(string texto)
        {
            if (texto == null) return "";
            return new string(texto.Where(char.IsDigit).ToArray());
        }
    }
}
