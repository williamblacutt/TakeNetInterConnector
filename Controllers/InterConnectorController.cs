using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TakeNetInterConnector.Controllers
{
    [Route("api/interconnector")]
    [ApiController]
    public class InterConnectorController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InterConnectorController> _logger;

        public InterConnectorController(IHttpClientFactory httpClientFactory, ILogger<InterConnectorController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpGet("repos")]
        public async Task<IActionResult> GetOldestCSharpRepositories()
        {
            try
            {
                string url = "https://api.github.com/orgs/takenet/repos?per_page=100";

                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TakeNet-InterConnector-Bot/1.0");

                _logger.LogInformation("Fazendo requisição para a API do GitHub...");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Erro ao acessar a API do GitHub. Código: {response.StatusCode}");
                    return StatusCode((int)response.StatusCode, "Erro ao buscar dados no GitHub.");
                }

                var content = await response.Content.ReadAsStringAsync();
                var repositories = JsonSerializer.Deserialize<List<InterConnectorRepo>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (repositories == null || !repositories.Any())
                {
                    _logger.LogWarning("Nenhum repositório C# encontrado.");
                    return NotFound("Nenhum repositório C# encontrado.");
                }

                var oldestRepos = repositories
                    .Where(r => !string.IsNullOrEmpty(r.Language) && r.Language.Equals("C#", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new
                    {
                        Name = r.FullName,
                        Description = r.Description ?? "Descrição não disponível",
                        AvatarUrl = "https://avatars.githubusercontent.com/u/4369522?v=4"
                    })
                    .ToList();

                _logger.LogInformation("Repositórios recuperados com sucesso.");
                return Ok(oldestRepos);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Erro na requisição HTTP: {ex.Message}");
                return StatusCode(500, "Erro ao acessar a API do GitHub.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro interno: {ex.Message}");
                return StatusCode(500, "Erro interno no servidor.");
            }
        }
    }

    public class InterConnectorRepo
    {
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}