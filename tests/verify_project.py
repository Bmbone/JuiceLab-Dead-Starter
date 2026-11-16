"""Source-level safety checks only; Unity import/playtesting is still required."""
import re
import subprocess
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = "1fc1ce6627cbb28e5c34243cc7955316fad1c7ac"
SCENES = ROOT / "Assets/_Project/Scenes"
FEEDBACK = ROOT / "Assets/_Project/Feedback"


def baseline(path):
    return subprocess.check_output(["git", "show", f"{BASE}:{path}"], cwd=ROOT)


def blocks(text):
    return {match.group(1): match.group(2) for match in re.finditer(
        r"^--- !u!\d+ &(\d+)\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S)}


class ProjectChecks(unittest.TestCase):
    def test_before_and_shared_gameplay_untouched(self):
        tracked = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", BASE], cwd=ROOT).decode().splitlines()
        protected = [p for p in tracked if p.startswith(("Assets/", "Packages/", "ProjectSettings/")) and p != "ProjectSettings/EditorBuildSettings.asset"]
        for path in protected:
            with self.subTest(path=path):
                self.assertEqual((ROOT / path).read_bytes(), baseline(path))

    def test_after_is_additive_to_before(self):
        before = blocks((SCENES / "Before.unity").read_text())
        after = blocks((SCENES / "After.unity").read_text())
        for key, data in before.items():
            with self.subTest(block=key):
                if key == "9223372036854775807":
                    self.assertEqual(after[key].replace("  - {fileID: 1900402001}\n", ""), data)
                elif key == "519420032":
                    self.assertEqual(after[key], data.replace("m_LocalPosition: {x: -1.93, y: 0, z: -10}", "m_LocalPosition: {x: 0, y: 0, z: -10}"))
                elif key == "988065188":
                    self.assertEqual(after[key], data.replace("m_LocalScale: {x: 0, y: 0, z: 0}", "m_LocalScale: {x: 1, y: 1, z: 1}"))
                else:
                    self.assertEqual(after[key], data)
        self.assertEqual(len(after) - len(before), 7)  # One GameObject, one Transform, five components.

    def test_feedback_metadata_and_references(self):
        scene = (SCENES / "After.unity").read_text()
        meta_by_guid = {}
        for path in (ROOT / "Assets").rglob("*.meta"):
            match = re.search(r"^guid: ([0-9a-f]{32})$", path.read_text(), re.M)
            if match:
                meta_by_guid.setdefault(match.group(1), []).append(path)
        for path in FEEDBACK.glob("*.cs"):
            guid = re.search(r"guid: ([0-9a-f]{32})", path.with_suffix(".cs.meta").read_text()).group(1)
            self.assertEqual(len(meta_by_guid[guid]), 1)
            self.assertIn(f"guid: {guid}", scene)
            self.assertIn(f"class {path.stem}", path.read_text())
        material_guid = re.search(r"guid: (\w+)", (FEEDBACK / "Feedback.mat.meta").read_text()).group(1)
        self.assertEqual(scene.count(f"guid: {material_guid}"), 2)
        shader_guid = re.search(r"guid: (\w+)", (FEEDBACK / "Feedback.shader.meta").read_text()).group(1)
        self.assertIn(shader_guid, (FEEDBACK / "Feedback.mat").read_text())

    def test_required_assets_and_build_scenes(self):
        self.assertTrue((ROOT / "Assets/TextMesh Pro/Sprites/EmojiOne.png").read_bytes().startswith(b"\x89PNG\r\n\x1a\n"))
        settings = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
        self.assertEqual(re.findall(r"path: (Assets/.*?\.unity)", settings), ["Assets/_Project/Scenes/After.unity", "Assets/_Project/Scenes/Before.unity"])

    def test_scene_wiring_is_visual_only(self):
        after = blocks((SCENES / "After.unity").read_text())
        self.assertIn("visual: {fileID: 1879245918}", after["1900402003"])
        self.assertIn("m_Father: {fileID: 467281904}", after["1879245918"])
        self.assertIn("scoreText: {fileID: 1875066871}", after["1900402004"])
        self.assertIn("ball: {fileID: 1534019714}", after["1900402005"])
        ball_object = re.search(r"m_GameObject: \{fileID: (\d+)\}", after["1534019714"]).group(1)
        self.assertIn("m_Name: Ball", after[ball_object])

    def test_event_lifecycle_and_random_isolation(self):
        events = (ROOT / "Assets/_Project/Scripts/Core/GameEvents.cs").read_text()
        for path in FEEDBACK.glob("*.cs"):
            source = path.read_text()
            adds = re.findall(r"GameEvents\.(\w+) \+= (\w+)", source)
            removes = re.findall(r"GameEvents\.(\w+) -= (\w+)", source)
            with self.subTest(script=path.name):
                self.assertTrue(adds)
                self.assertEqual(sorted(adds), sorted(removes))
                for event, _ in adds:
                    self.assertRegex(events, rf"event Action(?:<[^>]+>)? {event};")
                self.assertNotRegex(source, r"\bRandom\.(?:Range|value|InitState)")

    def test_score_and_trail_do_not_modify_gameplay(self):
        score = (FEEDBACK / "ScorePopFeedback.cs").read_text()
        self.assertIn("score > previousScore", score)
        self.assertNotRegex(score, r"\.text\s*=")
        trail = (FEEDBACK / "BallTrailFeedback.cs").read_text()
        self.assertIn("LateUpdate()", trail)
        self.assertIn("trail.Clear()", trail)
        self.assertNotRegex(trail, r"ball\.(?:position|localScale)\s*=")


if __name__ == "__main__":
    unittest.main(verbosity=2)
