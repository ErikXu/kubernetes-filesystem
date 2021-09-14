using k8s;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using WebApi;

namespace Kubernetes.FileSystem.Controllers
{
    [Route("api/containers")]
    [ApiController]
    public class ContainersController : ControllerBase
    {
        [HttpGet]
        public IActionResult List([FromQuery] string cluster, [FromQuery] string @namespace, [FromQuery] string pod)
        {
            var configPath = Path.Combine(Program.ConfigDir, cluster.ToLower());
            if (!System.IO.File.Exists(configPath))
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configPath);
            var client = new k8s.Kubernetes(config);
            var specificPod = client.ListNamespacedPod(@namespace).Items.Where(n => n.Metadata.Name == pod).First();
            var containers = specificPod.Spec.Containers.Select(n => n.Name);
            return Ok(containers);
        }
    }
}
