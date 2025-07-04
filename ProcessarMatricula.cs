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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processando nova matrícula");

            try
            {
                // Ler dados da requisição
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<MatriculaRequest>(requestBody);

                _logger.LogInformation("Dados recebidos: {Data}", JsonSerializer.Serialize(data));

                // Simular processamento assíncrono
                await Task.Delay(2000);

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
                    Status = "Processado",
                    DocumentosValidos = documentosValidos,
                    PagamentoValido = pagamentoValido,
                    ProximaEtapa = documentosValidos && pagamentoValido ? 
                        "Geração de contrato" : "Aguardando correções",
                    DataProcessamento = DateTime.Now,
                    Curso = data.Curso,
                    ValorCurso = ObterValorCurso(data.Curso),
                    TempoProcessamento = "2 segundos"
                };

                _logger.LogInformation("Matrícula processada com sucesso: {Resultado}", JsonSerializer.Serialize(resultado));

                // Criar resposta
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                
                await response.WriteStringAsync(JsonSerializer.Serialize(resultado));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar matrícula");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                errorResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                
                var error = new
                {
                    erro = "Erro interno do servidor",
                    mensagem = "Tente novamente em alguns minutos",
                    timestamp = DateTime.Now
                };
                
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(error));
                return errorResponse;
            }
        }

        // Função auxiliar para simular inserção no banco
        private async Task SimularInsercaoBanco(MatriculaRequest data, string protocolo)
        {
            _logger.LogInformation("Simulando inserção no banco de dados...");
            
            // Simular delay de banco de dados
            await Task.Delay(500);
            
            // Aqui você faria a conexão real com o SQL Database
            // usando Entity Framework ou SqlConnection
            
            _logger.LogInformation("Dados inseridos no banco com protocolo: {Protocolo}", protocolo);
        }

        // Função auxiliar para simular envio de email
        private async Task SimularEnvioEmail(string email, string protocolo)
        {
            _logger.LogInformation("Simulando envio de email para: {Email}", email);
            
            // Simular delay de envio
            await Task.Delay(300);
            
            _logger.LogInformation("Email enviado com sucesso para: {Email}", email);
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
        public string Email { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        // Adicione outros campos conforme necessário
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