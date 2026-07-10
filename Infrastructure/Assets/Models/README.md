# Modelos do Assistente de IA (embedding local)

Esta pasta guarda os dois arquivos que o `OnnxIntentEmbeddingMatcher`
(`Infrastructure/Services/Assistant/OnnxIntentEmbeddingMatcher.cs`) carrega em runtime
para casar a pergunta do usuário com as intenções do assistente, via embedding local
(sem LLM pago):

| Arquivo | Tamanho | Versionado no git? |
|---|---|---|
| `sentencepiece.bpe.model` | ~5 MB | Sim |
| `paraphrase-multilingual-MiniLM-L12-v2.onnx` | ~450 MB | **Não** — excede o limite de 100 MB do GitHub |

O `.onnx` está no `.gitignore` (`Infrastructure/Assets/Models/*.onnx`). Se você clonou o
repositório do zero, essa pasta vem **sem** ele — siga os passos abaixo antes de rodar a
API, ou qualquer chamada ao assistente (`AssistantController`) vai falhar ao iniciar o
`InferenceSession`.

## Passo a passo para obter o `.onnx`

1. Acesse o repositório do modelo no Hugging Face:
   https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/tree/main/onnx
2. Baixe o arquivo **`model.onnx`** (a versão fp32, ~470 MB — não use as variantes
   quantizadas `model_qint8_*`/`model_O*`, o pooling em `OnnxIntentEmbeddingMatcher`
   espera a saída `last_hidden_state` no formato do modelo original).
3. Renomeie o arquivo baixado para `paraphrase-multilingual-MiniLM-L12-v2.onnx`.
4. Coloque em `Infrastructure/Assets/Models/paraphrase-multilingual-MiniLM-L12-v2.onnx`
   (mesma pasta deste README).

Alternativa via terminal (não precisa de login/token, o repositório é público):

```bash
curl -L -o Infrastructure/Assets/Models/paraphrase-multilingual-MiniLM-L12-v2.onnx ^
  "https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/resolve/main/onnx/model.onnx"
```

(no PowerShell, troque `curl -L -o` por `Invoke-WebRequest -OutFile`).

## O `sentencepiece.bpe.model`

Esse arquivo já é versionado no git — normalmente você não precisa baixar de novo. Caso
falte (ex.: checkout parcial), ele vem do mesmo repositório do modelo, na raiz:
https://huggingface.co/sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2/blob/main/sentencepiece.bpe.model

## Configuração

Os caminhos são lidos de `Api/appsettings.json` (não versionado — ver
`.gitignore`/`Api/appsettings.Development.json` como referência de estrutura):

```json
"Assistant": {
  "EmbeddingModelPath": "Assets/Models/paraphrase-multilingual-MiniLM-L12-v2.onnx",
  "TokenizerModelPath": "Assets/Models/sentencepiece.bpe.model"
}
```

Esses caminhos são relativos ao diretório de saída do build (`AppContext.BaseDirectory`).
O `Infrastructure.csproj` já copia tudo em `Assets/Models/**` para o output
(`CopyToOutputDirectory=PreserveNewest`) — depois de colocar o `.onnx` aqui, um novo
build (`dotnet build`) é suficiente para ele aparecer em `Api/bin/.../Assets/Models/`.
