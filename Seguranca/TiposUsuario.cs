namespace SOSComida.Seguranca;

/// <summary>
/// Valores de <c>Usuario.Tipo</c>. Viram a claim "role" do token, então
/// <c>[Authorize(Roles = ...)]</c> e as policies dependem destes textos exatos.
/// </summary>
public static class TiposUsuario
{
    public const string Usuario = "usuario";
    public const string Instituicao = "instituicao";
    public const string Moderador = "moderador";
    public const string Admin = "admin";
}

public static class Politicas
{
    /// <summary>Moderador ou admin.</summary>
    public const string Moderacao = "Moderacao";

    /// <summary>Só admin: gestão de regiões e de moderadores.</summary>
    public const string Admin = "Admin";
}
