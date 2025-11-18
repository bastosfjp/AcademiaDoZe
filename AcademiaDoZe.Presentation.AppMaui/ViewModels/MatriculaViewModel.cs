using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Services;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using CommunityToolkit.Mvvm.Input;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    [QueryProperty(nameof(MatriculaId), "Id")]
    public partial class MatriculaViewModel : BaseViewModel
    {
        public IEnumerable<EAppMatriculaPlano> MatriculaPlanos { get; } = Enum.GetValues(typeof(EAppMatriculaPlano)).Cast<EAppMatriculaPlano>();
        public IEnumerable<EAppMatriculaRestricoes> MatriculaRestricoes { get; } = Enum.GetValues(typeof(EAppMatriculaRestricoes)).Cast<EAppMatriculaRestricoes>();
        private readonly IMatriculaService _matriculaService;
        private readonly IAlunoService _alunoService;
        private MatriculaDTO _matricula = new()
        {
            Id = 0,
            AlunoMatricula = new AlunoDTO
            {
                Nome = string.Empty,
                Cpf = string.Empty,
                DataNascimento = DateOnly.MinValue,
                Telefone = string.Empty,
                Endereco = new LogradouroDTO()
                {
                    Cep = string.Empty,
                    Nome = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Pais = string.Empty
                },
                Numero = string.Empty,
                Complemento = string.Empty,
                Senha = string.Empty,
                Foto = new ArquivoDTO()
            },
            Plano = EAppMatriculaPlano.Mensal,
            DataInicio = DateOnly.FromDateTime(DateTime.Today),
            DataFim = DateOnly.FromDateTime(DateTime.Today).AddMonths(1),
            Objetivo = string.Empty,
            RestricoesMedicas = EAppMatriculaRestricoes.None,
            ObservacoesRestricoes = string.Empty,
            LaudoMedico = new ArquivoDTO()
        };


        public MatriculaDTO Matricula
        {
            get => _matricula;
            set => SetProperty(ref _matricula, value);
        }
        private int _matriculaId;
        public int MatriculaId
        {
            get => _matriculaId;
            set => SetProperty(ref _matriculaId, value);
        }
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }
        public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
        {
            _matriculaService = matriculaService;
            _alunoService = alunoService;
            Title = "Detalhes da Matricula";
        }
        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
        public async Task InitializeAsync()
        {
            if (MatriculaId > 0)
            {
                IsEditMode = true;
                Title = "Editar Matrícula";
                await LoadMatriculaAsync();
            }
            else
            {
                IsEditMode = false;
                Title = "Nova Matricula";
            }
        }
        [RelayCommand]
        public async Task SearchByCpfAsync()
        {
            if (string.IsNullOrWhiteSpace(Matricula.AlunoMatricula.Cpf))
                return;

            try
            {
                IsBusy = true;

                var cpfNormalized = new string(Matricula.AlunoMatricula.Cpf.Where(char.IsDigit).ToArray());

                var aluno = await _alunoService.ObterPorCpfAsync(cpfNormalized);

                if (aluno == null)
                {
                    await Shell.Current.DisplayAlert("Aviso", "CPF não encontrado.", "OK");
                    return;
                }

                Matricula.AlunoMatricula = aluno;
                OnPropertyChanged(nameof(Matricula));
                await Shell.Current.DisplayAlert("Aviso", "Aluno encontrado! dados preenchidos automaticamente.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar CPF: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task LoadMatriculaAsync()
        {
            if (MatriculaId <= 0)
                return;
            try
            {
                IsBusy = true;
                var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId);

                if (matriculaData != null)
                {
                    Matricula = matriculaData;

                    // 🔥 AVISA ao MAUI que os valores mudaram
                    OnPropertyChanged(nameof(Matricula));
                    OnPropertyChanged(nameof(DataInicio));
                    OnPropertyChanged(nameof(DataFim));
                    OnPropertyChanged(nameof(Plano));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matricula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        public async Task SaveMatriculaAsync()
        {
            if (IsBusy)
                return;
            if (!ValidateMatricula(Matricula))
                return;
            try
            {
                IsBusy = true;
                // Verifica se o Id existe antes de continuar

                var alunoData = await _alunoService.ObterPorIdAsync(Matricula.AlunoMatricula.Id);
                if (alunoData == null)

                {
                    await Shell.Current.DisplayAlert("Erro", "O Id do aluno informado não existe. O cadastro não pode continuar.", "OK");
                    return;
                }
                Matricula.AlunoMatricula = alunoData;
                if (IsEditMode)
                {
                    await _matriculaService.AtualizarAsync(Matricula);

                    await Shell.Current.DisplayAlert("Sucesso", "Matrícula atualizada com sucesso!", "OK");

                }
                else
                {
                    await _matriculaService.AdicionarAsync(Matricula);

                    await Shell.Current.DisplayAlert("Sucesso", "Matrícula criada com sucesso!", "OK");

                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SelecionarFotoAsync()
        {
            try
            {
                string escolha = await Shell.Current.DisplayActionSheet("Origem da Imagem", "Cancelar", null, "Galeria", "Câmera");
                FileResult? result = null;
                if (escolha == "Galeria")

                {
                    result = await FilePicker.Default.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecione uma imagem",
                        FileTypes = FilePickerFileType.Images
                    });
                }
                else if (escolha == "Câmera")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        result = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Erro", "Captura de foto não suportada neste dispositivo.", "OK");
                        return;
                    }
                }
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    // ✅ Mantém o mesmo objeto e só atualiza o conteúdo
                    if (Matricula.LaudoMedico == null)
                        Matricula.LaudoMedico = new ArquivoDTO();

                    Matricula.LaudoMedico.Conteudo = ms.ToArray();

                    // Atualiza o binding da imagem
                    OnPropertyChanged(nameof(Matricula));
                }

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
            }
        }
        private static bool ValidateMatricula(MatriculaDTO matricula)
        {
            const string validationTitle = "Validação";
            if (string.IsNullOrWhiteSpace(matricula.AlunoMatricula.Cpf))
            {
                Shell.Current.DisplayAlert(validationTitle, "Dados do Aluno sáo Obrigatórios.", "OK");
                return false;
            }
            if (matricula.DataInicio == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de início é obrigatória.", "OK");
                return false;
            }
            if (matricula.DataFim == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Data de encerramento é obrigatória.", "OK");
                return false;
            }
            if (matricula.Objetivo == default)
            {
                Shell.Current.DisplayAlert(validationTitle, "Objetivo do Aluno é obrigatório.", "OK");
                return false;
            }
            return true;
        }
        public bool HasDiabetes
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.Diabetes);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.Diabetes;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.Diabetes;
                OnPropertyChanged(nameof(HasDiabetes));
            }
        }

        public bool HasPressaoAlta
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.PressaoAlta);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.PressaoAlta;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.PressaoAlta;
                OnPropertyChanged(nameof(HasPressaoAlta));
            }
        }

        public bool HasLabirintite
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.Labirintite);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.Labirintite;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.Labirintite;
                OnPropertyChanged(nameof(HasLabirintite));
            }
        }

        public bool HasAlergias
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.Alergias);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.Alergias;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.Alergias;
                OnPropertyChanged(nameof(HasAlergias));
            }
        }

        public bool HasProblemasRespiratorios
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.ProblemasRespiratorios);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.ProblemasRespiratorios;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.ProblemasRespiratorios;
                OnPropertyChanged(nameof(HasProblemasRespiratorios));
            }
        }

        public bool HasRemedioContinuo
        {
            get => Matricula.RestricoesMedicas.HasFlag(EAppMatriculaRestricoes.RemedioContinuo);
            set
            {
                if (value)
                    Matricula.RestricoesMedicas |= EAppMatriculaRestricoes.RemedioContinuo;
                else
                    Matricula.RestricoesMedicas &= ~EAppMatriculaRestricoes.RemedioContinuo;
                OnPropertyChanged(nameof(HasRemedioContinuo));
            }
        }

        public void CalcularDataFim()
        {
            Matricula.DataFim = Matricula.Plano switch
            {
                EAppMatriculaPlano.Mensal => Matricula.DataInicio.AddMonths(1),
                EAppMatriculaPlano.Trimestral => Matricula.DataInicio.AddMonths(3),
                EAppMatriculaPlano.Semestral => Matricula.DataInicio.AddMonths(6),
                EAppMatriculaPlano.Anual => Matricula.DataInicio.AddYears(1),
                _ => Matricula.DataInicio
            };
        }
        public DateOnly DataInicio
        {
            get => Matricula.DataInicio;
            set
            {
                if (Matricula.DataInicio != value)
                {
                    Matricula.DataInicio = value;
                    CalcularDataFim();
                    OnPropertyChanged(nameof(DataInicio));
                    OnPropertyChanged(nameof(DataFim));
                }
            }
        }

        public EAppMatriculaPlano Plano
        {
            get => Matricula.Plano;
            set
            {
                if (Matricula.Plano != value)
                {
                    Matricula.Plano = value;
                    CalcularDataFim();
                    OnPropertyChanged(nameof(Plano));
                    OnPropertyChanged(nameof(DataFim));
                }
            }
        }

        public DateOnly DataFim
        {
            get => Matricula.DataFim;
            set
            {
                Matricula.DataFim = value;
                OnPropertyChanged(nameof(DataFim));
            }
        }

    }
}