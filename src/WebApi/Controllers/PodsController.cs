using k8s;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using WebApi;

namespace Kubernetes.FileSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PodsController : ControllerBase
    {
        [HttpGet]
        public IActionResult List([FromQuery] string cluster, [FromQuery] string @namespace)
        {
            var configPath = Path.Combine(Program.ConfigDir, cluster.ToLower());
            if (!System.IO.File.Exists(configPath))
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configPath);
            var client = new k8s.Kubernetes(config);
            var pods = client.ListNamespacedPod(@namespace).Items.Select(n => n.Metadata.Name);
            return Ok(pods);
        }
    }
}
