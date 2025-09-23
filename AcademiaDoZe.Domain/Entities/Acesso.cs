namespace AcademiaDoZe.Domain.Entities
{
    public class Acesso : Entity
    {
        public Pessoa AlunoColaborador { get; private set; }
        public DateTime DataHora { get; private set; }
        public DateTime Entrada { get; private set; }      // Data e hora da entrada
        public DateTime? Saida { get; private set; }       // Data e hora da saída (pode ser null até registrar)

        public Acesso(Pessoa pessoa, DateTime entrada)
        {
            AlunoColaborador = pessoa ?? throw new ArgumentNullException(nameof(pessoa));
            Entrada = entrada;
            Saida = null;
        }

        // Registrar a saída do acesso
        public void RegistrarSaida(DateTime saida)
        {
            if (saida < Entrada)
                throw new ArgumentException("A saída não pode ser anterior à entrada.");

            if (Saida != null)
                throw new InvalidOperationException("Saída já foi registrada.");

            Saida = saida;
        }

        // Calcula o tempo total de permanência (retorna TimeSpan.Zero se saída não foi registrada)
        public TimeSpan TempoPermanencia()
        {
            return Saida.HasValue ? Saida.Value - Entrada : TimeSpan.Zero;
        }
    }
}