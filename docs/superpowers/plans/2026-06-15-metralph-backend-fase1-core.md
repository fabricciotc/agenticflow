# Meta-Ralph Backend Fase 1A — Core Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Crear los componentes base (`Message`, `Memory`, `Environment`, `ToolRegistry`, `RepoParser`) en `dashboard/core/` con tests unitarios, sin dependencias nuevas.

**Architecture:** Paquete `dashboard/core/` con clases livianas inspiradas en MetaGPT. `server.py` no se toca todavía; solo se valida que `core` se pueda importar sin romper el servidor. Tests con `unittest` de la stdlib.

**Tech Stack:** Python 3.10+, `dataclasses`, `ast`, `json`, `pathlib`, `unittest`.

---

## File Structure

```text
dashboard/
├── core/
│   ├── __init__.py
│   ├── models.py        # Message + Task
│   ├── memory.py        # Memory
│   ├── environment.py   # Environment
│   ├── registry.py      # ToolRegistry
│   └── repo_parser.py   # RepoParser
└── tests/
    ├── test_message.py
    ├── test_memory.py
    ├── test_environment.py
    ├── test_registry.py
    └── test_repo_parser.py
```

---

### Task 1: `Message` dataclass + serialización

**Files:**
- Create: `dashboard/core/__init__.py`
- Create: `dashboard/core/models.py`
- Create: `dashboard/tests/test_message.py`

- [ ] **Step 1: Write the failing test**

Create `dashboard/tests/test_message.py`:

```python
import sys
import uuid
import json
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import unittest
from core.models import Message


class TestMessage(unittest.TestCase):
    def test_create_message(self):
        msg = Message(content="hello", sent_from="orchestrator", cause_by="start")
        self.assertEqual(msg.content, "hello")
        self.assertEqual(msg.sent_from, "orchestrator")
        self.assertEqual(msg.cause_by, "start")
        self.assertEqual(msg.role, "assistant")
        self.assertEqual(msg.send_to, {"all"})
        self.assertIsNotNone(msg.id)

    def test_to_dict_roundtrip(self):
        msg = Message(content="hello", sent_from="orchestrator", cause_by="start")
        d = msg.to_dict()
        self.assertEqual(d["content"], "hello")
        restored = Message.from_dict(d)
        self.assertEqual(restored.content, "hello")
        self.assertEqual(restored.sent_from, "orchestrator")


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_message -v
```

Expected: FAIL with `ModuleNotFoundError: No module named 'core'`.

- [ ] **Step 3: Write minimal implementation**

Create `dashboard/core/__init__.py`:

```python
# Core orchestration components for meta-ralph
```

Create `dashboard/core/models.py`:

```python
from __future__ import annotations

import uuid
from dataclasses import dataclass, field, asdict
from datetime import datetime, timezone
from typing import Any, Dict, Set


@dataclass
class Message:
    content: str
    sent_from: str
    cause_by: str
    id: str = field(default_factory=lambda: uuid.uuid4().hex)
    role: str = "assistant"
    send_to: Set[str] = field(default_factory=lambda: {"all"})
    metadata: Dict[str, Any] = field(default_factory=dict)
    created_at: str = field(default_factory=lambda: datetime.now(timezone.utc).isoformat())

    def to_dict(self) -> Dict[str, Any]:
        data = asdict(self)
        data["send_to"] = list(data["send_to"])
        return data

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "Message":
        data = dict(data)
        data["send_to"] = set(data.get("send_to", ["all"]))
        return cls(**data)
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_message -v
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph
git add dashboard/core/__init__.py dashboard/core/models.py dashboard/tests/test_message.py
git commit -m "feat(core): Message dataclass with serialization"
```

---

### Task 2: `Memory` indexada por `cause_by` y rol

**Files:**
- Create: `dashboard/core/memory.py`
- Create: `dashboard/tests/test_memory.py`

- [ ] **Step 1: Write the failing test**

Create `dashboard/tests/test_memory.py`:

