using SOSComida.Data;
using SOSComida.Models;

namespace SOSComida.Services;

/// <summary>
/// Tipos gravados em <c>AcaoModeracao.TipoAcao</c>. O histórico da tela de
/// moderadores agrupa por eles.
/// </summary>
public static class AcoesAuditadas
{
    public const string ModeradorCriado = "moderador_criado";
    public const string ModeradorEditado = "moderador_editado";
    public const string ModeradorAtivado = "moderador_ativado";
    public const string ModeradorDesativado = "moderador_desativado";
    public const string ModeradorRemovido = "moderador_removido";
    public const string RegioesAtribuidas = "regioes_atribuidas";

    public const string CampanhaAprovada = "campanha_aprovada";
    public const string CampanhaPausada = "campanha_pausada";
    public const string CampanhaRejeitada = "campanha_rejeitada";
    public const string CampanhaFinalizada = "campanha_finalizada";
    public const string CampanhaEditada = "campanha_editada";
    public const string CampanhaDelegada = "campanha_delegada";

    public const string InstituicaoAprovada = "instituicao_aprovada";
    public const string InstituicaoRejeitada = "instituicao_rejeitada";

    public const string ParticipanteRemovido = "participante_removido";
    public const string Advertencia = "advertencia";
    public const string MensagemApagada = "mensagem_apagada";
    public const string AvisoEnviado = "aviso_enviado";
}

public static class ItensAuditados
{
    public const string Campanha = "campanha";
    public const string Usuario = "usuario";
    public const string Mensagem = "mensagem";
}

public static class AuditoriaModeracao
{
    /// <summary>
    /// Anota a ação no contexto; ela é gravada junto com a mudança, no mesmo
    /// <c>SaveChanges</c> — ou as duas entram, ou nenhuma.
    /// </summary>
    public static void Registrar(
        this ApplicationDbContext context,
        int moderadorId,
        string acao,
        string tipoItem,
        int itemId,
        string descricao,
        string? observacoes = null)
    {
        context.AcoesModeracao.Add(new AcaoModeracao
        {
            ModeradorId = moderadorId,
            TipoAcao = acao,
            TipoItem = tipoItem,
            ItemId = itemId,
            Motivo = descricao.Length > 500 ? descricao[..500] : descricao,
            Observacoes = observacoes,
            DataAcao = DateTime.UtcNow
        });
    }
}
