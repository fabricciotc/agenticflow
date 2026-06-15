# Spec: Backend MetaGPT-style para meta-ralph — Fase 1

## 1. Objetivo

Reestructurar el backend de meta-ralph para que deje de ser un `server.py` monolítico y adopte los patrones clave de MetaGPT que aportan valor sin arrastrar dependencias pesadas. El spec se adapta **al flujo real que hoy ejecuta `AgentRunner`**: no se redefine el pipeline desde cero, se formaliza.

Patrones a adoptar:

- `Message` como unidad de comunicación entre agentes.
- `Role` + `Action` para modelar agentes y sus capacidades.
- `Memory` indexada por acción/rol para audit trail y contexto.
- `Environment` como bus de mensajes.
- `Plan` con DAG de tareas.
- `ToolRegistry` para registrar herramientas usables por los agentes.
- `RepoParser` AST para dar contexto del codebase a los agentes.
- `Orchestrator` que coordine el loop multi-agente y sea independiente de Flask.

Skills preinstaladas y MCPs quedan **fuera del alcance de este spec**; se tratarán en fases posteriores sobre esta base.

## 2. Principios de diseño

1. **Mínimas dependencias.** No usar MetaGPT como librería. Reimplementar solo lo necesario con la stdlib y Flask.
2. **Separación de concerns.** `server.py` solo expone REST/WebSocket. La orquestación vive en `core/`.
3. **Backward compatible.** El board, `run-state.json`, los archivos `prd-<ticket>.md`, `tasks-<ticket>.json` y el dashboard continuarán funcionando igual.
4. **Evolutivo, no revolucionario.** Se mantiene el pipeline de 5 fases actual. Cada fase existente se convierte en una o varias `Action`s.
5. **Incremental.** Cada componente se puede mergear y probar por separado.

## 3. Adaptación al flujo actual de meta-ralph

Hoy `AgentRunner.run()` ejecuta secuencialmente:

| Orden | Fase | Método actual | Estado/Agente actual | Salida clave |
|---|---|---|---|---|
| 1 | Inicialización | — | `in-design`, `orchestrator` | — |
| 2 | PM Analysis | `run_pm_analysis()` | `in-design`, `pm-research-agents` | `prd-<ticket>.md` |
| 3 | Architecture | `run_architect()` (stub) | `in-design`, `architect` | nada hoy |
| 4 | Design Review | `_generate_design_questions()` + `_wait_for_design_answers()` | `design-review`, `design-review` | respuestas técnicas |
| 5 | Planning | `run_planner()` | `in-design`, `project-manager` | `tasks-<ticket>.json` |
| 6 | Parallel Execution | `run_execution()` | `in-progress`, `engineer-squad` | código modificado |
| 7 | QA Review | `run_qa()` | `in-review`, `qa-engineer` | aprobación/rechazo |
| 8 | Done | — | `completed` | ticket marcado done |

Puntos importantes a conservar:

- `_run_kimi_prompt()` es el único punto de invocación a `kimi -p`. Se mantiene como motor de ejecución de acciones.
- El prefijo de skill `dotnet` está hardcodeado hoy; se moverá a un mecanismo configurable en fase 2.
- Existe un sistema de `pendingQuestions` con timeout de 120s.
- El design review es una pausa síncrona con timeout de 60s.
- El QA Engineer paralelo revisa cada tarea con un subagente `qa-<taskId>`.
- La ejecución paralela de engineers trabaja sobre el mismo `repoPath` (sin worktrees reales hoy).

El refactor modelará cada fase como `Action`s y cada actor como `Role`s, pero **sin cambiar el comportamiento observable** en esta fase.

## 4. Arquitectura propuesta

```text
dashboard/
├── server.py                 # Flask + SocketIO, delega todo a core
├── core/                     # Nuevo: lógica de orquestación
│   ├── __init__.py
│   ├── models.py             # Message, Role, Action, Plan, Task
│   ├── memory.py             # Memory indexada por cause_by/role
│   ├── environment.py        # Bus de mensajes + dispatcher
│   ├── registry.py           # ToolRegistry
│   ├── repo_parser.py        # RepoParser AST
│   └── orchestrator.py       # Reemplaza la lógica de AgentRunner
├── roles/                    # Nuevo: definiciones de roles
│   ├── __init__.py
│   ├── base.py               # Role base
│   ├── pm_research.py
│   ├── architect.py
│   ├── planner.py
│   ├── engineer.py
│   └── qa.py
├── actions/                  # Nuevo: acciones reutilizables
│   ├── __init__.py
│   ├── base.py               # Action base
│   ├── research.py
│   ├── design.py
│   ├── plan.py
│   ├── implement.py
│   ├── review.py
│   └── ask_user.py           # pendingQuestions / design review
├── tools/                    # Nuevo: tools registradas
│   ├── __init__.py
│   ├── file_tool.py
│   ├── shell_tool.py
│   └── git_tool.py
└── static/                   # UI existente (sin cambios grandes)
```

