using SOSComida.Models;
using SOSComida.DTOs.Requests;
using SOSComida.DTOs.Responses;

namespace SOSComida.Mappers;

public static class CampanhaMapper
{
    public static CampanhaDto ToDto(this Campanha campanha)
    {
        return new CampanhaDto
        {
            Id = campanha.Id,
            Titulo = campanha.Titulo,
            Descricao = campanha.Descricao,
            ImagemUrl = campanha.ImagemUrl,
            Localizacao = campanha.Localizacao,
            MetaArrecadacao = campanha.MetaArrecadacao,
            ValorArrecadado = campanha.ValorArrecadado,
            DataInicio = campanha.DataInicio,
            DataFim = campanha.DataFim,
            Status = campanha.Status,
            Ativa = campanha.Ativa,
            DataCriacao = campanha.DataCriacao,
            NomeUsuario = campanha.Usuario?.Nome ?? "",
            Progresso = campanha.MetaArrecadacao > 0 
                ? (int)((campanha.ValorArrecadado / campanha.MetaArrecadacao) * 100) 
                : 0
        };
    }

    public static Campanha ToEntity(this CreateCampanhaDto dto, int usuarioId)
    {
        return new Campanha
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            ImagemUrl = dto.ImagemUrl,
            Localizacao = dto.Localizacao ?? string.Empty,
            MetaArrecadacao = dto.MetaArrecadacao,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            UsuarioId = usuarioId,
            Status = "ativa"
        };
    }

    public static void UpdateEntity(this UpdateCampanhaDto dto, Campanha campanha)
    {
        campanha.Titulo = dto.Titulo;
        campanha.Descricao = dto.Descricao;
        campanha.ImagemUrl = dto.ImagemUrl;
        campanha.MetaArrecadacao = dto.MetaArrecadacao;
        campanha.DataInicio = dto.DataInicio;
        campanha.DataFim = dto.DataFim;
        campanha.Status = dto.Status;
        campanha.Ativa = dto.Ativa;
        campanha.DataAtualizacao = DateTime.Now;
    }
}
