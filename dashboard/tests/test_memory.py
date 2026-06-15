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
