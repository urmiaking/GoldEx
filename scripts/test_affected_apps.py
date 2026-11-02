import importlib.util
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


spec = importlib.util.spec_from_file_location("affected_apps", Path(__file__).with_name("affected-apps.py"))
affected = importlib.util.module_from_spec(spec)
spec.loader.exec_module(affected)


class AffectedApplicationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.directory = tempfile.TemporaryDirectory()
        cls.root = Path(cls.directory.name) / "repository"
        cls.root.mkdir()
        cls.git("init", "--quiet")
        cls.git("config", "user.name", "Deployment tests")
        cls.git("config", "user.email", "tests@example.invalid")
        cls.git("config", "commit.gpgsign", "false")
        # Deliberately put shared code under App, matching the real calculator architecture.
        cls.shared = "src/App/Shared/Shared/Shared.csproj"
        for project in affected.APPLICATIONS.values():
            relative = os.path.relpath(cls.shared, Path(project).parent).replace(os.sep, "\\")
            cls.write(project, f'<Project><ItemGroup><ProjectReference Include="{relative}" /></ItemGroup></Project>')
        cls.write(cls.shared, "<Project />")
        cls.write("src/Unused/Unused.csproj", "<Project />")
        cls.write("src/Tests/Tests.csproj", "<Project />")
        cls.base = cls.commit()

    @classmethod
    def tearDownClass(cls):
        cls.directory.cleanup()

    @classmethod
    def git(cls, *args):
        return affected.git(cls.root, *args).decode("utf-8").strip()

    @classmethod
    def write(cls, path, content="changed"):
        target = cls.root / path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(content, encoding="utf-8")

    @classmethod
    def commit(cls):
        cls.git("add", "--all")
        cls.git("commit", "--quiet", "--allow-empty", "-m", "fixture")
        return cls.git("rev-parse", "HEAD")

    def setUp(self):
        self.git("checkout", "--quiet", "--detach", self.base)
        self.state = dict.fromkeys(affected.APPLICATIONS, self.base)

    def changed_apps(self, *paths):
        for path in paths:
            self.write(path)
        return set(affected.detect(self.root, self.commit(), self.state))

    def test_application_specific_code_and_dockerfiles(self):
        for app, project in affected.APPLICATIONS.items():
            with self.subTest(app=app):
                self.git("checkout", "--quiet", "--detach", self.base)
                self.assertEqual({app}, self.changed_apps(str(Path(project).parent / "Page.razor")))
                self.git("checkout", "--quiet", "--detach", self.base)
                self.assertEqual({app}, self.changed_apps(str(Path(project).parent / "Dockerfile")))

    def test_shared_project_under_app_affects_both(self):
        self.assertEqual(set(affected.APPLICATIONS), self.changed_apps("src/App/Shared/Shared/Helper.cs"))

    def test_transitive_reference_added_and_subsequent_source_change(self):
        self.write(self.shared, '<Project><ItemGroup><ProjectReference Include="../../../Sdk/Deep/Deep.csproj" /></ItemGroup></Project>')
        self.write("src/Sdk/Deep/Deep.csproj", "<Project />")
        previous = self.commit()
        self.state = dict.fromkeys(affected.APPLICATIONS, previous)
        self.assertEqual(set(affected.APPLICATIONS), self.changed_apps("src/Sdk/Deep/Utility.cs"))

    def test_deleted_reference_and_source_use_old_graph(self):
        self.write("src/App/Shared/Shared/Helper.cs")
        previous = self.commit()
        self.state = dict.fromkeys(affected.APPLICATIONS, previous)
        for project in affected.APPLICATIONS.values():
            self.write(project, "<Project />")
        (self.root / "src/App/Shared/Shared/Helper.cs").unlink()
        self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, self.commit(), self.state)))

    def test_rename_between_applications_counts_both_paths(self):
        source = str(Path(affected.APPLICATIONS["goldex"]).parent / "Page.razor")
        destination = str(Path(affected.APPLICATIONS["calculator"]).parent / "Page.razor")
        self.write(source)
        self.state = dict.fromkeys(affected.APPLICATIONS, self.commit())
        self.git("mv", source, destination)
        self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, self.commit(), self.state)))

    def test_docs_tests_and_unreferenced_projects_skip_deployment(self):
        self.assertEqual(set(), self.changed_apps(
            "docs/guide.md", "AGENTS.md", "scripts/test_example.py", "src/Tests/Test.cs", "src/Unused/Thing.cs",
        ))

    def test_global_build_inputs_and_unknown_source_affect_both(self):
        for path in ("nuget.config", "nuget/vendor.nupkg", "Directory.Build.props", ".dockerignore", "src/new/input.dat"):
            with self.subTest(path=path):
                self.git("checkout", "--quiet", "--detach", self.base)
                self.assertEqual(set(affected.APPLICATIONS), self.changed_apps(path))

    def test_fonts_only_affect_enterprise(self):
        self.assertEqual({"goldex"}, self.changed_apps("fonts/Font.ttf"))

    def test_missing_baseline_and_force_selection(self):
        self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, self.base, {})))
        self.assertEqual({"goldex"}, set(affected.detect(self.root, self.base, self.state, ["goldex"])))
        self.state["goldex"] = "0" * 40
        self.assertEqual({"goldex"}, set(affected.detect(self.root, self.base, self.state)))

    def test_failed_or_superseded_runs_are_included_per_application(self):
        self.write(str(Path(affected.APPLICATIONS["goldex"]).parent / "First.cs"))
        first = self.commit()  # goldex run failed or was removed from the pending queue.
        self.write(str(Path(affected.APPLICATIONS["calculator"]).parent / "Second.cs"))
        head = self.commit()
        self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, head, self.state)))
        # Only goldex has now deployed the intermediate revision.
        self.state["goldex"] = first
        self.assertEqual({"calculator"}, set(affected.detect(self.root, head, self.state)))

    def test_diverged_history_forces_rebuild(self):
        self.write("docs/one.md")
        other = self.commit()
        self.git("checkout", "--quiet", "--detach", self.base)
        self.write("docs/two.md")
        head = self.commit()
        self.state = dict.fromkeys(affected.APPLICATIONS, other)
        self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, head, self.state)))

    def test_malformed_or_dynamic_reference_is_conservative(self):
        for text in ('<Project>', '<Project><ProjectReference Include="$(Unknown)/Library.csproj" /></Project>'):
            with self.subTest(text=text):
                self.git("checkout", "--quiet", "--detach", self.base)
                self.write(self.shared, text)
                self.assertEqual(set(affected.APPLICATIONS), set(affected.detect(self.root, self.commit(), self.state)))

    def test_imported_msbuild_references_are_conservative(self):
        self.write("Directory.Build.props", '<Project><ProjectReference Include="src/Unused/Unused.csproj" /></Project>')
        self.state = dict.fromkeys(affected.APPLICATIONS, self.commit())
        self.assertEqual(set(), self.changed_apps("docs/imports.md"))
        self.assertEqual(set(affected.APPLICATIONS), self.changed_apps("src/Unused/Thing.cs"))

    def test_explicit_import_is_conservative(self):
        self.write(self.shared, '<Project><Import Project="extra.props" /></Project>')
        self.write("src/App/Shared/Shared/extra.props", "<Project />")
        self.state = dict.fromkeys(affected.APPLICATIONS, self.commit())
        self.assertEqual(set(affected.APPLICATIONS), self.changed_apps("src/Unused/Thing.cs"))

    def test_linked_source_outside_project_directory_is_tracked(self):
        self.write(self.shared, '<Project><ItemGroup><Compile Include="../../../../linked/Helper.cs" /></ItemGroup></Project>')
        self.write("linked/Helper.cs", "original")
        self.state = dict.fromkeys(affected.APPLICATIONS, self.commit())
        self.assertEqual(set(affected.APPLICATIONS), self.changed_apps("linked/Helper.cs"))

    def test_state_updates_only_selected_app_and_outputs_are_usable(self):
        state_file = Path(self.directory.name) / "state.json"
        affected.record_deployment(state_file, self.base, ["calculator"])
        head = self.commit()
        affected.record_deployment(state_file, head, ["goldex"])
        self.assertEqual({"goldex": head, "calculator": self.base}, affected.read_state(state_file))
        output = Path(self.directory.name) / "outputs"
        subprocess.run([
            sys.executable, str(Path(__file__).with_name("affected-apps.py")),
            "--root", str(self.root), "--state-file", str(state_file), "--target", "goldex",
        ], check=True, capture_output=True, env=dict(os.environ, GITHUB_OUTPUT=str(output), GITHUB_STEP_SUMMARY=""))
        self.assertEqual("goldex=true\ncalculator=false\nany=true\n", output.read_text(encoding="utf-8"))


class RepositoryDependencyTests(unittest.TestCase):
    def test_current_real_project_graph(self):
        root = Path(__file__).resolve().parent.parent
        snapshot = affected.Snapshot(root, "HEAD")
        cases = {
            "src/App/Client/GoldEx.Client/Pages/Example.razor": {"goldex"},
            "src/App/Server/GoldEx.Server/Controllers/Example.cs": {"goldex"},
            "src/Calculator/Client/GoldEx.Calculator.Client/Pages/Example.razor": {"calculator"},
            "src/App/Client/GoldEx.Client.Components/Themes/Example.cs": set(affected.APPLICATIONS),
            "src/App/Server/GoldEx.Server.Domain/Example.cs": set(affected.APPLICATIONS),
            "src/Sdk/GoldEx.Sdk.Common/Example.cs": set(affected.APPLICATIONS),
        }
        for path, expected in cases.items():
            with self.subTest(path=path):
                self.assertEqual(expected, {app for app in affected.APPLICATIONS if snapshot.affects(app, [path])})


if __name__ == "__main__":
    unittest.main()
