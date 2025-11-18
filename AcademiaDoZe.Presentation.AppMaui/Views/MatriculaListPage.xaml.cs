using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;
namespace AcademiaDoZe.Presentation.AppMaui.Views;
public partial class MatriculaListPage : ContentPage
{
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaListViewModel viewModel)
        {
            await viewModel.LoadMatriculasCommand.ExecuteAsync(null);
        }
    }
    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is MatriculaDTO matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.EditMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao editar matricula: {ex.Message}", "OK");
        }
    }
    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is MatriculaDTO matricula && BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.DeleteMatriculaCommand.ExecuteAsync(matricula);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir matricula: {ex.Message}", "OK");
        }
    }
    // CancellationTokenSource é uma classe do .NET usada para controlar o cancelamento de operações assíncronas,
    // como tarefas (Task) ou métodos async/await.
    // Ela permite que você envie um sinal de cancelamento para uma ou mais operações que estejam ouvindo esse sinal.
    private CancellationTokenSource? _searchCts;
    // Debounce é uma técnica usada para evitar que uma ação seja executada repetidamente em alta frequência,
    // especialmente durante eventos que disparam várias vezes em sequência, como digitação em um campo de busca.
    // No nosso caso, debounce serve para evitar que a busca seja feita a cada tecla pressionada.
    // Em vez disso, a busca só é executada após um pequeno intervalo sem novas digitações, por exemplo, 300ms.
    // Se o usuário continuar digitando, o timer reinicia e a busca só acontece quando ele parar de digitar por esse tempo.
    private async void OnSearchDebounceTextChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;
            // espera curta (300ms)
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;
            if (BindingContext is MatriculaListViewModel viewModel)
            {
                await viewModel.SearchMatriculasCommand.ExecuteAsync(null);
            }
        }
        catch (TaskCanceledException) { /* ignorar */ }
    }
}