```python
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import unittest
from core.models import Message
from core.memory import Memory


class TestMemory(unittest.TestCase):
    def test_add_and_get(self):
        mem = Memory()
        msg = Message(content="a", sent_from="pm-domain", cause_by="research")
        mem.add(msg)
        self.assertEqual(len(mem.get()), 1)
        self.assertEqual(mem.get()[0].content, "a")

    def test_get_by_cause(self):
        mem = Memory()
        mem.add(Message(content="a", sent_from="pm-domain", cause_by="research"))
        mem.add(Message(content="b", sent_from="pm-ux", cause_by="research"))
        mem.add(Message(content="c", sent_from="orchestrator", cause_by="start"))
        self.assertEqual(len(mem.get_by_cause("research")), 2)
        self.assertEqual(len(mem.get_by_role("pm-domain")), 1)

    def test_recent_context(self):
        mem = Memory()
        for i in range(5):
            mem.add(Message(content=str(i), sent_from="a", cause_by="x"))
        recent = mem.recent_context(3)
        self.assertEqual(len(recent), 3)
        self.assertEqual(recent[0].content, "2")

    def test_to_from_dict(self):
        mem = Memory()
        mem.add(Message(content="a", sent_from="x", cause_by="y"))
        d = mem.to_dict()
        restored = Memory.from_dict(d)
        self.assertEqual(len(restored.get()), 1)


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_memory -v
```

Expected: FAIL with `ModuleNotFoundError: No module named 'core.memory'`.

- [ ] **Step 3: Write minimal implementation**

Create `dashboard/core/memory.py`:

```python
from __future__ import annotations

from collections import defaultdict
from typing import Any, Dict, List

from core.models import Message


class Memory:
    def __init__(self):
        self.messages: List[Message] = []
        self._index_by_cause: Dict[str, List[Message]] = defaultdict(list)
        self._index_by_role: Dict[str, List[Message]] = defaultdict(list)

    def add(self, msg: Message) -> None:
        self.messages.append(msg)
        self._index_by_cause[msg.cause_by].append(msg)
        self._index_by_role[msg.sent_from].append(msg)

    def add_batch(self, msgs: List[Message]) -> None:
        for msg in msgs:
            self.add(msg)

    def get(self, k: int = 0) -> List[Message]:
        if k <= 0:
            return list(self.messages)
        return list(self.messages[-k:])

    def get_by_cause(self, cause: str) -> List[Message]:
        return list(self._index_by_cause.get(cause, []))

    def get_by_role(self, role: str) -> List[Message]:
        return list(self._index_by_role.get(role, []))

    def recent_context(self, n: int = 10) -> List[Message]:
        return self.get(k=n)

    def to_dict(self) -> Dict[str, Any]:
        return {"messages": [m.to_dict() for m in self.messages]}

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> "Memory":
        mem = cls()
        for m in data.get("messages", []):
            mem.add(Message.from_dict(m))
        return mem
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_memory -v
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph
git add dashboard/core/memory.py dashboard/tests/test_memory.py
git commit -m "feat(core): Memory indexed by cause and role"
```

---

### Task 3: `Environment` como bus de mensajes

**Files:**
- Create: `dashboard/core/environment.py`
- Create: `dashboard/tests/test_environment.py`

- [ ] **Step 1: Write the failing test**

Create `dashboard/tests/test_environment.py`:

```python
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import unittest
from core.models import Message
from core.memory import Memory
from core.environment import Environment


class DummyRole:
    def __init__(self, role_id):
        self.role_id = role_id
        self.observed = []
        self.addresses = {role_id}

    def observe(self, env):
        self.observed = env.get_messages_for(self.role_id)
        return self.observed

    async def run(self, env):
        msgs = self.observe(env)
        if msgs:
            env.publish_message(Message(content=f"ack from {self.role_id}", sent_from=self.role_id, cause_by="ack", send_to={"all"}))
            return True
        return False


class TestEnvironment(unittest.TestCase):
    def test_publish_and_observe(self):
        env = Environment()
        r1 = DummyRole("role1")
        r2 = DummyRole("role2")
        env.add_role(r1)
        env.add_role(r2)
        env.publish_message(Message(content="hello", sent_from="user", cause_by="start", send_to={"role1"}))
        self.assertEqual(len(env.get_messages_for("role1")), 1)
        self.assertEqual(len(env.get_messages_for("role2")), 0)

    def test_run_round(self):
        import asyncio
        env = Environment()
        r1 = DummyRole("role1")
        env.add_role(r1)
        env.publish_message(Message(content="hello", sent_from="user", cause_by="start", send_to={"role1"}))
        active = asyncio.run(env.run_round())
        self.assertTrue(active)
        self.assertEqual(len(env.memory.get()), 2)


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_environment -v
```

Expected: FAIL with `ModuleNotFoundError: No module named 'core.environment'`.

- [ ] **Step 3: Write minimal implementation**