## 5. Componentes

### 5.1 `Message`

Dataclass mínima inspirada en MetaGPT:

```python
@dataclass
class Message:
    id: str
    content: str
    role: str           # user / assistant / system
    cause_by: str       # id de la acción/rol que lo generó
    sent_from: str      # id del agente emisor
    send_to: Set[str]   # destinatarios; {"all"} = broadcast
    metadata: Dict[str, Any]
    created_at: str     # ISO 8601
```

Responsabilidades:
- Serializar a dict para `run-state.json`.
- Servir como evento en el `Environment`.
- Incluir metadata para tipo (`task_completed`, `request_review`, `request_help`, `question`, etc.).

### 5.2 `Role`

```python
class Role(ABC):
    role_id: str
    profile: str
    goal: str
    constraints: str
    actions: List[Action]
    memory: Memory
    addresses: Set[str]

    async def observe(self, env: Environment) -> List[Message]: ...
    async def think(self) -> Optional[Action]: ...
    async def act(self, action: Action, context: List[Message]) -> Message: ...
    async def run(self, env: Environment) -> Optional[Message]: ...
```

Responsabilidades:
- Observar mensajes del `Environment` que le correspondan.
- Elegir una `Action`.
- Ejecutarla y publicar el resultado.

Roles concretos (mapeados al flujo actual):
- `PMResearchRole`: investiga un área y genera notas (`pm-domain`, `pm-ux`, `pm-technical`, `pm-integration`, `pm-risk`).
- `PMLeadRole`: consolida notas en `prd-<ticket>.md`.
- `ArchitectRole`: genera decisiones técnicas (hoy stub; se enriquece con análisis real).
- `PlannerRole`: genera `Plan` y persiste `tasks-<ticket>.json`.
- `EngineerRole`: implementa una tarea del plan.
- `QARole`: revisa tareas; lead `qa-engineer` coordina subagentes `qa-<taskId>`.
- `OrchestratorRole`: coordina las fases del pipeline.
- `UserProxyRole`: representa al usuario para responder `pendingQuestions` y design review.

### 5.3 `Action`

```python
class Action(ABC):
    action_id: str
    name: str
    desc: str
    input_schema: Dict[str, Any]
    output_schema: Dict[str, Any]

    async def run(self, context: List[Message], **kwargs) -> Message: ...
```

Responsabilidades:
- Encapsular un paso atómico del pipeline actual.
- Construir el prompt para `kimi` o ejecutar una tool.
- Devolver un `Message` con el resultado.
- Guardar prompts/outputs en `scripts/meta-ralph/state/` como hace hoy `_run_kimi_prompt()`.

Acciones concretas (mapeadas al flujo actual):
- `ResearchAction`: ejecuta un PM Research Agent.
- `ConsolidatePRDAction`: consolida reportes en PRD.
- `ArchitectAction`: genera decisiones técnicas a partir del PRD.
- `GenerateDesignQuestionsAction`: genera preguntas de design review.
- `PlanAction`: genera `tasks-<ticket>.json`.
- `ImplementAction`: implementa una tarea (equivalente a `_run_single_engineer_task`).
- `ReviewAction`: revisa una tarea (equivalente a subagente QA).
- `AskUserAction`: bloquea esperando respuesta del usuario (pendingQuestions / design review).

### 5.4 `Memory`

```python
class Memory:
    messages: List[Message]
    index_by_cause: Dict[str, List[Message]]
    index_by_role: Dict[str, List[Message]]

    def add(self, msg: Message): ...
    def get(self, k: int = 0) -> List[Message]: ...
    def get_by_cause(self, cause: str) -> List[Message]: ...
    def get_by_role(self, role: str) -> List[Message]: ...
    def recent_context(self, n: int = 10) -> List[Message]: ...
```

