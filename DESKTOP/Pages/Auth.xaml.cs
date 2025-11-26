using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DESKTOP;

namespace DESKTOP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Page
    {
        private readonly HttpClient _http = new() 
        { 
            BaseAddress = new Uri("https://localhost:7180") 
        };

        public Auth()
        {
            InitializeComponent();
            
        }

        private async void btnAuth(object sender, RoutedEventArgs e)
        {
            var Email = BoxEmail.Text;
            var Password = BoxPassword.Text;

            if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            try
            {
                var payload = new DTORequest(Email, Password);
                var response = await _http.PostAsJsonAsync("/auth", payload);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<DTOResponse>();

                if (result is { Accepted: true })
                {
                    this.NavigationService.Navigate(new Main());
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аутентификации: {ex.Message}");
            }
        }

        // поля должны совпадать с тем, что возвращает API: { accepted = true, message = \"...\" }
        private record DTORequest(string Email, string Password);
        private record DTOResponse(bool Accepted, string? Message);
    }
}