Create `dashboard/core/environment.py`:

```python
from __future__ import annotations

import asyncio
from collections import deque
from typing import Dict, List, Optional

from core.models import Message
from core.memory import Memory


class Environment:
    def __init__(self):
        self.roles: Dict[str, any] = {}
        self.memory = Memory()
        self._queue: deque = deque()

    def add_role(self, role) -> None:
        self.roles[role.role_id] = role

    def publish_message(self, msg: Message) -> None:
        self._queue.append(msg)

    def get_messages_for(self, role_id: str) -> List[Message]:
        result = []
        for msg in list(self._queue):
            if "all" in msg.send_to or role_id in msg.send_to or role_id in msg.send_to:
                result.append(msg)
        return result

    def _drain_queue_to_memory(self) -> None:
        while self._queue:
            self.memory.add(self._queue.popleft())

    async def run_round(self) -> bool:
        self._drain_queue_to_memory()
        tasks = []
        for role in self.roles.values():
            if hasattr(role, "run"):
                tasks.append(role.run(self))
        if not tasks:
            return False
        results = await asyncio.gather(*tasks, return_exceptions=True)
        active = any(r is True for r in results if not isinstance(r, Exception))
        return active

    def is_idle(self) -> bool:
        return len(self._queue) == 0

    def history(self) -> List[Message]:
        return self.memory.get()
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_environment -v
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph
git add dashboard/core/environment.py dashboard/tests/test_environment.py
git commit -m "feat(core): Environment message bus with rounds"
```

---

### Task 4: `ToolRegistry`

**Files:**
- Create: `dashboard/core/registry.py`
- Create: `dashboard/tests/test_registry.py`

- [ ] **Step 1: Write the failing test**

Create `dashboard/tests/test_registry.py`:

```python
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import unittest
from core.registry import ToolRegistry


def dummy_tool(path: str) -> str:
    """Reads a file.\n\nArgs:\n    path: file path\n\nReturns:\n    file content"""
    return f"content of {path}"


class TestToolRegistry(unittest.TestCase):
    def test_register_and_invoke(self):
        reg = ToolRegistry()
        reg.register("read_file", dummy_tool)
        self.assertIn("read_file", reg.list_names())
        result = reg.invoke("read_file", {"path": "foo.txt"})
        self.assertEqual(result, "content of foo.txt")

    def test_schema_extraction(self):
        reg = ToolRegistry()
        reg.register("read_file", dummy_tool)
        schema = reg.get_schema("read_file")
        self.assertEqual(schema["name"], "read_file")
        self.assertIn("path", [p["name"] for p in schema["parameters"]])


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_registry -v
```

Expected: FAIL with `ModuleNotFoundError: No module named 'core.registry'`.

- [ ] **Step 3: Write minimal implementation**

Create `dashboard/core/registry.py`:

```python
from __future__ import annotations

import inspect
from typing import Any, Callable, Dict, List


class ToolRegistry:
    def __init__(self):
        self._tools: Dict[str, Callable] = {}
        self._schemas: Dict[str, Dict[str, Any]] = {}

    def register(self, name: str, fn: Callable, schema: Dict[str, Any] | None = None) -> None:
        self._tools[name] = fn
        self._schemas[name] = schema or self._infer_schema(fn)

    def _infer_schema(self, fn: Callable) -> Dict[str, Any]:
        sig = inspect.signature(fn)
        doc = inspect.getdoc(fn) or ""
        params = []
        for pname, param in sig.parameters.items():
            if pname == "self":
                continue
            params.append({
                "name": pname,
                "type": str(param.annotation) if param.annotation is not param.empty else "str",
                "required": param.default is param.empty,
            })
        return {"name": fn.__name__, "description": doc, "parameters": params}

    def get(self, name: str) -> Callable:
        return self._tools[name]

    def get_schema(self, name: str) -> Dict[str, Any]:
        return self._schemas[name]

    def list_names(self) -> List[str]:
        return list(self._tools.keys())

    def list(self) -> List[Dict[str, Any]]:
        return [self._schemas[name] for name in self.list_names()]

    def invoke(self, name: str, params: Dict[str, Any]) -> Any:
        fn = self._tools[name]
        return fn(**params)
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_registry -v
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph
git add dashboard/core/registry.py dashboard/tests/test_registry.py
git commit -m "feat(core): ToolRegistry with schema inference"
```

---

### Task 5: `RepoParser` AST básico

