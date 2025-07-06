using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ProcessarMatriculaApp
{
    public class ProcessarMatricula
    {
        private readonly ILogger _logger;

        public ProcessarMatricula(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessarMatricula>();
        }

        [Function("ProcessarMatricula")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "options")] HttpRequestData req)
        {
            _logger.LogInformation("Processando nova requisição");

            // Configurar CORS para todas as respostas
            var corsHeaders = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "POST, OPTIONS" },
                { "Access-Control-Allow-Headers", "Content-Type, Accept, Origin, X-Requested-With" },
                { "Access-Control-Max-Age", "3600" }
            };

            // Lidar com requisições OPTIONS (CORS preflight)
            if (req.Method.ToUpper() == "OPTIONS")
            {
                _logger.LogInformation("Processando requisição OPTIONS (CORS preflight)");
                var optionsResponse = req.CreateResponse(HttpStatusCode.OK);
                
                foreach (var header in corsHeaders)
                {
                    optionsResponse.Headers.Add(header.Key, header.Value);
                }
                
                return optionsResponse;
            }

            // Processar requisições POST
            if (req.Method.ToUpper() != "POST")
            {
                _logger.LogWarning("Método não permitido: {Method}", req.Method);
                var methodResponse = req.CreateResponse(HttpStatusCode.MethodNotAllowed);
                
                foreach (var header in corsHeaders)
                {
                    methodResponse.Headers.Add(header.Key, header.Value);
                }
                
                await methodResponse.WriteStringAsync(JsonSerializer.Serialize(new { erro = "Método não permitido. Use POST." }));
                return methodResponse;
            }

            try
            {
                // Ler dados da requisição
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Corpo da requisição: {RequestBody}", requestBody);

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    throw new Exception("Corpo da requisição está vazio");
                }

                var data = JsonSerializer.Deserialize<MatriculaRequest>(requestBody);
                
                if (data == null)
                {
                    throw new Exception("Não foi possível deserializar os dados da requisição");
                }

                _logger.LogInformation("Dados recebidos: Nome={Nome}, Email={Email}, Curso={Curso}", 
                    data.Nome, data.Email, data.Curso);

                // Validar dados obrigatórios
                if (string.IsNullOrWhiteSpace(data.Nome) || 
                    string.IsNullOrWhiteSpace(data.Email) || 
                    string.IsNullOrWhiteSpace(data.Curso))
                {
                    throw new Exception("Campos obrigatórios não preenchidos: Nome, Email e Curso são obrigatórios");
                }

                // Simular processamento assíncrono
                await Task.Delay(1000);

                // Simular validação de documentos
                bool documentosValidos = new Random().NextDouble() > 0.1; // 90% de chance de sucesso
                
                // Simular validação de pagamento
                bool pagamentoValido = new Random().NextDouble() > 0.05; // 95% de chance de sucesso

                // Gerar protocolo único
                string protocolo = $"PUCPR-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

                // Simular inserção no banco de dados
                await SimularInsercaoBanco(data, protocolo);

                // Simular envio de email
                await SimularEnvioEmail(data.Email, protocolo);

                var resultado = new MatriculaResponse
                {
                    Protocolo = protocolo,
                    Status = "Processado com Sucesso",
                    DocumentosValidos = documentosValidos,
                    PagamentoValido = pagamentoValido,
                    ProximaEtapa = documentosValidos && pagamentoValido ? 
                        "Geração de contrato" : "Aguardando correções",
                    DataProcessamento = DateTime.Now,
                    Curso = data.Curso,
                    ValorCurso = ObterValorCurso(data.Curso),
                    TempoProcessamento = "1 segundo"
                };

                _logger.LogInformation("Matrícula processada com sucesso: Protocolo={Protocolo}", resultado.Protocolo);

                // Criar resposta de sucesso
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                
                foreach (var header in corsHeaders)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
                
                await response.WriteStringAsync(JsonSerializer.Serialize(resultado));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar matrícula: {Message}", ex.Message);
                
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                
                foreach (var header in corsHeaders)
                {
                    errorResponse.Headers.Add(header.Key, header.Value);
                }
                
                var error = new
                {
                    erro = "Erro ao processar matrícula",
                    mensagem = ex.Message,
                    timestamp = DateTime.Now,
                    detalhes = "Verifique se todos os campos obrigatórios foram preenchidos"
                };
                
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(error));
                return errorResponse;
            }
        }

        // Função auxiliar para simular inserção no banco
        private async Task SimularInsercaoBanco(MatriculaRequest data, string protocolo)
        {
            _logger.LogInformation("Simulando inserção no banco de dados para protocolo: {Protocolo}", protocolo);
            
            // Simular delay de banco de dados
            await Task.Delay(300);
            
            // Aqui você faria a conexão real com o SQL Database
            // usando Entity Framework ou SqlConnection
            
            _logger.LogInformation("Dados inseridos no banco com sucesso");
        }

        // Função auxiliar para simular envio de email
        private async Task SimularEnvioEmail(string email, string protocolo)
        {
            _logger.LogInformation("Simulando envio de email para: {Email}", email);
            
            // Simular delay de envio
            await Task.Delay(200);
            
            _logger.LogInformation("Email enviado com sucesso");
        }

        // Função auxiliar para obter valor do curso
        private string ObterValorCurso(string cursoId)
        {
            var valores = new Dictionary<string, string>
            {
                { "1", "R$ 850,00" },
                { "2", "R$ 750,00" },
                { "3", "R$ 680,00" }
            };
            
            return valores.ContainsKey(cursoId) ? valores[cursoId] : "R$ 0,00";
        }
    }

    // Classes para deserialização
    public class MatriculaRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
    }

    public class MatriculaResponse
    {
        public string Protocolo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool DocumentosValidos { get; set; }
        public bool PagamentoValido { get; set; }
        public string ProximaEtapa { get; set; } = string.Empty;
        public DateTime DataProcessamento { get; set; }
        public string Curso { get; set; } = string.Empty;
        public string ValorCurso { get; set; } = string.Empty;
        public string TempoProcessamento { get; set; } = string.Empty;
    }
}