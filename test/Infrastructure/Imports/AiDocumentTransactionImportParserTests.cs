using System.IO.Compression;
using System.Text;
using Application.Abstractions.AI;
using Application.Configuration.AI;
using Domain.Exceptions;
using Infrastructure.Imports;
using Infrastructure.Imports.Parsers;
using Infrastructure.Imports.Spreadsheets;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.AI;
using Shared.Localization;
using Shouldly;

namespace Tests.Infrastructure.Imports;

public class AiDocumentTransactionImportParserTests
{
    private readonly Mock<IAiDocumentReader> _reader = new();
    private readonly Mock<ITransactionExtractionService> _extraction = new();

    private static readonly ExtractedTransaction Sample = new(new DateTime(2026, 10, 15), "uber", 23m, "E");

    private AiDocumentTransactionImportParser Build() =>
        new(_reader.Object, _extraction.Object, NullLogger<AiDocumentTransactionImportParser>.Instance);

    private static Stream AnyStream() => new MemoryStream(new byte[] { 1 });

    [Theory]
    [InlineData("a.pdf", true)]
    [InlineData("A.PNG", true)]
    [InlineData("a.jpg", true)]
    [InlineData("a.jpeg", true)]
    [InlineData("a.xlsx", true)]
    [InlineData("a.xls", true)]
    [InlineData("a.ofx", false)]
    [InlineData("a.csv", false)]
    [InlineData("a.zip", false)]
    public void CanParse_reconhece_apenas_os_formatos_de_ia(string fileName, bool expected)
    {
        Build().CanParse(fileName).ShouldBe(expected);
    }

    [Fact]
    public void E_um_parser_baseado_em_ia()
    {
        Build().IsAiBased.ShouldBeTrue();
    }

    [Fact]
    public async Task Documento_com_anexo_e_enviado_como_arquivo_com_a_funcionalidade_correta()
    {
        var attachment = new AiAttachment(new byte[] { 1 }, "application/pdf", "document.pdf");
        string? feature = null;

        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(attachment, Array.Empty<string>()));
        _extraction
            .Setup(e => e.ExtractFromFileAsync(7, attachment, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<int, AiAttachment, string, CancellationToken>((_, _, f, _) => feature = f)
            .ReturnsAsync(new[] { Sample });

        var result = (await Build().ParseAsync(AnyStream(), 7)).ToList();

        result.Single().Description.ShouldBe("uber");
        feature.ShouldBe(AiFeatures.DocumentTransactionImport);
    }

    [Fact]
    public async Task Documento_em_tabela_e_enviado_bloco_a_bloco()
    {
        var tables = new List<string>();

        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(null, new[] { "b1", "b2", "b3" }));
        _extraction
            .Setup(e => e.ExtractFromTableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<int, string, string, CancellationToken>((_, t, _, _) => tables.Add(t))
            .ReturnsAsync(new[] { Sample });

        var result = (await Build().ParseAsync(AnyStream(), 1)).ToList();

        tables.ShouldBe(new[] { "b1", "b2", "b3" });
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Nada_extraido_falha_com_mensagem_propria()
    {
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(null, new[] { "b1" }));
        _extraction
            .Setup(e => e.ExtractFromTableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExtractedTransaction>());

        var ex = await Should.ThrowAsync<DomainException>(() => Build().ParseAsync(AnyStream(), 1));

        ex.Message.ShouldBe(MessageKeys.AiNothingExtracted);
    }

    [Fact]
    public async Task Erro_de_dominio_do_leitor_e_propagado_com_a_chave_da_mensagem()
    {
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ThrowsAsync(new DomainException(MessageKeys.FileTooLarge));

        var ex = await Should.ThrowAsync<DomainException>(() => Build().ParseAsync(AnyStream(), 1));

        ex.Message.ShouldBe(MessageKeys.FileTooLarge);
    }

    [Fact]
    public async Task Erro_inesperado_da_ia_vira_mensagem_amigavel_sem_vazar_detalhes()
    {
        var attachment = new AiAttachment(new byte[] { 1 }, "image/png", "image.png");
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(attachment, Array.Empty<string>()));
        _extraction
            .Setup(e => e.ExtractFromFileAsync(It.IsAny<int>(), It.IsAny<AiAttachment>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("chave sk-123 invalida"));

        var ex = await Should.ThrowAsync<DomainException>(() => Build().ParseAsync(AnyStream(), 1));

        ex.Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }
}

public class AiDocumentReaderTests
{
    private readonly Mock<IAiAvailability> _availability = new();

    private AiDocumentReader Build(bool enabled = true, bool supportsFiles = true)
    {
        _availability
            .Setup(a => a.GetActive())
            .Returns(enabled
                ? new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "m", SupportsFiles = supportsFiles })
                : null);

