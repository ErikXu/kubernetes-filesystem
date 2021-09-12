using k8s;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using WebApi;

namespace Kubernetes.FileSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NamespacesController : ControllerBase
    {
        [HttpGet]
        public IActionResult List([FromQuery] string cluster)
        {
            var configPath = Path.Combine(Program.ConfigDir, cluster.ToLower());
            if (!System.IO.File.Exists(configPath))
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configPath);
            var client = new k8s.Kubernetes(config);
            var namespaces = client.ListNamespace().Items.Select(n => n.Metadata.Name);
            return Ok(namespaces);
        }
    }
}
