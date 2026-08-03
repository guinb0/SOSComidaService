using System.Globalization;
using System.Text.RegularExpressions;

namespace SOSComida.Services;

/// <summary>
/// Campos que conseguimos deduzir do texto do OCR. Tudo anulável: quando não
/// dá para afirmar, o campo volta vazio em vez de chutar. Numa tela de
/// prestação de contas, um valor inventado é pior que um campo em branco.
/// </summary>
public record CamposNotaFiscal(
    decimal? ValorTotal,
    string? CnpjEmitente,
    string? NomeEmitente,
    string? NumeroNota,
    DateTime? DataEmissao,
    string? ChaveAcesso);

/// <summary>
/// Interpreta o texto cru do OCR de uma nota fiscal / cupom fiscal brasileiro.
///
/// Trabalha por heurística sobre um texto que quase sempre vem sujo, então
/// cada campo passa por uma validação própria antes de ser aceito: CNPJ e
/// chave de acesso são conferidos pelo dígito verificador, o que descarta boa
/// parte das leituras erradas (o OCR troca 0 por O, 1 por I, 5 por S).
/// </summary>
public static partial class ExtratorNotaFiscal
{
    public static CamposNotaFiscal Extrair(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return new CamposNotaFiscal(null, null, null, null, null, null);

        var linhas = texto.Split('\n')
                          .Select(l => l.Trim())
                          .Where(l => l.Length > 0)
                          .ToArray();

        return new CamposNotaFiscal(
            ValorTotal: ExtrairValorTotal(linhas),
            CnpjEmitente: ExtrairCnpj(texto),
            NomeEmitente: ExtrairNomeEmitente(linhas),
            NumeroNota: ExtrairNumeroNota(texto),
            DataEmissao: ExtrairData(texto),
            ChaveAcesso: ExtrairChaveAcesso(texto));
    }

    // --- Valor total ---------------------------------------------------

