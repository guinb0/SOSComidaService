using SOSComida.Models;

namespace SOSComida.DTOs.Responses;

/// <summary>
/// O próprio usuário, como volta no login e na edição de perfil. Antes a API
/// devolvia a entidade <see cref="Usuario"/> inteira — com a senha junto, que
/// o front ainda guardava no localStorage.
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
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public string? FotoUrl { get; set; }
    public string? Cnpj { get; set; }
    public int? RegiaoAdministrativaId { get; set; }
    public string? NomeInstituicao { get; set; }
    public string? DescricaoInstituicao { get; set; }
    public string StatusAprovacao { get; set; } = string.Empty;

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
        DataCriacao = u.DataCriacao,
        FotoUrl = u.FotoUrl,
        Cnpj = u.Cnpj,
        RegiaoAdministrativaId = u.RegiaoAdministrativaId,
        NomeInstituicao = u.NomeInstituicao,
        DescricaoInstituicao = u.DescricaoInstituicao,
        StatusAprovacao = u.StatusAprovacao
    };
}
