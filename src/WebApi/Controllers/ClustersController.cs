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
    [Route("api/[controller]")]
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
                System.IO.File.Create(configPath);
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

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] string name, IFormFile file)
        {
            name = name.ToLower();
            if (_clusters.Any(n => n.Name.Equals(name)))
            {
                return BadRequest(new { Message = "Cluster is existed!" });
            }

            var certificatePath = Path.Combine(Program.ConfigDir, name);
            await using (var stream = System.IO.File.Create(certificatePath))
            {
                await file.CopyToAsync(stream);
            }

            _clusters.Add(new Cluster { Name = name });
            var json = JsonConvert.SerializeObject(_clusters);
            var configPath = Path.Combine(Program.ConfigDir, _configName);
            await System.IO.File.WriteAllTextAsync(configPath, json);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] string name)
        {
            name = name.ToLower();
            var cluster = _clusters.SingleOrDefault(n => n.Name == name);

            if (cluster == null)
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            _clusters.Remove(cluster);

            var certificatePath = Path.Combine(Program.ConfigDir, name);
            System.IO.File.Delete(certificatePath);

            var configPath = Path.Combine(Program.ConfigDir, _configName);
            var json = JsonConvert.SerializeObject(_clusters);
            await System.IO.File.WriteAllTextAsync(configPath, json);
            
            return NoContent();
        }
    }
    public class Cluster
    {
        public string Name { get; set; }
    }
}
