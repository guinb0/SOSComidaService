using Microsoft.EntityFrameworkCore;
using SOSComida.Models;

namespace SOSComida.Data;

/// <summary>
/// Popula o banco local com um cenário de demonstração.
///
/// Só roda quando o banco está vazio — nunca sobrescreve o que você já criou
/// testando. Para começar do zero, apague o arquivo soscomida.db e suba de novo.
///
/// As senhas ficam em texto puro porque é assim que o AuthController compara
/// hoje (`u.Senha == request.Senha`). Não é o certo, e vale trocar por hash
/// antes de qualquer coisa ir para produção.
/// </summary>
public static class SeedInicial
{
    // Fotos hospedadas no Unsplash. O app precisa de rede de qualquer jeito
    // (é uma WebView carregando o site), então não muda o requisito.
    private const string FotoArroz = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=800&q=80";
    private const string FotoCesta = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=800&q=80";
    private const string FotoSopa = "https://images.unsplash.com/photo-1547592180-85f173990554?w=800&q=80";
    private const string FotoHorta = "https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?w=800&q=80";
    private const string FotoPao = "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=800&q=80";
    private const string FotoVoluntarios = "https://images.unsplash.com/photo-1593113646773-028c64a8f1b8?w=800&q=80";

    public static async Task ExecutarAsync(ApplicationDbContext ctx)
    {
        await ctx.Database.MigrateAsync();

        if (await ctx.Usuarios.AnyAsync()) return;

        var agora = DateTime.UtcNow;

        // --- Regiões administrativas do DF ---
        var regioes = new List<RegiaoAdministrativa>
        {
            new() { Nome = "Plano Piloto", Sigla = "RA-I", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
            new() { Nome = "Gama", Sigla = "RA-II", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
            new() { Nome = "Taguatinga", Sigla = "RA-III", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
            new() { Nome = "Ceilândia", Sigla = "RA-IX", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
            new() { Nome = "Samambaia", Sigla = "RA-XII", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
            new() { Nome = "Planaltina", Sigla = "RA-VI", Estado = "DF", Cidade = "Brasília", Ativa = true, DataCriacao = agora },
        };
        ctx.RegioesAdministrativas.AddRange(regioes);
        await ctx.SaveChangesAsync();

        RegiaoAdministrativa RA(string sigla) => regioes.First(r => r.Sigla == sigla);

        // --- Usuários ---
        var admin = new Usuario
        {
            Nome = "Guilherme Admin", Email = "guinb@soscomida.com", Senha = "admin123",
            Telefone = "(61) 99999-0003", Endereco = "SQN 210, Bloco A", Cidade = "Brasília",
            Cpf = "98765432100", Tipo = "admin", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-8),
        };
        var moderador = new Usuario
        {
            Nome = "Renata Moreira", Email = "moderador@teste.com", Senha = "teste123",
            Telefone = "(61) 99999-0004", Endereco = "QNM 34, Conjunto F", Cidade = "Brasília",
            Cpf = "11122233344", Tipo = "moderador", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-6),
        };
        var doador = new Usuario
        {
            Nome = "Marcos Vinícius", Email = "doador@teste.com", Senha = "teste123",
            Telefone = "(61) 99999-0001", Endereco = "SQS 308, Bloco C", Cidade = "Brasília",
            Cpf = "12345678901", Tipo = "usuario", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-4),
        };
        var doadora2 = new Usuario
        {
            Nome = "Juliana Prado", Email = "juliana@teste.com", Senha = "teste123",
            Telefone = "(61) 99999-0005", Endereco = "QI 25, Lago Sul", Cidade = "Brasília",
            Cpf = "22233344455", Tipo = "usuario", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-3),
        };
        var instituicao = new Usuario
        {
            Nome = "Casa de Apoio Esperança", Email = "instituicao@teste.com", Senha = "teste123",
            Telefone = "(61) 3333-0002", Endereco = "QNN 14, Área Especial", Cidade = "Brasília",
            Tipo = "instituicao", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-5),
            Cnpj = "12.345.678/0001-99", NomeInstituicao = "Casa de Apoio Esperança",
            DescricaoInstituicao = "Atendemos 120 famílias em Ceilândia com cestas básicas e refeições diárias desde 2014.",
            RegiaoAdministrativaId = RA("RA-IX").Id, StatusAprovacao = "aprovado",
            DataAprovacao = agora.AddMonths(-4), AprovadoPorId = null,
        };
        var instituicaoPendente = new Usuario
        {
            Nome = "Instituto Semear", Email = "semear@teste.com", Senha = "teste123",
            Telefone = "(61) 3333-0007", Endereco = "QR 415, Samambaia Norte", Cidade = "Brasília",
            Tipo = "instituicao", SenhaTemporaria = false, DataCriacao = agora.AddDays(-6),
            Cnpj = "98.765.432/0001-11", NomeInstituicao = "Instituto Semear",
            DescricaoInstituicao = "Horta comunitária e distribuição de hortaliças para famílias em Samambaia.",
            RegiaoAdministrativaId = RA("RA-XII").Id, StatusAprovacao = "pendente",
        };
        var familia1 = new Usuario
        {
            Nome = "Dona Marlene", Email = "marlene@teste.com", Senha = "teste123",
            Telefone = "(61) 98888-1111", Endereco = "QNP 22, Conjunto D", Cidade = "Brasília",
            Cpf = "33344455566", Tipo = "usuario", SenhaTemporaria = false, DataCriacao = agora.AddMonths(-2),
        };

        ctx.Usuarios.AddRange(admin, moderador, doador, doadora2, instituicao, instituicaoPendente, familia1);
        await ctx.SaveChangesAsync();

        instituicao.AprovadoPorId = admin.Id;
        await ctx.SaveChangesAsync();

        // --- Campanhas ---
        var campanhas = new List<Campanha>
        {
            new()
            {
                Titulo = "Cestas básicas para 40 famílias em Ceilândia",
                Descricao = "Arroz, feijão, óleo, açúcar, café e macarrão para 40 famílias cadastradas na Casa de Apoio Esperança. "
                          + "A entrega acontece no último sábado de cada mês, com participação dos voluntários da região.",
                ImagemUrl = FotoCesta, Localizacao = "Ceilândia - DF",
                MetaArrecadacao = 3000, ValorArrecadado = 1850,
                DataInicio = agora.AddDays(-24), DataFim = agora.AddDays(12),
                Status = "ativa", Ativa = true, DataCriacao = agora.AddDays(-24),
                UsuarioId = instituicao.Id, InstituicaoId = instituicao.Id,
                StatusDelegacao = "aceita", DataDelegacao = agora.AddDays(-23),
                RegiaoId = RA("RA-IX").Id,
            },
            new()
            {
                Titulo = "Sopão solidário nas noites frias",
                Descricao = "Distribuição de sopa quente e pão para pessoas em situação de rua na Rodoviária do Plano Piloto, "
                          + "três noites por semana durante o período de seca.",
                ImagemUrl = FotoSopa, Localizacao = "Plano Piloto - DF",
                MetaArrecadacao = 5000, ValorArrecadado = 4720,
                DataInicio = agora.AddDays(-40), DataFim = agora.AddDays(20),
                Status = "ativa", Ativa = true, DataCriacao = agora.AddDays(-40),
                UsuarioId = doador.Id, RegiaoId = RA("RA-I").Id,
            },
            new()
            {
                Titulo = "Arroz e feijão para o Natal de Samambaia",
                Descricao = "Campanha de fim de ano para garantir a ceia de 80 famílias em Samambaia Norte. "
                          + "Cada cesta leva arroz, feijão, farofa, frango e uma sobremesa.",
                ImagemUrl = FotoArroz, Localizacao = "Samambaia - DF",
                MetaArrecadacao = 8000, ValorArrecadado = 2340,
                DataInicio = agora.AddDays(-10), DataFim = agora.AddDays(45),
                Status = "ativa", Ativa = true, DataCriacao = agora.AddDays(-10),
                UsuarioId = doadora2.Id, RegiaoId = RA("RA-XII").Id,
            },
            new()
            {
                Titulo = "Horta comunitária do Gama",
                Descricao = "Compra de sementes, mudas e ferramentas para a horta que abastece a creche municipal "
                          + "e mais 25 famílias do entorno.",
                ImagemUrl = FotoHorta, Localizacao = "Gama - DF",
                MetaArrecadacao = 2500, ValorArrecadado = 2500,
                DataInicio = agora.AddMonths(-3), DataFim = agora.AddDays(-5),
                Status = "finalizada", Ativa = true, DataCriacao = agora.AddMonths(-3),
                UsuarioId = instituicao.Id, RegiaoId = RA("RA-II").Id,
            },
            new()
            {
                Titulo = "Pão e leite para a creche Pequeno Príncipe",
                Descricao = "Café da manhã diário para 60 crianças de 2 a 6 anos em Taguatinga Sul.",
                ImagemUrl = FotoPao, Localizacao = "Taguatinga - DF",
                MetaArrecadacao = 4000, ValorArrecadado = 980,
                DataInicio = agora.AddDays(-6), DataFim = agora.AddDays(54),
                Status = "ativa", Ativa = true, DataCriacao = agora.AddDays(-6),
                UsuarioId = doador.Id, RegiaoId = RA("RA-III").Id,
            },
            new()
            {
                Titulo = "Mutirão de voluntários em Planaltina",
                Descricao = "Montagem e entrega de 200 kits de higiene. Precisamos de voluntários para o sábado de montagem.",
                ImagemUrl = FotoVoluntarios, Localizacao = "Planaltina - DF",
                MetaArrecadacao = 1500, ValorArrecadado = 0,
                DataInicio = agora.AddDays(-2), DataFim = agora.AddDays(30),
                Status = "pendente", Ativa = true, DataCriacao = agora.AddDays(-2),
                UsuarioId = doadora2.Id, RegiaoId = RA("RA-VI").Id,
            },
        };
        ctx.Campanhas.AddRange(campanhas);
        await ctx.SaveChangesAsync();

        // --- Doações ---
        var ceilandia = campanhas[0];
        var sopao = campanhas[1];
        var natal = campanhas[2];
        var horta = campanhas[3];

        ctx.Doacoes.AddRange(
            new Doacao { Valor = 250, Tipo = "dinheiro", Descricao = "Contribuição mensal", Status = "confirmada", UsuarioId = doador.Id, CampanhaId = ceilandia.Id, DataDoacao = agora.AddDays(-20), DataConfirmacao = agora.AddDays(-20) },
            new Doacao { Valor = 500, Tipo = "dinheiro", Descricao = "Doação única", Status = "confirmada", UsuarioId = doadora2.Id, CampanhaId = ceilandia.Id, DataDoacao = agora.AddDays(-18), DataConfirmacao = agora.AddDays(-18) },
            new Doacao { Valor = 100, Tipo = "dinheiro", Status = "confirmada", UsuarioId = familia1.Id, CampanhaId = ceilandia.Id, DataDoacao = agora.AddDays(-9), DataConfirmacao = agora.AddDays(-9) },
            new Doacao { Valor = 1000, Tipo = "dinheiro", Descricao = "Doação da empresa", Status = "confirmada", UsuarioId = doadora2.Id, CampanhaId = sopao.Id, DataDoacao = agora.AddDays(-30), DataConfirmacao = agora.AddDays(-30) },
            new Doacao { Valor = 320, Tipo = "dinheiro", Status = "confirmada", UsuarioId = doador.Id, CampanhaId = sopao.Id, DataDoacao = agora.AddDays(-14), DataConfirmacao = agora.AddDays(-14) },
            new Doacao { Valor = 150, Tipo = "dinheiro", Status = "pendente", UsuarioId = doador.Id, CampanhaId = natal.Id, DataDoacao = agora.AddDays(-3) },
            new Doacao { Valor = 400, Tipo = "dinheiro", Status = "confirmada", UsuarioId = doador.Id, CampanhaId = horta.Id, DataDoacao = agora.AddMonths(-2), DataConfirmacao = agora.AddMonths(-2) }
        );

        // --- Pedidos de ajuda ---
        ctx.PedidosAjuda.AddRange(
            new PedidoAjuda
            {
                Titulo = "Família de 5 pessoas sem alimento há 3 dias",
                Descricao = "Perdi o emprego no mês passado e não consegui repor a despensa. Tenho três crianças em idade escolar.",
                Localizacao = "QNP 22, Ceilândia Sul", Urgencia = "critica", Status = "pendente",
                QuantidadePessoas = 5, Telefone = "(61) 98888-1111", TipoAjuda = "alimentos",
                UsuarioId = familia1.Id, RegiaoId = RA("RA-IX").Id, DataCriacao = agora.AddDays(-1),
            },
            new PedidoAjuda
            {
                Titulo = "Idosa sozinha precisa de cesta básica",
                Descricao = "Moro sozinha, recebo só o BPC e este mês o remédio consumiu quase tudo.",
                Localizacao = "QR 415, Samambaia Norte", Urgencia = "alta", Status = "em_andamento",
                QuantidadePessoas = 1, Telefone = "(61) 98888-2222", TipoAjuda = "alimentos",
                UsuarioId = familia1.Id, RegiaoId = RA("RA-XII").Id, DataCriacao = agora.AddDays(-4),
                AtendidoPorId = instituicao.Id,
            },
            new PedidoAjuda
            {
                Titulo = "Kits de higiene para abrigo temporário",
                Descricao = "Abrigo com 18 pessoas precisa de sabonete, pasta de dente e absorventes.",
                Localizacao = "Setor Central, Planaltina", Urgencia = "media", Status = "pendente",
                QuantidadePessoas = 18, Telefone = "(61) 98888-3333", TipoAjuda = "higiene",
                UsuarioId = familia1.Id, RegiaoId = RA("RA-VI").Id, DataCriacao = agora.AddDays(-7),
            },
            new PedidoAjuda
            {
                Titulo = "Leite em pó para bebê de 8 meses",
                Descricao = "Meu filho tem intolerância e a fórmula é cara. Preciso de ajuda por dois meses.",
                Localizacao = "QI 25, Taguatinga", Urgencia = "alta", Status = "atendido",
                QuantidadePessoas = 3, Telefone = "(61) 98888-4444", TipoAjuda = "alimentos",
                UsuarioId = familia1.Id, RegiaoId = RA("RA-III").Id, DataCriacao = agora.AddDays(-20),
                DataAtendimento = agora.AddDays(-16), AtendidoPorId = instituicao.Id,
            }
        );

        await ctx.SaveChangesAsync();

        // --- Notas fiscais (prestação de contas da campanha de Ceilândia) ---
        // Sem imagem real: a coluna guarda a URL do upload, e aqui não houve
        // upload. As telas mostram a moldura vazia, que é o comportamento certo
        // para um comprovante sem foto anexada.
        ctx.NotasFiscais.AddRange(
            new NotaFiscal
            {
                CampanhaId = ceilandia.Id, EnviadoPorId = instituicao.Id,
                ImagemUrl = "/uploads/notas/exemplo-atacadao.jpg",
                Descricao = "Arroz tipo 1, feijão carioca e óleo — 40 cestas",
                ValorTotal = 892.40m, NomeEmitente = "Atacadão Ceilândia", CnpjEmitente = "11.222.333/0001-81",
                NumeroNota = "004512", DataEmissao = agora.AddDays(-17), ConfiancaOcr = 91.2f,
                Revisado = true, DataEnvio = agora.AddDays(-17), DataRevisao = agora.AddDays(-17),
                TextoOcr = "ATACADAO CEILANDIA\nCNPJ: 11.222.333/0001-81\nARROZ TIPO 1 5KG 40 UN\nFEIJAO CARIOCA 1KG 40 UN\nOLEO DE SOJA 900ML 40 UN\nVALOR TOTAL R$ 892,40",
            },
            new NotaFiscal
            {
                CampanhaId = ceilandia.Id, EnviadoPorId = instituicao.Id,
                ImagemUrl = "/uploads/notas/exemplo-mercado.jpg",
                Descricao = "Café, açúcar e macarrão",
                ValorTotal = 417.60m, NomeEmitente = "Mercado Bom Preço", CnpjEmitente = "11.222.333/0001-81",
                NumeroNota = "008830", DataEmissao = agora.AddDays(-11), ConfiancaOcr = 78.4f,
                Revisado = true, DataEnvio = agora.AddDays(-11), DataRevisao = agora.AddDays(-10),
                TextoOcr = "MERCADO BOM PRECO\nCAFE 500G 40 UN\nACUCAR REFINADO 1KG 40 UN\nMACARRAO 500G 80 UN\nVALOR TOTAL R$ 417,60",
            },
            new NotaFiscal
            {
                CampanhaId = ceilandia.Id, EnviadoPorId = instituicao.Id,
                ImagemUrl = "/uploads/notas/exemplo-hortifruti.jpg",
                Descricao = null,
                ValorTotal = 156.00m, NomeEmitente = "Hortifruti da Feira", CnpjEmitente = null,
                NumeroNota = null, DataEmissao = agora.AddDays(-2), ConfiancaOcr = 54.8f,
                Revisado = false, DataEnvio = agora.AddDays(-2),
                TextoOcr = "HORTIFRUT1 DA FE1RA\nBATATA 20KG\nCEB0LA 10KG\nT0TAL 156,OO",
            }
        );

        await ctx.SaveChangesAsync();
    }
}
