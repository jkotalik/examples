using k8s;
using k8s.Models;
using YamlDotNet.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var config = KubernetesClientConfiguration.IsInCluster()
    ? KubernetesClientConfiguration.InClusterConfig()
    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

var kubeClient = new Kubernetes(config);

var deployment = new V1Deployment
  {
            ApiVersion = "apps/v1",
            Metadata = new()
            {
                Name = "hello-world",
                Labels = new Dictionary<string, string>
                {
                    ["app.kubernetes.io/name"] = "hello-world"
                }
            },
            Spec = new()
            {
                Replicas = 1,
                Selector = new k8s.Models.V1LabelSelector()
                {
                    MatchLabels = new Dictionary<string, string>
                    {
                        ["app.kubernetes.io/name"] = "hello-world"
                    }
                },
                Template = new()
                {
                    Metadata = new()
                    {
                        Labels = new Dictionary<string, string>
                        {
                            ["app.kubernetes.io/name"] = "hello-world"
                        }
                    },
                    Spec = new()
                    {
                        Containers = new k8s.Models.V1Container[]
                        {
                            new()
                            {
                                Name = "hello-world",
                                Image = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest",
                                ImagePullPolicy = "Always",
                                Ports = new k8s.Models.V1ContainerPort[]
                                {
                                    new()
                                    {
                                        Name = "http",
                                        ContainerPort = 80
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

await kubeClient.CreateNamespacedDeploymentAsync(deployment, "default");

app.MapGet("/", () => "Hello World!");

app.Run();