Responsabilidades:
- Almacenar el historial de mensajes del `Environment`.
- Indexar por `cause_by` y `sent_from` para que los roles filtren rápido.
- Persistirse en `run-state.json` bajo `messages`.
- Migrar los `messages` actuales del run-state.

### 5.5 `Environment`

```python
class Environment:
    roles: Dict[str, Role]
    memory: Memory
    message_queue: Queue

    def add_role(self, role: Role): ...
    def publish_message(self, msg: Message): ...
    def get_messages_for(self, role_id: str) -> List[Message]: ...
    async def run_round(self) -> bool: ...  # True si hubo actividad
    def is_idle(self) -> bool: ...
```

Responsabilidades:
- Recibir mensajes de los roles.
- Entregar a cada rol los mensajes que le correspondan (`send_to` o `addresses`).
- Ejecutar rondas hasta que todos los roles estén idle.
- Exponer `history` para el dashboard.

### 5.6 `Plan`

```python
@dataclass
class Task:
    task_id: str
    instruction: str
    assignee: str       # role_id
    dependencies: List[str]
    status: str         # pending | ready | running | done | failed
    result: Optional[Message]
    # Campos actuales del JSON de tareas:
    title: str
    description: str
    files_to_touch: List[str]
    complexity: str
    qa_checklist: List[str]

class Plan:
    goal: str
    tasks: List[Task]
    task_map: Dict[str, Task]

    def add_tasks(self, tasks: List[Task]): ...
    def ready_tasks(self) -> List[Task]: ...
    def finish_task(self, task_id: str, result: Message): ...
    def is_finished(self) -> bool: ...
    def reset_downstream(self, task_id: str): ...
    def to_json(self) -> List[Dict]: ...
    @classmethod
    def from_json(cls, data: List[Dict]) -> "Plan": ...
```

Responsabilidades:
- Representar el DAG de tareas generado por `run_planner()`.
- Permitir ejecución paralela de tareas listas.
- Serializar/deserializar a `tasks-<ticket>.json` con el mismo schema actual.

### 5.7 `ToolRegistry`

```python
class ToolRegistry:
    tools: Dict[str, Callable]
    schemas: Dict[str, Dict[str, Any]]

    def register(self, name: str, fn: Callable, schema: Dict[str, Any]): ...
    def get(self, name: str) -> Callable: ...
    def list(self) -> List[Dict[str, Any]]: ...
    async def invoke(self, name: str, params: Dict[str, Any]) -> Any: ...
```

Responsabilidades:
- Registrar funciones Python como tools.
- Exponer schemas al LLM (inicialmente en prompts planos).
- Servir de base para integrar MCPs en fases posteriores.

Tools iniciales (equivalentes a operaciones que hacen los engineers hoy):
- `read_file(path)`
- `write_file(path, content)`
- `run_shell(command, cwd)`
- `git_status(cwd)`
- `git_diff(cwd)`

### 5.8 `RepoParser`

```python
class RepoParser:
    root_path: Path

    def generate_symbols(self) -> List[Symbol]: ...
    def get_structure(self) -> Dict[str, Any]: ...
```

Responsabilidades:
- Usar `ast` de la stdlib para extraer clases, funciones, métodos y relaciones.
- Generar un resumen JSON que se inyecte en los prompts de arquitecto/ingeniero.
- No depender de `pylint` ni `pyreverse`.

### 5.9 `Orchestrator`

```python
class Orchestrator:
    ticket: Dict[str, Any]
    env: Environment
    plan: Optional[Plan]
    state: Dict[str, Any]
    _stop_event: Event
    _pause_event: Event

    async def run(self): ...
    async def run_phase(self, phase: str): ...
    def pause(self): ...
    def resume(self): ...
    def stop(self): ...
    def to_run_state(self) -> Dict[str, Any]: ...
```

Responsabilidades:
- Reemplazar `AgentRunner`.
- Crear roles, acciones y plan según la fase actual.
- Ejecutar el pipeline de 5 fases actual, manteniendo el mismo orden y comportamiento.
- Gestionar `pendingQuestions` y design review mediante `AskUserAction`.
- Persistir estado en `run-state.json` y snapshots.
- Emitir actualizaciones al dashboard vía callback o SocketIO.
- Mantener pause/resume/stop y snapshots por ticket (funcionalidad ya existente).

## 6. Data flow adaptado