    // "TOTAL", "VALOR TOTAL", "VL TOTAL", "TOTAL A PAGAR", "VALOR A PAGAR".
    // Exclui "SUBTOTAL" e "TOTAL DE ITENS", que aparecem antes e enganariam.
    [GeneratedRegex(@"(?<!SUB)(?:VALOR\s+)?TOTAL(?:\s+A\s+PAGAR)?|VL\s*TOTAL|VALOR\s+A\s+PAGAR",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexRotuloTotal();

    [GeneratedRegex(@"TOTAL\s+DE\s+ITENS|QTD\.?\s*TOTAL|TOTAL\s+DE\s+PRODUTOS",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexTotalDeItens();

    // 1.234,56 | 1234,56 | 1234.56
    [GeneratedRegex(@"\d{1,3}(?:\.\d{3})*,\d{2}|\d+,\d{2}|\d+\.\d{2}")]
    private static partial Regex RegexMoeda();

    private static decimal? ExtrairValorTotal(string[] linhas)
    {
        // Varre de baixo para cima: em cupom fiscal o total fica no fim, e as
        // linhas de item acima têm valores que não queremos pegar.
        for (var i = linhas.Length - 1; i >= 0; i--)
        {
            var linha = linhas[i];

            if (RegexTotalDeItens().IsMatch(linha)) continue;
            if (!RegexRotuloTotal().IsMatch(linha)) continue;

            // O valor costuma estar na mesma linha do rótulo; se não estiver,
            // olha a linha seguinte, que é o outro layout comum.
            var valor = MaiorMoedaDaLinha(linha);
            if (valor is null && i + 1 < linhas.Length)
                valor = MaiorMoedaDaLinha(linhas[i + 1]);

            if (valor is not null) return valor;
        }

        return null;
    }

    private static decimal? MaiorMoedaDaLinha(string linha)
    {
        decimal? maior = null;

        foreach (Match m in RegexMoeda().Matches(linha))
        {
            if (!TentarLerMoeda(m.Value, out var v)) continue;
            // Valores absurdos quase sempre são leitura errada de código de
            // barras ou de chave de acesso partida ao meio.
            if (v <= 0 || v > 1_000_000m) continue;
            if (maior is null || v > maior) maior = v;
        }

        return maior;
    }

    private static bool TentarLerMoeda(string bruto, out decimal valor)
    {
        // "1.234,56" -> "1234.56"
        var normalizado = bruto.Contains(',')
            ? bruto.Replace(".", "").Replace(',', '.')
            : bruto;

        return decimal.TryParse(normalizado, NumberStyles.Number,
            CultureInfo.InvariantCulture, out valor);
    }

    // --- CNPJ ----------------------------------------------------------

    [GeneratedRegex(@"\d{2}\D?\d{3}\D?\d{3}\D?\d{4}\D?\d{2}")]
    private static partial Regex RegexCnpj();

    private static string? ExtrairCnpj(string texto)
    {
        foreach (Match m in RegexCnpj().Matches(texto))
        {
            var digitos = SomenteDigitos(m.Value);
            if (digitos.Length == 14 && CnpjValido(digitos))
                return Formatar(digitos, @"$1.$2.$3/$4-$5",
                    @"^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$");
        }

        return null;
    }

    /// <summary>Dígito verificador do CNPJ (módulo 11).</summary>
    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false; // 00000000000000 etc.

        int[] peso1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] peso2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int Digito(string parcial, int[] pesos)
        {
            var soma = parcial.Select((c, i) => (c - '0') * pesos[i]).Sum();
            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        var d1 = Digito(cnpj[..12], peso1);
        var d2 = Digito(cnpj[..13], peso2);

        return cnpj[12] - '0' == d1 && cnpj[13] - '0' == d2;
    }

    // --- Chave de acesso (44 dígitos) ----------------------------------

    // Vem impressa em grupos de 4 separados por espaço, ou tudo junto.
    [GeneratedRegex(@"(?:\d[\s.]?){43}\d")]
    private static partial Regex RegexChave();

    private static string? ExtrairChaveAcesso(string texto)
    {
        foreach (Match m in RegexChave().Matches(texto))
        {
            var digitos = SomenteDigitos(m.Value);
            if (digitos.Length == 44 && ChaveValida(digitos))
                return digitos;
        }

        return null;
    }

    /// <summary>
    /// Dígito verificador da chave (módulo 11, pesos 2..9 da direita para a
    /// esquerda). Só aceitamos a chave se ele bater — assim uma chave meio
    /// lida não vira uma "prova" falsa que ninguém consegue conferir na SEFAZ.
    /// </summary>
    private static bool ChaveValida(string chave)
    {
        var soma = 0;
        var peso = 2;

        for (var i = 42; i >= 0; i--)
        {
            soma += (chave[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        var dv = resto is 0 or 1 ? 0 : 11 - resto;

        return chave[43] - '0' == dv;
    }

    // --- Número da nota ------------------------------------------------

    [GeneratedRegex(@"(?:N[FÚU]?\.?\s*F?[-\s]?e?|N[ºo°]\.?|N[UÚ]MERO|EXTRATO\s+N[ºo°]?)\s*:?\s*(\d{3,9})",
        RegexOptions.IgnoreCase)]
    private static partial Regex RegexNumeroNota();

    private static string? ExtrairNumeroNota(string texto)
    {
        var m = RegexNumeroNota().Match(texto);
        return m.Success ? m.Groups[1].Value.TrimStart('0').PadLeft(1, '0') : null;
    }

    // --- Data de emissão -----------------------------------------------

    [GeneratedRegex(@"(\d{2})[/\-.](\d{2})[/\-.](\d{4}|\d{2})(?:\s+(\d{2}):(\d{2}))?")]
    private static partial Regex RegexData();

    private static DateTime? ExtrairData(string texto)
    {
        foreach (Match m in RegexData().Matches(texto))
        {
            var dia = int.Parse(m.Groups[1].Value);
            var mes = int.Parse(m.Groups[2].Value);
            var anoBruto = int.Parse(m.Groups[3].Value);
            var ano = m.Groups[3].Value.Length == 2 ? 2000 + anoBruto : anoBruto;

            if (dia is < 1 or > 31 || mes is < 1 or > 12) continue;
            if (ano < 2000 || ano > DateTime.UtcNow.Year + 1) continue;

            var hora = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
            var minuto = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
            if (hora > 23 || minuto > 59) { hora = 0; minuto = 0; }

            try
            {
                // UTC porque a coluna é timestamptz e o Npgsql recusa Unspecified.
                return new DateTime(ano, mes, dia, hora, minuto, 0, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Dia inválido para o mês (31/02). Segue para a próxima data.
            }
        }

        return null;
    }

    // --- Nome do emitente ----------------------------------------------

    private static string? ExtrairNomeEmitente(string[] linhas)
    {
        // O emitente é a primeira coisa impressa. Pulamos linhas que são só
        // números/símbolos (ruído de borda) ou que já são o CNPJ.
        foreach (var linha in linhas.Take(5))
        {
            var letras = linha.Count(char.IsLetter);
            if (letras < 4) continue;
            if (RegexCnpj().IsMatch(linha)) continue;
            if (linha.Contains("CUPOM", StringComparison.OrdinalIgnoreCase)) continue;
            if (linha.Contains("DANFE", StringComparison.OrdinalIgnoreCase)) continue;

            return linha.Length > 300 ? linha[..300] : linha;
        }

        return null;
    }

    // --- Utilitários ---------------------------------------------------

    private static string SomenteDigitos(string s) =>
        new(s.Where(char.IsDigit).ToArray());

    private static string Formatar(string valor, string substituicao, string padrao) =>
        Regex.Replace(valor, padrao, substituicao);
}
