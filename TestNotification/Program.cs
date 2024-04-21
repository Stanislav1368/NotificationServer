using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace TestNotification
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Период обращения к API (в секундах)
            int pollingIntervalSeconds = 60;

            // URL вашего API
            List<string> listUrl = new List<string> {"https://stalcraftdb.net/api/items/ok096/auction-lots?region=ru&page=0", "https://stalcraftdb.net/api/items/knqkv/auction-history?region=ru&page=0"  };
            
            

            // Список предметов, с которым будем сравнивать новые предметы
            List<string> previousItems = new List<string>();

            while (true)
            {
                LotResponse lots = new LotResponse();
                foreach (var item in listUrl)
                {

                    lots = await GetItemsFromApiAsync(item);



                    if (lots != null)
                    {
                        // Проверяем наличие новых предметов
                        List<string> newItems = GetNewItems(previousItems, lots.Lots);
                        if (newItems != null)
                        {
                            if (newItems.Count > 0)
                            {
                                // Отправляем новые предметы через сокет
                                Console.WriteLine($"Отправляем новые предметы через сокет. Время: {DateTime.Now}");
                                await SendNotificationAsync(lots);
                            }
                        }


                        try { previousItems = lots.Lots.Select(lot => $"ID: {lot.ItemId}, Стартовая цена: {lot.StartPrice}, Цена выкупа: {lot.BuyoutPrice}").ToList(); }
                        catch { }
                        // Обновляем список предметов

                    }
                    else
                    {
                        Console.WriteLine("lots равен null");
                    }
                }
                    // Ждем перед следующим обращением к API
                    await Task.Delay(pollingIntervalSeconds * 1000);
                
            }
        }

        static async Task<LotResponse> GetItemsFromApiAsync(string apiUrl)
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
                        return lots;
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

        static List<string> GetNewItems(List<string> previousItems, List<Lot> currentLots)
        {
            
            List<string> newItems = new List<string>();
            if(currentLots != null) {
                try
                {
                    foreach (var currentLot in currentLots)
                    {
                        string currentItem = $"ID: {currentLot.ItemId}, Стартовая цена: {currentLot.StartPrice}, Цена выкупа: {currentLot.BuyoutPrice}";
                        if (!previousItems.Contains(currentItem))
                        {
                            newItems.Add(currentItem);
                        }
                    }
                    return newItems;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
            else
            {
                Console.WriteLine("currentLots равно null");
                return null;
            }
            



}

        static async Task SendNotificationAsync(LotResponse lotResponse)
        {
            try
            {
                // Адрес и порт вашего сервера сокетов
                string serverAddress = "127.0.0.1";
                int serverPort = 8888;

                // Сериализация объекта LotResponse в формат JSON
                string jsonMessage = JsonConvert.SerializeObject(lotResponse);

                // Создание TCP клиента и подключение к серверу сокетов
                using (TcpClient client = new TcpClient(serverAddress, serverPort))
                using (NetworkStream stream = client.GetStream())
                {
                    // Преобразование сообщения в байты
                    byte[] data = Encoding.UTF8.GetBytes(jsonMessage);

                    // Отправка данных
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке оповещения: " + ex.Message);
            }
        }
    }
}