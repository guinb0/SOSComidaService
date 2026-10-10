# SOSComidaService

API do SOS Comida (.NET 9, EF Core + SQLite).

## Rodar

```bash
dotnet run --project SOSComida.csproj
```

Sobe em `http://localhost:5118` (Swagger em `/swagger`). O banco `soscomida.db` é criado
na primeira execução e populado com um cenário de demonstração. Para começar do zero,
apague o arquivo.

Usuários de demonstração: `guinb@soscomida.com` / `admin123` (admin) e
`moderador@teste.com`, `doador@teste.com`, `instituicao@teste.com` / `teste123`.

Bancos antigos, com senha em texto puro, são convertidos para hash automaticamente na
subida.

## Autenticação

`POST /auth/login` devolve um JWT. Os demais endpoints esperam
`Authorization: Bearer <token>`. Tudo exige login, a não ser o que está marcado com
`[AllowAnonymous]` (leitura de campanhas, regiões, prestação de contas, cadastro).
Quem é o usuário sai sempre do token, nunca de `usuarioId` no corpo ou na query.

Policies: `Moderacao` (moderador ou admin) e `Admin`.

## Configuração

| Chave | Desenvolvimento | Produção |
|---|---|---|
| `Jwt:Chave` | já vem no `appsettings.Development.json` | **obrigatória**, 32+ caracteres (`Jwt__Chave`) |
| `Recaptcha:SecretKey` | chave de teste do Google | **obrigatória**, senão o login é recusado |
| `Cors:Origens` | localhost, emulador e IP da LAN | URL do front (`Cors__Origens__0`) |
| `EmailSettings:SenderEmail` | vazio: e-mails só vão para o log | remetente SMTP real |
| `Ocr:ChaveApi` | vazio: Tesseract local | chave da Mistral (`Ocr__ChaveApi`); obrigatória para OCR no Docker |

Segredos locais podem ir em `appsettings.Local.json` (fora do git); veja
`appsettings.Local.example.json`.

### OCR das notas fiscais

Com `Ocr:ChaveApi` preenchida, a leitura usa a API da Mistral (`/v1/ocr`, modelo
`mistral-ocr-latest`, configurável em `Ocr:Modelo`). Sem chave, usa o Tesseract local,
que só roda no Windows. `Ocr:Provedor` força um dos dois (`mistral` ou `tesseract`). Em
qualquer caso os campos lidos voltam para o responsável revisar antes de contarem na
prestação de contas.

## Testes

```bash
dotnet test tests/SOSComida.Tests
```

Unitários (extrator de nota fiscal, hash de senha, validação de upload, permissões) e de
integração. Os de integração sobem a API com um SQLite temporário e conferem as regras
de acesso.

Só com o runtime do .NET 10 instalado, o `<RollForward>Major</RollForward>` dos projetos
faz rodar mesmo assim.

## Docker

O container guarda o banco em `/app/data` e os uploads em `/app/wwwroot/uploads`, os dois
como volumes. Para ter OCR no container, defina `Ocr__ChaveApi`.
