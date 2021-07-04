using System;
using System.Threading;
using k8s;
using k8s.Models;

namespace watch
{
    public class Program
    {
        // if the env var is not set, we use the default k8s namespace for KubeNamespace
        private static string KubeNamespace => !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("NAMESPACE")) ? Environment.GetEnvironmentVariable("NAMESPACE") : "default";
        private static void Main(string[] args)
        {
            Console.WriteLine($"Program started... will watch pods in namespace = {KubeNamespace}");
            // Initializes a new instance of the k8s.KubernetesClientConfiguration from default
            // locations If the KUBECONFIG environment variable is set, then that will be used.
            // Next, it looks for a config file at k8s.KubernetesClientConfiguration.KubeConfigDefaultLocation.
            // Then, it checks whether it is executing inside a cluster and will use k8s.KubernetesClientConfiguration.InClusterConfig.
            // Finally, if nothing else exists, it creates a default config with localhost:8080
            // as host.
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildDefaultConfig();

            // Use the config object to create a client.
            Kubernetes client = new Kubernetes(config);

            try
            {
                // Watch for changes to the pods and return them as a stream of
                // add, update, and remove notifications.
                var podlistResp = client.ListNamespacedPodWithHttpMessagesAsync(KubeNamespace, watch: true);
                using (podlistResp.Watch<V1Pod, V1PodList>((type, item) =>
                {
                    Console.WriteLine("Event Received From K8s");
                    // the type is: Added, Deleted, or Modified
                    // the item is a V1Pod
                    Monitor.Verify(type, item);

                }))
                {
                    Console.WriteLine("Waiting for SIGINT/SIGTERM");
                    var ctrlc = new ManualResetEventSlim(false);
                    Console.CancelKeyPress += (sender, eventArgs) => ctrlc.Set();
                    ctrlc.Wait();
                }
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine($"An error occured while trying to watch pods: {ex}");
            }
        }
    }
}
