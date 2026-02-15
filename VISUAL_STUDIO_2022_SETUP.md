# DDS - Cofre Real | Preparação para Visual Studio 2022

## Pré-requisitos
1. Visual Studio 2022 atualizado.
2. Workload `Desenvolvimento para desktop com .NET`.
3. SDK .NET 8 instalado (projeto fixado em `global.json` para `8.0.418`).

## Abrir e executar no VS
1. Abrir `AgendaContas.sln`.
2. Definir `AgendaContas.UI` como Startup Project.
3. Configuração recomendada:
- `Debug | x64` para desenvolvimento.
- `Release | x64` para publicação.
4. Pressionar `F5`.

## Verificações iniciais
1. Login com `admin / admin123`.
2. Abrir `Sobre App` -> `Sistema` e validar caminho do banco.
3. Rodar `Sobre App` -> `Fechar Etapa` para carga inicial de categorias e geração recorrente.

## Comandos equivalentes (terminal)
1. Build:
- `dotnet build AgendaContas.sln -c Debug`
2. Testes:
- `dotnet test AgendaContas.Tests\AgendaContas.Tests.csproj -c Debug`
3. Publicação:
- `dotnet publish AgendaContas.UI\AgendaContas.UI.csproj -c Release -r win-x64 --self-contained false -o .\artifacts\publish\DDS-CofreReal-win-x64`

## Observações
1. Banco local:
- `%LocalAppData%\AgendaContas\agenda.db`
2. Anexos:
- `%LocalAppData%\AgendaContas\Anexos`
3. Configuração de splash:
- `%LocalAppData%\AgendaContas\startup-settings.json`
