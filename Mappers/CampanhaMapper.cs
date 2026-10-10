using System.Text.Json;
using SOSComida.DTOs.Responses;
using SOSComida.Models;

namespace SOSComida.Mappers;

public static class CampanhaMapper
{
    public static CampanhaDto ToDto(this Campanha campanha)
    {
        List<string>? imagens = null;
        if (!string.IsNullOrEmpty(campanha.Imagens))
        {
            try
            {
                imagens = JsonSerializer.Deserialize<List<string>>(campanha.Imagens);
            }
            catch (JsonException) { }
        }

        return new CampanhaDto
        {
            Id = campanha.Id,
            Titulo = campanha.Titulo,
            Descricao = campanha.Descricao,
            ImagemUrl = campanha.ImagemUrl,
            Imagens = imagens,
            Localizacao = campanha.Localizacao,
            MetaArrecadacao = campanha.MetaArrecadacao,
            ValorArrecadado = campanha.ValorArrecadado,
            Progresso = campanha.MetaArrecadacao > 0
                ? (int)((campanha.ValorArrecadado / campanha.MetaArrecadacao) * 100)
                : 0,
            DataInicio = campanha.DataInicio,
            DataFim = campanha.DataFim,
            Status = campanha.Status,
            Ativa = campanha.Ativa,
            DataCriacao = campanha.DataCriacao,
            NomeUsuario = campanha.Usuario?.Nome ?? "",
            UsuarioId = campanha.UsuarioId,
            InstituicaoId = campanha.InstituicaoId,
            StatusDelegacao = campanha.StatusDelegacao,
            RegiaoId = campanha.RegiaoId,
            RegiaoNome = campanha.Regiao?.Nome
        };
    }
}
