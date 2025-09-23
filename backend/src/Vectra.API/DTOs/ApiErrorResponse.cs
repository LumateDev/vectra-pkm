using System.Text.Json.Serialization;

namespace Vectra.API.Dtos
{
    /// <summary>
    /// Стандартизированный ответ об ошибке для всех эндпоинтов VectraAPI.
    /// Заменяет Microsoft.AspNetCore.Mvc.ProblemDetails и ValidationProblemDetails.
    /// Решает проблему o.o.codegen.utils.ModelUtils - Failed to get the schema name: null
    /// </summary>
    public sealed class ApiErrorResponse
    {
        /// <summary>
        /// URI, идентифицирующий проблему (опционально).
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>
        /// Краткий заголовок ошибки.
        /// </summary>
        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Title { get; set; }

        /// <summary>
        /// HTTP статус-код.
        /// </summary>
        [JsonPropertyName("status")]
        public int? Status { get; set; }

        /// <summary>
        /// Подробное описание ошибки.
        /// </summary>
        [JsonPropertyName("detail")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Detail { get; set; }

        /// <summary>
        /// URI экземпляра проблемы (опционально).
        /// </summary>
        [JsonPropertyName("instance")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Instance { get; set; }

        /// <summary>
        /// Машинно-читаемый код ошибки для фронтенда (например, "UserAlreadyExists").
        /// </summary>
        [JsonPropertyName("code")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Code { get; set; }

        /// <summary>
        /// Словарь ошибок валидации. Ключ - имя поля, значение - массив сообщений об ошибках.
        /// Заменяет функциональность ValidationProblemDetails.
        /// </summary>
        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}