"""Source-level checks for the final After scene; Unity execution is separate."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "Assets/_Project"
SCENE = PROJECT / "Scenes/After.unity"


def blocks(text):
    return {m.group(1): m.group(2) for m in re.finditer(
        r"^--- !u!\d+ &(-?\d+)\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S)}


def script_guid(path):
    return re.search(r"^guid: (\w+)$", Path(str(PROJECT / path) + ".meta").read_text(), re.M).group(1)


class ProjectChecks(unittest.TestCase):
    def test_after_is_the_build_entry_with_before_comparison(self):
        self.assertEqual(set((PROJECT / "Scenes").glob("*.unity")),
                         {SCENE, PROJECT / "Scenes/Before.unity"})
        text = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text()
        entries = re.findall(r"path: (Assets/.*?\.unity)\n\s+guid: (\w+)", text)
        expected_guid = re.search(r"guid: (\w+)", Path(str(SCENE) + ".meta").read_text()).group(1)
        self.assertEqual(entries, [("Assets/_Project/Scenes/After.unity", expected_guid)])

    def test_round_and_spell_references_resolve_to_their_controllers(self):
        scene = blocks(SCENE.read_text())
        controllers = {}
        for name in ["LevelSession", "LevelBoard", "LevelBall", "LevelDeathZone", "LevelAbilityController"]:
            key = script_guid("Levels/" + name + ".cs")
            found = [(identifier, body) for identifier, body in scene.items() if "guid: " + key in body]
            self.assertEqual(len(found), 1, name)
            controllers[name] = found[0]
        session_id, session = controllers["LevelSession"]
        ability_id, ability = controllers["LevelAbilityController"]
        for field, name in [("ball", "LevelBall"), ("board", "LevelBoard"), ("abilities", "LevelAbilityController")]:
            self.assertIn(field + ": {fileID: " + controllers[name][0] + "}", session)
        self.assertIn("session: {fileID: " + session_id + "}", ability)
        self.assertIn("abilities: {fileID: " + ability_id + "}", controllers["LevelBall"][1])

    def test_final_feedback_layers_are_present(self):
        scene = SCENE.read_text()
        required = ["Feedback/PaddleSquashFeedback.cs", "Feedback/BallTrailFeedback.cs",
                    "Presentation/MedievalScoreFeedback.cs", "Presentation/MedievalInterfacePolish.cs",
                    "BreakEffects/StoneBreakFeedback.cs", "MasonryParticles/MasonryParticleFeedback.cs",
                    "ImpactMotion/ImpactMotionFeedback.cs", "RelicGear/RelicGearFeedback.cs",
                    "CastleAudio/CastleAudioFeedback.cs", "CastleMusic/CastleMusicController.cs",
                    "CourtSettings/CourtSettingsPanel.cs", "ChapterPresentation/ChapterPresentation.cs",
                    "FireballVfx/FireballVisualFeedback.cs", "RuneShieldVfx/RuneShieldVisualFeedback.cs",
                    "GiantGripVfx/GiantGripVisualFeedback.cs"]
        for path in required:
            self.assertIn("guid: " + script_guid(path), scene, path)

    def test_legacy_singletons_are_not_attached(self):
        scene = SCENE.read_text()
        for path in ["Scripts/Core/GameManager.cs", "Scripts/Core/HUD.cs", "Scripts/Gameplay/Ball.cs"]:
            self.assertNotIn("guid: " + script_guid(path), scene, path)

    def test_feedback_event_lifecycle_and_random_isolation(self):
        events = (PROJECT / "Scripts/Core/GameEvents.cs").read_text()
        for path in (PROJECT / "Feedback").glob("*.cs"):
            source = path.read_text()
            adds = re.findall(r"GameEvents\.(\w+) \+= (\w+)", source)
            removes = re.findall(r"GameEvents\.(\w+) -= (\w+)", source)
            with self.subTest(script=path.name):
                self.assertTrue(adds)
                self.assertEqual(sorted(adds), sorted(removes))
                for event, _ in adds:
                    self.assertRegex(events, rf"event Action(?:<[^>]+>)? {event};")
                self.assertNotRegex(source, r"\bRandom\.(?:Range|value|InitState)")

    def test_score_and_trail_remain_presentation_only(self):
        score = (PROJECT / "Presentation/MedievalScoreFeedback.cs").read_text()
        self.assertNotRegex(score, r"scoreText\.(?:text|SetText)\s*(?:=|\()")
        trail = (PROJECT / "Feedback/BallTrailFeedback.cs").read_text()
        self.assertIn("trail.Clear()", trail)
        self.assertNotRegex(trail, r"ball\.(?:position|localScale)\s*=")


if __name__ == "__main__":
    unittest.main(verbosity=2)
