using k8s;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WebApi;

namespace Kubernetes.FileSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string cluster, [FromQuery] string @namespace, [FromQuery] string pod, [FromQuery] string container)
        {
            var configPath = Path.Combine(Program.ConfigDir, cluster.ToLower());
            if (!System.IO.File.Exists(configPath))
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configPath);
            var client = new k8s.Kubernetes(config);

            var webSocket = await client.WebSocketNamespacedPodExecAsync(pod, @namespace, new string[] { "ls", "/" }, container).ConfigureAwait(false);
            var demux = new StreamDemuxer(webSocket);
            demux.Start();

            var buff = new byte[4096];
            var stream = demux.GetStream(1, 1);
            var read = stream.Read(buff, 0, 4096);
            var text = System.Text.Encoding.Default.GetString(buff).Trim();
            return Ok(text);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string cluster, [FromQuery] string @namespace, [FromQuery] string pod, [FromQuery] string container, [FromQuery] string dir, IFormFile file)
        {
            var configPath = Path.Combine(Program.ConfigDir, cluster.ToLower());
            if (!System.IO.File.Exists(configPath))
            {
                return BadRequest(new { Message = "Cluster is not existed!" });
            }

            var tmpPath = Path.Combine("/tmp", System.Guid.NewGuid().ToString());
            await using (var stream = System.IO.File.Create(tmpPath))
            {
                await file.CopyToAsync(stream);
            }

            var path  = Path.Combine(dir, file.FileName);
            var command = $"kubectl cp {tmpPath} {pod}:{path} -c {container} -n {@namespace} --kubeconfig {configPath}";
            var (code, message) = ExecuteCommand(command);

            System.IO.File.Delete(tmpPath);

            if (code == 0)
            {
                return Ok();
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = message });
        }

        private static (int, string) ExecuteCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            var message = process.StandardOutput.ReadToEnd();

            return (process.ExitCode, message);
        }
    }
}
