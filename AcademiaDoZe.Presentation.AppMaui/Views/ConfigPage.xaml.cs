using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
namespace AcademiaDoZe.Presentation.AppMaui.Views;
public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
    }
    private void CarregarTema()
    {
        // uso de express�o switch para carregar o �ndice selecionado
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch { "light" => 0, "dark" => 1, _ => 2, };
    }
    private async void OnSalvarTemaClicked(object sender, EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };
        Preferences.Set("Tema", selectedTheme);
        // Disparar mensagem para uso na recarga din�mica
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
        await DisplayAlert("Sucesso", "Dados salvos com sucesso!", "OK");
        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        // retornar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    // Ao fechar a p�gina, chama WeakReferenceMessenger.Default.UnregisterAll(this); para evitar vazamentos de mem�ria - memory leaks
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Desinscreve o mensageiro para evitar memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}