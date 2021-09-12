using k8s;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var webSocket = await client.WebSocketNamespacedPodExecAsync(pod, @namespace, new string[] { "ls", "-Alh", "--time-style", "long-iso", "/" }, container).ConfigureAwait(false);
            var demux = new StreamDemuxer(webSocket);
            demux.Start();

            var buff = new byte[4096];
            var stream = demux.GetStream(1, 1);
            var read = stream.Read(buff, 0, 4096);
            var bytes = TrimEnd(buff);
            var text = System.Text.Encoding.Default.GetString(bytes).Trim();

            var files = ToFiles(text);
            return Ok(files);
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

            var path = Path.Combine(dir, file.FileName);
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

        private static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }

        private static List<FileItem> ToFiles(string text)
        {
            var files = new List<FileItem>();

            var lines = text.Split(Environment.NewLine);

            foreach (var line in lines)
            {
                if (line.StartsWith("total"))
                {
                    continue;
                }
                var trimLine = line.Trim();
                var array = trimLine.Split(" ").ToList().Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

                var file = new FileItem
                {
                    Permission = array[0],
                    Links = array[1],
                    Owner = array[2],
                    Group = array[3],
                    Size = array[4],
                    Date = array[5],
                    Time = array[6],
                    Name = array[7]
                };

                if (file.Permission.StartsWith("l"))
                {
                    file.Name = $"{array[7]} {array[8]} {array[9]}";
                }
                files.Add(file);
            }

            return files;
        }
    }

    public class FileItem
    {
        public string Permission { get; set; }
        public string Links { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Name { get; set; }
    }
}