        return new AiDocumentReader(_availability.Object);
    }

    private static Stream Pdf() => new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.4 conteudo"));

    private static Stream Png() => new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A });

    private static Stream Jpeg() => new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

    [Fact]
    public async Task Ia_desligada_falha_com_mensagem_propria()
    {
        var ex = await Should.ThrowAsync<DomainException>(() => Build(enabled: false).ReadAsync(Pdf()));

        ex.Message.ShouldBe(MessageKeys.AiUnavailable);
    }

    [Fact]
    public async Task Pdf_com_provedor_sem_suporte_a_arquivos_falha_com_mensagem_propria()
    {
        var ex = await Should.ThrowAsync<DomainException>(() => Build(supportsFiles: false).ReadAsync(Pdf()));

        ex.Message.ShouldBe(MessageKeys.AiFilesUnsupported);
    }

    [Theory]
    [InlineData("pdf", "application/pdf")]
    [InlineData("png", "image/png")]
    [InlineData("jpg", "image/jpeg")]
    public async Task Arquivos_binarios_viram_anexo_com_o_tipo_de_midia_pelos_bytes(string kind, string mediaType)
    {
        var stream = kind switch { "pdf" => Pdf(), "png" => Png(), _ => Jpeg() };

        var document = await Build().ReadAsync(stream);

        document.Attachment!.MediaType.ShouldBe(mediaType);
        document.TableChunks.ShouldBeEmpty();
    }

    [Fact]
    public async Task Arquivo_acima_do_limite_falha_com_mensagem_propria()
    {
        var big = new MemoryStream(new byte[AiDocumentReader.MaxFileBytes + 1]);

        var ex = await Should.ThrowAsync<DomainException>(() => Build().ReadAsync(big));

        ex.Message.ShouldBe(MessageKeys.FileTooLarge);
    }

    [Fact]
    public async Task Texto_csv_vira_blocos_com_cabecalho_repetido()
    {
        var lines = new[] { "data;descricao;valor" }.Concat(Enumerable.Range(1, 450).Select(i => $"2026-10-01;loja {i};10"));
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\r\n", lines)));

        var document = await Build().ReadAsync(stream);

        document.Attachment.ShouldBeNull();
        document.TableChunks.Count.ShouldBe(3);
        document.TableChunks.ShouldAllBe(t => t.StartsWith("data;descricao;valor\n"));
    }

    [Fact]
    public async Task Tabela_acima_do_limite_de_linhas_falha_com_mensagem_propria()
    {
        var lines = Enumerable.Range(0, AiDocumentReader.MaxRows + 5).Select(i => $"linha {i}");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", lines)));

        var ex = await Should.ThrowAsync<DomainException>(() => Build().ReadAsync(stream));

        ex.Message.ShouldBe(MessageKeys.FileTooLarge);
    }

    [Fact]
    public async Task Planilha_xlsx_e_lida_localmente_e_vira_texto()
    {
        var document = await Build().ReadAsync(Xlsx(("data", "descricao", "valor"), ("2026-10-15", "uber", "23.5")));

        document.Attachment.ShouldBeNull();
        document.TableChunks.Single().ShouldContain("uber");
        document.TableChunks.Single().ShouldContain("descricao");
    }

    [Fact]
    public async Task Planilha_xlsx_nao_exige_suporte_a_arquivos_do_provedor()
    {
        var document = await Build(supportsFiles: false).ReadAsync(Xlsx(("a", "b", "c"), ("1", "2", "3")));

        document.TableChunks.ShouldNotBeEmpty();
    }

    [Fact]
    public void Leitor_de_planilha_ignora_linhas_vazias_e_separa_colunas()
    {
        var rows = SpreadsheetTextReader.ReadRows(Xlsx(("a", "b", "c"), ("", "", ""), ("1", "2", "3")));

        rows.Count.ShouldBe(2);
        rows[0].ShouldBe("a | b | c");
        rows[1].ShouldBe("1 | 2 | 3");
    }

    private static Stream Xlsx(params (string, string, string)[] rows)
    {
        var sheet = new StringBuilder(
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

        for (var index = 0; index < rows.Length; index++)
        {
            sheet.Append($"<row r=\"{index + 1}\">");
            AppendCell(sheet, "A", index + 1, rows[index].Item1);
            AppendCell(sheet, "B", index + 1, rows[index].Item2);
            AppendCell(sheet, "C", index + 1, rows[index].Item3);
            sheet.Append("</row>");
        }

        sheet.Append("</sheetData></worksheet>");

        var memory = new MemoryStream();

        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(zip, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            AddEntry(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            AddEntry(zip, "xl/workbook.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Plan1\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            AddEntry(zip, "xl/_rels/workbook.xml.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            AddEntry(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
        }

        memory.Position = 0;
        return memory;
    }

    private static void AppendCell(StringBuilder sheet, string column, int row, string value)
    {
        if (value.Length == 0)
            return;

        sheet.Append($"<c r=\"{column}{row}\" t=\"inlineStr\"><is><t>{value}</t></is></c>");
    }

    private static void AddEntry(ZipArchive zip, string name, string content)
    {
        var entry = zip.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
