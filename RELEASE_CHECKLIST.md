# DDS - Cofre Real | Checklist de Fechamento e Release

## 1) Validação técnica (obrigatório)
1. `dotnet build AgendaContas.sln -c Release`
2. `dotnet test AgendaContas.Tests\AgendaContas.Tests.csproj -c Release`
3. Resultado esperado:
- Build sem erros.
- Todos os testes aprovados.

## 2) Homologação funcional (manual)
1. Abrir app e autenticar com `admin/admin123`.
2. Validar CRUD:
- Categorias: criar, editar, desativar.
- Contas: criar, editar, desativar.
- Lançamentos: criar, editar, excluir, pagar.
3. Validar filtros no dashboard:
- Competência, status, categoria e busca textual combinados.
4. Validar competência:
- Fechar mês, tentar alterar lançamento (deve bloquear), reabrir mês.
5. Validar anexos:
- Anexar comprovante, abrir comprovante, remover comprovante.
6. Validar exportações:
- Exportar CSV.
- Exportar PDF.
7. Validar backup/restore:
- Criar backup.
- Restaurar backup e confirmar dados.
8. Validar integração BCB:
- Em `Sobre App` -> `Sistema` -> `Atualizar BCB`.
9. Validar fechamento de etapa:
- Em `Sobre App` -> `Fechar Etapa`.

## 3) Geração do pacote para distribuição
1. Publicar (framework-dependent):
- `dotnet publish AgendaContas.UI\AgendaContas.UI.csproj -c Release -r win-x64 --self-contained false -o .\artifacts\publish\DDS-CofreReal-win-x64`
2. Compactar:
- `cmd /c "if exist artifacts\publish\DDS-CofreReal-win-x64.zip del /f /q artifacts\publish\DDS-CofreReal-win-x64.zip & tar -a -c -f artifacts\publish\DDS-CofreReal-win-x64.zip -C artifacts\publish\DDS-CofreReal-win-x64 ."`
3. Fallback de compactação (se política bloquear comando):
- Compactar manualmente a pasta `artifacts\publish\DDS-CofreReal-win-x64` via Explorer (`Enviar para > Pasta compactada`).
4. Entregáveis:
- Pasta: `artifacts\publish\DDS-CofreReal-win-x64`
- Zip: `artifacts\publish\DDS-CofreReal-win-x64.zip`

## 4) Observações para operação
1. Banco padrão:
- `%LocalAppData%\AgendaContas\agenda.db`
2. Anexos:
- `%LocalAppData%\AgendaContas\Anexos`
3. Configuração do splash:
- `%LocalAppData%\AgendaContas\startup-settings.json`

## 5) Critério de aceite desta etapa
1. Build e testes aprovados.
2. Homologação manual concluída sem bloqueadores.
3. Pacote de release gerado e versionado para entrega.
