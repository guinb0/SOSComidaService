using SOSComida.Services;

namespace SOSComida.Tests;

public class ExtratorNotaFiscalTests
{
    private const string Cupom = """
        MERCADO BOM PRECO LTDA
        CNPJ: 11.222.333/0001-81
        CUPOM FISCAL ELETRONICO
        ARROZ 5KG            2 x 25,90   51,80
        FEIJAO 1KG           3 x 8,50    25,50
        SUBTOTAL                         77,30
        QTD. TOTAL DE ITENS 5
        VALOR TOTAL R$                   77,30
        EMISSAO: 15/03/2026 10:42
        """;

    [Fact]
    public void Le_o_valor_total_ignorando_subtotal_e_itens()
    {
        Assert.Equal(77.30m, ExtratorNotaFiscal.Extrair(Cupom).ValorTotal);
    }

    [Fact]
    public void Aceita_cnpj_com_digito_verificador_valido()
    {
        Assert.Equal("11222333000181", ExtratorNotaFiscal.Extrair(Cupom).CnpjEmitente?.Replace(".", "").Replace("/", "").Replace("-", ""));
    }

    [Fact]
    public void Descarta_cnpj_com_digito_verificador_errado()
    {
        var campos = ExtratorNotaFiscal.Extrair("CNPJ: 11.222.333/0001-99\nTOTAL 10,00");
        Assert.Null(campos.CnpjEmitente);
    }

    [Fact]
    public void Le_a_data_de_emissao()
    {
        Assert.Equal(new DateTime(2026, 3, 15), ExtratorNotaFiscal.Extrair(Cupom).DataEmissao?.Date);
    }

    [Fact]
    public void Texto_vazio_nao_inventa_campos()
    {
        var campos = ExtratorNotaFiscal.Extrair("   ");
        Assert.Null(campos.ValorTotal);
        Assert.Null(campos.CnpjEmitente);
        Assert.Null(campos.DataEmissao);
    }
}
