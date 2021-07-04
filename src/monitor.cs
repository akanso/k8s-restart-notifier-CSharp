using System;
using k8s;
using k8s.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;


namespace watch
{
    public class Monitor
    {
        /// <summary>
        /// webhookUrl is a microsoft Teams webhook
        /// </summary>
        private static readonly string webhookUrl = Environment.GetEnvironmentVariable("TEAMS_WEBHOOK");

        public  static void Watch()
        {
            
        }
        /// <summary>
        /// Verify wil check if any on pods' containers is terminated and call NotifyTeams
        /// </summary>
        /// <param name="type"></param>
        /// <param name="item"></param>
        public async static void Verify(WatchEventType type, V1Pod item)
        {
            Console.WriteLine("Event type = {0}, occured on pod with name = {1}", type, item.Metadata.Name);

            if ((item.Status != null) && (item.Status.ContainerStatuses != null))
            {
                foreach (V1ContainerStatus status in item.Status.ContainerStatuses)
                {
                    if (status.State.Terminated != null)
                    {
                        string msg = $"Inside pod `{item.Metadata.Name}`, a container with image `{status.Image}` has terminated at = `{status.State.Terminated.FinishedAt}`";

                        Console.WriteLine(msg);

                        await NotifyTeams(msg);
                    }
                }
            }
        }

        /// <summary>
        /// NotifyTeams posts a message on teams
        /// needs a teams webhook url
        /// </summary>
        /// <param name="msg">the content of a message to post on teams</param>
        /// <returns></returns>
        static async Task NotifyTeams(string msg)
        {
            Console.WriteLine("notifying teams...");

            if (string.IsNullOrEmpty(webhookUrl))
            {
                Console.WriteLine($"teams webhook is not set in env var TEAMS_WEBHOOK, no notification will be sent");
                return;
            }

            try
            {
                Message message = new Message() { text = msg };

                string json = JsonConvert.SerializeObject(message);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpClient httpClient = new HttpClient();

                HttpResponseMessage response = await httpClient.PostAsync(webhookUrl, data);

                Console.WriteLine($"teams request response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }           
        }
    }

    // to ensure proper message format
    class Message
    {
        public string text { get; set; }
    }
}
