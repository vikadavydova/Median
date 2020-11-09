using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace ConsoleApp1
{
    class Program
    {
        private const int PORT = 2012;
        private const string SERVER = "88.212.241.115";
        private const int amountOfNumbers = 2018;
        private const int amountOfTasks = 250;
        private static int[] numbers = new int[amountOfNumbers];
        static object locker = new object();
        static int currentI = 0;


        static void Main(string[] args)
        {

            Task[] tasks = new Task[amountOfTasks];

            for (int i = 1; i < amountOfTasks + 1; i++)
            {
                tasks[i - 1] = new Task(() => GetResponse());
                tasks[i - 1].Start();
            }

            Task.WaitAll(tasks);

            Array.Sort(numbers);
            Console.WriteLine($"Итоговый ответ {(numbers[amountOfNumbers / 2 - 1] + numbers[amountOfNumbers / 2]) / 2}");
            Console.Read();
        }

        public static void GetResponse()
        {
            int currentState = 1;
            int prevState = 1;
            Encoding koi8r = Encoding.GetEncoding("koi8-r");
            StringBuilder response = new StringBuilder();
            bool isResponseReceived = true;

            while (true)
            {

                using (TcpClient client = new TcpClient())
                {
                    try
                    {
                        client.Connect(SERVER, PORT);
                        byte[] data = new byte[256];
                        using (NetworkStream stream = client.GetStream())
                        {
                            while (true)
                            {
                                response.Clear();

                                //checking if the request worked out correctly
                                if (isResponseReceived)
                                {
                                    lock (locker)
                                    {
                                        currentState = ++currentI; ;
                                    }
                                    prevState = currentState;
                                    isResponseReceived = false;
                                    if (currentState > amountOfNumbers)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    currentState = prevState;
                                }


                                //sending request to the server
                                string message = $"{currentState}\n";
                                byte[] msg = koi8r.GetBytes(message);
                                stream.Write(msg, 0, msg.Length);

                                //get response from the server
                                do
                                {
                                    int bytes = stream.Read(data, 0, data.Length);
                                    response.Append(koi8r.GetString(data, 0, bytes));
                                } while (response[response.Length - 1] != '\n');

                                int checkValue;
                                bool check;

                                //remove '.' from the string
                                response.Replace(".", null);
                                check = Int32.TryParse(response.ToString(), out checkValue);

                                numbers[currentState - 1] = checkValue;
                                Console.WriteLine($"{currentState - 1}: {checkValue}");
                                isResponseReceived = true;
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        continue;
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
        }
    }
}

