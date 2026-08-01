using SOSComida.Models;

namespace SOSComida.DTOs.Responses;

/// <summary>
/// Usuário como ele sai da API.
///
/// Existe por um motivo específico: o login devolvia a entidade <see cref="Usuario"/>
/// inteira, e nela vai o campo <c>Senha</c> — ou seja, toda resposta de login
/// mandava a senha de volta pela rede e ela ia parar no localStorage do navegador.
/// Este DTO simplesmente não tem esse campo.
/// </summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string Tipo { get; set; } = "usuario";
    public bool SenhaTemporaria { get; set; }
    public DateTime DataCriacao { get; set; }
    public string? FotoUrl { get; set; }

    // Instituição
    public string? Cnpj { get; set; }
    public int? RegiaoAdministrativaId { get; set; }
    public string? NomeInstituicao { get; set; }
    public string? DescricaoInstituicao { get; set; }
    public string StatusAprovacao { get; set; } = "nao_aplicavel";
    public DateTime? DataAprovacao { get; set; }
    public string? MotivoRejeicao { get; set; }

    public static UsuarioDto De(Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        Telefone = u.Telefone,
        Endereco = u.Endereco,
        Cidade = u.Cidade,
        Cpf = u.Cpf,
        Tipo = u.Tipo,
        SenhaTemporaria = u.SenhaTemporaria,
        DataCriacao = u.DataCriacao,
        FotoUrl = u.FotoUrl,
        Cnpj = u.Cnpj,
        RegiaoAdministrativaId = u.RegiaoAdministrativaId,
        NomeInstituicao = u.NomeInstituicao,
        DescricaoInstituicao = u.DescricaoInstituicao,
        StatusAprovacao = u.StatusAprovacao,
        DataAprovacao = u.DataAprovacao,
        MotivoRejeicao = u.MotivoRejeicao,
    };
}