1. Usuario da **play** a un ticket.
2. `server.py` llama a `Orchestrator.run(ticket)`.
3. `Orchestrator` crea `Environment` y añade `OrchestratorRole`.
4. `OrchestratorRole` publica mensaje "start_phase:pm_analysis".
5. `PMResearchRole`s observan, ejecutan `ResearchAction` en paralelo y publican notas.
6. `PMLeadRole` consolida notas con `ConsolidatePRDAction` y escribe `prd-<ticket>.md`.
7. `ArchitectRole` ejecuta `ArchitectAction` (hoy stub; enriquecido más adelante).
8. `GenerateDesignQuestionsAction` genera preguntas; `AskUserAction` espera respuestas o usa asumidas por timeout.
9. `PlannerRole` ejecuta `PlanAction`, lee `prd-<ticket>.md` y escribe `tasks-<ticket>.json`.
10. `EngineerRole`s ejecutan `ImplementAction` en paralelo respetando dependencias.
11. `QARole` lead coordina subagentes `qa-<taskId>` con `ReviewAction`; hasta 3 rondas de corrección.
12. Al finalizar, ticket pasa a `done`.

Cada cambio de fase actualiza `run-state.json` y el dashboard refleja progreso.

## 7. Refactor de `server.py`

`server.py` debe quedar como una capa delgada:

- Carga/guarda `board.json` y `run-state.json`.
- Expone endpoints REST y WebSocket.
- Mantiene el thread del `Orchestrator` activo.
- Delega toda la lógica de agentes a `core/`.

No debe contener prompts, lógica de fases ni llamadas directas a `kimi`. Eso vive en `roles/` y `actions/`.

### Migración gradual sugerida

No reescribir `server.py` de golpe. Pasos:

1. Crear `core/models.py`, `core/memory.py`, `core/environment.py`.
2. Extraer `RepoParser` y `ToolRegistry` a `core/`.
3. Crear `actions/base.py` y mover un par de fases (ej. research/consolidate) a `Action`s.
4. Crear `roles/base.py` y roles mínimos.
5. Crear `Orchestrator` que envuelva `AgentRunner` y vaya delegando fases.
6. Una vez todas las fases estén en `Orchestrator`, eliminar `AgentRunner`.

## 8. Persistencia

- `board.json`: sin cambios de schema.
- `run-state.json`: se mantiene como fuente de verdad del dashboard.
  - `messages` pasa a ser la serialización de `Memory`.
  - `agents` refleja los roles activos.
  - `plan` opcional con el DAG actual.
- Snapshots: se mantienen como `run-state.<ticketId>.json` (funcionalidad ya implementada).
- Archivos por ticket: `prd-<ticket>.md`, `tasks-<ticket>.json`, prompts/outputs: mismas rutas y formatos actuales.

## 9. Integración con dashboard (frontend)

- Sin cambios de API en un primer momento: `/api/run-state`, `/api/board`, WebSocket.
- Futuro: añadir endpoint `/api/tools` para listar tools registradas.
- Futuro: añadir endpoint `/api/repo-structure?path=...` para el RepoParser.

## 10. Error handling

- Si un `Role` falla, publica un mensaje de error con `level=error`.
- `Orchestrator` decide: retry, replanificar o marcar ticket como `failed`.
- Límite de reintentos por acción (default 3).
- Timeout por acción (default 30 min).
- Se conserva el mecanismo actual de `stop_event` para cancelar ejecución paralela.

## 11. Testing plan

- Tests unitarios para `Message`, `Memory`, `Plan`, `ToolRegistry`, `RepoParser`.
- Tests de integración para `Environment` con roles dummy.
- Test de comparación: ejecutar un ticket con `AgentRunner` actual y con `Orchestrator` nuevo; comparar `run-state.json` y archivos generados.
- Test end-to-end con acciones mockeadas.

## 12. Entregables

1. Nuevo paquete `dashboard/core/` con modelos, memory, environment, registry, repo_parser y orchestrator.
2. Nuevo paquete `dashboard/roles/` con roles concretos.
3. Nuevo paquete `dashboard/actions/` con acciones concretas.
4. Nuevo paquete `dashboard/tools/` con tools iniciales.
5. `server.py` refactorizado a capa delgada.
6. Tests básicos.
7. Documentación de arquitectura actualizada.

## 13. Qué queda fuera de este spec

- Skills preinstaladas de Kimi (`dotnet`, `ui`, `code-review`, etc.) — fase 2.
- Integración con servidores MCP — fase 3.
- Vector search / RAG.
- Cambios grandes en la UI del dashboard.
- Cambios en el CLI `meta-ralph.sh`.