**Files:**
- Create: `dashboard/core/repo_parser.py`
- Create: `dashboard/tests/test_repo_parser.py`

- [ ] **Step 1: Write the failing test**

Create `dashboard/tests/test_repo_parser.py`:

```python
import sys
import tempfile
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent.parent))

import unittest
from core.repo_parser import RepoParser


class TestRepoParser(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        (self.root / "foo.py").write_text("""
class Foo:
    def bar(self, x: int) -> int:
        return x + 1
""")

    def tearDown(self):
        self.tmp.cleanup()

    def test_generate_symbols(self):
        parser = RepoParser(self.root)
        symbols = parser.generate_symbols()
        classes = [s for s in symbols if s["kind"] == "class"]
        methods = [s for s in symbols if s["kind"] == "method"]
        self.assertEqual(len(classes), 1)
        self.assertEqual(classes[0]["name"], "Foo")
        self.assertEqual(len(methods), 1)
        self.assertEqual(methods[0]["name"], "bar")

    def test_get_structure(self):
        parser = RepoParser(self.root)
        structure = parser.get_structure()
        self.assertIn("files", structure)
        self.assertEqual(len(structure["files"]), 1)


if __name__ == "__main__":
    unittest.main()
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_repo_parser -v
```

Expected: FAIL with `ModuleNotFoundError: No module named 'core.repo_parser'`.

- [ ] **Step 3: Write minimal implementation**

Create `dashboard/core/repo_parser.py`:

```python
from __future__ import annotations

import ast
from pathlib import Path
from typing import Dict, List


class RepoParser:
    def __init__(self, root_path: str | Path):
        self.root_path = Path(root_path)

    def generate_symbols(self) -> List[Dict[str, any]]:
        symbols = []
        for py_file in self.root_path.rglob("*.py"):
            if "venv" in py_file.parts or "__pycache__" in py_file.parts:
                continue
            try:
                tree = ast.parse(py_file.read_text(encoding="utf-8"), filename=str(py_file))
            except SyntaxError:
                continue
            for node in ast.walk(tree):
                if isinstance(node, ast.ClassDef):
                    symbols.append({
                        "kind": "class",
                        "name": node.name,
                        "file": str(py_file.relative_to(self.root_path)),
                        "line": node.lineno,
                    })
                elif isinstance(node, ast.FunctionDef):
                    parent_kind = "method" if any(isinstance(p, ast.ClassDef) for p in ast.walk(tree)) else "function"
                    symbols.append({
                        "kind": "method" if parent_kind == "method" else "function",
                        "name": node.name,
                        "file": str(py_file.relative_to(self.root_path)),
                        "line": node.lineno,
                        "args": [a.arg for a in node.args.args],
                    })
        return symbols

    def get_structure(self) -> Dict[str, any]:
        files = []
        for py_file in self.root_path.rglob("*.py"):
            if "venv" in py_file.parts or "__pycache__" in py_file.parts:
                continue
            files.append(str(py_file.relative_to(self.root_path)))
        return {"root": str(self.root_path), "files": files, "symbols": self.generate_symbols()}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_repo_parser -v
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph
git add dashboard/core/repo_parser.py dashboard/tests/test_repo_parser.py
git commit -m "feat(core): RepoParser using stdlib ast"
```

---

### Task 6: Verificar que `server.py` sigue arrancando

**Files:**
- Modify: ninguno
- Test: `dashboard/server.py`

- [ ] **Step 1: Run syntax check**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
source .venv/bin/activate
python -m py_compile server.py
```

Expected: no output (success).

- [ ] **Step 2: Run existing tests**

```bash
cd /Users/fabricciotornero/.kimi-code/skills/meta-ralph/dashboard
python -m unittest tests.test_ticket_flow -v
```

Expected: PASS (los tests actuales no deberían romperse).

- [ ] **Step 3: Commit (si todo OK)**

No hay cambios nuevos; este paso no genera commit.

---

## Self-Review Checklist

- [ ] Spec coverage: cada componente del spec (Message, Memory, Environment, ToolRegistry, RepoParser) tiene una tarea.
- [ ] Placeholder scan: no hay TBD, TODO, ni pasos sin código.
- [ ] Type consistency: `Message.to_dict()`/`from_dict()` usan mismos nombres; `Memory` usa `Message` consistentemente.
- [ ] Testing: cada componente tiene tests unitarios.
- [ ] No se toca `server.py` ni la UI en esta fase.
