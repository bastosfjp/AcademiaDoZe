﻿using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();

            // aplicar o tema salvo nas preferências
            AplicarTema();

            // assinar para receber mensagens de alteração de preferências
            // toda vez que o usuário alterar o tema, essa mensagem será enviada
            // e o tema será atualizado
            WeakReferenceMessenger.Default.Register<Message.TemaPreferencesUpdatedMessage>(this, (r, m) =>

            {
                // m.Value contém o valor enviado na mensagem
                AplicarTema();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void AplicarTema()
        {
            UserAppTheme = Preferences.Get("Tema", "system") switch
            {
                "light" => AppTheme.Light,
                "dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,

            };
        }
    }
}