import pytest

from app import chunkers


@pytest.mark.parametrize("method", ["fixed", "recursive", "paragraph"])
def test_every_method_respects_max_chars(method, sample_text):
    chunks = chunkers.chunk_text(sample_text * 5, method=method, chunk_size=300, chunk_overlap=50)
    assert chunks, "expected at least one chunk"
    assert all(len(c) <= 300 for c in chunks), "a chunk exceeded chunk_size"
    assert all(c.strip() for c in chunks), "produced a blank chunk"


def test_all_advertised_methods_are_implemented():
    """The UI dropdown offers these four; the backend must implement all four."""
    assert chunkers.available_methods() == ["fixed", "paragraph", "recursive", "semantic"]


def test_unknown_method_is_rejected(sample_text):
    with pytest.raises(ValueError, match="Unknown chunking method"):
        chunkers.chunk_text(sample_text, method="telepathy")


def test_empty_input_yields_no_chunks():
    for method in chunkers.available_methods():
        assert chunkers.chunk_text("   \n\n  ", method=method) == []


def test_fixed_overlap_actually_overlaps():
    text = "".join(f"sentence number {i}. " for i in range(200))
    chunks = chunkers.chunk_text(text, method="fixed", chunk_size=200, chunk_overlap=50)
    assert len(chunks) > 1
    # The tail of chunk N should reappear at the head of chunk N+1.
    assert chunks[0][-20:] in chunks[1]


def test_overlap_larger_than_size_is_clamped(sample_text):
    """A pathological overlap must not produce an infinite/zero-progress loop."""
    chunks = chunkers.chunk_text(sample_text, method="fixed", chunk_size=200, chunk_overlap=500)
    assert 0 < len(chunks) < 100


def test_clean_text_collapses_blank_runs():
    assert chunkers.clean_text("a\r\n\r\n\r\n\r\nb") == "a\n\nb"


def test_paragraph_packs_small_paragraphs_together():
    text = "\n\n".join(f"Paragraph {i} is short." for i in range(10))
    chunks = chunkers.chunk_text(text, method="paragraph", chunk_size=500, chunk_overlap=0)
    assert len(chunks) < 10, "short paragraphs should be packed, not emitted one-per-chunk"
