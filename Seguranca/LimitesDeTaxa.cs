namespace SOSComida.Seguranca;

public static class LimitesDeTaxa
{
    /// <summary>
    /// Login, cadastro e recuperação de senha: 10 tentativas por minuto por IP
    /// (LimitesDeTaxa:AutenticacaoPorMinuto). Sem isso dava para testar senhas
    /// em loop contra qualquer e-mail.
    /// </summary>
    public const string Autenticacao = "autenticacao";
}
