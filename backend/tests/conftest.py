"""Test fixtures.

Every test runs against a throwaway Chroma directory and registry so the suite
never touches the developer's real index.
"""

import os
import tempfile
from pathlib import Path

import pytest

_TMP = Path(tempfile.mkdtemp(prefix="synapse-tests-"))
os.environ["CHROMA_DIR"] = str(_TMP / "chroma")
os.environ["DATA_DIR"] = str(_TMP / "data")
os.environ["CHROMA_COLLECTION"] = "test_documents"
os.environ.pop("GROQ_API_KEY", None)


@pytest.fixture(scope="session")
def client():
    from fastapi.testclient import TestClient

    from app.main import app

    with TestClient(app) as c:
        yield c


@pytest.fixture
def sample_text() -> str:
    return (
        "Strength training builds muscle mass and supports joint health.\n\n"
        "Progressive overload means gradually increasing stress on the muscles "
        "by adding weight, reps, or reducing rest.\n\n"
        "Cardio improves heart health and reduces stress. Aim for 150 minutes "
        "of moderate intensity work each week.\n\n"
        "Recovery is where real progress happens. Sleep seven to nine hours a "
        "night and take rest days.\n\n"
    )
