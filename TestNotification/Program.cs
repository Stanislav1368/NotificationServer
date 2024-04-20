using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestNotification
{//dasgasdgasdgasd zalupa o4ko
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Период обращения к API (в секундах)
            int pollingIntervalSeconds = 60;

            // URL вашего API
            string apiUrl = "https://stalcraft.wiki/api/available-lots?region=ru&id=zz6km";

            // Список предметов, с которым будем сравнивать новые предметы
            List<string> previousItems = new List<string>();

            while (true)
            {
                List<string> currentItems = await GetItemsFromApiAsync(apiUrl);

                if (currentItems != null)
                {
                    // Проверяем наличие новых предметов
                    List<string> newItems = GetNewItems(previousItems, currentItems);

                    if (newItems.Count > 0)
                    {
                        // Отправляем новые предметы через сокет
                        Console.WriteLine($"Отправляем новые предметы через сокет. Время: {DateTime.Now}");
                        SendNotification(JsonConvert.SerializeObject(newItems));
                    }

                    // Обновляем список предметов
                    previousItems = currentItems;
                }

                // Ждем перед следующим обращением к API
                Thread.Sleep(pollingIntervalSeconds * 1000);
            }
        }

        static async Task<List<string>> GetItemsFromApiAsync(string apiUrl)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var lots = JsonConvert.DeserializeObject<LotResponse>(jsonResponse);
                        try
                        {
                            // Проверка на null для объекта lots
                            if (lots != null)
                            {
                                List<string> items = new List<string>();

                                foreach (var lot in lots.Lots)
                                {
                                    items.Add($"ID: {lot.ItemId}, Стартовая цена: {lot.StartPrice}, Цена выкупа: {lot.BuyoutPrice}");
                                }

                                return items;
                            }
                            else
                            {
                                Console.WriteLine("API вернуло пустой ответ.");
                                return null;
                            }
                        }
                        catch (Exception)
                        {

                            return null;
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Ошибка при получении данных с API. Код состояния: " + response.StatusCode);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при обращении к API: " + ex.Message);
                    return null;
                }
            }
        }

        static List<string> GetNewItems(List<string> previousItems, List<string> currentItems)
        {
            List<string> newItems = new List<string>();

            foreach (var currentItem in currentItems)
            {
                if (!previousItems.Contains(currentItem))
                {
                    newItems.Add(currentItem);
                }
            }

            return newItems;
        }

        static void SendNotification(string message)
        {
            try
            {
                // Адрес и порт вашего сервера сокетов
                string serverAddress = "127.0.0.1";
                int serverPort = 8888;

                // Создание TCP клиента и подключение к серверу сокетов
                using (TcpClient client = new TcpClient(serverAddress, serverPort))
                using (NetworkStream stream = client.GetStream())
                {
                    // Преобразование сообщения в байты
                    byte[] data = Encoding.UTF8.GetBytes(message);

                    // Отправка данных
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке оповещения: " + ex.Message);
            }
        }
    }
}