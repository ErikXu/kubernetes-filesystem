using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi;

namespace Kubernetes.FileSystem.Controllers
{
    [Route("api/clusters")]
    [ApiController]
    public class ClustersController : ControllerBase
    {
        private readonly string _configName = "cluster.json";
        private List<Cluster> _clusters;

        public ClustersController()
        {
            _clusters = new List<Cluster>();

            if (!Directory.Exists(Program.ConfigDir))
            {
                Directory.CreateDirectory(Program.ConfigDir);
            }

            var configPath = Path.Combine(Program.ConfigDir, _configName);
            if (!System.IO.File.Exists(configPath))
            {
                var json = JsonConvert.SerializeObject(_clusters);
                System.IO.File.WriteAllText(configPath, json);
            }
            else
            {
                var json = System.IO.File.ReadAllText(configPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _clusters = JsonConvert.DeserializeObject<List<Cluster>>(json);
                }
            }
        }

        [HttpGet]
        public IActionResult List()
        {
            return Ok(_clusters);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var cluster = _clusters.SingleOrDefault(n=>n.Id == id);
            if (cluster == null)
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var certificatePath = Path.Combine(Program.ConfigDir, cluster.Name);
            var certificate = await System.IO.File.ReadAllTextAsync(certificatePath);
            cluster.Certificate = certificate;

            return Ok(cluster);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cluster cluster)
        {
            cluster.Name = cluster.Name.ToLower();
            if (_clusters.Any(n => n.Name.Equals(cluster.Name)))
            {
                return BadRequest(new { Message = "Cluster name is existed!" });
            }

            var certificatePath = Path.Combine(Program.ConfigDir, cluster.Name);
            await System.IO.File.WriteAllTextAsync(certificatePath, cluster.Certificate);

            cluster.Id = Guid.NewGuid().ToString();
            cluster.Certificate = string.Empty;
            _clusters.Add(cluster);
            var json = JsonConvert.SerializeObject(_clusters);
            var configPath = Path.Combine(Program.ConfigDir, _configName);
            await System.IO.File.WriteAllTextAsync(configPath, json);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Cluster form)
        {
            var cluster = _clusters.SingleOrDefault(n=>n.Id == id);
            if (cluster == null)
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var certificatePath = Path.Combine(Program.ConfigDir, cluster.Name);
            System.IO.File.Delete(certificatePath);

            cluster.Name = form.Name;
            certificatePath = Path.Combine(Program.ConfigDir, form.Name);
            await System.IO.File.WriteAllTextAsync(certificatePath, form.Certificate);

            var json = JsonConvert.SerializeObject(_clusters);
            var configPath = Path.Combine(Program.ConfigDir, _configName);
            await System.IO.File.WriteAllTextAsync(configPath, json);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var cluster = _clusters.SingleOrDefault(n=>n.Id == id);
            if (cluster == null)
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            _clusters.Remove(cluster);

            var certificatePath = Path.Combine(Program.ConfigDir, cluster.Name);
            System.IO.File.Delete(certificatePath);

            var configPath = Path.Combine(Program.ConfigDir, _configName);
            var json = JsonConvert.SerializeObject(_clusters);
            await System.IO.File.WriteAllTextAsync(configPath, json);

            return NoContent();
        }
    }
    public class Cluster
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Certificate { get; set; }
    }
}
