using System.Security.Claims;
using SOSComida.Models;

namespace SOSComida.Seguranca;

public static class PermissoesCampanha
{
    /// <summary>
    /// Criador da campanha, instituição que aceitou a delegação, moderador ou
    /// admin. Ninguém mais presta contas, edita ou vê os contatos dos
    /// voluntários de uma campanha que não é sua.
    /// </summary>
    public static bool PodeResponderPor(Campanha campanha, int usuarioId, bool ehModerador)
    {
        if (ehModerador) return true;
        if (campanha.UsuarioId == usuarioId) return true;

        return campanha.InstituicaoId == usuarioId
            && campanha.StatusDelegacao == StatusDelegacoes.Aceita;
    }

    public static bool PodeResponderPor(Campanha campanha, ClaimsPrincipal user) =>
        PodeResponderPor(campanha, user.Id(), user.EhModerador());
}
