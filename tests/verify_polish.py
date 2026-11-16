"""Standard-library source/asset regression checks. These do not run the Unity engine."""
import re
import unittest
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
LATEST = ASSETS / "_Project/Scenes/After.unity"
VFX = ASSETS / "_Project/GiantGripVfx"


def blocks(path):
    return re.findall(r"^--- !u!\d+ &(-?\d+)\n(.*?)(?=^--- !u!|\Z)", path.read_text(), re.M | re.S)


def guid(path):
    return re.search(r"^guid: ([a-f0-9]{32})$", path.read_text(), re.M).group(1)


class PolishChecks(unittest.TestCase):
    def test_asset_ids_are_unique_and_metadata_has_targets(self):
        seen = {}
        for meta in ASSETS.rglob("*.meta"):
            key = guid(meta)
            self.assertNotIn(key, seen, f"Duplicate GUID: {meta} and {seen.get(key)}")
            seen[key] = meta
            self.assertTrue(Path(str(meta)[:-5]).exists(), f"Orphan metadata: {meta}")

    def test_all_scene_and_prefab_local_references_resolve(self):
        for path in [*ASSETS.rglob("*.unity"), *ASSETS.rglob("*.prefab")]:
            entries = blocks(path)
            ids = {key for key, _ in entries}
            self.assertEqual(len(ids), len(entries), f"Duplicate fileID: {path}")
            for key, body in entries:
                for line in body.splitlines():
                    if "guid:" in line:
                        continue
                    for reference in re.findall(r"\{fileID: (-?\d+)\}", line):
                        self.assertTrue(reference == "0" or reference in ids, f"{path}: {key} -> {reference}")

    def test_build_scenes_have_matching_metadata(self):
        text = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
        scenes = re.findall(r"path: (Assets/.*?\.unity)\n\s+guid: (\w+)", text)
        self.assertEqual(len(scenes), 1)
        self.assertEqual(ROOT / scenes[0][0], LATEST)
        for path, key in scenes:
            self.assertTrue((ROOT / path).is_file())
            self.assertEqual(guid(Path(str(ROOT / path) + ".meta")), key)

    def test_after_scene_roots_match_root_transforms(self):
        scene = dict(blocks(LATEST))
        root_transforms = {key for key, body in scene.items()
                           if body.startswith(("Transform:\n", "RectTransform:\n"))
                           and "m_Father: {fileID: 0}" in body}
        root_records = [body for body in scene.values() if body.startswith("SceneRoots:\n")]
        self.assertEqual(len(root_records), 1)
        listed_roots = re.findall(r"\{fileID: (-?\d+)\}", root_records[0])
        self.assertEqual(len(listed_roots), len(set(listed_roots)))
        self.assertEqual(set(listed_roots), root_transforms)

    def test_latest_project_scripts_and_required_references(self):
        by_guid = {guid(meta): Path(str(meta)[:-5]) for meta in ASSETS.rglob("*.cs.meta")}
        value_types = {"bool", "int", "float", "double", "string", "Color", "Vector2", "Vector3", "Vector4"}
        for key, body in blocks(LATEST):
            if "Assembly-CSharp::" not in body:
                continue  # Unity/package components resolve during package import.
            script = re.search(r"m_Script: .*guid: (\w+)", body)
            self.assertIsNotNone(script, key)
            self.assertIn(script.group(1), by_guid, f"Unresolved project script on {key}")
            source = by_guid[script.group(1)].read_text()
            fields = re.findall(r"\[SerializeField(?:,[^\]]*)?\]\s+private\s+(\w+)\s+(\w+)", source)
            for field_type, field in fields:
                if field_type in value_types:
                    continue
                target = re.search(r"^  " + re.escape(field) + r": \{fileID: (-?\d+)", body, re.M)
                self.assertIsNotNone(target, f"Missing {field} on {key}")
                self.assertNotEqual(target.group(1), "0", f"Unassigned {field} on {key}")

    def test_new_materials_resolve_to_their_shader(self):
        shader = VFX / "GiantGrip.shader"
        shader_guid = guid(Path(str(shader) + ".meta"))
        for mode, name in enumerate(["EmeraldAura", "GoldGripRune", "GripMote"]):
            text = (VFX / (name + ".mat")).read_text()
            self.assertIn(f"guid: {shader_guid}", text)
            self.assertIn(f"- _Mode: {mode}", text)
        self.assertIn('Shader "JuiceStudy/GiantGripVfx"', shader.read_text())

    def test_audio_files_have_valid_pcm_payloads(self):
        paths = list((ASSETS / "_Project").rglob("*.wav"))
        self.assertEqual(len(paths), 8)
        for path in paths:
            with wave.open(str(path), "rb") as clip:
                self.assertIn(clip.getnchannels(), (1, 2), str(path))
                self.assertGreaterEqual(clip.getframerate(), 22050, str(path))
                self.assertGreater(clip.getnframes(), 0, str(path))
                self.assertEqual(clip.getcomptype(), "NONE", str(path))
                self.assertEqual(len(clip.readframes(clip.getnframes())),
                                 clip.getnframes() * clip.getnchannels() * clip.getsampwidth(), str(path))


if __name__ == "__main__":
    unittest.main(verbosity=2)
