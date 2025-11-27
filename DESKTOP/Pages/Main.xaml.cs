using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DESKTOP.DTO.ALLDTO;

namespace DESKTOP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private readonly HttpClient _http = new()
        {
            BaseAddress = new Uri("https://localhost:7180")
        };
        public Main()
        {
            InitializeComponent();
            ReloadProducts();

        }

        private async void PingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Output.Text = "Запрос к /ping...\r\n";
                using var resp = await _http.GetAsync("/ping");
                var body = await resp.Content.ReadAsStringAsync();
                Output.AppendText($"Статус: {(int)resp.StatusCode} {resp.ReasonPhrase}\r\n");
                Output.AppendText($"Тело: {body}\r\n");
            }
            catch (Exception ex)
            {
                Output.AppendText($"Ошибка /ping: {ex.Message}\r\n");
            }
        }
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var name = (NameBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Output.AppendText("Введите название.\r\n");
                return;
            }

            var category = (CategoryBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(category))
            {
                Output.AppendText("Введите категорию.\r\n");
                return;
            }

            var description = (DescriptionBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                Output.AppendText("Введите описание товара.\r\n");
                return;
            }

            try
            {
                AddButton.IsEnabled = false;
                Output.AppendText($"Отправка запроса: {name} / {category}\r\n");

                var payload = new Product
                {
                    Name = name,
                    Description = description,
                    CategoryName = category
                };

                var resp = await _http.PostAsJsonAsync("/AddProduct", payload);
                
                var responseContent = await resp.Content.ReadAsStringAsync();
                Output.AppendText($"Ответ сервера (статус {(int)resp.StatusCode}): {responseContent}\r\n");

                if (!resp.IsSuccessStatusCode)
                {
                    Output.AppendText($"Ошибка при добавлении товара. Статус: {(int)resp.StatusCode} {resp.ReasonPhrase}. Детали: {responseContent}\r\n");
                    return;
                }

                Output.AppendText($"Добавлено: {name} Описание: {description}\r\n");

                NameBox.Clear();
                DescriptionBox.Clear();
                CategoryBox.Clear();

                await ReloadProducts();
            }
            catch (Exception ex)
            {
                Output.AppendText($"Ошибка /products (POST): {ex.Message}\r\n");
            }
            finally
            {
                AddButton.IsEnabled = true;
            }
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsGrid.SelectedItem is not Product u)
                {
                    Output.AppendText("Не выбран элемент для удаления\r\n");
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Удалить продукт #{u.Id}: {u.Name}?",
                    "Подтверждение",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.OK)
                    return;

                var resp = await _http.DeleteAsync($"/DeleteProduct/{u.Id}");

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    Output.AppendText($"DELETE /DeleteProduct/{u.Id} → {(int)resp.StatusCode} {resp.ReasonPhrase}: {err}\r\n");
                    return;
                }

                Output.AppendText($"Удалено: #{u.Id} {u.Name}\r\n");
                await ReloadProducts();
            }
            catch (Exception ex)
            {
                Output.AppendText($"Ошибка DELETE: {ex.Message}\r\n");
            }
        }

        private async Task ReloadProducts()
        {
            try
            {
                var resp = await _http.GetAsync("/GetAllProducts");
                if (resp.IsSuccessStatusCode)
                {
                    var products = await resp.Content.ReadFromJsonAsync<List<Product>>();
                    ProductsGrid.ItemsSource = products ?? new List<Product>();
                }
                else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ProductsGrid.ItemsSource = new List<Product>();
                }
            }
            catch (Exception ex)
            {
                Output.AppendText($"Ошибка загрузки данных: {ex.Message}\r\n");
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadProducts();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsGrid.SelectedItem is not Product u)
                {
                    Output.AppendText("Не выбран элемент для изменения\r\n");
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Изменить продукт #{u.Id}: {u.Name}?",
                    "Подтверждение",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.OK)
                    return;

                string name = NameBox.Text;
                if (string.IsNullOrEmpty(name)) 
                {
                    MessageBox.Show("Не введенно имя продукта!");
                    return;
                }
                    

                string description = DescriptionBox.Text;
                if (string.IsNullOrEmpty(description))
                {
                    MessageBox.Show("Не введенно описание продукта!");
                    return;
                }
                 
                string category = CategoryBox.Text;
                if (string.IsNullOrEmpty(category))
                {
                    MessageBox.Show("Не введенна категория продукта!");
                    return;
                }
                   
                var payload = new EditProductRequest() 
                { 
                    Id = u.Id,
                    Name = name,
                    Description = description,
                    CategoryName = category
                };

                var resp = await _http.PostAsJsonAsync($"/EditProduct/{u.Id}", payload);

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    Output.AppendText($"UPDATE /EditProduct/{u.Id} → {(int)resp.StatusCode} {resp.ReasonPhrase}: {err}\r\n");
                    return;
                }

                Output.AppendText($"Изменено: #{u.Id} {u.Name}\r\n");
                await ReloadProducts();
            }
            catch (Exception ex)
            {
                Output.AppendText($"Ошибка EDIT: {ex.Message}\r\n");
            }
        }
    }
}
