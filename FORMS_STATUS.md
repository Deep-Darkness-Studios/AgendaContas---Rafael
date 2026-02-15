## Status dos Forms - AgendaContas

### LoginForm
- Arquivos: `AgendaContas.UI\Forms\LoginForm.cs`, `AgendaContas.UI\Forms\LoginForm.Designer.cs`
- Status: implementado com Designer e autenticação via `AuthService`.
- Características:
  - Campos `txtLogin` e `txtSenha`
  - Botões `Entrar` e `Cancelar`
  - `AcceptButton` configurado
  - Usuário autenticado exposto para a aplicação (`AuthenticatedUser`)
  - Credencial inicial funcional: `admin/admin123` (persistida com hash PBKDF2 no banco)

### MainForm
- Arquivos: `AgendaContas.UI\Forms\MainForm.cs`, `AgendaContas.UI\Forms\MainForm.Designer.cs`
- Status: fluxo financeiro com filtros, auditoria e controle de perfil (Admin/Operador).
- Características:
  - Cards de resumo por competência
  - Grid de lançamentos
  - Filtros: competência, status, categoria e busca textual
  - Ações: pagar, gerenciar contas, novo lançamento, gerenciar categorias, exportar CSV/PDF, editar/excluir lançamento
  - Bloqueio de ações administrativas para perfil `Operador`
  - Registro de auditoria para ações principais (login, pagamentos, CRUD, backup/restauração, competência)
  - Menus de contexto e duplo clique para edição de lançamento

### CategoriaManagementForm / CategoriaForm
- Arquivos: `AgendaContas.UI\Forms\CategoriaManagementForm.cs`, `AgendaContas.UI\Forms\CategoriaForm.cs`
- Status: CRUD de categorias com soft delete.

### ContaManagementForm / ContaForm
- Arquivos: `AgendaContas.UI\Forms\ContaManagementForm.cs`, `AgendaContas.UI\Forms\ContaForm.cs`
- Status: CRUD de contas com edição completa e soft delete.

### LancamentoForm
- Arquivo: `AgendaContas.UI\Forms\LancamentoForm.cs`
- Status: criação e edição manual de lançamentos.

### InfoForm
- Arquivos: `AgendaContas.UI\Forms\InfoForm.cs`, `AgendaContas.UI\Forms\InfoForm.Designer.cs`
- Status: mantido no projeto, sem impacto no fluxo principal do CRUD.

### Serviços e Persistência
- `AgendaContas.Data\Repositories\AppRepository.cs`
  - Migração idempotente para coluna `Usuarios.Perfil`
  - Tabela `Auditoria` com índices e API de registro/consulta
- `AgendaContas.UI\Services\PdfReportService.cs`
  - Geração de relatório PDF de lançamentos com resumo da competência
