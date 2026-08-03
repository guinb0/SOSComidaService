namespace SOSComida.Models;

/// <summary>
/// Comprovante de GASTO de uma campanha: a nota fiscal que o responsável
/// (criador ou instituição delegada) envia para provar em que o dinheiro
/// arrecadado foi usado.
///
/// Não confundir com <see cref="Doacao.ComprovanteUrl"/>, que é a prova do
/// lado de quem doou. Aqui é o outro lado: a prestação de contas.
///
/// Os campos extraídos pelo OCR são todos anuláveis de propósito. Foto de
/// cupom térmico amassado falha com frequência, e um valor errado numa tela
/// de prestação de contas é pior que um campo vazio — por isso nada é
/// considerado válido até um humano revisar (<see cref="Revisado"/>).
/// </summary>
public class NotaFiscal
{
    public int Id { get; set; }

    /// <summary>Imagem original. É ela a prova; o OCR é só conveniência.</summary>
    public string ImagemUrl { get; set; } = string.Empty;

    /// <summary>Texto bruto devolvido pelo OCR, guardado para auditoria.</summary>
    public string? TextoOcr { get; set; }

    /// <summary>O que foi comprado, escrito pelo responsável.</summary>
    public string? Descricao { get; set; }

    // --- Campos extraídos do texto, corrigíveis à mão ---
    public decimal? ValorTotal { get; set; }
    public string? CnpjEmitente { get; set; }
    public string? NomeEmitente { get; set; }
    public string? NumeroNota { get; set; }
    public DateTime? DataEmissao { get; set; }

    /// <summary>
    /// Chave de acesso da NF-e/NFC-e (44 dígitos). Quando o OCR consegue ler,
    /// é a informação mais forte aqui: dá para conferir a nota na SEFAZ.
    /// </summary>
    public string? ChaveAcesso { get; set; }

    /// <summary>
    /// Confiança média do OCR (0-100) na leitura. Serve para a tela destacar
    /// notas que provavelmente precisam de conferência.
    /// </summary>
    public float? ConfiancaOcr { get; set; }

    /// <summary>
    /// Um humano conferiu os campos contra a imagem. Enquanto for false, a
    /// nota entra na soma como "não confirmada".
    /// </summary>
    public bool Revisado { get; set; } = false;

    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
    public DateTime? DataRevisao { get; set; }

    // Campanha cujo dinheiro foi gasto
    public int CampanhaId { get; set; }
    public Campanha Campanha { get; set; } = null!;

    // Quem enviou (criador da campanha, instituição delegada ou moderador)
    public int EnviadoPorId { get; set; }
    public Usuario EnviadoPor { get; set; } = null!;
}
