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
