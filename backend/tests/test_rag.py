"""Prompt-assembly tests -- pure functions, no models or network needed."""

from app import llm, rag
from app.schemas import ChatMessage, ContextChunk


def _chunk(idx: int, text: str) -> ContextChunk:
    return ContextChunk(
        chunk_id=f"f::{idx}",
        text=text,
        score=1.0,
        file_id="f",
        file_name="doc.txt",
        chunk_index=idx,
    )


def test_system_prompt_includes_persona_and_rules():
    prompt = rag.build_system_prompt("Patient teacher", "Be helpful.", "Never guess.")
    assert "Persona: Patient teacher" in prompt
    assert "Be helpful." in prompt
    assert "Never guess." in prompt


def test_system_prompt_falls_back_to_default():
    prompt = rag.build_system_prompt(None, "   ", None)
    assert rag.DEFAULT_SYSTEM_PROMPT in prompt


def test_context_block_is_truncated():
    huge = [_chunk(i, "x" * 5000) for i in range(10)]
    block = rag.build_context_block(huge)
    assert len(block) <= rag.MAX_CONTEXT_CHARS + 200  # headers add a little


def test_empty_context_is_explicit():
    assert rag.build_context_block([]) == "NO CONTEXT FOUND"


def test_history_is_replayed_and_capped():
    history = [
        ChatMessage(role="user" if i % 2 == 0 else "assistant", content=f"turn {i}")
        for i in range(30)
    ]
    messages = rag.build_messages("now what?", [_chunk(0, "ctx")], None, None, None, history)

    assert messages[0]["role"] == "system"
    assert messages[-1]["role"] == "user"
    assert "now what?" in messages[-1]["content"]
    assert len(messages) == rag.MAX_HISTORY_TURNS + 2  # system + history + current
    assert "turn 29" in messages[-2]["content"]  # most recent turn survived


def test_reasoning_tags_are_stripped():
    assert llm.clean_output("<think>hmm, let me see</think>Final answer.") == "Final answer."
    assert llm.clean_output("<THINKING>a</THINKING> B") == "B"
    assert llm.clean_output("plain") == "plain"
    assert llm.clean_output(None) == ""
