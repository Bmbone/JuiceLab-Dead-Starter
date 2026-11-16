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
                elif key in ("468270651", "1875066870"):
                    if key == "468270651":
                        expected = data.replace("m_AnchoredPosition: {x: -115, y: -40}", "m_AnchoredPosition: {x: -64, y: -40}")
                        expected = expected.replace("m_SizeDelta: {x: 400, y: 60}", "m_SizeDelta: {x: 320, y: 64}")
                    else:
                        expected = data.replace("m_AnchoredPosition: {x: 460, y: -40}", "m_AnchoredPosition: {x: 64, y: -40}")
                        expected = expected.replace("m_SizeDelta: {x: 600, y: 70}", "m_SizeDelta: {x: 480, y: 64}")
                    self.assertEqual(after[key], expected)
                elif key in ("468270652", "1875066871"):
                    expected = data.replace("m_RaycastTarget: 1", "m_RaycastTarget: 0")
                    for old, new in (("m_fontSize: 48", "m_fontSize: 36"),
                                     ("m_fontSizeBase: 48", "m_fontSizeBase: 36"),
                                     ("m_fontWeight: 400", "m_fontWeight: 700"),
                                     ("m_fontStyle: 0", "m_fontStyle: 1"),
                                     ("m_characterSpacing: 0", "m_characterSpacing: 1.5")):
                        expected = expected.replace(old, new)
                    color, rgba = ("{r: 0.91764706, g: 0.85882353, b: 0.76078431, a: 1}", 4290960362) if key == "468270652" else ("{r: 0.90588235, g: 0.77254902, b: 0.49411765, a: 1}", 4286498279)
                    expected = expected.replace("m_fontColor: {r: 1, g: 1, b: 1, a: 1}", f"m_fontColor: {color}")
                    expected = expected.replace("m_fontColor32:\n    serializedVersion: 2\n    rgba: 4294967295", f"m_fontColor32:\n    serializedVersion: 2\n    rgba: {rgba}")
                    self.assertEqual(after[key], expected)
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
        self.assertEqual(re.findall(r"path: (Assets/.*?\.unity)", settings), ["Assets/_Project/Scenes/Medieval09_CastleMusic.unity", "Assets/_Project/Scenes/Medieval08_CastleAudio.unity", "Assets/_Project/Scenes/Medieval07_BallPaddle.unity", "Assets/_Project/Scenes/Medieval06_ImpactMotion.unity", "Assets/_Project/Scenes/Medieval05_MasonryParticles.unity", "Assets/_Project/Scenes/Medieval04_BrickBreak.unity", "Assets/_Project/Scenes/Medieval03_StoneCourt.unity", "Assets/_Project/Scenes/MedievalUI02_RoyalInterface.unity", "Assets/_Project/Scenes/MedievalUI01_Score.unity", "Assets/_Project/Scenes/Medieval.unity", "Assets/_Project/Scenes/Levels.unity", "Assets/_Project/Scenes/Abilities.unity", "Assets/_Project/Scenes/Arcade.unity", "Assets/_Project/Scenes/After.unity", "Assets/_Project/Scenes/Before.unity"])
        for path, guid in re.findall(r"path: (Assets/.*?\.unity)\n\s+guid: (\w+)", settings):
            self.assertIn(f"guid: {guid}", Path(str(ROOT / path) + ".meta").read_text())

    def test_previous_scene_checkpoints_are_frozen(self):
        checkpoint = "cd3db7da18b89d0b7ea35f5a97a72413b2448f9f"
        paths = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", checkpoint, "Assets"], cwd=ROOT).decode().splitlines()
        for path in paths:
            with self.subTest(path=path):
                expected = subprocess.check_output(["git", "show", f"{checkpoint}:{path}"], cwd=ROOT)
                self.assertEqual((ROOT / path).read_bytes(), expected)

    def test_levels_scene_wiring_and_metadata(self):
        text = (SCENES / "Levels.unity").read_text()
        scene = blocks(text)
        self.assertEqual(len(scene), len(re.findall(r"^--- !u!\d+ &", text, re.M)))
        types = {"196722578": "LevelBoard", "414140399": "LevelDeathZone", "1027157232": "LevelSession",
                 "1534019715": "LevelBall", "1900502001": "LevelAbilityController"}
        for block, name in types.items():
            guid = re.search(r"guid: (\w+)", (ROOT / f"Assets/_Project/Levels/{name}.cs.meta").read_text()).group(1)
            self.assertIn(f"guid: {guid}", scene[block])
            self.assertIn(f"Assembly-CSharp::JuiceStudy.Levels.{name}", scene[block])
        for key in ("1027157232", "1534019715"):
            self.assertIn("abilities: {fileID: 1900502001}", scene[key])
        self.assertIn("component: {fileID: 1900502001}", scene["1027157230"])
        for field, target in {"session": "1027157232", "paddle": "467281905", "ballVisual": "535787112",
                              "paddleVisual": "1879245919", "hudParent": "988065188"}.items():
            self.assertIn(f"{field}: {{fileID: {target}}}", scene["1900502001"])
            self.assertIn(target, scene)
        self.assertIn("Assembly-CSharp::JuiceStudy.Abilities.AbilityPaddle", scene["467281905"])
        self.assertNotIn("  columns:", scene["196722578"])
        self.assertNotIn("  rows:", scene["196722578"])
        prefab = ROOT / "Assets/_Project/Prefabs/LevelBrick.prefab"
        guid = re.search(r"guid: (\w+)", Path(str(prefab) + ".meta").read_text()).group(1)
        self.assertIn(f"guid: {guid}", scene["196722578"])
        self.assertIn("Assembly-CSharp::JuiceStudy.Levels.LevelBrick", prefab.read_text())
        all_guids = [re.search(r"^guid: (\w+)$", p.read_text(), re.M).group(1)
                     for p in (ROOT / "Assets").rglob("*.meta")]
        assets = list((ROOT / "Assets/_Project/Levels").glob("*.cs")) + [SCENES / "Levels.unity", prefab]
        for asset in assets:
            guid = re.search(r"^guid: ([0-9a-f]{32})$", Path(str(asset) + ".meta").read_text(), re.M).group(1)
            self.assertEqual(all_guids.count(guid), 1, str(asset))

    def test_abilities_scene_wiring_and_metadata(self):
        text = (SCENES / "Abilities.unity").read_text()
        scene = blocks(text)
        self.assertEqual(len(scene), len(re.findall(r"^--- !u!\d+ &", text, re.M)))
        types = {"196722578": "AbilityBoard", "414140399": "AbilityDeathZone", "1027157232": "AbilitySession",
                 "1534019715": "AbilityBall", "467281905": "AbilityPaddle", "1900502001": "AbilityController"}
        for block, name in types.items():
            guid = re.search(r"guid: (\w+)", (ROOT / f"Assets/_Project/Abilities/{name}.cs.meta").read_text()).group(1)
            self.assertIn(f"guid: {guid}", scene[block])
            self.assertIn(f"Assembly-CSharp::JuiceStudy.Abilities.{name}", scene[block])
        self.assertIn("component: {fileID: 1900502001}", scene["1027157230"])
        for key in ("1027157232", "1534019715"):
            self.assertIn("abilities: {fileID: 1900502001}", scene[key])
        for field, target in {"session": "1027157232", "paddle": "467281905", "ballVisual": "535787112",
                              "paddleVisual": "1879245919", "hudParent": "988065188"}.items():
            self.assertIn(f"{field}: {{fileID: {target}}}", scene["1900502001"])
            self.assertIn(target, scene)
        for asset in ("AbilityBrick.prefab",):
            path = ROOT / "Assets/_Project/Prefabs" / asset
            guid = re.search(r"guid: (\w+)", Path(str(path) + ".meta").read_text()).group(1)
            self.assertIn(f"guid: {guid}", scene["196722578"])
            self.assertIn("Assembly-CSharp::JuiceStudy.Abilities.AbilityBrick", path.read_text())
        all_guids = [re.search(r"^guid: (\w+)$", p.read_text(), re.M).group(1)
                     for p in (ROOT / "Assets").rglob("*.meta")]
        assets = list((ROOT / "Assets/_Project/Abilities").glob("*.cs")) + [SCENES / "Abilities.unity"]
        for asset in assets:
            guid = re.search(r"^guid: ([0-9a-f]{32})$", Path(str(asset) + ".meta").read_text(), re.M).group(1)
            self.assertEqual(all_guids.count(guid), 1, str(asset))

    def test_arcade_scene_references_and_isolation(self):
        scene = (SCENES / "Arcade.unity").read_text()
        arcade = blocks(scene)
        self.assertEqual(len(arcade), len(re.findall(r"^--- !u!\d+ &", scene, re.M)))
        component_types = {"196722578": "ArcadeBoard", "414140399": "ArcadeDeathZone",
                           "1027157232": "ArcadeSession", "1534019715": "ArcadeBall"}
        for block, script in component_types.items():
            meta = (ROOT / f"Assets/_Project/Arcade/{script}.cs.meta").read_text()
            guid = re.search(r"guid: (\w+)", meta).group(1)
            self.assertIn(f"guid: {guid}", arcade[block])
            self.assertIn(f"Assembly-CSharp::JuiceStudy.Arcade.{script}", arcade[block])
        for field, target in {"ball": "1534019715", "paddle": "467281905", "board": "196722578",
                              "hud": "988065184", "scoreText": "1875066871", "livesText": "468270652",
                              "feedback": "1900402000"}.items():
            self.assertIn(f"{field}: {{fileID: {target}}}", arcade["1027157232"])
            self.assertIn(target, arcade)
        self.assertIn("session: {fileID: 1027157232}", arcade["1534019715"])
        self.assertNotIn("988065189", scene)  # The baseline HUD would read the baseline singleton.
        self.assertNotIn("Assembly-CSharp::GameManager", scene)
        self.assertNotIn("Assembly-CSharp::Ball\n", scene)
        after = blocks((SCENES / "After.unity").read_text())
        for key, value in arcade.items():
            if value.startswith(("Rigidbody2D:\n", "BoxCollider2D:\n", "CircleCollider2D:\n")):
                self.assertEqual(value, after[key], f"Physics shape/settings changed in {key}")
        prefab = ROOT / "Assets/_Project/Prefabs/ArcadeBrick.prefab"
        prefab_guid = re.search(r"guid: (\w+)", prefab.with_suffix(".prefab.meta").read_text()).group(1)
        self.assertIn(f"guid: {prefab_guid}", arcade["196722578"])
        self.assertIn("Assembly-CSharp::JuiceStudy.Arcade.ArcadeBrick", prefab.read_text())

    def test_arcade_metadata_is_unique(self):
        all_guids = [re.search(r"^guid: (\w+)$", p.read_text(), re.M).group(1)
                     for p in (ROOT / "Assets").rglob("*.meta")]
        new_assets = list((ROOT / "Assets/_Project/Arcade").glob("*.cs")) + [SCENES / "Arcade.unity"]
        for asset in new_assets:
            guid = re.search(r"^guid: ([0-9a-f]{32})$", Path(str(asset) + ".meta").read_text(), re.M).group(1)
            self.assertEqual(all_guids.count(guid), 1, str(asset))

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

