namespace SOSComida.Models;

// Valores gravados nas colunas de texto de status/tipo. Ficam em constantes
// porque um erro de digitação ("ativo" no lugar de "ativa") compilava e
// simplesmente fazia a consulta não achar nada. O front compara com os mesmos
// textos, então mudar um valor aqui exige mudar lá também.

public static class StatusCampanhas
{
    public const string Pendente = "pendente";
    public const string Ativa = "ativa";
    public const string Pausada = "pausada";
    public const string Rejeitada = "rejeitada";
    public const string Delegada = "delegada";
    public const string Finalizada = "finalizada";
}

public static class StatusDelegacoes
{
    public const string Pendente = "pendente";
    public const string Aceita = "aceita";
    public const string Recusada = "recusada";
}

public static class StatusParticipantes
{
    public const string Ativo = "ativo";
    public const string Removido = "removido";
}

public static class StatusPedidos
{
    public const string Pendente = "pendente";
    public const string EmAndamento = "em_andamento";
    public const string Atendido = "atendido";
    public const string Cancelado = "cancelado";
}

public static class UrgenciasPedido
{
    public const string Baixa = "baixa";
    public const string Media = "media";
    public const string Alta = "alta";
    public const string Critica = "critica";
}

public static class StatusAprovacoes
{
    public const string NaoAplicavel = "nao_aplicavel";
    public const string Pendente = "pendente";
    public const string Aprovado = "aprovado";
    public const string Rejeitado = "rejeitado";
}

public static class TiposAviso
{
    public const string Informativo = "informativo";
    public const string Advertencia = "advertencia";
    public const string Urgente = "urgente";
}

public static class TiposNotificacao
{
    public const string DelegacaoCampanha = "delegacao_campanha";
    public const string RespostaDelegacao = "resposta_delegacao";
}

public static class StatusDoacoes
{
    public const string Pendente = "pendente";
    public const string Confirmada = "confirmada";
    public const string Cancelada = "cancelada";
}
