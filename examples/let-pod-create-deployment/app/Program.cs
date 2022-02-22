using k8s;
using k8s.Models;
using YamlDotNet.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var config = KubernetesClientConfiguration.IsInCluster()
    ? KubernetesClientConfiguration.InClusterConfig()
    : KubernetesClientConfiguration.BuildConfigFromConfigFile();

var kubeClient = new Kubernetes(config);

var deserializer = new DeserializerBuilder().Build();

var deployment = deserializer.Deserialize<V1Deployment>($@"
apiVersion: apps/v1
kind: Deployment
metadata:
    name: hello-world
    namespace: default
    labels:
        control-plane: hello-world
spec:
    selector:
        matchLabels:
            app.kubernetes.io/name: hello-world
    template:
        metadata:
            labels:
                app.kubernetes.io/name: hello-world
        spec:
            containers:
            -   name: hello-world
                image: mcr.microsoft.com/azuredocs/containerapps-helloworld:latest
                ports:
                -   name: http
                    containerPort: 8080
            serviceAccountName: manager
");

await kubeClient.CreateNamespacedDeploymentAsync(deployment, "default");

app.MapGet("/", () => "Hello World!");

app.Run();
